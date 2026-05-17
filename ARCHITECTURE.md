# TecnoFisc.Sped — Architecture and Development Plan

> Master document for the TecnoFisc.Sped library. Serves as persistent context across development sessions. Each section describes **what** and **why**; the **how** belongs in code.

This document is for the **TecnoFisc.Sped** repository — the library family that handles all SPED (Sistema Público de Escrituração Digital) projects in .NET. It is a self-contained library, designed to be published as private NuGet packages and potentially open-sourced in the future.

---

## 1. Naming and language conventions

### 1.1 Library name

**TecnoFisc.Sped** — TecnoFisc is the company name, Sped is the system family. Each format-specific project follows the pattern `TecnoFisc.Sped.<Project>`, e.g., `TecnoFisc.Sped.EfdContribuicoes`.

### 1.2 Document language

This document is written in **English** because LLMs (including Claude Code) follow English instructions more reliably and produce more consistent output.

### 1.3 Code language rule (CRITICAL)

Code uses **Portuguese for all SPED-specific concepts** (record names, fiscal terms, field names) and **English only for technical universal patterns and language keywords**. No mixing within the same logical layer.

**Portuguese (mandatory) for:**

- Record classes: `Registro0000`, `RegistroC100`, `RegistroC170`
- Fiscal value objects: `Cnpj`, `Cpf`, `Cfop`, `Ncm`, `Cest`, `Cst`, `ChaveAcesso`
- Enums representing fiscal concepts: `IndicadorOperacao`, `IndicadorEmitente`, `ModeloDocumento`
- Properties matching SPED field names: `IndOper`, `CodPart`, `DtDoc`, `VlDoc`
- Domain methods: `LerArquivo`, `EscreverArquivo`, `ValidarLayout`

**English (allowed/expected) for:**

- C# language keywords (`class`, `public`, `async`, `await`)
- BCL types (`List<T>`, `DateOnly`, `Dictionary<,>`)
- Universal technical patterns: `Parser`, `Generator`, `Reader`, `Writer`, `Builder`, `Factory`
- Test conventions: `*Tests` classes, `Should_*` methods
- Infrastructure concerns: `Stream`, `Pipeline`, `Buffer`

**Forbidden:** mixing Portuguese and English randomly. SPED record classes and fiscal terms stay in Portuguese; technical infrastructure stays in English.

### 1.4 Documentation language

- Architecture documents (this one): English (for LLM efficiency).
- README files for end users: Portuguese.
- Code comments: Portuguese for fiscal/format explanations, English for technical notes.
- Commit messages: Portuguese (Conventional Commits prefixes in English: `feat:`, `fix:`, `refactor:`).

---

## 2. Library overview

### 2.1 Purpose

TecnoFisc.Sped provides .NET classes and parsing/generation infrastructure for all SPED (Sistema Público de Escrituração Digital) projects published by Receita Federal do Brasil. It abstracts the file format details, exposing strongly-typed record classes that consumers can read, manipulate, and write back.

### 2.2 What the library is

- A collection of .NET 10 NuGet packages, one per SPED project.
- A common core (`TecnoFisc.Sped.Core`) with shared infrastructure.
- Strongly-typed record classes for each SPED record specification.
- High-performance parsers using `PipeReader` and minimal allocations.
- Generators that produce SPED-compliant output files.

### 2.3 What the library is NOT

- Not a fiscal application. Has no business logic.
- Not aware of any consuming application's domain.
- Not coupled to any database, storage, or persistence mechanism.
- Not a validator of fiscal correctness — it validates only format conformance.

### 2.4 Distribution

Published as private NuGet packages on Azure Artifacts (or GitHub Packages — to be decided). Independent semantic versioning per package. Eventually may be open-sourced separately.

---

## 3. SPED context and library scope

**SPED (Sistema Público de Escrituração Digital)** é o sistema-guarda-chuva da Receita Federal que abrange uma família grande de projetos (EFD Contribuições, EFD ICMS-IPI, ECD, ECF, EFD-Reinf, eSocial, e-Financeira, DeRE, Central de Balanços, NF-e, NFC-e, NFS-e, CT-e, MDF-e, etc.).

**Library scope (definitive).** TecnoFisc.Sped cobre apenas o subset abaixo. Outros projetos SPED **não** serão implementados e qualquer referência a eles no repositório deve ser removida quando encontrada:

| Projeto SPED | Pacote NuGet | Tipo |
| --- | --- | --- |
| EFD Contribuições | `TecnoFisc.Sped.EfdContribuicoes` | `.txt` (Latin1) |
| EFD ICMS-IPI | `TecnoFisc.Sped.EfdIcmsIpi` | `.txt` (Latin1) |
| ECD | `TecnoFisc.Sped.Ecd` | `.txt` (Latin1) |
| ECF | `TecnoFisc.Sped.Ecf` | `.txt` (Latin1) |
| NF-e | `TecnoFisc.Sped.NFe` | XML (UTF-8) |
| NFC-e | `TecnoFisc.Sped.NFCe` | XML (UTF-8) |
| CT-e | `TecnoFisc.Sped.CTe` | XML (UTF-8) |

Além desses, dois pacotes transversais completam a família:

- `TecnoFisc.Sped.Core` — infraestrutura compartilhada (value objects fiscais, parser/gerador genérico, abstrações de catálogo, identificador dinâmico de arquivos SPED que reconhece o leiaute pela primeira linha — ver §12).
- `TecnoFisc.Sped` — metapacote que referencia todos os leiautes acima em uma única dependência (ver Stage 13).

