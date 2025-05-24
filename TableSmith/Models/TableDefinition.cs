using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace TableSmith.Models
{
    public class TableDefinition // Changed from 'internal' to 'public'
    {
        public required string TableName { get; set; }
        public required string TableDisplayName { get; set; }
        public required List<ColumnDefinition> Columns { get; set; }
    }

}
