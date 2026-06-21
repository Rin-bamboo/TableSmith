using System.Collections.ObjectModel;

namespace TableSmith.Models
{
    /// <summary>
    /// テーブルに設定するインデックス定義です。
    /// </summary>
    public class IndexDefinition
    {
        /// <summary>
        /// インデックス名です。
        /// </summary>
        public string IndexName { get; set; } = string.Empty;

        /// <summary>
        /// 一意インデックスかどうかを表します。
        /// </summary>
        public bool IsUnique { get; set; }

        /// <summary>
        /// クラスタ化インデックスかどうかを表します。SQL Server向けの設定です。
        /// </summary>
        public bool IsClustered { get; set; }

        /// <summary>
        /// インデックス対象カラム一覧です。
        /// </summary>
        public ObservableCollection<IndexColumnDefinition> Columns { get; set; } = new();

        /// <summary>
        /// インデックスの説明です。
        /// </summary>
        public string Description { get; set; } = string.Empty;
    }
}
