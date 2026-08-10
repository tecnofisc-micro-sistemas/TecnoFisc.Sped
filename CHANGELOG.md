# Changelog

Todas as mudanças relevantes deste repositório são documentadas neste arquivo.

O formato segue [Keep a Changelog](https://keepachangelog.com/pt-BR/1.1.0/) e o projeto adota [Semantic Versioning](https://semver.org/lang/pt-BR/). Cada pacote NuGet possui versão independente; as seções abaixo agrupam as mudanças por release do repositório.

## [Não publicado]

### TecnoFisc.Sped.Ecf

#### Adicionado

- Novo pacote **read-only** para a Escrituração Contábil Fiscal, com **modelo tipado único do leiaute 12** e leitura dos leiautes 8 a 12, com parser buffered/streaming, deltas oficiais de versão, fixtures sintéticas por bloco e fixtures anonimizadas de aceitação. O catálogo tipa os 180 registros dos 17 blocos do leiaute 12. Os registros e as colunas exclusivos dos leiautes 8 a 10 são **reconhecidos, não tipados**: o leitor não aborta ao encontrá-los, e o conteúdo deles chega ao consumidor em bruto em `RegistroSped.ColunasNaoModeladas` — o que não é materializado nesta versão é a propriedade tipada.
- Os sete registros do bloco X removidos no leiaute 11 — `X291`, `X300`, `X305`, `X310`, `X320`, `X325`, `X330` — entram no catálogo com `[Descontinuado(EmVersao = (int)LayoutEcf.V011)]` e **zero campos modelados**. Efeito: ler um arquivo de leiaute 8 a 10 que ainda os contenha deixa de degradar a linha para `RegistroNaoReconhecido` — o registro real é materializado, ainda que sem nenhuma propriedade preenchida. O catálogo passa de 180 para **187** registros (180 do leiaute 12 + 7 descontinuados); o manifesto continua descrevendo os 180 do leiaute 12 e o teste de paridade catálogo × manifesto exclui os descontinuados. Modelar os campos deles é evolução planejada e **puramente aditiva**: quem lê hoje continua lendo igual quando ela chegar.
- Arquivo de leiaute **fora da faixa 8–12** (`COD_VER` menor que 8 ou maior que 12) passa a ser lido em **modo tolerante** em vez de abortar: o `0000` recebe um aviso não fatal em `ErrosDeFormato` apontando `COD_VER`, um código de registro que o catálogo não conhece vira `RegistroNaoReconhecido` e uma falha de conversão de campo vira diagnóstico em vez de exceção. Dentro da faixa 8–12 o rigor continua exatamente o mesmo. A validação de domínio de enum (`ValidarDominioDeEnum`) **não** é relaxada nesse cenário.
- `ParserEcf.ParseLinha` ganha a sobrecarga `ParseLinha(ReadOnlySpan<char> linha, LayoutEcf leiaute, long numeroLinha = 0)`, que aplica a vigência do leiaute informado — o mesmo critério que `ReadStreamingAsync` deriva do `COD_VER` do arquivo, tanto para o registro (um `Y730`, introduzido no leiaute 12, devolve falha sob `LayoutEcf.V009`) quanto para os campos (um campo introduzido depois não recebe valor). A sobrecarga sem `LayoutEcf` continua **não** aplicando vigência nenhuma (todos os campos do catálogo são aceitos, inclusive os introduzidos em leiautes posteriores); isso agora está dito explicitamente no XML doc, onde antes só se inferia do comportamento.
- `MetadadosCampo.Nome` e `ErroFormato.Campo` passam a reportar o nome normativo do leiaute (ex.: `COD_NAT`) em vez do nome CLR da propriedade (`CodNat`) em ~126 registros — cobertura completa do alias declarado no manifesto, para os 896 campos do catálogo. Duas normalizações são **deliberadas**, não erro de extração: `IND_E-COM_TI` (registro `0020`) vira `IND_E_COM_TI` e `NIF/CNPJ` (registro `X357`) vira `NIF_CNPJ`, porque hífen e barra não são separadores válidos em identificador — quem cruzar o catálogo com o texto literal do PDF da RFB deve esperar essas duas divergências pontuais. Como a ECF é pacote inédito (ainda `[Não publicado]`), isto não é breaking.
- `Registro0020.IndPrTransf` e `Registro0020.PossuiCebras` voltam, agora como `IndicadorSimNao?` calculado sobre `VersaoDoArquivo`: `IndPrTransf` responde nos leiautes 10 e 11, `PossuiCebras` do 12 em diante, e cada um devolve `null` fora da sua faixa em vez de entregar em silêncio o valor do outro campo. `IndicadorPosicao31` continua sendo a porta única e sem interpretação. Num registro que não veio de leitura de arquivo (`VersaoDoArquivo == 0`) os dois devolvem `null`.

#### Quebrado

> A ECF ainda é pacote inédito (`[Não publicado]`), então nenhuma versão liberada em nuget.org é afetada. As duas quebras abaixo estão registradas porque contradizem API e comportamento já descritos nesta mesma seção durante o desenvolvimento.

- `Registro0020.IndPrTransf` e `Registro0020.PossuiCebras` foram **removidos**. Eram dois aliases sem guarda sobre a **mesma** posição 31, cuja semântica normativa muda com o leiaute (`IND_PR_TRANSF` nos leiautes 10 e 11, `POSSUI_CEBRAS` no 12) — no leiaute errado, cada alias devolvia em silêncio o valor do outro campo. Passa a existir uma única porta, `Registro0020.IndicadorPosicao31`; a interpretação depende do `COD_VER` declarado no `0000`, e o XML doc da propriedade explica a ambiguidade.
- `Registro0000.VersaoLeiaute` muda de contrato: passa a devolver o número declarado em `COD_VER` mesmo fora da faixa 8–12, exigindo exatamente 4 caracteres numéricos. Antes, qualquer valor fora de `0008`–`0012` virava `0`, o que desligava o gate de vigência **em silêncio** e fazia o arquivo ser lido como se fosse leiaute 12. Zero agora significa apenas `COD_VER` ausente, de comprimento diferente de 4 ou não numérico — arquivo inválido, não leiaute novo. Um leiaute numérico fora da faixa é sinalizado à parte, por `RegistroSped.IsLeiauteConhecido`.

#### Limitações conhecidas

- Ao ler um arquivo de leiaute 10 ou 11, um `ErroFormato` na posição 31 do `0020` rotula o campo como `POSSUI_CEBRAS`, mesmo quando naquele leiaute a posição significa `IND_PR_TRANSF`. Afeta **só a mensagem de diagnóstico**, não o valor interpretado. Um rótulo composto (`IND_PR_TRANSF/POSSUI_CEBRAS`) não é possível: `CatalogoBuilder.IsFieldNameValid` rejeita `/`, e o manifesto do leiaute 12 — a verdade sobre o leiaute vigente, comparada campo a campo — nomeia o campo `POSSUI_CEBRAS`.

### TecnoFisc.Sped.Ecd

#### Quebrado

- `TecnoFisc.Sped.Ecd.Enums.IndicadorDebitoCredito` e `TecnoFisc.Sped.Ecd.Enums.IndicadorTipoConta` — cópias idênticas que também existiam em `TecnoFisc.Sped.Ecf.Enums` — foram unificados em `TecnoFisc.Sped.Txt.Engine.Enums`. Troque `using TecnoFisc.Sped.Ecd.Enums;` por `using TecnoFisc.Sped.Txt.Engine.Enums;` nos pontos que referenciam esses dois tipos. Ver também a quebra em `TecnoFisc.Sped.Txt.Engine` (`MetadadosCampo`).

### TecnoFisc.Sped.EfdIcmsIpi

#### Corrigido

- `Registro0210` e `Registro1600` passam a expor `DescontinuadoEm == 16` no catálogo que os parsers realmente usam. Ambos declaram `[Descontinuado(EmVersao = (int)LayoutEfdIcmsIpi.V016)]` na classe desde sempre, mas o `RegistroSpedCatalogoGenerator` (que gera `CatalogoSpedGerado` em compile-time) **nunca lia** o atributo — só o `CatalogoBuilder` reflexivo, usado apenas em teste, lia. Resultado: em **todas as versões publicadas** do pacote, quem inspecionasse `MetadadosRegistro.DescontinuadoEm` pelo catálogo de produção via `0` nos dois registros, enquanto o teste que cobria o caso lia o catálogo reflexivo e passava. Corrigido no gerador; a cobertura nova (`CatalogoGerado_Registro0210_ExpoeDescontinuadoEm16` e `CatalogoGerado_Registro1600_ExpoeDescontinuadoEm16`) exercita o catálogo **gerado** diretamente. Não muda o comportamento de leitura nem de escrita — só o metadado exposto ao consumidor que consulta o catálogo.

### TecnoFisc.Sped.Txt.Engine

#### Adicionado

- `ReadingOptions.RespeitarVigenciaDoLeiaute` (`bool?`) — quando `true`, omite registros anteriores a `IntroduzidoEm` e não atribui campos anteriores a `DesdeVersao`, usando a versão declarada no registro `0000`. `null` (padrão) delega a decisão ao parser do leiaute: a ECF liga; EFD Contribuições, EFD ICMS-IPI e ECD mantêm o modelo informacional completo (comportamento anterior).
- `ReadingOptions.ValidarDominioDeEnum` (`bool?`) — quando `true`, um código numérico fora do domínio declarado de um enum fechado (sem `[SpedValor]`) vira erro de campo em vez de cast permissivo. `null` (padrão) delega a decisão ao parser do leiaute: a ECF liga; os demais mantêm o cast permissivo — a Receita publica códigos novos entre versões do guia e um arquivo que hoje é lido não pode passar a falhar por atualização de pacote.
- Nova validação de catálogo: `DesdeVersao` precisa ser não-decrescente ao longo das posições declaradas de um registro. Enforçada em runtime por `CatalogoBuilder.ValidarVigenciaCrescente` e em build-time pelo diagnóstico `TFSPED003` (erro) do source generator em `TecnoFisc.Sped.Txt.Engine.SourceGenerators`. Consequência: modelar um campo versionado fora do fim do registro passa a ser **erro de build**, não mais um bug silencioso de leitura.
- `RegistroSped.IsLeiauteConhecido` (`virtual`, `true` por padrão) — o registro `0000` de cada leiaute pode devolver `false` quando o `COD_VER` do arquivo está fora da faixa que o módulo modela. Ao ver `false`, `LeitorSpedTxt.ReadStreamingAsync` registra um `ErroFormato` não fatal em `COD_VER` no próprio `0000` e passa a ler em modo tolerante (ver a catraca em **Quebrado**), em vez de abortar. Hoje só a ECF sobrescreve a propriedade; os demais leiautes herdam `true` e não mudam de comportamento. Ressalva de escopo: o próprio `0000` é interpretado **antes** de o leitor conseguir consultar a propriedade, então um leiaute desconhecido que mude o formato de um campo do `0000` ainda pode abortar; o modo tolerante vale para os registros **seguintes**.
- `LeitorSpedTxt.ParseLinha` ganha o parâmetro opcional `versaoLeiaute`, que aplica a vigência a uma linha isolada nos **dois níveis**, em paridade com `ReadStreamingAsync`: um registro cujo `IntroduzidoEm` é posterior à versão informada devolve `ResultadoParse.Falha` com a mesma mensagem que o streaming usa na sentinela (`Registro posterior à versão declarada no 0000 (N).`), e um campo cujo `DesdeVersao` é posterior não recebe valor. Em linha isolada não há hierarquia, então não há corte de subárvore — a decisão vale só para a própria linha. Sem o parâmetro, `ParseLinha` continua sem aplicar vigência nenhuma — divergência em relação a `ReadStreamingAsync` que agora é escolha explícita do chamador, não surpresa.
- Leitura com `COD_VER` **ilegível** (ausente, com comprimento diferente de 4 ou não numérico) deixa de ser silenciosa: o leitor registra um `ErroFormato` não fatal apontando `COD_VER` no próprio registro de abertura, dizendo que a vigência do leiaute não será aplicada. Isso **não** liga o modo tolerante — dado corrompido não é evolução de leiaute, então o rigor de `LenientLayout`/`LenientFieldParsing` continua exatamente como o chamador configurou (a catraca de mão única descrita em **Quebrado** só vale para leiaute fora da faixa, que tem versão positiva). Antes, esse subconjunto não emitia diagnóstico nenhum e o arquivo era lido sem gate de vigência, como se fosse o leiaute mais recente. O leitor avalia versão e faixa uma única vez, no primeiro registro que carrega versão. Sem efeito para EFD Contribuições, EFD ICMS-IPI e ECD: os três herdam `IsLeiauteConhecido == true` e nenhum dos dois ramos dispara para eles.
- `SnifferSped` passa a classificar arquivos ECF: `IdentificarAsync` reconhece o discriminador `LECF` do registro `0000` (via `ClassificarEcf`) e devolve `MetadadosArquivoSped { Projeto = ProjetoSped.Ecf, VersaoLeiaute }` — onde antes devolvia `ProjetoSped.Desconhecido` para qualquer arquivo ECF. Quem classifica é o **discriminador**, não a versão: `COD_VER` é convertido numericamente (exatamente 4 caracteres numéricos, mesma regra de `Registro0000.VersaoLeiaute`) e devolvido como declarado mesmo fora da faixa 8–12, porque um arquivo de leiaute 13 continua sendo ECF e precisa chegar ao `ParserEcf`, que o lê em modo tolerante. Só `COD_VER` ilegível volta a `Desconhecido`. A largura da linha é checada por **mínimo** — o bastante para alcançar discriminador e `COD_VER` —, não por igualdade, para que uma coluna acrescentada ao `0000` num leiaute futuro não desligue o roteamento; validar a largura do registro é trabalho do parser, que reporta linha, registro e campo. Mudança aditiva (novo projeto reconhecido) e restrita ao caminho `LECF`: a classificação de ECD, EFD Contribuições e EFD ICMS-IPI é bit a bit a mesma, inclusive a exigência de largura exata dos leiautes numéricos.
- `RegistroSped.ColunasNaoModeladas` (`IReadOnlyList<ColunaNaoModelada>`) — colunas presentes na linha sem propriedade tipada que as receba, com `Posicao` (numeração do Guia Prático, `1` = `REG`), `Valor` verbatim e `MotivoColunaNaoModelada` (`AlemDoModelo`, `PosteriorAVersaoDeclarada`). Vazia no caminho comum, e alocada só quando há o que reportar, no mesmo padrão de `ErrosDeFormato`. O custo no caminho feliz é nulo: a condição já era avaliada no leitor e só o ramo vazio passou a fazer algo (medido em `ColunasNaoModeladasBenchmark`). Não há flag em `ReadingOptions` para desligar — o contrato é "nunca perder em silêncio", e a flag existiria para poder perdê-lo.
- `RegistroSped.VersaoDoArquivo` (`int`) — versão declarada no `0000` do arquivo em que o registro foi lido, atribuída pelo leitor a cada registro materializado, inclusive ao próprio `0000` e às sentinelas. `0` quando o registro não veio de leitura, ou veio de `ParseLinha` sem versão informada. Distinta de `VersaoLeiaute`, que é a versão que o próprio registro declara.
- `RegistroNaoReconhecido.Motivo` (`MotivoNaoReconhecimento`: `CodigoDesconhecido`, `PosteriorAVersaoDeclarada`) — separa as duas origens da sentinela sem casar substring na mensagem em português do diagnóstico.
- O modelo raiz de arquivo dos quatro leiautes (`ArquivoEcf`, `ArquivoEcd`, `ArquivoEfdContribuicoes`, `ArquivoEfdIcmsIpi`) passa a ser `ArquivoSpedBase<TBloco>` (`TecnoFisc.Sped.Txt.Engine.Abstracoes`): `Adicionar`, `EnumerarBlocos`, `EnumerarRegistros` e `RegistrosNaoReconhecidos` agora são herdados em vez de declarados em cada classe concreta. A mudança é source- e binary-compatível — mover membros de instância para cima na hierarquia não quebra nenhum ponto de chamada existente, porque a resolução de membro percorre a cadeia de herança —, então nenhum consumidor precisa mudar nada. Cada leiaute continua fornecendo sua ordem de blocos, sua fábrica de blocos e o gancho `AdicionarAoBloco`, este último porque o `Adicionar` de cada bloco é `internal` ao assembly do próprio leiaute.

#### Alterado

- `Arquivo*.Adicionar` passa a coletar `RegistroNaoReconhecido` em `RegistrosNaoReconhecidos` em vez de lançar, nos quatro leiautes. Registro tipado de bloco inexistente continua lançando.
- A ordem de enumeração de `CatalogoSpedGerado.EnumerarRegistros()` passa a ser a ordem canônica de bloco (`0`, blocos alfabéticos, blocos `1`–`8`, `9`) em todos os módulos. Quem dependia da ordem puramente lexicográfica do código precisa reordenar.

#### Corrigido

- Registro descartado por vigência do leiaute deixa de sumir em silêncio: passa a ser emitido como `RegistroNaoReconhecido` com a linha crua e o motivo.
- Campo barrado por vigência não desloca mais as colunas seguintes do registro.
- Um diagnóstico de campo (`TFSPED001`/`TFSPED002`/`TFSPED003`) deixa de suprimir a emissão do catálogo, o que soterrava a causa sob uma cascata de `CS0246`.
- Sob `LenientLayout`, EFD ICMS-IPI e EFD Contribuições deixam de absorver silenciosamente sentinelas de código desconhecido iniciado por `1` dentro de `Bloco1.Registros`: agora vão para `RegistrosNaoReconhecidos`, igual a qualquer outro código fora do catálogo. Quem adotou `LenientLayout` desde a `0.9.0` deve revisar o que fazia com `Bloco1.Registros` — o comportamento antigo corrompia silenciosamente esse bloco com um objeto de tipo errado; a mudança é correção, não quebra anunciável.
- O gate de vigência deixa de rodar antes do filtro de `RegistrosIgnorados`/`BlocosIgnorados`: um registro que o chamador pediu para descartar não é mais decodificado nem devolvido como `RegistroNaoReconhecido` só porque também está fora da versão declarada no `0000`.

#### Quebrado

- O construtor público de `MetadadosCampo` ganhou o parâmetro opcional `definidorEstrito` (último da lista). É **source-compatible** (código existente compila sem alteração), mas **binary-incompatible**: um assembly de terceiro compilado contra a `0.9.0` que construa `MetadadosCampo` diretamente — cenário legítimo, já que `IRegistroSpedCatalogo` é pública e um catálogo customizado é caminho documentado — recebe `MissingMethodException` em runtime ao carregar contra esta versão, não erro de compilação. O major bump desta release absorve a quebra.
- `CatalogoBuilder.BuildFromAssembly` passa a lançar `InvalidOperationException` quando um registro declara `DesdeVersao` num campo que não está no fim do layout (ver a validação nova em **Adicionado**). Antes a anotação era puramente informacional no caminho reflexivo, e um campo versionado no meio do registro deslocava silenciosamente as colunas seguintes. **Não há opt-out**: mova o campo para o fim do registro ou remova `DesdeVersao`. A mensagem da exceção nomeia registro, campo e posição, que é o dado necessário para corrigir a declaração. O equivalente em build-time é o diagnóstico `TFSPED003` do source generator.
- `EscritorSpedTxt` passa a lançar `InvalidOperationException` ao receber um registro cujo catálogo não tem **nenhum campo modelado**, em vez de emitir uma linha só com o código (`|CODIGO|`) — perda de dados silenciosa. A mensagem é a mesma de "não suportado" já usada para código desconhecido. Na prática atinge os sete registros descontinuados da ECF (`X291`, `X300`, `X305`, `X310`, `X320`, `X325`, `X330`), alcançáveis pela API pública: `new EscritorSpedTxt(new CatalogoSpedGerado())` mais um `RegistroX300`. Nenhum registro dos leiautes já publicados (EFD Contribuições, EFD ICMS-IPI, ECD) tinha zero campos antes desta versão, então para eles a checagem é inerte.
- `ReadingOptions.LenientFieldParsing` e `ReadingOptions.LenientLayout` viram **catraca de mão única**: sob um arquivo cujo leiaute está fora da faixa conhecida pelo módulo (`RegistroSped.IsLeiauteConhecido == false`), o leitor força ambas para `true` independentemente do que o chamador configurou, e **não há como desligar**. Quem passava `false` para ter fail-fast deixa de tê-lo nesse cenário específico — um leiaute que a biblioteca não modela não autoriza afirmar que o dado está errado. Dentro da faixa conhecida, as duas opções continuam valendo exatamente como configuradas. `ValidarDominioDeEnum` **não** entra na catraca: desligá-la faria um valor fora do domínio ser aceito em silêncio, e quem converte a exceção de domínio em diagnóstico sob leiaute desconhecido é o alargamento de `LenientFieldParsing`.
- O construtor de `RegistroNaoReconhecido` passa a exigir um quarto parâmetro, `MotivoNaoReconhecimento`. Só o leitor constrói a sentinela no caminho normal; quem a instanciava à mão (teste, dublê) precisa informar a origem.

### TecnoFisc.Sped.Txt / TecnoFisc.Sped

#### Alterado

- Os metapacotes textuais passam a incluir `TecnoFisc.Sped.Ecf`. A versão continua sendo definida automaticamente pelo fluxo de release; não houve alteração manual de versão.

## [0.9.0] — 2026-06-24

Release de **parsing tolerante opt-in**: o leitor TXT passa a oferecer modos lenientes em que uma falha de campo ou um registro desconhecido não aborta mais a linha/arquivo, deixando o consumidor materializar apontamentos em vez de só receber uma exceção. Tudo **opt-in e backward-compatible** — o comportamento padrão (lançar no primeiro erro) é preservado. (#525)

### TecnoFisc.Sped.Core

#### Adicionado

- `ErroFormato.ValorBruto` (`TecnoFisc.Sped.Core.Erros`) — propriedade `init` nullable que preserva o texto cru do campo que falhou a conversão, permitindo ao consumidor exibir/registrar o valor original. Aditiva: não muda o construtor posicional nem o `ToString()`.

### TecnoFisc.Sped.Txt.Engine

#### Adicionado

- `ReadingOptions.LenientFieldParsing` (`TecnoFisc.Sped.Txt.Engine.Parser`) — quando `true`, uma falha de conversão de campo não aborta a leitura: o campo fica no default, o erro é acumulado em `RegistroSped.ErrosDeFormato` (lista lazy, zero alocação no caminho feliz) e o parsing continua. Padrão `false` reproduz o comportamento atual (lança `ErroFormatoSpedException` no primeiro erro).
- `ReadingOptions.LenientLayout` (mesmo namespace) — quando `true`, um código de registro desconhecido pelo catálogo não aborta a leitura: o leitor emite um `RegistroNaoReconhecido` (linha crua + `ErroLayout`) como folha na hierarquia e continua. O sentinela nunca vira pai. Padrão `false` reproduz o comportamento atual (lança `ErroLayoutSpedException`). Independente de `LenientFieldParsing`.
- `RegistroNaoReconhecido` (`TecnoFisc.Sped.Txt.Engine.Abstracoes`) — registro sentinela sempre-folha emitido sob `LenientLayout`, carregando a linha crua e o erro de layout.
- `LeitorSpedTxt.ParseLinha` (`TecnoFisc.Sped.Txt.Engine.Parser`) — método público novo que parseia uma única linha isolada, sem hierarquia nem streaming, sempre leniente. Devolve `Ok(registro)` com eventuais erros de campo em `ErrosDeFormato`; `Falha` apenas quando nenhum registro sai (linha sem pipes ou código desconhecido).

## [0.8.0] — 2026-06-18

Release de automação de publicação e enriquecimento do sniffer TXT.

### TecnoFisc.Sped.Txt.Engine

#### Adicionado

- Sniffer fiscal TXT opt-in: novo `SnifferSpedFiscal.IdentificarAsync(Stream)` devolve os metadados fiscais básicos encontrados no registro `0000` (`Cnpj`, período inicial e período final), mantendo o `SnifferSped` existente como API leve de identificação de projeto/leiaute.

### Infraestrutura

#### Alterado

- Publicação NuGet automatizada no merge para `main`: o workflow de release agora lê a versão de `Directory.Build.props`, valida tag/pacotes duplicados, empacota, publica no nuget.org, cria a tag `vX.Y.Z` e gera a GitHub Release sem etapa manual de tag.
- CI volta a ficar focado em build/test; o empacotamento de release passa a ser responsabilidade exclusiva do workflow `Release`.

## [0.7.1] — 2026-06-01

Patch de **publicação**. Não há mudança de API nem de comportamento em relação à `0.7.0`: esta versão republica toda a família de pacotes num número de versão consistente após a publicação da `0.7.0` ter falhado parcialmente no NuGet.

**O que aconteceu.** Na `0.7.0`, o push ao nuget.org abortou com HTTP 400 ao enviar o pacote de símbolos (`.snupkg`) de um metapacote guarda-chuva: como `TecnoFisc.Sped` e `TecnoFisc.Sped.Txt` não têm código (`IncludeBuildOutput=false`), o `.snupkg` saía sem `.pdb` e o symbol server o rejeita. Como `dotnet nuget push` para no primeiro erro, parte dos pacotes não foi publicada. Resultado: a `0.7.0` ficou incompleta no NuGet (faltaram `TecnoFisc.Sped`, `TecnoFisc.Sped.Core` e `TecnoFisc.Sped.EfdContribuicoes`), e o metapacote `TecnoFisc.Sped.Txt 0.7.0` ficou com dependência apontando para um `EfdContribuicoes 0.7.0` inexistente.

**Correção.** `IncludeSymbols=false` nos dois metapacotes (não geram mais `.snupkg` vazio), permitindo publicar a família inteira de forma consistente na `0.7.1`. Consumidores devem usar **`0.7.1`**; as versões `0.7.0` parcialmente publicadas serão removidas da listagem (unlisted) e não devem ser usadas.

### Corrigido

- **Empacotamento.** `TecnoFisc.Sped` e `TecnoFisc.Sped.Txt` (metapacotes sem código) passam a definir `IncludeSymbols=false`, evitando o `.snupkg` vazio que o symbol server do nuget.org rejeita com HTTP 400 e que abortava a publicação. (#518)

## [0.7.0] — 2026-06-01

> **Nota.** A publicação da `0.7.0` no NuGet ficou **incompleta** (ver `[0.7.1]`). Todo o conteúdo descrito abaixo está disponível na **`0.7.1`**, que é a versão a usar.

Release de **reorganização em camadas** (Stage 18) somada à **fundação do mundo XML**. Quebra o antigo `Core` monolítico em `Core` universal + dois engines (`TecnoFisc.Sped.Txt.Engine`, `TecnoFisc.Sped.Xml.Engine`), introduz os guarda-chuvas `TecnoFisc.Sped.Txt` e `TecnoFisc.Sped` (Stage 13), adiciona os sniffers de identificação de documento por mundo (Stage 12) e estreia o pacote XML **`TecnoFisc.Sped.NFeNFCe`** em **preview** (NF-e modelo 55 parcial — ver ressalva na seção do pacote). Release **breaking**: consumidores do `Core` que referenciavam a maquinaria TXT/XML pelo namespace antigo precisam migrar para os novos engines (detalhes abaixo).

### TecnoFisc.Sped.Core

#### Adicionado

- Value objects fiscais novos, base do mundo XML NF-e/NFC-e (fundação Stage 14, slice 14.1): `Cest` (Código Especificador da Substituição Tributária, com dígito verificador), `Gtin` (GTIN-8/12/13/14 com validação de check digit), `CodigoMunicipioIbge` (código IBGE de 7 dígitos) e `Csosn` (Código de Situação da Operação no Simples Nacional). São transversais por design (Critical rule 2 / §4.2) — `ChaveAcesso` e `ModeloDocumento` continuam reusados sem duplicação. (#505)

#### Alterado (breaking)

- Maquinaria especifica de TXT saiu do `Core` para `TecnoFisc.Sped.Txt.Engine`: abstracoes de registros/blocos/arquivos TXT, atributos `[RegistroSped]`/`[CampoSped]`/`[BlocoSped]`/`[SpedValor]`, catalogo, parser, gerador, streaming e enums transversais TXT. Consumidores que referenciavam esses tipos pelo namespace `TecnoFisc.Sped.Core.*` devem trocar para `TecnoFisc.Sped.Txt.Engine.*`.
- Maquinaria especifica de XML saiu do `Core` para `TecnoFisc.Sped.Xml.Engine`: `IdentificadorXmlFiscal`, `IDocumentoFiscalXml` e `TipoDocumentoFiscalXml`. Consumidores devem trocar `TecnoFisc.Sped.Core.Xml` por `TecnoFisc.Sped.Xml.Engine`.
- Enums de leiaute unico sairam do `Core` para seus pacotes donos: enums EFD ICMS-IPI para `TecnoFisc.Sped.EfdIcmsIpi.Enums` e enums NF-e/NFC-e para `TecnoFisc.Sped.NFeNFCe.Enums`.

#### Mantido

- Tipos universais continuam no `Core`: value objects fiscais, `ResultadoParse`/erros, `DescontinuadoAttribute` e enums fiscais transversais como `CodigoSituacaoDocumentoFiscal`, `IndicadorOperacao`, `OrigemMercadoria` e `ModalidadeFrete`.

### TecnoFisc.Sped.Txt.Engine

#### Adicionado

- Novo pacote de maquinaria TXT compartilhada pelos leiautes textuais. Contem contratos base (`RegistroSped`, `IArquivoSped`, `IBlocoSped`, `ILeitorSped`, `IEscritorSped`, `IRegistroSpedCatalogo`), atributos de metadados TXT, catalogo, parser/gerador `.txt`, helpers de streaming, `SnifferSped` e enums transversais TXT.
- Sniffer SPED textual (Stage 12): `SnifferSped.IdentificarAsync(Stream)` lê apenas a primeira linha `|0000|…|` e devolve `MetadadosArquivoSped { ProjetoSped, VersaoLeiaute, EncodingDetectado, … }`; `SnifferSped.AbrirParserAsync(Stream, factories)` devolve o `ILeitorSped` do leiaute identificado com o stream reposicionado na origem (replay-safe). O engine não referencia projetos de leiaute — quem monta o ponto de entrada registra as factories de `ParserEfdContribuicoes`/`ParserEfdIcmsIpi`/`ParserEcd`. (#512)

### TecnoFisc.Sped.Txt.Engine.SourceGenerators

#### Alterado (breaking)

- Pacote de source generators renomeado de `TecnoFisc.Sped.Core.SourceGenerators` para `TecnoFisc.Sped.Txt.Engine.SourceGenerators`. Continua sendo referenciado como analyzer (`OutputItemType="Analyzer"` e `ReferenceOutputAssembly="false"`) pelos pacotes de leiaute TXT.

### TecnoFisc.Sped.Xml.Engine

#### Adicionado

- Novo pacote de maquinaria XML compartilhada pelos leiautes XML. Contem `IdentificadorXmlFiscal` (sniffer do mundo XML — Stage 12), `IDocumentoFiscalXml` e `TipoDocumentoFiscalXml`, dependendo apenas de `TecnoFisc.Sped.Core`. O sniffer lê o início do stream com `XmlReader` forward-only, order-independent e XXE-safe (DTD proibido), e devolve `TipoDocumentoFiscalXml` (NF-e/NFC-e/eventos/envelope SERPRO), discriminando modelo 55 de 65 pelo `<mod>`.

### TecnoFisc.Sped.Txt

#### Adicionado

- Novo pacote guarda-chuva TXT, sem codigo proprio, agregando os leiautes textuais existentes (`TecnoFisc.Sped.EfdContribuicoes`, `TecnoFisc.Sped.EfdIcmsIpi` e `TecnoFisc.Sped.Ecd`).

### TecnoFisc.Sped

#### Adicionado

- Novo pacote guarda-chuva raiz, sem codigo proprio, agregando `TecnoFisc.Sped.Txt`. O guarda-chuva XML permanece adiado ate a chegada do CT-e.

### TecnoFisc.Sped.NFeNFCe

#### Adicionado (preview)

- **Pacote XML novo, read-only, em preview.** Estreia o primeiro leiaute do mundo XML (NF-e/NFC-e 4.00). Nesta release entrega as slices 14.1–14.4 da Stage 14: parser `XmlReader` forward-only, **order-independent** (tolera o XML canônico `nfeProc`/`NFe` e o envelope SERPRO de documento único), XXE-safe (`DtdProcessing.Prohibit`), e modelo tipado nativo da NF-e. (#505, #506, #507, #508)
- API pública: `ParserNFe` (`ReadNFeAsync(Stream)` → `Task<NFe>`; `ReadAsync(Stream)` → `Task<IDocumentoFiscalXml>` para pattern matching), `ParserNFeOptions` e os modelos `NFe`, `Identificacao`, `Emitente`, `Destinatario`, `Endereco`, `Item`, `Produto`, `Total`. Encoding canônico UTF-8.
- **Polimorfismo de impostos completo** (slice 14.4): grupo `imposto` com `Icms` (todos os CST `00`–`90` + `IcmsPart`/`IcmsST`/`IcmsSN` por CSOSN), `Ipi` (`IpiTrib`/`IpiNt`), `Pis` (`PisAliq`/`PisQtde`/`PisNt`/`PisOutr`/`PisSt`), `Cofins` (variantes análogas), `Ii` e `Issqn`.
- Sniffer XML `IdentificadorXmlFiscal` (mundo XML da Stage 12) consumido aqui para discriminar NF-e (modelo 55) de NFC-e (modelo 65) pelo `<mod>`; ver seção `TecnoFisc.Sped.Xml.Engine`.

> **Ressalva de preview.** O nome do pacote antecipa a cobertura-alvo (NF-e **e** NFC-e). Nesta `0.7.0` apenas a **NF-e modelo 55** está implementada e ainda **parcial**: faltam os grupos `transp`, `cobr`, `pag`, `infAdic`, `infRespTec`, `autXML` e `protNFe` (slice 14.5). **NFC-e modelo 65** (`infNFeSupl`/QR Code — slice 14.6), **eventos** (cancelamento/genérico — 14.7), `ReadDirectoryAsync` (14.8) e `Correlator` (14.9) ainda não existem. Use em produção apenas para os campos já cobertos; a API pode mudar até a cobertura completa. Tracking: `sped/STAGE_14_NFE_NFCE.md`.

## [0.6.0] — 2026-05-26

Adiciona o pacote **`TecnoFisc.Sped.Ecd`** (ECD — Escrituração Contábil Digital / Sped Contábil), cobrindo o leiaute 9 (vigente a partir do ano-calendário 2020) completo em modo **read-only**: 72 registros nos blocos `0 → C → I → J → K → 9`, parser streaming/buffered e modelo tipado, validado contra arquivo real anonimizado. No `Core`, adiciona `ReadingOptions` para descartar registros/blocos antes da materialização. Release aditiva (sem breaking changes em relação à `0.5.0`).

### TecnoFisc.Sped.Core 0.6.0

#### Adicionado

- `ReadingOptions` (`TecnoFisc.Sped.Core.Parser`) — permite ao consumidor descartar registros (`RegistrosIgnorados`) ou blocos inteiros (`BlocosIgnorados`) **antes** da materialização. O descarte acontece em nível de byte no `LeitorSpedTxt`: registros ignorados não são decodificados, não são devolvidos no stream e não entram na hierarquia Pai/Filhos — junto com toda a sua subárvore (filhos/netos). Útil para pular registros pesados como `J800`/`J801` da ECD (campo-arquivo RTF de até 30 MB). Os parsers de cada formato expõem um construtor que aceita `ReadingOptions`. Consequência documentada: contagens `9900`/`9990` e validações de hash podem não fechar quando há filtro — é escolha de quem lê. (#502)

### TecnoFisc.Sped.Ecd 0.6.0

#### Adicionado

- Pacote novo, **read-only** (§2.5 — sem `GeradorEcd`, sem `IEscritorSped`, sem round-trip de geração). Cobre a Stage 10 baseline do leiaute 9 (Manual de Orientação anexo ao Ato Declaratório Executivo Cofis nº 01/2026): **72 registros** distribuídos nos 6 blocos `0`, `C`, `I`, `J`, `K`, `9`, com `[RegistroSped]`/`[CampoSped]` declarados, hierarquia validada e fixtures por bloco. Tracking: `sped/STAGE_10_ECD_BASELINE.md`.
- API pública: `ArquivoEcd` (blocos `Bloco0`/`BlocoC`/`BlocoI`/`BlocoJ`/`BlocoK`/`Bloco9`), `BlocoEcd`, `ParserEcd` (`ReadStreamingAsync` → `IAsyncEnumerable<RegistroSped>`; `ReadAsync` → `Task<ArquivoEcd>` buffered). Encoding do `.txt`: ISO-8859-1 (Latin-1).
- Enum `LayoutEcd` (`V009 = 9`) informacional. Diferente do EFD, o `Registro0000` da ECD **não** carrega `COD_VER` (campo 02 é o literal `"LECD"`); a versão do leiaute contábil mora em `I010.COD_VER_LC` (`"9.00"` para AC2024+).
- Leitura de registros multi-linha `J800`/`J801` (campo-arquivo `ARQ_RTF`, conteúdo RTF embutido que pode conter quebras de linha e o delimitador `|`), sem corromper o parsing dos registros seguintes. (#499)
- Regras `REGRA_*` do manual e obrigatoriedade condicional dirigida por `IND_ESC` (`I010`) tratadas como `UPDATE/Doc` (doc-comment XML) — validação fiscal fica com o consumidor (§2.3). Formato malformado (tipo/tamanho/obrigatório ausente) continua sendo erro de parse.

### Documentação

#### Alterado

- Ordem de implementação dos pacotes restantes (todos **read-only**) definida como **NF-e → NFC-e → CT-e → ECF**. `ARCHITECTURE.md` (§2.5, §3, §4.7, §6, §12, §13) e `README.md` reordenados para refletir essa sequência — o ECF passa a ser o último leiaute textual planejado, depois dos três pacotes XML. `ARCHITECTURE.md` Stage 10 marcada como concluída na `0.6.0`.

## [0.5.0] — 2026-05-24

Release breaking. Revisa a convenção de nomenclatura da API pública (verbos, factories estáticos e predicados booleanos passam a usar inglês idiomático; substantivos do domínio SPED permanecem em português) e adiciona três helpers de persistência sobre o `IAsyncEnumerable<RegistroSped>` produzido pelos parsers: `OfType<T>()`, `Batch(n)` e `WithContext()`, mais um dispatcher Visitor source-generated por leiaute.

### Documentação

#### Alterado (breaking)

- Convenção de nomenclatura revisada (ARCHITECTURE §1.3): verbos, factories estáticos e predicados booleanos passam a usar inglês idiomático; substantivos do domínio SPED permanecem em português. API pública renomeada — ver detalhes abaixo.

### TecnoFisc.Sped.Core 0.5.0

#### Adicionado

- Namespace `TecnoFisc.Sped.Core.Streaming` com dois extension methods sobre o `IAsyncEnumerable<RegistroSped>` produzido pelos parsers: `OfType<T>()` filtra pelo tipo concreto de registro (zero reflection — pattern matching resolvido em compile-time) e `Batch(int size)` agrupa em lotes para bulk-insert em banco (EF Core `AddRangeAsync`, Dapper, `SqlBulkCopy`). Cobre o caso de uso mais comum de ingestão SPED → banco sem o consumidor precisar implementar boilerplate de cast + buffer manual. Memória continua bounded — só o lote corrente fica em memória. (#414)
- `WithContext()` (mesmo namespace) enriquece o stream com `ContextoPersistencia { IdRegistroAtual, IdPai }` contendo IDs surrogate sequenciais já amarrados à hierarquia. Resolve persistência relacional (PK/FK) sem o consumidor precisar manter stack manual de IDs. Overload `WithContext(startAt: ...)` permite retomar import multi-arquivo. (#416)

#### Alterado (breaking)

- Value objects: `Criar` → `Create` em `Cnpj`, `Cpf`, `Cfop`, `Ncm`, `Cst`, `ChaveAcesso`, `InscricaoEstadual`, `ModeloDocumento`, `GeneroItem`.
- `Cfop.EhEntrada` / `Cfop.EhSaida` → `Cfop.IsEntrada` / `Cfop.IsSaida`.
- `InscricaoEstadual.EhIsento` → `IsIsento`.
- `CodigosUf.EhValido` → `IsValid`.
- `ILeitorSped.LerStreamingAsync` / `LerAsync` → `ReadStreamingAsync` / `ReadAsync`. `LeitorSpedTxt` segue o mesmo.
- `IEscritorSped.EscreverAsync` → `WriteAsync`. `EscritorSpedTxt` segue.
- `CatalogoBuilder.ConstruirMetadadosDoTipo` → `BuildMetadataForType`.

### TecnoFisc.Sped.EfdContribuicoes 0.5.0

#### Alterado (breaking)

- `ParserEfdContribuicoes.LerAsync` / `LerStreamingAsync` → `ReadAsync` / `ReadStreamingAsync`.
- `GeradorEfdContribuicoes.EscreverAsync` → `WriteAsync`.
- `ArquivoEfdContribuicoes.CarregarAsync` → `LoadAsync`.

### TecnoFisc.Sped.EfdIcmsIpi 0.5.0

#### Alterado (breaking)

- `ParserEfdIcmsIpi.LerAsync` / `LerStreamingAsync` → `ReadAsync` / `ReadStreamingAsync`.
- `ArquivoEfdIcmsIpi.CarregarAsync` → `LoadAsync` (se aplicável).

### TecnoFisc.Sped.Core.SourceGenerators 0.5.0

#### Adicionado

- Source generator passa a emitir, por assembly consumidor, uma interface `IRegistroSpedVisitor` com um overload `VisitAsync(TipoConcreto)` default vazio para cada classe decorada com `[RegistroSped]`, mais `VisitUnknownAsync(RegistroSped)` para registros fora do assembly. Acompanha extension `RegistroSpedVisitorExtensions.DispatchAsync(IAsyncEnumerable<RegistroSped>, IRegistroSpedVisitor, CancellationToken)` que despacha cada registro para o overload correto via `switch` resolvido em compile-time. Permite ao consumidor evitar o `switch` gigante (200+ casos no EFD Contribuições, 255+ no EFD ICMS-IPI) sobrescrevendo apenas os tipos que importam. Zero reflection, zero boxing. (#415)

#### Alterado (breaking)

- Código gerado passa a invocar `Create` em vez de `Criar` nos value objects.

## [0.4.0] — 2026-05-24

Consolida três incrementos do leiaute EFD ICMS-IPI (V018, V019, V020 — vigente em 2026) e oficializa o pacote como **read-only**, alinhado ao caso de uso real (ingestão rápida + modelo tipado). Também esclarece o escopo dos pacotes XML (NF-e, NFC-e, CT-e), que passam a ser planejados também como read-only — o único pacote SPED com geração de arquivo confirmada permanece sendo o `TecnoFisc.Sped.EfdContribuicoes`.

### TecnoFisc.Sped.EfdIcmsIpi 0.4.0

#### Adicionado

- Suporte ao leiaute **V018** (Guias Práticos 3.1.5/3.1.6, vigência fiscal jan/2024): novos campos 21-23 `QTD_RESIDUO_DDG/WDG/CANA` no `Registro1391` e doc-comments mecânicos cobrindo NF3-e (modelo 66 no `RegistroC700`), Convênio 115/03 e escrituração consolidada NFCom (`D700`/`D730`/`D750`/`D760`), com reflexos nas apurações `E110`/`E113`/`E210`/`E240` e no `Registro1400`.
- Suporte ao leiaute **V019** (Guias Práticos 3.1.7/3.1.8/3.1.9, vigência fiscal jan/2025): novo campo `DED` (valor das deduções) em `RegistroD700` (32) e `RegistroD750` (17), e doc-comments mecânicos cobrindo CT-e Simplificado (`D130`), DSI no `C120`, observação sobre Reforma Tributária do Consumo (IBS/CBS/IS) no `C100`/`C190`, DIFAL EC 87/2015 no `0150` e revisões de obrigatoriedade/validação em `C700`/`D100`/`E113`/`D700`/`D750`.
- Suporte ao leiaute **V020** (Guias Práticos 3.2.0/3.2.1/3.2.2, vigência fiscal jan/2026 — leiaute vigente): novo campo `CAP_TANQUE` (capacidade do tanque em litros) em `Registro1310` (11) e doc-comments mecânicos cobrindo Reforma Tributária do Consumo + Ajuste SINIEF 49/25 no `C100`, orientações de preenchimento em `0150`/`D100`/`D700`/`K230` e mudança de tipo N→C do campo `SER` no `D700` (já modelado lazy como `string?` desde V017).

#### Alterado (breaking)

- Pacote passa a ser **read-only** (ARCHITECTURE §2.5). API pública `GeradorEfdIcmsIpi` removida; `IEscritorSped` deixa de ser implementado neste pacote. Consumidores que precisam emitir arquivos EFD ICMS-IPI devem usar o PVA da Receita ou outro caminho — o propósito do pacote é ingestão rápida + modelo tipado.
- `[Descontinuado(EmVersao=...)]` vira informacional no read path — registros descontinuados continuam sendo reconhecidos pelo parser para que arquivos históricos sejam lidos sem erro de leiaute.
- Testes de round-trip parse → generate → parse removidos (`RoundTripFixtureRealTests` renomeado para `ParserFixtureRealTests`, cobrindo apenas o caminho de leitura).

### Documentação

#### Alterado

- `ARCHITECTURE.md` §2.5 e §4.7, `README.md` e `CLAUDE.md` atualizados para refletir que os pacotes XML planejados (`TecnoFisc.Sped.NFe`, `NFCe`, `CTe`) também serão **read-only**. O caso de uso confirmado nos três é ingestão de XMLs já emitidos (parser + validação de assinatura + modelo tipado). Geração/emissão para SEFAZ depende de confirmação externa e, quando ocorrer, entra como stage dedicada (igual a ECD/ECF). Resultado: o único pacote SPED com geração de arquivo confirmada hoje é `TecnoFisc.Sped.EfdContribuicoes`.
- Stages 14/15/16 (NFe/NFCe/CTe) em `ARCHITECTURE.md` reescritos como pacotes read-only — `GeradorNFe`/`GeradorNFCe`/`GeradorCTe` saem do escopo inicial.
- README do repositório atualizado para refletir EFD ICMS-IPI 0.4.0 (V015 baseline + V016-V020 incrementos, parser apenas) e marcar pacotes XML como `planejado (XML, read-only)`.

## [0.3.1] — 2026-05-17

Corrige nomenclatura das versões do leiaute EFD ICMS-IPI: o que estava sendo chamado de `V306` é, na verdade, o leiaute **V015** (`COD_VER` do registro `0000`, conforme Tabela "Versão do Leiaute" da Nota Técnica EFD ICMS-IPI nº 2020.001 — Ato COTEPE/ICMS nº 44/2018). O número `306` é a versão do Guia Prático (3.0.6) que descreve o leiaute, não o leiaute em si. Múltiplas versões do Guia (3.0.6, 3.1.x, 3.2.x) descrevem o mesmo leiaute 015.

### TecnoFisc.Sped.EfdIcmsIpi 0.3.1

#### Alterado (breaking)

- Enum público `LayoutEfdIcmsIpi`: constante `V306` renomeada para `V015` (valor `306` → `15`). As 16 constantes anteriormente listadas como `V307`..`V322` foram removidas — elas mapeavam atualizações textuais do Guia Prático, não leiautes. Incrementos reais (`V016`, `V017`, …) serão adicionados conforme a Receita publicar novas Notas Técnicas com leiaute novo.
- Documentação (`ARCHITECTURE.md` §12, `README.md`, `CLAUDE.md`, `sped/STAGE_8_EFD_ICMS_IPI_V015.md`) reescrita para distinguir versão do leiaute (`COD_VER` do `0000`) de versão do Guia Prático.

#### Notas de migração

- Consumidores que usavam `LayoutEfdIcmsIpi.V306` devem trocar para `LayoutEfdIcmsIpi.V015`. Nenhum impacto em arquivos SPED gerados/lidos — o `COD_VER` correto no registro `0000` sempre foi `015`.
- Tracking file `sped/STAGE_8_EFD_ICMS_IPI_V306.md` renomeado para `sped/STAGE_8_EFD_ICMS_IPI_V015.md`.

## [0.3.0] — 2026-05-17

Conclui a Stage 8 baseline do `ARCHITECTURE.md`: EFD ICMS-IPI leiaute V015 (`COD_VER` do registro `0000`) com todos os 255 registros tipados, API pública de parser/gerador e validação round-trip end-to-end contra arquivo real emitido pelo PVA da Receita. *Nota: esta release foi originalmente publicada referindo-se ao leiaute como "V306"; ver release 0.3.1 para a correção de nomenclatura.*

### TecnoFisc.Sped.EfdIcmsIpi 0.3.0

#### Adicionado

- Pacote novo. Cobre a Stage 8 baseline V015 (Ato COTEPE/ICMS nº 44/2018, NT 2020.001, descrito no Guia Prático v3.0.6 e posteriores): 255 registros distribuídos nos 10 blocos (`0`, `B`, `C`, `D`, `E`, `G`, `H`, `K`, `1`, `9`), com `[RegistroSped]`/`[CampoSped]` declarados, validação de níveis hierárquicos e fixtures por bloco.
- API pública: `ArquivoEfdIcmsIpi`, `BlocoEfdIcmsIpi`, `ParserEfdIcmsIpi`, `GeradorEfdIcmsIpi`. Espelha o contrato de `EfdContribuicoes` — leitura streaming via `IAsyncEnumerable<RegistroSped>`, leitura buffered para o modelo tipado, escrita pipe-delimitada em Latin1/Windows-1252.
- Validação round-trip end-to-end (`RoundTripFixtureRealTests`) contra arquivo SPED real emitido pelo PVA, exercitando os 10 blocos. Invariante: `parse → serialize → parse → serialize` é byte-idêntica entre as duas passagens de serialização.
- Suporte para o registro `9999` final seguido de bloco PKCS#7 anexo: parser encerra silenciosamente no marcador `|9999|` e descarta o trailer binário da assinatura digital do PVA, sem perder registros nem cuspir erro de layout.

### TecnoFisc.Sped.Core.SourceGenerators 0.3.0

#### Corrigido

- `RegistroSpedCatalogoGenerator` agora honra `[SpedValor("S")]`/`[SpedValor("N")]` em membros de enum. Setter emitido vira sequência de `valor.SequenceEqual("X".AsSpan())` com despacho para o membro do enum; serializador vira `switch` por valor. Antes da correção, o gerador sempre emitia `int.Parse(valor)` para qualquer enum, o que quebrava em runtime qualquer campo SPED textual (`IndicadorSimNao` no EFD ICMS-IPI, descoberto via round-trip real). O caminho integral via `EnumUnderlyingType` continua intacto para enums sem `[SpedValor]`.
- Teste de regressão: `CatalogoSpedGeradoEnumTextualTests` exercita o `CatalogoSpedGerado` direto (não o builder reflexivo) com `Registro1010.IndExp`, garantindo que o caminho gerado lê/escreve `"S"`/`"N"` corretamente.

### Documentação

#### Alterado

- README e tabela de status do repositório passam a refletir EFD ICMS-IPI 0.3.0 publicada.
- Registrada regra dura de integração: merges para `dev` devem usar sempre Squash and Merge; branches de trabalho podem manter commits granulares.

## [0.2.0] — 2026-05-06

Conclui as Stages 5 e 6 de `ARCHITECTURE.md`: API streaming pública e source generator do catálogo de registros, com migração do `ParserEfdContribuicoes` para o catálogo gerado em compile-time.

### TecnoFisc.Sped.Core 0.2.0

#### Adicionado

- Contrato `ILeitorSped.LerStreamingAsync` (com `ReadOnlySpan<char>` no caminho dos campos) — semântica explícita de leitura registro-a-registro sem bufferizar o arquivo todo. Memória consumida fica limitada ao buffer do `PipeReader`, independente do tamanho do arquivo.
- Helper público `CatalogoBuilder.ConstruirMetadadosDoTipo` — usado pelo source generator para reutilizar a extração de campos via reflexão one-time durante a inicialização do catálogo gerado, mantendo zero reflexão na hot path.

#### Alterado

- `MetadadosCampo` passa a expor apenas dois delegates compostos: `Action<RegistroSped, ReadOnlySpan<char>>` para parse + atribuição e `Func<RegistroSped, string>` para serialização. O caminho reflexivo (`CatalogoBuilder`) adapta a API antiga preservando comportamento; o caminho gerado (Stage 6) implementa os delegates inline com casts diretos para o tipo concreto, sem boxing.
- `LeitorSpedTxt.LerAsync` renomeado para `LeitorSpedTxt.LerStreamingAsync`. Mantém a mesma assinatura (`IAsyncEnumerable<RegistroSped>`); o nome novo deixa claro que é o caminho memory-bounded.

### TecnoFisc.Sped.Core.SourceGenerators 0.2.0

#### Adicionado

- Pacote novo (`netstandard2.0`, `IsRoslynComponent=true`). Distribuído como Analyzer (`OutputItemType=Analyzer ReferenceOutputAssembly=false`), não embarca no runtime do consumidor.
- `RegistroSpedCatalogoGenerator` — `IIncrementalGenerator` que detecta classes decoradas com `[RegistroSped]` no projeto consumidor e emite, em compile-time, `CatalogoSpedGerado : CatalogoSpedBase` com o dicionário de registros já populado. Cada propriedade decorada com `[CampoSped]` vira um par de helpers privados estáticos (parse + serialize) com cast direto para o tipo concreto — sem `Assembly.GetTypes()`, sem `Expression.Compile`, sem boxing.
- Cobertura zero-alloc para os tipos de campo: `string`, `int/long/short` (e nullables), `decimal`, `DateOnly` (com `Formato`), `bool`, `char`, enums (com `EnumUnderlyingType` real) e os value objects fiscais expostos pelo Core (`Cnpj`, `Cpf`, `Cfop`, `Ncm`, `ChaveAcesso`, `InscricaoEstadual`, `ModeloDocumento`, `GeneroItem` — todos com `Criar(ReadOnlySpan<char>)`). Tipos fora dessa lista caem num fallback que delega para `ConversoresPrimitivosCatalogo`.

### TecnoFisc.Sped.EfdContribuicoes 0.2.0

#### Adicionado

- `ParserEfdContribuicoes.LerStreamingAsync(Stream)` — caminho streaming explicito, retorna `IAsyncEnumerable<RegistroSped>`. Os registros saem com Pai/Filhos já vinculados.
- `ParserEfdContribuicoes.LerAsync(Stream)` — conveniência buffered que devolve `Task<ArquivoEfdContribuicoes>` com todos os blocos populados; encapsula `LerStreamingAsync` + `ArquivoEfdContribuicoes.CarregarAsync`.

#### Alterado

- O parser passa a usar `new CatalogoSpedGerado()` por padrão (catálogo gerado em compile-time). Elimina o scan reflexivo via `Assembly.GetTypes()` que era feito na primeira chamada do parser. O construtor `ParserEfdContribuicoes(IRegistroSpedCatalogo)` continua disponível para injeção de catálogo customizado.

### Benchmarks

#### Adicionado

- Projeto `benchmarks/TecnoFisc.Sped.Benchmarks` (BenchmarkDotNet, .NET 10) com:
  - `StreamingVsBufferedBenchmark` — comparação memory-bounded entre `LerStreamingAsync` e `LerAsync` (buffered) sobre fluxos sintéticos de até 1M de registros, com `MemoryDiagnoser` ativo.
  - `PeakHeapProbe` (acionado por `--probe peak`) — sonda standalone que amostra `GC.GetTotalMemory` em segundo plano para evidenciar o pico de memória viva, métrica que a coluna `Allocated` do BDN não captura.
  - `InicializacaoCatalogoBenchmark` — comparação de tempo e alocação na inicialização do catálogo entre o caminho reflexivo (`CatalogoBuilder.BuildFromAssembly`) e o catálogo gerado (`CatalogoSpedGerado`).
  - `ParserCatalogoBenchmark` — comparação no caminho quente do parser entre os dois catálogos.

### Notas de release

- Stage 7 (`Layout V007`) descrito em `ARCHITECTURE.md` foi descartado: a Receita não publicou novo leiaute do EFD Contribuições desde a versão V006 (2021). O suporte a leiautes futuros volta à roadmap quando houver um.

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

[Não publicado]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/compare/v0.9.0...HEAD
[0.9.0]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/releases/tag/v0.9.0
[0.8.0]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/releases/tag/v0.8.0
[0.7.1]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/releases/tag/v0.7.1
[0.7.0]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/releases/tag/v0.7.0
[0.6.0]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/releases/tag/v0.6.0
[0.5.0]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/releases/tag/v0.5.0
[0.4.0]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/releases/tag/v0.4.0
[0.3.1]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/releases/tag/v0.3.1
[0.3.0]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/releases/tag/v0.3.0
[0.2.0]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/releases/tag/v0.2.0
[0.1.0]: https://github.com/tecnofisc-micro-sistemas/TecnoFisc.Sped/releases/tag/v0.1.0
