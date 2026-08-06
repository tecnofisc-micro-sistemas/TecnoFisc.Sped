"""Build and transactionally promote the reviewed ECF manifest/tracker pair."""

from __future__ import annotations

import json
import os
import tempfile
from pathlib import Path

from jsonschema import Draft202012Validator
from jsonschema.exceptions import SchemaError, ValidationError

from ecf_layout.manifest import (
    _quarantine_items,
    _write_bytes_atomically,
    validate_and_promote,
    write_quarantine,
)


class ArtifactPromotionError(ValueError):
    """Raised before promoted artifacts can be safely replaced."""


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
) -> tuple[Path, Path]:
    """Validate and write an unpromoted candidate manifest/tracker pair."""
    candidate = validate_and_promote(records, work_dir)
    manifest_out = Path(manifest_out)
    tracker_out = Path(tracker_out)
    if candidate.resolve() != manifest_out.resolve():
        _write_bytes_atomically(manifest_out, candidate.read_bytes())
    _write_bytes_atomically(tracker_out, render_tracker(records).encode("utf-8"))
    return manifest_out, tracker_out


def promote_artifacts(
    work_dir: Path,
    manifest_out: Path,
    tracker_out: Path,
    *,
    schema_path: Path,
) -> tuple[Path, Path]:
    """Promote both candidates only after every pair invariant passes."""
    work_dir = Path(work_dir)
    manifest_candidate = work_dir / "candidate" / "layout-12-manifest.json"
    tracker_candidate = work_dir / "candidate" / "STAGE_17_ECF_BASELINE.md"
    try:
        quarantine = json.loads((work_dir / "quarantine.json").read_text(encoding="utf-8"))
        if not isinstance(quarantine, dict) or quarantine.get("items") != []:
            raise ArtifactPromotionError("quarantine must exist and contain an empty items array")
        records = json.loads(manifest_candidate.read_text(encoding="utf-8"))
        tracker_text = tracker_candidate.read_text(encoding="utf-8")
        schema = json.loads(Path(schema_path).read_text(encoding="utf-8"))
    except ArtifactPromotionError:
        raise
    except (OSError, UnicodeError, json.JSONDecodeError) as error:
        reason = f"cannot load promotion inputs: {error}"
        raise _record_promotion_failure(work_dir, reason) from error

    if not isinstance(records, list):
        raise _record_promotion_failure(work_dir, "candidate manifest root must be an array")
    items = _quarantine_items(records, require_reviewed=True)
    if items:
        write_quarantine(work_dir, items)
        raise ArtifactPromotionError(f"manifest validation quarantined {len(items)} item(s)")
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

    _replace_pair_transactionally(
        Path(manifest_out),
        manifest_candidate.read_bytes(),
        Path(tracker_out),
        tracker_candidate.read_bytes(),
    )
    return Path(manifest_out), Path(tracker_out)


def apply_review_evidence(work_dir: Path, evidence_paths: list[Path]) -> tuple[Path, Path]:
    """Mark candidates reviewed only from complete, unambiguous visual attestations."""
    work_dir = Path(work_dir)
    manifest_path = work_dir / "candidate" / "layout-12-manifest.json"
    tracker_path = work_dir / "candidate" / "STAGE_17_ECF_BASELINE.md"
    try:
        records = json.loads(manifest_path.read_text(encoding="utf-8"))
        tracker_text = tracker_path.read_text(encoding="utf-8")
        evidence_payloads = [
            json.loads(Path(path).read_text(encoding="utf-8")) for path in evidence_paths
        ]
    except (OSError, UnicodeError, json.JSONDecodeError) as error:
        raise ArtifactPromotionError(f"cannot load review evidence: {error}") from error
    if not isinstance(records, list) or _quarantine_items(records, require_reviewed=False):
        raise ArtifactPromotionError("candidate manifest is not structurally valid")
    if tracker_text != render_tracker(records):
        raise ArtifactPromotionError("candidate tracker does not match candidate manifest")

    seen: set[int] = set()
    for payload in evidence_payloads:
        if not isinstance(payload, dict):
            raise ArtifactPromotionError("review evidence root must be an object")
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
        if len(indices) != len(reviewed_records) or any(not isinstance(index, int) for index in indices):
            raise ArtifactPromotionError("every review record must have an integer index")
        if bounds != {"startIndex": min(indices), "endIndex": max(indices)}:
            raise ArtifactPromotionError("review evidence range does not match its records")
        pages = set()
        for item, index in zip(reviewed_records, indices, strict=True):
            if index in seen or not 1 <= index <= len(records):
                raise ArtifactPromotionError("review evidence indices must be unique and in range")
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
            raise ArtifactPromotionError("review evidence page count does not match record ranges")

    if seen != set(range(1, len(records) + 1)):
        raise ArtifactPromotionError("review evidence must cover every candidate exactly once")
    reviewed = [{**record, "reviewed": True} for record in records]
    if _quarantine_items(reviewed, require_reviewed=True):
        raise ArtifactPromotionError("reviewed candidate failed final manifest validation")
    _replace_pair_transactionally(
        manifest_path,
        (json.dumps(reviewed, ensure_ascii=False, indent=2) + "\n").encode("utf-8"),
        tracker_path,
        render_tracker(reviewed).encode("utf-8"),
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
    write_quarantine(work_dir, [{"code": None, "reasons": [reason], "pages": []}])
    return ArtifactPromotionError(reason)


def _cell(value: object) -> str:
    return " ".join(str(value).split()).replace("|", "&#124;")


def _replace_pair_transactionally(
    first_path: Path,
    first_data: bytes,
    second_path: Path,
    second_data: bytes,
) -> None:
    paths = (first_path, second_path)
    data = (first_data, second_data)
    prior = tuple(path.read_bytes() if path.exists() else None for path in paths)
    staged: list[Path] = []
    try:
        for path, payload in zip(paths, data, strict=True):
            path.parent.mkdir(parents=True, exist_ok=True)
            descriptor, name = tempfile.mkstemp(
                prefix=f".{path.name}.", suffix=".tmp", dir=path.parent
            )
            temporary = Path(name)
            staged.append(temporary)
            with os.fdopen(descriptor, "wb") as stream:
                stream.write(payload)
        os.replace(staged[0], first_path)
        os.replace(staged[1], second_path)
    except Exception:
        for path, previous in zip(paths, prior, strict=True):
            if previous is None:
                path.unlink(missing_ok=True)
            else:
                _write_bytes_atomically(path, previous)
        raise
    finally:
        for temporary in staged:
            temporary.unlink(missing_ok=True)
