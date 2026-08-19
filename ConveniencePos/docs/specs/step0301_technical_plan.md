# 技術計画: コンビニPOSシステム MVP

## 1. 既存ドキュメントとの関係

本ドキュメントは `How（どうやって）` を定義する。技術スタック・DB設計・UI設計・テスト戦略の詳細は既存ドキュメントを参照する。

| テーマ | 参照先 |
|--------|--------|
| 技術スタック・コード規約 | `step0100_constitution.md` |
| データモデル構造 | 本ドキュメント §2 |
| 軽減税率の仕様 | `step0206_extension_tax_rate.md` |
| UI レイアウト | `step0204_ui.md` |
| レシート出力仕様 | `step0207_extension_receipt_simple.md` |
| テスト戦略・カバレッジ対象 | `step0302_teststrategy.md` |
| 機能仕様（何をやるか） | `step0205_mvp_pos.md` |
| 要件定義（Why / 受け入れ基準） | `step0200_spec.md` |
| サービス層設計方針 | `step0208_service_architecture.md` |

## 2. データモデル構造 (Data Models)

### 2.1. Product (商品情報)
| フィールド名 | 型 | 制約 | 説明 |
|---|---|---|---|
| `Id` | int | PRIMARY KEY, IDENTITY | 商品ID |
| `JanCode` | string | UNIQUE, NOT NULL | JANコード（バーコード文字列） |
| `Name` | string | NOT NULL | 商品名 |
| `Price` | decimal | NOT NULL, CHECK (>= 0) | 税抜価格 |
| `TaxRate` | int | NOT NULL, CHECK (8 or 10) | 税率: 8 or 10 |

### 2.2. Transaction (取引概要)
| フィールド名 | 型 | 制約 | 説明 |
|---|---|---|---|
| `Id` | int | PRIMARY KEY, IDENTITY | 取引ID |
| `CreatedAt` | DateTime | NOT NULL | 取引日時（UTC） |
| `TotalAmount` | decimal | NOT NULL | 税込合計金額 |
| `TaxAmount` | decimal | NOT NULL | 消費税合計額 |
| `Items` | ICollection\<TransactionItem\> | | 取引明細コレクション |

### 2.3. TransactionItem (取引明細)
| フィールド名 | 型 | 制約 | 説明 |
|---|---|---|---|
| `Id` | int | PRIMARY KEY, IDENTITY | 明細ID |
| `TransactionId` | int | FOREIGN KEY → Transactions.Id | 親取引ID |
| `ProductId` | int | FOREIGN KEY → Products.Id | 商品ID |
| `Quantity` | int | NOT NULL, CHECK (>= 1) | 数量 |
| `UnitPrice` | decimal | NOT NULL, CHECK (>= 0) | 販売時単価（税抜） |
| `AppliedTaxRate` | int | NOT NULL, CHECK (8 or 10) | 購入時点の適用税率 |

## 3. ViewModel 定義

### 3.1. MainViewModel プロパティ
| プロパティ名 | 型 | 説明 |
|---|---|---|
| `BarcodeInput` | string | バーコード入力値 |
| `ReceivedAmount` | decimal | 預かり金額 |
| `CartItems` | ObservableCollection\<CartItemViewModel\> | カート明細 |
| `Subtotal` | decimal | 税抜合計（算出） |
| `TaxableAmount8` | decimal | 8%対象額（算出） |
| `TaxableAmount10` | decimal | 10%対象額（算出） |
| `TaxAmount8` | decimal | 8%消費税（算出、端数切り捨て） |
| `TaxAmount10` | decimal | 10%消費税（算出、端数切り捨て） |
| `TaxAmount` | decimal | 消費税合計（算出） |
| `TotalAmount` | decimal | 税込合計（算出） |
| `Change` | decimal | お釣り（算出） |

### 3.2. MainViewModel コマンド
| コマンド名 | 処理 |
|---|---|
| `AddItemCommand` | JANコードで商品検索→カートに追加 or 数量+1 |
| `ConfirmTransactionCommand` | 取引保存→レシート生成(Desktop)→カートクリア |

