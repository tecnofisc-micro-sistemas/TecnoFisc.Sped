"""Deterministic, format-aware anonymization of private ECF fixtures."""

from __future__ import annotations

import hashlib
import hmac
import json
import os
import re
import tempfile
from collections import Counter
from dataclasses import dataclass
from datetime import date, datetime
from decimal import Decimal, InvalidOperation
from itertools import product
from pathlib import Path
from typing import Callable, Iterable, Sequence


_HMAC_PREFIX = b"ecf-layout-anonymizer-v1\0"
_SUPPORTED_VERSIONS = frozenset(range(8, 13))
_CODE_PATTERN = re.compile(r"[0-9A-Z]{4}\Z")
_NUMBER_PATTERN = re.compile(r"[+-]?\d+(?:[,.]\d+)?\Z")
_UPPER_ASCII = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"
_LOWER_ASCII = "abcdefghijklmnopqrstuvwxyz"
_UPPER_ACCENTED = "ÁÀÂÃÉÊÍÓÔÕÚÜÇ"
_LOWER_ACCENTED = "áàâãéêíóôõúüç"
_DIGITS = "0123456789"


class AnonymizationError(ValueError):
    """A path-free error safe to surface from the anonymizer CLI."""


@dataclass(frozen=True)
class AnonymizeResult:
    record_count: int
    codes: tuple[str, ...]

    @property
    def log_line(self) -> str:
        return f"anonymized: records={self.record_count} codes={','.join(self.codes)}"


@dataclass(frozen=True)
class _ParsedRecord:
    code: str
    cells: tuple[str, ...]
    metadata: dict


@dataclass(frozen=True)
class _ValueInventory:
    forbidden: frozenset[str]
    documents: frozenset[str]
    dates: frozenset[str]


Interrupt = Callable[[str], None]


class _Pseudonymizer:
    def __init__(self, fixture_id: str, forbidden: Iterable[str]) -> None:
        self.fixture_id = fixture_id
        self.forbidden = set(forbidden)
        self.used: set[str] = set()
        self.mappings: dict[str, str] = {}

    def value(self, original: str, *, max_length: int) -> str:
        alternatives: list[tuple[str, Callable[[str], str]]] = []
        for length in range(len(original) + 1, max_length + 1):
            template = _expanded_shape_template(original, length)
            if template is None:
                break
            alternatives.append(
                (
                    f"value-expand-{length}",
                    lambda domain, template=template: _shape_preserving_value(
                        template, fixture_id=self.fixture_id, domain=domain
                    ),
                )
            )
        return self._map(
            "value",
            original,
            lambda domain: _shape_preserving_value(
                original, fixture_id=self.fixture_id, domain=domain
            ),
            alternatives=alternatives,
        )

    def numeric(self, original: str) -> str:
        existing = self.mappings.get(original)
        if existing is not None:
            return existing
        if _numeric_lexeme_space_is_saturated(original, self.forbidden):
            candidate = _numeric_derangement(original, fixture_id=self.fixture_id)
            if candidate == original or candidate in self.used:
                raise AnonymizationError("privacy audit failed")
            self.mappings[original] = candidate
            self.used.add(candidate)
            return candidate
        return self._map(
            "value",
            original,
            lambda domain: _shape_preserving_value(
                original, fixture_id=self.fixture_id, domain=domain
            ),
        )

    def document(self, original: str) -> str:
        kind = "cpf" if len(original) == 11 else "cnpj"
        return self._map(
            kind,
            original,
            lambda domain: _document(original, fixture_id=self.fixture_id, domain=domain),
        )

    def _map(
        self,
        domain: str,
        original: str,
        generate: Callable[[str], str],
        *,
        alternatives: Sequence[tuple[str, Callable[[str], str]]] = (),
    ) -> str:
        existing = self.mappings.get(original)
        if existing is not None:
            return existing
        for candidate_domain, candidate_generator in ((domain, generate), *alternatives):
            for attempt in range(256):
                attempt_domain = (
                    candidate_domain
                    if attempt == 0
                    else f"{candidate_domain}:{attempt}"
                )
                candidate = candidate_generator(attempt_domain)
                if candidate not in self.forbidden and candidate not in self.used:
                    self.mappings[original] = candidate
                    self.used.add(candidate)
                    return candidate
        raise AnonymizationError("privacy audit failed")


