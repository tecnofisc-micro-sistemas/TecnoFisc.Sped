---
description: Implementa ou atualiza o próximo registro SPED pendente com testes completos, em um único PR. Funciona para qualquer módulo SPED (EFD Contribuições, EFD ICMS-IPI baseline + incrementos V016+).
argument-hint: [módulo] [single] [resume] (módulo opcional: efd-contribuicoes, efd-icms-ipi [v016..v020]; `single` desabilita batch; `resume` força retomada do trabalho parcial na branch atual)
allowed-tools: Read, Write, Edit, Glob, Grep, Bash, Agent
---

Você é um implementador de registros SPED. Identifique o(s) próximo(s) sub-estágio(s) pendente(s), determine o **modo** (CREATE para registro novo, UPDATE para alteração de registro existente), implemente com testes, e entregue em **um único PR**. Commits granulares dentro do PR são preferidos — o merge para `dev` sempre faz **Squash-Merge** (regra dura do repositório). Siga os passos em ordem.

## PASSO -1 — Detecção de retomada (ANTES de qualquer outra ação)

Rode `git branch --show-current` e `git status --porcelain` em paralelo. Em seguida:

1. Se a branch atual casa `feat/stage-*-registro-*` (sub-stage de 2 ou 3 segmentos: `stage-8-001`, `stage-8-016-001`, etc.) ou `feat/stage-*-bloco*-batch`, **OU** `$ARGUMENTS` contém `resume`:
   → **Modo retomada ativo.** Você está continuando trabalho interrompido (limite de sessão anterior). **Não recrie nada, não faça checkout de outra branch, não delete arquivos.**
   - Inspecione o estado real:
     - `git status` — arquivos não rastreados/modificados (implementação parcial)
     - `git log dev..HEAD --oneline` — commits já feitos
     - `git ls-remote --heads origin <branch>` — se já foi pushed
     - `gh pr list --head <branch>` — se PR já existe
   - Identifique o registro alvo pelo nome da branch (`feat/stage-{N-NNN}-registro-{code}` → sub-stage `{N.NNN}`, registro `{CODE}`). Confirme contra o tracking file (deve estar `[ ]`).
   - Pule direto para o ponto correspondente do fluxo:
     - Arquivos parciais existem → continue implementação a partir do que falta (PASSO 4/5).
     - Implementação completa, sem build/test rodado → PASSO 6.
     - Build/test OK, tracking não marcado → PASSO 7.
     - Tracking marcado, sem commit → PASSO 8.
     - Commit feito, sem push → PASSO 9 (push + PR).
     - Push feito, sem PR → só `gh pr create`.
   - **Nunca** descarte arquivos parciais. Eles representam tokens já gastos. Complete o trabalho.

2. Se branch atual = `dev` e working tree limpo:
   → Modo normal. Siga PASSO 0.5 → PASSO 9 em ordem.

## Resolução de módulo (PASSO 0.5 — derivar antes de qualquer Glob)

Resolva o módulo lendo `$ARGUMENTS` (primeira palavra que casa com um conhecido). Se ausente, escolha o módulo com mais sub-stages `[ ]` pendentes entre os tracking files de `sped/STAGE_*.md`. Da resolução, derive:

| Módulo (`$MODULO`) | `$PROJ_SRC` | `$PROJ_TESTS` | `$TRACKING` | Prefixo commit |
| --- | --- | --- | --- | --- |
| `efd-contribuicoes` | `TecnoFisc.Sped.EfdContribuicoes` | `TecnoFisc.Sped.EfdContribuicoes.Tests` | `sped/STAGE_4_REGISTROS.md` | `feat(efd-contribuicoes):` |
| `efd-icms-ipi` (baseline) | `TecnoFisc.Sped.EfdIcmsIpi` | `TecnoFisc.Sped.EfdIcmsIpi.Tests` | `sped/STAGE_8_EFD_ICMS_IPI_V015.md` | `feat(efd-icms-ipi):` |
| `efd-icms-ipi v016`..`v020` | `TecnoFisc.Sped.EfdIcmsIpi` | `TecnoFisc.Sped.EfdIcmsIpi.Tests` | `sped/STAGE_8_INCR_V{NNN}.md` (uppercase) | `feat(efd-icms-ipi):` |

