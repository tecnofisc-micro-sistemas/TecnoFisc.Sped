# Design — Parsing tolerante no TecnoFisc.Sped

> **Origem:** proposta do consumidor FiscTax em `docs/proposta-tecnofisc-sped-parsing-tolerante.md`.
> Este documento é o design validado (decisões fechadas) para implementação no TecnoFisc.Sped.
> **Escopo:** P1 + P2 + P3 + P4, todos opt-in.

## 1. Objetivo

Permitir que o parser .txt **não aborte a linha/arquivo** quando um campo individual falha a conversão
(ex.: `ChaveAcesso` com DV mod-11 inválido) ou quando um código de registro é desconhecido pelo catálogo.
O consumidor (FiscTax) precisa "granularizar tudo o que é capaz" e tratar o resto como apontamento, sem
que um dado sujo de terceiros derrube o arquivo inteiro.

**Invariante absoluta:** todo o comportamento novo é **opt-in**. Com as opções no default
(`false`), o parser reproduz o comportamento atual **byte a byte** — nenhum teste existente muda de
resultado.

## 2. Premissas verificadas no código (sem drift)

- O `catch` que aborta por erro de campo é único: `LeitorSpedTxt.InterpretarLinha`, linhas ≈509–516,
  filtrando `FormatException | ArgumentException | OverflowException`.
- Há **três** sítios de chamada a `campo.Definidor(...)`: campo normal (≈507), `CapturaTudo` (≈487) e
  `CampoArquivo` (≈499/502/504), todos dentro do mesmo `try`.
- O único erro de **layout** que aborta hoje é **código de registro desconhecido** (≈linha 467,
  `ErroLayoutSpedException`). `PilhaHierarquica.Empilhar` **não lança** — resolve o melhor pai possível.
  Não existe caminho de "hierarquia inconsistente" que estoure exceção. P3 cobre apenas o código
  desconhecido.
- Tipos de apoio já existem: `ErroFormato` (record posicional `Linha/CodigoRegistro/Campo/Mensagem`),
  `ErroLayout`/`ErroLayoutSpedException`, `ResultadoParse<T>`, `ReadingOptions` (com precedente de
  filtros `RegistrosIgnorados`/`BlocosIgnorados` e fast-path `HasFilter`).
- `LeitorSpedTxt` guarda `_catalogo` e `_opcoes`; os parsers de formato (`ParserEfdContribuicoes`,
  `ParserEfdIcmsIpi`, ECD) o embrulham e repassam `ReadingOptions`.

## 3. P1 — modo leniente de campo

### 3.1 Flag em `ReadingOptions`

```csharp
/// <summary>
/// Quando <c>true</c>, uma falha de conversão de campo (FormatException/ArgumentException/
/// OverflowException no Definidor) NÃO aborta a leitura: o campo fica no default, o erro é
/// acumulado em <see cref="RegistroSped.ErrosDeFormato"/> e o parsing continua. Padrão: <c>false</c>
/// (comportamento atual — lança ErroFormatoSpedException no primeiro erro de campo).
/// Não afeta erros de LAYOUT (registro desconhecido) — ver <see cref="LenientLayout"/>.
/// </summary>
public bool LenientFieldParsing { get; init; } = false;
```

### 3.2 Acúmulo no registro base

Em `RegistroSped` (`Txt.Engine.Abstracoes.RegistroSped`):

```csharp
private List<ErroFormato>? _errosDeFormato;

/// <summary>
/// Erros de conversão de campo capturados em modo leniente (ver ReadingOptions.LenientFieldParsing
/// e ParseLinha). Vazia quando o registro foi lido sem problemas ou em modo estrito. O campo
/// correspondente a cada erro permanece no valor default.
/// </summary>
public IReadOnlyList<ErroFormato> ErrosDeFormato => _errosDeFormato ?? (IReadOnlyList<ErroFormato>)[];

internal void RegistrarErroDeFormato(ErroFormato erro) => (_errosDeFormato ??= []).Add(erro);
```

Lista lazy (`null` até o primeiro erro) → **zero alocação no caminho feliz** (hard rule de performance).

### 3.3 `InterpretarLinha` — helper local

Encapsular as chamadas a `Definidor` num helper local **síncrono** (para poder receber o
`ReadOnlySpan<char>` por parâmetro — span não pode ser capturado em closure):

