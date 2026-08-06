"""Extract detailed ECF record sections without promoting summaries or ambiguous tables."""

from __future__ import annotations

import re
from collections import Counter
from dataclasses import dataclass
from pathlib import Path
from typing import Iterable

from ecf_layout.fixups import apply


_DETAIL_START = re.compile(r"^#+\s+\**4\.5\. Leiaute dos Registros\**\s*$", re.IGNORECASE)
# Title-case `Registro` is the section anchor. Uppercase `REGISTRO` is repeated inside the detail box.
_RECORD = re.compile(r"^(?:#+\s+)?(?:\*\*)?Registro\s+([0-9A-Z]{4}):")
_LEVEL = re.compile(r"NívelHierárquico[–-](\d+)", re.IGNORECASE)
_OCCURRENCE = re.compile(r"Ocorrência[–-]([0-9]+(?::(?:[0-9]+|N))?)", re.IGNORECASE)
_FIELD_HEADER = re.compile(
    r"N\s*º.*Campo.*Descrição.*Tipo.*Tamanho.*Decimal.*(?:Valores\s+)?Válidos.*Obrigatório",
    re.IGNORECASE,
)
_RULES_BOUNDARY = re.compile(r"\*\*[IVX]+\s*[–-]\s*Regr", re.IGNORECASE)


@dataclass(frozen=True)
class RecordFragment:
    code: str
    block: str | None
    page_start: int
    page_end: int
    level: str | None
    occurrence: str | None
    fields: list[str]
    markdown: str


@dataclass(frozen=True)
class FragmentationResult:
    fragments: list[RecordFragment]
    errors: list[str]


def fragment_pages(pages: Iterable[tuple[int, str]]) -> list[RecordFragment]:
    """Return only detailed record fragments; ambiguous candidates are excluded."""
    return fragment_pages_with_errors(pages).fragments


def fragment_pages_with_errors(pages: Iterable[tuple[int, str]]) -> FragmentationResult:
    lines: list[tuple[int, str]] = []
    for page, markdown in pages:
        lines.extend((page, line) for line in apply(markdown).splitlines(keepends=True))

    detail_seen = False
    candidates: list[tuple[str, int, int]] = []
    for index, (_, line) in enumerate(lines):
        if _DETAIL_START.match(line.strip()):
            detail_seen = True
            continue
        if detail_seen and (match := _RECORD.match(line.strip())):
            code = match.group(1)
            candidates.append((code, index, len(lines)))
    candidates = [
        (code, start, candidates[position + 1][1] if position + 1 < len(candidates) else len(lines))
        for position, (code, start, _) in enumerate(candidates)
    ]

    fragments: list[RecordFragment] = []
    errors: list[str] = []
    code_counts = Counter(code for code, _, _ in candidates)
    duplicate_codes = {code for code, count in code_counts.items() if count > 1}
    reported_duplicates: set[str] = set()
    for code, start, end in candidates:
        if code in duplicate_codes:
            if code not in reported_duplicates:
                errors.append(f"record {code}: duplicate detailed sections")
                reported_duplicates.add(code)
            continue
        fragment_lines = lines[start:end]
        fields, field_error = _fields(fragment_lines)
        if field_error is not None:
            errors.append(f"record {code}: {field_error}")
            continue
        markdown = "".join(line for _, line in fragment_lines).rstrip("\r\n") + "\n"
        level, occurrence = _metadata(fragment_lines)
        if level is None or occurrence is None:
            errors.append(f"record {code}: missing detailed level or occurrence")
            continue
        fragments.append(
            RecordFragment(
                code=code,
                block=_block_before(lines, start),
                page_start=fragment_lines[0][0],
                page_end=fragment_lines[-1][0],
                level=level,
                occurrence=occurrence,
                fields=fields,
                markdown=markdown,
            )
        )
    return FragmentationResult(fragments, errors)


def write_fragments(fragments: Iterable[RecordFragment], directory: Path) -> None:
    fragments = list(fragments)
    directory.mkdir(parents=True, exist_ok=True)
    expected = {f"{fragment.code}.md" for fragment in fragments}
    for stale in directory.glob("*.md"):
        if stale.name not in expected:
            stale.unlink()
    for fragment in fragments:
        (directory / f"{fragment.code}.md").write_text(fragment.markdown, encoding="utf-8")


