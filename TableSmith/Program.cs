using System.Configuration;
using System.Reflection;
using System.Windows;
using TableSmith.Common;
using Velopack;
using WpfApp1.Page;

namespace TableSmith
{
    /// <summary>
    /// 起動WPFアプリケーションのエントリポイント
    /// </summary>
    public class Program // クラス名を 'Main' から 'Program' に変更  
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));

        [STAThread]
        public static void Main()
        {
            // Velopackのインストール・更新・アンインストール時のフック処理を実行します。
            // WPFやログの初期化より前に呼び出し、更新処理時に不要な画面を生成しないようにします。
            VelopackApp.Build().Run();

            try
            {
                // log4netの初期化  
                log4net.Config.XmlConfigurator.Configure();

                log.Debug("===============================");
                log.Debug("       Application Start       ");
                log.Debug("===============================");

                // 実行モードの確認
                log.Debug(log.IsDebugEnabled ? "Debugモード" : "Releaseモード");

                // 実行環境情報のログ出力
                log.Debug($"OS Version: {Environment.OSVersion}");
                log.Debug($".NET Version: {Environment.Version}");
                log.Debug($"Machine Name: {Environment.MachineName}");
                log.Debug($"User Name: {Environment.UserName}");
                log.Debug($"Current Directory: {Environment.CurrentDirectory}");
                log.Debug($"Command Line: {Environment.CommandLine}");

                // アセンブリ情報のログ出力
                var assembly = Assembly.GetExecutingAssembly();
                log.Debug($"Assembly Full Name: {assembly.FullName}");
                log.Debug($"Assembly Location: {assembly.Location}");

                // アプリケーションインスタンスを作成  
                var app = new Application();
                // 任意のウィンドウを起動（MainWindow に限らなくてOK）  
                // フォームの選択処理を実装
                // 起動対象のウィンドウを取得
                string? targetWindow = ConfigurationManager.AppSettings["StartupWindow"]; ; // 例: MainWindow を起動する場合
                var form = new Form();
                if (string.IsNullOrEmpty(targetWindow))
                {
                    throw new ArgumentNullException("targetWindow", "起動対象のウィンドウが指定されていません。");
                }
                switch (targetWindow)
                {
                    case "MainWindow":
                        //form.mainWindowOpen();
                        app.Run(form.mainWindowOpen());
                        form.mainWindowClose();
                        break;
                    case "OtherWindow":
                        // 他のウィンドウを起動する場合の処理
                        break;
                    default:
                        // デフォルトのウィンドウを起動する場合の処理
                        break;
                }

                log.Debug("===============================");
                log.Debug("       Application End         ");
                log.Debug("===============================");

            }
            catch (Exception ex)
            {
                // エラーログを出力  
                SystemErrorDialog.Show(ex);
                //Dialog.Show(
                //    $"アプリケーションでエラーが発生しました\n{ex}",
                //    "エラー",
                //    EnumDialogButtons.Ok,
                //    EnumDialogType.Error);
                log.Error("===============================");
                log.Error("       Application Error       ");
                log.Error("===============================");
                log.Error("アプリケーションでエラーが発生しました", ex);
            }
        }
    }
}

