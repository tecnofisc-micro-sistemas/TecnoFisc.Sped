# Proposta para o TecnoFisc.Sped — parsing tolerante (campo falho não aborta a linha/arquivo)

> **Origem:** repositório consumidor **FiscTax** (inteligência tributária — retificação de EFD
> Contribuições). Este documento foi escrito **no** FiscTax para ser recortado e colado **no** repo
> `TecnoFisc.Sped`. Ele é a especificação de uma mudança que o FiscTax precisa que a biblioteca
> implemente. Todas as referências de código apontam para arquivos do **TecnoFisc.Sped**, com
> `arquivo:linha` aproximados (podem ter drift — confira o trecho citado).
>
> **Conformidade com as convenções do TecnoFisc.Sped:** a proposta respeita (i) a regra de idioma
> (substantivos fiscais em PT, verbos/predicados/capacidades em EN), (ii) a postura "não é validador de
> correção fiscal — valida só conformidade de formato" (§2.3), (iii) performance-first com zero overhead
> no caminho atual, e (iv) compatibilidade retroativa total (tudo é **opt-in**).

---

## 1. Contexto do consumidor — por que o FiscTax precisa disto

O FiscTax tem uma postura de produto explícita: **ele deve ser o mais agnóstico possível dos arquivos
SPED**. O SPED é uma **fonte de alimentação descartável**, não o modelo de trabalho. O FiscTax importa o
arquivo para uma **camada crua** (linha SPED canônica verbatim) e depois **granulariza** essa camada para
o seu domínio próprio (operação canônica + incidência tipada), re-alimentando cada linha no parser tipado
do TecnoFisc.Sped para obter os value objects (`Cfop`, `Ncm`, `ChaveAcesso`, decimais, datas).

Disso decorre uma regra de ferro do FiscTax: **a granularização não nega operação por causa de dado
sujo.** O objetivo é **granularizar TUDO** o que for capaz, e o que **não** for possível converter vira um
**apontamento em tela** para o usuário tratar depois — nunca um motivo para abortar. Concretamente, a
regra do FiscTax é:

> Se um campo falha a conversão, o sistema **granulariza tudo o que é capaz daquele registro**, deixa o
> campo que falhou no **valor default**, e **registra o problema** (com o valor cru preservado) como
> apontamento.

Hoje a biblioteca impede essa postura: **um único campo malformado aborta a linha inteira** e, no fluxo de
streaming do FiscTax, **derruba a granularização do arquivo inteiro**.

---

## 2. O caso concreto que quebrou

Arquivo real de EFD Contribuições, registro `C100`, campo `ChvNfe`:

```
Linha 236 (C100.ChvNfe): Valor não é uma chave de acesso válida:
'32251100011998216756550252411200013640116541'.
```

A chave tem 44 dígitos — o que falha é o **dígito verificador (mod-11)** (ou um dos campos embutidos:
cUF/CNPJ). É **dado real sujo de terceiros**, clássico em SPED. E a chave de acesso é **metadado puro** do
documento: **não entra em nenhuma conta** de apuração. Abortar a granularização do arquivo inteiro por
causa dela é exatamente o que não pode acontecer.

No FiscTax isso aparece como `ErroFormatoSpedException` borbulhando de
`ParserEfdContribuicoes.ReadStreamingAsync` para dentro do `await foreach` do consumidor, que então aborta
o arquivo. O log mostra o arquivo inteiro falhando, repetidamente (re-tentativas).

---

## 3. Causa-raiz no TecnoFisc.Sped (caminho exato)

O ponto de estrangulamento é **único** e já está perfeitamente isolado:

`src/TecnoFisc.Sped.Txt.Engine/Parser/LeitorSpedTxt.cs`, método `InterpretarLinha`, o `try/catch` em
torno do `Definidor` do campo (≈ linhas 507-516):

```csharp
try
{
    // ... (CapturaTudo / CampoArquivo) ...
    campo.Definidor(registro, fatia);          // chama o setter gerado
}
catch (Exception ex) when (ex is FormatException
                              or ArgumentException
                              or OverflowException)
{
    throw new ErroFormatoSpedException(         // <-- aborta a linha inteira
        new ErroFormato(numeroLinha, metadados.Codigo, campo.Nome, ex.Message),
        ex);
}
```

O `Definidor` é o setter gerado pelo source generator
(`RegistroSpedCatalogoGenerator` → `CatalogoSpedGerado.g.cs`), por exemplo `Set_C100_ChvNfe`, que chama
`ChaveAcesso.Create(valor)` (em `src/TecnoFisc.Sped.Core/ValueObjects/ChaveAcesso.cs`). O `Create` lança
`FormatException` quando `TentarCriar` falha a validação de DV/cUF/CNPJ.