Para EFD ICMS-IPI, se `$ARGUMENTS` indicar versão (ex.: `efd-icms-ipi v016`), use o tracking incremental correspondente. Sub-stages do incremento usam **3 segmentos** (e.g., `8.016.001`); branch correspondente é `feat/stage-8-016-001-registro-{code}`. Baseline V015 mantém 2 segmentos (`8.001`).

Substitua qualquer referência a `STAGE_4_REGISTROS.md`, `efd-contribuicoes`, `EfdContribuicoes`, `stage-4` nos PASSOS abaixo pelos valores derivados aqui.

## Regra dura: 1 PR por execução

**Tudo que esta execução produz vai num único PR. Sem exceções.**

- Você pode implementar 1 registro **ou** um batch de registros simples no mesmo PR.
- Cap absoluto: 10 registros por PR.
- **Modo `single`:** se `$ARGUMENTS` contiver a palavra `single`, **desabilite avaliação de batch** — implemente exatamente 1 sub-estágio, mesmo que outros sejam elegíveis. Usado por automação para minimizar trabalho perdido em interrupções por limite de sessão.
- Se durante a execução perceber que precisa de mais de 1 PR (escopos divergentes, dependência circular, refator transversal), **pare** e reporte ao usuário antes de continuar. Não abra PRs adicionais por conta própria.
- Não crie branches paralelos. Um único branch, um único commit (ou commits coesos no mesmo branch), um único `gh pr create`.

## PASSO 0 — Discovery paralelo (FAÇA EM UMA ÚNICA LEVA DE TOOL CALLS)

Antes de qualquer leitura sequencial, **dispare em paralelo**:

1. `Glob("sped/STAGE_*.md")` — descobrir tracking files do(s) módulo(s) (baseline + incrementos)
2. `Glob("src/**/Registros/**/Registro*.cs")` — listar registros já implementados (também usado para detectar modo UPDATE — se `Registro{CODE}.cs` já existe na pasta do módulo alvo, modo = UPDATE)
3. `Glob("src/**/Enums/*.cs")` — listar enums existentes (Core + módulo separadamente — Core dita reuso transversal)
4. `Glob("src/**/ValueObjects/*.cs")` — listar value objects do Core
5. `Glob("src/{ProjetoSrc}/Versionamento/*.cs")` — confirmar enum `LayoutEfdIcmsIpi` (ou equivalente) e quais versões já foram declaradas
6. `Read` no tracking file do módulo ativo (use `$ARGUMENTS` se dado; senão escolha o que tem mais `[ ]`)
7. `Read` em **um registro recente similar** (ex.: último registro com filhos se for implementar registro com filhos; senão último simples) — serve de template canônico para modo CREATE
8. `Read` em **um teste recente correspondente** (`Registro{XXXX}Tests.cs` do registro acima)
9. `Read` em **um enum existente** com `[SpedValor]` (ex.: `src/.../Enums/IndicadorAtividade.cs`) — copia padrão exato
10. Se modo UPDATE: `Read` na classe existente (`Registro{CODE}.cs`) e nos testes existentes (`Registro{CODE}Tests.cs`) — você vai estender, não recriar

**Por que paralelo:** templates reais matam a maior parte das dúvidas de naming, atributos, namespaces, helper de round-trip, conversores. Não tente derivar do `ARCHITECTURE.md` o que já está cristalizado em código.

**O que extrair desses templates:**
- Atributos exatos (`[RegistroSped]`, `[CampoSped]`, `[SpedValor]`) com nomes de propriedades reais.
- Estrutura de `using`s e namespaces.
- Helper de `RoundTripAsync` — copie literal.
- Como filhos são modelados (`List<RegistroXxxx>` etc.) se o caso pedir.

Só leia ARCHITECTURE.md se o template for ambíguo.

## PASSO 1 — Selecionar sub-estágio(s) e determinar modo

No tracking file lido no PASSO 0:

