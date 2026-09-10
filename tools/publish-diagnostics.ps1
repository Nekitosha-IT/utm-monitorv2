param(
    [string]$RepoPath = (Get-Location).Path,
    [switch]$NoPush
)

$ErrorActionPreference = 'Stop'

$repo = (Resolve-Path -LiteralPath $RepoPath).Path
$sourceDir = Join-Path $env:LOCALAPPDATA 'EGAISInspector\logs'
$today = Get-Date -Format 'yyyy-MM-dd'
$source = Join-Path $sourceDir "egais-inspector-$today.log"
$diagnosticsDir = Join-Path $repo 'diagnostics'
$target = Join-Path $diagnosticsDir 'latest.log'

if (-not (Test-Path -LiteralPath (Join-Path $repo '.git'))) {
    throw "Не найден Git-репозиторий: $repo"
}

if (-not (Test-Path -LiteralPath $source)) {
    throw "Сегодняшний лог не найден: $source"
}

New-Item -ItemType Directory -Force -Path $diagnosticsDir | Out-Null

$text = Get-Content -LiteralPath $source -Raw -Encoding UTF8

# Remove common secret-bearing values before anything is committed to GitHub.
$text = $text -replace '(?i)(authorization\s*[:=]\s*bearer\s+)[^\s\r\n]+', '$1[REDACTED]'
$text = $text -replace '(?i)(bearer\s+)[A-Za-z0-9._~+/=-]{20,}', '$1[REDACTED]'
$text = $text -replace '(?i)(password|passwd|pwd|token|access[_-]?token|client[_-]?secret)\s*[:=]\s*[^\s\r\n;]+', '$1=[REDACTED]'
$text = $text -replace '(?i)(private\s*key|secret)\s*[:=]\s*[^\r\n]+', '$1=[REDACTED]'

Set-Content -LiteralPath $target -Value $text -Encoding UTF8

Push-Location $repo
try {
    git add -- diagnostics/latest.log

    $status = git status --porcelain -- diagnostics/latest.log
    if (-not $status) {
        Write-Host 'Диагностический лог не изменился.'
        exit 0
    }

    git commit -m "chore: update diagnostics log"

    if (-not $NoPush) {
        git push origin main
        Write-Host 'Лог опубликован в GitHub: diagnostics/latest.log'
    } else {
        Write-Host 'Коммит создан. Push пропущен (-NoPush).'
    }
}
finally {
    Pop-Location
}