**Fatos que tornam a correção barata e segura:**

1. **A captura já está num único lugar** — não há `try/catch` espalhado por setter; é o `catch` acima.
2. **O campo já tem default natural na falha.** Como a atribuição (`campo.Definidor`) lança **antes** de
   escrever a propriedade, o campo simplesmente **permanece no default do construtor** (`null` para campos
   nullable como `ChvNfe`, `default(T)` para value types). Não é preciso "setar default" — basta capturar e
   seguir para o próximo campo.
3. **Os tipos de diagnóstico já existem:** `ErroFormato` (record com `Linha`/`CodigoRegistro`/`Campo`/
   `Mensagem`) e `ResultadoParse<T>` em `src/TecnoFisc.Sped.Core/Erros/`.
4. **Já há precedente de opções de leitura:** `ReadingOptions`
   (`src/TecnoFisc.Sped.Txt.Engine/Parser/ReadingOptions.cs`), hoje usado para
   `RegistrosIgnorados`/`BlocosIgnorados`.

---

## 4. Mudança pedida — prioridades

### P1 (MÍNIMO — desbloqueia o FiscTax): modo leniente de campo

> Esta é a "primeira questão" mencionada na conversa. **O mínimo a entregar.**

Um **modo opt-in** em que uma falha de conversão de **campo** não lança: o campo fica no default, o erro é
**acumulado no próprio registro**, e o parsing **continua** (próximo campo, próxima linha, arquivo
inteiro). O comportamento padrão (lançar no primeiro erro) **permanece intacto**.

**(a) Flag em `ReadingOptions`** (nome em EN por ser capacidade técnica, conforme §1.3 do ARCHITECTURE):

```csharp
/// <summary>
/// Quando <c>true</c>, uma falha de conversão de um campo (FormatException/ArgumentException/
/// OverflowException no Definidor) NÃO aborta a leitura: o campo fica no default, o erro é
/// acumulado em <see cref="RegistroSped.ErrosDeFormato"/> e o parsing continua. Padrão: <c>false</c>
/// (comportamento atual — lança ErroFormatoSpedException no primeiro erro de campo).
/// Não afeta erros de LAYOUT (registro desconhecido/hierarquia) — ver P3.
/// </summary>
public bool LenientFieldParsing { get; init; } = false;
```

**(b) Acúmulo no registro** — `RegistroSped` base
(`src/TecnoFisc.Sped.Txt.Engine/Abstracoes/RegistroSped.cs`):

```csharp
private List<ErroFormato>? _errosDeFormato;

/// <summary>
/// Erros de conversão de campo capturados em modo leniente (ver ReadingOptions.LenientFieldParsing).
/// Vazia quando o registro foi lido sem problemas ou em modo estrito. O campo correspondente a cada
/// erro permanece no valor default.
/// </summary>
public IReadOnlyList<ErroFormato> ErrosDeFormato => _errosDeFormato ?? (IReadOnlyList<ErroFormato>)[];

internal void RegistrarErroDeFormato(ErroFormato erro) => (_errosDeFormato ??= []).Add(erro);
```

> Lista lazy (`null` até o primeiro erro) → **zero alocação** no caminho feliz, preservando a postura
> performance-first (§4.4). O nome `ErrosDeFormato` espelha o tipo `ErroFormato` (substantivo já existente
> no repo); ajustar se preferirem outra grafia.

**(c) `InterpretarLinha`** — o único ponto que muda. O `catch` passa a ramificar pela flag. Como há três
sítios de `Definidor` (campo normal, `CapturaTudo`, `CampoArquivo`), o mais limpo é encapsular a chamada
num helper local que decide lançar ou acumular:

```csharp
// dentro de InterpretarLinha, substituindo as chamadas diretas campo.Definidor(...)
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
        registro!.RegistrarErroDeFormato(erro);   // default fica; segue o baile
    }
}
```

> Atenção a `ReadOnlySpan<char>` não poder ser capturado em closure/`async`: o helper acima é **síncrono e
> local**, então `valor` (o span) pode ser passado por parâmetro. Se a estrutura atual dificultar o helper,
> a alternativa é repetir o `try/catch` ramificado nos três sítios — funcionalmente idêntico.

**Invasividade:** baixa. Um arquivo de motor, um da base, um de opções. Sem tocar value objects, source
generator, catálogo, nem a API pública dos parsers. **100% backward-compatible** (default `false`).

---

### P2 (MÍNIMO, junto do P1): diagnóstico carrega o valor cru estruturado