1. Primeira linha com `| [ ] |` = candidato.
2. Em trackings de incremento (V016+), há uma coluna **Tipo** logo após `Sub-stage`:
   - `NEW` → modo CREATE (criar classe + tests do zero, igual baseline).
   - `UPDATE/Campo` → adicionar property em registro existente com `DesdeVersao`.
   - `UPDATE/Obrig` → alterar obrigatoriedade — validador cross-versão; `[CampoSped]` permanece.
   - `UPDATE/Validação` → criar/editar validador em `Validadores/`; doc-comment.
   - `UPDATE/Subclasse` → ARCHITECTURE §4.7. `Registro{CODE}V{NNN} : Registro{CODE}` (mudança de tipo, tamanho, decimais, formato).
   - `UPDATE/Doc` → apenas doc-comment XML. Sem código novo.
   - `UPDATE/Descontinuado` → marcar `[Descontinuado(EmVersao = V{NNN})]` (atributo first-use se não existir).
   Em baseline V015 não há coluna Tipo — assume CREATE.
3. Se `$ARGUMENTS` contém `single` → **pular avaliação de batch**, lista final = `[primeiro_candidato]`. Vá para PASSO 2.
4. Senão, avalie batch (todos os critérios devem ser verdadeiros):
   - Mesmo tipo (todos NEW simples, ou todos UPDATE/Doc, etc.)
   - 2-3 campos sem decimais, sem enums, sem value objects além de formatação trivial
   - Sem filhos hierárquicos
   - Sem bloco "Regras de Validação" relevante
   - Contíguos no mesmo bloco com candidatos igualmente simples
5. Se elegível, inclua até os 2-3 sub-estágios seguintes (cap 10).
6. **Anote a lista final** (ex.: `[4.034, 4.041, 4.045]` ou `[8.016.001]`).

## PASSO 2 — Ler PDF (apenas as páginas necessárias)

**Modo CREATE:** `Read` com `pages` apontando para a página do registro em Cap. III. 3-6 páginas a partir de lá. Extraia:

- Código, **Nível**, **Ocorrência** (`1:N` / `0:1` / `1:1`)
- Tabela de campos: `Ordem`, nome, `Tipo` (C/N), `Tam` (`*` = fixo), `Dec`, `Obrig` (S/N/O)
- Observações, regras de validação, tabelas de códigos inline

**Modo UPDATE:** ler **dois lugares**:
- Página do registro em Cap. III (mesma fonte do CREATE) — confirma estado atual da tabela de campos. Use a coluna `Fonte` do tracking para localizar.
- Página das "Principais alterações" no fim do guia (p. 358-362 do guia v3.2.2) — descreve o delta exato. Usar o número de item citado no tracking (e.g., "3.0.7 item 11").

Para registros tocados em múltiplas versões (e.g., D700 em V017/V018/V019/V020), confirmar a cadeia de modificações antes de implementar — cada incremento herda do anterior.

Janela fiscal de 5 anos: ignore marcos de versão anteriores ao corte vigente.

## PASSO 3 — Branch único

**Modo retomada (PASSO -1):** pular este passo. Você já está na branch correta.

```powershell
git checkout dev
git pull
git checkout -b feat/stage-{N-NNN}-registro-{CODE}            # 1 registro, baseline (2-seg sub-stage)
# ou
git checkout -b feat/stage-{N-NNN-MMM}-registro-{CODE}        # 1 registro, incremento (3-seg sub-stage)
# ou
git checkout -b feat/stage-{N-NNN}-{N-MMM}-bloco{X}-batch     # batch
```

Exemplos:
- Sub-stage `4.034` (baseline EFD Contribuições) → `feat/stage-4-034-registro-c100`.
- Sub-stage `8.001` (baseline EFD ICMS-IPI V015) → `feat/stage-8-001-registro-0000`.
- Sub-stage `8.016.001` (incremento V016) → `feat/stage-8-016-001-registro-1601`.

## PASSO 4 — Implementar

### 4a. Classe do registro — modo CREATE (registro novo)

Caminho: `src/{ProjetoSrc}/Registros/Bloco{X}/Registro{CODE}.cs`. Copie shape do template lido no PASSO 0.

**Tipos:**
- C → `string?` (default)
- C data DDMMAAAA → `DateOnly?` + `Formato = "ddMMyyyy"`
- C lista de códigos → enum nullable (criar se primeiro uso)
- C CNPJ/CPF/CFOP/NCM/ChaveAcesso → tipo forte do Core
- N sem decimais, valores pequenos → `int?`; grandes (totais) → `long?`
- N com decimais → `decimal?` + `Decimais = {DEC}`