def anonymize_bytes(
    source: bytes,
    *,
    fixture_id: str,
    denylist: Iterable[str],
    manifest: Sequence[dict],
) -> bytes:
    """Build and audit a compact canonical CP1252 fixture in memory."""

    if not fixture_id or fixture_id.strip() != fixture_id:
        raise AnonymizationError("invalid fixture id")
    checked_denylist = _validate_denylist_values(denylist, require_nonempty=False)
    records, version, ordered_manifest = _parse_source(source, manifest)
    inventory = _inventory_values(records, version)
    date_offset = _select_date_offset(fixture_id, inventory.dates, inventory.forbidden)
    shifted_dates = {
        _shift_date(value, date_offset=date_offset) for value in inventory.dates
    }
    selected = _compact(records)
    pseudonymizer = _Pseudonymizer(
        fixture_id,
        set(inventory.forbidden) | shifted_dates,
    )
    transformed = [
        _transform_record(
            record,
            version=version,
            pseudonymizer=pseudonymizer,
            inventory=inventory,
            date_offset=date_offset,
        )
        for record in selected
        if record.code != "9900"
    ]
    output_records = _insert_9900(transformed, selected, ordered_manifest, version)
    _recompute_counts(output_records)
    output = _serialize(output_records)
    _audit_output(source, output, checked_denylist)
    _parse_source(output, manifest)
    return output


def anonymize_file(
    source: Path,
    output: Path,
    *,
    fixture_id: str,
    denylist_path: Path,
    manifest_path: Path,
    private_root: Path,
    interrupt: Interrupt | None = None,
) -> AnonymizeResult:
    """Authorize, construct and atomically promote one anonymized fixture."""

    try:
        source = Path(source)
        output = Path(output)
        denylist_path = Path(denylist_path)
        manifest_path = Path(manifest_path)
        private_root = Path(private_root)
        sidecar = source.with_name(source.name + ".sha256")
    except (OSError, TypeError, ValueError):
        raise AnonymizationError("source authorization failed") from None
    _require_private_input(source, private_root)
    _require_private_input(denylist_path, private_root)
    _require_private_input(sidecar, private_root)
    protected_inputs = (source, sidecar, denylist_path, manifest_path)
    _require_distinct_inputs(protected_inputs)
    if any(
        _paths_alias(output, protected)
        for protected in protected_inputs
    ):
        raise AnonymizationError("output authorization failed")

    try:
        source_bytes = source.read_bytes()
        expected_hash_text = sidecar.read_text(encoding="ascii")
    except (OSError, UnicodeError):
        raise AnonymizationError("source authorization failed") from None
    if not re.fullmatch(r"[0-9a-fA-F]{64}(?:\r?\n)?", expected_hash_text):
        raise AnonymizationError("source authorization failed")
    expected_hash = expected_hash_text.rstrip("\r\n")
    if not hmac.compare_digest(hashlib.sha256(source_bytes).hexdigest(), expected_hash.lower()):
        raise AnonymizationError("source authorization failed")

    denylist = _read_denylist(denylist_path)
    try:
        manifest = json.loads(manifest_path.read_text(encoding="utf-8"))
    except (OSError, UnicodeError, json.JSONDecodeError):
        raise AnonymizationError("invalid manifest") from None
    if not isinstance(manifest, list):
        raise AnonymizationError("invalid manifest")
    fixture = anonymize_bytes(
        source_bytes,
        fixture_id=fixture_id,
        denylist=denylist,
        manifest=manifest,
    )
    reparsed, _, _ = _parse_source(fixture, manifest)

    descriptor = -1
    temporary: Path | None = None
    try:
        output.parent.mkdir(parents=True, exist_ok=True)
        descriptor, temporary_name = tempfile.mkstemp(
            prefix=f".{output.name}.", suffix=".tmp", dir=output.parent
        )
        temporary = Path(temporary_name)
        with os.fdopen(descriptor, "wb") as stream:
            descriptor = -1
            stream.write(fixture)
            stream.flush()
            _interrupt(interrupt, "after_write")
            os.fsync(stream.fileno())
        _interrupt(interrupt, "after_fsync")
        written = temporary.read_bytes()
        if written != fixture:
            raise OSError("temporary validation mismatch")
        _parse_source(written, manifest)
        _audit_output(source_bytes, written, denylist)
        _interrupt(interrupt, "after_validation")
        os.replace(temporary, output)
        temporary = None
        _fsync_parent_directory(output.parent)
    except Exception as error:
        if isinstance(error, AnonymizationError):
            raise
        raise AnonymizationError("atomic promotion failed") from None
    finally:
        if descriptor >= 0:
            try:
                os.close(descriptor)
            except OSError:
                pass
        if temporary is not None:
            try:
                temporary.unlink(missing_ok=True)
            except OSError:
                pass

    codes = tuple(dict.fromkeys(record.code for record in reparsed))
    return AnonymizeResult(record_count=len(reparsed), codes=codes)


