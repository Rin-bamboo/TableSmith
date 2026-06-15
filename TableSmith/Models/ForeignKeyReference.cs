namespace TableSmith.Models
{
    public class ForeignKeyReference
    {
        public string ReferenceId { get; init; } = string.Empty;
        public string TableName { get; init; } = string.Empty;
        public string TableDisplayName { get; init; } = string.Empty;
        public string ColumnName { get; init; } = string.Empty;
        public string ColumnDisplayName { get; init; } = string.Empty;

        public string DisplayName
        {
            get
            {
                var tableLabel = string.IsNullOrWhiteSpace(TableDisplayName) ? TableName : $"{TableDisplayName} ({TableName})";
                var columnLabel = string.IsNullOrWhiteSpace(ColumnDisplayName) ? ColumnName : $"{ColumnDisplayName} ({ColumnName})";
                return $"{tableLabel} - {columnLabel}";
            }
        }
    }
}
