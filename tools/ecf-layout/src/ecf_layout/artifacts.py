"""Build and transactionally promote the reviewed ECF manifest/tracker pair."""

from __future__ import annotations

import hashlib
import json
import os
import shutil
import stat
import time
import uuid
from contextlib import contextmanager
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
_PAIR_MARKER_DIR_NAME = ".artifact-pairs"
_LOCK_NAME = ".artifact.lock"
_LOCK_MAGIC = b"ecf-layout-artifact-lock-v1\n"
_DEFAULT_LOCK_TIMEOUT = 10.0


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
    with _artifact_lock(
        work_dir, timeout=lock_timeout, protected_paths=protected_paths
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
    with _artifact_lock(
        work_dir, timeout=lock_timeout, protected_paths=initial_paths
    ):
        return _promote_artifacts_locked(
            work_dir,
            Path(manifest_out),
            Path(tracker_out),
            schema_path=Path(schema_path),
            before_publish=before_publish,
        )


def _promote_artifacts_locked(
    work_dir: Path,
    manifest_out: Path,
    tracker_out: Path,
    *,
    schema_path: Path,
    before_publish: BeforePublishHook | None,
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
        work_dir, timeout=lock_timeout, protected_paths=initial_paths
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
    timeout: float = _DEFAULT_LOCK_TIMEOUT,
    protected_paths: list[Path] | None = None,
) -> Iterator[None]:
    """Serialize tooling readers/writers with an OS lock released on process death."""
    if timeout < 0:
        raise ValueError("artifact lock timeout must be non-negative")
    work_dir = Path(work_dir)
    work_dir.mkdir(parents=True, exist_ok=True)
    lock_path = work_dir / _LOCK_NAME
    protected_paths = [Path(path) for path in (protected_paths or [])]
    _ensure_distinct_paths([lock_path, *protected_paths])
    descriptor = _open_verified_lock_file(lock_path)
    deadline = time.monotonic() + timeout
    acquired = False
    try:
        while True:
            try:
                _try_os_lock(descriptor)
                acquired = True
                _verify_lock_identity(descriptor, lock_path)
                break
            except OSError as error:
                if time.monotonic() >= deadline:
                    raise ArtifactLockTimeout(
                        f"timed out waiting for artifact lock: {lock_path}"
                    ) from error
                time.sleep(min(0.01, max(0.0, deadline - time.monotonic())))
        yield
    finally:
        if acquired:
            _release_os_lock(descriptor)
        os.close(descriptor)


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


def _try_os_lock(descriptor: int) -> None:
    os.lseek(descriptor, 0, os.SEEK_SET)
    if os.name == "nt":
        import msvcrt

        msvcrt.locking(descriptor, msvcrt.LK_NBLCK, 1)
    else:
        import fcntl

        fcntl.flock(descriptor, fcntl.LOCK_EX | fcntl.LOCK_NB)


def _release_os_lock(descriptor: int) -> None:
    os.lseek(descriptor, 0, os.SEEK_SET)
    if os.name == "nt":
        import msvcrt

        msvcrt.locking(descriptor, msvcrt.LK_UNLCK, 1)
    else:
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
    with _artifact_lock(
        work_dir,
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
    _ensure_private_directory(transaction_root, protected_paths=protected_paths)
    _ensure_distinct_paths([transaction_dir, *targets, *protected_paths])
    transaction_dir.mkdir(exist_ok=False)
    _sync_directory(transaction_dir.parent)
    descriptor_paths = [
        *root_descriptors,
        transaction_dir / _TRANSACTION_DESCRIPTOR_NAME,
    ]
    entries: list[dict] = []
    for index, (target, data) in enumerate(replacements):
        target = Path(target)
        payload_paths = [
            transaction_dir / f"new-{index}.0.bin",
            target.parent
            / f".{target.name}.{transaction_id}.{index}.recovery.bin",
        ]
        entries.append(
            {
                "target": str(target.resolve()),
                "payloads": [str(path.resolve()) for path in payload_paths],
                "sha256": sha256_bytes(data),
            }
        )
    all_payloads = [Path(path) for entry in entries for path in entry["payloads"]]
    marker_paths = _pair_marker_paths(work_dir, targets)
    _ensure_distinct_paths(
        [
            *targets,
            *descriptor_paths,
            *all_payloads,
            *marker_paths,
            *protected_paths,
        ]
    )
    _preflight_pair_markers(
        work_dir, targets, protected_paths=protected_paths
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
        _recover_or_reject_orphan_transactions(work_dir, transaction_root)
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
        _recover_or_reject_orphan_transactions(work_dir, transaction_root)
        return
    descriptor = {**descriptor, "state": state}
    if state == "staging":
        _cleanup_transaction(descriptor)
        _remove_empty_transaction_directories(transaction_root)
        return

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
    else:
        transaction_root = Path(work_dir) / _TRANSACTION_DIR_NAME
        transaction_markers = []
        if transaction_root.is_dir():
            for transaction_dir in transaction_root.iterdir():
                if transaction_dir.is_dir() and not transaction_dir.is_symlink():
                    marker = _find_pair_marker_by_transaction(
                        work_dir, transaction_dir.name
                    )
                    if marker is not None:
                        transaction_markers.append(marker)
        if len(transaction_markers) == 1:
            return _marker_to_descriptor(work_dir, transaction_markers[0]), "committed"
        if len(transaction_markers) > 1:
            raise ArtifactPromotionError(
                "multiple committed transactions require recovery"
            )

    if not identity_quorums and len(groups) == 1 and descriptors:
        selected = next(iter(groups.values()))
        marker = _find_pair_marker_by_transaction(
            work_dir, selected[0]["transactionId"]
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
                work_dir, group[0]["transactionId"]
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
    work_dir: Path, transaction_root: Path
) -> None:
    if not transaction_root.is_dir():
        return
    recovered = False
    for transaction_dir in list(transaction_root.iterdir()):
        if not transaction_dir.is_dir() or transaction_dir.is_symlink():
            raise ArtifactPromotionError("unexpected artifact transaction storage entry")
        if not any(transaction_dir.iterdir()):
            transaction_dir.rmdir()
            continue
        marker = _find_pair_marker_by_transaction(work_dir, transaction_dir.name)
        if marker is None:
            raise ArtifactPromotionError(
                "orphan artifact recovery material has no usable descriptor or marker"
            )
        _verify_marker_targets(marker)
        _cleanup_transaction(_marker_to_descriptor(work_dir, marker))
        recovered = True
    if transaction_root.exists() and not any(transaction_root.iterdir()):
        transaction_root.rmdir()
    if recovered:
        return


def _root_descriptor_paths(work_dir: Path) -> list[Path]:
    return [
        Path(work_dir) / _JOURNAL_NAME,
        Path(work_dir) / _JOURNAL_BACKUP_NAME,
    ]


def _pair_set_id(targets: list[Path]) -> str:
    identity = "\n".join(str(Path(target).resolve()) for target in targets) + "\n"
    return sha256_bytes(identity.encode("utf-8"))


def _pair_marker_paths(work_dir: Path, targets: list[Path]) -> list[Path]:
    marker_dir = Path(work_dir) / _PAIR_MARKER_DIR_NAME / _pair_set_id(targets)
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
    marker_root = Path(work_dir) / _PAIR_MARKER_DIR_NAME
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
    if expected_targets is not None and [target.resolve() for target in targets] != [
        Path(target).resolve() for target in expected_targets
    ]:
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


def _find_pair_marker_by_transaction(work_dir: Path, transaction_id: str) -> dict | None:
    marker_root = Path(work_dir) / _PAIR_MARKER_DIR_NAME
    if not marker_root.is_dir() or marker_root.is_symlink():
        return None
    matches: list[dict] = []
    for marker_dir in marker_root.iterdir():
        if not marker_dir.is_dir() or marker_dir.is_symlink():
            continue
        paths = [marker_dir / f"marker-{index}.json" for index in range(3)]
        valid: list[dict] = []
        for path in paths:
            if not path.is_file() or path.is_symlink():
                continue
            try:
                marker = _load_json_snapshot(_read_owned_regular_file(path))
                _validate_pair_marker(work_dir, marker)
                assert isinstance(marker, dict)
                if marker["transactionId"] == transaction_id:
                    valid.append(marker)
            except (ArtifactPromotionError, OSError, UnicodeError, json.JSONDecodeError):
                continue
        groups: dict[str, list[dict]] = {}
        for marker in valid:
            groups.setdefault(_marker_identity(marker), []).append(marker)
        matches.extend(group[0] for group in groups.values() if len(group) >= 2)
    if len(matches) > 1:
        raise ArtifactPromotionError("multiple committed pair identities match transaction")
    return matches[0] if matches else None


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
    _ensure_distinct_paths([*targets, *descriptor_paths, *payloads])


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
    for entry in entries:
        for path_value in entry["payloads"]:
            _unlink_durably(Path(path_value))


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
        work_dir, timeout=lock_timeout, protected_paths=[first, second]
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
        expected_hashes = [entry["sha256"] for entry in marker["entries"]]
        if [sha256_bytes(snapshot) for snapshot in snapshots] != expected_hashes:
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
    with _artifact_lock(work_dir, timeout=lock_timeout):
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
