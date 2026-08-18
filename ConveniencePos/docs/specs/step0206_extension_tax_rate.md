# 仕様拡張: 軽減税率対応 (8% / 10%)

## 1. 概要
日本の消費税法に基づき、商品ごとに「軽減税率（8%）」または「標準税率（10%）」を適用し、それぞれの消費税額および合計金額を正しく計算・管理できるようにシステムを拡張する。

## 2. 仕様定義 (Specify)
- **商品マスターの拡張**: 
  - すべての商品に税率（8% または 10%）を持たせる。
  - お弁当・飲料などの飲食料品は 「8%」 を適用する。
  - 日用品・雑誌などは 「10%」 を適用する。
- **会計計算の拡張**:
  - 買い物かご内の商品の税率に応じて、8%対象の合計金額と10%対象の合計金額をそれぞれ集計する。
  - 消費税額は「8%対象の税額」と「10%対象の税額」をそれぞれ計算（端数は切捨て）し、その合算値を画面に表示する。
- **画面表示 (UI) の拡張**:
  - 商品一覧（DataGrid）に、その商品の税率（8% または 10%）を表示する列を追加する。
  - 画面下部の計算エリアに、「8%対象額」「10%対象額」それぞれの内訳を表示する。

## 3. 技術計画 (Plan)
- **データモデルの変更 (Database)**:
  - `Product` クラスに `TaxRate` (int) プロパティを追加する。値は `8` または `10` とする。
  - `TransactionItem` クラスに `AppliedTaxRate` (int) プロパティを追加する（購入時点の税率を記録するため）。
- **マイグレーション**:
  - EF Coreのマイグレーション機能を利用し、SQL Serverのテーブル構造をアップデートする。
  - 初期データ（Seedデータ）を更新し、おにぎり梅・緑茶は「8%」、ポテトチップスは「10%」、新規に追加する日用品（例：ティッシュ 200円）は「10%」とする。
- **ViewModelのロジック変更**:
  - `MainViewModel` 内の合計金額・消費税計算処理を、商品の `TaxRate` を判定して計算するロジックに修正する。

## 4. 実装内容 (Implement)

### 4-1. データモデル (完了)
- `Product.TaxRate` (int): 商品の適用税率 (8 or 10)。既存モデルに追加済み。
- `TransactionItem.AppliedTaxRate` (int): 購入時点の税率記録。既存モデルに追加済み。

### 4-2. ViewModel (`ViewModels/MainViewModel.cs`, `ViewModels/CartItemViewModel.cs`) (完了)
- `CartItemViewModel.TaxRate` (int): カート内商品の税率を保持するプロパティを追加。
- `CartItemViewModel.Quantity` (int): 数量プロパティ。最小値は1で、未満を設定すると `ArgumentOutOfRangeException` がスローされる。
- `CartItemViewModel.LineTotalWithTax` (decimal): 税込小計。`Math.Floor(UnitPrice × Quantity × (1 + TaxRate / 100))` で計算。数量変更時に再計算される。
- `AddItemAsync`: 商品追加時に `product.TaxRate` を `CartItemViewModel.TaxRate` に設定。新規商品は `PropertyChanged` イベントを `MainViewModel` に登録する。
- `OnCartItemPropertyChanged`: `CartItemViewModel.Quantity` 変更時に `RefreshTotals()` を呼び、合計金額をリアルタイムに再計算する。
- 税率別集計プロパティ:
  - `TaxableAmount8`: 税率8%商品のLineTotal合算額
  - `TaxableAmount10`: 税率10%商品のLineTotal合算額
  - `TaxAmount8`: `Math.Floor(TaxableAmount8 * 0.08m)` (端数切捨て)
  - `TaxAmount10`: `Math.Floor(TaxableAmount10 * 0.10m)` (端数切捨て)
  - `TaxAmount`: `TaxAmount8 + TaxAmount10` (合算消費税)
  - `TotalAmount`: `Subtotal + TaxAmount`
- `ConfirmTransactionAsync`: 各 `TransactionItem` に `AppliedTaxRate = i.TaxRate` を設定して保存。保存前に全カートアイテムの `PropertyChanged` イベントを解除し、カートをクリアする。
- `RefreshTotals`: 追加した全プロパティの `OnPropertyChanged` を通知。

### 4-3. UI (`MainWindow.xaml`) (完了)
- **DataGrid**: 「税率」カラムを追加。バインディング: `{Binding TaxRate, StringFormat='{}{0}%'}`
- **DataGrid 数量カラム**: `DataGridTemplateColumn` で実装し、通常時は `TextBlock` 表示、編集時は `TextBox` 表示に切り替わり数量を直接変更可能とする。バーコードスキャン後にデフォルトで「1」が設定される。
- **DataGrid 小計カラム**: 税込金額 (`LineTotalWithTax`) を表示。`Math.Floor(UnitPrice × Quantity × (1 + TaxRate / 100))` で計算。
- **右パレット（お会計エリア）**:
  - Row1: 小計 (Subtotal)
  - Row2: 8%対象額 (TaxableAmount8)
  - Row3: 消費税 8% (TaxAmount8)
  - Row4: 10%対象額 (TaxableAmount10)
  - Row5: 消費税 10% (TaxAmount10)
  - Row6: 合計 (TotalAmount)
  - Row7: 預かり金額 / お釣り
  - Row8: 会計確定ボタン

### 4-4. 計算ロジック詳細
```
例: おにぎり梅 ¥120 (8%) × 1 + ポテトチップス ¥180 (10%) × 1

TaxableAmount8  = 120
TaxableAmount10 = 180
TaxAmount8  = Math.Floor(120 × 0.08) = Math.Floor(9.6) = 9
TaxAmount10 = Math.Floor(180 × 0.10) = Math.Floor(18.0) = 18
TaxAmount   = 9 + 18 = 27
TotalAmount = 300 + 27 = 327
```

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
