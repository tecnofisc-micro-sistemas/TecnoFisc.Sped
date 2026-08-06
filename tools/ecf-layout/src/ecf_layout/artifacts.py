"""Build and transactionally promote the reviewed ECF manifest/tracker pair."""

from __future__ import annotations

import hashlib
import json
import os
import shutil
import stat
import time
import uuid
from contextlib import ExitStack, contextmanager
from pathlib import Path
from typing import Callable, Iterator

from jsonschema import Draft202012Validator
from jsonschema.exceptions import SchemaError, ValidationError

from ecf_layout.manifest import (
    ManifestValidationError,
    _quarantine_items,
    _sanitize_record,
)
from ecf_layout.cache import sha256_file


class ArtifactPromotionError(ValueError):
    """Raised before promoted artifacts can be safely replaced."""


class ArtifactPathError(ArtifactPromotionError):
    """Raised when two artifact roles resolve to the same filesystem object."""


class ArtifactLockTimeout(ArtifactPromotionError):
    """Raised when another process retains the artifact lock past the deadline."""


InterruptHook = Callable[[str], None]
BeforePublishHook = Callable[[], None]
_JOURNAL_NAME = ".artifact-transaction.json"
_JOURNAL_BACKUP_NAME = ".artifact-transaction.backup.json"
_TRANSACTION_DIR_NAME = ".artifact-transactions"
_TRANSACTION_DESCRIPTOR_NAME = "descriptor.json"
_TRANSACTION_INTENT_NAME = "intent.json"
_RECOVERY_QUARANTINE_NAME = "recovery-quarantine.json"
_PAIR_MARKER_DIR_NAME = ".artifact-pairs"
_TARGET_MARKER_DIR_NAME = ".artifact-targets"
_LOCK_NAME = ".artifact.lock"
_LOCK_MAGIC = b"ecf-layout-artifact-lock-v1\n"
_DEFAULT_LOCK_TIMEOUT = 10.0
_IS_WINDOWS = os.name == "nt"


def sha256_bytes(data: bytes) -> str:
    return hashlib.sha256(data).hexdigest()


def canonical_candidate_sha256(records: object) -> str:
    return sha256_bytes(
        json.dumps(
            records, ensure_ascii=False, sort_keys=True, separators=(",", ":")
        ).encode("utf-8")
    )


def generation_id(pdf_sha256: str, candidate_sha256: str) -> str:
    return sha256_bytes(
        f"ecf-layout-12\n{pdf_sha256}\n{candidate_sha256}\n".encode("ascii")
    )


def render_tracker(records: list[dict]) -> str:
    """Render the bounded one-row-per-record Stage 17 tracker."""
    lines = [
        "# Stage 17 - ECF Layout 12 Baseline",
        "",
        "| Substage | Record | Title | Start page | End page | Block | Status |",
        "| --- | --- | --- | ---: | ---: | --- | --- |",
    ]
    for position, record in enumerate(records, start=2):
        lines.append(
            f"| 17.{position:03d} | {_cell(record['code'])} | {_cell(record['title'])} | "
            f"{record['pageStart']} | {record['pageEnd']} | {_cell(record['block'])} | [ ] |"
        )
    return "\n".join(lines) + "\n"


def build_artifacts(
    records: list[dict],
    work_dir: Path,
    manifest_out: Path,
    tracker_out: Path,
    *,
    pdf: Path,
    lock_timeout: float = _DEFAULT_LOCK_TIMEOUT,
) -> tuple[Path, Path]:
    """Validate and durably publish a provenance-bound local candidate generation."""
    work_dir = Path(work_dir)
    manifest_out = Path(manifest_out)
    tracker_out = Path(tracker_out)
    pdf = Path(pdf)
    generation_path = work_dir / "candidate" / "generation.json"
    build_state_path = work_dir / "build-state.json"
    quarantine_path = work_dir / "quarantine.json"
    journal_paths = _root_descriptor_paths(work_dir)
    protected_paths = [
        pdf,
        manifest_out,
        tracker_out,
        generation_path,
        build_state_path,
        quarantine_path,
        *journal_paths,
    ]
    _preflight_output_destinations(
        [manifest_out, tracker_out, generation_path, build_state_path, quarantine_path]
    )
    with _artifact_lock(
        work_dir,
        targets=[
            manifest_out,
            tracker_out,
            generation_path,
            build_state_path,
            quarantine_path,
        ],
        timeout=lock_timeout,
        protected_paths=protected_paths,
    ):
        try:
            _ensure_distinct_paths(protected_paths)
            _recover_artifact_transaction_locked(work_dir)
            if not pdf.is_file():
                raise ManifestValidationError(f"normative PDF not found: {pdf}")
            items = _quarantine_items(records, require_reviewed=False)
            if items:
                raise ManifestValidationError(
                    f"manifest validation quarantined {len(items)} item(s)"
                )
            sanitized = [_sanitize_record(record) for record in records]
            manifest_data = _pretty_json_bytes(sanitized)
            tracker_data = render_tracker(sanitized).encode("utf-8")
            pdf_sha256 = sha256_file(pdf)
            review_sha256 = canonical_candidate_sha256(sanitized)
            identifier = generation_id(pdf_sha256, review_sha256)
            metadata = {
                "schemaVersion": 1,
                "generationId": identifier,
                "state": "candidate",
                "pdfPath": str(pdf.resolve()),
                "pdfSha256": pdf_sha256,
                "reviewCandidateSha256": review_sha256,
                "candidateSha256": review_sha256,
                "trackerSha256": sha256_bytes(tracker_data),
                "evidencePaths": [],
            }
            _replace_artifacts_durably_locked(
                work_dir,
                [
                    (manifest_out, manifest_data),
                    (tracker_out, tracker_data),
                    (generation_path, _pretty_json_bytes(metadata)),
                    (
                        build_state_path,
                        _pretty_json_bytes(
                            {"state": "valid", "generationId": identifier}
                        ),
                    ),
                    (quarantine_path, _pretty_json_bytes({"items": []})),
                ],
                protected_paths=[pdf],
            )
            return manifest_out, tracker_out
        except ArtifactPathError:
            raise
        except (
            ArtifactPromotionError,
            ManifestValidationError,
            OSError,
            UnicodeError,
        ) as error:
            _invalidate_generation_locked(work_dir, str(error), protected_paths=[pdf])
            raise


def promote_artifacts(
    work_dir: Path,
    manifest_out: Path,
    tracker_out: Path,
    *,
    schema_path: Path,
    before_publish: BeforePublishHook | None = None,
    interrupt: InterruptHook | None = None,
    lock_timeout: float = _DEFAULT_LOCK_TIMEOUT,
) -> tuple[Path, Path]:
    work_dir = Path(work_dir)
    initial_paths = [
        Path(manifest_out),
        Path(tracker_out),
        Path(schema_path),
        work_dir / "candidate" / "layout-12-manifest.json",
        work_dir / "candidate" / "STAGE_17_ECF_BASELINE.md",
        work_dir / "candidate" / "generation.json",
        work_dir / "build-state.json",
        work_dir / "quarantine.json",
        *_root_descriptor_paths(work_dir),
    ]
    _preflight_output_destinations([Path(manifest_out), Path(tracker_out)])
    with _artifact_lock(
        work_dir,
        targets=[Path(manifest_out), Path(tracker_out)],
        timeout=lock_timeout,
        protected_paths=initial_paths,
    ):
        return _promote_artifacts_locked(
            work_dir,
            Path(manifest_out),
            Path(tracker_out),
            schema_path=Path(schema_path),
            before_publish=before_publish,
            interrupt=interrupt,
        )


def _promote_artifacts_locked(
    work_dir: Path,
    manifest_out: Path,
    tracker_out: Path,
    *,
    schema_path: Path,
    before_publish: BeforePublishHook | None,
    interrupt: InterruptHook | None,
) -> tuple[Path, Path]:
    """Promote the exact candidate byte snapshots that passed every invariant."""
    work_dir = Path(work_dir)
    _recover_artifact_transaction_locked(work_dir)
    manifest_out = Path(manifest_out)
    tracker_out = Path(tracker_out)
    schema_path = Path(schema_path)
    manifest_candidate = work_dir / "candidate" / "layout-12-manifest.json"
    tracker_candidate = work_dir / "candidate" / "STAGE_17_ECF_BASELINE.md"
    generation_path = work_dir / "candidate" / "generation.json"
    build_state_path = work_dir / "build-state.json"
    quarantine_path = work_dir / "quarantine.json"
    journal_paths = _root_descriptor_paths(work_dir)
    _ensure_distinct_paths(
        [
            manifest_out,
            tracker_out,
            schema_path,
            manifest_candidate,
            tracker_candidate,
            generation_path,
            build_state_path,
            quarantine_path,
            *journal_paths,
        ]
    )

    # Generation metadata is snapshotted first so its normative PDF and evidence paths can
    # participate in alias checks before any other input is read or any output is replaced.
    try:
        generation_snapshot = generation_path.read_bytes()
        generation = _load_json_snapshot(generation_snapshot)
        _validate_generation_metadata(generation, required_state="reviewed")
    except (OSError, UnicodeError, json.JSONDecodeError) as error:
        reason = f"cannot load candidate generation metadata: {error}"
        raise _record_promotion_failure(work_dir, reason) from error
    except ArtifactPromotionError as error:
        raise _record_promotion_failure(work_dir, str(error)) from error

    evidence_paths = [Path(path) for path in generation["evidencePaths"]]
    normative_pdf = Path(generation["pdfPath"])
    protected_inputs = [
        normative_pdf,
        manifest_out,
        tracker_out,
        schema_path,
        manifest_candidate,
        tracker_candidate,
        generation_path,
        build_state_path,
        quarantine_path,
        *journal_paths,
        *evidence_paths,
    ]
    _ensure_distinct_paths(protected_inputs)

    # Each mutable input is read exactly once. Validation and publication below use only these
    # immutable in-memory snapshots, closing the validation-to-replacement race.
    try:
        manifest_snapshot = manifest_candidate.read_bytes()
        tracker_snapshot = tracker_candidate.read_bytes()
        build_state_snapshot = build_state_path.read_bytes()
        quarantine_snapshot = quarantine_path.read_bytes()
        schema_snapshot = schema_path.read_bytes()
        evidence_snapshots = [path.read_bytes() for path in evidence_paths]
        records = _load_json_snapshot(manifest_snapshot)
        tracker_text = tracker_snapshot.decode("utf-8")
        build_state = _load_json_snapshot(build_state_snapshot)
        quarantine = _load_json_snapshot(quarantine_snapshot)
        schema = _load_json_snapshot(schema_snapshot)
        evidence_payloads = [
            _load_json_snapshot(snapshot) for snapshot in evidence_snapshots
        ]
    except (OSError, UnicodeError, json.JSONDecodeError) as error:
        reason = f"cannot load promotion input snapshot: {error}"
        raise _record_promotion_failure(work_dir, reason) from error

    try:
        _validate_generation_provenance(
            records,
            tracker_text,
            generation,
            build_state,
            quarantine,
            required_state="reviewed",
        )
        _validate_review_evidence(records, generation, evidence_payloads)
    except ArtifactPromotionError as error:
        raise _record_promotion_failure(work_dir, str(error)) from error
    items = _quarantine_items(records, require_reviewed=True)
    if items:
        reason = f"manifest validation quarantined {len(items)} item(s)"
        raise _record_promotion_failure(work_dir, reason)
    try:
        Draft202012Validator.check_schema(schema)
        Draft202012Validator(schema).validate(records)
    except (SchemaError, ValidationError) as error:
        reason = f"candidate does not match manifest schema: {error}"
        raise _record_promotion_failure(work_dir, reason) from error

    if tracker_text != render_tracker(records):
        raise _record_promotion_failure(
            work_dir, "candidate tracker is not the bounded rendering of the manifest"
        )
    try:
        rows = _parse_tracker(tracker_text)
    except ArtifactPromotionError as error:
        raise _record_promotion_failure(work_dir, str(error)) from error
    if len(rows) != len(records):
        raise _record_promotion_failure(
            work_dir, "tracker must contain exactly one row per manifest record"
        )
    expected_substages = [f"17.{number:03d}" for number in range(2, 182)]
    if [row[0] for row in rows] != expected_substages:
        raise _record_promotion_failure(
            work_dir, "tracker substages are not contiguous from 17.002 to 17.181"
        )
    for row, record in zip(rows, records, strict=True):
        expected = (
            row[0],
            str(record["code"]),
            _cell(record["title"]),
            str(record["pageStart"]),
            str(record["pageEnd"]),
            str(record["block"]),
            "[ ]",
        )
        if row != expected:
            raise _record_promotion_failure(
                work_dir,
                f"tracker/manifest mismatch at {row[0]} for record {record['code']}",
            )

    if before_publish is not None:
        before_publish()
    _replace_artifacts_durably_locked(
        work_dir,
        [
            (manifest_out, manifest_snapshot),
            (tracker_out, tracker_snapshot),
        ],
        protected_paths=[
            normative_pdf,
            schema_path,
            manifest_candidate,
            tracker_candidate,
            generation_path,
            build_state_path,
            quarantine_path,
            *evidence_paths,
        ],
        interrupt=interrupt,
    )
    return manifest_out, tracker_out


