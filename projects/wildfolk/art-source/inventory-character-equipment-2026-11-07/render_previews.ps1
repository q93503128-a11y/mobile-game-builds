$ErrorActionPreference = 'Stop'
$sourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$wildfolkDir = (Resolve-Path (Join-Path $sourceDir '..\..')).Path
$previewDir = Join-Path $wildfolkDir 'validation\inventory-character-equipment-2026-11-07\previews'
$phase3dDir = Join-Path $wildfolkDir 'validation\inventory-ui-phase3d-2026-11-06\previews'
$edgePath = 'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'
if (-not (Test-Path -LiteralPath $edgePath)) { throw "Microsoft Edge not found at $edgePath" }

py -3.13 (Join-Path $sourceDir 'generate_screens.py')
if ($LASTEXITCODE -ne 0) { throw 'Character/equipment SVG generation failed.' }

$profileDir = Join-Path $env:TEMP 'lowfire-inventory-character-equipment-render'
$jobs = @(
    @{ Name='A-topdown-character-844x390'; Svg=(Join-Path $sourceDir 'screens\A-topdown-character.svg') },
    @{ Name='B-equipment-rail-844x390'; Svg=(Join-Path $sourceDir 'screens\B-equipment-rail.svg') }
)
foreach ($job in $jobs) {
    $png = Join-Path $previewDir ($job.Name + '.png')
    $raw = [System.IO.File]::ReadAllText($job.Svg)
    $match = [regex]::Match($raw, '<svg[^>]*\bwidth="([0-9.]+)"[^>]*\bheight="([0-9.]+)"')
    if (-not $match.Success) { throw "Could not read SVG dimensions for $($job.Name)." }
    $width = [int][math]::Round([double]::Parse($match.Groups[1].Value,[cultureinfo]::InvariantCulture))
    $height = [int][math]::Round([double]::Parse($match.Groups[2].Value,[cultureinfo]::InvariantCulture))
    $url = ([System.Uri]::new($job.Svg)).AbsoluteUri
    $args = @('--headless=new','--disable-gpu','--hide-scrollbars','--force-device-scale-factor=1',
        '--no-first-run','--no-default-browser-check',"--user-data-dir=$profileDir",
        "--window-size=$width,$height","--screenshot=$png",$url)
    $previousPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    & $edgePath @args 2>$null | Out-Null
    $ErrorActionPreference = $previousPreference
    if ($LASTEXITCODE -ne 0 -or -not (Test-Path -LiteralPath $png)) { throw "Edge did not render $($job.Name)." }
    Write-Output "$($job.Name) -> $width x $height"
}

$oldPath = Join-Path $phase3dDir '03-equipment-equip.png'
$aPath = Join-Path $previewDir 'A-topdown-character-844x390.png'
$bPath = Join-Path $previewDir 'B-equipment-rail-844x390.png'
$comparisonPath = Join-Path $previewDir 'Phase3D-face-A-B-compare-2532x390.png'
Add-Type -AssemblyName System.Drawing
$old = [System.Drawing.Bitmap]::FromFile($oldPath)
$a = [System.Drawing.Bitmap]::FromFile($aPath)
$b = [System.Drawing.Bitmap]::FromFile($bPath)
try {
    foreach($bitmap in @($old,$a,$b)) { if ($bitmap.Width -ne 844 -or $bitmap.Height -ne 390) { throw 'Comparison PNG size mismatch.' } }
    $comparison = [System.Drawing.Bitmap]::new(2532,390,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $graphics = [System.Drawing.Graphics]::FromImage($comparison)
        try {
            $graphics.DrawImageUnscaled($old,0,0)
            $graphics.DrawImageUnscaled($a,844,0)
            $graphics.DrawImageUnscaled($b,1688,0)
            $comparison.Save($comparisonPath,[System.Drawing.Imaging.ImageFormat]::Png)
        } finally { $graphics.Dispose() }
    } finally { $comparison.Dispose() }
} finally { $old.Dispose(); $a.Dispose(); $b.Dispose() }
Write-Output 'Comparison -> Phase 3D face crop, A top-down actor, B equipment rail (2532 x 390)'
