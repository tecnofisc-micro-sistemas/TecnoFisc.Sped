"""Small, deterministic repairs for Markdown emitted from the ECF manual."""

from __future__ import annotations

import re


_FOOTER = re.compile(r"^(?:P[aá]gina\s+)?\d+\s+de\s+\d+$", re.IGNORECASE)
_FIELD_HEADER = re.compile(
    r"^\|\s*Nº.*Campo.*Descrição.*Tipo.*Tamanho.*Decimal.*(?:Valores\s+)?Válidos.*Obrigatório.*\|$",
    re.IGNORECASE,
)
_UPPERCASE_FIELD = re.compile(r"^(?:<br>)*[A-Z0-9_]+(?:<br>[A-Z0-9_]+)+(?:<br>)*$")
_FIELD_TOKEN = re.compile(r"^[A-Z0-9_]+$")

_CANONICAL_FIELD_HEADER = (
    "|**Nº**|**Campo**|**Descrição**|**Tipo**|**Tamanho**|**Decimal**|"
    "**Valores Válidos**|**Obrigatório**|"
)
_CANONICAL_SEPARATOR = "|" + "---|" * 8


def apply(markdown: str) -> str:
    """Remove full-line ECF page chrome and repair only field-table cells."""
    lines = [line for line in markdown.splitlines(keepends=True) if not _is_page_chrome(line)]
    in_field_table = False
    repaired: list[str] = []
    for line in lines:
        body, ending = _split_ending(line)
        if _is_field_header(body):
            repaired.append(_CANONICAL_FIELD_HEADER + ending)
            in_field_table = True
            continue
        if not body.strip():
            repaired.append(line)
            continue
        if in_field_table and re.fullmatch(r"\|(?:---\|)+", body):
            repaired.append(_CANONICAL_SEPARATOR + ending)
            continue
        if not body.startswith("|"):
            in_field_table = False
            repaired.append(line)
            continue
        repaired.append(_repair_field_cell(body) + ending if in_field_table else line)
    return "".join(repaired)


def _is_page_chrome(line: str) -> bool:
    normalized = _plain(line)
    return (
        normalized.startswith("Anexo ao Ato Declaratório Executivo Cofis")
        or normalized.startswith("Atualização:")
        or normalized.startswith("RFB/Subsecretaria de Fiscalização/")
        or bool(_FOOTER.fullmatch(normalized))
    )


def _is_field_header(line: str) -> bool:
    normalized = re.sub(r"<br\s*/?>", " ", line, flags=re.IGNORECASE)
    return bool(_FIELD_HEADER.fullmatch(_plain(normalized)))


def _repair_field_cell(line: str) -> str:
    cells = line.split("|")
    # A normal field table has a leading empty cell, ordinal at index 1 and Campo at 2.
    if len(cells) > 3 and re.fullmatch(r"\s*(?:<br>)*\**\d+\**(?:<br>)*\s*", cells[1]):
        candidate = cells[2]
        if _UPPERCASE_FIELD.fullmatch(candidate):
            cells[2] = candidate.replace("<br>", "")
        suffix = re.match(r"^([A-Z0-9_])<br>(.*)$", cells[3], re.DOTALL)
        if suffix and _FIELD_TOKEN.fullmatch(cells[2]):
            cells[2] += suffix.group(1)
            cells[3] = suffix.group(2)
        return "|".join(cells)
    # A merged Nº/Campo header carries both values in the first data cell.
    if len(cells) > 2 and re.fullmatch(r"\s*\**\d+\**<br>[A-Z0-9_]+(?:<br>[A-Z0-9_]+)*\s*", cells[1]):
        ordinal, field = cells[1].split("<br>", maxsplit=1)
        cells[1] = ordinal
        cells.insert(2, field.replace("<br>", ""))
    return "|".join(cells)


def _plain(line: str) -> str:
    plain = re.sub(r"<[^>]+>", "", line)
    return plain.strip().lstrip("#").replace("**", "").strip()


def _split_ending(line: str) -> tuple[str, str]:
    if line.endswith("\r\n"):
        return line[:-2], "\r\n"
    if line.endswith("\n"):
        return line[:-1], "\n"
    return line, ""
