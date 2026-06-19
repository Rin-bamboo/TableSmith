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
        private readonly CreateSqlFileExportService _createSqlFileExportService = new();

        public ObservableCollection<TableExportSelectionItem> SelectionItems { get; }
        public IReadOnlyList<SqlDialectOption> DialectOptions { get; }
        public SqlDialectOption SelectedDialectOption { get; set; }

        public CreateSqlExport(IEnumerable<TableDefinition> tables)
        {
            this.SelectionItems = new ObservableCollection<TableExportSelectionItem>(
                tables.Select(table => new TableExportSelectionItem(table)));
            this.DialectOptions = new[]
            {
                new SqlDialectOption { Dialect = SqlDialect.SqlServer, DisplayName = "SQL Server" },
                new SqlDialectOption { Dialect = SqlDialect.MySql, DisplayName = "MySQL" },
                new SqlDialectOption { Dialect = SqlDialect.Oracle, DisplayName = "Oracle" }
            };
            this.SelectedDialectOption = this.DialectOptions[0];

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
            var selectedTables = GetSelectedTables();

            if (selectedTables.Count == 0)
            {
                MessageBox.Show("CREATE文を出力するテーブルを1件以上選択してください。", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                var sql = _createTableSqlService.GenerateAll(
                    selectedTables,
                    this.SelectedDialectOption.Dialect);
                var preview = new SqlPreview(
                    sql,
                    $"TableSmith: CREATE TABLE - {this.SelectedDialectOption.DisplayName}")
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
        /// 保存先フォルダを選択し、各テーブルのCREATE文を個別のSQLファイルへ出力します。
        /// </summary>
        private void ExportFilesButton_Click(object sender, RoutedEventArgs e)
        {
            var selectedTables = GetSelectedTables();
            if (selectedTables.Count == 0)
            {
                MessageBox.Show("CREATE文を出力するテーブルを1件以上選択してください。", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "CREATE文の保存先フォルダを選択してください",
                Multiselect = false
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            try
            {
                var outputCount = _createSqlFileExportService.Export(
                    dialog.FolderName,
                    selectedTables,
                    this.SelectedDialectOption.Dialect);

                MessageBox.Show(
                    $"{outputCount}件のSQLファイルを出力しました。\n保存先: {dialog.FolderName}",
                    "Info",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"SQLファイルの出力に失敗しました。\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 現在チェックされているテーブル一覧を取得します。
        /// </summary>
        private List<TableDefinition> GetSelectedTables()
        {
            return this.SelectionItems
                .Where(item => item.IsSelected)
                .Select(item => item.Table)
                .ToList();
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
