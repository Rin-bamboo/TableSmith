using System.Collections.ObjectModel;
using System.Windows;
using MahApps.Metro.Controls;
using TableSmith.Models;
using TableSmith.Services;

namespace TableSmith.Views
{
    /// <summary>
    /// CREATE文を生成するテーブルを選択する画面です。
    /// </summary>
    public partial class CreateSqlExport : MetroWindow
    {
        private readonly CreateTableSqlService _createTableSqlService = new();

        public ObservableCollection<TableExportSelectionItem> SelectionItems { get; }

        public CreateSqlExport(IEnumerable<TableDefinition> tables)
        {
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
        /// 選択したテーブルのCREATE文をまとめて生成し、プレビュー表示します。
        /// </summary>
        private void PreviewButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedTables = this.SelectionItems
                .Where(item => item.IsSelected)
                .Select(item => item.Table)
                .ToList();

            if (selectedTables.Count == 0)
            {
                MessageBox.Show("CREATE文を出力するテーブルを1件以上選択してください。", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var sql = _createTableSqlService.GenerateAll(selectedTables);
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
        /// CREATE文を生成せずに画面を閉じます。
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
