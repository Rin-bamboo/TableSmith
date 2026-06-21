namespace TableSmith.Models
{
    /// <summary>
    /// アプリケーション更新のダウンロード・適用結果です。
    /// </summary>
    public sealed record UpdateApplyResult(
        bool Success,
        bool Restarting,
        string? Message,
        Exception? Exception = null);
}
