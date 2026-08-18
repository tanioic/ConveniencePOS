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

## 4. サービスインターフェース

### 4.1. IBarcodeService

```csharp
public interface IBarcodeService
{
    Task<Product?> LookupByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
}
```

| メソッド | 入力 | 出力 | 動作 |
|----------|------|------|------|
| `LookupByBarcodeAsync` | `string barcode`, `CancellationToken` | `Product?` | JANコードに合致する商品をDBから検索し、存在しなければ `null` を返す |

### 4.2. ITransactionService

```csharp
public interface ITransactionService
{
    Task<Transaction> SaveTransactionAsync(
        decimal totalAmount,
        decimal taxAmount,
        IReadOnlyList<TransactionItem> items,
        CancellationToken cancellationToken = default);
}
```

### 4.3. IReceiptService

```csharp
public interface IReceiptService
{
    string GenerateReceipt(ReceiptContext context);
    Task SaveReceiptAsync(int transactionId, string receiptContent, CancellationToken cancellationToken = default);
}

public record ReceiptItem(string Name, int Quantity, decimal LineTotalWithTax, int TaxRate);

public record ReceiptContext(
    int TransactionId, DateTime TransactionTime, IReadOnlyList<ReceiptItem> Items,
    decimal Subtotal, decimal TaxableAmount8, decimal TaxableAmount10,
    decimal TaxAmount8, decimal TaxAmount10, decimal TaxAmount, decimal TotalAmount,
    decimal ReceivedAmount, decimal Change);
```

### 4.3. 例外

- DB接続エラーが発生した場合、例外は呼び出し元（ViewModel）に伝播する
- MVP では例外ハンドリングは ViewModel 側で `try-catch` により行う

## 5. BarcodeService 実装

### 5.1. 依存関係

```
BarcodeService
  ├── IDbContextFactory<PosDbContext> (コンストラクタ経由で注入)
  └── ILogger<BarcodeService> (コンストラクタ経由で注入)
```

### 5.2. 実装コード

```csharp
public class BarcodeService : IBarcodeService
{
    private readonly IDbContextFactory<PosDbContext> _contextFactory;
    private readonly ILogger<BarcodeService> _logger;

    public BarcodeService(IDbContextFactory<PosDbContext> contextFactory, ILogger<BarcodeService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<Product?> LookupByBarcodeAsync(string barcode, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(barcode);
        _logger.LogDebug("バーコード検索: {Barcode}", barcode);
        await using var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        return await dbContext.Products
            .FirstOrDefaultAsync(p => p.JanCode == barcode, cancellationToken);
    }
}
```

### 5.3. パフォーマンス要件

- 商品検索は `JanCode` カラムのインデックスを使用する（DB設計でUNIQUE制約あり）
- 応答時間: 100ms以内（MVP要件）

## 6. ViewModel への DI 注入

### 6.1. MainViewModel のコンストラクタ

```csharp
public MainViewModel(
    IBarcodeService barcodeService,
    ITransactionService transactionService,
    IReceiptService receiptService,
    ILogger<MainViewModel> logger)
{
    _barcodeService = barcodeService;
    _transactionService = transactionService;
    _receiptService = receiptService;
    _logger = logger;
}
```

### 6.2. 本番環境での利用（App.xaml.cs）

```csharp
services.AddLogging(builder => { builder.AddConsole(); });
services.AddDbContextFactory<PosDbContext>(options => options.UseSqlServer(connectionString));
services.AddSingleton<IBarcodeService, BarcodeService>();
services.AddSingleton<ITransactionService, TransactionService>();
services.AddSingleton<IReceiptService>(sp => { ... });
services.AddSingleton<MainViewModel>();
```

> **DI ライフタイム方針**: ViewModel は WPF ウィンドウの生命周期と同一であるため、全サービスを `AddSingleton` で登録する。DbContext は `IDbContextFactory` を使用して毎操作ごとに短寿命インスタンスを生成し、スレッドセーフ性とライフタイム問題を回避する。

### 6.3. テスト環境での利用

```csharp
// テストコードでモックを注入
var mockBarcodeService = new Mock<IBarcodeService>();
var mockTransactionService = new Mock<ITransactionService>();
var mockReceiptService = new Mock<IReceiptService>();
var mockLogger = new Mock<ILogger<MainViewModel>>();
var vm = new MainViewModel(
    mockBarcodeService.Object,
    mockTransactionService.Object,
    mockReceiptService.Object,
    mockLogger.Object);
```

### 6.4. DI コンテナの採用

Microsoft.Extensions.DependencyInjection を App.xaml.cs で使用し、コンストラクタ経由でサービスを注入する。

| 理由 | 説明 |
|------|------|
| テスト容易性 | モック注入による単体テストが容易 |
| 切り離し容易性 | サービス実装の変更が ViewModel に影響しない |
| 拡張性 | 新規サービス追加時に DI 登録のみで対応可能 |

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
              ├──> ITransactionService.SaveTransactionAsync()
              │         │
              │         v
              │    [TransactionService] ──> [PosDbContext] ──> [SQL Server]
              │         │
              │         └── Transaction を返却
              │
              └── レシート出力 → デスクトップ
```

## 8. テストカバレッジ

### 8.1. BarcodeService のテスト方針

- 単体テスト: モックを使用した `IBarcodeService` の呼び出しテスト（MainViewModelTests でカバー）
- 統合テスト: 実際の DB との接続テスト（TransactionIntegrationTests でカバー）

### 8.2. 現在のテスト状況

| テストクラス | テスト件数 | カバー対象 |
|-------------|----------|-----------|
| `CartItemViewModelTests` | 9件 | CartItemViewModel の計算ロジック |
| `MainViewModelTests` | 25件 | MainViewModel の合計計算、DI対応済み |
| `BarcodeServiceTests` | 10件 | バーコード検索ロジック |
| `TransactionServiceTests` | 8件 | 取引保存のバリデーション・永続化 |
| `TransactionIntegrationTests` | 9件 | 会計確定→DB保存→レシート出力 |
| `ModelTests` | 15件 | モデルクラスのプロパティ |
| **合計** | **76件** | - |

## バージョン履歴

| バージョン | 変更日 | 変更内容 | 変更者 |
|-----------|--------|----------|--------|
| 1.0 | 2026-08-18 | 初版作成 | 開発チーム |
| 1.1 | 2026-08-18 | ITransactionService, IReceiptService追加、CancellationToken対応、DI ライフタイム修正（Singleton統一）、ReceiptContext パラメータオブジェクト導入 | 開発チーム |
| 1.2 | 2026-08-18 | IDbContextFactory 導入によるスレッドセーフ DbContext 管理、全サービスに ILogger 注入、全公開メンバーに XML ドキュメント追加 | 開発チーム |
| 1.3 | 2026-08-18 | BarcodeService に ILogger 注入・ArgumentNullException追加、ReceiptService に null チェック追加、EnsureCreated() 導入によるマイグレーション不要化 | 開発チーム |

## 承認記録

| 役割 | 氏名 | 承認日 | 署名 |
|------|------|--------|------|
| プロジェクトオーナー | 田中太郎 | 2026-08-18 | ✓ |
| 技術リーダー | 田中太郎 | 2026-08-18 | ✓ |
| QAリーダー | 田中太郎 | 2026-08-18 | ✓ |
