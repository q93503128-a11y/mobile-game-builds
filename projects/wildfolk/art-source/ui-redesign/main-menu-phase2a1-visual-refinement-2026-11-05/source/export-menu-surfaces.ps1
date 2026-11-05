$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$sourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$artDir = Split-Path -Parent $sourceDir
$svgDir = Join-Path $artDir 'assets\svg'
$pngDir = Join-Path $artDir 'assets\png'
New-Item -ItemType Directory -Force -Path $svgDir,$pngDir | Out-Null

function Convert-Color([string]$hex) {
    return [System.Drawing.ColorTranslator]::FromHtml($hex)
}

function Get-ButtonPoints([bool]$inner, [int]$offsetY) {
    if ($inner) {
        $coords = @(@(19,12),@(341,12),@(348,20),@(348,100),@(341,108),@(19,108),@(12,100),@(12,20))
    } else {
        $coords = @(@(18,4),@(342,4),@(356,18),@(356,102),@(342,116),@(18,116),@(4,102),@(4,18))
    }
    return ,([System.Drawing.PointF[]]@($coords | ForEach-Object { [System.Drawing.PointF]::new([single]$_[0],[single]($_[1]+$offsetY)) }))
}

function Points-ToSvg([System.Drawing.PointF[]]$points) {
    $items = @($points | ForEach-Object { '{0:0.#},{1:0.#}' -f $_.X,$_.Y })
    return 'M ' + ($items -join ' L ') + ' Z'
}

function New-ButtonSvg([string]$name, [hashtable]$style, [bool]$selected, [int]$offsetY) {
    $outer = Points-ToSvg (Get-ButtonPoints $false $offsetY)
    $inner = Points-ToSvg (Get-ButtonPoints $true $offsetY)
    $marker = if ($selected) { '<path d="M 23 43 V 77" fill="none" stroke="' + $style.Marker + '" stroke-width="3" stroke-linecap="round"/>' } else { '' }
    $lightY = 16 + $offsetY
    return @"
<svg xmlns="http://www.w3.org/2000/svg" width="720" height="240" viewBox="0 0 360 120" role="img" aria-label="$name Lowfire button surface">
  <defs>
    <linearGradient id="face" x1="0" y1="0" x2="0" y2="1">
      <stop offset="0" stop-color="$($style.Top)"/><stop offset="1" stop-color="$($style.Bottom)"/>
    </linearGradient>
  </defs>
  <path d="$outer" fill="$($style.Edge)"/>
  <path d="$inner" fill="url(#face)"/>
  <path d="M 46 26 V 94 M 314 26 V 94" fill="none" stroke="$($style.CapSeam)" stroke-width="1"/>
  <path d="M 52 $lightY H 308" fill="none" stroke="$($style.Highlight)" stroke-width="1" stroke-linecap="round"/>
$marker
</svg>
"@
}

