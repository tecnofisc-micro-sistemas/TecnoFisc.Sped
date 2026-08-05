from pathlib import Path
from subprocess import CompletedProcess

import pytest

from ecf_layout.cache import CacheKey
from ecf_layout.converter import ConversionFailed
from ecf_layout.cli import prepare_pages


def test_cache_key_changes_when_pdf_or_converter_changes() -> None:
    original = CacheKey("pdf-a", "converter-a", 58)
    changed_pdf = CacheKey("pdf-b", "converter-a", 58)
    changed_converter = CacheKey("pdf-a", "converter-b", 58)

    assert original != changed_pdf
    assert original != changed_converter
    assert changed_pdf != changed_converter


def test_prepare_reuses_completed_pages_and_converts_only_missing_pages(tmp_path: Path) -> None:
    pdf = tmp_path / "manual.pdf"
    pdf.write_bytes(b"manual")
    calls: list[int] = []

    def run_converter(_pdf: Path, page: int) -> str:
        calls.append(page)
        return f"# page {page}\n"

    first = prepare_pages(pdf, tmp_path / ".local", range(58, 61), run_converter)
    second = prepare_pages(pdf, tmp_path / ".local", range(58, 61), run_converter)

    assert calls == [58, 59, 60]
    assert first.converted == 3
    assert first.cache_hits == 0
    assert second.converted == 0
    assert second.cache_hits == 3


def test_failed_page_never_appears_as_completed(tmp_path: Path) -> None:
    pdf = tmp_path / "manual.pdf"
    pdf.write_bytes(b"manual")

    def failing_converter(_pdf: Path, _page: int) -> str:
        raise ConversionFailed(CompletedProcess([], 1, b"", b"conversion failed"))

    with pytest.raises(ConversionFailed):
        prepare_pages(pdf, tmp_path / ".local", range(58, 59), failing_converter)

    assert list((tmp_path / ".local" / "cache").rglob("*.md")) == []
