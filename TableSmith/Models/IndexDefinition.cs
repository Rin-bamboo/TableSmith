using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TableSmith.Models
{
    /// <summary>
    /// テーブルに設定するインデックス定義です。
    /// </summary>
    public class IndexDefinition : INotifyPropertyChanged
    {
        private string _indexName = string.Empty;
        private bool _isUnique;
        private bool _isClustered;
        private bool _isNameCustomized = true;
        private string _description = string.Empty;
        private ObservableCollection<IndexColumnDefinition> _columns = new();

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// インデックス名です。
        /// </summary>
        public string IndexName
        {
            get => _indexName;
            set => SetField(ref _indexName, value);
        }

        /// <summary>
        /// 標準命名ではなく任意のインデックス名を使用するかどうかを表します。
        /// 旧JSONの既存名を維持するため、既定値はtrueです。
        /// </summary>
        public bool IsNameCustomized
        {
            get => _isNameCustomized;
            set => SetField(ref _isNameCustomized, value);
        }

        /// <summary>
        /// 一意インデックスかどうかを表します。
        /// </summary>
        public bool IsUnique
        {
            get => _isUnique;
            set => SetField(ref _isUnique, value);
        }

        /// <summary>
        /// クラスタ化インデックスかどうかを表します。SQL Server向けの設定です。
        /// </summary>
        public bool IsClustered
        {
            get => _isClustered;
            set => SetField(ref _isClustered, value);
        }

        /// <summary>
        /// インデックス対象カラム一覧です。
        /// </summary>
        public ObservableCollection<IndexColumnDefinition> Columns
        {
            get => _columns;
            set => SetField(ref _columns, value);
        }

        /// <summary>
        /// インデックスの説明です。
        /// </summary>
        public string Description
        {
            get => _description;
            set => SetField(ref _description, value);
        }

        private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return;
            }

            field = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
