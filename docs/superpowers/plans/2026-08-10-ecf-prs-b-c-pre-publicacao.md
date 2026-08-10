# ECF — PRs B e C antes da publicação: Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Fechar os dois PRs que o design do PR #531 deixou pendentes antes da publicação da ECF — o contrato de diagnóstico (`ColunasNaoModeladas`, motivo da sentinela, `VersaoDoArquivo`) e a limpeza do modelo raiz (`ArquivoSpedBase`).

**Architecture:** Um mecanismo novo e correções pontuais. `RegistroSped` ganha duas coleções/propriedades (`ColunasNaoModeladas`, `VersaoDoArquivo`) no mesmo padrão preguiçoso de `_errosDeFormato`; `LeitorSpedTxt` passa a preencher o ramo `else` que hoje descarta colunas em silêncio; `RegistroNaoReconhecido` ganha um discriminador de origem. Depois, os quatro modelos raiz (`ArquivoEcf`, `ArquivoEcd`, `ArquivoEfdContribuicoes`, `ArquivoEfdIcmsIpi`) passam a herdar de uma base genérica em `TecnoFisc.Sped.Txt.Engine`, sem mudança de comportamento.

**Tech Stack:** .NET 10, C# file-scoped namespaces, xUnit v3 + FluentAssertions, BenchmarkDotNet, `TecnoFisc.Sped.slnx`.

## Global Constraints

- **Sem dependência externa de runtime** em qualquer projeto: sem DB, sem file-system config, sem rede. Streams entram, streams saem.
- **Projetos de leiaute nunca se referenciam.** O código compartilhado desce para `TecnoFisc.Sped.Txt.Engine` / `TecnoFisc.Sped.Core`; duplicação no nível de registro é correta por design.
- **Sem reflexão em caminho quente de parsing.** `Activator.CreateInstance`/`PropertyInfo.SetValue` por registro é proibido.
- **Código sensível a performance exige benchmark BenchmarkDotNet** (Hard rule 5). A Task 3 é a contrapartida obrigatória da Task 2.
- **Naming:** substantivos do domínio SPED em português (`ColunaNaoModelada`, `MotivoColunaNaoModelada`, `MotivoNaoReconhecimento`, `VersaoDoArquivo`); verbos, predicados e nomes de capacidade em inglês. Os dois idiomas nunca se misturam dentro do mesmo identificador. **Exceção local, deliberada:** o mutador `RegistrarColunaNaoModelada` acompanha o `RegistrarErroDeFormato` que já existe imediatamente acima dele em `RegistroSped`, junto de `AdicionarFilho` — trocar o idioma só no membro novo criaria um par assimétrico no mesmo arquivo. Mesma razão para `AdicionarAoBloco` e `PreencherAsync` na Task 7, que espelham o `Adicionar`/`EnumerarBlocos` já públicos nos quatro modelos raiz.
- **Classes `sealed` por padrão;** `partial` só nas que o source generator estende. `ArquivoSpedBase` é a exceção necessária (abstrata).
- **`Result<T>` para falha esperada de parser; exceção para erro de programação.** I/O `async` com `ConfigureAwait(false)`.
- **Encoding dos `.txt`: Latin1 / Windows-1252.**
- **Conventional Commits são load-bearing** (Hard rule 8): cada commit e cada título de PR é um Conventional Commit válido, tipo em inglês minúsculo, descrição em inglês imperativo. Corpo do commit em português.
- **Merge em `dev` é sempre Squash and Merge**; o título do PR é a unidade que o semantic-release analisa.
- **Documentação:** `ARCHITECTURE.md` em inglês; `README.md` e `CHANGELOG.md` em português; comentários de código em português para explicação fiscal, inglês para nota técnica.

---

## Contexto: o que ficou pendente e por quê

O PR #531 (`feat(ecf)!: add read-only ECF layout 12 model with layouts 8-12 reading`, mergeado em `dev` em 2026-08-08) fechou seis dos dez achados do review de follow-up. O design aprovado (`docs/superpowers/specs/2026-08-08-pr531-achados-followup-design.md`) distribuiu o resto em dois PRs a serem feitos **antes da publicação**:

| PR | Achados | Natureza |
|---|---|---|
| **B** | 2, 8, 9 | Contrato de diagnóstico. Muda `RegistroSped`, afeta os quatro leiautes. |
| **C** | 10 + parked 1, 3 | Limpeza. Sem mudança de comportamento. |

O que hoje está errado, em uma frase cada:

1. **Achado 2 — coluna descartada em silêncio.** `LeitorSpedTxt.InterpretarLinha` ignora toda coluna além do último campo declarado (`LeitorSpedTxt.cs:743-744`, um comentário onde deveria haver um `else`). Consequência registrada no `CHANGELOG.md:28` como limitação conhecida: um `RegistroX300` de arquivo histórico chega **sem nenhuma propriedade e sem a linha crua** — quem lia com `LenientLayout = true` tinha a linha em `RegistroNaoReconhecido.LinhaCrua` e perdeu isso.
2. **Achado 8 — campo barrado por vigência some sem sinal.** Mesmo ponto do código: um campo cujo `DesdeVersao` é posterior ao `COD_VER` do arquivo simplesmente não recebe valor, e o valor presente na linha é jogado fora.
3. **Achado 9 — sentinela sem discriminador.** `RegistroNaoReconhecido` é produzido por duas origens (código desconhecido; registro fora de vigência) e a única forma de distingui-las é casar substring na mensagem em português de `Erro.Mensagem`.
4. **Parked (achado 4 do review anterior) — aliases do `0020`.** `IndPrTransf` e `PossuiCebras` foram removidos no PR A porque não sabiam a versão do arquivo. Voltam guardados assim que o registro souber em que leiaute foi lido.
5. **Achado 10 — modelo raiz quadruplicado.** `ArquivoEcf`, `ArquivoEcd`, `ArquivoEfdContribuicoes` e `ArquivoEfdIcmsIpi` têm o mesmo dicionário de blocos, a mesma lista `_naoReconhecidos`, o mesmo roteamento de `Adicionar` e as mesmas duas enumerações.
6. **Parked 1 e 3.** Um teste de defesa em profundidade sobre `RegistrosIgnorados` com registro filho, e a ordem das subseções do `CHANGELOG` entre pacotes.

### Divergências entre o plano de 2026-08-08 e o `dev` de hoje

O plano anterior (`docs/superpowers/plans/2026-08-08-pr531-achados-followup.md`, tasks 12–18) foi escrito **antes** da execução do PR A. Este plano o substitui para as tasks pendentes, com quatro correções verificadas contra o código de `dev`:

- **Helper de teste renomeado.** `LeiauteForaDaFaixaTests.ReadAsync` não existe mais como helper compartilhado; o commit `00294a0` consolidou os quatro consumidores em `FixtureEcf.ReadAsync` (`tests/TecnoFisc.Sped.Ecf.Tests/Versionamento/FixtureEcf.cs`). Todos os testes abaixo usam `FixtureEcf`.
- **Sem `InternalsVisibleTo` no engine.** Só `TecnoFisc.Sped.Ecf.csproj:21` declara um. `TecnoFisc.Sped.Txt.Engine` **não** expõe internals ao seu projeto de teste, e este plano **não** vai abrir essa porta: a acumulação de `ColunasNaoModeladas` é coberta pelo leitor (Task 2), não por chamada direta ao membro `internal`.
- **`VersaoDoArquivo` precisa de dois pontos de atribuição, não um.** O `0000` é interpretado **antes** de o leitor conhecer a versão (é ele quem a carrega), então atribuir só dentro de `InterpretarLinha` deixaria o próprio `0000` com `VersaoDoArquivo == 0`. A Task 5 cobre os dois pontos.
- **Benchmark em arquivo próprio.** `ParserVigenciaBenchmark` tem um `[GlobalSetup]` sem `Targets`; acrescentar casos ali obrigaria a anotar `Targets` em tudo. A Task 3 cria `ColunasNaoModeladasBenchmark.cs` ao lado, sem tocar no que já mede vigência.

### Explicitamente fora de escopo

