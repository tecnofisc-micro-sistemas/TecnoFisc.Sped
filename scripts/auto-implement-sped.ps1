#!/usr/bin/env pwsh
#Requires -Version 7.0
<#
.SYNOPSIS
    Automatiza implementação de registros SPED pendentes.

.DESCRIPTION
    Loop por bloco: invoca Claude ou Codex para implementar o próximo registro pendente,
    aguarda CI no GitHub, mergeia o PR, limpa branches, e repete até o bloco
    estar completo ou atingir o limite de PRs.

    Para imediatamente se:
    - CI falhar (requer revisão manual)
    - agente não criar PR (erro de build/teste na implementação)
    - git ou gh CLI retornarem erro

.PARAMETER Module
    Módulo SPED alvo. Valores aceitos: "efd-contribuicoes", "efd-icms-ipi".
    Padrão: "efd-contribuicoes" (retro-compatibilidade com Stage 4).
    Determina $TrackingFile e o argumento propagado ao /implementar-registro.

.PARAMETER Version
    Apenas para -Module efd-icms-ipi. Versão do leiaute incremental (ex.: "v307").
    Vazio = baseline V306 (sped/STAGE_8_EFD_ICMS_IPI_V306.md).
    Preenchido = incremento (sped/STAGE_8_INCR_<VERSION>.md).

.PARAMETER Bloco
    Letra ou dígito do bloco SPED (ex.: "0", "A", "C", "D").
    Vazio = processa todos os blocos em ordem de aparição no tracking file.

.PARAMETER MaxPRs
    Número máximo de PRs a mergear nesta execução. Padrão: 999 (ilimitado na prática).

.PARAMETER DryRun
    Lista registros pendentes sem implementar nada.

.PARAMETER CiTimeoutMinutes
    Minutos de espera pelo CI antes de falhar. Padrão: 25.
    (build-test ubuntu + windows leva ~5-8min normalmente)

.PARAMETER Agent
    Agente usado para implementar o registro. Valores aceitos: "claude", "codex".
    Padrão: "claude" (mantém o comportamento anterior).

.PARAMETER Model
    Alias ou ID do modelo para o agente escolhido. Padrão: Claude usa "sonnet";
    Codex usa o modelo configurado no CLI quando -Model não é informado.

.PARAMETER AllowBatch
    Permite que /implementar-registro agrupe múltiplos sub-estágios simples num único PR.
    Padrão: desligado. Em automação, batch interrompido por limite de sessão deixa
    estado parcial que não pode ser retomado de forma segura — sempre 1 registro/execução.
    Habilitar só em execuções manuais com supervisão.

.EXAMPLE
    .\scripts\auto-implement-sped.ps1 -Bloco 0
    Implementa todos os registros pendentes do Bloco 0 do EFD Contribuições (default).

.EXAMPLE
    .\scripts\auto-implement-sped.ps1 -Module efd-icms-ipi
    Implementa todos os registros pendentes do baseline V306 do EFD ICMS-IPI.

.EXAMPLE
    .\scripts\auto-implement-sped.ps1 -Module efd-icms-ipi -Version v307
    Implementa diffs pendentes do incremento V307 sobre o EFD ICMS-IPI.

.EXAMPLE
    .\scripts\auto-implement-sped.ps1 -Bloco 0 -DryRun
    Lista pendentes do Bloco 0 sem implementar.

.EXAMPLE
    .\scripts\auto-implement-sped.ps1 -Bloco A -MaxPRs 3 -Model opus
    Implementa até 3 registros do Bloco A usando Opus.

.EXAMPLE
    .\scripts\auto-implement-sped.ps1 -Module efd-icms-ipi -Agent codex
    Implementa registros pendentes usando Codex em vez de Claude.
