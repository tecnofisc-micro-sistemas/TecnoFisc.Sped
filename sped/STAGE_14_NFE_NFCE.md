# Stage 14 — TecnoFisc.Sped.NFeNFCe v1 (design + vertical slices)

> Operational appendix to `ARCHITECTURE.md` §14. **Funde os antigos Stage 14 (NFe) + Stage 15 (NFCe) em um único pacote** `TecnoFisc.Sped.NFeNFCe`, **read-only**, leiaute 4.00.
>
> Spec de design da v1, consolidando o brainstorm de 2026-05-26 e o documento de decisões original (`NFE_V1_DECISOES.md`). Cada decisão aqui **trava** o desenho; detalhes de implementação ficam a critério do agente respeitando estas escolhas + `ARCHITECTURE.md`.

## 0. Por que pacote único (delta vs ARCHITECTURE.md)

NFe (mod 55) e NFCe (mod 65) usam o **mesmo XSD** (`leiauteNFe_v4.00.xsd`) e **evoluem juntas** (4.00 para ambas, mesma Nota Técnica). A regra de independência de formato (`ARCHITECTURE.md` §4.2) existe porque registros SPED evoluem em **cadências independentes** — NFe/NFCe **não** se encaixam nessa premissa. NFCe ≈ NFe + `infNFeSupl` (QR Code) + `dest` opcional + restrições de grupos. Logo: **um pacote, dois tipos de modelo** (`NFe`, `NFCe`).

**Deltas a aplicar em `ARCHITECTURE.md` (deliverable do primeiro PR):**
- Fundir Stage 14 + Stage 15 → Stage 14 `TecnoFisc.Sped.NFeNFCe`. **Não renumerar a jusante** (evita churn): Stage 16 (CT-e) e Stage 17 (ECF) mantêm os números; o antigo Stage 15 fica marcado como "absorvido pelo Stage 14".
- Tabela de escopo (§3): linha `NF-e | TecnoFisc.Sped.NFe` + `NFC-e | TecnoFisc.Sped.NFCe` → uma linha `NF-e / NFC-e | TecnoFisc.Sped.NFeNFCe | XML (UTF-8)`.
- Mapa de pastas (§6) e regras de dependência (§6.1): um diretório `src/TecnoFisc.Sped.NFeNFCe/`.
- Metapacote (§13): referenciar `NFeNFCe` em vez de `NFe` + `NFCe`.
- §2.5 / §12 nota 12: tabela read-only passa a citar `NFeNFCe`.

## 1. Escopo da v1

Read-only. Cobre:
- **NFe mod 55** (`procNFe` autorizada e `NFe` pura)
- **NFCe mod 65** (mesmo XSD, semântica distinta)
- **Eventos** (`procEventoNFe` e `eventoNFe`) — ver §6 para a estratégia tipado-vs-genérico

Profundidade = **modelo essencial**, critério: **o que a EFD ICMS-IPI exige por documento fiscal**. O modelo é **nativo da NFe** (espelha a estrutura do XML), **não** moldado em registro C100 — "paralelo a C100" é só referência conceitual de escopo, não de forma. Concretamente cobre: identificação (`ide`), emitente (`emit`), destinatário (`dest`, opcional em NFCe), itens (`det`/`prod` + impostos completos por CST/CSOSN), totais (`total`), transporte essencial (`transp`), cobrança (`cobr`), pagamento (`pag`), informações adicionais (`infAdic`), protocolo (`protNFe`), responsável técnico (`infRespTec`), autorizados a download (`autXML`).

**Fora da v1** (release posterior atrás de API/pacote separado): combustíveis, medicamentos, veículos novos, armamento, cana, papel imune, exportação detalhada (`detExport`), importação detalhada (DI/`adi`), rastreabilidade, intermediador detalhado. Também fora: assinatura ICP-Brasil (validação), validação XSD, NFe 3.10, geração/emissão, source generator, modo header-only, CT-e/MDF-e.

## 2. Fonte de verdade (não há guia consolidado como nos SPEDs)

O **XSD é a fonte de verdade** do XML — define cada elemento, tipo, tamanho, ocorrência (`min/maxOccurs`) e ordem canônica (`xs:sequence`). Melhor que PDF: machine-readable e exato. Schemas locais (gitignored, `sped/schemas/nfe/`):

