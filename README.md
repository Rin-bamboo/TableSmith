# TableSmith

TableSmithは、データベースのテーブル設計を支援するWindowsデスクトップアプリケーションです。

テーブルとカラムの定義、主キー・外部キーの関連付け、プロジェクトのJSON保存に加え、CREATE TABLE文、Excelテーブル定義書、ER図を出力できます。

現在のバージョン: **0.2.0**

## 主な機能

### プロジェクト管理

- 任意のプロジェクト名を設定
- プロジェクト全体をJSON形式で保存・読み込み
- 未保存状態でアプリを終了する際の保存確認
- 拡張子 `.tablesmith.json` に対応

### テーブル設計

- テーブル物理名・論理名・説明の入力
- カラム物理名・論理名・データ型・サイズの入力
- PK、FK、Not Null、既定値の設定
- カラムの追加・削除
- 作成済みテーブルの一覧表示・編集
- 入力値の必須・形式・重複チェック
- 各入力項目のツールチップ表示

### 標準カラムテンプレート

新規テーブルには、次のカラムが初期設定されます。

| カラム | 型 | 設定 |
|---|---|---|
| `id` | `bigint` | PK / Not Null |
| `created_at` | `datetime` | Not Null / `CURRENT_TIMESTAMP` |
| `created_by` | `nvarchar(100)` | Not Null |
| `updated_at` | `datetime` | Not Null / `CURRENT_TIMESTAMP` |
| `updated_by` | `nvarchar(100)` | Not Null |

### 外部キー

- 既存テーブルのPKを参照先として選択
- 参照元PKの型、サイズ、論理名、Not Nullを継承
- FKカラム名を `参照テーブル名_参照カラム名` で自動設定
- 自動設定後のカラム名を任意に変更可能
- FKと参照先PKの型・サイズ整合性をチェック

### CREATE文出力

- SQL Server、MySQL、Oracleの出力形式を選択
- 複数テーブルをチェックボックスで選択
- 全選択・全解除
- 選択したテーブルのCREATE TABLE文をまとめて表示
- 保存先フォルダを指定し、テーブルごとに `テーブル物理名.sql` を出力
- SQLファイルは日本語コメントに対応したUTF-8形式
- PK制約、FK制約、Not Null、既定値を出力
- テーブル・カラムの論理名と説明をRDBのコメント機能へ出力

選択したRDBに合わせて、識別子の引用符、データ型、DEFAULT句、複数文の区切りを切り替えます。

- SQL Server: `sp_addextendedproperty`
- MySQL: `COMMENT`
- Oracle: `COMMENT ON TABLE` / `COMMENT ON COLUMN`

### Excelテーブル定義書

- 複数テーブルを選択して1つのExcelファイルへ出力
- 全選択・全解除
- テーブル一覧シート
- テーブルごとの定義シート
- PK、FK、Not Null、既定値、参照先、説明を掲載

### ER図

- プロジェクト内の全テーブルを自動配置
- テーブル名、カラム名、型、PK、FKを表示
- FKから参照先PKへのリレーションを描画
- プレビューの拡大・縮小
- SVG・PNG形式で保存

## 動作環境

- Windows
- .NET 8 Desktop Runtime

開発には.NET 8 SDKが必要です。

## 使用技術

- C# / .NET 8
- WPF
- MahApps.Metro
- ClosedXML
- log4net
- WpfAnimatedGif

## ビルド

リポジトリをクローンし、ソリューションをビルドします。

```powershell
git clone https://github.com/Rin-bamboo/TableSmith.git
cd TableSmith
dotnet build WPF_TableSmith.sln
```

## 起動

```powershell
dotnet run --project TableSmith\TableSmith.csproj
```

Visual Studioから `WPF_TableSmith.sln` を開いて起動することもできます。

## 基本的な使い方

1. メイン画面でプロジェクト名を入力します。
2. `テーブル定義作成` からテーブル情報とカラム情報を入力します。
3. 必要に応じてFKを有効にし、参照するテーブルのPKを選択します。
4. `テーブル一覧` から作成内容を確認・編集します。
5. ファイルメニューからプロジェクトを保存します。
6. メイン画面からCREATE文、Excel定義書、ER図を出力します。

## プロジェクトファイル

プロジェクトはJSONとして保存されます。

```json
{
  "Version": 1,
  "ProjectName": "サンプルプロジェクト",
  "Tables": []
}
```

プロジェクトファイルには、テーブル、カラム、PK、FK、参照先などの設計情報が含まれます。

## サンプル

`Samples` ディレクトリに、動作確認用の架空プロジェクトを収録しています。

- `案件タスク管理システム.tablesmith.json`
- `案件タスク管理システム_テーブル定義書.xlsx`

アプリの `ファイル` → `開く` からJSONファイルを読み込めます。

## プロジェクト構成

```text
TableSmith/
├─ TableSmith/
│  ├─ Models/       データモデル
│  ├─ Services/     JSON、SQL、Excel、ER図の生成処理
│  ├─ Views/        WPF画面
│  └─ Resources/    画像リソース
├─ Utility/         共通ユーティリティ
├─ Samples/         動作確認用データ
└─ WPF_TableSmith.sln
```

## ライセンス

このプロジェクトは [MIT License](LICENSE) の下で公開されています。