Modelar os campos dos sete registros descontinuados (`X291`, `X300`, `X305`, `X310`, `X320`, `X325`, `X330`) e as colunas antigas do `X450` com `[CampoSped]`. O design descartou isso por custo (`§ Desvio consciente de ARCHITECTURE §4.7`): os manuais dos leiautes 8–11 estão em `sped/guides/`, mas sem índice por registro, e esses registros não têm relevância no contexto atual da TecnoFisc. Depois deste plano a migração continua **puramente aditiva** — e, com `ColunasNaoModeladas` entregue, o conteúdo dessas colunas deixa de se perder enquanto ela não chega.

---

## PR B — contrato de diagnóstico

Branch a partir de `dev`. Título do PR (é o que o semantic-release analisa): `feat(txt)!: expose unmodeled columns and discriminate sentinel origin`.

### Task 1: `ColunaNaoModelada` e a coleção no registro base

**Files:**
- Create: `src/TecnoFisc.Sped.Txt.Engine/Abstracoes/ColunaNaoModelada.cs`
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Abstracoes/RegistroSped.cs:10-24`
- Test: `tests/TecnoFisc.Sped.Txt.Engine.Tests/Abstracoes/RegistroSpedTests.cs` (arquivo e pasta novos)

**Interfaces:**
- Produces: `MotivoColunaNaoModelada` (enum: `AlemDoModelo = 0`, `PosteriorAVersaoDeclarada = 1`), `readonly record struct ColunaNaoModelada(int Posicao, string Valor, MotivoColunaNaoModelada Motivo)`, `RegistroSped.ColunasNaoModeladas` (`IReadOnlyList<ColunaNaoModelada>`), `RegistroSped.RegistrarColunaNaoModelada(ColunaNaoModelada)` (`internal`).

- [ ] **Step 1: Escrever o teste que falha**

Criar `tests/TecnoFisc.Sped.Txt.Engine.Tests/Abstracoes/RegistroSpedTests.cs`:

```csharp
using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Txt.Engine.Tests.Abstracoes;

public sealed class RegistroSpedTests
{
    [Fact]
    public void ColunasNaoModeladas_EhVaziaPorPadrao()
        => new RegistroDeTeste().ColunasNaoModeladas.Should().BeEmpty();

    private sealed class RegistroDeTeste : RegistroSped
    {
        public override string Codigo => "TST2";
    }
}
```

`FluentAssertions` e `Xunit` já são `global using` neste projeto de teste (`tests/TecnoFisc.Sped.Txt.Engine.Tests/GlobalUsings.cs`) — não repetir os `using`.

- [ ] **Step 2: Rodar e confirmar que falha**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~RegistroSpedTests"`
Expected: FAIL na compilação — `ColunasNaoModeladas` não existe em `RegistroSped`.

- [ ] **Step 3: Criar o tipo**

`src/TecnoFisc.Sped.Txt.Engine/Abstracoes/ColunaNaoModelada.cs`:

```csharp
namespace TecnoFisc.Sped.Txt.Engine.Abstracoes;

/// <summary>Por que uma coluna presente na linha não virou propriedade do registro.</summary>
public enum MotivoColunaNaoModelada
{
    /// <summary>
    /// A coluna vem depois do último campo declarado no catálogo — leiaute mais novo que o
    /// modelado, ou registro reconhecido sem nenhum campo modelado (ARCHITECTURE §4.7).
    /// </summary>
    AlemDoModelo = 0,

    /// <summary>
    /// O campo existe no catálogo mas foi introduzido em versão posterior à declarada no
    /// <c>0000</c>, então não vigorava no arquivo lido.
    /// </summary>
    PosteriorAVersaoDeclarada = 1,
}

/// <summary>
/// Coluna presente na linha SPED que o modelo tipado não representa. Preserva o valor em bruto
/// para que nenhum dado do arquivo se perca em silêncio.
/// </summary>
/// <param name="Posicao">
/// Posição na nomenclatura do Guia Prático — a mesma numeração de <c>CampoSpedAttribute.Ordem</c>:
/// <c>1</c> é o próprio <c>REG</c> e os campos do leiaute começam em <c>2</c>.
/// </param>
/// <param name="Valor">Conteúdo da coluna, verbatim, sem trim nem conversão.</param>
/// <param name="Motivo">Por que a coluna não foi materializada.</param>
public readonly record struct ColunaNaoModelada(
    int Posicao, string Valor, MotivoColunaNaoModelada Motivo);
```

Em `src/TecnoFisc.Sped.Txt.Engine/Abstracoes/RegistroSped.cs`, logo abaixo de `RegistrarErroDeFormato` (linha 24), espelhando o mesmo padrão preguiçoso:

```csharp
    private List<ColunaNaoModelada>? _colunasNaoModeladas;

    /// <summary>
    /// Colunas presentes na linha que o modelo tipado não representa — coluna além do último
    /// campo declarado, ou campo cuja vigência é posterior ao <c>COD_VER</c> do arquivo. Vazia no
    /// caso comum, que é o de um arquivo do leiaute modelado. O valor fica em bruto: nada do
    /// arquivo se perde em silêncio, mesmo onde a biblioteca não sabe interpretar.
    /// </summary>
    public IReadOnlyList<ColunaNaoModelada> ColunasNaoModeladas
        => _colunasNaoModeladas ?? (IReadOnlyList<ColunaNaoModelada>)[];

    internal void RegistrarColunaNaoModelada(ColunaNaoModelada coluna)
        => (_colunasNaoModeladas ??= []).Add(coluna);
```

Declarar o campo `_colunasNaoModeladas` junto de `_errosDeFormato` (linha 13) se preferir manter os campos agrupados no topo — o importante é não criar um segundo padrão de alocação.

- [ ] **Step 4: Rodar e confirmar que passa**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~RegistroSpedTests"`
Expected: PASS.

- [ ] **Step 5: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine/Abstracoes tests/TecnoFisc.Sped.Txt.Engine.Tests/Abstracoes
git commit -m "feat(txt): add ColunasNaoModeladas to the base record"
```

---

### Task 2: Captura no leitor

**Files:**
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs:704-745`
- Test: `tests/TecnoFisc.Sped.Ecf.Tests/Parser/ColunasNaoModeladasTests.cs` (novo)

**Interfaces:**
- Consumes: `RegistroSped.RegistrarColunaNaoModelada`, `ColunaNaoModelada`, `MotivoColunaNaoModelada` (Task 1).

- [ ] **Step 1: Medir o baseline de performance ANTES de tocar o leitor**

Run: `dotnet run -c Release --project benchmarks/TecnoFisc.Sped.Benchmarks --filter "*ParserVigenciaBenchmark*"`
Anotar as médias absolutas de `SemVigencia` e `ComVigencia` e a alocação de cada um. Esses números são o baseline pré-mudança comparado na Task 3 — depois de implementar não há como obtê-los sem `git stash`.

- [ ] **Step 2: Escrever o teste que falha**

Criar `tests/TecnoFisc.Sped.Ecf.Tests/Parser/ColunasNaoModeladasTests.cs`:

```csharp
using TecnoFisc.Sped.Ecf.Registros.BlocoX;
using TecnoFisc.Sped.Ecf.Tests.Versionamento;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Ecf.Tests.Parser;

/// <summary>
/// Prova o contrato "nada se perde em silêncio": toda coluna presente na linha que o modelo
/// tipado não representa chega ao consumidor em bruto, com o motivo.
/// </summary>
public sealed class ColunasNaoModeladasTests
{
    [Fact]
    public async Task X450DeLeiaute10_PreservaAsColunasDeDetalheEmBruto()
    {
        var registros = await FixtureEcf.ReadAsync(
            10, "|X450|249|11222333000181|BENEFICIARIO|1234,56|01|N|");

        var x450 = registros.OfType<RegistroX450>().Single();
        x450.Pais.Should().Be("249");
        x450.ColunasNaoModeladas.Select(coluna => (coluna.Posicao, coluna.Valor)).Should().Equal(
            (3, "11222333000181"), (4, "BENEFICIARIO"), (5, "1234,56"), (6, "01"), (7, "N"));
        x450.ColunasNaoModeladas.Should().OnlyContain(
            coluna => coluna.Motivo == MotivoColunaNaoModelada.AlemDoModelo);
    }

    [Fact]
    public async Task RegistroRemovidoNoLeiaute11_PreservaTodasAsColunas()
    {
        var registros = await FixtureEcf.ReadAsync(10, "|X300|000001|EXPORTACAO|1234,56|");

        var x300 = registros.Single(registro => registro.Codigo == "X300");
        x300.ColunasNaoModeladas.Select(coluna => coluna.Valor)
            .Should().Equal("000001", "EXPORTACAO", "1234,56");
        x300.ColunasNaoModeladas.Should().OnlyContain(
            coluna => coluna.Motivo == MotivoColunaNaoModelada.AlemDoModelo);
    }

    [Fact]
    public async Task CampoPosteriorAoLeiauteDeclarado_ViraColunaNaoModelada()
    {
        // 0020 tem 31 colunas de dado (Ordem 2..32). As duas últimas — POSSUI_CEBRAS (Ordem 31,
        // DesdeVersao 10) e CEBAS (Ordem 32, DesdeVersao 12) — não vigoram no leiaute 9.
        var valores = new List<string> { "1", "1" };
        valores.AddRange(Enumerable.Repeat("N", 27));
        valores.Add("S");
        valores.Add("CEBAS-TESTE");
        string linha = "|0020|" + string.Join('|', valores) + "|";

        var registros = await FixtureEcf.ReadAsync(9, linha);

        var registro0020 = registros.Single(registro => registro.Codigo == "0020");
        registro0020.ColunasNaoModeladas.Should().BeEquivalentTo([
            new ColunaNaoModelada(31, "S", MotivoColunaNaoModelada.PosteriorAVersaoDeclarada),
            new ColunaNaoModelada(32, "CEBAS-TESTE", MotivoColunaNaoModelada.PosteriorAVersaoDeclarada),
        ]);
    }

    [Fact]
    public async Task LinhaSemExcedente_NaoRegistraNada()
    {
        var registros = await FixtureEcf.ReadAsync(12, "|0001|0|");

        registros.Should().OnlyContain(registro => registro.ColunasNaoModeladas.Count == 0);
    }
}
```

- [ ] **Step 3: Rodar e confirmar que falha**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~ColunasNaoModeladasTests"`
Expected: FAIL — os três primeiros casos acusam coleção vazia; as colunas são descartadas hoje.

- [ ] **Step 4: Implementar o `else`**

Em `LeitorSpedTxt.InterpretarLinha`, o bloco `else if (metadados is not null && registro is not null)` (linha 704) termina hoje com um `if` sem `else` e um comentário nas linhas 743-744:

```csharp
                    Definir(campo, fatia);
                }
                // Campos posteriores ao último declarado são ignorados — layouts novos
                // podem adicionar colunas no fim sem quebrar leitores antigos.
```

Substituir o comentário por um `else` real:

```csharp
                    Definir(campo, fatia);
                }
                else
                {
                    // Nada do arquivo se perde em silêncio: a coluna existe na linha e não tem
                    // propriedade que a receba. Ou vem depois do último campo declarado — leiaute
                    // mais novo que o modelado, ou registro reconhecido sem campos modelados
                    // (ARCHITECTURE §4.7) —, ou o campo só vigora a partir de versão posterior à
                    // declarada no 0000.
                    // Zero cost on the happy path: the condition was already evaluated; only the
                    // branch that did nothing now does something.
                    var motivo = indice < metadados.Campos.Count
                        ? MotivoColunaNaoModelada.PosteriorAVersaoDeclarada
                        : MotivoColunaNaoModelada.AlemDoModelo;
                    registro.RegistrarColunaNaoModelada(
                        new ColunaNaoModelada(posicaoCampo, fatia.ToString(), motivo));
                }
```

Duas notas para quem revisar, ambas comportamento aceito e não bug:

1. Um campo `CapturaTudo`/`CampoArquivo` **inativo por vigência** cai neste `else` e não consome o resto da linha, então cada trecho separado por `|` vira uma `ColunaNaoModelada` própria. É a leitura honesta: sem o campo ativo, a biblioteca não sabe onde o valor variádico termina.
2. `posicaoCampo` já é a numeração do Guia (1 = `REG`), idêntica a `CampoSpedAttribute.Ordem` — confirmado em `Registro0020`, cujo primeiro campo declara `Ordem = 2`. Nenhuma conversão é necessária.

- [ ] **Step 5: Rodar e confirmar que passa**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~ColunasNaoModeladasTests"`
Expected: PASS nos quatro casos.

- [ ] **Step 6: Rodar a suíte inteira**

Run: `dotnet test TecnoFisc.Sped.slnx`
Expected: PASS. Atenção especial a `ParserEcfFixtureAnonimizadaTests` e `CompatibilidadeLayoutEcfTests`: se algum deles quebrar, a fixture tem coluna excedente que antes passava despercebida — investigar a fixture antes de mexer no leitor.

- [ ] **Step 7: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs tests/TecnoFisc.Sped.Ecf.Tests/Parser/ColunasNaoModeladasTests.cs
git commit -m "feat(txt): capture unmodeled columns instead of discarding them"
```

---

### Task 3: Benchmark do caminho feliz (Hard rule 5)

**Files:**
- Create: `benchmarks/TecnoFisc.Sped.Benchmarks/ColunasNaoModeladasBenchmark.cs`

A tese a provar é dupla: (a) o caminho **sem** coluna excedente — o de produção nos quatro leiautes — não paga nada pela mudança da Task 2; (b) o caminho **com** excedente paga só a alocação da lista e das strings, proporcional ao que preserva.

- [ ] **Step 1: Criar o benchmark**

`benchmarks/TecnoFisc.Sped.Benchmarks/ColunasNaoModeladasBenchmark.cs`:

```csharp
using System.Text;
using BenchmarkDotNet.Attributes;
using TecnoFisc.Sped.Ecf.Parser;
using TecnoFisc.Sped.Txt.Engine.Parser;

namespace TecnoFisc.Sped.Benchmarks;

/// <summary>
/// Guarda de regressão da captura de <c>ColunasNaoModeladas</c> (achados 2 e 8, PR #531). O ponto
/// é o baseline: <see cref="SemColunaExcedente"/> é o caminho de produção dos quatro leiautes —
/// arquivo do leiaute modelado, nenhuma coluna sobrando — e não pode pagar nada pela captura,
/// porque a condição do <c>if</c> já era avaliada antes e só o ramo vazio mudou.
/// <see cref="ComColunaExcedente"/> mede o custo quando há o que preservar: uma lista por registro
/// mais uma <c>string</c> por coluna, proporcional ao dado que antes era jogado fora.
/// </summary>
[MemoryDiagnoser]
public class ColunasNaoModeladasBenchmark
{
    private byte[] _semExcedente = null!;
    private byte[] _comExcedente = null!;

    [GlobalSetup]
    public void Setup()
    {
        _semExcedente = MontarArquivoEcf(registros: 10_000, colunasExcedentes: 0);
        _comExcedente = MontarArquivoEcf(registros: 10_000, colunasExcedentes: 5);
    }

    [Benchmark(Baseline = true)]
    public async Task<int> SemColunaExcedente() => await ContarAsync(_semExcedente);

    [Benchmark]
    public async Task<int> ComColunaExcedente() => await ContarAsync(_comExcedente);

    private static async Task<int> ContarAsync(byte[] arquivo)
    {
        var parser = new ParserEcf(new ReadingOptions { RespeitarVigenciaDoLeiaute = true });
        using var stream = new MemoryStream(arquivo, writable: false);
        int n = 0;
        await foreach (var _ in parser.ReadStreamingAsync(stream))
            n++;
        return n;
    }

