from pathlib import Path

import ecf_layout.cli as cli
from ecf_layout.cache import CacheKey
from ecf_layout.cli import PrepareResult


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
