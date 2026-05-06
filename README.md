# TecnoFisc.Sped

Família de bibliotecas .NET para leitura, geração e manipulação tipada de arquivos
publicados pelos projetos do **SPED — Sistema Público de Escrituração Digital**
(Receita Federal do Brasil).

> Status: **em construção** — Stage 0 (fundação) concluído. Próxima etapa: registros
> e parser/gerador da EFD Contribuições, layout 006.

## Visão geral

A biblioteca expõe registros fortemente tipados para cada projeto SPED, com leitura
e escrita simétricas (round-trip preservado), parser baseado em
`System.IO.Pipelines` para arquivos de múltiplos gigabytes, e zero dependências
externas em tempo de execução.

Cada projeto SPED é distribuído como um pacote NuGet independente. Esta organização
mantém os layouts isolados — quando a Receita publica um novo layout, somente o
pacote afetado é versionado.

| Projeto SPED | Pacote NuGet | Status |
| --- | --- | --- |
| EFD Contribuições | `TecnoFisc.Sped.EfdContribuicoes` | esqueleto |
| EFD ICMS-IPI | `TecnoFisc.Sped.Fiscal` | planejado |
| NF-e / NFC-e / CT-e / MDF-e | `TecnoFisc.Sped.NFe`, etc. | planejado |
| eSocial / EFD-Reinf / ECD / ECF | pacotes próprios | planejado |

## Princípios

- **Auto-contido.** Sem banco de dados, sem arquivos de configuração externos,
  sem chamadas de rede. Streams entram, streams saem.
- **Independência de formato.** Projetos específicos nunca dependem uns dos outros —
  registros que parecem iguais (ex.: `RegistroC100` na EFD Contribuições e na
  ICMS-IPI) são classes distintas, propositalmente duplicadas.
- **Performance em primeiro lugar.** `PipeReader`, `ReadOnlySpan<byte>`,
  `Utf8Parser.TryParse`, catálogos gerados em tempo de compilação. Sem reflection
  no caminho quente.
- **Tipagem forte de ponta a ponta.** Consumidores nunca lidam com `string` ou
  `string[]` — recebem `Cnpj`, `Cfop`, `DateOnly`, `decimal`, enums.
- **Round-trip simétrico.** Ler → gerar → ler precisa devolver o mesmo arquivo
  (modulo normalizações deliberadas). Invariante coberta por testes.

## Arquivos assinados pelo PVA

Arquivos emitidos pelo PVA da Receita Federal trazem um bloco de assinatura
digital PKCS#7 anexado após o registro `|9999|`. O parser encerra o consumo
no `|9999|` e descarta silenciosamente todo o conteúdo posterior — não é
necessário pré-processar o arquivo para remover a assinatura. A geração não
re-anexa nenhuma assinatura: a saída contém apenas a porção textual de
registros. Quem precisar reassinar deve fazê-lo fora da biblioteca, com um
provedor PKCS#7/CMS dedicado.

## Requisitos

- .NET SDK **10.0** (preview) ou superior
- Windows, Linux ou macOS

## Build

```powershell
dotnet build  TecnoFisc.Sped.slnx
dotnet test   TecnoFisc.Sped.slnx
dotnet pack   TecnoFisc.Sped.slnx -c Release
```

Filtrar testes:

```powershell
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~Cnpj"
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~Cnpj.ValidaDigito"
```

## Estrutura do repositório

```text
TecnoFisc.Sped/
├── src/
│   ├── TecnoFisc.Sped.Core/                # Value objects fiscais + infraestrutura compartilhada
│   └── TecnoFisc.Sped.EfdContribuicoes/    # Layout EFD Contribuições (em construção)
├── tests/
│   └── TecnoFisc.Sped.Core.Tests/
├── sped/
│   ├── STAGE_4_REGISTROS.md                # Decomposição do Stage 4 em sub-stages
│   └── guides/                             # PDFs oficiais Receita Federal (gitignored)
│       └── Guia_Pratico_EFD_Contribuicoes_*.pdf
├── ARCHITECTURE.md                         # Documento mestre (inglês, para LLMs)
└── CLAUDE.md                               # Instruções para Claude Code
```

A árvore-alvo (com `benchmarks/`, `samples/` e demais pacotes) está descrita em
`ARCHITECTURE.md` §6.

## Convenções

- **Português** para conceitos SPED: classes de registro (`Registro0000`,
  `RegistroC100`), value objects fiscais (`Cnpj`, `Cfop`, `Ncm`),
  campos (`IndOper`, `VlDoc`), métodos de domínio (`LerArquivo`, `EscreverArquivo`).
- **Inglês** para infraestrutura técnica universal: `Parser`, `Generator`,
  `Reader`, `Writer`, `Builder`, tipos da BCL, palavras-chave de C#.
- Encoding dos `.txt` SPED: **Latin1 / Windows-1252**. UTF-8 apenas para os
  pacotes XML (família NF-e).
- Commits seguem [Conventional Commits](https://www.conventionalcommits.org/)
  (prefixo em inglês: `feat:`, `fix:`, `refactor:`...) com corpo em português.

Detalhes completos em `ARCHITECTURE.md` §1.3 e §13.

## Licença

[MIT](LICENSE) — © 2026 TecnoFisc Micro Sistemas.

## Contribuição

Repositório em fase inicial e mantido internamente. Issues e pull requests externos
ainda não são aceitos. Para colaboradores internos, leia primeiro
`ARCHITECTURE.md` (documento mestre) e `CLAUDE.md`.
