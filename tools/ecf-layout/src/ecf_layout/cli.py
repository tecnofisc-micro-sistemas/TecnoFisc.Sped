"""Command-line entry point for resumable ECF manual preparation."""

from __future__ import annotations

import argparse
import os
import tempfile
from dataclasses import dataclass
from pathlib import Path
from typing import Callable, Iterable

from ecf_layout.cache import CacheKey, converter_fingerprint, sha256_file
from ecf_layout.converter import EmptyMarkdownError, convert_page
from ecf_layout.fragmenter import fragment_pages_with_errors, write_fragments


Converter = Callable[[Path, int], str]


@dataclass(frozen=True)
class PrepareResult:
    converted: int
    cache_hits: int


def prepare_pages(pdf: Path, work_dir: Path, pages: Iterable[int], converter: Converter = convert_page) -> PrepareResult:
    pdf_sha256 = sha256_file(pdf)
    converter_sha256 = converter_fingerprint()
    cache_dir = work_dir / "cache"
    converted = 0
    cache_hits = 0

    for page in pages:
        output = CacheKey(pdf_sha256, converter_sha256, page).output_path(cache_dir)
        if output.is_file() and output.stat().st_size > 0:
            cache_hits += 1
            print(f"cache hit: page {page}")
            continue

        markdown = converter(pdf, page)
        if not markdown.strip():
            raise EmptyMarkdownError(f"converter returned empty Markdown for page {page}")
        _write_atomically(output, markdown)
        converted += 1
        print(f"converted: page {page}")

    return PrepareResult(converted=converted, cache_hits=cache_hits)


def _write_atomically(output: Path, markdown: str) -> None:
    output.parent.mkdir(parents=True, exist_ok=True)
    descriptor, temporary_name = tempfile.mkstemp(prefix=f".{output.name}.", suffix=".tmp", dir=output.parent)
    temporary = Path(temporary_name)
    try:
        with os.fdopen(descriptor, "w", encoding="utf-8", newline="") as stream:
            stream.write(markdown)
        temporary.replace(output)
    except Exception:
        temporary.unlink(missing_ok=True)
        raise


def _parse_pages(value: str) -> range:
    try:
        start_text, end_text = value.split("-", maxsplit=1)
        start, end = int(start_text), int(end_text)
    except ValueError as error:
        raise argparse.ArgumentTypeError("pages must use START-END") from error
    if start < 1 or end < start:
        raise argparse.ArgumentTypeError("pages must be a non-empty positive range")
    return range(start, end + 1)


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(prog="ecf-layout")
    subcommands = parser.add_subparsers(dest="command", required=True)
    prepare = subcommands.add_parser("prepare")
    prepare.add_argument("--pdf", type=Path, required=True)
    prepare.add_argument("--work-dir", type=Path, required=True)
    prepare.add_argument("--pages", type=_parse_pages, required=True)
    args = parser.parse_args(argv)

    pages = args.pages
    result = prepare_pages(args.pdf, args.work_dir, pages)
    print(f"prepared: {result.converted} converted, {result.cache_hits} cache hits")
    pdf_sha256 = sha256_file(args.pdf)
    converter_sha256 = converter_fingerprint()
    cached_pages = [
        (page, CacheKey(pdf_sha256, converter_sha256, page).output_path(args.work_dir / "cache").read_text(encoding="utf-8"))
        for page in pages
    ]
    fragmentation = fragment_pages_with_errors(cached_pages)
    write_fragments(fragmentation.fragments, args.work_dir / "fragments")
    print(f"fragmented: {len({fragment.code for fragment in fragmentation.fragments})} unique codes")
    for error in fragmentation.errors:
        print(f"fragment error: {error}")
    return 0
