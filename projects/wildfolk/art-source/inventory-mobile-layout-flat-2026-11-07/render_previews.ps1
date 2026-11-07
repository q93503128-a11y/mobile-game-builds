$ErrorActionPreference = 'Stop'
$sourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$wildfolkDir = (Resolve-Path (Join-Path $sourceDir '..\..')).Path
$previewDir = Join-Path $wildfolkDir 'validation\inventory-mobile-layout-flat-2026-11-07\previews'
$edgePath = 'C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe'
if (-not (Test-Path -LiteralPath $edgePath)) { throw "Microsoft Edge not found at $edgePath" }

py -3.13 (Join-Path $sourceDir 'generate_layouts.py')
if ($LASTEXITCODE -ne 0) { throw 'Flat layout SVG generation failed.' }

$profileDir = Join-Path $env:TEMP 'lowfire-inventory-flat-mobile-render-profile'
$jobs = @(
    @{ Name='A-all27-844x390'; Svg=(Join-Path $sourceDir 'screens\A-all27.svg') },
    @{ Name='B-page18-844x390'; Svg=(Join-Path $sourceDir 'screens\B-page18.svg') }
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

$aPath = Join-Path $previewDir 'A-all27-844x390.png'
$bPath = Join-Path $previewDir 'B-page18-844x390.png'
$comparisonPath = Join-Path $previewDir 'A-B-comparison-1688x390.png'
$baselinePath = Join-Path $wildfolkDir 'validation\inventory-ui-phase3d-2026-11-06\previews\01-bag-basic.png'
$threeWayPath = Join-Path $previewDir 'Phase3D-A-B-comparison-2532x390.png'
Add-Type -AssemblyName System.Drawing
$a = [System.Drawing.Bitmap]::FromFile($aPath)
$b = [System.Drawing.Bitmap]::FromFile($bPath)
$baseline = [System.Drawing.Bitmap]::FromFile($baselinePath)
try {
    if ($a.Width -ne 844 -or $a.Height -ne 390 -or $b.Width -ne 844 -or $b.Height -ne 390 -or $baseline.Width -ne 844 -or $baseline.Height -ne 390) { throw 'Rendered PNG size mismatch.' }
    $comparison = [System.Drawing.Bitmap]::new(1688,390,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $graphics = [System.Drawing.Graphics]::FromImage($comparison)
        try {
            $graphics.DrawImageUnscaled($a,0,0)
            $graphics.DrawImageUnscaled($b,844,0)
            $comparison.Save($comparisonPath,[System.Drawing.Imaging.ImageFormat]::Png)
        } finally { $graphics.Dispose() }
    } finally { $comparison.Dispose() }
    $threeWay = [System.Drawing.Bitmap]::new(2532,390,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    try {
        $graphics = [System.Drawing.Graphics]::FromImage($threeWay)
        try {
            $graphics.DrawImageUnscaled($baseline,0,0)
            $graphics.DrawImageUnscaled($a,844,0)
            $graphics.DrawImageUnscaled($b,1688,0)
            $threeWay.Save($threeWayPath,[System.Drawing.Imaging.ImageFormat]::Png)
        } finally { $graphics.Dispose() }
    } finally { $threeWay.Dispose() }
} finally { $a.Dispose(); $b.Dispose(); $baseline.Dispose() }
Write-Output 'A/B comparison -> 1688 x 390'
Write-Output 'Phase 3D/A/B comparison -> 2532 x 390'
