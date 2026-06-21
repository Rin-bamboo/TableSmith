# TableSmith

TableSmithは、データベースのテーブル設計を支援するWindowsデスクトップアプリケーションです。

テーブルとカラムの定義、主キー・外部キーの関連付け、プロジェクトのJSON保存に加え、CREATE TABLE文、Excelテーブル定義書、ER図を出力できます。

現在のバージョン: **0.3.0**

## v0.3.0

将来の既存DB取得、ALTER文生成、テストデータ作成、ストアド作成支援に備え、DB設計情報の基礎を拡張しました。

- プロジェクト単位の既定RDB、スキーマ、文字コード、照合順序を管理
- 文字コード・照合順序・SQLファイル文字コードの候補をApp.configで変更
- スキーマ管理画面で複数スキーマを登録し、既定スキーマを選択
- スキーマ管理・インデックス管理をメイン画面の機能ボタンから利用
- ヘルプメニューから各機能の操作説明を表示
- SQLファイル出力時の文字コードを選択
- テーブル単位のスキーマ、文字コード、照合順序を設定
- decimal型の精度と小数桁数を設定
- RDBごとの自動採番を設定
- 複合カラム、Unique、Clustered、昇順・降順を含むインデックスを定義
- インデックス名を `IX_テーブル名_カラム名` の標準規則で自動生成
- 複合インデックスは対象カラム順に名前へ反映し、必要な場合はカスタム名へ変更
- 暗号化・ハッシュ化・マスキングなどの保護対象と方式を設計情報として保持
- 旧バージョンのプロジェクトJSONに不足する設定を自動補完

## 主な機能

### プロジェクト管理

- 任意のプロジェクト名を設定
- プロジェクト全体をJSON形式で保存・読み込み
- 未保存状態でアプリを終了する際の保存確認
- 拡張子 `.tablesmith.json` に対応

### テーブル設計

- テーブル物理名・論理名・説明の入力
- カラム物理名・論理名・データ型・サイズの入力
- スキーマ名・文字コード・照合順序の入力
- テーブルのスキーマはプロジェクトに登録済みの一覧から選択
- decimal型の精度・小数桁数の入力
- 自動採番の開始値・増分の入力
- 保護対象・保護方式の設計情報を入力
- PK、FK、Not Null、既定値の設定
- カラムの追加・削除
- 作成済みテーブルの一覧表示・編集
- 入力値の必須・形式・重複チェック
- 各入力項目のツールチップ表示

### 標準カラムテンプレート

新規テーブルには、次のカラムが初期設定されます。

| カラム | 型 | 設定 |
|---|---|---|
| `id` | `bigint` | PK / Not Null / 自動採番 |
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
- SQLファイル文字コードはUTF-8 BOM付き、UTF-8 BOMなし、Shift_JIS、UTF-16から選択
- PK制約、FK制約、Not Null、既定値を出力
- decimalの精度・小数桁数とRDB固有の自動採番句を出力
- CREATE TABLE文に続けてインデックス作成文を出力
- MySQLではテーブルの文字コード・照合順序を出力
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
- スキーマ名、文字コード、照合順序を掲載
- 精度、小数桁数、自動採番、保護対象、保護方式を掲載
- インデックス一覧シートを出力

### ER図

- プロジェクト内の全テーブルを自動配置
- テーブル名、カラム名、型、PK、FKを表示
- decimal型は `decimal(10,2)` のように精度・小数桁数を表示
- FKから参照先PKへのリレーションを描画
- プレビューの拡大・縮小
- SVG・PNG形式で保存

## 動作環境

- Windows
- .NET 10 Desktop Runtime

開発にはVisual Studio 2026または.NET 10 SDKが必要です。

## 使用技術

- C# / .NET 10
- WPF
- MahApps.Metro
- ClosedXML
- log4net
- WpfAnimatedGif
- Velopack

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

Visual Studio 2026から `WPF_TableSmith.sln` を開いて起動することもできます。

## インストールと更新

GitHub Releasesから`BambooSystem.TableSmith-win-Setup.exe`をダウンロードして実行します。
インストーラー版では、`ヘルプ` → `更新を確認`またはバージョン情報画面の
`更新を確認`から最新リリースを確認できます。

