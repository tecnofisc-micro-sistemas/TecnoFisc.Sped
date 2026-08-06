"""Deterministic, conservative C# drafts from the reviewed ECF manifest."""

from __future__ import annotations

import html
import json
import os
import re
import tempfile
import unicodedata
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable


_CODE_PATTERN = re.compile(r"[0-9A-Z]{4}\Z")
_CSHARP_IDENTIFIER_PATTERN = re.compile(r"[A-Za-z_][A-Za-z0-9_]*\Z")


class ScaffoldError(ValueError):
    """Raised when scaffold input cannot be consumed without ambiguity."""


@dataclass(frozen=True)
class GeneratedFile:
    path: Path
    content: str


def scaffold(
    manifest_path: Path,
    output_root: Path,
    *,
    codes: Iterable[str],
    tests_only: bool = False,
    source: bool = False,
    force: bool = False,
) -> tuple[Path, ...]:
    """Generate one deterministic file per requested reviewed manifest record."""

    if tests_only == source:
        raise ScaffoldError("select exactly one scaffold mode: tests-only or source")

    manifest_path = Path(manifest_path)
    output_root = Path(output_root)
    if not manifest_path.is_file():
        raise ScaffoldError(f"manifest path is not a file: {manifest_path}")
    if output_root.exists() and not output_root.is_dir():
        raise ScaffoldError(f"output root is not a directory: {output_root}")

    records = _load_manifest(manifest_path)
    requested = _requested_codes(codes)
    by_code = {record["code"]: record for record in records}
    missing = [code for code in requested if code not in by_code]
    if missing:
        raise ScaffoldError(f"requested codes not found in manifest: {', '.join(missing)}")

    selected = [record for record in records if record["code"] in requested]
    unreviewed = [record["code"] for record in selected if record["reviewed"] is not True]
    if unreviewed:
        raise ScaffoldError(f"requested records are not reviewed: {', '.join(unreviewed)}")

    generated = [
        _generate_test(record, output_root) if tests_only else _generate_source(record, output_root)
        for record in selected
    ]
    _validate_destinations(generated, manifest_path, output_root, force=force)
    for item in generated:
        _write_atomically(item.path, item.content)
    return tuple(item.path for item in generated)


def _load_manifest(path: Path) -> list[dict]:
    try:
        payload = json.loads(path.read_text(encoding="utf-8"))
    except (OSError, UnicodeError, json.JSONDecodeError) as error:
        raise ScaffoldError(f"cannot read manifest JSON: {error}") from error
    if not isinstance(payload, list):
        raise ScaffoldError("manifest must be a JSON array")

    records: list[dict] = []
    seen: set[str] = set()
    for index, value in enumerate(payload, start=1):
        record = _validate_record(value, index)
        code = record["code"]
        if code in seen:
            raise ScaffoldError(f"duplicate manifest code: {code}")
        seen.add(code)
        records.append(record)
    return records


def _validate_record(value: object, index: int) -> dict:
    if not isinstance(value, dict):
        raise ScaffoldError(f"manifest record {index} must be an object")
    code = value.get("code")
    block = value.get("block")
    if not isinstance(code, str) or _CODE_PATTERN.fullmatch(code) is None:
        raise ScaffoldError(f"manifest record {index} has an invalid code")
    if not isinstance(block, str) or block != code[0]:
        raise ScaffoldError(f"manifest record {code} has an invalid block")
    if not isinstance(value.get("title"), str) or not value["title"].strip():
        raise ScaffoldError(f"manifest record {code} has an invalid title")
    try:
        level = int(value.get("level"))
    except (TypeError, ValueError) as error:
        raise ScaffoldError(f"manifest record {code} has an invalid level") from error
    if level < 0:
        raise ScaffoldError(f"manifest record {code} has an invalid level")
    if not isinstance(value.get("reviewed"), bool):
        raise ScaffoldError(f"manifest record {code} has an invalid reviewed flag")
    fields = value.get("fields")
    if not isinstance(fields, list) or not fields:
        raise ScaffoldError(f"manifest record {code} has no fields")
    normalized_fields = [_validate_field(field, code, position) for position, field in enumerate(fields, start=1)]
    if [field["number"] for field in normalized_fields] != list(range(1, len(fields) + 1)):
        raise ScaffoldError(f"manifest record {code} has non-contiguous field numbers")
    if normalized_fields[0]["name"] != "REG":
        raise ScaffoldError(f"manifest record {code} must start with REG")
    property_names = [_property_name(field["name"]) for field in normalized_fields[1:]]
    if len(property_names) != len(set(property_names)):
        raise ScaffoldError(f"manifest record {code} has colliding normalized field names")
    return {**value, "level": level, "fields": normalized_fields}


def _validate_field(value: object, code: str, position: int) -> dict:
    if not isinstance(value, dict):
        raise ScaffoldError(f"manifest record {code} field {position} must be an object")
    if value.get("number") != position:
        raise ScaffoldError(f"manifest record {code} has non-contiguous field numbers")
    name = value.get("name")
    if not isinstance(name, str) or not name.strip():
        raise ScaffoldError(f"manifest record {code} field {position} has an invalid name")
    try:
        _property_name(name)
    except ScaffoldError as error:
        raise ScaffoldError(f"manifest record {code} field {position} has an invalid name") from error
    description = value.get("description")
    if not isinstance(description, str) or not description.strip():
        raise ScaffoldError(f"manifest record {code} field {position} has an invalid description")
    for key in ("type", "size", "required"):
        if not isinstance(value.get(key), str) or not value[key].strip():
            raise ScaffoldError(f"manifest record {code} field {position} has an invalid {key}")
    for key in ("decimals", "validValues"):
        if not isinstance(value.get(key), str):
            raise ScaffoldError(f"manifest record {code} field {position} has an invalid {key}")
    try:
        _required_value(value["required"])
    except ScaffoldError as error:
        raise ScaffoldError(
            f"manifest record {code} field {position} has an invalid required flag"
        ) from error
    return value


