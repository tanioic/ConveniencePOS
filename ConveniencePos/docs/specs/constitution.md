# プロジェクト憲法: コンビニPOSシステム MVP

## 1. 絶対的な技術スタック
- 言語: C# (.NET 8.0)
- UIフレームワーク: WPF (Windows Presentation Foundation)
- アーキテクチャ: MVVMパターン (CommunityToolkit.Mvvm を使用)
- データベース: SQL Server (LocalDB: `(localdb)\MSSQLLocalDB`)
- ORM: Entity Framework Core (コードファーストアプローチ)

## 2. 実装・コード規約
- すべてのUIロジックはViewModelに記述し、View（XAMLの分離コード）にはイベントハンドラを極力書かない。
- 変数名やメソッド名はC#の一般的な命名規則（PascalCase、camelCase）に従う。
- データベース接続の管理は、DbContextの寿命（Scope）を適切に管理すること。
