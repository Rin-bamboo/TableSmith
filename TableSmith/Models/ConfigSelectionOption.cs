namespace TableSmith.Models
{
    /// <summary>
    /// コンフィグから読み込んだ文字列選択肢です。
    /// </summary>
    public class ConfigSelectionOption
    {
        public string Value { get; init; } = string.Empty;
        public string DisplayName { get; init; } = string.Empty;

        /// <summary>
        /// 画面表示名と、JSON・SQLで使用する実際の設定値を併記します。
        /// </summary>
        public string DisplayText => string.IsNullOrWhiteSpace(Value)
            ? DisplayName
            : $"{DisplayName} [{Value}]";
    }

    /// <summary>
    /// SQLファイル文字コードの選択肢です。
    /// </summary>
    public class SqlFileEncodingOption
    {
        public SqlFileEncoding Value { get; init; }
        public string DisplayName { get; init; } = string.Empty;

        /// <summary>
        /// 画面表示名と、JSONへ保存するenum値を併記します。
        /// </summary>
        public string DisplayText => $"{DisplayName} [{Value}]";
    }
}
