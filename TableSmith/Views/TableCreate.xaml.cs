using System.Windows;
using System.Collections.ObjectModel;
using System.Text;
using System.Text.RegularExpressions;
using MahApps.Metro.Controls;
using TableSmith.Models;
using TableSmith.Services;

namespace TableSmith.Views
{
    /// <summary>
    /// テーブル定義作成画面です。
    /// </summary>
    public partial class TableCreate : MetroWindow
    {
        private static readonly Regex SqlNameRegex = new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

        private readonly string? _editingOriginalTableName;
        private readonly DatabaseSettings _databaseSettings;

        public TableDefinition CurrentTable { get; set; } = new();
        public ColumnDefinition? SelectedColumn { get; set; }
        public ObservableCollection<TableDefinition> ExistingTables { get; }
        public ObservableCollection<SchemaDefinition> SchemaDefinitions => _databaseSettings.Schemas;
        public IReadOnlyList<ConfigSelectionOption> CharacterSetOptions { get; private set; } =
            Array.Empty<ConfigSelectionOption>();
        public IReadOnlyList<ConfigSelectionOption> CollationOptions { get; private set; } =
            Array.Empty<ConfigSelectionOption>();
        public ObservableCollection<ForeignKeyReference> ForeignKeyReferences { get; } = new();

        public TableCreate()
            : this(new ObservableCollection<TableDefinition>(), new DatabaseSettings())
        {
        }

        /// <summary>
        /// テーブル新規作成用の画面を初期化します。
        /// </summary>
        /// <param name="existingTables">外部キー候補や重複チェックに使う既存テーブル一覧。</param>
        public TableCreate(ObservableCollection<TableDefinition> existingTables)
            : this(existingTables, new DatabaseSettings())
        {
        }

        /// <summary>
        /// テーブル新規作成用の画面をDB基本設定付きで初期化します。
        /// </summary>
        public TableCreate(
            ObservableCollection<TableDefinition> existingTables,
            DatabaseSettings databaseSettings)
        {
            this.ExistingTables = existingTables;
            this._databaseSettings = databaseSettings;
            this.CurrentTable.SchemaName = databaseSettings.DefaultSchemaName;
            LoadConfigSelectionOptions();
            LoadForeignKeyReferences();
            InitializeComponent();
            this.DataContext = this;
            ApplyDefaultColumnsTemplate();
        }

        /// <summary>
        /// テーブル編集用の画面を初期化します。キャンセル時に元データを壊さないよう、編集対象はコピーして保持します。
        /// </summary>
        /// <param name="existingTables">外部キー候補や重複チェックに使う既存テーブル一覧。</param>
        /// <param name="tableToEdit">編集対象のテーブル。</param>
        public TableCreate(ObservableCollection<TableDefinition> existingTables, TableDefinition tableToEdit)
            : this(existingTables, tableToEdit, new DatabaseSettings())
        {
        }

        /// <summary>
        /// テーブル編集用の画面をDB基本設定付きで初期化します。
        /// </summary>
        public TableCreate(
            ObservableCollection<TableDefinition> existingTables,
            TableDefinition tableToEdit,
            DatabaseSettings databaseSettings)
        {
            this.ExistingTables = existingTables;
            this._databaseSettings = databaseSettings;
            this.CurrentTable = CloneTable(tableToEdit);
            if (string.IsNullOrWhiteSpace(this.CurrentTable.SchemaName))
            {
                this.CurrentTable.SchemaName = databaseSettings.DefaultSchemaName;
            }
            LoadConfigSelectionOptions();
            this._editingOriginalTableName = tableToEdit.TableName;
            LoadForeignKeyReferences();
            InitializeComponent();
            this.Title = "TableSmith: テーブル定義編集";
            this.SaveButton.Content = "更新";
            this.DataContext = this;
            this.SelectedColumn = this.CurrentTable.Columns.FirstOrDefault();
        }

        /// <summary>
        /// 入力内容を破棄し、新しいテーブル定義を入力できる状態に戻します。
        /// </summary>
        private void NewMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.CurrentTable = new TableDefinition
            {
                SchemaName = _databaseSettings.DefaultSchemaName
            };
            LoadForeignKeyReferences();
            ApplyDefaultColumnsTemplate();
            this.DataContext = null;
            this.DataContext = this;
        }

