# ConveniencePos インストールスクリプト
# このスクリプトを管理者として実行してください

$ErrorActionPreference = "Stop"
$InstallDir = "C:\Program Files\ConveniencePos"
$DesktopPath = [Environment]::GetFolderPath("Desktop")
$StartMenuPath = "$env:APPDATA\Microsoft\Windows\Start Menu\Programs"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  ConveniencePos - コンビニPOSシステム  " -ForegroundColor Cyan
Write-Host "  インストーラ v1.0.0                   " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# 1. アプリケーションフォルダ作成
Write-Host "[1/4] インストールフォルダを作成中..." -ForegroundColor Yellow
if (Test-Path $InstallDir) {
    Write-Host "  既存のフォルダを削除中..." -ForegroundColor Gray
    Remove-Item -Path $InstallDir -Recurse -Force
}
New-Item -ItemType Directory -Path $InstallDir -Force | Out-Null
Write-Host "  OK: $InstallDir" -ForegroundColor Green

# 2. ファイルコピー
Write-Host "[2/4] ファイルをコピー中..." -ForegroundColor Yellow
$SourceDir = Join-Path $PSScriptRoot "app"
if (-not (Test-Path $SourceDir)) {
    $SourceDir = $PSScriptRoot
}
Copy-Item -Path "$SourceDir\*" -Destination $InstallDir -Recurse -Force
$FileCount = (Get-ChildItem $InstallDir -Recurse -File).Count
Write-Host "  OK: $FileCount ファイルをコピーしました" -ForegroundColor Green

# 3. ショートカット作成
Write-Host "[3/4] ショートカットを作成中..." -ForegroundColor Yellow

# スタートメニュー
$WScriptShell = New-Object -ComObject WScript.Shell
$SMShortcut = $WScriptShell.CreateShortcut("$StartMenuPath\ConveniencePos.lnk")
$SMShortcut.TargetPath = "$InstallDir\ConveniencePos.exe"
$SMShortcut.WorkingDirectory = $InstallDir
$SMShortcut.Description = "コンビニPOSシステム"
$SMShortcut.Save()
Write-Host "  OK: スタートメニュー" -ForegroundColor Green

# デスクトップ
$DeskShortcut = $WScriptShell.CreateShortcut("$DesktopPath\ConveniencePos.lnk")
$DeskShortcut.TargetPath = "$InstallDir\ConveniencePos.exe"
$DeskShortcut.WorkingDirectory = $InstallDir
$DeskShortcut.Description = "コンビニPOSシステム"
$DeskShortcut.Save()
Write-Host "  OK: デスクトップ" -ForegroundColor Green

# 4. アンインストール情報保存
Write-Host "[4/4] アンインストール情報を保存中..." -ForegroundColor Yellow
$UninstallKey = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\ConveniencePos"
New-Item -Path $UninstallKey -Force | Out-Null
Set-ItemProperty -Path $UninstallKey -Name "DisplayName" -Value "ConveniencePos - コンビニPOSシステム"
Set-ItemProperty -Path $UninstallKey -Name "DisplayVersion" -Value "1.0.0"
Set-ItemProperty -Path $UninstallKey -Name "Publisher" -Value "ConveniencePos Inc."
Set-ItemProperty -Path $UninstallKey -Name "InstallLocation" -Value $InstallDir
Set-ItemProperty -Path $UninstallKey -Name "UninstallString" -Value "powershell -ExecutionPolicy Bypass -File `"$InstallDir\Uninstall.ps1`""
Set-ItemProperty -Path $UninstallKey -Name "NoModify" -Value 1
Set-ItemProperty -Path $UninstallKey -Name "NoRepair" -Value 1
Write-Host "  OK" -ForegroundColor Green

# アンインストールスクリプト生成
$UninstallScript = @"
`$ErrorActionPreference = "Stop"
Write-Host "ConveniencePos をアンインストールしています..."
`$InstallDir = "C:\Program Files\ConveniencePos"
Remove-Item -Path "$StartMenuPath\ConveniencePos.lnk" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "$DesktopPath\ConveniencePos.lnk" -Force -ErrorAction SilentlyContinue
Remove-Item -Path "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\ConveniencePos" -Force -ErrorAction SilentlyContinue
Remove-Item -Path `$InstallDir -Recurse -Force
Write-Host "アンインストールが完了しました。"
"@
$UninstallScript | Out-File -FilePath "$InstallDir\Uninstall.ps1" -Encoding UTF8

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  インストール完了！" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "インストール先: $InstallDir" -ForegroundColor Cyan
Write-Host "デスクトップにショートカットを作成しました" -ForegroundColor Cyan
Write-Host ""
Write-Host "起動するにはデスクトップの ConveniencePos をダブルクリックしてください。" -ForegroundColor White
