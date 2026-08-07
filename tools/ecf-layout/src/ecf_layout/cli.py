"""Command-line entry point for resumable ECF manual preparation."""

from __future__ import annotations

import argparse
import json
import os
import re
import tempfile
from dataclasses import dataclass
from pathlib import Path
from typing import Callable, Iterable

from ecf_layout.anonymize import AnonymizationError, anonymize_file
from ecf_layout.cache import CacheKey, converter_fingerprint, sha256_file
from ecf_layout.artifacts import (
    ArtifactPromotionError,
    build_artifacts,
    invalidate_generation,
    promote_artifacts,
)
from ecf_layout.converter import EmptyMarkdownError, convert_page
from ecf_layout.field_names import aplicar, nomes_por_ordem
from ecf_layout.fragmenter import fragment_pages_with_errors, write_fragments
from ecf_layout.manifest import (
    ManifestValidationError,
    records_from_work_dir,
    validate_and_promote,
    write_quarantine,
)
from ecf_layout.render import render_suspicious_pages
from ecf_layout.scaffold import ScaffoldError, scaffold


Converter = Callable[[Path, int], str]
EXPECTED_FRAGMENT_COUNT = 180
_ARQUIVO_REGISTRO = re.compile(r"^Registro(?P<codigo>[0-9A-Z]+)\.cs$")


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


def _parse_codes(value: str) -> tuple[str, ...]:
    codes = tuple(value.split(","))
    if not codes or any(not code for code in codes):
        raise argparse.ArgumentTypeError("codes must be a non-empty comma-separated list")
    return codes


class FieldNamesError(Exception):
    """Sinaliza inconsistência entre o manifesto e os registros gerados."""


def apply_field_names(manifest: Path, registros_root: Path) -> int:
    """Reescreve todos os registros ECF com o alias `Nome` normativo do manifesto.

    Casa cada arquivo `RegistroXXXX.cs` pelo código do registro no nome do
    arquivo e aplica `aplicar` usando os nomes de campo do manifesto. Retorna
    a quantidade de arquivos efetivamente alterados.
    """
    nomes = nomes_por_ordem(manifest)
    arquivos = sorted(registros_root.rglob("*.cs"))
    if not arquivos:
        raise FieldNamesError(f"nenhum arquivo .cs encontrado em {registros_root}")

    alterados = 0
    for arquivo in arquivos:
        correspondencia = _ARQUIVO_REGISTRO.match(arquivo.name)
        if correspondencia is None:
            raise FieldNamesError(
                f"arquivo {arquivo} não segue o padrão RegistroXXXX.cs"
            )
        codigo = correspondencia.group("codigo")
        nomes_do_registro = nomes.get(codigo)
        if nomes_do_registro is None:
            raise FieldNamesError(
                f"registro {codigo} ({arquivo}) não encontrado no manifesto"
            )

        with open(arquivo, "r", encoding="utf-8", newline="") as stream:
            original = stream.read()
        reescrito = aplicar(original, nomes_do_registro)
        if reescrito != original:
            with open(arquivo, "w", encoding="utf-8", newline="") as stream:
                stream.write(reescrito)
            alterados += 1

    return alterados


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
    scaffold_parser = subcommands.add_parser("scaffold")
    scaffold_parser.add_argument("--manifest", type=Path, required=True)
    scaffold_parser.add_argument("--output-root", type=Path, default=Path("."))
    scaffold_parser.add_argument("--codes", type=_parse_codes, required=True)
    modes = scaffold_parser.add_mutually_exclusive_group(required=True)
    modes.add_argument("--tests-only", action="store_true")
    modes.add_argument("--source", action="store_true")
    scaffold_parser.add_argument("--force", action="store_true")
    anonymize_parser = subcommands.add_parser("anonymize")
    anonymize_parser.add_argument(
        "--source",
        type=Path,
        required=True,
        help="authorized private input; requires a sibling SOURCE.sha256 sidecar",
    )
    anonymize_parser.add_argument("--output", type=Path, required=True)
    anonymize_parser.add_argument("--fixture-id", required=True)
    anonymize_parser.add_argument("--denylist", type=Path, required=True)
    field_names_parser = subcommands.add_parser("field-names")
    field_names_parser.add_argument(
        "--manifest",
        type=Path,
        default=Path("sped/ecf/layout-12-manifest.json"),
    )
    field_names_parser.add_argument(
        "--registros-root",
        type=Path,
        default=Path("src/TecnoFisc.Sped.Ecf/Registros"),
    )
    args = parser.parse_args(argv)

    if args.command == "validate":
        return _validate(args.work_dir, args.pdf, render_suspicious=args.render_suspicious)
    if args.command == "build-artifacts":
        try:
            records = records_from_work_dir(args.work_dir, args.pdf)
        except (ManifestValidationError, OSError, UnicodeError) as error:
            try:
                invalidate_generation(args.work_dir, str(error))
            except ArtifactPromotionError:
                pass
            print(f"artifact build failed: {error}")
            return 1
        try:
            build_artifacts(
                records,
                args.work_dir,
                args.manifest_out,
                args.tracker_out,
                pdf=args.pdf,
            )
        except (
            ArtifactPromotionError,
            ManifestValidationError,
            OSError,
            UnicodeError,
        ) as error:
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
    if args.command == "scaffold":
        try:
            generated = scaffold(
                args.manifest,
                args.output_root,
                codes=args.codes,
                tests_only=args.tests_only,
                source=args.source,
                force=args.force,
            )
        except (ScaffoldError, OSError, UnicodeError) as error:
            print(f"scaffold failed: {error}")
            return 1
        print(f"scaffolded: {len(generated)} files")
        return 0
    if args.command == "anonymize":
        try:
            result = anonymize_file(
                args.source,
                args.output,
                fixture_id=args.fixture_id,
                denylist_path=args.denylist,
                manifest_path=Path("sped/ecf/layout-12-manifest.json"),
                private_root=Path(".local/ecf-layout/private"),
            )
        except AnonymizationError as error:
            print(f"anonymization failed: {error}")
            return 1
        print(result.log_line)
        return 0
    if args.command == "field-names":
        try:
            changed = apply_field_names(args.manifest, args.registros_root)
        except FieldNamesError as error:
            print(f"field-names failed: {error}")
            return 1
        print(f"field-names: {changed} arquivos atualizados")
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
