# Changelog

Todas as mudanças relevantes deste repositório são documentadas neste arquivo.

O formato segue [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/) e o projeto adota [Semantic Versioning](https://semver.org/lang/pt-BR/). Cada pacote NuGet possui versão independente; as seções abaixo agrupam as mudanças por release do repositório.

## [Não publicado]

## [0.1.0] — 2026-05-06

Release inicial. Conclui a Stage 4 de `ARCHITECTURE.md`: implementação completa do leiaute V006 da EFD Contribuições (Guia Prático v1.35) com parser e gerador capazes de fazer round-trip de um arquivo real anonimizado.

### TecnoFisc.Sped.Core 0.1.0

#### Adicionado

- Abstrações base: `RegistroSped`, `IArquivoSped`, `IBlocoSped`, `ILeitorSped`, `IEscritorSped`, `IRegistroSpedCatalogo`.
- Atributos de metadados: `[RegistroSped]`, `[CampoSped]`, `[BlocoSped]`.
- Catálogo dinâmico de registros (`CatalogoBuilder`, `CatalogoSpedBase`, `MetadadosRegistro`, `MetadadosCampo`) com cache via reflexão em startup — sem reflexão no caminho quente. O source generator (Stage 6) substituirá esta camada mantendo a API.
- Parser binário baseado em `PipeReader` + `Utf8Parser` (`LeitorSpedTxt`, `PilhaHierarquica`, `ParseadoresPrimitivos`, `EncodingSped` com Latin1/Windows-1252).
- Gerador binário (`EscritorSpedTxt`, `SerializadoresPrimitivos`, `TotalizadorBlocos`).
- Tipo `ResultadoParse` e hierarquia `ErroFormato` / `ErroLayout` para falhas esperadas; exceções reservadas para erros de programador.
- Value objects fiscais transversais com validação de dígito verificador e formatação canônica: `Cnpj`, `Cpf`, `Cfop`, `Ncm`, `Cst`, `ChaveAcesso`, `InscricaoEstadual`, `CodigosUf`, `TipoTributo`, `GeneroItem`, `ModeloDocumento` (Tabela 4.1.1, regida pelo Ato COTEPE/ICMS).
- Enums transversais regidos pelo Ato COTEPE/ICMS: `CodigoSituacaoDocumentoFiscal` (Tabela 4.1.2), `IndicadorApuracaoIpi`, `IndicadorMovimentacaoFisica`.

### TecnoFisc.Sped.EfdContribuicoes 0.1.0

#### Adicionado

- 203 classes de registro cobrindo todo o leiaute V006:
  - **Bloco 0** (abertura, identificação, tabelas): `0000`, `0001`, `0035`, `0100`, `0110`, `0111`, `0120`, `0140`, `0145`, `0150`, `0190`, `0200`, `0205`, `0206`, `0208`, `0400`, `0450`, `0500`, `0600`, `0900`, `0990`.
  - **Bloco A** (serviços): `A001`, `A010`, `A100`, `A110`, `A111`, `A120`, `A170`, `A990`.
  - **Bloco C** (documentos fiscais — mercadorias): `C001`, `C010`, `C100`, `C110`, `C111`, `C120`, `C170`, `C175`, `C180`, `C181`, `C185`, `C188`, `C190`, `C191`, `C195`, `C198`, `C199`, `C380`, `C381`, `C385`, `C395`, `C396`, `C400`, `C405`, `C481`, `C485`, `C489`, `C490`, `C491`, `C495`, `C499`, `C500`, `C501`, `C505`, `C509`, `C600`, `C601`, `C605`, `C609`, `C800`, `C810`, `C820`, `C830`, `C860`, e demais conforme `sped/STAGE_4_REGISTROS.md`.
  - **Bloco D** (serviços de comunicação/transporte): conjunto completo, incluindo registros referenciados.
  - **Bloco F** (demais documentos e operações): conjunto completo.
  - **Bloco I** (operações de instituições financeiras e seguros): conjunto completo.
  - **Bloco M** (apuração da contribuição e do crédito): conjunto completo, incluindo detalhamentos por CST.
  - **Bloco P** (apuração da contribuição previdenciária sobre receita bruta): conjunto completo.
  - **Bloco 1** (complemento da escrituração): `1001`, `1010`, `1011`, `1020`, `1050`, `1100`, `1101`, `1102`, `1200`, `1210`, `1220`, `1300`, `1500`, `1501`, `1502`, `1600`, `1610`, `1620`, `1700`, `1800`, `1809`, `1900`, `1990`.
  - **Bloco 9** (controle e encerramento): `9001`, `9900`, `9990`, `9999`.
- `ParserEfdContribuicoes` — leitura de arquivo `.txt` em Latin1/Windows-1252 com construção da árvore hierárquica e detecção de erros de formato e leiaute.
- `GeradorEfdContribuicoes` — escrita de arquivo `.txt` com totalizadores por bloco (`9900`) e contagem global (`9999`) calculados automaticamente.
- `ArquivoEfdContribuicoes` e `BlocoEfdContribuicoes` — modelo em memória para manipulação após o parse e antes da geração.
- Round-trip end-to-end (`parse → generate → parse`) validado contra fixture real anonimizada quando disponível em `sped/fixtures/` (PR #106, #107).

### Notas de release

- Distribuição: ainda não publicado em feed NuGet remoto. Pacotes `.nupkg` são gerados pelo CI (`pack` job em push para `main`) e disponibilizados como artefato. A escolha entre Azure Artifacts e GitHub Packages permanece aberta (`ARCHITECTURE.md` §15).
- Performance: parser e gerador implementados sobre `PipeReader` + `ReadOnlySpan<byte>`; benchmarks dedicados (`benchmarks/TecnoFisc.Sped.Benchmarks`) entram na Stage 6 junto com o source generator.
- API streaming (`IAsyncEnumerable<RegistroSped>`) é objetivo da Stage 5 e não está disponível neste release.
- Suporte a leiautes mais novos (V007+) entra na Stage 7.

[Não publicado]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/compare/v0.1.0...HEAD
[0.1.0]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/releases/tag/v0.1.0
