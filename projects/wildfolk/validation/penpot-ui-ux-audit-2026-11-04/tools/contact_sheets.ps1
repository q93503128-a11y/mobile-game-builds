$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.Drawing
$auditRoot = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$reviewRoot = Join-Path $auditRoot 'evidence/review-sheets'
[IO.Directory]::CreateDirectory($reviewRoot) | Out-Null
$images = @(Get-ChildItem (Join-Path $auditRoot 'evidence/live') -Filter '*.png' | Sort-Object Name)
$manifest = @()
for ($offset=0; $offset -lt $images.Count; $offset+=6) {
    $canvas = [Drawing.Bitmap]::new(1688,1530)
    $graphics = [Drawing.Graphics]::FromImage($canvas)
    $graphics.Clear([Drawing.Color]::FromArgb(24,24,24))
    $font = [Drawing.Font]::new('Arial',18)
    for ($tile=0; $tile -lt 6 -and $offset+$tile -lt $images.Count; $tile++) {
        $file = $images[$offset+$tile]
        $source = [Drawing.Image]::FromFile($file.FullName)
        $scale = [Math]::Min(844.0/$source.Width,475.0/$source.Height)
        $x = ($tile % 2)*844
        $y = [Math]::Floor($tile/2)*510
        $graphics.DrawString($file.Name,$font,[Drawing.Brushes]::White,[single]($x+5),[single]($y+2))
        $graphics.DrawImage($source,[single]$x,[single]($y+32),[single]($source.Width*$scale),[single]($source.Height*$scale))
        $source.Dispose()
        $manifest += [PSCustomObject]@{file=$file.Name;sheet=('sheet-{0:D2}.png' -f ([int]($offset/6)+1));tile=$tile}
    }
    $canvas.Save((Join-Path $reviewRoot ('sheet-{0:D2}.png' -f ([int]($offset/6)+1))),[Drawing.Imaging.ImageFormat]::Png)
    $font.Dispose(); $graphics.Dispose(); $canvas.Dispose()
}
$manifest | ConvertTo-Json | Set-Content (Join-Path $reviewRoot 'manifest.json') -Encoding utf8
Write-Output "Composed $($images.Count) existing screenshots for review."
