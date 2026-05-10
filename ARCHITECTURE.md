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

## 3. SPED context

**SPED (Sistema Público de Escrituração Digital)** is the umbrella system, encompassing all of the following official projects:

- Central de Balanços
- CT-e (Conhecimento de Transporte Eletrônico)
- DeRE
- ECD (Escrituração Contábil Digital)
- ECF (Escrituração Contábil Fiscal)
- EFD Contribuições
- EFD ICMS-IPI
- EFD-Reinf
- e-Financeira
- eSocial
- MDF-e (Manifesto Eletrônico de Documentos Fiscais)
- NFC-e (Nota Fiscal de Consumidor Eletrônica)
- NF-e (Nota Fiscal Eletrônica)
- NFS-e (Nota Fiscal de Serviços Eletrônica)

The library family covers all of these (incrementally, prioritizing EFD Contribuições first), each as its own package.

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
│   ├── TecnoFisc.Sped.Core/                          # Shared parser/generator infrastructure, value objects
│   ├── TecnoFisc.Sped.Core.SourceGenerators/         # Source generators for catalog and serialization
│   ├── TecnoFisc.Sped.EfdContribuicoes/                 # EFD Contribuições (.txt)
│   ├── TecnoFisc.Sped.EfdIcmsIpi/                    # EFD ICMS-IPI (.txt)
│   ├── TecnoFisc.Sped.Reinf/                         # EFD-Reinf
│   ├── TecnoFisc.Sped.Ecd/                           # ECD
│   ├── TecnoFisc.Sped.Ecf/                           # ECF
│   ├── TecnoFisc.Sped.NFe/                           # NF-e XML
│   ├── TecnoFisc.Sped.NFCe/                          # NFC-e XML
│   ├── TecnoFisc.Sped.NFSe/                          # NFS-e
│   ├── TecnoFisc.Sped.CTe/                           # CT-e
│   ├── TecnoFisc.Sped.MDFe/                          # MDF-e
│   └── TecnoFisc.Sped.ESocial/                       # eSocial
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
TecnoFisc.Sped.EfdContribuicoes         ← TecnoFisc.Sped.Core, TecnoFisc.Sped.Core.SourceGenerators (analyzer)
TecnoFisc.Sped.EfdIcmsIpi            ← TecnoFisc.Sped.Core, TecnoFisc.Sped.Core.SourceGenerators (analyzer)
TecnoFisc.Sped.NFe                   ← TecnoFisc.Sped.Core, TecnoFisc.Sped.Core.SourceGenerators (analyzer)
... and so on
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
│   ├── LeitorXmlBase.cs                     # For NF-e, NFC-e, CT-e, MDF-e
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

### Stage 7 — Layout V007 EFD Contribuições (and subsequent)

- Triggered when the Receita publishes guia v1.36+ (PDF dropped in `sped/guides/`).
- `LayoutEfdContribuicoes` enum extended.
- Subclasses or version-aware serialization for changed records.
- Parser auto-detects from `Registro0000` and instantiates appropriate variants.
- Tests covering both V006 and V007 round-trips.

### Stage 8 — TecnoFisc.Sped.EfdIcmsIpi (EFD ICMS-IPI, baseline V306)

Same internal structure as `EfdContribuicoes`. Independent set of record classes — no inter-project references (per Hard Rule 2). Shared enums/value objects regidos pelo Ato COTEPE migrate to `Core` on first use (EFD ICMS-IPI is the regente — duplication = drift bug).

**Versioning strategy.** The Receita publishes EFD ICMS-IPI layouts approximately yearly. The 5-year fiscal window currently covers **18 layouts: V306 through V322**. Strict-incremental property: a newer layout never removes a field or changes meaning of an existing one — it only adds fields or registros. Strategy:

1. **Baseline V306.** Implement every registro of guia v3.0.6 as Stage 8 sub-stages `8.001` … `8.NNN`. Tracking file: `sped/STAGE_8_EFD_ICMS_IPI_V306.md`. Same conventions as Stage 4.
2. **Incremental V307 … V322.** Each subsequent layout gets its own tracking file (`sped/STAGE_8_INCR_V307.md`, … `STAGE_8_INCR_V322.md`) listing **only the diffs**: registros novos, campos adicionados, enums extendidos. Sub-stages numbered `8.1.001`, `8.2.001`, etc.
3. **Code model.** One class per registro (e.g., `RegistroC100`). New fields added in later layouts annotated with `DesdeVersao = LayoutEfdIcmsIpi.VXXX`. Parser/gerador and source generator honor `DesdeVersao` against the version read from `Registro0000`. Structurally divergent registros (rare) → subclass `RegistroXxxxVXXX : RegistroXxxx`. New registros introduced in later layouts → annotated with `IntroduzidoEm = LayoutEfdIcmsIpi.VXXX` on `[RegistroSped]`.
4. **`LayoutEfdIcmsIpi`** enum in `src/TecnoFisc.Sped.EfdIcmsIpi/Versionamento/` with `V306`, `V307`, …, `V322` (18 values).

Publish v0.3.0 only after baseline V306 is complete and round-trips a real anonymized arquivo. Each incremental layout (V307+) ships as a minor bump (0.3.1, 0.3.2, …).

### Stage 9 — TecnoFisc.Sped.NFe (XML)

- XML parser using `System.Xml.Linq`.
- Strongly-typed model classes mapping NF-e XML schema.
- Digital signature validation (validation only — not signing).
- Generator for NF-e XML (for retificadoras and similar use cases).

### Stage 10 — Additional projects (incremental)

- TecnoFisc.Sped.Reinf
- TecnoFisc.Sped.Ecd
- TecnoFisc.Sped.Ecf
- TecnoFisc.Sped.NFCe
- TecnoFisc.Sped.NFSe
- TecnoFisc.Sped.CTe
- TecnoFisc.Sped.MDFe
- TecnoFisc.Sped.ESocial

Each follows the same pattern: records, enums, parser, generator, tests.

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
13. When in doubt about scope, ask before coding.
14. Write tests alongside code, not after.

Update this document when:

- A stage completes.
- An open decision is resolved.
- An architectural decision changes (with rationale).
