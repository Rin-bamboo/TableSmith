namespace TableSmith.Models
{
    /// <summary>
    /// SQLファイル出力時に使用する文字コードを表します。
    /// </summary>
    public enum SqlFileEncoding
    {
        Utf8Bom,
        Utf8NoBom,
        ShiftJis,
        Utf16
    }
}
