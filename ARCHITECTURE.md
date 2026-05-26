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

Code separates **substantives from verbs**. The split is by **meaning, not by identifier kind** — it governs **every identifier**: class names, method names, properties, fields and locals alike. Only nouns that name a **SPED / Brazilian fiscal-tax concept** stay in Portuguese (record classes, fiscal value objects, enum types, SPED field properties). Everything else — **verbs, boolean predicates, capabilities and generic/technical concepts** — uses **idiomatic English**, including **class names**. Portuguese verb/predicate identifiers degrade when accent marks have to be simulated (`EhEntrada` faking `É entrada`, `TemFiltro` faking `Tem filtro`), and English aligns with C# convention and BCL patterns.

This applies to **class names** too: `RegistroC100`, `BlocoC`, `Cnpj` stay Portuguese because they name SPED/fiscal nouns; a class that names an **action, capability or technical concept** uses English (`ReadingOptions`, `Parser`, `Generator`, `Reader`, `Writer`). A boolean predicate is English even when **private** (`IsKnownValueObject`, `HasFilter`, `ShouldIgnore`).

**Portuguese (mandatory) for:**

- Record classes: `Registro0000`, `RegistroC100`, `RegistroC170`
- Fiscal value objects: `Cnpj`, `Cpf`, `Cfop`, `Ncm`, `Cest`, `Cst`, `ChaveAcesso`
- Enums representing fiscal concepts: `IndicadorOperacao`, `IndicadorEmitente`, `ModeloDocumento`
- Properties matching SPED field names: `IndOper`, `CodPart`, `DtDoc`, `VlDoc`
- Domain nouns: `ArquivoEfdContribuicoes`, `BlocoC`, `CatalogoSped`, namespaces (`TecnoFisc.Sped.EfdContribuicoes.Parser`)

**English (mandatory) for:**

- C# language keywords (`class`, `public`, `async`, `await`)
- BCL types (`List<T>`, `DateOnly`, `Dictionary<,>`)
- Universal technical patterns: `Parser`, `Generator`, `Reader`, `Writer`, `Builder`, `Factory`
- **Domain verbs / methods**: `ReadAsync`, `ReadStreamingAsync`, `WriteAsync`, `LoadAsync`, `Create` (static factory on value objects)
- **Boolean predicates** (incl. private): `IsEntrada`, `IsSaida`, `IsIsento`, `IsValid`, `IsKnownValueObject`, `HasFilter`, `ShouldIgnore`
- **Capability / option / technical classes**: `ReadingOptions`, `Parser`, `Generator`, `Reader`, `Writer`, `Builder`
- Test conventions: `*Tests` classes, `Should_*` methods
- Infrastructure concerns: `Stream`, `Pipeline`, `Buffer`

**Examples:**

```csharp
Cnpj documento = Cnpj.Create("12345678000195");
if (cfop.IsEntrada) { ... }
await foreach (var registro in parser.ReadStreamingAsync(stream)) { ... }
ArquivoEfdContribuicoes arquivo = await parser.ReadAsync(stream);
await gerador.WriteAsync(stream, arquivo);
```

The noun (`Cnpj`, `Cfop`, `RegistroC100`, `ArquivoEfdContribuicoes`) is Portuguese; the verb (`Create`, `ReadAsync`, `WriteAsync`) and the predicate (`IsEntrada`) are English. The two languages never mix **inside one identifier**.

**Forbidden:** Portuguese verbs and predicates in code, regardless of identifier kind (`Criar`, `LerAsync`, `EscreverAsync`, `CarregarAsync`, `EhEntrada`, `TemFiltro`, `DeveIgnorar`, `Filtrar`). Portuguese is reserved for **substantives** — the SPED/fiscal vocabulary the consumer reads in their domain.

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
- Not a validator of fiscal correctness — it validates only format conformance. Regras tributárias, obrigatoriedade condicional, dependências cross-registro e validações cruzadas entre blocos são responsabilidade do consumidor (PVA da Receita, regras próprias). Tracker entries do tipo `UPDATE/Validação` e `UPDATE/Obrig` são tratadas como `UPDATE/Doc` — viram doc-comment XML descrevendo a regra para o consumidor, sem código de validação.

