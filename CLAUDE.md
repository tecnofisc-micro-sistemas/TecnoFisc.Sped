# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Source of truth

`ARCHITECTURE.md` at the repo root is the master design document. Read it first every session. It defines naming rules, dependency rules, the staged development plan, and the public API shape. The notes below are operational shortcuts — they do not replace it.

`sped/STAGE_4_REGISTROS.md` is the operational appendix for Stage 4. It lists every EFD Contribuições registro as an atomic sub-stage `4.001` … `4.203`, with its PDF page number in the layout guide. Before starting work on a registro, look up its sub-stage row there and open the PDF directly at the listed page (do not read the whole guide).

## Build / test / run

Solution file is `TecnoFisc.Sped.slnx` (new SDK XML format). All projects target **.NET 10**.

```powershell
dotnet build  TecnoFisc.Sped.slnx
dotnet test   TecnoFisc.Sped.slnx
dotnet test   TecnoFisc.Sped.slnx --filter "FullyQualifiedName~Cnpj"        # single class
dotnet test   TecnoFisc.Sped.slnx --filter "FullyQualifiedName~Cnpj.ValidaDigito"  # single test
dotnet pack   TecnoFisc.Sped.slnx -c Release
```

Benchmarks (when the project exists):

```powershell
dotnet run -c Release --project benchmarks/TecnoFisc.Sped.Benchmarks
```

## Authoritative specs

PDFs in `sped/guides/` (gitignored, local only):

- `Guia_Pratico_EFD_Contribuicoes_Versao_1_35 - 18_06_2021.pdf` — EFD Contribuições layout guide (v1.35, layout 006). Source of truth para campos do `RegistroXxxx` deste leiaute.
- `Guia Prático EFD - Versão 3.2.2.pdf` — EFD ICMS-IPI guide (Ato COTEPE/ICMS no 44/2018). Source of truth para EFD ICMS-IPI **e** para tabelas/enums regidos pelo Ato COTEPE referenciados por outros leiautes (Tabela 4.1.1 Modelos, Tabela 4.1.2 Situação, etc.).

Both files are large (4–6 MB, hundreds of pages). Never read whole. Use `Read` tool's `pages` parameter to fetch only the section needed.

**Hierarquia de autoridade.** EFD ICMS-IPI é o **regente** do Ato COTEPE. Quando o guia EFD Contribuições dizer "conforme Tabela 4.1.1" / "conforme Tabela 4.1.2", a definição canônica está no guia do EFD ICMS-IPI. Mudanças na origem propagam para EFD Contribuições — não duplicar.

When the user upgrades to a newer layout, expect newer PDF dropped alongside the existing one in `sped/guides/` — keep both, do not delete.

## Repo folder map

- `sped/` — SPED-related operational docs and local-only specs.
  - `sped/STAGE_4_REGISTROS.md` — Stage 4 sub-stage tracking (EFD Contribuições).
  - `sped/guides/` — Receita Federal PDFs (gitignored, local only).
- `src/`, `tests/`, `benchmarks/`, `samples/` — target solution layout (per `ARCHITECTURE.md`).
- `scripts/` — automation (e.g., `auto-implement-sped.ps1`).

## Repository state

Current development has shipped (release `0.4.0`) EFD Contribuições V006 (read+write) and EFD ICMS-IPI V015 baseline + incrementos V016 → V020 (leiaute vigente em 2026, **read-only**). The numbers `006` (EFD Contribuições) and `015`–`020` (EFD ICMS-IPI) são `COD_VER` do registro `0000` — **não** confundir com versão do Guia Prático (1.35 do EFD Contribuições, 3.2.2 do EFD ICMS-IPI). Before starting new work, use `ARCHITECTURE.md` §12 and the tracking files under `sped/` to identify the next stage or increment. Do not jump ahead in the staged plan without an explicit ask.

## Hard rules (failing these blocks the change)

1. **No external runtime dependencies in any project.** No DB, no file-system config, no network calls. Streams in, streams out.
2. **Format-specific projects never reference each other** — duplication is correct **at the registro level**. `RegistroC100` em EFD Contribuições e EFD ICMS-IPI são duas classes distintas por design (filhos, hierarquia, validações cross-record divergem). **Exceção:** tabelas/enums regidos pelo Ato COTEPE/ICMS (Tabela 4.1.1 Modelos, Tabela 4.1.2 Situação, etc.) e value objects fiscais transversais (`Cnpj`, `Cfop`, `Ncm`, `ChaveAcesso`, indicadores convergidos como `IndicadorPagamento`/`IndicadorFrete`) ficam em `TecnoFisc.Sped.Core`. EFD ICMS-IPI é o regente do Ato COTEPE; outros leiautes referenciam. Duplicar enum referenciado = drift bug. Ver `ARCHITECTURE.md` §4.2.
3. **Source generator project is referenced as analyzer only:** `<ProjectReference OutputItemType="Analyzer" ReferenceOutputAssembly="false" />`. It must not ship at runtime.
4. **No reflection in parsing hot paths.** Reflection at startup (cached) is fine. `Activator.CreateInstance`, `PropertyInfo.SetValue` per-record is forbidden — use source-generated factory delegates (Stage 6) or the cached `CatalogoBuilder` fallback (Stage 2).
5. **Performance-sensitive code requires a BenchmarkDotNet benchmark.** Performance regressions block merge.
6. **Janela fiscal de 5 anos.** Receita só permite revisão dos últimos 5 anos. Marcos de versionamento de campos com vigência anterior ao corte (hoje, anteriores a 2021-01) **não são modelados** em código. Versões antigas de enums (`IND_PGTO` pré-2012-07, `IND_FRT` pré-2017-10/2018-01, etc.) ficam de fora — só a versão vigente no corte e a evolução posterior contam. Ver `ARCHITECTURE.md` §4.3.
7. **Merges em `dev` são sempre Squash and Merge.** Branches de trabalho podem ter commits granulares durante a implementação, mas o merge para `dev` deve entrar como um único commit squashed do PR.