def apply_review_evidence(
    work_dir: Path,
    evidence_paths: list[Path],
    *,
    lock_timeout: float = _DEFAULT_LOCK_TIMEOUT,
) -> tuple[Path, Path]:
    work_dir = Path(work_dir)
    evidence_paths = [Path(path) for path in evidence_paths]
    initial_paths = [
        work_dir / "candidate" / "layout-12-manifest.json",
        work_dir / "candidate" / "STAGE_17_ECF_BASELINE.md",
        work_dir / "candidate" / "generation.json",
        work_dir / "build-state.json",
        work_dir / "quarantine.json",
        *_root_descriptor_paths(work_dir),
        *evidence_paths,
    ]
    with _artifact_lock(
        work_dir,
        targets=initial_paths[:5],
        timeout=lock_timeout,
        protected_paths=initial_paths,
    ):
        return _apply_review_evidence_locked(work_dir, evidence_paths)


def _apply_review_evidence_locked(
    work_dir: Path, evidence_paths: list[Path]
) -> tuple[Path, Path]:
    """Mark candidates reviewed only from complete, unambiguous visual attestations."""
    work_dir = Path(work_dir)
    _recover_artifact_transaction_locked(work_dir)
    manifest_path = work_dir / "candidate" / "layout-12-manifest.json"
    tracker_path = work_dir / "candidate" / "STAGE_17_ECF_BASELINE.md"
    generation_path = work_dir / "candidate" / "generation.json"
    build_state_path = work_dir / "build-state.json"
    quarantine_path = work_dir / "quarantine.json"
    evidence_paths = [Path(path) for path in evidence_paths]
    try:
        records = json.loads(manifest_path.read_text(encoding="utf-8"))
        tracker_text = tracker_path.read_text(encoding="utf-8")
        generation = json.loads(generation_path.read_text(encoding="utf-8"))
        build_state = json.loads(build_state_path.read_text(encoding="utf-8"))
        quarantine = json.loads(quarantine_path.read_text(encoding="utf-8"))
        evidence_payloads = [
            json.loads(path.read_text(encoding="utf-8")) for path in evidence_paths
        ]
    except (OSError, UnicodeError, json.JSONDecodeError) as error:
        raise ArtifactPromotionError(f"cannot load review evidence: {error}") from error
    _ensure_distinct_paths(
        [
            manifest_path,
            tracker_path,
            generation_path,
            build_state_path,
            quarantine_path,
            *_root_descriptor_paths(work_dir),
            *evidence_paths,
        ]
    )
    _validate_generation_provenance(
        records,
        tracker_text,
        generation,
        build_state,
        quarantine,
        required_state="candidate",
    )
    if not isinstance(records, list) or _quarantine_items(records, require_reviewed=False):
        raise ArtifactPromotionError("candidate manifest is not structurally valid")
    if tracker_text != render_tracker(records):
        raise ArtifactPromotionError("candidate tracker does not match candidate manifest")

    _validate_review_evidence(records, generation, evidence_payloads)
    reviewed = [{**record, "reviewed": True} for record in records]
    if _quarantine_items(reviewed, require_reviewed=True):
        raise ArtifactPromotionError("reviewed candidate failed final manifest validation")
    reviewed_manifest_data = _pretty_json_bytes(reviewed)
    reviewed_tracker_data = render_tracker(reviewed).encode("utf-8")
    reviewed_generation = {
        **generation,
        "state": "reviewed",
        "candidateSha256": canonical_candidate_sha256(reviewed),
        "trackerSha256": sha256_bytes(reviewed_tracker_data),
        "evidencePaths": [str(path.resolve()) for path in evidence_paths],
    }
    _replace_artifacts_durably_locked(
        work_dir,
        [
            (manifest_path, reviewed_manifest_data),
            (tracker_path, reviewed_tracker_data),
            (generation_path, _pretty_json_bytes(reviewed_generation)),
        ],
        protected_paths=[
            Path(generation["pdfPath"]),
            build_state_path,
            quarantine_path,
            *evidence_paths,
        ],
    )
    return manifest_path, tracker_path


def _parse_tracker(text: str) -> list[tuple[str, str, str, str, str, str, str]]:
    rows = []
    for line in text.splitlines():
        if not line.startswith("| 17."):
            continue
        cells = tuple(cell.strip() for cell in line.strip().strip("|").split("|"))
        if len(cells) != 7:
            raise ArtifactPromotionError("tracker rows must contain exactly seven cells")
        rows.append(cells)
    return rows


def _record_promotion_failure(work_dir: Path, reason: str) -> ArtifactPromotionError:
    _invalidate_generation_locked(work_dir, reason)
    return ArtifactPromotionError(reason)


def _load_json_snapshot(data: bytes) -> object:
    return json.loads(data.decode("utf-8"))


def _validate_generation_metadata(generation: object, *, required_state: str) -> None:
    generation_keys = {
        "schemaVersion",
        "generationId",
        "state",
        "pdfPath",
        "pdfSha256",
        "reviewCandidateSha256",
        "candidateSha256",
        "trackerSha256",
        "evidencePaths",
    }
    if not isinstance(generation, dict) or set(generation) != generation_keys:
        raise ArtifactPromotionError("candidate generation metadata is malformed")
    if (
        generation.get("schemaVersion") != 1
        or generation.get("state") != required_state
        or not all(
            isinstance(generation.get(key), str) and len(generation[key]) == 64
            for key in (
                "generationId",
                "pdfSha256",
                "reviewCandidateSha256",
                "candidateSha256",
                "trackerSha256",
            )
        )
        or not isinstance(generation.get("pdfPath"), str)
        or not isinstance(generation.get("evidencePaths"), list)
        or any(not isinstance(path, str) for path in generation["evidencePaths"])
    ):
        raise ArtifactPromotionError("candidate generation metadata has invalid values")


def _validate_generation_provenance(
    records: object,
    tracker_text: str,
    generation: object,
    build_state: object,
    quarantine: object,
    *,
    required_state: str,
) -> None:
    _validate_generation_metadata(generation, required_state=required_state)
    assert isinstance(generation, dict)
    if build_state != {
        "state": "valid",
        "generationId": generation["generationId"],
    }:
        raise ArtifactPromotionError("current build state does not authorize this generation")
    if quarantine != {"items": []}:
        raise ArtifactPromotionError("quarantine must be empty for the current generation")
    if not isinstance(records, list):
        raise ArtifactPromotionError("candidate manifest root must be an array")
    candidate_sha256 = canonical_candidate_sha256(records)
    if candidate_sha256 != generation["candidateSha256"]:
        raise ArtifactPromotionError("candidate content digest does not match generation metadata")
    review_records = [{**record, "reviewed": False} for record in records]
    review_sha256 = canonical_candidate_sha256(review_records)
    if review_sha256 != generation["reviewCandidateSha256"]:
        raise ArtifactPromotionError("review candidate digest does not match current content")
    if generation_id(generation["pdfSha256"], review_sha256) != generation["generationId"]:
        raise ArtifactPromotionError("candidate generation identifier is inconsistent")
    pdf = Path(generation["pdfPath"])
    if not pdf.is_file() or sha256_file(pdf) != generation["pdfSha256"]:
        raise ArtifactPromotionError("normative PDF digest does not match candidate generation")
    if sha256_bytes(tracker_text.encode("utf-8")) != generation["trackerSha256"]:
        raise ArtifactPromotionError("candidate tracker digest does not match generation metadata")
    if required_state == "candidate":
        if any(record.get("reviewed") is not False for record in records):
            raise ArtifactPromotionError("unreviewed candidate generation has reviewed records")
        if generation["evidencePaths"] != []:
            raise ArtifactPromotionError("candidate generation unexpectedly names review evidence")
    elif required_state == "reviewed":
        if any(record.get("reviewed") is not True for record in records):
            raise ArtifactPromotionError("reviewed generation contains unreviewed records")
        if not generation["evidencePaths"]:
            raise ArtifactPromotionError("reviewed generation has no bound evidence")


