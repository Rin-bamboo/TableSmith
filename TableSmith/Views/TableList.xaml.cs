using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using MahApps.Metro.Controls;
using TableSmith.Models;

namespace TableSmith.Views
{
    /// <summary>
    /// 作成済みテーブルの一覧画面です。
    /// </summary>
    public partial class TableList : MetroWindow, INotifyPropertyChanged
    {
        private TableDefinition? _selectedTable;

        public event PropertyChangedEventHandler? PropertyChanged;

        public ObservableCollection<TableDefinition> Tables { get; }

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

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
