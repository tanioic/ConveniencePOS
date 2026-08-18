# 仕様書作成順序マニュアル: Spec指向開発

## 1. 概要

本ドキュメントは、ConveniencePos プロジェクトにおける仕様書の**作成順序・役割・責任者**を定義する大元の仕様書です。
ウォーターフォールの各フェーズに相当する4ステップで、仕様 → 設計 → 実装計画 → タスク分解を進めます。

## 2. 作成順序フロー

```
Step1: step0100_constitution.md（事前準備）
  │   プロジェクトの立ち上げ時に1回だけ作成
  │   「絶対に破ってはいけないルール」を定義
  │
  ▼
Step2: step0200_spec.md（要件定義 / 外部設計）
  │   Why（なぜ必要か）+ What（何ができるようになるか）
  │   業務ロジックと受け入れ基準を明確に
  │
  ▼
Step3: step0300_plan.md（内部設計 / プログラム設計）
  │   How（どうやって実装するか）
  │   ファイル変更、DBスキーマ、API設計を定義
  │
  ▼
Step4: step0400_tasks.md（モジュール設計 / テスト設計）
      実装すべきソースコード・テストを細かいチェックリストに分解
      AIがこのリストを上から順に実行し、自動テストをパスしながら開発
```

## 3. 各ステップ詳細

### Step1: 事前準備（step0100_constitution.md）

| 項目 | 内容 |
|------|------|
| **対象ファイル** | `step0100_constitution.md` |
| **タイミング** | プロジェクトの立ち上げ時に1回だけ作成 |
| **作成者** | 人間（エンジニア） |
| **相当するウォーターフォール** | なし（プロジェクト憲法はウォーターフォール以前） |

**記述内容:**
- 開発言語、フレームワーク、テスト方針
- 共通のコーディング規約
- プロジェクト全体で「絶対に破ってはいけないルール」

**本プロジェクトでの対応:**
```
step0100_constitution.md に定義済み:
- 言語: C# (.NET 8.0)
- UI: WPF
- アーキテクチャ: MVVM (CommunityToolkit.Mvvm)
- DB: SQL Server LocalDB
- ORM: Entity Framework Core (コードファースト)
- コード規約: PascalCase/camelCase、Viewにはロジックを書かない
```

**step0100_constitution.md から派生する補助ドキュメント:**

| ファイル | 役割 | 作成タイミング |
|----------|------|---------------|
| `step0101_development_history.md` | 開発履歴・実施記録 | step0100_constitution.md 確定後 |

---

### Step2: step0200_spec.md の作成（要件定義 / 外部設計）

| 項目 | 内容 |
|------|------|
| **対象ファイル** | `step0200_spec.md` |
| **タイミング** | step0100_constitution.md 確定後 |
| **作成者** | 人間（プロダクトマネージャーまたはエンジニア） |
| **相当するウォーターフォール** | 要件定義、外部設計 |

**記述内容:**
- Why: なぜこの機能が必要なのか
- What: ユーザーは何ができるようになるのか
- 業務ロジック（計算式、条件分岐）
- 受け入れ基準（ゴールの定義）

**ポイント:**
- 画面の細かいボタン配置は書かない
- 技術的な実装方法は書かない
- 業務ロジックと受け入れ基準のみを明確にする

**本プロジェクトでの対応:**
```
step0200_spec.md に定義済み:
- プロジェクト背景・ステークホルダー
- ユーザージャーニー
- 業務上の目的（KPI付き）
- 機能要件の優先度（Must/Should）
- 受け入れ基準（AC-1 〜 AC-13）
- スコープ外
```

**step0200_spec.md から派生する補助ドキュメント:**

| ファイル | 役割 | 作成タイミング |
|----------|------|---------------|
| `step0201_business_rules.md` | 業務ルールの詳細定義 | step0200_spec.md 作成時または直後 |
| `step0202_process_flow.md` | 業務フロー図・状態遷移図 | step0200_spec.md 作成時または直後 |
| `step0203_non_functional_requirements.md` | 非機能要件（パフォーマンス等） | step0200_spec.md 作成時または直後 |
| `step0204_ui.md` | 画面設計（レイアウト・バインディング） | step0200_spec.md 確定後 |
| `step0205_mvp_pos.md` | 機能仕様の詳細 | step0200_spec.md 確定後 |
| `step02[67]_extension_*.md` | 仕様拡張 | step0200_spec.md 確定後に必要に応じて |

