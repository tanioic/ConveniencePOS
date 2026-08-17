# テスト戦略書 (Test Strategy)

## 1. 概要

ConveniencePos（コンビニPOSシステム）の品質保証ためのユニットテスト戦略を定義する。

## 2. テスト環境

| 項目 | 詳細 |
|------|------|
| **対象フレームワーク** | .NET 8.0-windows |
| **テストフレームワーク** | xUnit 2.9.3 |
| **モックライブラリ** | Moq 4.20.72 |
| **カバレッジツール** | coverlet.collector 6.0.4 |
| **テストプロジェクト** | `ConveniencePos.Tests` |

## 3. テストカバレッジ対象

### 3.1. Model層テスト (`Models/ProductTests.cs`)

| テスト対象 | テスト内容 |
|------------|-----------|
| `Product` デフォルト値 | `JanCode` が `string.Empty` で初期化されること |
| `Product` プロパティ設定 | 各プロパティ（Id, JanCode, Name, Price, TaxRate）が正しく設定・取得できること |
| `Transaction` デフォルト値 | `Items` が空の `List<TransactionItem>` で初期化されること |
| `Transaction` プロパティ設定 | CreatedAt, TotalAmount, TaxAmount が正しく設定・取得できること |
| `TransactionItem` プロパティ設定 | 全プロパティ（TransactionId, ProductId, Quantity, UnitPrice, AppliedTaxRate）が正しく設定・取得できること |

### 3.2. ViewModel層テスト (`ViewModels/MainViewModelTests.cs`)

#### CartItemViewModel テスト

| テスト対象 | テスト内容 |
|------------|-----------|
| `LineTotal` 計算 | `UnitPrice * Quantity` が正しく計算されること |
| `LineTotalWithTax` 計算（8%） | `Math.Floor(LineTotal * 1.08)` が正しく計算されること |
| `LineTotalWithTax` 計算（10%） | `Math.Floor(LineTotal * 1.10)` が正しく計算されること |
| `Quantity` 変更通知 | Quantity 変更時に `LineTotal` と `LineTotalWithTax` の `PropertyChanged` が発行されること |
| `Quantity` が0の場合 | LineTotal と LineTotalWithTax が 0 となること |

#### MainViewModel 計算プロパティ テスト

| テスト対象 | テスト内容 |
|------------|-----------|
| `Subtotal` | カート内全商品の `LineTotal` 合計が正しいこと |
| `TaxableAmount8` | 税率8%商品の `LineTotal` 合計が正しいこと |
| `TaxableAmount10` | 税率10%商品の `LineTotal` 合計が正しいこと |
| `TaxAmount8` | `Math.Floor(TaxableAmount8 * 0.08)` の計算が正しいこと |
| `TaxAmount10` | `Math.Floor(TaxableAmount10 * 0.10)` の計算が正しいこと |
| `TaxAmount` | `TaxAmount8 + TaxAmount10` の計算が正しいこと |
| `TotalAmount` | `Subtotal + TaxAmount` の計算が正しいこと |
| `Change` | `ReceivedAmount - TotalAmount` のお釣り計算が正しいこと |
| `Change`（支払い不足時） | お預かり金額が合計未満の場合、0 が返ること |

#### MainViewModel 複合シナリオ テスト

| テスト対象 | テスト内容 |
|------------|-----------|
| 軽減税率（8%）のみカート | おにぎり等の8%対象商品のみの場合の計算が正しいこと |
| 標準税率（10%）のみカート | チップス等の10%対象商品のみの場合の計算が正しいこと |
| 混合税率カート | 8%と10%の商品が混在する場合の計算が正しいこと（日本の消費税ルール準拠） |

### 3.3. テストカバレッジ対象外

| 対象 | 理由 |
|------|------|
| `PosDbContext` | インテグレーションテストの対象（本テストスコープ外） |
| `AddItemCommand` | DB アクセスを伴うため、結合テストで検証 |
| `ConfirmTransactionCommand` | DB アクセス・ファイル書き込みを伴うため、結合テストで検証 |
| `MainWindow` (View) | UI テストの対象（本テストスコープ外） |

## 4. テスト実行

```bash
# テスト実行
dotnet test ConveniencePos.Tests/ConveniencePos.Tests.csproj

# カバレッジ付き実行
dotnet test ConveniencePos.Tests/ConveniencePos.Tests.csproj --collect:"XPlat Code Coverage"

# レポート生成
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:./TestResults/**/coverage.cobertura.xml -targetdir:./TestResults/coverage_report
```

## 5. テスト件数目標

| レイヤー | 目標件数 |
|----------|---------|
| Model層 | 10件以上 |
| ViewModel層 | 20件以上 |
| **合計** | **30件以上** |
