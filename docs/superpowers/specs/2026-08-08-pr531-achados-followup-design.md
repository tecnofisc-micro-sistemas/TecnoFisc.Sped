# Achados do review de follow-up do PR 531 — design

Data: 2026-08-08. Status: aprovado.

## Objetivo

Tratar os dez achados verificados do review de follow-up do PR 531 (ECF, leiautes 8–12,
read-only) e os três itens parked do review anterior, distribuídos em três PRs, todos
concluídos antes da publicação da próxima versão major.

A revisão de follow-up rodou como workflow multi-agente (4 finders + verificação
adversarial por localização): 28 candidatos, 8 refutados, 10 mantidos.

## Decisões de escopo

Três decisões tomadas antes do design, nesta ordem:

1. **A promessa é "sem perda silenciosa", não "fidelidade total".** O que sumiu dos
   leiautes antigos não é modelado como propriedade tipada, mas nada some sem sinal:
   registro fora do modelo e coluna sem propriedade viram diagnóstico estruturado.
2. **Registro removido é reconhecido, não modelado.** Os sete códigos removidos no
   leiaute 11 entram no catálogo; seus campos, não.
3. **Três PRs.** Bloqueadores dentro do #531; contrato de diagnóstico e limpeza depois
   do merge, antes da publicação.

### Desvio consciente de ARCHITECTURE §4.7

O repo já tem `DescontinuadoAttribute` (`src/TecnoFisc.Sped.Core/Atributos/`), e o
EFD ICMS-IPI já o aplica: `Registro0210` e `Registro1600` são descontinuados na V016 e
seguem tipados e legíveis, porque §4.7 determina que pacotes read-only continuem lendo
registros descontinuados — arquivos históricos ainda os contêm.

O caminho consistente com esse padrão seria modelar X291, X300, X305, X310, X320, X325 e
X330 e as colunas antigas do X450 como classes e propriedades tipadas com
`[Descontinuado(EmVersao = (int)LayoutEcf.V011)]`, sem mecanismo novo nenhum. **Não é o
que este design faz**, por decisão explícita: esses registros não têm relevância no
contexto atual da TecnoFisc, e o custo de extraí-los da especificação não se paga agora.

O custo, para que a decisão futura parta do número certo: os quatro manuais **estão**
disponíveis em `sped/guides/ecf-layout-8` … `ecf-layout-11`, cada um com `inventory.md`
de proveniência. O que não existe é índice por registro — o `inventory.md` traz metadados
da fonte, não a página de cada registro como o `STAGE_4_REGISTROS.md` faz. Localizar os
sete registros num PDF de 672 páginas é busca, não leitura direta.

A migração futura para `[Descontinuado]` é **puramente aditiva** sobre o que este design
entrega: acrescentar `[CampoSped]` às classes que já existirão.

## Princípio unificador

Um registro sem campos modelados é só um registro com zero `[CampoSped]`. Com X300
existindo como classe vazia anotada, o catálogo o conhece, o leitor não aborta, a
hierarquia o posiciona, e seu conteúdo cai em `ColunasNaoModeladas` — a coleção que o
achado 2 já obriga a criar. Sem terceiro conceito de vigência ao lado de `IntroduzidoEm`
e `DescontinuadoEm`, e sem caminho especial no `LeitorSpedTxt`.

O X450 usa o mesmo molde sem classe nova: mantém só `PAIS` e deixa as colunas de detalhe
dos leiautes 8–10 saírem por `ColunasNaoModeladas`.

Resultado: **um mecanismo novo** (`ColunasNaoModeladas`) e correções pontuais.

## Distribuição

| PR | Achados | Natureza |
|---|---|---|
| A — dentro do #531 | 1, 3, 4, 5, 6, 7 + parked 2 | O código mente ou aborta. Sem API nova no núcleo. |
| B — após merge | 2, 8, 9 | Contrato de diagnóstico. Muda `RegistroSped`, afeta os quatro leiautes. |
| C — após merge | 10 + parked 1, 3 | Limpeza. Sem mudança de comportamento. |

A não depende de B. Entre A e B o comportamento dos sete registros e das colunas
excedentes é "reconhece e ignora o conteúdo" — pior que o destino, melhor que hoje, que
aborta a leitura inteira. Como os três saem antes da publicação, o intervalo só existe
dentro de `dev`.

## PR A — bloqueadores

### Achado 1 — registros removidos no leiaute 11 abortam a leitura

Sete classes vazias em `src/TecnoFisc.Sped.Ecf/Registros/BlocoX/`, cada uma apenas com
`[RegistroSped(Codigo, Bloco = "X", Nivel)]` e
`[Descontinuado(EmVersao = (int)LayoutEcf.V011)]`, sem nenhum `[CampoSped]`:
X291, X300, X305, X310, X320, X325, X330.

