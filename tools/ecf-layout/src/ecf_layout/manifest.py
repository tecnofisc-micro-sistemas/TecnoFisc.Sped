"""Fail-closed validation and atomic promotion for the ECF layout manifest."""

from __future__ import annotations

import json
import os
import re
import tempfile
from collections import Counter
from pathlib import Path
from ecf_layout.cache import sha256_file
from ecf_layout.fragmenter import RecordFragment, fragment_pages_with_errors


CANONICAL_CODES_BY_BLOCK = {
    "0": ("0000", "0001", "0010", "0020", "0021", "0030", "0035", "0930", "0990"),
    "C": ("C001", "C040", "C050", "C051", "C053", "C100", "C150", "C155", "C157", "C350", "C355", "C990"),
    "E": ("E001", "E010", "E015", "E020", "E030", "E155", "E355", "E990"),
    "J": ("J001", "J050", "J051", "J053", "J100", "J990"),
    "K": ("K001", "K030", "K155", "K156", "K355", "K356", "K915", "K935", "K990"),
    "L": ("L001", "L030", "L100", "L200", "L210", "L300", "L990"),
    "M": ("M001", "M010", "M030", "M300", "M305", "M310", "M312", "M315", "M350", "M355", "M360", "M362", "M365", "M410", "M415", "M500", "M510", "M990"),
    "N": ("N001", "N030", "N500", "N600", "N605", "N610", "N615", "N620", "N630", "N650", "N660", "N670", "N990"),
    "P": ("P001", "P030", "P100", "P130", "P150", "P200", "P230", "P300", "P400", "P500", "P990"),
    "Q": ("Q001", "Q100", "Q990"),
    "T": ("T001", "T030", "T120", "T150", "T170", "T181", "T990"),
    "U": ("U001", "U030", "U100", "U150", "U180", "U182", "U990"),
    "V": ("V001", "V010", "V020", "V030", "V100", "V990"),
    "W": ("W001", "W100", "W200", "W250", "W300", "W990"),
    "X": ("X001", "X280", "X292", "X340", "X350", "X351", "X352", "X353", "X354", "X355", "X356", "X357", "X360", "X365", "X366", "X370", "X371", "X375", "X390", "X400", "X410", "X420", "X430", "X450", "X451", "X460", "X470", "X480", "X485", "X490", "X500", "X510", "X990"),
    "Y": ("Y001", "Y520", "Y570", "Y590", "Y600", "Y612", "Y620", "Y630", "Y640", "Y650", "Y660", "Y672", "Y680", "Y681", "Y682", "Y720", "Y730", "Y750", "Y800", "Y990"),
    "9": ("9001", "9100", "9900", "9990", "9999"),
}
CANONICAL_BLOCKS = tuple(CANONICAL_CODES_BY_BLOCK)
EXPECTED_CODES = tuple(code for codes in CANONICAL_CODES_BY_BLOCK.values() for code in codes)
_BLOCK_BY_CODE = {code: block for block, codes in CANONICAL_CODES_BY_BLOCK.items() for code in codes}
_OCCURRENCE = re.compile(r"^[01]:(?:[1-9][0-9]*|N)$")
_RECORD_KEYS = (
    "code",
    "block",
    "title",
    "pageStart",
    "pageEnd",
    "level",
    "occurrence",
    "fields",
    "reviewed",
)
_FIELD_KEYS = (
    "number",
    "name",
    "description",
    "type",
    "size",
    "decimals",
    "required",
    "validValues",
)
_REQUIRED_MARKERS = ("Sim", "sim", "S", "Não", "N", "-", "OC")
_REQUIRED_MARKER_SET = frozenset(_REQUIRED_MARKERS)


class ManifestValidationError(ValueError):
    """Raised after quarantine is recorded for any invalid candidate."""


def block_for_code(code: str) -> str:
    return _BLOCK_BY_CODE[code]