def _validate_review_evidence(
    records: list[dict], generation: dict, evidence_payloads: list[object]
) -> None:
    seen: set[int] = set()
    for payload in evidence_payloads:
        if not isinstance(payload, dict):
            raise ArtifactPromotionError("review evidence root must be an object")
        if (
            payload.get("generationId") != generation["generationId"]
            or payload.get("pdfSha256") != generation["pdfSha256"]
            or payload.get("candidateSha256")
            != generation["reviewCandidateSha256"]
        ):
            raise ArtifactPromotionError("review evidence provenance does not match candidate")
        reviewed_records = payload.get("records")
        bounds = payload.get("range")
        if (
            payload.get("allPagesOpened") is not True
            or payload.get("ambiguities") != []
            or not isinstance(reviewed_records, list)
            or not isinstance(bounds, dict)
            or not reviewed_records
        ):
            raise ArtifactPromotionError("review evidence is incomplete or ambiguous")
        indices = [item.get("index") for item in reviewed_records if isinstance(item, dict)]
        if len(indices) != len(reviewed_records) or any(
            not isinstance(index, int) for index in indices
        ):
            raise ArtifactPromotionError("every review record must have an integer index")
        if bounds != {"startIndex": min(indices), "endIndex": max(indices)}:
            raise ArtifactPromotionError("review evidence range does not match its records")
        pages = set()
        for item, index in zip(reviewed_records, indices, strict=True):
            if index in seen or not 1 <= index <= len(records):
                raise ArtifactPromotionError(
                    "review evidence indices must be unique and in range"
                )
            record = records[index - 1]
            expected = {
                "index": index,
                "code": record["code"],
                "pageStart": record["pageStart"],
                "pageEnd": record["pageEnd"],
                "reviewed": True,
            }
            if not all(item.get(key) == value for key, value in expected.items()):
                raise ArtifactPromotionError(
                    f"review evidence does not match candidate record at index {index}"
                )
            if not isinstance(item.get("note"), str) or not item["note"].strip():
                raise ArtifactPromotionError("every reviewed record must have a concise note")
            pages.update(range(record["pageStart"], record["pageEnd"] + 1))
            seen.add(index)
        if payload.get("uniquePagesOpened") != len(pages):
            raise ArtifactPromotionError(
                "review evidence page count does not match record ranges"
            )
    if seen != set(range(1, len(records) + 1)):
        raise ArtifactPromotionError("review evidence must cover every candidate exactly once")


def _cell(value: object) -> str:
    return " ".join(str(value).split()).replace("|", "&#124;")


@contextmanager
def _artifact_lock(
    work_dir: Path,
    *,
    targets: list[Path] | None = None,
    timeout: float = _DEFAULT_LOCK_TIMEOUT,
    protected_paths: list[Path] | None = None,
) -> Iterator[None]:
    """Serialize local recovery plus every shared destination directory."""
    if timeout < 0:
        raise ValueError("artifact lock timeout must be non-negative")
    work_dir = Path(work_dir)
    work_dir.mkdir(parents=True, exist_ok=True)
    targets = [Path(path) for path in (targets or [])]
    protected_paths = [Path(path) for path in (protected_paths or [])]
    deadline = time.monotonic() + timeout
    while True:
        hinted_targets = _recovery_target_hints(work_dir)
        lock_directories = _artifact_lock_directories(
            work_dir, [*targets, *hinted_targets]
        )
        with ExitStack() as stack:
            for directory in lock_directories:
                stack.enter_context(
                    _directory_lock(
                        directory,
                        deadline=deadline,
                        protected_paths=protected_paths,
                    )
                )
            current_targets = _recovery_target_hints(work_dir)
            required_directories = _artifact_lock_directories(
                work_dir, [*targets, *current_targets]
            )
            locked = {
                os.path.normcase(str(directory.resolve()))
                for directory in lock_directories
            }
            required = {
                os.path.normcase(str(directory.resolve()))
                for directory in required_directories
            }
            if not required.issubset(locked):
                if time.monotonic() >= deadline:
                    raise ArtifactLockTimeout(
                        "timed out while artifact recovery destinations changed"
                    )
                continue
            yield
            return


def _artifact_lock_directories(work_dir: Path, targets: list[Path]) -> list[Path]:
    directories = [Path(work_dir)]
    for target in targets:
        parent = Path(target).parent
        parent.mkdir(parents=True, exist_ok=True)
        directories.append(parent)
    unique = {
        os.path.normcase(str(directory.resolve())): directory.resolve()
        for directory in directories
    }
    return [unique[key] for key in sorted(unique)]


def _recovery_target_hints(work_dir: Path) -> list[Path]:
    transaction_root = Path(work_dir) / _TRANSACTION_DIR_NAME
    if not transaction_root.is_dir() or transaction_root.is_symlink():
        return []
    targets: list[Path] = []
    try:
        transaction_dirs = list(transaction_root.iterdir())
    except OSError:
        return []
    for transaction_dir in transaction_dirs:
        if not transaction_dir.is_dir() or transaction_dir.is_symlink():
            continue
        try:
            intent = _load_transaction_intent(work_dir, transaction_dir)
        except (ArtifactPromotionError, OSError):
            continue
        targets.extend(Path(path) for path in intent["targets"])
    return targets


@contextmanager
def _directory_lock(
    directory: Path,
    *,
    deadline: float,
    protected_paths: list[Path],
) -> Iterator[None]:
    if _IS_WINDOWS:
        with _windows_directory_lock(
            directory, deadline=deadline, protected_paths=protected_paths
        ):
            yield
        return
    with _posix_directory_lock(directory, deadline=deadline):
        yield


@contextmanager
def _windows_directory_lock(
    directory: Path,
    *,
    deadline: float,
    protected_paths: list[Path],
) -> Iterator[None]:
    lock_path = Path(directory) / _LOCK_NAME
    _ensure_distinct_paths([lock_path, *protected_paths])
    descriptor = _open_verified_lock_file(lock_path)
    acquired = False
    try:
        while True:
            try:
                _try_windows_lock(descriptor)
                acquired = True
                _verify_lock_identity(descriptor, lock_path)
                break
            except OSError as error:
                _raise_lock_timeout_or_wait(deadline, lock_path, error)
        yield
    finally:
        if acquired:
            _release_windows_lock(descriptor)
        os.close(descriptor)


@contextmanager
def _posix_directory_lock(directory: Path, *, deadline: float) -> Iterator[None]:
    flags = os.O_RDONLY | getattr(os, "O_DIRECTORY", 0)
    descriptor = os.open(directory, flags)
    acquired = False
    try:
        metadata = os.fstat(descriptor)
        if not stat.S_ISDIR(metadata.st_mode):
            raise ArtifactPathError(f"artifact lock identity is not a directory: {directory}")
        while True:
            try:
                _try_posix_lock(descriptor)
                acquired = True
                current = os.stat(directory)
                if (metadata.st_dev, metadata.st_ino) != (
                    current.st_dev,
                    current.st_ino,
                ):
                    raise ArtifactPathError(
                        f"artifact lock directory identity changed: {directory}"
                    )
                break
            except OSError as error:
                _raise_lock_timeout_or_wait(deadline, directory, error)
        yield
    finally:
        if acquired:
            _release_posix_lock(descriptor)
        os.close(descriptor)


def _raise_lock_timeout_or_wait(
    deadline: float, identity: Path, error: OSError
) -> None:
    if time.monotonic() >= deadline:
        raise ArtifactLockTimeout(
            f"timed out waiting for artifact lock: {identity}"
        ) from error
    time.sleep(min(0.01, max(0.0, deadline - time.monotonic())))


def _open_verified_lock_file(path: Path) -> int:
    flags = os.O_RDWR | getattr(os, "O_BINARY", 0)
    try:
        descriptor = os.open(path, flags | os.O_CREAT | os.O_EXCL, 0o600)
    except FileExistsError:
        if path.is_symlink():
            raise ArtifactPathError(f"artifact lock path is a symlink: {path}")
        descriptor = os.open(path, flags | getattr(os, "O_NOFOLLOW", 0))
        metadata = os.fstat(descriptor)
        if not stat.S_ISREG(metadata.st_mode) or metadata.st_nlink != 1:
            os.close(descriptor)
            raise ArtifactPathError(f"artifact lock path is not an owned regular file: {path}")
        return descriptor
    try:
        remaining = memoryview(_LOCK_MAGIC)
        while remaining:
            written = os.write(descriptor, remaining)
            if written == 0:
                raise OSError("could not initialize artifact lock identity")
            remaining = remaining[written:]
        os.fsync(descriptor)
        _sync_directory(path.parent)
        return descriptor
    except BaseException:
        os.close(descriptor)
        path.unlink(missing_ok=True)
        raise


def _verify_lock_identity(descriptor: int, path: Path) -> None:
    metadata = os.fstat(descriptor)
    if not stat.S_ISREG(metadata.st_mode) or metadata.st_nlink != 1:
        raise ArtifactPathError(f"artifact lock path is not an owned regular file: {path}")
    os.lseek(descriptor, 0, os.SEEK_SET)
    if os.read(descriptor, len(_LOCK_MAGIC) + 1) != _LOCK_MAGIC:
        raise ArtifactPathError(f"artifact lock identity is invalid: {path}")


def _try_windows_lock(descriptor: int) -> None:
    os.lseek(descriptor, 0, os.SEEK_SET)
    import msvcrt

    msvcrt.locking(descriptor, msvcrt.LK_NBLCK, 1)


def _release_windows_lock(descriptor: int) -> None:
    os.lseek(descriptor, 0, os.SEEK_SET)
    import msvcrt

    msvcrt.locking(descriptor, msvcrt.LK_UNLCK, 1)


def _try_posix_lock(descriptor: int) -> None:
    import fcntl

    fcntl.flock(descriptor, fcntl.LOCK_EX | fcntl.LOCK_NB)


def _release_posix_lock(descriptor: int) -> None:
    import fcntl

    fcntl.flock(descriptor, fcntl.LOCK_UN)


def replace_artifacts_durably(
    work_dir: Path,
    replacements: list[tuple[Path, bytes]],
    *,
    interrupt: InterruptHook | None = None,
    protected_paths: list[Path] | None = None,
    lock_timeout: float = _DEFAULT_LOCK_TIMEOUT,
) -> None:
    work_dir = Path(work_dir)
    protected_paths = [Path(path) for path in (protected_paths or [])]
    targets = [Path(path) for path, _data in replacements]
    _preflight_output_destinations(targets)
    with _artifact_lock(
        work_dir,
        targets=targets,
        timeout=lock_timeout,
        protected_paths=[*targets, *protected_paths],
    ):
        _replace_artifacts_durably_locked(
            work_dir,
            replacements,
            interrupt=interrupt,
            protected_paths=protected_paths,
        )


