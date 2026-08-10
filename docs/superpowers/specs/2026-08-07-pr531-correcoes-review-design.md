# Correções do review do PR 531 - design

Data: 2026-08-07. Status: aprovado.

## Objetivo

Tratar os dez achados verificados do code review do PR 531 (`feat/ecf-layout-12`) sem regredir os pacotes já publicados e sem reduzir o escopo read-only do ECF.

O PR entrega o pacote ECF leiaute 12, mas também altera o TXT Engine compartilhado. Como esse engine é consumido por EFD Contribuições, EFD ICMS-IPI e ECD já publicados no nuget.org, toda mudança de comportamento ali precisa ser deliberada e declarada. O objetivo central deste trabalho é separar as duas coisas: o que o ECF precisa passa a ser opt-in, e o que muda para todo mundo passa a ser explícito.

Origem dos achados: review multi-agente em nível `high` sobre `git diff origin/dev...origin/feat/ecf-layout-12` (56 arquivos, 35 candidatos, 29 verificados, 10 reportados).

## Princípio orientador

**Nenhum comportamento padrão do TXT Engine muda para os leiautes já publicados.** Toda a rigidez nova (validação de domínio de enum, vigência de leiaute) nasce desligada e é ligada explicitamente por quem a quer — no caso, o `ParserEcf`.

A única exceção deliberada é a unificação de enums duplicados (seção B3), que é breaking assumido e discutida na seção de versionamento.

## Decisões confirmadas

- Validação de domínio de enum é **opt-in** via `ReadingOptions`, não default.
- Vigência do leiaute no ECF tem default `true`, mas aceita override explícito do chamador.
- Descarte de registro por vigência nunca é silencioso.
- `LenientLayout` deixa de estourar em bloco desconhecido nos **quatro** leiautes.
- Nome de campo no ECF é sempre o nome normativo do manual, nos 180 registros.
- Enums contábeis duplicados são unificados em `Txt.Engine`, com ECD migrado — breaking assumido.
- O PR passa a ser `feat(ecf)!:` e promove o pacote a `1.0.0`.

## A. TXT Engine compartilhado

### A1 - Validação de domínio de enum vira opt-in

Achados 1 e 7.

O branch trocou o cast permissivo por uma checagem `Enum.IsDefined` que dispara `FormatException`. Sob as opções padrão (`LenientFieldParsing = false`) isso vira `ErroFormatoSpedException` e aborta o arquivo inteiro. Um consumidor que apenas atualiza o pacote vê importações que funcionavam passarem a estourar — por exemplo um `IND_TIPO_ITEM=12` que a Receita criou depois do enum, ou um código aposentado num arquivo arquivado. A exposição alcança cerca de 120 tipos de enum sem `[SpedValor]` nos três leiautes publicados.

O mesmo commit removeu o fallback `Enum.Parse(alvo, s)` do catálogo reflexivo, derrubando junto o parsing por nome de membro e a forma de flags separada por vírgula.

**Desenho.** Nova propriedade em `ReadingOptions`:

```csharp
public bool? ValidarDominioDeEnum { get; init; }
```

- `null` (padrão): decisão do leiaute. EFD Contribuições, EFD ICMS-IPI e ECD resolvem para `false`; ECF resolve para `true`.
- `false`: comportamento idêntico ao de `origin/dev` — cast permissivo para enums numéricos sem `[SpedValor]`, e `Enum.Parse` de volta para parsing por nome e por flags.
- `true`: código fora do domínio declarado vira `FormatException`, que segue o tratamento normal de erro de campo (aborta sob opções estritas, vira `ErroFormato` coletável sob `LenientFieldParsing`).

**Restrições de implementação.**

- A flag precisa existir nos **três lugares** que descrevem metadados de campo: o atributo/`MetadadosCampo`, o `CatalogoBuilder` reflexivo e o `RegistroSpedCatalogoGenerator`. Divergência entre catálogo gerado e reflexivo é um bug silencioso conhecido deste repositório.
- Custo zero no hot path quando desligada. Sem `Enum.IsDefined` por registro no caminho permissivo, sem reflection por registro em nenhum dos dois caminhos.
- Enums marcados com `[Flags]` continuam fora da validação, como no branch atual.

