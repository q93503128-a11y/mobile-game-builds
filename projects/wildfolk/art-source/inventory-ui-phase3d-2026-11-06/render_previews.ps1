$ErrorActionPreference = 'Stop'
$sourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$wildfolkDir = (Resolve-Path (Join-Path $sourceDir '..\..')).Path
$validationDir = Join-Path $wildfolkDir 'validation\inventory-ui-phase3d-2026-11-06'
$screenDir = Join-Path $sourceDir 'screens'
$graphicsDir = Join-Path $sourceDir 'graphics'
$edgePath = 'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'
if (-not (Test-Path -LiteralPath $edgePath)) { throw "Microsoft Edge not found at $edgePath" }

py -3.13 (Join-Path $sourceDir 'generate_previews.py')
if ($LASTEXITCODE -ne 0) { throw 'SVG generation failed.' }

$profileDir = Join-Path $env:TEMP 'lowfire-inventory-phase3d-render-profile'
$jobs = @(
    @{ Name='01-bag-basic'; Svg=(Join-Path $screenDir 'basic-inventory.svg'); Png=(Join-Path $validationDir 'previews\01-bag-basic.png') },
    @{ Name='02-item-selected'; Svg=(Join-Path $screenDir 'item-selected.svg'); Png=(Join-Path $validationDir 'previews\02-item-selected.png') },
    @{ Name='03-equipment-equip'; Svg=(Join-Path $screenDir 'equipment-equip.svg'); Png=(Join-Path $validationDir 'previews\03-equipment-equip.png') },
    @{ Name='original-wood-ui-kit'; Svg=(Join-Path $graphicsDir 'lowfire-reference-wood-kit.svg'); Png=(Join-Path $validationDir 'assets\lowfire-reference-wood-kit.png') }
)
foreach ($job in $jobs) {
    $raw = [System.IO.File]::ReadAllText($job.Svg)
    $match = [regex]::Match($raw, '<svg[^>]*\bwidth="([0-9.]+)"[^>]*\bheight="([0-9.]+)"')
    if (-not $match.Success) { throw "Could not read SVG dimensions: $($job.Name).svg" }
    $width = [int][math]::Round([double]::Parse($match.Groups[1].Value,[cultureinfo]::InvariantCulture))
    $height = [int][math]::Round([double]::Parse($match.Groups[2].Value,[cultureinfo]::InvariantCulture))
    $url = ([System.Uri]::new($job.Svg)).AbsoluteUri
    $edgeArgs = @('--headless=new','--disable-gpu','--hide-scrollbars','--force-device-scale-factor=1',
        '--no-first-run','--no-default-browser-check',"--user-data-dir=$profileDir",
        "--window-size=$width,$height","--screenshot=$($job.Png)",$url)
    $previousPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    & $edgePath @edgeArgs 2>$null | Out-Null
    $ErrorActionPreference = $previousPreference
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $job.Png)) { throw "Edge did not render $($job.Name).svg" }
    Write-Output "$($job.Name) -> $width x $height"
}
