using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TableSmith.Models
{
    /// <summary>
    /// プロジェクト内で利用できるデータベーススキーマの定義です。
    /// </summary>
    public class SchemaDefinition : INotifyPropertyChanged
    {
        private string _schemaName = string.Empty;
        private string _description = string.Empty;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// スキーマの物理名です。
        /// </summary>
        public string SchemaName
        {
            get => _schemaName;
            set
            {
                if (_schemaName == value) return;
                _schemaName = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// スキーマの用途や補足説明です。
        /// </summary>
        public string Description
        {
            get => _description;
            set
            {
                if (_description == value) return;
                _description = value;
                OnPropertyChanged();
            }
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