def validate_and_promote(
    records: object, work_dir: Path, *, promote_to: Path | None = None
) -> Path:
    """Validate, write a candidate, and optionally atomically promote reviewed data."""
    if not isinstance(records, list):
        write_quarantine(
            work_dir,
            [{"code": None, "reasons": ["manifest root must be an array"], "pages": []}],
        )
        raise ManifestValidationError("manifest root must be an array")
    items = _quarantine_items(records, require_reviewed=promote_to is not None)
    work_dir = Path(work_dir)
    quarantine_path = work_dir / "quarantine.json"
    _write_json_atomically(quarantine_path, {"items": items})
    if items:
        raise ManifestValidationError(f"manifest validation quarantined {len(items)} item(s)")

    sanitized = [_sanitize_record(record) for record in records]
    candidate = work_dir / "candidate" / "layout-12-manifest.json"
    _write_json_atomically(candidate, sanitized)
    if promote_to is None:
        return candidate

    promote_to = Path(promote_to)
    _write_bytes_atomically(promote_to, candidate.read_bytes())
    return promote_to


def records_from_work_dir(work_dir: Path, pdf: Path) -> list[dict]:
    """Rebuild records from the complete raw cache matching the promoted fragments."""
    work_dir = Path(work_dir)
    pdf = Path(pdf)
    if not pdf.is_file():
        raise ManifestValidationError(f"normative PDF not found: {pdf}")
    fragment_dir = work_dir / "fragments"
    fragment_files = {path.stem: path for path in fragment_dir.glob("*.md")}
    if set(fragment_files) != set(EXPECTED_CODES):
        raise ManifestValidationError("fragment corpus does not contain the exact 180 canonical codes")

    pdf_cache = work_dir / "cache" / sha256_file(pdf)
    cache_directories = sorted(path for path in pdf_cache.iterdir() if path.is_dir()) if pdf_cache.is_dir() else []
    for cache_directory in cache_directories:
        page_files = sorted(cache_directory.glob("page-*.md"))
        if not page_files:
            continue
        page_numbers = [int(path.stem.rsplit("-", 1)[1]) for path in page_files]
        if page_numbers != list(range(1, page_numbers[-1] + 1)):
            continue
        pages = [
            (page, path.read_text(encoding="utf-8"))
            for page, path in zip(page_numbers, page_files, strict=True)
        ]
        result = fragment_pages_with_errors(pages)
        if result.errors or tuple(fragment.code for fragment in result.fragments) != EXPECTED_CODES:
            continue
        if any(
            fragment.markdown != fragment_files[fragment.code].read_text(encoding="utf-8")
            for fragment in result.fragments
        ):
            continue
        return [record_from_fragment(fragment) for fragment in result.fragments]
    raise ManifestValidationError("no complete raw cache matches the 180-fragment corpus")


def record_from_fragment(fragment: RecordFragment) -> dict:
    """Convert one bounded Markdown fragment into structural manifest data only."""
    title_match = re.search(
        rf"^(?:#+\s+)?(?:\*\*)?Registro\s+{re.escape(fragment.code)}:\s*(.+?)(?:\*\*)?\s*$",
        fragment.markdown,
        flags=re.MULTILINE,
    )
    title = (
        _plain(re.split(r"<br\s*/?>", title_match.group(1), maxsplit=1, flags=re.IGNORECASE)[0])
        if title_match
        else ""
    )
    parsed_fields: dict[int, dict] = {}
    ambiguities: list[str] = []
    current_number: int | None = None
    for line in fragment.markdown.splitlines():
        if not line.startswith("|"):
            if current_number is not None and re.search(r"Regras? de Validação", _plain(line), re.IGNORECASE):
                break
            continue
        cells = line.rstrip().strip("|").split("|")
        first_parts = _parts(cells[0]) if cells else []
        number_match = re.fullmatch(r"(\d+)", first_parts[0]) if first_parts else None
        if number_match:
            number = int(number_match.group(1))
            if not 1 <= number <= len(fragment.fields) or number in parsed_fields:
                continue
            expected_name = fragment.fields[number - 1]
            field, name_matches = _parse_field_cells(number, expected_name, cells, first_parts)
            parsed_fields[number] = field
            if not name_matches:
                ambiguities.append(
                    f"field {number} name {field['name']!r} does not match fragment structure {expected_name!r}"
                )
            current_number = number
            continue
        if current_number is not None and re.search(r"Regras? de Validação", _plain(line), re.IGNORECASE):
            break
        if current_number is not None and len(cells) >= 3 and not _plain(cells[0]) and not _plain(cells[1]):
            continuation = _plain(cells[2])
            if continuation:
                prior = parsed_fields[current_number]["description"]
                parsed_fields[current_number]["description"] = " ".join(part for part in (prior, continuation) if part)

    fields = [parsed_fields[number] for number in sorted(parsed_fields)]
    record = {
        "code": fragment.code,
        "block": block_for_code(fragment.code),
        "title": title,
        "pageStart": fragment.page_start,
        "pageEnd": fragment.page_end,
        "level": fragment.level,
        "occurrence": fragment.occurrence,
        "fields": fields,
        "reviewed": False,
    }
    if len(fields) != len(fragment.fields):
        ambiguities.append(f"expected {len(fragment.fields)} structured fields, parsed {len(fields)}")
    if ambiguities:
        record["ambiguities"] = ambiguities
    return record