O `Nivel` de cada um não é conhecido hoje. Obter no manual do leiaute 10 — sete números
numa região contígua do PDF (bloco X), busca localizada. Se a busca sair cara, inferir do
vizinho já modelado (X291 entre X280 e X292) e marcar no código que é inferência a
validar.

Impacto em testes que hoje fixam o oposto:

- `CatalogoAtual_NaoReintroduzRegistrosRemovidosNoLeiaute11` inverte de sentido.
- O catálogo passa de 180 para 187 registros.
- O manifesto JSON descreve o **leiaute 12**, onde esses sete não existem: continua com
  180. `ManifestoCatalogoTests` passa a aceitar catálogo ⊋ manifesto exatamente no
  conjunto dos descontinuados, e em nenhum outro.

### Achado 3 — COD_VER fora de 0008–0012 desliga o gate de vigência

`Registro0000` do ECF registra um `ErroFormato` quando `COD_VER` não pertence a
`LayoutEcf`. No modo estrito (default) isso aborta a leitura; com `LenientLayout = true`
fica como diagnóstico e a vigência permanece desligada.

Abortar é deliberado: um leiaute desconhecido não tem mapeamento de colunas confiável, e
o reúso da posição 31 no `0020` prova isso. Custo aceito: um ECF de leiaute 13 deixa de
ser lido até a biblioteca ser atualizada; o consumidor destrava via modo leniente.

### Achado 4 — aliases do 0020 trocam de semântica por leiaute

Remover `IndPrTransf` e `PossuiCebras`. Fica `IndicadorPosicao31`, com XML doc
explicitando que a semântica depende do `COD_VER` do arquivo. Corrigir junto o
`Nome = "POSSUI_CEBRAS"` do atributo, que rotula errado o campo nos leiautes 10 e 11.

Os aliases **voltam no PR B**, guardados por `VersaoDoArquivo`.

### Achado 5 — ParseLinha diverge de ReadAsync

`LeitorSpedTxt.ParseLinha` ganha parâmetro opcional `int versaoLeiaute = 0` — não quebra
ECD, EFD Contribuições nem ICMS-IPI. `ParserEcf.ParseLinha` expõe sobrecarga tipada em
`LayoutEcf`. O XML doc passa a dizer que sem versão informada não há vigência.

### Achado 6 — ValidarVigenciaCrescente é breaking não documentado

Manter a exceção. Documentar no `CHANGELOG` como breaking de fato e fazer a mensagem
nomear registro, campo e posição. A assimetria com o `TFSPED003` (exceção em runtime de
um lado, diagnóstico suprimível do outro) fica registrada como deliberada.

### Achado 7 — validação de domínio de enum sem cobertura

Remover o remark falso de
`tests/TecnoFisc.Sped.Ecf.Tests/Parser/ValidacaoDominioEnumEcfTests.cs`. Acrescentar
cobertura sobre campos reais: `IND_DAD` num registro de abertura de bloco e `COD_NAT` em
C050/J050, com valor fora do domínio.

Manter `ValidarDominioDeEnum = true` como default do ECF, coerente com "falha explícita,
nunca perda silenciosa". Custo aceito: um código novo da RFB derruba a leitura até a
biblioteca acompanhar; o escape é o consumidor passar `false`.

### Parked 2 — nivelCorteVigencia

Já aplicado no worktree: o parâmetro `ref int nivelCorteSubarvore` de
`ShouldIgnoreByVersion` passou a se chamar `nivelCorteVigencia`, e o comentário do estado
de descarte passou a documentar os dois cortes independentes.

### Documentação — o que originou os achados 1 e 2

Deixar de prometer leitura plena de 8–12 em `README.md` (4 ocorrências),
`ARCHITECTURE.md` (3), `CHANGELOG.md`, `TecnoFisc.Sped.Ecf.csproj` (Description) e
`sped/STAGE_17_ECF_BASELINE.md`. A formulação verdadeira: modelo tipado do leiaute 12,
leitura dos leiautes 8–12, registros e colunas exclusivos dos leiautes antigos
reconhecidos mas não tipados, e `[Descontinuado]` como evolução planejada.

## PR B — contrato de diagnóstico

### ColunasNaoModeladas

Lista preguiçosa em `RegistroSped`, no mesmo padrão do `_errosDeFormato` que já existe
ali: nula no caminho comum, alocada só quando há o que reportar.

Item: `ColunaNaoModelada(int Posicao, string Valor, MotivoColunaNaoModelada Motivo)`, com
`Posicao` na numeração do SPED (1-based, a mesma do `Ordem` do atributo).

`MotivoColunaNaoModelada`:

- `AlemDoModelo` — coluna além da última declarada (achado 2, caso X450).
- `PosteriorAVersaoDeclarada` — campo declarado mas inativo pelo `COD_VER` (achado 8).

