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

.EXAMPLE
    .\scripts\auto-implement-sped.ps1 -Bloco 0
    Implementa todos os registros pendentes do Bloco 0.

.EXAMPLE
    .\scripts\auto-implement-sped.ps1 -Bloco 0 -DryRun
    Lista pendentes do Bloco 0 sem implementar.

.EXAMPLE
    .\scripts\auto-implement-sped.ps1 -Bloco A -MaxPRs 3 -Model opus
    Implementa até 3 registros do Bloco A usando Opus.
#>
param(
    [string]$Bloco            = "",
    [int]   $MaxPRs           = 999,
    [switch]$DryRun,
    [int]   $CiTimeoutMinutes = 25,
    [string]$Model            = "sonnet"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$RepoRoot     = Split-Path $PSScriptRoot -Parent
$TrackingFile = Join-Path $RepoRoot "STAGE_4_REGISTROS.md"

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
    # Returns array of {number, headRefName}
    $json = gh pr list --state open --base dev --json number,headRefName --limit 50
    if (-not $json) { return @() }
    return $json | ConvertFrom-Json
}

function Wait-ForCI {
    param([int]$PrNumber, [int]$TimeoutMinutes)

    $startTime = Get-Date
    $deadline  = $startTime.AddMinutes($TimeoutMinutes)

    Write-Step "Aguardando CI no PR #$PrNumber (timeout: ${TimeoutMinutes}min)..."

    # Give GitHub time to queue checks
    Start-Sleep -Seconds 20

    while ((Get-Date) -lt $deadline) {
        $checksJson = gh pr checks $PrNumber --json name,state,bucket 2>$null

        if ($checksJson) {
            $checks = $checksJson | ConvertFrom-Json

            # gh pr checks --json uses "bucket" field: pass | fail | pending | skipping | cancel
            $failed  = @($checks | Where-Object { $_.bucket -in @("fail","cancel") })
            $running = @($checks | Where-Object { $_.bucket -eq "pending" })
            $passed  = @($checks | Where-Object { $_.bucket -in @("pass","skipping") })

            if ($failed.Count -gt 0) {
                $names = ($failed | ForEach-Object { $_.name }) -join ", "
                Write-Step "CI FALHOU: $names" "Red"
                return "failed"
            }

            if ($running.Count -eq 0 -and $passed.Count -gt 0) {
                Write-Step "CI APROVADO ($($passed.Count) checks)" "Green"
                return "success"
            }

            $elapsed = [int]((Get-Date) - $startTime).TotalMinutes
            Write-Step "${elapsed}min decorrido — $($running.Count) checks em andamento, $($passed.Count) concluidos..."
        } else {
            Write-Step "Checks ainda nao apareceram no GitHub..."
        }

        Start-Sleep -Seconds 30
    }

    Write-Step "TIMEOUT apos ${TimeoutMinutes} minutos" "Yellow"
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
        $pending = Get-PendingRegistros -TargetBloco $Bloco
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
    $initialPending = Get-PendingRegistros -TargetBloco $Bloco
    $blocoStr       = if ($Bloco) { "Bloco $Bloco" } else { "todos os blocos" }

    if ($initialPending.Count -eq 0) {
        Write-Host "Nenhum registro pendente ($blocoStr). Ja concluido!" -ForegroundColor Green
        exit 0
    }

    Write-Banner "Auto-implementacao SPED — $blocoStr — $($initialPending.Count) registro(s) pendente(s)" "Cyan"
    Write-Host "  Modelo    : $Model"
    Write-Host "  MaxPRs    : $MaxPRs"
    Write-Host "  CI timeout: ${CiTimeoutMinutes}min"
    Write-Host ""

    $mergedCount = 0

    # ── loop principal ─────────────────────────────────────────────────────────
    while ($mergedCount -lt $MaxPRs) {

        # Re-read pendentes a cada iteracao (tracking file muda apos cada merge)
        $pending = Get-PendingRegistros -TargetBloco $Bloco

        if ($pending.Count -eq 0) {
            Write-Banner "Todos os registros concluidos!" "Green"
            break
        }

        $next = $pending[0]
        Write-Banner "$($next.SubStage)  Registro $($next.Code) — $($next.Description)" "Cyan"

        # ── garantir dev atualizado ────────────────────────────────────────────
        Write-Step "Sincronizando branch dev..."
        git checkout dev --quiet
        git pull --quiet

        # ── snapshot de PRs abertos antes ─────────────────────────────────────
        $beforePRNumbers = @(Get-OpenPRs | ForEach-Object { $_.number })
        Write-Step "PRs abertos antes: $(if ($beforePRNumbers.Count -eq 0) { 'nenhum' } else { $beforePRNumbers -join ', ' })"

        # ── invocar Claude ─────────────────────────────────────────────────────
        Write-Step "Invocando: claude -p /implementar-registro --model $Model"
        Write-Host ""

        claude --print "/implementar-registro" `
            --dangerously-skip-permissions `
            --model $Model `
            --no-session-persistence

        $claudeExit = $LASTEXITCODE
        Write-Host ""

        if ($claudeExit -ne 0) {
            Write-Step "claude terminou com erro (exit code $claudeExit). Parando." "Red"
            exit 1
        }

        # ── encontrar novo PR ──────────────────────────────────────────────────
        Write-Step "Verificando novo PR criado..."
        Start-Sleep -Seconds 5

        $afterPRs = @(Get-OpenPRs)
        $newPR    = $afterPRs | Where-Object { $beforePRNumbers -notcontains $_.number } | Select-Object -First 1

        if (-not $newPR) {
            Write-Step "Nenhum novo PR encontrado. Claude nao criou PR (possivel falha de build/teste)." "Red"
            Write-Step "Verifique o output acima e corrija manualmente antes de reiniciar." "Yellow"
            exit 1
        }

        $prNumber   = [int]$newPR.number
        $branchName = $newPR.headRefName
        $prUrl      = gh pr view $prNumber --json url --jq ".url" 2>$null

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

        # Pausa breve entre iteracoes
        Start-Sleep -Seconds 8
    }

    # ── sumario final ──────────────────────────────────────────────────────────
    Write-Host ""
    Write-Host ("=" * 70) -ForegroundColor Green
    Write-Host "  Automacao finalizada. $mergedCount PR(s) mergeados." -ForegroundColor Green

    $remaining = (Get-PendingRegistros -TargetBloco $Bloco).Count
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
