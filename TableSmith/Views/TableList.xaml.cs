using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using MahApps.Metro.Controls;
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

        public event PropertyChangedEventHandler? PropertyChanged;

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

        public TableList(ObservableCollection<TableDefinition> tables)
        {
            InitializeComponent();
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