def _replace_artifacts_durably_locked(
    work_dir: Path,
    replacements: list[tuple[Path, bytes]],
    *,
    interrupt: InterruptHook | None = None,
    protected_paths: list[Path] | None = None,
) -> None:
    """Publish artifacts with redundant, bounded roll-forward recovery material.

    The filesystem cannot atomically replace unrelated files. Before replacing any target,
    however, three independently flushed descriptors and two copies of every new payload exist.
    One descriptor or payload copy may disappear and tooling recovery can still converge the set.
    This is a bounded single-artifact-loss guarantee, not an arbitrary power-loss guarantee.
    """
    work_dir = Path(work_dir)
    protected_paths = [Path(path) for path in (protected_paths or [])]
    _recover_artifact_transaction_locked(work_dir)
    if not replacements:
        raise ArtifactPathError("a durable transaction requires at least one target")
    targets = [Path(path) for path, _data in replacements]
    _preflight_output_destinations(targets)
    transaction_root = work_dir / _TRANSACTION_DIR_NAME
    root_descriptors = _root_descriptor_paths(work_dir)
    _ensure_distinct_paths(
        [*targets, *root_descriptors, transaction_root, *protected_paths]
    )
    for target in targets:
        if _is_within(target, transaction_root):
            raise ArtifactPathError("artifact targets cannot be inside transaction storage")

    transaction_id = uuid.uuid4().hex
    transaction_dir = transaction_root / transaction_id
    _ensure_distinct_paths([transaction_dir, *targets, *protected_paths])
    descriptor_paths = [
        *root_descriptors,
        transaction_dir / _TRANSACTION_DESCRIPTOR_NAME,
    ]
    intent_path = transaction_dir / _TRANSACTION_INTENT_NAME
    entries: list[dict] = []
    base_payloads: list[list[Path]] = []
    base_target_states = _preflight_target_markers(
        targets, protected_paths=protected_paths
    )
    base_snapshots: list[bytes | None] = []
    for index, (target, data) in enumerate(replacements):
        target = Path(target)
        payload_paths = [
            transaction_dir / f"new-{index}.0.bin",
            target.parent
            / f".{target.name}.{transaction_id}.{index}.recovery.bin",
        ]
        base_payload_paths = [
            transaction_dir / f"old-{index}.0.bin",
            target.parent
            / f".{target.name}.{transaction_id}.{index}.rollback.bin",
        ]
        base_state = base_target_states[index]
        base_data = _committed_base_snapshot(target, base_state)
        base_snapshots.append(base_data)
        base_payloads.append(base_payload_paths if base_data is not None else [])
        entries.append(
            {
                "target": str(target.resolve()),
                "payloads": [str(path.resolve()) for path in payload_paths],
                "sha256": sha256_bytes(data),
            }
        )
    all_payloads = [
        Path(path)
        for entry in entries
        for path in entry["payloads"]
    ]
    all_payloads.extend(path for paths in base_payloads for path in paths)
    marker_paths = _pair_marker_paths(work_dir, targets)
    _ensure_distinct_paths(
        [
            *targets,
            *descriptor_paths,
            intent_path,
            *all_payloads,
            *marker_paths,
            *protected_paths,
        ]
    )
    _preflight_pair_markers(
        work_dir, targets, protected_paths=protected_paths
    )
    try:
        base_marker = _load_pair_marker_quorum(work_dir, targets)
        base_transaction_id = base_marker["transactionId"]
    except ArtifactPromotionError:
        base_transaction_id = None

    _ensure_private_directory(transaction_root, protected_paths=protected_paths)
    transaction_dir.mkdir(exist_ok=False)
    _sync_directory(transaction_dir.parent)
    _interrupt(interrupt, "after_transaction_mkdir")

    intent = {
        "schemaVersion": 2,
        "transactionId": transaction_id,
        "baseTransactionId": base_transaction_id,
        "targets": [str(path.resolve()) for path in targets],
        "baseTargets": [
            {
                "target": str(target.resolve()),
                "version": state["version"] if state is not None else 0,
                "transactionId": (
                    state["transactionId"] if state is not None else None
                ),
                "exists": state["exists"] if state is not None else snapshot is not None,
                "sha256": (
                    state["sha256"]
                    if state is not None
                    else (sha256_bytes(snapshot) if snapshot is not None else None)
                ),
            }
            for target, state, snapshot in zip(
                targets, base_target_states, base_snapshots, strict=True
            )
        ],
    }
    _create_durable_file_exclusive(
        intent_path, _pretty_json_bytes(intent), protected_paths=protected_paths
    )

    descriptor = {
        "schemaVersion": 2,
        "transactionId": transaction_id,
        "state": "staging",
        "descriptorPaths": [str(path.resolve()) for path in descriptor_paths],
        "entries": entries,
    }
    for index, path in enumerate(descriptor_paths):
        _create_durable_file_exclusive(
            path, _pretty_json_bytes(descriptor), protected_paths=protected_paths
        )
        _interrupt(interrupt, f"after_staging_descriptor_{index}")

    for (_target, data), entry in zip(replacements, entries, strict=True):
        for payload_path in entry["payloads"]:
            _create_durable_file_exclusive(
                Path(payload_path), data, protected_paths=protected_paths
            )
    for base_data, payload_paths in zip(
        base_snapshots, base_payloads, strict=True
    ):
        if base_data is None:
            continue
        for payload_path in payload_paths:
            _create_durable_file_exclusive(
                payload_path, base_data, protected_paths=protected_paths
            )
    _interrupt(interrupt, "after_payloads")

    descriptor["state"] = "prepared"
    for index, path in enumerate(descriptor_paths):
        _write_json_durably(path, descriptor, protected_paths=protected_paths)
        _interrupt(interrupt, f"after_descriptor_{index}")
    _interrupt(interrupt, "after_journal")
    for index, entry in enumerate(entries):
        _replace_target_from_payload(entry, protected_paths=protected_paths)
        _interrupt(interrupt, f"after_replace_{index}")

    descriptor["state"] = "committed"
    for index, path in enumerate(descriptor_paths):
        _write_json_durably(path, descriptor, protected_paths=protected_paths)
        _interrupt(interrupt, f"after_committed_descriptor_{index}")
    _interrupt(interrupt, "after_committed")

    _commit_target_markers(
        descriptor,
        intent,
        protected_paths=protected_paths,
        interrupt=interrupt,
    )

    _commit_pair_markers(
        work_dir,
        descriptor,
        protected_paths=protected_paths,
        interrupt=interrupt,
    )

    for index, path in enumerate(descriptor_paths[:2]):
        _unlink_durably(path)
        _interrupt(interrupt, f"after_removed_descriptor_{index}")
    _remove_payload_copies(entries)
    _unlink_durably(descriptor_paths[2])
    _interrupt(interrupt, "after_removed_descriptor_2")
    _interrupt(interrupt, "after_journal_removed")
    _remove_transaction_dir(transaction_dir, transaction_root)


def recover_artifact_transaction(
    work_dir: Path, *, lock_timeout: float = _DEFAULT_LOCK_TIMEOUT
) -> None:
    work_dir = Path(work_dir)
    with _artifact_lock(work_dir, timeout=lock_timeout):
        _recover_artifact_transaction_locked(work_dir)


def _recover_artifact_transaction_locked(work_dir: Path) -> None:
    """Recover one transaction without accepting a mixed or under-described set."""
    work_dir = Path(work_dir)
    transaction_root = work_dir / _TRANSACTION_DIR_NAME
    descriptor_candidates = [
        *_root_descriptor_paths(work_dir),
        *_internal_descriptor_candidates(transaction_root),
    ]
    existing_descriptors = [
        path for path in descriptor_candidates if path.exists() or path.is_symlink()
    ]
    if not existing_descriptors:
        _recover_or_reject_orphan_transactions(
            work_dir, transaction_root, invalid_descriptors=[]
        )
        return

    valid_descriptors: list[dict] = []
    for path in existing_descriptors:
        try:
            descriptor = _load_json_snapshot(_read_owned_regular_file(path))
            _validate_transaction_descriptor(work_dir, descriptor)
            assert isinstance(descriptor, dict)
            valid_descriptors.append(descriptor)
        except (
            ArtifactPromotionError,
            OSError,
            UnicodeError,
            json.JSONDecodeError,
        ):
            continue

    descriptor, state = _select_descriptor_quorum(work_dir, valid_descriptors)
    if descriptor is None:
        _recover_or_reject_orphan_transactions(
            work_dir,
            transaction_root,
            invalid_descriptors=existing_descriptors,
        )
        return
    descriptor = {**descriptor, "state": state}
    if state == "staging":
        _cleanup_transaction(descriptor)
        _remove_empty_transaction_directories(transaction_root)
        return
    if _discard_superseded_transaction(work_dir, descriptor):
        _remove_empty_transaction_directories(transaction_root)
        return

    intent = _load_transaction_intent(
        work_dir, transaction_root / descriptor["transactionId"]
    )

    for entry in descriptor["entries"]:
        data = _recoverable_payload(entry)
        if data is None:
            target = Path(entry["target"])
            if target.is_file() and sha256_file(target) == entry["sha256"]:
                continue
            raise ArtifactPromotionError(
                "artifact transaction lost every valid payload copy before recovery"
            )
        _write_bytes_durably_atomically(Path(entry["target"]), data)
    for entry in descriptor["entries"]:
        target = Path(entry["target"])
        if not target.is_file() or sha256_file(target) != entry["sha256"]:
            raise ArtifactPromotionError("artifact transaction recovery did not converge")
    _preflight_pair_markers(
        work_dir,
        [Path(entry["target"]) for entry in descriptor["entries"]],
        allow_incomplete=True,
    )
    _preflight_target_markers(
        [Path(entry["target"]) for entry in descriptor["entries"]],
        allow_incomplete=True,
    )
    _commit_target_markers(descriptor, intent)
    _commit_pair_markers(work_dir, descriptor)
    _cleanup_transaction(descriptor)
    _remove_empty_transaction_directories(transaction_root)


