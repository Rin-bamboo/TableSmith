using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using MahApps.Metro.Controls;

namespace TableSmith.Views
{
    /// <summary>
    /// TableSmithの各機能について操作手順を表示する画面です。
    /// </summary>
    public partial class OperationGuide : MetroWindow, INotifyPropertyChanged
    {
        private OperationGuideTopic? _selectedTopic;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 画面で選択できる操作説明の一覧です。
        /// </summary>
        public ObservableCollection<OperationGuideTopic> Topics { get; } = CreateTopics();

        public OperationGuideTopic? SelectedTopic
        {
            get => _selectedTopic;
            set
            {
                if (_selectedTopic == value) return;
                _selectedTopic = value;
                OnPropertyChanged();
            }
        }

        public OperationGuide(string? initialTopicKey = null)
        {
            SelectedTopic = Topics.FirstOrDefault(topic => topic.Key == initialTopicKey)
                ?? Topics.FirstOrDefault();

            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 各機能の概要、手順、注意点を作成します。
        /// </summary>
        private static ObservableCollection<OperationGuideTopic> CreateTopics()
        {
            return new ObservableCollection<OperationGuideTopic>
            {
                new(
                    "project",
                    "プロジェクトの保存・読込",
                    "TableSmithの設計情報を.tablesmith.jsonファイルとして保存し、後から作業を再開します。",
                    "保存前にメイン画面のプロジェクト名を入力してください。プロジェクトにはDB基本設定、スキーマ、テーブル、カラム、外部キー、インデックスが含まれます。",
                    new[]
                    {
                        "新しい設計を開始する場合は、ファイルメニューの「新規」を選択します。",
                        "初回保存は「名前を付けて保存」を選択し、保存先とファイル名を指定します。",
                        "保存済みプロジェクトを更新する場合は「保存」を選択します。",
                        "既存プロジェクトは「開く」から.tablesmith.jsonファイルを選択します。",
                        "読込後、プロジェクト名、DB基本設定、テーブル一覧が反映されていることを確認します。"
                    },
                    "未保存の変更がある状態で新規作成や終了を行うと保存確認が表示されます。旧形式JSONは不足項目を補完して読み込みます。"),
                new(
                    "database-settings",
                    "DB基本設定",
                    "CREATE文や新規テーブルで使用する、プロジェクト共通のRDB・スキーマ・文字設定を管理します。",
                    "スキーマを追加する場合は、先にメイン画面の「スキーマ管理」で登録してください。",
                    new[]
                    {
                        "メイン画面で「既定RDB」を選択します。CREATE文出力画面の初期選択に使用されます。",
                        "登録済み一覧から「既定スキーマ」を選択します。新規テーブルへ初期設定されます。",
                        "「文字コード」と「照合順序」を選択します。指定RDBの対応するCREATE TABLE文や定義情報へ反映されます。",
                        "「SQL文字コード」を選択します。テーブルごとの.sqlファイル保存時に使用されます。",
                        "表示名の後ろにある角括弧内の値が、JSONやSQLで使用される実際の設定値です。"
                    },
                    "候補はApp.configで変更できます。プロジェクト読込時に候補外の値がある場合は、JSONの値を正として「保存済み設定」に追加し、そのまま選択表示します。"),
                new(
                    "schema",
                    "スキーマ管理",
                    "プロジェクトで利用するスキーマを登録し、新規テーブルへ適用する既定スキーマを設定します。",
                    "標準スキーマとしてdboが初期登録されています。追加するスキーマの物理名を決めておいてください。",
                    new[]
                    {
                        "メイン画面の「スキーマ管理」を選択します。",
                        "「追加」を選択し、スキーマ名と用途が分かる説明を入力します。",
                        "画面上部の「既定スキーマ」から、新規テーブルで使用するスキーマを選択します。",
                        "不要なスキーマを削除する場合は一覧から選択して「削除」を選択します。",
                        "内容を確認して「確定」を選択します。"
                    },
                    "テーブルで使用中のスキーマは削除または名前変更できません。先に対象テーブルのスキーマを変更してください。"),
                new(
                    "table-create",
                    "テーブル定義作成",
                    "テーブル情報とカラム情報を入力し、新しいテーブル定義を作成します。",
                    "外部キーを設定する場合は、参照先となるPKを持つテーブルを先に作成してください。利用するスキーマもスキーマ管理で登録します。",
                    new[]
                    {
                        "メイン画面の「テーブル定義作成」を選択します。",
                        "テーブル物理名、論理名、登録済みスキーマ、説明を入力します。",
                        "必要に応じてテーブル固有の文字コード・照合順序を選択します。「プロジェクト既定」はDB基本設定を使用します。",
                        "カラムを追加・削除し、物理名、論理名、型、サイズ、Not Null、既定値を設定します。",
                        "decimal型はサイズ／全体桁数と小数桁数を設定します。自動採番、保護対象、保護方式も必要に応じて設定します。",
                        "PK・FKを設定します。FKは参照PKを選ぶと型、サイズ／全体桁数などを継承します。",
                        "入力内容を確認して「作成」を選択します。"
                    },
                    "新規テーブルにはIDと監査カラムの標準テンプレートが設定されます。インデックスは作成後にメイン画面の「インデックス管理」から設定します。"),
                new(
                    "column-settings",
                    "カラム設定項目",
                    "テーブル定義作成・編集画面のカラム一覧で設定できる各項目の意味を説明します。",
                    "対象テーブルの用途、主キー、外部キー、データ量、検索方法を整理してから設定してください。",
                    new[]
                    {
                        "テーブル定義作成または編集画面を開きます。",
                        "カラム一覧を横スクロールし、必要な設計項目を設定します。",
                        "下表で各項目の意味と入力条件を確認します。",
                        "入力完了後に「作成」または「更新」を選択し、入力チェック結果を確認します。"
                    },
                    "型によって必要な項目が異なります。decimal型はサイズ／全体桁数が必須です。PKはNot Null、自動採番はPKかつ数値型を推奨します。",
                    new[]
                    {
                        new OperationGuideItem("No", "カラムの表示順", "追加・削除時に自動採番され、編集できません。"),
                        new OperationGuideItem("物理名", "データベース上のカラム名", "英字またはアンダースコアで始め、英数字とアンダースコアを使用します。テーブル内で重複できません。"),
                        new OperationGuideItem("論理名", "業務上の名称・日本語名", "Excel定義書、ER図、RDBのコメント情報へ使用します。"),
                        new OperationGuideItem("型", "カラムのデータ型", "指定RDB向けの型へCREATE文生成時に変換されます。"),
                        new OperationGuideItem("サイズ／全体桁数", "文字列長・整数の設計桁数・decimalの全体桁数", "文字列型、int、bigint、decimalで入力できます。decimal(10,2)では10を設定します。整数型では設計情報として保持し、INT(n)は出力しません。"),
                        new OperationGuideItem("小数桁数", "decimal型の小数部桁数", "decimal型の場合だけ入力できます。0以上かつ全体桁数以下にします。"),
                        new OperationGuideItem("PK", "主キーカラムの指定", "複数カラムを選択すると複合主キーになります。PKにはNot Nullが必要です。"),
                        new OperationGuideItem("Not Null", "NULLを許可しない指定", "必須入力となるカラムで有効にします。"),
                        new OperationGuideItem("FK", "外部キーカラムの指定", "有効にした後、「参照PK」から既存テーブルの主キーを選択します。"),
                        new OperationGuideItem("参照PK", "参照先テーブルの主キー", "選択すると型、サイズ／全体桁数、小数桁数、論理名などを参照元から継承します。"),
                        new OperationGuideItem("自動採番", "RDBの自動採番機能を使用", "数値型の場合だけ設定できます。SQL ServerはIDENTITY、MySQLはAUTO_INCREMENT、OracleはIDENTITY句を出力します。"),
                        new OperationGuideItem("開始値", "自動採番の開始番号", "自動採番を有効にした場合だけ入力できます。1以上、未入力時は1です。"),
                        new OperationGuideItem("増分", "自動採番ごとの増加量", "自動採番を有効にした場合だけ入力できます。1以上、未入力時は1です。"),
                        new OperationGuideItem("保護対象", "暗号化・マスキング等の対象指定", "v0.3.0では設計情報として保持するだけで、実際の保護処理は行いません。"),
                        new OperationGuideItem("保護方式", "想定する保護方法のメモ", "保護対象を有効にした場合だけ入力できます。例: アプリ側暗号化、DB側暗号化、ハッシュ化、マスキング。"),
                        new OperationGuideItem("既定値", "INSERT時に値がない場合の初期値", "SQL式またはリテラルを入力します。例: CURRENT_TIMESTAMP、1、'todo'。"),
                        new OperationGuideItem("説明", "用途・入力規則・補足情報", "Excel定義書やRDBのカラムコメントへ出力されます。")
                    }),
                new(
                    "table-list",
                    "テーブル一覧・編集",
                    "作成済みテーブルの定義を確認し、テーブル情報やカラム情報を変更します。",
                    "プロジェクト内にテーブルが1件以上必要です。",
                    new[]
                    {
                        "メイン画面の「テーブル一覧」を選択します。",
                        "左側の一覧から確認するテーブルを選択します。",
                        "右側でテーブル情報とカラム情報を確認します。",
                        "変更する場合は「編集」を選択します。",
                        "テーブル定義編集画面で修正し、「更新」を選択します。"
                    },
                    "テーブル名やカラム名を変更した場合は、外部キーとインデックスの参照先が正しいか確認してください。"),
                new(
                    "index",
                    "インデックス管理",
                    "テーブルごとに検索・一意性を支援するインデックスと対象カラムの順序を設定します。",
                    "対象テーブルとインデックスに含めるカラムを先に作成してください。",
                    new[]
                    {
                        "メイン画面の「インデックス管理」を選択します。",
                        "左側から対象テーブルを選択し、「定義を編集」を選択します。",
                        "「追加」を選択します。先頭カラムが初期設定され、標準名が自動生成されます。",
                        "対象カラムを選択し、必要に応じてカラムを追加・削除します。",
                        "各カラムをASCまたはDESCに設定します。複合インデックスは一覧の順序がSQLのカラム順になります。",
                        "必要に応じてUnique、SQL Server向けのClustered、説明を設定します。",
                        "標準名はIX_テーブル名_カラム1_カラム2です。任意名が必要な場合のみ「カスタム名」を有効にします。",
                        "内容を確認して「確定」を選択します。"
                    },
                    "同一テーブル内でインデックス名は重複できません。同一インデックスへ同じカラムを複数指定することもできません。"),
                new(
                    "create-sql",
                    "CREATE文出力",
                    "テーブル定義からSQL Server、MySQL、Oracle向けのCREATE TABLE文とインデックス作成文を生成します。",
                    "出力対象テーブルを1件以上作成してください。スキーマ、文字設定、カラム、制約、インデックスを事前に確認します。",
                    new[]
                    {
                        "メイン画面の「CREATE文」を選択します。",
                        "対象RDBを選び、出力するテーブルをチェックします。「全選択」「全解除」も利用できます。",
                        "「SQL表示」を選択し、複数テーブルをまとめたDDLを確認します。",
                        "ファイル保存する場合は「SQLファイル出力」を選択します。",
                        "保存先フォルダを指定します。テーブルごとにテーブル物理名.sqlが作成されます。"
                    },
                    "decimalの全体桁数・小数桁数、自動採番、PK・FK、論理名・説明、インデックスがRDB別の形式で出力されます。ファイル文字コードはDB基本設定の「SQL文字コード」を使用します。"),
                new(
                    "definition",
                    "テーブル定義書出力",
                    "選択したテーブルの設計情報を、一覧・テーブル定義・インデックス一覧を含むExcelファイルへ出力します。",
                    "出力対象テーブルを1件以上作成してください。",
                    new[]
                    {
                        "メイン画面の「定義書出力」を選択します。",
                        "出力対象テーブルを選択します。全選択・全解除も利用できます。",
                        "「出力」を選択し、Excelファイル名と保存先を指定します。",
                        "テーブル一覧、テーブルごとの定義、インデックス一覧シートを確認します。",
                        "スキーマ、文字コード、照合順序、サイズ／全体桁数、小数桁数、自動採番、保護設定が反映されていることを確認します。"
                    },
                    "既存の同名ファイルを開いている場合は保存できないため、Excelを閉じてから再実行してください。"),
                new(
                    "er-diagram",
                    "ER図出力",
                    "プロジェクト内の全テーブル、カラム、PK・FK、外部キー関係をER図として表示・保存します。",
                    "テーブルを1件以上作成してください。リレーションを表示する場合は外部キーの参照PKを設定します。",
                    new[]
                    {
                        "メイン画面の「ER図」を選択します。",
                        "プレビューでテーブル、カラム、PK、FK、リレーションを確認します。",
                        "必要に応じて表示倍率を変更します。",
                        "SVGまたはPNGの保存ボタンを選択し、保存先を指定します。"
                    },
                    "ER図は全テーブルを対象にします。decimal型は全体桁数・小数桁数を表示しますが、インデックスは表示しません。")
            };
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    /// <summary>
    /// 1機能分の操作説明を保持します。
    /// </summary>
    public class OperationGuideTopic
    {
        public string Key { get; }
        public string Title { get; }
        public string Overview { get; }
        public string Prerequisite { get; }
        public IReadOnlyList<OperationGuideStep> Steps { get; }
        public IReadOnlyList<OperationGuideItem> Items { get; }
        public bool HasItems => Items.Count > 0;
        public string Note { get; }

        public OperationGuideTopic(
            string key,
            string title,
            string overview,
            string prerequisite,
            IEnumerable<string> steps,
            string note,
            IEnumerable<OperationGuideItem>? items = null)
        {
            Key = key;
            Title = title;
            Overview = overview;
            Prerequisite = prerequisite;
            Steps = steps
                .Select((text, index) => new OperationGuideStep(index + 1, text))
                .ToList();
            Items = items?.ToList()
                ?? (IReadOnlyList<OperationGuideItem>)Array.Empty<OperationGuideItem>();
            Note = note;
        }
    }

    /// <summary>
    /// 操作説明の番号付き手順です。
    /// </summary>
    public class OperationGuideStep
    {
        public int Number { get; }
        public string Text { get; }

        public OperationGuideStep(int number, string text)
        {
            Number = number;
            Text = text;
        }
    }

    /// <summary>
    /// カラムなどの設定項目に対する説明です。
    /// </summary>
    public class OperationGuideItem
    {
        public string Name { get; }
        public string Input { get; }
        public string Detail { get; }

        public OperationGuideItem(string name, string input, string detail)
        {
            Name = name;
            Input = input;
            Detail = detail;
        }
    }
}
