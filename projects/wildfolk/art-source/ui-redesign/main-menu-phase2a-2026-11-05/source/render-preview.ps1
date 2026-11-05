param([switch]$IncludeStateStudy)

$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$sourceDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$artDir = Split-Path -Parent $sourceDir
$repoRoot = (Resolve-Path (Join-Path $sourceDir '..\..\..\..\..\..')).Path
$previewDir = Join-Path $repoRoot 'projects\wildfolk\validation\ui-redesign-phase2a-main-menu-2026-11-05\previews'
$genDir = Join-Path $artDir 'assets\generated'
$reuseDir = Join-Path $artDir 'assets\reused'

$fontCollection = New-Object System.Drawing.Text.PrivateFontCollection
$fontCollection.AddFontFile((Join-Path $env:WINDIR 'Fonts\NotoSansKR-VF.ttf'))
$fontFamily = $fontCollection.Families[0]

$imgBackground = [System.Drawing.Bitmap]::FromFile((Join-Path $genDir 'background-cleanup-concept.png'))
$imgPrimary = [System.Drawing.Bitmap]::FromFile((Join-Path $genDir 'button-primary-9slice.png'))
$imgSecondary = [System.Drawing.Bitmap]::FromFile((Join-Path $genDir 'button-secondary-9slice.png'))
$imgDialog = [System.Drawing.Bitmap]::FromFile((Join-Path $genDir 'dialog-frame-9slice.png'))
$imgLogo = [System.Drawing.Bitmap]::FromFile((Join-Path $reuseDir 'lowfire_wordmark.png'))
$imgPlay = [System.Drawing.Bitmap]::FromFile((Join-Path $reuseDir 'ui_play.png'))
$imgUsers = [System.Drawing.Bitmap]::FromFile((Join-Path $reuseDir 'ui_users.png'))
$imgSettings = [System.Drawing.Bitmap]::FromFile((Join-Path $reuseDir 'ui_settings.png'))
$imgProfile = [System.Drawing.Bitmap]::FromFile((Join-Path $reuseDir 'ui_profile.png'))
$imgAward = [System.Drawing.Bitmap]::FromFile((Join-Path $reuseDir 'ui_award.png'))
$imgCalendar = [System.Drawing.Bitmap]::FromFile((Join-Path $reuseDir 'ui_calendar.png'))
$imgGem = [System.Drawing.Bitmap]::FromFile((Join-Path $reuseDir 'journal_gem.png'))

function New-Canvas([int]$width, [int]$height, [bool]$transparent = $false) {
    $bmp = [System.Drawing.Bitmap]::new($width, $height, [System.Drawing.Imaging.PixelFormat]::Format32bppArgb)
    $g = [System.Drawing.Graphics]::FromImage($bmp)
    $g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
    $g.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
    $g.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
    $g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::AntiAliasGridFit
    if ($transparent) { $g.Clear([System.Drawing.Color]::Transparent) }
    return @($bmp, $g)
}

function New-Font([float]$size, [bool]$bold = $false) {
    $style = if ($bold) { [System.Drawing.FontStyle]::Bold } else { [System.Drawing.FontStyle]::Regular }
    return [System.Drawing.Font]::new($fontFamily, $size, $style, [System.Drawing.GraphicsUnit]::Pixel)
}

function New-Brush([int]$r, [int]$green, [int]$b, [int]$a = 255) {
    return [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb($a, $r, $green, $b))
}

function Draw-Cover($g, $image, [int]$x, [int]$y, [int]$width, [int]$height) {
    $sourceAspect = $image.Width / [double]$image.Height
    $targetAspect = $width / [double]$height
    if ($sourceAspect -gt $targetAspect) {
        $cropW = [int]($image.Height * $targetAspect)
        $cropX = [int](($image.Width - $cropW) / 2)
        $src = [System.Drawing.RectangleF]::new($cropX, 0, $cropW, $image.Height)
    } else {
        $cropH = [int]($image.Width / $targetAspect)
        $cropY = [int](($image.Height - $cropH) * 0.52)
        $src = [System.Drawing.RectangleF]::new(0, $cropY, $image.Width, $cropH)
    }
    $dst = [System.Drawing.RectangleF]::new($x, $y, $width, $height)
    $g.DrawImage($image, $dst, $src, [System.Drawing.GraphicsUnit]::Pixel)
}

