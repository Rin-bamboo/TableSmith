namespace TableSmith.Models
{
    /// <summary>
    /// インデックスに含めるカラム定義です。
    /// </summary>
    public class IndexColumnDefinition
    {
        /// <summary>
        /// 対象カラム名です。
        /// </summary>
        public string ColumnName { get; set; } = string.Empty;

        /// <summary>
        /// 降順指定かどうかを表します。falseの場合は昇順として扱います。
        /// </summary>
        public bool IsDescending { get; set; }
    }
}
