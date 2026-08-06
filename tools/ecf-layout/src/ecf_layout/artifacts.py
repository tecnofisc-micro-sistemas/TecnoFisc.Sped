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
BeforePublishHook = Callable[[], None]
_JOURNAL_NAME = ".artifact-transaction.json"
_JOURNAL_BACKUP_NAME = ".artifact-transaction.backup.json"
_TRANSACTION_DIR_NAME = ".artifact-transactions"
_TRANSACTION_DESCRIPTOR_NAME = "descriptor.json"


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
    journal_paths = _root_descriptor_paths(work_dir)
    try:
        _ensure_distinct_paths(
            [
                pdf,
                manifest_out,
                tracker_out,
                generation_path,
                build_state_path,
                quarantine_path,
                *journal_paths,
            ]
        )
        recover_artifact_transaction(work_dir)
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
    except ArtifactPathError:
        raise
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
    before_publish: BeforePublishHook | None = None,
) -> tuple[Path, Path]:
    """Promote the exact candidate byte snapshots that passed every invariant."""
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
    _ensure_distinct_paths(
        [
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
    )

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
    replace_artifacts_durably(
        work_dir,
        [
            (manifest_out, manifest_snapshot),
            (tracker_out, tracker_snapshot),
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


def replace_artifacts_durably(
    work_dir: Path,
    replacements: list[tuple[Path, bytes]],
    *,
    interrupt: InterruptHook | None = None,
) -> None:
    """Publish artifacts with redundant, bounded roll-forward recovery material.

    The filesystem cannot atomically replace unrelated files. Before replacing any target,
    however, three independently flushed descriptors and two copies of every new payload exist.
    One descriptor or payload copy may disappear and tooling recovery can still converge the set.
    This is a bounded single-artifact-loss guarantee, not an arbitrary power-loss guarantee.
    """
    work_dir = Path(work_dir)
    recover_artifact_transaction(work_dir)
    if not replacements:
        raise ArtifactPathError("a durable transaction requires at least one target")
    targets = [Path(path) for path, _data in replacements]
    transaction_root = work_dir / _TRANSACTION_DIR_NAME
    root_descriptors = _root_descriptor_paths(work_dir)
    _ensure_distinct_paths([*targets, *root_descriptors])
    for target in targets:
        if _is_within(target, transaction_root):
            raise ArtifactPathError("artifact targets cannot be inside transaction storage")

    transaction_id = uuid.uuid4().hex
    transaction_dir = transaction_root / transaction_id
    transaction_dir.mkdir(parents=True, exist_ok=False)
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
    _ensure_distinct_paths([*targets, *descriptor_paths, *all_payloads])

    descriptor = {
        "schemaVersion": 2,
        "transactionId": transaction_id,
        "state": "staging",
        "descriptorPaths": [str(path.resolve()) for path in descriptor_paths],
        "entries": entries,
    }
    for index, path in enumerate(descriptor_paths):
        _write_json_durably(path, descriptor)
        _interrupt(interrupt, f"after_staging_descriptor_{index}")

    for (_target, data), entry in zip(replacements, entries, strict=True):
        for payload_path in entry["payloads"]:
            _write_durable_file(Path(payload_path), data)
    _interrupt(interrupt, "after_payloads")

    descriptor["state"] = "prepared"
    for index, path in enumerate(descriptor_paths):
        _write_json_durably(path, descriptor)
        _interrupt(interrupt, f"after_descriptor_{index}")
    _interrupt(interrupt, "after_journal")
    for index, entry in enumerate(entries):
        _replace_target_from_payload(entry)
        _interrupt(interrupt, f"after_replace_{index}")

    descriptor["state"] = "committed"
    for index, path in enumerate(descriptor_paths):
        _write_json_durably(path, descriptor)
        _interrupt(interrupt, f"after_committed_descriptor_{index}")
    _interrupt(interrupt, "after_committed")

    for index, path in enumerate(descriptor_paths[:2]):
        _unlink_durably(path)
        _interrupt(interrupt, f"after_removed_descriptor_{index}")
    _remove_payload_copies(entries)
    _unlink_durably(descriptor_paths[2])
    _interrupt(interrupt, "after_removed_descriptor_2")
    _interrupt(interrupt, "after_journal_removed")
    _remove_transaction_dir(transaction_dir, transaction_root)


def recover_artifact_transaction(work_dir: Path) -> None:
    """Recover one transaction without accepting a mixed or under-described set."""
    work_dir = Path(work_dir)
    transaction_root = work_dir / _TRANSACTION_DIR_NAME
    descriptor_candidates = [
        *_root_descriptor_paths(work_dir),
        *_internal_descriptor_candidates(transaction_root),
    ]
    existing_descriptors = [path for path in descriptor_candidates if path.is_file()]
    if not existing_descriptors:
        _remove_empty_transaction_directories(transaction_root)
        return

    valid_descriptors: list[dict] = []
    invalid_errors: list[Exception] = []
    for path in existing_descriptors:
        try:
            descriptor = _load_json_snapshot(path.read_bytes())
            _validate_transaction_descriptor(work_dir, descriptor)
            assert isinstance(descriptor, dict)
            valid_descriptors.append(descriptor)
        except (
            ArtifactPromotionError,
            OSError,
            UnicodeError,
            json.JSONDecodeError,
        ) as error:
            invalid_errors.append(error)
    if not valid_descriptors:
        detail = invalid_errors[0] if invalid_errors else "no valid descriptor copy"
        raise ArtifactPromotionError(
            f"cannot recover artifact transaction: {detail}"
        )

    identities = {_transaction_identity(descriptor) for descriptor in valid_descriptors}
    if len(identities) != 1:
        raise ArtifactPromotionError("artifact transaction descriptor copies disagree")
    descriptor = max(
        valid_descriptors,
        key=lambda value: {"staging": 0, "prepared": 1, "committed": 2}[value["state"]],
    )
    if all(copy["state"] == "staging" for copy in valid_descriptors):
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
    _cleanup_transaction(descriptor)
    _remove_empty_transaction_directories(transaction_root)


def _root_descriptor_paths(work_dir: Path) -> list[Path]:
    return [
        Path(work_dir) / _JOURNAL_NAME,
        Path(work_dir) / _JOURNAL_BACKUP_NAME,
    ]


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


def read_artifact_pair(work_dir: Path, first: Path, second: Path) -> tuple[bytes, bytes]:
    recover_artifact_transaction(work_dir)
    first = Path(first)
    second = Path(second)
    _ensure_distinct_paths([first, second])
    if first.exists() != second.exists():
        raise ArtifactPromotionError("artifact pair is mixed after recovery")
    return first.read_bytes(), second.read_bytes()


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
    data = _recoverable_payload(entry)
    if data is None:
        raise ArtifactPromotionError("artifact transaction has no valid payload copy")
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


def _interrupt(hook: InterruptHook | None, boundary: str) -> None:
    if hook is not None:
        hook(boundary)
