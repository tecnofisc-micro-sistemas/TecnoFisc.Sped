#!/usr/bin/env pwsh
#Requires -Version 7.0
<#
.SYNOPSIS
    Automatiza implementação de registros SPED pendentes.

.DESCRIPTION
    Loop por bloco: invoca Claude para implementar o próximo registro pendente,
    aguarda CI no GitHub, mergeia o PR, limpa branches, e repete até o bloco
    estar completo ou atingir o limite de PRs.

    Para imediatamente se:
    - CI falhar (requer revisão manual)
    - Claude não criar PR (erro de build/teste na implementação)
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

.PARAMETER Model
    Alias ou ID do modelo Claude. Padrão: "sonnet" (claude-sonnet-4-6).
    Use "opus" para maior capacidade em registros complexos.

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
#>
param(
    [ValidateSet('efd-contribuicoes', 'efd-icms-ipi')]
    [string]$Module           = "efd-contribuicoes",
    [string]$Version          = "",
    [string]$Bloco            = "",
    [int]   $MaxPRs           = 999,
    [switch]$DryRun,
    [int]   $CiTimeoutMinutes = 25,
    [string]$Model            = "sonnet",
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
    # Detecta mensagem de limite de sessao do Claude e dorme ate o reset.
    # Padrao observado: "You've hit your limit · resets 4:10am (America/Sao_Paulo)"
    # Retorna $true se detectou e aguardou (caller deve repetir iteracao).
    param([string]$ClaudeOutput)

    if (-not $ClaudeOutput) { return $false }
    if ($ClaudeOutput -notmatch "(?i)hit your limit.*?resets\s+(\d{1,2}):(\d{2})\s*(am|pm)") {
        return $false
    }

    $hour   = [int]$Matches[1]
    $minute = [int]$Matches[2]
    $ampm   = $Matches[3].ToLower()

    if     ($ampm -eq 'pm' -and $hour -lt 12) { $hour += 12 }
    elseif ($ampm -eq 'am' -and $hour -eq 12) { $hour  = 0  }

    $now   = Get-Date
    $reset = Get-Date -Hour $hour -Minute $minute -Second 0 -Millisecond 0
    if ($reset -le $now) { $reset = $reset.AddDays(1) }

    # Buffer 5min apos reset para garantir propagacao no backend
    $resumeAt = $reset.AddMinutes(5)
    $wait     = $resumeAt - $now

    Write-Step "Limite de sessao atingido. Reset em $($reset.ToString('HH:mm')). Retomando $($resumeAt.ToString('HH:mm')) (~$([int]$wait.TotalMinutes)min)..." "Yellow"
    Start-Sleep -Seconds ([int]$wait.TotalSeconds)
    Write-Step "Sessao reiniciada. Retomando execucao." "Green"
    return $true
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
Assert-Tool "claude"
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
    Write-Host "  Modelo    : $Model"
    Write-Host "  MaxPRs    : $MaxPRs"
    Write-Host "  CI timeout: ${CiTimeoutMinutes}min"
    Write-Host "  Modo      : $(if ($AllowBatch) { 'batch (cap 10/PR)' } else { 'single (1 registro/PR — recomendado p/ automacao)' })"
    Write-Host ""

    $mergedCount = 0

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

        # ── verificacao de estado pre-iteracao ─────────────────────────────────
        # Cobre casos onde execucao anterior foi interrompida (limite de sessao,
        # crash, Ctrl-C) e deixou: branch checkada != dev, working tree dirty,
        # ou branches feat/stage-* orfas (sem PR aberto correspondente).
        $currentBranch = Get-CurrentBranch
        if ($currentBranch -ne 'dev') {
            Write-Step "Branch atual '$currentBranch' (esperado 'dev') — possivel execucao anterior interrompida." "Yellow"
            $prOnCurrent = Find-PRByBranch -BranchName $currentBranch
            if ($prOnCurrent) {
                Write-Step "Branch atual tem PR #$($prOnCurrent.number) — voltando para dev (PR sera reutilizado se for do registro atual)." "Cyan"
                git checkout dev --quiet
            }
            else {
                $diverged = git log "dev..$currentBranch" --oneline 2>$null
                $dirty    = -not (Test-WorkingTreeClean)
                Write-Step "Branch '$currentBranch' sem PR. Commits unicos: $([bool]$diverged); working tree dirty: $dirty." "Red"
                Write-Step "Resolver manualmente para evitar perda de trabalho:" "Yellow"
                Write-Step "  git status                                # inspecionar mudancas" "Yellow"
                Write-Step "  git checkout dev && git branch -D $currentBranch  # descartar branch local" "Yellow"
                Write-Step "  ou 'gh pr create --base dev --head $currentBranch' se trabalho deve virar PR" "Yellow"
                exit 1
            }
        }

        if (-not (Test-WorkingTreeClean)) {
            Write-Step "Working tree na branch dev tem mudancas nao-commitadas — execucao anterior interrompida." "Red"
            Write-Step "Inspecionar 'git status' e decidir 'git stash' / 'git checkout .' antes de prosseguir." "Yellow"
            exit 1
        }

        # ── garantir dev atualizado ────────────────────────────────────────────
        Write-Step "Sincronizando branch dev..."
        git checkout dev --quiet
        git pull --quiet

        # ── varrer branches feat/stage-* orfas (sem PR aberto) ─────────────────
        # Isto pega o caso onde claude pushou branch (de single OU batch) mas
        # o limite cortou antes de `gh pr create`. Find-PRByBranch nao detecta
        # branches de batch porque seu nome difere de Get-ExpectedBranch.
        $orphans = @(Find-OrphanFeatureBranches)
        if ($orphans.Count -gt 0) {
            Write-Step "Branches feat/stage-* orfas detectadas (sem PR aberto):" "Red"
            foreach ($o in $orphans) {
                $loc = if ($o.Local) { 'local' }  else { '' }
                $rem = if ($o.Remote) { 'remote' } else { '' }
                $tag = ($loc, $rem | Where-Object { $_ }) -join '+'
                Write-Step "  - $($o.Name) [$tag]" "Yellow"
            }
            Write-Step "Cada uma pode conter trabalho parcial de execucao anterior. Resolver:" "Yellow"
            Write-Step "  - Se branch tem implementacao completa: 'gh pr create --base dev --head <branch>'" "Yellow"
            Write-Step "  - Se trabalho parcial descartavel: 'git push origin --delete <branch>' (se remote) e/ou 'git branch -D <branch>' (se local)" "Yellow"
            exit 1
        }

        # ── idempotencia: reutilizar PR/branch ja existente para este registro ─
        # Recuperacao apos limite de sessao mid-execucao do Claude. Estados possiveis:
        #   (a) PR aberto p/ branch esperada    → reutilizar (skip claude).
        #   (b) Branch remota sem PR            → estado ambiguo (push ok, gh pr create falhou).
        #   (c) Branch local sem PR             → trabalho parcial nao pushed.
        #   (d) Nada                            → invocar claude normalmente.
        $expectedBranch = Get-ExpectedBranch -SubStage $next.SubStage -Code $next.Code
        $existingPR     = Find-PRByBranch    -BranchName $expectedBranch
        $newPR          = $null

        if ($existingPR) {
            Write-Step "PR existente detectado para '$expectedBranch': #$($existingPR.number). Pulando invocacao do Claude." "Cyan"
            $newPR = $existingPR
        }
        else {
            # (b) branch remota orfa → manual: pode ter commits unicos ou faltar gh pr create.
            if (Test-RemoteBranchExists -BranchName $expectedBranch) {
                Write-Step "Branch remota '$expectedBranch' existe sem PR — estado ambiguo." "Red"
                Write-Step "Resolver manualmente: 'gh pr create --base dev --head $expectedBranch' ou 'git push origin --delete $expectedBranch'." "Yellow"
                exit 1
            }

            # (c) branch local orfa → trabalho parcial. Se diverge de dev, parar; senao apagar.
            $localExists = git branch --list $expectedBranch
            if ($localExists) {
                $diverged = git log "dev..$expectedBranch" --oneline 2>$null
                if ($diverged) {
                    Write-Step "Branch local '$expectedBranch' tem commits unicos sem push/PR — estado ambiguo." "Red"
                    Write-Step "Inspecionar com 'git log dev..$expectedBranch' e resolver manualmente." "Yellow"
                    exit 1
                }
                Write-Step "Branch local '$expectedBranch' sem commits unicos. Removendo p/ retomada limpa." "Yellow"
                git branch -D $expectedBranch 2>$null | Out-Null
            }

            # ── snapshot de PRs abertos antes (fallback p/ deteccao de novo PR) ─
            $beforePRNumbers = @(Get-OpenPRs | ForEach-Object { $_.number })
            Write-Step "PRs abertos antes: $(if ($beforePRNumbers.Count -eq 0) { 'nenhum' } else { $beforePRNumbers -join ', ' })"

            # ── invocar Claude ─────────────────────────────────────────────────
            # Default `single` (sem batch) para limitar exposicao a estado parcial em
            # caso de interrupcao por limite de sessao. `-AllowBatch` libera batch.
            # Modulo (e versao se aplicavel) propagado como primeiro argumento.
            $moduleArg = $Module
            if ($Version) { $moduleArg = "$Module $Version" }
            $slashCmd = if ($AllowBatch) { "/implementar-registro $moduleArg" } else { "/implementar-registro $moduleArg single" }
            Write-Step "Invocando: claude -p '$slashCmd' --model $Model"
            Write-Host ""

            $claudeLog = New-TemporaryFile
            try {
                "" | claude --print $slashCmd `
                    --dangerously-skip-permissions `
                    --model $Model `
                    --no-session-persistence 2>&1 | Tee-Object -FilePath $claudeLog.FullName

                $claudeExit   = $LASTEXITCODE
                $claudeOutput = Get-Content $claudeLog.FullName -Raw
            } finally {
                Remove-Item $claudeLog.FullName -ErrorAction SilentlyContinue
            }
            Write-Host ""

            # Limite de sessao atingido: aguardar reset e repetir mesmo registro.
            # Proxima iteracao re-detecta PR/branch criado parcialmente (idempotente).
            if (Wait-ForSessionReset -ClaudeOutput $claudeOutput) {
                continue
            }

            if ($claudeExit -ne 0) {
                Write-Step "claude terminou com erro (exit code $claudeExit). Parando." "Red"
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
                Write-Step "Nenhum PR encontrado para '$expectedBranch'. Claude nao criou PR (possivel falha de build/teste)." "Red"
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
