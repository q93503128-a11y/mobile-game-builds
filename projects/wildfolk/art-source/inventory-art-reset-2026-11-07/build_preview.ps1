$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$repo = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..\..')).Path
$sourceDir = Join-Path $PSScriptRoot 'source-generated'
$exportDir = Join-Path $PSScriptRoot 'exports'
$previewDir = Join-Path $repo 'projects\wildfolk\validation\inventory-art-reset-2026-11-07\previews'
$phase3d = Join-Path $repo 'projects\wildfolk\validation\inventory-ui-phase3d-2026-11-06\previews'
$basePath = Join-Path $phase3d '01-bag-basic.png'

New-Item -ItemType Directory -Force -Path $exportDir, $previewDir | Out-Null

function Get-AlphaCrop([System.Drawing.Bitmap]$image) {
    $minX = $image.Width; $minY = $image.Height; $maxX = -1; $maxY = -1
    for ($y = 0; $y -lt $image.Height; $y += 2) {
        for ($x = 0; $x -lt $image.Width; $x += 2) {
            if ($image.GetPixel($x, $y).A -gt 16) {
                if ($x -lt $minX) { $minX = $x }
                if ($y -lt $minY) { $minY = $y }
                if ($x -gt $maxX) { $maxX = $x }
                if ($y -gt $maxY) { $maxY = $y }
            }
        }
    }
    if ($maxX -lt $minX) { throw 'Artwork source contains no visible pixels.' }
    $left = [Math]::Max(0, $minX - 4); $top = [Math]::Max(0, $minY - 4)
    $right = [Math]::Min($image.Width - 1, $maxX + 5); $bottom = [Math]::Min($image.Height - 1, $maxY + 5)
    return [System.Drawing.Rectangle]::new($left, $top, $right - $left + 1, $bottom - $top + 1)
}

function Draw-Trimmed([System.Drawing.Graphics]$graphics, [string]$path, [System.Drawing.Rectangle]$target) {
    $bitmap = [System.Drawing.Bitmap]::FromFile($path)
    try {
        $crop = Get-AlphaCrop $bitmap
        $graphics.DrawImage($bitmap, $target, $crop, [System.Drawing.GraphicsUnit]::Pixel)
    }
    finally { $bitmap.Dispose() }
}