### 3.3. MainViewModel メソッド
| メソッド名 | 処理 |
|---|---|
| `RefreshTotals()` | 全合計プロパティの再計算・通知 |
| `AddItemAsync()` | 商品追加時の非同期処理 |
| `ConfirmTransactionAsync()` | 会計確定時の非同期処理 |
| `OnCartItemPropertyChanged()` | カート内商品のプロパティ変更イベントハンドラ |

### 3.4. CartItemViewModel プロパティ
| プロパティ名 | 型 | 説明 |
|---|---|---|
| `ProductId` | int | 商品ID |
| `Name` | string | 商品名 |
| `UnitPrice` | decimal | 単価 |
| `TaxRate` | int | 税率 (8 or 10) |
| `Quantity` | int | 数量（最小値: 1、未満は `ArgumentOutOfRangeException` 投出） |
| `LineTotal` | decimal | 税抜小計（算出: UnitPrice x Quantity） |
| `LineTotalWithTax` | decimal | 税込小計（算出: Floor(LineTotal x (1+TaxRate/100))） |

## 4. 計算ロジック（変数名定義）

### 4.1. 税込小計計算（商品単位）
```
LineTotalWithTax = Math.Floor(UnitPrice × Quantity × (1 + TaxRate / 100))
```

### 4.2. 税率別集計
```
TaxableAmount8  = カート内すべての8%商品の (UnitPrice × Quantity) の合計
TaxableAmount10 = カート内すべての10%商品の (UnitPrice × Quantity) の合計
```

### 4.3. 消費税額計算（税率別）
```
TaxAmount8  = Math.Floor(TaxableAmount8 × 0.08)
TaxAmount10 = Math.Floor(TaxableAmount10 × 0.10)
```

### 4.4. 合計金額
```
TaxAmount  = TaxAmount8 + TaxAmount10
Subtotal   = TaxableAmount8 + TaxableAmount10
TotalAmount = Subtotal + TaxAmount
```

### 4.5. お釣り計算
```
Change = ReceivedAmount - TotalAmount
ReceivedAmount < TotalAmount の場合: Change = 0
```

### 4.6. 計算例
```
【例1: 8%商品のみ】
おにぎり梅 ¥120 (8%) × 2個
LineTotalWithTax = Floor(120 × 2 × 1.08) = Floor(259.2) = 259
TaxAmount8 = Floor(240 × 0.08) = Floor(19.2) = 19
TotalAmount = 240 + 19 = 259

【例2: 10%商品のみ】
ポテトチップス ¥180 (10%) × 1個
LineTotalWithTax = Floor(180 × 1 × 1.10) = Floor(198) = 198
TaxAmount10 = Floor(180 × 0.10) = Floor(18) = 18
TotalAmount = 180 + 18 = 198

【例3: 混合税率】
おにぎり ¥120 (8%) × 1 + チップス ¥180 (10%) × 1
TaxableAmount8  = 120
TaxableAmount10 = 180
TaxAmount8  = Floor(120 × 0.08) = Floor(9.6) = 9
TaxAmount10 = Floor(180 × 0.10) = Floor(18.0) = 18
Subtotal    = 300
TaxAmount   = 9 + 18 = 27
TotalAmount = 300 + 27 = 327
```

## 5. サービスインターフェース定義 (C#)

### 5.1. IBarcodeService
```csharp
public interface IBarcodeService
{
    Task<Product?> LookupByBarcodeAsync(string barcode, CancellationToken cancellationToken = default);
}
```

### 5.2. ITransactionService
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

### 5.3. IReceiptService
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

## 6. サービス実装 (C#)

### 6.1. BarcodeService
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

### 6.2. 依存関係
```
BarcodeService
  ├── IDbContextFactory<PosDbContext> (コンストラクタ経由で注入)
  └── ILogger<BarcodeService> (コンストラクタ経由で注入)
```

