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

### 4-2. ViewModel (`MainViewModel.cs`) (完了)
- `CartItemViewModel.TaxRate` (int): カート内商品の税率を保持するプロパティを追加。
- `AddItemAsync`: 商品追加時に `product.TaxRate` を `CartItemViewModel.TaxRate` に設定。
- 税率別集計プロパティ:
  - `TaxableAmount8`: 税率8%商品のLineTotal合算額
  - `TaxableAmount10`: 税率10%商品のLineTotal合算額
  - `TaxAmount8`: `Math.Floor(TaxableAmount8 * 0.08m)` (端数切捨て)
  - `TaxAmount10`: `Math.Floor(TaxableAmount10 * 0.10m)` (端数切捨て)
  - `TaxAmount`: `TaxAmount8 + TaxAmount10` (合算消費税)
  - `TotalAmount`: `Subtotal + TaxAmount`
- `ConfirmTransactionAsync`: 各 `TransactionItem` に `AppliedTaxRate = i.TaxRate` を設定して保存。
- `RefreshTotals`: 追加した全プロパティの `OnPropertyChanged` を通知。

### 4-3. UI (`MainWindow.xaml`) (完了)
- **DataGrid**: 「税率」カラムを追加。バインディング: `{Binding TaxRate, StringFormat='{}{0}%'}`
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
例: おにぎり梅 ¥160 (8%) × 1 + ポテトチップス ¥200 (10%) × 1

TaxableAmount8  = 160
TaxableAmount10 = 200
TaxAmount8  = Math.Floor(160 × 0.08) = Math.Floor(12.8) = 12
TaxAmount10 = Math.Floor(200 × 0.10) = Math.Floor(20.0) = 20
TaxAmount   = 12 + 20 = 32
TotalAmount = 360 + 32 = 392
```
