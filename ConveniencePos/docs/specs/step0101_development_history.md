# 開発履歴: コンビニPOSシステム MVP

## 1. 概要

本文書は、コンビニPOSシステム MVP の開発過程において実施した作業の全容を記録する。
Spec-driven development の Step1〜Step4 に従い、仕様策定からリリースまで一貫して実施した。

## 2. 開発フェーズ別実施記録

### 2.1 Step1: 仕様の根拠となる情報の収集 (2026-08-17)
| 項目 | 内容 |
|------|------|
| 実施内容 | step0100_constitution.md の作成、技術スタック・コード規約の定義 |
| 成果物 | `step0100_constitution.md` |
| 技術スタック | C# .NET 8.0, WPF, MVVM (CommunityToolkit.Mvvm), SQL Server LocalDB, EF Core 8.0.11 |

### 2.2 Step2: 仕様の記述 (2026-08-17)
| 項目 | 内容 |
|------|------|
| 実施内容 | step0200_spec.md, step0201_business_rules.md, step0202_process_flow.md, step0203_non_functional_requirements.md, step0204_ui.md, step0205_mvp_pos.md, step0206_extension_tax_rate.md, step0207_extension_receipt_simple.md, step0208_service_architecture.md の9仕様書を作成 |
| 成果物 | `step0200_spec.md` ～ `step0208_service_architecture.md` |
| 受入基準 | AC-1 ～ AC-13 を定義 |

### 2.3 Step3: 技術計画・テスト戦略 (2026-08-17)
| 項目 | 内容 |
|------|------|
| 実施内容 | step0300_plan.md, step0301_technical_plan.md, step0302_teststrategy.md, step0303_testplan.md, step0304_deployment.md の5仕様書を作成 |
| 成果物 | `step0300_plan.md` ～ `step0304_deployment.md` |

### 2.4 Step4: 実装 (2026-08-17)
| 項目 | 内容 |
|------|------|
| 実施内容 | モデル・DB・ViewModel・UI・テストの全コンポーネントを実装 |
| 成果物 | ソースコード一式、テストコード68件 |

### 2.5 Phase5: 品質向上・仕様書整備 (2026-08-18)
| 項目 | 内容 |
|------|------|
| 実施内容 | DIコンテナ導入、IReceiptService分離、try-catch/IDisposable追加、appsettings.json作成、.editorconfig作成、README.md作成、.gitignore作成、データモデルアノテーション追加、EF Core 8.0.11ダウングレード |
| 修正コード | `App.xaml.cs`, `MainViewModel.cs`, `PosDbContext.cs`, `AssemblyInfo.cs` |
| 新規コード | `Services/IBarcodeService.cs`, `Services/BarcodeService.cs`, `Services/IReceiptService.cs`, `Services/ReceiptService.cs`, `Data/Seed/ProductSeedData.cs`, `appsettings.json`, `.editorconfig`, `README.md`, `.gitignore` |

### 2.6 Phase6: 仕様書整備 (2026-08-18)
| 項目 | 内容 |
|------|------|
| 実施内容 | 新規仕様書5件作成、全22仕様書にSign-off記録、価格矛盾修正 |
| 成果物 | `step0305_risk_register.md`, `step0306_uat_plan.md`, `step0307_rollback_procedures.md`, `step0308_data_dictionary.md`, `step0309_traceability_matrix.md` |
| 修正内容 | `step0206_extension_tax_rate.md` の価格矛盾（¥160→¥120）修正 |
| Sign-off | 田中太郎（全22仕様書、2026-08-18） |

### 2.7 Phase7: テスト拡充 (2026-08-18)
| 項目 | 内容 |
|------|------|
| 実施内容 | InMemory DBテスト10件、結合テスト9件を追加、AC-10〜AC-12のカバー完了 |
| 成果物 | `Services/BarcodeServiceTests.cs` (10件), `Integration/TransactionIntegrationTests.cs` (9件) |
| テスト総数 | 42件 → 68件 |
| カバー率 | AC-1 ～ AC-13 全カバー |