        /// <summary>
        /// テーブル作成/編集画面を閉じます。
        /// </summary>
        private void CloseMenuItem_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// カラム入力行を1行追加します。
        /// </summary>
        private void AddColumnButton_Click(object sender, RoutedEventArgs e)
        {
            AddColumn();
        }

        /// <summary>
        /// カラム設定項目の操作説明を直接表示します。
        /// </summary>
        private void ColumnHelpButton_Click(object sender, RoutedEventArgs e)
        {
            var guide = new OperationGuide("column-settings")
            {
                Owner = this
            };
            guide.ShowDialog();
        }

        /// <summary>
        /// 選択中のカラム入力行を削除し、行番号を振り直します。
        /// </summary>
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

        /// <summary>
        /// 外部キー参照先が選択されたとき、参照PKの属性を現在のカラムへ反映します。
        /// </summary>
        private void ForeignKeyReferenceComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (sender is not System.Windows.Controls.ComboBox { DataContext: ColumnDefinition column })
            {
                return;
            }

            ApplyForeignKeyReference(column);
        }

        /// <summary>
        /// 入力内容を確定せずに画面を閉じます。
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        /// <summary>
        /// DataGridの編集中セルを確定し、入力チェック後にテーブル作成/更新を確定します。
        /// </summary>
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

        /// <summary>
        /// 初期値付きのカラムを追加し、追加行を選択状態にします。
        /// </summary>
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

        /// <summary>
        /// 新規テーブルにidと監査情報の標準カラムを投入します。
        /// </summary>
        private void ApplyDefaultColumnsTemplate()
        {
            this.CurrentTable.Columns = TableTemplateService.CreateDefaultColumns();
            this.SelectedColumn = this.CurrentTable.Columns.FirstOrDefault();
            this.ColumnDataGrid.SelectedItem = this.SelectedColumn;
            if (this.SelectedColumn != null)
            {
                this.ColumnDataGrid.ScrollIntoView(this.SelectedColumn);
            }
        }

        /// <summary>
        /// カラム一覧の表示順に合わせてNoを振り直します。
        /// </summary>
        private void RenumberColumns()
        {
            for (var index = 0; index < this.CurrentTable.Columns.Count; index++)
            {
                this.CurrentTable.Columns[index].No = index + 1;
            }

        }

        /// <summary>
        /// テーブル情報とカラム情報の入力チェックをまとめて実行します。
        /// </summary>
        /// <returns>入力内容が有効な場合はtrue。</returns>
        private bool ValidateInputs()
        {
            var errors = new StringBuilder();
            var warnings = new StringBuilder();

            ValidateTable(errors);
            ValidateColumns(errors, warnings);

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "入力確認", MessageBoxButton.OK, MessageBoxImage.Warning);
                return false;
            }

            if (warnings.Length == 0)
            {
                return true;
            }

            var result = MessageBox.Show(
                warnings + Environment.NewLine + "この内容で保存しますか？",
                "推奨設定の確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
            return result == MessageBoxResult.Yes;
        }

        /// <summary>
        /// テーブル物理名・論理名・重複の入力チェックを行います。
        /// </summary>
        private void ValidateTable(StringBuilder errors)
        {
            this.CurrentTable.TableName = this.CurrentTable.TableName.Trim();
            this.CurrentTable.TableDisplayName = this.CurrentTable.TableDisplayName.Trim();
            this.CurrentTable.Description = this.CurrentTable.Description.Trim();
            this.CurrentTable.SchemaName = this.CurrentTable.SchemaName.Trim();
            this.CurrentTable.CharacterSet = this.CurrentTable.CharacterSet.Trim();
            this.CurrentTable.Collation = this.CurrentTable.Collation.Trim();

            if (string.IsNullOrWhiteSpace(this.CurrentTable.TableName))
            {
                errors.AppendLine("・テーブル物理名を入力してください。");
            }
            else if (!SqlNameRegex.IsMatch(this.CurrentTable.TableName))
            {
                errors.AppendLine("・テーブル物理名は英字またはアンダースコアで始め、英数字とアンダースコアで入力してください。");
            }
            else if (this.ExistingTables.Any(table =>
                !IsEditingOriginalTable(table)
                && table.TableName.Equals(this.CurrentTable.TableName, StringComparison.OrdinalIgnoreCase)))
            {
                errors.AppendLine($"・テーブル物理名 '{this.CurrentTable.TableName}' は既に作成されています。");
            }

            if (string.IsNullOrWhiteSpace(this.CurrentTable.TableDisplayName))
            {
                errors.AppendLine("・テーブル論理名を入力してください。");
            }

            if (string.IsNullOrWhiteSpace(this.CurrentTable.SchemaName))
            {
                errors.AppendLine("・スキーマを選択してください。");
            }
            else if (!SchemaDefinitions.Any(schema =>
                         schema.SchemaName.Equals(
                             this.CurrentTable.SchemaName,
                             StringComparison.OrdinalIgnoreCase)))
            {
                errors.AppendLine($"・スキーマ '{this.CurrentTable.SchemaName}' は登録されていません。");
            }
        }

        /// <summary>
        /// カラム必須項目、名前重複、サイズ、PK/FK制約の入力チェックを行います。
        /// </summary>
        private void ValidateColumns(StringBuilder errors, StringBuilder warnings)
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
                column.ProtectionType = column.ProtectionType.Trim();

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

                if (column.Precision.HasValue && column.Precision.Value < 1)
                {
                    errors.AppendLine($"・{rowName}: 精度は1以上で入力してください。");
                }

                if (column.Scale.HasValue && column.Scale.Value < 0)
                {
                    errors.AppendLine($"・{rowName}: 小数桁数は0以上で入力してください。");
                }

                if (column.Precision.HasValue
                    && column.Scale.HasValue
                    && column.Scale.Value > column.Precision.Value)
                {
                    errors.AppendLine($"・{rowName}: 小数桁数は精度以下で入力してください。");
                }

                if (column.DataType.Equals("decimal", StringComparison.OrdinalIgnoreCase)
                    && !column.Precision.HasValue)
                {
                    errors.AppendLine($"・{rowName}: decimal型には精度を入力してください。");
                }

                if (column.IdentitySeed.HasValue && column.IdentitySeed.Value < 1)
                {
                    errors.AppendLine($"・{rowName}: 自動採番の開始値は1以上で入力してください。");
                }

                if (column.IdentityIncrement.HasValue && column.IdentityIncrement.Value < 1)
                {
                    errors.AppendLine($"・{rowName}: 自動採番の増分は1以上で入力してください。");
                }

                if (column.IsIdentity
                    && (!column.IsPrimaryKey || !IsNumericType(column.DataType)))
                {
                    warnings.AppendLine($"・{rowName}: 自動採番カラムはPKかつ数値型にすることを推奨します。");
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

        /// <summary>
        /// 外部キー設定の参照先PK、型、サイズの整合性をチェックします。
        /// </summary>
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

        /// <summary>
        /// 既存テーブルから外部キー参照候補となるPKカラム一覧を作成します。
        /// </summary>
        private void LoadForeignKeyReferences()
        {
            this.ForeignKeyReferences.Clear();

            foreach (var table in this.ExistingTables)
            {
                if (IsEditingOriginalTable(table))
                {
                    continue;
                }

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

        /// <summary>
        /// FK未選択時に参照先情報をクリアします。
        /// </summary>
        private static void ClearForeignKeyReference(ColumnDefinition column)
        {
            column.ForeignKeyReferenceId = string.Empty;
            column.ReferenceTableName = string.Empty;
            column.ReferenceColumnName = string.Empty;
            column.AutoForeignKeyColumnName = string.Empty;
        }

        /// <summary>
        /// テーブル定義を編集用に複製します。
        /// </summary>
        public static TableDefinition CloneTable(TableDefinition source)
        {
            return new TableDefinition
            {
                TableName = source.TableName,
                TableDisplayName = source.TableDisplayName,
                Description = source.Description,
                SchemaName = source.SchemaName,
                CharacterSet = source.CharacterSet,
                Collation = source.Collation,
                Columns = new ObservableCollection<ColumnDefinition>(
                    source.Columns.Select(CloneColumn)),
                Indexes = new ObservableCollection<IndexDefinition>(
                    source.Indexes.Select(IndexDefinitionEdit.CloneIndex))
            };
        }

        /// <summary>
        /// 編集画面で確定された値を元のテーブル定義へ反映します。
        /// </summary>
        public static void CopyTableValues(TableDefinition source, TableDefinition target)
        {
            target.TableName = source.TableName;
            target.TableDisplayName = source.TableDisplayName;
            target.Description = source.Description;
            target.SchemaName = source.SchemaName;
            target.CharacterSet = source.CharacterSet;
            target.Collation = source.Collation;
            target.Columns = new ObservableCollection<ColumnDefinition>(
                source.Columns.Select(CloneColumn));
            target.Indexes = new ObservableCollection<IndexDefinition>(
                source.Indexes.Select(IndexDefinitionEdit.CloneIndex));
        }

        /// <summary>
        /// 編集中の元テーブル自身かどうかを判定します。重複チェックやFK候補作成から自分自身を除外するために使います。
        /// </summary>
        private bool IsEditingOriginalTable(TableDefinition table)
        {
            return !string.IsNullOrWhiteSpace(this._editingOriginalTableName)
                && table.TableName.Equals(this._editingOriginalTableName, StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// カラム定義を複製します。
        /// </summary>
        private static ColumnDefinition CloneColumn(ColumnDefinition source)
        {
            return new ColumnDefinition
            {
                No = source.No,
                ColumnName = source.ColumnName,
                ColumnDisplayName = source.ColumnDisplayName,
                DataType = source.DataType,
                DataSize = source.DataSize,
                Precision = source.Precision,
                Scale = source.Scale,
                IsPrimaryKey = source.IsPrimaryKey,
                IsNotNull = source.IsNotNull,
                IsForeignKey = source.IsForeignKey,
                ForeignKeyReferenceId = source.ForeignKeyReferenceId,
                ReferenceTableName = source.ReferenceTableName,
                ReferenceColumnName = source.ReferenceColumnName,
                AutoForeignKeyColumnName = source.AutoForeignKeyColumnName,
                IsIdentity = source.IsIdentity,
                IdentitySeed = source.IdentitySeed,
                IdentityIncrement = source.IdentityIncrement,
                IsProtected = source.IsProtected,
                ProtectionType = source.ProtectionType,
                DefaultValue = source.DefaultValue,
                Description = source.Description
            };
        }

        /// <summary>
        /// 選択された参照PKのカラム情報をFKカラムへ継承し、物理名の初期値を設定します。
        /// </summary>
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
            column.Precision = referencedColumn.Precision;
            column.Scale = referencedColumn.Scale;
            column.IsNotNull = referencedColumn.IsNotNull;
            column.ReferenceTableName = reference.TableName;
            column.ReferenceColumnName = reference.ColumnName;

        }

        /// <summary>
        /// サイズ指定が必須となるデータ型かどうかを判定します。
        /// </summary>
        private static bool RequiresSize(string dataType)
        {
            return dataType.Equals("char", StringComparison.OrdinalIgnoreCase)
                || dataType.Equals("nchar", StringComparison.OrdinalIgnoreCase)
                || dataType.Equals("varchar", StringComparison.OrdinalIgnoreCase)
                || dataType.Equals("nvarchar", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// 自動採番に適した数値型かどうかを判定します。
        /// </summary>
        private static bool IsNumericType(string dataType)
        {
            return dataType.Equals("bigint", StringComparison.OrdinalIgnoreCase)
                || dataType.Equals("int", StringComparison.OrdinalIgnoreCase)
                || dataType.Equals("decimal", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// App.configからテーブル固有設定用の文字コード・照合順序候補を読み込みます。
        /// </summary>
        private void LoadConfigSelectionOptions()
        {
            CharacterSetOptions = ConfigSelectionService.GetCharacterSetOptions(
                CurrentTable.CharacterSet,
                includeEmpty: true);
            CollationOptions = ConfigSelectionService.GetCollationOptions(
                CurrentTable.Collation,
                includeEmpty: true);
        }
    }
}
