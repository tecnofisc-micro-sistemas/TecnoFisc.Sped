"""One-page PDF-to-Markdown converter, isolated behind a subprocess boundary."""

from __future__ import annotations

import argparse
import subprocess
import sys
from pathlib import Path
from typing import Callable


class ConversionFailed(RuntimeError):
    def __init__(self, result: subprocess.CompletedProcess[bytes]) -> None:
        self.result = result
        super().__init__(f"converter exited with code {result.returncode}")


class EmptyMarkdownError(RuntimeError):
    pass


Runner = Callable[..., subprocess.CompletedProcess[bytes]]


def convert_page(pdf: Path, page: int, *, run: Runner = subprocess.run) -> str:
    result = run(
        [sys.executable, "-m", "ecf_layout.converter", "single", "--pdf", str(pdf), "--page", str(page)],
        capture_output=True,
        check=False,
    )
    if result.returncode != 0:
        raise ConversionFailed(result)
    if not result.stdout.strip():
        raise EmptyMarkdownError(f"converter returned empty Markdown for page {page}")
    return result.stdout.decode("utf-8")


def _write_page_markdown(pdf: Path, page: int) -> None:
    if page < 1:
        raise ValueError("page must be at least 1")

    import pymupdf4llm

    markdown = pymupdf4llm.to_markdown(str(pdf), pages=[page - 1], show_progress=False)
    sys.stdout.buffer.write(markdown.encode("utf-8"))


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser()
    subcommands = parser.add_subparsers(dest="command", required=True)
    single = subcommands.add_parser("single")
    single.add_argument("--pdf", type=Path, required=True)
    single.add_argument("--page", type=int, required=True)
    args = parser.parse_args(argv)

    try:
        _write_page_markdown(args.pdf, args.page)
    except Exception as error:
        print(f"conversion failed: {error}", file=sys.stderr)
        return 1
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
