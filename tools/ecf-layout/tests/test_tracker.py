import json
import importlib
from pathlib import Path

import pytest
from jsonschema import Draft202012Validator

from ecf_layout.manifest import CANONICAL_BLOCKS, EXPECTED_CODES


REPO_ROOT = Path(__file__).resolve().parents[3]
SCHEMA_PATH = REPO_ROOT / "sped" / "ecf" / "layout-12-manifest.schema.json"
MANIFEST_PATH = REPO_ROOT / "sped" / "ecf" / "layout-12-manifest.json"
TRACKER_PATH = REPO_ROOT / "sped" / "STAGE_17_ECF_BASELINE.md"


def _valid_records(*, reviewed: bool) -> list[dict]:
    return [
        {
            "code": code,
            "block": code[0],
            "title": f"Registro {code}",
            "pageStart": position + 10,
            "pageEnd": position + 10,
            "level": "1",
            "occurrence": "1:1",
            "fields": [
                {
                    "number": 1,
                    "name": "REG",
                    "description": f"Identificacao {code}",
                    "type": "C",
                    "size": "4",
                    "decimals": "-",
                    "required": "Sim",
                    "validValues": f"[{code}]",
                }
            ],
            "reviewed": reviewed,
        }
        for position, code in enumerate(EXPECTED_CODES)
    ]


def _candidate_tracker(records: list[dict], *, status: str) -> str:
    lines = [
        "# Stage 17 - ECF Layout 12 Baseline",
        "",
        "| Substage | Record | Title | Start page | End page | Block | Status |",
        "| --- | --- | --- | ---: | ---: | --- | --- |",
    ]
    lines.extend(
        f"| 17.{position:03d} | {record['code']} | {record['title']} | "
        f"{record['pageStart']} | {record['pageEnd']} | {record['block']} | {status} |"
        for position, record in enumerate(records, start=2)
    )
    return "\n".join(lines) + "\n"


def _test_schema() -> dict:
    return {
        "$schema": "https://json-schema.org/draft/2020-12/schema",
        "type": "array",
        "minItems": 180,
        "maxItems": 180,
        "items": {"type": "object"},
    }


def _load_artifacts() -> tuple[dict, list[dict], list[dict[str, str]]]:
    schema = json.loads(SCHEMA_PATH.read_text(encoding="utf-8"))
    records = json.loads(MANIFEST_PATH.read_text(encoding="utf-8"))
    rows = []
    for line in TRACKER_PATH.read_text(encoding="utf-8").splitlines():
        if not line.startswith("| 17."):
            continue
        cells = [cell.strip() for cell in line.strip().strip("|").split("|")]
        assert len(cells) == 7, f"tracker row must have exactly seven cells: {line}"
        rows.append(
            dict(
                zip(
                    (
                        "substage",
                        "code",
                        "title",
                        "pageStart",
                        "pageEnd",
                        "block",
                        "status",
                    ),
                    cells,
                    strict=True,
                )
            )
        )
    return schema, records, rows


def test_versioned_manifest_matches_schema_and_has_180_unique_codes() -> None:
    schema, records, _rows = _load_artifacts()

    Draft202012Validator.check_schema(schema)
    Draft202012Validator(schema).validate(records)
    codes = [record["code"] for record in records]

    assert len(records) == 180
    assert len(set(codes)) == 180
    assert tuple(codes) == EXPECTED_CODES
    assert tuple(dict.fromkeys(record["block"] for record in records)) == CANONICAL_BLOCKS
    assert all(record["reviewed"] is True for record in records)


def test_tracker_has_one_row_per_manifest_record_in_same_order() -> None:
    _schema, records, rows = _load_artifacts()

    assert [row["code"] for row in rows] == [record["code"] for record in records]
    assert len(rows) == len(records) == 180
    assert [row["title"] for row in rows] == [record["title"] for record in records]
    assert [int(row["pageStart"]) for row in rows] == [record["pageStart"] for record in records]
    assert [int(row["pageEnd"]) for row in rows] == [record["pageEnd"] for record in records]
    assert [row["block"] for row in rows] == [record["block"] for record in records]
    assert {row["status"] for row in rows} == {"[ ]"}


