---
description: Implementa o próximo registro SPED pendente com testes completos, cria commit e PR. Funciona para qualquer módulo SPED (EFD Contribuições, Fiscal, etc.).
argument-hint: [módulo] (opcional: efd-contribuicoes, fiscal — auto-detecta se omitido)
allowed-tools: Read, Write, Edit, Glob, Grep, Bash, Agent
---

Você é um implementador de registros SPED. Sua tarefa é identificar o próximo sub-estágio pendente, implementar o registro com testes completos, e criar o commit e PR. Siga cada passo em ordem, sem pular etapas.

## PASSO 1 — Detectar módulo e mapeamento

### 1a. Identificar o módulo ativo

Se `$ARGUMENTS` contiver um módulo (ex.: `efd-contribuicoes`, `fiscal`), use-o. Caso contrário, auto-detecte:

1. Liste todos os arquivos `STAGE_*_REGISTROS*.md` na raiz do repositório com `Glob(pattern: "STAGE_*_REGISTROS*.md")`
2. Para cada arquivo encontrado, leia as primeiras linhas para identificar o módulo
3. Prefira o arquivo com sub-estágios mais recentemente implementados (marcados `[x]`)
4. Se houver apenas um arquivo, use-o

### 1b. Mapa de módulos

Módulos conhecidos e seus mapeamentos:

**EFD Contribuições (Stage 4)**
- Tracking: `STAGE_4_REGISTROS.md`
- PDF: `Guia_Pratico_EFD_Contribuicoes_Versao_1_35 - 18_06_2021.pdf`
- Projeto src: `TecnoFisc.Sped.EfdContribuicoes`
- Path src: `src/TecnoFisc.Sped.EfdContribuicoes/Registros/Bloco{X}/`
- Namespace src: `TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco{X}`
- Projeto tests: `TecnoFisc.Sped.EfdContribuicoes.Tests`
- Path tests: `tests/TecnoFisc.Sped.EfdContribuicoes.Tests/Registros/Bloco{X}/`
- Namespace tests: `TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco{X}`
- Namespace enums: `TecnoFisc.Sped.EfdContribuicoes.Enums`
- Path enums: `src/TecnoFisc.Sped.EfdContribuicoes/Enums/`

**Novos módulos futuros (Stages 8-10)**
- Quando um novo arquivo `STAGE_N_REGISTROS_*.md` existir, leia seu cabeçalho para obter os caminhos equivalentes
- O padrão é sempre: `TecnoFisc.Sped.{NomeMódulo}` para src e `.Tests` para tests
- O PDF será referenciado no cabeçalho do arquivo de tracking

Para blocos com letra maiúscula (A, C, D, F, I, M, P): `Bloco{X}` onde `{X}` é a letra. Para blocos numéricos (0, 1, 9): `Bloco{X}` onde `{X}` é o dígito.

## PASSO 2 — Identificar próximo(s) sub-estágio(s)

### 2a. Ler o arquivo de tracking

Leia o arquivo de tracking completo. Encontre a **primeira linha com `| [ ] |`** (não implementada). Extraia:
- Número do sub-estágio (ex.: `4.004`)
- Código do registro (ex.: `0100`, `A010`, `C100`)
- Descrição
- Número da página PDF

### 2b. Avaliar batching

Verifique se o registro candidato é elegível para batch (todos os critérios devem ser verdadeiros):
- Dois ou três campos, sem decimais, sem enums, sem validação de value object além de formatação
- Sem registros filhos hierárquicos
- Sem bloco "Regras de Validação" / observações além da tabela de campos
- Contíguo no mesmo bloco com outros candidatos igualmente simples

Se elegível, inspecione os 2-3 sub-estágios seguintes e avalie incluí-los no mesmo PR. Cap máximo: 10 registros por PR.

**Decisão final:** anote os sub-estágios a implementar nesta execução (ex.: `[4.004]` ou `[4.034, 4.041, 4.045, 4.056, 4.060]`).