def _select_descriptor_quorum(
    work_dir: Path, descriptors: list[dict]
) -> tuple[dict | None, str]:
    groups: dict[str, list[dict]] = {}
    for descriptor in descriptors:
        groups.setdefault(_transaction_identity(descriptor), []).append(descriptor)
    identity_quorums = [group for group in groups.values() if len(group) >= 2]
    if len(identity_quorums) > 1:
        raise ArtifactPromotionError("multiple artifact descriptor identities reached quorum")
    if identity_quorums:
        selected = identity_quorums[0]
    if not identity_quorums and len(groups) == 1 and descriptors:
        selected = next(iter(groups.values()))
        marker = _find_pair_marker_by_transaction(
            work_dir,
            selected[0]["transactionId"],
            [Path(entry["target"]) for entry in selected[0]["entries"]],
        )
        if len(selected) == 1 and marker is not None:
            marker_descriptor = _marker_to_descriptor(work_dir, marker)
            if _transaction_identity(marker_descriptor) != _transaction_identity(selected[0]):
                raise ArtifactPromotionError(
                    "committed pair marker disagrees with surviving descriptor"
                )
            return marker_descriptor, "committed"
        if len(selected) == 1 and selected[0]["state"] != "staging":
            raise ArtifactPromotionError("artifact descriptor identity has no quorum")
    elif not identity_quorums and not descriptors:
        return None, "staging"
    elif not identity_quorums:
        corroborated: list[dict] = []
        for group in groups.values():
            marker = _find_pair_marker_by_transaction(
                work_dir,
                group[0]["transactionId"],
                [Path(entry["target"]) for entry in group[0]["entries"]],
            )
            if marker is None:
                continue
            marker_descriptor = _marker_to_descriptor(work_dir, marker)
            if _transaction_identity(marker_descriptor) == _transaction_identity(group[0]):
                corroborated.append(marker_descriptor)
        if len(corroborated) == 1:
            return corroborated[0], "committed"
        raise ArtifactPromotionError("artifact descriptor identity has no two-of-three quorum")

    state_counts: dict[str, int] = {}
    for descriptor in selected:
        state_counts[descriptor["state"]] = state_counts.get(descriptor["state"], 0) + 1
    state_quorums = [state for state, count in state_counts.items() if count >= 2]
    if len(state_quorums) == 1:
        state = state_quorums[0]
    else:
        state = max(
            state_counts,
            key=lambda value: {"staging": 0, "prepared": 1, "committed": 2}[value],
        )
    return selected[0], state


def _recover_or_reject_orphan_transactions(
    work_dir: Path,
    transaction_root: Path,
    *,
    invalid_descriptors: list[Path],
) -> None:
    if not transaction_root.is_dir():
        return
    recovered = False
    for transaction_dir in list(transaction_root.iterdir()):
        if not transaction_dir.is_dir() or transaction_dir.is_symlink():
            raise ArtifactPromotionError("unexpected artifact transaction storage entry")
        if not any(transaction_dir.iterdir()):
            transaction_dir.rmdir()
            _sync_directory(transaction_root)
            recovered = True
            continue
        intent = _load_transaction_intent(work_dir, transaction_dir)
        targets = [Path(path) for path in intent["targets"]]
        marker = _find_pair_marker_by_transaction(
            work_dir, transaction_dir.name, targets
        )
        if marker is not None:
            _verify_marker_targets(marker)
            _cleanup_invalid_transaction(
                work_dir,
                transaction_dir,
                targets,
                invalid_descriptors=invalid_descriptors,
            )
            recovered = True
            continue
        try:
            prior_marker = _load_pair_marker_quorum(work_dir, targets)
            _verify_marker_targets(prior_marker)
        except ArtifactPromotionError as error:
            _quarantine_unproven_transaction(transaction_dir, intent, str(error))
            raise ArtifactPromotionError(
                "orphan artifact recovery quarantined an unproven transaction; "
                "restore a committed target pair or inspect and remove its recovery material"
            ) from error
        _cleanup_invalid_transaction(
            work_dir,
            transaction_dir,
            targets,
            invalid_descriptors=invalid_descriptors,
        )
        recovered = True
    if transaction_root.exists() and not any(transaction_root.iterdir()):
        transaction_root.rmdir()
    if recovered:
        return


def _load_transaction_intent(work_dir: Path, transaction_dir: Path) -> dict:
    intent_path = transaction_dir / _TRANSACTION_INTENT_NAME
    try:
        intent = _load_json_snapshot(_read_owned_regular_file(intent_path))
    except (OSError, UnicodeError, json.JSONDecodeError) as error:
        raise ArtifactPromotionError(
            "orphan artifact recovery material has no usable transaction intent"
        ) from error
    if (
        not isinstance(intent, dict)
        or set(intent)
        != {
            "schemaVersion",
            "transactionId",
            "baseTransactionId",
            "targets",
            "baseTargets",
        }
        or intent.get("schemaVersion") != 2
        or intent.get("transactionId") != transaction_dir.name
        or not _is_lower_hex(intent.get("transactionId"), length=32)
        or (
            intent.get("baseTransactionId") is not None
            and not _is_lower_hex(intent.get("baseTransactionId"), length=32)
        )
        or not isinstance(intent.get("targets"), list)
        or not intent["targets"]
        or any(not isinstance(path, str) for path in intent["targets"])
        or not isinstance(intent.get("baseTargets"), list)
        or len(intent["baseTargets"]) != len(intent["targets"])
    ):
        raise ArtifactPromotionError("artifact transaction intent is malformed")
    for target_value, base in zip(
        intent["targets"], intent["baseTargets"], strict=True
    ):
        if (
            not isinstance(base, dict)
            or set(base)
            != {"target", "version", "transactionId", "exists", "sha256"}
            or not isinstance(base.get("target"), str)
            or Path(base["target"]).resolve() != Path(target_value).resolve()
            or not isinstance(base.get("version"), int)
            or isinstance(base.get("version"), bool)
            or base["version"] < 0
            or (
                base["version"] == 0
                and base.get("transactionId") is not None
            )
            or (
                base["version"] > 0
                and not _is_lower_hex(base.get("transactionId"), length=32)
            )
            or not isinstance(base.get("exists"), bool)
            or (
                base["exists"]
                and not _is_lower_hex(base.get("sha256"), length=64)
            )
            or (not base["exists"] and base.get("sha256") is not None)
        ):
            raise ArtifactPromotionError("artifact transaction base target is malformed")
    targets = [Path(path) for path in intent["targets"]]
    transaction_root = Path(work_dir) / _TRANSACTION_DIR_NAME
    _ensure_distinct_paths(
        [
            *targets,
            *_root_descriptor_paths(work_dir),
            transaction_root,
            transaction_dir,
            intent_path,
        ]
    )
    if any(_is_within(target, transaction_root) for target in targets):
        raise ArtifactPromotionError("artifact transaction intent target is reserved")
    return intent


def _discard_superseded_transaction(work_dir: Path, descriptor: dict) -> bool:
    transaction_id = descriptor["transactionId"]
    transaction_dir = (
        Path(work_dir) / _TRANSACTION_DIR_NAME / transaction_id
    )
    intent = _load_transaction_intent(work_dir, transaction_dir)
    descriptor_targets = [Path(entry["target"]) for entry in descriptor["entries"]]
    intent_targets = [Path(path) for path in intent["targets"]]
    if {
        os.path.normcase(str(path.resolve())) for path in descriptor_targets
    } != {
        os.path.normcase(str(path.resolve())) for path in intent_targets
    }:
        raise ArtifactPromotionError(
            "artifact transaction intent disagrees with descriptor targets"
        )
    base_by_target = {
        os.path.normcase(str(Path(base["target"]).resolve())): base
        for base in intent["baseTargets"]
    }
    classifications: list[tuple[dict, dict | None, str]] = []
    for entry in descriptor["entries"]:
        target = Path(entry["target"])
        base = base_by_target[os.path.normcase(str(target.resolve()))]
        marker = _load_target_marker_for_recovery(
            target, entry, base, transaction_id
        )
        if marker is None:
            if base["version"] != 0:
                raise ArtifactPromotionError(
                    "committed target identity disappeared during recovery"
                )
            classification = "base"
        elif (
            marker["version"] == base["version"]
            and marker["transactionId"] == base["transactionId"]
        ):
            classification = "base"
        elif (
            marker["version"] == base["version"] + 1
            and marker["transactionId"] == transaction_id
        ):
            classification = "this"
        elif marker["version"] > base["version"]:
            classification = "later"
        else:
            raise ArtifactPromotionError(
                "committed target identity is ambiguous during recovery"
            )
        classifications.append((entry, marker, classification))

    if not any(item[2] == "later" for item in classifications):
        return False

    for entry, marker, classification in classifications:
        if classification != "later":
            continue
        assert marker is not None
        target = Path(entry["target"])
        if (
            not marker["exists"]
            or not target.is_file()
            or target.is_symlink()
            or sha256_file(target) != marker["sha256"]
        ):
            raise ArtifactPromotionError(
                "later committed target identity does not match its live artifact"
            )

    rollback_payloads: dict[int, bytes] = {}
    for index, (entry, _marker, classification) in enumerate(classifications):
        if classification == "later":
            continue
        target = Path(entry["target"])
        base = base_by_target[os.path.normcase(str(target.resolve()))]
        if not base["exists"]:
            continue
        data = _recoverable_base_payload(descriptor, index, base)
        if data is None:
            raise ArtifactPromotionError(
                "stale artifact transaction lost every valid base payload copy"
            )
        rollback_payloads[index] = data

    for index, (entry, _marker, classification) in enumerate(classifications):
        if classification == "later":
            continue
        target = Path(entry["target"])
        base = base_by_target[os.path.normcase(str(target.resolve()))]
        if base["exists"]:
            _write_bytes_durably_atomically(target, rollback_payloads[index])
        elif target.exists() or target.is_symlink():
            _read_owned_regular_file(target)
            _unlink_durably(target)
        _restore_target_marker_base(base)

    _cleanup_invalid_transaction(
        work_dir,
        transaction_dir,
        intent_targets,
        invalid_descriptors=[Path(path) for path in descriptor["descriptorPaths"]],
    )
    return True


def _recoverable_base_payload(
    descriptor: dict, index: int, base: dict
) -> bytes | None:
    if not base["exists"]:
        return None
    entry = descriptor["entries"][index]
    target = Path(entry["target"])
    transaction_dir = (
        Path(descriptor["descriptorPaths"][2]).parent
    )
    paths = [
        transaction_dir / f"old-{index}.0.bin",
        target.parent
        / f".{target.name}.{descriptor['transactionId']}.{index}.rollback.bin",
    ]
    for path in paths:
        try:
            data = _read_owned_regular_file(path, protected_paths=[target])
        except (ArtifactPromotionError, OSError):
            continue
        if sha256_bytes(data) == base["sha256"]:
            return data
    if target.is_file() and not target.is_symlink():
        data = target.read_bytes()
        if sha256_bytes(data) == base["sha256"]:
            return data
    return None