## Naming convention (CRITICAL — see ARCHITECTURE.md §1.3)

Substantivos do domínio SPED em **português**; verbos, factories estáticos e predicados booleanos em **inglês idiomático**. A divisão é por **significado, não por tipo de identificador** — vale para **nomes de classe, métodos, propriedades, campos e variáveis locais**.

- **Portuguese (mandatory)** for SPED **nouns**: record classes (`Registro0000`, `RegistroC100`), fiscal value objects (`Cnpj`, `Cfop`, `Ncm`, `ChaveAcesso`), fiscal enums (`IndicadorOperacao`, `ModeloDocumento`), SPED field properties (`IndOper`, `CodPart`, `VlDoc`, `CodVer`), namespaces and top-level domain types (`ArquivoEfdContribuicoes`, `BlocoC`).
- **English (mandatory)** for **verbs and predicates** (incl. private): factory methods (`Cnpj.Create(...)`), I/O verbs (`parser.ReadAsync`, `parser.ReadStreamingAsync`, `gerador.WriteAsync`, `arquivo.LoadAsync`), boolean predicates (`Cfop.IsEntrada`, `InscricaoEstadual.IsIsento`, `CodigosUf.IsValid`, `IsKnownValueObject`, `HasFilter`, `ShouldIgnore`), and **classes that name a capability / option / technical pattern** (`ReadingOptions`, `Parser`, `Generator`, `Reader`, `Writer`, `Builder`, `*Tests`, `Should_*`, `Stream`, `Pipeline`, `Buffer`, BCL types).
- The two languages never mix **inside a single identifier**. Example: `Cfop.IsEntrada` (noun PT + verb EN); `parser.ReadStreamingAsync(stream)`; `Cnpj.Create("12345678000195")`.
- **Proibido** (verbo/predicado em PT, qualquer tipo de identificador): `Criar`, `LerAsync`, `EhEntrada`, `TemFiltro` (→ `HasFilter`), `DeveIgnorar` (→ `ShouldIgnore`), `Filtrar` (→ `Filter`).

## Code conventions worth remembering

- Sealed classes by default. `partial` on classes the source generator extends.
- Value objects: immutable struct/sealed class, private ctor + static `Create`, value equality, `ToString()` returns canonical SPED form.
- `Result<T>` for expected parser failures; exceptions for programmer errors.
- All I/O `async` with `ConfigureAwait(false)` (this is a library).
- File-scoped namespaces.
- Encoding for `.txt` SPED files: **Latin1 / Windows-1252**. UTF-8 only for the XML projects (NF-e family).
- Parsing uses `PipeReader` + `ReadOnlySpan<byte>` + `Utf8Parser.TryParse`. No `StreamReader.ReadLine` allocating strings per record.
- Round-trip invariant: parse → generate → parse must equal the original (modulo deliberate normalization). Cover with property-based or fixture round-trip tests for every record type.

## Commits

Conventional Commits prefixes in English (`feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `perf:`), message body in Portuguese. Branches podem ter commits granulares, um por ideia coesa. Ao integrar em `dev`, usar sempre Squash and Merge.

## Release flow (CRITICAL)

Publicação em nuget.org é automática no merge para `main`. Sequência canônica:

1. Trabalho de feature/fix: branch curta → PR para `dev` → **Squash and Merge** (regra padrão acima).
2. Quando acumular escopo suficiente para release (ou quando o usuário pedir explicitamente), preparar o release ainda em `dev`:
   - Bumpar `Directory.Build.props` `<Version>` para a próxima versão SemVer.
   - Consolidar `CHANGELOG.md` em `[X.Y.Z] — yyyy-mm-dd`.
   - Atualizar `README.md` status quando o conteúdo público mudar.
3. Abrir PR `dev` → `main`. Estratégia: **Merge commit** (preserva history dos commits granulares). Não usar squash — main precisa ver os commits individuais para auditoria.
4. Merge em `main` dispara `.github/workflows/release.yml`, que faz build/test/pack, valida que `vX.Y.Z` ainda não existe, valida que os pacotes `X.Y.Z` ainda não existem no nuget.org, publica os `.nupkg`, cria a tag `vX.Y.Z` no commit de merge e cria a GitHub Release.
5. Continuar trabalho em `dev` para a próxima release.

Não criar tags manualmente para releases normais. A tag é saída do workflow de release, não entrada. Se uma publicação parcial ocorrer, NuGet é imutável: corrigir em nova versão patch, nunca tentar republicar o mesmo `X.Y.Z`.

## Documentation language

- `ARCHITECTURE.md` and other architecture docs: English (LLM consistency).
- `README.md` for end users: Portuguese.
- Code comments: Portuguese for fiscal/format explanations, English for technical notes.

## When the architecture doc and reality disagree

The doc describes the target. The repo is mid-bootstrap. If a path or project named in `ARCHITECTURE.md` does not exist yet, that is expected — don't invent it just to satisfy the doc. Either the user is asking you to create it now, or it belongs to a later stage. Ask if unclear.