def _parse_source(
    source: bytes, manifest: Sequence[dict]
) -> tuple[list[_ParsedRecord], int, list[dict]]:
    text = _decode_source(source)
    if not text or "\x00" in text:
        raise AnonymizationError("invalid source")
    lines = text.splitlines()
    if not lines or any(not line for line in lines):
        raise AnonymizationError("invalid source")

    ordered_manifest = _validate_manifest(manifest)
    by_code = {record["code"]: record for record in ordered_manifest}
    basic: list[tuple[str, tuple[str, ...]]] = []
    for line in lines:
        if not line.startswith("|") or not line.endswith("|"):
            raise AnonymizationError("invalid source")
        cells = tuple(line[1:-1].split("|"))
        code = cells[0] if cells else ""
        if not _CODE_PATTERN.fullmatch(code) or code not in by_code:
            raise AnonymizationError("invalid source")
        basic.append((code, cells))

    if basic[0][0] != "0000" or basic[-1][0] != "9999":
        raise AnonymizationError("invalid source")
    if (
        len(basic[0][1]) < 3
        or basic[0][1][1] != "LECF"
        or not re.fullmatch(r"00(?:0[89]|1[0-2])", basic[0][1][2])
    ):
        raise AnonymizationError("invalid source")
    version = int(basic[0][1][2])
    if version not in _SUPPORTED_VERSIONS:
        raise AnonymizationError("invalid source")
    parent_codes = _manifest_parent_codes(ordered_manifest, version)
    manifest_positions = {
        record["code"]: position
        for position, record in enumerate(ordered_manifest)
        if record["code"] in parent_codes
    }
    parsed: list[_ParsedRecord] = []
    seen_in_parent: Counter[tuple[str, int | None]] = Counter()
    total_seen: Counter[str] = Counter()
    last_sibling_position: dict[int | None, int] = {}
    stack: list[tuple[int, _ParsedRecord]] = []
    for source_position, (code, cells) in enumerate(basic):
        metadata = by_code[code]
        introduced = metadata.get("introducedIn", 8)
        if not isinstance(introduced, int) or introduced > version:
            raise AnonymizationError("invalid source")
        fields = _active_fields(metadata, version)
        if len(cells) != len(fields):
            raise AnonymizationError("invalid source")
        for value, field in zip(cells, fields):
            _validate_field_value(value, field)
        level = _level(metadata)
        if level > len(stack):
            raise AnonymizationError("invalid source")
        stack = stack[:level]
        if level and len(stack) != level:
            raise AnonymizationError("invalid source")
        expected_parent = parent_codes[code]
        actual_parent = stack[-1][1].code if stack else None
        if actual_parent != expected_parent:
            raise AnonymizationError("invalid source")
        record = _ParsedRecord(code=code, cells=cells, metadata=metadata)
        parent_position = stack[-1][0] if stack else None
        manifest_position = manifest_positions[code]
        if manifest_position < last_sibling_position.get(parent_position, -1):
            raise AnonymizationError("invalid source")
        last_sibling_position[parent_position] = manifest_position
        stack.append((source_position, record))
        total_seen[code] += 1
        occurrence_key = (code, parent_position)
        seen_in_parent[occurrence_key] += 1
        _, maximum = _occurrence_bounds(metadata)
        if maximum is not None and seen_in_parent[occurrence_key] > maximum:
            raise AnonymizationError("invalid source")
        parsed.append(record)
    if total_seen["0000"] != 1 or total_seen["9999"] != 1:
        raise AnonymizationError("invalid source")
    return parsed, version, ordered_manifest


def _decode_source(source: bytes) -> str:
    return _decode_private_cp1252(source, "invalid source")


