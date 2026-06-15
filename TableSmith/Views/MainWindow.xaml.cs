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

        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveProject(saveAs: false);
        }

        private void SaveAsMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SaveProject(saveAs: true);
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

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

        private void ButtonTableList_Click(object sender, RoutedEventArgs e)
        {
            var tableList = new TableList(this.Tables)
            {
                Owner = this
            };

            tableList.ShowDialog();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (!ConfirmSaveUnsavedChanges())
            {
                e.Cancel = true;
                return;
            }

            base.OnClosing(e);
        }

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

        private TableSmithProject CreateProject()
        {
            return new TableSmithProject
            {
                Version = 1,
                Tables = new ObservableCollection<TableDefinition>(this.Tables)
            };
        }

        private void LoadProject(TableSmithProject project)
        {
            this.Tables.Clear();
            foreach (var table in project.Tables)
            {
                this.Tables.Add(table);
            }

            this.CurrentTable = this.Tables.FirstOrDefault() ?? new TableDefinition();
        }

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