    /// <summary>
    /// Arquivo ECF sintético do leiaute 12 com <paramref name="registros"/> linhas X450. O X450
    /// modela um único campo (PAIS), então cada coluna acrescentada além dele é exatamente uma
    /// <c>ColunaNaoModelada</c> com motivo <c>AlemDoModelo</c>.
    /// </summary>
    private static byte[] MontarArquivoEcf(int registros, int colunasExcedentes)
    {
        // Bloco X: X001(1) + X450(N) + X990(1) = N + 2
        // Arquivo: 0000(1) + X001(1) + X450(N) + X990(1) + 9999(1) = N + 4
        int qtdBlocoX = registros + 2;
        int totalLinhas = registros + 4;

        var excedente = new StringBuilder();
        for (int c = 0; c < colunasExcedentes; c++)
            excedente.Append("COLUNA EXCEDENTE|");

        var sb = new StringBuilder(capacity: registros * 60 + 256);
        sb.Append("|0000|LECF|0012|11111111000191|EMPRESA TESTE|0|0|||01012025|31122025|N||0||\r\n");
        sb.Append("|X001|0|\r\n");
        for (int i = 0; i < registros; i++)
            sb.Append("|X450|249|").Append(excedente).Append("\r\n");
        sb.Append("|X990|").Append(qtdBlocoX).Append("|\r\n");
        sb.Append("|9999|").Append(totalLinhas).Append("|\r\n");

        return EncodingSped.Latin1.GetBytes(sb.ToString());
    }
}
```

Conferir o código do registro de abertura e de encerramento do bloco X do ECF antes de rodar (`grep -rn "X001\|X990" src/TecnoFisc.Sped.Ecf/Registros/BlocoX/`). Se os códigos divergirem, ajustar as duas linhas — o benchmark precisa de um arquivo que o `ParserEcf` leia sem sentinela, senão mede o caminho errado.

- [ ] **Step 2: Rodar os dois benchmarks e comparar com o baseline da Task 2**

Run: `dotnet run -c Release --project benchmarks/TecnoFisc.Sped.Benchmarks --filter "*ColunasNaoModeladasBenchmark*"`
Run: `dotnet run -c Release --project benchmarks/TecnoFisc.Sped.Benchmarks --filter "*ParserVigenciaBenchmark*"`

Expected: `SemColunaExcedente` com alocação idêntica ao caso equivalente pré-mudança; `ParserVigenciaBenchmark` dentro do ruído (±2%) das médias anotadas na Task 2, Step 1. Se `SemVigencia`/`ComVigencia` regredirem fora do ruído, **parar e reportar** — não seguir para a Task 4. `ComColunaExcedente` mais lento e alocando mais é esperado e não é regressão: é o custo de preservar o que antes se perdia.

- [ ] **Step 3: Commit**

```bash
git add benchmarks/TecnoFisc.Sped.Benchmarks/ColunasNaoModeladasBenchmark.cs
git commit -m "test(bench): measure the cost of capturing unmodeled columns"
```

---

### Task 4: Discriminador de origem da sentinela

**Files:**
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Abstracoes/RegistroNaoReconhecido.cs`
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs:128-134` e `:695`
- Modify: `src/TecnoFisc.Sped.Ecf/ArquivoEcf.cs:41-46`, `src/TecnoFisc.Sped.Ecd/ArquivoEcd.cs:38-42`, `src/TecnoFisc.Sped.EfdContribuicoes/ArquivoEfdContribuicoes.cs:43-47`, `src/TecnoFisc.Sped.EfdIcmsIpi/ArquivoEfdIcmsIpi.cs:42-46` (só XML doc)
- Test: `tests/TecnoFisc.Sped.Ecf.Tests/Parser/MotivoNaoReconhecimentoTests.cs` (novo)

**Interfaces:**
- Produces: `MotivoNaoReconhecimento` (enum: `CodigoDesconhecido = 0`, `PosteriorAVersaoDeclarada = 1`), `RegistroNaoReconhecido.Motivo`. O construtor passa a exigir o motivo como quarto parâmetro — **breaking**, mas não há uso direto fora do leitor (verificado: `grep -rn "new RegistroNaoReconhecido" src tests benchmarks` devolve só as duas linhas de `LeitorSpedTxt`).

- [ ] **Step 1: Escrever o teste que falha**

Criar `tests/TecnoFisc.Sped.Ecf.Tests/Parser/MotivoNaoReconhecimentoTests.cs`:

```csharp
using TecnoFisc.Sped.Ecf.Tests.Versionamento;
using TecnoFisc.Sped.Txt.Engine.Abstracoes;

namespace TecnoFisc.Sped.Ecf.Tests.Parser;

/// <summary>
/// As duas origens de <see cref="RegistroNaoReconhecido"/> passam a ser separáveis por um
/// discriminador tipado, sem casar substring na mensagem em português do diagnóstico.
/// </summary>
public sealed class MotivoNaoReconhecimentoTests
{
    [Fact]
    public async Task RegistroForaDeVigencia_TemMotivoPosteriorAVersaoDeclarada()
    {
        // Y730 foi introduzido no leiaute 12; num arquivo de leiaute 9 é sentinela por vigência.
        var registros = await FixtureEcf.ReadAsync(9, "|Y730|1|");

        var sentinela = registros.OfType<RegistroNaoReconhecido>().Single();
        sentinela.Codigo.Should().Be("Y730");
        sentinela.Motivo.Should().Be(MotivoNaoReconhecimento.PosteriorAVersaoDeclarada);
    }

    [Fact]
    public async Task CodigoDesconhecido_TemMotivoCodigoDesconhecido()
    {
        // Leiaute 13 está fora da faixa modelada: código desconhecido degrada para sentinela.
        var registros = await FixtureEcf.ReadAsync(13, "|X999|conteudo novo|");

        var sentinela = registros.OfType<RegistroNaoReconhecido>().Single();
        sentinela.Codigo.Should().Be("X999");
        sentinela.Motivo.Should().Be(MotivoNaoReconhecimento.CodigoDesconhecido);
    }
}
```

- [ ] **Step 2: Rodar e confirmar que falha**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~MotivoNaoReconhecimentoTests"`
Expected: FAIL na compilação — `MotivoNaoReconhecimento` não existe.

- [ ] **Step 3: Implementar**

Em `src/TecnoFisc.Sped.Txt.Engine/Abstracoes/RegistroNaoReconhecido.cs`, acrescentar o enum antes da classe:

```csharp
/// <summary>Origem de um <see cref="RegistroNaoReconhecido"/>.</summary>
public enum MotivoNaoReconhecimento
{
    /// <summary>Código de registro que o catálogo do módulo não conhece.</summary>
    CodigoDesconhecido = 0,

    /// <summary>
    /// Registro conhecido pelo catálogo, descartado por ter <c>IntroduzidoEm</c> posterior à
    /// versão declarada no <c>0000</c> do arquivo.
    /// </summary>
    PosteriorAVersaoDeclarada = 1,
}
```

Trocar o construtor e acrescentar a propriedade:

```csharp
    public RegistroNaoReconhecido(
        string codigo, string linhaCrua, ErroLayout erro, MotivoNaoReconhecimento motivo)
    {
        ArgumentNullException.ThrowIfNull(codigo);
        ArgumentNullException.ThrowIfNull(linhaCrua);
        ArgumentNullException.ThrowIfNull(erro);
        _codigo = codigo;
        LinhaCrua = linhaCrua;
        Erro = erro;
        Motivo = motivo;
    }
```

```csharp
    /// <summary>
    /// Origem desta sentinela. Prefira este discriminador a inspecionar <see cref="Erro"/>:
    /// a mensagem é texto livre, em português, e pode ser reescrita sem aviso.
    /// </summary>
    public MotivoNaoReconhecimento Motivo { get; }
```

Atualizar o XML doc da classe (linhas 5-18) e o de `Codigo` (linhas 33-38): os dois mandam hoje o consumidor usar `Erro` para distinguir as origens; passam a apontar `Motivo`.

Nos dois pontos de construção em `LeitorSpedTxt`:

```csharp
                        yield return new RegistroNaoReconhecido(
                            codigo,
                            linhaCrua,
                            new ErroLayout(
                                linhaRegistro,
                                codigo,
                                $"Registro posterior à versão declarada no 0000 ({versaoLeiaute})."),
                            MotivoNaoReconhecimento.PosteriorAVersaoDeclarada);
```

```csharp
                    var sentinela = new RegistroNaoReconhecido(
                        fatia.ToString(), linha.ToString(), erroLayout,
                        MotivoNaoReconhecimento.CodigoDesconhecido);
```

Nas quatro classes `Arquivo*`, o XML doc de `RegistrosNaoReconhecidos` diz hoje "código desconhecido pelo catálogo ou descartado por vigência". Acrescentar a frase, idêntica nas quatro:

```csharp
    /// Use <see cref="RegistroNaoReconhecido.Motivo"/> para separar as duas origens — a mensagem
    /// de <see cref="RegistroNaoReconhecido.Erro"/> é texto livre e não é contrato.
```

