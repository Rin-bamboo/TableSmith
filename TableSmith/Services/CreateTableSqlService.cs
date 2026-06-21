using System.Text;
using TableSmith.Models;

namespace TableSmith.Services
{
    /// <summary>
    /// テーブル定義からRDBごとのCREATE TABLE文を生成します。
    /// </summary>
    public class CreateTableSqlService
    {
        /// <summary>
        /// 指定されたテーブル定義のCREATE TABLE文を生成します。
        /// </summary>
        public string Generate(
            TableDefinition table,
            SqlDialect dialect,
            DatabaseSettings? databaseSettings = null)
        {
            ValidateTable(table);
            var schemaName = ResolveSchemaName(table, databaseSettings);

            var lines = new List<string>();
            foreach (var column in table.Columns.OrderBy(column => column.No))
            {
                lines.Add($"    {BuildColumnDefinition(column, dialect)}");
            }

            var primaryKeyColumns = table.Columns
                .Where(column => column.IsPrimaryKey)
                .OrderBy(column => column.No)
                .Select(column => EscapeName(column.ColumnName, dialect))
                .ToList();

            if (primaryKeyColumns.Count > 0)
            {
                lines.Add(
                    $"    CONSTRAINT {EscapeName($"PK_{table.TableName}", dialect)} " +
                    $"PRIMARY KEY ({string.Join(", ", primaryKeyColumns)})");
            }

            foreach (var foreignKey in table.Columns.Where(column => column.IsForeignKey).OrderBy(column => column.No))
            {
                lines.Add($"    {BuildForeignKeyConstraint(table, foreignKey, dialect, schemaName)}");
            }

            var builder = new StringBuilder();
            builder.AppendLine($"CREATE TABLE {BuildQualifiedTableName(schemaName, table.TableName, dialect)}");
            builder.AppendLine("(");
            builder.AppendLine(string.Join("," + Environment.NewLine, lines));

            if (dialect == SqlDialect.MySql)
            {
                var tableComment = BuildCombinedComment(table.TableDisplayName, table.Description);
                builder.Append(')');
                if (!string.IsNullOrWhiteSpace(tableComment))
                {
                    builder.Append($" COMMENT = '{EscapeSqlLiteral(tableComment)}'");
                }
                AppendMySqlTableOptions(builder, table, databaseSettings);
                builder.AppendLine(";");
            }
            else
            {
                builder.AppendLine(");");
            }

            AppendIndexes(builder, table, dialect, schemaName);
            AppendObjectComments(builder, table, dialect, schemaName);
            return builder.ToString();
        }

        /// <summary>
        /// 指定された全テーブルのCREATE TABLE文をまとめて生成します。
        /// </summary>
        public string GenerateAll(
            IEnumerable<TableDefinition> tables,
            SqlDialect dialect,
            DatabaseSettings? databaseSettings = null)
        {
            var tableList = tables.ToList();
            if (tableList.Count == 0)
            {
                throw new InvalidOperationException("CREATE文を作成するテーブルがありません。");
            }

            var separator = dialect == SqlDialect.SqlServer
                ? Environment.NewLine + "GO" + Environment.NewLine + Environment.NewLine
                : Environment.NewLine + Environment.NewLine;

            return string.Join(
                separator,
                tableList.Select(table => Generate(table, dialect, databaseSettings)));
        }

