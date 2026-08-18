# UI.md - ユーザーインターフェース

## 画面構成
`MainWindow.xaml` に全画面を1ファイルで実装。323行のXAML。
コードビハインド `MainWindow.xaml.cs` でキーボードイベント処理。

## 全体レイアウト（2カラム構造）
```
┌──────────────────────────────────────────────────────────┐
│  WINDOW: Convenience POS (1100x720) 背景 #F0F2F5          │
├─────────────────────────────┬────────────────────────────┤
│  LEFT PANEL (Width=*)       │  RIGHT PANEL (Width=320)   │
│                             │                            │
│  ┌───────────────────────┐ │  ┌──────────────────────┐  │
│  │ バーコード入力エリア   │ │  │ お会計               │  │
│  │ [📦アイコン]           │ │  │                      │  │
│  │ [TextBox] [検索]      │ │  │ 小計        ¥xxx     │  │
│  └───────────────────────┘ │  │ 8%対象額    ¥xxx     │  │
│                             │  │ 消費税(8%)  ¥xxx     │  │
│  ┌───────────────────────┐ │  │ 10%対象額   ¥xxx     │  │
│  │ カート一覧            │ │  │ 消費税(10%) ¥xxx     │  │
│  │ (DataGrid)            │ │  │                      │  │
│  │ [商品名][税率]        │ │  │ ┌──────────────────┐ │  │
│  │ [数量][単価][小計]    │ │  │ │ 合計  ¥xxx (金)  │ │  │
│  └───────────────────────┘ │  │ └──────────────────┘ │  │
│                             │  │                      │  │
│                             │  │ 預かり金額            │  │
│                             │  │ [TextBox]            │  │
│                             │  │                      │  │
│                             │  │ お釣り    ¥xxx (青)  │  │
│                             │  │                      │  │
│                             │  │ [会計確定]            │  │
│                             │  └──────────────────────┘  │
└─────────────────────────────┴────────────────────────────┘
```