function Draw-Fit($g, $image, [float]$x, [float]$y, [float]$width, [float]$height, [float]$alpha = 1.0) {
    $scale = [Math]::Min($width / $image.Width, $height / $image.Height)
    $drawW = $image.Width * $scale
    $drawH = $image.Height * $scale
    $rect = [System.Drawing.RectangleF]::new($x + ($width - $drawW) / 2, $y + ($height - $drawH) / 2, $drawW, $drawH)
    if ($alpha -ge 0.999) { $g.DrawImage($image, $rect); return }
    $matrix = [System.Drawing.Imaging.ColorMatrix]::new()
    $matrix.Matrix00 = 1; $matrix.Matrix11 = 1; $matrix.Matrix22 = 1; $matrix.Matrix33 = $alpha; $matrix.Matrix44 = 1
    $attrs = [System.Drawing.Imaging.ImageAttributes]::new()
    $attrs.SetColorMatrix($matrix)
    $destInt = [System.Drawing.Rectangle]::new([int][Math]::Round($rect.X), [int][Math]::Round($rect.Y), [Math]::Max(1,[int][Math]::Round($rect.Width)), [Math]::Max(1,[int][Math]::Round($rect.Height)))
    $g.DrawImage($image, $destInt, 0, 0, $image.Width, $image.Height, [System.Drawing.GraphicsUnit]::Pixel, $attrs)
    $attrs.Dispose()
}

