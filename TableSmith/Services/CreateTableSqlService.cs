using System.Text;
using TableSmith.Models;

namespace TableSmith.Services
{
    /// <summary>
    /// テーブル定義からCREATE TABLE文を生成します。
    /// </summary>
    public class CreateTableSqlService
    {
        /// <summary>
        /// 指定されたテーブル定義のCREATE TABLE文を生成します。
        /// </summary>
        public string Generate(TableDefinition table)
        {
            ValidateTable(table);

            var lines = new List<string>();

            foreach (var column in table.Columns.OrderBy(column => column.No))
            {
                lines.Add($"    {BuildColumnDefinition(column)}");
            }

            var primaryKeyColumns = table.Columns
                .Where(column => column.IsPrimaryKey)
                .OrderBy(column => column.No)
                .Select(column => EscapeName(column.ColumnName))
                .ToList();

            if (primaryKeyColumns.Count > 0)
            {
                lines.Add($"    CONSTRAINT {EscapeName($"PK_{table.TableName}")} PRIMARY KEY ({string.Join(", ", primaryKeyColumns)})");
            }

            foreach (var foreignKey in table.Columns.Where(column => column.IsForeignKey).OrderBy(column => column.No))
            {
                lines.Add($"    {BuildForeignKeyConstraint(table, foreignKey)}");
            }

            var builder = new StringBuilder();
            builder.AppendLine($"CREATE TABLE {EscapeName(table.TableName)}");
            builder.AppendLine("(");
            builder.AppendLine(string.Join("," + Environment.NewLine, lines));
            builder.AppendLine(");");

            return builder.ToString();
        }

        /// <summary>
        /// 指定された全テーブルのCREATE TABLE文をまとめて生成します。
        /// </summary>
        public string GenerateAll(IEnumerable<TableDefinition> tables)
        {
            var tableList = tables.ToList();
            if (tableList.Count == 0)
            {
                throw new InvalidOperationException("CREATE文を作成するテーブルがありません。");
            }

            return string.Join(
                Environment.NewLine + "GO" + Environment.NewLine + Environment.NewLine,
                tableList.Select(Generate));
        }

        /// <summary>
        /// カラム定義行を生成します。
        /// </summary>
        private static string BuildColumnDefinition(ColumnDefinition column)
        {
            var builder = new StringBuilder();
            builder.Append(EscapeName(column.ColumnName));
            builder.Append(' ');
            builder.Append(BuildDataType(column));

            if (column.IsNotNull)
            {
                builder.Append(" NOT NULL");
            }
            else
            {
                builder.Append(" NULL");
            }

            if (!string.IsNullOrWhiteSpace(column.DefaultValue))
            {
                builder.Append(" DEFAULT ");
                builder.Append(column.DefaultValue);
            }

            return builder.ToString();
        }

        /// <summary>
        /// サイズ指定を含むデータ型表現を生成します。
        /// </summary>
        private static string BuildDataType(ColumnDefinition column)
        {
            if (column.DataSize.HasValue && UsesSize(column.DataType))
            {
                return $"{column.DataType}({column.DataSize.Value})";
            }

            return column.DataType;
        }

        /// <summary>
        /// 外部キー制約行を生成します。
        /// </summary>
        private static string BuildForeignKeyConstraint(TableDefinition table, ColumnDefinition column)
        {
            var constraintName = $"FK_{table.TableName}_{column.ReferenceTableName}_{column.ColumnName}";
            return $"CONSTRAINT {EscapeName(constraintName)} FOREIGN KEY ({EscapeName(column.ColumnName)}) REFERENCES {EscapeName(column.ReferenceTableName)} ({EscapeName(column.ReferenceColumnName)})";
        }

        /// <summary>
        /// SQL Server形式で識別子をエスケープします。
        /// </summary>
        private static string EscapeName(string name)
        {
            return $"[{name.Replace("]", "]]")}]";
        }

        /// <summary>
        /// CREATE TABLE文の生成に必要な値が揃っているか確認します。
        /// </summary>
        private static void ValidateTable(TableDefinition table)
        {
            if (string.IsNullOrWhiteSpace(table.TableName))
            {
                throw new InvalidOperationException("テーブル物理名が未設定のため、CREATE文を作成できません。");
            }

            if (table.Columns.Count == 0)
            {
                throw new InvalidOperationException("カラム情報が未設定のため、CREATE文を作成できません。");
            }

            foreach (var column in table.Columns)
            {
                if (string.IsNullOrWhiteSpace(column.ColumnName))
                {
                    throw new InvalidOperationException($"{column.No}行目のカラム物理名が未設定です。");
                }

                if (string.IsNullOrWhiteSpace(column.DataType))
                {
                    throw new InvalidOperationException($"{column.No}行目のデータ型が未設定です。");
                }

                if (column.IsForeignKey
                    && (string.IsNullOrWhiteSpace(column.ReferenceTableName)
                        || string.IsNullOrWhiteSpace(column.ReferenceColumnName)))
                {
                    throw new InvalidOperationException($"{column.No}行目の外部キー参照先が未設定です。テーブルを編集して参照PKを選択してください。");
                }
            }
        }

        /// <summary>
        /// サイズ指定をSQLに出力するデータ型かどうかを判定します。
        /// </summary>
        private static bool UsesSize(string dataType)
        {
            return dataType.Equals("char", StringComparison.OrdinalIgnoreCase)
                || dataType.Equals("nchar", StringComparison.OrdinalIgnoreCase)
                || dataType.Equals("varchar", StringComparison.OrdinalIgnoreCase)
                || dataType.Equals("nvarchar", StringComparison.OrdinalIgnoreCase)
                || dataType.Equals("decimal", StringComparison.OrdinalIgnoreCase);
        }
    }
}
