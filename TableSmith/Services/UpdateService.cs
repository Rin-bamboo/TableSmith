using System.Reflection;
using TableSmith.Models;
using Velopack;
using Velopack.Sources;

namespace TableSmith.Services
{
    /// <summary>
    /// GitHub Releasesを更新元として、TableSmithの更新確認と適用を行います。
    /// </summary>
    public sealed class UpdateService
    {
        /// <summary>
        /// TableSmithの公開GitHubリポジトリURLです。
        /// </summary>
        private const string RepositoryUrl = "https://github.com/Rin-bamboo/TableSmith";

        /// <summary>
        /// GitHub Releasesから利用可能な更新を確認します。
        /// </summary>
        public async Task<UpdateCheckResult> CheckForUpdatesAsync(
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var manager = CreateUpdateManager();
                var currentVersion = GetCurrentVersion(manager);
                if (!manager.IsInstalled)
                {
                    return new UpdateCheckResult(
                        IsInstalled: false,
                        HasUpdate: false,
                        CurrentVersion: currentVersion,
                        LatestVersion: null,
                        Message: "このアプリはインストーラー経由で起動されていないため、更新確認は利用できません。");
                }

                // CheckForUpdatesAsync自体にはCancellationToken引数がないため、
                // 呼び出し側の待機をキャンセルできるようWaitAsyncを使用します。
                var updateInfo = await manager
                    .CheckForUpdatesAsync()
                    .WaitAsync(cancellationToken)
                    .ConfigureAwait(false);

                if (updateInfo == null)
                {
                    return new UpdateCheckResult(
                        IsInstalled: true,
                        HasUpdate: false,
                        CurrentVersion: currentVersion,
                        LatestVersion: null,
                        Message: "現在のバージョンは最新です。");
                }

                return new UpdateCheckResult(
                    IsInstalled: true,
                    HasUpdate: true,
                    CurrentVersion: currentVersion,
                    LatestVersion: updateInfo.TargetFullRelease.Version.ToString(),
                    Message: "新しいバージョンがあります。");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new UpdateCheckResult(
                    IsInstalled: false,
                    HasUpdate: false,
                    CurrentVersion: GetAssemblyVersion(),
                    LatestVersion: null,
                    Message: "更新確認に失敗しました。ネットワーク接続を確認してください。",
                    Exception: ex);
            }
        }

        /// <summary>
        /// 最新版をダウンロードし、Velopackへ更新適用と再起動を依頼します。
        /// </summary>
        public async Task<UpdateApplyResult> DownloadAndApplyUpdatesAsync(
            IProgress<int>? progress = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var manager = CreateUpdateManager();
                if (!manager.IsInstalled)
                {
                    return new UpdateApplyResult(
                        Success: false,
                        Restarting: false,
                        Message: "このアプリはインストーラー経由で起動されていないため、更新できません。");
                }

                var updateInfo = await manager
                    .CheckForUpdatesAsync()
                    .WaitAsync(cancellationToken)
                    .ConfigureAwait(false);
                if (updateInfo == null)
                {
                    return new UpdateApplyResult(
                        Success: true,
                        Restarting: false,
                        Message: "現在のバージョンは最新です。");
                }

                await manager
                    .DownloadUpdatesAsync(
                        updateInfo,
                        value => progress?.Report(value),
                        cancellationToken)
                    .ConfigureAwait(false);

                cancellationToken.ThrowIfCancellationRequested();

                // 未保存データなど、アプリ固有の保存処理はこのメソッドを呼ぶ前に完了させます。
                // ApplyUpdatesAndRestartを呼ぶと現在のプロセスは終了し、更新後に再起動します。
                manager.ApplyUpdatesAndRestart(updateInfo.TargetFullRelease);

                return new UpdateApplyResult(
                    Success: true,
                    Restarting: true,
                    Message: "更新を適用するため、アプリを再起動します。");
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return new UpdateApplyResult(
                    Success: false,
                    Restarting: false,
                    Message: "更新のダウンロードまたは適用に失敗しました。",
                    Exception: ex);
            }
        }

        /// <summary>
        /// 公開GitHub Releasesを参照するUpdateManagerを作成します。
        /// </summary>
        private static UpdateManager CreateUpdateManager()
        {
            var source = new GithubSource(
                RepositoryUrl,
                accessToken: null,
                prerelease: false,
                downloader: null);
            return new UpdateManager(source);
        }

        /// <summary>
        /// インストール済みの場合はVelopackのバージョンを、未インストール時はアセンブリ情報を返します。
        /// </summary>
        private static string GetCurrentVersion(UpdateManager manager)
        {
            return manager.CurrentVersion?.ToString() ?? GetAssemblyVersion();
        }

        /// <summary>
        /// 実行アセンブリから表示用の現在バージョンを取得します。
        /// </summary>
        private static string GetAssemblyVersion()
        {
            var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            var informationalVersion = assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                ?.InformationalVersion;

            if (!string.IsNullOrWhiteSpace(informationalVersion))
            {
                var metadataIndex = informationalVersion.IndexOf('+');
                return metadataIndex >= 0
                    ? informationalVersion[..metadataIndex]
                    : informationalVersion;
            }

            return assembly.GetName().Version?.ToString(3) ?? "0.0.0";
        }
    }
}
