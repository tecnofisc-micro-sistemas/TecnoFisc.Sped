"""Command-line entry point for resumable ECF manual preparation."""

from __future__ import annotations

import argparse
import json
import os
import tempfile
from dataclasses import dataclass
from pathlib import Path
from typing import Callable, Iterable

from ecf_layout.cache import CacheKey, converter_fingerprint, sha256_file
from ecf_layout.artifacts import ArtifactPromotionError, build_artifacts, promote_artifacts
from ecf_layout.converter import EmptyMarkdownError, convert_page
from ecf_layout.fragmenter import fragment_pages_with_errors, write_fragments
from ecf_layout.manifest import (
    ManifestValidationError,
    records_from_work_dir,
    validate_and_promote,
    write_quarantine,
)
from ecf_layout.render import render_suspicious_pages


Converter = Callable[[Path, int], str]
EXPECTED_FRAGMENT_COUNT = 180


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
    validate = subcommands.add_parser("validate")
    validate.add_argument("--work-dir", type=Path, required=True)
    validate.add_argument(
        "--pdf",
        type=Path,
        default=Path("sped/guides/Manual_ECF_Leiaute_12_29_01_2026.pdf"),
    )
    validate.add_argument("--render-suspicious", action="store_true")
    build = subcommands.add_parser("build-artifacts")
    build.add_argument("--work-dir", type=Path, required=True)
    build.add_argument(
        "--pdf",
        type=Path,
        default=Path("sped/guides/Manual_ECF_Leiaute_12_29_01_2026.pdf"),
    )
    build.add_argument("--manifest-out", type=Path, required=True)
    build.add_argument("--tracker-out", type=Path, required=True)
    promote = subcommands.add_parser("promote")
    promote.add_argument("--work-dir", type=Path, required=True)
    promote.add_argument("--manifest-out", type=Path, required=True)
    promote.add_argument("--tracker-out", type=Path, required=True)
    args = parser.parse_args(argv)

    if args.command == "validate":
        return _validate(args.work_dir, args.pdf, render_suspicious=args.render_suspicious)
    if args.command == "build-artifacts":
        try:
            records = records_from_work_dir(args.work_dir, args.pdf)
            build_artifacts(records, args.work_dir, args.manifest_out, args.tracker_out)
        except (ManifestValidationError, OSError, UnicodeError) as error:
            print(f"artifact build failed: {error}")
            return 1
        print(f"built: {len(records)} manifest entries and {len(records)} tracker rows")
        return 0
    if args.command == "promote":
        try:
            promote_artifacts(
                args.work_dir,
                args.manifest_out,
                args.tracker_out,
                schema_path=args.manifest_out.with_name("layout-12-manifest.schema.json"),
            )
        except ArtifactPromotionError as error:
            print(f"promotion failed: {error}")
            return 1
        print("promoted: 180 reviewed manifest entries and 180 tracker rows")
        return 0

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
    unique_count = len({fragment.code for fragment in fragmentation.fragments})
    accepted = not fragmentation.errors and unique_count == EXPECTED_FRAGMENT_COUNT
    write_fragments(fragmentation.fragments if accepted else [], args.work_dir / "fragments")
    print(f"fragmented: {unique_count} unique codes")
    for error in fragmentation.errors:
        print(f"fragment error: {error}")
    if unique_count != EXPECTED_FRAGMENT_COUNT:
        print(f"fragment error: expected {EXPECTED_FRAGMENT_COUNT} unique codes, found {unique_count}")
    return 0 if accepted else 1


def _validate(work_dir: Path, pdf: Path, *, render_suspicious: bool) -> int:
    try:
        records = records_from_work_dir(work_dir, pdf)
    except (ValueError, OSError) as error:
        reason = _input_failure_reason(error)
        report = {"items": [{"code": None, "reasons": [reason], "pages": []}]}
        write_quarantine(work_dir, report["items"])
        return _report_validation_failure(work_dir, pdf, report, reason, render_suspicious)
    try:
        validate_and_promote(records, work_dir)
    except ManifestValidationError as error:
        report = json.loads((work_dir / "quarantine.json").read_text(encoding="utf-8"))
        return _report_validation_failure(work_dir, pdf, report, str(error), render_suspicious)
    print(f"validated: {len(records)} records")
    print("quarantined: 0")
    return 0


def _report_validation_failure(
    work_dir: Path,
    pdf: Path,
    report: dict,
    error: str,
    render_suspicious: bool,
) -> int:
    if render_suspicious:
        rendered = render_suspicious_pages(pdf, report["items"], work_dir / "rendered")
        for item in report["items"]:
            item["renderedPages"] = [
                str(rendered[page]) for page in item.get("pages", []) if page in rendered
            ]
        write_quarantine(work_dir, report["items"])
    print(f"validation failed: {error}")
    print(f"quarantined: {len(report['items'])}")
    return 1


def _input_failure_reason(error: ValueError | OSError) -> str:
    if isinstance(error, ManifestValidationError):
        return str(error)
    if isinstance(error, UnicodeError):
        return f"invalid text encoding in cache input: {error}"
    return f"invalid cache input: {error}"