### A2 - Vigência de campo volta ao mapeamento posicional

Achado 4.

O branch substituiu `int indice = posicaoCampo - 2` por um cursor sequencial que avança sobre entradas de metadados cujo `DesdeVersao` excede o `COD_VER` do arquivo. O mapeamento antigo era auto-corretivo: cada coluna física ia para o campo daquela posição. O cursor assume que campo barrado por vigência está fisicamente ausente do arquivo. Quando a coluna existe mesmo assim — PVA que preenche todas as colunas, retificadora, arquivo montado à mão — o cursor consome a coluna com os metadados do campo *seguinte*, e todo o resto da linha desloca um campo à esquerda sem emitir nenhum `ErroFormato`.

A verificação no manifesto do leiaute 12 mostrou que **nenhum dos 180 registros tem campo com `sinceVersion` fora do fim do registro**. O cursor não compra nada com os dados atuais e só adiciona a superfície de desalinhamento.

**Desenho.** Restaurar o mapeamento posicional e apenas *pular a atribuição* quando o campo estiver inativo pela vigência. A coluna é consumida normalmente; o valor é descartado.

Teste de regressão obrigatório: registro sintético com campo barrado por vigência seguido de outros campos, alimentado com um arquivo que traz todas as colunas — os valores posteriores precisam continuar nos campos corretos.

### A3 - Descarte por vigência deixa de ser silencioso

Achado 2.

`ShouldIgnoreByVersion` hoje faz `continue` sem deixar rastro, e ainda corta a subárvore inteira via `nivelCorteVigencia`. O consumidor recebe um `ArquivoEcf` com registros e filhos faltando, sem erro, sem sentinela, sem contador. Os onze valores de `IntroduzidoEm` foram definidos à mão (N605, X360, X365, X366, X370, X371, X375, X485 em V010; X451 em V011; Y750 em V009; Y730 em V012) e as fixtures de aceitação dos leiautes 8 a 11 não exercitam essas portas.

**Desenho.** Cada linha descartada por vigência emite um `RegistroNaoReconhecido(codigo, linhaCrua, erroLayout)` — o mesmo veículo que o `LenientLayout` já usa. O `ErroLayout` carrega o motivo: registro posterior à versão declarada no `0000`. Vale para o registro barrado e para cada descendente cortado junto.

O consumidor que não se importa filtra a sentinela; o consumidor que se importa passa a ter como saber. Nada some sem sinal.

Isto compõe com A4: no `ArquivoEcf`, as sentinelas de vigência caem na coleção `RegistrosNaoReconhecidos`, então o descarte fica auditável tanto no streaming quanto no modelo carregado.

### A4 - LenientLayout para de estourar em bloco desconhecido

Achado 3.

Sob `LenientLayout = true` o leitor deliberadamente emite `RegistroNaoReconhecido` em vez de lançar, para que o chamador colete os erros de layout. Mas `Arquivo*.Adicionar` faz `char.ToUpperInvariant(codigo[0])` e lança `InvalidOperationException` para qualquer letra fora dos blocos conhecidos — e `ArgumentException` para código vazio. A leitura tolerante estoura mesmo assim, com um tipo de exceção que este repositório reserva para erro de programação.

O código é idêntico em `ArquivoEcd` e `ArquivoEfdIcmsIpi` no `dev`: é padrão pré-existente, não regressão deste PR. A correção vale para os quatro leiautes.

**Desenho.** `Adicionar` passa a rotear por tipo:

- `RegistroNaoReconhecido` → coleção `RegistrosNaoReconhecidos` exposta como `IReadOnlyList` no `Arquivo*`, nunca lança.
- Qualquer outro registro cujo bloco não exista → continua lançando `InvalidOperationException`. Isso é uso incorreto da API, não dado ruim, e deve continuar falhando alto.