## PASSO 3 — Ler o PDF

Para cada registro a implementar, use `Read` com o parâmetro `pages` apontando para a página listada no tracking file. Leia 3-6 páginas a partir dessa página (até o próximo cabeçalho `Registro XXXX`).

Do PDF extraia obrigatoriamente:
- **Código** do registro (`Registro XXXX`)
- **Nível hierárquico** (`Nível: N`) — crítico para `PilhaHierarquica`
- **Ocorrência** (`1:N`, `0:1`, `1:1`) — determina se é filho obrigatório/opcional/repetido
- **Tabela de campos**: posição na linha (`Ordem`), nome do campo, tipo (`C`/`N`), tamanho (`Tam`; `*` = fixo), decimais (`Dec`), obrigatoriedade (`Obrig`: S/N/O)
- **Observações, regras de validação e tabelas de códigos** — encriptar como lógica de validação, não comentários

## PASSO 4 — Criar branch git

Antes de criar qualquer arquivo, crie o branch:

- 1 registro: `feat/stage-{N-NNN}-registro-{CODE}` (ex.: `feat/stage-4-004-registro-0100`)
- Batch de vários: `feat/stage-{N-NNN}-{N-MMM}-bloco{X}-batch` (ex.: `feat/stage-4-034-036-bloco-c-processos`)

```powershell
git checkout dev
git pull
git checkout -b feat/stage-...
```

## PASSO 5 — Implementar registro(s)

### 5a. Verificar pré-existência

Antes de criar, verifique com `Glob` se o arquivo já existe. Se existir, compare com o PDF e complemente se necessário.

### 5b. Classe do registro

Crie `src/{ProjetoSrc}/Registros/Bloco{X}/Registro{CODE}.cs`:

```csharp
using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
// Adicionar: using TecnoFisc.Sped.Core.ValueObjects; se usar Cnpj, Cpf, etc.
// Adicionar: using TecnoFisc.Sped.EfdContribuicoes.Enums; se usar enums do módulo

namespace TecnoFisc.Sped.EfdContribuicoes.Registros.Bloco{X};

/// <summary>
/// Registro {CODE} — {Descrição completa do guia}. Nível hierárquico {N}, ocorrência {OCC}.
/// Conforme Guia Prático v1.35, p. {PAGINA}.
/// </summary>
[RegistroSped(Codigo = "{CODE}", Nivel = {N}, Bloco = "{X}")]
public sealed partial class Registro{CODE} : RegistroSped
{
    public override string Codigo => "{CODE}";

    [CampoSped(Ordem = 2, Tamanho = {TAM}, Obrigatorio = {true|false})]
    public {Tipo}? {NomeProp} { get; set; }
    // ... campos na ordem exata do guia
}
```

**Regras de mapeamento de tipos:**
- Tipo `C`, sem validação especial → `string?`
- Tipo `C`, data `DDMMAAAA` → `DateOnly` (ou `DateOnly?` se opcional); adicionar `Formato = "ddMMyyyy"` no atributo
- Tipo `C`, lista de códigos pequena → enum nullable (criar enum se primeiro uso)
- Tipo `C`, CNPJ/CPF/CFOP/NCM/ChaveAcesso → tipo forte correspondente do Core
- Tipo `N`, sem decimais, valores pequenos → `int?`
- Tipo `N`, sem decimais, valores grandes (totais) → `long?`
- Tipo `N`, com decimais → `decimal?`; adicionar `Decimais = {DEC}` no atributo

**Regras do `[CampoSped]`:**
- `Ordem`: posição na linha SPED (campo `|REG|` é Ordem=1; campos do registro começam em Ordem=2)
- `Tamanho`: número do guia; `0` se variável (sem asterisco no guia)
- `Decimais`: omitir se zero
- `Obrigatorio = true`: apenas quando coluna `Obrig` = `S`; omitir quando `N` ou `O`
- `Formato`: apenas para datas — `"ddMMyyyy"`