---

### Step3: step0300_plan.md の作成（内部設計 / プログラム設計）

| 項目 | 内容 |
|------|------|
| **対象ファイル** | `step0300_plan.md` |
| **タイミング** | step0200_spec.md 確定後 |
| **作成者** | AIエージェント、または人間とAIの共同作業 |
| **相当するウォーターフォール** | 内部設計、プログラム設計 |

**記述内容:**
- How: step0200_spec.md を満たすために具体的にどう実装するか
- ファイル変更の対象と方針
- DBスキーマ設計
- API / インターフェース設計
- テスト方針（unit test framework、mock戦略）
- 既存コードとの差分管理

**ポイント:**
- 実装を始める前に、AIが「正しいアプローチを選択しているか」を人間がレビューして軌道修正するためのドキュメント
- 人間のレビューを経てからStep4に進む

**本プロジェクトでの対応:**
```
step0300_plan.md に定義済み:
- 既存ドキュメントとの関係（参照表）
- タスク間の依存関係と実装順序
- 既存コードとの差分管理
- セキュリティ要件
```

**step0300_plan.md から派生する補助ドキュメント:**

| ファイル | 役割 | 作成タイミング |
|----------|------|---------------|
| `step0301_technical_plan.md` | データモデル・DB設計・API設計の詳細 | step0300_plan.md 作成時 |
| `step0302_teststrategy.md` | テスト戦略（フレームワーク・カバレッジ） | step0300_plan.md 作成時 |
| `step0303_testplan.md` | テスト計画（スケジュール・進入条件） | step0300_plan.md 作成時 |
| `step0304_deployment.md` | 運用・配置計画 | step0300_plan.md 作成時 |

---

### Step4: step0400_tasks.md の作成（モジュール設計 / テスト設計）

| 項目 | 内容 |
|------|------|
| **対象ファイル** | `step0400_tasks.md` |
| **タイミング** | step0300_plan.md のレビュー完了後 |
| **作成者** | AIエージェント（自動生成） |
| **相当するウォーターフォール** | モジュール設計、テスト設計 |

**記述内容:**
- step0300_plan.md の方針に沿った、実装すべきソースコード・テストのチェックリスト
- 1タスク = 1つの具体的な作業（ファイル作成、関数追加、テスト作成等）
- 各タスクに [X] 完了 / [ ] 未完了のチェックボックス

**ポイント:**
- AIはこのチェックリストを上から順に実行する
- 各タスク完了時に自動テストをパスさせながら開発を進める
- タスク間の依存関係は step0300_plan.md で定義済みの順序に従う

**本プロジェクトでの対応:**
```
step0400_tasks.md に定義済み:
- フェーズ1: データベースとモデルの構築（完了）
- フェーズ2: ロジックとViewModelの実装（一部完了）
- フェーズ3: 画面（UI）の構築と結合（未着手）
- フェーズ4: 単体テスト（xUnit）の作成（完了）
```

## 4. ドキュメント間の依存関係図

```
step0100_constitution.md (Step1)
    │
    ├──> step0101_development_history.md
    │
    ├──> step0200_spec.md (Step2)
    │       │
    │       ├──> step0201_business_rules.md
    │       ├──> step0202_process_flow.md
    │       ├──> step0203_non_functional_requirements.md
    │       ├──> step0204_ui.md
    │       ├──> step0205_mvp_pos.md
    │       ├──> step0206_extension_tax_rate.md
    │       ├──> step0207_extension_receipt_simple.md
    │       └──> step0208_service_architecture.md
    │
    └──> step0300_plan.md (Step3)
            │
            ├──> step0301_technical_plan.md
            ├──> step0302_teststrategy.md
            ├──> step0303_testplan.md
            ├──> step0304_deployment.md
            │
            └──> step0400_tasks.md (Step4)
```

## 5. ステップごとのチェックポイント

### Step1 完了条件
- [X] step0100_constitution.md が存在し、技術スタックが定義されている
- [X] コード規約が定義されている
- [X] プロジェクトメンバーが内容を認識している

### Step2 完了条件
- [X] step0200_spec.md が存在し、Why + What が記述されている
- [X] 受け入れ基準（AC）がすべて定義されている
- [X] スコープ外が明確に定義されている
- [X] step0201_business_rules.md が作成されている
- [X] step0202_process_flow.md が作成されている
- [X] 画面設計（step0204_ui.md）が作成されている

