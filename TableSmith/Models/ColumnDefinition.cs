using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace TableSmith.Models
{
    /// <summary>
    /// テーブルに含まれる1カラム分の設計情報を表します。
    /// </summary>
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
        private int? _precision;
        private int? _scale;
        private bool _isIdentity;
        private int? _identitySeed;
        private int? _identityIncrement;
        private bool _isProtected;
        private string _protectionType = string.Empty;

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

        /// <summary>
        /// decimal型などで使用する全体桁数です。
        /// </summary>
        public int? Precision
        {
            get => _precision;
            set
            {
                if (_precision == value) return;
                _precision = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// decimal型などで使用する小数桁数です。
        /// </summary>
        public int? Scale
        {
            get => _scale;
            set
            {
                if (_scale == value) return;
                _scale = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// RDBの自動採番機能を使用するカラムかどうかを表します。
        /// </summary>
        public bool IsIdentity
        {
            get => _isIdentity;
            set
            {
                if (_isIdentity == value) return;
                _isIdentity = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 自動採番の開始値です。
        /// </summary>
        public int? IdentitySeed
        {
            get => _identitySeed;
            set
            {
                if (_identitySeed == value) return;
                _identitySeed = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 自動採番の増分値です。
        /// </summary>
        public int? IdentityIncrement
        {
            get => _identityIncrement;
            set
            {
                if (_identityIncrement == value) return;
                _identityIncrement = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 暗号化・マスキングなどの保護対象かどうかを表します。
        /// </summary>
        public bool IsProtected
        {
            get => _isProtected;
            set
            {
                if (_isProtected == value) return;
                _isProtected = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 保護方式の設計メモです。v0.3.0では実処理を行いません。
        /// </summary>
        public string ProtectionType
        {
            get => _protectionType;
            set
            {
                if (_protectionType == value) return;
                _protectionType = value;
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