**Campos obrigatórios não anuláveis:**
- Se `Obrigatorio = true` e tipo forte (enum, DateOnly, int, etc.), declare sem `?`
- Se `Obrigatorio = true` mas pode ser null em implementações parciais, use `?` e documente

**Sealed + partial:** sempre `sealed partial class`.

### 5c. Enums (se necessário — regra first-use)

Verifique se o enum já existe com `Glob(pattern: "src/**/Enums/*.cs")` + `Grep`.

Se não existir, crie `src/{ProjetoSrc}/Enums/{NomeEnum}.cs`:

```csharp
namespace TecnoFisc.Sped.EfdContribuicoes.Enums;

/// <summary>Descrição da tabela conforme Guia Prático v1.35, p. {PAGINA}.</summary>
public enum {NomeEnum}
{
    /// <summary>Descrição do valor.</summary>
    [SpedValor("{código}")]
    NomeValor = {N},
    // ...
}
```

Use exatamente os valores listados no guia para aquele campo. Sem sentinelas como `Desconhecido` ou `Outros`.

> Verifique como `SpedValor` ou equivalente está implementado nos enums existentes (`Glob("src/**/Enums/*.cs")`, leia um) e replique o padrão exato.

## PASSO 6 — Escrever testes

Crie `tests/{ProjetoTests}/Registros/Bloco{X}/Registro{CODE}Tests.cs`.

**Estrutura base obrigatória:**

```csharp
using System.Reflection;

using TecnoFisc.Sped.Core.Abstracoes;
using TecnoFisc.Sped.Core.Atributos;
using TecnoFisc.Sped.Core.Catalogo;
using TecnoFisc.Sped.Core.Gerador;
using TecnoFisc.Sped.Core.Parser;
// + using para enums/value objects usados nos testes

namespace TecnoFisc.Sped.EfdContribuicoes.Tests.Registros.Bloco{X};

public sealed class Registro{CODE}Tests
{
    private static readonly IRegistroSpedCatalogo _catalogo =
        CatalogoBuilder.BuildFromAssembly(typeof(Registro{CODE}).Assembly);

    // --- Testes obrigatórios ---
}
```

**Testes obrigatórios (implemente todos):**

1. `Atributo_Declara{CODE}_Nivel{N}_Bloco{X}` — verifica atributo `[RegistroSped]`
2. `Catalogo_ExpoeRegistro{CODE}Com{N}CamposNaOrdem` — verifica lista de campos e ordens via `meta.Campos`
3. `Definidor_AtribuiTodosOsCampos` — chama `meta.Campos[i].Definidor(registro, valor.AsSpan())` para todos os campos e verifica propriedades
4. `Definidor_CampoVazio_DevolveNulo` — campos opcionais com `ReadOnlySpan<char>.Empty` retornam null
5. `RoundTrip_ComTodosOsCampos_PreservaTextoCanonico` — linha SPED completa (cópia literal do guia, adaptada para exemplo válido)
6. Pelo menos 1 `RoundTrip_Com{Cenario}_PreservaTextoCanonico` adicional (cenário parcial: campos opcionais vazios, filhos, etc.)

**Testes adicionais para enums (se houver):**
- `[Theory]` com `[InlineData]` para cada valor do enum → código SPED esperado
- Teste de serialização com valor null → empty

**Helper de round-trip (copiar exato):**

```csharp
private static async Task<string> RoundTripAsync(string sped, CancellationToken cancelamento)
{
    var leitor = new LeitorSpedTxt(_catalogo);
    var escritor = new EscritorSpedTxt(_catalogo);

    using var entrada = new MemoryStream(EncodingSped.Latin1.GetBytes(sped));
    var registros = new List<RegistroSped>();
    await foreach (var registro in leitor.LerAsync(entrada, cancelamento))
        registros.Add(registro);

    using var saida = new MemoryStream();
    await escritor.EscreverAsync(saida, registros, cancelamento);

    return EncodingSped.Latin1.GetString(saida.ToArray());
}
```