### 6.3. TransactionService
```csharp
public class TransactionService : ITransactionService
{
    private readonly IDbContextFactory<PosDbContext> _contextFactory;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(IDbContextFactory<PosDbContext> contextFactory, ILogger<TransactionService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<Transaction> SaveTransactionAsync(
        decimal totalAmount,
        decimal taxAmount,
        IReadOnlyList<TransactionItem> items,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count == 0)
            throw new ArgumentException("取引明細は1件以上必要です。", nameof(items));

        if (totalAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(totalAmount), totalAmount, "合計金額は0以上である必要があります。");

        if (taxAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(taxAmount), taxAmount, "消費税額は0以上である必要があります。");

        var transaction = new Transaction
        {
            CreatedAt = DateTime.UtcNow,
            TotalAmount = totalAmount,
            TaxAmount = taxAmount,
            Items = items.ToList()
        };

        await using var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
        dbContext.Transactions.Add(transaction);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("取引 TRX-{TransactionId} を保存しました (合計: {TotalAmount})", transaction.Id, totalAmount);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "取引保存に失敗しました (合計: {TotalAmount})", totalAmount);
            throw new InvalidOperationException(
                "取引の保存に失敗しました。データベース接続を確認してください。", ex);
        }

        return transaction;
    }
}
```

#### TransactionService 依存関係
```
TransactionService
  ├── IDbContextFactory<PosDbContext> (コンストラクタ経由で注入)
  └── ILogger<TransactionService> (コンストラクタ経由で注入)
```

### 6.4. ReceiptService
```csharp
public class ReceiptService : IReceiptService
{
    private readonly string _storeName;
    private readonly string _registerNumber;
    private readonly string _operatorName;
    private readonly string _outputDirectory;
    private readonly int _width;
    private readonly ILogger<ReceiptService> _logger;

    public ReceiptService(
        string storeName = "Convenience POS Store",
        string registerNumber = "レジ#01",
        string operatorName = "谷本 レジ担当",
        string outputDirectory = "Desktop",
        int width = 32,
        ILogger<ReceiptService>? logger = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storeName);
        ArgumentException.ThrowIfNullOrWhiteSpace(registerNumber);
        ArgumentException.ThrowIfNullOrWhiteSpace(operatorName);
        ArgumentException.ThrowIfNullOrWhiteSpace(outputDirectory);

        _storeName = storeName;
        _registerNumber = registerNumber;
        _operatorName = operatorName;
        _outputDirectory = outputDirectory;
        _width = width;
        _logger = logger ?? NullLogger<ReceiptService>.Instance;
    }

    public string GenerateReceipt(ReceiptContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        var sb = new StringBuilder();
        sb.AppendLine(new string('=', _width));
        sb.AppendLine(CenterText(_storeName, _width));
        sb.AppendLine($"{_registerNumber}  担当: {_operatorName}");
        sb.AppendLine();
        sb.AppendLine($"取引番号: TRX-{context.TransactionId}");
        sb.AppendLine(context.TransactionTime.ToString("yyyy/MM/dd HH:mm"));
        sb.AppendLine();
        foreach (var item in context.Items)
        {
            sb.AppendLine($"{item.Name} {item.Quantity}  ¥{item.LineTotalWithTax:N0} {item.TaxRate}%");
        }
        sb.AppendLine(AmountLine("税抜合計", $"¥{context.Subtotal:N0}"));
        sb.AppendLine($"8% 対象: ¥{context.TaxableAmount8:N0} 消費税: ¥{context.TaxAmount8:N0}");
        sb.AppendLine($"10%対象: ¥{context.TaxableAmount10:N0} 消費税: ¥{context.TaxAmount10:N0}");
        sb.AppendLine(AmountLine("消費税合計", $"¥{context.TaxAmount:N0}"));
        sb.AppendLine();
        sb.AppendLine(AmountLine("税込合計", $"¥{context.TotalAmount:N0}"));
        sb.AppendLine(AmountLine("[現金] お預かり", $"¥{context.ReceivedAmount:N0}"));
        sb.AppendLine(AmountLine("お釣り", $"¥{context.Change:N0}"));
        sb.AppendLine("ありがとうお越し下さいました");
        return sb.ToString();
    }

    public async Task SaveReceiptAsync(int transactionId, string receiptContent, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNullOrWhiteSpace(receiptContent);

        var basePath = ResolveOutputDirectory();
        Directory.CreateDirectory(basePath);

        var filePath = Path.Combine(basePath, $"receipt_{transactionId}.txt");
        await File.WriteAllTextAsync(filePath, receiptContent, cancellationToken);
        _logger.LogDebug("レシートを保存しました: {FilePath}", filePath);
    }

    private string ResolveOutputDirectory() => _outputDirectory switch
    {
        "Desktop" => Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
        _ => _outputDirectory
    };

    private int DisplayWidth(string s)
    {
        int width = 0;
        foreach (var c in s)
        {
            if (c >= 0x3000 && c < 0xA000) width += 2;
            else if (c >= 0xAC00 && c < 0xD800) width += 2;
            else if (c >= 0xF900 && c < 0xFB00) width += 2;
            else if (c >= 0xFF01 && c < 0xFF5F) width += 2;
            else width += 1;
        }
        return width;
    }

    private string CenterText(string text, int totalWidth)
    {
        int pad = Math.Max(0, (totalWidth - DisplayWidth(text)) / 2);
        return new string(' ', pad) + text;
    }

    private string AmountLine(string label, string amount)
    {
        int spaces = _width - DisplayWidth(label) - DisplayWidth(amount);
        return label + new string(' ', Math.Max(1, spaces)) + amount;
    }
}
```