Todos os outros projetos SPED listados no parágrafo de contexto ficam **explicitamente fora do escopo** e não devem ganhar pacote, stage no roadmap, nem entrada em tracking files.

---

## 4. Architectural principles

### 4.1 Self-containment

Each project depends only on `TecnoFisc.Sped.Core` (which itself depends on nothing external). No project depends on databases, CSV files, or external configuration sources. Hierarchical metadata, layout versions, and validation rules are all embedded in the assembly.

### 4.2 Format independence (with shared authoritative tables)

Each SPED project gets its own package. The duplication rule applies at the **registro level**, not at the table/enum level.

**Registros duplicate per leiaute.** When two projects have records that look similar (e.g., `RegistroC100` in EFD Contribuições and EFD ICMS-IPI), they are duplicated in their respective projects. They have different children, different hierarchy, different cross-record validations, and evolve through different PRs because the Receita Federal publishes their layouts on independent cadences.

**Ato COTEPE-referenced tables/enums live in `TecnoFisc.Sped.Core` (single source of truth).** EFD ICMS-IPI is the **regente** of the Ato COTEPE/ICMS no 44/2018 fiscal tables (`Tabela 4.1.1 - Modelos`, `Tabela 4.1.2 - Situação`, etc.). EFD Contribuições and other leiautes **reference** these tables — they do not redefine them. When the Ato COTEPE changes (e.g., `COD_SIT` codes 04 and 05 descontinuados a partir de 2023-01), the change propagates automatically to every leiaute that references the table. Duplicating the enum across leiautes would create silent drift bugs.

**Concrete classification:**

| Type | Location | Rationale |
| --- | --- | --- |
| `RegistroC100`, `RegistroC170`, etc. | Per-leiaute project | Filhos, hierarquia, validações cross-record divergem. |
| `ModeloDocumento` (Tabela 4.1.1), `SituacaoDocumento` (Tabela 4.1.2) | `Core` | Regidos por Ato COTEPE; EFD ICMS-IPI é regente; demais leiautes referenciam. |
| `Cnpj`, `Cpf`, `Cfop`, `Ncm`, `Cest`, `Cst`, `ChaveAcesso` | `Core` | Conceitos fiscais transversais. |
| `IndicadorOperacao`, `IndicadorEmitente`, `IndicadorPagamento`, `IndicadorFrete` | `Core` | Mesma semântica e mesmos valores em todos os leiautes dentro da janela fiscal de 5 anos. |
| Enums específicos do leiaute (ex.: regimes de PIS/Cofins, blocos M de apuração) | Per-leiaute project | Não existem fora do leiaute. |

### 4.3 Janela fiscal de 5 anos

A Receita Federal só permite revisão/escrituração dos últimos 5 anos. Marcos de versionamento de campos anteriores ao corte (hoje, anteriores a 2021-01) são **irrelevantes para implementação** — dados desse período não podem mais ser escriturados via SPED.

Implicações práticas:

- Versões anteriores de enums (ex.: `IND_PGTO` com código `9` antes de 2012-07; `IND_FRT` com semântica diferente antes de 2017-10/2018-01) **não precisam ser modeladas**.
- Enums que aparentam divergir entre leiautes por marcos antigos (e.g., `IND_FRT` v3 a partir de 2017-10 no EFD Contribuições vs 2018-01 no EFD ICMS-IPI) **convergem** dentro da janela e são compartilháveis em `Core`.
- Quando o corte avança (2027-01, 2028-01, ...), revisar enums Core para remover códigos descontinuados que saíram da janela.

### 4.4 Performance-first

The library is designed for processing files of multiple gigabytes. This drives concrete decisions:

- `PipeReader` over `StreamReader.ReadLine`.
- `ReadOnlySpan<byte>` parsing, not string allocation per line.
- `Utf8Parser.TryParse` for numeric and date fields.
- Source-generated catalogs to eliminate reflection in hot paths.
- Minimal allocations during parsing.

### 4.5 Strongly-typed everything

Consumers should never deal with `string` or `string[]` representations of SPED data. Records expose typed properties (`Cfop`, `DateOnly`, `decimal`, enums). Parsing failures surface as exceptions or `Result<T>` at the parser boundary, not deep in the consumer code.

### 4.6 Symmetric reading and writing

Whatever the parser produces, the generator must consume back. Round-trip a file through the library and the result must match the original (modulo deliberate normalization). This invariant is enforced via property-based testing.

### 4.7 Layout versioning

SPED projects publish new layouts approximately yearly. The library handles multiple versions transparently:

- A `LayoutVersao` enum per project (e.g., `LayoutEfdContribuicoes`, `LayoutEfdIcmsIpi`).
- The parser reads the layout version from `Registro0000` and instantiates appropriate variants.
- **Default:** version-aware serialization on a single class per registro. Fields added in later layouts annotated with `DesdeVersao = LayoutXxx.VYYY` on `[CampoSped]`; parser/gerador honor it against the file's layout. New registros annotated with `IntroduzidoEm` on `[RegistroSped]`.
- **Exception:** structurally divergent registros (rare — order/type/length of an existing field changes) are subclassed `RegistroXxxxVYYY : RegistroXxxx`.
- Receita Federal layouts são **strict-incremental** — uma versão posterior nunca remove campos nem altera significado dos existentes, apenas acrescenta. Isto autoriza o modelo de uma classe + anotações por versão (em vez de uma classe por versão).

### 4.8 Zero reflection in hot paths