Para o FiscTax construir o apontamento e **preservar o valor original** (ex.: guardar a chave inválida
verbatim para o usuário corrigir depois), o `ErroFormato` precisa expor o **valor cru** de forma
**estruturada** — hoje ele só aparece embutido na `Mensagem` (`"...: '3225...'"`), o que obrigaria o
consumidor a fazer parsing de string.

Em `src/TecnoFisc.Sped.Core/Erros/ErroFormato.cs`, adicionar uma propriedade `init` **nullable** (mantém o
record posicional atual sem quebrar chamadores):

```csharp
public sealed record ErroFormato(
    long Linha,
    string? CodigoRegistro,
    string? Campo,
    string Mensagem)
{
    /// <summary>Texto cru do campo que falhou a conversão (preservado para o consumidor). Null quando
    /// o erro não está associado a um valor de campo específico (ex.: linha sem '|').</summary>
    public string? ValorBruto { get; init; }

    // ToString() existente permanece
}
```

Com `Campo` (nome da propriedade, ex.: `"ChvNfe"`) + `ValorBruto` + `CodigoRegistro` + `Linha`, o FiscTax
tem tudo para mapear o erro de volta ao seu registro cru e materializar o apontamento.

**Invasividade:** trivial. Adição de propriedade opcional.

---

### P3 (DESEJÁVEL): tolerância a erro de LAYOUT (registro desconhecido / hierarquia)

O FiscTax re-alimenta o parser com o arquivo **reconstruído da camada crua**. Se o cru contiver um
**código de registro desconhecido pelo catálogo** (leiaute novo, registro fora do escopo modelado) ou uma
**hierarquia inconsistente**, hoje `InterpretarLinha` lança `ErroLayoutSpedException` (≈ linha 467) e
**aborta o arquivo** — a mesma patologia do P1, mas na dimensão de layout.

Coerente com a filosofia "granulariza tudo o que é capaz", seria desejável um modo em que erro de layout
**não aborta**: o registro problemático é **pulado** (ou devolvido como sentinela "não reconhecido") com um
diagnóstico, e o resto do arquivo segue.

Como `ErroLayout` **não tem um `RegistroSped` onde se ancorar** (o registro é justamente o desconhecido), o
surfacing é diferente do P1. Opções (decisão do TecnoFisc.Sped):

- **Callback/sink de diagnósticos** em `ReadingOptions` (ex.: `Action<ErroLayout>? OnLayoutError`), ou
- um **registro-sentinela** `RegistroNaoReconhecido : RegistroSped` (carrega a linha crua + o erro) que o
  parser yielda quando leniente, ou
- pular silenciosamente acumulando num canal de diagnósticos do leitor.

Sugiro **não** sobre-projetar agora: gate pela mesma flag (`LenientFieldParsing`) ou uma irmã
(`LenientLayout`), e o surfacing mais barato que sirva. O FiscTax consegue conviver com "pula e me avisa".

**Invasividade:** média (depende do surfacing escolhido). Pode ficar para uma segunda iteração se o P1+P2
forem entregues primeiro.

---

### P4 (CONSIDERAR — encaixe estratégico de longo prazo): API de parse por linha única, tolerante

O modelo do FiscTax é **endereçável por linha**: a camada crua guarda cada registro SPED individualmente,
com seu código e o vínculo de pai (`id_surrogate` / `id_pai_surrogate`). Hoje o FiscTax **reconstrói o
texto inteiro** e usa `ReadStreamingAsync` só para retipar — herdando a hierarquia do parser, quando já a
tem no cru.

Uma API que **parseia uma linha isolada** e devolve `ResultadoParse<RegistroSped>` (tolerante por natureza)
seria o encaixe perfeito para esse modelo: o FiscTax parsearia **registro a registro do cru**, cada um
independente, costurando o pai pelo próprio cru — e **uma linha ruim jamais afetaria outra**, sem depender
de streaming nem de reconstrução de arquivo.

Esboço (em `LeitorSpedTxt` ou no parser de formato, reusando `MetadadosRegistro?` já resolvido pelo
catálogo):

```csharp
/// <summary>
/// Parseia uma única linha SPED canônica (|REG|...|) isoladamente, sem hierarquia nem streaming.
/// Nunca lança por erro de campo: erros vêm em ResultadoParse.Erros e o registro (em Valor) traz os
/// campos conversíveis preenchidos e os que falharam no default.
/// </summary>
ResultadoParse<RegistroSped> ParseLinha(ReadOnlySpan<char> linha, long numeroLinha = 0);
```