#### ReceiptService 依存関係
```
ReceiptService
  ├── string storeName (店舗名、デフォルト: "Convenience POS Store")
  ├── string registerNumber (レジ番号、デフォルト: "レジ#01")
  ├── string operatorName (担当者名、デフォルト: "谷本 レジ担当")
  ├── string outputDirectory (出力先、デフォルト: "Desktop")
  ├── int width (レシート幅、デフォルト: 32)
  └── ILogger<ReceiptService> (コンストラクタ経由で注入)
```

### 6.5. パフォーマンス要件
- 商品検索は `JanCode` カラムのインデックスを使用する（DB設計でUNIQUE制約あり）
- 応答時間: 100ms以内（MVP要件）

## 7. ViewModel の DI 注入 (C#)

### 7.1. MainViewModel のコンストラクタ
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

### 7.2. 本番環境での利用（App.xaml.cs）
```csharp
services.AddLogging(builder => { builder.AddConsole(); });
services.AddDbContextFactory<PosDbContext>(options => options.UseSqlServer(connectionString));
services.AddSingleton<IBarcodeService, BarcodeService>();
services.AddSingleton<ITransactionService, TransactionService>();
services.AddSingleton<IReceiptService>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    return new ReceiptService(
        storeName: config["Receipt:StoreName"] ?? "Convenience POS Store",
        registerNumber: config["Receipt:RegisterNumber"] ?? "レジ#01",
        operatorName: config["Receipt:OperatorName"] ?? "谷本 レジ担当",
        outputDirectory: config["Receipt:OutputDirectory"] ?? "Desktop",
        width: int.TryParse(config["Receipt:Width"], out var w) ? w : 32,
        logger: loggerFactory.CreateLogger<ReceiptService>());
});
services.AddSingleton<MainViewModel>();
```

> **DI ライフタイム方針**: ViewModel は WPF ウィンドウの生命周期と同一であるため、全サービスを `AddSingleton` で登録する。DbContext は `IDbContextFactory` を使用して毎操作ごとに短寿命インスタンスを生成し、スレッドセーフ性とライフタイム問題を回避する。

### 7.3. テスト環境での利用
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

## 8. 例外処理の実装パターン (C#)
```csharp
// TransactionService の例（IDbContextFactory 使用）
public async Task<Transaction> SaveTransactionAsync(
    decimal totalAmount, decimal taxAmount,
    IReadOnlyList<TransactionItem> items, CancellationToken cancellationToken = default)
{
    var transaction = new Transaction { ... };
    await using var dbContext = await _contextFactory.CreateDbContextAsync(cancellationToken);
    dbContext.Transactions.Add(transaction);
    try
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateException ex)
    {
        throw new InvalidOperationException(
            "取引の保存に失敗しました。データベース接続を確認してください。", ex);
    }
    return transaction;
}
```

## 9. タスク間の依存関係と実装順序

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

## 10. 既存コードとの差分管理

### 10.1. モデル層
- `Product.TaxRate` と `TransactionItem.AppliedTaxRate` は既に追加済み（`step0206_extension_tax_rate.md` 実装済み）
- 新規追加不要

