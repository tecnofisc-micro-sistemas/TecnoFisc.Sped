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

## Authoritative spec for EFD Contribuições

`sped/guides/Guia_Pratico_EFD_Contribuicoes_Versao_1_35 - 18_06_2021.pdf` is the Receita Federal layout guide (v1.35, layout 006). Use it as the source of truth when implementing fields on records (`RegistroXxxx`): order, type, length, decimals, optionality, valid value lists.

It is large (~4 MB, hundreds of pages). Never read the whole file. Use the `Read` tool's `pages` parameter to fetch only the section for the record you are implementing (e.g., `pages: "120-125"` for one record). The TOC near the start lists each registro and its page.

The `sped/guides/` folder is gitignored — PDFs live there locally only. When the user upgrades to a newer layout (v007+), expect a newer PDF dropped alongside this one in `sped/guides/` — keep both, do not delete.

## Repo folder map

- `sped/` — SPED-related operational docs and local-only specs.
  - `sped/STAGE_4_REGISTROS.md` — Stage 4 sub-stage tracking (EFD Contribuições).
  - `sped/guides/` — Receita Federal PDFs (gitignored, local only).
- `src/`, `tests/`, `benchmarks/`, `samples/` — target solution layout (per `ARCHITECTURE.md`).
- `scripts/` — automation (e.g., `auto-implement-sped.ps1`).

## Repository state

Skeleton only. Two empty SDK-style projects exist: `TecnoFisc.Sped.Core` and `TecnoFisc.Sped.EfdContribuicoes`. No source files, no tests, no `src/`/`tests/` layout yet — the `ARCHITECTURE.md` solution tree describes the **target** structure, not the current one. When adding the first real code, mirror the target tree (move projects under `src/`, add `tests/`, `benchmarks/`, `samples/`).

The current development stage is **Stage 0 → Stage 1** (foundation + core value objects). Do not jump ahead in the staged plan (see ARCHITECTURE.md §12) without an explicit ask.

## Hard rules (failing these blocks the change)

1. **No external runtime dependencies in any project.** No DB, no file-system config, no network calls. Streams in, streams out.
2. **Format-specific projects never reference each other.** `RegistroC100` in EFD Contribuições and EFD ICMS-IPI are two different classes by design — duplication is correct.
3. **Source generator project is referenced as analyzer only:** `<ProjectReference OutputItemType="Analyzer" ReferenceOutputAssembly="false" />`. It must not ship at runtime.
4. **No reflection in parsing hot paths.** Reflection at startup (cached) is fine. `Activator.CreateInstance`, `PropertyInfo.SetValue` per-record is forbidden — use source-generated factory delegates (Stage 6) or the cached `CatalogoBuilder` fallback (Stage 2).
5. **Performance-sensitive code requires a BenchmarkDotNet benchmark.** Performance regressions block merge.

## Naming convention (CRITICAL — see ARCHITECTURE.md §1.3)

- **Portuguese** for SPED domain: record classes (`Registro0000`, `RegistroC100`), fiscal value objects (`Cnpj`, `Cfop`, `Ncm`, `ChaveAcesso`), enums of fiscal concepts (`IndicadorOperacao`), SPED field properties (`IndOper`, `CodPart`, `VlDoc`), domain methods (`LerArquivo`, `EscreverArquivo`).
- **English** for technical infra: BCL types, `Parser`/`Generator`/`Reader`/`Writer`/`Builder`, test class names (`*Tests`, `Should_*`), `Stream`, `Pipeline`, `Buffer`.
- Do not mix the two within one logical layer.

## Code conventions worth remembering

- Sealed classes by default. `partial` on classes the source generator extends.
- Value objects: immutable struct/sealed class, private ctor + static `Criar`, value equality, `ToString()` returns canonical SPED form.
- `Result<T>` for expected parser failures; exceptions for programmer errors.
- All I/O `async` with `ConfigureAwait(false)` (this is a library).
- File-scoped namespaces.
- Encoding for `.txt` SPED files: **Latin1 / Windows-1252**. UTF-8 only for the XML projects (NF-e family).
- Parsing uses `PipeReader` + `ReadOnlySpan<byte>` + `Utf8Parser.TryParse`. No `StreamReader.ReadLine` allocating strings per record.
- Round-trip invariant: parse → generate → parse must equal the original (modulo deliberate normalization). Cover with property-based or fixture round-trip tests for every record type.

## Commits

Conventional Commits prefixes in English (`feat:`, `fix:`, `refactor:`, `test:`, `docs:`, `perf:`), message body in Portuguese. One commit per cohesive idea.

## Documentation language

- `ARCHITECTURE.md` and other architecture docs: English (LLM consistency).
- `README.md` for end users: Portuguese.
- Code comments: Portuguese for fiscal/format explanations, English for technical notes.

## When the architecture doc and reality disagree

The doc describes the target. The repo is mid-bootstrap. If a path or project named in `ARCHITECTURE.md` does not exist yet, that is expected — don't invent it just to satisfy the doc. Either the user is asking you to create it now, or it belongs to a later stage. Ask if unclear.
