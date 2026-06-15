using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TableSmith.Models
{
    public class TableDefinition : INotifyPropertyChanged
    {
        private string _tableName = string.Empty;
        private string _tableDisplayName = string.Empty;
        private string _description = string.Empty;
        private ObservableCollection<ColumnDefinition> _columns = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        public string TableName
        {
            get => _tableName;
            set
            {
                if (_tableName == value)
                {
                    return;
                }

                _tableName = value;
                OnPropertyChanged();
            }
        }

        public string TableDisplayName
        {
            get => _tableDisplayName;
            set
            {
                if (_tableDisplayName == value)
                {
                    return;
                }

                _tableDisplayName = value;
                OnPropertyChanged();
            }
        }

        public string Description
        {
            get => _description;
            set
            {
                if (_description == value)
                {
                    return;
                }

                _description = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<ColumnDefinition> Columns
        {
            get => _columns;
            set
            {
                if (_columns == value)
                {
                    return;
                }

                _columns = value;
                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
