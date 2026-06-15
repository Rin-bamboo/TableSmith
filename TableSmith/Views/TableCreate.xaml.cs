using System.Windows;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using MahApps.Metro.Controls;
using TableSmith.Models;

namespace TableSmith.Views
{
    /// <summary>
    /// テーブル定義作成画面です。
    /// </summary>
    public partial class TableCreate : MetroWindow
    {
        private static readonly Regex SqlNameRegex = new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

        public TableDefinition CurrentTable { get; set; } = new();
        public ColumnDefinition? SelectedColumn { get; set; }
        public ObservableCollection<TableDefinition> ExistingTables { get; }
        public ObservableCollection<ForeignKeyReference> ForeignKeyReferences { get; } = new();

        public TableCreate()
            : this(new ObservableCollection<TableDefinition>())
        {
        }

        public TableCreate(ObservableCollection<TableDefinition> existingTables)
        {
            this.ExistingTables = existingTables;
            LoadForeignKeyReferences();
            InitializeComponent();
            this.DataContext = this;
            AddColumn();
        }

        private void NewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentTable = new TableDefinition();
            LoadForeignKeyReferences();
            AddColumn();
            this.DataContext = null;
            this.DataContext = this;
        }

        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AddColumnButton_Click(object sender, RoutedEventArgs e)
        {
            AddColumn();
        }

