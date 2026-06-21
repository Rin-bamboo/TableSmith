namespace TableSmith.Services
{
    /// <summary>
    /// インデックス名の標準命名規則を提供します。
    /// </summary>
    public static class IndexNamingService
    {
        /// <summary>
        /// IX_テーブル名_カラム1_カラム2 の形式で標準インデックス名を生成します。
        /// </summary>
        public static string CreateStandardName(
            string tableName,
            IEnumerable<string> columnNames)
        {
            var names = columnNames
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(name => name.Trim())
                .ToList();

            return names.Count == 0
                ? $"IX_{tableName}"
                : $"IX_{tableName}_{string.Join("_", names)}";
        }
    }
}
