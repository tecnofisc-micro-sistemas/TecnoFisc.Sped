# ECF leiaute 12 read-only - design

Data: 2026-08-04. Status: aprovado.

## Objetivo

Adicionar o pacote `TecnoFisc.Sped.Ecf` para leitura da Escrituração Contábil Fiscal. O pacote terá um modelo tipado único baseado no leiaute 12 e aceitará arquivos dos leiautes 8 a 12, cobrindo a janela fiscal de cinco anos adotada pelo repositório.

A implementação será incremental, mas o pacote só estará apto a publicação quando o leiaute estiver completo. O critério de completude é reconhecer os 180 registros dos 17 blocos, validar a estrutura contra o manual e ler integralmente as fixtures de aceitação.

Fonte autoritativa do baseline: `sped/guides/Manual_ECF_Leiaute_12_29_01_2026.pdf`, Manual de Orientação do Leiaute 12 da ECF, anexo ao ADE Cofis nº 02/2026, atualização janeiro/2026.

## Decisões confirmadas

- Pacote read-only: parser e modelo tipado, sem gerador.
- Modelo único do leiaute 12, sem subclasses por versão.
- Compatibilidade de leitura dos leiautes 8, 9, 10, 11 e 12.
- Desenvolvimento incremental por bloco e substage.
- Release somente após implementação integral.
- Abordagem híbrida: extração determinística auxilia o trabalho, mas o manual e a revisão das páginas continuam autoritativos.
- Quatro arquivos privados dos leiautes 8 a 11 serão usados apenas em testes locais.
- O CI receberá fixtures compactas e anonimizadas derivadas desses arquivos.

## Escopo do leiaute

O leiaute 12 contém 180 registros distribuídos em 17 blocos, na ordem canônica:

`0 -> C -> E -> J -> K -> L -> M -> N -> P -> Q -> T -> U -> V -> W -> X -> Y -> 9`.

Todos os registros precisam ser reconhecidos antes da publicação. Registros que não aparecem nos quatro arquivos reais ainda serão implementados a partir do manual.

## Arquitetura

### Tooling de preparação

Um conjunto de ferramentas sob `tools/ecf-layout/` preparará o manual para consumo durante o desenvolvimento. A solução reutilizará, de forma adaptada e isolada, os padrões comprovados no repositório `G:\tecnofisc\incidencia-consumo-lab`:

- `pymupdf4llm` com versão fixada para conversão PDF para Markdown table-aware;
- cache identificado pelo hash do PDF e do conversor;
- reparos determinísticos de tabelas e nomes de campos;
- fragmentação por registro;
- testes baseados em defeitos reais dos guias SPED.

O tooling será executado explicitamente. Ele não participará de `dotnet build`, `dotnet test`, empacotamento ou runtime do consumidor.

O perfil específico da ECF deverá:

- processar o PDF por páginas ou lotes retomáveis;
- remover cabeçalhos e rodapés próprios do manual da ECF;
- distinguir ocorrências de sumário das seções detalhadas;
- exigir exatamente 180 códigos únicos no conjunto detalhado;
- detectar registros duplicados, ausentes, sem tabela ou com tabelas ambíguas;
- renderizar páginas suspeitas para inspeção visual;
- manter cache e conteúdo extenso do manual fora do Git.

### Artefatos de controle

O preparo produzirá:

- um fragmento local por registro;
- o manifesto estrutural versionado `sped/ecf/layout-12-manifest.json`, com código, bloco, página, nível, ocorrência e campos extraídos;
- um relatório de inconsistências e itens em quarentena;
- o tracker versionado `sped/STAGE_17_ECF_BASELINE.md`, com 180 substages.

O tracker e o manifesto depois de revisados serão versionados. O Markdown integral derivado do manual, imagens renderizadas e caches permanecerão ignorados.

O manifesto é uma ajuda verificável, não uma segunda fonte normativa. Uma classe não será considerada correta apenas porque corresponde à saída do extrator; a página relevante do manual também deverá ser revisada.

### Pacote público

`TecnoFisc.Sped.Ecf` seguirá o padrão do `TecnoFisc.Sped.Ecd`:

- `Parser/ParserEcf.cs`;
- `ArquivoEcf.cs`;
- `BlocoEcf.cs`;
- `Registros/Bloco*/`;
- `Enums/`;
- `Versionamento/LayoutEcf.cs`;
- referência ao source generator apenas como analyzer;
- referências de runtime somente a `TecnoFisc.Sped.Core` e `TecnoFisc.Sped.Txt.Engine`.

