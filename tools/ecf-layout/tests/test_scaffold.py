from __future__ import annotations

import hashlib
import json
import subprocess
from copy import deepcopy
from pathlib import Path

import pytest

from ecf_layout import cli
from ecf_layout.scaffold import ScaffoldError, scaffold


MINIMAL_LAYOUT_12_MANIFEST = [
    {
        "code": "0001",
        "block": "0",
        "title": "Abertura do Bloco 0",
        "pageStart": 67,
        "pageEnd": 67,
        "level": "1",
        "occurrence": "1:1",
        "reviewed": True,
        "fields": [
            {
                "number": 1,
                "name": "REG",
                "description": "Texto fixo contendo 0001.",
                "type": "C",
                "size": "4",
                "decimals": "-",
                "required": "Sim",
                "validValues": "[0001]",
            },
            {
                "number": 2,
                "name": "CAMPO_FISCAL",
                "description": "Domínio fiscal externo & sujeito a revisão.",
                "type": "C",
                "size": "12",
                "decimals": "-",
                "required": "Sim",
                "validValues": "[ALFA; BETA]",
            },
            {
                "number": 3,
                "name": "COD_AUX",
                "description": "Código auxiliar preservado como texto.",
                "type": "C",
                "size": "3",
                "decimals": "-",
                "required": "Não",
                "validValues": "-",
            },
        ],
    },
    {
        "code": "C001",
        "block": "C",
        "title": "Abertura do Bloco C",
        "pageStart": 104,
        "pageEnd": 104,
        "level": "1",
        "occurrence": "1:1",
        "reviewed": True,
        "fields": [
            {
                "number": 1,
                "name": "REG",
                "description": "Texto fixo contendo C001.",
                "type": "C",
                "size": "4",
                "decimals": "-",
                "required": "Sim",
                "validValues": "[C001]",
            },
            {
                "number": 2,
                "name": "VL_TOTAL",
                "description": "Valor total informado.",
                "type": "N",
                "size": "10",
                "decimals": "2",
                "required": "Não",
                "validValues": "-",
            },
        ],
    },
]


EXPECTED_TEST_TREE = {
    "tests/TecnoFisc.Sped.Ecf.Tests/Registros/Bloco0/Registro0001Tests.cs": """using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.Bloco0;

public sealed class Registro0001Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistro0001()
    {
        AssertRegistroEcf.CodesAreImplemented("0001");
    }
}
""",
    "tests/TecnoFisc.Sped.Ecf.Tests/Registros/BlocoC/RegistroC001Tests.cs": """using TecnoFisc.Sped.Ecf.Tests.Manifesto;

namespace TecnoFisc.Sped.Ecf.Tests.Registros.BlocoC;

public sealed class RegistroC001Tests
{
    [Fact]
    public void Catalogo_ImplementaRegistroC001()
    {
        AssertRegistroEcf.CodesAreImplemented("C001");
    }
}
""",
}


EXPECTED_SOURCE_TREE = {
    "src/TecnoFisc.Sped.Ecf/Registros/Bloco0/Registro0001.cs": """using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.Bloco0;

/// <summary>
/// Draft do Registro 0001 — Abertura do Bloco 0. Revise os tipos antes de integrar.
/// </summary>
[RegistroSped(Codigo = "0001", Nivel = 1, Bloco = "0")]
public sealed partial class Registro0001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0001";

    /// <summary>Domínio fiscal externo &amp; sujeito a revisão.</summary>
    [CampoSped(Ordem = 2, Tamanho = 12, Obrigatorio = true)]
    public string? CampoFiscal { get; set; }

    /// <summary>Código auxiliar preservado como texto.</summary>
    [CampoSped(Ordem = 3, Tamanho = 3)]
    public string? CodAux { get; set; }
}
""",
    "src/TecnoFisc.Sped.Ecf/Registros/BlocoC/RegistroC001.cs": """using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.BlocoC;

/// <summary>
/// Draft do Registro C001 — Abertura do Bloco C. Revise os tipos antes de integrar.
/// </summary>
[RegistroSped(Codigo = "C001", Nivel = 1, Bloco = "C")]
public sealed partial class RegistroC001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "C001";

    /// <summary>Valor total informado.</summary>
    [CampoSped(Ordem = 2, Tamanho = 10)]
    public string? VlTotal { get; set; }
}
""",
}


