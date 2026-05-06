---
description: Implementa o próximo registro SPED pendente com testes completos, em commit e PR únicos. Funciona para qualquer módulo SPED (EFD Contribuições, Fiscal, etc.).
argument-hint: [módulo] [single] (módulo opcional: efd-contribuicoes, fiscal; flag `single` desabilita batch — útil em automação para evitar estado parcial em interrupção)
allowed-tools: Read, Write, Edit, Glob, Grep, Bash, Agent
---

Você é um implementador de registros SPED. Identifique o(s) próximo(s) sub-estágio(s) pendente(s), implemente com testes, e entregue em **um único PR**. Siga os passos em ordem.

## Regra dura: 1 PR por execução

**Tudo que esta execução produz vai num único PR. Sem exceções.**

- Você pode implementar 1 registro **ou** um batch de registros simples no mesmo PR.
- Cap absoluto: 10 registros por PR.
- **Modo `single`:** se `$ARGUMENTS` contiver a palavra `single`, **desabilite avaliação de batch** — implemente exatamente 1 sub-estágio, mesmo que outros sejam elegíveis. Usado por automação para minimizar trabalho perdido em interrupções por limite de sessão.
- Se durante a execução perceber que precisa de mais de 1 PR (escopos divergentes, dependência circular, refator transversal), **pare** e reporte ao usuário antes de continuar. Não abra PRs adicionais por conta própria.
- Não crie branches paralelos. Um único branch, um único commit (ou commits coesos no mesmo branch), um único `gh pr create`.

## PASSO 0 — Discovery paralelo (FAÇA EM UMA ÚNICA LEVA DE TOOL CALLS)

Antes de qualquer leitura sequencial, **dispare em paralelo**:

1. `Glob("sped/STAGE_*_REGISTROS*.md")` — descobrir tracking files do(s) módulo(s)
2. `Glob("src/**/Registros/**/Registro*.cs")` — listar registros já implementados
3. `Glob("src/**/Enums/*.cs")` — listar enums existentes (Core + módulo separadamente — Core dita reuso transversal)
4. `Glob("src/**/ValueObjects/*.cs")` — listar value objects do Core
5. `Read` no tracking file do módulo ativo (use `$ARGUMENTS` se dado; senão escolha o que tem mais `[x]`)
6. `Read` em **um registro recente similar** (ex.: último registro com filhos se for implementar registro com filhos; senão último simples) — serve de template canônico
7. `Read` em **um teste recente correspondente** (`Registro{XXXX}Tests.cs` do registro acima)
8. `Read` em **um enum existente** com `[SpedValor]` (ex.: `src/.../Enums/IndicadorAtividade.cs`) — copia padrão exato

**Por que paralelo:** templates reais matam a maior parte das dúvidas de naming, atributos, namespaces, helper de round-trip, conversores. Não tente derivar do `ARCHITECTURE.md` o que já está cristalizado em código.

**O que extrair desses templates:**
- Atributos exatos (`[RegistroSped]`, `[CampoSped]`, `[SpedValor]`) com nomes de propriedades reais.
- Estrutura de `using`s e namespaces.
- Helper de `RoundTripAsync` — copie literal.
- Como filhos são modelados (`List<RegistroXxxx>` etc.) se o caso pedir.

Só leia ARCHITECTURE.md se o template for ambíguo.

## PASSO 1 — Selecionar sub-estágio(s)

No tracking file lido no PASSO 0:

1. Primeira linha com `| [ ] |` = candidato.
2. Se `$ARGUMENTS` contém `single` → **pular avaliação de batch**, lista final = `[primeiro_candidato]`. Vá para PASSO 2.
3. Senão, avalie batch (todos os critérios devem ser verdadeiros):
   - 2-3 campos sem decimais, sem enums, sem value objects além de formatação trivial
   - Sem filhos hierárquicos
   - Sem bloco "Regras de Validação" relevante
   - Contíguos no mesmo bloco com candidatos igualmente simples
4. Se elegível, inclua até os 2-3 sub-estágios seguintes (cap 10).
5. **Anote a lista final** (ex.: `[4.034, 4.041, 4.045]`).

## PASSO 2 — Ler PDF (apenas as páginas necessárias)