Não existirão `GeradorEcf`, pasta `Gerador/` nem API pública de escrita.

O sniffer reconhecerá o literal `LECF` e a versão declarada pelo `Registro0000`. A promoção para os metapacotes `TecnoFisc.Sped.Txt` e `TecnoFisc.Sped` ocorrerá apenas no fechamento da stage, evitando uma superfície pública incompleta.

## Versionamento do modelo

`LayoutEcf` declarará `V008` a `V012`. `Registro0000.CodVer` fornecerá a versão declarada pelo arquivo.

O modelo continuará único:

- registros novos recebem `IntroduzidoEm`;
- campos novos recebem `DesdeVersao`;
- alterações compatíveis de tamanho são aplicadas no atributo atual e documentadas;
- mudanças incompatíveis de tipo usam um tipo capaz de preservar todas as versões, normalmente `string`, com interpretação opcional pelo consumidor;
- não haverá tipos como `RegistroXxxxV009`;
- a versão é informacional e não seleciona outro catálogo.

A autoridade para determinar vigência será, em ordem:

1. manuais oficiais das versões correspondentes, quando disponíveis;
2. histórico de alterações publicado pela Receita;
3. estrutura observada nas fixtures reais como evidência de uma diferença a investigar, nunca como única especificação.

Os manuais ou históricos oficiais necessários para os leiautes 8 a 11 deverão ser obtidos antes de declarar completa a respectiva compatibilidade. Uma fixture que parseia sem erro não basta para provar que todos os campos foram modelados semanticamente.

Diferenças legítimas de versões serão modeladas explicitamente. O modo leniente não será usado para ocultá-las.

## Tabelas dinâmicas

A planilha oficial de tabelas dinâmicas do leiaute 12 encontrada no corpus local possui 79 planilhas e servirá como fonte complementar de documentação, amostras e conferência de códigos.

Essas tabelas não serão automaticamente embarcadas no pacote e não criarão validação tributária em runtime. Códigos pequenos e estáveis podem originar enums após revisão. Domínios extensos, parametrizados ou sujeitos a atualização permanecerão como `string`.

## Fronteira de validação

A biblioteca validará conformidade de formato:

- código e estrutura de campos;
- conversão de datas, números, enums estáveis e value objects;
- hierarquia sintática;
- encoding e terminação das linhas.

Permanecem sob responsabilidade do consumidor e do PGE:

- obrigatoriedade tributária condicional;
- cruzamentos entre registros e blocos;
- reconciliação de saldos;
- cálculos de IRPJ e CSLL;
- regras fiscais identificadas como `REGRA_*`.

Essas regras poderão aparecer em comentários XML quando forem relevantes para o uso correto do campo, seguindo a política já adotada pelo ECD.

## Fluxo de implementação

### 17.000 - preparação do corpus

- Adaptar o extrator.
- Processar o manual em lotes retomáveis.
- Isolar a seção detalhada dos registros.
- Produzir e validar os 180 fragmentos únicos.
- Gerar o manifesto e o tracker.
- Revisar visualmente páginas classificadas como ambíguas.

### 17.001 - fundação

- Criar os projetos de produção e testes.
- Registrar os projetos na solução.
- Criar `LayoutEcf` com `V008` a `V012`.
- Implementar parser, arquivo, blocos e catálogo mínimo.
- Integrar `LECF` ao sniffer sem promover ainda os metapacotes.
- Implementar `Registro0000`.

### Registros por ordem canônica

Os registros serão implementados na ordem dos blocos. Registros com filhos, campos condicionais, novos tipos fiscais ou documentação extensa terão uma substage própria. Aberturas, encerramentos e folhas triviais contíguas poderão ser agrupados, preferencialmente dentro do mesmo bloco e com limite aproximado de dez registros por PR.

### Gate por registro

- Página e tabela revisadas.
- Classe e atributos implementados.
- Enums e value objects criados no primeiro uso.
- Testes de catálogo, campos e parsing de linha.
- Tracker atualizado.

### Gate por bloco

- Todos os códigos do bloco presentes no catálogo.
- Hierarquia pai/filho coberta.
- Fixture sintética do bloco lida integralmente.
- Comparação automática entre manifesto e assembly sem divergências.