def _validate_manifest(manifest: Sequence[dict]) -> list[dict]:
    if not isinstance(manifest, Sequence) or isinstance(manifest, (str, bytes)) or not manifest:
        raise AnonymizationError("invalid manifest")
    records = list(manifest)
    codes: list[str] = []
    for record in records:
        if not isinstance(record, dict):
            raise AnonymizationError("invalid manifest")
        code = record.get("code")
        if not isinstance(code, str) or not _CODE_PATTERN.fullmatch(code):
            raise AnonymizationError("invalid manifest")
        _level(record)
        fields = record.get("fields")
        if not isinstance(fields, list) or not fields:
            raise AnonymizationError("invalid manifest")
        if [field.get("number") for field in fields] != list(range(1, len(fields) + 1)):
            raise AnonymizationError("invalid manifest")
        if fields[0].get("name") != "REG":
            raise AnonymizationError("invalid manifest")
        _occurrence_bounds(record)
        codes.append(code)
    if len(codes) != len(set(codes)) or codes[0] != "0000" or codes[-1] != "9999":
        raise AnonymizationError("invalid manifest")
    return records


def _occurrence_bounds(record: dict) -> tuple[int, int | None]:
    occurrence = record.get("occurrence")
    if not isinstance(occurrence, str):
        raise AnonymizationError("invalid manifest")
    match = re.fullmatch(r"(0|[1-9]\d*):(0|[1-9]\d*|N)", occurrence)
    if match is None:
        raise AnonymizationError("invalid manifest")
    minimum = int(match.group(1))
    maximum = None if match.group(2) == "N" else int(match.group(2))
    if maximum is not None and minimum > maximum:
        raise AnonymizationError("invalid manifest")
    return minimum, maximum


def _manifest_parent_codes(manifest: list[dict], version: int) -> dict[str, str | None]:
    parents: dict[str, str | None] = {}
    stack: list[str] = []
    for record in manifest:
        introduced = record.get("introducedIn", 8)
        if not isinstance(introduced, int) or introduced not in _SUPPORTED_VERSIONS:
            raise AnonymizationError("invalid manifest")
        if introduced > version:
            continue
        level = _level(record)
        if level > len(stack):
            raise AnonymizationError("invalid manifest")
        stack = stack[:level]
        parents[record["code"]] = stack[-1] if stack else None
        stack.append(record["code"])
    return parents


def _level(record: dict) -> int:
    try:
        level = int(record["level"])
    except (KeyError, TypeError, ValueError):
        raise AnonymizationError("invalid manifest") from None
    if level < 0:
        raise AnonymizationError("invalid manifest")
    return level


def _active_fields(record: dict, version: int) -> list[dict]:
    fields = []
    for field in record["fields"]:
        since = field.get("sinceVersion", 8)
        if not isinstance(since, int) or since not in _SUPPORTED_VERSIONS:
            raise AnonymizationError("invalid manifest")
        if since <= version:
            fields.append(field)
    return fields


def _validate_field_value(value: str, field: dict) -> None:
    try:
        value.encode("cp1252", errors="strict")
    except UnicodeEncodeError:
        raise AnonymizationError("invalid source") from None
    size = field.get("size", "-")
    if isinstance(size, str) and size.isdigit() and len(value) > int(size):
        raise AnonymizationError("invalid source")
    domain = _domain_rule(field)
    if not value:
        return
    document_kind = _expected_document_kind(field)
    if document_kind is not None and not _valid_document(value, document_kind):
        raise AnonymizationError("invalid source")
    if domain is not None and not _matches_domain(value, domain):
        raise AnonymizationError("invalid source")
    if _is_date_field(field):
        try:
            datetime.strptime(value, "%d%m%Y")
        except ValueError:
            raise AnonymizationError("invalid source") from None
    if value and _expects_numeric_syntax(field):
        if not _NUMBER_PATTERN.fullmatch(value):
            raise AnonymizationError("invalid source")


def _compact(records: list[_ParsedRecord]) -> list[_ParsedRecord]:
    first_positions: dict[str, int] = {}
    ancestor_positions: list[tuple[int, ...]] = []
    stack: list[int] = []
    for position, record in enumerate(records):
        level = _level(record.metadata)
        stack = stack[:level]
        ancestor_positions.append(tuple(stack))
        stack.append(position)
        first_positions.setdefault(record.code, position)
    required = set(first_positions.values())
    for position in tuple(required):
        required.update(ancestor_positions[position])
    selected = [record for position, record in enumerate(records) if position in required]
    return selected


def _inventory_values(records: list[_ParsedRecord], version: int) -> _ValueInventory:
    forbidden: set[str] = set()
    documents: set[str] = set()
    dates: set[str] = set()
    for record in records:
        if record.code == "9900":
            continue
        fields = _active_fields(record.metadata, version)
        for value, field in zip(record.cells[1:], fields[1:]):
            if not value or _preserves_value(value, field):
                continue
            forbidden.add(value)
            if _is_document_field(field, value):
                documents.add(value)
            if _is_date_field(field):
                dates.add(value)
    if documents & dates:
        raise AnonymizationError("invalid source")
    return _ValueInventory(
        forbidden=frozenset(forbidden),
        documents=frozenset(documents),
        dates=frozenset(dates),
    )


