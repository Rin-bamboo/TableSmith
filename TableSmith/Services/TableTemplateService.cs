using System.Collections.ObjectModel;
using TableSmith.Models;

namespace TableSmith.Services
{
    /// <summary>
    /// 新規テーブルへ自動投入する標準カラムテンプレートを提供します。
    /// </summary>
    public static class TableTemplateService
    {
        /// <summary>
        /// idと監査情報を含む標準カラム一覧を作成します。
        /// </summary>
        public static ObservableCollection<ColumnDefinition> CreateDefaultColumns()
        {
            return new ObservableCollection<ColumnDefinition>
            {
                new()
                {
                    No = 1,
                    ColumnName = "id",
                    ColumnDisplayName = "ID",
                    DataType = "bigint",
                    IsPrimaryKey = true,
                    IsNotNull = true,
                    Description = "レコードを一意に識別する主キーです。"
                },
                new()
                {
                    No = 2,
                    ColumnName = "created_at",
                    ColumnDisplayName = "作成日時",
                    DataType = "datetime",
                    IsNotNull = true,
                    DefaultValue = "CURRENT_TIMESTAMP",
                    Description = "レコードを作成した日時です。"
                },
                new()
                {
                    No = 3,
                    ColumnName = "created_by",
                    ColumnDisplayName = "作成者",
                    DataType = "nvarchar",
                    DataSize = 100,
                    IsNotNull = true,
                    Description = "レコードを作成したユーザーを識別する値です。"
                },
                new()
                {
                    No = 4,
                    ColumnName = "updated_at",
                    ColumnDisplayName = "更新日時",
                    DataType = "datetime",
                    IsNotNull = true,
                    DefaultValue = "CURRENT_TIMESTAMP",
                    Description = "レコードを最後に更新した日時です。"
                },
                new()
                {
                    No = 5,
                    ColumnName = "updated_by",
                    ColumnDisplayName = "更新者",
                    DataType = "nvarchar",
                    DataSize = 100,
                    IsNotNull = true,
                    Description = "レコードを最後に更新したユーザーを識別する値です。"
                }
            };
        }
    }
}