**`[CampoSped]`:**
- `Ordem` começa em 2 (campo 1 é `|REG|`)
- `Tamanho`: número do guia; `0` se variável
- `Decimais`: omitir se zero
- `Obrigatorio = true` apenas quando coluna `Obrig = S`
- `Formato` apenas para datas

**Sealed partial sempre.** Campos `Obrigatorio = true` com tipo de valor → sem `?`, exceto se nullable for tecnicamente necessário.

Para registros NEW em incrementos (V016+), incluir também `IntroduzidoEm`:
```csharp
[RegistroSped(Codigo = "1601", Nivel = 2, Bloco = "1", IntroduzidoEm = (int)LayoutEfdIcmsIpi.V016)]
public sealed partial class Registro1601 : RegistroSped { /* ... */ }
```

### 4a-update. Classe do registro — modo UPDATE (registro existente)

Não recrie. Aplique o delta:

**UPDATE/Campo** — adicionar property no fim da classe (logo antes do fechamento), com `DesdeVersao`:
```csharp
[CampoSped(Ordem = 4, Tamanho = 14, DesdeVersao = (int)LayoutEfdIcmsIpi.V016)]
public string? CodBarra { get; set; }
```
Doc-comment XML descrevendo o campo. **Não** reordenar properties existentes. **Não** mexer em `Ordem` de campos anteriores.

**UPDATE/Obrig** — `[CampoSped]` permanece como estava (baseline). Adicionar validador em `src/{ProjetoSrc}/Validadores/Versionados/V{NNN}/Registro{CODE}V{NNN}Validador.cs` que aplica a regra condicionada a `Registro0000.CodVer >= V{NNN}`.

**UPDATE/Validação** — mesmo padrão que UPDATE/Obrig (validador versionado). Doc-comment XML atualizado.

**UPDATE/Subclasse** — criar `src/{ProjetoSrc}/Registros/Bloco{X}/Versionado/Registro{CODE}V{NNN}.cs`:
```csharp
[RegistroSped(Codigo = "{CODE}", Nivel = N, Bloco = "X", IntroduzidoEm = (int)LayoutEfdIcmsIpi.V{NNN})]
public sealed partial class Registro{CODE}V{NNN} : Registro{CODE}
{
    // override property com novo Ordem, Tamanho, Tipo etc.
}
```
Parser/gerador escolhe a variante baseada em `Registro0000.CodVer`. Confirme integração com `CatalogoBuilder` antes do primeiro PR de subclasse (ARCHITECTURE §4.7).

**UPDATE/Doc** — apenas alterar doc-comment XML da property ou da classe.

**UPDATE/Descontinuado** — criar atributo `[DescontinuadoAttribute(EmVersao = (int)LayoutEfdIcmsIpi.V{NNN})]` em `src/TecnoFisc.Sped.Core/Atributos/` se ainda não existir (first-use). Aplicar à classe do registro ou à property descontinuada. Parser rejeita uso em arquivos `CodVer >= V{NNN}`.

### 4b. Enums (regra first-use + decisão de localização)

Antes de criar, `Grep` no diretório de enums (Core **e** módulo). Se não existir, decida onde criar **antes** de escrever:

**Default → `src/TecnoFisc.Sped.Core/Enums/`** quando qualquer um for verdadeiro:

- Enum modela campo regido por **Ato COTEPE/ICMS** (Tabela 4.1.1 Modelos, 4.1.2 Situação, etc.). EFD ICMS-IPI é regente — outros leiautes referenciam.
- Campo é de **IPI** (`IndApur`, `CstIpi`, `CodEnq`...). IPI é tributo ICMS-IPI domain.
- Campo é de **ICMS** ou **ICMS-ST** (CST ICMS, origem mercadoria, modalidade BC ST...).
- Campo é fiscal **transversal** (movimentação física `IndMov`, indicador frete, indicador pagamento, modelo doc fiscal, situação doc).
- Registro sendo implementado é **replicado literalmente** de outro leiaute (C100, C170, C500, etc. são canônicos do ICMS-IPI). Os enums dos campos desse registro quase sempre são Core.
- Guia explicita "conforme Tabela 4.x.x" ou "ver leiaute ICMS-IPI".

**Módulo `src/{ProjetoSrc}/Enums/`** apenas quando:

- Enum modela campo **exclusivo** do tributo do módulo (PIS/COFINS para EFD Contribuições; ICMS/IPI específico para EFD ICMS-IPI sem reuso).
- Sem citação a tabela Ato COTEPE no guia.
- Não aparece em registros replicados.

**Em dúvida → Core.** Drift bug de duplicar enum Ato COTEPE é pior que enum no Core que poderia estar no módulo.

Padrão **idêntico** ao enum lido no PASSO 0. Sem sentinelas (`Desconhecido`, `Outros`).

## PASSO 5 — Testes

**Modo CREATE:** caminho `tests/{ProjetoTests}/Registros/Bloco{X}/Registro{CODE}Tests.cs`. Estrutura e helper de round-trip: copie do template lido no PASSO 0.

**Testes obrigatórios (CREATE):**

1. `Atributo_Declara{CODE}_Nivel{N}_Bloco{X}` — verifica `[RegistroSped]`
2. `Catalogo_ExpoeRegistro{CODE}Com{N}CamposNaOrdem` — `meta.Campos`
3. `Definidor_AtribuiTodosOsCampos` — itera `meta.Campos[i].Definidor(...)`
4. `Definidor_CampoVazio_DevolveNulo` — opcionais com `Span.Empty`
5. `RoundTrip_ComTodosOsCampos_PreservaTextoCanonico`
6. ≥1 `RoundTrip_Com{Cenario}_PreservaTextoCanonico` adicional

**Modo UPDATE:** **não tocar** nos tests baseline existentes. Adicionar tests específicos da versão:

- UPDATE/Campo: adicionar `[Fact]` cobrindo round-trip com campo novo populado **e** vazio (compat com arquivos baseline). Acrescentar ao arquivo de tests existente.
- UPDATE/Subclasse: criar arquivo independente `tests/{ProjetoTests}/Registros/Bloco{X}/Versionado/Registro{CODE}V{NNN}Tests.cs`. Cobrir o catálogo da subclasse + round-trip do campo alterado.
- UPDATE/Validação ou UPDATE/Obrig: tests do validador específico em `tests/{ProjetoTests}/Validadores/Versionados/V{NNN}/`. Casos: passa quando `CodVer < V{NNN}`, falha/avisa quando `CodVer >= V{NNN}` e regra é violada.
- UPDATE/Doc ou UPDATE/Descontinuado: tests só se houver mudança observável em comportamento (e.g., parser rejeita registro descontinuado em arquivos `V{NNN}+`).

Enums novos: `[Theory] [InlineData]` cobrindo cada valor + null → empty.

Linhas SPED de teste: `|{CODE}|c2|c3|...|cN|\r\n`. Decimais com ponto. Datas `DDMMAAAA`.

## PASSO 6 — Build e testes

```powershell
dotnet build TecnoFisc.Sped.slnx
dotnet test TecnoFisc.Sped.slnx --filter "FullyQualifiedName~Registro{CODE}"
```

Erros bloqueiam progresso. Não pule testes.

## PASSO 7 — Atualizar tracking

No `STAGE_N_REGISTROS.md`, marque cada sub-estágio coberto: `| [ ] |` → `| [x] |`.

## PASSO 8 — Commits granulares no branch (squash acontece no merge)

**Regra dura do repositório:** merges para `dev` são sempre **Squash-Merge**. Isso libera você a usar **commits granulares** no branch — cada commit pode cobrir uma ideia coesa (implementação, tests, tracking) e o squash consolida tudo num único commit no `dev`. **Não** faça rebase/amend antes do merge.

```powershell
git add src/{ProjetoSrc}/Registros/Bloco{X}/Registro{CODE}.cs
git commit -m "..."
git add tests/{ProjetoTests}/Registros/Bloco{X}/Registro{CODE}Tests.cs
git commit -m "..."
# + enums/value objects criados, em commits próprios
git add $TRACKING
git commit -m "..."
```

Padrão de commit (Conventional Commits, título inglês, corpo português):

**Modo CREATE:**
- `{Prefixo}: adiciona Registro{CODE} (sub-stage {N.NNN})` — implementação
- `test(...): cobre Registro{CODE} round-trip` — tests
- `chore(...): marca sub-stage {N.NNN} done` — tracking

