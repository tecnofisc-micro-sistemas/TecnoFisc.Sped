"""Build and transactionally promote the reviewed ECF manifest/tracker pair."""

from __future__ import annotations

import hashlib
import json
import os
import shutil
import tempfile
import uuid
from pathlib import Path
from typing import Callable

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


InterruptHook = Callable[[str], None]
_JOURNAL_NAME = ".artifact-transaction.json"
_TRANSACTION_DIR_NAME = ".artifact-transactions"


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
) -> tuple[Path, Path]:
    """Validate and durably publish a provenance-bound local candidate generation."""
    work_dir = Path(work_dir)
    manifest_out = Path(manifest_out)
    tracker_out = Path(tracker_out)
    pdf = Path(pdf)
    generation_path = work_dir / "candidate" / "generation.json"
    build_state_path = work_dir / "build-state.json"
    quarantine_path = work_dir / "quarantine.json"
    journal_path = work_dir / _JOURNAL_NAME
    try:
        recover_artifact_transaction(work_dir)
        _ensure_distinct_paths(
            [
                pdf,
                manifest_out,
                tracker_out,
                generation_path,
                build_state_path,
                quarantine_path,
                journal_path,
            ]
        )
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
        replace_artifacts_durably(
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
        )
        return manifest_out, tracker_out
    except (
        ArtifactPromotionError,
        ManifestValidationError,
        OSError,
        UnicodeError,
    ) as error:
        invalidate_generation(work_dir, str(error))
        raise


def promote_artifacts(
    work_dir: Path,
    manifest_out: Path,
    tracker_out: Path,
    *,
    schema_path: Path,
) -> tuple[Path, Path]:
    """Promote both candidates only after every pair invariant passes."""
    work_dir = Path(work_dir)
    recover_artifact_transaction(work_dir)
    manifest_out = Path(manifest_out)
    tracker_out = Path(tracker_out)
    schema_path = Path(schema_path)
    manifest_candidate = work_dir / "candidate" / "layout-12-manifest.json"
    tracker_candidate = work_dir / "candidate" / "STAGE_17_ECF_BASELINE.md"
    generation_path = work_dir / "candidate" / "generation.json"
    build_state_path = work_dir / "build-state.json"
    quarantine_path = work_dir / "quarantine.json"
    journal_path = work_dir / _JOURNAL_NAME
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
            journal_path,
        ]
    )
    try:
        generation = json.loads(generation_path.read_text(encoding="utf-8"))
        build_state = json.loads(build_state_path.read_text(encoding="utf-8"))
        quarantine = json.loads(quarantine_path.read_text(encoding="utf-8"))
        records = json.loads(manifest_candidate.read_text(encoding="utf-8"))
        tracker_text = tracker_candidate.read_text(encoding="utf-8")
        schema = json.loads(schema_path.read_text(encoding="utf-8"))
    except (OSError, UnicodeError, json.JSONDecodeError) as error:
        reason = f"cannot load promotion inputs: {error}"
        raise _record_promotion_failure(work_dir, reason) from error

    try:
        evidence_paths = [Path(path) for path in generation.get("evidencePaths", [])]
    except AttributeError as error:
        raise _record_promotion_failure(
            work_dir, "candidate generation metadata is malformed"
        ) from error
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
            journal_path,
            *evidence_paths,
        ]
    )
    try:
        evidence_payloads = [
            json.loads(path.read_text(encoding="utf-8")) for path in evidence_paths
        ]
    except (OSError, UnicodeError, json.JSONDecodeError) as error:
        reason = f"cannot load bound review evidence: {error}"
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

    replace_artifacts_durably(
        work_dir,
        [
            (manifest_out, manifest_candidate.read_bytes()),
            (tracker_out, tracker_candidate.read_bytes()),
        ],
    )
    return manifest_out, tracker_out


