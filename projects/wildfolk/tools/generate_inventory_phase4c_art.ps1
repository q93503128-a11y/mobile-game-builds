[CmdletBinding()]
param(
    [string]$OutputDirectory
)

$ErrorActionPreference = 'Stop'
if ([string]::IsNullOrWhiteSpace($OutputDirectory)) {
    $OutputDirectory = Join-Path $PSScriptRoot '..\client\Assets\Wildfolk\Art'
}
Add-Type -AssemblyName System.Drawing
Add-Type -Path (Join-Path $PSScriptRoot 'InventoryUiArtGenerator.cs') -ReferencedAssemblies 'System.Drawing.dll'
$resolvedOutput = [System.IO.Path]::GetFullPath($OutputDirectory)
[Wildfolk.Tools.InventoryUiArtGenerator]::Generate($resolvedOutput)
Write-Output "Wrote Phase 4C inventory surfaces to $resolvedOutput"