def _restore_target_marker_base(base: dict) -> None:
    target = Path(base["target"])
    marker_paths = _target_marker_paths(target)
    if base["version"] == 0:
        for path in marker_paths:
            if path.exists() or path.is_symlink():
                _read_owned_regular_file(path, protected_paths=[target])
                _unlink_durably(path)
        return
    marker = _target_marker_from_base(base)
    _write_target_marker(marker, protected_paths=[target])


def _cleanup_invalid_transaction(
    work_dir: Path,
    transaction_dir: Path,
    targets: list[Path],
    *,
    invalid_descriptors: list[Path],
) -> None:
    transaction_id = transaction_dir.name
    cleanup_paths = [*invalid_descriptors]
    for index, target in enumerate(targets):
        cleanup_paths.append(
            target.parent
            / f".{target.name}.{transaction_id}.{index}.recovery.bin"
        )
        cleanup_paths.append(
            target.parent
            / f".{target.name}.{transaction_id}.{index}.rollback.bin"
        )
    for path in cleanup_paths:
        if not path.exists() and not path.is_symlink():
            continue
        _read_owned_regular_file(path, protected_paths=targets)
        _unlink_durably(path)
    transaction_root = Path(work_dir) / _TRANSACTION_DIR_NAME
    _remove_transaction_dir(transaction_dir, transaction_root)


def _quarantine_unproven_transaction(
    transaction_dir: Path, intent: dict, detail: str
) -> None:
    quarantine_path = transaction_dir / _RECOVERY_QUARANTINE_NAME
    if quarantine_path.exists() or quarantine_path.is_symlink():
        _read_owned_regular_file(quarantine_path)
        return
    payload = {
        "state": "blocked",
        "transactionId": intent["transactionId"],
        "targets": intent["targets"],
        "reason": detail,
    }
    _create_durable_file_exclusive(quarantine_path, _pretty_json_bytes(payload))


def _root_descriptor_paths(work_dir: Path) -> list[Path]:
    return [
        Path(work_dir) / _JOURNAL_NAME,
        Path(work_dir) / _JOURNAL_BACKUP_NAME,
    ]


def _target_id(target: Path) -> str:
    identity = os.path.normcase(str(Path(target).resolve())) + "\n"
    return sha256_bytes(identity.encode("utf-8"))


def _target_marker_paths(target: Path) -> list[Path]:
    target = Path(target).resolve()
    marker_dir = target.parent / _TARGET_MARKER_DIR_NAME / _target_id(target)
    return [marker_dir / f"marker-{index}.json" for index in range(3)]


def _validate_target_marker(marker: object, target: Path) -> None:
    target = Path(target).resolve()
    if (
        not isinstance(marker, dict)
        or set(marker)
        != {
            "schemaVersion",
            "targetId",
            "target",
            "transactionId",
            "version",
            "exists",
            "sha256",
            "markerPaths",
        }
        or marker.get("schemaVersion") != 1
        or marker.get("targetId") != _target_id(target)
        or not isinstance(marker.get("target"), str)
        or Path(marker["target"]).resolve() != target
        or not _is_lower_hex(marker.get("transactionId"), length=32)
        or not isinstance(marker.get("version"), int)
        or isinstance(marker.get("version"), bool)
        or marker["version"] < 1
        or not isinstance(marker.get("exists"), bool)
        or (
            marker["exists"]
            and not _is_lower_hex(marker.get("sha256"), length=64)
        )
        or (not marker["exists"] and marker.get("sha256") is not None)
        or not isinstance(marker.get("markerPaths"), list)
        or len(marker["markerPaths"]) != 3
        or any(not isinstance(path, str) for path in marker["markerPaths"])
    ):
        raise ArtifactPromotionError("committed target marker is malformed")
    expected_paths = _target_marker_paths(target)
    if [Path(path).resolve() for path in marker["markerPaths"]] != [
        path.resolve() for path in expected_paths
    ]:
        raise ArtifactPromotionError("committed target marker paths are invalid")


def _load_target_marker_quorum(target: Path) -> dict:
    target = Path(target)
    valid: list[dict] = []
    for path in _target_marker_paths(target):
        if not path.is_file() or path.is_symlink():
            continue
        try:
            marker = _load_json_snapshot(_read_owned_regular_file(path))
            _validate_target_marker(marker, target)
            assert isinstance(marker, dict)
            valid.append(marker)
        except (ArtifactPromotionError, OSError, UnicodeError, json.JSONDecodeError):
            continue
    groups: dict[str, list[dict]] = {}
    for marker in valid:
        groups.setdefault(_marker_identity(marker), []).append(marker)
    quorum = [group for group in groups.values() if len(group) >= 2]
    if len(quorum) != 1:
        raise ArtifactPromotionError("committed target identity has no two-of-three quorum")
    return quorum[0][0]


def _preflight_target_markers(
    targets: list[Path],
    *,
    protected_paths: list[Path] | None = None,
    allow_incomplete: bool = False,
) -> list[dict | None]:
    protected_paths = [Path(path) for path in (protected_paths or [])]
    states: list[dict | None] = []
    for target in targets:
        marker_paths = _target_marker_paths(target)
        _ensure_distinct_paths([*marker_paths, *targets, *protected_paths])
        marker_root = marker_paths[0].parent.parent
        _ensure_private_directory(marker_root, protected_paths=protected_paths)
        _ensure_private_directory(marker_paths[0].parent, protected_paths=protected_paths)
        if not any(path.exists() or path.is_symlink() for path in marker_paths):
            states.append(None)
            continue
        try:
            states.append(_load_target_marker_quorum(target))
        except ArtifactPromotionError:
            if not allow_incomplete:
                raise
            states.append(None)
    return states


def _committed_base_snapshot(target: Path, marker: dict | None) -> bytes | None:
    target = Path(target)
    if marker is None:
        if not target.exists():
            return None
        return _read_owned_regular_file(target)
    if not marker["exists"]:
        return None
    try:
        data = _read_owned_regular_file(target)
    except OSError:
        data = None
    if data is not None and sha256_bytes(data) == marker["sha256"]:
        return data

    pattern = f".{target.name}.*.*.rollback.bin"
    for candidate in target.parent.glob(pattern):
        try:
            candidate_data = _read_owned_regular_file(candidate, protected_paths=[target])
        except (ArtifactPromotionError, OSError):
            continue
        if sha256_bytes(candidate_data) == marker["sha256"]:
            return candidate_data
    raise ArtifactPromotionError(
        "committed target identity does not have a recoverable base snapshot"
    )


def _target_marker_from_base(base: dict) -> dict:
    target = Path(base["target"])
    marker_paths = _target_marker_paths(target)
    return {
        "schemaVersion": 1,
        "targetId": _target_id(target),
        "target": str(target.resolve()),
        "transactionId": base["transactionId"],
        "version": base["version"],
        "exists": base["exists"],
        "sha256": base["sha256"],
        "markerPaths": [str(path.resolve()) for path in marker_paths],
    }


def _write_target_marker(marker: dict, *, protected_paths: list[Path] | None = None) -> None:
    protected_paths = [Path(path) for path in (protected_paths or [])]
    data = _pretty_json_bytes(marker)
    for path_value in marker["markerPaths"]:
        path = Path(path_value)
        if path.exists() or path.is_symlink():
            _read_owned_regular_file(path, protected_paths=protected_paths)
            _write_bytes_durably_atomically(path, data, protected_paths=protected_paths)
        else:
            _create_durable_file_exclusive(path, data, protected_paths=protected_paths)


def _commit_target_markers(
    descriptor: dict,
    intent: dict,
    *,
    protected_paths: list[Path] | None = None,
    interrupt: InterruptHook | None = None,
) -> None:
    bases = {
        os.path.normcase(str(Path(base["target"]).resolve())): base
        for base in intent["baseTargets"]
    }
    for entry_index, entry in enumerate(descriptor["entries"]):
        target = Path(entry["target"])
        base = bases[os.path.normcase(str(target.resolve()))]
        marker = _target_marker_for_commit(
            entry, base, descriptor["transactionId"]
        )
        marker_paths = [Path(path) for path in marker["markerPaths"]]
        data = _pretty_json_bytes(marker)
        for replica_index, path in enumerate(marker_paths):
            if path.exists() or path.is_symlink():
                _read_owned_regular_file(path, protected_paths=protected_paths)
                _write_bytes_durably_atomically(
                    path, data, protected_paths=protected_paths
                )
            else:
                _create_durable_file_exclusive(
                    path, data, protected_paths=protected_paths
                )
            _interrupt(
                interrupt,
                f"after_target_marker_{entry_index}_{replica_index}",
            )


def _target_marker_for_commit(entry: dict, base: dict, transaction_id: str) -> dict:
    target = Path(entry["target"])
    marker_paths = _target_marker_paths(target)
    return {
        "schemaVersion": 1,
        "targetId": _target_id(target),
        "target": str(target.resolve()),
        "transactionId": transaction_id,
        "version": base["version"] + 1,
        "exists": True,
        "sha256": entry["sha256"],
        "markerPaths": [str(path.resolve()) for path in marker_paths],
    }


def _load_target_marker_for_recovery(
    target: Path, entry: dict, base: dict, transaction_id: str
) -> dict | None:
    marker_paths = _target_marker_paths(target)
    if not any(path.exists() or path.is_symlink() for path in marker_paths):
        return None
    try:
        return _load_target_marker_quorum(target)
    except ArtifactPromotionError as quorum_error:
        valid: list[dict] = []
        for path in marker_paths:
            if not path.exists() and not path.is_symlink():
                continue
            try:
                marker = _load_json_snapshot(_read_owned_regular_file(path))
                _validate_target_marker(marker, target)
                assert isinstance(marker, dict)
                valid.append(marker)
            except (ArtifactPromotionError, OSError, UnicodeError, json.JSONDecodeError):
                raise ArtifactPromotionError(
                    "incomplete committed target identity is not recoverable"
                ) from quorum_error
        expected = _target_marker_for_commit(entry, base, transaction_id)
        allowed = {_marker_identity(expected)}
        if base["version"] > 0:
            allowed.add(_marker_identity(_target_marker_from_base(base)))
        if not valid or any(_marker_identity(marker) not in allowed for marker in valid):
            raise ArtifactPromotionError(
                "incomplete committed target identity is not corroborated"
            ) from quorum_error
        own = [
            marker
            for marker in valid
            if marker["transactionId"] == transaction_id
        ]
        return own[0] if own else valid[0]


def _pair_set_id(targets: list[Path]) -> str:
    identity = "\n".join(
        sorted(os.path.normcase(str(Path(target).resolve())) for target in targets)
    ) + "\n"
    return sha256_bytes(identity.encode("utf-8"))


