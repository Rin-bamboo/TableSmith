using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows;
using MahApps.Metro.Controls;
using TableSmith.Models;
using TableSmith.Services;

namespace TableSmith.Views
{
    /// <summary>
    /// テーブルに設定するインデックスを編集する画面です。
    /// </summary>
    public partial class IndexDefinitionEdit : MetroWindow, INotifyPropertyChanged
    {
        private readonly TableDefinition _table;
        private IndexDefinition? _selectedIndex;
        private IndexColumnDefinition? _selectedIndexColumn;

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// キャンセル時に元定義を変更しないためのインデックス作業コピーです。
        /// </summary>
        public ObservableCollection<IndexDefinition> Indexes { get; }

        /// <summary>
        /// インデックスへ追加できるテーブル内カラム名です。
        /// </summary>
        public IReadOnlyList<string> AvailableColumnNames { get; }

        public IndexDefinition? SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (_selectedIndex == value) return;
                _selectedIndex = value;
                SelectedIndexColumn = value?.Columns.FirstOrDefault();
                OnPropertyChanged();
            }
        }

        public IndexColumnDefinition? SelectedIndexColumn
        {
            get => _selectedIndexColumn;
            set
            {
                if (_selectedIndexColumn == value) return;
                _selectedIndexColumn = value;
                OnPropertyChanged();
            }
        }

        public IndexDefinitionEdit(TableDefinition table)
        {
            _table = table;
            Indexes = new ObservableCollection<IndexDefinition>(
                table.Indexes.Select(CloneIndex));
            foreach (var index in Indexes)
            {
                SubscribeIndex(index);
            }
            AvailableColumnNames = table.Columns
                .OrderBy(column => column.No)
                .Select(column => column.ColumnName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();
            SelectedIndex = Indexes.FirstOrDefault();

            InitializeComponent();
            IndexColumnNameColumn.ItemsSource = AvailableColumnNames;
            DataContext = this;
        }

        /// <summary>
        /// 空のインデックス定義を追加します。
        /// </summary>
        private void AddIndexButton_Click(object sender, RoutedEventArgs e)
        {
            var index = new IndexDefinition
            {
                IsNameCustomized = false
            };
            if (AvailableColumnNames.Count > 0)
            {
                index.Columns.Add(new IndexColumnDefinition
                {
                    ColumnName = AvailableColumnNames[0]
                });
            }

            SubscribeIndex(index);
            UpdateStandardIndexName(index);
            Indexes.Add(index);
            SelectedIndex = index;
        }

        /// <summary>
        /// 選択中のインデックス定義を削除します。
        /// </summary>
        private void RemoveIndexButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedIndex == null)
            {
                MessageBox.Show("削除するインデックスを選択してください。", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            UnsubscribeIndex(SelectedIndex);
            Indexes.Remove(SelectedIndex);
            SelectedIndex = Indexes.FirstOrDefault();
        }

        /// <summary>
        /// 選択中インデックスへ対象カラムを追加します。
        /// </summary>
        private void AddIndexColumnButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedIndex == null)
            {
                MessageBox.Show("先にインデックスを選択してください。", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var column = new IndexColumnDefinition
            {
                ColumnName = AvailableColumnNames.FirstOrDefault() ?? string.Empty
            };
            SelectedIndex.Columns.Add(column);
            SelectedIndexColumn = column;
            UpdateStandardIndexName(SelectedIndex);
        }

        /// <summary>
        /// 選択中インデックスから対象カラムを削除します。
        /// </summary>
        private void RemoveIndexColumnButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedIndex == null || SelectedIndexColumn == null)
            {
                MessageBox.Show("削除する対象カラムを選択してください。", "確認", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            SelectedIndex.Columns.Remove(SelectedIndexColumn);
            SelectedIndexColumn = SelectedIndex.Columns.FirstOrDefault();
            UpdateStandardIndexName(SelectedIndex);
        }

        /// <summary>
        /// カスタム名を解除したとき、現在の対象カラムから標準名を再生成します。
        /// </summary>
        private void CustomNameCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (SelectedIndex != null && !SelectedIndex.IsNameCustomized)
            {
                UpdateStandardIndexName(SelectedIndex);
            }
        }

        /// <summary>
        /// 入力内容を検証し、確定したインデックスをテーブルへ反映します。
        /// </summary>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            var errors = ValidateIndexes();
            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString(), "入力確認", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _table.Indexes = new ObservableCollection<IndexDefinition>(Indexes.Select(CloneIndex));
            DialogResult = true;
            Close();
        }

        /// <summary>
        /// インデックス名、対象カラム、重複、参照先の整合性を確認します。
        /// </summary>
        private StringBuilder ValidateIndexes()
        {
            var errors = new StringBuilder();
            var names = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var availableNames = AvailableColumnNames.ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var index in Indexes)
            {
                UpdateStandardIndexName(index);
                index.IndexName = index.IndexName.Trim();
                index.Description = index.Description.Trim();

                if (string.IsNullOrWhiteSpace(index.IndexName))
                {
                    errors.AppendLine("・インデックス名を入力してください。");
                }
                else if (!names.Add(index.IndexName))
                {
                    errors.AppendLine($"・インデックス名 '{index.IndexName}' が重複しています。");
                }

                if (index.Columns.Count == 0)
                {
                    errors.AppendLine($"・インデックス '{index.IndexName}' の対象カラムを1件以上追加してください。");
                    continue;
                }

                var usedColumns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var column in index.Columns)
                {
                    if (!availableNames.Contains(column.ColumnName))
                    {
                        errors.AppendLine($"・インデックス '{index.IndexName}' に存在しないカラムが指定されています。");
                    }
                    else if (!usedColumns.Add(column.ColumnName))
                    {
                        errors.AppendLine($"・インデックス '{index.IndexName}' でカラム '{column.ColumnName}' が重複しています。");
                    }
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

        /// <summary>
        /// インデックス定義を子カラムも含めて複製します。
        /// </summary>
        public static IndexDefinition CloneIndex(IndexDefinition source)
        {
            return new IndexDefinition
            {
                IndexName = source.IndexName,
                IsNameCustomized = source.IsNameCustomized,
                IsUnique = source.IsUnique,
                IsClustered = source.IsClustered,
                Description = source.Description,
                Columns = new ObservableCollection<IndexColumnDefinition>(
                    source.Columns.Select(column => new IndexColumnDefinition
                    {
                        ColumnName = column.ColumnName,
                        IsDescending = column.IsDescending
                    }))
            };
        }

        /// <summary>
        /// 対象カラムの追加・削除・名称変更を監視し、標準名へ反映します。
        /// </summary>
        private void SubscribeIndex(IndexDefinition index)
        {
            index.Columns.CollectionChanged += IndexColumns_CollectionChanged;
            foreach (var column in index.Columns)
            {
                column.PropertyChanged += IndexColumn_PropertyChanged;
            }
        }

        private void UnsubscribeIndex(IndexDefinition index)
        {
            index.Columns.CollectionChanged -= IndexColumns_CollectionChanged;
            foreach (var column in index.Columns)
            {
                column.PropertyChanged -= IndexColumn_PropertyChanged;
            }
        }

        private void IndexColumns_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (IndexColumnDefinition column in e.OldItems)
                {
                    column.PropertyChanged -= IndexColumn_PropertyChanged;
                }
            }
            if (e.NewItems != null)
            {
                foreach (IndexColumnDefinition column in e.NewItems)
                {
                    column.PropertyChanged += IndexColumn_PropertyChanged;
                }
            }

            var index = Indexes.FirstOrDefault(item => ReferenceEquals(item.Columns, sender));
            if (index != null)
            {
                UpdateStandardIndexName(index);
            }
        }

        private void IndexColumn_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(IndexColumnDefinition.ColumnName))
            {
                return;
            }

            var changedColumn = sender as IndexColumnDefinition;
            var index = changedColumn == null
                ? null
                : Indexes.FirstOrDefault(item => item.Columns.Contains(changedColumn));
            if (index != null)
            {
                UpdateStandardIndexName(index);
            }
        }

        /// <summary>
        /// 一般的な命名規則 IX_テーブル名_カラム1_カラム2 で標準名を生成します。
        /// </summary>
        private void UpdateStandardIndexName(IndexDefinition index)
        {
            if (index.IsNameCustomized)
            {
                return;
            }

            var columnNames = index.Columns
                .Select(column => column.ColumnName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();
            index.IndexName = IndexNamingService.CreateStandardName(
                _table.TableName,
                columnNames);
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