**Construção de linhas SPED para testes:**
- Formato: `|{CODE}|campo2|campo3|...|campoN|\r\n`
- Campos opcionais vazios: `||`
- Números decimais: ponto como separador (ex.: `1234.56`)
- Datas: `DDMMAAAA`

## PASSO 7 — Build e testes

Execute em sequência:

```powershell
dotnet build TecnoFisc.Sped.slnx
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~Registro{CODE}"
```

Se houver erros de compilação, corrija-os antes de prosseguir. Se testes falharem, investigue e corrija (não pule testes).

## PASSO 8 — Atualizar tracking file

No arquivo `STAGE_N_REGISTROS.md`, marque cada sub-estágio implementado:
- `| [ ] |` → `| [x] |`

Faça isso para todos os sub-estágios cobertos nesta execução.

## PASSO 9 — Commit

Stage exatamente os arquivos criados/modificados:

```powershell
git add src/TecnoFisc.Sped.EfdContribuicoes/Registros/Bloco{X}/Registro{CODE}.cs
git add tests/TecnoFisc.Sped.EfdContribuicoes.Tests/Registros/Bloco{X}/Registro{CODE}Tests.cs
# + enums e value objects se criados
git add STAGE_4_REGISTROS.md
```

Mensagem de commit (Conventional Commits em inglês, corpo em português):

- 1 registro: `feat(efd-contribuicoes): adiciona Registro{CODE} (sub-stage {N.NNN})`
- Batch: `feat(efd-contribuicoes): adiciona Registros {CODEs} — Bloco {X} batch (sub-stages {N.NNN}-{N.MMM})`

Se enums foram criados, mencione no corpo: `Cria enum IndicadorXxx (first-use).`

## PASSO 10 — Criar PR

```powershell
gh pr create `
  --title "feat: Registro {CODE} — {Descrição} (sub-stage {N.NNN})" `
  --body "..." `
  --base dev
```

**Corpo do PR deve conter:**
- Lista de sub-estágios cobertos com código e descrição
- Lista de campos implementados (nome, tipo, obrigatoriedade)
- Enums/value objects criados nesta PR (se houver)
- Referência à página PDF (`Guia Prático v1.35, p. {PAGINA}`)
- Checklist: `- [x] Build passando`, `- [x] Testes passando`, `- [x] Round-trip verificado`, `- [x] Tracking file atualizado`

---

## Regras invioláveis

1. **SEMPRE leia o PDF antes de implementar** — nunca assuma campos por inferência
2. Português para nomes fiscais (campos, classes, enums); inglês para infra técnica (tests, helpers, builders)
3. `sealed partial class` em todos os registros
4. Sem comentários além do docstring de classe e WHY não-óbvio
5. Sem runtime dependencies externas
6. Encoding SPED = Latin1/Windows-1252 (`EncodingSped.Latin1`)
7. Round-trip é obrigatório — se falhar, o registro está errado
8. Marcar tracking file ANTES do commit, não depois
9. Branch sempre a partir de `dev`, PR sempre com `--base dev`
10. Nunca implementar mais de 10 registros em uma única PR

## Tratamento de erros comuns

**Enum com atributo desconhecido:** Verifique como `SpedValor` (ou atributo equivalente) está implementado em um enum existente e replique o padrão.

**Campo com tipo forte (Cnpj, etc.) falhando no catálogo:** O conversor do tipo forte precisa estar registrado em `ConversoresPrimitivosCatalogo`. Verifique se o tipo existe lá antes de usá-lo.

**Round-trip diverge:** Compare o texto gerado com o input. Geralmente indica `Ordem` incorreta, `Tamanho` com asterisco (fixo) vs. variável, ou `Formato` de data ausente.

**Build falha por namespace:** Confirme que o `using` da namespace está correto e que o arquivo está no caminho certo.