```csharp
void Definir(MetadadosCampo campo, ReadOnlySpan<char> valor)
{
    try
    {
        campo.Definidor(registro!, valor);
    }
    catch (Exception ex) when (ex is FormatException or ArgumentException or OverflowException)
    {
        var erro = new ErroFormato(numeroLinha, metadados!.Codigo, campo.Nome, ex.Message)
        {
            ValorBruto = valor.ToString()   // ver P2
        };
        if (!_opcoes.LenientFieldParsing)
            throw new ErroFormatoSpedException(erro, ex);
        registro!.RegistrarErroDeFormato(erro);   // default permanece; segue para o próximo campo
    }
}
```

Os três sítios (`campo.Definidor(registro, fatia)`, `CapturaTudo`, `CampoArquivo`) passam a chamar
`Definir(...)`. Se a estrutura dificultar o helper único nos branches `CapturaTudo`/`CampoArquivo`
(que têm `break` e chamadas adicionais), a alternativa equivalente é replicar o `try/catch` ramificado
nesses sítios — funcionalmente idêntico; decidir na implementação pelo que ficar mais limpo.

## 4. P2 — `ErroFormato.ValorBruto`

Propriedade aditiva no record posicional (não quebra chamadores, mantém o construtor posicional):

```csharp
public sealed record ErroFormato(long Linha, string? CodigoRegistro, string? Campo, string Mensagem)
{
    /// <summary>Texto cru do campo que falhou a conversão (preservado para o consumidor). Null quando
    /// o erro não está associado a um valor de campo específico (ex.: linha sem '|').</summary>
    public string? ValorBruto { get; init; }

    // ToString() existente permanece inalterado.
}
```

Preenchida no ponto de captura (P1 e P4). `Campo` + `ValorBruto` + `CodigoRegistro` + `Linha` dão ao
consumidor tudo para mapear o erro de volta ao registro cru e materializar o apontamento.

## 5. P3 — tolerância a layout (registro desconhecido)

### 5.1 Flag separada

```csharp
/// <summary>
/// Quando <c>true</c>, um código de registro desconhecido pelo catálogo NÃO aborta a leitura: o
/// leitor emite um <see cref="RegistroNaoReconhecido"/> (linha crua + erro) como folha na hierarquia
/// e continua. Padrão: <c>false</c> (lança ErroLayoutSpedException, comportamento atual).
/// Independente de <see cref="LenientFieldParsing"/>.
/// </summary>
public bool LenientLayout { get; init; } = false;
```

### 5.2 Registro-sentinela

Novo `RegistroNaoReconhecido : RegistroSped` em `Txt.Engine.Abstracoes`:

- `Codigo` retorna o código cru lido (a `fatia` da posição 1).
- Carrega a **linha crua completa** (string) e o `ErroLayout` correspondente.
- É um substantivo fiscal/domínio → nome em PT (`RegistroNaoReconhecido`).

### 5.3 Comportamento na hierarquia — sempre folha

No ponto de código desconhecido (≈linha 467): se `!LenientLayout` → lança como hoje; senão:

- Instancia `RegistroNaoReconhecido` com a linha crua + `ErroLayout`.
- **Pendura como folha no topo atual da pilha** (`pilha.Topo?.AdicionarFilho(sentinela)`) **sem
  empilhá-lo**. Consequência: o sentinela nunca vira pai; registros conhecidos seguintes se ancoram no
  pai real vigente, ignorando o sentinela. Não há adivinhação de nível.
- Yielda o sentinela no stream (o fluxo `await foreach` permanece uniforme: tudo é `RegistroSped`).

## 6. P4 — `ParseLinha` (linha isolada, sempre tolerante)

```csharp
/// <summary>
/// Parseia uma única linha SPED canônica (|REG|...|) isoladamente, sem hierarquia nem streaming.
/// Nunca lança por erro de campo: o registro (em Valor) traz os campos conversíveis preenchidos e os
/// que falharam no valor default, com os erros acumulados em RegistroSped.ErrosDeFormato.
/// Falha apenas quando nenhum registro pôde ser produzido (código desconhecido, linha sem '|').
/// </summary>
ResultadoParse<RegistroSped> ParseLinha(ReadOnlySpan<char> linha, long numeroLinha = 0);
```

Mora em `LeitorSpedTxt`; os parsers de formato delegam. Roda **sempre leniente internamente**
(independe das flags e do streaming; sem vinculação Pai/Filhos).

### Semântica (decisão fechada)

- Produziu registro (código conhecido) → `ResultadoParse.Ok(registro)`. Erros de campo ficam **só** em
  `registro.ErrosDeFormato`. `Sucesso == true` **mesmo com erros de campo**.