### Compatibilidade progressiva

Os quatro arquivos privados serão executados localmente à medida que a cobertura aumentar. Códigos ainda não implementados serão classificados pelo tracker; diferenças de quantidade ou semântica de campos serão investigadas como possíveis deltas de versão.

## Estratégia de testes

### Extrator

Testes com recortes pequenos cobrirão:

- cabeçalhos de tabela fundidos;
- nomes de campo quebrados;
- títulos repetidos entre sumário e detalhe;
- fragmentação por registro;
- cabeçalhos e rodapés da ECF;
- contagem e unicidade dos 180 registros.

### Manifesto

O manifesto terá schema validado e invariantes sobre códigos, ordem, blocos, páginas, níveis, ocorrência e sequência de campos.

### Registro

Cada registro cobrirá:

- `RegistroSpedAttribute`;
- `CampoSpedAttribute`;
- ordem, tipo e opcionalidade dos campos;
- fábrica do catálogo;
- atribuição dos campos;
- campos vazios e formatos inválidos relevantes;
- parsing de uma linha extraída ou construída a partir do manual.

### Bloco e arquivo

Fixtures sintéticas validarão catálogo, hierarquia e materialização em `ArquivoEcf`.

### Aceitação

- Quatro fixtures compactas e anonimizadas dos leiautes 8 a 11 serão executadas no CI.
- Os quatro arquivos privados completos serão executados somente localmente.
- Um arquivo real do leiaute 12 será adicionado ao gate final quando estiver disponível.

Os testes privados localizarão os arquivos pela variável de ambiente `TECNOFISC_SPED_ECF_FIXTURES_DIR`, sem caminho absoluto ou identificação empresarial no código. Na ausência da configuração local, serão explicitamente ignorados.

## Anonimização

O anonimizador será determinístico e consciente dos formatos SPED:

- o mesmo valor original receberá o mesmo pseudônimo dentro da fixture;
- CNPJ e CPF serão substituídos por documentos fictícios com dígitos verificadores válidos;
- datas serão deslocadas consistentemente;
- textos serão substituídos preservando encoding e limites de tamanho;
- valores numéricos manterão sinal, escala e casas decimais;
- chaves relacionadas entre registros manterão correlação;
- a amostra conservará ao menos uma ocorrência de cada código presente e seus ancestrais necessários;
- logs não conterão valores, nomes ou caminhos privados.

Antes de versionar uma fixture, uma verificação procurará identificadores originais conhecidos e comparará hashes para impedir a inclusão acidental do arquivo fonte.

## Tratamento de falhas

O preparo será fail-closed:

- conversão incompleta não promove o manifesto revisado;
- duplicidade, ausência ou tabela ambígua vai para quarentena;
- ambiguidade requer inspeção da página renderizada;
- arquivos são produzidos em área temporária e promovidos somente após validação integral.

No parser:

- erros de campos conhecidos usam os resultados estruturados do `Txt.Engine`;
- registros desconhecidos falham no modo estrito;
- o modo leniente existente pode preservar leitura parcial sem lógica exclusiva do ECF;
- cancelamento é propagado pelas APIs assíncronas;
- `ReadStreamingAsync` é o caminho recomendado para arquivos grandes.

## Gates de qualidade

Cada entrega requer:

- build sem warnings;
- testes da substage e do projeto ECF;
- comparação manifesto versus catálogo;
- testes da solução quando código compartilhado for alterado;
- revisão do diff contra a página do manual;
- ausência de dados privados;
- Conventional Commit válido;
- tracker atualizado no mesmo commit da implementação.

A Stage 17 só estará concluída com:

- 180 de 180 registros;
- 17 de 17 blocos;
- compatibilidade dos leiautes 8 a 12 demonstrada;
- zero código desconhecido nos quatro arquivos completos;
- zero divergência entre manifesto e classes;
- fixtures anonimizadas aprovadas;
- documentação pública, sniffer e metapacotes atualizados;
- testes da solução aprovados.

## Fora de escopo

- Geração ou escrita de ECF.
- Validação tributária ou reprodução das regras do PGE.
- Cálculos de IRPJ e CSLL.
- Banco de dados, rede ou configuração externa no pacote de runtime.
- Publicação parcial de blocos.
- Inferência de schema exclusivamente a partir de arquivos reais.
