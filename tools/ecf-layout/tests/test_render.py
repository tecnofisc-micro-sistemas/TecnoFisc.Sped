from pathlib import Path

import pytest

import ecf_layout.render as render
from ecf_layout.render import render_page, render_suspicious_pages


def test_render_receives_only_suspicious_page_numbers(tmp_path: Path) -> None:
    received: list[int] = []

    def render_page(_pdf: Path, page: int, output: Path) -> Path:
        received.append(page)
        output.parent.mkdir(parents=True, exist_ok=True)
        output.write_bytes(b"png")
        return output

    items = [
        {"code": "0000", "pages": [12, 10]},
        {"code": "0010", "pages": [12]},
        {"code": "0020", "pages": []},
    ]

    rendered = render_suspicious_pages(
        tmp_path / "manual.pdf", items, tmp_path / "rendered", render_page=render_page
    )

    assert received == [10, 12]
    assert rendered == {
        10: tmp_path / "rendered" / "page-010.png",
        12: tmp_path / "rendered" / "page-012.png",
    }


def test_render_page_uses_one_based_pdftoppm_and_requires_output(tmp_path: Path, monkeypatch) -> None:
    pdf = tmp_path / "manual.pdf"
    output = tmp_path / "rendered" / "page-007.png"
    calls: list[list[str]] = []

    def successful_run(command, **kwargs):
        calls.append(command)
        Path(command[-1] + ".png").write_bytes(b"png")

    monkeypatch.setattr(render.subprocess, "run", successful_run)

    assert render_page(pdf, 7, output) == output
    assert calls == [[
        "pdftoppm", "-f", "7", "-l", "7", "-singlefile", "-png", str(pdf), str(output.with_suffix(""))
    ]]

    monkeypatch.setattr(render.subprocess, "run", lambda *_args, **_kwargs: None)
    output.write_bytes(b"stale png")
    with pytest.raises(FileNotFoundError):
        render_page(pdf, 7, output)
    assert not output.exists()
