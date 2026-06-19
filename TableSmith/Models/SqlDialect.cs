namespace TableSmith.Models
{
    /// <summary>
    /// CREATE TABLE文の出力対象となるRDBを表します。
    /// </summary>
    public enum SqlDialect
    {
        SqlServer,
        MySql,
        Oracle
    }

    /// <summary>
    /// CREATE文出力画面に表示するRDB選択肢です。
    /// </summary>
    public class SqlDialectOption
    {
        public SqlDialect Dialect { get; init; }
        public string DisplayName { get; init; } = string.Empty;
    }
}