`ResultadoParse<T>` já existe exatamente para isto (`Core/Erros/ResultadoParse.cs`) — hoje sem uso no
leitor. Esta API o aproveitaria e daria ao FiscTax (e a qualquer consumidor com store endereçável por
linha) o caminho mais robusto possível.

**Observação:** P4 é independente de P1. P1 desbloqueia já; P4 é a forma elegante que o FiscTax pode adotar
numa sessão futura. Avaliem o custo/benefício — não é pré-requisito.

---

## 5. Divisão de responsabilidade (contrato com o FiscTax)

Para o TecnoFisc.Sped entender a fronteira (a lib **não vira validador fiscal**, §2.3):

| Responsabilidade | Onde mora |
|---|---|
| Detectar que um campo não conforma o formato e **não abortar** | **TecnoFisc.Sped** (P1) |
| Expor **o que** falhou, **onde** e **o valor cru** | **TecnoFisc.Sped** (P2) |
| Decidir que isso vira "apontamento", classificar materialidade, oferecer ferramenta de correção | **FiscTax** (domínio próprio) |
| Preservar o valor cru verbatim para reprodução na retificadora | **FiscTax** (camada crua) |

A lib só precisa **parar de abortar** e **entregar o diagnóstico estruturado**. Toda a inteligência de
apontamento e correção é do FiscTax.

---

## 6. Checklist de conformidade (TecnoFisc.Sped)

- [ ] **Opt-in / backward-compatible:** padrão `LenientFieldParsing = false` reproduz o comportamento atual
      byte a byte. Nenhum teste existente muda de resultado.
- [ ] **Idioma:** flag/predicados/capacidades em EN (`LenientFieldParsing`, `RegistrarErroDeFormato`);
      substantivos fiscais/erro em PT (`ErroFormato`, `ErrosDeFormato`, `ValorBruto`).
- [ ] **Performance-first:** lista de erros **lazy** (zero alocação no caminho feliz); `catch` só é
      exercido quando há erro; o `when` filtra exatamente as exceções de conversão já tratadas hoje.
- [ ] **Não vira validador fiscal:** a lib só sinaliza não-conformidade de **formato** — nada de regra
      tributária (§2.3).
- [ ] **Sem mudança na superfície dos parsers:** `ReadStreamingAsync`/`ReadAsync` mantêm assinatura; o
      modo é configurado via `ReadingOptions` (precedente existente).

---

## 7. Testes sugeridos (no TecnoFisc.Sped)

- **Estrito (default) inalterado:** linha `C100` com `ChvNfe` de DV inválido → `ErroFormatoSpedException`
  (como hoje).
- **Leniente, campo nullable:** mesma linha com `LenientFieldParsing = true` → registro emitido, `ChvNfe ==
  null`, `ErrosDeFormato` com 1 item (`Campo == "ChvNfe"`, `ValorBruto == "3225...6541"`); demais campos do
  `C100` corretamente preenchidos.
- **Leniente, value type não-nullable:** linha `C170` com `Cfop` inválido → registro emitido, `Cfop ==
  default`, erro acumulado; o resto da operação preenchido.
- **Múltiplos erros na mesma linha:** dois campos ruins → `ErrosDeFormato.Count == 2`, parsing não aborta.
- **Arquivo inteiro:** arquivo com N linhas, uma com campo ruim → em leniente, as N linhas são emitidas (a
  ruim com o campo no default + diagnóstico); em estrito, aborta na linha ruim.
- **Caminho feliz não aloca:** registro sem erros → `ErrosDeFormato` é a instância vazia compartilhada
  (`_errosDeFormato == null`).

---

## 8. Fora de escopo desta proposta

- Correção/normalização do valor inválido (ex.: recalcular DV) — é **decisão de domínio do FiscTax**, não
  da lib.
- Validação de regra tributária, obrigatoriedade condicional, cross-registro (§2.3 — permanece do
  consumidor).
- Mudanças no caminho de **escrita** (gerador). A retificadora do FiscTax reproduz registros intocados
  **verbatim a partir do cru**, então o valor inválido round-trips sem passar pelo gerador tipado; se um
  caso futuro exigir escrever de volta um campo que não cabe no value object, abrimos proposta própria.

---

## 9. Resumo de uma linha

> Adicionar `ReadingOptions.LenientFieldParsing` (opt-in) que, no `catch` de conversão de campo em
> `LeitorSpedTxt.InterpretarLinha`, **acumula o erro no registro** (`RegistroSped.ErrosDeFormato`, com
> `ErroFormato.ValorBruto`) e **continua** em vez de lançar — para o FiscTax granularizar tudo e apontar o
> resto. P3 (layout tolerante) e P4 (parse por linha) são extensões desejáveis.
