# ロールバック・障害復旧手順: コンビニPOSシステム MVP

## 1. ドキュメント概要

| 項目 | 内容 |
|------|------|
| バージョン | 1.0 |
| 作成日 | 2026-08-18 |
| 役割 | デプロイ失敗時のロールバック手順と障害復旧手順を定義する |

## 2. ロールバック手順

### 2-1. アプリケーションのロールバック

```bash
# 1. 実行中のアプリを終了
taskkill /F /IM ConveniencePos.exe

# 2. バックアップフォルダから前バージョンを復元
xcopy /E /Y "C:\Backup\ConveniencePos\v1.0" "C:\Program Files\ConveniencePos\"

# 3. アプリケーションを再起動
"C:\Program Files\ConveniencePos\ConveniencePos.exe"
```

### 2-2. データベースのロールバック

```bash
# 1. マイグレーションのロールバック（特定のバージョンまで）
dotnet ef database update <移行前のマイグレーション名> --project ConveniencePos

# 2. バックアップからの復元（重大な場合）
sqllocaldb stop MSSQLLocalDB
sqllocaldb start MSSQLLocalDB
# バックアップファイルからリストア
```

### 2-3. 設定のロールバック

```bash
# appsettings.json をバックアップから復元
copy /Y "C:\Backup\ConveniencePos\appsettings.json" "C:\Program Files\ConveniencePos\appsettings.json"
```

## 3. 障害復旧手順

### 3-1. DB接続障害

| ステップ | 操作 |
|---------|------|
| 1 | SQL Server LocalDB の起動確認: `sqllocaldb info MSSQLLocalDB` |
| 2 | 未起動の場合: `sqllocaldb start MSSQLLocalDB` |
| 3 | DB存在確認: `sqllocaldb info MSSQLLocalDB` で接続文字列を確認 |
| 4 | DBが存在しない場合: `dotnet ef database update` で再作成 |
| 5 | アプリケーション再起動 |

### 3-2. アプリケーションクラッシュ

| ステップ | 操作 |
|---------|------|
| 1 | エラーログの確認（コンソール出力 / ログファイル） |
| 2 | 異常プロセスの終了: `taskkill /F /IM ConveniencePos.exe` |
| 3 | 必要に応じて DB 接続文字列の確認 |
| 4 | アプリケーション再起動 |
| 5 | 問題が継続する場合: 前バージョンにロールバック |

### 3-3. レシート出力失敗

| ステップ | 操作 |
|---------|------|
| 1 | デスクトップの書き込み権限を確認 |
| 2 | デスクトップ容量の確認 |
| 3 | 取引データは DB に保存済み（レシートは再生成可能） |
| 4 | アプリケーション再起動後、必要に応じて手動でレシート再出力 |

## 4. バックアップ手順

### 4-1. DBバックアップ（日次）

```bash
# SQL Server LocalDB のバックアップ
sqllocaldb stop MSSQLLocalDB
copy "%LOCALAPPDATA%\Microsoft\SQL Server Local DB\Instances\MSSQLLocalDB\ConveniencePosDb.mdf" "C:\Backup\ConveniencePos\db\ConveniencePosDb_%DATE:~0,4%%DATE:~5,2%%DATE:~8,2%.mdf"
sqllocaldb start MSSQLLocalDB
```

### 4-2. アプリケーションバックアップ（デプロイ前）

```bash
# デプロイ前のバックアップ
xcopy /E /Y "C:\Program Files\ConveniencePos" "C:\Backup\ConveniencePos\v1.0_%DATE:~0,4%%DATE:~5,2%%DATE:~8,2%\"
```

## 5. 復旧目標

| 項目 | 目標値 |
|------|--------|
| RPO（Recovery Point Objective） | 24時間（日次バックアップ） |
| RTO（Recovery Time Objective） | 1時間以内 |

## 6. 連絡先

| 役割 | 氏名 | 電話番号 |
|------|------|---------|
| システム管理者 | （未定） | （未定） |
| 開発チームリーダー | （未定） | （未定） |
| DB管理者 | （未定） | （未定） |

## バージョン履歴

| バージョン | 変更日 | 変更内容 | 変更者 |
|-----------|--------|----------|--------|
| 1.0 | 2026-08-18 | 初版作成 | 開発チーム |

## 承認記録

| 役割 | 氏名 | 承認日 | 署名 |
|------|------|--------|------|
| プロジェクトオーナー | 田中太郎 | 2026-08-18 | ✓ |
| 技術リーダー | 田中太郎 | 2026-08-18 | ✓ |
| QAリーダー | 田中太郎 | 2026-08-18 | ✓ |