EXPECTED_RESERVED_MEMBER_SOURCE = """using TecnoFisc.Sped.Txt.Engine.Abstracoes;
using TecnoFisc.Sped.Txt.Engine.Atributos;

namespace TecnoFisc.Sped.Ecf.Registros.Bloco0;

/// <summary>
/// Draft do Registro 0001 — Abertura do Bloco 0. Revise os tipos antes de integrar.
/// </summary>
[RegistroSped(Codigo = "0001", Nivel = 1, Bloco = "0")]
public sealed partial class Registro0001 : RegistroSped
{
    /// <inheritdoc />
    public override string Codigo => "0001";

    /// <summary>Domínio fiscal externo &amp; sujeito a revisão.</summary>
    [CampoSped(Ordem = 2, Tamanho = 12, Obrigatorio = true)]
    public string? CampoCodigo { get; set; }

    /// <summary>Código auxiliar preservado como texto.</summary>
    [CampoSped(Ordem = 3, Tamanho = 3)]
    public string? CodAux { get; set; }
}
"""


def _write_manifest(tmp_path: Path, records: list[dict] | object = MINIMAL_LAYOUT_12_MANIFEST) -> Path:
    manifest = tmp_path / "layout-12-manifest.json"
    manifest.write_text(json.dumps(records, ensure_ascii=False), encoding="utf-8")
    return manifest


def _tree(root: Path) -> dict[str, str]:
    return {
        path.relative_to(root).as_posix(): path.read_text(encoding="utf-8")
        for path in sorted(root.rglob("*"))
        if path.is_file() and path.name != "layout-12-manifest.json"
    }


def _tree_hash(tree: dict[str, str]) -> str:
    digest = hashlib.sha256()
    for relative_path, content in sorted(tree.items()):
        digest.update(relative_path.encode("utf-8"))
        digest.update(b"\0")
        digest.update(content.encode("utf-8"))
        digest.update(b"\0")
    return digest.hexdigest()


def test_tests_only_emits_catalog_contract_without_production_source(tmp_path: Path) -> None:
    manifest = _write_manifest(tmp_path)

    scaffold(manifest, tmp_path, codes=["0001", "C001"], tests_only=True)

    assert _tree(tmp_path) == EXPECTED_TEST_TREE


def test_source_emits_sealed_partial_record_and_ordered_fields(tmp_path: Path) -> None:
    manifest = _write_manifest(tmp_path)

    scaffold(manifest, tmp_path, codes=["0001", "C001"], source=True)

    assert _tree(tmp_path) == EXPECTED_SOURCE_TREE


def test_source_keeps_unknown_domains_as_string_and_never_invents_enum(tmp_path: Path) -> None:
    manifest = _write_manifest(tmp_path)

    scaffold(manifest, tmp_path, codes=["0001"], source=True)

    assert _tree(tmp_path) == {
        path: content for path, content in EXPECTED_SOURCE_TREE.items() if "Registro0001.cs" in path
    }
    source = next(iter(_tree(tmp_path).values()))
    assert "public string? CampoFiscal" in source
    assert "enum" not in source.casefold()
    assert "Alfa" not in source and "Beta" not in source


def test_source_disambiguates_codigo_from_generated_override_with_exact_name(tmp_path: Path) -> None:
    records = deepcopy(MINIMAL_LAYOUT_12_MANIFEST[:1])
    records[0]["fields"][1]["name"] = "CODIGO"
    manifest = _write_manifest(tmp_path, records)

    scaffold(manifest, tmp_path, codes=["0001"], source=True)

    assert _tree(tmp_path) == {
        "src/TecnoFisc.Sped.Ecf/Registros/Bloco0/Registro0001.cs": EXPECTED_RESERVED_MEMBER_SOURCE
    }


@pytest.mark.parametrize(
    "manual_name, expected_name",
    [
        ("ERROS_DE_FORMATO", "CampoErrosDeFormato"),
        ("VERSAO_LEIAUTE", "CampoVersaoLeiaute"),
        ("PAI", "CampoPai"),
        ("FILHOS", "CampoFilhos"),
        ("TO_STRING", "CampoToString"),
        ("REGISTRO0001", "CampoRegistro0001"),
    ],
)
def test_source_disambiguates_base_and_enclosing_type_members(
    tmp_path: Path, manual_name: str, expected_name: str
) -> None:
    records = deepcopy(MINIMAL_LAYOUT_12_MANIFEST[:1])
    records[0]["fields"][1]["name"] = manual_name
    manifest = _write_manifest(tmp_path, records)

    scaffold(manifest, tmp_path, codes=["0001"], source=True)

    source = next(iter(_tree(tmp_path).values()))
    assert f"public string? {expected_name} {{ get; set; }}" in source