- [ ] **Step 4: Rodar e confirmar que passa**

Run: `dotnet test TecnoFisc.Sped.slnx`
Expected: PASS. A suíte inteira roda aqui porque o construtor mudou e os quatro pacotes compilam contra ele.

- [ ] **Step 5: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine src/TecnoFisc.Sped.Ecf src/TecnoFisc.Sped.Ecd src/TecnoFisc.Sped.EfdContribuicoes src/TecnoFisc.Sped.EfdIcmsIpi tests/TecnoFisc.Sped.Ecf.Tests
git commit -m "feat(txt)!: discriminate the origin of RegistroNaoReconhecido"
```

---

### Task 5: `VersaoDoArquivo` e o retorno guardado dos aliases do `0020`

**Files:**
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Abstracoes/RegistroSped.cs`
- Modify: `src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs:128-134`, `:148-152`, `:700-703`
- Modify: `src/TecnoFisc.Sped.Ecf/Registros/Bloco0/Registro0020.cs:137-150`
- Test: `tests/TecnoFisc.Sped.Ecf.Tests/Registros/Bloco0/Registro0020Tests.cs`, `tests/TecnoFisc.Sped.Txt.Engine.Tests/Abstracoes/RegistroSpedTests.cs`

**Interfaces:**
- Produces: `RegistroSped.VersaoDoArquivo` (`int`, `public get` / `internal set`); `Registro0020.IndPrTransf` e `Registro0020.PossuiCebras`, ambos `IndicadorSimNao?`.

**Decisão de contrato (confirmada com o usuário):** o alias no leiaute errado devolve `null`. Getter não lança — a ausência é o sinal.

- [ ] **Step 1: Escrever o teste que falha**

Em `tests/TecnoFisc.Sped.Ecf.Tests/Registros/Bloco0/Registro0020Tests.cs` (se o arquivo não existir, criar com o `namespace TecnoFisc.Sped.Ecf.Tests.Registros.Bloco0` e os `using` de `TecnoFisc.Sped.Ecf.Registros.Bloco0`, `TecnoFisc.Sped.Ecf.Tests.Versionamento` e `TecnoFisc.Sped.Txt.Engine.Enums`):

```csharp
    [Theory]
    [InlineData(10)]
    [InlineData(11)]
    public async Task IndPrTransf_RespondeSomenteNosLeiautes10E11(int versao)
    {
        var registro = await Ler0020(versao);

        registro.IndPrTransf.Should().Be(registro.IndicadorPosicao31);
        registro.PossuiCebras.Should().BeNull();
    }

    [Fact]
    public async Task PossuiCebras_RespondeSomenteDoLeiaute12EmDiante()
    {
        var registro = await Ler0020(12);

        registro.PossuiCebras.Should().Be(registro.IndicadorPosicao31);
        registro.IndPrTransf.Should().BeNull();
    }

    [Fact]
    public async Task VersaoDoArquivo_EhPropagadaParaTodoRegistroLido_InclusiveO0000()
    {
        var registros = await FixtureEcf.ReadAsync(11, "|0001|0|");

        registros.Should().NotBeEmpty();
        registros.Should().OnlyContain(registro => registro.VersaoDoArquivo == 11);
    }

    /// <summary>
    /// Monta um 0020 com as 30 primeiras colunas de dado (Ordem 2..31), a última sendo o campo
    /// posicional 31 — ativo a partir do leiaute 10.
    /// </summary>
    private static async Task<Registro0020> Ler0020(int versao)
    {
        var valores = new List<string> { "1", "1" };
        valores.AddRange(Enumerable.Repeat("N", 27));
        valores.Add("S");
        string linha = "|0020|" + string.Join('|', valores) + "|";
        return (await FixtureEcf.ReadAsync(versao, linha)).OfType<Registro0020>().Single();
    }
```

E acrescentar, em `tests/TecnoFisc.Sped.Txt.Engine.Tests/Abstracoes/RegistroSpedTests.cs`, o caso do default no registro que não veio de leitura:

```csharp
    [Fact]
    public void VersaoDoArquivo_EhZeroQuandoORegistroNaoVeioDeLeitura()
        => new RegistroDeTeste().VersaoDoArquivo.Should().Be(0);
```

- [ ] **Step 2: Rodar e confirmar que falha**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~Registro0020Tests"`
Expected: FAIL na compilação — nem `VersaoDoArquivo` nem os dois aliases existem.

- [ ] **Step 3: Acrescentar `VersaoDoArquivo` ao registro base**

Em `src/TecnoFisc.Sped.Txt.Engine/Abstracoes/RegistroSped.cs`, logo abaixo de `VersaoLeiaute` (linha 36):

```csharp
    /// <summary>
    /// Versão do leiaute declarada no <c>0000</c> do arquivo em que este registro foi lido, ou
    /// <c>0</c> quando o registro não veio de uma leitura de arquivo (construído à mão, ou lido
    /// por <see cref="Parser.LeitorSpedTxt.ParseLinha"/> sem versão informada). Distinto de
    /// <see cref="VersaoLeiaute"/>, que é a versão que o <b>próprio</b> registro declara e só o
    /// <c>0000</c> conhece. Serve ao registro que precisa saber em que leiaute foi lido para
    /// interpretar uma posição cuja semântica mudou entre versões.
    /// </summary>
    public int VersaoDoArquivo { get; internal set; }
```

- [ ] **Step 4: Preencher nos três pontos do leitor**

**(a)** Em `InterpretarLinha`, logo depois de `registro = metadados.Fabrica();` (linha 702):

```csharp
                registro.VersaoDoArquivo = versaoLeiaute;
```

**(b)** Em `ReadStreamingAsync`, dentro do bloco que avalia a versão uma única vez, logo depois de `versaoLeiaute = registro.VersaoLeiaute;` (linha 151):

```csharp
                            // The version carrier (in practice the 0000) is interpreted before
                            // its version is known, so the assignment in InterpretarLinha left it
                            // at zero: fix it here, so the 0000 itself answers like the rest of
                            // the file.
                            registro.VersaoDoArquivo = versaoLeiaute;
```

**(c)** Na sentinela de vigência (linha 128), para que ela responda como os registros materializados:

```csharp
                        yield return new RegistroNaoReconhecido(
                            codigo,
                            linhaCrua,
                            new ErroLayout(
                                linhaRegistro,
                                codigo,
                                $"Registro posterior à versão declarada no 0000 ({versaoLeiaute})."),
                            MotivoNaoReconhecimento.PosteriorAVersaoDeclarada)
                        {
                            VersaoDoArquivo = versaoLeiaute
                        };
```

A sentinela de código desconhecido (linha 695) já é criada dentro de `InterpretarLinha`, que não conhece o objeto antes de construí-lo; deixá-la em `0` seria inconsistente, então acrescentar a atribuição junto do `pilha.Topo?.AdicionarFilho(sentinela)`:

```csharp
                    var sentinela = new RegistroNaoReconhecido(
                        fatia.ToString(), linha.ToString(), erroLayout,
                        MotivoNaoReconhecimento.CodigoDesconhecido)
                    {
                        VersaoDoArquivo = versaoLeiaute
                    };
```

- [ ] **Step 5: Devolver os aliases guardados**

Em `src/TecnoFisc.Sped.Ecf/Registros/Bloco0/Registro0020.cs`, depois de `IndicadorPosicao31` (linha 150). Não tocar no `[CampoSped]` nem no XML doc dele — os aliases são propriedades calculadas, sem atributo, e não entram no catálogo:

```csharp
    /// <summary>
    /// Semântica do campo 31 nos leiautes 10 e 11: opção pelas novas regras de preços de
    /// transferência (<c>IND_PR_TRANSF</c>). <c>null</c> em qualquer outro leiaute, onde a posição
    /// significa outra coisa — ver <see cref="IndicadorPosicao31"/>. Propriedade calculada: não é
    /// campo do catálogo, e depende de <see cref="RegistroSped.VersaoDoArquivo"/>, que só é
    /// preenchida em registro vindo de leitura de arquivo.
    /// </summary>
    public IndicadorSimNao? IndPrTransf
        => VersaoDoArquivo is 10 or 11 ? IndicadorPosicao31 : null;

    /// <summary>
    /// Semântica do campo 31 a partir do leiaute 12: posse de certificado Cebas
    /// (<c>POSSUI_CEBRAS</c>). <c>null</c> nos leiautes anteriores — ver
    /// <see cref="IndicadorPosicao31"/>. Responde também em leiaute posterior ao 12, ainda não
    /// modelado: enquanto a Receita não reaproveitar a posição de novo, a leitura vigente é essa.
    /// </summary>
    public IndicadorSimNao? PossuiCebras
        => VersaoDoArquivo >= 12 ? IndicadorPosicao31 : null;
```