function Draw-NineSlice($g, $image, [float]$x, [float]$y, [float]$width, [float]$height, [float]$sliceX, [float]$sliceY, [float]$alpha = 1.0) {
    $sx = [Math]::Min($sliceX, $image.Width / 3); $sy = [Math]::Min($sliceY, $image.Height / 3)
    $scale = [Math]::Min($height / $image.Height, $width / $image.Width)
    $dl = [int][Math]::Round($sx * $scale); $dr = $dl
    $dt = [int][Math]::Round($sy * $scale); $db = $dt
    $x0 = [int][Math]::Round($x); $y0 = [int][Math]::Round($y)
    $x3 = [int][Math]::Round($x + $width); $y3 = [int][Math]::Round($y + $height)
    if (($dl + $dr) -gt (($x3 - $x0) * 0.48)) { $dl = [int](($x3 - $x0) * 0.24); $dr = $dl }
    if (($dt + $db) -gt (($y3 - $y0) * 0.48)) { $dt = [int](($y3 - $y0) * 0.24); $db = $dt }
    $srcX = @(0, [int][Math]::Round($sx), [int]($image.Width - [Math]::Round($sx)))
    $srcY = @(0, [int][Math]::Round($sy), [int]($image.Height - [Math]::Round($sy)))
    $srcW = @($srcX[1], ($image.Width - 2 * $srcX[1]), $srcX[1])
    $srcH = @($srcY[1], ($image.Height - 2 * $srcY[1]), $srcY[1])
    $dstX = @($x0, ($x0 + $dl), ($x3 - $dr)); $dstY = @($y0, ($y0 + $dt), ($y3 - $db))
    $dstW = @($dl, ($x3 - $x0 - $dl - $dr), $dr); $dstH = @($dt, ($y3 - $y0 - $dt - $db), $db)
    $attrs = $null
    if ($alpha -lt 0.999) {
        $matrix = [System.Drawing.Imaging.ColorMatrix]::new()
        $matrix.Matrix00 = 1; $matrix.Matrix11 = 1; $matrix.Matrix22 = 1; $matrix.Matrix33 = $alpha; $matrix.Matrix44 = 1
        $attrs = [System.Drawing.Imaging.ImageAttributes]::new()
        $attrs.SetColorMatrix($matrix)
    }
    for ($row = 0; $row -lt 3; $row++) {
        for ($col = 0; $col -lt 3; $col++) {
            $leftOverlap = if ($col -gt 0) { 1 } else { 0 }
            $rightOverlap = if ($col -lt 2) { 1 } else { 0 }
            $topOverlap = if ($row -gt 0) { 1 } else { 0 }
            $bottomOverlap = if ($row -lt 2) { 1 } else { 0 }
            $src = [System.Drawing.Rectangle]::new([int]$srcX[$col], [int]$srcY[$row], [Math]::Max(1,[int]$srcW[$col]), [Math]::Max(1,[int]$srcH[$row]))
            $dst = [System.Drawing.Rectangle]::new([int]($dstX[$col] - $leftOverlap), [int]($dstY[$row] - $topOverlap), [Math]::Max(1,[int]($dstW[$col] + $leftOverlap + $rightOverlap)), [Math]::Max(1,[int]($dstH[$row] + $topOverlap + $bottomOverlap)))
            if ($attrs) {
                $g.DrawImage($image, $dst, $src.X, $src.Y, $src.Width, $src.Height, [System.Drawing.GraphicsUnit]::Pixel, $attrs)
            } else {
                $g.DrawImage($image, $dst, $src, [System.Drawing.GraphicsUnit]::Pixel)
            }
        }
    }
    if ($attrs) { $attrs.Dispose() }
}
function Draw-Text($g, [string]$text, [float]$x, [float]$y, [float]$width, [float]$height, [float]$size, [bool]$bold = $false, [string]$align = 'Near', [int]$r = 244, [int]$green = 239, [int]$b = 220, [bool]$shadow = $true) {
    $font = New-Font $size $bold
    $format = [System.Drawing.StringFormat]::new()
    if ($align -eq 'Center') { $format.Alignment = [System.Drawing.StringAlignment]::Center }
    elseif ($align -eq 'Far') { $format.Alignment = [System.Drawing.StringAlignment]::Far }
    else { $format.Alignment = [System.Drawing.StringAlignment]::Near }
    $format.LineAlignment = [System.Drawing.StringAlignment]::Center
    $format.Trimming = [System.Drawing.StringTrimming]::EllipsisCharacter
    $rect = [System.Drawing.RectangleF]::new($x, $y, $width, $height)
    if ($shadow) {
        $shadowBrush = New-Brush 14 22 18 170
        $shadowRect = [System.Drawing.RectangleF]::new($x + 1.5, $y + 2, $width, $height)
        $g.DrawString($text, $font, $shadowBrush, $shadowRect, $format)
        $shadowBrush.Dispose()
    }
    $brush = New-Brush $r $green $b
    $g.DrawString($text, $font, $brush, $rect, $format)
    $brush.Dispose(); $font.Dispose(); $format.Dispose()
}