- Nenhum registro produzido (código desconhecido pelo catálogo, ou linha sem `|` inicial/final) →
  `ResultadoParse.Falhar(erro)` (com `ErroFormato.ValorBruto` quando aplicável).
- **Canal único** para erro de campo (o registro). Não cria construtor "Ok com erros" em
  `ResultadoParse`. `ResultadoParse.Erros` carrega apenas o erro estrutural da falha total.

## 7. Propagação pela superfície pública

- Os parsers de formato (`ParserEfdContribuicoes`, `ParserEfdIcmsIpi`, ECD) já recebem/repassam
  `ReadingOptions`; as novas flags fluem por esse canal sem mudança de assinatura.
- `ReadStreamingAsync`/`ReadAsync` mantêm assinatura. `ParseLinha` é adicionada como novo método;
  cada parser de formato expõe um overload delegando ao `LeitorSpedTxt` com seu catálogo.
- Nenhuma mudança em value objects, source generator ou catálogo.

## 8. Testes

Cobertura mínima (xUnit + FluentAssertions, padrão do repo):

**P1 / P2:**
- Estrito (default) inalterado: `C100.ChvNfe` com DV inválido → `ErroFormatoSpedException` (como hoje).
- Leniente, campo nullable: mesma linha com `LenientFieldParsing = true` → registro emitido,
  `ChvNfe == null`, `ErrosDeFormato` com 1 item (`Campo == "ChvNfe"`, `ValorBruto == "3225...6541"`);
  demais campos do `C100` preenchidos.
- Leniente, value type: `C170` com `Cfop` inválido → registro emitido, `Cfop == default`, erro
  acumulado, resto preenchido.
- Múltiplos erros na mesma linha → `ErrosDeFormato.Count == 2`, não aborta.
- Arquivo inteiro: N linhas, uma com campo ruim → leniente emite as N (a ruim no default + diagnóstico);
  estrito aborta na ruim.
- Caminho feliz não aloca: registro sem erros → `_errosDeFormato == null`, `ErrosDeFormato` é a
  instância vazia compartilhada.

**P3:**
- Código desconhecido em estrito → `ErroLayoutSpedException` (como hoje).
- Código desconhecido com `LenientLayout = true` → stream emite `RegistroNaoReconhecido` (linha crua +
  `ErroLayout`), pendurado como folha no pai vigente, e os registros conhecidos seguintes mantêm o pai
  correto (sentinela não vira pai).

**P4:**
- Linha com campo ruim → `Sucesso == true`, `Valor.ErrosDeFormato.Count == 1`.
- Código desconhecido → `Falha`, erro estrutural em `Erros`.
- Linha sem `|` → `Falha` com `ValorBruto`/mensagem apropriada.
- Linha limpa → `Ok`, `ErrosDeFormato` vazio.

## 9. Performance

- Lista de erros lazy = zero alocação no caminho feliz.
- `catch` só é exercido quando há erro; o `when` filtra exatamente as exceções já tratadas hoje.
- Helper síncrono evita captura de span em closure.
- **Hard rule §5:** validar com benchmark de regressão no caminho estrito (sem flags) — não pode
  regredir vs. baseline.

## 10. Conformidade (ARCHITECTURE.md)

- **Idioma:** EN para flags/verbos/capacidades (`LenientFieldParsing`, `LenientLayout`,
  `RegistrarErroDeFormato`, `ParseLinha`); PT para substantivos fiscais/domínio (`ErroFormato`,
  `ErrosDeFormato`, `ValorBruto`, `RegistroNaoReconhecido`).
- **Opt-in / backward-compatible:** defaults `false`; nenhum teste existente muda.
- **Não vira validador fiscal (§2.3):** só sinaliza não-conformidade de **formato**, nada de regra
  tributária. A inteligência de apontamento/correção é do FiscTax.
- **Sem mudança na superfície dos parsers de streaming.**

## 11. Fora de escopo

- Correção/normalização do valor inválido (ex.: recalcular DV) — domínio do FiscTax.
- Validação de regra tributária / obrigatoriedade condicional / cross-registro (§2.3 — consumidor).
- Caminho de escrita (gerador): a retificadora do FiscTax reproduz registros intocados verbatim a
  partir do cru; o valor inválido round-trips sem passar pelo gerador tipado. Se um caso futuro exigir
  escrever de volta um campo que não cabe no value object, abre-se proposta própria.