A distinção por tipo cobre os casos do achado — linha `|1010|…|`, linha truncada, linha `||`, registro de leiaute futuro — porque todos chegam como `RegistroNaoReconhecido` no modo tolerante.

### A5 - Gerador emite o catálogo mesmo com diagnóstico

Achado 6.

Um único diagnóstico TFSPED001/TFSPED002 faz `RegisterSourceOutput` retornar antes de `AddSource`, então nem `CatalogoSpedGerado` nem `IRegistroSpedVisitor` são emitidos para o assembly inteiro. O build passa a falhar com centenas de CS0246 vindos de `ParserEcf.cs` e de cada arquivo de teste, soterrando o único diagnóstico acionável.

**Desenho.** Remover o `if (hasErrors) return;`. Os diagnósticos continuam sendo reportados e continuam falhando o build; a emissão prossegue, usando o nome CLR como fallback para os campos com alias inválido. A lista de erros passa a conter o problema real, não a cascata.

Teste: cenário de gerador com alias inválido produz exatamente um diagnóstico e o catálogo continua presente.

### A6 - Ordem do catálogo vira contrato explícito

Achado 8.

`OrdenarRegistros` trocou um `OrderBy(Codigo, Ordinal)` por um ranking de bloco (`0` → letras → dígitos 1-8 → `9`). Como `CatalogoSpedGerado.EnumerarRegistros()` é API pública, a ordem observável de enumeração dos três pacotes publicados muda nesta release sem que nenhum deles tenha pedido e sem teste correspondente nas respectivas suítes.

**Desenho.** Manter a ordenação — a ordem canônica de bloco é semanticamente correta para todos os leiautes SPED. Mas transformá-la em contrato: teste de ordem de enumeração nos quatro módulos e entrada no `CHANGELOG.md` registrando a mudança.

### A7 - Benchmarks das mudanças em hot path

Achado 9.

A regra 5 do `CLAUDE.md` exige benchmark do BenchmarkDotNet para código sensível a performance. O PR mexe nos laços por registro e por campo do `LeitorSpedTxt` e no conversor de enum do `CatalogoBuilder` — os dois caminhos mais quentes da biblioteca — sem adicionar nada a `benchmarks/TecnoFisc.Sped.Benchmarks`.

**Desenho.** Dois benchmarks:

- `ParserVigenciaBenchmark`: leitura com e sem `RespeitarVigenciaDoLeiaute`, medindo o custo do gate por registro e do `CampoAtivo` por campo.
- Extensão do `ParserCatalogoBenchmark` existente com e sem `ValidarDominioDeEnum`, confirmando que o caminho desligado não paga nada.

Baseline registrado no corpo do PR.

## B. ECF

### B1 - ParserEcf respeita override do chamador

Achado 2.

`ComVigenciaDoLeiaute` reconstrói as `ReadingOptions` do chamador e sobrescreve `RespeitarVigenciaDoLeiaute` com `true`, inclusive quando o chamador passou uma instância explícita. Não há opt-out por nenhuma sobrecarga de construtor.

**Desenho.** As duas flags de rigidez viram `bool?` em `ReadingOptions`:

```csharp
public bool? RespeitarVigenciaDoLeiaute { get; init; }
public bool? ValidarDominioDeEnum { get; init; }
```

`null` significa "decisão do leiaute": o `ParserEcf` resolve as duas para `true`, os demais parsers resolvem para `false`. Valor explícito do chamador sempre vence. Um consumidor de ECF que quer o arquivo bruto completo passa `RespeitarVigenciaDoLeiaute = false` e recebe tudo.

### B2 - Nome normativo em todos os campos ECF

Achado 5.

O atributo `Nome` é novo neste PR e foi aplicado a 54 dos 180 registros, de forma irregular: em `RegistroM510` o campo 12 recebe `IND_SD_FIM_LAL` mas o campo 6 equivalente não recebe nada e reporta `IndSdIniLal`. Em `RegistroM500`, quatro dos dez campos reportam nome normativo e seis reportam nome CLR. Um consumidor que monta mapa nome→valor a partir de `MetadadosRegistro.Campos`, ou que roteia `ErroFormato.Campo` contra a lista normativa, resolve parte dos campos e perde o resto.

