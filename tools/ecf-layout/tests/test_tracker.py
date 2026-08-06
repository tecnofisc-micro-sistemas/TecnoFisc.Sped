import json
import importlib
import os
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
    "after_removed_descriptor_0",
    "after_removed_descriptor_1",
    "after_removed_descriptor_2",
    "after_journal_removed",
]


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
