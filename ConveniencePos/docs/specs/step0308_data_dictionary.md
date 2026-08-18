# データ辞書: コンビニPOSシステム MVP

## 1. ドキュメント概要

| 項目 | 内容 |
|------|------|
| バージョン | 1.0 |
| 作成日 | 2026-08-18 |
| 役割 | 全テーブル・カラムの正式な定義 |

## 2. テーブル一覧

| テーブル名 | 説明 |
|-----------|------|
| Products | 商品マスタ |
| Transactions | 取引概要 |
| TransactionItems | 取引明細 |

## 3. Products（商品マスタ）

| カラム名 | データ型 | NULL可否 | 制約 | 説明 |
|----------|---------|---------|------|------|
| Id | int | NOT NULL | PRIMARY KEY, IDENTITY | 商品ID |
| JanCode | nvarchar(20) | NOT NULL | UNIQUE | JANコード（バーコード文字列） |
| Name | nvarchar(100) | NOT NULL | | 商品名 |
| Price | decimal(18,2) | NOT NULL | CHECK (Price >= 0) | 税抜価格 |
| TaxRate | int | NOT NULL | CHECK (TaxRate IN (8, 10)) | 税率（8: 軽減税率, 10: 標準税率） |

### シードデータ

| Id | JanCode | Name | Price | TaxRate |
|----|---------|------|-------|---------|
| 1 | 777777 | おにぎり 梅 | 120.00 | 8 |
| 2 | 888888 | 緑茶 500ml | 150.00 | 8 |
| 3 | 999999 | ポテトチップス | 180.00 | 10 |
| 4 | 111111 | ティッシュ | 200.00 | 10 |
| 5 | 222222 | コーヒー 熱 350ml | 110.00 | 10 |

## 4. Transactions（取引概要）

| カラム名 | データ型 | NULL可否 | 制約 | 説明 |
|----------|---------|---------|------|------|
| Id | int | NOT NULL | PRIMARY KEY, IDENTITY | 取引ID |
| CreatedAt | datetime2 | NOT NULL | | 取引日時（UTC） |
| TotalAmount | decimal(18,2) | NOT NULL | | 税込合計金額 |
| TaxAmount | decimal(18,2) | NOT NULL | | 消費税合計額 |

## 5. TransactionItems（取引明細）

| カラム名 | データ型 | NULL可否 | 制約 | 説明 |
|----------|---------|---------|------|------|
| Id | int | NOT NULL | PRIMARY KEY, IDENTITY | 明細ID |
| TransactionId | int | NOT NULL | FOREIGN KEY → Transactions.Id | 親取引ID |
| ProductId | int | NOT NULL | FOREIGN KEY → Products.Id | 商品ID |
| Quantity | int | NOT NULL | CHECK (Quantity >= 1) | 数量 |
| UnitPrice | decimal(18,2) | NOT NULL | CHECK (UnitPrice >= 0) | 販売時単価（税抜） |
| AppliedTaxRate | int | NOT NULL | CHECK (AppliedTaxRate IN (8, 10)) | 購入時点の適用税率 |

## 6. リレーション

```
Products (1) ──< (N) TransactionItems
Transactions (1) ──< (N) TransactionItems
```

## 7. インデックス

| テーブル | カラム | 種類 | 目的 |
|----------|--------|------|------|
| Products | JanCode | UNIQUE | バーコード検索の高速化 |
| TransactionItems | TransactionId | INDEX | 取引明細の一括取得 |
| TransactionItems | ProductId | INDEX | 商品別の売上集計 |

## 8. データ保持方針

| テーブル | 保持期間 | 理由 |
|----------|---------|------|
| Products | 無期限 | 商品マスタ |
| Transactions | 7年間 | 日本消費税法に基づく帳簿書類の保存義務 |
| TransactionItems | 7年間 | 取引明細 |

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
