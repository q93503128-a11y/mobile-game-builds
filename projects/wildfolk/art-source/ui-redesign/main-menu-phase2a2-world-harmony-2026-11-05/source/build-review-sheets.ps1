$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$sourceDir=Split-Path -Parent $MyInvocation.MyCommand.Path
$artDir=Split-Path -Parent $sourceDir
$repoRoot=(Resolve-Path (Join-Path $sourceDir '..\..\..\..\..\..')).Path
$previewRoot=Join-Path $repoRoot 'projects\wildfolk\validation\ui-redesign-phase2a2-world-harmony-2026-11-05\previews'
$compareDir=Join-Path $previewRoot 'comparisons'
$fontCollection=[System.Drawing.Text.PrivateFontCollection]::new()
$fontCollection.AddFontFile((Join-Path $env:WINDIR 'Fonts\NotoSansKR-VF.ttf'))
$fontFamily=$fontCollection.Families[0]
New-Item -ItemType Directory -Force -Path $compareDir | Out-Null

function New-Font([float]$size,[bool]$bold=$false){
    $style=if($bold){[System.Drawing.FontStyle]::Bold}else{[System.Drawing.FontStyle]::Regular}
    return [System.Drawing.Font]::new($fontFamily,$size,$style,[System.Drawing.GraphicsUnit]::Pixel)
}
function Brush([int]$r,[int]$g,[int]$b,[int]$a=255){return [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb($a,$r,$g,$b))}
function Draw-Label($g,[string]$text,[float]$x,[float]$y,[float]$w,[float]$h,[float]$size,[bool]$bold=$false,[string]$align='Center'){
    $font=New-Font $size $bold
    $format=[System.Drawing.StringFormat]::new()
    $format.Alignment=if($align -eq 'Near'){[System.Drawing.StringAlignment]::Near}else{[System.Drawing.StringAlignment]::Center}
    $format.LineAlignment=[System.Drawing.StringAlignment]::Center
    $format.Trimming=[System.Drawing.StringTrimming]::EllipsisCharacter
    $ink=Brush 242 237 220
    $g.DrawString($text,$font,$ink,[System.Drawing.RectangleF]::new($x,$y,$w,$h),$format)
    $ink.Dispose();$font.Dispose();$format.Dispose()
}
function Build-Comparison([int]$width,[int]$height,[string]$language){
    $headerHeight=44;$rowLabelHeight=28;$gap=7
    $rows=@(
        @{Label='H01 · Saved world';Name="main-menu-h01-$language-$($width)x$($height).png"},
        @{Label='H01 · No saved world';Name="main-menu-h01-$language-no-save-$($width)x$($height).png"},
        @{Label='H02 · Replace confirmation';Name="main-menu-h02-$language-$($width)x$($height).png"}
    )
    $boardWidth=$width*2;$boardHeight=$headerHeight+($rows.Count*($rowLabelHeight+$height))+($rows.Count-1)*$gap
    $bmp=[System.Drawing.Bitmap]::new($boardWidth,$boardHeight,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g=[System.Drawing.Graphics]::FromImage($bmp);$g.SmoothingMode=[System.Drawing.Drawing2D.SmoothingMode]::HighQuality;$g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $background=Brush 22 37 29;$g.FillRectangle($background,0,0,$boardWidth,$boardHeight);$background.Dispose()
    Draw-Label $g "A · SOFT FOREST · $($language.ToUpperInvariant()) · ${width}×${height}" 0 0 $width $headerHeight 17 $true
    Draw-Label $g "B · REFINED UTILITY · $($language.ToUpperInvariant()) · ${width}×${height}" $width 0 $width $headerHeight 17 $true
    $line=[System.Drawing.Pen]::new([System.Drawing.Color]::FromArgb(130,153,163,135),1);$g.DrawLine($line,$width,0,$width,$boardHeight);$line.Dispose()
    $y=$headerHeight
    foreach($row in $rows){
        $band=Brush 39 57 45;$g.FillRectangle($band,0,$y,$boardWidth,$rowLabelHeight);$band.Dispose()
        Draw-Label $g $row.Label 14 $y ($boardWidth-28) $rowLabelHeight 14 $true 'Near'
        $y+=$rowLabelHeight
        $a=[System.Drawing.Bitmap]::FromFile((Join-Path $previewRoot "a-soft-forest\$($row.Name)"))
        $b=[System.Drawing.Bitmap]::FromFile((Join-Path $previewRoot "b-refined-utility\$($row.Name)"))
        try{$g.DrawImage($a,0,$y,$width,$height);$g.DrawImage($b,$width,$y,$width,$height)}finally{$a.Dispose();$b.Dispose()}
        $y+=$height+$gap
    }
    $out=Join-Path $compareDir "ab-$language-$($width)x$($height).png"
    $bmp.Save($out,[System.Drawing.Imaging.ImageFormat]::Png);$g.Dispose();$bmp.Dispose();Write-Output $out
}

foreach($language in @('ko','en')){
    Build-Comparison 1920 1080 $language
    Build-Comparison 1280 720 $language
    Build-Comparison 844 390 $language
}

$stateA=[System.Drawing.Bitmap]::FromFile((Join-Path $previewRoot 'a-soft-forest\button-state-study-1920x540.png'))
$stateB=[System.Drawing.Bitmap]::FromFile((Join-Path $previewRoot 'b-refined-utility\button-state-study-1920x540.png'))
$stateWidth=960;$stateHeight=270;$stateHeader=40
$board=[System.Drawing.Bitmap]::new($stateWidth*2,$stateHeight+$stateHeader,[System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
$g=[System.Drawing.Graphics]::FromImage($board);$g.SmoothingMode=[System.Drawing.Drawing2D.SmoothingMode]::HighQuality;$g.InterpolationMode=[System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$background=Brush 22 37 29;$g.FillRectangle($background,0,0,$board.Width,$board.Height);$background.Dispose()
Draw-Label $g 'A · SOFT FOREST · BUTTON STATES' 0 0 $stateWidth $stateHeader 16 $true
Draw-Label $g 'B · REFINED UTILITY · BUTTON STATES' $stateWidth 0 $stateWidth $stateHeader 16 $true
$g.DrawImage($stateA,0,$stateHeader,$stateWidth,$stateHeight);$g.DrawImage($stateB,$stateWidth,$stateHeader,$stateWidth,$stateHeight)
$out=Join-Path $compareDir 'ab-button-states.png';$board.Save($out,[System.Drawing.Imaging.ImageFormat]::Png)
$stateA.Dispose();$stateB.Dispose();$g.Dispose();$board.Dispose();Write-Output $out