Reflection at startup (once) for catalog discovery is acceptable. Reflection during parsing of millions of records is forbidden. Source generators (`IIncrementalGenerator`) produce factory delegates and metadata at compile time.

---

## 5. Technology stack

| Layer | Technology |
| --- | --- |
| Language | C# / .NET 10 |
| Distribution | NuGet packages (Azure Artifacts or GitHub Packages) |
| Parsing | `System.IO.Pipelines` (`PipeReader`) |
| Encoding | Latin1 / Windows-1252 for `.txt`, UTF-8 for XML |
| XML | `System.Xml.Linq` and `System.Xml.Serialization` for XML-based projects |
| Code generation | `IIncrementalGenerator` (Roslyn source generators) |
| Testing | xUnit, FluentAssertions, BenchmarkDotNet |

---

## 6. Solution structure

```text
TecnoFisc.Sped/
├── src/
│   ├── TecnoFisc.Sped/                               # Metapacote (referencia todos os leiautes)
│   ├── TecnoFisc.Sped.Core/                          # Infra compartilhada + sniffer de arquivos SPED
│   ├── TecnoFisc.Sped.Core.SourceGenerators/         # Source generators (catalog + serialization)
│   ├── TecnoFisc.Sped.EfdContribuicoes/              # EFD Contribuições (.txt)
│   ├── TecnoFisc.Sped.EfdIcmsIpi/                    # EFD ICMS-IPI (.txt)
│   ├── TecnoFisc.Sped.Ecd/                           # ECD (.txt)
│   ├── TecnoFisc.Sped.Ecf/                           # ECF (.txt)
│   ├── TecnoFisc.Sped.NFe/                           # NF-e XML
│   ├── TecnoFisc.Sped.NFCe/                          # NFC-e XML
│   └── TecnoFisc.Sped.CTe/                           # CT-e XML
│
├── tests/
│   ├── TecnoFisc.Sped.Core.Tests/
│   ├── TecnoFisc.Sped.Core.SourceGenerators.Tests/
│   ├── TecnoFisc.Sped.EfdContribuicoes.Tests/
│   └── ...
│
├── benchmarks/
│   └── TecnoFisc.Sped.Benchmarks/                    # BenchmarkDotNet performance tests
│
└── samples/
    └── TecnoFisc.Sped.Samples/                       # Console examples for documentation
```

### 6.1 Dependency rules

```text
TecnoFisc.Sped.Core                  ← (no dependencies)
TecnoFisc.Sped.Core.SourceGenerators ← (no dependencies, references Roslyn analyzer APIs)
TecnoFisc.Sped.EfdContribuicoes      ← Core, Core.SourceGenerators (analyzer)
TecnoFisc.Sped.EfdIcmsIpi            ← Core, Core.SourceGenerators (analyzer)
TecnoFisc.Sped.Ecd                   ← Core, Core.SourceGenerators (analyzer)
TecnoFisc.Sped.Ecf                   ← Core, Core.SourceGenerators (analyzer)
TecnoFisc.Sped.NFe                   ← Core, Core.SourceGenerators (analyzer)
TecnoFisc.Sped.NFCe                  ← Core, Core.SourceGenerators (analyzer)
TecnoFisc.Sped.CTe                   ← Core, Core.SourceGenerators (analyzer)
TecnoFisc.Sped (metapacote)          ← todos os pacotes de leiaute acima
```

**Critical rule 1:** No project in TecnoFisc.Sped depends on any database, file system configuration, or external service.

**Critical rule 2:** Format-specific projects (`EfdContribuicoes`, `EfdIcmsIpi`, etc.) do NOT depend on each other. If `RegistroC100` exists in two projects, it is two distinct classes. **However**, this duplication rule applies only to **registros** (with their leiaute-specific filhos, hierarchy, and validations). Tables and enums regidos pelo Ato COTEPE/ICMS (e.g., `Tabela 4.1.1 - Modelos`, `Tabela 4.1.2 - Situação`) live in `TecnoFisc.Sped.Core` as a single source of truth — EFD ICMS-IPI is the regente; other leiautes reference. Duplicating those would create silent drift (see §4.2).

**Critical rule 3:** The source generator project is referenced as an analyzer (`<ProjectReference OutputItemType="Analyzer" ReferenceOutputAssembly="false" />`), not as a runtime dependency. It produces code at compile time, ships nothing to the consumer at runtime.

---

## 7. TecnoFisc.Sped.Core

### 7.1 Composition

```text
TecnoFisc.Sped.Core/
├── ValueObjects/
│   ├── Cnpj.cs
│   ├── Cpf.cs
│   ├── Cfop.cs
│   ├── Ncm.cs
│   ├── Cest.cs
│   ├── Cst.cs
│   ├── ChaveAcesso.cs
│   ├── InscricaoEstadual.cs
│   └── CodigoMunicipio.cs
│
├── Abstracoes/
│   ├── RegistroSped.cs                      # Abstract base for all SPED records
│   ├── IBlocoSped.cs
│   ├── IArquivoSped.cs
│   ├── IRegistroSpedCatalogo.cs
│   └── ILeitorSped.cs / IEscritorSped.cs
│
├── Atributos/
│   ├── RegistroSpedAttribute.cs             # Marks SPED record classes
│   ├── CampoSpedAttribute.cs                # Marks fields with order, type, optionality
│   └── BlocoSpedAttribute.cs
│
├── Catalogo/
│   ├── MetadadosRegistro.cs                 # Description of a record type
│   ├── CatalogoBuilder.cs                   # Reflection-based builder (fallback)
│   └── CatalogoSpedBase.cs                  # Base for source-generated catalogs
│
├── Parser/
│   ├── LeitorSpedTxt.cs                     # PipeReader-based reader
│   ├── PilhaHierarquica.cs                  # Parent stack for hierarchical linking
│   ├── ParseadoresPrimitivos.cs             # Date, decimal, integer parsers
│   └── EncodingSped.cs                      # Latin1/Windows-1252 helpers
│
├── Gerador/
│   ├── EscritorSpedTxt.cs
│   ├── SerializadoresPrimitivos.cs
│   └── TotalizadorBlocos.cs                 # Generates X990 closers and 9999
│
├── Xml/
│   ├── LeitorXmlBase.cs                     # For NF-e, NFC-e, CT-e
│   └── ValidadorAssinaturaDigital.cs
│
└── Erros/
    ├── ErroFormato.cs
    ├── ErroLayout.cs
    └── ResultadoParse.cs
```