function Draw-ShelfItem($g, [string]$label, $icon, [float]$x, [float]$y, [float]$width, [float]$height, [float]$scale, [bool]$mobile) {
    if ($mobile) {
        $iconSize = 22
        $iconX = $x + ($width - $iconSize) / 2
        Draw-Fit $g $icon $iconX ($y + 8) $iconSize $iconSize
        Draw-Text $g $label ($x + 4) ($y + 37) ($width - 8) 32 13 $true 'Center' 244 239 220 $true
        return
    }
    $iconSize = [Math]::Round(34 * $scale)
    $fontSize = 21 * $scale
    $font = New-Font $fontSize $true
    $measure = $g.MeasureString($label, $font).Width
    $font.Dispose()
    $totalWidth = $iconSize + (8 * $scale) + $measure
    $startX = $x + ($width - $totalWidth) / 2
    $iy = $y + ($height - $iconSize) / 2
    Draw-Fit $g $icon $startX $iy $iconSize $iconSize
    $startX += $iconSize + (8 * $scale)
    Draw-Text $g $label $startX $y ($x + $width - $startX - 3) $height $fontSize $true 'Near' 244 239 220 $true
}
function Render-Main([int]$width, [int]$height, [string]$language, [bool]$saveExists, [bool]$confirmDialog = $false) {
    $pair = New-Canvas $width $height $false
    $bmp = $pair[0]; $g = $pair[1]
    Draw-Cover $g $imgBackground 0 0 $width $height
    $mobile = $height -le 400
    $scale = $width / 1920.0
    $ko = $language -eq 'ko'

    if ($ko) {
        $camp = '야영지에서'; $continue = '이어하기'; $single = '싱글 플레이'; $multi = '멀티플레이'
        $newAction = '새 세계 시작'; $noSave = '저장된 세계 없음 · 싱글 플레이로 시작'
        $continueHint = '저장된 세계를 이어갑니다'; $singleHint = '새 세계 시작'; $multiHint = '호스트 또는 참가'
        $class = '직업'; $settings = '설정'; $profile = '프로필'
        $gemStore = '보석 상점'; $achievements = '업적'; $attendance = '출석'
        $confirmTitle = '새로 시작할까요?'
        $nl = [Environment]::NewLine
        $confirmBody = '새 세계를 저장하면 현재 세계가 교체됩니다.' + $nl + '이후 이어하기에서는 이전 세계를 불러올 수 없습니다.'
        $cancel = '취소'; $confirm = '새 세계 시작'; $tagline = '생존 · 탐험 · 제작'
    } else {
        $camp = 'At the camp'; $continue = 'Continue'; $single = 'Single Player'; $multi = 'Multiplayer'
        $newAction = 'Start new world'; $noSave = 'No saved world · Start Single Player'
        $continueHint = 'Continue your saved world'; $singleHint = 'Start a new world'; $multiHint = 'Host or join'
        $class = 'Class'; $settings = 'Settings'; $profile = 'Profile'
        $gemStore = 'Gem Store'; $achievements = 'Achievements'; $attendance = 'Attendance'
        $confirmTitle = 'Begin again?'
        $nl = [Environment]::NewLine
        $confirmBody = 'A new world will replace your current world when saved.' + $nl + 'Your previous world will no longer be available through Continue.'
        $cancel = 'Cancel'; $confirm = 'Start new world'; $tagline = 'SURVIVE · EXPLORE · BUILD'
    }

    if ($mobile) {
        $logoX = 24; $logoY = 20; $logoW = 166; $logoH = 50
        Draw-Fit $g $imgLogo $logoX $logoY $logoW $logoH
        Draw-Text $g $tagline 24 72 320 22 12 $true 'Near' 244 239 220 $true
        $mx = 425; $mw = $width - $mx - 24
        Draw-Text $g $camp $mx 12 $mw 28 20 $true 'Near' 244 239 220 $true
        $primaryY = 48; $primaryH = 57; $pairY = 112; $pairH = 51; $actionY = 171; $actionH = 48
        $footerX = $mx; $footerY = 273; $footerW = $mw; $footerH = 79
        $titleSize = 17; $subSize = 11; $secSize = 14; $railFont = 13; $mainIcon = 27; $secIcon = 20
    } else {
        if ($width -ge 1800) {
            $logoX = 74; $logoY = 70; $logoW = 390; $logoH = 118; $tagY = 197; $tagH = 34; $tagSize = 22
            $mx = 1010; $mw = 840; $headY = 140; $headH = 54; $primaryY = 204; $primaryH = 122
            $pairY = 345; $pairH = 105; $actionY = 468; $actionH = 84
            $footerX = 70; $footerY = 875; $footerW = 850; $footerH = 140
            $titleSize = 32; $subSize = 19; $secSize = 24; $railFont = 21; $mainIcon = 44; $secIcon = 31
        } else {
            $logoX = 52; $logoY = 38; $logoW = 285; $logoH = 86; $tagY = 130; $tagH = 28; $tagSize = 17
            $mx = 660; $mw = 570; $headY = 74; $headH = 42; $primaryY = 126; $primaryH = 94
            $pairY = 235; $pairH = 78; $actionY = 329; $actionH = 66
            $footerX = 50; $footerY = 580; $footerW = 545; $footerH = 96
            $titleSize = 24; $subSize = 15; $secSize = 19; $railFont = 17; $mainIcon = 35; $secIcon = 24
        }
        Draw-Fit $g $imgLogo $logoX $logoY $logoW $logoH
        Draw-Text $g $tagline ($logoX + 4) $tagY $logoW $tagH $tagSize $true 'Near' 244 239 220 $true
        Draw-Text $g $camp $mx $headY $mw $headH ([Math]::Round(30 * $scale)) $true 'Near' 244 239 220 $true
    }

    $primaryText = if ($saveExists) { $continue } else { $newAction }
    $primarySub = if ($saveExists) { $continueHint } else { $noSave }
    Draw-NineSlice $g $imgPrimary $mx $primaryY $mw $primaryH 300 78
    $ix = $mx + (24 * $scale); if ($mobile) { $ix = $mx + 17 }
    $iy = $primaryY + ($primaryH - $mainIcon) / 2
    Draw-Fit $g $imgPlay $ix $iy $mainIcon $mainIcon
    $textX = $ix + $mainIcon + (17 * $scale); if ($mobile) { $textX = $ix + $mainIcon + 10 }
    $textW = $mx + $mw - $textX - (20 * $scale)
    if ($mobile) {
        Draw-Text $g $primaryText $textX ($primaryY + 4) $textW 27 $titleSize $true 'Near' 250 244 227 $true
        Draw-Text $g $primarySub $textX ($primaryY + 31) $textW 18 $subSize $false 'Near' 244 235 211 $true
    } else {
        Draw-Text $g $primaryText $textX ($primaryY + 16 * $scale) $textW (46 * $scale) $titleSize $true 'Near' 250 244 227 $true
        Draw-Text $g $primarySub $textX ($primaryY + 65 * $scale) $textW (34 * $scale) $subSize $false 'Near' 244 235 211 $true
    }

    if ($saveExists) {
        $gap = if ($mobile) { 9 } else { 14 * $scale }
        $bw = ($mw - $gap) / 2
        $items = @(
            @{ Label = $single; Hint = $singleHint; Icon = $imgPlay },
            @{ Label = $multi; Hint = $multiHint; Icon = $imgUsers }
        )
        for ($i = 0; $i -lt 2; $i++) {
            $bx = $mx + $i * ($bw + $gap)
            Draw-NineSlice $g $imgSecondary $bx $pairY $bw $pairH 300 72
            $iconSize = if ($mobile) { $secIcon } else { [Math]::Round(30 * $scale) }
            $iconX = $bx + (16 * $scale); if ($mobile) { $iconX = $bx + 11 }
            Draw-Fit $g $items[$i].Icon $iconX ($pairY + ($pairH - $iconSize) / 2) $iconSize $iconSize
            $tx = $iconX + $iconSize + (12 * $scale); if ($mobile) { $tx = $iconX + $iconSize + 7 }
            $tw = $bx + $bw - $tx - (12 * $scale)
            if ($mobile) {
                Draw-Text $g $items[$i].Label $tx ($pairY + 4) $tw ($pairH - 8) $secSize $true 'Near' 244 239 220 $true
            } else {
                Draw-Text $g $items[$i].Label $tx ($pairY + 10 * $scale) $tw (32 * $scale) $secSize $true 'Near' 244 239 220 $true
                Draw-Text $g $items[$i].Hint $tx ($pairY + 44 * $scale) $tw (24 * $scale) $subSize $false 'Near' 213 222 198 $true
            }
        }
    } else {
        Draw-NineSlice $g $imgSecondary $mx $pairY $mw $pairH 300 72
        $iconSize = if ($mobile) { $secIcon } else { [Math]::Round(30 * $scale) }
        $iconX = $mx + (22 * $scale); if ($mobile) { $iconX = $mx + 15 }
        Draw-Fit $g $imgUsers $iconX ($pairY + ($pairH - $iconSize) / 2) $iconSize $iconSize
        $tx = $iconX + $iconSize + (14 * $scale)
        Draw-Text $g $multi $tx ($pairY + 7 * $scale) ($mw - ($tx - $mx) - 18) (32 * $scale) $secSize $true 'Near' 244 239 220 $true
        if (-not $mobile) { Draw-Text $g $multiHint $tx ($pairY + 42 * $scale) ($mw - ($tx - $mx) - 18) (24 * $scale) $subSize $false 'Near' 213 222 198 $true }
    }

    Draw-NineSlice $g $imgSecondary $mx $actionY $mw $actionH 300 72
    $colW = $mw / 3
    $actionItems = @(
        @{ Label = $class; Icon = $null },
        @{ Label = $settings; Icon = $imgSettings },
        @{ Label = $profile; Icon = $imgProfile }
    )
    for ($i = 0; $i -lt 3; $i++) {
        $cx = $mx + $i * $colW; $icon = $actionItems[$i].Icon; $label = $actionItems[$i].Label
        if ($icon) {
            $iconSize = if ($mobile) { 19 } else { [Math]::Round(28 * $scale) }
            $groupW = $iconSize + (9 * $scale) + ($label.Length * $railFont * 0.56)
            $gx = $cx + ($colW - $groupW) / 2
            Draw-Fit $g $icon $gx ($actionY + ($actionH - $iconSize) / 2) $iconSize $iconSize
            Draw-Text $g $label ($gx + $iconSize + 8 * $scale) $actionY ($colW - ($gx - $cx) - $iconSize - 10 * $scale) $actionH $railFont $true 'Near' 244 239 220 $true
        } else {
            Draw-Text $g $label $cx $actionY $colW $actionH $railFont $true 'Center' 244 239 220 $true
        }
    }

    Draw-NineSlice $g $imgSecondary $footerX $footerY $footerW $footerH 300 72
    $fw = $footerW / 3
    Draw-ShelfItem $g $gemStore $imgGem $footerX $footerY $fw $footerH $scale $mobile
    Draw-ShelfItem $g $achievements $imgAward ($footerX + $fw) $footerY $fw $footerH $scale $mobile
    Draw-ShelfItem $g $attendance $imgCalendar ($footerX + 2 * $fw) $footerY $fw $footerH $scale $mobile

    if ($confirmDialog) {
        $veil = New-Brush 6 14 12 148
        $g.FillRectangle($veil, 0, 0, $width, $height); $veil.Dispose()
        if ($mobile) { $dw = 666; $dh = 244 }
        else { $dw = if ($width -ge 1800) { 820 } else { 720 }; $dh = if ($width -ge 1800) { 400 } else { 350 } }
        $dx = [int](($width - $dw) / 2); $dy = [int](($height - $dh) / 2)
        Draw-NineSlice $g $imgDialog $dx $dy $dw $dh 48 40
        if ($mobile) {
            $pad = 38; $headSize = 22; $bodySize = 15; $btnH = 55; $btnGap = 14
            $titleY = $dy + 28; $titleBoxH = 36; $bodyY = $dy + 70; $bodyBoxH = 92; $bottomGap = 15
        } elseif ($width -ge 1800) {
            $pad = 68; $headSize = 32; $bodySize = 20; $btnH = 74; $btnGap = 20
            $titleY = $dy + 38; $titleBoxH = 46; $bodyY = $dy + 94; $bodyBoxH = 84; $bottomGap = 28
        } else {
            $pad = 48; $headSize = 26; $bodySize = 18; $btnH = 60; $btnGap = 16
            $titleY = $dy + 30; $titleBoxH = 38; $bodyY = $dy + 74; $bodyBoxH = 76; $bottomGap = 20
        }
        $btnW = ($dw - 2 * $pad - $btnGap) / 2
        Draw-Text $g $confirmTitle ($dx + $pad) $titleY ($dw - 2 * $pad) $titleBoxH $headSize $true 'Near' 250 244 227 $true
        Draw-Text $g $confirmBody ($dx + $pad) $bodyY ($dw - 2 * $pad) $bodyBoxH $bodySize $false 'Near' 231 235 219 $true
        $by = $dy + $dh - $btnH - $bottomGap
        $cancelX = $dx + $pad; $confirmX = $cancelX + $btnW + $btnGap
        Draw-NineSlice $g $imgSecondary $cancelX $by $btnW $btnH 300 72
        Draw-NineSlice $g $imgPrimary $confirmX $by $btnW $btnH 300 78
        $dialogButtonFont = if ($mobile) { 16 } else { 22 * $scale }
        Draw-Text $g $cancel $cancelX $by $btnW $btnH $dialogButtonFont $true 'Center' 244 239 220 $true
        Draw-Text $g $confirm $confirmX $by $btnW $btnH $dialogButtonFont $true 'Center' 250 244 227 $true
    }

    $suffix = $width.ToString() + 'x' + $height.ToString()
    if ($confirmDialog) { $outName = "main-menu-h02-$language-$suffix.png" }
    elseif (-not $saveExists) { $outName = "main-menu-h01-$language-no-save-$suffix.png" }
    else { $outName = "main-menu-h01-$language-$suffix.png" }
    $outPath = Join-Path $previewDir $outName
    $bmp.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
    Write-Output $outPath
}

