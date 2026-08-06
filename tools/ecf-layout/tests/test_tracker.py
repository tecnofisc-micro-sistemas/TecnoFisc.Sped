import json
import importlib
import multiprocessing
import os
import shutil
import threading
import time
from contextlib import contextmanager
from pathlib import Path

import pytest
from jsonschema import Draft202012Validator

from ecf_layout.manifest import CANONICAL_BLOCKS, EXPECTED_CODES


REPO_ROOT = Path(__file__).resolve().parents[3]
SCHEMA_PATH = REPO_ROOT / "sped" / "ecf" / "layout-12-manifest.schema.json"
MANIFEST_PATH = REPO_ROOT / "sped" / "ecf" / "layout-12-manifest.json"
TRACKER_PATH = REPO_ROOT / "sped" / "STAGE_17_ECF_BASELINE.md"


def _hold_artifact_lock_in_process(
    work_dir: str,
    ready_path: str,
    release_path: str,
    *target_paths: str,
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    with artifacts._artifact_lock(
        Path(work_dir),
        targets=[Path(path) for path in target_paths],
        timeout=5.0,
    ):
        Path(ready_path).write_text("ready", encoding="utf-8")
        deadline = time.monotonic() + 10
        while not Path(release_path).exists():
            if time.monotonic() >= deadline:
                raise TimeoutError("test process did not receive release signal")
            time.sleep(0.01)


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


def test_schema_encodes_canonical_code_block_order_with_prefix_items() -> None:
    schema, records, _rows = _load_artifacts()

    assert schema["items"] is False
    assert len(schema["prefixItems"]) == len(EXPECTED_CODES) == 180
    encoded_pairs = [
        (
            item["allOf"][1]["properties"]["code"]["const"],
            item["allOf"][1]["properties"]["block"]["const"],
        )
        for item in schema["prefixItems"]
    ]
    assert encoded_pairs == [(code, code[0]) for code in EXPECTED_CODES]

    wrong_order = [dict(record) for record in records]
    wrong_order[0], wrong_order[1] = wrong_order[1], wrong_order[0]
    assert list(Draft202012Validator(schema).iter_errors(wrong_order))


def test_schema_documents_relational_invariants_enforced_by_tooling() -> None:
    schema, _records, _rows = _load_artifacts()

    comment = schema["$comment"]
    assert "pageEnd >= pageStart" in comment
    assert "contiguous field numbers" in comment
    assert "semantic validation" in comment


def test_schema_limits_required_markers_to_the_normative_vocabulary() -> None:
    schema, _records, _rows = _load_artifacts()

    assert schema["$defs"]["field"]["properties"]["required"]["enum"] == [
        "Sim",
        "sim",
        "S",
        "Não",
        "N",
        "-",
        "OC",
    ]


def test_tracker_has_one_row_per_manifest_record_in_same_order() -> None:
    _schema, records, rows = _load_artifacts()

    assert [row["code"] for row in rows] == [record["code"] for record in records]
    assert len(rows) == len(records) == 180
    assert [row["title"] for row in rows] == [record["title"] for record in records]
    assert [int(row["pageStart"]) for row in rows] == [record["pageStart"] for record in records]
    assert [int(row["pageEnd"]) for row in rows] == [record["pageEnd"] for record in records]
    assert [row["block"] for row in rows] == [record["block"] for record in records]
    assert {row["status"] for row in rows} <= {"[ ]", "[x]"}


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
    pdf = _manual(tmp_path)

    artifacts.build_artifacts(
        _valid_records(reviewed=False), tmp_path, manifest_out, tracker_out, pdf=pdf
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
    if gate == "unreviewed":
        artifacts = importlib.import_module("ecf_layout.artifacts")
        records = _valid_records(reviewed=False)
        pdf = _manual(tmp_path)
        manifest = tmp_path / "candidate" / "layout-12-manifest.json"
        tracker_path = tmp_path / "candidate" / "STAGE_17_ECF_BASELINE.md"
        artifacts.build_artifacts(records, tmp_path, manifest, tracker_path, pdf=pdf)
    else:
        artifacts, _records, _pdf, _work_dir, manifest, tracker_path, _evidence = (
            _prepare_reviewed_generation(tmp_path)
        )
    if gate == "quarantine":
        (tmp_path / "work" / "quarantine.json").write_text(
            json.dumps(
                {"items": [{"code": "0000", "reasons": ["ambiguous"], "pages": [10]}]}
            ),
            encoding="utf-8",
        )
    elif gate == "tracker-order":
        tracker_path.write_text(
            tracker_path.read_text(encoding="utf-8").replace(
                "| 17.002 | 0000 |", "| 17.002 | 0001 |", 1
            ),
            encoding="utf-8",
        )
    elif gate == "tracker-extra":
        tracker_path.write_text(
            tracker_path.read_text(encoding="utf-8")
            + "Unbounded manual content must not be promoted.\n",
            encoding="utf-8",
        )
    work_dir = tmp_path if gate == "unreviewed" else tmp_path / "work"
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
            work_dir, manifest_out, tracker_out, schema_path=schema
        )

    assert manifest_out.read_text(encoding="utf-8") == "previous manifest\n"
    assert tracker_out.read_text(encoding="utf-8") == "previous tracker\n"
    report = json.loads((work_dir / "quarantine.json").read_text(encoding="utf-8"))
    assert report["items"]


def test_promotion_replaces_both_destinations_after_all_pair_invariants_pass(
    tmp_path: Path,
) -> None:
    artifacts, _records, _pdf, work_dir, manifest, tracker, _evidence = (
        _prepare_reviewed_generation(tmp_path)
    )
    candidate_manifest = manifest.read_bytes()
    candidate_tracker = tracker.read_bytes()
    schema = tmp_path / "schema.json"
    schema.write_text(json.dumps(_test_schema()), encoding="utf-8")
    manifest_out = tmp_path / "promoted" / "manifest.json"
    tracker_out = tmp_path / "promoted" / "tracker.md"

    artifacts.promote_artifacts(work_dir, manifest_out, tracker_out, schema_path=schema)

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
    pdf = _manual(tmp_path)
    artifacts.build_artifacts(records, tmp_path, manifest, tracker, pdf=pdf)
    evidence = tmp_path / "review.json"
    evidence.write_text(
        json.dumps(_provenance_evidence(records, _provenance(tmp_path))), encoding="utf-8"
    )

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
    pdf = _manual(tmp_path)
    artifacts.build_artifacts(records, tmp_path, manifest, tracker, pdf=pdf)
    prior_manifest = manifest.read_bytes()
    prior_tracker = tracker.read_bytes()
    payload = _provenance_evidence(records, _provenance(tmp_path))
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


def _manual(tmp_path: Path, content: bytes = b"normative ECF manual") -> Path:
    pdf = tmp_path / "manual.pdf"
    pdf.write_bytes(content)
    return pdf


def _provenance(work_dir: Path) -> dict:
    return json.loads(
        (work_dir / "candidate" / "generation.json").read_text(encoding="utf-8")
    )


def _provenance_evidence(records: list[dict], generation: dict) -> dict:
    evidence = _review_evidence(records)
    evidence.update(
        {
            "generationId": generation["generationId"],
            "pdfSha256": generation["pdfSha256"],
            "candidateSha256": generation["reviewCandidateSha256"],
        }
    )
    return evidence


def test_build_persists_exact_pdf_and_full_candidate_provenance(tmp_path: Path) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    records = _valid_records(reviewed=False)
    pdf = _manual(tmp_path)
    work_dir = tmp_path / "work"
    manifest = work_dir / "candidate" / "layout-12-manifest.json"
    tracker = work_dir / "candidate" / "STAGE_17_ECF_BASELINE.md"

    artifacts.build_artifacts(records, work_dir, manifest, tracker, pdf=pdf)

    generation = _provenance(work_dir)
    candidate = json.loads(manifest.read_text(encoding="utf-8"))
    assert generation == {
        "schemaVersion": 1,
        "generationId": artifacts.generation_id(
            generation["pdfSha256"], generation["reviewCandidateSha256"]
        ),
        "state": "candidate",
        "pdfPath": str(pdf.resolve()),
        "pdfSha256": artifacts.sha256_file(pdf),
        "reviewCandidateSha256": artifacts.canonical_candidate_sha256(candidate),
        "candidateSha256": artifacts.canonical_candidate_sha256(candidate),
        "trackerSha256": artifacts.sha256_bytes(tracker.read_bytes()),
        "evidencePaths": [],
    }
    build_state = json.loads((work_dir / "build-state.json").read_text(encoding="utf-8"))
    assert build_state == {"state": "valid", "generationId": generation["generationId"]}


@pytest.mark.parametrize("mutation", ["candidate-content", "pdf-bytes"])
def test_review_evidence_rejects_content_or_pdf_mutation_without_code_page_change(
    tmp_path: Path, mutation: str
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    records = _valid_records(reviewed=False)
    pdf = _manual(tmp_path)
    work_dir = tmp_path / "work"
    manifest = work_dir / "candidate" / "layout-12-manifest.json"
    tracker = work_dir / "candidate" / "STAGE_17_ECF_BASELINE.md"
    artifacts.build_artifacts(records, work_dir, manifest, tracker, pdf=pdf)
    evidence = tmp_path / "review.json"
    evidence.write_text(
        json.dumps(_provenance_evidence(records, _provenance(work_dir))), encoding="utf-8"
    )
    if mutation == "candidate-content":
        mutated = json.loads(manifest.read_text(encoding="utf-8"))
        mutated[0]["fields"][0]["description"] = "content changed after visual review"
        manifest.write_text(json.dumps(mutated), encoding="utf-8")
    else:
        pdf.write_bytes(b"different PDF bytes")

    with pytest.raises(artifacts.ArtifactPromotionError):
        artifacts.apply_review_evidence(work_dir, [evidence])

    current = json.loads(manifest.read_text(encoding="utf-8"))
    assert all(record["reviewed"] is False for record in current)


TRANSACTION_BOUNDARIES = [
    "after_transaction_mkdir",
    "after_staging_descriptor_0",
    "after_staging_descriptor_1",
    "after_staging_descriptor_2",
    "after_payloads",
    "after_descriptor_0",
    "after_descriptor_1",
    "after_descriptor_2",
    "after_journal",
    "after_replace_0",
    "after_replace_1",
    "after_committed_descriptor_0",
    "after_committed_descriptor_1",
    "after_committed_descriptor_2",
    "after_committed",
    "after_target_marker_0_0",
    "after_target_marker_0_1",
    "after_target_marker_0_2",
    "after_target_marker_1_0",
    "after_target_marker_1_1",
    "after_target_marker_1_2",
    "after_marker_0",
    "after_marker_1",
    "after_marker_2",
    "after_removed_descriptor_0",
    "after_removed_descriptor_1",
    "after_removed_descriptor_2",
    "after_journal_removed",
]


@pytest.mark.parametrize("outputs_exist", [False, True], ids=["new", "existing"])
def test_empty_pre_intent_transaction_is_removed_and_next_publish_converges(
    tmp_path: Path, outputs_exist: bool
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    first = tmp_path / "published" / "manifest.json"
    second = tmp_path / "published" / "tracker.md"
    if outputs_exist:
        artifacts.replace_artifacts_durably(
            tmp_path,
            [(first, b"old manifest"), (second, b"old tracker")],
        )

    class AbruptStop(BaseException):
        pass

    def interrupt(boundary: str) -> None:
        if boundary == "after_transaction_mkdir":
            raise AbruptStop(boundary)

    with pytest.raises(AbruptStop):
        artifacts.replace_artifacts_durably(
            tmp_path,
            [(first, b"new manifest"), (second, b"new tracker")],
            interrupt=interrupt,
        )

    transaction_root = tmp_path / ".artifact-transactions"
    transaction_dirs = list(transaction_root.iterdir())
    assert len(transaction_dirs) == 1
    assert not any(transaction_dirs[0].iterdir())

    artifacts.recover_artifact_transaction(tmp_path)
    assert not transaction_root.exists()
    expected = (
        (b"old manifest", b"old tracker") if outputs_exist else (None, None)
    )
    assert _pair_state(first, second) == expected

    artifacts.replace_artifacts_durably(
        tmp_path,
        [(first, b"later manifest"), (second, b"later tracker")],
    )
    assert artifacts.read_artifact_pair(tmp_path, first, second) == (
        b"later manifest",
        b"later tracker",
    )


@pytest.mark.parametrize("outputs_exist", [False, True], ids=["new", "existing"])
def test_marker_preflight_failure_does_not_create_transaction_namespace(
    tmp_path: Path, outputs_exist: bool, monkeypatch: pytest.MonkeyPatch
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    first = tmp_path / "published" / "manifest.json"
    second = tmp_path / "published" / "tracker.md"
    if outputs_exist:
        artifacts.replace_artifacts_durably(
            tmp_path,
            [(first, b"old manifest"), (second, b"old tracker")],
        )
    real_preflight = artifacts._preflight_pair_markers
    failed = False

    def fail_once(*args: object, **kwargs: object) -> None:
        nonlocal failed
        if not failed:
            failed = True
            raise artifacts.ArtifactPromotionError("injected marker preflight failure")
        real_preflight(*args, **kwargs)

    monkeypatch.setattr(artifacts, "_preflight_pair_markers", fail_once)
    with pytest.raises(
        artifacts.ArtifactPromotionError, match="injected marker preflight"
    ):
        artifacts.replace_artifacts_durably(
            tmp_path,
            [(first, b"new manifest"), (second, b"new tracker")],
        )

    assert not (tmp_path / ".artifact-transactions").exists()
    artifacts.recover_artifact_transaction(tmp_path)
    artifacts.replace_artifacts_durably(
        tmp_path,
        [(first, b"later manifest"), (second, b"later tracker")],
    )
    assert artifacts.read_artifact_pair(tmp_path, first, second) == (
        b"later manifest",
        b"later tracker",
    )


def test_unknown_nonempty_pre_intent_transaction_fails_closed(tmp_path: Path) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    first = tmp_path / "published" / "manifest.json"
    second = tmp_path / "published" / "tracker.md"

    class AbruptStop(BaseException):
        pass

    def interrupt(boundary: str) -> None:
        if boundary == "after_transaction_mkdir":
            raise AbruptStop(boundary)

    with pytest.raises(AbruptStop):
        artifacts.replace_artifacts_durably(
            tmp_path,
            [(first, b"new manifest"), (second, b"new tracker")],
            interrupt=interrupt,
        )
    transaction_dir = next((tmp_path / ".artifact-transactions").iterdir())
    (transaction_dir / "unknown.bin").write_bytes(b"unproven")

    with pytest.raises(artifacts.ArtifactPromotionError, match="intent|unproven"):
        artifacts.recover_artifact_transaction(tmp_path)
    assert (transaction_dir / "unknown.bin").read_bytes() == b"unproven"


def _pair_state(first: Path, second: Path) -> tuple[bytes | None, bytes | None]:
    return tuple(path.read_bytes() if path.exists() else None for path in (first, second))


@pytest.mark.parametrize("outputs_exist", [False, True], ids=["new", "existing"])
@pytest.mark.parametrize("boundary", TRANSACTION_BOUNDARIES)
def test_durable_transaction_recovers_old_or_new_coherent_pair_after_abrupt_stop(
    tmp_path: Path, boundary: str, outputs_exist: bool
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    first = tmp_path / "published" / "manifest.json"
    second = tmp_path / "published" / "tracker.md"
    first.parent.mkdir()
    if outputs_exist:
        first.write_bytes(b"old manifest")
        second.write_bytes(b"old tracker")

    class AbruptStop(BaseException):
        pass

    def interrupt(current: str) -> None:
        if current == boundary:
            raise AbruptStop(current)

    with pytest.raises(AbruptStop):
        artifacts.replace_artifacts_durably(
            tmp_path,
            [(first, b"new manifest"), (second, b"new tracker")],
            interrupt=interrupt,
        )

    artifacts.recover_artifact_transaction(tmp_path)
    assert _pair_state(first, second) in {
        (None, None),
        (b"old manifest", b"old tracker"),
        (b"new manifest", b"new tracker"),
    }
    assert not (tmp_path / ".artifact-transaction.json").exists()


@pytest.mark.parametrize(
    "descriptor_kind", ["primary", "backup", "transaction"],
)
def test_recovery_tolerates_loss_of_each_single_descriptor_after_first_replace(
    tmp_path: Path, descriptor_kind: str
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    first = tmp_path / "published" / "manifest.json"
    second = tmp_path / "published" / "tracker.md"
    first.parent.mkdir()
    first.write_bytes(b"old manifest")
    second.write_bytes(b"old tracker")

    class AbruptStop(BaseException):
        pass

    def interrupt(boundary: str) -> None:
        if boundary == "after_replace_0":
            raise AbruptStop(boundary)

    with pytest.raises(AbruptStop):
        artifacts.replace_artifacts_durably(
            tmp_path,
            [(first, b"new manifest"), (second, b"new tracker")],
            interrupt=interrupt,
        )

    transaction_dir = next((tmp_path / ".artifact-transactions").iterdir())
    descriptors = {
        "primary": tmp_path / ".artifact-transaction.json",
        "backup": tmp_path / ".artifact-transaction.backup.json",
        "transaction": transaction_dir / "descriptor.json",
    }
    descriptors[descriptor_kind].unlink()

    assert artifacts.read_artifact_pair(tmp_path, first, second) == (
        b"new manifest",
        b"new tracker",
    )


@pytest.mark.parametrize("entry_index", [0, 1])
@pytest.mark.parametrize("copy_index", [0, 1])
def test_recovery_tolerates_loss_of_each_single_payload_copy_after_first_replace(
    tmp_path: Path, entry_index: int, copy_index: int
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    first = tmp_path / "published" / "manifest.json"
    second = tmp_path / "published" / "tracker.md"
    first.parent.mkdir()
    first.write_bytes(b"old manifest")
    second.write_bytes(b"old tracker")

    class AbruptStop(BaseException):
        pass

    def interrupt(boundary: str) -> None:
        if boundary == "after_replace_0":
            raise AbruptStop(boundary)

    with pytest.raises(AbruptStop):
        artifacts.replace_artifacts_durably(
            tmp_path,
            [(first, b"new manifest"), (second, b"new tracker")],
            interrupt=interrupt,
        )

    descriptor = json.loads(
        (tmp_path / ".artifact-transaction.json").read_text(encoding="utf-8")
    )
    Path(descriptor["entries"][entry_index]["payloads"][copy_index]).unlink()

    assert artifacts.read_artifact_pair(tmp_path, first, second) == (
        b"new manifest",
        b"new tracker",
    )


def test_recovery_preserves_unidentified_payloads_and_refuses_a_mixed_pair(
    tmp_path: Path,
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    first = tmp_path / "published" / "manifest.json"
    second = tmp_path / "published" / "tracker.md"
    first.parent.mkdir()
    first.write_bytes(b"old manifest")
    second.write_bytes(b"old tracker")

    class AbruptStop(BaseException):
        pass

    def interrupt(boundary: str) -> None:
        if boundary == "after_replace_0":
            raise AbruptStop(boundary)

    with pytest.raises(AbruptStop):
        artifacts.replace_artifacts_durably(
            tmp_path,
            [(first, b"new manifest"), (second, b"new tracker")],
            interrupt=interrupt,
        )
    transaction_dir = next((tmp_path / ".artifact-transactions").iterdir())
    descriptors = [
        tmp_path / ".artifact-transaction.json",
        tmp_path / ".artifact-transaction.backup.json",
        transaction_dir / "descriptor.json",
    ]
    for descriptor in descriptors:
        descriptor.unlink()
    retained_payload = transaction_dir / "new-1.0.bin"

    with pytest.raises(artifacts.ArtifactPromotionError, match="orphan"):
        artifacts.read_artifact_pair(tmp_path, first, second)

    assert retained_payload.exists()
    assert _pair_state(first, second) == (b"new manifest", b"old tracker")


def test_build_rejects_identical_or_hardlinked_manifest_tracker_paths(tmp_path: Path) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    records = _valid_records(reviewed=False)
    pdf = _manual(tmp_path)
    work_dir = tmp_path / "work"
    same = tmp_path / "same-output"

    with pytest.raises(artifacts.ArtifactPathError):
        artifacts.build_artifacts(records, work_dir, same, same, pdf=pdf)

    first = tmp_path / "first-output"
    second = tmp_path / "second-output"
    first.write_bytes(b"existing")
    try:
        os.link(first, second)
    except OSError:
        pytest.skip("hardlinks are not supported on this filesystem")
    with pytest.raises(artifacts.ArtifactPathError):
        artifacts.build_artifacts(records, work_dir, first, second, pdf=pdf)


def _prepare_reviewed_generation(tmp_path: Path):
    artifacts = importlib.import_module("ecf_layout.artifacts")
    records = _valid_records(reviewed=False)
    pdf = _manual(tmp_path)
    work_dir = tmp_path / "work"
    manifest = work_dir / "candidate" / "layout-12-manifest.json"
    tracker = work_dir / "candidate" / "STAGE_17_ECF_BASELINE.md"
    artifacts.build_artifacts(records, work_dir, manifest, tracker, pdf=pdf)
    evidence = tmp_path / "review.json"
    evidence.write_text(
        json.dumps(_provenance_evidence(records, _provenance(work_dir))), encoding="utf-8"
    )
    artifacts.apply_review_evidence(work_dir, [evidence])
    return artifacts, records, pdf, work_dir, manifest, tracker, evidence


@pytest.mark.parametrize("failure", ["missing-build-pdf", "changed-reviewed-pdf", "content"])
def test_stale_or_mutated_reviewed_generation_cannot_promote(
    tmp_path: Path, failure: str
) -> None:
    artifacts, _records, pdf, work_dir, manifest, _tracker, _evidence = (
        _prepare_reviewed_generation(tmp_path)
    )
    schema = tmp_path / "published" / "layout-12-manifest.schema.json"
    schema.parent.mkdir()
    schema.write_text(json.dumps(_test_schema()), encoding="utf-8")
    manifest_out = schema.parent / "layout-12-manifest.json"
    tracker_out = schema.parent / "STAGE_17_ECF_BASELINE.md"
    manifest_out.write_text("previous manifest\n", encoding="utf-8")
    tracker_out.write_text("previous tracker\n", encoding="utf-8")
    if failure == "missing-build-pdf":
        exit_code = importlib.import_module("ecf_layout.cli").main(
            [
                "build-artifacts",
                "--pdf",
                str(tmp_path / "missing.pdf"),
                "--work-dir",
                str(work_dir),
                "--manifest-out",
                str(work_dir / "candidate" / "layout-12-manifest.json"),
                "--tracker-out",
                str(work_dir / "candidate" / "STAGE_17_ECF_BASELINE.md"),
            ]
        )
        assert exit_code == 1
    elif failure == "changed-reviewed-pdf":
        pdf.write_bytes(b"swapped PDF after review")
    else:
        mutated = json.loads(manifest.read_text(encoding="utf-8"))
        mutated[0]["title"] = "mutated after visual review"
        manifest.write_text(json.dumps(mutated), encoding="utf-8")

    with pytest.raises(artifacts.ArtifactPromotionError):
        artifacts.promote_artifacts(
            work_dir, manifest_out, tracker_out, schema_path=schema
        )

    assert manifest_out.read_text(encoding="utf-8") == "previous manifest\n"
    assert tracker_out.read_text(encoding="utf-8") == "previous tracker\n"
    if failure == "missing-build-pdf":
        assert json.loads((work_dir / "build-state.json").read_text(encoding="utf-8"))[
            "state"
        ] == "invalid"
        assert json.loads((work_dir / "quarantine.json").read_text(encoding="utf-8"))[
            "items"
        ]


def test_promotion_publishes_only_the_candidate_snapshot_that_was_validated(
    tmp_path: Path,
) -> None:
    artifacts, records, _pdf, work_dir, manifest, tracker, _evidence = (
        _prepare_reviewed_generation(tmp_path)
    )
    validated_manifest = manifest.read_bytes()
    validated_tracker = tracker.read_bytes()
    schema = tmp_path / "published" / "layout-12-manifest.schema.json"
    schema.parent.mkdir()
    schema.write_text(json.dumps(_test_schema()), encoding="utf-8")
    manifest_out = schema.parent / "layout-12-manifest.json"
    tracker_out = schema.parent / "STAGE_17_ECF_BASELINE.md"

    def swap_after_validation() -> None:
        swapped = [{**record, "reviewed": True} for record in records]
        swapped[0] = {**swapped[0], "title": "unvalidated post-check swap"}
        manifest.write_text(json.dumps(swapped), encoding="utf-8")
        tracker.write_text(
            artifacts.render_tracker(swapped), encoding="utf-8"
        )

    artifacts.promote_artifacts(
        work_dir,
        manifest_out,
        tracker_out,
        schema_path=schema,
        before_publish=swap_after_validation,
    )

    assert manifest_out.read_bytes() == validated_manifest
    assert tracker_out.read_bytes() == validated_tracker
    assert manifest_out.read_bytes() != manifest.read_bytes()


@pytest.mark.parametrize(
    "role",
    [
        "manifest-out",
        "tracker-out",
        "schema",
        "candidate-manifest",
        "candidate-tracker",
        "quarantine",
        "evidence",
    ],
)
def test_promote_rejects_every_applicable_path_role_aliasing_normative_pdf(
    tmp_path: Path, role: str
) -> None:
    artifacts, _records, pdf, work_dir, manifest, tracker, evidence = (
        _prepare_reviewed_generation(tmp_path)
    )
    schema = tmp_path / "published" / "layout-12-manifest.schema.json"
    schema.parent.mkdir()
    schema.write_text(json.dumps(_test_schema()), encoding="utf-8")
    manifest_out = schema.parent / "layout-12-manifest.json"
    tracker_out = schema.parent / "STAGE_17_ECF_BASELINE.md"
    role_paths = {
        "manifest-out": manifest_out,
        "tracker-out": tracker_out,
        "schema": schema,
        "candidate-manifest": manifest,
        "candidate-tracker": tracker,
        "quarantine": work_dir / "quarantine.json",
        "evidence": evidence,
    }
    alias = role_paths[role]
    alias.unlink(missing_ok=True)
    try:
        os.link(pdf, alias)
    except OSError:
        pytest.skip("hardlinks are not supported on this filesystem")

    with pytest.raises(artifacts.ArtifactPathError):
        artifacts.promote_artifacts(
            work_dir,
            manifest_out,
            tracker_out,
            schema_path=schema,
        )


@pytest.mark.parametrize(
    "descriptor_name",
    [".artifact-transaction.json", ".artifact-transaction.backup.json"],
)
def test_build_rejects_normative_pdf_at_transaction_descriptor_path(
    tmp_path: Path, descriptor_name: str
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    work_dir = tmp_path / "work"
    work_dir.mkdir()
    pdf = work_dir / descriptor_name
    pdf.write_bytes(b"normative PDF")

    with pytest.raises(artifacts.ArtifactPathError):
        artifacts.build_artifacts(
            _valid_records(reviewed=False),
            work_dir,
            work_dir / "candidate" / "layout-12-manifest.json",
            work_dir / "candidate" / "STAGE_17_ECF_BASELINE.md",
            pdf=pdf,
        )


@pytest.mark.parametrize("alias_kind", ["symlink", "case"])
def test_promote_rejects_symlink_or_case_alias_to_normative_pdf(
    tmp_path: Path, alias_kind: str
) -> None:
    artifacts, _records, pdf, work_dir, _manifest, _tracker, _evidence = (
        _prepare_reviewed_generation(tmp_path)
    )
    schema = tmp_path / "published" / "layout-12-manifest.schema.json"
    schema.parent.mkdir()
    schema.write_text(json.dumps(_test_schema()), encoding="utf-8")
    tracker_out = schema.parent / "STAGE_17_ECF_BASELINE.md"
    if alias_kind == "symlink":
        manifest_out = schema.parent / "layout-12-manifest.json"
        try:
            manifest_out.symlink_to(pdf)
        except OSError:
            pytest.skip("symlinks are not supported for this test user")
    else:
        if os.name != "nt":
            pytest.skip("case aliases require a case-insensitive filesystem")
        manifest_out = Path(str(pdf).swapcase())

    with pytest.raises(artifacts.ArtifactPathError):
        artifacts.promote_artifacts(
            work_dir,
            manifest_out,
            tracker_out,
            schema_path=schema,
        )


def test_apply_and_promote_reject_path_aliases_including_hardlinks(tmp_path: Path) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    records = _valid_records(reviewed=False)
    pdf = _manual(tmp_path)
    work_dir = tmp_path / "work"
    manifest = work_dir / "candidate" / "layout-12-manifest.json"
    tracker = work_dir / "candidate" / "STAGE_17_ECF_BASELINE.md"
    artifacts.build_artifacts(records, work_dir, manifest, tracker, pdf=pdf)
    evidence_alias = tmp_path / "evidence-hardlink.json"
    try:
        os.link(manifest, evidence_alias)
    except OSError:
        pytest.skip("hardlinks are not supported on this filesystem")
    with pytest.raises(artifacts.ArtifactPathError):
        artifacts.apply_review_evidence(work_dir, [evidence_alias])

    evidence_alias.unlink()
    evidence = tmp_path / "review.json"
    evidence.write_text(
        json.dumps(_provenance_evidence(records, _provenance(work_dir))), encoding="utf-8"
    )
    artifacts.apply_review_evidence(work_dir, [evidence])
    same_output = tmp_path / "same-published-output"
    schema = tmp_path / "schema.json"
    schema.write_text(json.dumps(_test_schema()), encoding="utf-8")
    with pytest.raises(artifacts.ArtifactPathError):
        artifacts.promote_artifacts(
            work_dir, same_output, same_output, schema_path=schema
        )

    schema.unlink()
    os.link(manifest, schema)
    with pytest.raises(artifacts.ArtifactPathError):
        artifacts.promote_artifacts(
            work_dir,
            tmp_path / "published-manifest",
            tmp_path / "published-tracker",
            schema_path=schema,
        )


def _crash_pair_transaction(
    tmp_path: Path, boundary: str, *, outputs_exist: bool
) -> tuple[object, Path, Path, Path]:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    work_dir = tmp_path / "work"
    first = tmp_path / "published" / "manifest.json"
    second = tmp_path / "published" / "tracker.md"
    if outputs_exist:
        artifacts.replace_artifacts_durably(
            work_dir,
            [(first, b"old manifest"), (second, b"old tracker")],
        )

    class AbruptStop(BaseException):
        pass

    def interrupt(current: str) -> None:
        if current == boundary:
            raise AbruptStop(current)

    with pytest.raises(AbruptStop):
        artifacts.replace_artifacts_durably(
            work_dir,
            [(first, b"new manifest"), (second, b"new tracker")],
            interrupt=interrupt,
        )
    return artifacts, work_dir, first, second


@pytest.mark.parametrize("outputs_exist", [False, True], ids=["new", "existing"])
@pytest.mark.parametrize("operation", ["delete", "corrupt"])
@pytest.mark.parametrize("descriptor_index", [0, 1, 2])
def test_committed_pair_recovers_after_late_loss_of_any_descriptor_replica(
    tmp_path: Path,
    outputs_exist: bool,
    operation: str,
    descriptor_index: int,
) -> None:
    artifacts, work_dir, first, second = _crash_pair_transaction(
        tmp_path, "after_removed_descriptor_1", outputs_exist=outputs_exist
    )
    transaction_dir = next((work_dir / ".artifact-transactions").iterdir())
    descriptors = [
        work_dir / ".artifact-transaction.json",
        work_dir / ".artifact-transaction.backup.json",
        transaction_dir / "descriptor.json",
    ]
    selected = descriptors[descriptor_index]
    if operation == "delete":
        selected.unlink(missing_ok=True)
    else:
        selected.write_text('{"corrupt": true}', encoding="utf-8")

    assert artifacts.read_artifact_pair(work_dir, first, second) == (
        b"new manifest",
        b"new tracker",
    )


def test_descriptor_identity_uses_two_of_three_quorum_when_one_valid_copy_is_stale(
    tmp_path: Path,
) -> None:
    artifacts, work_dir, first, second = _crash_pair_transaction(
        tmp_path, "after_journal", outputs_exist=True
    )
    primary = work_dir / ".artifact-transaction.json"
    stale = json.loads(primary.read_text(encoding="utf-8"))
    stale_id = "0" * 32
    stale_dir = work_dir / ".artifact-transactions" / stale_id
    stale["transactionId"] = stale_id
    stale["descriptorPaths"] = [
        str((work_dir / ".artifact-transaction.json").resolve()),
        str((work_dir / ".artifact-transaction.backup.json").resolve()),
        str((stale_dir / "descriptor.json").resolve()),
    ]
    for index, entry in enumerate(stale["entries"]):
        target = Path(entry["target"])
        entry["payloads"] = [
            str((stale_dir / f"new-{index}.0.bin").resolve()),
            str(
                (
                    target.parent
                    / f".{target.name}.{stale_id}.{index}.recovery.bin"
                ).resolve()
            ),
        ]
    primary.write_text(json.dumps(stale), encoding="utf-8")

    assert artifacts.read_artifact_pair(work_dir, first, second) == (
        b"new manifest",
        b"new tracker",
    )


def test_descriptor_state_uses_two_of_three_quorum(tmp_path: Path) -> None:
    artifacts, work_dir, first, second = _crash_pair_transaction(
        tmp_path, "after_payloads", outputs_exist=True
    )
    primary = work_dir / ".artifact-transaction.json"
    minority = json.loads(primary.read_text(encoding="utf-8"))
    minority["state"] = "prepared"
    primary.write_text(json.dumps(minority), encoding="utf-8")

    assert artifacts.read_artifact_pair(work_dir, first, second) == (
        b"old manifest",
        b"old tracker",
    )


@pytest.mark.parametrize("outputs_exist", [False, True], ids=["new", "existing"])
@pytest.mark.parametrize(
    "boundary",
    [
        "after_journal",
        "after_replace_0",
        "after_committed",
        "after_removed_descriptor_0",
        "after_removed_descriptor_1",
    ],
)
@pytest.mark.parametrize("operation", ["delete", "corrupt"])
@pytest.mark.parametrize("entry_index", [0, 1])
@pytest.mark.parametrize("copy_index", [0, 1])
def test_payload_loss_or_corruption_matrix_recovers_committed_pair(
    tmp_path: Path,
    outputs_exist: bool,
    boundary: str,
    operation: str,
    entry_index: int,
    copy_index: int,
) -> None:
    artifacts, work_dir, first, second = _crash_pair_transaction(
        tmp_path, boundary, outputs_exist=outputs_exist
    )
    descriptors = [
        work_dir / ".artifact-transaction.json",
        work_dir / ".artifact-transaction.backup.json",
        *(
            (work_dir / ".artifact-transactions").glob("*/descriptor.json")
        ),
    ]
    descriptor_path = next(path for path in descriptors if path.is_file())
    descriptor = json.loads(descriptor_path.read_text(encoding="utf-8"))
    payload = Path(descriptor["entries"][entry_index]["payloads"][copy_index])
    if operation == "delete":
        payload.unlink(missing_ok=True)
    else:
        payload.write_bytes(b"corrupt payload copy")

    assert artifacts.read_artifact_pair(work_dir, first, second) == (
        b"new manifest",
        b"new tracker",
    )


@pytest.mark.parametrize("marker_failure", ["delete", "corrupt"])
def test_clean_pair_requires_redundant_committed_identity_and_checks_live_hashes(
    tmp_path: Path, marker_failure: str
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    work_dir = tmp_path / "work"
    first = tmp_path / "published" / "manifest.json"
    second = tmp_path / "published" / "tracker.md"
    artifacts.replace_artifacts_durably(
        work_dir,
        [(first, b"manifest"), (second, b"tracker")],
    )

    markers = artifacts._pair_marker_paths(work_dir, [first, second])
    assert len(markers) == 3
    assert all(path.is_file() for path in markers)
    if marker_failure == "delete":
        markers[0].unlink()
    else:
        markers[0].write_text('{"corrupt": true}', encoding="utf-8")
    assert artifacts.read_artifact_pair(work_dir, first, second) == (
        b"manifest",
        b"tracker",
    )

    first.write_bytes(b"unmarked mutation")
    with pytest.raises(artifacts.ArtifactPromotionError, match="committed pair"):
        artifacts.read_artifact_pair(work_dir, first, second)


def test_read_pair_rejects_arbitrary_existing_bytes_without_committed_identity(
    tmp_path: Path,
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    first = tmp_path / "manifest.json"
    second = tmp_path / "tracker.md"
    first.write_bytes(b"arbitrary manifest")
    second.write_bytes(b"arbitrary tracker")

    with pytest.raises(artifacts.ArtifactPromotionError, match="committed pair"):
        artifacts.read_artifact_pair(tmp_path / "work", first, second)


def test_clean_pair_marker_identity_uses_two_of_three_quorum(tmp_path: Path) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    work_dir = tmp_path / "work"
    first = tmp_path / "manifest.json"
    second = tmp_path / "tracker.md"
    artifacts.replace_artifacts_durably(
        work_dir,
        [(first, b"manifest"), (second, b"tracker")],
    )
    markers = artifacts._pair_marker_paths(work_dir, [first, second])
    stale = json.loads(markers[0].read_text(encoding="utf-8"))
    stale["transactionId"] = "f" * 32
    markers[0].write_text(json.dumps(stale), encoding="utf-8")

    assert artifacts.read_artifact_pair(work_dir, first, second) == (
        b"manifest",
        b"tracker",
    )


def test_committed_pair_identity_is_shared_across_work_directories(
    tmp_path: Path,
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    first = tmp_path / "published" / "manifest.json"
    second = tmp_path / "published" / "tracker.md"
    work_a = tmp_path / "work-a"
    work_b = tmp_path / "work-b"

    artifacts.replace_artifacts_durably(
        work_a,
        [(first, b"manifest"), (second, b"tracker")],
    )

    assert artifacts._pair_marker_paths(work_a, [first, second]) == (
        artifacts._pair_marker_paths(work_b, [second, first])
    )
    assert artifacts.read_artifact_pair(work_b, first, second) == (
        b"manifest",
        b"tracker",
    )
    assert artifacts.read_artifact_pair(work_b, second, first) == (
        b"tracker",
        b"manifest",
    )


def test_different_work_directories_serialize_the_same_destination_set(
    tmp_path: Path,
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    first = tmp_path / "published" / "manifest.json"
    second = tmp_path / "published" / "tracker.md"
    paused_after_first_target = threading.Event()
    release_first_writer = threading.Event()
    results: list[str] = []
    errors: list[BaseException] = []

    def first_interrupt(boundary: str) -> None:
        if boundary == "after_replace_0":
            paused_after_first_target.set()
            if not release_first_writer.wait(timeout=5):
                raise TimeoutError("first destination-set writer was not released")

    def publish(label: str, work_dir: Path, payload: bytes, interrupt=None) -> None:
        try:
            artifacts.replace_artifacts_durably(
                work_dir,
                [(first, payload + b" manifest"), (second, payload + b" tracker")],
                interrupt=interrupt,
                lock_timeout=5,
            )
            results.append(label)
        except BaseException as error:
            errors.append(error)

    first_thread = threading.Thread(
        target=publish,
        args=("A", tmp_path / "work-a", b"A", first_interrupt),
    )
    second_thread = threading.Thread(
        target=publish,
        args=("B", tmp_path / "work-b", b"B"),
    )
    first_thread.start()
    assert paused_after_first_target.wait(timeout=5)
    second_thread.start()
    second_thread.join(timeout=0.1)
    try:
        assert second_thread.is_alive()
    finally:
        release_first_writer.set()
        first_thread.join(timeout=5)
        second_thread.join(timeout=5)

    assert not first_thread.is_alive()
    assert not second_thread.is_alive()
    assert errors == []
    assert results == ["A", "B"]
    assert artifacts.read_artifact_pair(
        tmp_path / "reader-work", first, second
    ) == (b"B manifest", b"B tracker")


def test_overlapping_destination_sets_serialize_on_the_shared_target(
    tmp_path: Path,
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    first = tmp_path / "published-a" / "first.json"
    shared = tmp_path / "shared" / "shared.json"
    third = tmp_path / "published-b" / "third.json"
    paused_after_first_target = threading.Event()
    release_first_writer = threading.Event()
    second_finished = threading.Event()
    errors: list[BaseException] = []

    def interrupt(boundary: str) -> None:
        if boundary == "after_replace_0":
            paused_after_first_target.set()
            if not release_first_writer.wait(timeout=5):
                raise TimeoutError("overlapping writer was not released")

    def first_writer() -> None:
        try:
            artifacts.replace_artifacts_durably(
                tmp_path / "work-a",
                [(first, b"A first"), (shared, b"A shared")],
                interrupt=interrupt,
                lock_timeout=5,
            )
        except BaseException as error:
            errors.append(error)

    def second_writer() -> None:
        try:
            artifacts.replace_artifacts_durably(
                tmp_path / "work-b",
                [(shared, b"B shared"), (third, b"B third")],
                lock_timeout=5,
            )
            second_finished.set()
        except BaseException as error:
            errors.append(error)

    first_thread = threading.Thread(target=first_writer)
    second_thread = threading.Thread(target=second_writer)
    first_thread.start()
    assert paused_after_first_target.wait(timeout=5)
    second_thread.start()
    second_thread.join(timeout=0.1)
    try:
        assert not second_finished.is_set()
        assert second_thread.is_alive()
    finally:
        release_first_writer.set()
        first_thread.join(timeout=5)
        second_thread.join(timeout=5)

    assert errors == []
    assert first.read_bytes() == b"A first"
    assert artifacts.read_artifact_pair(
        tmp_path / "reader-work", shared, third
    ) == (b"B shared", b"B third")


def test_reader_cannot_recover_an_in_flight_prepared_writer(tmp_path: Path) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    work_dir = tmp_path / "work"
    first = tmp_path / "published" / "manifest.json"
    second = tmp_path / "published" / "tracker.md"
    artifacts.replace_artifacts_durably(
        work_dir,
        [(first, b"old manifest"), (second, b"old tracker")],
    )
    prepared = threading.Event()
    release = threading.Event()
    writer_errors: list[BaseException] = []

    def interrupt(boundary: str) -> None:
        if boundary == "after_journal":
            prepared.set()
            if not release.wait(timeout=5):
                raise TimeoutError("test writer was not released")

    def writer() -> None:
        try:
            artifacts.replace_artifacts_durably(
                work_dir,
                [(first, b"new manifest"), (second, b"new tracker")],
                interrupt=interrupt,
            )
        except BaseException as error:
            writer_errors.append(error)

    thread = threading.Thread(target=writer)
    thread.start()
    assert prepared.wait(timeout=5)
    try:
        with pytest.raises(artifacts.ArtifactLockTimeout):
            artifacts.read_artifact_pair(
                work_dir, first, second, lock_timeout=0.05
            )
        assert _pair_state(first, second) == (b"old manifest", b"old tracker")
    finally:
        release.set()
        thread.join(timeout=5)

    assert not thread.is_alive()
    assert writer_errors == []
    assert artifacts.read_artifact_pair(work_dir, first, second) == (
        b"new manifest",
        b"new tracker",
    )


def test_recovery_locks_destinations_discovered_from_transaction_intent(
    tmp_path: Path,
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    recovery_work = tmp_path / "recovery-work"
    other_work = tmp_path / "other-work"
    first = tmp_path / "published" / "manifest.json"
    second = tmp_path / "published" / "tracker.md"
    artifacts.replace_artifacts_durably(
        recovery_work,
        [(first, b"old manifest"), (second, b"old tracker")],
    )

    class AbruptStop(BaseException):
        pass

    def crash_prepared(boundary: str) -> None:
        if boundary == "after_journal":
            raise AbruptStop(boundary)

    with pytest.raises(AbruptStop):
        artifacts.replace_artifacts_durably(
            recovery_work,
            [(first, b"A manifest"), (second, b"A tracker")],
            interrupt=crash_prepared,
        )

    paused_after_first_target = threading.Event()
    release_other = threading.Event()
    errors: list[BaseException] = []

    def pause_other(boundary: str) -> None:
        if boundary == "after_replace_0":
            paused_after_first_target.set()
            if not release_other.wait(timeout=5):
                raise TimeoutError("other writer was not released")

    def other_writer() -> None:
        try:
            artifacts.replace_artifacts_durably(
                other_work,
                [(first, b"B manifest"), (second, b"B tracker")],
                interrupt=pause_other,
                lock_timeout=5,
            )
        except BaseException as error:
            errors.append(error)

    thread = threading.Thread(target=other_writer)
    thread.start()
    assert paused_after_first_target.wait(timeout=5)
    try:
        with pytest.raises(artifacts.ArtifactLockTimeout):
            artifacts.recover_artifact_transaction(
                recovery_work, lock_timeout=0.05
            )
    finally:
        release_other.set()
        thread.join(timeout=5)

    assert not thread.is_alive()
    assert errors == []
    artifacts.recover_artifact_transaction(recovery_work)
    assert artifacts.read_artifact_pair(
        tmp_path / "reader", first, second
    ) == (b"B manifest", b"B tracker")


@pytest.mark.parametrize("boundary", ["after_journal", "after_replace_0"])
def test_stale_recovery_cannot_overwrite_a_later_cross_workdir_commit(
    tmp_path: Path, boundary: str
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    stale_work = tmp_path / "stale-work"
    later_work = tmp_path / "later-work"
    first = tmp_path / "published" / "manifest.json"
    second = tmp_path / "published" / "tracker.md"
    artifacts.replace_artifacts_durably(
        stale_work,
        [(first, b"old manifest"), (second, b"old tracker")],
    )

    class AbruptStop(BaseException):
        pass

    def interrupt(current: str) -> None:
        if current == boundary:
            raise AbruptStop(current)

    with pytest.raises(AbruptStop):
        artifacts.replace_artifacts_durably(
            stale_work,
            [(first, b"stale manifest"), (second, b"stale tracker")],
            interrupt=interrupt,
        )
    artifacts.replace_artifacts_durably(
        later_work,
        [(first, b"later manifest"), (second, b"later tracker")],
    )

    artifacts.recover_artifact_transaction(stale_work)

    assert artifacts.read_artifact_pair(
        tmp_path / "reader", first, second
    ) == (b"later manifest", b"later tracker")
    assert not (stale_work / ".artifact-transactions").exists()


@pytest.mark.parametrize(
    "boundary",
    [
        "after_replace_0",
        "after_replace_1",
        "after_committed",
        "after_marker_2",
    ],
)
@pytest.mark.parametrize("shared_first", [False, True], ids=["forward", "reverse"])
def test_overlapping_later_commit_supersedes_stale_recovery_per_target(
    tmp_path: Path, boundary: str, shared_first: bool
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    stale_work = tmp_path / "stale-work"
    later_work = tmp_path / "later-work"
    first = tmp_path / "a-first" / "first.json"
    shared = tmp_path / "b-shared" / "shared.json"
    third = tmp_path / "c-third" / "third.json"
    artifacts.replace_artifacts_durably(
        stale_work,
        [(first, b"old first"), (shared, b"old shared")],
    )

    class AbruptStop(BaseException):
        pass

    def interrupt(current: str) -> None:
        if current == boundary:
            raise AbruptStop(current)

    stale_replacements = [
        (shared, b"stale shared"),
        (first, b"stale first"),
    ] if shared_first else [
        (first, b"stale first"),
        (shared, b"stale shared"),
    ]
    with pytest.raises(AbruptStop):
        artifacts.replace_artifacts_durably(
            stale_work,
            stale_replacements,
            interrupt=interrupt,
        )
    later_replacements = [
        (third, b"later third"),
        (shared, b"later shared"),
    ] if shared_first else [
        (shared, b"later shared"),
        (third, b"later third"),
    ]
    artifacts.replace_artifacts_durably(later_work, later_replacements)
    later_marker = artifacts._load_pair_marker_quorum(
        later_work, [shared, third]
    )

    artifacts.recover_artifact_transaction(stale_work)

    assert first.read_bytes() == b"old first"
    assert artifacts.read_artifact_pair(
        tmp_path / "reader", shared, third
    ) == (b"later shared", b"later third")
    shared_marker = artifacts._load_target_marker_quorum(shared)
    assert shared_marker["transactionId"] == later_marker["transactionId"]
    assert shared_marker["sha256"] == artifacts.sha256_bytes(b"later shared")
    assert shared_marker["version"] >= 2
    assert not (stale_work / ".artifact-transactions").exists()


@pytest.mark.parametrize("lost_replica", [0, 1, 2])
def test_target_identity_quorum_tolerates_loss_of_each_single_replica(
    tmp_path: Path, lost_replica: int
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    first = tmp_path / "published" / "first.json"
    second = tmp_path / "published" / "second.json"
    artifacts.replace_artifacts_durably(
        tmp_path / "work",
        [(first, b"first"), (second, b"second")],
    )
    marker_paths = artifacts._target_marker_paths(first)
    marker_paths[lost_replica].unlink()

    marker = artifacts._load_target_marker_quorum(first)

    assert marker["target"] == str(first.resolve())
    assert marker["sha256"] == artifacts.sha256_bytes(b"first")
    assert marker["version"] == 1


def test_artifact_lock_is_os_backed_across_processes(tmp_path: Path) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    writer_work_dir = tmp_path / "writer-work"
    reader_work_dir = tmp_path / "reader-work"
    first = tmp_path / "published" / "manifest.json"
    second = tmp_path / "published" / "tracker.md"
    artifacts.replace_artifacts_durably(
        writer_work_dir,
        [(first, b"manifest"), (second, b"tracker")],
    )
    ready = tmp_path / "process-ready"
    release = tmp_path / "process-release"
    process = multiprocessing.get_context("spawn").Process(
        target=_hold_artifact_lock_in_process,
        args=(
            str(writer_work_dir),
            str(ready),
            str(release),
            str(first),
            str(second),
        ),
    )
    process.start()
    deadline = time.monotonic() + 5
    while not ready.exists() and time.monotonic() < deadline:
        time.sleep(0.01)
    assert ready.exists()
    try:
        with pytest.raises(artifacts.ArtifactLockTimeout):
            artifacts.read_artifact_pair(
                reader_work_dir, first, second, lock_timeout=0.05
            )
    finally:
        release.write_text("release", encoding="utf-8")
        process.join(timeout=5)
        if process.is_alive():
            process.terminate()
            process.join(timeout=5)
    assert process.exitcode == 0


@pytest.mark.skipif(os.name == "nt", reason="requires POSIX directory flock")
def test_posix_directory_lock_survives_auxiliary_lock_path_replacement(
    tmp_path: Path,
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    published = tmp_path / "published"
    published.mkdir()
    first = published / "manifest.json"
    second = published / "tracker.md"
    entered = threading.Event()
    release = threading.Event()
    errors: list[BaseException] = []

    def holder() -> None:
        try:
            with artifacts._artifact_lock(
                tmp_path / "work-a", targets=[first, second], timeout=5
            ):
                entered.set()
                if not release.wait(timeout=5):
                    raise TimeoutError("POSIX lock holder was not released")
        except BaseException as error:
            errors.append(error)

    thread = threading.Thread(target=holder)
    thread.start()
    assert entered.wait(timeout=5)
    auxiliary = published / ".artifact.lock"
    auxiliary.write_bytes(b"replaceable auxiliary path")
    auxiliary.unlink()
    auxiliary.write_bytes(b"replacement auxiliary path")
    try:
        with pytest.raises(artifacts.ArtifactLockTimeout):
            with artifacts._artifact_lock(
                tmp_path / "work-b", targets=[first, second], timeout=0.05
            ):
                raise AssertionError("replacement pathname bypassed directory inode lock")
    finally:
        release.set()
        thread.join(timeout=5)

    assert not thread.is_alive()
    assert errors == []


def test_posix_backend_contract_keys_locking_by_canonical_destination_directory(
    tmp_path: Path, monkeypatch: pytest.MonkeyPatch
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    registry: dict[str, threading.Lock] = {}
    registry_guard = threading.Lock()

    @contextmanager
    def fake_posix_directory_lock(directory: Path, *, deadline: float):
        key = os.path.normcase(str(Path(directory).resolve()))
        with registry_guard:
            lock = registry.setdefault(key, threading.Lock())
        if not lock.acquire(timeout=max(0.0, deadline - time.monotonic())):
            raise artifacts.ArtifactLockTimeout(
                f"timed out waiting for artifact lock: {directory}"
            )
        try:
            yield
        finally:
            lock.release()

    monkeypatch.setattr(artifacts, "_IS_WINDOWS", False)
    monkeypatch.setattr(
        artifacts, "_posix_directory_lock", fake_posix_directory_lock
    )
    published = tmp_path / "published"
    first = published / "manifest.json"
    second = published / "tracker.md"
    entered = threading.Event()
    release = threading.Event()

    def holder() -> None:
        with artifacts._artifact_lock(
            tmp_path / "work-a", targets=[first, second], timeout=5
        ):
            entered.set()
            release.wait(timeout=5)

    thread = threading.Thread(target=holder)
    thread.start()
    assert entered.wait(timeout=5)
    try:
        with pytest.raises(artifacts.ArtifactLockTimeout):
            with artifacts._artifact_lock(
                tmp_path / "work-b", targets=[second, first], timeout=0.05
            ):
                raise AssertionError("canonical destination lock was bypassed")
    finally:
        release.set()
        thread.join(timeout=5)

    assert not thread.is_alive()
    assert not (published / ".artifact.lock").exists()


def test_concurrent_promotions_serialize_without_lost_success(tmp_path: Path) -> None:
    (tmp_path / "first").mkdir()
    artifacts, _records, _pdf, work_dir, manifest, tracker, _evidence = (
        _prepare_reviewed_generation(tmp_path / "first")
    )
    second_root = tmp_path / "second"
    second_root.mkdir()
    second_records = _valid_records(reviewed=False)
    second_records[0] = {**second_records[0], "title": "Second serialized generation"}
    second_pdf = _manual(second_root)
    second_work = second_root / "work"
    second_manifest = second_work / "candidate" / "layout-12-manifest.json"
    second_tracker = second_work / "candidate" / "STAGE_17_ECF_BASELINE.md"
    artifacts.build_artifacts(
        second_records,
        second_work,
        second_manifest,
        second_tracker,
        pdf=second_pdf,
    )
    second_evidence = second_root / "review.json"
    second_evidence.write_text(
        json.dumps(
            _provenance_evidence(second_records, _provenance(second_work))
        ),
        encoding="utf-8",
    )
    artifacts.apply_review_evidence(second_work, [second_evidence])
    expected_second_manifest = second_manifest.read_bytes()
    expected_second_tracker = second_tracker.read_bytes()

    schema = tmp_path / "published" / "layout-12-manifest.schema.json"
    schema.parent.mkdir()
    schema.write_text(json.dumps(_test_schema()), encoding="utf-8")
    manifest_out = schema.parent / "layout-12-manifest.json"
    tracker_out = schema.parent / "STAGE_17_ECF_BASELINE.md"
    first_validated = threading.Event()
    release_first = threading.Event()
    results: list[str] = []
    errors: list[BaseException] = []

    def stage_second_generation() -> None:
        for relative in (
            "candidate/layout-12-manifest.json",
            "candidate/STAGE_17_ECF_BASELINE.md",
            "candidate/generation.json",
            "build-state.json",
            "quarantine.json",
        ):
            destination = work_dir / relative
            destination.parent.mkdir(parents=True, exist_ok=True)
            shutil.copyfile(second_work / relative, destination)
        first_validated.set()
        if not release_first.wait(timeout=5):
            raise TimeoutError("first promotion was not released")

    def promote(label: str, hook=None) -> None:
        try:
            artifacts.promote_artifacts(
                work_dir,
                manifest_out,
                tracker_out,
                schema_path=schema,
                before_publish=hook,
                lock_timeout=5,
            )
            results.append(label)
        except BaseException as error:
            errors.append(error)

    first_thread = threading.Thread(
        target=promote, args=("first", stage_second_generation)
    )
    second_thread = threading.Thread(target=promote, args=("second",))
    first_thread.start()
    assert first_validated.wait(timeout=5)
    second_thread.start()
    second_thread.join(timeout=0.1)
    try:
        assert second_thread.is_alive()
    finally:
        release_first.set()
        first_thread.join(timeout=5)
        second_thread.join(timeout=5)

    assert not first_thread.is_alive()
    assert not second_thread.is_alive()
    assert errors == []
    assert results == ["first", "second"]
    assert manifest_out.read_bytes() == expected_second_manifest
    assert tracker_out.read_bytes() == expected_second_tracker


def test_concurrent_promotions_from_different_work_dirs_serialize_targets(
    tmp_path: Path,
) -> None:
    first_root = tmp_path / "first"
    first_root.mkdir()
    artifacts, _records, _pdf, first_work, _manifest, _tracker, _evidence = (
        _prepare_reviewed_generation(first_root)
    )
    second_root = tmp_path / "second"
    second_root.mkdir()
    second_records = _valid_records(reviewed=False)
    second_records[0] = {**second_records[0], "title": "Second work directory"}
    second_pdf = _manual(second_root)
    second_work = second_root / "work"
    second_manifest = second_work / "candidate" / "layout-12-manifest.json"
    second_tracker = second_work / "candidate" / "STAGE_17_ECF_BASELINE.md"
    artifacts.build_artifacts(
        second_records,
        second_work,
        second_manifest,
        second_tracker,
        pdf=second_pdf,
    )
    second_evidence = second_root / "review.json"
    second_evidence.write_text(
        json.dumps(
            _provenance_evidence(second_records, _provenance(second_work))
        ),
        encoding="utf-8",
    )
    artifacts.apply_review_evidence(second_work, [second_evidence])
    expected_manifest = second_manifest.read_bytes()
    expected_tracker = second_tracker.read_bytes()

    published = tmp_path / "published"
    published.mkdir()
    schema = published / "layout-12-manifest.schema.json"
    schema.write_text(json.dumps(_test_schema()), encoding="utf-8")
    manifest_out = published / "layout-12-manifest.json"
    tracker_out = published / "STAGE_17_ECF_BASELINE.md"
    paused_after_first_target = threading.Event()
    release_first = threading.Event()
    results: list[str] = []
    errors: list[BaseException] = []

    def interrupt(boundary: str) -> None:
        if boundary == "after_replace_0":
            paused_after_first_target.set()
            if not release_first.wait(timeout=5):
                raise TimeoutError("first promotion was not released")

    def promote(label: str, work_dir: Path, hook=None) -> None:
        try:
            artifacts.promote_artifacts(
                work_dir,
                manifest_out,
                tracker_out,
                schema_path=schema,
                interrupt=hook,
                lock_timeout=5,
            )
            results.append(label)
        except BaseException as error:
            errors.append(error)

    first_thread = threading.Thread(
        target=promote, args=("A", first_work, interrupt)
    )
    second_thread = threading.Thread(
        target=promote, args=("B", second_work)
    )
    first_thread.start()
    assert paused_after_first_target.wait(timeout=5)
    second_thread.start()
    second_thread.join(timeout=0.1)
    try:
        assert second_thread.is_alive()
    finally:
        release_first.set()
        first_thread.join(timeout=5)
        second_thread.join(timeout=5)

    assert not first_thread.is_alive()
    assert not second_thread.is_alive()
    assert errors == []
    assert results == ["A", "B"]
    assert manifest_out.read_bytes() == expected_manifest
    assert tracker_out.read_bytes() == expected_tracker
    assert artifacts.read_artifact_pair(
        tmp_path / "reader", manifest_out, tracker_out
    ) == (expected_manifest, expected_tracker)


@pytest.mark.parametrize("outputs_exist", [False, True], ids=["first-ever", "existing"])
@pytest.mark.parametrize("operation", ["delete", "corrupt"])
@pytest.mark.parametrize(
    ("boundary", "survivor_index"),
    [
        ("after_staging_descriptor_0", 0),
        ("after_staging_descriptor_1", 0),
        ("after_staging_descriptor_1", 1),
        ("after_staging_descriptor_2", 0),
        ("after_staging_descriptor_2", 1),
        ("after_staging_descriptor_2", 2),
    ],
)
def test_sole_invalid_early_descriptor_requires_a_proven_prior_pair(
    tmp_path: Path,
    outputs_exist: bool,
    operation: str,
    boundary: str,
    survivor_index: int,
) -> None:
    artifacts, work_dir, first, second = _crash_pair_transaction(
        tmp_path, boundary, outputs_exist=outputs_exist
    )
    transaction_dir = next((work_dir / ".artifact-transactions").iterdir())
    descriptors = [
        work_dir / ".artifact-transaction.json",
        work_dir / ".artifact-transaction.backup.json",
        transaction_dir / "descriptor.json",
    ]
    created_count = int(boundary[-1]) + 1
    for index, descriptor in enumerate(descriptors[:created_count]):
        if index != survivor_index:
            descriptor.unlink()
    survivor = descriptors[survivor_index]
    if operation == "delete":
        survivor.unlink()
    else:
        survivor.write_text('{"corrupt": true}', encoding="utf-8")

    if not outputs_exist:
        with pytest.raises(artifacts.ArtifactPromotionError, match="quarantined.*unproven"):
            artifacts.recover_artifact_transaction(work_dir)
        assert (transaction_dir / "recovery-quarantine.json").is_file()
        with pytest.raises(artifacts.ArtifactPromotionError, match="quarantined.*unproven"):
            artifacts.replace_artifacts_durably(
                work_dir,
                [(first, b"later manifest"), (second, b"later tracker")],
            )
        assert _pair_state(first, second) == (None, None)
        return

    artifacts.recover_artifact_transaction(work_dir)
    assert _pair_state(first, second) == (b"old manifest", b"old tracker")
    artifacts.replace_artifacts_durably(
        work_dir,
        [(first, b"later manifest"), (second, b"later tracker")],
    )
    assert artifacts.read_artifact_pair(work_dir, first, second) == (
        b"later manifest",
        b"later tracker",
    )


def _make_filesystem_alias(source: Path, alias: Path, alias_kind: str) -> None:
    alias.parent.mkdir(parents=True, exist_ok=True)
    try:
        if alias_kind == "hardlink":
            os.link(source, alias)
        else:
            alias.symlink_to(source)
    except OSError:
        pytest.skip(f"{alias_kind} is not supported for this test user")


def test_final_component_symlink_is_rejected_before_destination_locking(
    tmp_path: Path,
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    first = tmp_path / "real-a" / "first.json"
    shared = tmp_path / "real-b" / "shared.json"
    third = tmp_path / "third" / "third.json"
    alias = tmp_path / "alias" / "shared-link.json"
    artifacts.replace_artifacts_durably(
        tmp_path / "work-a",
        [(first, b"old first"), (shared, b"old shared")],
    )
    _make_filesystem_alias(shared, alias, "symlink")
    paused = threading.Event()
    release = threading.Event()
    errors: list[BaseException] = []

    def interrupt(boundary: str) -> None:
        if boundary == "after_replace_0":
            paused.set()
            if not release.wait(timeout=5):
                raise TimeoutError("real-path writer was not released")

    def real_writer() -> None:
        try:
            artifacts.replace_artifacts_durably(
                tmp_path / "work-a",
                [(first, b"real first"), (shared, b"real shared")],
                interrupt=interrupt,
                lock_timeout=5,
            )
        except BaseException as error:
            errors.append(error)

    thread = threading.Thread(target=real_writer)
    thread.start()
    assert paused.wait(timeout=5)
    try:
        with pytest.raises(artifacts.ArtifactPathError, match="symlink|reparse"):
            artifacts.replace_artifacts_durably(
                tmp_path / "work-b",
                [(alias, b"alias shared"), (third, b"alias third")],
                lock_timeout=0.05,
            )
        assert alias.is_symlink()
        assert shared.read_bytes() == b"old shared"
        assert not third.exists()
    finally:
        release.set()
        thread.join(timeout=5)

    assert not thread.is_alive()
    assert errors == []
    assert artifacts.read_artifact_pair(
        tmp_path / "reader", first, shared
    ) == (b"real first", b"real shared")


@pytest.mark.parametrize("alias_kind", ["hardlink", "symlink"])
def test_artifact_lock_alias_never_changes_normative_pdf(
    tmp_path: Path, alias_kind: str
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    pdf = _manual(tmp_path)
    original_pdf = pdf.read_bytes()
    work_dir = tmp_path / "work"
    lock_path = work_dir / ".artifact.lock"
    _make_filesystem_alias(pdf, lock_path, alias_kind)

    with pytest.raises(artifacts.ArtifactPathError):
        artifacts.build_artifacts(
            _valid_records(reviewed=False),
            work_dir,
            work_dir / "candidate" / "layout-12-manifest.json",
            work_dir / "candidate" / "STAGE_17_ECF_BASELINE.md",
            pdf=pdf,
        )

    assert pdf.read_bytes() == original_pdf


@pytest.mark.parametrize("alias_kind", ["hardlink", "symlink"])
@pytest.mark.parametrize("dynamic_class", ["descriptor", "payload"])
def test_dynamic_recovery_file_creation_is_exclusive_and_preserves_pdf(
    tmp_path: Path, alias_kind: str, dynamic_class: str
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    pdf = _manual(tmp_path)
    original_pdf = pdf.read_bytes()
    suffix = "descriptor.json" if dynamic_class == "descriptor" else "new-0.0.bin"
    dynamic_path = tmp_path / "work" / ".artifact-transactions" / ("a" * 32) / suffix
    _make_filesystem_alias(pdf, dynamic_path, alias_kind)

    with pytest.raises(artifacts.ArtifactPathError):
        artifacts._create_durable_file_exclusive(
            dynamic_path,
            b"transaction data",
            protected_paths=[pdf],
        )

    assert pdf.read_bytes() == original_pdf


@pytest.mark.parametrize("alias_kind", ["hardlink", "symlink"])
def test_persistent_marker_alias_blocks_replacement_without_changing_pdf(
    tmp_path: Path, alias_kind: str
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    pdf = _manual(tmp_path)
    original_pdf = pdf.read_bytes()
    work_dir = tmp_path / "work"
    first = tmp_path / "published" / "manifest.json"
    second = tmp_path / "published" / "tracker.md"
    artifacts.replace_artifacts_durably(
        work_dir,
        [(first, b"old manifest"), (second, b"old tracker")],
        protected_paths=[pdf],
    )
    marker = artifacts._pair_marker_paths(work_dir, [first, second])[0]
    marker.unlink()
    _make_filesystem_alias(pdf, marker, alias_kind)

    with pytest.raises(artifacts.ArtifactPathError):
        artifacts.replace_artifacts_durably(
            work_dir,
            [(first, b"new manifest"), (second, b"new tracker")],
            protected_paths=[pdf],
        )

    assert pdf.read_bytes() == original_pdf
    assert _pair_state(first, second) == (b"old manifest", b"old tracker")


@pytest.mark.parametrize("alias_kind", ["hardlink", "symlink"])
def test_atomic_temporary_alias_fails_without_truncating_pdf_or_target(
    tmp_path: Path, alias_kind: str
) -> None:
    artifacts = importlib.import_module("ecf_layout.artifacts")
    pdf = _manual(tmp_path)
    original_pdf = pdf.read_bytes()
    target = tmp_path / "target.json"
    target.write_bytes(b"old target")
    temporary = tmp_path / ".target.fixed.tmp"
    _make_filesystem_alias(pdf, temporary, alias_kind)

    with pytest.raises(artifacts.ArtifactPathError):
        artifacts._write_bytes_durably_atomically(
            target,
            b"new target",
            protected_paths=[pdf],
            temporary_path=temporary,
        )

    assert pdf.read_bytes() == original_pdf
    assert target.read_bytes() == b"old target"