### 7.2 Hierarchical metadata strategy

Hierarchical level information lives **with the record class**, declared via attribute:

```csharp
[RegistroSped(Codigo = "C100", Nivel = 2, Bloco = "C")]
public sealed partial class RegistroC100 : RegistroSped { ... }
```

The `partial` modifier matters because the source generator produces companion code in a separate file.

### 7.3 Source generator strategy

The `TecnoFisc.Sped.Core.SourceGenerators` project contains an `IIncrementalGenerator` that:

1. Scans the consuming project for classes inheriting from `RegistroSped` and decorated with `[RegistroSped]`.
2. Generates a static catalog class with all metadata pre-populated.
3. Generates factory delegates (`Func<RegistroSped>`) for each record type, eliminating `Activator.CreateInstance` from hot paths.
4. Generates per-class serialization code (writing the record back to SPED format) based on `[CampoSped]` attributes on properties.

Example of generated code (auto-generated, not hand-written):

```csharp
// <auto-generated />
namespace TecnoFisc.Sped.EfdContribuicoes.Generated;

public static class CatalogoEfdContribuicoes
{
    public static readonly IReadOnlyDictionary<string, MetadadosRegistro> Registros = 
        new Dictionary<string, MetadadosRegistro>(StringComparer.Ordinal)
        {
            ["0000"] = new("0000", Nivel: 0, Bloco: "0", 
                           TipoCSharp: typeof(Registro0000),
                           Factory: static () => new Registro0000()),
            ["C100"] = new("C100", Nivel: 2, Bloco: "C",
                           TipoCSharp: typeof(RegistroC100),
                           Factory: static () => new RegistroC100()),
            // ...
        };
}
```

### 7.4 Performance characteristics of the catalog

Catalog lookup during parsing is `Dictionary<string, MetadadosRegistro>` with `StringComparer.Ordinal` — O(1) with negligible constant. Factory invocation is delegate call — same cost as direct `new`. No reflection, no `Activator.CreateInstance`, no `PropertyInfo.SetValue` in the hot path.

### 7.5 Fallback for non-source-generator scenarios

`CatalogoBuilder.BuildFromAssembly(Assembly)` exists as a runtime fallback for scenarios where source generators cannot run (older tooling, dynamic assemblies, etc.). It uses reflection but caches the result in a `ConcurrentDictionary` keyed by assembly. Cost is paid once on first use and never again. This is the path used in the **first development phase** before source generators are introduced.

### 7.6 Two-phase implementation strategy

**Phase 1 — Library bootstrap:** use `CatalogoBuilder.BuildFromAssembly` (reflection at startup, cached). Library is functional, ships, gets adopted.

**Phase 2 — Performance optimization:** introduce source generator. Public API does not change. Consumers automatically benefit on next package update. No breaking changes.

This staged approach avoids over-engineering early and lets the source generator be designed once the library API has stabilized.

---

## 8. Format-specific projects

Each format-specific project follows this internal structure:

```text
TecnoFisc.Sped.EfdContribuicoes/
├── Registros/
│   ├── Bloco0/
│   │   ├── Registro0000.cs
│   │   ├── Registro0001.cs
│   │   ├── Registro0140.cs
│   │   ├── Registro0150.cs
│   │   ├── Registro0200.cs
│   │   └── ...
│   ├── BlocoC/
│   │   ├── RegistroC001.cs
│   │   ├── RegistroC100.cs
│   │   ├── RegistroC170.cs
│   │   ├── RegistroC190.cs
│   │   └── ...
│   ├── BlocoD/ ... BlocoF/ ... BlocoM/ ... Bloco1/ ... Bloco9/
│   └── ...
│
├── Enums/
│   ├── IndicadorOperacao.cs
│   ├── IndicadorEmitente.cs
│   ├── ModeloDocumento.cs
│   ├── SituacaoDocumento.cs
│   └── ...
│
├── Versionamento/
│   ├── LayoutEfdContribuicoes.cs           # Enum with V006, V007, ...
│   └── DiferencasPorVersao.cs           # Mapping of records that change per version
│
├── Parser/
│   └── ParserEfdContribuicoes.cs           # Specialization that knows the catalog
│
├── Gerador/
│   └── GeradorEfdContribuicoes.cs
│
└── ArquivoEfdContribuicoes.cs              # Top-level model representing a complete file
```

### 8.1 Public API surface

The library exposes two main entry points per project:

