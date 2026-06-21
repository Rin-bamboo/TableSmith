using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using TableSmith.Models;
using TableSmith.Services;

namespace TableSmith.Views
{
    /// <summary>
    /// メイン画面です。
    /// </summary>
    public partial class MainWindow : MetroWindow, INotifyPropertyChanged
    {
        private readonly ProjectJsonService _projectJsonService = new();
        private string? _currentProjectFilePath;
        private bool _hasUnsavedChanges;
        private string _projectName = string.Empty;
        private DatabaseSettings _databaseSettings = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public TableDefinition CurrentTable { get; set; } = new();
        public ObservableCollection<TableDefinition> Tables { get; } = new();
        public IReadOnlyList<SqlDialect> SqlDialectValues { get; } = Enum.GetValues<SqlDialect>();
        public IReadOnlyList<ConfigSelectionOption> CharacterSetOptions { get; private set; } =
            Array.Empty<ConfigSelectionOption>();
        public IReadOnlyList<ConfigSelectionOption> CollationOptions { get; private set; } =
            Array.Empty<ConfigSelectionOption>();
        public IReadOnlyList<SqlFileEncodingOption> SqlFileEncodingOptions { get; private set; } =
            Array.Empty<SqlFileEncodingOption>();

        /// <summary>
        /// プロジェクト全体のDB基本設定です。
        /// </summary>
        public DatabaseSettings DatabaseSettings
        {
            get => _databaseSettings;
            private set
            {
                if (_databaseSettings == value)
                {
                    return;
                }

                _databaseSettings.PropertyChanged -= DatabaseSettings_PropertyChanged;
                _databaseSettings = value;
                _databaseSettings.PropertyChanged += DatabaseSettings_PropertyChanged;

                // 読み込んだ値がコンフィグ候補外でも失われないよう、
                // DatabaseSettingsの変更通知より先に新しい値を含む候補一覧を準備します。
                RefreshConfigSelectionOptions();
                OnPropertyChanged();
            }
        }

        public string ProjectName
        {
            get => _projectName;
            set
            {
                if (_projectName == value)
                {
                    return;
                }

                _projectName = value;
                this._hasUnsavedChanges = true;
                OnPropertyChanged();
            }
        }

        public MainWindow()
        {
            _databaseSettings.PropertyChanged += DatabaseSettings_PropertyChanged;
            RefreshConfigSelectionOptions();
            InitializeComponent();
            this.DataContext = this;
        }

        /// <summary>
        /// 現在のプロジェクトを閉じ、新しい空のプロジェクトを開始します。
        /// </summary>
        private void NewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!ConfirmSaveUnsavedChanges())
            {
                return;
            }