### 10.2. ViewModel層
- `MainViewModel` の税率別集計ロジック（TaxableAmount8/10, TaxAmount8/10）は既に実装済み
- `CartItemViewModel.LineTotalWithTax` は既に実装済み
- 追加実装が必要な箇所: なし（MVP機能は全て実装済み）

### 10.3. テスト層
- `ConveniencePos.Tests` プロジェクトは既に存在し、xUnit + Moq + coverlet が導入済み
- `Models/ModelTests.cs`: 15件のModel層テストが実装済み
- `ViewModels/ViewModelTests.cs`: 27件のViewModel層テストが実装済み
- 残タスク: Task 4.6（全件パス確認）と Task 4.7（カバレッジ計測）のみ

## 11. データフロー図

```
[ユーザー] 
    | バーコード入力 / 預かり金額入力 / ボタン操作
    v
[MainWindow.xaml] --バインディング--> [MainViewModel]
    |                                       |
    |                                       | バーコード検索
    |                                       v
    |                              [IBarcodeService] --DI--> [BarcodeService]
    |                                       |
    |                                       v
    |                              [PosDbContext] --EF Core--> [SQL Server]
    |                                       |
    |                                       | 取引保存
    |                                       v
    |                              [ITransactionService] --DI--> [TransactionService]
    |                                       |
    |                                       v
    |                              [PosDbContext] --EF Core--> [SQL Server]
    |                                       |
    |                                       | レシート出力
    |                                       v
    |                              [IReceiptService] --DI--> [ReceiptService]
    |                                       |
    |                                       v
    |                              [File.WriteAllTextAsync] --> [デスクトップ/receipt_[ID].txt]
    |
    | UI更新（PropertyChanged通知）
    v
[画面表示]
```

## 12. テストカバレッジ

| テストクラス | ファイルパス | 件数 | カバー対象 |
|-------------|------------|------|-----------|
| CartItemViewModelTests | ViewModels/ViewModelTests.cs | 9件 | CartItemViewModel の計算ロジック |
| MainViewModelTests | ViewModels/ViewModelTests.cs | 25件 | MainViewModel の合計計算、DI対応 |
| BarcodeServiceTests | Services/BarcodeServiceTests.cs | 10件 | バーコード検索ロジック |
| TransactionServiceTests | Services/TransactionServiceTests.cs | 8件 | 取引保存のバリデーション・永続化 |
| TransactionIntegrationTests | Integration/TransactionIntegrationTests.cs | 9件 | 会計確定→DB保存→レシート出力 |
| ModelTests | Models/ModelTests.cs | 15件 | モデルクラスのプロパティ |
| **合計** | | **76件** | |

## バージョン履歴

| バージョン | 変更日 | 変更内容 | 変更者 |
|-----------|--------|----------|--------|
| 1.0 | 2026-08-18 | 初版作成 | 開発チーム |
| 1.1 | 2026-08-18 | BarcodeService DI設計反映 | 開発チーム |
| 1.2 | 2026-08-18 | ITransactionService/IReceiptService追加、CancellationToken対応、DbUpdateExceptionハンドリング、ReceiptContext導入、非同期ファイル出力対応 | 開発チーム |
| 1.3 | 2026-08-18 | IDbContextFactory 導入、全サービスに ILogger 注入、接続文字列起動時バリデーション、エラーハンドリング粒度改善、XML ドキュメント追加 | 開発チーム |
| 1.4 | 2026-08-18 | ウォーターフォール観点の修正: 上流仕様書(step0204/0206/0208等)から除去したViewModel定義・サービスインターフェース・計算ロジック・C#コードをすべて本ドキュメントに集約 | 開発チーム |
| 1.5 | 2026-08-18 | TransactionService/ReceiptService実装を完全に記述、依存関係図を追加、DI登録詳細を補完 | 開発チーム |

## 承認記録

| 役割 | 氏名 | 承認日 | 署名 |
|------|------|--------|------|
| プロジェクトオーナー | 田中太郎 | 2026-08-18 | ✓ |
| 技術リーダー | 田中太郎 | 2026-08-18 | ✓ |
| QAリーダー | 田中太郎 | 2026-08-18 | ✓ |
