# サービス層アーキテクチャ: コンビニPOSシステム MVP

## 1. ドキュメント概要

| 項目 | 内容 |
|------|------|
| バージョン | 1.0 |
| 作成日 | 2026-08-18 |
| 役割 | サービス層の設計と依存関係の注入（DI）方針を定義する |
| 関連ドキュメント | `step0301_technical_plan.md`（技術計画）、`step0205_mvp_pos.md`（機能仕様） |

## 2. 設計方針

### 2.1. なぜサービス層を分離するのか

- **テスト容易性**: データベースアクセスを抽象化し、単体テストでモックを使用できる
- **責務分離**: ViewModel はUIロジックと計算に専念し、DB操作はサービス層が担当
- **将来拡張性**: サービス実装を変更してもViewModelに影響しない

### 2.2. 依存関係の原則

- ViewModel は具象クラスではなく**インターフェース**に依存する
- インターフェースと実装の両方を `Services/` フォルダに配置する
- テストではモックオブジェクトを注入して、DB接続なしにテスト可能にする

## 3. サービス一覧

| サービス名 | ファイルパス | 役割 |
|-----------|------------|------|
| `IBarcodeService` | `Services/IBarcodeService.cs` | バーコードによる商品検索のインターフェース |
| `BarcodeService` | `Services/BarcodeService.cs` | バーコード検索の実装（DB接続） |

## 4. IBarcodeService インターフェース

### 4.1. 定義

```csharp
public interface IBarcodeService
{
    Task<Product?> LookupByBarcodeAsync(string barcode);
}
```

### 4.2. 動作仕様

| メソッド | 入力 | 出力 | 動作 |
|----------|------|------|------|
| `LookupByBarcodeAsync` | `string barcode` (JANコード) | `Product?` | JANコードに合致する商品をDBから検索し、存在しなければ `null` を返す |

### 4.3. 例外

- DB接続エラーが発生した場合、例外は呼び出し元（ViewModel）に伝播する
- MVP では例外ハンドリングは ViewModel 側で `try-catch` により行う

## 5. BarcodeService 実装

### 5.1. 依存関係

```
BarcodeService
  └── PosDbContext (コンストラクタ経由で注入)
```

### 5.2. 実装コード

```csharp
public class BarcodeService : IBarcodeService
{
    private readonly PosDbContext _dbContext;

    public BarcodeService(PosDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Product?> LookupByBarcodeAsync(string barcode)
    {
        return await _dbContext.Products
            .FirstOrDefaultAsync(p => p.JanCode == barcode);
    }
}
```

### 5.3. パフォーマンス要件

- 商品検索は `JanCode` カラムのインデックスを使用する（DB設計でUNIQUE制約あり）
- 応答時間: 100ms以内（MVP要件）

## 6. ViewModel への DI 注入

### 6.1. MainViewModel のコンストラクタ

```csharp
// デフォルトコンストラクタ（本番用）
public MainViewModel()
    : this(new PosDbContext(), new BarcodeService(new PosDbContext())) { }

// テスト用コンストラクタ（DI注入）
public MainViewModel(PosDbContext dbContext, IBarcodeService barcodeService)
{
    _dbContext = dbContext;
    _barcodeService = barcodeService;
}
```

### 6.2. 本番環境での利用

```csharp
// MainWindow.xaml.cs でデフォルトコンストラクタを使用
var vm = new MainViewModel();
DataContext = vm;
```

### 6.3. テスト環境での利用

```csharp
// テストコードでモックを注入
var mockDbContext = new Mock<PosDbContext>();
var mockBarcodeService = new Mock<IBarcodeService>();
var vm = new MainViewModel(mockDbContext.Object, mockBarcodeService.Object);
```

### 6.4. DI コンテナ未使用の理由

MVP では Microsoft.Extensions.DependencyInjection を導入せず、**コンストラクタオーバーロード**による簡易DIパターンを採用する。

| 理由 | 説明 |
|------|------|
| 複雑性の回避 | DI コンテナの設定・管理はMVPの規模に対して過大 |
| WPFの制約 | App.xaml.cs での DI セットアップが必要 |
| 将来拡張 | 本番化時に DI コンテナへの移行は容易 |

## 7. データフロー図（サービス層含む）

```
[MainWindow.xaml]
    │ KeyDown (Enter)
    v
[MainViewModel]
    │ BarcodeInput プロパティ
    │
    ├──> IBarcodeService.LookupByBarcodeAsync()
    │         │
    │         v
    │    [BarcodeService] ──> [PosDbContext] ──> [SQL Server]
    │         │
    │         └── Product? を返却
    │
    ├──> CartItems に商品を追加/数量更新
    ├──> RefreshTotals() で合計再計算
    │
    └──> ConfirmTransactionAsync()
              │
              ├── [PosDbContext] に取引保存
              └── レシート出力 → デスクトップ
```

## 8. テストカバレッジ

### 8.1. BarcodeService のテスト方針

- 単体テスト: モックを使用した `IBarcodeService` の呼び出しテスト（MainViewModelTests でカバー）
- 統合テスト: 実際の DB との接続テスト（将来対応）

### 8.2. 現在のテスト状況

| テストクラス | テスト件数 | カバー対象 |
|-------------|----------|-----------|
| `CartItemViewModelTests` | 9件 | CartItemViewModel の計算ロジック |
| `MainViewModelTests` | 18件 | MainViewModel の合計計算、DI対応済み |
| `ModelTests` | 15件 | モデルクラスのプロパティ |
| **合計** | **42件** | - |

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