            this.Tables.Clear();
            this.CurrentTable = new TableDefinition();
            this.ProjectName = string.Empty;
            this.DatabaseSettings = new DatabaseSettings();
            RefreshConfigSelectionOptions();
            this._currentProjectFilePath = null;
            this._hasUnsavedChanges = false;
        }

        /// <summary>
        /// JSONファイルからTableSmithプロジェクトを読み込みます。
        /// </summary>
        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (!ConfirmSaveUnsavedChanges())
            {
                return;
            }

            var dialog = new OpenFileDialog
            {
                Title = "TableSmith プロジェクトを開く",
                Filter = "TableSmith Project (*.tablesmith.json)|*.tablesmith.json|JSON File (*.json)|*.json|All Files (*.*)|*.*"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            try
            {
                var project = _projectJsonService.Load(dialog.FileName);
                LoadProject(project);
                this._currentProjectFilePath = dialog.FileName;
                this._hasUnsavedChanges = false;
                MessageBox.Show("プロジェクトを読み込みました。", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"プロジェクトの読み込みに失敗しました。\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 現在の保存先へプロジェクトを保存します。
        /// </summary>
        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveProject(saveAs: false);
        }

        /// <summary>
        /// 保存先を選択してプロジェクトを保存します。
        /// </summary>
        private void SaveAsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveProject(saveAs: true);
        }

        /// <summary>
        /// アプリケーションを終了します。未保存変更がある場合は終了前に確認します。
        /// </summary>
        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// アプリケーションのバージョン情報画面を表示します。
        /// </summary>
        private void VersionInfoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var versionInfo = new VersionInfo
            {
                Owner = this
            };

            versionInfo.ShowDialog();
        }

        /// <summary>
        /// 選択された機能の操作説明画面を表示します。
        /// </summary>
        private void OperationGuideMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var topicKey = (sender as System.Windows.Controls.MenuItem)?.Tag as string;
            var operationGuide = new OperationGuide(topicKey)
            {
                Owner = this
            };

            operationGuide.ShowDialog();
        }

        /// <summary>
        /// プロジェクトで使用するスキーマ一覧の管理画面を表示します。
        /// </summary>
        private void ButtonSchemaManagement_Click(object sender, RoutedEventArgs e)
        {
            var schemaManagement = new SchemaManagement(DatabaseSettings, Tables)
            {
                Owner = this
            };

            if (schemaManagement.ShowDialog() == true)
            {
                _hasUnsavedChanges = true;
            }
        }

        /// <summary>
        /// プロジェクト内テーブルのインデックスを管理する画面を表示します。
        /// </summary>
        private void ButtonIndexManagement_Click(object sender, RoutedEventArgs e)
        {
            if (Tables.Count == 0)
            {
                MessageBox.Show(
                    "インデックスを設定するテーブルがありません。",
                    "確認",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var indexManagement = new IndexManagement(Tables)
            {
                Owner = this
            };

            indexManagement.ShowDialog();
            if (indexManagement.HasChanges)
            {
                _hasUnsavedChanges = true;
            }
        }

        /// <summary>
        /// テーブル作成画面を開き、確定されたテーブルを現在のプロジェクトへ追加します。
        /// </summary>
        private void ButtonTableCreate_Click(object sender, RoutedEventArgs e)
        {
            var tableCreate = new TableCreate(this.Tables, this.DatabaseSettings)
            {
                Owner = this
            };

            if (tableCreate.ShowDialog() == true)
            {
                this.CurrentTable = tableCreate.CurrentTable;
                this.Tables.Add(tableCreate.CurrentTable);
                this._hasUnsavedChanges = true;
                MessageBox.Show("テーブル情報を作成しました。", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        /// <summary>
        /// テーブル一覧画面を開き、一覧画面で編集が発生した場合は未保存状態にします。
        /// </summary>
        private void ButtonTableList_Click(object sender, RoutedEventArgs e)
        {
            var tableList = new TableList(this.Tables, this.DatabaseSettings)
            {
                Owner = this
            };

            tableList.ShowDialog();
            if (tableList.HasChanges)
            {
                this._hasUnsavedChanges = true;
            }
        }

        /// <summary>
        /// テーブル選択画面を開き、選択されたテーブルの定義書をExcelへ出力します。
        /// </summary>
        private void ButtonDefinitionExport_Click(object sender, RoutedEventArgs e)
        {
            if (this.Tables.Count == 0)
            {
                MessageBox.Show("定義書を出力するテーブルがありません。", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var exportWindow = new TableDefinitionExport(
                this.ProjectName,
                this.Tables,
                this.DatabaseSettings)
            {
                Owner = this
            };

            exportWindow.ShowDialog();
        }

        /// <summary>
        /// プロジェクト内の全テーブルからER図を生成してプレビュー表示します。
        /// </summary>
        private void ButtonErDiagram_Click(object sender, RoutedEventArgs e)
        {
            if (this.Tables.Count == 0)
            {
                MessageBox.Show("ER図を作成するテーブルがありません。", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var preview = new ErDiagramPreview(this.ProjectName, this.Tables)
                {
                    Owner = this
                };

                preview.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ER図の作成に失敗しました。\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// CREATE文の出力対象テーブルを選択する画面を開きます。
        /// </summary>
        private void ButtonCreateSql_Click(object sender, RoutedEventArgs e)
        {
            if (this.Tables.Count == 0)
            {
                MessageBox.Show("CREATE文を作成するテーブルがありません。", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var exportWindow = new CreateSqlExport(this.Tables, this.DatabaseSettings)
            {
                Owner = this
            };

            exportWindow.ShowDialog();
        }

        /// <summary>
        /// ウィンドウを閉じる前に未保存変更の保存確認を行います。
        /// </summary>
        protected override void OnClosing(CancelEventArgs e)
        {
            if (!ConfirmSaveUnsavedChanges())
            {
                e.Cancel = true;
                return;
            }

            base.OnClosing(e);
        }

        /// <summary>
        /// プロジェクトをJSONファイルへ保存します。
        /// </summary>
        /// <param name="saveAs">trueの場合は保存先を選択し直します。</param>
        /// <returns>保存できた場合はtrue、キャンセルまたは失敗した場合はfalse。</returns>
        private bool SaveProject(bool saveAs)
        {
            this.ProjectName = this.ProjectName.Trim();
            if (string.IsNullOrWhiteSpace(this.ProjectName))
            {
                MessageBox.Show("プロジェクト名を入力してください。", "入力確認", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            var filePath = this._currentProjectFilePath;
            if (saveAs || string.IsNullOrWhiteSpace(filePath))
            {
                var dialog = new SaveFileDialog
                {
                    Title = "TableSmith プロジェクトを保存",
                    Filter = "TableSmith Project (*.tablesmith.json)|*.tablesmith.json|JSON File (*.json)|*.json|All Files (*.*)|*.*",
                    FileName = $"{CreateSafeFileName(this.ProjectName)}.tablesmith.json",
                    AddExtension = true,
                    DefaultExt = ".tablesmith.json"
                };

                if (dialog.ShowDialog(this) != true)
                {
                    return false;
                }

                filePath = dialog.FileName;
            }

            try
            {
                _projectJsonService.Save(filePath, CreateProject());
                this._currentProjectFilePath = filePath;
                this._hasUnsavedChanges = false;
                MessageBox.Show("プロジェクトを保存しました。", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"プロジェクトの保存に失敗しました。\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
        }

        /// <summary>
        /// 現在のテーブル一覧から保存用のプロジェクトモデルを作成します。
        /// </summary>
        private TableSmithProject CreateProject()
        {
            return new TableSmithProject
            {
                Version = 2,
                ProjectName = this.ProjectName,
                DatabaseSettings = this.DatabaseSettings,
                Tables = new ObservableCollection<TableDefinition>(this.Tables)
            };
        }

        /// <summary>
        /// 読み込んだプロジェクトモデルを画面上のテーブル一覧へ反映します。
        /// </summary>
        private void LoadProject(TableSmithProject project)
        {
            // ComboBoxが古い候補一覧を使って読込値を未選択へ戻さないよう、
            // プロジェクト反映中はバインディングを一時的に切り離します。
            this.DataContext = null;
            try
            {
                this.ProjectName = project.ProjectName;
                this.DatabaseSettings = project.DatabaseSettings;
                this.Tables.Clear();
                foreach (var table in project.Tables)
                {
                    this.Tables.Add(table);
                }

                this.CurrentTable = this.Tables.FirstOrDefault() ?? new TableDefinition();
            }
            finally
            {
                this.DataContext = this;
            }
        }

        /// <summary>
        /// プロジェクト名をファイル名として利用できる文字列へ変換します。
        /// </summary>
        private static string CreateSafeFileName(string projectName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var safeName = new string(projectName
                .Select(character => invalidChars.Contains(character) ? '_' : character)
                .ToArray())
                .Trim();

            return string.IsNullOrWhiteSpace(safeName) ? "TableSmithProject" : safeName;
        }

        /// <summary>
        /// 未保存変更がある場合に保存・破棄・キャンセルをユーザーに確認します。
        /// </summary>
        /// <returns>後続処理を続けてよい場合はtrue、キャンセルする場合はfalse。</returns>
        private bool ConfirmSaveUnsavedChanges()
        {
            if (!this._hasUnsavedChanges)
            {
                return true;
            }

            var result = MessageBox.Show(
                "保存されていない変更があります。保存しますか？",
                "確認",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);

            return result switch
            {
                MessageBoxResult.Yes => SaveProject(saveAs: false),
                MessageBoxResult.No => true,
                _ => false
            };
        }

        /// <summary>
        /// 画面バインディングへプロパティ変更を通知します。
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// DB基本設定の変更を未保存変更として記録します。
        /// </summary>
        private void DatabaseSettings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            _hasUnsavedChanges = true;
        }

        /// <summary>
        /// App.configの定義と現在値から、画面表示用の選択肢を更新します。
        /// </summary>
        private void RefreshConfigSelectionOptions()
        {
            CharacterSetOptions = ConfigSelectionService.GetCharacterSetOptions(
                DatabaseSettings.DefaultCharacterSet,
                includeEmpty: true,
                emptyDisplayName: "未設定");
            CollationOptions = ConfigSelectionService.GetCollationOptions(
                DatabaseSettings.DefaultCollation,
                includeEmpty: true,
                emptyDisplayName: "未設定");
            SqlFileEncodingOptions = ConfigSelectionService.GetSqlFileEncodingOptions(
                DatabaseSettings.SqlFileEncoding);

            OnPropertyChanged(nameof(CharacterSetOptions));
            OnPropertyChanged(nameof(CollationOptions));
            OnPropertyChanged(nameof(SqlFileEncodingOptions));
        }
    }
}