        /// <summary>
        /// RDBごとの構文に合わせてカラム定義行を生成します。
        /// </summary>
        private static string BuildColumnDefinition(ColumnDefinition column, SqlDialect dialect)
        {
            var builder = new StringBuilder();
            builder.Append(EscapeName(column.ColumnName, dialect));
            builder.Append(' ');
            builder.Append(BuildDataType(column, dialect));
            AppendIdentityClause(builder, column, dialect);

            // OracleはDEFAULT句をインライン制約より前に配置します。
            if (dialect == SqlDialect.Oracle && !string.IsNullOrWhiteSpace(column.DefaultValue))
            {
                builder.Append(" DEFAULT ");
                builder.Append(NormalizeDefaultValue(column.DefaultValue, dialect));
            }

            builder.Append(column.IsNotNull ? " NOT NULL" : " NULL");

            if (dialect != SqlDialect.Oracle && !string.IsNullOrWhiteSpace(column.DefaultValue))
            {
                builder.Append(" DEFAULT ");
                builder.Append(NormalizeDefaultValue(column.DefaultValue, dialect));
            }

            if (dialect == SqlDialect.MySql)
            {
                var comment = BuildCombinedComment(column.ColumnDisplayName, column.Description);
                if (!string.IsNullOrWhiteSpace(comment))
                {
                    builder.Append($" COMMENT '{EscapeSqlLiteral(comment)}'");
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// RDB固有のテーブル・カラムコメント登録文を追加します。
        /// </summary>
        private static void AppendObjectComments(
            StringBuilder builder,
            TableDefinition table,
            SqlDialect dialect,
            string schemaName)
        {
            if (dialect == SqlDialect.MySql)
            {
                return;
            }

            builder.AppendLine();
            if (dialect == SqlDialect.SqlServer)
            {
                AppendSqlServerProperty(builder, schemaName, table.TableName, null, "LogicalName", table.TableDisplayName);
                AppendSqlServerProperty(builder, schemaName, table.TableName, null, "MS_Description", table.Description);

                foreach (var column in table.Columns.OrderBy(column => column.No))
                {
                    AppendSqlServerProperty(
                        builder,
                        schemaName,
                        table.TableName,
                        column.ColumnName,
                        "LogicalName",
                        column.ColumnDisplayName);
                    AppendSqlServerProperty(
                        builder,
                        schemaName,
                        table.TableName,
                        column.ColumnName,
                        "MS_Description",
                        column.Description);
                }
                return;
            }

            var tableComment = BuildCombinedComment(table.TableDisplayName, table.Description);
            if (!string.IsNullOrWhiteSpace(tableComment))
            {
                builder.AppendLine(
                    $"COMMENT ON TABLE {BuildQualifiedTableName(schemaName, table.TableName, dialect)} " +
                    $"IS '{EscapeSqlLiteral(tableComment)}';");
            }

            foreach (var column in table.Columns.OrderBy(column => column.No))
            {
                var columnComment = BuildCombinedComment(column.ColumnDisplayName, column.Description);
                if (string.IsNullOrWhiteSpace(columnComment))
                {
                    continue;
                }

                builder.AppendLine(
                    $"COMMENT ON COLUMN {BuildQualifiedTableName(schemaName, table.TableName, dialect)}." +
                    $"{EscapeName(column.ColumnName, dialect)} " +
                    $"IS '{EscapeSqlLiteral(columnComment)}';");
            }
        }

        /// <summary>
        /// SQL Serverの拡張プロパティ登録文を追加します。
        /// </summary>
        private static void AppendSqlServerProperty(
            StringBuilder builder,
            string schemaName,
            string tableName,
            string? columnName,
            string propertyName,
            string propertyValue)
        {
            if (string.IsNullOrWhiteSpace(propertyValue))
            {
                return;
            }

            builder.Append("EXEC sys.sp_addextendedproperty ");
            builder.Append($"@name = N'{EscapeSqlLiteral(propertyName)}', ");
            builder.Append($"@value = N'{EscapeSqlLiteral(propertyValue)}', ");
            builder.Append(
                $"@level0type = N'SCHEMA', @level0name = N'{EscapeSqlLiteral(DefaultSchema(schemaName))}', ");
            builder.Append($"@level1type = N'TABLE', @level1name = N'{EscapeSqlLiteral(tableName)}'");

            if (!string.IsNullOrWhiteSpace(columnName))
            {
                builder.Append(
                    $", @level2type = N'COLUMN', @level2name = N'{EscapeSqlLiteral(columnName)}'");
            }

            builder.AppendLine(";");
        }

        /// <summary>
        /// 論理名と説明を、単一コメントしか持てないRDB向けに結合します。
        /// </summary>
        private static string BuildCombinedComment(string logicalName, string description)
        {
            if (string.IsNullOrWhiteSpace(logicalName))
            {
                return description.Trim();
            }

            if (string.IsNullOrWhiteSpace(description))
            {
                return logicalName.Trim();
            }

            return $"{logicalName.Trim()} - {description.Trim()}";
        }

        /// <summary>
        /// SQL文字列リテラル内のシングルクォートをエスケープします。
        /// </summary>
        private static string EscapeSqlLiteral(string value)
        {
            return value.Replace("'", "''");
        }

        /// <summary>
        /// アプリ内の共通データ型を各RDBのデータ型へ変換します。
        /// </summary>
        private static string BuildDataType(ColumnDefinition column, SqlDialect dialect)
        {
            var sourceType = column.DataType.Trim().ToLowerInvariant();
            var mappedType = dialect switch
            {
                SqlDialect.SqlServer => MapSqlServerType(sourceType),
                SqlDialect.MySql => MapMySqlType(sourceType),
                SqlDialect.Oracle => MapOracleType(sourceType),
                _ => column.DataType
            };

            // decimalは専用の精度・小数桁数を優先し、旧JSONのDataSizeも後方互換として残します。
            if (sourceType == "decimal" && column.Precision.HasValue)
            {
                return column.Scale.HasValue
                    ? $"{mappedType}({column.Precision.Value},{column.Scale.Value})"
                    : $"{mappedType}({column.Precision.Value})";
            }

            if (column.DataSize.HasValue && UsesSize(sourceType))
            {
                return $"{mappedType}({column.DataSize.Value})";
            }

            return mappedType;
        }

        private static string MapSqlServerType(string dataType)
        {
            return dataType switch
            {
                "bigint" => "BIGINT",
                "bit" => "BIT",
                "date" => "DATE",
                "datetime" => "DATETIME",
                "decimal" => "DECIMAL",
                "int" => "INT",
                "nchar" => "NCHAR",
                "nvarchar" => "NVARCHAR",
                "uniqueidentifier" => "UNIQUEIDENTIFIER",
                "varchar" => "VARCHAR",
                _ => dataType.ToUpperInvariant()
            };
        }

        private static string MapMySqlType(string dataType)
        {
            return dataType switch
            {
                "bigint" => "BIGINT",
                "bit" => "TINYINT(1)",
                "date" => "DATE",
                "datetime" => "DATETIME",
                "decimal" => "DECIMAL",
                "int" => "INT",
                "nchar" => "CHAR",
                "nvarchar" => "VARCHAR",
                "uniqueidentifier" => "CHAR(36)",
                "varchar" => "VARCHAR",
                _ => dataType.ToUpperInvariant()
            };
        }

        private static string MapOracleType(string dataType)
        {
            return dataType switch
            {
                "bigint" => "NUMBER(19)",
                "bit" => "NUMBER(1)",
                "date" => "DATE",
                "datetime" => "TIMESTAMP",
                "decimal" => "NUMBER",
                "int" => "NUMBER(10)",
                "nchar" => "NCHAR",
                "nvarchar" => "NVARCHAR2",
                "uniqueidentifier" => "RAW(16)",
                "varchar" => "VARCHAR2",
                _ => dataType.ToUpperInvariant()
            };
        }

        /// <summary>
        /// 外部キー制約行をRDBごとの識別子形式で生成します。
        /// </summary>
        private static string BuildForeignKeyConstraint(
            TableDefinition table,
            ColumnDefinition column,
            SqlDialect dialect,
            string schemaName)
        {
            var constraintName = $"FK_{table.TableName}_{column.ReferenceTableName}_{column.ColumnName}";
            return
                $"CONSTRAINT {EscapeName(constraintName, dialect)} " +
                $"FOREIGN KEY ({EscapeName(column.ColumnName, dialect)}) " +
                $"REFERENCES {BuildQualifiedTableName(schemaName, column.ReferenceTableName, dialect)} " +
                $"({EscapeName(column.ReferenceColumnName, dialect)})";
        }

        /// <summary>
        /// RDBごとの引用符でテーブル名・カラム名をエスケープします。
        /// </summary>
        private static string EscapeName(string name, SqlDialect dialect)
        {
            return dialect switch
            {
                SqlDialect.SqlServer => $"[{name.Replace("]", "]]")}]",
                SqlDialect.MySql => $"`{name.Replace("`", "``")}`",
                SqlDialect.Oracle => $"\"{name.Replace("\"", "\"\"")}\"",
                _ => name
            };
        }

        /// <summary>
        /// スキーマ名を含むテーブル識別子を生成します。
        /// </summary>
        private static string BuildQualifiedTableName(
            string schemaName,
            string tableName,
            SqlDialect dialect)
        {
            return string.IsNullOrWhiteSpace(schemaName)
                ? EscapeName(tableName, dialect)
                : $"{EscapeName(schemaName, dialect)}.{EscapeName(tableName, dialect)}";
        }

        /// <summary>
        /// テーブル固有値を優先して有効なスキーマ名を取得します。
        /// </summary>
        private static string ResolveSchemaName(
            TableDefinition table,
            DatabaseSettings? databaseSettings)
        {
            if (!string.IsNullOrWhiteSpace(table.SchemaName))
            {
                return table.SchemaName.Trim();
            }

            return databaseSettings?.DefaultSchemaName?.Trim() ?? string.Empty;
        }

        /// <summary>
        /// SQL Serverの拡張プロパティで必要となる既定スキーマ名を返します。
        /// </summary>
        private static string DefaultSchema(string schemaName)
        {
            return string.IsNullOrWhiteSpace(schemaName) ? "dbo" : schemaName;
        }

        /// <summary>
        /// RDBごとの自動採番句をカラム定義へ追加します。
        /// </summary>
        private static void AppendIdentityClause(
            StringBuilder builder,
            ColumnDefinition column,
            SqlDialect dialect)
        {
            if (!column.IsIdentity)
            {
                return;
            }

            switch (dialect)
            {
                case SqlDialect.SqlServer:
                    builder.Append(
                        $" IDENTITY({column.IdentitySeed ?? 1},{column.IdentityIncrement ?? 1})");
                    break;
                case SqlDialect.MySql:
                    builder.Append(" AUTO_INCREMENT");
                    break;
                case SqlDialect.Oracle:
                    builder.Append(" GENERATED BY DEFAULT AS IDENTITY");
                    break;
            }
        }

        /// <summary>
        /// MySQL向けの文字コードと照合順序をCREATE TABLE末尾へ追加します。
        /// </summary>
        private static void AppendMySqlTableOptions(
            StringBuilder builder,
            TableDefinition table,
            DatabaseSettings? databaseSettings)
        {
            var characterSet = string.IsNullOrWhiteSpace(table.CharacterSet)
                ? databaseSettings?.DefaultCharacterSet
                : table.CharacterSet;
            var collation = string.IsNullOrWhiteSpace(table.Collation)
                ? databaseSettings?.DefaultCollation
                : table.Collation;

            if (!string.IsNullOrWhiteSpace(characterSet))
            {
                builder.Append($" DEFAULT CHARACTER SET {characterSet.Trim()}");
            }

            if (!string.IsNullOrWhiteSpace(collation))
            {
                builder.Append($" COLLATE {collation.Trim()}");
            }
        }

        /// <summary>
        /// CREATE TABLE文に続けてインデックス作成文を出力します。
        /// </summary>
        private static void AppendIndexes(
            StringBuilder builder,
            TableDefinition table,
            SqlDialect dialect,
            string schemaName)
        {
            foreach (var index in table.Indexes.Where(item => item.Columns.Count > 0))
            {
                builder.AppendLine();
                builder.Append("CREATE ");
                if (index.IsUnique)
                {
                    builder.Append("UNIQUE ");
                }
                if (dialect == SqlDialect.SqlServer && index.IsClustered)
                {
                    builder.Append("CLUSTERED ");
                }

                builder.Append($"INDEX {EscapeName(index.IndexName, dialect)}");
                builder.AppendLine();
                builder.Append($"ON {BuildQualifiedTableName(schemaName, table.TableName, dialect)} (");
                builder.Append(string.Join(
                    ", ",
                    index.Columns.Select(column =>
                        $"{EscapeName(column.ColumnName, dialect)} " +
                        $"{(column.IsDescending ? "DESC" : "ASC")}")));
                builder.AppendLine(");");
            }
        }

        /// <summary>
        /// RDB間で表記が異なる既定値を調整します。
        /// </summary>
        private static string NormalizeDefaultValue(string defaultValue, SqlDialect dialect)
        {
            var value = defaultValue.Trim();
            if (value.Equals("CURRENT_TIMESTAMP", StringComparison.OrdinalIgnoreCase))
            {
                return "CURRENT_TIMESTAMP";
            }

            return value;
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
                    throw new InvalidOperationException(
                        $"{column.No}行目の外部キー参照先が未設定です。テーブルを編集して参照PKを選択してください。");
                }
            }

            ValidateIndexes(table);
        }

        /// <summary>
        /// SQL生成前にインデックス定義の必須値と参照カラムを確認します。
        /// </summary>
        private static void ValidateIndexes(TableDefinition table)
        {
            var indexNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var columnNames = table.Columns
                .Select(column => column.ColumnName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var index in table.Indexes)
            {
                if (string.IsNullOrWhiteSpace(index.IndexName))
                {
                    throw new InvalidOperationException("インデックス名が未設定です。");
                }
                if (!indexNames.Add(index.IndexName))
                {
                    throw new InvalidOperationException($"インデックス名 '{index.IndexName}' が重複しています。");
                }
                if (index.Columns.Count == 0)
                {
                    continue;
                }

                var usedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var indexColumn in index.Columns)
                {
                    if (!columnNames.Contains(indexColumn.ColumnName))
                    {
                        throw new InvalidOperationException(
                            $"インデックス '{index.IndexName}' のカラム '{indexColumn.ColumnName}' は存在しません。");
                    }
                    if (!usedColumns.Add(indexColumn.ColumnName))
                    {
                        throw new InvalidOperationException(
                            $"インデックス '{index.IndexName}' でカラム '{indexColumn.ColumnName}' が重複しています。");
                    }
                }
            }
        }

        private static bool UsesSize(string dataType)
        {
            return dataType is "char" or "nchar" or "varchar" or "nvarchar" or "decimal";
        }
    }
}