### 2.4 Distribution

Published as private NuGet packages on Azure Artifacts (or GitHub Packages — to be decided). Independent semantic versioning per package. Eventually may be open-sourced separately.

### 2.5 Modo de operação por pacote (leitura vs leitura+escrita)

Nem todos os pacotes precisam de gerador. A decisão é por caso de uso real:

| Pacote | Parser | Gerador | Justificativa |
| --- | --- | --- | --- |
| `TecnoFisc.Sped.EfdContribuicoes` | ✅ | ✅ | Consumidor TecnoFisc emite o arquivo. |
| `TecnoFisc.Sped.EfdIcmsIpi` | ✅ | ❌ | Uso é ingestão para análise; não há emissão. |
| `TecnoFisc.Sped.Ecd` | ✅ | ⏸️ | Parser implementado (0.6.0). Gerador depende de confirmação externa — entra em stage dedicada quando demanda confirmada. |
| `TecnoFisc.Sped.NFe` / `NFCe` / `CTe` | ✅ | ⏸️ | Caso de uso confirmado é ingestão dos XMLs já emitidos (validação de assinatura, leitura tipada). Geração/emissão para SEFAZ depende de confirmação externa não controlada só pelo usuário — entra em stage dedicada quando confirmada. |
| `TecnoFisc.Sped.Ecf` | ✅ | ⏸️ | Mesma regra do ECD. Último leiaute textual planejado (depois dos pacotes XML). |

**Implicações dos pacotes read-only:**

- Sem `GeradorXxx.cs`, sem `EscritorSpedTxt` instanciado, sem `IEscritorSped` exposto.
- Sem testes de round-trip (parse → generate → parse).
- Habilita a estratégia de modelo único do leiaute mais recente (§4.7) — sem subclasses por versão.
- `[Descontinuado(EmVersao=...)]` vira informacional no read path (registros históricos ainda aparecem em arquivos antigos).
- Migrar pacote para read+write no futuro = stage dedicada que (i) reativa subclasses por versão quando necessário, (ii) implementa gerador, (iii) restaura testes de round-trip.

---

## 3. SPED context and library scope

**SPED (Sistema Público de Escrituração Digital)** é o sistema-guarda-chuva da Receita Federal que abrange uma família grande de projetos (EFD Contribuições, EFD ICMS-IPI, ECD, ECF, EFD-Reinf, eSocial, e-Financeira, DeRE, Central de Balanços, NF-e, NFC-e, NFS-e, CT-e, MDF-e, etc.).

**Library scope (definitive).** TecnoFisc.Sped cobre apenas o subset abaixo. Outros projetos SPED **não** serão implementados e qualquer referência a eles no repositório deve ser removida quando encontrada:

| Projeto SPED | Pacote NuGet | Tipo |
| --- | --- | --- |
| EFD Contribuições | `TecnoFisc.Sped.EfdContribuicoes` | `.txt` (Latin1) |
| EFD ICMS-IPI | `TecnoFisc.Sped.EfdIcmsIpi` | `.txt` (Latin1) |
| ECD | `TecnoFisc.Sped.Ecd` | `.txt` (Latin1) |
| NF-e | `TecnoFisc.Sped.NFe` | XML (UTF-8) |
| NFC-e | `TecnoFisc.Sped.NFCe` | XML (UTF-8) |
| CT-e | `TecnoFisc.Sped.CTe` | XML (UTF-8) |
| ECF | `TecnoFisc.Sped.Ecf` | `.txt` (Latin1) |

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

SPED projects publish new layouts approximately yearly. A estratégia depende do **modo de operação do pacote** (§2.5):

**Pacotes read+write (EFD Contribuições — único confirmado hoje):**