```csharp
// Reading
ArquivoEfdContribuicoes arquivo = await ParserEfdContribuicoes.LerAsync(stream, cancellationToken);

// Writing
await GeradorEfdContribuicoes.EscreverAsync(arquivo, stream, cancellationToken);
```

`ArquivoEfdContribuicoes` exposes blocks as strongly-typed collections (`Bloco0`, `BlocoC`, `BlocoF`, `Bloco9`, etc.), each containing typed records.

### 8.2 Streaming alternative

For large files where loading the entire arquivo into memory is undesirable, a streaming API is also exposed:

```csharp
await foreach (var registro in ParserEfdContribuicoes.LerStreamingAsync(stream, ct))
{
    // Process one record at a time without buffering the whole file
}
```

This is what consumers will use during heavy import.

---

## 9. Brazilian fiscal value objects

Located in `TecnoFisc.Sped.Core.ValueObjects`. Each is an immutable struct or sealed class with:

- Private constructor + static factory method (`Criar`).
- Validation enforcing format and check digits where applicable.
- `IEquatable<T>` and value-based equality.
- `ToString()` returning canonical SPED representation.
- Implicit conversion to `string` where it makes sense.

Examples:

**Cnpj** — 14 digits, validates check digits, formats as canonical 14-digit string.

**Cpf** — 11 digits, validates check digits.

**Cfop** — 4 digits, validates that first digit is in {1,2,3,5,6,7} (entry/exit, intra/interstate/foreign).

**Ncm** — 8 digits.

**Cst** — variable length depending on tribute (3 chars for ICMS, 2 for PIS/COFINS), tribute context required.

**ChaveAcesso** — 44 digits NF-e/NFC-e/CT-e access key, validates check digit and embedded fields (UF, AAMM, CNPJ, model, series, number).

These objects represent **format-level concerns** — they validate that the value conforms to SPED's format expectations. They do NOT represent business rules from any consuming domain.

---

## 10. Database and storage

**None.** This library has no database. No file system access except as a pass-through (consumer provides streams). No configuration files at runtime. Self-contained.

---

## 11. Performance benchmarks

The `TecnoFisc.Sped.Benchmarks` project uses BenchmarkDotNet to validate performance assumptions on each release:

- Parsing throughput in MB/s for synthetic files of varying sizes.
- Memory allocation per million records parsed.
- Generator throughput.
- Round-trip overhead (parse + regenerate).

Performance regression in any benchmark blocks merging.

---

## 12. Development plan — ordered stages

### Stage 0 — Foundation

- Repository setup, `Directory.Packages.props`, `EditorConfig`, `.gitignore`, README in Portuguese.
- CI pipeline (build + test) on GitHub Actions.
- Empty solution with placeholder projects.

### Stage 1 — Core value objects

- `Cnpj`, `Cpf`, `Cfop`, `Ncm`, `Cst`, `ChaveAcesso`, `InscricaoEstadual`.
- Comprehensive unit tests (validation, equality, formatting).

### Stage 2 — Core parsing infrastructure

- `RegistroSped` abstract base class.
- `RegistroSpedAttribute` and `CampoSpedAttribute`.
- `LeitorSpedTxt` with `PipeReader`.
- `PilhaHierarquica` for parent linking.
- `EncodingSped`, primitive parsers/serializers.
- `CatalogoBuilder.BuildFromAssembly` (reflection-based, cached).
- Unit tests with synthetic record streams.

### Stage 3 — Core generation infrastructure

- `EscritorSpedTxt`.
- `TotalizadorBlocos` (X990 closers, 9999 file closer).
- Round-trip tests (parse → generate → parse equality).

### Stage 4 — TecnoFisc.Sped.EfdContribuicoes (registros, layout V006)

This stage implements **every registro** of the EFD Contribuições layout V006 (guide v1.35). It is decomposed into **203 sub-stages**, numbered `4.001` … `4.203`. The full decomposition table — sub-stage number, registro code, description, PDF page — lives in **`sped/STAGE_4_REGISTROS.md`**. Read that file when planning a sub-stage; do not duplicate the table here.

Source of the decomposition is the Section 3 TOC of `sped/guides/Guia_Pratico_EFD_Contribuicoes_Versao_1_35 - 18_06_2021.pdf` (PDF page 3). Order of sub-stages = TOC order: Bloco 0 → A → C → D → F → I → M → P → 1 → 9. Within each block, registro codes ascend.

PR granularity: a typical PR covers **one sub-stage**, but trivial registros (block openers/closers, simple "Processo Referenciado" entries with two or three fields and no validations) **may be batched** into one PR when grouping is logically clean (e.g., all `X990` closers at the end, or a contiguous run of `X9xx` referenciados within the same block). Non-trivial registros — anything with hierarchical children, conditional fields, calculated totalizers, or value-object validation — stay as single-sub-stage PRs.

Stage-level deliverables (independent of the sub-stages):

- All enums and value objects required by the registros (added on demand in the sub-stage that first needs them).
- `ParserEfdContribuicoes`, `GeradorEfdContribuicoes`, `ArquivoEfdContribuicoes` — added incrementally; the first sub-stages bootstrap the minimum needed to parse/generate, and later sub-stages extend.
- Real anonymized fixture file exercising every block round-trip.

Publishing: SPED arquivos are all-or-nothing — a partial implementation cannot read a real production file because every record code present in the file must be recognized. Therefore there is **no intermediate release**. Stage 4 ships **v0.1.0 only after all 203 sub-stages are merged** and the parser+generator can round-trip a full real anonymized arquivo.

### Stage 5 — Streaming API