def test_source_rejects_collision_created_by_reserved_member_disambiguation(tmp_path: Path) -> None:
    records = deepcopy(MINIMAL_LAYOUT_12_MANIFEST[:1])
    records[0]["fields"][1]["name"] = "CODIGO"
    records[0]["fields"][2]["name"] = "CAMPO_CODIGO"
    manifest = _write_manifest(tmp_path, records)

    with pytest.raises(ScaffoldError, match="colliding"):
        scaffold(manifest, tmp_path, codes=["0001"], source=True)

    assert _tree(tmp_path) == {}


@pytest.mark.parametrize(
    "manual_name, property_name",
    [
        ("IND_ REC_EXT", "IndRecExt"),
        ("IND_E-COM_TI", "IndEComTi"),
        ("NÍVEL", "Nivel"),
        ("VL_LCTO_PARTE B", "VlLctoParteB"),
        ("NIF/CNPJ", "NifCnpj"),
        ("CONTEÚDO", "Conteudo"),
    ],
)
def test_source_normalizes_manual_field_separators_and_diacritics(
    tmp_path: Path, manual_name: str, property_name: str
) -> None:
    records = deepcopy(MINIMAL_LAYOUT_12_MANIFEST[:1])
    records[0]["fields"][1]["name"] = manual_name
    manifest = _write_manifest(tmp_path, records)

    scaffold(manifest, tmp_path, codes=["0001"], source=True)

    source = next(iter(_tree(tmp_path).values()))
    assert f"public string? {property_name} {{ get; set; }}" in source


@pytest.mark.parametrize(
    "manual_required, expected_attribute",
    [
        ("Sim", "[CampoSped(Ordem = 2, Tamanho = 12, Obrigatorio = true)]"),
        ("S", "[CampoSped(Ordem = 2, Tamanho = 12, Obrigatorio = true)]"),
        ("Sim”", "[CampoSped(Ordem = 2, Tamanho = 12, Obrigatorio = true)]"),
        ("sim", "[CampoSped(Ordem = 2, Tamanho = 12, Obrigatorio = true)]"),
        ("Não", "[CampoSped(Ordem = 2, Tamanho = 12)]"),
        ("N", "[CampoSped(Ordem = 2, Tamanho = 12)]"),
        ("-", "[CampoSped(Ordem = 2, Tamanho = 12)]"),
    ],
)
def test_source_normalizes_unambiguous_required_markers(
    tmp_path: Path, manual_required: str, expected_attribute: str
) -> None:
    records = deepcopy(MINIMAL_LAYOUT_12_MANIFEST[:1])
    records[0]["fields"][1]["required"] = manual_required
    manifest = _write_manifest(tmp_path, records)

    scaffold(manifest, tmp_path, codes=["0001"], source=True)

    assert expected_attribute in next(iter(_tree(tmp_path).values()))


def test_source_rejects_ambiguous_required_marker(tmp_path: Path) -> None:
    records = deepcopy(MINIMAL_LAYOUT_12_MANIFEST[:1])
    records[0]["fields"][1]["required"] = "Talvez"
    manifest = _write_manifest(tmp_path, records)

    with pytest.raises(ScaffoldError, match="required"):
        scaffold(manifest, tmp_path, codes=["0001"], source=True)

    assert _tree(tmp_path) == {}


@pytest.mark.parametrize(
    "impostor",
    [
        "???",
        "123",
        "--",
        "\t",
        "   ",
        "",
        "—",
        "\u200b",
        "nao",
        "False",
    ],
)
def test_required_marker_rejects_every_non_vocabulary_impostor_before_any_write(
    tmp_path: Path, impostor: str
) -> None:
    records = deepcopy(MINIMAL_LAYOUT_12_MANIFEST)
    records[1]["fields"][1]["required"] = impostor
    manifest = _write_manifest(tmp_path, records)

    with pytest.raises(ScaffoldError, match="required"):
        scaffold(manifest, tmp_path, codes=["0001", "C001"], source=True)

    assert _tree(tmp_path) == {}


def test_source_accepts_blank_non_inferred_decimal_and_domain_metadata(tmp_path: Path) -> None:
    records = deepcopy(MINIMAL_LAYOUT_12_MANIFEST[:1])
    records[0]["fields"][1]["decimals"] = ""
    records[0]["fields"][1]["validValues"] = ""
    manifest = _write_manifest(tmp_path, records)

    scaffold(manifest, tmp_path, codes=["0001"], source=True)

    assert _tree(tmp_path) == {
        path: content for path, content in EXPECTED_SOURCE_TREE.items() if "Registro0001.cs" in path
    }