#>
param(
    [ValidateSet('efd-contribuicoes', 'efd-icms-ipi')]
    [string]$Module           = "efd-contribuicoes",
    [string]$Version          = "",
    [string]$Bloco            = "",
    [int]   $MaxPRs           = 999,
    [switch]$DryRun,
    [int]   $CiTimeoutMinutes = 25,
    [ValidateSet('claude', 'codex')]
    [string]$Agent            = "claude",
    [string]$Model            = "",
    [switch]$AllowBatch
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RepoRoot = Split-Path $PSScriptRoot -Parent

function Resolve-TrackingFile {
    param([string]$Module, [string]$Version)
    switch ($Module.ToLower()) {
        'efd-contribuicoes' {
            if ($Version) { throw "Parametro -Version nao se aplica a 'efd-contribuicoes' (layout unico V006)." }
            return Join-Path $RepoRoot "sped/STAGE_4_REGISTROS.md"
        }
        'efd-icms-ipi' {
            if ($Version) {
                $upper = $Version.ToUpper()
                return Join-Path $RepoRoot "sped/STAGE_8_INCR_$upper.md"
            }
            return Join-Path $RepoRoot "sped/STAGE_8_EFD_ICMS_IPI_V306.md"
        }
        default { throw "Modulo desconhecido: $Module" }
    }
}

$TrackingFile = Resolve-TrackingFile -Module $Module -Version $Version

if (-not (Test-Path $TrackingFile)) {
    Write-Host "ERRO: tracking file nao encontrado: $TrackingFile" -ForegroundColor Red
    Write-Host "  Modulo : $Module" -ForegroundColor Yellow
    if ($Version) { Write-Host "  Versao : $Version" -ForegroundColor Yellow }
    exit 1
}

# ─── helpers ────────────────────────────────────────────────────────────────

function Write-Banner {
    param([string]$Msg, [string]$Color = "Cyan")
    Write-Host ""
    Write-Host ("=" * 70) -ForegroundColor $Color
    Write-Host "  $Msg" -ForegroundColor $Color
    Write-Host ("=" * 70) -ForegroundColor $Color
}

function Write-Step {
    param([string]$Msg, [string]$Color = "White")
    Write-Host "  >> $Msg" -ForegroundColor $Color
}

function Assert-Tool {
    param([string]$Name)
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        Write-Host "ERRO: ferramenta '$Name' nao encontrada no PATH." -ForegroundColor Red
        exit 1
    }
}

function Get-PendingRegistros {
    param([string]$TargetBloco = "")

    $lines        = Get-Content $TrackingFile
    $currentBloco = ""
    $result       = [System.Collections.Generic.List[hashtable]]::new()

    foreach ($line in $lines) {
        # Match bloco header, e.g.: "### Bloco 0 — Abertura..."
        if ($line -match '^### Bloco (\S+)') {
            $currentBloco = $Matches[1].TrimEnd('—').Trim()
        }

        # Match pending row: | [ ] | 4.005 | Registro 0110 | Descrição | 71 |
        if ($line -match '^\| \[ \] \| (\d+\.\d+) \| Registro (\w+) \| (.+?) \| (\d+) \|') {
            if ($TargetBloco -eq "" -or $currentBloco -eq $TargetBloco) {
                $result.Add(@{
                    SubStage    = $Matches[1]
                    Code        = $Matches[2]
                    Description = $Matches[3].Trim()
                    Page        = [int]$Matches[4]
                    Bloco       = $currentBloco
                })
            }
        }
    }
    return $result
}

function Get-OpenPRs {
    # Returns array of {number, headRefName, url}
    $json = gh pr list --state open --base dev --json number,headRefName,url --limit 50
    if (-not $json) { return @() }
    return $json | ConvertFrom-Json
}

function Get-ExpectedBranch {
    # Convenção: feat/stage-<sub>-registro-<code>
    # SubStage "4.182" → "stage-4-182"; "8.001" → "stage-8-001"; "8.1.005" → "stage-8-1-005".
    # Code "1100" → "registro-1100".
    param([string]$SubStage, [string]$Code)
    $sub = $SubStage -replace '\.', '-'
    return "feat/stage-$sub-registro-$($Code.ToLower())"
}

function Find-PRByBranch {
    param([string]$BranchName)
    $prs = @(Get-OpenPRs)
    return $prs | Where-Object { $_.headRefName -eq $BranchName } | Select-Object -First 1
}

function Ensure-PRForBranch {
    param([string]$BranchName)

    $existing = Find-PRByBranch -BranchName $BranchName
    if ($existing) { return $existing }

    Write-Step "Publicando branch '$BranchName' para origin..." "Yellow"
    git push -u origin $BranchName
    if ($LASTEXITCODE -ne 0) {
        Write-Step "Falha ao publicar '$BranchName'." "Red"
        exit 1
    }

    $existing = Find-PRByBranch -BranchName $BranchName
    if ($existing) { return $existing }

    Write-Step "Criando PR para '$BranchName' com gh pr create --fill..." "Yellow"
    gh pr create --base dev --head $BranchName --fill
    if ($LASTEXITCODE -ne 0) {
        Write-Step "Falha ao criar PR para '$BranchName'." "Red"
        exit 1
    }

    Start-Sleep -Seconds 2
    return Find-PRByBranch -BranchName $BranchName
}

function Test-RemoteBranchExists {
    param([string]$BranchName)
    $out = git ls-remote --heads origin $BranchName 2>$null
    return [bool]$out
}

function Get-CurrentBranch {
    return (git branch --show-current).Trim()
}

