using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using MahApps.Metro.Controls;
using TableSmith.Models;

namespace TableSmith.Views
{
    /// <summary>
    /// プロジェクトで利用するスキーマ一覧と既定スキーマを編集する画面です。
    /// </summary>
    public partial class SchemaManagement : MetroWindow, INotifyPropertyChanged
    {
        private static readonly Regex SqlNameRegex =
            new(@"^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

        private readonly DatabaseSettings _databaseSettings;
        private readonly IReadOnlyCollection<TableDefinition> _tables;
        private SchemaDefinition? _selectedSchema;
        private string _defaultSchemaName;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// キャンセル時に元設定を変更しないためのスキーマ作業コピーです。
        /// </summary>
        public ObservableCollection<SchemaDefinition> Schemas { get; }

        public SchemaDefinition? SelectedSchema
        {
            get => _selectedSchema;
            set
            {
                if (_selectedSchema == value) return;
                _selectedSchema = value;
                OnPropertyChanged();
            }
        }

        public string DefaultSchemaName
        {
            get => _defaultSchemaName;
            set
            {
                if (_defaultSchemaName == value) return;
                _defaultSchemaName = value;
                OnPropertyChanged();
            }
        }

        public SchemaManagement(
            DatabaseSettings databaseSettings,
            IEnumerable<TableDefinition> tables)
        {
            _databaseSettings = databaseSettings;
            _tables = tables.ToList();
            Schemas = new ObservableCollection<SchemaDefinition>(
                databaseSettings.Schemas.Select(CloneSchema));
            _defaultSchemaName = databaseSettings.DefaultSchemaName;
            SelectedSchema = Schemas.FirstOrDefault();

            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 新しい空のスキーマ定義を追加します。
        /// </summary>
        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            var schema = new SchemaDefinition();
            Schemas.Add(schema);
            SelectedSchema = schema;
        }

        /// <summary>
        /// 選択したスキーマを作業一覧から削除します。
        /// </summary>
        private void RemoveButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedSchema == null)
            {
                MessageBox.Show("削除するスキーマを選択してください。", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var schemaName = SelectedSchema.SchemaName;
            if (!string.IsNullOrWhiteSpace(schemaName)
                && _tables.Any(table =>
                    table.SchemaName.Equals(schemaName, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show(
                    $"スキーマ '{schemaName}' はテーブルで使用されているため削除できません。",
                    "確認",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }

            Schemas.Remove(SelectedSchema);
            SelectedSchema = Schemas.FirstOrDefault();
            if (string.Equals(schemaName, DefaultSchemaName, StringComparison.OrdinalIgnoreCase))
            {
                DefaultSchemaName = Schemas.FirstOrDefault()?.SchemaName ?? string.Empty;
            }
        }

        /// <summary>
        /// 入力内容を検証し、プロジェクトのDB基本設定へ反映します。
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var errors = ValidateSchemas();
            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "入力確認", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _databaseSettings.Schemas = new ObservableCollection<SchemaDefinition>(
                Schemas.Select(CloneSchema));
            _databaseSettings.DefaultSchemaName = DefaultSchemaName;
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// スキーマ名の必須・形式・重複と既定スキーマの整合性を確認します。
        /// </summary>
        private StringBuilder ValidateSchemas()
        {
            var errors = new StringBuilder();
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (Schemas.Count == 0)
            {
                errors.AppendLine("・スキーマを1件以上登録してください。");
                return errors;
            }

            foreach (var schema in Schemas)
            {
                schema.SchemaName = schema.SchemaName.Trim();
                schema.Description = schema.Description.Trim();

                if (string.IsNullOrWhiteSpace(schema.SchemaName))
                {
                    errors.AppendLine("・スキーマ名を入力してください。");
                }
                else if (!SqlNameRegex.IsMatch(schema.SchemaName))
                {
                    errors.AppendLine($"・スキーマ名 '{schema.SchemaName}' は英数字とアンダースコアで入力してください。");
                }
                else if (!names.Add(schema.SchemaName))
                {
                    errors.AppendLine($"・スキーマ名 '{schema.SchemaName}' が重複しています。");
                }
            }

            if (string.IsNullOrWhiteSpace(DefaultSchemaName)
                || !names.Contains(DefaultSchemaName))
            {
                errors.AppendLine("・登録済みスキーマから既定スキーマを選択してください。");
            }

            foreach (var usedSchemaName in _tables
                         .Select(table => table.SchemaName)
                         .Where(name => !string.IsNullOrWhiteSpace(name))
                         .Distinct(StringComparer.OrdinalIgnoreCase))
            {
                if (!names.Contains(usedSchemaName))
                {
                    errors.AppendLine(
                        $"・スキーマ '{usedSchemaName}' はテーブルで使用されているため、削除または名前変更できません。");
                }
            }

            return errors;
        }

        /// <summary>
        /// 編集内容を反映せずに画面を閉じます。
        /// </summary>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private static SchemaDefinition CloneSchema(SchemaDefinition source)
        {
            return new SchemaDefinition
            {
                SchemaName = source.SchemaName,
                Description = source.Description
            };
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
