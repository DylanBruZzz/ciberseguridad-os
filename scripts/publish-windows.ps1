param(
    [string]$Configuration = "Release",
    [string]$Runtime = "win-x64",
    [string]$OutputPath
)

$ErrorActionPreference = "Stop"

function Resolve-FullPath([string]$Path) {
    $executionContext.SessionState.Path.GetUnresolvedProviderPathFromPSPath($Path)
}

function Assert-ChildPath([string]$Path, [string]$ParentPath) {
    $resolvedPath = Resolve-FullPath $Path
    $resolvedParent = Resolve-FullPath $ParentPath

    if (-not $resolvedPath.StartsWith($resolvedParent, [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "Unsafe path outside expected parent: $resolvedPath"
    }
}

function Invoke-Native([string]$FileName, [string[]]$Arguments, [string]$WorkingDirectory) {
    Push-Location $WorkingDirectory
    try {
        & $FileName @Arguments
        if ($LASTEXITCODE -ne 0) {
            throw "$FileName exited with code $LASTEXITCODE"
        }
    }
    finally {
        Pop-Location
    }
}

$repoRoot = Resolve-FullPath (Join-Path $PSScriptRoot "..")
$frontendDir = Join-Path $repoRoot "frontend"
$apiProject = Join-Path $repoRoot "src\Aprendizaje.Api\Aprendizaje.Api.csproj"
$wwwroot = Join-Path $repoRoot "src\Aprendizaje.Api\wwwroot"
$distBrowser = Join-Path $frontendDir "dist\frontend\browser"
$artifactsRoot = Join-Path $repoRoot "artifacts"

if ([string]::IsNullOrWhiteSpace($OutputPath)) {
    $OutputPath = Join-Path $artifactsRoot "CiberseguridadOS-win-x64"
}

$OutputPath = Resolve-FullPath $OutputPath

Assert-ChildPath $wwwroot (Join-Path $repoRoot "src\Aprendizaje.Api")
Assert-ChildPath $OutputPath $repoRoot

if (-not (Test-Path -LiteralPath (Join-Path $frontendDir "node_modules"))) {
    if (Test-Path -LiteralPath (Join-Path $frontendDir "package-lock.json")) {
        Invoke-Native "npm" @("ci") $frontendDir
    }
    else {
        Invoke-Native "npm" @("install") $frontendDir
    }
}

Invoke-Native "npm" @("run", "build") $frontendDir

if (-not (Test-Path -LiteralPath (Join-Path $distBrowser "index.html"))) {
    throw "Angular production output was not found at $distBrowser"
}

if (Test-Path -LiteralPath $wwwroot) {
    Remove-Item -LiteralPath $wwwroot -Recurse -Force
}

New-Item -ItemType Directory -Path $wwwroot | Out-Null
Copy-Item -Path (Join-Path $distBrowser "*") -Destination $wwwroot -Recurse -Force

if (Test-Path -LiteralPath $OutputPath) {
    Remove-Item -LiteralPath $OutputPath -Recurse -Force
}

New-Item -ItemType Directory -Path $OutputPath | Out-Null

Invoke-Native "dotnet" @(
    "publish",
    $apiProject,
    "-c", $Configuration,
    "-r", $Runtime,
    "--self-contained", "true",
    "-p:BuildFrontendOnPublish=false",
    "-p:PublishSingleFile=false",
    "-p:PublishTrimmed=false",
    "-p:PublishReadyToRun=false",
    "-o", $OutputPath
) $repoRoot

$exe = Join-Path $OutputPath "CiberseguridadOS.exe"
$index = Join-Path $OutputPath "wwwroot\index.html"
$personalSettings = Join-Path $OutputPath "appsettings.Personal.json"
$marker = Join-Path $OutputPath "CiberseguridadOS.packaged"

foreach ($requiredPath in @($exe, $index, $personalSettings, $marker)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Expected publish output was not found: $requiredPath"
    }
}

$files = Get-ChildItem -LiteralPath $OutputPath -Recurse -File
$sizeBytes = ($files | Measure-Object -Property Length -Sum).Sum
$sizeMb = [Math]::Round($sizeBytes / 1MB, 2)
$exeMb = [Math]::Round((Get-Item -LiteralPath $exe).Length / 1MB, 2)

Write-Host "Publish completed: $OutputPath"
Write-Host "Files: $($files.Count)"
Write-Host "Total size: $sizeMb MB"
Write-Host "Executable size: $exeMb MB"