function Save-Export([string]$name, [int]$targetWidth, [int]$targetHeight) {
    $inputPath = Join-Path $sourceDir "$name.png"
    $bitmap = [System.Drawing.Bitmap]::FromFile($inputPath)
    $output = [System.Drawing.Bitmap]::new($targetWidth * 4, $targetHeight * 4, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $graphics = [System.Drawing.Graphics]::FromImage($output)
    try {
        $graphics.Clear([System.Drawing.Color]::Transparent)
        $graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
        $graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
        $graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality
        $crop = Get-AlphaCrop $bitmap
        $dest = [System.Drawing.Rectangle]::new(0, 0, $output.Width, $output.Height)
        $graphics.DrawImage($bitmap, $dest, $crop, [System.Drawing.GraphicsUnit]::Pixel)
        $output.Save((Join-Path $exportDir "$name-4x.png"), [System.Drawing.Imaging.ImageFormat]::Png)
    }
    finally { $graphics.Dispose(); $output.Dispose(); $bitmap.Dispose() }
}

Save-Export 'slot-normal' 48 41
Save-Export 'slot-selected' 48 41
Save-Export 'equipment-panel' 208 176
Save-Export 'button-primary' 188 44

$base = [System.Drawing.Bitmap]::FromFile($basePath)
if ($base.Width -ne 844 -or $base.Height -ne 390) { throw "Expected Phase 3D preview 844x390, got $($base.Width)x$($base.Height)." }
$native = [System.Drawing.Bitmap]::new(844, 390, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g = [System.Drawing.Graphics]::FromImage($native)
try {
    $g.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceCopy
    $g.DrawImageUnscaled($base, 0, 0)
    $g.CompositingMode = [System.Drawing.Drawing2D.CompositingMode]::SourceOver
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality

    # The empty bottom storage row and the unused right region are used only as a material-fit test.
    Draw-Trimmed $g (Join-Path $sourceDir 'slot-normal.png') ([System.Drawing.Rectangle]::new(148, 342, 48, 41))
    Draw-Trimmed $g (Join-Path $sourceDir 'slot-selected.png') ([System.Drawing.Rectangle]::new(197, 342, 48, 41))
    Draw-Trimmed $g (Join-Path $sourceDir 'equipment-panel.png') ([System.Drawing.Rectangle]::new(616, 91, 208, 176))
    Draw-Trimmed $g (Join-Path $sourceDir 'button-primary.png') ([System.Drawing.Rectangle]::new(626, 275, 188, 44))
    $native.Save((Join-Path $previewDir 'native-size-material-composite-844x390.png'), [System.Drawing.Imaging.ImageFormat]::Png)
}
finally { $g.Dispose(); $native.Dispose(); $base.Dispose() }

# A strict side-by-side comparison. The left screen is untouched Phase 3D; the right adds only the four samples.
$before = [System.Drawing.Bitmap]::FromFile($basePath)
$after = [System.Drawing.Bitmap]::FromFile((Join-Path $previewDir 'native-size-material-composite-844x390.png'))
$comparison = [System.Drawing.Bitmap]::new(1688, 390, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$cg = [System.Drawing.Graphics]::FromImage($comparison)
try {
    $cg.DrawImageUnscaled($before, 0, 0)
    $cg.DrawImageUnscaled($after, 844, 0)
    $comparison.Save((Join-Path $previewDir 'phase3d-before-vs-art-samples-1688x390.png'), [System.Drawing.Imaging.ImageFormat]::Png)
}
finally { $cg.Dispose(); $comparison.Dispose(); $before.Dispose(); $after.Dispose() }

# The original vector slot is cropped from the unmodified 1:1 Phase 3D screenshot for direct material comparison.
$base = [System.Drawing.Bitmap]::FromFile($basePath)
$slotBoard = [System.Drawing.Bitmap]::new(600, 184, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$sg = [System.Drawing.Graphics]::FromImage($slotBoard)
try {
    $sg.Clear([System.Drawing.Color]::FromArgb(255, 35, 47, 41))
    $sg.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::NearestNeighbor
    $labels = @('PHASE 3D · CURRENT SVG', 'NEW · NORMAL', 'NEW · SELECTED')
    $srcs = @($null, (Join-Path $sourceDir 'slot-normal.png'), (Join-Path $sourceDir 'slot-selected.png'))
    for ($i = 0; $i -lt 3; $i++) {
        $x = 18 + $i * 194
        if ($i -eq 0) {
            $cell = [System.Drawing.Bitmap]::new(48, 41, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
            $cellGraphics = [System.Drawing.Graphics]::FromImage($cell)
            try { $cellGraphics.DrawImage($base, [System.Drawing.Rectangle]::new(0, 0, 48, 41), [System.Drawing.Rectangle]::new(148, 342, 48, 41), [System.Drawing.GraphicsUnit]::Pixel) }
            finally { $cellGraphics.Dispose() }
            $sg.DrawImage($cell, [System.Drawing.Rectangle]::new($x, 20, 144, 123))
            $cell.Dispose()
        }
        else { Draw-Trimmed $sg $srcs[$i] ([System.Drawing.Rectangle]::new($x, 20, 144, 123)) }
        $font = [System.Drawing.Font]::new('Segoe UI', 8, [System.Drawing.FontStyle]::Bold)
        $brush = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(255, 239, 229, 199))
        try { $sg.DrawString($labels[$i], $font, $brush, [single]$x, [single]150) }
        finally { $font.Dispose(); $brush.Dispose() }
    }
    $slotBoard.Save((Join-Path $previewDir 'slot-material-before-after-3x.png'), [System.Drawing.Imaging.ImageFormat]::Png)
}
finally { $sg.Dispose(); $slotBoard.Dispose(); $base.Dispose() }

Get-ChildItem $exportDir, $previewDir -File | Select-Object FullName, Length
