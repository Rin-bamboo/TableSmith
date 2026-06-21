using System.Collections.ObjectModel;
using TableSmith.Models;

namespace TableSmith.Services
{
    /// <summary>
    /// 旧バージョンのJSONから読み込んだプロジェクトへ不足値を補完します。
    /// </summary>
    public class ProjectMigrationService
    {
        /// <summary>
        /// 現行モデルで安全に扱えるよう、設定とコレクションのnullを補完します。
        /// </summary>
        public TableSmithProject Migrate(TableSmithProject project)
        {
            project.DatabaseSettings ??= new DatabaseSettings();
            project.Tables ??= new ObservableCollection<TableDefinition>();
            project.ProjectName ??= string.Empty;
            project.DatabaseSettings.DefaultSchemaName ??= "dbo";
            project.DatabaseSettings.DefaultCharacterSet ??= string.Empty;
            project.DatabaseSettings.DefaultCollation ??= string.Empty;

            foreach (var table in project.Tables)
            {
                table.TableName ??= string.Empty;
                table.TableDisplayName ??= string.Empty;
                table.Description ??= string.Empty;
                table.SchemaName ??= string.Empty;
                table.CharacterSet ??= string.Empty;
                table.Collation ??= string.Empty;
                table.Columns ??= new ObservableCollection<ColumnDefinition>();
                table.Indexes ??= new ObservableCollection<IndexDefinition>();

                foreach (var column in table.Columns)
                {
                    column.ColumnName ??= string.Empty;
                    column.ColumnDisplayName ??= string.Empty;
                    column.DataType ??= string.Empty;
                    column.ForeignKeyReferenceId ??= string.Empty;
                    column.ReferenceTableName ??= string.Empty;
                    column.ReferenceColumnName ??= string.Empty;
                    column.AutoForeignKeyColumnName ??= string.Empty;
                    column.DefaultValue ??= string.Empty;
                    column.Description ??= string.Empty;
                    column.ProtectionType ??= string.Empty;
                }

                foreach (var index in table.Indexes)
                {
                    index.Columns ??= new ObservableCollection<IndexColumnDefinition>();
                    index.IndexName ??= string.Empty;
                    index.Description ??= string.Empty;
                    foreach (var indexColumn in index.Columns)
                    {
                        indexColumn.ColumnName ??= string.Empty;
                    }
                }
            }

            // Version 1以前のJSONを現行構造として扱える状態へ更新します。
            project.Version = Math.Max(project.Version, 2);
            return project;
        }
    }
}