## バーコード入力エリア
- 白背景ボーダー (CornerRadius=8, Padding=16,12)
- 左端に📦アイコン (FontSize=28)
- 「バーコード」ラベル (FontSize=14, 色 #888)
- TextBox: FontSize=22、`BarcodeInput` にバインド
- 「検索」ボタン: 緑背景 (#27AE60)、`AddItemCommand` にバインド

## カート一覧（DataGrid）
- 白背景ボーダー (CornerRadius=8)
- ヘッダ行: 暗色背景 (#34495E)、白文字
- 交互行背景: #FAFBFC

| カラム | バインド先 | 幅 | 備考 |
|---|---|---|---|
| 商品名 | `Name` | * (自動) | FontSize=15 |
| 税率 | `TaxRate` | 60 | `StringFormat='{}{0}%'` |
| 数量 | `Quantity` | 80 | 編集可能 (TextBoxに切替) |
| 単価 | `UnitPrice` | 100 | `StringFormat='¥{0:N0}'` 右寄せ |
| 小計 | `LineTotalWithTax` | 110 | `StringFormat='¥{0:N0}'` 太字右寄せ |

## お会計パネル（右側）
- 白背景ボーダー (CornerRadius=8, Padding=20)
- 見出し: 「お会計」 FontSize=22 Bold 色 #2C3E50

### 表示項目（各 #F8F9FA 背景ボーダー、CornerRadius=6）
| 項目 | バインド先 | フォーマット |
|---|---|---|
| 小計 | `Subtotal` | `¥{0:N0}` |
| 8%対象額 | `TaxableAmount8` | `¥{0:N0}` |
| 消費税 (8%) | `TaxAmount8` | `¥{0:N0}` |
| 10%対象額 | `TaxableAmount10` | `¥{0:N0}` |
| 消費税 (10%) | `TaxAmount10` | `¥{0:N0}` |

### 合計金額バナー
- 暗色背景 (#1A1A2E)、CornerRadius=6
- 左: 「合計」白色 FontSize=18 Bold
- 右: `TotalAmount` 金色 (#F39C12) FontSize=28 Bold

### 預かり金額入力
- ラベル: 「預かり金額」 FontSize=14 色 #555
- TextBox: FontSize=20、`ReceivedAmount` にバインド

### お釣り表示
- 水色背景 (#EBF5FB)、CornerRadius=6
- 左: 「お釣り」 色 #2E86C1
- 右: `Change` FontSize=26 Bold 色 #2E86C1

### 会計確定ボタン
- `ConfirmTransactionCommand` にバインド
- 青背景 (#2E86C1)、白色文字、CornerRadius=6

## モーダル画面
**現在は未実装。**

### 将来構想
1. **お会計モーダル**: 支払方法選択（現金/クレジット/電子マネー/QQR）
2. **会計完了モーダル**: 取引完了通知、レシート表示
3. **レシートプレビューモーダル**: レシートプレビュー、印刷

## キーボード操作
| キー | 場所 | アクション |
|---|---|---|
| `Enter` | バーコード入力欄 | 商品登録 (`AddItemCommand` 実行後、フォーカス再設定) |

- 起動時にバーコード入力欄にフォーカス
- コードビハインド `MainWindow.xaml.cs` で `KeyDown` イベント処理

## ViewModel（`MainViewModel.cs`）

### プロパティ
| プロパティ | 型 | 説明 |
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

### コマンド
| コマンド | 処理 |
|---|---|
| `AddItemCommand` | JANコードで商品検索→カートに追加 or 数量+1 |
| `ConfirmTransactionCommand` | 取引保存→レシート生成(Desktop)→カートクリア |

## CartItemViewModel
| プロパティ | 型 | 説明 |
|---|---|---|
| `ProductId` | int | 商品ID |
| `Name` | string | 商品名 |
| `UnitPrice` | decimal | 単価 |
| `TaxRate` | int | 税率 (8 or 10) |
| `Quantity` | int | 数量 |
| `LineTotal` | decimal | 税抜小計（算出: UnitPrice x Quantity） |
| `LineTotalWithTax` | decimal | 税込小計（算出: Floor(LineTotal x (1+TaxRate/100))） |

## 色彩（インライン定義）
| 色 | 用途 |
|---|---|
| `#F0F2F5` | ウィンドウ背景 |
| `#F8F9FA` | 金額表示行背景 |
| `#1A1A2E` | 合計バナー背景 |
| `#F39C12` | 合計金額テキスト（金色） |
| `#2E86C1` | お会計関連ボタン・お釣り表示 |
| `#27AE60` | 検索ボタン（緑） |
| `#34495E` | DataGrid ヘッダ背景 |
| `#EBF5FB` | お釣り表示背景（水色） |
| `#DDD` | ボーダー線 |
| `#333` | ラベルテキスト |
| `#888` | プレースホルダーテキスト |

## Value Converter
**現在は未実装。** 金額フォーマットは XAML の `StringFormat` で対応。

## データモデル

### Product
| フィールド | 型 |
|---|---|
| `Id` | int |
| `JanCode` | string |
| `Name` | string |
| `Price` | decimal |
| `TaxRate` | int |

### Transaction
| フィールド | 型 |
|---|---|
| `Id` | int |
| `CreatedAt` | DateTime |
| `TotalAmount` | decimal |
| `TaxAmount` | decimal |
| `Items` | ICollection\<TransactionItem\> |

### TransactionItem
| フィールド | 型 |
|---|---|
| `Id` | int |
| `TransactionId` | int |
| `ProductId` | int |
| `Quantity` | int |
| `UnitPrice` | decimal |
| `AppliedTaxRate` | int |

## レシート出力
- テキストファイルとしてデスクトップに保存 (`receipt_{trxId}.txt`)
- 幅32文字の固定幅フォーマット
- 全角文字は2幅としてカウント
- 内容: 店舗名、レジ番号、担当者、取引番号、日時、明細、税内訳、合計、預かり、お釣り

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