- [ ] **Step 6: Rodar e confirmar que passa**

Run: `dotnet test TecnoFisc.Sped.slnx`
Expected: PASS.

- [ ] **Step 7: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine src/TecnoFisc.Sped.Ecf tests
git commit -m "feat(txt): propagate the file layout version to every record read"
```

---

### Task 6: Documentação — fechar a limitação conhecida

**Files:**
- Modify: `CHANGELOG.md:13-29` e a seção `### TecnoFisc.Sped.Txt.Engine`
- Modify: `README.md:12`, `:56`, `:108`
- Modify: `ARCHITECTURE.md:102`, `:220`, `:779`
- Modify: `sped/STAGE_17_ECF_BASELINE.md:7`
- Modify: `src/TecnoFisc.Sped.Ecf/TecnoFisc.Sped.Ecf.csproj:4` (`<Description>`)

O que muda em todos: a afirmação "o conteúdo dessas colunas **não é materializado**" deixa de ser verdadeira sem qualificação. Passa a ser "não é materializado **como propriedade tipada** — fica disponível em bruto em `ColunasNaoModeladas`".

- [ ] **Step 1: Reescrever a promessa nos cinco arquivos de documentação**

Formulação-padrão, a adaptar ao tamanho de cada trecho:

> Os registros e as colunas exclusivos dos leiautes 8 a 10 são **reconhecidos, não tipados**: o leitor não aborta, e o conteúdo dessas colunas chega ao consumidor em bruto em `RegistroSped.ColunasNaoModeladas`, com a posição e o motivo — o que não existe é propriedade tipada para elas.

Em `ARCHITECTURE.md:220` (§4.7), acrescentar a frase ao parágrafo do `[Descontinuado]`:

> "Reconhecido" não implica "tipado", mas também não implica "perdido": as colunas de um registro sem `[CampoSped]` saem por `RegistroSped.ColunasNaoModeladas`, com motivo `AlemDoModelo`.

- [ ] **Step 2: Reescrever a limitação conhecida do CHANGELOG**

Em `CHANGELOG.md`, na subseção `#### Limitações conhecidas` do `TecnoFisc.Sped.Ecf`, o primeiro bullet (linha 28) descreve a lacuna que este PR fecha e termina com "O PR seguinte fecha a lacuna expondo as colunas não modeladas em `ColunasNaoModeladas`". **Remover o bullet inteiro** e registrar a entrega em `#### Adicionado`:

```markdown
- O conteúdo das colunas que o modelo tipado não representa deixa de ser descartado: um registro reconhecido sem campos modelados (os sete removidos no leiaute 11) e uma coluna além do último campo declarado (caso do `X450` nos leiautes 8 a 10) chegam ao consumidor em bruto, em `RegistroSped.ColunasNaoModeladas`, com a posição na numeração do Guia Prático e o motivo (`AlemDoModelo` ou `PosteriorAVersaoDeclarada`). Fecha a contrapartida registrada como limitação conhecida durante o desenvolvimento: quem lia arquivos históricos com `LenientLayout = true` e dependia de `RegistroNaoReconhecido.LinhaCrua` passa a ter o tipo certo **e** o conteúdo das colunas.
```

O segundo bullet de limitações conhecidas (rótulo `POSSUI_CEBRAS` em `ErroFormato` nos leiautes 10 e 11) **permanece** — este PR não o resolve; o rótulo continua vindo do manifesto do leiaute 12.

Na seção `### TecnoFisc.Sped.Txt.Engine`, acrescentar em `#### Adicionado`:

```markdown
- `RegistroSped.ColunasNaoModeladas` (`IReadOnlyList<ColunaNaoModelada>`) — colunas presentes na linha sem propriedade tipada que as receba, com `Posicao` (numeração do Guia Prático, `1` = `REG`), `Valor` verbatim e `MotivoColunaNaoModelada` (`AlemDoModelo`, `PosteriorAVersaoDeclarada`). Vazia no caminho comum, e alocada só quando há o que reportar, no mesmo padrão de `ErrosDeFormato`. O custo no caminho feliz é nulo: a condição já era avaliada no leitor e só o ramo vazio passou a fazer algo (medido em `ColunasNaoModeladasBenchmark`). Não há flag em `ReadingOptions` para desligar — o contrato é "nunca perder em silêncio", e a flag existiria para poder perdê-lo.
- `RegistroSped.VersaoDoArquivo` (`int`) — versão declarada no `0000` do arquivo em que o registro foi lido, atribuída pelo leitor a cada registro materializado, inclusive ao próprio `0000` e às sentinelas. `0` quando o registro não veio de leitura, ou veio de `ParseLinha` sem versão informada. Distinta de `VersaoLeiaute`, que é a versão que o próprio registro declara.
- `RegistroNaoReconhecido.Motivo` (`MotivoNaoReconhecimento`: `CodigoDesconhecido`, `PosteriorAVersaoDeclarada`) — separa as duas origens da sentinela sem casar substring na mensagem em português do diagnóstico.
```

E em `#### Quebrado`:

```markdown
- O construtor de `RegistroNaoReconhecido` passa a exigir um quarto parâmetro, `MotivoNaoReconhecimento`. Só o leitor constrói a sentinela no caminho normal; quem a instanciava à mão (teste, dublê) precisa informar a origem.
```

Na seção `### TecnoFisc.Sped.Ecf`, em `#### Adicionado`:

```markdown
- `Registro0020.IndPrTransf` e `Registro0020.PossuiCebras` voltam, agora como `IndicadorSimNao?` calculado sobre `VersaoDoArquivo`: `IndPrTransf` responde nos leiautes 10 e 11, `PossuiCebras` do 12 em diante, e cada um devolve `null` fora da sua faixa em vez de entregar em silêncio o valor do outro campo. `IndicadorPosicao31` continua sendo a porta única e sem interpretação. Num registro que não veio de leitura de arquivo (`VersaoDoArquivo == 0`) os dois devolvem `null`.
```

- [ ] **Step 3: Verificar que nada ficou para trás**

Run: `grep -rn "não é materializado\|nao e materializado" README.md ARCHITECTURE.md CHANGELOG.md sped/STAGE_17_ECF_BASELINE.md src/TecnoFisc.Sped.Ecf/TecnoFisc.Sped.Ecf.csproj`
Expected: nenhuma ocorrência sem a qualificação "como propriedade tipada".

Run: `grep -rn "ColunasNaoModeladas" README.md ARCHITECTURE.md CHANGELOG.md sped/STAGE_17_ECF_BASELINE.md`
Expected: ao menos uma ocorrência em cada.

- [ ] **Step 4: Commit e abertura do PR**

```bash
git add CHANGELOG.md README.md ARCHITECTURE.md sped/STAGE_17_ECF_BASELINE.md src/TecnoFisc.Sped.Ecf/TecnoFisc.Sped.Ecf.csproj
git commit -m "docs: record the unmodeled-column contract and close the known limitation"
```

- [ ] **Step 5: Rodar tudo antes de abrir o PR B**

Run: `dotnet build TecnoFisc.Sped.slnx`
Expected: 0 erros, 0 avisos.

Run: `dotnet test TecnoFisc.Sped.slnx`
Expected: PASS.

Abrir o PR com título **exatamente** `feat(txt)!: expose unmodeled columns and discriminate sentinel origin` — é ele que vira o squash em `dev` e o que o semantic-release lê. O `!` é obrigatório: o construtor de `RegistroNaoReconhecido` mudou.

---

## PR C — limpeza

Branch a partir de `dev`, **depois** do merge do PR B. Título do PR: `refactor(txt): unify the root file model across the four layouts`. Tipo `refactor` não gera release, que é o correto — não há mudança de comportamento.

### Task 7: `ArquivoSpedBase`