def test_tracker_substages_are_contiguous_from_17_002_to_17_181() -> None:
    _schema, _records, rows = _load_artifacts()

    assert [row["substage"] for row in rows] == [
        f"17.{number:03d}" for number in range(2, 182)
    ]


def test_build_artifacts_writes_unreviewed_candidate_pair_only_to_requested_paths(
    tmp_path: Path,
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    manifest_out = tmp_path / "candidate" / "layout-12-manifest.json"
    tracker_out = tmp_path / "candidate" / "STAGE_17_ECF_BASELINE.md"

    artifacts.build_artifacts(
        _valid_records(reviewed=False), tmp_path, manifest_out, tracker_out
    )

    candidate = json.loads(manifest_out.read_text(encoding="utf-8"))
    tracker = tracker_out.read_text(encoding="utf-8")
    assert [record["code"] for record in candidate] == list(EXPECTED_CODES)
    assert all(record["reviewed"] is False for record in candidate)
    assert tracker.count("| 17.") == 180
    assert "| 17.002 | 0000 |" in tracker
    assert "| 17.181 | 9999 |" in tracker
    assert tracker.count("| [ ] |") == 180
    assert json.loads((tmp_path / "quarantine.json").read_text(encoding="utf-8")) == {
        "items": []
    }


@pytest.mark.parametrize(
    "gate", ["unreviewed", "quarantine", "tracker-order", "tracker-extra", "schema"]
)
def test_promotion_fails_closed_and_preserves_both_destinations(
    tmp_path: Path, gate: str
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    records = _valid_records(reviewed=gate != "unreviewed")
    candidate_dir = tmp_path / "candidate"
    candidate_dir.mkdir()
    (candidate_dir / "layout-12-manifest.json").write_text(
        json.dumps(records), encoding="utf-8"
    )
    tracker = _candidate_tracker(records, status="[ ]")
    if gate == "tracker-order":
        tracker = tracker.replace("| 17.002 | 0000 |", "| 17.002 | 0001 |", 1)
    elif gate == "tracker-extra":
        tracker += "Unbounded manual content must not be promoted.\n"
    (candidate_dir / "STAGE_17_ECF_BASELINE.md").write_text(tracker, encoding="utf-8")
    quarantine_items = (
        [{"code": "0000", "reasons": ["ambiguous"], "pages": [10]}]
        if gate == "quarantine"
        else []
    )
    (tmp_path / "quarantine.json").write_text(
        json.dumps({"items": quarantine_items}), encoding="utf-8"
    )
    schema = tmp_path / "schema.json"
    schema_payload = _test_schema()
    if gate == "schema":
        schema_payload = {**schema_payload, "type": "object"}
    schema.write_text(json.dumps(schema_payload), encoding="utf-8")
    manifest_out = tmp_path / "promoted" / "manifest.json"
    tracker_out = tmp_path / "promoted" / "tracker.md"
    manifest_out.parent.mkdir()
    manifest_out.write_text("previous manifest\n", encoding="utf-8")
    tracker_out.write_text("previous tracker\n", encoding="utf-8")

    with pytest.raises(artifacts.ArtifactPromotionError):
        artifacts.promote_artifacts(
            tmp_path, manifest_out, tracker_out, schema_path=schema
        )

    assert manifest_out.read_text(encoding="utf-8") == "previous manifest\n"
    assert tracker_out.read_text(encoding="utf-8") == "previous tracker\n"
    report = json.loads((tmp_path / "quarantine.json").read_text(encoding="utf-8"))
    assert report["items"]


def test_promotion_replaces_both_destinations_after_all_pair_invariants_pass(
    tmp_path: Path,
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    records = _valid_records(reviewed=True)
    candidate_dir = tmp_path / "candidate"
    candidate_dir.mkdir()
    candidate_manifest = (json.dumps(records, ensure_ascii=False, indent=2) + "\n").encode()
    candidate_tracker = _candidate_tracker(records, status="[ ]").encode()
    (candidate_dir / "layout-12-manifest.json").write_bytes(candidate_manifest)
    (candidate_dir / "STAGE_17_ECF_BASELINE.md").write_bytes(candidate_tracker)
    (tmp_path / "quarantine.json").write_text('{"items": []}\n', encoding="utf-8")
    schema = tmp_path / "schema.json"
    schema.write_text(json.dumps(_test_schema()), encoding="utf-8")
    manifest_out = tmp_path / "promoted" / "manifest.json"
    tracker_out = tmp_path / "promoted" / "tracker.md"

    artifacts.promote_artifacts(tmp_path, manifest_out, tracker_out, schema_path=schema)

    assert manifest_out.read_bytes() == candidate_manifest
    assert tracker_out.read_bytes() == candidate_tracker


def _review_evidence(records: list[dict]) -> dict:
    pages = {
        page
        for record in records
        for page in range(record["pageStart"], record["pageEnd"] + 1)
    }
    return {
        "range": {"startIndex": 1, "endIndex": 180},
        "uniquePagesOpened": len(pages),
        "allPagesOpened": True,
        "records": [
            {
                "index": index,
                "code": record["code"],
                "pageStart": record["pageStart"],
                "pageEnd": record["pageEnd"],
                "reviewed": True,
                "note": "table visually checked",
            }
            for index, record in enumerate(records, start=1)
        ],
        "ambiguities": [],
    }


def test_review_evidence_marks_candidate_pair_only_after_exact_visual_coverage(
    tmp_path: Path,
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    records = _valid_records(reviewed=False)
    manifest = tmp_path / "candidate" / "layout-12-manifest.json"
    tracker = tmp_path / "candidate" / "STAGE_17_ECF_BASELINE.md"
    artifacts.build_artifacts(records, tmp_path, manifest, tracker)
    evidence = tmp_path / "review.json"
    evidence.write_text(json.dumps(_review_evidence(records)), encoding="utf-8")

    artifacts.apply_review_evidence(tmp_path, [evidence])

    reviewed = json.loads(manifest.read_text(encoding="utf-8"))
    assert [record["code"] for record in reviewed] == list(EXPECTED_CODES)
    assert all(record["reviewed"] is True for record in reviewed)
    assert tracker.read_text(encoding="utf-8").count("| [ ] |") == 180


@pytest.mark.parametrize("defect", ["unopened", "ambiguous", "missing", "page-range"])
def test_invalid_review_evidence_preserves_unreviewed_candidate_pair(
    tmp_path: Path, defect: str
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    records = _valid_records(reviewed=False)
    manifest = tmp_path / "candidate" / "layout-12-manifest.json"
    tracker = tmp_path / "candidate" / "STAGE_17_ECF_BASELINE.md"
    artifacts.build_artifacts(records, tmp_path, manifest, tracker)
    prior_manifest = manifest.read_bytes()
    prior_tracker = tracker.read_bytes()
    payload = _review_evidence(records)
    if defect == "unopened":
        payload["allPagesOpened"] = False
    elif defect == "ambiguous":
        payload["ambiguities"] = ["0000 unreadable"]
        payload["records"][0]["reviewed"] = False
    elif defect == "missing":
        payload["records"].pop()
    else:
        payload["records"][0]["pageEnd"] += 1
    evidence = tmp_path / "review.json"
    evidence.write_text(json.dumps(payload), encoding="utf-8")

    with pytest.raises(artifacts.ArtifactPromotionError):
        artifacts.apply_review_evidence(tmp_path, [evidence])

    assert manifest.read_bytes() == prior_manifest
    assert tracker.read_bytes() == prior_tracker
