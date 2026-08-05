from pathlib import Path

from ecf_layout.render import render_suspicious_pages


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
