# ConveniencePos - コンビニPOSシステム MVP

## 概要

コンビニ店舗向けPOS（Point of Sale）システムのMVP版。
バーコード入力による商品登録、軽減税率（8%）/標準税率（10%）の消費税計算、会計確定・DB保存・レシート出力を行う。

## 技術スタック

| 項目 | 技術 |
|------|------|
| 言語 | C# (.NET 8.0) |
| UI | WPF (Windows Presentation Foundation) |
| アーキテクチャ | MVVM (CommunityToolkit.Mvvm) |
| DB | SQL Server LocalDB |
| ORM | Entity Framework Core 8.x (コードファースト) |
| テスト | xUnit + Moq + coverlet |

## 構成

```
ConveniencePos/
├── Models/              # データモデル (Product, Transaction, TransactionItem)
├── ViewModels/          # MVVM ViewModel (MainViewModel, CartItemViewModel)
├── Views/               # WPF画面 (MainWindow)
├── Services/            # サービス層 (IBarcodeService, BarcodeService, ITransactionService, TransactionService, IReceiptService, ReceiptService)
├── Data/                # DBコンテキストとシードデータ
│   ├── PosDbContext.cs
│   └── Seed/ProductSeedData.cs
├── Migrations/          # EF Coreマイグレーション
├── appsettings.json     # 設定ファイル
└── ConveniencePos.csproj
```

## セットアップ

### 前提条件

- .NET 8.0 SDK
- SQL Server LocalDB (`(localdb)\MSSQLLocalDB`)

### ビルドと実行

```bash
# ビルド
dotnet build

# 実行
dotnet run --project ConveniencePos

# テスト実行
dotnet test

# カバレッジ計測
dotnet test --collect:"XPlat Code Coverage"
```

## 基本操作

1. バーコード入力欄にJANコードを入力 → Enterキーまたは「検索」ボタン
2. 商品がカートに追加される（同じ商品は数量+1）
3. 預かり金額を入力 → お釣りが自動計算
4. 「会計確定」ボタン押下 → DB保存 + デスクトップにレシート出力

## テストデータ（シードデータ）

| JANコード | 商品名 | 単価 | 税率 |
|-----------|--------|------|------|
| 777777 | おにぎり 梅 | ¥120 | 8% |
| 888888 | 緑茶 500ml | ¥150 | 8% |
| 999999 | ポテトチップス | ¥180 | 10% |
| 111111 | ティッシュ | ¥200 | 10% |
| 222222 | コーヒー 熱 350ml | ¥110 | 10% |

## ライセンス

MIT License
