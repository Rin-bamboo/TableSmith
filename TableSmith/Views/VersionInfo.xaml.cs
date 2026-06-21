using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using MahApps.Metro.Controls;
using TableSmith.Models;
using TableSmith.Services;

namespace TableSmith.Views
{
    /// <summary>
    /// アプリケーションの製品情報と実行環境を表示する画面です。
    /// </summary>
    public partial class VersionInfo : MetroWindow
    {
        private static readonly log4net.ILog Log = log4net.LogManager.GetLogger(typeof(VersionInfo));
        private readonly UpdateService _updateService = new();
        private readonly Func<bool>? _prepareForRestart;
        private readonly bool _checkOnOpen;
        private CancellationTokenSource? _updateCancellationTokenSource;
        private bool _isUpdateOperationRunning;
        private UpdateCheckResult? _lastCheckResult;

        public string ProductName { get; }
        public string VersionText { get; }
        public string Description { get; }
        public string AssemblyName { get; }
        public string RuntimeText { get; }
        public string Copyright { get; }

        public VersionInfo(
            Func<bool>? prepareForRestart = null,
            bool checkOnOpen = false)
        {
            _prepareForRestart = prepareForRestart;
            _checkOnOpen = checkOnOpen;

            var assembly = Assembly.GetExecutingAssembly();
            var assemblyName = assembly.GetName();

            this.ProductName = GetAttribute<AssemblyProductAttribute>(assembly)?.Product
                ?? assemblyName.Name
                ?? "TableSmith";
            this.VersionText = $"Version {GetVersion(assembly)}";
            this.Description = GetAttribute<AssemblyDescriptionAttribute>(assembly)?.Description
                ?? "テーブル設計支援アプリケーション";
            this.AssemblyName = assemblyName.Name ?? "TableSmith";
            this.RuntimeText = $"{RuntimeInformation.FrameworkDescription} / {RuntimeInformation.OSDescription}";
            this.Copyright = GetAttribute<AssemblyCopyrightAttribute>(assembly)?.Copyright
                ?? "Copyright (c) 2025 Bamboo";

            InitializeComponent();
            this.DataContext = this;
            this.Loaded += VersionInfo_Loaded;
            this.Closed += VersionInfo_Closed;
        }

        /// <summary>
        /// メニューの「更新を確認」から開かれた場合、画面表示後に更新確認を開始します。
        /// </summary>
        private async void VersionInfo_Loaded(object sender, RoutedEventArgs e)
        {
            if (_checkOnOpen)
            {
                await CheckForUpdatesAsync();
            }
        }

        /// <summary>
        /// 更新確認ボタンからGitHub Releasesの確認を開始します。
        /// </summary>
        private async void CheckUpdateButton_Click(object sender, RoutedEventArgs e)
        {
            await CheckForUpdatesAsync();
        }

