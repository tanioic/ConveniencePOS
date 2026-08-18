# 技術実装計画: コンビニPOSシステム MVP

## 1. 既存ドキュメントとの関係

本ドキュメントは `How（どうやって）` を定義する。技術スタック・DB設計・UI設計・テスト戦略の詳細は既存ドキュメントを参照する。

| テーマ | 参照先 |
|--------|--------|
| 技術スタック・コード規約 | `step0100_constitution.md` |
| データモデル構造 | `step0301_technical_plan.md` |
| 軽減税率のDB・ViewModel変更 | `step0206_extension_tax_rate.md` |
| UI レイアウト・バインディング | `step0204_ui.md` |
| レシート出力仕様 | `step0207_extension_receipt_simple.md` |
| テスト戦略・カバレッジ対象 | `step0302_teststrategy.md` |
| 機能仕様（何をやるか） | `step0205_mvp_pos.md` |
| 要件定義（Why / 受け入れ基準） | `step0200_spec.md` |

## 2. タスク間の依存関係と実装順序

```
フェーズ1: DB + モデル
  Task 1.1 (Product, Transaction, TransactionItem)
  Task 1.2 (PosDbContext)
  Task 1.3 (マイグレーション)
  Task 1.4 (シードデータ)
       │
       ▼
フェーズ2: ロジック + ViewModel
  Task 2.1 (BarcodeService)
  Task 2.2 (MainViewModel)
       │
       ▼
フェーズ3: UI
  Task 3.1 (MainWindow.xaml)
  Task 3.2 (結合テスト)
       │
       ▼
フェーズ4: 単体テスト (xUnit)
  Task 4.1 (テストプロジェクト構築)
  Task 4.2 (Model層テスト)
  Task 4.3 (CartItemViewModelテスト)
  Task 4.4 (MainViewModelテスト)
  Task 4.5 (複合シナリオテスト)
  Task 4.6 (全件パス確認)
  Task 4.7 (カバレッジ計測)
```

### 依存ルール
- **フェーズ1 → フェーズ2**: モデルクラスが確定しないとViewModelの型安全性が確保できない
- **フェーズ2 → フェーズ3**: ViewModelのプロパティ・コマンドが確定しないとXAMLのバインディングができない
- **フェーズ3 → フェーズ4**: 動作確認後にテスト値を精査し、テストを安定させる
- **フェーズ4 のタスク間**: 4.1→4.2〜4.5→4.6→4.7 の順序

## 3. 既存コードとの差分管理

### 3.1. モデル層
- `Product.TaxRate` と `TransactionItem.AppliedTaxRate` は既に追加済み（`step0206_extension_tax_rate.md` 実装済み）
- 新規追加不要

### 3.2. ViewModel層
- `MainViewModel` の税率別集計ロジック（TaxableAmount8/10, TaxAmount8/10）は既に実装済み
- `CartItemViewModel.LineTotalWithTax` は既に実装済み
- 追加実装が必要な箇所: なし（MVP機能は全て実装済み）

### 3.3. テスト層
- `ConveniencePos.Tests` プロジェクトは既に存在し、xUnit + Moq + coverlet が導入済み
- `Models/ModelTests.cs`: 15件のModel層テストが実装済み
- `ViewModels/ViewModelTests.cs`: 27件のViewModel層テストが実装済み
- 残タスク: Task 4.6（全件パス確認）と Task 4.7（カバレッジ計測）のみ

## 4. セキュリティ要件
- DB接続文字列は `appsettings.json` に管理（`step0100_constitution.md` の DB 接続管理規約に準拠）
- レシート出力はデスクトップのみ（外部ネットワーク送信なし）
- 認証・認権はMVP対象外（単一ユーザー前提）

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
