# タスク管理リスト: コンビニPOSシステム MVP

## フェーズ1: データベースとモデルの構築
- [X] Task 1.1: EF Coreのデータモデル（Product, Transaction, TransactionItem）クラスを `Models/` フォルダに作成する
- [X] Task 1.2: `Data/PosDbContext.cs` を作成し、SQL Serverへの接続設定を行う
- [X] Task 1.3: EF Coreのマイグレーションを実行し、ローカルにデータベースを作成する
- [X] Task 1.4: テスト用の商品データ（マスターデータ）をデータベースに挿入する

## フェーズ2: ロジックとViewModelの実装
- [ ] Task 2.1: バーコード入力を処理する `BarcodeService` を作成する
- [ ] Task 2.2: 会計計算ロジックと画面のデータを保持する `MainViewModel.cs` を作成する

## フェーズ3: 画面（UI）の構築と結合
- [ ] Task 3.1: `MainWindow.xaml` をデザインし、ViewModelとバインディングする
- [ ] Task 3.2: 実際に動作させて、商品のスキャン、計算、保存が正しく行われるか確認する
