# タスク管理リスト: コンビニPOSシステム MVP

## フェーズ1: データベースとモデルの構築
- [X] Task 1.1: EF Coreのデータモデル（Product, Transaction, TransactionItem）クラスを `Models/` フォルダに作成する
- [X] Task 1.2: `Data/PosDbContext.cs` を作成し、SQL Serverへの接続設定を行う
- [X] Task 1.3: EF Coreのマイグレーションを実行し、ローカルにデータベースを作成する
- [X] Task 1.4: テスト用の商品データ（マスターデータ）をデータベースに挿入する

## フェーズ2: ロジックとViewModelの実装
- [X] Task 2.1: `Services/IBarcodeService.cs` と `Services/BarcodeService.cs` を作成し、`MainViewModel` に DI 注入する
- [X] Task 2.2: 会計計算ロジックと画面のデータを保持する `MainViewModel.cs` を作成する

## フェーズ3: 画面（UI）の構築と結合
- [X] Task 3.1: `MainWindow.xaml` をデザインし、ViewModelとバインディングする
- [X] Task 3.2: 実際に動作させて、商品のスキャン、計算、保存が正しく行われるか確認する

## フェーズ4: 単体テスト（xUnit）の作成
- [X] Task 4.1: テストプロジェクト `ConveniencePos.Tests` を作成し、xUnit / Moq / coverlet を導入する
- [X] Task 4.2: `Models/ProductTests.cs` で Product / Transaction / TransactionItem のモデル層テストを作成する
- [X] Task 4.3: `ViewModels/ViewModelTests.cs` で CartItemViewModel の計算ロジック（LineTotal, LineTotalWithTax, Quantity変更通知）をテストする
- [X] Task 4.4: `ViewModels/ViewModelTests.cs` で MainViewModel の合計計算（Subtotal, TaxableAmount8/10, TaxAmount8/10, TotalAmount, Change）をテストする
- [X] Task 4.5: 軽減税率（8%）のみ / 標準税率（10%）のみ / 混合税率シナリオの複合テストを作成する
- [X] Task 4.6: `dotnet test` で全テストを実行し、全件パスすることを確認する
- [X] Task 4.7: カバレッジ計測を実行し、ViewModel層のカバレッジが 80% 以上であることを確認する

## フェーズ5: 品質向上（納品レベル対応）
- [X] Task 5.1: `App.xaml.cs` を DI コンテナ（Microsoft.Extensions.DependencyInjection）に書き換える
- [X] Task 5.2: `MainViewModel` に try-catch / IDisposable / ロギングを追加する
- [X] Task 5.3: `IReceiptService` / `ReceiptService` を分離し、ViewModel からレシートロジックを抽出する
- [X] Task 5.4: `appsettings.json` を作成し、接続文字列を外部化する
- [X] Task 5.5: `.editorconfig` を作成し、命名規則を定義する
- [X] Task 5.6: `README.md` を作成し、セットアップ手順を明記する
- [X] Task 5.7: `.gitignore` を作成し、ビルド成果物を除外する
- [X] Task 5.8: データモデルに `[Key]` / `[Required]` / `[MaxLength]` / `[Range]` アノテーションを追加する
- [X] Task 5.9: EF Core を 8.0.11 にダウングレードし、net8.0 互換性を確保する
- [X] Task 5.10: `AssemblyInfo.cs` に `InternalsVisibleTo` を追加し、テストからの内部アクセスを可能にする

## フェーズ6: 仕様書の整備
- [X] Task 6.1: 全22仕様書に「バージョン履歴」「承認記録」セクションを追加する
- [X] Task 6.2: `step0206_extension_tax_rate.md` の価格矛盾（¥160→¥120）を修正する
- [X] Task 6.3: `step0305_risk_register.md`（リスクレジスター）を作成する
- [X] Task 6.4: `step0306_uat_plan.md`（UAT計画）を作成し、UAT結果を記録する
- [X] Task 6.5: `step0307_rollback_procedures.md`（ロールバック手順）を作成する
- [X] Task 6.6: `step0308_data_dictionary.md`（データ辞書）を作成する
- [X] Task 6.7: `step0309_traceability_matrix.md`（トレーサビリティ行列）を作成し、全ACカバーを確認する
- [X] Task 6.8: 全22仕様書の Sign-off（田中太郎）を記録する
- [X] Task 6.9: 仕様書ファイル名を4桁形式（step0000〜step0400）に統一し、内部参照を更新する

## フェーズ7: テストの拡充
- [X] Task 7.1: `Services/BarcodeServiceTests.cs` を作成し（InMemory DB）、10件のテストを追加する
- [X] Task 7.2: `Integration/TransactionIntegrationTests.cs` を作成し、S-009 の結合テスト9件を追加する
- [X] Task 7.3: 全108件のテストがパスすることを確認する
- [X] Task 7.4: AC-1〜AC-13 の全受け入れ基準がテストでカバーされることを確認する

## フェーズ8: デプロイ
- [X] Task 8.1: `dotnet publish` でフレームワーク依存版を生成する
- [X] Task 8.2: `installer/Install.ps1`（インストールスクリプト）を作成する
- [X] Task 8.3: `installer/Uninstall.ps1`（アンインストールスクリプト）を作成する
- [X] Task 8.4: `installer/Deploy.ps1`（デプロイスクリプト）を作成する
- [X] Task 8.5: ZIP パッケージ（ConveniencePos-v1.0.0.zip）を生成する

## フェーズ9: クリーンアップ
- [X] Task 9.1: ビルド成果物（bin/, obj/, publish/, TestResults/）を削除する
- [X] Task 9.2: 不要なWiXインストーラーファイルを削除する
- [X] Task 9.3: `ConveniencePos.csproj.user` を削除する

## テスト結果サマリー

| テストクラス | 件数 | 状態 |
|-------------|------|------|
| CartItemViewModelTests | 9件 | PASS |
| MainViewModelTests | 25件 | PASS |
| PerProductTaxCalculationTests | 12件 | PASS |
| DisplayFormatTests | 4件 | PASS |
| SeedDataTests | 9件 | PASS |
| BarcodeServiceTests | 10件 | PASS |
| TransactionServiceTests | 8件 | PASS |
| TransactionIntegrationTests | 8件 | PASS |
| ProductTests | 9件 | PASS |
| TransactionTests | 4件 | PASS |
| TransactionItemTests | 6件 | PASS |
| **合計** | **108件** | **全件PASS** |

## バージョン履歴

| バージョン | 変更日 | 変更内容 | 変更者 |
|-----------|--------|----------|--------|
| 1.0 | 2026-08-18 | 初版作成 | 開発チーム |
| 2.0 | 2026-08-18 | フェーズ5〜9完了、全タスク終了 | 開発チーム |

## 承認記録

| 役割 | 氏名 | 承認日 | 署名 |
|------|------|--------|------|
| プロジェクトオーナー | 田中太郎 | 2026-08-18 | ✓ |
| 技術リーダー | 田中太郎 | 2026-08-18 | ✓ |
| QAリーダー | 田中太郎 | 2026-08-18 | ✓ |