- `PL_010c_NT2022_002v1.30/` — NFe **e** NFCe leiaute 4.00: `leiauteNFe_v4.00.xsd` (tudo: ide/emit/dest/det/prod/imposto/total/transp/cobr/pag/infAdic/`infNFeSupl`), `nfe_v4.00.xsd` (procNFe), `tiposBasico_v4.00.xsd`, `DFeTiposBasicos_v1.00.xsd`, `xmldsig-core-schema_v1.01.xsd`.
- `Evento_Canc_PL_v1.01/` — cancelamento 110111: `e110111_v1.00.xsd`, `eventoCancNFe_v1.00.xsd`, `procEventoCancNFe_v1.00.xsd`, etc.

**Faltando** (a dropar quando forem tipados, release posterior): CCe (110110), manifestação (210xxx), EPEC (110140), `leiauteEvento` genérico.

**Envelope SERPRO sem XSD próprio (confirmado na doc da API).** A SERPRO **não publica schema próprio**: declara que a resposta segue o leiaute NF-e do ENCAT (os XSD oficiais que já temos em `sped/schemas/nfe/`). Logo o **conteúdo interno** (`nfeProc`/`NFe`/eventos) conforma ao XSD oficial — apenas o **envelope externo** (`soap:Envelope` → `retConsNFeLog` → `NFeLog`), a reordenação de elementos e a conversão atributo→elemento são SERPRO-específicos. Reverse-engineering fica restrito ao **wrapper**, a partir do fixture (`sped/fixtures/xml/*_serpro.xml`). A SERPRO também oferece resposta em **JSON** (mesma spec do XSD; números em formato NUMBER / notação científica) — **fora do escopo v1** (a lib processa XML).

Fonte: doc da API SERPRO Consulta NF-e (`apicenter.estaleiro.serpro.gov.br/documentacao/consulta-nfe`), seções "Leiautes e formatos" e "Tipos de Eventos retornados".

`sped/schemas/` é **gitignored** (mesmo padrão de `sped/guides/`).

## 3. Parser core (a trava de design)

`XmlReader` da BCL forward-only como tokenizador. Config obrigatória: `Async=true`, `IgnoreComments=true`, `IgnoreProcessingInstructions=true`, `IgnoreWhitespace=true`, `DtdProcessing=Prohibit` (XXE), `CloseInput=false`.

**REGRA DE OURO — zero desserializador posicional.** Todo tipo é desserializado por um loop `switch (reader.LocalName)` **order-independent**. Razão: o arquivo SERPRO embaralha a ordem dos elementos em **todo nível**, inverte `protNFe`/`NFe`, e transforma atributos (`Id`, `versao`) em elementos-filho. Um desserializador posicional quebra; o switch-loop lê **canônico e SERPRO na mesma velocidade** (despacho O(1) por elemento; posicional exigiria mais ramos pra validar ordem). Order-independence = robustez **e** performance, alinhadas.

Consequências:
- `Id` / `versao`: ler como **atributo ou elemento-filho** (checar ambos).
- **SOAP-unwrap + colheita de eventos embutidos** ficam em **um lugar** (entry point), não espalhados pelos desserializadores. O entry point varre até achar o nó relevante (`infNFe` / `NFe` / `nfeProc`) independente do que o embrulha (envelope SOAP, `NFeLog`, raiz nua).
- Sem `XDocument`/DOM por arquivo (sem alocação de árvore).
- **Sem source generator na v1.** Desserializadores à mão num padrão único (helper `ReadChildren(reader, dispatch)` + `switch` plano por tipo). Mecânico, revisável, testável isolado. O padrão uniforme é o alvo ideal pra codegen depois (decisão de promover entra **após a slice 4**, quando ~10-15 desserializadores manuais existirem).

`ParserNFeOptions` (contrato mínimo v1):
- `Strict` (default `false`) — elemento desconhecido é ignorado, não lança. Protege contra Notas Técnicas futuras.
- `ValidateChecksums` (default `true`) — valida DV de chave/value objects na construção.
- `Parallelism` (default `Environment.ProcessorCount`) — para `ReadDirectoryAsync`.
- `FailFast` (default `true`).

`ParserNFe` é thread-safe sem estado mutável (documentar).

## 4. Modelagem