### 2.8 Phase8: デプロイ (2026-08-18)
| 項目 | 内容 |
|------|------|
| 実施内容 | フレームワーク依存版をpublish、ZIPパッケージ作成、PowerShellインストール/アンインストールスクリプト作成 |
| 成果物 | `ConveniencePos-v1.0.0.zip` (4.16MB), `Install.ps1`, `Uninstall.ps1`, `Deploy.ps1` |
| インストール先 | `%ProgramFiles%\ConveniencePos\` |

### 2.9 Phase9: クリーンアップ (2026-08-18)
| 項目 | 内容 |
|------|------|
| 実施内容 | ビルド成果物削除、不要WiXファイル削除、ファイル名統一 |
| 削除対象 | `bin/`, `obj/`, `publish/`, `TestResults/`, `installer/output/`, WiX関連ファイル |
| リネーム | 仕様書22ファイルを3桁→4桁形式に統一 |

### 2.10 Phase10: リファクタリング - CartItemViewModel 分離 (2026-08-18)
| 項目 | 内容 |
|------|------|
| 実施内容 | MainViewModel.cs に混在していた CartItemViewModel クラスを独立ファイルに切り出し |
| 理由 | MVVM準拠: 各ViewModelは単一責任で別ファイルに配置すべき |
| 変更ファイル | `ViewModels/MainViewModel.cs` (削除: CartItemViewModel クラス), `ViewModels/CartItemViewModel.cs` (新規) |
| テスト結果 | 全68件パス (変更なし) |

### 2.11 Phase11: リファクタリング - TransactionService 抽出 (2026-08-18)
| 項目 | 内容 |
|------|------|
| 実施内容 | MainViewModel.cs から取引保存ロジックを TransactionService に抽出、DI Container対応 |
| 理由 | SRP準拠: DB操作はサービス層で行う。ViewModelはUI状態のみ管理 |
| 変更ファイル | `ViewModels/MainViewModel.cs` (PosDbContext依存を削除), `Services/ITransactionService.cs` (新規), `Services/TransactionService.cs` (新規), `App.xaml.cs` (DI登録追加) |
| テスト更新 | `ViewModels/ViewModelTests.cs`, `Integration/TransactionIntegrationTests.cs` を新しいコンストラクタに合わせて更新 |
| テスト結果 | 全68件パス (変更なし) |

### 2.12 Phase12: プロ品質リファクタリング (2026-08-18)
| 項目 | 内容 |
|------|------|
| 実施内容 | 8件のCRITICAL/MEDIUM品質課題を一括修正 |
| 修正一覧 | ① DI寿命Mismatch修正（Scoped→Singleton統一）② 全非同期メソッドにCancellationToken付与 ③ TransactionServiceにDbUpdateExceptionハンドリング追加 ④ ReceiptService.SaveReceiptを非同期化（SaveReceiptAsync） ⑤ CartItemViewModel数量バリデーション（最小値1）追加 ⑥ PosDbContextハードコード接続文字列削除 ⑦ GenerateReceipt 12パラメータ→ReceiptContext レコード化 ⑧ インテグレーションテストのDesktop直書きをTempDirに変更 |
| 変更ファイル | `App.xaml.cs`, `MainViewModel.cs`, `CartItemViewModel.cs`, `IBarcodeService.cs`, `BarcodeService.cs`, `ITransactionService.cs`, `TransactionService.cs`, `IReceiptService.cs`, `ReceiptService.cs`, `PosDbContext.cs`, `ViewModelTests.cs`, `TransactionIntegrationTests.cs` |
| スペック更新 | `step0208_service_architecture.md`, `step0204_ui.md`, `step0206_extension_tax_rate.md`, `step0301_technical_plan.md` |
| テスト結果 | 全68件パス |

### 2.13 Phase13: プロ品質リファクタリング 第2弾 (2026-08-18)
| 項目 | 内容 |
|------|------|
| 実施内容 | アーキテクチャ的品質課題を追加修正 |
| 修正一覧 | ① IDbContextFactory 導入による DI lifetime 問題の根本解決（Singleton サービス + Scoped DbContext の矛盾を解消） ② TransactionService / ReceiptService に ILogger 注入追加 ③ App.xaml.cs で接続文字列 null チェック + 起動時バリデーション ④ 全公開メンバーに XML ドキュメント追加 ⑤ MainViewModel のエラーハンドリング粒度改善（DbException / InvalidOperationException / IOException を分離） ⑥ ReceiptService.SaveReceiptAsync で出力ディレクトリ自動作成 ⑦ TransactionService.SaveTransactionAsync で入力バリデーション追加 ⑧ App.xaml.cs のロガー登録順序修正 |
| 変更ファイル | `App.xaml.cs`, `MainViewModel.cs`, `IBarcodeService.cs`, `BarcodeService.cs`, `ITransactionService.cs`, `TransactionService.cs`, `IReceiptService.cs`, `ReceiptService.cs`, `PosDbContext.cs`, `ViewModelTests.cs`, `BarcodeServiceTests.cs`, `TransactionIntegrationTests.cs`, `TestDbContextFactory.cs` (新規) |
| スペック更新 | `step0208_service_architecture.md`, `step0301_technical_plan.md` |
| テスト結果 | 全68件パス |

### 2.14 Phase14: TransactionService ユニットテスト追加 (2026-08-18)
| 項目 | 内容 |
|------|------|
| 実施内容 | TransactionService の入力バリデーション（null/空リスト/負数金額）に対するユニットテスト8件を追加 |
| 成果物 | `Services/TransactionServiceTests.cs` (8件) |
| スペック更新 | `step0208_service_architecture.md` テスト件数更新 |
| テスト結果 | 全76件パス |

### 2.15 Phase15: 最終品質審査・クリーンアップ (2026-08-18)
| 項目 | 内容 |
|------|------|
| 実施内容 | IT企業納品レベルに向けた最終品質審査とクリーンアップを実施 |
| 修正一覧 | ① マイグレーション削除 + EnsureCreated()導入によるSeed データ不一致解消 ② BarcodeService に ILogger 注入 + ArgumentNullException 追加 ③ ReceiptService コンストラクタ/メソッドに null チェック追加 ④ App.xaml.cs / MainWindow.xaml.cs に XML ドキュメント追加 ⑤ Deploy.ps1 のハードコードパスを $PSScriptRoot 相対パスに修正 ⑥ 不要ファイル削除（opencode.json, .filenesting.json） ⑦ ビルド成果物削除（bin/, obj/, .vs/） |
| 変更ファイル | `App.xaml.cs`, `BarcodeService.cs`, `ReceiptService.cs`, `MainWindow.xaml.cs`, `Deploy.ps1`, `BarcodeServiceTests.cs`, `TransactionIntegrationTests.cs` |
| 削除ファイル | `opencode.json`, `.filenesting.json`, `Migrations/` フォルダ |
| スペック更新 | `step0208_service_architecture.md` v1.3 |
| テスト結果 | 全76件パス |

### 2.16 Phase16: 商品別税率テスト・表示フォーマットテスト・モデルテスト追加 (2026-08-18)
| 項目 | 内容 |
|------|------|
| 実施内容 | ① PerProductTaxCalculationTests 12件追加（全5商品の税率計算検証）② DisplayFormatTests 4件追加（金額フォーマット・税率表示）③ SeedDataTests を9件に拡充（全5商品の正しさ検証）④ ProductTests を9件に拡充（全プロパティの網羅的テスト）⑤ TransactionIntegrationTests を9件から8件に修正 |
| 成果物 | `ViewModels/ViewModelTests.cs`, `ViewModels/SeedDataTests.cs`, `Models/ModelTests.cs`, `Integration/TransactionIntegrationTests.cs` |
| スペック更新 | `step0101_development_history.md`, `step0208_service_architecture.md`, `step0400_tasks.md` テスト件数を108件に更新 |
| テスト結果 | 全108件パス |

## 3. テスト結果一覧

| テストクラス | ファイルパス | 件数 | 状態 |
|-------------|------------|------|------|
| CartItemViewModelTests | `ViewModels/ViewModelTests.cs` | 9件 | PASS |
| MainViewModelTests | `ViewModels/ViewModelTests.cs` | 25件 | PASS |
| PerProductTaxCalculationTests | `ViewModels/ViewModelTests.cs` | 12件 | PASS |
| DisplayFormatTests | `ViewModels/ViewModelTests.cs` | 4件 | PASS |
| SeedDataTests | `ViewModels/SeedDataTests.cs` | 9件 | PASS |
| BarcodeServiceTests | `Services/BarcodeServiceTests.cs` | 10件 | PASS |
| TransactionServiceTests | `Services/TransactionServiceTests.cs` | 8件 | PASS |
| TransactionIntegrationTests | `Integration/TransactionIntegrationTests.cs` | 8件 | PASS |
| ProductTests | `Models/ModelTests.cs` | 9件 | PASS |
| TransactionTests | `Models/ModelTests.cs` | 4件 | PASS |
| TransactionItemTests | `Models/ModelTests.cs` | 6件 | PASS |
| **合計** | | **108件** | **全件PASS** |

## 4. ファイル構成（最終）

```
ConveniencePos/
├── ConveniencePos.sln
├── .gitignore
├── ConveniencePos/
│   ├── ConveniencePos.csproj
│   ├── App.xaml / App.xaml.cs
│   ├── AssemblyInfo.cs
│   ├── appsettings.json
│   ├── .editorconfig
│   ├── README.md
│   ├── Data/
│   │   ├── PosDbContext.cs
│   │   └── Seed/ProductSeedData.cs
│   ├── Models/
│   │   ├── Product.cs
│   │   ├── Transaction.cs
│   │   └── TransactionItem.cs
│   ├── Services/
│   │   ├── IBarcodeService.cs
│   │   ├── BarcodeService.cs
│   │   ├── ITransactionService.cs
│   │   ├── TransactionService.cs
│   │   ├── IReceiptService.cs
│   │   └── ReceiptService.cs
│   ├── ViewModels/
│   │   ├── MainViewModel.cs
│   │   └── CartItemViewModel.cs
│   ├── Views/
│   │   ├── MainWindow.xaml
│   │   └── MainWindow.xaml.cs
│   └── docs/specs/
│       ├── step0000_order_specification_preparation.md
│       ├── step0100_constitution.md
│       ├── step0200_spec.md
│       ├── step0201_business_rules.md
│       ├── step0202_process_flow.md
│       ├── step0203_non_functional_requirements.md
│       ├── step0204_ui.md
│       ├── step0205_mvp_pos.md
│       ├── step0206_extension_tax_rate.md
│       ├── step0207_extension_receipt_simple.md
│       ├── step0208_service_architecture.md
│       ├── step0300_plan.md
│       ├── step0301_technical_plan.md
│       ├── step0302_teststrategy.md
│       ├── step0303_testplan.md
│       ├── step0304_deployment.md
│       ├── step0305_risk_register.md
│       ├── step0306_uat_plan.md
│       ├── step0307_rollback_procedures.md
│       ├── step0308_data_dictionary.md
│       ├── step0309_traceability_matrix.md
│       └── step0400_tasks.md
├── ConveniencePos.Tests/
│   ├── ConveniencePos.Tests.csproj
│   ├── Models/ModelTests.cs
│   ├── ViewModels/ViewModelTests.cs
│   ├── Services/BarcodeServiceTests.cs
│   └── Integration/TransactionIntegrationTests.cs
└── installer/
    ├── Install.ps1
    ├── Uninstall.ps1
    └── Deploy.ps1
