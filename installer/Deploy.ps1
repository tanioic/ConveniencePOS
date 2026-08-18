<# 
  ConveniencePos デプロイスクリプト (ZIP版)
  使用方法: .\Deploy.ps1
#>

$ErrorActionPreference = "Stop"
$Version = "1.0.0"
$OutputDir = Join-Path $PSScriptRoot "output"
$ZipFile = Join-Path $OutputDir "ConveniencePos-v$Version.zip"

Write-Host "ConveniencePos v$Version をデプロイ中..." -ForegroundColor Cyan

# publish実行
Write-Host "[1/3] ビルド＆publish中..." -ForegroundColor Yellow
dotnet publish ConveniencePos/ConveniencePos.csproj -c Release -r win-x64 --self-contained false -o "$OutputDir\app" --verbosity minimal
if ($LASTEXITCODE -ne 0) { throw "ビルド失敗" }

# インストール/アンインストールスクリプトコピー
Copy-Item "installer\Install.ps1" -Destination "$OutputDir\" -Force
Copy-Item "installer\Uninstall.ps1" -Destination "$OutputDir\" -Force

# ZIP作成
Write-Host "[2/3] ZIPファイル作成中..." -ForegroundColor Yellow
if (Test-Path $ZipFile) { Remove-Item $ZipFile -Force }
Compress-Archive -Path "$OutputDir\*" -DestinationPath $ZipFile -Force
$ZipSize = [math]::Round((Get-Item $ZipFile).Length / 1MB, 2)
Write-Host "  OK: $ZipFile ($ZipSize MB)" -ForegroundColor Green

# 結果
Write-Host "[3/3] 完了" -ForegroundColor Yellow
Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "  デプロイパッケージ作成完了" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host ""
Write-Host "出力先: $ZipFile" -ForegroundColor Cyan
Write-Host ""
Write-Host "配布手順:" -ForegroundColor White
Write-Host "  1. 上記ZIPファイルを対象PCにコピー" -ForegroundColor White
Write-Host "  2. ZIPを展開" -ForegroundColor White
Write-Host "  3. Install.ps1 を管理者権限で実行" -ForegroundColor White
Write-Host ""
Write-Host "要件: .NET 8.0 Desktop Runtime, SQL Server LocalDB" -ForegroundColor Gray