def test_existing_file_requires_force_and_is_never_silently_overwritten(tmp_path: Path) -> None:
    manifest = _write_manifest(tmp_path)
    scaffold(manifest, tmp_path, codes=["0001"], source=True)
    output = tmp_path / "src/TecnoFisc.Sped.Ecf/Registros/Bloco0/Registro0001.cs"
    output.write_text("user-owned\n", encoding="utf-8")

    with pytest.raises(ScaffoldError, match="--force"):
        scaffold(manifest, tmp_path, codes=["0001"], source=True)

    assert output.read_text(encoding="utf-8") == "user-owned\n"
    scaffold(manifest, tmp_path, codes=["0001"], source=True, force=True)
    assert output.read_text(encoding="utf-8") == EXPECTED_SOURCE_TREE[
        "src/TecnoFisc.Sped.Ecf/Registros/Bloco0/Registro0001.cs"
    ]


def test_blocked_destination_path_fails_before_writing_any_file(tmp_path: Path) -> None:
    manifest = _write_manifest(tmp_path)
    blocked = tmp_path / "src/TecnoFisc.Sped.Ecf/Registros/BlocoC"
    blocked.parent.mkdir(parents=True)
    blocked.write_text("not-a-directory\n", encoding="utf-8")

    with pytest.raises(ScaffoldError, match="directory"):
        scaffold(manifest, tmp_path, codes=["0001", "C001"], source=True)

    assert not (
        tmp_path / "src/TecnoFisc.Sped.Ecf/Registros/Bloco0/Registro0001.cs"
    ).exists()
    assert blocked.read_text(encoding="utf-8") == "not-a-directory\n"


def test_same_manifest_produces_byte_identical_tree_and_hash(tmp_path: Path) -> None:
    manifest = _write_manifest(tmp_path)
    first = tmp_path / "first"
    second = tmp_path / "second"

    scaffold(manifest, first, codes=["C001", "0001"], source=True)
    scaffold(manifest, second, codes=["C001", "0001"], source=True)

    first_tree = _tree(first)
    second_tree = _tree(second)
    assert first_tree == second_tree == EXPECTED_SOURCE_TREE
    assert _tree_hash(first_tree) == _tree_hash(second_tree)


def test_modes_are_mutually_exclusive_and_require_exactly_one(tmp_path: Path) -> None:
    manifest = _write_manifest(tmp_path)

    with pytest.raises(ScaffoldError, match="exactly one"):
        scaffold(manifest, tmp_path, codes=["0001"])
    with pytest.raises(ScaffoldError, match="exactly one"):
        scaffold(manifest, tmp_path, codes=["0001"], tests_only=True, source=True)

    assert _tree(tmp_path) == {}


def test_cli_modes_are_mutually_exclusive(tmp_path: Path) -> None:
    manifest = _write_manifest(tmp_path)

    with pytest.raises(SystemExit) as error:
        cli.main(
            [
                "scaffold",
                "--manifest",
                str(manifest),
                "--output-root",
                str(tmp_path),
                "--codes",
                "0001",
                "--tests-only",
                "--source",
            ]
        )

    assert error.value.code == 2
    assert _tree(tmp_path) == {}


def test_cli_scaffold_generates_only_requested_mode(tmp_path: Path) -> None:
    manifest = _write_manifest(tmp_path)

    exit_code = cli.main(
        [
            "scaffold",
            "--manifest",
            str(manifest),
            "--output-root",
            str(tmp_path),
            "--codes",
            "0001,C001",
            "--tests-only",
        ]
    )

    assert exit_code == 0
    assert _tree(tmp_path) == EXPECTED_TEST_TREE


@pytest.mark.parametrize(
    "records, codes, message",
    [
        ({"records": []}, ["0001"], "JSON array"),
        ([{**MINIMAL_LAYOUT_12_MANIFEST[0], "reviewed": False}], ["0001"], "reviewed"),
        ([{**MINIMAL_LAYOUT_12_MANIFEST[0], "code": "../X"}], ["../X"], "code"),
        (MINIMAL_LAYOUT_12_MANIFEST, ["ZZZZ"], "not found"),
    ],
)
def test_malformed_or_unreviewed_manifest_fails_closed_without_outputs(
    tmp_path: Path, records: object, codes: list[str], message: str
) -> None:
    manifest = _write_manifest(tmp_path, records)

    with pytest.raises(ScaffoldError, match=message):
        scaffold(manifest, tmp_path, codes=codes, source=True)

    assert _tree(tmp_path) == {}


