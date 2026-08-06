"""Render only quarantined ECF manual pages for visual review."""

from __future__ import annotations

import subprocess
from pathlib import Path
from typing import Callable, Iterable


RenderPage = Callable[[Path, int, Path], Path]


def render_page(pdf: Path, page: int, output: Path) -> Path:
    output.parent.mkdir(parents=True, exist_ok=True)
    output.unlink(missing_ok=True)
    prefix = output.with_suffix("")
    subprocess.run(
        [
            "pdftoppm",
            "-f",
            str(page),
            "-l",
            str(page),
            "-singlefile",
            "-png",
            str(pdf),
            str(prefix),
        ],
        check=True,
        capture_output=True,
    )
    if not output.is_file():
        raise FileNotFoundError(f"pdftoppm did not create expected output: {output}")
    return output


def render_suspicious_pages(
    pdf: Path,
    quarantine_items: Iterable[dict],
    output_dir: Path,
    *,
    render_page: RenderPage = render_page,
) -> dict[int, Path]:
    pages = sorted(
        {
            page
            for item in quarantine_items
            for page in item.get("pages", [])
            if isinstance(page, int) and page > 0
        }
    )
    return {
        page: render_page(pdf, page, output_dir / f"page-{page:03d}.png")
        for page in pages
    }
