# 技術計画: コンビニPOSシステム MVP

## 1. データモデル構造 (Data Models)
- **Product (商品情報)**
  - `Id` (int, 主キー)
  - `JanCode` (string, ユニークキー, バーコード文字列)
  - `Name` (string, 商品名)
  - `Price` (decimal, 税抜価格)
- **Transaction (取引概要)**
  - `Id` (int, 主キー)
  - `CreatedAt` (DateTime, 取引日時)
  - `TotalAmount` (decimal, 税込合計金額)
  - `TaxAmount` (decimal, 内消費税額10%)
- **TransactionItem (取引明細)**
  - `Id` (int, 主キー)
  - `TransactionId` (int, 外部キー)
  - `ProductId` (int, 外部キー)
  - `Quantity` (int, 数量)
  - `UnitPrice` (decimal, 販売時単価)

## 2. 依存関係の注入とサービス
- **PosDbContext**: Entity Framework Coreのデータベースコンテキスト。
- **IBarcodeService**: バーコードの入力をシミュレートまたは検知し、ViewModelに通知するサービス。