- `ParserEfdContribuicoes.LerStreamingAsync` returning `IAsyncEnumerable<RegistroSped>`.
- Memory-bounded benchmarks proving constant memory for arbitrary file size.
- Can land mid-Stage 4, after enough registros exist to exercise the streaming path end-to-end.

### Stage 6 — Source generator (performance phase)

- `TecnoFisc.Sped.Core.SourceGenerators` project.
- Generator scanning for `[RegistroSped]` and producing static catalog.
- Generator producing factory delegates.
- Migration of `EfdContribuicoes` to use generated catalog.
- Benchmark comparison: reflection cache vs source-generated.
- Lands once the registro shape has stabilized — typically after Bloco 0 and Bloco C are complete.

### Stage 7 — EFD Contribuições V007+ (placeholder, sem trigger ativo)

A Receita não publicou novo leiaute de EFD Contribuições desde V006 (vigente desde 2020-01). Stage permanece em standby até que um novo leiaute apareça. Quando ativado, segue o mesmo padrão de Stage 9 (incrementos EFD ICMS-IPI):

- PDF do novo Guia Prático dropado em `sped/guides/`.
- Constante adicionada ao enum `LayoutEfdContribuicoes` (`V007 = 7`, …).
- Tracking file `sped/STAGE_7_EFD_CONTRIBUICOES_INCR_V0XX.md` listando apenas o delta.
- Novos campos com `[CampoSped(DesdeVersao = (int)LayoutEfdContribuicoes.V0XX)]`; novos registros com `[RegistroSped(IntroduzidoEm = (int)LayoutEfdContribuicoes.V0XX)]`.
- Tests cobrindo round-trip de V006 e do novo leiaute.

### Stage 8 — TecnoFisc.Sped.EfdIcmsIpi (EFD ICMS-IPI, baseline V015)

Same internal structure as `EfdContribuicoes`. Independent set of record classes — no inter-project references (per Hard Rule 2). Shared enums/value objects regidos pelo Ato COTEPE migrate to `Core` on first use (EFD ICMS-IPI is the regente — duplication = drift bug).

**Versioning strategy.** Receita publishes EFD ICMS-IPI layouts approximately yearly. **Versão do leiaute ≠ versão do Guia Prático.** O leiaute é identificado pelo `COD_VER` do registro `0000` (Tabela "Versão do Leiaute" da Nota Técnica conforme Ato COTEPE/ICMS nº 44/2018 e alterações); o Guia Prático é a publicação textual que descreve esse leiaute, com numeração própria (3.0.6, 3.1.x, 3.2.x, …). Várias revisões do Guia podem descrever o mesmo leiaute. Layout V015 é o que entra na janela fiscal de 5 anos (vigente desde janeiro/2021, NT 2020.001). Strict-incremental property: a newer layout never removes a field or changes meaning of an existing one — it only adds fields or registros. Strategy:

1. **Baseline V015.** Implement every registro do leiaute 015 as Stage 8 sub-stages `8.001` … `8.NNN`. Tracking file: `sped/STAGE_8_EFD_ICMS_IPI_V015.md`. Same conventions as Stage 4. As páginas referenciadas usam o Guia Prático mais recente disponível em `sped/guides/` (atualmente 3.2.2), que descreve esse mesmo leiaute.
2. **Incremental V016+.** Cada novo leiaute publicado pela Receita ganha seu próprio tracking file (`sped/STAGE_8_INCR_V016.md`, …) listando **only the diffs**: registros novos, campos adicionados, enums extendidos. Sub-stages numbered `8.1.001`, `8.2.001`, etc. Adicionados ao enum `LayoutEfdIcmsIpi` quando o trabalho for iniciado.
3. **Code model.** One class per registro (e.g., `RegistroC100`). New fields added in later layouts annotated with `DesdeVersao = (int)LayoutEfdIcmsIpi.VXXX`. Parser/gerador and source generator honor `DesdeVersao` against the version read from `Registro0000`. Structurally divergent registros (rare) → subclass `RegistroXxxxVXXX : RegistroXxxx`. New registros introduced in later layouts → annotated with `IntroduzidoEm = (int)LayoutEfdIcmsIpi.VXXX` on `[RegistroSped]`.
4. **`LayoutEfdIcmsIpi`** enum in `src/TecnoFisc.Sped.EfdIcmsIpi/Versionamento/` começa com `V015 = 15`. Convenção: valor inteiro = `COD_VER` do registro `0000` (`V015 = 15`) — permite cast direto em atributos (`DesdeVersao = (int)LayoutEfdIcmsIpi.V015`) e comparação aritmética. Incrementos (`V016`, `V017`, …) são adicionados conforme novos leiautes são implementados.

Publish v0.3.0 only after baseline V015 is complete and round-trips a real anonymized arquivo. Each incremental layout (V016+) ships as a minor bump (0.3.x).

### Stage 9 — EFD ICMS-IPI incrementos V016 … V020

Implementa cumulativamente os leiautes posteriores ao baseline V015, até o leiaute vigente em 2026 (V020). Para cada novo leiaute publicado pela Receita (uma Nota Técnica por ano):