def _transform_record(
    record: _ParsedRecord,
    *,
    version: int,
    pseudonymizer: _Pseudonymizer,
    inventory: _ValueInventory,
    date_offset: int,
) -> _ParsedRecord:
    cells = list(record.cells)
    fields = _active_fields(record.metadata, version)
    for position in range(1, len(cells)):
        cells[position] = _transform_value(
            cells[position],
            fields[position],
            pseudonymizer=pseudonymizer,
            inventory=inventory,
            date_offset=date_offset,
        )
    return _ParsedRecord(record.code, tuple(cells), record.metadata)


def _transform_value(
    value: str,
    field: dict,
    *,
    pseudonymizer: _Pseudonymizer,
    inventory: _ValueInventory,
    date_offset: int,
) -> str:
    if not value:
        return value
    if _preserves_value(value, field):
        return value
    if value in inventory.documents:
        return pseudonymizer.document(value)
    if value in inventory.dates:
        return _shift_date(value, date_offset=date_offset)
    if _expects_numeric_syntax(field):
        if not _NUMBER_PATTERN.fullmatch(value):
            raise AnonymizationError("invalid source")
        return pseudonymizer.numeric(value)
    size = str(field.get("size", ""))
    max_length = int(size) if size.isdigit() else len(value) + 4
    return pseudonymizer.value(value, max_length=max_length)


def _expanded_shape_template(value: str, length: int) -> str | None:
    expandable = next((char for char in reversed(value) if _alphabet(char)), None)
    if expandable is None or length <= len(value):
        return None
    return value + expandable * (length - len(value))


def _numeric_lexeme_space_is_saturated(value: str, forbidden: set[str]) -> bool:
    if not _NUMBER_PATTERN.fullmatch(value):
        return False
    positions = [position for position, char in enumerate(value) if char.isdigit()]
    if not positions or len(positions) > 3:
        return False
    variants = set()
    for digits in product(_DIGITS, repeat=len(positions)):
        candidate = list(value)
        for position, digit in zip(positions, digits, strict=True):
            candidate[position] = digit
        variants.add("".join(candidate))
    return variants.issubset(forbidden)


def _numeric_derangement(value: str, *, fixture_id: str) -> str:
    shift = _digest(fixture_id, "numeric-derangement", "numeric-space")[0] % 9 + 1
    return "".join(
        str((int(char) + shift) % 10) if char in _DIGITS else char
        for char in value
    )


def _preserves_value(value: str, field: dict) -> bool:
    name = str(field.get("name", "")).upper()
    return (
        name in {"NOME_ESC", "COD_VER"}
        or name.startswith("QTD_LIN")
        or _is_closed_value(value, field)
    )


def _is_closed_value(value: str, field: dict) -> bool:
    rule = _domain_rule(field)
    return rule is not None and _matches_domain(value, rule)


def _domain_rule(field: dict) -> tuple[str, tuple[str, ...], bool] | None:
    if str(field.get("name", "")).upper() == "REG":
        return None
    valid = field.get("validValues")
    if not isinstance(valid, str):
        return None
    match = re.fullmatch(r"\[([^\[\]]+)\](?:\s+ou\s+Vazio)?", valid.strip(), re.IGNORECASE)
    if match is None:
        return None
    body = match.group(1).strip()
    allow_empty = bool(re.search(r"\]\s+ou\s+Vazio\Z", valid.strip(), re.IGNORECASE))
    range_match = re.fullmatch(
        r"(-?\d+(?:[,.]\d+)?)\s+a\s+(-?\d+(?:[,.]\d+)?)",
        body,
        re.IGNORECASE,
    )
    if range_match:
        return "range", (range_match.group(1), range_match.group(2)), allow_empty
    if re.search(r"[.:/]", body) or " a " in body.casefold():
        return None
    tokens = tuple(
        token.strip().strip('"“”')
        for token in re.split(r"[;,]", body)
        if token.strip()
    )
    if not tokens or any(not re.fullmatch(r"[0-9A-Za-z_-]+", token) for token in tokens):
        return None
    if len(tokens) == 1 and re.fullmatch(r"[0-9A-Z]{4}_.+", tokens[0]):
        return None
    size = str(field.get("size", ""))
    composite = (
        size.isdigit()
        and int(size) > max(len(token) for token in tokens)
        and all(len(token) == 1 for token in tokens)
    )
    return "options-composite" if composite else "options", tokens, allow_empty


