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
    /// 作成済みテーブルの一覧画面です。
    /// </summary>
    public partial class TableList : MetroWindow, INotifyPropertyChanged
    {
        private TableDefinition? _selectedTable;
        private readonly CreateTableSqlService _createTableSqlService = new();
        private readonly TableDefinitionExcelService _tableDefinitionExcelService = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public string ProjectName { get; }
        public ObservableCollection<TableDefinition> Tables { get; }
        public bool HasChanges { get; private set; }

        public TableDefinition? SelectedTable
        {
            get => _selectedTable;
            set
            {
                if (_selectedTable == value)
                {
                    return;
                }

                _selectedTable = value;
                OnPropertyChanged();
            }
        }

        public TableList(string projectName, ObservableCollection<TableDefinition> tables)
        {
            InitializeComponent();
            this.ProjectName = projectName;
            this.Tables = tables;
            this.SelectedTable = this.Tables.Count > 0 ? this.Tables[0] : null;
            this.DataContext = this;
        }

        /// <summary>
        /// テーブル一覧画面を閉じます。
        /// </summary>
        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 選択中テーブルを編集画面で開き、確定された内容を一覧へ反映します。
        /// </summary>
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedTable == null)
            {
                MessageBox.Show("編集するテーブルを選択してください。", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var tableCreate = new TableCreate(this.Tables, this.SelectedTable)
            {
                Owner = this
            };

            if (tableCreate.ShowDialog() != true)
            {
                return;
            }

            TableCreate.CopyTableValues(tableCreate.CurrentTable, this.SelectedTable);
            this.HasChanges = true;
            OnPropertyChanged(nameof(SelectedTable));
        }

        /// <summary>
        /// 選択中テーブルのCREATE TABLE文を生成して表示します。
        /// </summary>
        private void CreateSqlButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedTable == null)
            {
                MessageBox.Show("CREATE文を作成するテーブルを選択してください。", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var sql = _createTableSqlService.Generate(this.SelectedTable);
                var preview = new SqlPreview(sql)
                {
                    Owner = this
                };

                preview.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"CREATE文の作成に失敗しました。\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 全テーブルのCREATE TABLE文をまとめて生成して表示します。
        /// </summary>
        private void CreateAllSqlButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var sql = _createTableSqlService.GenerateAll(this.Tables);
                var preview = new SqlPreview(sql)
                {
                    Owner = this
                };

                preview.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"CREATE文の作成に失敗しました。\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 選択中テーブルの定義書をExcelファイルへ出力します。
        /// </summary>
        private void ExportSelectedExcelButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedTable == null)
            {
                MessageBox.Show("定義書を出力するテーブルを選択してください。", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ExportExcel(
                new[] { this.SelectedTable },
                $"{this.SelectedTable.TableName}_テーブル定義書.xlsx");
        }

        /// <summary>
        /// 全テーブルの定義書を1つのExcelファイルへ出力します。
        /// </summary>
        private void ExportAllExcelButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.Tables.Count == 0)
            {
                MessageBox.Show("定義書を出力するテーブルがありません。", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ExportExcel(this.Tables, $"{CreateSafeFileName(this.ProjectName)}_テーブル定義書.xlsx");
        }

        /// <summary>
        /// 保存先を選択し、指定されたテーブル一覧をExcel定義書として出力します。
        /// </summary>
        private void ExportExcel(IEnumerable<TableDefinition> tables, string defaultFileName)
        {
            var dialog = new SaveFileDialog
            {
                Title = "テーブル定義書を出力",
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = defaultFileName,
                AddExtension = true,
                DefaultExt = ".xlsx"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            try
            {
                _tableDefinitionExcelService.Export(dialog.FileName, this.ProjectName, tables);
                MessageBox.Show("テーブル定義書を出力しました。", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"テーブル定義書の出力に失敗しました。\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
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

            return string.IsNullOrWhiteSpace(safeName) ? "TableSmith" : safeName;
        }

        /// <summary>
        /// テーブル一覧画面を閉じます。
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 画面バインディングへプロパティ変更を通知します。
        /// </summary>
        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
