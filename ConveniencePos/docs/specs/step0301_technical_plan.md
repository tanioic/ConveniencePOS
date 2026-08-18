# 技術計画: コンビニPOSシステム MVP

## 1. データモデル構造 (Data Models)
- **Product (商品情報)**
  - `Id` (int, 主キー)
  - `JanCode` (string, ユニークキー, バーコード文字列)
  - `Name` (string, 商品名)
  - `Price` (decimal, 税抜価格)
  - `TaxRate` (int, 税率: 8 or 10)
- **Transaction (取引概要)**
  - `Id` (int, 主キー)
  - `CreatedAt` (DateTime, 取引日時)
  - `TotalAmount` (decimal, 税込合計金額)
  - `TaxAmount` (decimal, 消費税合計額)
- **TransactionItem (取引明細)**
  - `Id` (int, 主キー)
  - `TransactionId` (int, 外部キー)
  - `ProductId` (int, 外部キー)
  - `Quantity` (int, 数量)
  - `UnitPrice` (decimal, 販売時単価)
  - `AppliedTaxRate` (int, 購入時点の適用税率)

## 2. 依存関係の注入とサービス
- **PosDbContext**: Entity Framework Coreのデータベースコンテキスト。`IDbContextFactory<PosDbContext>` を通じて毎操作ごとに短寿命インスタンスを生成。
- **IBarcodeService**: バーコードJANコードによる商品検索を行うインターフェース。全メソッドに `CancellationToken` 対応。詳細は `step0208_service_architecture.md` を参照。
- **BarcodeService**: `IBarcodeService` の実装。`IDbContextFactory` を受け取り、毎回新しい DbContext を生成して DB から商品を検索する。
- **ITransactionService**: 取引保存を行うインターフェース。`SaveTransactionAsync` で `CancellationToken` 対応、`DbUpdateException` ハンドリング付き。
- **TransactionService**: `ITransactionService` の実装。`IDbContextFactory` と `ILogger` を使用。
- **IReceiptService**: レシート生成・保存を行うインターフェース。`ReceiptContext` レコードを使用してパラメータを整理。`SaveReceiptAsync` で非同期ファイル書き込み。
- **DI方針**: `AddSingleton` で全サービスを登録。DbContext は `AddDbContextFactory` でファクトリ登録し、毎操作ごとに短寿命インスタンスを生成してスレッドセーフ性を確保。

## 3. 外部システム連携

### 3.1. 現在の連携対象
- なし（MVPは単体システム）

### 3.2. 将来の連携候補

| 連携対象 | データ形式 | 同期方式 | 優先度 |
|----------|-----------|----------|--------|
| 会計レジ管理システム | CSV/API | バッチ/リアルタイム | 中 |
| 在庫管理システム | CSV/API | バッチ | 中 |
| 会計ソフト（弥生等） | CSV | バッチ | 低 |

### 3.3. データエクスポート仕様
- レシート: テキストファイル（固定幅32文字） -> デスクトップ（`ReceiptService.SaveReceiptAsync` で非同期書き込み）
- 取引データ: SQL Server に直接保存
- 将来の売上集計: SQL クエリまたはビューで集計

## 4. 障害対策・エラーハンドリング設計

### 4.1. エラーハンドリング方針

| エラーレベル | 処理方針 | ユーザーへの表示 |
|-------------|----------|-----------------|
| 致命的 (DB接続不可) | アプリケーション継続不可能 | エラーメッセージ + 終了 |
| 重大 (DB保存失敗) | カート維持、再試行可能 | エラーメッセージ |
| 軽微 (商品未発見) | 処理続行 | エラーメッセージ |
| 軽微 (レシート出力失敗) | 取引は保存済み、処理続行 | エラーログのみ |

### 4.2. 例外処理の実装パターン
```csharp
// TransactionService の例
public async Task<Transaction> SaveTransactionAsync(
    decimal totalAmount, decimal taxAmount,
    IReadOnlyList<TransactionItem> items, CancellationToken cancellationToken = default)
{
    var transaction = new Transaction { ... };
    _dbContext.Transactions.Add(transaction);
    try
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateException ex)
    {
        throw new InvalidOperationException(
            "取引の保存に失敗しました。データベース接続を確認してください。", ex);
    }
    return transaction;
}
```

### 4.3. データ整合性保証
- Transaction + TransactionItem は同一トランザクションで保存
- AppliedTaxRate は購入時点のスナップショットとして記録
- 外部キー制約で不正な参照を防止

## 5. ログ設計

### 5.1. ログレベル定義

| レベル | 使用場所 | 出力先 |
|--------|----------|--------|
| ERROR | DB保存失敗、DB接続エラー | コンソール + ファイル（将来） |
| WARN | レシート出力失敗 | コンソール |
| INFO | 取引完了、商品追加 | コンソール（開発時のみ） |
| DEBUG | 各処理の開始/終了 | コンソール（開発時のみ） |

### 5.2. ログフォーマット
```
[yyyy-MM-dd HH:mm:ss] [LEVEL] メッセージ
例: [2026-08-18 14:30:05] [ERROR] DB保存エラー: テーブル 'Transactions' への接続に失敗しました
```

### 5.3. ログ出力先
- MVP: `Console.WriteLine`（Visual Studio 出力ウィンドウ）
- 将来: `ILogger` を使用したファイルログ（NLog / Serilog）

## 6. セキュリティ設計

### 6.1. 脆弱性対策

| 脆弱性 | 対策 | 実装状況 |
|--------|------|----------|
| SQLインジェクション | EF Core のパラメータバインディング | 自動対策済み |
| DB接続文字列の漏洩 | appsettings.json管理（Git管理対象外） | 要確認 |
| ファイル書き込み権限 | デスクトップのみ（システムフォルダ不使用） | 実装済み |

### 6.2. 認証・認権
- MVP: なし（単一ユーザー前提）
- 将来: ログイン機能、役割別権限管理

## 7. データフロー図

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


## バージョン履歴

| バージョン | 変更日 | 変更内容 | 変更者 |
|-----------|--------|----------|--------|
| 1.0 | 2026-08-18 | 初版作成 | 開発チーム |
| 1.1 | 2026-08-18 | BarcodeService DI設計反映 | 開発チーム |
| 1.2 | 2026-08-18 | ITransactionService/IReceiptService追加、CancellationToken対応、DbUpdateExceptionハンドリング、ReceiptContext導入、非同期ファイル出力対応 | 開発チーム |
| 1.3 | 2026-08-18 | IDbContextFactory 導入、全サービスに ILogger 注入、接続文字列起動時バリデーション、エラーハンドリング粒度改善、XML ドキュメント追加 | 開発チーム |

## 承認記録

| 役割 | 氏名 | 承認日 | 署名 |
|------|------|--------|------|
| プロジェクトオーナー | 田中太郎 | 2026-08-18 | ✓ |
| 技術リーダー | 田中太郎 | 2026-08-18 | ✓ |
| QAリーダー | 田中太郎 | 2026-08-18 | ✓ |