Ponto de captura: `LeitorSpedTxt.cs:604`, onde hoje o `if` tem um `else` que é só
comentário. O `else` passa a existir. **Custo zero no caminho feliz** — a condição já é
avaliada; muda apenas o ramo que hoje não faz nada. Ainda assim entra um caso no
`ParserVigenciaBenchmark`, por causa da regra 5.

Sem flag em `ReadingOptions`: o contrato é "nunca perder em silêncio", e uma flag
existiria para poder perdê-lo em silêncio.

### Discriminador de sentinela (achado 9)

`RegistroNaoReconhecido` ganha `Motivo`, do tipo `MotivoNaoReconhecimento`:
`CodigoDesconhecido` e `PosteriorAVersaoDeclarada`. Quem hoje trata
`RegistrosNaoReconhecidos` como "arquivo corrompido" passa a filtrar por motivo em vez de
casar substring numa mensagem em português. XML docs das quatro classes `Arquivo*`
atualizados.

Sem helpers de filtro — `Where` no motivo basta.

Considerado e rejeitado: um terceiro motivo para "registro descontinuado presente num
leiaute onde já não existe". `[Descontinuado]` é declaradamente informacional; inventar
comportamento em cima dele aqui contradiria o atributo nos outros pacotes.

### VersaoDoArquivo

`RegistroSped` ganha `VersaoDoArquivo { get; internal set; }`, atribuída pelo leitor a
cada registro materializado.

Nome distinto de `VersaoLeiaute` de propósito: `RegistroSped.VersaoLeiaute` é "versão que
este registro declara", conhecida só pelo `0000`. Nomes iguais para conceitos diferentes
foi a armadilha removida do `ShouldIgnoreByVersion` (parked 2).

Com isso `IndPrTransf` e `PossuiCebras` voltam ao `Registro0020`, cada um se recusando a
responder no leiaute errado em vez de devolver o valor do outro.

## PR C — limpeza

### ArquivoSpedBase (achado 10)

Classe abstrata em `TecnoFisc.Sped.Txt.Engine`, recebendo a ordem dos blocos no
construtor e concentrando o que está quadruplicado em `ArquivoEcf`, `ArquivoEcd`,
`ArquivoEfdContribuicoes` e `ArquivoEfdIcmsIpi`: dicionário de blocos, `_naoReconhecidos`,
roteamento do `Adicionar`, `EnumerarBlocos`, `EnumerarRegistros`.

Não arranha a regra de leiautes não se referenciarem: os quatro já dependem desse
projeto, onde moram `IArquivoSped` e `IBlocoSped`.

`LoadAsync` fica fora da unificação — cada um é factory estático que devolve o próprio
tipo concreto, e forçar isso na base custaria genéricos que não se pagam. Vira um
`protected` de preenchimento reaproveitado pelos quatro.

Critério de aceite: suíte dos quatro pacotes verde **sem nenhuma alteração de teste**. Se
um teste precisar mudar, o comportamento mudou e o refactor está errado.

### Parked 1 e 3

- Teste com `RegistrosIgnorados` sobre a fixture com registro filho (defense-in-depth; o
  caminho já foi provado seguro analiticamente no review anterior).
- Ordem das subseções do `CHANGELOG` entre pacotes.

## Riscos e verificações antecipadas

Três premissas do design não estão verificadas no código e precisam ser confirmadas
**antes** de escrever o resto do PR A, porque uma falha em qualquer delas muda a solução:

1. **Registro com zero `[CampoSped]`.** O design inteiro do achado 1 depende de o
   `RegistroSpedCatalogoGenerator` e o `CatalogoBuilder` aceitarem uma classe anotada sem
   nenhum campo. Se algum dos dois exigir pelo menos um campo, a saída é declarar um
   único campo técnico (o próprio `REG`) ou tratar esses códigos fora do catálogo de
   classes. Verificar com uma classe descartável antes de criar as sete.
2. **`Nivel` dos sete registros.** Sem ele a hierarquia sai errada. Buscar no manual do
   leiaute 10; inferência do vizinho é fallback declarado, não plano A.
3. **Herança em pacotes publicados (PR C).** `ArquivoEcd`, `ArquivoEfdContribuicoes` e
   `ArquivoEfdIcmsIpi` passam a herdar de código novo. O critério de aceite — suíte verde
   sem alterar teste — é o que detecta regressão aqui.

## Conventional Commits

- PR A: o título de #531 (`feat(ecf)!: add complete read-only layouts 8 through 12`)
  mantém tipo, escopo e `!`; o que muda é a descrição, que hoje afirma "complete" e passa
  a nomear o recorte real (modelo tipado do leiaute 12, leitura de 8–12).
- PR B: `feat(txt)!:` — muda `RegistroSped`, API pública dos quatro pacotes.
- PR C: `refactor(txt):` — sem release.