- A `LayoutVersao` enum per project (e.g., `LayoutEfdContribuicoes`).
- The parser reads the layout version from `Registro0000` and instantiates appropriate variants.
- **Default:** version-aware serialization on a single class per registro. Fields added in later layouts annotated with `DesdeVersao = LayoutXxx.VYYY` on `[CampoSped]`; parser/gerador honor it against the file's layout. New registros annotated with `IntroduzidoEm` on `[RegistroSped]`.
- **Exception:** structurally divergent registros (rare — order/type/length of an existing field changes) are subclassed `RegistroXxxxVYYY : RegistroXxxx`. O catálogo passa a indexar variantes por versão (decisão futura quando esse caso aparecer em pacote read+write).
- Receita Federal layouts são **strict-incremental** — uma versão posterior nunca remove campos nem altera significado dos existentes, apenas acrescenta. Isto autoriza o modelo de uma classe + anotações por versão (em vez de uma classe por versão).

**Pacotes read-only (EFD ICMS-IPI, ECD; NFe/NFCe/CTe/ECF até confirmação de gerador):**

- Existe **uma única modelagem** correspondente ao **leiaute mais recente** dentro da janela fiscal de 5 anos.
- Sem subclasses por versão. Sem catálogo polimórfico por versão. `Dictionary<string, MetadadosRegistro>` 1:1 permanece.
- `LayoutXxx` enum existe apenas para representar o `COD_VER` lido do `Registro0000` e expô-lo ao consumidor; **não** filtra propriedades nem registros durante o parse.
- `[CampoSped(DesdeVersao = V0XX)]` e `[RegistroSped(IntroduzidoEm = V0XX)]` viram **informacionais** (doc/auditoria) — campos novos em arquivos antigos ficam vazios (`null`/`default`), registros novos não aparecem.
- `[Descontinuado(EmVersao = V0XX)]` vira informacional **no read path** — registros descontinuados continuam sendo reconhecidos pelo parser porque ainda aparecem em arquivos históricos.
- Campos com **regressão de tipo** (raríssimo — texto → numérico ou vice-versa entre versões dentro da janela) são modelados como `string` lazy; o consumidor converte se precisar. Justificativa: mantém compatibilidade com arquivos de qualquer versão dentro da janela ao custo de tipagem fraca no campo regredido.
- Migrar pacote para read+write no futuro = reativar a estratégia padrão (subclasses + catálogo polimórfico + atributos com efeito real).

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
│   ├── TecnoFisc.Sped.NFe/                           # NF-e XML
│   ├── TecnoFisc.Sped.NFCe/                          # NFC-e XML
│   ├── TecnoFisc.Sped.CTe/                           # CT-e XML
│   └── TecnoFisc.Sped.Ecf/                           # ECF (.txt)
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
TecnoFisc.Sped.NFe                   ← Core, Core.SourceGenerators (analyzer)
TecnoFisc.Sped.NFCe                  ← Core, Core.SourceGenerators (analyzer)
TecnoFisc.Sped.CTe                   ← Core, Core.SourceGenerators (analyzer)
TecnoFisc.Sped.Ecf                   ← Core, Core.SourceGenerators (analyzer)
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
ArquivoEfdContribuicoes arquivo = await ParserEfdContribuicoes.ReadAsync(stream, cancellationToken);

