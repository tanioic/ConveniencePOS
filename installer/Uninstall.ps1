# ConveniencePos アンインストールスクリプト

$ErrorActionPreference = "Stop"
$InstallDir = "C:\Program Files\ConveniencePos"
$DesktopPath = [Environment]::GetFolderPath("Desktop")
$StartMenuPath = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs"

Write-Host "ConveniencePos をアンインストールしています..." -ForegroundColor Yellow

# ショートカット削除
Remove-Item -Path "$StartMenuPath\ConveniencePos.lnk" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "$DesktopPath\ConveniencePos.lnk" -Force -ErrorAction SilentlyContinue

# レジストリ削除
Remove-Item -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\ConveniencePos" -Force -ErrorAction SilentlyContinue

# ファイル削除
Remove-Item -Path $InstallDir -Recurse -Force

Write-Host "アンインストールが完了しました。" -ForegroundColor Green
