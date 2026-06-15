using System.Collections.ObjectModel;

namespace TableSmith.Models
{
    public class TableDefinition
    {
        public string TableName { get; set; } = string.Empty;
        public string TableDisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ObservableCollection<ColumnDefinition> Columns { get; set; } = new();
    }
}