        private void RemoveColumnButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.SelectedColumn == null)
            {
                MessageBox.Show("削除するカラムを選択してください。", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            this.CurrentTable.Columns.Remove(this.SelectedColumn);
            RenumberColumns();
        }

        private void ForeignKeyReferenceComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (sender is not System.Windows.Controls.ComboBox { DataContext: ColumnDefinition column })
            {
                return;
            }

            ApplyForeignKeyReference(column);
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            this.ColumnDataGrid.CommitEdit();
            this.ColumnDataGrid.CommitEdit(System.Windows.Controls.DataGridEditingUnit.Row, true);

            if (!ValidateInputs())
            {
                return;
            }

            this.DialogResult = true;
            this.Close();
        }

        private void AddColumn()
        {
            var nextNo = this.CurrentTable.Columns.Count + 1;
            var column = new ColumnDefinition
            {
                No = nextNo,
                DataType = "nvarchar",
                IsNotNull = true
            };

            this.CurrentTable.Columns.Add(column);
            this.ColumnDataGrid.SelectedItem = column;
            this.ColumnDataGrid.ScrollIntoView(column);
        }

        private void RenumberColumns()
        {
            for (var index = 0; index < this.CurrentTable.Columns.Count; index++)
            {
                this.CurrentTable.Columns[index].No = index + 1;
            }

        }

        private bool ValidateInputs()
        {
            var errors = new StringBuilder();

            ValidateTable(errors);
            ValidateColumns(errors);

            if (errors.Length == 0)
            {
                return true;
            }

            MessageBox.Show(errors.ToString(), "入力確認", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        private void ValidateTable(StringBuilder errors)
        {
            this.CurrentTable.TableName = this.CurrentTable.TableName.Trim();
            this.CurrentTable.TableDisplayName = this.CurrentTable.TableDisplayName.Trim();
            this.CurrentTable.Description = this.CurrentTable.Description.Trim();

            if (string.IsNullOrWhiteSpace(this.CurrentTable.TableName))
            {
                errors.AppendLine("・テーブル物理名を入力してください。");
            }
            else if (!SqlNameRegex.IsMatch(this.CurrentTable.TableName))
            {
                errors.AppendLine("・テーブル物理名は英字またはアンダースコアで始め、英数字とアンダースコアで入力してください。");
            }
            else if (this.ExistingTables.Any(table => table.TableName.Equals(this.CurrentTable.TableName, StringComparison.OrdinalIgnoreCase)))
            {
                errors.AppendLine($"・テーブル物理名 '{this.CurrentTable.TableName}' は既に作成されています。");
            }

            if (string.IsNullOrWhiteSpace(this.CurrentTable.TableDisplayName))
            {
                errors.AppendLine("・テーブル論理名を入力してください。");
            }
        }

        private void ValidateColumns(StringBuilder errors)
        {
            var columnNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (this.CurrentTable.Columns.Count == 0)
            {
                errors.AppendLine("・カラム情報を1件以上入力してください。");
                return;
            }

            foreach (var column in this.CurrentTable.Columns)
            {
                var rowName = column.No > 0 ? $"{column.No}行目" : "カラム情報";

                column.ColumnName = column.ColumnName.Trim();
                column.ColumnDisplayName = column.ColumnDisplayName.Trim();
                column.DataType = column.DataType.Trim();
                column.DefaultValue = column.DefaultValue.Trim();
                column.Description = column.Description.Trim();

                if (string.IsNullOrWhiteSpace(column.ColumnName))
                {
                    errors.AppendLine($"・{rowName}: カラム物理名を入力してください。");
                }
                else
                {
                    if (!SqlNameRegex.IsMatch(column.ColumnName))
                    {
                        errors.AppendLine($"・{rowName}: カラム物理名は英字またはアンダースコアで始め、英数字とアンダースコアで入力してください。");
                    }

                    if (!columnNames.Add(column.ColumnName))
                    {
                        errors.AppendLine($"・{rowName}: カラム物理名 '{column.ColumnName}' が重複しています。");
                    }
                }

                if (string.IsNullOrWhiteSpace(column.ColumnDisplayName))
                {
                    errors.AppendLine($"・{rowName}: カラム論理名を入力してください。");
                }

                if (string.IsNullOrWhiteSpace(column.DataType))
                {
                    errors.AppendLine($"・{rowName}: 型を選択してください。");
                }

                if (column.DataSize.HasValue && column.DataSize.Value <= 0)
                {
                    errors.AppendLine($"・{rowName}: サイズは1以上で入力してください。");
                }

                if (RequiresSize(column.DataType) && !column.DataSize.HasValue)
                {
                    errors.AppendLine($"・{rowName}: 型 '{column.DataType}' にはサイズを入力してください。");
                }

                if (column.IsPrimaryKey && !column.IsNotNull)
                {
                    errors.AppendLine($"・{rowName}: PK のカラムは Not Null をチェックしてください。");
                }

                if (column.IsForeignKey)
                {
                    ValidateForeignKey(column, rowName, errors);
                }
                else
                {
                    ClearForeignKeyReference(column);
                }
            }
        }

        private void ValidateForeignKey(ColumnDefinition column, string rowName, StringBuilder errors)
        {
            if (this.ForeignKeyReferences.Count == 0)
            {
                errors.AppendLine($"・{rowName}: 参照できるPKが他のテーブルにありません。先にPKを持つテーブルを作成してください。");
                return;
            }

            if (string.IsNullOrWhiteSpace(column.ForeignKeyReferenceId))
            {
                errors.AppendLine($"・{rowName}: 外部キーの参照PKを選択してください。");
                return;
            }

            var reference = this.ForeignKeyReferences.FirstOrDefault(item => item.ReferenceId == column.ForeignKeyReferenceId);
            if (reference == null)
            {
                errors.AppendLine($"・{rowName}: 外部キーの参照PKが存在しません。");
                return;
            }

            ApplyForeignKeyReference(column);

            column.ReferenceTableName = reference.TableName;
            column.ReferenceColumnName = reference.ColumnName;

            var referencedColumn = this.ExistingTables
                .FirstOrDefault(table => table.TableName.Equals(reference.TableName, StringComparison.OrdinalIgnoreCase))
                ?.Columns
                .FirstOrDefault(item => item.ColumnName.Equals(reference.ColumnName, StringComparison.OrdinalIgnoreCase) && item.IsPrimaryKey);

            if (referencedColumn == null)
            {
                errors.AppendLine($"・{rowName}: 外部キーの参照PKが存在しません。");
                return;
            }

            if (!column.DataType.Equals(referencedColumn.DataType, StringComparison.OrdinalIgnoreCase))
            {
                errors.AppendLine($"・{rowName}: 外部キーの型は参照PK '{reference.ReferenceId}' と同じ '{referencedColumn.DataType}' にしてください。");
            }

            if (column.DataSize != referencedColumn.DataSize)
            {
                var size = referencedColumn.DataSize.HasValue ? referencedColumn.DataSize.Value.ToString() : "未指定";
                errors.AppendLine($"・{rowName}: 外部キーのサイズは参照PK '{reference.ReferenceId}' と同じ '{size}' にしてください。");
            }
        }

        private void LoadForeignKeyReferences()
        {
            this.ForeignKeyReferences.Clear();

            foreach (var table in this.ExistingTables)
            {
                foreach (var column in table.Columns.Where(column => column.IsPrimaryKey))
                {
                    if (string.IsNullOrWhiteSpace(table.TableName) || string.IsNullOrWhiteSpace(column.ColumnName))
                    {
                        continue;
                    }

                    this.ForeignKeyReferences.Add(new ForeignKeyReference
                    {
                        ReferenceId = $"{table.TableName}.{column.ColumnName}",
                        TableName = table.TableName,
                        TableDisplayName = table.TableDisplayName,
                        ColumnName = column.ColumnName,
                        ColumnDisplayName = column.ColumnDisplayName
                    });
                }
            }
        }

        private static void ClearForeignKeyReference(ColumnDefinition column)
        {
            column.ForeignKeyReferenceId = string.Empty;
            column.ReferenceTableName = string.Empty;
            column.ReferenceColumnName = string.Empty;
            column.AutoForeignKeyColumnName = string.Empty;
        }

        private void ApplyForeignKeyReference(ColumnDefinition column)
        {
            if (!column.IsForeignKey || string.IsNullOrWhiteSpace(column.ForeignKeyReferenceId))
            {
                return;
            }

            var reference = this.ForeignKeyReferences.FirstOrDefault(item => item.ReferenceId == column.ForeignKeyReferenceId);
            if (reference == null)
            {
                return;
            }

            var referencedColumn = this.ExistingTables
                .FirstOrDefault(table => table.TableName.Equals(reference.TableName, StringComparison.OrdinalIgnoreCase))
                ?.Columns
                .FirstOrDefault(item => item.ColumnName.Equals(reference.ColumnName, StringComparison.OrdinalIgnoreCase) && item.IsPrimaryKey);

            if (referencedColumn == null)
            {
                return;
            }

            var defaultColumnName = $"{reference.TableName}_{reference.ColumnName}";
            if (string.IsNullOrWhiteSpace(column.ColumnName) || column.ColumnName == column.AutoForeignKeyColumnName)
            {
                column.ColumnName = defaultColumnName;
            }

            column.AutoForeignKeyColumnName = defaultColumnName;
            column.ColumnDisplayName = referencedColumn.ColumnDisplayName;
            column.DataType = referencedColumn.DataType;
            column.DataSize = referencedColumn.DataSize;
            column.IsNotNull = referencedColumn.IsNotNull;
            column.ReferenceTableName = reference.TableName;
            column.ReferenceColumnName = reference.ColumnName;

        }

        private static bool RequiresSize(string dataType)
        {
            return dataType.Equals("char", StringComparison.OrdinalIgnoreCase)
                || dataType.Equals("nchar", StringComparison.OrdinalIgnoreCase)
                || dataType.Equals("varchar", StringComparison.OrdinalIgnoreCase)
                || dataType.Equals("nvarchar", StringComparison.OrdinalIgnoreCase)
                || dataType.Equals("decimal", StringComparison.OrdinalIgnoreCase);
        }
    }
}