- Tracking file próprio sob `sped/STAGE_8_INCR_V0XX.md` listando **apenas o delta** sobre o leiaute anterior (novos registros, novos campos, alterações de obrigatoriedade, descontinuações).
- Constante adicionada ao enum `LayoutEfdIcmsIpi` (`V016 = 16` … `V020 = 20`) — valor inteiro = `COD_VER` do registro `0000`.
- Novos campos anotados com `[CampoSped(DesdeVersao = (int)LayoutEfdIcmsIpi.V0XX)]`; novos registros com `[RegistroSped(IntroduzidoEm = (int)LayoutEfdIcmsIpi.V0XX)]`. Parser/gerador respeitam essas anotações contra o `COD_VER` lido do `0000`.
- Round-trip real para cada leiaute novo: fixture anonimizada exercitando registros adicionados.
- Cada leiaute implementado entra como minor bump (`0.3.x`) do pacote `TecnoFisc.Sped.EfdIcmsIpi`.

Sub-stages numbered `9.1.001…` (V016), `9.2.001…` (V017), etc. Ordem dentro de cada leiaute: registros novos antes de campos novos antes de mudanças de obrigatoriedade.

### Stage 10 — TecnoFisc.Sped.Ecd (baseline leiaute 2021)

Novo pacote para ECD (Escrituração Contábil Digital). Estrutura interna idêntica a `EfdContribuicoes` / `EfdIcmsIpi`: pasta `Registros/` por bloco, `Enums/`, `Versionamento/`, `Parser/`, `Gerador/`, `Arquivo*.cs`.

**Baseline:** o leiaute vigente em 2021 (versão exata será confirmada lendo o `COD_VER` do registro `0000` do guia oficial no momento da implementação). PDF do Manual de Orientação deve ser dropado em `sped/guides/`. Tracking file: `sped/STAGE_10_ECD_BASELINE.md` com a tabela completa de sub-stages na ordem da Seção 2.6.1 do manual.

**Compartilhamento com Core.** Value objects fiscais (`Cnpj`, `Cpf`, `InscricaoEstadual`, etc.) e enums regidos por Ato COTEPE já vivem em `TecnoFisc.Sped.Core` (Stage 1) e são reutilizados — não duplicar. Enums específicos da ECD (planos de contas, naturezas contábeis) ficam no pacote.

**Independência por registro.** `Registro0000` da ECD é distinto dos `Registro0000` de outros leiautes (Hard Rule 2). Apenas tabelas/enums verdadeiramente transversais migram para Core.

Publica `TecnoFisc.Sped.Ecd 0.4.0` quando baseline 2021 completo e round-trip real estiver validado.

### Stage 11 — ECD incrementos até leiaute vigente

Mesmo padrão de Stage 9: para cada leiaute publicado depois de 2021 até o vigente em 2026, um tracking file `sped/STAGE_10_INCR_V0XX.md` descrevendo apenas o delta. Constantes incrementais no enum `LayoutEcd`. Cada leiaute = minor bump (`0.4.x`).

### Stage 12 — Identificador dinâmico de arquivos SPED (sniffer)

Componente novo em `TecnoFisc.Sped.Core` que **identifica o leiaute SPED a partir da primeira linha do arquivo**. A primeira linha de qualquer arquivo SPED é sempre o `|0000|...|`, e os campos imediatamente seguintes (especialmente `COD_VER`) permitem inferir o projeto (EFD Contribuições vs ICMS-IPI vs ECD vs ECF) e a versão exata do leiaute.

Funcionamento:

- API `SnifferSped.IdentificarAsync(Stream)` lê **apenas a primeira linha não vazia** sem consumir o resto. Devolve `MetadadosArquivoSped { ProjetoSped, VersaoLeiaute, EncodingDetectado, ... }`.
- Caso o consumidor queira prosseguir, expõe `SnifferSped.AbrirParserAsync(Stream)` que devolve o `ILeitorSped` específico do leiaute identificado e o stream posicionado na origem (replay-safe). Internamente delega para `ParserEfdContribuicoes`, `ParserEfdIcmsIpi`, `ParserEcd`, `ParserEcf` conforme apropriado.
- Heurística: combinação `(Bloco do primeiro registro, COD_VER, layout do `0000`)`. Registros `0000` divergem entre leiautes em campos e tamanhos, então o discriminator é sólido.
- Sem reflexão no hot path — o despacho é via `switch` gerado em compile time pelo source generator (extensão de Stage 6) ou tabela estática.

Tests cobrem todos os leiautes suportados + arquivo malformado + EOF prematuro + encoding mismatch.

### Stage 13 — Metapacote TecnoFisc.Sped

Pacote agregador (`TecnoFisc.Sped`) que referencia todos os pacotes de leiaute em uma única dependência NuGet. Útil para consumidores que querem suporte abrangente sem listar cada pacote no `csproj`.

- Sem código próprio — apenas `<PackageReference>` para cada um dos pacotes de leiaute (`EfdContribuicoes`, `EfdIcmsIpi`, `Ecd`, `Ecf`, `NFe`, `NFCe`, `CTe`).
- Versão acompanha a mais alta dos pacotes referenciados; bumps coordenados por release notes consolidados.
- Documentação no README do pacote orienta consumidores a preferir o metapacote quando não souberem antecipadamente qual leiaute vão consumir, ou quando o sniffer (Stage 12) for o ponto de entrada.

Publica `TecnoFisc.Sped 0.5.0` na primeira vez que todos os leiautes textuais estiverem em uso (EFD Contribuições + EFD ICMS-IPI + ECD; ECF pode ser placeholder até Stage 17).

### Stage 14 — TecnoFisc.Sped.NFe (XML)

