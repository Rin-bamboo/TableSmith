using System.Collections.ObjectModel;

namespace TableSmith.Models
{
    public class TableSmithProject
    {
        public int Version { get; set; } = 1;
        public string ProjectName { get; set; } = string.Empty;
        public ObservableCollection<TableDefinition> Tables { get; set; } = new();
    }
}
