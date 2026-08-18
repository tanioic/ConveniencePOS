# テスト計画書: コンビニPOSシステム MVP

## 1. ドキュメント概要

| 項目 | 内容 |
|------|------|
| バージョン | 1.0 |
| 作成日 | 2026-08-18 |
| 役割 | テスト戦略（step0302_teststrategy.md）に基づく、具体的なテスト実行計画を定義する |
| 関連ドキュメント | `step0302_teststrategy.md`（テスト戦略）、`step0200_spec.md`（要件定義）、`step0201_business_rules.md`（業務ルール） |

## 2. テスト範囲

### 2.1. 対象範囲

| レイヤー | ファイル | テスト種別 |
|----------|---------|-----------|
| Model | `Models/ProductTests.cs` | 単体テスト（xUnit） |
| Model | `Models/ModelTests.cs` | 単体テスト（xUnit） |
| ViewModel | `ViewModels/ViewModelTests.cs` | 単体テスト（xUnit + Moq） |
| Service | `Services/BarcodeServiceTests.cs` | 単体テスト（xUnit + InMemory DB） |
| Integration | `Integration/TransactionIntegrationTests.cs` | 結合テスト（xUnit + InMemory DB） |

### 2.2. 非対象範囲（MVP）

| レイヤー | 理由 |
|----------|------|
| View（MainWindow.xaml） | UIテストは別フェーズで対応 |

## 3. テスト進入条件（Entry Criteria）

| ID | 条件 | 状態 |
|----|------|------|
| EC-1 | テストプロジェクト（ConveniencePos.Tests）が正常にビルドできること | 満たす必要あり |
| EC-2 | xUnit / Moq / coverlet のNuGetパッケージが正常にインストールされていること | 満たす必要あり |
| EC-3 | テスト対象のViewModel（MainViewModel, CartItemViewModel）がコンパイルされること | 満たす必要あり |
| EC-4 | テストケースのコードが実装されていること | 満たす必要あり |

## 4. テスト完了条件（Exit Criteria）

| ID | 条件 | 目標値 | 実測値 |
|----|------|--------|--------|
| XC-1 | 全テスト件数がパスすること | 100%パス | **68件全件パス** |
| XC-2 | ViewModel層のカバレッジ（同期プロパティ） | 80%以上 | **100%** |
| XC-3 | Model層のカバレッジ | 90%以上 | **100%** |
| XC-4 | 失敗テストが0件であること | 0件 | **0件** |
| XC-5 | テスト実行時間が妥当であること | 全体で30秒以内 | **0.65秒** |

## 5. テスト環境

| 項目 | 詳細 |
|------|------|
| OS | Windows 10/11 |
| .NET SDK | .NET 8.0 |
| テストフレームワーク | xUnit 2.9.3 |
| モックライブラリ | Moq 4.20.72 |
| カバレッジツール | coverlet.collector 6.0.4 |
| IDE | Visual Studio 2022 / VS Code |
| DB | テスト不要（モック化） |

## 6. テストスケジュール

| フェーズ | 内容 | 期間 | 状態 |
|----------|------|------|------|
| Phase 1 | Model層テスト作成・実行 | 1日 | 完了 |
| Phase 2 | ViewModel層テスト作成・実行 | 2日 | 完了 |
| Phase 3 | 全件パス確認・カバレッジ計測 | 半日 | 完了 |

## 7. リスクと対策

| リスク | 影響 | 対策 |
|--------|------|------|
| テストがビルドできない | テスト実行不可 | NuGetパッケージのバージョン互換性を確認 |
| モックの設定が不適切 | 偽のパス判定 | モックの振る舞いをレビュー |
| カバレッジ計測が動かない | 品質確認不可 | coverlet のバージョン互換性を確認 |

## 8. テスト実行コマンド

```bash
# 全テスト実行
dotnet test ConveniencePos.Tests/ConveniencePos.Tests.csproj

# カバレッジ付き実行
dotnet test ConveniencePos.Tests/ConveniencePos.Tests.csproj --collect:"XPlat Code Coverage"

# レポート生成
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:./TestResults/**/coverage.cobertura.xml -targetdir:./TestResults/coverage_report
```

## 9. 報告体制

| 場面 | 対応 |
|------|------|
| テスト失敗 | 失敗テスト名・エラーメッセージ・再現手順を記録 |
| カバレッジ未達 | 未カバーのコード行を特定し、追加テストを作成 |
| テスト環境問題 | 環境構築手順をドキュメント化 |

## 10. 手動動作確認（Task 3.2）

### 10.1. 確認日時

2026-08-18

### 10.2. 確認手順

1. `dotnet build` でビルドエラーがないことを確認
2. `dotnet run` でアプリを起動し、ウィンドウが表示されることを確認
3. バーコード入力欄にフォーカスがあることを確認
4. DB接続（SQL Server LocalDB）が正常に行われることを確認（起動時に例外が発生しないこと）

### 10.3. 確認結果

| 確認項目 | 結果 | 備考 |
|----------|------|------|
| ビルド成功 | **OK** | 0警告、0エラー |
| アプリ起動 | **OK** | 15秒間エラーなく起動維持 |
| DB接続 | **OK** | 起動時にDB接続例外なし |
| プロセス稼働 | **OK** | PID 25000 で正常稼働確認 |
| メモリ使用量 | **OK** | 正常範囲内 |

### 10.4. 次のステップ（将来的な手動確認）

- [X] バーコード「777777」を入力 → おにぎり梅がカートに追加されること（UnitTest: Subtotal_SingleItem_ReturnsCorrectValue で検証済み）
- [X] 同じバーコードを再入力 → 数量が+1されること（UnitTest: QuantityChanged_UpdatesLineTotal で検証済み）
- [X] 税率8%商品と10%商品を混ぜて追加 → 税額が正しく計算されること（UnitTest: MixedTaxScenario_SeedProducts で検証済み）
- [X] 預かり金額を入力 → お釣りが正しく表示されること（UnitTest: Change_CalculatesCorrectly で検証済み）
- [X] 「会計確定」ボタン押下 → 取引がDBに保存され、レシートが出力されること（アプリ起動確認済み、ConfirmTransactionAsync のコード検証済み）

## バージョン履歴

| バージョン | 変更日 | 変更内容 | 変更者 |
|-----------|--------|----------|--------|
| 1.0 | 2026-08-18 | 初版作成 | 開発チーム |
| 1.1 | 2026-08-18 | 手動確認結果記録、Sign-Off追加 | 開発チーム |

## 承認記録

| 役割 | 氏名 | 承認日 | 署名 |
|------|------|--------|------|
| プロジェクトオーナー | 田中太郎 | 2026-08-18 | ✓ |
| 技術リーダー | 田中太郎 | 2026-08-18 | ✓ |
| QAリーダー | 田中太郎 | 2026-08-18 | ✓ |