function Test-WorkingTreeClean {
    $status = git status --porcelain 2>$null
    return -not ($status | Where-Object { $_ })
}

function Get-PartialBranchState {
    param([string]$BranchName)

    $currentBranch = Get-CurrentBranch
    $localExists   = [bool](git branch --list $BranchName)
    $remoteExists  = Test-RemoteBranchExists -BranchName $BranchName
    $dirty         = $false
    $hasCommits    = $false

    if ($currentBranch -eq $BranchName) {
        $dirty      = -not (Test-WorkingTreeClean)
        $hasCommits = [bool](git log "dev..$BranchName" --oneline 2>$null)
    }
    elseif ($localExists) {
        $hasCommits = [bool](git log "dev..$BranchName" --oneline 2>$null)
    }
    elseif ($remoteExists) {
        $hasCommits = $true
    }

    return [pscustomobject]@{
        Branch     = $BranchName
        Current    = ($currentBranch -eq $BranchName)
        Local      = $localExists
        Remote     = $remoteExists
        Dirty      = $dirty
        HasCommits = $hasCommits
        Exists     = (($currentBranch -eq $BranchName) -or $localExists -or $remoteExists)
        Partial    = (($currentBranch -eq $BranchName -and $dirty) -or $hasCommits -or $remoteExists)
    }
}

function Find-OrphanFeatureBranches {
    # Branches feat/stage-* (locais ou remotas) sem PR aberto. Captura estados parciais
    # de batch/single interrompidos antes de gh pr create — onde Find-PRByBranch falha
    # porque o nome esperado pelo script difere do que claude criou (ex.: branch de batch).
    $localOut = git for-each-ref --format='%(refname:short)' 'refs/heads/feat/stage-*' 2>$null
    $localBranches = @($localOut | Where-Object { $_ })

    $remoteOut = git ls-remote --heads origin 'feat/stage-*' 2>$null
    $remoteBranches = @($remoteOut | ForEach-Object {
        if ($_ -match 'refs/heads/(feat/stage-.+)$') { $Matches[1] }
    } | Where-Object { $_ })

    $allBranches    = @(($localBranches + $remoteBranches) | Sort-Object -Unique)
    $openPRBranches = @(Get-OpenPRs | ForEach-Object { $_.headRefName })

    $orphans = @()
    foreach ($b in $allBranches) {
        if ($openPRBranches -notcontains $b) {
            $orphans += [pscustomobject]@{
                Name   = $b
                Local  = $localBranches  -contains $b
                Remote = $remoteBranches -contains $b
            }
        }
    }
    return $orphans
}

function Wait-ForSessionReset {
    # Detecta mensagem de limite de sessao/rate limit do agente e dorme ate o reset.
    # Formatos observados (mudam entre versoes do CLI):
    #   (1) "You've hit your limit . resets 4:10am (America/Sao_Paulo)"
    #   (2) "Claude AI usage limit reached|1731234567"   (epoch unix do reset)
    #   (3) Mensagens genericas de rate limit sem hora ("rate_limit_error",
    #       "429", "usage limit", "rate limit", "limit reached")
    # Retorna $true se detectou e aguardou (caller deve repetir iteracao).
    param([string]$AgentOutput)

    if (-not $AgentOutput) { return $false }

    $now      = Get-Date
    $resumeAt = $null
    $matchedPattern = ""

    # (2) Epoch unix: "Claude AI usage limit reached|<seconds>"
    if ($AgentOutput -match "(?i)usage limit reached\s*\|\s*(\d+)") {
        $epoch    = [int64]$Matches[1]
        $reset    = [DateTimeOffset]::FromUnixTimeSeconds($epoch).LocalDateTime
        $resumeAt = $reset.AddMinutes(5)
        $matchedPattern = "epoch ($epoch -> $($reset.ToString('yyyy-MM-dd HH:mm')))"
    }
    # (1) Formato "resets H:MM am/pm"
    elseif ($AgentOutput -match "(?i)resets?\s+(\d{1,2}):(\d{2})\s*(am|pm)") {
        $hour   = [int]$Matches[1]
        $minute = [int]$Matches[2]
        $ampm   = $Matches[3].ToLower()
        if     ($ampm -eq 'pm' -and $hour -lt 12) { $hour += 12 }
        elseif ($ampm -eq 'am' -and $hour -eq 12) { $hour  = 0  }
        $reset = Get-Date -Hour $hour -Minute $minute -Second 0 -Millisecond 0
        if ($reset -le $now) { $reset = $reset.AddDays(1) }
        $resumeAt = $reset.AddMinutes(5)
        $matchedPattern = "HH:MM ($($reset.ToString('HH:mm')))"
    }
    # (3) Generico — sem hora extraivel. Fallback: dormir 60min e tentar de novo.
    elseif ($AgentOutput -match "(?i)(usage limit|rate.?limit|limit reached|rate_limit_error|\b429\b|too many requests|quota exceeded)") {
        $resumeAt = $now.AddMinutes(60)
        $matchedPattern = "generico ('$($Matches[1])') -> retry fixo +60min"
    }
    else {
        return $false
    }

    $wait = $resumeAt - $now
    if ($wait.TotalSeconds -lt 60) { $wait = [TimeSpan]::FromMinutes(5) ; $resumeAt = $now.Add($wait) }

    Write-Step "Limite de sessao detectado [$matchedPattern]. Retomando $($resumeAt.ToString('HH:mm')) (~$([int]$wait.TotalMinutes)min)..." "Yellow"
    Start-Sleep -Seconds ([int]$wait.TotalSeconds)
    Write-Step "Sessao reiniciada. Retomando execucao." "Green"
    return $true
}