- XML parser baseado em `System.Xml.Linq` (ou `XmlReader` para arquivos grandes em modo streaming).
- Classes de modelo fortemente tipadas mapeando o schema NF-e (procNFe, infNFe, det, total, transp, cobr, infAdic, etc.).
- Validação de assinatura digital (apenas validação — não assinatura).
- Generator de NF-e XML para casos de retificadora e geração in-house.
- Encoding canônico do XML = UTF-8 (diferente dos `.txt` SPED que são Latin1).

### Stage 15 — TecnoFisc.Sped.NFCe (XML)

Estrutura idêntica a Stage 14, schema NFC-e (Nota Fiscal de Consumidor Eletrônica, modelo 65). Muito código pode ser compartilhado com NF-e via base classes em `TecnoFisc.Sped.Core/Xml/`, mas os tipos públicos do leiaute ficam no pacote NFCe.

### Stage 16 — TecnoFisc.Sped.CTe (XML)

Estrutura idêntica a Stage 14, schema CT-e (Conhecimento de Transporte Eletrônico, modelo 57). Mesmas regras de assinatura digital. Específico do transporte: modais, carga, valores prestados.

### Stage 17 — TecnoFisc.Sped.Ecf (baseline + incrementos)

Pacote para ECF (Escrituração Contábil Fiscal). Padrão `.txt` igual EFD/ECD. Baseline = leiaute vigente quando a stage começar; incrementos seguem o mesmo modelo de Stages 9 e 11 (constantes no enum `LayoutEcf`, tracking files por leiaute, minor bumps por versão).

---

## 13. Code conventions

### 13.1 Naming

- Portuguese for SPED concepts (record names, fiscal terms, field names).
- English for technical universal patterns.
- No mixing within the same logical layer.

### 13.2 Patterns

- Sealed classes by default unless designed for inheritance.
- Records for immutable value objects.
- Private constructors + static factory methods (`Criar`) for value objects with invariants.
- `Result<T>` for parser operations that can fail in expected ways.
- Exceptions for unexpected/programmatic failures.
- `async`/`await` on all I/O. `ConfigureAwait(false)` everywhere (this is a library).
- File-scoped namespaces.
- `partial` modifier on classes that the source generator extends.

### 13.3 Tests

- Convention: `MetodoSendoTestado_Cenario_ResultadoEsperado`.
- AAA (Arrange, Act, Assert) with blank lines.
- FluentAssertions.
- BenchmarkDotNet for performance-sensitive code.
- Coverage target: 90%+ on Core, 85%+ on format projects.
- Round-trip tests for every record type.

### 13.4 Commits

- Conventional Commits (`feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `perf:`).
- One commit per cohesive idea.
- Messages in Portuguese.

### 13.5 Versioning

- Semantic versioning (SemVer).
- Independent versions per package.
- Breaking changes documented in CHANGELOG.md per package.

---

## 14. Glossary

- **SPED** — Sistema Público de Escrituração Digital (umbrella system).
- **EFD** — Escrituração Fiscal Digital (subset of SPED projects).
- **PVA** — Programa Validador e Assinador (Receita Federal validator).
- **Layout** — versioned specification of an SPED project (e.g., EFD Contribuições layout 6.x, 7.x).
- **Bloco** — group of records with related purpose (e.g., bloco C in EFD Contribuições covers electronic invoices).
- **Registro** — a single typed line in an SPED file (e.g., C100, C170).
- **Nível hierárquico** — depth in the parent/child tree of records.
- **CST** — Código de Situação Tributária.
- **CFOP** — Código Fiscal de Operações e Prestações.
- **NCM** — Nomenclatura Comum do Mercosul.
- **CEST** — Código Especificador da Substituição Tributária.

---

## 15. Open decisions

- Internal NuGet feed: Azure Artifacts vs GitHub Packages.
- Source generator scope: include serialization codegen from `[CampoSped]`, or limit to catalog only?
- AOT compatibility: full AOT support is a goal but not blocking initial releases.
- Open-source strategy: timing and license (MIT? Apache 2.0?).
- API surface for streaming vs full-file loading: how prominently to expose streaming?

---

## 16. Notes for Claude Code sessions

When starting a session in this repository:

1. Read this entire document first.
2. Confirm this is the **TecnoFisc.Sped** repository (library).
3. Identify the current development stage (Section 12).
4. Implement only the requested stage. Do not advance unprompted.
5. Apply naming conventions from Section 1.3 strictly.
6. Respect dependency rules from Section 6.1.
7. **NEVER** add database, file-system configuration, or external service dependencies to any project.
8. **NEVER** make format-specific projects depend on each other.
9. **Registros duplicate per leiaute; Ato COTEPE-referenced tables/enums (Tabela 4.1.1, 4.1.2, etc.) live in `Core`** — see §4.2.
10. **5-year fiscal window:** ignore versionamento de campos com vigência anterior a `(hoje - 5 anos)`. Dentro da janela, marcos temporais antigos não são modelados em código (vide §4.3).
11. **EFD ICMS-IPI é o regente do Ato COTEPE.** Quando uma tabela/enum aparecer referenciada em múltiplos leiautes, extrair uma vez no leiaute-origem (EFD ICMS-IPI) e tratar como compartilhada.
12. Performance-sensitive code requires a BenchmarkDotNet benchmark.
13. **Merges into `dev` are always Squash and Merge.** Feature branches may contain granular commits while work is in progress, but the integration commit that lands on `dev` must be a single squashed PR merge.
14. When in doubt about scope, ask before coding.
15. Write tests alongside code, not after.

Update this document when:

- A stage completes.
- An open decision is resolved.
- An architectural decision changes (with rationale).
