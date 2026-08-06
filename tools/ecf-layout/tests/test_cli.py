import json
from pathlib import Path

import ecf_layout.cli as cli
from ecf_layout.cache import CacheKey
from ecf_layout.cli import PrepareResult
from ecf_layout.manifest import EXPECTED_CODES, block_for_code


FIXTURES = Path(__file__).parent / "fixtures"
PDF_SHA256 = "pdf-sha256"
CONVERTER_SHA256 = "converter-sha256"


def _run_prepare(tmp_path: Path, monkeypatch, markdown: str) -> tuple[int, Path]:
    pdf = tmp_path / "manual.pdf"
    pdf.write_bytes(b"manual")
    work_dir = tmp_path / "work"
    cached_page = CacheKey(PDF_SHA256, CONVERTER_SHA256, 1).output_path(work_dir / "cache")
    cached_page.parent.mkdir(parents=True)
    cached_page.write_text(markdown, encoding="utf-8")
    fragments_dir = work_dir / "fragments"
    fragments_dir.mkdir()
    (fragments_dir / "STALE.md").write_text("stale\n", encoding="utf-8")

    monkeypatch.setattr(cli, "prepare_pages", lambda *_args: PrepareResult(converted=0, cache_hits=1))
    monkeypatch.setattr(cli, "sha256_file", lambda _path: PDF_SHA256)
    monkeypatch.setattr(cli, "converter_fingerprint", lambda: CONVERTER_SHA256)

    exit_code = cli.main(
        ["prepare", "--pdf", str(pdf), "--work-dir", str(work_dir), "--pages", "1-1"]
    )
    return exit_code, fragments_dir


def test_prepare_fails_closed_when_fragment_count_is_not_180(tmp_path: Path, monkeypatch) -> None:
    markdown = (FIXTURES / "plain-record-title.md").read_text(encoding="utf-8")

    exit_code, fragments_dir = _run_prepare(tmp_path, monkeypatch, markdown)

    assert exit_code == 1
    assert list(fragments_dir.glob("*.md")) == []


def test_prepare_fails_closed_when_fragmentation_has_errors(tmp_path: Path, monkeypatch) -> None:
    markdown = (FIXTURES / "competing-field-tables.md").read_text(encoding="utf-8")

    exit_code, fragments_dir = _run_prepare(tmp_path, monkeypatch, markdown)

    assert exit_code == 1
    assert list(fragments_dir.glob("*.md")) == []


def test_validate_writes_candidate_without_promoting(tmp_path: Path, monkeypatch) -> None:
    records = [
        {
            "code": code,
            "block": block_for_code(code),
            "title": f"Registro {code}",
            "pageStart": position + 1,
            "pageEnd": position + 1,
            "level": "1",
            "occurrence": "1:1",
            "fields": [
                {
                    "number": 1,
                    "name": "REG",
                    "description": "Identificacao",
                    "type": "C",
                    "size": "4",
                    "decimals": "-",
                    "required": "Sim",
                    "validValues": f"[{code}]",
                }
            ],
            "reviewed": False,
        }
        for position, code in enumerate(EXPECTED_CODES)
    ]
    monkeypatch.setattr(cli, "records_from_work_dir", lambda _work_dir, _pdf: records)

    exit_code = cli.main(["validate", "--work-dir", str(tmp_path)])

    candidate = json.loads(
        (tmp_path / "candidate" / "layout-12-manifest.json").read_text(encoding="utf-8")
    )
    assert exit_code == 0
    assert len(candidate) == 180
    assert json.loads((tmp_path / "quarantine.json").read_text(encoding="utf-8")) == {"items": []}
    assert list(tmp_path.glob("layout-12-manifest.json")) == []


def test_validate_replaces_stale_quarantine_when_cache_loading_fails(tmp_path: Path, monkeypatch) -> None:
    stale = {"items": [{"code": "OLD", "reasons": ["stale failure"], "pages": [99]}]}
    (tmp_path / "quarantine.json").write_text(json.dumps(stale), encoding="utf-8")

    def fail(_work_dir: Path, _pdf: Path):
        raise cli.ManifestValidationError("current PDF provenance failure")

    monkeypatch.setattr(cli, "records_from_work_dir", fail)

    exit_code = cli.main(["validate", "--work-dir", str(tmp_path)])

    report = json.loads((tmp_path / "quarantine.json").read_text(encoding="utf-8"))
    assert exit_code == 1
    assert report == {
        "items": [
            {"code": None, "reasons": ["current PDF provenance failure"], "pages": []}
        ]
    }


def test_validate_converts_invalid_cache_encoding_to_current_quarantine(tmp_path: Path, monkeypatch) -> None:
    stale = {"items": [{"code": "OLD", "reasons": ["stale failure"], "pages": [99]}]}
    (tmp_path / "quarantine.json").write_text(json.dumps(stale), encoding="utf-8")

    def fail(_work_dir: Path, _pdf: Path):
        raise UnicodeDecodeError("utf-8", b"\xff", 0, 1, "invalid start byte")

    monkeypatch.setattr(cli, "records_from_work_dir", fail)

    exit_code = cli.main(["validate", "--work-dir", str(tmp_path)])

    report = json.loads((tmp_path / "quarantine.json").read_text(encoding="utf-8"))
    assert exit_code == 1
    assert len(report["items"]) == 1
    assert "invalid text encoding in cache input" in report["items"][0]["reasons"][0]
    assert "stale failure" not in report["items"][0]["reasons"][0]
