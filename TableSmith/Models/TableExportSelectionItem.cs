using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TableSmith.Models
{
    /// <summary>
    /// 定義書へ出力するテーブルの選択状態を保持します。
    /// </summary>
    public class TableExportSelectionItem : INotifyPropertyChanged
    {
        private bool _isSelected;

        public event PropertyChangedEventHandler? PropertyChanged;

        public TableDefinition Table { get; }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                _isSelected = value;
                OnPropertyChanged();
            }
        }

        public TableExportSelectionItem(TableDefinition table, bool isSelected = true)
        {
            this.Table = table;
            this._isSelected = isSelected;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
