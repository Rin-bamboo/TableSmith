using System.Collections.ObjectModel;

namespace TableSmith.Models
{
    /// <summary>
    /// TableSmithで管理するプロジェクト全体の設計情報です。
    /// </summary>
    public class TableSmithProject
    {
        /// <summary>
        /// JSON構造のバージョンです。
        /// </summary>
        public int Version { get; set; } = 2;

        /// <summary>
        /// プロジェクト名です。
        /// </summary>
        public string ProjectName { get; set; } = string.Empty;

        /// <summary>
        /// プロジェクト全体のDB基本設定です。
        /// </summary>
        public DatabaseSettings DatabaseSettings { get; set; } = new();

        /// <summary>
        /// プロジェクトに含まれるテーブル一覧です。
        /// </summary>
        public ObservableCollection<TableDefinition> Tables { get; set; } = new();
    }
}