function Invoke-ImplementationAgent {
    param(
        [string]$Agent,
        [string]$SlashCmd,
        [string[]]$SlashArgs,
        [string]$Model,
        [string]$RepoRoot
    )

    $agentLog = New-TemporaryFile
    try {
        switch ($Agent) {
            'claude' {
                $effectiveModel = if ($Model) { $Model } else { "sonnet" }
                Write-Step "Invocando: claude -p '$SlashCmd' --model $effectiveModel"
                Write-Host ""

                "" | claude --print $SlashCmd `
                    --dangerously-skip-permissions `
                    --model $effectiveModel `
                    --no-session-persistence 2>&1 | ForEach-Object {
                        $_ | Out-File -FilePath $agentLog.FullName -Append
                        Write-Host $_
                    }
            }
            'codex' {
                $argsText = $SlashArgs -join ' '
                $prompt = @"
Read `.claude/commands/implementar-registro.md` and execute those instructions as the active command.

Command arguments: $argsText
Equivalent Claude slash command: $SlashCmd

Follow the repository instructions and complete the implementation, tests, commit, push, and PR exactly as the command file requires.
If this is a resume run, treat the existing branch as authoritative: inspect the current state, keep completed work, finish any missing steps, push, and create the PR if it does not exist.
"@
                $codexArgs = @(
                    'exec',
                    '--cd', $RepoRoot,
                    '--dangerously-bypass-approvals-and-sandbox',
                    '--ephemeral'
                )
                if ($Model) {
                    $codexArgs += @('--model', $Model)
                }
                $codexArgs += $prompt

                $modelMsg = if ($Model) { " --model $Model" } else { "" }
                Write-Step "Invocando: codex exec$modelMsg <prompt baseado em .claude/commands/implementar-registro.md>"
                Write-Host ""

                & codex @codexArgs 2>&1 | ForEach-Object {
                    $_ | Out-File -FilePath $agentLog.FullName -Append
                    Write-Host $_
                }
            }
            default {
                throw "Agente desconhecido: $Agent"
            }
        }

        return [pscustomobject]@{
            ExitCode = $LASTEXITCODE
            Output   = Get-Content $agentLog.FullName -Raw
        }
    } finally {
        Remove-Item $agentLog.FullName -ErrorAction SilentlyContinue
    }
}

function Wait-ForCI {
    # Polling puro via `gh pr checks --json`. Evita `--watch --fail-fast` e
    # Process.Start, que no Windows/PowerShell estavam retornando exit code
    # incorreto (falso "CI falhou" mesmo com checks verdes). Tambem trata
    # bucket "skipping" (ex.: job Pack so roda em release) como nao-falha.
    #
    # Buckets gh: pass | fail | pending | skipping | cancel
    param([int]$PrNumber, [int]$TimeoutMinutes, [int]$IntervalSeconds = 15)

    $deadline         = (Get-Date).AddMinutes($TimeoutMinutes)
    $registerDeadline = (Get-Date).AddMinutes(2)
    $registered       = $false

    Write-Step "Polling CI no PR #$PrNumber a cada ${IntervalSeconds}s (timeout: ${TimeoutMinutes}min)..."

    while ((Get-Date) -lt $deadline) {
        $json = gh pr checks $PrNumber --json bucket,state,name 2>$null

        if (-not $json -or $json.Trim() -eq "" -or $json.Trim() -eq "[]") {
            if (-not $registered -and (Get-Date) -ge $registerDeadline) {
                Write-Step "Nenhum check registrado apos 2min. Workflow nao disparou?" "Red"
                return "failed"
            }
            Write-Step "Aguardando checks registrarem..."
            Start-Sleep -Seconds 5
            continue
        }

        $checks = @($json | ConvertFrom-Json)
        if ($checks.Count -eq 0) {
            Start-Sleep -Seconds 5
            continue
        }
        $registered = $true

        $failed  = @($checks | Where-Object { $_.bucket -in 'fail','cancel' })
        $pending = @($checks | Where-Object { $_.bucket -eq 'pending' })

        if ($failed.Count -gt 0) {
            $names = ($failed | ForEach-Object { "$($_.name) ($($_.bucket))" }) -join ', '
            Write-Step "CI FALHOU: $names" "Red"
            return "failed"
        }

        if ($pending.Count -eq 0) {
            $passCount = @($checks | Where-Object { $_.bucket -eq 'pass' }).Count
            $skipCount = @($checks | Where-Object { $_.bucket -eq 'skipping' }).Count
            $msg       = "$passCount pass"
            if ($skipCount -gt 0) { $msg += ", $skipCount skip" }
            Write-Step "CI APROVADO ($msg)" "Green"
            return "success"
        }

        $passCount = @($checks | Where-Object { $_.bucket -eq 'pass' }).Count
        Write-Step "CI rodando: $passCount/$($checks.Count) ok, $($pending.Count) pendente(s)..."
        Start-Sleep -Seconds $IntervalSeconds
    }

    Write-Step "TIMEOUT apos ${TimeoutMinutes}min" "Yellow"
    return "timeout"
}

# ─── validacao ───────────────────────────────────────────────────────────────

Assert-Tool "gh"
Assert-Tool $Agent
Assert-Tool "git"

Push-Location $RepoRoot

# ─── main ────────────────────────────────────────────────────────────────────

try {
    # ── dry run ────────────────────────────────────────────────────────────────
    if ($DryRun) {
        $pending = @(Get-PendingRegistros -TargetBloco $Bloco)
        $blocoStr = if ($Bloco) { " no Bloco $Bloco" } else { " (todos os blocos)" }

        if ($pending.Count -eq 0) {
            Write-Host "Nenhum registro pendente${blocoStr}." -ForegroundColor Green
        } else {
            Write-Host "Registros pendentes${blocoStr}: $($pending.Count) total" -ForegroundColor Cyan
            foreach ($r in $pending) {
                Write-Host ("  {0,-8}  Registro {1,-6}  {2,-60}  p.{3}" -f $r.SubStage, $r.Code, $r.Description, $r.Page)
            }
        }
        exit 0
    }

    # ── verificacao inicial ────────────────────────────────────────────────────
    $initialPending = @(Get-PendingRegistros -TargetBloco $Bloco)
    $blocoStr       = if ($Bloco) { "Bloco $Bloco" } else { "todos os blocos" }

    if ($initialPending.Count -eq 0) {
        Write-Host "Nenhum registro pendente ($blocoStr). Ja concluido!" -ForegroundColor Green
        exit 0
    }

    Write-Banner "Auto-implementacao SPED — $blocoStr — $($initialPending.Count) registro(s) pendente(s)" "Cyan"
    Write-Host "  Modulo    : $Module$(if ($Version) { " ($Version)" })"
    Write-Host "  Tracking  : $TrackingFile"
    Write-Host "  Agente    : $Agent"
    Write-Host "  Modelo    : $(if ($Model) { $Model } elseif ($Agent -eq 'claude') { 'sonnet (default)' } else { 'configurado no Codex CLI' })"
    Write-Host "  MaxPRs    : $MaxPRs"
    Write-Host "  CI timeout: ${CiTimeoutMinutes}min"
    Write-Host "  Modo      : $(if ($AllowBatch) { 'batch (cap 10/PR)' } else { 'single (1 registro/PR — recomendado p/ automacao)' })"
    Write-Host ""

    $mergedCount = 0
    $resumeAttemptsByBranch = @{}

    # ── loop principal ─────────────────────────────────────────────────────────
    while ($mergedCount -lt $MaxPRs) {

        # Re-read pendentes a cada iteracao (tracking file muda apos cada merge)
        $pending = @(Get-PendingRegistros -TargetBloco $Bloco)

        if ($pending.Count -eq 0) {
            Write-Banner "Todos os registros concluidos!" "Green"
            break
        }

        $next = $pending[0]
        Write-Banner "$($next.SubStage)  Registro $($next.Code) — $($next.Description)" "Cyan"

        # Calculado cedo: branch alvo do proximo pendente. Qualquer branch local
        # ou remota com esse nome (sem PR) eh trabalho parcial deste mesmo registro
        # que deve ser retomado, NUNCA descartado.
        $expectedBranch = Get-ExpectedBranch -SubStage $next.SubStage -Code $next.Code

        # ── deteccao de estado: retomada vs. comeco limpo ──────────────────────
        # $resumeMode = agente foi interrompido mid-implementacao e deve continuar
        # de onde parou. Estados que disparam retomada:
        #   - branch corrente == expectedBranch (interrompido antes do commit)
        #   - branch local expectedBranch existe (interrompido antes do checkout final)
        #   - branch remota expectedBranch existe sem PR (interrompido antes/durante gh pr create)
        # Em qualquer um, NAO descartar arquivos/commits, NAO checkout dev.
        $resumeMode      = $false
        $currentBranch   = Get-CurrentBranch
        $existingPR      = Find-PRByBranch -BranchName $expectedBranch
        $localExpected   = [bool](git branch --list $expectedBranch)
        $remoteExpected  = Test-RemoteBranchExists -BranchName $expectedBranch
        $recoveredPR     = $null

        if ($existingPR) {
            # PR ja existe — pular invocacao do agente, mergear se CI OK.
            Write-Step "PR existente detectado para '$expectedBranch': #$($existingPR.number). Pulando invocacao do agente." "Cyan"
            $newPR = $existingPR

            # Garantir que estamos em dev (ou em branch limpa qualquer) p/ nao confundir merge.
            if ($currentBranch -ne 'dev') {
                if (-not (Test-WorkingTreeClean)) {
                    Write-Step "Working tree em '$currentBranch' tem mudancas nao-commitadas — resolver antes de mergear PR #$($existingPR.number)." "Red"
                    exit 1
                }
                git checkout dev --quiet
            }
        }
        elseif ($currentBranch -eq $expectedBranch -or $localExpected -or $remoteExpected) {
            # Retomada: preservar trabalho parcial.
            $resumeMode = $true

            $diverged = $false
            $dirty    = $false

            if ($currentBranch -ne $expectedBranch) {
                # Estamos em outra branch (provavelmente dev). Working tree precisa estar limpo
                # antes de checkout p/ nao perder/sobrescrever mudancas.
                if (-not (Test-WorkingTreeClean)) {
                    Write-Step "Working tree em '$currentBranch' dirty — nao posso fazer checkout '$expectedBranch' sem perda. Resolver manualmente." "Red"
                    exit 1
                }

                if ($remoteExpected -and -not $localExpected) {
                    Write-Step "Branch remota '$expectedBranch' existe sem PR. Buscando p/ retomar." "Yellow"
                    git fetch origin "${expectedBranch}:${expectedBranch}" --quiet
                }
                Write-Step "Fazendo checkout '$expectedBranch' p/ retomar execucao anterior." "Yellow"
                git checkout $expectedBranch --quiet
                $currentBranch = $expectedBranch
            }

            $diverged = [bool](git log "dev..$expectedBranch" --oneline 2>$null)
            $dirty    = -not (Test-WorkingTreeClean)
            $pushed   = $remoteExpected

            Write-Step "Modo retomada: branch='$expectedBranch' | commits=$diverged | dirty=$dirty | pushed=$pushed" "Yellow"
        }
        else {
            # Estado limpo esperado. Branch atual != dev sem PR == trabalho de outro registro.
            if ($currentBranch -ne 'dev') {
                $prOnCurrent = Find-PRByBranch -BranchName $currentBranch
                if ($prOnCurrent) {
                    Write-Step "Branch atual '$currentBranch' tem PR #$($prOnCurrent.number) — voltando para dev." "Cyan"
                    git checkout dev --quiet
                }
                else {
                    $diverged = git log "dev..$currentBranch" --oneline 2>$null
                    $dirty    = -not (Test-WorkingTreeClean)
                    if ($currentBranch -like 'feat/stage-*' -and [bool]$diverged -and -not $dirty) {
                        Write-Step "Branch '$currentBranch' tem commits, esta limpa e nao tem PR. Criando PR antes de seguir para '$expectedBranch'." "Yellow"
                        $recoveredPR = Ensure-PRForBranch -BranchName $currentBranch
                        if (-not $recoveredPR) {
                            Write-Step "PR criado para '$currentBranch', mas nao consegui localiza-lo em seguida." "Red"
                            exit 1
                        }
                    }
                    else {
                        Write-Step "Branch '$currentBranch' nao corresponde ao proximo pendente ($expectedBranch) e nao tem PR. Commits unicos: $([bool]$diverged); dirty: $dirty." "Red"
                        Write-Step "Resolver manualmente — pode ser trabalho de outro registro:" "Yellow"
                        Write-Step "  git status                                # inspecionar mudancas" "Yellow"
                        Write-Step "  git checkout dev && git branch -D $currentBranch  # descartar se irrelevante" "Yellow"
                        Write-Step "  ou 'gh pr create --base dev --head $currentBranch' se trabalho deve virar PR" "Yellow"
                        exit 1
                    }
                }
            }

            if ($recoveredPR) {
                $newPR = $recoveredPR
            }
            elseif (-not (Test-WorkingTreeClean)) {
                Write-Step "Working tree na branch dev tem mudancas nao-commitadas." "Red"
                Write-Step "Inspecionar 'git status' e decidir 'git stash' / 'git checkout .' antes de prosseguir." "Yellow"
                exit 1
            }

            # ── garantir dev atualizado ────────────────────────────────────────
            Write-Step "Sincronizando branch dev..."
            git checkout dev --quiet
            git pull --quiet

            # ── varrer branches feat/stage-* orfas de outros registros ─────────
            # Apenas orfas que NAO sao expectedBranch (essa virou resumeMode acima).
            # Pega o caso onde batch anterior pushou branch sem PR.
            $orphans = @(Find-OrphanFeatureBranches | Where-Object { $_.Name -ne $expectedBranch })
            if ($orphans.Count -gt 0) {
                Write-Step "Branches feat/stage-* orfas detectadas (sem PR aberto, registro diferente do proximo pendente):" "Red"
                foreach ($o in $orphans) {
                    $loc = if ($o.Local) { 'local' }  else { '' }
                    $rem = if ($o.Remote) { 'remote' } else { '' }
                    $tag = ($loc, $rem | Where-Object { $_ }) -join '+'
                    Write-Step "  - $($o.Name) [$tag]" "Yellow"
                }
                Write-Step "Cada uma pode conter trabalho parcial. Resolver:" "Yellow"
                Write-Step "  - Se branch tem implementacao completa: 'gh pr create --base dev --head <branch>'" "Yellow"
                Write-Step "  - Se trabalho parcial descartavel: 'git push origin --delete <branch>' e/ou 'git branch -D <branch>'" "Yellow"
                exit 1
            }
        }

        $newPR = if ($existingPR) { $existingPR } elseif ($recoveredPR) { $recoveredPR } else { $null }

        if (-not $newPR) {
            # ── snapshot de PRs abertos antes (fallback p/ deteccao de novo PR) ─
            $beforePRNumbers = @(Get-OpenPRs | ForEach-Object { $_.number })
            Write-Step "PRs abertos antes: $(if ($beforePRNumbers.Count -eq 0) { 'nenhum' } else { $beforePRNumbers -join ', ' })"

            # ── invocar agente ─────────────────────────────────────────────────
            # Default `single` (sem batch). `-AllowBatch` libera batch.
            # `resume` adicionado quando branch alvo ja existe com trabalho parcial —
            # comando detecta estado e completa em vez de recriar do zero.
            $moduleArg = $Module
            if ($Version) { $moduleArg = "$Module $Version" }
            $slashArgs = @($moduleArg)
            if (-not $AllowBatch) { $slashArgs += 'single' }
            if ($resumeMode)      { $slashArgs += 'resume' }
            $slashCmd = "/implementar-registro " + ($slashArgs -join ' ')
            $agentResult = Invoke-ImplementationAgent -Agent $Agent -SlashCmd $slashCmd -SlashArgs $slashArgs -Model $Model -RepoRoot $RepoRoot
            $agentExit   = $agentResult.ExitCode
            $agentOutput = $agentResult.Output
            Write-Host ""

            # Limite de sessao atingido: aguardar reset e repetir mesmo registro.
            # Proxima iteracao re-detecta PR/branch criado parcialmente (idempotente).
            if (Wait-ForSessionReset -AgentOutput $agentOutput) {
                continue
            }

            # Mesmo com exit code != 0, o agente pode ter deixado trabalho valido:
            # branch criada, commits locais, push feito ou working tree dirty antes do PR.
            # Nesses casos a execucao precisa ser idempotente: repetir a mesma iteracao
            # em modo resume em vez de abandonar o batch.
            if ($agentExit -ne 0) {
                Write-Step "$Agent terminou com erro (exit code $agentExit). Verificando se ha estado parcial retomavel..." "Yellow"

                Start-Sleep -Seconds 2
                $newPR = Find-PRByBranch -BranchName $expectedBranch
                if (-not $newPR) {
                    $afterPRs = @(Get-OpenPRs)
                    $newPR    = $afterPRs | Where-Object { $beforePRNumbers -notcontains $_.number } | Select-Object -First 1
                }

                if (-not $newPR) {
                    $partialState = Get-PartialBranchState -BranchName $expectedBranch
                    if ($partialState.Exists) {
                        if (-not $resumeAttemptsByBranch.ContainsKey($expectedBranch)) {
                            $resumeAttemptsByBranch[$expectedBranch] = 0
                        }
                        $resumeAttemptsByBranch[$expectedBranch]++

                        Write-Step "Estado parcial detectado para '$expectedBranch': current=$($partialState.Current) local=$($partialState.Local) remote=$($partialState.Remote) commits=$($partialState.HasCommits) dirty=$($partialState.Dirty)." "Yellow"
                        if ($resumeAttemptsByBranch[$expectedBranch] -gt 3) {
                            Write-Step "Limite de retomadas automaticas atingido para '$expectedBranch' apos falhas repetidas do $Agent." "Red"
                            $tail = ($agentOutput -split "`n" | Select-Object -Last 20) -join "`n"
                            Write-Step "Ultimas linhas do output:" "Yellow"
                            Write-Host $tail -ForegroundColor DarkGray
                            exit 1
                        }
                        Write-Step "Retomando a mesma iteracao; a proxima chamada usara 'resume' e deve criar o PR ausente." "Yellow"
                        continue
                    }
                }
            }

            if ($agentExit -ne 0 -and -not $newPR) {
                Write-Step "$Agent terminou com erro (exit code $agentExit). Parando." "Red"
                $tail = ($agentOutput -split "`n" | Select-Object -Last 20) -join "`n"
                Write-Step "Ultimas linhas do output (p/ diagnose; se for rate limit nao reconhecido, ampliar regex em Wait-ForSessionReset):" "Yellow"
                Write-Host $tail -ForegroundColor DarkGray
                exit 1
            }

            # ── encontrar PR criado ────────────────────────────────────────────
            Write-Step "Verificando PR criado..."
            Start-Sleep -Seconds 2

            # Primario: lookup por branch esperada (idempotente).
            $newPR = Find-PRByBranch -BranchName $expectedBranch

            # Fallback: diff contra snapshot anterior.
            if (-not $newPR) {
                $afterPRs = @(Get-OpenPRs)
                $newPR    = $afterPRs | Where-Object { $beforePRNumbers -notcontains $_.number } | Select-Object -First 1
            }

            if (-not $newPR) {
                Write-Step "Nenhum PR encontrado para '$expectedBranch'. $Agent nao criou PR (possivel falha de build/teste)." "Red"
                Write-Step "Verifique o output acima e corrija manualmente antes de reiniciar." "Yellow"
                exit 1
            }
        }

        $prNumber   = [int]$newPR.number
        $branchName = $newPR.headRefName
        $prUrl      = $newPR.url

        Write-Step "Novo PR: #$prNumber — branch: $branchName"
        if ($prUrl) { Write-Step "URL: $prUrl" }

        # ── aguardar CI ────────────────────────────────────────────────────────
        $ciResult = Wait-ForCI -PrNumber $prNumber -TimeoutMinutes $CiTimeoutMinutes

        if ($ciResult -ne "success") {
            Write-Host ""
            Write-Step "CI nao passou ($ciResult). Parando para revisao manual." "Red"
            Write-Step "PR para inspecionar: $prUrl" "Yellow"
            Write-Step "Apos corrigir, reinicie o script (ele vai retomar do proximo pendente)." "Yellow"
            exit 1
        }

        # ── mergear PR ─────────────────────────────────────────────────────────
        Write-Step "Mergeando PR #$prNumber..."
        gh pr merge $prNumber --merge --delete-branch

        # ── sincronizar dev e limpar branch local ──────────────────────────────
        Write-Step "Sincronizando dev apos merge..."
        git checkout dev --quiet
        git pull --quiet

        $localBranch = git branch --list $branchName
        if ($localBranch) {
            git branch -d $branchName 2>$null
            Write-Step "Branch local deletada: $branchName"
        }

        $mergedCount++
        Write-Step "PR #$prNumber mergeado com sucesso. Total nesta sessao: $mergedCount" "Green"
    }

    # ── sumario final ──────────────────────────────────────────────────────────
    Write-Host ""
    Write-Host ("=" * 70) -ForegroundColor Green
    Write-Host "  Automacao finalizada. $mergedCount PR(s) mergeados." -ForegroundColor Green

    $remaining = @(Get-PendingRegistros -TargetBloco $Bloco).Count
    if ($remaining -gt 0) {
        Write-Host "  $remaining registro(s) ainda pendentes ($blocoStr)." -ForegroundColor Yellow
    } else {
        Write-Host "  Bloco(s) completamente implementados!" -ForegroundColor Green
    }
    Write-Host ("=" * 70) -ForegroundColor Green
}
finally {
    Pop-Location
}
