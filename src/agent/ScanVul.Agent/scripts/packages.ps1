$registryPaths = @(
    "HKLM:\Software\Microsoft\Windows\CurrentVersion\Uninstall",
    "HKLM:\Software\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall"
)

$installedPrograms = foreach ($path in $registryPaths) {
    Get-ItemProperty -Path $path\* | Select-Object DisplayName, DisplayVersion
}

# Remove duplicates and display results
$installedPrograms | Sort-Object -Property DisplayName -Unique