**Files:**
- Create: `src/TecnoFisc.Sped.Txt.Engine/Abstracoes/ArquivoSpedBase.cs`
- Modify: `src/TecnoFisc.Sped.Ecf/ArquivoEcf.cs`, `src/TecnoFisc.Sped.Ecd/ArquivoEcd.cs`, `src/TecnoFisc.Sped.EfdContribuicoes/ArquivoEfdContribuicoes.cs`, `src/TecnoFisc.Sped.EfdIcmsIpi/ArquivoEfdIcmsIpi.cs`

**Interfaces:**
- Produces: `ArquivoSpedBase<TBloco>` com `RegistrosNaoReconhecidos`, `EnumerarBlocos`, `EnumerarRegistros`, `Adicionar`, e os membros protegidos `NomeDoLeiaute`, `Bloco(string)`, `AdicionarAoBloco(TBloco, RegistroSped)`, `PreencherAsync`.

**Critério de aceite:** suíte dos quatro pacotes verde **sem nenhuma alteração de teste**. Se um teste precisar mudar, o comportamento mudou e o refactor está errado — parar e reportar.

**Restrição descoberta na verificação:** `IBlocoSped` (`src/TecnoFisc.Sped.Txt.Engine/Abstracoes/IBlocoSped.cs`) declara só `Identificador` e `EnumerarRegistros()`. O `Adicionar` de cada bloco concreto (`BlocoEcf.Adicionar`, etc.) é `internal` ao assembly do leiaute, então a base **não consegue chamá-lo**. As duas saídas eram declarar `Adicionar` em `IBlocoSped` — o que tornaria a mutação de bloco parte da API pública dos quatro pacotes, contra a intenção read-only do modelo — ou um hook protegido implementado dentro de cada assembly. Este plano usa o hook.

- [ ] **Step 1: Rodar a suíte e guardar o baseline**

Run: `dotnet test TecnoFisc.Sped.slnx`
Expected: PASS. **Anotar a contagem exata de testes aprovados** — é ela que o Step 6 compara.

- [ ] **Step 2: Criar a base**

`src/TecnoFisc.Sped.Txt.Engine/Abstracoes/ArquivoSpedBase.cs`:

```csharp
namespace TecnoFisc.Sped.Txt.Engine.Abstracoes;

/// <summary>
/// Modelo raiz comum aos leiautes: agrupa registros em blocos na ordem canônica, roteia
/// <see cref="RegistroNaoReconhecido"/> para uma coleção à parte e enumera blocos e registros.
/// Each concrete layout supplies its block order, its block factory, and how to add to a block —
/// the last one because each block's <c>Adicionar</c> is internal to its layout assembly,
/// deliberately outside the read-only model's public API.
/// </summary>
/// <typeparam name="TBloco">Tipo de bloco do leiaute.</typeparam>
public abstract class ArquivoSpedBase<TBloco> : IArquivoSped
    where TBloco : IBlocoSped
{
    private readonly string[] _ordemBlocos;
    private readonly Dictionary<string, TBloco> _blocos;
    private readonly List<RegistroNaoReconhecido> _naoReconhecidos = [];

    /// <param name="ordemBlocos">Identificadores dos blocos na ordem canônica do leiaute.</param>
    /// <param name="criarBloco">Fábrica do bloco, chamada uma vez por identificador.</param>
    protected ArquivoSpedBase(string[] ordemBlocos, Func<string, TBloco> criarBloco)
    {
        ArgumentNullException.ThrowIfNull(ordemBlocos);
        ArgumentNullException.ThrowIfNull(criarBloco);
        _ordemBlocos = ordemBlocos;
        _blocos = new Dictionary<string, TBloco>(ordemBlocos.Length, StringComparer.Ordinal);
        foreach (var id in ordemBlocos)
            _blocos.Add(id, criarBloco(id));
    }

    /// <summary>Nome do leiaute, usado na mensagem de erro de roteamento.</summary>
    protected abstract string NomeDoLeiaute { get; }

    /// <summary>
    /// Adds to the block. Implemented per layout because each block's <c>Adicionar</c> is
    /// internal to its own assembly.
    /// </summary>
    protected abstract void AdicionarAoBloco(TBloco bloco, RegistroSped registro);

    /// <summary>Bloco pelo identificador. Lança se o bloco não existir no leiaute.</summary>
    protected TBloco Bloco(string id) => _blocos[id];

    /// <summary>
    /// Registros que o leitor não conseguiu classificar — código desconhecido pelo catálogo ou
    /// descartado por vigência. Só é populado sob <c>LenientLayout</c> ou vigência ligada; sob
    /// leitura estrita o parser já teria abortado antes. Use
    /// <see cref="RegistroNaoReconhecido.Motivo"/> para separar as duas origens — a mensagem de
    /// <see cref="RegistroNaoReconhecido.Erro"/> é texto livre e não é contrato.
    /// </summary>
    public IReadOnlyList<RegistroNaoReconhecido> RegistrosNaoReconhecidos => _naoReconhecidos;

    /// <inheritdoc />
    public IEnumerable<IBlocoSped> EnumerarBlocos()
    {
        foreach (var id in _ordemBlocos)
            yield return _blocos[id];
    }

    /// <summary>Enumera todos os registros na ordem canônica dos blocos.</summary>
    public IEnumerable<RegistroSped> EnumerarRegistros()
    {
        foreach (var id in _ordemBlocos)
            foreach (var registro in _blocos[id].EnumerarRegistros())
                yield return registro;
    }

    /// <summary>
    /// Adiciona um registro ao bloco correspondente à primeira posição do código.
    /// <see cref="RegistroNaoReconhecido"/> desvia para <see cref="RegistrosNaoReconhecidos"/> em
    /// vez de ser roteado por código — nunca lança. Qualquer outro registro cujo bloco não exista
    /// lança <see cref="InvalidOperationException"/>: é erro de uso da API (registro tipado de um
    /// bloco que o leiaute não tem), não dado ruim de arquivo.
    /// </summary>
    public void Adicionar(RegistroSped registro)
    {
        ArgumentNullException.ThrowIfNull(registro);

        if (registro is RegistroNaoReconhecido naoReconhecido)
        {
            _naoReconhecidos.Add(naoReconhecido);
            return;
        }

        var codigo = registro.Codigo;
        if (string.IsNullOrEmpty(codigo))
            throw new ArgumentException("Registro com código vazio não pode ser adicionado.", nameof(registro));

        var idBloco = char.ToUpperInvariant(codigo[0]).ToString();
        if (!_blocos.TryGetValue(idBloco, out var bloco))
            throw new InvalidOperationException(
                $"Código '{codigo}' não pertence a um bloco conhecido do leiaute {NomeDoLeiaute}.");

        AdicionarAoBloco(bloco, registro);
    }

    /// <summary>Consome o fluxo do parser preenchendo este arquivo.</summary>
    protected async Task PreencherAsync(
        IAsyncEnumerable<RegistroSped> registros, CancellationToken cancelamento)
    {
        ArgumentNullException.ThrowIfNull(registros);
        await foreach (var registro in registros.WithCancellation(cancelamento).ConfigureAwait(false))
            Adicionar(registro);
    }
}
```

Antes de migrar, comparar a mensagem de `InvalidOperationException` acima com a de cada uma das quatro classes. Hoje o ECF diz `"...de um bloco conhecido do leiaute ECF."`. Se alguma das outras três usar redação diferente, **conferir se algum teste casa a mensagem** (`grep -rn "não pertence a um bloco" tests/`) antes de uniformizar: mensagem diferente com teste que a verifica viola o critério de aceite desta task.

- [ ] **Step 3: Migrar `ArquivoEcf`**