- `NFe` e `NFCe` são **tipos distintos**. Base comum só onde a estrutura é literalmente idêntica (`Ide` parcial, `Emit`, value objects). **Sem polimorfismo entre os dois** — o consumidor sabe o que processa via identificador de documento.
- **Polimorfismo de impostos:** `abstract record` na base + `sealed record` nos concretos. ICMS (todas variantes CST 00/10/20/30/40/41/50/51/60/70/90 + CSOSN 101/102/103/201/202/203/300/400/500/900 + ICMSPart, ICMSST, ICMSSN, FCP, desoneração), IPI (IPITrib/IPINT), PIS, COFINS, II, ISSQN.
- **Records grandes** (`NFe`, `NFCe`, `Item`, `Evento`): override manual de `Equals`/`GetHashCode` por identidade de domínio (chave; chave+nItem para `Item`; chave+nSeqEvento para `Evento`). Equality estrutural só nos compostos pequenos (value objects, grupos de imposto).
- **Records com muitos opcionais** (`Total`, `IcmsTotal`, `Prod`, `InfAdic`): `record` com ctor implícito + `required`/`init`. Ctor primário só nos compactos.
- `NFe.Protocolo` é `Protocolo?` nullable (preenchido quando `procNFe`, `null` quando `NFe` pura) + predicado `IsAutorizada`. Sem subtipo `NFeAutorizada` nem `NFeCancelada` — autorização é flag binária; "cancelada" é correlação (§6.1), não estado do XML. Status rico autorizada/cancelada vive no `Correlator` (§6.1).

**Value objects — reusar Core, não duplicar:**
- `ChaveAcesso` (Core, já `readonly struct` completo: DV mod-11, CNPJ embutido validado, UF validada, props decompostas; cobre 55/65/57) → **NÃO criar `ChaveAcessoNFe`**.
- `ModeloDocumento` (Core, Tabela 4.1.1, já tem 55 e 65) → reusar.
- Reusar existentes: `Cnpj`, `Cpf`, `Cfop`, `Ncm`, `Cst`, `InscricaoEstadual`, `CodigosUf`.

**Value objects novos — criar no Core** (transversais, servem CT-e futuro):
- `Cest` (7 dígitos com DV), `Gtin` (8/12/13/14 com checksum; aceitar `SEM GTIN`), `CodigoMunicipioIbge` (7 dígitos), `Csosn`.
- Enums do `<ide>` e correlatos: `TipoAmbiente`, `TipoEmissao`, `FinalidadeEmissao`, `IndicadorPresenca`, `IndicadorIntermediador`, `ModalidadeFrete`, `OrigemMercadoria`.

## 5. API pública

- `ParserNFe.ReadNFeAsync` / `ReadNFCeAsync` / `ReadEventoAsync(Stream, ct)` — consumidor sabe o tipo.
- `ParserNFe.ReadAsync(Stream, ct)` → `IDocumentoFiscalXml` — fluxo genérico/diretório (pattern matching no retorno).
- `ParserNFe.ReadDirectoryAsync(path, ParserNFeOptions, ct)` → `IAsyncEnumerable<IDocumentoFiscalXml>` — **caminho principal "milhões de XMLs"**. Paralelismo I/O interno (Channel + N consumidores). Erro por arquivo: **log + skip** default, configurável.
- Integração com helpers de streaming do Core (`OfType<NFe>()`, `Batch(n)`, `WithContext()`): funcionam porque o stream é `IAsyncEnumerable<IDocumentoFiscalXml>`.

**Eventos embutidos = itens separados no stream.** Um arquivo SERPRO/`NFeLog` com `nfeProc` + N `procEventoNFe` emite **itens distintos**: a `NFe` (com `Protocolo`) e cada `Evento` como item próprio. Consumidor correlaciona por `ChaveAcesso`. Mantém o modelo de streaming limpo (decisão do brainstorm).

**Sniffer `IdentificadorXmlFiscal`** (Core, espelha `IdentificadorArquivoSped`): `Stream` → enum `TipoDocumentoFiscalXml` { `NFeProc`, `NFe`, `NFCeProc`, `NFCe`, `ProcEventoNFe`, `EventoNFe`, `NFeLogSerpro`, `Desconhecido` }. Distingue 55 de 65 lendo até `<mod>`. **Atenção:** no SERPRO o `<ide>` vem quase no fim — o sniffer pode precisar varrer mais fundo nesse formato (ou reconhecer o envelope `NFeLog` primeiro).

## 6. Eventos e cancelamento (read-only, stateless)

**Escopo v1:** `EventoCancelamento` tipado (first-class) + `EventoGenerico` fallback para todos os demais (CCe, manifestação 210xxx, EPEC, vinculação CT-e 610600, etc.). Tipar os demais em release posterior quando os XSD forem dropados. O fixture SERPRO (210210 + 610600) cai em `EventoGenerico` na v1 — comportamento esperado e testado.

**Dois formatos de evento na resposta SERPRO** (confirmado na doc da API — ver §2):
- **Completo:** cancelamento 110111, CCe 110110, EPEC 110140, manifestação 210200–210240, SUFRAMA, prorrogação 111500–411503 (todos os campos do leiaute do evento).
- **Resumido:** 610500–610615 (vinculação CT-e/MDF-e, registro de passagem) — só `cOrgao`, `dhEvento`, `tpEvento`, `descEvento`, `nProt`, `dhRegEvento`. **O 610600 "parcial" do fixture é este leiaute resumido, NÃO malformado.**