def _pair_marker_paths(work_dir: Path, targets: list[Path]) -> list[Path]:
    del work_dir
    canonical_targets = sorted(
        (Path(target).resolve() for target in targets),
        key=lambda path: os.path.normcase(str(path)),
    )
    if not canonical_targets:
        raise ArtifactPathError("committed pair identity requires at least one target")
    marker_dir = (
        canonical_targets[0].parent
        / _PAIR_MARKER_DIR_NAME
        / _pair_set_id(canonical_targets)
    )
    return [marker_dir / f"marker-{index}.json" for index in range(3)]


def _preflight_pair_markers(
    work_dir: Path,
    targets: list[Path],
    *,
    protected_paths: list[Path] | None = None,
    allow_incomplete: bool = False,
) -> None:
    protected_paths = [Path(path) for path in (protected_paths or [])]
    marker_paths = _pair_marker_paths(work_dir, targets)
    _ensure_distinct_paths([*marker_paths, *targets, *protected_paths])
    marker_root = marker_paths[0].parent.parent
    _ensure_private_directory(marker_root, protected_paths=protected_paths)
    _ensure_private_directory(marker_paths[0].parent, protected_paths=protected_paths)
    existing_count = 0
    valid: list[dict] = []
    for marker_path in marker_paths:
        if not marker_path.exists() and not marker_path.is_symlink():
            continue
        existing_count += 1
        data = _read_owned_regular_file(
            marker_path, protected_paths=protected_paths
        )
        try:
            payload = _load_json_snapshot(data)
            _validate_pair_marker(work_dir, payload, expected_targets=targets)
            assert isinstance(payload, dict)
            valid.append(payload)
        except (ArtifactPromotionError, UnicodeError, json.JSONDecodeError):
            continue
    if existing_count and not allow_incomplete:
        groups: dict[str, list[dict]] = {}
        for marker in valid:
            groups.setdefault(_marker_identity(marker), []).append(marker)
        if not any(len(group) >= 2 for group in groups.values()):
            raise ArtifactPromotionError(
                "existing committed pair markers have no recoverable quorum"
            )


def _commit_pair_markers(
    work_dir: Path,
    descriptor: dict,
    *,
    protected_paths: list[Path] | None = None,
    interrupt: InterruptHook | None = None,
) -> None:
    protected_paths = [Path(path) for path in (protected_paths or [])]
    targets = [Path(entry["target"]) for entry in descriptor["entries"]]
    marker_paths = _pair_marker_paths(work_dir, targets)
    marker = {
        "schemaVersion": 1,
        "setId": _pair_set_id(targets),
        "transactionId": descriptor["transactionId"],
        "markerPaths": [str(path.resolve()) for path in marker_paths],
        "entries": [
            {"target": entry["target"], "sha256": entry["sha256"]}
            for entry in descriptor["entries"]
        ],
    }
    data = _pretty_json_bytes(marker)
    for index, path in enumerate(marker_paths):
        if path.exists() or path.is_symlink():
            _read_owned_regular_file(path, protected_paths=protected_paths)
            _write_bytes_durably_atomically(
                path, data, protected_paths=protected_paths
            )
        else:
            _create_durable_file_exclusive(
                path, data, protected_paths=protected_paths
            )
        _interrupt(interrupt, f"after_marker_{index}")


def _validate_pair_marker(
    work_dir: Path,
    marker: object,
    *,
    expected_targets: list[Path] | None = None,
) -> None:
    if (
        not isinstance(marker, dict)
        or set(marker)
        != {"schemaVersion", "setId", "transactionId", "markerPaths", "entries"}
        or marker.get("schemaVersion") != 1
        or not _is_lower_hex(marker.get("setId"), length=64)
        or not _is_lower_hex(marker.get("transactionId"), length=32)
        or not isinstance(marker.get("markerPaths"), list)
        or len(marker["markerPaths"]) != 3
        or any(not isinstance(path, str) for path in marker["markerPaths"])
        or not isinstance(marker.get("entries"), list)
        or not marker["entries"]
    ):
        raise ArtifactPromotionError("committed pair marker is malformed")
    targets: list[Path] = []
    for entry in marker["entries"]:
        if (
            not isinstance(entry, dict)
            or set(entry) != {"target", "sha256"}
            or not isinstance(entry.get("target"), str)
            or not _is_lower_hex(entry.get("sha256"), length=64)
        ):
            raise ArtifactPromotionError("committed pair marker entry is malformed")
        targets.append(Path(entry["target"]))
    if marker["setId"] != _pair_set_id(targets):
        raise ArtifactPromotionError("committed pair marker set identity is invalid")
    expected_paths = _pair_marker_paths(Path(work_dir), targets)
    if [Path(path).resolve() for path in marker["markerPaths"]] != [
        path.resolve() for path in expected_paths
    ]:
        raise ArtifactPromotionError("committed pair marker paths are invalid")
    if expected_targets is not None and {
        os.path.normcase(str(target.resolve())) for target in targets
    } != {
        os.path.normcase(str(Path(target).resolve())) for target in expected_targets
    }:
        raise ArtifactPromotionError("committed pair marker targets do not match")


def _load_pair_marker_quorum(
    work_dir: Path, targets: list[Path]
) -> dict:
    marker_paths = _pair_marker_paths(work_dir, targets)
    valid: list[dict] = []
    for path in marker_paths:
        if not path.is_file() or path.is_symlink():
            continue
        try:
            marker = _load_json_snapshot(_read_owned_regular_file(path))
            _validate_pair_marker(work_dir, marker, expected_targets=targets)
            assert isinstance(marker, dict)
            valid.append(marker)
        except (ArtifactPromotionError, OSError, UnicodeError, json.JSONDecodeError):
            continue
    groups: dict[str, list[dict]] = {}
    for marker in valid:
        groups.setdefault(_marker_identity(marker), []).append(marker)
    quorum = [group for group in groups.values() if len(group) >= 2]
    if len(quorum) != 1:
        raise ArtifactPromotionError("committed pair identity has no two-of-three quorum")
    return quorum[0][0]


def _marker_identity(marker: dict) -> str:
    return json.dumps(marker, ensure_ascii=False, sort_keys=True, separators=(",", ":"))


def _find_pair_marker_by_transaction(
    work_dir: Path, transaction_id: str, targets: list[Path]
) -> dict | None:
    try:
        marker = _load_pair_marker_quorum(work_dir, targets)
    except ArtifactPromotionError:
        return None
    return marker if marker["transactionId"] == transaction_id else None


def _marker_to_descriptor(work_dir: Path, marker: dict) -> dict:
    transaction_id = marker["transactionId"]
    transaction_dir = Path(work_dir) / _TRANSACTION_DIR_NAME / transaction_id
    descriptor_paths = [
        *_root_descriptor_paths(Path(work_dir)),
        transaction_dir / _TRANSACTION_DESCRIPTOR_NAME,
    ]
    entries = []
    for index, marker_entry in enumerate(marker["entries"]):
        target = Path(marker_entry["target"])
        entries.append(
            {
                "target": marker_entry["target"],
                "payloads": [
                    str((transaction_dir / f"new-{index}.0.bin").resolve()),
                    str(
                        (
                            target.parent
                            / f".{target.name}.{transaction_id}.{index}.recovery.bin"
                        ).resolve()
                    ),
                ],
                "sha256": marker_entry["sha256"],
            }
        )
    return {
        "schemaVersion": 2,
        "transactionId": transaction_id,
        "state": "committed",
        "descriptorPaths": [str(path.resolve()) for path in descriptor_paths],
        "entries": entries,
    }


def _verify_marker_targets(marker: dict) -> None:
    for entry in marker["entries"]:
        target = Path(entry["target"])
        if not target.is_file() or sha256_file(target) != entry["sha256"]:
            raise ArtifactPromotionError(
                "committed pair identity does not match live target hashes"
            )


def _internal_descriptor_candidates(transaction_root: Path) -> list[Path]:
    if not transaction_root.is_dir():
        return []
    return sorted(
        child / _TRANSACTION_DESCRIPTOR_NAME
        for child in transaction_root.iterdir()
        if child.is_dir()
    )


def _validate_transaction_descriptor(work_dir: Path, descriptor: object) -> None:
    if (
        not isinstance(descriptor, dict)
        or set(descriptor)
        != {"schemaVersion", "transactionId", "state", "descriptorPaths", "entries"}
        or descriptor.get("schemaVersion") != 2
        or not isinstance(descriptor.get("transactionId"), str)
        or not _is_lower_hex(descriptor["transactionId"], length=32)
        or descriptor.get("state") not in {"staging", "prepared", "committed"}
        or not isinstance(descriptor.get("descriptorPaths"), list)
        or len(descriptor["descriptorPaths"]) != 3
        or any(not isinstance(path, str) for path in descriptor["descriptorPaths"])
        or not isinstance(descriptor.get("entries"), list)
        or not descriptor["entries"]
    ):
        raise ArtifactPromotionError("artifact transaction descriptor is malformed")

    transaction_root = Path(work_dir) / _TRANSACTION_DIR_NAME
    transaction_dir = transaction_root / descriptor["transactionId"]
    expected_descriptors = [
        *_root_descriptor_paths(Path(work_dir)),
        transaction_dir / _TRANSACTION_DESCRIPTOR_NAME,
    ]
    descriptor_paths = [Path(path) for path in descriptor["descriptorPaths"]]
    if [path.resolve() for path in descriptor_paths] != [
        path.resolve() for path in expected_descriptors
    ]:
        raise ArtifactPromotionError("artifact transaction descriptor paths are invalid")

    targets: list[Path] = []
    payloads: list[Path] = []
    base_payloads: list[Path] = []
    for index, entry in enumerate(descriptor["entries"]):
        if (
            not isinstance(entry, dict)
            or set(entry) != {"target", "payloads", "sha256"}
            or not isinstance(entry.get("target"), str)
            or not isinstance(entry.get("payloads"), list)
            or len(entry["payloads"]) != 2
            or any(not isinstance(path, str) for path in entry["payloads"])
            or not _is_lower_hex(entry.get("sha256"), length=64)
        ):
            raise ArtifactPromotionError("artifact transaction entry is malformed")
        target = Path(entry["target"])
        entry_payloads = [Path(path) for path in entry["payloads"]]
        expected_payloads = [
            transaction_dir / f"new-{index}.0.bin",
            target.parent
            / f".{target.name}.{descriptor['transactionId']}.{index}.recovery.bin",
        ]
        if [path.resolve() for path in entry_payloads] != [
            path.resolve() for path in expected_payloads
        ]:
            raise ArtifactPromotionError("artifact transaction payload paths are invalid")
        targets.append(target)
        payloads.extend(entry_payloads)
        base_payloads.extend(
            [
                transaction_dir / f"old-{index}.0.bin",
                target.parent
                / (
                    f".{target.name}.{descriptor['transactionId']}."
                    f"{index}.rollback.bin"
                ),
            ]
        )
    _ensure_distinct_paths(
        [*targets, *descriptor_paths, *payloads, *base_payloads]
    )


