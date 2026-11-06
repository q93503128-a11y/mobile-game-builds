$ErrorActionPreference = 'Stop'
$sourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$wildfolkDir = (Resolve-Path (Join-Path $sourceDir '..\..')).Path
$validationDir = Join-Path $wildfolkDir 'validation\inventory-ui-phase3a-2026-11-06\previews'
$svgDir = Join-Path $validationDir 'svg'
$edgePath = 'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'
if (-not (Test-Path -LiteralPath $edgePath)) { throw "Microsoft Edge not found at $edgePath" }

py -3.13 (Join-Path $sourceDir 'generate_previews.py')
if ($LASTEXITCODE -ne 0) { throw 'SVG generation failed.' }

$profileDir = Join-Path $env:TEMP 'lowfire-inventory-phase3a-render-profile'
$svgs = @(Get-ChildItem -LiteralPath $svgDir -File -Filter '*.svg')
$kit = Join-Path $sourceDir 'inventory-ui-kit.svg'
if (Test-Path -LiteralPath $kit) { $svgs += Get-Item -LiteralPath $kit }

foreach ($svg in $svgs) {
    $raw = [System.IO.File]::ReadAllText($svg.FullName)
    $match = [regex]::Match($raw, '<svg[^>]*\bwidth="(\d+)"[^>]*\bheight="(\d+)"')
    if (-not $match.Success) { throw "Could not read SVG dimensions: $($svg.Name)" }
    $width = [int]$match.Groups[1].Value
    $height = [int]$match.Groups[2].Value
    $outputDir = $validationDir
    $outputPath = Join-Path $outputDir ($svg.BaseName + '.png')
    $url = ([System.Uri]::new($svg.FullName)).AbsoluteUri
    $args = @('--headless=new', '--disable-gpu', '--hide-scrollbars', '--force-device-scale-factor=1',
              '--no-first-run', '--no-default-browser-check', "--user-data-dir=$profileDir",
              "--window-size=$width,$height", "--screenshot=$outputPath", $url)
    $previousPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    & $edgePath @args 2>$null | Out-Null
    $ErrorActionPreference = $previousPreference
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $outputPath)) {
        throw "Edge did not render $($svg.Name)."
    }
    Write-Output "$($svg.Name) -> $($svg.BaseName).png ($width x $height)"
}