def write_quarantine(work_dir: Path, items: list[dict]) -> Path:
    path = Path(work_dir) / "quarantine.json"
    _write_json_atomically(path, {"items": items})
    return path


def _quarantine_items(records: list[dict], *, require_reviewed: bool) -> list[dict]:
    items: list[dict] = []
    reasons_by_record: list[list[str]] = [[] for _record in records]
    record_keys = set(_RECORD_KEYS)
    field_keys = set(_FIELD_KEYS)
    for index, record in enumerate(records):
        reasons = reasons_by_record[index]
        if not isinstance(record, dict):
            reasons.append("record must be an object")
            continue
        actual_keys = set(record)
        missing_keys = sorted(record_keys - actual_keys)
        unknown_keys = sorted(actual_keys - record_keys - {"ambiguities"}, key=str)
        if missing_keys:
            reasons.append(f"missing record keys: {', '.join(missing_keys)}")
        if unknown_keys:
            reasons.append(f"unknown record keys: {', '.join(map(str, unknown_keys))}")
        for key in ("code", "block", "title", "level", "occurrence"):
            if key in record and not isinstance(record[key], str):
                reasons.append(f"record {key} must be a string")
        if isinstance(record.get("title"), str) and not record["title"].strip():
            reasons.append("record title must not be empty")
        for key in ("pageStart", "pageEnd"):
            if key in record and (not isinstance(record[key], int) or isinstance(record[key], bool)):
                reasons.append(f"record {key} must be an integer")
        if "reviewed" in record and not isinstance(record["reviewed"], bool):
            reasons.append("record reviewed must be a boolean")
        ambiguities = record.get("ambiguities", [])
        if not isinstance(ambiguities, list) or any(not isinstance(reason, str) for reason in ambiguities):
            reasons.append("record ambiguities must be a list of strings")
        fields = record.get("fields")
        if "fields" in record and not isinstance(fields, list):
            reasons.append("record fields must be an array")
        if not isinstance(fields, list):
            continue
        for number, field in enumerate(fields, start=1):
            if not isinstance(field, dict):
                reasons.append(f"field {number} must be an object")
                continue
            actual_field_keys = set(field)
            missing_field_keys = sorted(field_keys - actual_field_keys)
            unknown_field_keys = sorted(actual_field_keys - field_keys, key=str)
            if missing_field_keys:
                reasons.append(f"field {number} missing keys: {', '.join(missing_field_keys)}")
            if unknown_field_keys:
                reasons.append(
                    f"field {number} unknown keys: {', '.join(map(str, unknown_field_keys))}"
                )
            if "number" in field and (
                not isinstance(field["number"], int) or isinstance(field["number"], bool)
            ):
                reasons.append(f"field {number} number must be an integer")
            for key in _FIELD_KEYS[1:]:
                if key in field and not isinstance(field[key], str):
                    reasons.append(f"field {number} {key} must be a string")
            required = field.get("required")
            if isinstance(required, str) and required not in _REQUIRED_MARKER_SET:
                reasons.append(
                    f"field {number} required must be one of: {', '.join(_REQUIRED_MARKERS)}"
                )

    codes = [record.get("code") for record in records if isinstance(record, dict) and isinstance(record.get("code"), str)]
    counts = Counter(codes)
    structural_reasons: list[str] = []
    if len(records) != len(EXPECTED_CODES):
        structural_reasons.append(f"expected 180 records, found {len(records)}")
    duplicates = sorted(str(code) for code, count in counts.items() if count > 1)
    missing = sorted(set(EXPECTED_CODES) - set(codes))
    unknown = sorted(set(codes) - set(EXPECTED_CODES))
    if duplicates:
        structural_reasons.append(f"duplicate record codes: {', '.join(duplicates)}")
    if missing:
        structural_reasons.append(f"missing record codes: {', '.join(missing)}")
    if unknown:
        structural_reasons.append(f"unknown record codes: {', '.join(map(str, unknown))}")
    if tuple(codes) != EXPECTED_CODES:
        structural_reasons.append("record codes are not in canonical order")
    if structural_reasons:
        items.append({"code": None, "reasons": structural_reasons, "pages": []})

    for index, record in enumerate(records):
        reasons = reasons_by_record[index]
        if not isinstance(record, dict):
            items.append({"code": None, "reasons": reasons, "pages": []})
            continue
        code = record.get("code")
        if isinstance(code, str) and code in _BLOCK_BY_CODE and record.get("block") != block_for_code(code):
            reasons.append(f"expected canonical block {block_for_code(code)}")
        page_start = record.get("pageStart")
        page_end = record.get("pageEnd")
        if not isinstance(page_start, int) or page_start < 1:
            reasons.append("pageStart must be a positive integer")
        if not isinstance(page_end, int) or not isinstance(page_start, int) or page_end < page_start:
            reasons.append("pageEnd must be at least pageStart")
        if not isinstance(record.get("level"), str) or not record["level"].isdigit():
            reasons.append("level must contain decimal digits")
        if not isinstance(record.get("occurrence"), str) or not _OCCURRENCE.fullmatch(record["occurrence"]):
            reasons.append("occurrence must use MIN:MAX or MIN:N")
        fields = record.get("fields")
        numbers = [field.get("number") for field in fields if isinstance(field, dict)] if isinstance(fields, list) else []
        if not numbers or numbers != list(range(1, len(numbers) + 1)):
            reasons.append("field numbers must be present and contiguous from 1")
        elif any(
            not isinstance(field.get("name"), str) or not field.get("name")
            for field in fields
            if isinstance(field, dict)
        ):
            reasons.append("every field must have a name")
        elif any(
            not isinstance(field.get("type"), str)
            or field.get("type") not in {"C", "N", "NS", "D"}
            for field in fields
            if isinstance(field, dict)
        ):
            reasons.append("every field must have a recognized type")
        ambiguities = record.get("ambiguities")
        if isinstance(ambiguities, list):
            reasons.extend(f"ambiguous: {reason}" for reason in ambiguities if isinstance(reason, str))
        if require_reviewed and record.get("reviewed") is not True:
            reasons.append("record is not reviewed")
        if reasons:
            pages = []
            if isinstance(page_start, int) and isinstance(page_end, int) and page_end >= page_start:
                pages = list(range(page_start, page_end + 1))
            items.append({"code": code if isinstance(code, str) else None, "reasons": reasons, "pages": pages})
    return items