```csharp
public sealed class ArquivoEcf : ArquivoSpedBase<BlocoEcf>
{
    private static readonly string[] _ordemBlocos = ["0", "C", "E", "J", "K", "L", "M", "N", "P", "Q", "T", "U", "V", "W", "X", "Y", "9"];

    public ArquivoEcf() : base(_ordemBlocos, id => new BlocoEcf(id)) { }

    /// <inheritdoc />
    protected override string NomeDoLeiaute => "ECF";

    /// <inheritdoc />
    protected override void AdicionarAoBloco(BlocoEcf bloco, RegistroSped registro)
        => bloco.Adicionar(registro);

    public BlocoEcf Bloco0 => Bloco("0");
    public BlocoEcf BlocoC => Bloco("C");
    public BlocoEcf BlocoE => Bloco("E");
    public BlocoEcf BlocoJ => Bloco("J");
    public BlocoEcf BlocoK => Bloco("K");
    public BlocoEcf BlocoL => Bloco("L");
    public BlocoEcf BlocoM => Bloco("M");
    public BlocoEcf BlocoN => Bloco("N");
    public BlocoEcf BlocoP => Bloco("P");
    public BlocoEcf BlocoQ => Bloco("Q");
    public BlocoEcf BlocoT => Bloco("T");
    public BlocoEcf BlocoU => Bloco("U");
    public BlocoEcf BlocoV => Bloco("V");
    public BlocoEcf BlocoW => Bloco("W");
    public BlocoEcf BlocoX => Bloco("X");
    public BlocoEcf BlocoY => Bloco("Y");
    public BlocoEcf Bloco9 => Bloco("9");

    /// <summary>Constrói o arquivo a partir do fluxo do parser.</summary>
    public static async Task<ArquivoEcf> LoadAsync(
        IAsyncEnumerable<RegistroSped> registros,
        CancellationToken cancelamento = default)
    {
        var arquivo = new ArquivoEcf();
        await arquivo.PreencherAsync(registros, cancelamento).ConfigureAwait(false);
        return arquivo;
    }
}
```

O XML doc da classe (`/// Modelo raiz read-only de um arquivo ECF...`) permanece. O de `RegistrosNaoReconhecidos`, o de `Adicionar` e o de `EnumerarRegistros` saem daqui — passam a vir da base por herança de documentação.

- [ ] **Step 4: Rodar só a suíte do ECF**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~TecnoFisc.Sped.Ecf.Tests"`
Expected: PASS sem alterar teste algum. Se algum falhar, o refactor mudou comportamento — reverter e reportar.

- [ ] **Step 5: Migrar os outros três, um por vez**

Mesma forma do Step 3, trocando o tipo de bloco, a ordem de blocos (copiar a de cada arquivo, **não** reescrever de memória — a ordem canônica difere entre leiautes), o `NomeDoLeiaute` (`"ECD"`, `"EFD Contribuições"`, `"EFD ICMS-IPI"` — usar a redação que a mensagem de cada classe já tem hoje) e os acessores tipados de bloco.

Run após cada migração: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~TecnoFisc.Sped.<Pacote>.Tests"`
Expected: PASS sem alterar teste algum. Se um deles exigir mudança de teste, reverter **aquela** migração e reportar, sem tocar nas demais.

- [ ] **Step 6: Rodar a suíte inteira e comparar com o baseline**

Run: `dotnet build TecnoFisc.Sped.slnx`
Expected: 0 erros, 0 avisos.

Run: `dotnet test TecnoFisc.Sped.slnx`
Expected: PASS com a **mesma contagem** de testes anotada no Step 1.

- [ ] **Step 7: Commit**

```bash
git add src/TecnoFisc.Sped.Txt.Engine/Abstracoes/ArquivoSpedBase.cs src/TecnoFisc.Sped.Ecf/ArquivoEcf.cs src/TecnoFisc.Sped.Ecd/ArquivoEcd.cs src/TecnoFisc.Sped.EfdContribuicoes/ArquivoEfdContribuicoes.cs src/TecnoFisc.Sped.EfdIcmsIpi/ArquivoEfdIcmsIpi.cs
git commit -m "refactor(txt): unify the root file model across the four layouts"
```

---

### Task 8: Os dois parked restantes

**Files:**
- Modify: `tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/VigenciaComFiltroTests.cs`
- Modify: `CHANGELOG.md`

- [ ] **Step 1: Teste de defesa em profundidade sobre `RegistrosIgnorados` com filho**

O review anterior provou analiticamente que o filtro por código (`RegistrosIgnorados`) corta a subárvore antes do gate de vigência, e que os dois cortes não se atrapalham — mas isso não tem teste sobre a fixture que tem registro filho. Acrescentar a `VigenciaComFiltroTests` (conferir os helpers e as constantes de arquivo sintético já usados no arquivo e reaproveitá-los, em vez de montar um novo):

```csharp
    [Fact]
    public async Task RegistroIgnoradoPorCodigo_CortaSubarvore_MesmoQuandoTambemEstaForaDeVigencia()
    {
        var catalogo = CatalogoBuilder.BuildFromAssembly(typeof(RegistroVigenciaColunaSintetico).Assembly);
        var opcoes = new ReadingOptions
        {
            RespeitarVigenciaDoLeiaute = true,
            RegistrosIgnorados = ["A400"],
        };
        var leitor = new LeitorSpedTxt(catalogo, opcoes);
        using var stream = new MemoryStream(EncodingSped.Latin1.GetBytes(
            "|0000|010|01012025|31012025|EMPRESA|11222333000181|\r\n" +
            "|A400|desc|\r\n" +
            "|A410|desc-filho|\r\n" +
            "|9999|3|\r\n"));

        var lidos = new List<RegistroSped>();
        await foreach (var registro in leitor.ReadStreamingAsync(stream).ConfigureAwait(false))
            lidos.Add(registro);

        // O filtro por código preempta o gate de vigência: nem o A400 nem o filho A410 saem —
        // nem materializados, nem como sentinela de vigência.
        lidos.Select(registro => registro.Codigo).Should().Equal("0000", "9999");
        lidos.OfType<RegistroNaoReconhecido>().Should().BeEmpty();
    }
```

Ajustar os códigos de registro (`A400`/`A410`) e o conteúdo do `0000` sintético ao que a fixture do projeto realmente declara — `SentinelaVigenciaTests` usa exatamente esse arquivo sintético e é a referência a copiar.

- [ ] **Step 2: Rodar**

Run: `dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~VigenciaComFiltroTests"`
Expected: PASS já na primeira execução. Este teste é defesa em profundidade sobre comportamento existente — se falhar, **não ajustar o teste**: há um bug real, parar e reportar.

- [ ] **Step 3: Uniformizar a ordem das subseções do CHANGELOG**

Dentro de cada pacote, a ordem das subseções passa a ser sempre: `#### Adicionado`, `#### Alterado`, `#### Corrigido`, `#### Quebrado`, `#### Limitações conhecidas`. Reordenar apenas onde divergir, **sem reescrever conteúdo** — o diff deve ser só movimentação de blocos.

- [ ] **Step 4: Rodar tudo e commitar**

Run: `dotnet build TecnoFisc.Sped.slnx && dotnet test TecnoFisc.Sped.slnx`
Expected: 0 avisos, PASS.

```bash
git add tests/TecnoFisc.Sped.Txt.Engine.Tests/Parser/VigenciaComFiltroTests.cs CHANGELOG.md
git commit -m "test(txt): cover subtree cut by code filter under layout versioning"
```

Abrir o PR com título **exatamente** `refactor(txt): unify the root file model across the four layouts`.

---

## Verificação final antes da publicação

- [ ] `dotnet build TecnoFisc.Sped.slnx` — 0 erros, 0 avisos
- [ ] `dotnet test TecnoFisc.Sped.slnx` — tudo verde, contagem ≥ a de `dev` antes do PR B
- [ ] `dotnet run -c Release --project benchmarks/TecnoFisc.Sped.Benchmarks --filter "*Vigencia*"` — dentro do ruído do baseline da Task 2
- [ ] `dotnet run -c Release --project benchmarks/TecnoFisc.Sped.Benchmarks --filter "*ColunasNaoModeladas*"` — `SemColunaExcedente` sem alocação extra
- [ ] `dotnet pack TecnoFisc.Sped.slnx -c Release` — concluído
- [ ] Os dois PRs mergeados em `dev` por **Squash and Merge**, cada um com o título em Conventional Commit exato
- [ ] `CHANGELOG.md` com a lista de breaking changes completa desta onda: construtor de `RegistroNaoReconhecido` e a herança de `ArquivoSpedBase` nos quatro modelos raiz
- [ ] `sped/STAGE_17_ECF_BASELINE.md` reflete o recorte real: modelo tipado do leiaute 12, leitura de 8–12, colunas não tipadas preservadas em bruto