def apply_review_evidence(work_dir: Path, evidence_paths: list[Path]) -> tuple[Path, Path]:
    """Mark candidates reviewed only from complete, unambiguous visual attestations."""
    work_dir = Path(work_dir)
    recover_artifact_transaction(work_dir)
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
            work_dir / _JOURNAL_NAME,
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
    replace_artifacts_durably(
        work_dir,
        [
            (manifest_path, reviewed_manifest_data),
            (tracker_path, reviewed_tracker_data),
            (generation_path, _pretty_json_bytes(reviewed_generation)),
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
    invalidate_generation(work_dir, reason)
    return ArtifactPromotionError(reason)


def _validate_generation_provenance(
    records: object,
    tracker_text: str,
    generation: object,
    build_state: object,
    quarantine: object,
    *,
    required_state: str,
) -> None:
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


def replace_artifacts_durably(
    work_dir: Path,
    replacements: list[tuple[Path, bytes]],
    *,
    interrupt: InterruptHook | None = None,
) -> None:
    """Publish an artifact set with a durable roll-forward recovery journal.

    A crash may expose a mixed raw filesystem view until the next tooling read. Every tooling
    reader calls recovery first; recovery then rolls the complete journaled set forward before
    returning any bytes. This is recoverable durability, not impossible cross-file atomicity.
    """
    work_dir = Path(work_dir)
    recover_artifact_transaction(work_dir)
    if not replacements:
        raise ArtifactPathError("a durable transaction requires at least one target")
    targets = [Path(path) for path, _data in replacements]
    journal_path = work_dir / _JOURNAL_NAME
    transaction_root = work_dir / _TRANSACTION_DIR_NAME
    _ensure_distinct_paths([*targets, journal_path])
    for target in targets:
        if _is_within(target, transaction_root):
            raise ArtifactPathError("artifact targets cannot be inside transaction storage")

    transaction_id = uuid.uuid4().hex
    transaction_dir = transaction_root / transaction_id
    transaction_dir.mkdir(parents=True, exist_ok=False)
    _sync_directory(transaction_dir.parent)
    entries: list[dict] = []
    for index, (target, data) in enumerate(replacements):
        payload_path = transaction_dir / f"new-{index}.bin"
        _write_durable_file(payload_path, data)
        entries.append(
            {
                "target": str(Path(target).resolve()),
                "payload": str(payload_path.resolve()),
                "sha256": sha256_bytes(data),
            }
        )
    _interrupt(interrupt, "after_payloads")

    journal = {
        "schemaVersion": 1,
        "transactionId": transaction_id,
        "state": "prepared",
        "entries": entries,
    }
    _write_json_durably(journal_path, journal)
    _interrupt(interrupt, "after_journal")
    for index, entry in enumerate(entries):
        _replace_target_from_payload(entry)
        _interrupt(interrupt, f"after_replace_{index}")
    journal["state"] = "committed"
    _write_json_durably(journal_path, journal)
    _interrupt(interrupt, "after_committed")
    journal_path.unlink()
    _sync_directory(journal_path.parent)
    _interrupt(interrupt, "after_journal_removed")
    _remove_transaction_dir(transaction_dir, transaction_root)


def recover_artifact_transaction(work_dir: Path) -> None:
    """Idempotently roll a journaled artifact set forward before any tooling read."""
    work_dir = Path(work_dir)
    journal_path = work_dir / _JOURNAL_NAME
    transaction_root = work_dir / _TRANSACTION_DIR_NAME
    if not journal_path.exists():
        _remove_orphan_transactions(transaction_root)
        return
    try:
        journal = json.loads(journal_path.read_text(encoding="utf-8"))
    except (OSError, UnicodeError, json.JSONDecodeError) as error:
        raise ArtifactPromotionError(f"cannot recover artifact transaction: {error}") from error
    if (
        not isinstance(journal, dict)
        or journal.get("schemaVersion") != 1
        or not isinstance(journal.get("transactionId"), str)
        or journal.get("state") not in {"prepared", "committed"}
        or not isinstance(journal.get("entries"), list)
        or not journal["entries"]
    ):
        raise ArtifactPromotionError("artifact transaction journal is malformed")
    transaction_dir = transaction_root / journal["transactionId"]
    entries = journal["entries"]
    targets: list[Path] = []
    for index, entry in enumerate(entries):
        if not isinstance(entry, dict):
            raise ArtifactPromotionError("artifact transaction entry is malformed")
        payload = Path(entry.get("payload", ""))
        expected_payload = (transaction_dir / f"new-{index}.bin").resolve()
        if payload.resolve() != expected_payload or not _is_within(payload, transaction_root):
            raise ArtifactPromotionError("artifact transaction payload escaped reserved storage")
        data = payload.read_bytes()
        if sha256_bytes(data) != entry.get("sha256"):
            raise ArtifactPromotionError("artifact transaction payload digest mismatch")
        targets.append(Path(entry.get("target", "")))
    _ensure_distinct_paths([*targets, journal_path])
    for entry in entries:
        _replace_target_from_payload(entry)
    journal_path.unlink()
    _sync_directory(journal_path.parent)
    _remove_transaction_dir(transaction_dir, transaction_root)
    _remove_orphan_transactions(transaction_root)


def read_artifact_pair(work_dir: Path, first: Path, second: Path) -> tuple[bytes, bytes]:
    recover_artifact_transaction(work_dir)
    _ensure_distinct_paths([Path(first), Path(second)])
    return Path(first).read_bytes(), Path(second).read_bytes()


def invalidate_generation(work_dir: Path, reason: str) -> None:
    """Durably make every prior candidate generation non-promotable."""
    work_dir = Path(work_dir)
    recover_artifact_transaction(work_dir)
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
    replace_artifacts_durably(work_dir, replacements)


def _replace_target_from_payload(entry: dict) -> None:
    target = Path(entry["target"])
    payload = Path(entry["payload"])
    data = payload.read_bytes()
    if sha256_bytes(data) != entry["sha256"]:
        raise ArtifactPromotionError("artifact transaction payload digest mismatch")
    _write_bytes_durably_atomically(target, data)


def _pretty_json_bytes(payload: object) -> bytes:
    return (json.dumps(payload, ensure_ascii=False, indent=2) + "\n").encode("utf-8")


def _write_json_durably(path: Path, payload: object) -> None:
    _write_bytes_durably_atomically(path, _pretty_json_bytes(payload))


def _write_durable_file(path: Path, data: bytes) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("wb") as stream:
        stream.write(data)
        stream.flush()
        os.fsync(stream.fileno())
    _sync_directory(path.parent)


def _write_bytes_durably_atomically(path: Path, data: bytes) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    descriptor, name = tempfile.mkstemp(
        prefix=f".{path.name}.", suffix=".tmp", dir=path.parent
    )
    temporary = Path(name)
    try:
        with os.fdopen(descriptor, "wb") as stream:
            stream.write(data)
            stream.flush()
            os.fsync(stream.fileno())
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


def _remove_orphan_transactions(root: Path) -> None:
    if not root.exists():
        return
    for child in root.iterdir():
        if child.is_dir() and _is_within(child, root):
            shutil.rmtree(child)
        elif child.is_file():
            child.unlink()
    if not any(root.iterdir()):
        root.rmdir()


def _interrupt(hook: InterruptHook | None, boundary: str) -> None:
    if hook is not None:
        hook(boundary)