**Modo UPDATE:**
- `{Prefixo}: adiciona campo {NomeCampo} ao Registro{CODE} (V{NNN})` — UPDATE/Campo
- `{Prefixo}: subclasse Registro{CODE}V{NNN} (V{NNN})` — UPDATE/Subclasse
- `{Prefixo}: valida Registro{CODE} campo {N} (V{NNN})` — UPDATE/Validação ou UPDATE/Obrig
- `test(...): cobre Registro{CODE} V{NNN} {tipo}` — tests
- `docs(...): atualiza orientação Registro{CODE} (V{NNN})` — UPDATE/Doc isolado
- `chore(...): marca sub-stage {N.NNN.MMM} done` — tracking

**Batch:** `{Prefixo}: adiciona Registros {CODEs} — Bloco {X} batch (sub-stages {N.NNN}-{N.MMM})`.

Mencione enums/atributos novos no corpo do commit que os criou (`Cria enum IndicadorXxx (first-use).`, `Cria atributo DescontinuadoAttribute (first-use).`).

## PASSO 9 — PR único + Squash-Merge

```powershell
gh pr create `
  --title "feat: Registro {CODE} — {Descrição} (sub-stage {N.NNN})" `
  --body "..." `
  --base dev
```

Corpo do PR:
- Sub-estágios cobertos (código + descrição)
- Modo (CREATE / UPDATE/Campo / UPDATE/Subclasse / etc.)
- Campos implementados ou alterados (nome, tipo, obrigatoriedade, `DesdeVersao` quando aplicável)
- Enums/value objects/atributos criados
- Página PDF (`Guia Prático v3.2.2, p. {PAGINA}`) — para UPDATE incluir também a entrada das "Principais alterações" (p. 358-362)
- Checklist: build, testes, round-trip, tracking atualizado

**Merge:** após CI verde, o merge é **Squash-Merge** (`gh pr merge --squash --delete-branch`). É regra dura do repositório (`CLAUDE.md` item 7). **Nunca** use `--merge` ou `--rebase` para integração em `dev`. O script `auto-implement-sped.ps1` já faz isso automaticamente; quando merge manual, sempre `--squash`.

---

## Regras invioláveis

1. **1 PR por execução.** Cap 10 registros. Escopo extra ⇒ parar e reportar.
2. **Squash-Merge no `dev`.** Commits granulares dentro do branch são bem-vindos; o merge consolida em um único commit no `dev`. **Nunca** `--merge` ou `--rebase` para integração. Regra dura — `CLAUDE.md` item 7.
3. **Sempre leia o PDF** dos registros a implementar — nunca infira campos. Modo UPDATE lê **dois lugares** (página do registro + entrada nas "Principais alterações" p. 358-362).
4. **PASSO 0 é paralelo** — discovery em uma única leva de tool calls.
5. **Modo UPDATE não recria.** Não delete arquivos baseline. Não reordene properties. Não toque em tests baseline.
6. Português para nomes fiscais, inglês para infra técnica.
7. `sealed partial class` em todos os registros.
8. Sem comentários além de docstring de classe e WHY não-óbvio.
9. Sem runtime dependencies externas.
10. Encoding `EncodingSped.Latin1`.
11. Round-trip obrigatório.
12. Tracking marcado **antes** do commit final do PR.
13. Branch sempre a partir de `dev`. PR sempre `--base dev`.

## Erros comuns

- **Atributo desconhecido em enum:** copie padrão exato de um enum existente (PASSO 0).
- **Tipo forte falha no catálogo:** verifique se está em `ConversoresPrimitivosCatalogo`.
- **Round-trip diverge:** revise `Ordem`, asterisco no `Tamanho`, `Formato` de data.
- **Namespace não compila:** confirme caminho × namespace × `using`.
- **Enum no módulo errado:** se modela IPI/ICMS/Ato COTEPE/transversal fiscal → Core. Reler critério 4b.
- **Modo UPDATE quebra round-trip baseline:** novos campos opcionais devem aceitar vazio sem mudar o canônico de arquivos pré-versão. Cobrir explicitamente com test "campo vazio preserva texto baseline".
- **Subclasse não é selecionada pelo parser:** confirmar integração com `CatalogoBuilder` — selector deve usar `Registro0000.CodVer`. Se não suporta ainda, parar e reportar (Stage 6 source generator pode bloquear).
- **`LayoutEfdIcmsIpi.V{NNN}` não compila:** criar o membro na enum (first-use). Adicionar ao mesmo PR.
