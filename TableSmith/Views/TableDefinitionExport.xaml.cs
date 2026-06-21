using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using TableSmith.Models;
using TableSmith.Services;

namespace TableSmith.Views
{
    /// <summary>
    /// Excel定義書へ出力するテーブルを選択する画面です。
    /// </summary>
    public partial class TableDefinitionExport : MetroWindow
    {
        private readonly TableDefinitionExcelService _tableDefinitionExcelService = new();
        private readonly string _projectName;
        private readonly DatabaseSettings _databaseSettings;

        public ObservableCollection<TableExportSelectionItem> SelectionItems { get; }

        public TableDefinitionExport(
            string projectName,
            IEnumerable<TableDefinition> tables,
            DatabaseSettings? databaseSettings = null)
        {
            this._projectName = projectName;
            this._databaseSettings = databaseSettings ?? new DatabaseSettings();
            this.SelectionItems = new ObservableCollection<TableExportSelectionItem>(
                tables.Select(table => new TableExportSelectionItem(table)));

            InitializeComponent();
            this.DataContext = this;
        }

        /// <summary>
        /// すべてのテーブルを出力対象にします。
        /// </summary>
        private void SelectAllButton_Click(object sender, RoutedEventArgs e)
        {
            SetSelection(true);
        }

        /// <summary>
        /// すべてのテーブルを出力対象から外します。
        /// </summary>
        private void ClearAllButton_Click(object sender, RoutedEventArgs e)
        {
            SetSelection(false);
        }

        /// <summary>
        /// 選択状態を全テーブルへ一括設定します。
        /// </summary>
        private void SetSelection(bool isSelected)
        {
            foreach (var item in this.SelectionItems)
            {
                item.IsSelected = isSelected;
            }
        }

        /// <summary>
        /// 選択したテーブルを1つのExcel定義書へ出力します。
        /// </summary>
        private void ExportButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedTables = this.SelectionItems
                .Where(item => item.IsSelected)
                .Select(item => item.Table)
                .ToList();

            if (selectedTables.Count == 0)
            {
                MessageBox.Show("出力するテーブルを1件以上選択してください。", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new SaveFileDialog
            {
                Title = "テーブル定義書を出力",
                Filter = "Excel Workbook (*.xlsx)|*.xlsx",
                FileName = $"{CreateSafeFileName(this._projectName)}_テーブル定義書.xlsx",
                AddExtension = true,
                DefaultExt = ".xlsx"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            try
            {
                _tableDefinitionExcelService.Export(
                    dialog.FileName,
                    this._projectName,
                    selectedTables,
                    this._databaseSettings);
                MessageBox.Show("テーブル定義書を出力しました。", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
                this.DialogResult = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"テーブル定義書の出力に失敗しました。\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 出力せずに画面を閉じます。
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
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
    }
}
