using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace TableSmith.Models
{
    /// <summary>
    /// インデックスに含めるカラム定義です。
    /// </summary>
    public class IndexColumnDefinition : INotifyPropertyChanged
    {
        private string _columnName = string.Empty;
        private bool _isDescending;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 対象カラム名です。
        /// </summary>
        public string ColumnName
        {
            get => _columnName;
            set => SetField(ref _columnName, value);
        }

        /// <summary>
        /// 降順指定かどうかを表します。falseの場合は昇順として扱います。
        /// </summary>
        public bool IsDescending
        {
            get => _isDescending;
            set
            {
                if (_isDescending == value) return;
                _isDescending = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(SortDirection));
            }
        }

        /// <summary>
        /// 画面表示用の昇順・降順表記です。
        /// </summary>
        [JsonIgnore]
        public string SortDirection => IsDescending ? "DESC" : "ASC";

        private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
