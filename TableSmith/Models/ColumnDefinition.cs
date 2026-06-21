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
                NormalizeTypeDependentValues();
                NotifyInputAvailabilityChanged();
            }
        }

        public int? DataSize
        {
            get => _dataSize;
            set
            {
                var normalizedValue = IsDataSizeEnabled ? value : null;
                if (_dataSize == normalizedValue)
                {
                    return;
                }

                _dataSize = normalizedValue;
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
                var normalizedValue = IsPrecisionEnabled ? value : null;
                if (_precision == normalizedValue) return;
                _precision = normalizedValue;
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
                var normalizedValue = IsScaleEnabled ? value : null;
                if (_scale == normalizedValue) return;
                _scale = normalizedValue;
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
                var normalizedValue = IsIdentityEnabled && value;
                if (_isIdentity == normalizedValue) return;
                _isIdentity = normalizedValue;
                if (!_isIdentity)
                {
                    IdentitySeed = null;
                    IdentityIncrement = null;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsIdentityValueEnabled));
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
                var normalizedValue = IsIdentityValueEnabled ? value : null;
                if (_identitySeed == normalizedValue) return;
                _identitySeed = normalizedValue;
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
                var normalizedValue = IsIdentityValueEnabled ? value : null;
                if (_identityIncrement == normalizedValue) return;
                _identityIncrement = normalizedValue;
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
                if (!_isProtected)
                {
                    ProtectionType = string.Empty;
                }
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsProtectionTypeEnabled));
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

        /// <summary>
        /// 現在の型でサイズを入力できるかどうかを表します。
        /// </summary>
        [JsonIgnore]
        public bool IsDataSizeEnabled => IsStringType(DataType);

        /// <summary>
        /// 現在の型で精度を入力できるかどうかを表します。
        /// </summary>
        [JsonIgnore]
        public bool IsPrecisionEnabled => IsDecimalType(DataType);

        /// <summary>
        /// 現在の型で小数桁数を入力できるかどうかを表します。
        /// </summary>
        [JsonIgnore]
        public bool IsScaleEnabled => IsDecimalType(DataType);

        /// <summary>
        /// 現在の型で自動採番を設定できるかどうかを表します。
        /// </summary>
        [JsonIgnore]
        public bool IsIdentityEnabled => IsNumericType(DataType);

        /// <summary>
        /// 自動採番の開始値・増分値を入力できるかどうかを表します。
        /// </summary>
        [JsonIgnore]
        public bool IsIdentityValueEnabled => IsIdentityEnabled && IsIdentity;

        /// <summary>
        /// 保護方式を入力できるかどうかを表します。
        /// </summary>
        [JsonIgnore]
        public bool IsProtectionTypeEnabled => IsProtected;

        /// <summary>
        /// データ型変更後に使用しない設定値をクリアします。
        /// </summary>
        private void NormalizeTypeDependentValues()
        {
            if (!IsDataSizeEnabled)
            {
                DataSize = null;
            }

            if (!IsPrecisionEnabled)
            {
                Precision = null;
                Scale = null;
            }

            if (!IsIdentityEnabled)
            {
                IsIdentity = false;
            }
        }

        /// <summary>
        /// DataGridの入力可否に関係する算出プロパティへ変更を通知します。
        /// </summary>
        private void NotifyInputAvailabilityChanged()
        {
            OnPropertyChanged(nameof(IsDataSizeEnabled));
            OnPropertyChanged(nameof(IsPrecisionEnabled));
            OnPropertyChanged(nameof(IsScaleEnabled));
            OnPropertyChanged(nameof(IsIdentityEnabled));
            OnPropertyChanged(nameof(IsIdentityValueEnabled));
        }

        private static bool IsStringType(string dataType)
        {
            return dataType.Equals("char", StringComparison.OrdinalIgnoreCase)
                || dataType.Equals("nchar", StringComparison.OrdinalIgnoreCase)
                || dataType.Equals("varchar", StringComparison.OrdinalIgnoreCase)
                || dataType.Equals("nvarchar", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsDecimalType(string dataType)
        {
            return dataType.Equals("decimal", StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsNumericType(string dataType)
        {
            return dataType.Equals("bigint", StringComparison.OrdinalIgnoreCase)
                || dataType.Equals("int", StringComparison.OrdinalIgnoreCase)
                || dataType.Equals("decimal", StringComparison.OrdinalIgnoreCase);
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