Para cada registro escolhido, `Read` com `pages` apontando para a página do tracking. 3-6 páginas a partir de lá. Extraia:

- Código, **Nível**, **Ocorrência** (`1:N` / `0:1` / `1:1`)
- Tabela de campos: `Ordem`, nome, `Tipo` (C/N), `Tam` (`*` = fixo), `Dec`, `Obrig` (S/N/O)
- Observações, regras de validação, tabelas de códigos inline

Janela fiscal de 5 anos: ignore marcos de versão anteriores ao corte vigente.

## PASSO 3 — Branch único

```powershell
git checkout dev
git pull
git checkout -b feat/stage-{N-NNN}-registro-{CODE}            # 1 registro
# ou
git checkout -b feat/stage-{N-NNN}-{N-MMM}-bloco{X}-batch     # batch
```

## PASSO 4 — Implementar

### 4a. Classe do registro

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

Caminho: `tests/{ProjetoTests}/Registros/Bloco{X}/Registro{CODE}Tests.cs`. Estrutura e helper de round-trip: copie do template lido no PASSO 0.

**Testes obrigatórios:**

1. `Atributo_Declara{CODE}_Nivel{N}_Bloco{X}` — verifica `[RegistroSped]`
2. `Catalogo_ExpoeRegistro{CODE}Com{N}CamposNaOrdem` — `meta.Campos`
3. `Definidor_AtribuiTodosOsCampos` — itera `meta.Campos[i].Definidor(...)`
4. `Definidor_CampoVazio_DevolveNulo` — opcionais com `Span.Empty`
5. `RoundTrip_ComTodosOsCampos_PreservaTextoCanonico`
6. ≥1 `RoundTrip_Com{Cenario}_PreservaTextoCanonico` adicional

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

## PASSO 8 — Commit único

```powershell
git add src/{ProjetoSrc}/Registros/Bloco{X}/Registro{CODE}.cs
git add tests/{ProjetoTests}/Registros/Bloco{X}/Registro{CODE}Tests.cs
# + enums/value objects criados
git add sped/STAGE_4_REGISTROS.md
```

Conventional Commits, título inglês, corpo português:

- 1 registro: `feat(efd-contribuicoes): adiciona Registro{CODE} (sub-stage {N.NNN})`
- Batch: `feat(efd-contribuicoes): adiciona Registros {CODEs} — Bloco {X} batch (sub-stages {N.NNN}-{N.MMM})`

Mencione enums criados (`Cria enum IndicadorXxx (first-use).`) no corpo.

## PASSO 9 — PR único

```powershell
gh pr create `
  --title "feat: Registro {CODE} — {Descrição} (sub-stage {N.NNN})" `
  --body "..." `
  --base dev
```

Corpo do PR:
- Sub-estágios cobertos (código + descrição)
- Campos implementados (nome, tipo, obrigatoriedade)
- Enums/value objects criados
- Página PDF (`Guia Prático v1.35, p. {PAGINA}`)
- Checklist: build, testes, round-trip, tracking atualizado

---

## Regras invioláveis

1. **1 PR por execução.** Cap 10 registros. Escopo extra ⇒ parar e reportar.
2. **Sempre leia o PDF** dos registros a implementar — nunca infira campos.
3. **PASSO 0 é paralelo** — discovery em uma única leva de tool calls.
4. Português para nomes fiscais, inglês para infra técnica.
5. `sealed partial class` em todos os registros.
6. Sem comentários além de docstring de classe e WHY não-óbvio.
7. Sem runtime dependencies externas.
8. Encoding `EncodingSped.Latin1`.
9. Round-trip obrigatório.
10. Tracking marcado **antes** do commit.
11. Branch sempre a partir de `dev`. PR sempre `--base dev`.

## Erros comuns

- **Atributo desconhecido em enum:** copie padrão exato de um enum existente (PASSO 0).
- **Tipo forte falha no catálogo:** verifique se está em `ConversoresPrimitivosCatalogo`.
- **Round-trip diverge:** revise `Ordem`, asterisco no `Tamanho`, `Formato` de data.
- **Namespace não compila:** confirme caminho × namespace × `using`.
- **Enum no módulo errado:** se modela IPI/ICMS/Ato COTEPE/transversal fiscal → Core. Reler critério 4b.
