using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

namespace TableSmith.Models
{
    /// <summary>
    /// プロジェクト全体に適用するDB基本設定です。
    /// </summary>
    public class DatabaseSettings : INotifyPropertyChanged
    {
        private SqlDialect _defaultDialect = SqlDialect.SqlServer;
        private string _defaultSchemaName = "dbo";
        private string _defaultCharacterSet = string.Empty;
        private string _defaultCollation = string.Empty;
        private SqlFileEncoding _sqlFileEncoding = SqlFileEncoding.Utf8Bom;
        private ObservableCollection<SchemaDefinition> _schemas =
            new()
            {
                new SchemaDefinition
                {
                    SchemaName = "dbo",
                    Description = "SQL Serverの標準スキーマです。"
                }
            };

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// CREATE文の出力対象となるRDB種別です。
        /// </summary>
        public SqlDialect DefaultDialect
        {
            get => _defaultDialect;
            set => SetField(ref _defaultDialect, value);
        }

        /// <summary>
        /// テーブル作成時に既定で使用するスキーマ名です。
        /// </summary>
        public string DefaultSchemaName
        {
            get => _defaultSchemaName;
            set => SetField(ref _defaultSchemaName, value);
        }

        /// <summary>
        /// プロジェクト内でテーブルへ割り当て可能なスキーマ一覧です。
        /// </summary>
        public ObservableCollection<SchemaDefinition> Schemas
        {
            get => _schemas;
            set => SetField(ref _schemas, value);
        }

        /// <summary>
        /// 指定RDBで使用する既定の文字コードです。
        /// </summary>
        public string DefaultCharacterSet
        {
            get => _defaultCharacterSet;
            set => SetField(ref _defaultCharacterSet, value);
        }

        /// <summary>
        /// 既定の照合順序です。
        /// </summary>
        public string DefaultCollation
        {
            get => _defaultCollation;
            set => SetField(ref _defaultCollation, value);
        }

        /// <summary>
        /// SQLファイル出力時の文字コードです。
        /// </summary>
        public SqlFileEncoding SqlFileEncoding
        {
            get => _sqlFileEncoding;
            set => SetField(ref _sqlFileEncoding, value);
        }

        /// <summary>
        /// 値を更新し、画面へ変更を通知します。
        /// </summary>
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