// Writing
await GeradorEfdContribuicoes.WriteAsync(arquivo, stream, cancellationToken);
```

`ArquivoEfdContribuicoes` exposes blocks as strongly-typed collections (`Bloco0`, `BlocoC`, `BlocoF`, `Bloco9`, etc.), each containing typed records.

### 8.2 Streaming alternative

For large files where loading the entire arquivo into memory is undesirable, a streaming API is also exposed:

```csharp
await foreach (var registro in ParserEfdContribuicoes.ReadStreamingAsync(stream, ct))
{
    // Process one record at a time without buffering the whole file
}
```

This is what consumers will use during heavy import.

---

## 9. Brazilian fiscal value objects

Located in `TecnoFisc.Sped.Core.ValueObjects`. Each is an immutable struct or sealed class with:

- Private constructor + static factory method (`Create`).
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

- `ParserEfdContribuicoes.ReadStreamingAsync` returning `IAsyncEnumerable<RegistroSped>`.
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

### Stage 8 — TecnoFisc.Sped.EfdIcmsIpi (EFD ICMS-IPI, **read-only**, baseline V015)

Same internal structure as `EfdContribuicoes` **menos o gerador** (§2.5). Independent set of record classes — no inter-project references (per Hard Rule 2). Shared enums/value objects regidos pelo Ato COTEPE migrate to `Core` on first use (EFD ICMS-IPI is the regente — duplication = drift bug).

**Modo read-only.** O pacote expõe apenas parser + modelo tipado. Não existe `GeradorEfdIcmsIpi`. Não existem testes de round-trip parse→generate→parse — apenas testes de fixture-load + asserts sobre o modelo lido. Habilita a estratégia §4.7 read-only de modelo único do leiaute mais recente.

**Versioning strategy.** Receita publishes EFD ICMS-IPI layouts approximately yearly. **Versão do leiaute ≠ versão do Guia Prático.** O leiaute é identificado pelo `COD_VER` do registro `0000` (Tabela "Versão do Leiaute" da Nota Técnica conforme Ato COTEPE/ICMS nº 44/2018 e alterações); o Guia Prático é a publicação textual que descreve esse leiaute, com numeração própria (3.0.6, 3.1.x, 3.2.x, …). Várias revisões do Guia podem descrever o mesmo leiaute. Strict-incremental property: a newer layout never removes a field or changes meaning of an existing one — it only adds fields or registros. Strategy:

1. **Baseline V015 já implementado.** Tracking file: `sped/STAGE_8_EFD_ICMS_IPI_V015.md`. As páginas referenciadas usam o Guia Prático mais recente disponível em `sped/guides/` (atualmente 3.2.2). O modelo evolui no lugar (§4.7 read-only) — campos novos das versões posteriores são acrescentados às classes existentes; o baseline V015 não é "reescrito".
2. **Incrementos V016 → leiaute vigente (V020).** Cada leiaute novo publicado pela Receita ganha seu próprio tracking file (`sped/STAGE_8_INCR_V016.md`, …, `STAGE_8_INCR_V020.md`) listando **apenas os deltas que afetam o read path**: registros novos, campos adicionados, enums estendidos, campos com regressão de tipo (declarados como `string` lazy). Sub-stages numbered `8.016.001` (V016), `8.017.001` (V017), etc. Adicionados ao enum `LayoutEfdIcmsIpi` no PR do primeiro sub-stage que consumir o membro (first-use).
3. **Code model (read-only).** Uma única classe por registro (e.g., `RegistroC100`). Campos novos em leiautes posteriores entram como properties novas na mesma classe com `[CampoSped(DesdeVersao = (int)LayoutEfdIcmsIpi.VXXX)]` (anotação informacional — parser não filtra). Novos registros = classes novas com `[RegistroSped(IntroduzidoEm = (int)LayoutEfdIcmsIpi.VXXX)]`. **Sem subclasses** `RegistroXxxxVXXX : RegistroXxxx`. Mudanças de tamanho são aplicadas in-place no atributo. Mudanças de tipo regressivas são modeladas como `string` lazy.
4. **Categorias de delta que viram `UPDATE/Doc` (sem código de validação):** `UPDATE/Validação`, `UPDATE/Obrig` (mudança S↔OC↔O). Conforme §2.3, validações fiscais ficam com o consumidor. Doc-comment XML registra a regra para referência.
5. **`LayoutEfdIcmsIpi`** enum em `src/TecnoFisc.Sped.EfdIcmsIpi/Versionamento/` começa com `V015 = 15`. Convenção: valor inteiro = `COD_VER` do registro `0000`. Incrementos (`V016`, `V017`, …, `V020`) são adicionados conforme novos leiautes são consumidos.

Publicada `v0.3.0` quando baseline V015 ficou completo e o parser leu um arquivo real anonimizado. Incrementos V016/V017 saíram como minor bumps `0.3.x`. A `v0.4.0` consolida V018+V019+V020 com a flip oficial para read-only (remoção de `GeradorEfdIcmsIpi` e dos testes de round-trip). Incrementos futuros (V021+) voltam ao modelo de minor bump por leiaute (`0.4.x`).

### Stage 9 — EFD ICMS-IPI incrementos V016 … V020 (read-only)

Implementa cumulativamente os leiautes posteriores ao baseline V015, até o leiaute vigente em 2026 (V020). Modo **read-only** (§2.5) + estratégia de modelo único do leiaute mais recente (§4.7). Para cada novo leiaute publicado pela Receita (uma Nota Técnica por ano):

- Tracking file próprio sob `sped/STAGE_8_INCR_V0XX.md` listando **apenas os deltas relevantes ao read path**: novos registros, novos campos, mudanças de tamanho (in-place), regressões de tipo (lazy `string`), descontinuações (informacionais).
- Constante adicionada ao enum `LayoutEfdIcmsIpi` (`V016 = 16` … `V020 = 20`) — valor inteiro = `COD_VER` do registro `0000`.
- Novos campos anotados com `[CampoSped(DesdeVersao = (int)LayoutEfdIcmsIpi.V0XX)]` **(informacional)**; novos registros com `[RegistroSped(IntroduzidoEm = (int)LayoutEfdIcmsIpi.V0XX)]` **(informacional)**.
- Fixture anonimizada por leiaute exercitando o parse dos registros/campos adicionados (sem round-trip de geração).
- Cada leiaute implementado entra como minor bump (`0.3.x`) do pacote `TecnoFisc.Sped.EfdIcmsIpi`.

Sub-stages numerados `8.016.001…` (V016), `8.017.001…` (V017), etc. Ordem dentro de cada leiaute: registros novos antes de campos novos antes de docs.

**Fora do escopo deste Stage 9 (read-only):** `UPDATE/Validação`, `UPDATE/Obrig`, `UPDATE/Subclasse` — viram `UPDATE/Doc` (doc-comment XML) ou `UPDATE/Campo` (mudança in-place no atributo) conforme aplicável. Validações fiscais ficam com o consumidor (§2.3).

### Stage 10 — TecnoFisc.Sped.Ecd (baseline leiaute 9, read-only inicial)

Novo pacote para ECD (Escrituração Contábil Digital). Estrutura interna idêntica a `EfdIcmsIpi` (read-only — sem `Gerador/`, sem round-trip de geração). Pasta `Registros/` por bloco, `Enums/`, `Versionamento/`, `Parser/`, `Arquivo*.cs`. Operational appendix: **`sped/STAGE_10_ECD_BASELINE.md`** (72 sub-stages, blocos `0 → C → I → J → K → 9`).

**Modo de operação.** Read-only inicialmente (§2.5) — gerador depende de confirmação de necessidade externa. Quando confirmada, criar stage dedicada ativando subclasses por versão + gerador + round-trip.

**Baseline: leiaute 9, vigente a partir do ano-calendário 2020** (não há leiaute posterior — ver Stage 11). PDF do Manual de Orientação (`Manual_de_Orientação_da_ECD_Leiaute_20_maio_2026.pdf`, Ato Declaratório Executivo Cofis nº 01/2026) já dropado em `sped/guides/`. Encoding do `.txt`: ISO-8859-1 (Latin-1). Ordem das sub-stages segue a **Seção 3.6 (Leiaute dos Registros)** do manual — fonte autoritativa quando diverge da tabela-resumo 3.2 (e.g., `C052` consta em 3.6, ausente em 3.2).

**Versão do leiaute fora do `0000`.** Diferente de EFD, o `Registro0000` da ECD **não** carrega `COD_VER` — campo 02 é o literal `"LECD"`. O código da versão do leiaute mora em **`I010.COD_VER_LC`** (`"9.00"` para AC2024+). `LayoutEcd` enum (`V009 = 9`) é informacional (§4.7 read-only). Impacta o sniffer (§12).

**Compartilhamento com Core.** Value objects fiscais (`Cnpj`, `Cpf`, `InscricaoEstadual`, `CodigoMunicipio`, etc.) já vivem em `TecnoFisc.Sped.Core` (Stage 1) e são reutilizados — não duplicar. Enums específicos da ECD (forma de escrituração `IND_ESC` `G/R/A/B/Z`, naturezas contábeis) ficam no pacote. `IND_ESC` dirige obrigatoriedade condicional (análogo ao `PERFIL` do EFD ICMS-IPI) → `UPDATE/Doc`, validação fica com o consumidor (§2.3).

**Independência por registro.** `Registro0000` da ECD é distinto dos `Registro0000` de outros leiautes (Hard Rule 2). Apenas value objects/tabelas verdadeiramente transversais migram para Core.

Publica `TecnoFisc.Sped.Ecd` (bump apropriado no release) quando todas as 72 sub-stages do baseline leiaute 9 completas e parser lê real anonimizado. **✅ Concluído na `0.6.0`** — 72 sub-stages merged, `ParserEcd`/`ArquivoEcd` lendo arquivo real anonimizado.

### Stage 11 — ECD incrementos até leiaute vigente (standby — sem trigger ativo)

**Sem incrementos hoje.** A Receita não publicou leiaute de ECD posterior ao **leiaute 9** (vigente desde o ano-calendário 2020). O baseline de Stage 10 já é o leiaute vigente — existe **uma única modelagem**. Stage 11 permanece em standby até surgir um leiaute 10.

Quando ativado, segue o mesmo padrão de Stage 9: para cada novo leiaute publicado, um tracking file `sped/STAGE_10_INCR_V0XX.md` descrevendo apenas o delta do read path. Constantes incrementais no enum `LayoutEcd` (`V010 = 10`, …). Cada leiaute = minor bump.

### Stage 12 — Identificador dinâmico de arquivos SPED (sniffer)

Componente novo em `TecnoFisc.Sped.Core` que **identifica o leiaute SPED a partir da primeira linha do arquivo**. A primeira linha de qualquer arquivo SPED é sempre o `|0000|...|`, e os campos imediatamente seguintes (especialmente `COD_VER`) permitem inferir o projeto (EFD Contribuições vs ICMS-IPI vs ECD vs ECF) e a versão exata do leiaute.

Funcionamento:

- API `SnifferSped.IdentificarAsync(Stream)` lê **apenas a primeira linha não vazia** sem consumir o resto. Devolve `MetadadosArquivoSped { ProjetoSped, VersaoLeiaute, EncodingDetectado, ... }`.
- Caso o consumidor queira prosseguir, expõe `SnifferSped.AbrirParserAsync(Stream)` que devolve o `ILeitorSped` específico do leiaute identificado e o stream posicionado na origem (replay-safe). Internamente delega para `ParserEfdContribuicoes`, `ParserEfdIcmsIpi`, `ParserEcd`, `ParserEcf` conforme apropriado.
- Heurística: combinação `(Bloco do primeiro registro, campo discriminador, layout do `0000`)`. Registros `0000` divergem entre leiautes em campos e tamanhos, então o discriminator é sólido. **Caso EFD** (Contribuições / ICMS-IPI): `COD_VER` no próprio `0000` dá projeto + versão. **Caso ECD:** o `0000` **não** tem `COD_VER` — o campo 02 é o literal `"LECD"`, que identifica o projeto pela primeira linha; a versão do leiaute (`COD_VER_LC`) só aparece no `I010`, então o sniffer reporta o projeto ECD na linha 1 mas a `VersaoLeiaute` exige ler até o `I010` (ou assumir o baseline único — leiaute 9 — enquanto não houver incrementos).
- Sem reflexão no hot path — o despacho é via `switch` gerado em compile time pelo source generator (extensão de Stage 6) ou tabela estática.

Tests cobrem todos os leiautes suportados + arquivo malformado + EOF prematuro + encoding mismatch.

### Stage 13 — Metapacote TecnoFisc.Sped

Pacote agregador (`TecnoFisc.Sped`) que referencia todos os pacotes de leiaute em uma única dependência NuGet. Útil para consumidores que querem suporte abrangente sem listar cada pacote no `csproj`.

- Sem código próprio — apenas `<PackageReference>` para cada um dos pacotes de leiaute (`EfdContribuicoes`, `EfdIcmsIpi`, `Ecd`, `NFe`, `NFCe`, `CTe`, `Ecf`).
- Versão acompanha a mais alta dos pacotes referenciados; bumps coordenados por release notes consolidados.
- Documentação no README do pacote orienta consumidores a preferir o metapacote quando não souberem antecipadamente qual leiaute vão consumir, ou quando o sniffer (Stage 12) for o ponto de entrada.

Publica `TecnoFisc.Sped 0.5.0` na primeira vez que todos os leiautes textuais estiverem em uso (EFD Contribuições + EFD ICMS-IPI + ECD; ECF pode ser placeholder até Stage 17).

### Stage 14 — TecnoFisc.Sped.NFe (XML, **read-only**)

- XML parser baseado em `System.Xml.Linq` (ou `XmlReader` para arquivos grandes em modo streaming).
- Classes de modelo fortemente tipadas mapeando o schema NF-e (procNFe, infNFe, det, total, transp, cobr, infAdic, etc.).
- Validação de assinatura digital (apenas validação — não assinatura).
- Encoding canônico do XML = UTF-8 (diferente dos `.txt` SPED que são Latin1).
- **Modo read-only (§2.5).** Sem `GeradorNFe`, sem emissão para SEFAZ, sem round-trip parse→generate→parse. Caso de uso confirmado é ingestão dos XMLs já emitidos. Geração/emissão entra em stage dedicada se confirmada por demanda externa.

### Stage 15 — TecnoFisc.Sped.NFCe (XML, **read-only**)

Estrutura idêntica a Stage 14, schema NFC-e (Nota Fiscal de Consumidor Eletrônica, modelo 65). Muito código pode ser compartilhado com NF-e via base classes em `TecnoFisc.Sped.Core/Xml/`, mas os tipos públicos do leiaute ficam no pacote NFCe. Modo read-only (§2.5) — sem `GeradorNFCe`.

### Stage 16 — TecnoFisc.Sped.CTe (XML, **read-only**)

Estrutura idêntica a Stage 14, schema CT-e (Conhecimento de Transporte Eletrônico, modelo 57). Validação de assinatura digital igual a NFe/NFCe. Específico do transporte: modais, carga, valores prestados. Modo read-only (§2.5) — sem `GeradorCTe`.

### Stage 17 — TecnoFisc.Sped.Ecf (baseline + incrementos, read-only inicial)

Pacote para ECF (Escrituração Contábil Fiscal). Padrão `.txt` igual EFD/ECD. Read-only inicialmente (§2.5) — gerador depende de confirmação externa. Baseline = leiaute vigente quando a stage começar; incrementos seguem o mesmo modelo read-only de Stage 9 (constantes no enum `LayoutEcf`, tracking files por leiaute, minor bumps por versão).

---

## 13. Code conventions

### 13.1 Naming

- Portuguese for SPED **nouns**: record classes, fiscal value objects, fiscal enums, SPED field properties.
- English for **verbs**, static factory methods (`Create`), boolean predicates (`IsEntrada`, `IsValid`), and technical universal patterns (`Parser`, `Generator`, `Builder`).
- No mixing within a single identifier. See §1.3 for the full rule and examples.

### 13.2 Patterns

- Sealed classes by default unless designed for inheritance.
- Records for immutable value objects.
- Private constructors + static factory methods (`Create`) for value objects with invariants.
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
12. **Modo de operação do pacote dita a estratégia de versionamento (§2.5 + §4.7).** EFD ICMS-IPI, ECD, NF-e, NFC-e, CT-e e ECF são read-only por padrão — sem `Gerador/`, sem round-trip de geração, modelo único do leiaute mais recente. EFD Contribuições é o único pacote read+write confirmado. Não criar gerador para pacote read-only sem promover oficialmente a stage de migração.
13. **Validações fiscais ficam com o consumidor (§2.3).** `UPDATE/Validação` e `UPDATE/Obrig` em trackers viram `UPDATE/Doc` — apenas doc-comment XML. Não criar pasta `Validadores/Versionados/` nem `IValidadorVersionado<T>`.
14. Performance-sensitive code requires a BenchmarkDotNet benchmark.
15. **Merges into `dev` are always Squash and Merge.** Feature branches may contain granular commits while work is in progress, but the integration commit that lands on `dev` must be a single squashed PR merge.
16. When in doubt about scope, ask before coding.
17. Write tests alongside code, not after.

Update this document when:

- A stage completes.
- An open decision is resolved.
- An architectural decision changes (with rationale).