`EventoGenerico` expõe: `tpEvento`, `descEvento`, `dhEvento`, `cOrgao`, `chNFe`, `nProt`, `cStat`, `nSeqEvento`, `dhRegEvento` + `detEvento` cru (string/sub-árvore). **Todos os campos opcionais/nullable** para absorver tanto o leiaute completo quanto o resumido sem lançar (com `Strict=false`).

**Sinalização de cancelamento** (requisito explícito — nota cancelada causa problemas a jusante). A lib é **read-only e stateless** (stream entra, modelo sai, sem estado cross-arquivo) → não muta uma NFe lida antes como "cancelada" por um evento que chega em outro arquivo depois. Sinalização correta dentro da restrição:
- `EventoCancelamento` (sealed record, 110111): `chNFe`, `nProt` (protocolo do cancelamento), `nProtRef` (autorização cancelada), `xJust`, `dhEvento`.
- Efetividade via `retEvento.cStat`: **135** (registrado/vinculado) ou **155** (fora do prazo) ⇒ cancelamento válido. Predicado `IsEffective`.
- Correlação por `ChaveAcesso`: consumidor agrupa o stream por chave; presença de `EventoCancelamento` efetivo ⇒ nota cancelada. **Documentar esse padrão em destaque** (README + doc-comment).
- No SERPRO empacotado (`nfeProc` + cancelamento no mesmo arquivo): itens separados, mesma chave → correlação trivial dentro do arquivo.

Mutar a NFe para "cancelada" entre arquivos seria **estado** — viola read-only/stateless. O sinal é um **evento tipado inconfundível + predicado de efetividade + correlação por chave documentada**.

`NFe`/`NFCe` expõem `Protocolo?` nullable + predicado `IsAutorizada` (`Protocolo is not null`). **Não** há subtipo `NFeAutorizada` nem `NFeCancelada` — autorização é flag binária (predicado idiomático); "cancelada" é correlação (abaixo), não estado do XML.

### 6.1 Correlator (agregado autorizada/cancelada, stateless)

Helper opcional que correlaciona notas e eventos **já lidos** (função pura sobre a coleção passada — zero estado cross-arquivo de parser). Produz:
```
NFeComEventos { IDocumentoFiscalXml Documento; IReadOnlyList<Evento> Eventos;
                bool IsAutorizada; bool IsCancelada; EventoCancelamento? Cancelamento; }
```

**Dois APIs com perfis de performance distintos** (join notas × eventos):

