$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing

$sourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$artDir = Split-Path -Parent $sourceDir
$svgRoot = Join-Path $artDir 'assets\svg'
$pngRoot = Join-Path $artDir 'assets\png'
New-Item -ItemType Directory -Force -Path $svgRoot,$pngRoot | Out-Null

function C([string]$hex) { [System.Drawing.ColorTranslator]::FromHtml($hex) }
function Normalize-Svg([string]$svg) {
    return [Regex]::Replace($svg, '[\t ]+(?=\r?$)', '', [Text.RegularExpressions.RegexOptions]::Multiline).Trim()
}
function New-RoundPath([float]$x,[float]$y,[float]$w,[float]$h,[float]$r) {
    $p = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $d = $r * 2
    $p.AddArc($x,$y,$d,$d,180,90)
    $p.AddArc($x+$w-$d,$y,$d,$d,270,90)
    $p.AddArc($x+$w-$d,$y+$h-$d,$d,$d,0,90)
    $p.AddArc($x,$y+$h-$d,$d,$d,90,90)
    $p.CloseFigure()
    return $p
}
function New-ChamferPath([float]$x,[float]$y,[float]$w,[float]$h,[float]$c) {
    $p = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $p.AddPolygon([System.Drawing.PointF[]]@(
        [System.Drawing.PointF]::new($x+$c,$y),[System.Drawing.PointF]::new($x+$w-$c,$y),
        [System.Drawing.PointF]::new($x+$w,$y+$c),[System.Drawing.PointF]::new($x+$w,$y+$h-$c),
        [System.Drawing.PointF]::new($x+$w-$c,$y+$h),[System.Drawing.PointF]::new($x+$c,$y+$h),
        [System.Drawing.PointF]::new($x,$y+$h-$c),[System.Drawing.PointF]::new($x,$y+$c)
    ))
    $p.CloseFigure()
    return $p
}
function Get-PathSvg([string]$direction,[int]$offsetY) {
    if ($direction -eq 'A') {
        $y = 7 + $offsetY
        return "<rect x=`"5`" y=`"$y`" width=`"350`" height=`"106`" rx=`"22`" />"
    }
    $y = 5 + $offsetY
    return "<path d=`"M 18 $y H 342 L 355 $($y+13) V $($y+93) L 342 $($y+106) H 18 L 5 $($y+93) V $($y+13) Z`" />"
}
function Write-ButtonSvg([string]$file,[string]$direction,[hashtable]$style,[bool]$selected,[int]$offsetY) {
    $folder=if($direction -eq 'A'){'a-soft-forest'}else{'b-refined-utility'}
    $outer = Get-PathSvg $direction $offsetY
    if ($direction -eq 'A') {
        $inner = "<rect x=`"10`" y=`"$(12+$offsetY)`" width=`"340`" height=`"96`" rx=`"18`" fill=`"url(#face)`" />"
        $highlight = "<path d=`"M 34 $(16+$offsetY) H 326`" stroke=`"$($style.Highlight)`" stroke-opacity=`".55`" stroke-width=`"1`" stroke-linecap=`"round`" />"
        $lower = "<path d=`"M 34 $(103+$offsetY) H 326`" stroke=`"$($style.Lower)`" stroke-opacity=`".42`" stroke-width=`"1`" stroke-linecap=`"round`" />"
    } else {
        $inner = "<path d=`"M 21 $(11+$offsetY) H 339 L 349 $(21+$offsetY) V $(99+$offsetY) L 339 $(109+$offsetY) H 21 L 11 $(99+$offsetY) V $(21+$offsetY) Z`" fill=`"$($style.Face)`" />"
        $highlight = "<path d=`"M 38 $(17+$offsetY) H 322`" stroke=`"$($style.Highlight)`" stroke-opacity=`".28`" stroke-width=`"1`" />"
        $lower = "<path d=`"M 38 $(103+$offsetY) H 322`" stroke=`"$($style.Lower)`" stroke-opacity=`".42`" stroke-width=`"1`" />"
    }
    $marker = if ($selected) { "<path d=`"M 22 $(43+$offsetY) V $(77+$offsetY)`" stroke=`"$($style.Marker)`" stroke-width=`"3`" stroke-linecap=`"round`" />" } else { '' }
    $gradient = if ($direction -eq 'A') { "<linearGradient id=`"face`" x1=`"0`" y1=`"0`" x2=`"0`" y2=`"1`"><stop offset=`"0`" stop-color=`"$($style.Top)`"/><stop offset=`"1`" stop-color=`"$($style.Bottom)`"/></linearGradient>" } else { '' }
    $face = if ($direction -eq 'A') { '' } else { " fill=`"$($style.Face)`"" }
    $svg = @"
<svg xmlns="http://www.w3.org/2000/svg" width="720" height="240" viewBox="0 0 360 120" role="img" aria-label="Lowfire $file button surface">
  <defs>$gradient</defs>
  <g fill="$($style.Edge)" stroke="$($style.Edge)" stroke-width="$($style.Stroke)" stroke-linejoin="round">$outer</g>
  $inner
  $highlight
  $lower
  $marker
</svg>
"@
    [IO.File]::WriteAllText((Join-Path $svgRoot "$folder\$file.svg"),(Normalize-Svg $svg)+[Environment]::NewLine,[Text.UTF8Encoding]::new($false))
}
function Export-ButtonPng([string]$file,[string]$direction,[hashtable]$style,[bool]$selected,[int]$offsetY) {
    $folder=if($direction -eq 'A'){'a-soft-forest'}else{'b-refined-utility'}
    $bmp = [System.Drawing.Bitmap]::new(720,240,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.ScaleTransform(2,2)
    if ($direction -eq 'A') {
        $outer = New-RoundPath 5 (7+$offsetY) 350 106 22
        $inner = New-RoundPath 10 (12+$offsetY) 340 96 18
    } else {
        $outer = New-ChamferPath 5 (5+$offsetY) 350 110 13
        $inner = New-ChamferPath 11 (11+$offsetY) 338 98 10
    }
    $edgeBrush = [System.Drawing.SolidBrush]::new((C $style.Edge))
    $g.FillPath($edgeBrush,$outer)
    $edgeBrush.Dispose()
    if ($direction -eq 'A') {
        $bounds = [System.Drawing.RectangleF]::new(10,(12+$offsetY),340,96)
        $faceBrush = [System.Drawing.Drawing2D.LinearGradientBrush]::new($bounds,(C $style.Top),(C $style.Bottom),[System.Drawing.Drawing2D.LinearGradientMode]::Vertical)
    } else { $faceBrush = [System.Drawing.SolidBrush]::new((C $style.Face)) }
    $g.FillPath($faceBrush,$inner)
    $faceBrush.Dispose()
    $highlightPen = [System.Drawing.Pen]::new((C $style.Highlight),1)
    $highlightPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $highlightPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    if ($direction -eq 'A') { $g.DrawLine($highlightPen,34,16+$offsetY,326,16+$offsetY) }
    else { $g.DrawLine($highlightPen,38,17+$offsetY,322,17+$offsetY) }
    $highlightPen.Dispose()
    $lowerPen = [System.Drawing.Pen]::new((C $style.Lower),1)
    $lowerPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
    $lowerPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
    if ($direction -eq 'A') { $g.DrawLine($lowerPen,34,103+$offsetY,326,103+$offsetY) }
    else { $g.DrawLine($lowerPen,38,103+$offsetY,322,103+$offsetY) }
    $lowerPen.Dispose()
    if ($selected) {
        $markerPen = [System.Drawing.Pen]::new((C $style.Marker),3)
        $markerPen.StartCap = [System.Drawing.Drawing2D.LineCap]::Round
        $markerPen.EndCap = [System.Drawing.Drawing2D.LineCap]::Round
        $g.DrawLine($markerPen,22,43+$offsetY,22,77+$offsetY)
        $markerPen.Dispose()
    }
    $outer.Dispose(); $inner.Dispose(); $g.Dispose()
    $bmp.Save((Join-Path $pngRoot "$folder\$file.png"),[System.Drawing.Imaging.ImageFormat]::Png)
    $bmp.Dispose()
}
function Write-Dialog([string]$direction,[hashtable]$style) {
    $folder=if($direction -eq 'A'){'a-soft-forest'}else{'b-refined-utility'}
    $file='dialog-frame'
    if ($direction -eq 'A') {
        $svg=@"
<svg xmlns="http://www.w3.org/2000/svg" width="720" height="520" viewBox="0 0 360 260" role="img" aria-label="Lowfire soft forest confirmation frame">
 <defs><linearGradient id="body" x1="0" y1="0" x2="0" y2="1"><stop offset="0" stop-color="$($style.Top)"/><stop offset="1" stop-color="$($style.Bottom)"/></linearGradient></defs>
 <rect x="5" y="5" width="350" height="250" rx="19" fill="$($style.Edge)"/>
 <rect x="10" y="10" width="340" height="240" rx="15" fill="url(#body)" stroke="$($style.Highlight)" stroke-opacity=".54" stroke-width="1"/>
 <path d="M 31 17 H 329" stroke="$($style.Highlight)" stroke-opacity=".5" stroke-width="1" stroke-linecap="round"/>
</svg>
"@
    } else {
        $svg=@"
<svg xmlns="http://www.w3.org/2000/svg" width="720" height="520" viewBox="0 0 360 260" role="img" aria-label="Lowfire refined utility confirmation frame">
 <path d="M 17 5 H 343 L 355 17 V 243 L 343 255 H 17 L 5 243 V 17 Z" fill="$($style.Edge)"/>
 <path d="M 20 11 H 340 L 349 20 V 240 L 340 249 H 20 L 11 240 V 20 Z" fill="$($style.Face)" stroke="$($style.Highlight)" stroke-opacity=".58" stroke-width="1"/>
 <path d="M 30 18 H 330" stroke="$($style.Highlight)" stroke-opacity=".38" stroke-width="1"/>
</svg>
"@
    }
    [IO.File]::WriteAllText((Join-Path $svgRoot "$folder\$file.svg"),(Normalize-Svg $svg)+[Environment]::NewLine,[Text.UTF8Encoding]::new($false))
    $bmp=[System.Drawing.Bitmap]::new(720,520,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g=[System.Drawing.Graphics]::FromImage($bmp);$g.SmoothingMode=[System.Drawing.Drawing2D.SmoothingMode]::AntiAlias;$g.PixelOffsetMode=[System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality;$g.ScaleTransform(2,2)
    if($direction -eq 'A'){$outer=New-RoundPath 5 5 350 250 19;$inner=New-RoundPath 10 10 340 240 15}else{$outer=New-ChamferPath 5 5 350 250 12;$inner=New-ChamferPath 11 11 338 238 9}
    $edge=[System.Drawing.SolidBrush]::new((C $style.Edge));$g.FillPath($edge,$outer);$edge.Dispose()
    if($direction -eq 'A'){$bounds=[System.Drawing.RectangleF]::new(10,10,340,240);$body=[System.Drawing.Drawing2D.LinearGradientBrush]::new($bounds,(C $style.Top),(C $style.Bottom),[System.Drawing.Drawing2D.LinearGradientMode]::Vertical)}else{$body=[System.Drawing.SolidBrush]::new((C $style.Face))}
    $g.FillPath($body,$inner);$body.Dispose()
    $pen=[System.Drawing.Pen]::new((C $style.Highlight),1);$g.DrawLine($pen,31,17,329,17);$pen.Dispose()
    $outer.Dispose();$inner.Dispose();$g.Dispose();$bmp.Save((Join-Path $pngRoot "$folder\$file.png"),[System.Drawing.Imaging.ImageFormat]::Png);$bmp.Dispose()
}

$styles = @{
 A = @{
   'button-primary-default'=@{Edge='#554538';Top='#905B3C';Bottom='#835035';Face='#8A563A';Highlight='#C9A078';Lower='#594333';Marker='#F2E7CC';Stroke='1.4'}
   'button-primary-pressed'=@{Edge='#4B3C32';Top='#835139';Bottom='#774B35';Face='#7E5037';Highlight='#B98B69';Lower='#4B392F';Marker='#F2E7CC';Stroke='1.4'}
   'button-primary-disabled'=@{Edge='#40473F';Top='#555D53';Bottom='#4C554B';Face='#535B51';Highlight='#7B8478';Lower='#3F483F';Marker='#C9CBBE';Stroke='1.3'}
   'button-secondary-default'=@{Edge='#314334';Top='#4C6047';Bottom='#3F533E';Face='#455940';Highlight='#9DA88B';Lower='#354638';Marker='#F2EBD8';Stroke='1.35'}
   'button-secondary-selected'=@{Edge='#3B4E36';Top='#5E704B';Bottom='#556647';Face='#5A6D4A';Highlight='#A7B38A';Lower='#45563C';Marker='#F2EBD8';Stroke='1.45'}
   'button-secondary-pressed'=@{Edge='#2C3B31';Top='#3B4E3B';Bottom='#334535';Face='#374A38';Highlight='#78886D';Lower='#2B3C32';Marker='#E3E6D4';Stroke='1.35'}
   'button-secondary-disabled'=@{Edge='#404940';Top='#525A51';Bottom='#4B544B';Face='#4F584E';Highlight='#7A8278';Lower='#3F4840';Marker='#C9CBBE';Stroke='1.3'}
   'dialog-frame'=@{Edge='#1F3228';Top='#34483A';Bottom='#2C4135';Face='#304437';Highlight='#8C9877';Lower='#22372C';Marker='#F2EBD8';Stroke='1.4'}
 }
 B = @{
   'button-primary-default'=@{Edge='#594538';Top='#945C3D';Bottom='#945C3D';Face='#945C3D';Highlight='#CF9C75';Lower='#6B4634';Marker='#F2E7CC';Stroke='1.8'}
   'button-primary-pressed'=@{Edge='#4A392F';Top='#7F4D35';Bottom='#7F4D35';Face='#7F4D35';Highlight='#AD7959';Lower='#563A30';Marker='#F2E7CC';Stroke='1.8'}
   'button-primary-disabled'=@{Edge='#40473F';Top='#545C52';Bottom='#545C52';Face='#545C52';Highlight='#7A8376';Lower='#40473F';Marker='#C9CBBE';Stroke='1.7'}
   'button-secondary-default'=@{Edge='#2B3C31';Top='#394D3E';Bottom='#394D3E';Face='#394D3E';Highlight='#82917B';Lower='#2D4034';Marker='#F2EBD8';Stroke='1.8'}
   'button-secondary-selected'=@{Edge='#3B5038';Top='#5A704F';Bottom='#5A704F';Face='#5A704F';Highlight='#A9B78C';Lower='#43583D';Marker='#F2EBD8';Stroke='1.8'}
   'button-secondary-pressed'=@{Edge='#26372E';Top='#304235';Bottom='#304235';Face='#304235';Highlight='#6E806C';Lower='#26392E';Marker='#E3E6D4';Stroke='1.8'}
   'button-secondary-disabled'=@{Edge='#3F4840';Top='#4E574F';Bottom='#4E574F';Face='#4E574F';Highlight='#768075';Lower='#3F4840';Marker='#C9CBBE';Stroke='1.7'}
   'dialog-frame'=@{Edge='#1E3027';Top='#2B3D32';Bottom='#2B3D32';Face='#2B3D32';Highlight='#71806E';Lower='#1E3027';Marker='#F2EBD8';Stroke='1.8'}
 }
}

foreach($direction in @('A','B')){
    foreach($name in @('button-primary-default','button-primary-pressed','button-primary-disabled','button-secondary-default','button-secondary-selected','button-secondary-pressed','button-secondary-disabled')){
        $state=$styles[$direction][$name]
        $offset=if($name -match 'pressed'){3}else{0}
        $selected=$name -eq 'button-secondary-selected'
        Write-ButtonSvg $name $direction $state $selected $offset
        Export-ButtonPng $name $direction $state $selected $offset
    }
    Write-Dialog $direction $styles[$direction]['dialog-frame']
}

$manifest=@{
    source='assets/svg/{a-soft-forest,b-refined-utility}'
    exporter='source/export-components.ps1'
    rasterScale=2
    buttonCanvasPx=@(720,240)
    buttonSliceBorderPx=@{left=96;right=96;top=56;bottom=56}
    dialogCanvasPx=@(720,520)
    dialogSliceBorderPx=@{left=64;right=64;top=64;bottom=64}
    states=@('primary-default','primary-pressed','primary-disabled','secondary-default','secondary-selected','secondary-pressed','secondary-disabled','modal-frame')
    textBakedIntoAssets=$false
    verifiedInUnity=$false
}
[IO.File]::WriteAllText((Join-Path $artDir 'surface-manifest.json'),($manifest | ConvertTo-Json -Depth 5)+[Environment]::NewLine,[Text.UTF8Encoding]::new($false))
Write-Output "Exported 16 editable SVG originals and 16 raster UI surfaces under $artDir\assets"