def _matches_domain(value: str, rule: tuple[str, tuple[str, ...], bool]) -> bool:
    kind, values, allow_empty = rule
    if not value:
        return allow_empty
    if kind == "range":
        if _NUMBER_PATTERN.fullmatch(value) is None:
            return False
        try:
            candidate = Decimal(value.replace(",", "."))
            lower = Decimal(values[0].replace(",", "."))
            upper = Decimal(values[1].replace(",", "."))
            return lower <= candidate <= upper
        except InvalidOperation:
            return False
    if value in values:
        return True
    return kind == "options-composite" and all(char in values for char in value)


def _is_document_field(field: dict, value: str) -> bool:
    expected = _expected_document_kind(field)
    if expected is not None:
        return _valid_document(value, expected)
    if len(value) not in {11, 14} or not value.isascii() or not value.isdigit():
        return False
    context = f"{field.get('name', '')} {field.get('description', '')}".upper()
    return "CPF" in context or "CNPJ" in context or field.get("name") == "COD_SCP"


def _expected_document_kind(field: dict) -> str | None:
    name = str(field.get("name", "")).upper()
    if "NIF" in name or "TIN" in name:
        return None
    if "CPF_CNPJ" in name or name in {"IDENT_CPF_CNPJ", "DESTINATARIO"}:
        return "cpf-cnpj"
    tokens = set(name.split("_"))
    if name == "COD_SCP" or "CNPJ" in tokens:
        return "cnpj"
    if "CPF" in tokens:
        return "cpf"
    return None


def _valid_document(value: str, kind: str) -> bool:
    if not value.isascii() or not value.isdigit() or len(set(value)) == 1:
        return False
    if kind == "cpf-cnpj":
        return _valid_document(value, "cpf" if len(value) == 11 else "cnpj")
    if kind == "cpf":
        if len(value) != 11:
            return False
        first = _cpf_digit(value[:9], range(10, 1, -1))
        second = _cpf_digit(value[:9] + first, range(11, 1, -1))
        return value[-2:] == first + second
    if kind == "cnpj":
        if len(value) != 14:
            return False
        first = _cnpj_digit(value[:12], (5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2))
        second = _cnpj_digit(
            value[:12] + first, (6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2)
        )
        return value[-2:] == first + second
    raise AnonymizationError("invalid manifest")


def _expects_numeric_syntax(field: dict) -> bool:
    if _is_date_field(field) or str(field.get("type", "")).upper() != "N":
        return False
    decimals = str(field.get("decimals", ""))
    name = str(field.get("name", "")).upper()
    return bool(decimals and decimals.isdigit()) or name.startswith("QTD_")


def _document(value: str, *, fixture_id: str, domain: str) -> str:
    if len(value) == 11:
        base = _digest_digits(fixture_id, domain, value, 9)
        base = _avoid_repeated_document_base(base)
        first = _cpf_digit(base, range(10, 1, -1))
        return base + first + _cpf_digit(base + first, range(11, 1, -1))
    base = _digest_digits(fixture_id, domain, value, 12)
    base = _avoid_repeated_document_base(base)
    first = _cnpj_digit(base, (5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2))
    return base + first + _cnpj_digit(
        base + first, (6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2)
    )


def _digest_digits(fixture_id: str, domain: str, value: str, count: int) -> str:
    digest = _digest(fixture_id, domain, value)
    return "".join(str(byte % 10) for byte in digest[:count])


def _avoid_repeated_document_base(value: str) -> str:
    if len(set(value)) != 1:
        return value
    replacement = "1" if value[0] != "1" else "2"
    return replacement + value[1:]


def _cpf_digit(value: str, weights: range) -> str:
    digit = 11 - sum(int(char) * weight for char, weight in zip(value, weights)) % 11
    return "0" if digit >= 10 else str(digit)


def _cnpj_digit(value: str, weights: tuple[int, ...]) -> str:
    remainder = sum(int(char) * weight for char, weight in zip(value, weights)) % 11
    return "0" if remainder < 2 else str(11 - remainder)


def _is_date_field(field: dict) -> bool:
    name = str(field.get("name", "")).upper()
    size = str(field.get("size", "")).lstrip("0")
    return size == "8" and (
        name.startswith("DT_") or name.startswith("DAT_") or "DATA" in name
    )