def _sanitize_record(record: dict) -> dict:
    sanitized = {key: record[key] for key in _RECORD_KEYS if key != "fields"}
    sanitized["fields"] = [
        {key: field.get(key, "") for key in _FIELD_KEYS}
        for field in record["fields"]
    ]
    return sanitized


def _parse_field_cells(
    number: int, expected_name: str, cells: list[str], first_parts: list[str]
) -> tuple[dict, bool]:
    if len(first_parts) >= 2 and first_parts[1] == expected_name and any(
        "REGRA_" in part or "Regras de Validação" in part for part in first_parts[2:]
    ):
        description_parts = _parts(cells[1]) if len(cells) > 1 else []
        description = description_parts[0] if description_parts else ""
        values = [(_parts(cell) or [""])[0] for cell in cells[2:7]]
        values.extend([""] * (5 - len(values)))
        return _field(number, expected_name, description, values), True

    conditional = _parse_conditional_required_cells(number, expected_name, cells)
    if conditional is not None:
        return conditional

    tokens = [part for cell in cells for part in (_parts(cell) or [""])]
    name_index = 1
    parsed_name = tokens[name_index] if len(tokens) > name_index else expected_name
    name_end = name_index
    name_matches = True
    if _is_field_identifier(expected_name):
        name_matches = False
        joined_name = ""
        for candidate_end in range(name_index, len(tokens)):
            token = tokens[candidate_end]
            if not _is_field_identifier(token):
                break
            joined_name += token
            if _identifier_key(joined_name) == _identifier_key(expected_name):
                parsed_name = expected_name
                name_end = candidate_end
                name_matches = True
                break
            if not _identifier_key(expected_name).startswith(_identifier_key(joined_name)):
                break
    required_index = len(tokens) - 1
    type_indexes = [
        index
        for index, token in enumerate(tokens[:required_index])
        if token in {"C", "N", "NS", "D"} and required_index - index >= 4
    ]
    type_index = type_indexes[-1] if type_indexes else required_index
    field_type = tokens[type_index] if type_index < required_index else ""
    size = tokens[type_index + 1] if type_index + 1 < required_index else ""
    decimals = tokens[type_index + 2] if type_index + 2 < required_index else ""
    valid_values = " ".join(token for token in tokens[type_index + 3 : required_index] if token)
    description = _without_rule_text(" ".join(token for token in tokens[name_end + 1 : type_index] if token))
    values = [
        field_type,
        size,
        decimals,
        valid_values,
        tokens[required_index],
    ]
    return _field(number, parsed_name, description, values), name_matches