新しいバージョンがある場合は、画面の案内に従ってダウンロードすると、
Velopackが更新を適用してTableSmithを再起動します。

Visual Studioや`dotnet run`から起動した未インストール版では、
安全のためアプリ内更新を実行しません。

## 開発者向けリリース手順

リリースは`v*`形式のタグをpushすると、GitHub Actionsが自動作成します。

1. リリース対象の変更を`main`ブランチへ反映します。
2. SemVer形式でタグを作成します。
3. タグをGitHubへpushします。

```powershell
git tag v1.0.0
git push origin v1.0.0
```

`.github/workflows/release.yml`が.NET 10のself-containedアプリを
`win-x64`向けに発行し、Velopackのインストーラー、更新パッケージ、
リリースフィードをGitHub Releasesへアップロードします。

ワークフロー内の`GITHUB_TOKEN`はリリースのアップロードだけに使用します。
アプリ本体にはGitHubトークンを埋め込んでいません。

## Velopackのローカルパッケージ作成

Velopack CLIは、アプリが参照するNuGetパッケージと同じ`1.2.0`を使用します。

```powershell
dotnet tool install --global vpk --version 1.2.0

dotnet publish TableSmith\TableSmith.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -o publish `
  /p:Version=1.0.0

vpk pack `
  --packId BambooSystem.TableSmith `
  --packTitle TableSmith `
  --packVersion 1.0.0 `
  --packDir publish `
  --mainExe TableSmith.exe `
  --runtime win-x64 `
  --icon TableSmith\Resources\Images\TableSmith.ico `
  --outputDir Releases
```

差分更新パッケージも作成する場合は、`vpk pack`の前に公開済みの最新版を取得します。

```powershell
vpk download github `
  --repoUrl https://github.com/Rin-bamboo/TableSmith `
  --outputDir Releases
```

作成された`Releases`フォルダには、セットアッププログラム、フル更新パッケージ、
ポータブル版、`releases.win.json`などが出力されます。

一般配布前にはWindowsコード署名証明書でインストーラーと実行ファイルへ
署名することを推奨します。署名されていない場合、SmartScreenの警告が表示されます。

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
  "Version": 2,
  "ProjectName": "サンプルプロジェクト",
  "DatabaseSettings": {
    "DefaultDialect": 0,
    "DefaultSchemaName": "dbo",
    "DefaultCharacterSet": "",
    "DefaultCollation": "",
    "SqlFileEncoding": 0
  },
  "Tables": []
}
```

プロジェクトファイルには、テーブル、カラム、PK、FK、参照先などの設計情報が含まれます。
旧バージョンのJSONは読み込み時に新しい設定とコレクションが補完されます。

## 選択肢のコンフィグ設定

文字コード、照合順序、SQLファイル文字コードの候補は
`TableSmith/App.config`の以下のキーで変更できます。

- `CharacterSetOptions`
- `CollationOptions`
- `SqlFileEncodingOptions`

各候補は`内部値|画面表示名`の形式で記述し、複数候補をセミコロンで区切ります。

```xml
<add key="CharacterSetOptions"
     value="utf8mb4|UTF-8（utf8mb4）;sjis|Shift_JIS" />
```

変更内容はアプリケーションの再起動後に反映されます。保存済みJSONにコンフィグ未登録の値や表記の異なる値が含まれる場合は、プロジェクトの値を正として「保存済み設定」の候補へ追加し、そのまま選択表示します。

## 今後予定している機能

- 既存テーブル取得機能
- テーブル変更時スクリプト作成
- テストデータ作成
- ストアド作成支援

v0.3.0の保護対象・保護方式は設計情報として保持するのみで、暗号化やマスキング処理、SQLの自動生成は行いません。

## サンプル

`Samples` ディレクトリに、動作確認用の架空プロジェクトを収録しています。

- `案件タスク管理システム.tablesmith.json`
- `案件タスク管理システム_テーブル定義書.xlsx`

サンプルには、次の設計情報を一通り登録しています。

- `dbo`、`app`、`audit`の複数スキーマと既定スキーマ
- PK、FK、Not Null、既定値、自動採番
- decimal型の精度・小数桁数
- 保護対象と保護方式
- Unique、Clustered、複合、ASC・DESCのインデックス
- 標準命名とカスタム命名のインデックス
- テーブル・カラムの論理名と説明

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