def _select_date_offset(
    fixture_id: str, values: Iterable[str], forbidden: Iterable[str]
) -> int:
    parsed = [datetime.strptime(value, "%d%m%Y").date() for value in values]
    forbidden_values = set(forbidden)
    try:
        digest = _digest(fixture_id, "date-offset", "dates")
    except UnicodeError:
        raise AnonymizationError("invalid fixture id") from None
    initial = int.from_bytes(digest[:2], "big") % 731 - 365
    if initial == 0:
        initial = 1
    candidates = list(dict.fromkeys((initial, -initial)))
    candidates.extend(
        offset
        for magnitude in range(1, 366)
        for offset in (magnitude, -magnitude)
        if offset not in candidates
    )
    for offset in candidates:
        shifted_ordinals = [item.toordinal() + offset for item in parsed]
        if not all(date.min.toordinal() <= ordinal <= date.max.toordinal() for ordinal in shifted_ordinals):
            continue
        shifted_values = {
            f"{item.day:02d}{item.month:02d}{item.year:04d}"
            for item in map(date.fromordinal, shifted_ordinals)
        }
        if shifted_values.isdisjoint(forbidden_values):
            return offset
    raise AnonymizationError("invalid source")


def _shift_date(value: str, *, date_offset: int) -> str:
    try:
        parsed = datetime.strptime(value, "%d%m%Y").date()
        shifted = date.fromordinal(parsed.toordinal() + date_offset)
    except (ValueError, OverflowError):
        raise AnonymizationError("invalid source") from None
    return f"{shifted.day:02d}{shifted.month:02d}{shifted.year:04d}"


def _shape_preserving_value(value: str, *, fixture_id: str, domain: str = "value") -> str:
    digest = _digest(fixture_id, domain, value)
    output: list[str] = []
    for position, char in enumerate(value):
        alphabet = _alphabet(char)
        output.append(alphabet[digest[position % len(digest)] % len(alphabet)] if alphabet else char)
    result = "".join(output)
    if result == value:
        for position, char in enumerate(value):
            alphabet = _alphabet(char)
            if alphabet:
                index = (alphabet.index(char) + 1) % len(alphabet)
                return result[:position] + alphabet[index] + result[position + 1 :]
    return result


def _alphabet(char: str) -> str | None:
    if char in _UPPER_ASCII:
        return _UPPER_ASCII
    if char in _LOWER_ASCII:
        return _LOWER_ASCII
    if char in _UPPER_ACCENTED:
        return _UPPER_ACCENTED
    if char in _LOWER_ACCENTED:
        return _LOWER_ACCENTED
    if char in _DIGITS:
        return _DIGITS
    return None


def _digest(fixture_id: str, domain: str, value: str) -> bytes:
    message = _HMAC_PREFIX + domain.encode("ascii") + b"\0" + value.encode("cp1252")
    return hmac.new(fixture_id.encode("utf-8"), message, hashlib.sha256).digest()


def _insert_9900(
    transformed: list[_ParsedRecord],
    selected: list[_ParsedRecord],
    manifest: list[dict],
    version: int,
) -> list[_ParsedRecord]:
    if not any(record.code == "9900" for record in selected):
        return transformed
    metadata = next(record.metadata for record in selected if record.code == "9900")
    fields = _active_fields(metadata, version)
    if len(fields) < 3:
        raise AnonymizationError("invalid manifest")
    code_order = {record["code"]: position for position, record in enumerate(manifest)}
    source_codes = sorted({record.code for record in selected}, key=code_order.__getitem__)
    expanded: list[_ParsedRecord] = []
    inserted = False
    for record in transformed:
        if not inserted and code_order[record.code] > code_order["9900"]:
            expanded.extend(_totalizer_records(metadata, fields, source_codes))
            inserted = True
        expanded.append(record)
    if not inserted:
        expanded.extend(_totalizer_records(metadata, fields, source_codes))
    return expanded


def _totalizer_records(metadata: dict, fields: list[dict], codes: list[str]) -> list[_ParsedRecord]:
    records = []
    for code in codes:
        cells = ["" for _ in fields]
        cells[0] = "9900"
        cells[1] = code
        cells[2] = "0"
        records.append(_ParsedRecord("9900", tuple(cells), metadata))
    return records