def _parse_conditional_required_cells(
    number: int, expected_name: str, cells: list[str]
) -> tuple[dict, bool] | None:
    if len(cells) != 8:
        return None
    required_parts = _parts(cells[7])
    type_parts = _parts(cells[3])
    if not required_parts or required_parts[0] != "OC":
        return None
    if not type_parts or type_parts[0] not in {"C", "N", "NS", "D"}:
        return None

    name = "".join(_parts(cells[1]))
    name_matches = _identifier_key(name) == _identifier_key(expected_name)
    parsed_name = expected_name if name_matches else name
    description = _plain(cells[2])
    condition = " ".join(required_parts[1:])
    if condition:
        description = " ".join(part for part in (description, condition) if part)
    values = [
        type_parts[0],
        _plain(cells[4]),
        _plain(cells[5]),
        _plain(cells[6]),
        required_parts[0],
    ]
    return _field(number, parsed_name, description, values), name_matches


def _field(number: int, name: str, description: str, values: list[str]) -> dict:
    return {
        "number": number,
        "name": name,
        "description": description,
        "type": values[0],
        "size": values[1],
        "decimals": values[2],
        "required": values[4],
        "validValues": values[3],
    }


def _parts(value: str) -> list[str]:
    return [_plain(part) for part in re.split(r"<br\s*/?>", value, flags=re.IGNORECASE) if _plain(part)]


def _plain(value: str) -> str:
    value = re.sub(r"<[^>]+>", " ", value).replace("**", "")
    return re.sub(r"\s+", " ", value).strip()


def _without_rule_text(value: str) -> str:
    return re.split(r"\bREGRA_[A-Z0-9_]+", value, maxsplit=1)[0].rstrip(" :;-(")


def _is_field_identifier(value: str) -> bool:
    return bool(value) and re.fullmatch(r"[A-ZÀ-Ü0-9_ /]+", value) is not None


def _identifier_key(value: str) -> str:
    return re.sub(r"\s+", "", value)


def _write_json_atomically(path: Path, payload: object) -> None:
    data = (json.dumps(payload, ensure_ascii=False, indent=2) + "\n").encode("utf-8")
    _write_bytes_atomically(path, data)


def _write_bytes_atomically(path: Path, data: bytes) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    descriptor, temporary_name = tempfile.mkstemp(prefix=f".{path.name}.", suffix=".tmp", dir=path.parent)
    temporary = Path(temporary_name)
    try:
        with os.fdopen(descriptor, "wb") as stream:
            stream.write(data)
        os.replace(temporary, path)
    except Exception:
        temporary.unlink(missing_ok=True)
        raise