def _fields(lines: list[tuple[int, str]]) -> tuple[list[str], str | None]:
    fields: list[str] = []
    expected_ordinal = 1
    field_table_closed = False
    header_seen = False
    continuation_header = False
    for _, line in lines:
        if _is_field_header(line):
            if field_table_closed:
                return [], "competing field tables"
            continuation_header = header_seen and bool(fields)
            header_seen = True
            continue
        if not header_seen or field_table_closed:
            continue
        if re.fullmatch(r"\|(?:---\|)+\s*", line.rstrip("\r\n")):
            continue

        boundary = bool(_RULES_BOUNDARY.search(line))
        row = _field_row(line, expected_ordinal)
        if row is not None:
            ordinal, field = row
            if ordinal != expected_ordinal:
                if ordinal < expected_ordinal:
                    if continuation_header:
                        return [], "competing field tables"
                    field_table_closed = True
                    continue
                if _is_structural_field_name(field):
                    return [], f"non-contiguous field table at {ordinal}"
                continue
            fields.append(field)
            expected_ordinal += 1
            continuation_header = False
            if boundary:
                field_table_closed = True
        elif boundary:
            field_table_closed = True
        else:
            first_cell = _first_cell(line)
            if first_cell and not first_cell[0].isdigit():
                field_table_closed = True

    if not header_seen:
        return [], "0 candidate field tables"
    if not fields:
        return [], "field table has no fields"
    return fields, None


def _is_field_header(line: str) -> bool:
    if not line.startswith("|"):
        return False
    plain = re.sub(r"<[^>]+>", " ", line).replace("**", "").replace("|", " ")
    return bool(_FIELD_HEADER.search(plain))


def _field_row(line: str, expected_ordinal: int | None = None) -> tuple[int, str] | None:
    if not line.startswith("|"):
        return None
    cells = line.rstrip("\r\n").split("|")
    if len(cells) < 3:
        return None
    first = _cell_parts(cells[1])
    if not first or not first[0].isdigit():
        return None
    ordinal = int(first[0])
    if len(first) > 1:
        field_parts = first[1:]
    else:
        second = _cell_parts(cells[2])
        if (
            expected_ordinal is not None
            and len(second) > 1
            and second[0].isdigit()
            and int(first[0] + second[0]) == expected_ordinal
        ):
            ordinal = expected_ordinal
            field_parts = second[1:]
        else:
            field_parts = second
    field = _leading_structural_field_name(field_parts)
    return (ordinal, field) if _is_structural_field_name(field) else None


def _leading_structural_field_name(parts: list[str]) -> str:
    field_parts: list[str] = []
    for part in parts:
        if not _is_structural_field_name(part):
            break
        field_parts.append(part)
    return "".join(field_parts).strip()


def _is_structural_field_name(value: str) -> bool:
    if re.search(r"\s", value) and "_" not in value and "/" not in value:
        return False
    compact = re.sub(r"\s+", "", value)
    if not compact or not compact[0].isalpha() or compact.upper() != compact:
        return False
    return all(character.isalnum() or character in "_/-" for character in compact)


def _cell_parts(cell: str) -> list[str]:
    parts = re.split(r"<br\s*/?>", cell, flags=re.IGNORECASE)
    return [re.sub(r"<[^>]+>", "", part).replace("**", "").strip() for part in parts if part.strip()]


def _first_cell(line: str) -> str:
    cells = line.rstrip("\r\n").split("|")
    return re.sub(r"<[^>]+>", "", cells[1]).replace("**", "").strip() if len(cells) > 2 else ""


def _metadata(lines: list[tuple[int, str]]) -> tuple[str | None, str | None]:
    metadata_lines: list[str] = []
    for _, line in lines:
        if _is_field_header(line):
            break
        metadata_lines.append(line)
    plain = re.sub(r"<[^>]+>", "", "".join(metadata_lines))
    compact = re.sub(r"\s+", "", plain.replace("**", "").replace("|", "").replace("#", ""))
    level = _LEVEL.search(compact)
    occurrence = _OCCURRENCE.search(compact)
    occurrence_value = occurrence.group(1) if occurrence else _fragmented_occurrence(metadata_lines)
    return (level.group(1) if level else None, occurrence_value)


def _fragmented_occurrence(lines: list[str]) -> str | None:
    fragments: list[str] = []
    for line in lines:
        for cell in line.split("|"):
            for fragment in re.split(r"<br\s*/?>", cell, flags=re.IGNORECASE):
                plain = re.sub(r"<[^>]+>", "", fragment).replace("**", "").strip()
                if plain:
                    fragments.append(plain)
    prefix_positions = [
        index
        for index, fragment in enumerate(fragments)
        if re.fullmatch(r"Oco(?:r{0,2})?", fragment, re.IGNORECASE)
    ]
    for prefix in prefix_positions:
        for fragment in fragments[prefix + 1 : prefix + 4]:
            suffix = re.match(
                r"(?:r{0,2})?ência\s*[–-]\s*([0-9]+(?::(?:[0-9]+|N))?)",
                fragment,
                re.IGNORECASE,
            )
            if suffix:
                return suffix.group(1)
    return None


def _block_before(lines: list[tuple[int, str]], start: int) -> str | None:
    for _, line in reversed(lines[:start]):
        match = re.match(r"^#+\s+\**Bloco\s+([0-9A-Z]+):", line.strip(), re.IGNORECASE)
        if match:
            return match.group(1)
    return None