- **`Correlator.Correlate(IEnumerable<IDocumentoFiscalXml>)` → `IEnumerable<NFeComEventos>`** — agrupa por `ChaveAcesso`. **O(N) tempo, O(N) memória.** Uso: coleção **limitada** (uma resposta SERPRO, um `Batch(n)`, um chunk de diretório). **Não** usar para milhões numa chamada — segurar tudo em memória estoura.
- **Padrão índice (escalável a milhões):** explora que cancelamento é fração pequena do total.
  - `Correlator.BuildCancelamentoIndex(IAsyncEnumerable<Evento>)` → `IReadOnlyDictionary<ChaveAcesso, EventoCancelamento>` — stream só os eventos, filtra cancelamento efetivo. Memória = **O(nº de cancelamentos)**, não O(total de notas).
  - Depois stream as notas e `index.TryGetValue(nfe.ChaveAcesso, …)` O(1). **Memória constante** no passo das notas. Total: O(N) tempo, O(#cancelamentos) memória.
- **SERPRO empacotado:** correlação **por arquivo** (eventos vêm junto da nota no `NFeLog`) — O(eventos do arquivo), sem índice. Caso de cancelamento mais comum na ingestão SERPRO.

**Benchmark obrigatório:** `ChaveAcesso.GetHashCode` recomputa hash de string de 44 chars por lookup (BCL não cacheia). Se aparecer no profile de milhões de lookups, otimizar com hash pré-computado no struct. Struct evita alocação por chave (string-backing já existe do parse).

## 7. Fixtures (corpus)

As fixtures cruas (reais) ficam em `sped/fixtures/xml/` (**gitignored**). As **anonimizadas** para teste vão **commitadas** sob `tests/` (mesmo padrão dos `.txt` SPED).

O fixture atual (`31210786666716000148550050007156531436322400[_serpro].xml`) cobre **um caso só**: NFe 55, CRT=3, ICMS60, IPITrib CST99, PISNT/COFINSNT CST08, totais zerados (só ST), protNFe autorizado; SERPRO embute eventos 210210 (completo) + 610600 (leiaute resumido, não malformado — ver §6). **Corpus a montar** (≥1 por ramo polimórfico): demais CSTs ICMS + CSOSN + FCP + desoneração; PIS/COFINS tributados; IPINT; NFCe com `infNFeSupl`/QR + `pag` cartão; totais não-zerados (vICMS/vFrete/vSeg/vDesc/II/ISSQNtot); evento de cancelamento standalone. **Não** precisam de fixture na v1: combustível/medicamento/veículo/importação/exportação (fora do escopo).

## 8. Vertical slices (ordem de implementação)

Cada slice entrega valor testável. PR por slice (ou sub-PR coeso dentro de uma slice grande).

| Feito | Slice | Entrega |
| --- | --- | --- |
| [ ] | 14.1 | **Fundação Core:** value objects novos (`Cest`, `Gtin`, `CodigoMunicipioIbge`, `Csosn` + enums do `<ide>`) e `IdentificadorXmlFiscal` + enum `TipoDocumentoFiscalXml`. Reusa `ChaveAcesso`/`ModeloDocumento`. Testes unitários extensos (especialmente DV/checksum). Sem parser. |
| [ ] | 14.2 | **Estrutura do pacote** `src/TecnoFisc.Sped.NFeNFCe/` + `tests/` + registro no `.slnx`. `ParserNFeOptions`, esqueleto de `ParserNFe`, `IDocumentoFiscalXml`. **Deliverable: editar `ARCHITECTURE.md` (§0 deltas).** |
| [ ] | 14.3 | **Piloto NFe 55 mínimo:** `Ide`, `Emit`, `Dest`, `Total`, `Item`+`Prod`+ICMS60 (usa o fixture real). Desserializador à mão order-independent. **SOAP-unwrap do SERPRO single-doc** (lê a NFe de dentro do `NFeLog`). E2E com NFe real (canônico + SERPRO) + benchmark inicial. |
| [ ] | 14.4 | **Polimorfismo de impostos completo:** ICMS (todos CST/CSOSN + ICMSPart/ICMSST/ICMSSN/FCP/desoneração), IPI, PIS, COFINS, II, ISSQN. ← **decisão sobre source generator entra após esta slice.** |
| [ ] | 14.5 | **Resto do essencial NFe 55:** `transp`, `cobr`, `pag`, `infAdic`, `infRespTec`, `autXML`, `protNFe` (`Protocolo`). |
| [ ] | 14.6 | **NFCe 65:** `infNFeSupl` (QR Code, `urlChave`), `dest` opcional/ausente, `pag` com cartão. |
| [ ] | 14.7 | **Eventos:** `EventoCancelamento` tipado + `IsEffective` + `EventoGenerico` fallback. **Colheita de eventos embutidos no SERPRO** (itens separados no stream). Correlação por chave documentada. |
| [ ] | 14.8 | **`ReadDirectoryAsync`** (Channel + N consumidores) + integração com helpers de streaming do Core. Benchmarks de carga em diretório. |
| [ ] | 14.9 | **`Correlator`** (§6.1): `NFeComEventos`, `Correlate(coleção)` (O(N), lotes) + `BuildCancelamentoIndex` (escalável, O(#cancelamentos)). Benchmark de escala (milhões de notas × índice de cancelamentos) + memória bounded. Predicados `IsAutorizada`/`IsCancelada`. |

## 9. Convenções (vide `ARCHITECTURE.md` §1.3, §13)

- Substantivos do domínio em PT (`NotaFiscal`/`NFe`, `NFCe`, `Item`, `Imposto`, `Emitente`, `Destinatario`, `Protocolo`, `Evento`); verbos/factories/predicados em EN (`ReadAsync`, `Create`, `IsEffective`, `IsAutorizada`).
- Encoding XML = **UTF-8** (a Receita declara UTF-8). Tolerar declaração UTF-8 com bytes ISO-8859-1 (emissores antigos) — definir em teste de regressão.
- Independência de formato: nenhuma referência de projeto a `EfdIcmsIpi` ou outros pacotes SPED textuais. Classes que parecem registros SPED **são** distintas e propositalmente duplicadas.
- Auto-contido: sem dependências externas em runtime, sem banco, sem rede. (A lib **não** consome o WebService SERPRO — só parseia o XML que ele produz.)
- Sealed por padrão; `partial` em classes que o source generator estenderá (Stage 6 futuro).
- Todo I/O `async` com `ConfigureAwait(false)`. File-scoped namespaces.
- Performance-sensitive (parser, `ReadDirectoryAsync`) exige benchmark BenchmarkDotNet.
