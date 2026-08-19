# ループエンジニアリング
ConveniPosUsingSDDAndLoopEngでは、ループ処理を簡単に行うための便利な機能が提供されています。以下に、C#でのループ処理の基本的な使い方を説明します。

## ループ
ConveniPosUsingSDDAndLoopEng\docs以下にある仕様書に従って、コードをC#のソースコードを実装・修正してください。
修正後、必ず以下の手順で検証ループを回してください。

1. `dotnet test`を実行。ビルドエラーが出た場合は原因を修正して手順1. をやり直す。
2．ビルドが成功したら、`dotnet clean; dotnet test`を実行。
3．テストが一つでも落ちた場合、エラーログを分析してコードを修正し、手順1．に戻る。
4．ビルドおよび全テストが合格（Green）になったらループを終了し、結果を報告する。

【制約】
－ `step0200_spec.md`のファイル自体は絶対に書き換えないこと。
－ 最大10回ループしても成功しない場合は、作業を中断して人間に報告（エスカーレーション）すること。

【初回ループ時の必須確認事項】
WPF+DI構成の場合、以下のパターンでNullReferenceExceptionが発生するため、必ず以下のルールを守ること：
- XAMLに `<Window.DataContext><vm:MainViewModel/></Window.DataContext>` を書かない。DIコンテナから取得したインスタンスをコードビハインドで設定する。
- 同様に `<UserControl.DataContext>` や `<Window.DataContext>` にViewModelを直接インスタンス化しない。
- 理由: WPFはXAML解析時にパラメータなしコンストラクタでViewModelを生成しようとするため、コンストラクタにDI引数を持つViewModelは必ずNullReferenceExceptionになる。
- 正しいパターン: App.xaml.cs等のコードから `window.DataContext = serviceProvider.GetRequiredService<MainViewModel>();` と設定する。

<!--
プロンプトの例
ConveniPosUsingSDDAndLoopEng\docs\csharp_loop.mdの内容に沿って、ループエンジニアリングすること。
-->