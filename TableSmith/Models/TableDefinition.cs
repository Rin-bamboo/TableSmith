using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TableSmith.Models
{
    /// <summary>
    /// 1テーブル分の設計情報を表します。
    /// </summary>
    public class TableDefinition : INotifyPropertyChanged
    {
        private string _tableName = string.Empty;
        private string _tableDisplayName = string.Empty;
        private string _description = string.Empty;
        private string _schemaName = string.Empty;
        private string _characterSet = string.Empty;
        private string _collation = string.Empty;
        private ObservableCollection<ColumnDefinition> _columns = new();
        private ObservableCollection<IndexDefinition> _indexes = new();

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

        /// <summary>
        /// テーブルが所属するスキーマ名です。未設定の場合はプロジェクトの既定値を使用します。
        /// </summary>
        public string SchemaName
        {
            get => _schemaName;
            set
            {
                if (_schemaName == value)
                {
                    return;
                }

                _schemaName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// テーブル単位の文字コードです。未設定の場合はプロジェクトの既定値を使用します。
        /// </summary>
        public string CharacterSet
        {
            get => _characterSet;
            set
            {
                if (_characterSet == value)
                {
                    return;
                }

                _characterSet = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// テーブル単位の照合順序です。未設定の場合はプロジェクトの既定値を使用します。
        /// </summary>
        public string Collation
        {
            get => _collation;
            set
            {
                if (_collation == value)
                {
                    return;
                }

                _collation = value;
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

        /// <summary>
        /// テーブルに設定するインデックス一覧です。
        /// </summary>
        public ObservableCollection<IndexDefinition> Indexes
        {
            get => _indexes;
            set
            {
                if (_indexes == value)
                {
                    return;
                }

                _indexes = value;
                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