        /// <summary>
        /// 更新確認を実行し、結果と更新操作をユーザーへ案内します。
        /// </summary>
        private async Task CheckForUpdatesAsync()
        {
            if (_isUpdateOperationRunning)
            {
                return;
            }

            BeginUpdateOperation("更新確認中です...", isIndeterminate: true);
            try
            {
                _updateCancellationTokenSource = new CancellationTokenSource();
                _lastCheckResult = await _updateService.CheckForUpdatesAsync(
                    _updateCancellationTokenSource.Token);

                if (_lastCheckResult.Exception != null)
                {
                    Log.Error("更新確認に失敗しました。", _lastCheckResult.Exception);
                }

                UpdateStatusTextBlock.Text = CreateCheckResultMessage(_lastCheckResult);
                UpdateNowButton.Visibility = _lastCheckResult.HasUpdate
                    ? Visibility.Visible
                    : Visibility.Collapsed;

                if (_lastCheckResult.Exception != null)
                {
                    MessageBox.Show(
                        this,
                        _lastCheckResult.Message,
                        "更新確認",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                else if (!_lastCheckResult.IsInstalled)
                {
                    MessageBox.Show(
                        this,
                        _lastCheckResult.Message,
                        "更新確認",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else if (!_lastCheckResult.HasUpdate)
                {
                    MessageBox.Show(
                        this,
                        "現在のバージョンは最新です。",
                        "更新確認",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
                else
                {
                    var result = MessageBox.Show(
                        this,
                        $"新しいバージョンがあります。\n\n"
                        + $"現在: {_lastCheckResult.CurrentVersion}\n"
                        + $"最新: {_lastCheckResult.LatestVersion}\n\n"
                        + "今すぐ更新しますか？",
                        "更新確認",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        // 更新確認のビジー状態を解除してから、ダウンロード処理へ切り替えます。
                        EndUpdateOperation();
                        await DownloadAndApplyUpdatesAsync();
                        return;
                    }
                }
            }
            catch (OperationCanceledException)
            {
                UpdateStatusTextBlock.Text = "更新確認をキャンセルしました。";
            }
            finally
            {
                EndUpdateOperation();
            }
        }

        /// <summary>
        /// 更新ボタンから最新版をダウンロードし、適用と再起動を開始します。
        /// </summary>
        private async void UpdateNowButton_Click(object sender, RoutedEventArgs e)
        {
            if (_lastCheckResult?.HasUpdate != true)
            {
                await CheckForUpdatesAsync();
                return;
            }

            var result = MessageBox.Show(
                this,
                $"バージョン {_lastCheckResult.LatestVersion} をダウンロードして更新しますか？\n"
                + "更新適用時にアプリを再起動します。",
                "更新確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                await DownloadAndApplyUpdatesAsync();
            }
        }

        /// <summary>
        /// 未保存データを確認した後、更新のダウンロード・適用を実行します。
        /// </summary>
        private async Task DownloadAndApplyUpdatesAsync()
        {
            if (_isUpdateOperationRunning)
            {
                return;
            }

            // Velopackによる再起動前に、メイン画面の既存保存確認を完了させます。
            if (_prepareForRestart != null && !_prepareForRestart())
            {
                UpdateStatusTextBlock.Text = "更新をキャンセルしました。";
                return;
            }

            BeginUpdateOperation("更新をダウンロードしています... 0%", isIndeterminate: false);
            try
            {
                _updateCancellationTokenSource = new CancellationTokenSource();
                var progress = new Progress<int>(value =>
                {
                    UpdateProgressBar.Value = value;
                    UpdateStatusTextBlock.Text = $"更新をダウンロードしています... {value}%";
                });

                var result = await _updateService.DownloadAndApplyUpdatesAsync(
                    progress,
                    _updateCancellationTokenSource.Token);

                if (result.Exception != null)
                {
                    Log.Error("更新のダウンロードまたは適用に失敗しました。", result.Exception);
                }

                UpdateStatusTextBlock.Text = result.Message ?? string.Empty;
                if (!result.Success)
                {
                    MessageBox.Show(
                        this,
                        result.Message,
                        "更新",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                if (result.Restarting)
                {
                    MessageBox.Show(
                        this,
                        "更新を適用するため、アプリを再起動します。",
                        "更新",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (OperationCanceledException)
            {
                UpdateStatusTextBlock.Text = "更新をキャンセルしました。";
            }
            finally
            {
                EndUpdateOperation();
            }
        }

        /// <summary>
        /// 更新処理中の表示へ切り替え、二重実行を防止します。
        /// </summary>
        private void BeginUpdateOperation(string message, bool isIndeterminate)
        {
            _isUpdateOperationRunning = true;
            CheckUpdateButton.IsEnabled = false;
            UpdateNowButton.IsEnabled = false;
            UpdateStatusTextBlock.Text = message;
            UpdateProgressBar.Value = 0;
            UpdateProgressBar.IsIndeterminate = isIndeterminate;
            UpdateProgressBar.Visibility = Visibility.Visible;
        }

        /// <summary>
        /// 更新操作のUI状態を通常表示へ戻します。
        /// </summary>
        private void EndUpdateOperation()
        {
            _updateCancellationTokenSource?.Dispose();
            _updateCancellationTokenSource = null;
            _isUpdateOperationRunning = false;
            CheckUpdateButton.IsEnabled = true;
            UpdateNowButton.IsEnabled = true;
            UpdateProgressBar.IsIndeterminate = false;
            UpdateProgressBar.Visibility = Visibility.Collapsed;
        }

        /// <summary>
        /// 更新確認結果から画面表示用メッセージを作成します。
        /// </summary>
        private static string CreateCheckResultMessage(UpdateCheckResult result)
        {
            if (!result.HasUpdate)
            {
                return result.Message ?? string.Empty;
            }

            return $"新しいバージョンがあります。現在: {result.CurrentVersion} / 最新: {result.LatestVersion}";
        }

        /// <summary>
        /// 情報属性をアセンブリから取得します。
        /// </summary>
        private static T? GetAttribute<T>(Assembly assembly)
            where T : Attribute
        {
            return assembly.GetCustomAttribute<T>();
        }

        /// <summary>
        /// 情報バージョンを優先して表示用バージョンを取得します。
        /// </summary>
        private static string GetVersion(Assembly assembly)
        {
            var informationalVersion = GetAttribute<AssemblyInformationalVersionAttribute>(assembly)
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

        /// <summary>
        /// バージョン情報画面を閉じます。
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        /// <summary>
        /// 画面を閉じる際、実行中の更新確認またはダウンロードへキャンセルを通知します。
        /// </summary>
        private void VersionInfo_Closed(object? sender, EventArgs e)
        {
            _updateCancellationTokenSource?.Cancel();
            _updateCancellationTokenSource?.Dispose();
            _updateCancellationTokenSource = null;
        }
    }
}
