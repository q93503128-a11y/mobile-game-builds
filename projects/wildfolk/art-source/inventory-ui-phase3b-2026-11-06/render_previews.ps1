$ErrorActionPreference = 'Stop'
$sourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$wildfolkDir = (Resolve-Path (Join-Path $sourceDir '..\..')).Path
$validationDir = Join-Path $wildfolkDir 'validation\inventory-ui-phase3b-2026-11-06'
$svgDir = Join-Path $validationDir 'previews\svg'
$edgePath = 'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'
if (-not (Test-Path -LiteralPath $edgePath)) { throw "Microsoft Edge not found at $edgePath" }

py -3.13 (Join-Path $sourceDir 'generate_previews.py')
if ($LASTEXITCODE -ne 0) { throw 'SVG generation failed.' }

$profileDir = Join-Path $env:TEMP 'lowfire-inventory-phase3b-render-profile'
$names = @('a-basic','a-selected','a-equipment','b-basic','b-selected','b-equipment','ab-overview')
foreach ($name in $names) {
    $svgPath = Join-Path $svgDir ($name + '.svg')
    $raw = [System.IO.File]::ReadAllText($svgPath)
    $match = [regex]::Match($raw, '<svg[^>]*\bwidth="([0-9.]+)"[^>]*\bheight="([0-9.]+)"')
    if (-not $match.Success) { throw "Could not read SVG dimensions: $($name).svg" }
    $width = [int][math]::Round([double]::Parse($match.Groups[1].Value,[cultureinfo]::InvariantCulture))
    $height = [int][math]::Round([double]::Parse($match.Groups[2].Value,[cultureinfo]::InvariantCulture))
    $outputPath = Join-Path $validationDir ('previews\' + $name + '.png')
    $url = ([System.Uri]::new($svgPath)).AbsoluteUri
    $edgeArgs = @('--headless=new','--disable-gpu','--hide-scrollbars','--force-device-scale-factor=1',
              '--no-first-run','--no-default-browser-check',"--user-data-dir=$profileDir",
              "--window-size=$width,$height","--screenshot=$outputPath",$url)
    $previousPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    & $edgePath @edgeArgs 2>$null | Out-Null
    $ErrorActionPreference = $previousPreference
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $outputPath)) { throw "Edge did not render $name.svg" }
    Write-Output "$name -> $width x $height"
}