### Step3 完了条件
- [X] step0300_plan.md が存在し、How が記述されている
- [X] step0300_plan.md が人間によってレビューされ、承認されている
- [X] step0301_technical_plan.md が作成されている
- [X] テスト戦略（step0302_teststrategy.md）が作成されている

### Step4 完了条件
- [X] step0400_tasks.md が存在し、チェックリスト形式で分解されている
- [X] 各タスクが step0300_plan.md の方針に合致している
- [X] タスクの実行順序が step0300_plan.md の依存関係に合致している

## 6. ルール

1. **Step の順序は厳守する**: Step1 → Step2 → Step3 → Step4 の順で進める
2. **Step3 は人間のレビュー必須**: step0300_plan.md はAIが作成しても、人間がレビューしてからStep4に進む
3. **Step4 は自動生成**: step0400_tasks.md はstep0300_plan.md の方針に沿ってAIが自動生成する
4. **仕様変更時は上流から修正**: step0200_spec.md を変更した場合、step0300_plan.md と step0400_tasks.md も再点検する
5. **step0100_constitution.md はプロジェクト全体で共有**: すべての開発者が参照し、ルール遵守の責任を持つ

## 7. 現状のステップ状況

| Step | ファイル | 状態 | 備考 |
|------|---------|------|------|
| Step1 | `step0100_constitution.md` | 完了 | 技術スタック・コード規約定義済み |
| Step2 | `step0200_spec.md` | 完了 | 要件定義・受入基準定義済み |
| Step2 補助 | `step0201_business_rules.md` | 完了 | 26ルール定義済み |
| Step2 補助 | `step0202_process_flow.md` | 完了 | フロー図・例外フロー定義済み |
| Step2 補助 | `step0203_non_functional_requirements.md` | 完了 | 23要件定義済み |
| Step2 補助 | `step0204_ui.md` | 完了 | 画面設計定義済み |
| Step2 補助 | `step0205_mvp_pos.md` | 完了 | 機能仕様定義済み |
| Step2 補助 | `step0206_extension_tax_rate.md` | 完了 | 軽減税率仕様拡張定義済み |
| Step2 補助 | `step0207_extension_receipt_simple.md` | 完了 | レシート出力仕様拡張定義済み |
| Step2 補助 | `step0208_service_architecture.md` | 完了 | サービス層アーキテクチャ定義済み |
| Step3 | `step0300_plan.md` | 完了 | 技術計画定義済み |
| Step3 補助 | `step0301_technical_plan.md` | 完了 | DB設計・API設計定義済み |
| Step3 補助 | `step0302_teststrategy.md` | 完了 | テスト戦略定義済み |
| Step3 補助 | `step0303_testplan.md` | 完了 | テスト計画定義済み |
| Step3 補助 | `step0304_deployment.md` | 完了 | 運用・配置計画定義済み |
| Step3 補助 | `step0305_risk_register.md` | 完了 | プロジェクトリスク定義済み |
| Step3 補助 | `step0306_uat_plan.md` | 完了 | ユーザー受入テスト計画定義済み |
| Step3 補助 | `step0307_rollback_procedures.md` | 完了 | ロールバック・障害復旧手順定義済み |
| Step3 補助 | `step0308_data_dictionary.md` | 完了 | データ辞書定義済み |
| Step3 補助 | `step0309_traceability_matrix.md` | 完了 | AC-テストトレーサビリティ矩阵定義済み |
| Step4 | `step0400_tasks.md` | 完了 | チェックリスト定義済み |
| 履歴 | `step0101_development_history.md` | 完了 | 開発履歴・実施記録 |

## バージョン履歴

| バージョン | 変更日 | 変更内容 | 変更者 |
|-----------|--------|----------|--------|
| 1.0 | 2026-08-18 | 初版作成 | 開発チーム |
| 1.1 | 2026-08-18 | 仕様書ファイル名を4桁形式に変更、内部参照更新 | 開発チーム |

## 承認記録

| 役割 | 氏名 | 承認日 | 署名 |
|------|------|--------|------|
| プロジェクトオーナー | 田中太郎 | 2026-08-18 | ✓ |
| 技術リーダー | 田中太郎 | 2026-08-18 | ✓ |
| QAリーダー | 田中太郎 | 2026-08-18 | ✓ |
