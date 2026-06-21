using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using MahApps.Metro.Controls;
using TableSmith.Models;

namespace TableSmith.Views
{
    /// <summary>
    /// プロジェクト内のテーブルを選択してインデックス定義を管理する画面です。
    /// </summary>
    public partial class IndexManagement : MetroWindow, INotifyPropertyChanged
    {
        private TableDefinition? _selectedTable;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<TableDefinition> Tables { get; }
        public bool HasChanges { get; private set; }

        public TableDefinition? SelectedTable
        {
            get => _selectedTable;
            set
            {
                if (_selectedTable == value) return;
                _selectedTable = value;
                OnPropertyChanged();
            }
        }

        public IndexManagement(ObservableCollection<TableDefinition> tables)
        {
            Tables = tables;
            SelectedTable = tables.FirstOrDefault();

            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 選択中テーブルのインデックス定義編集画面を開きます。
        /// </summary>
        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedTable == null)
            {
                MessageBox.Show(
                    "インデックスを編集するテーブルを選択してください。",
                    "確認",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }

            var editor = new IndexDefinitionEdit(SelectedTable)
            {
                Owner = this
            };

            if (editor.ShowDialog() == true)
            {
                HasChanges = true;
                OnPropertyChanged(nameof(SelectedTable));
            }
        }

        /// <summary>
        /// インデックス管理画面を閉じます。
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