def _recompute_counts(records: list[_ParsedRecord]) -> None:
    counts = Counter(record.code for record in records)
    for position, record in enumerate(records):
        cells = list(record.cells)
        if record.code == "9900":
            cells[2] = str(counts[cells[1]])
        elif record.code == "9999":
            _set_line_count(cells, record.metadata, len(records))
        elif record.code == "9990":
            start = next(
                (index for index, candidate in enumerate(records) if candidate.code.startswith("9")),
                position,
            )
            _set_line_count(cells, record.metadata, position - start + 1)
        elif re.fullmatch(r"[0A-Z]990", record.code):
            block = record.code[0]
            start = next(
                (index for index, candidate in enumerate(records) if candidate.code.startswith(block)),
                position,
            )
            _set_line_count(cells, record.metadata, position - start + 1)
        records[position] = _ParsedRecord(record.code, tuple(cells), record.metadata)


def _set_line_count(cells: list[str], metadata: dict, count: int) -> None:
    for field in metadata["fields"]:
        if str(field.get("name", "")).startswith("QTD_LIN"):
            cells[field["number"] - 1] = str(count)
            return
    raise AnonymizationError("invalid manifest")


def _serialize(records: list[_ParsedRecord]) -> bytes:
    text = "".join("|" + "|".join(record.cells) + "|\r\n" for record in records)
    try:
        return text.encode("cp1252", errors="strict")
    except UnicodeEncodeError:
        raise AnonymizationError("invalid source") from None


def _audit_output(source: bytes, output: bytes, denylist: Iterable[str]) -> None:
    if hmac.compare_digest(hashlib.sha256(source).digest(), hashlib.sha256(output).digest()):
        raise AnonymizationError("privacy audit failed")
    for value in _validate_denylist_values(denylist, require_nonempty=False):
        try:
            encoded = value.encode("cp1252", errors="strict")
        except UnicodeEncodeError:
            raise AnonymizationError("privacy audit failed") from None
        if encoded in output:
            raise AnonymizationError("privacy audit failed")


def _read_denylist(path: Path) -> tuple[str, ...]:
    try:
        raw = path.read_bytes()
    except OSError:
        raise AnonymizationError("privacy audit failed") from None
    text = _decode_private_cp1252(raw, "privacy audit failed")
    return _validate_denylist_values(text.splitlines(), require_nonempty=True)


def _validate_denylist_values(
    values: Iterable[str], *, require_nonempty: bool
) -> tuple[str, ...]:
    checked = tuple(values)
    if (
        (require_nonempty and not checked)
        or any(
            not isinstance(value, str)
            or not value
            or value.strip() != value
            for value in checked
        )
        or len(checked) != len(set(checked))
    ):
        raise AnonymizationError("privacy audit failed")
    return checked


def _require_private_input(path: Path, private_root: Path) -> None:
    try:
        path.resolve().relative_to(private_root.resolve())
    except (OSError, RuntimeError, ValueError):
        raise AnonymizationError("source authorization failed") from None


def _require_distinct_inputs(paths: Sequence[Path]) -> None:
    try:
        aliases = any(
            _paths_alias(paths[first], paths[second])
            for first in range(len(paths))
            for second in range(first + 1, len(paths))
        )
    except AnonymizationError:
        raise AnonymizationError("source authorization failed") from None
    if aliases:
        raise AnonymizationError("source authorization failed")


def _paths_alias(first: Path, second: Path) -> bool:
    try:
        if first.resolve() == second.resolve():
            return True
        return first.exists() and second.exists() and os.path.samefile(first, second)
    except (OSError, RuntimeError):
        raise AnonymizationError("output authorization failed") from None


def _fsync_parent_directory(directory: Path) -> None:
    if os.name == "nt":
        return
    flags = os.O_RDONLY | getattr(os, "O_DIRECTORY", 0)
    descriptor = os.open(directory, flags)
    try:
        os.fsync(descriptor)
    finally:
        os.close(descriptor)


def _decode_private_cp1252(raw: bytes, error_message: str) -> str:
    if raw.startswith((b"\xef\xbb\xbf", b"\xff\xfe", b"\xfe\xff")):
        raise AnonymizationError(error_message)
    try:
        utf8 = raw.decode("utf-8", errors="strict")
    except UnicodeDecodeError:
        utf8 = ""
    if any(ord(char) > 127 for char in utf8):
        raise AnonymizationError(error_message)
    try:
        return raw.decode("cp1252", errors="strict")
    except UnicodeDecodeError:
        raise AnonymizationError(error_message) from None


def _interrupt(interrupt: Interrupt | None, stage: str) -> None:
    if interrupt is not None:
        interrupt(stage)
