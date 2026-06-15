using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace TableSmith.Models
{
    public class ColumnDefinition : INotifyPropertyChanged
    {
        private bool _isForeignKey;
        private string _foreignKeyReferenceId = string.Empty;
        private string _referenceTableName = string.Empty;
        private string _referenceColumnName = string.Empty;
        private string _autoForeignKeyColumnName = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        private int _no;
        private string _columnName = string.Empty;
        private string _columnDisplayName = string.Empty;
        private string _dataType = "nvarchar";
        private int? _dataSize;
        private bool _isPrimaryKey;
        private bool _isNotNull;

        public int No
        {
            get => _no;
            set
            {
                if (_no == value)
                {
                    return;
                }

                _no = value;
                OnPropertyChanged();
            }
        }

        public string ColumnName
        {
            get => _columnName;
            set
            {
                if (_columnName == value)
                {
                    return;
                }

                _columnName = value;
                OnPropertyChanged();
            }
        }

        public string ColumnDisplayName
        {
            get => _columnDisplayName;
            set
            {
                if (_columnDisplayName == value)
                {
                    return;
                }

                _columnDisplayName = value;
                OnPropertyChanged();
            }
        }

        public string DataType
        {
            get => _dataType;
            set
            {
                if (_dataType == value)
                {
                    return;
                }

                _dataType = value;
                OnPropertyChanged();
            }
        }

        public int? DataSize
        {
            get => _dataSize;
            set
            {
                if (_dataSize == value)
                {
                    return;
                }

                _dataSize = value;
                OnPropertyChanged();
            }
        }

        public bool IsPrimaryKey
        {
            get => _isPrimaryKey;
            set
            {
                if (_isPrimaryKey == value)
                {
                    return;
                }

                _isPrimaryKey = value;
                OnPropertyChanged();
            }
        }

        public bool IsNotNull
        {
            get => _isNotNull;
            set
            {
                if (_isNotNull == value)
                {
                    return;
                }

                _isNotNull = value;
                OnPropertyChanged();
            }
        }

        public bool IsForeignKey
        {
            get => _isForeignKey;
            set
            {
                if (_isForeignKey == value)
                {
                    return;
                }

                _isForeignKey = value;
                if (!_isForeignKey)
                {
                    ForeignKeyReferenceId = string.Empty;
                    ReferenceTableName = string.Empty;
                    ReferenceColumnName = string.Empty;
                    AutoForeignKeyColumnName = string.Empty;
                }

                OnPropertyChanged();
            }
        }

        public string ForeignKeyReferenceId
        {
            get => _foreignKeyReferenceId;
            set
            {
                if (_foreignKeyReferenceId == value)
                {
                    return;
                }

                _foreignKeyReferenceId = value;
                OnPropertyChanged();
            }
        }

        public string ReferenceTableName
        {
            get => _referenceTableName;
            set
            {
                if (_referenceTableName == value)
                {
                    return;
                }

                _referenceTableName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ForeignKeyDisplayName));
            }
        }

        public string ReferenceColumnName
        {
            get => _referenceColumnName;
            set
            {
                if (_referenceColumnName == value)
                {
                    return;
                }

                _referenceColumnName = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ForeignKeyDisplayName));
            }
        }

        [JsonIgnore]
        public string ForeignKeyDisplayName =>
            string.IsNullOrWhiteSpace(ReferenceTableName) || string.IsNullOrWhiteSpace(ReferenceColumnName)
                ? string.Empty
                : $"{ReferenceTableName}.{ReferenceColumnName}";

        public string AutoForeignKeyColumnName
        {
            get => _autoForeignKeyColumnName;
            set
            {
                if (_autoForeignKeyColumnName == value)
                {
                    return;
                }

                _autoForeignKeyColumnName = value;
                OnPropertyChanged();
            }
        }

        public string DefaultValue { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