@pytest.mark.parametrize(
    "case",
    [
        "missing-page-start",
        "missing-occurrence",
        "extra-record-key",
        "extra-field-key",
        "float-page-start",
        "bool-page-end",
        "float-field-number",
        "integer-level",
        "integer-reviewed",
    ],
)
def test_manifest_requires_exact_keys_and_json_types_without_partial_output(
    tmp_path: Path, case: str
) -> None:
    records = deepcopy(MINIMAL_LAYOUT_12_MANIFEST)
    if case == "missing-page-start":
        del records[0]["pageStart"]
    elif case == "missing-occurrence":
        del records[0]["occurrence"]
    elif case == "extra-record-key":
        records[0]["ambiguities"] = []
    elif case == "extra-field-key":
        records[0]["fields"][1]["domain"] = "invented"
    elif case == "float-page-start":
        records[0]["pageStart"] = 67.0
    elif case == "bool-page-end":
        records[0]["pageEnd"] = True
    elif case == "float-field-number":
        records[0]["fields"][1]["number"] = 2.0
    elif case == "integer-level":
        records[0]["level"] = 1
    elif case == "integer-reviewed":
        records[0]["reviewed"] = 1

    manifest = _write_manifest(tmp_path, records)
    with pytest.raises(ScaffoldError, match="manifest"):
        scaffold(manifest, tmp_path, codes=["0001"], source=True)

    assert _tree(tmp_path) == {}


@pytest.mark.parametrize(
    "case",
    [
        "page-end-before-start",
        "invalid-occurrence",
        "invalid-field-type",
        "zero-field-number",
        "invalid-block",
    ],
)
def test_manifest_rejects_invalid_ranges_and_contract_values(tmp_path: Path, case: str) -> None:
    records = deepcopy(MINIMAL_LAYOUT_12_MANIFEST[:1])
    if case == "page-end-before-start":
        records[0]["pageEnd"] = 66
    elif case == "invalid-occurrence":
        records[0]["occurrence"] = "many"
    elif case == "invalid-field-type":
        records[0]["fields"][1]["type"] = "ENUM"
    elif case == "zero-field-number":
        records[0]["fields"][0]["number"] = 0
    elif case == "invalid-block":
        records[0]["block"] = "Z"

    manifest = _write_manifest(tmp_path, records)
    with pytest.raises(ScaffoldError, match="manifest"):
        scaffold(manifest, tmp_path, codes=["0001"], source=True)

    assert _tree(tmp_path) == {}


def test_all_real_manifest_sources_compile_against_base_contract_stubs(tmp_path: Path) -> None:
    repository = Path(__file__).parents[3]
    manifest = repository / "sped/ecf/layout-12-manifest.json"
    records = json.loads(manifest.read_text(encoding="utf-8"))
    codes = [record["code"] for record in records]
    scaffold(manifest, tmp_path, codes=codes, source=True)
    (tmp_path / "CompileAudit.csproj").write_text(
        """<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
    <EnableNETAnalyzers>false</EnableNETAnalyzers>
    <GenerateDocumentationFile>false</GenerateDocumentationFile>
  </PropertyGroup>
</Project>
""",
        encoding="utf-8",
    )
    (tmp_path / "Stubs.cs").write_text(
        """namespace TecnoFisc.Sped.Txt.Engine.Atributos
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class RegistroSpedAttribute : Attribute
    {
        public string Codigo { get; set; } = string.Empty;
        public int Nivel { get; set; }
        public string Bloco { get; set; } = string.Empty;
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CampoSpedAttribute : Attribute
    {
        public int Ordem { get; set; }
        public int Tamanho { get; set; }
        public bool Obrigatorio { get; set; }
    }
}

namespace TecnoFisc.Sped.Txt.Engine.Abstracoes
{
    public abstract class RegistroSped
    {
        public IReadOnlyList<object> ErrosDeFormato => Array.Empty<object>();
        public abstract string Codigo { get; }
        public virtual int VersaoLeiaute => 0;
        public RegistroSped? Pai { get; internal set; }
        public IReadOnlyList<RegistroSped> Filhos => Array.Empty<RegistroSped>();
    }
}
""",
        encoding="utf-8",
    )

    result = subprocess.run(
        ["dotnet", "build", "CompileAudit.csproj", "--nologo", "-v:q"],
        cwd=tmp_path,
        capture_output=True,
        text=True,
        timeout=120,
        check=False,
    )

    assert result.returncode == 0, result.stdout + result.stderr