O harness não detecta porque `AssertRegistroEcf.CanonicalFieldName` remove todo caractere não alfanumérico antes de comparar, tornando `SdIniLal` e `SD_INI_LAL` indistinguíveis.

**Desenho.**

- Gerar `Nome = "<NOME_NORMATIVO>"` para todos os campos dos 180 registros a partir de `sped/ecf/layout-12-manifest.json`, via script determinístico em `tools/ecf-layout`. O manifesto já carrega o campo `name` de cada campo, extraído do manual.
- `AssertRegistroEcf` para de canonicalizar na asserção de nome: passa a comparar o nome normativo do manifesto com `MetadadosCampo.Nome` de forma exata. `CanonicalFieldName` permanece apenas onde é usado para identificar os pares NIF/CNPJ.
- Com isso o harness passa a pegar exatamente a classe de regressão que hoje escapa.

### B3 - Unificação dos enums contábeis duplicados

Achado 10.

`TecnoFisc.Sped.Ecf.Enums.IndicadorDebitoCredito` é byte a byte idêntico a `TecnoFisc.Sped.Ecd.Enums.IndicadorDebitoCredito` — mesmos valores, mesmos tokens SPED, mesma semântica. O mesmo vale para `IndicadorTipoConta`. O repositório já promove enums fiscais compartilhados não regidos pelo Ato COTEPE para `TecnoFisc.Sped.Txt.Engine/Enums` (`IndicadorSimNao` e `CodigoNaturezaContaContabil` vivem lá e o ECF já os consome).

**Desenho.** Promover os dois enums para `TecnoFisc.Sped.Txt.Engine/Enums`, migrando ECD e ECF para o tipo único. As cópias em `Ecd/Enums` e `Ecf/Enums` são removidas.

`TypeForwardedTo` não resolve, porque a mudança de namespace exige alteração de `using` no código do consumidor. Isso é source-breaking para quem consome `TecnoFisc.Sped.Ecd` e está assumido — ver versionamento.

## Versionamento e comunicação

A seção B3 é a única mudança breaking. As demais são aditivas ou restauram o comportamento de `origin/dev`.

- Título do PR e do squash: `feat(ecf)!: add complete read-only layouts 8 through 12` — o `!` promove o pacote a `1.0.0` via semantic-release.
- Rodapé `BREAKING CHANGE:` descrevendo a mudança de namespace de `IndicadorDebitoCredito` e `IndicadorTipoConta`, com o `using` antigo e o novo lado a lado.
- `CHANGELOG.md` recebe, além disso, a nota da mudança de ordem de enumeração do catálogo (A6) e a documentação das duas flags novas de `ReadingOptions`.

## Critérios de aceitação

1. `dotnet build TecnoFisc.Sped.slnx -warnaserror` sem warnings nem erros.
2. `dotnet test TecnoFisc.Sped.slnx` verde, incluindo as suítes de aceitação privadas.
3. Teste que prova que um código de enum fora do domínio continua sendo lido nos três leiautes publicados com opções padrão, e falha no ECF.
4. Teste que prova que uma coluna presente no arquivo mas barrada por vigência não desloca os campos seguintes.
5. Teste que prova que todo registro descartado por vigência aparece como `RegistroNaoReconhecido` no stream.
6. Teste que prova que `LenientLayout = true` lê um arquivo com linha de bloco desconhecido sem lançar, nos quatro leiautes.
7. Teste de gerador que prova que um alias inválido produz um diagnóstico e não a cascata de CS0246.
8. Teste de ordem de enumeração do catálogo nos quatro módulos.
9. Conformidade de nome exata contra o manifesto nos 180 registros ECF, sem canonicalização.
10. Benchmarks executados e baseline registrado no PR.