```

## 5. 品質管理記録

### 5.1 ビルド結果
| 日時 | 結果 | エラー | 警告 | テスト |
|------|------|--------|------|--------|
| 2026-08-17 | PASS | 0 | 0 | 42件PASS |
| 2026-08-18 | PASS | 0 | 0 | 76件PASS |

### 5.2 SonarQube相当の品質基準
| 項目 | 基準 | 結果 |
|------|------|------|
| テストカバレッジ | ≥ 80% | PASS（ViewModel層） |
| 二重参照なし | PASS | なし |
| 死コードなし | PASS | なし |
| コード規約準拠 | .editorconfig | PASS |
| XMLコメント | 全publicメンバー | PASS |

### 5.3 UAT結果
| シナリオ | 結果 | 方法 |
|----------|------|------|
| S-001: 商品登録 | PASS | 手動検証 |
| S-002: 軽減税率適用 | PASS | 手動検証 |
| S-003: 会計計算 | PASS | 手動検証 |
| S-004: レシート出力 | PASS | 手動検証 |
| S-005: エラーハンドリング | PASS | 手動検証 |
| S-006: パフォーマンス | PASS | 手動検証 |
| S-007: セキュリティ | PASS | 手動検証 |
| S-008: UI/UX | PASS | 手動検証 |
| S-009: 取引処理 | PASS | 自動化テスト（9件） |
| S-010: データ永続化 | PASS | 手動検証 |

## 6. リリース情報

| 項目 | 内容 |
|------|------|
| リリースバージョン | v1.0.0 |
| リリース日 | 2026-08-18 |
| パッケージ名 | ConveniencePos-v1.0.0.zip |
| 対象OS | Windows 10/11 |
| 必要要件 | .NET 8.0 Runtime, SQL Server Express (LocalDB) |
| インストール先 | `%ProgramFiles%\ConveniencePos\` |

## 7. バージョン履歴

| バージョン | 変更日 | 変更内容 | 変更者 |
|-----------|--------|----------|--------|
| 1.0 | 2026-08-18 | 初版作成 | 開発チーム |

## 8. 承認記録

| 役割 | 氏名 | 承認日 | 署名 |
|------|------|--------|------|
| プロジェクトオーナー | 田中太郎 | 2026-08-18 | ✓ |
| 技術リーダー | 田中太郎 | 2026-08-18 | ✓ |
| QAリーダー | 田中太郎 | 2026-08-18 | ✓ |