function Render-StateStudy() {
    $width = 1920; $height = 540
    $pair = New-Canvas $width $height $false; $bmp = $pair[0]; $g = $pair[1]
    Draw-Cover $g $imgBackground 0 0 $width $height
    $veil = New-Brush 18 35 28 162
    $g.FillRectangle($veil, 0, 0, $width, $height); $veil.Dispose()
    $states = @(
        @{ Label = '기본'; Image = $imgSecondary; Alpha = 1.0; Pressed = $false },
        @{ Label = '선택'; Image = $imgPrimary; Alpha = 1.0; Pressed = $false },
        @{ Label = '눌림'; Image = $imgPrimary; Alpha = 0.82; Pressed = $true },
        @{ Label = '비활성 견본'; Image = $imgSecondary; Alpha = 0.46; Pressed = $false }
    )
    $cardW = 408; $gap = 42; $left = 264; $y = 195; $h = 116
    for ($i = 0; $i -lt 4; $i++) {
        $x = $left + $i * ($cardW + $gap)
        Draw-Text $g $states[$i].Label $x 118 $cardW 48 27 $true 'Center' 244 239 220 $true
        $yy = $y + $(if ($states[$i].Pressed) { 5 } else { 0 })
        Draw-NineSlice $g $states[$i].Image $x $yy $cardW $h 300 72 $states[$i].Alpha
    }
    $outPath = Join-Path $previewDir 'button-state-study-ko-1920x540.png'
    $bmp.Save($outPath, [System.Drawing.Imaging.ImageFormat]::Png)
    $g.Dispose(); $bmp.Dispose()
    Write-Output $outPath
}

New-Item -ItemType Directory -Force -Path $previewDir | Out-Null
$viewports = @(@(1920,1080), @(1280,720), @(844,390))
foreach ($vp in $viewports) {
    foreach ($lang in @('ko','en')) {
        Render-Main $vp[0] $vp[1] $lang $true $false
        Render-Main $vp[0] $vp[1] $lang $true $true
        Render-Main $vp[0] $vp[1] $lang $false $false
    }
}
if ($IncludeStateStudy) { Render-StateStudy }

foreach ($image in @($imgBackground,$imgPrimary,$imgSecondary,$imgDialog,$imgLogo,$imgPlay,$imgUsers,$imgSettings,$imgProfile,$imgAward,$imgCalendar,$imgGem)) { $image.Dispose() }
