namespace TableSmith.Models
{
    /// <summary>
    /// アプリケーションの更新確認結果です。
    /// </summary>
    public sealed record UpdateCheckResult(
        bool IsInstalled,
        bool HasUpdate,
        string? CurrentVersion,
        string? LatestVersion,
        string? Message,
        Exception? Exception = null);
}
