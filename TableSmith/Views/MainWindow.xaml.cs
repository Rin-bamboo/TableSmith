using System.Collections.ObjectModel;
using System.ComponentModel;
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
    public partial class MainWindow : MetroWindow
    {
        private readonly ProjectJsonService _projectJsonService = new();
        private string? _currentProjectFilePath;
        private bool _hasUnsavedChanges;

        public TableDefinition CurrentTable { get; set; } = new();
        public ObservableCollection<TableDefinition> Tables { get; } = new();

        public MainWindow()
        {
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
        /// テーブル作成画面を開き、確定されたテーブルを現在のプロジェクトへ追加します。
        /// </summary>
        private void ButtonTableCreate_Click(object sender, RoutedEventArgs e)
        {
            var tableCreate = new TableCreate(this.Tables)
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
            var tableList = new TableList(this.Tables)
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
            var filePath = this._currentProjectFilePath;
            if (saveAs || string.IsNullOrWhiteSpace(filePath))
            {
                var dialog = new SaveFileDialog
                {
                    Title = "TableSmith プロジェクトを保存",
                    Filter = "TableSmith Project (*.tablesmith.json)|*.tablesmith.json|JSON File (*.json)|*.json|All Files (*.*)|*.*",
                    FileName = "table-smith-project.tablesmith.json",
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
                Version = 1,
                Tables = new ObservableCollection<TableDefinition>(this.Tables)
            };
        }

        /// <summary>
        /// 読み込んだプロジェクトモデルを画面上のテーブル一覧へ反映します。
        /// </summary>
        private void LoadProject(TableSmithProject project)
        {
            this.Tables.Clear();
            foreach (var table in project.Tables)
            {
                this.Tables.Add(table);
            }

            this.CurrentTable = this.Tables.FirstOrDefault() ?? new TableDefinition();
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
    }
}