def _requested_codes(codes: Iterable[str]) -> set[str]:
    values = list(codes)
    if not values:
        raise ScaffoldError("at least one code is required")
    if any(not isinstance(code, str) or _CODE_PATTERN.fullmatch(code) is None for code in values):
        raise ScaffoldError("requested code is invalid")
    if len(values) != len(set(values)):
        raise ScaffoldError("requested codes must be unique")
    return set(values)


def _generate_test(record: dict, output_root: Path) -> GeneratedFile:
    code = record["code"]
    block = record["block"]
    content = f'''using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.Bloco{block};

public sealed class Registro{code}Tests
{{
    [Fact]
    public void Catalogo_ImplementaRegistro{code}()
    {{
        AssertRegistroEcf.CodesAreImplemented("{code}");
    }}
}}
'''
    relative = Path("tests") / "TecnoFisc.Sped.Ecf.Tests" / "Registros" / f"Bloco{block}" / f"Registro{code}Tests.cs"
    return GeneratedFile(output_root / relative, content)


def _generate_source(record: dict, output_root: Path) -> GeneratedFile:
    code = record["code"]
    block = record["block"]
    title = _xml_text(record["title"])
    lines = [
        "using TecnoFisc.Sped.Txt.Engine.Abstracoes;",
        "using TecnoFisc.Sped.Txt.Engine.Atributos;",
        "",
        f"namespace TecnoFisc.Sped.Ecf.Registros.Bloco{block};",
        "",
        "/// <summary>",
        f"/// Draft do Registro {code} — {title}. Revise os tipos antes de integrar.",
        "/// </summary>",
        f'[RegistroSped(Codigo = "{code}", Nivel = {record["level"]}, Bloco = "{block}")]',
        f"public sealed partial class Registro{code} : RegistroSped",
        "{",
        "    /// <inheritdoc />",
        f'    public override string Codigo => "{code}";',
    ]
    for field in record["fields"][1:]:
        lines.extend(
            [
                "",
                f'    /// <summary>{_xml_text(field["description"])}</summary>',
                f"    {_field_attribute(field)}",
                f'    public string? {_property_name(field["name"])} {{ get; set; }}',
            ]
        )
    lines.extend(["}", ""])
    relative = Path("src") / "TecnoFisc.Sped.Ecf" / "Registros" / f"Bloco{block}" / f"Registro{code}.cs"
    return GeneratedFile(output_root / relative, "\n".join(lines))


def _property_name(name: str) -> str:
    ascii_name = unicodedata.normalize("NFKD", name).encode("ascii", "ignore").decode("ascii")
    parts = re.findall(r"[A-Za-z0-9]+", ascii_name)
    candidate = "".join(part[0].upper() + part[1:].lower() for part in parts)
    if _CSHARP_IDENTIFIER_PATTERN.fullmatch(candidate) is None:
        raise ScaffoldError(f"field name cannot be normalized to a C# identifier: {name}")
    return candidate


def _xml_text(value: str) -> str:
    return html.escape(" ".join(value.split()), quote=False)


def _field_attribute(field: dict) -> str:
    arguments = [f'Ordem = {field["number"]}']
    if field["size"].isdigit():
        arguments.append(f'Tamanho = {int(field["size"])}')
    if _required_value(field["required"]):
        arguments.append("Obrigatorio = true")
    return f"[CampoSped({', '.join(arguments)})]"


def _required_value(value: str) -> bool:
    ascii_value = unicodedata.normalize("NFKD", value).encode("ascii", "ignore").decode("ascii")
    marker = "".join(re.findall(r"[A-Za-z]+", ascii_value)).lower()
    if marker in {"s", "sim"}:
        return True
    if marker in {"", "n", "nao"}:
        return False
    raise ScaffoldError(f"ambiguous required marker: {value}")


def _validate_destinations(
    generated: list[GeneratedFile],
    manifest_path: Path,
    output_root: Path,
    *,
    force: bool,
) -> None:
    root = output_root.resolve()
    manifest = manifest_path.resolve()
    for item in generated:
        destination = item.path.resolve()
        if not destination.is_relative_to(root):
            raise ScaffoldError(f"generated path escapes output root: {item.path}")
        if destination == manifest:
            raise ScaffoldError("generated path collides with manifest path")
        _validate_parent_chain(item.path.parent, output_root)
        if item.path.exists() and not item.path.is_file():
            raise ScaffoldError(f"generated path is not a file: {item.path}")
        if item.path.exists() and not force:
            raise ScaffoldError(f"refusing to overwrite {item.path}; pass --force explicitly")


def _validate_parent_chain(parent: Path, output_root: Path) -> None:
    current = parent
    root = output_root
    while True:
        if current.exists():
            if not current.is_dir():
                raise ScaffoldError(f"generated parent path is not a directory: {current}")
            return
        if current == root or current == current.parent:
            return
        current = current.parent


def _write_atomically(path: Path, content: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    descriptor, temporary_name = tempfile.mkstemp(prefix=f".{path.name}.", suffix=".tmp", dir=path.parent)
    temporary = Path(temporary_name)
    try:
        with os.fdopen(descriptor, "w", encoding="utf-8", newline="\n") as stream:
            stream.write(content)
            stream.flush()
            os.fsync(stream.fileno())
        temporary.replace(path)
    except BaseException:
        temporary.unlink(missing_ok=True)
        raise
