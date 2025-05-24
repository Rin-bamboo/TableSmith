using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TableSmith.Common.Enum;
using WpfAnimatedGif;
using static TableSmith.Common.Enum.CommonEnum;

namespace WpfApp1.Page
{
    /// <summary>
    /// エラーダイアログを表示するためのクラスです。ボタン構成やタイトルなどを指定して呼び出すことができます。
    /// </summary>
    public partial class SystemErrorDialog : Window
    {
        public string TitleMessage { get; set; }
        public string WindowTitle { get; set; }
        private EnumDialogResult _result = CommonEnum.EnumDialogResult.None;

        /// <summary>
        /// 指定されたメッセージ・タイトル・ボタン構成でエラーダイアログを生成します。
        /// </summary>
        /// <param name="message">ダイアログに表示するエラーメッセージ。</param>
        /// <param name="title">ウィンドウのタイトルバーに表示する文字列</param>
        /// <param name="buttons">表示ボタンの種類（OK,キャンセル/OK、はい/いいえなど</param>
        /// <param name="enumDialogType">表示するダイアログの種類　情報・警告・エラー・質問等</param>
        public SystemErrorDialog(int errorCode, string message)
        {
            this.InitializeComponent();
            this.DataContext = this;
            string title = "システムエラー";
            this.TitleMessage = title;
            this.WindowTitle = title;
            this.ErrorMessageBox.Text = message;
            this.ErrorCodeTextBlock.Text = $"エラー番号: 0x{errorCode:X8} ({errorCode})";
            this.CreateButtons();
            this.Title = this.WindowTitle;
        }

        private void CreateButtons()
        {
            void AddButton(string label, EnumDialogResult result)
            {
                var btn = new Button
                {
                    Content = label,
                    Width = 80,
                    Margin = new Thickness(5),
                    Tag = result
                };
                btn.Click += (s, e) =>
                {
                    this._result = result;
                    this.Close();
                };
                this.ButtonPanel.Children.Add(btn);
            }
            AddButton("OK", EnumDialogResult.Ok);
            var imageUri = new Uri("pack://application:,,,/Resources/Images/ErrorAnimation.gif");
            var image = new BitmapImage(imageUri);
            ImageBehavior.SetAnimatedSource(this.errorGifImage, image);
            this.Title = this.WindowTitle;


        }

        /// <summary>
        /// ダイアログを表示し、ユーザーの操作結果を返します。
        /// </summary>
        /// <returns>ユーザーが押したボタンに対応する <see cref="EnumDialogResult"/>。</returns>
        public static void Show(Exception ex, Window? owner = null)
        {
            var dialog = new SystemErrorDialog(ex.HResult, ex.Message);
            if (owner != null)
                dialog.Owner = owner;

            dialog.ShowDialog();
        }
    }
}
