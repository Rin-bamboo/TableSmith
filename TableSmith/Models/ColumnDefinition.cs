using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TableSmith.Models
{
    public class ColumnDefinition
    {
        public required string ColumnName { get; set; }         // 物理名
        public required string ColumnDisplayName { get; set; }  // 論理名
        public required string DataType { get; set; }
        public int? DataSize { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsNotNull { get; set; }
        public required string Description { get; set; }
    }
}