function Export-Button([string]$name, [hashtable]$style, [bool]$selected = $false, [int]$offsetY = 0) {
    $outer = Get-ButtonPoints $false $offsetY
    $inner = Get-ButtonPoints $true $offsetY
    $bmp = [System.Drawing.Bitmap]::new(720,240,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.ScaleTransform(2.0,2.0)
    if ($offsetY -eq 0) {
        $shadowPoints = Get-ButtonPoints $false 6
        $shadow = [System.Drawing.SolidBrush]::new((Convert-Color $style.Shadow))
        $g.FillPolygon($shadow,$shadowPoints)
        $shadow.Dispose()
    }
    $edge = [System.Drawing.SolidBrush]::new((Convert-Color $style.Edge))
    $g.FillPolygon($edge,$outer); $edge.Dispose()
    $bounds = [System.Drawing.RectangleF]::new(12,12,336,96)
    $fill = [System.Drawing.Drawing2D.LinearGradientBrush]::new($bounds,(Convert-Color $style.Top),(Convert-Color $style.Bottom),[System.Drawing.Drawing2D.LinearGradientMode]::Vertical)
    $g.FillPolygon($fill,$inner); $fill.Dispose()
    $seamPen = [System.Drawing.Pen]::new((Convert-Color $style.CapSeam),0.8)
    $g.DrawLine($seamPen,46,26+$offsetY,46,94+$offsetY)
    $g.DrawLine($seamPen,314,26+$offsetY,314,94+$offsetY)
    $seamPen.Dispose()
    $topPen = [System.Drawing.Pen]::new((Convert-Color $style.Highlight),0.85)
    $topPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $topPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    $g.DrawLine($topPen,52,16+$offsetY,308,16+$offsetY); $topPen.Dispose()
    if ($selected) {
        $markerPen = [System.Drawing.Pen]::new((Convert-Color $style.Marker),3.0)
        $markerPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
        $markerPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
        $g.DrawLine($markerPen,23,43+$offsetY,23,77+$offsetY); $markerPen.Dispose()
    }
    $g.Dispose()
    $bmp.Save((Join-Path $pngDir ($name+'.png')),[System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
    [IO.File]::WriteAllText((Join-Path $svgDir ($name+'.svg')),(New-ButtonSvg $name $style $selected $offsetY).Trim()+[Environment]::NewLine,[System.Text.UTF8Encoding]::new($false))
}

$styles = @{
    Primary = @{ Edge='#543326'; Top='#9F5439'; Bottom='#8B432D'; CapSeam='#75412F'; Highlight='#DDA07A'; Shadow='#13221C'; Marker='#F0D7AC' }
    Secondary = @{ Edge='#172D24'; Top='#3B5947'; Bottom='#2A4336'; CapSeam='#294234'; Highlight='#8AA17A'; Shadow='#111F19'; Marker='#C5D49A' }
    Selected = @{ Edge='#203B2B'; Top='#526F47'; Bottom='#426645'; CapSeam='#365A3C'; Highlight='#A9BD87'; Shadow='#14251C'; Marker='#E2E9BF' }
    PressedPrimary = @{ Edge='#42281F'; Top='#95492F'; Bottom='#7A3829'; CapSeam='#623627'; Highlight='#B77254'; Shadow='#14221B'; Marker='#E5C49A' }
    PressedSecondary = @{ Edge='#12251D'; Top='#2F493B'; Bottom='#243B31'; CapSeam='#25392E'; Highlight='#6F8968'; Shadow='#101D17'; Marker='#B9C98F' }
    DisabledPrimary = @{ Edge='#343A32'; Top='#626457'; Bottom='#505448'; CapSeam='#505448'; Highlight='#898A75'; Shadow='#121C17'; Marker='#B1B29D' }
    DisabledSecondary = @{ Edge='#17251F'; Top='#37443A'; Bottom='#2D3931'; CapSeam='#29352B'; Highlight='#66705D'; Shadow='#101915'; Marker='#9DA88A' }
}

Export-Button 'button-primary-default' $styles.Primary
Export-Button 'button-primary-pressed' $styles.PressedPrimary $false 2
Export-Button 'button-primary-disabled' $styles.DisabledPrimary
Export-Button 'button-secondary-default' $styles.Secondary
Export-Button 'button-secondary-selected' $styles.Selected $true
Export-Button 'button-secondary-pressed' $styles.PressedSecondary $false 2
Export-Button 'button-secondary-disabled' $styles.DisabledSecondary

$dialogSvg = @'
<svg xmlns="http://www.w3.org/2000/svg" width="720" height="520" viewBox="0 0 720 520" role="img" aria-label="Lowfire confirmation frame">
  <defs><linearGradient id="face" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="#293F33"/><stop offset="1" stop-color="#21362C"/></linearGradient></defs>
  <path d="M 40 4 H 680 L 716 40 V 480 L 680 516 H 40 L 4 480 V 40 Z" fill="#182B23"/>
  <path d="M 44 15 H 676 L 704 43 V 477 L 676 505 H 44 L 16 477 V 43 Z" fill="#805536"/>
  <path d="M 48 23 H 672 L 696 47 V 473 L 672 497 H 48 L 24 473 V 47 Z" fill="url(#face)"/>
  <path d="M 55 30 H 665" fill="none" stroke="#6F8A63" stroke-opacity=".65" stroke-width="1"/>
</svg>
'@
[IO.File]::WriteAllText((Join-Path $svgDir 'dialog-frame.svg'),$dialogSvg.Trim()+[Environment]::NewLine,[System.Text.UTF8Encoding]::new($false))
$dialog = [System.Drawing.Bitmap]::new(720,520,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$dg = [System.Drawing.Graphics]::FromImage($dialog)
$dg.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$dg.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
function Fill-Polygon($graphics,[string]$color,[object[]]$coords) {
    $points = [System.Drawing.PointF[]]@($coords | ForEach-Object { [System.Drawing.PointF]::new([single]$_[0],[single]$_[1]) })
    $brush = [System.Drawing.SolidBrush]::new((Convert-Color $color))
    $graphics.FillPolygon($brush,$points); $brush.Dispose()
}
$darkBrush=[System.Drawing.SolidBrush]::new((Convert-Color '#182B23'))
$dg.FillPolygon($darkBrush,[System.Drawing.PointF[]]@([System.Drawing.PointF]::new(40,4),[System.Drawing.PointF]::new(680,4),[System.Drawing.PointF]::new(716,40),[System.Drawing.PointF]::new(716,480),[System.Drawing.PointF]::new(680,516),[System.Drawing.PointF]::new(40,516),[System.Drawing.PointF]::new(4,480),[System.Drawing.PointF]::new(4,40)))
$darkBrush.Dispose()
Fill-Polygon $dg '#805536' @(@(44,15),@(676,15),@(704,43),@(704,477),@(676,505),@(44,505),@(16,477),@(16,43))
$dialogFace=[System.Drawing.PointF[]]@([System.Drawing.PointF]::new(48,23),[System.Drawing.PointF]::new(672,23),[System.Drawing.PointF]::new(696,47),[System.Drawing.PointF]::new(696,473),[System.Drawing.PointF]::new(672,497),[System.Drawing.PointF]::new(48,497),[System.Drawing.PointF]::new(24,473),[System.Drawing.PointF]::new(24,47))
$dialogGradient=[System.Drawing.Drawing2D.LinearGradientBrush]::new([System.Drawing.RectangleF]::new(24,23,672,474),(Convert-Color '#293F33'),(Convert-Color '#21362C'),[System.Drawing.Drawing2D.LinearGradientMode]::Vertical)
$dg.FillPolygon($dialogGradient,$dialogFace); $dialogGradient.Dispose()
$guidePen=[System.Drawing.Pen]::new((Convert-Color '#6F8A63'),1)
$dg.DrawLine($guidePen,55,30,665,30); $guidePen.Dispose()
$dg.Dispose(); $dialog.Save((Join-Path $pngDir 'dialog-frame.png'),[System.Drawing.Imaging.ImageFormat]::Png); $dialog.Dispose()

$manifest = @'
{
  "source": "assets/svg",
  "exporter": "source/export-menu-surfaces.ps1",
  "rasterScale": 2,
  "buttonCanvasPx": [720, 240],
  "buttonSliceBorderPx": { "left": 96, "right": 96, "top": 56, "bottom": 56 },
  "dialogCanvasPx": [720, 520],
  "dialogSliceBorderPx": { "left": 64, "right": 64, "top": 64, "bottom": 64 },
  "states": ["primary-default", "primary-pressed", "primary-disabled", "secondary-default", "secondary-selected", "secondary-pressed", "secondary-disabled"],
  "textBakedIntoAssets": false
}
'@
[IO.File]::WriteAllText((Join-Path $artDir 'surface-manifest.json'),$manifest.Trim()+[Environment]::NewLine,[System.Text.UTF8Encoding]::new($false))
Write-Output "Exported vector source and 8 raster surfaces to $pngDir"