def _transaction_identity(descriptor: dict) -> str:
    stable = {key: value for key, value in descriptor.items() if key != "state"}
    return json.dumps(stable, ensure_ascii=False, sort_keys=True, separators=(",", ":"))


def _is_lower_hex(value: object, *, length: int) -> bool:
    return (
        isinstance(value, str)
        and len(value) == length
        and all(character in "0123456789abcdef" for character in value)
    )


def _recoverable_payload(entry: dict) -> bytes | None:
    for path_value in entry["payloads"]:
        path = Path(path_value)
        try:
            data = path.read_bytes()
        except OSError:
            continue
        if sha256_bytes(data) == entry["sha256"]:
            return data
    return None


def _cleanup_transaction(descriptor: dict) -> None:
    descriptor_paths = [Path(path) for path in descriptor["descriptorPaths"]]
    for path in descriptor_paths[:2]:
        _unlink_durably(path)
    _remove_payload_copies(descriptor["entries"])
    _unlink_durably(descriptor_paths[2])
    transaction_root = descriptor_paths[2].parent.parent
    _remove_transaction_dir(descriptor_paths[2].parent, transaction_root)


def _remove_payload_copies(entries: list[dict]) -> None:
    for index, entry in enumerate(entries):
        for path_value in entry["payloads"]:
            _unlink_durably(Path(path_value))
        transaction_dir = Path(entry["payloads"][0]).parent
        target = Path(entry["target"])
        for path in (
            transaction_dir / f"old-{index}.0.bin",
            target.parent
            / f".{target.name}.{transaction_dir.name}.{index}.rollback.bin",
        ):
            _unlink_durably(path)


def _unlink_durably(path: Path) -> None:
    if path.exists():
        path.unlink()
        _sync_directory(path.parent)


def _remove_empty_transaction_directories(root: Path) -> None:
    if not root.is_dir():
        return
    for child in root.iterdir():
        if not child.is_dir() or not _is_within(child, root):
            raise ArtifactPromotionError("unexpected artifact transaction storage entry")
        if any(child.iterdir()):
            raise ArtifactPromotionError(
                "orphan artifact recovery material has no usable descriptor"
            )
        child.rmdir()
    if not any(root.iterdir()):
        root.rmdir()


def read_artifact_pair(
    work_dir: Path,
    first: Path,
    second: Path,
    *,
    lock_timeout: float = _DEFAULT_LOCK_TIMEOUT,
) -> tuple[bytes, bytes]:
    work_dir = Path(work_dir)
    first = Path(first)
    second = Path(second)
    with _artifact_lock(
        work_dir,
        targets=[first, second],
        timeout=lock_timeout,
        protected_paths=[first, second],
    ):
        _recover_artifact_transaction_locked(work_dir)
        _ensure_distinct_paths([first, second])
        if first.exists() != second.exists():
            raise ArtifactPromotionError("committed pair is mixed after recovery")
        try:
            snapshots = (first.read_bytes(), second.read_bytes())
        except OSError as error:
            raise ArtifactPromotionError("committed pair targets are missing") from error
        marker = _load_pair_marker_quorum(work_dir, [first, second])
        expected_hashes = {
            os.path.normcase(str(Path(entry["target"]).resolve())): entry["sha256"]
            for entry in marker["entries"]
        }
        live_hashes = {
            os.path.normcase(str(path.resolve())): sha256_bytes(snapshot)
            for path, snapshot in zip((first, second), snapshots, strict=True)
        }
        if live_hashes != expected_hashes:
            raise ArtifactPromotionError(
                "committed pair identity does not match live target hashes"
            )
        return snapshots


def invalidate_generation(
    work_dir: Path,
    reason: str,
    *,
    lock_timeout: float = _DEFAULT_LOCK_TIMEOUT,
) -> None:
    """Durably make every prior candidate generation non-promotable."""
    work_dir = Path(work_dir)
    targets = [work_dir / "build-state.json", work_dir / "quarantine.json"]
    _preflight_output_destinations(targets)
    with _artifact_lock(work_dir, targets=targets, timeout=lock_timeout):
        _recover_artifact_transaction_locked(work_dir)
        _invalidate_generation_locked(work_dir, reason)


def _invalidate_generation_locked(
    work_dir: Path, reason: str, *, protected_paths: list[Path] | None = None
) -> None:
    build_state = work_dir / "build-state.json"
    quarantine = work_dir / "quarantine.json"
    replacements = [
        (
            build_state,
            _pretty_json_bytes({"state": "invalid", "reason": reason}),
        ),
        (
            quarantine,
            _pretty_json_bytes(
                {"items": [{"code": None, "reasons": [reason], "pages": []}]}
            ),
        ),
    ]
    _replace_artifacts_durably_locked(
        work_dir, replacements, protected_paths=protected_paths
    )


def _replace_target_from_payload(
    entry: dict, *, protected_paths: list[Path] | None = None
) -> None:
    target = Path(entry["target"])
    data = _recoverable_payload(entry)
    if data is None:
        raise ArtifactPromotionError("artifact transaction has no valid payload copy")
    _write_bytes_durably_atomically(
        target, data, protected_paths=protected_paths
    )


def _pretty_json_bytes(payload: object) -> bytes:
    return (json.dumps(payload, ensure_ascii=False, indent=2) + "\n").encode("utf-8")


def _write_json_durably(
    path: Path, payload: object, *, protected_paths: list[Path] | None = None
) -> None:
    _read_owned_regular_file(path, protected_paths=protected_paths)
    _write_bytes_durably_atomically(
        path, _pretty_json_bytes(payload), protected_paths=protected_paths
    )


def _read_owned_regular_file(
    path: Path, *, protected_paths: list[Path] | None = None
) -> bytes:
    path = Path(path)
    protected_paths = [Path(value) for value in (protected_paths or [])]
    _ensure_distinct_paths([path, *protected_paths])
    if path.is_symlink():
        raise ArtifactPathError(f"dynamic artifact path is a symlink: {path}")
    metadata = path.stat()
    if not stat.S_ISREG(metadata.st_mode) or metadata.st_nlink != 1:
        raise ArtifactPathError(f"dynamic artifact path is not owned: {path}")
    return path.read_bytes()


def _preflight_output_destinations(paths: list[Path]) -> None:
    """Reject an existing final-component link before acquiring destination locks."""
    reparse_flag = getattr(stat, "FILE_ATTRIBUTE_REPARSE_POINT", 0x400)
    for value in paths:
        path = Path(value)
        try:
            metadata = path.lstat()
        except FileNotFoundError:
            continue
        attributes = getattr(metadata, "st_file_attributes", 0)
        if stat.S_ISLNK(metadata.st_mode) or attributes & reparse_flag:
            raise ArtifactPathError(
                f"artifact output destination is a symlink or reparse point: {path}"
            )


def _ensure_private_directory(
    path: Path, *, protected_paths: list[Path] | None = None
) -> None:
    path = Path(path)
    protected_paths = [Path(value) for value in (protected_paths or [])]
    _ensure_distinct_paths([path, *protected_paths])
    if path.exists() or path.is_symlink():
        if path.is_symlink() or not path.is_dir():
            raise ArtifactPathError(f"dynamic artifact directory is not owned: {path}")
        return
    path.mkdir(parents=True, exist_ok=False)
    _sync_directory(path.parent)


def _create_durable_file_exclusive(
    path: Path, data: bytes, *, protected_paths: list[Path] | None = None
) -> None:
    path = Path(path)
    protected_paths = [Path(value) for value in (protected_paths or [])]
    _ensure_distinct_paths([path, *protected_paths])
    path.parent.mkdir(parents=True, exist_ok=True)
    flags = os.O_WRONLY | os.O_CREAT | os.O_EXCL | getattr(os, "O_BINARY", 0)
    try:
        descriptor = os.open(path, flags, 0o600)
    except FileExistsError as error:
        raise ArtifactPathError(f"dynamic artifact path already exists: {path}") from error
    try:
        with os.fdopen(descriptor, "wb") as stream:
            stream.write(data)
            stream.flush()
            os.fsync(stream.fileno())
        _sync_directory(path.parent)
    except BaseException:
        path.unlink(missing_ok=True)
        raise


def _write_bytes_durably_atomically(
    path: Path,
    data: bytes,
    *,
    protected_paths: list[Path] | None = None,
    temporary_path: Path | None = None,
) -> None:
    path = Path(path)
    protected_paths = [Path(value) for value in (protected_paths or [])]
    path.parent.mkdir(parents=True, exist_ok=True)
    temporary = Path(temporary_path) if temporary_path else (
        path.parent / f".{path.name}.{uuid.uuid4().hex}.tmp"
    )
    _ensure_distinct_paths([path, temporary, *protected_paths])
    try:
        _create_durable_file_exclusive(
            temporary, data, protected_paths=[path, *protected_paths]
        )
        os.replace(temporary, path)
        _sync_directory(path.parent)
    finally:
        temporary.unlink(missing_ok=True)


def _sync_directory(path: Path) -> None:
    if os.name == "nt":
        return
    descriptor = os.open(path, os.O_RDONLY)
    try:
        os.fsync(descriptor)
    finally:
        os.close(descriptor)


def _ensure_distinct_paths(paths: list[Path]) -> None:
    normalized: dict[str, Path] = {}
    for value in paths:
        path = Path(value)
        key = os.path.normcase(str(path.resolve(strict=False)))
        if key in normalized:
            raise ArtifactPathError(f"artifact paths alias: {normalized[key]} and {path}")
        for existing in normalized.values():
            if path.exists() and existing.exists() and os.path.samefile(path, existing):
                raise ArtifactPathError(f"artifact paths alias: {existing} and {path}")
        normalized[key] = path


def _is_within(path: Path, root: Path) -> bool:
    try:
        Path(path).resolve(strict=False).relative_to(Path(root).resolve(strict=False))
        return True
    except ValueError:
        return False


def _remove_transaction_dir(path: Path, root: Path) -> None:
    if not _is_within(path, root) or path.resolve(strict=False) == root.resolve(strict=False):
        raise ArtifactPathError("refusing to remove transaction path outside reserved storage")
    if path.exists():
        shutil.rmtree(path)
    if root.exists() and not any(root.iterdir()):
        root.rmdir()


def _interrupt(hook: InterruptHook | None, boundary: str) -> None:
    if hook is not None:
        hook(boundary)
