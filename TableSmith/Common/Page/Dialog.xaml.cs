using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using TableSmith.Common.Enum;
using static TableSmith.Common.Enum.CommonEnum;

namespace TableSmith.Page
{
    /// <summary>
    /// エラーダイアログを表示するためのクラスです。ボタン構成やタイトルなどを指定して呼び出すことができます。
    /// </summary>
    public partial class Dialog : Window
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
        public Dialog(string message, string title, EnumDialogButtons buttons, EnumDialogType enumDialogType)
        {
            this.InitializeComponent();
            this.DataContext = this;
            this.TitleMessage = title;
            this.WindowTitle = title;
            this.ErrorMessageBox.Text = message;
            this.CreateButtons(buttons, enumDialogType);
            this.Title = this.WindowTitle;
        }

        private void CreateButtons(EnumDialogButtons buttons, EnumDialogType enumDialogType)
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

            switch (buttons)
            {
                case EnumDialogButtons.Ok:
                    AddButton("OK", EnumDialogResult.Ok);
                    break;
                case EnumDialogButtons.OkCancel:
                    AddButton("OK", EnumDialogResult.Ok);
                    AddButton("キャンセル", EnumDialogResult.Cancel);
                    break;
                case EnumDialogButtons.YesNo:
                    AddButton("はい", EnumDialogResult.Yes);
                    AddButton("いいえ", EnumDialogResult.No);
                    break;
                case EnumDialogButtons.YesNoCancel:
                    AddButton("はい", EnumDialogResult.Yes);
                    AddButton("いいえ", EnumDialogResult.No);
                    AddButton("キャンセル", EnumDialogResult.Cancel);
                    break;
            }
            switch (enumDialogType)
            {
                case EnumDialogType.Information:
                    this.Background = System.Windows.Media.Brushes.LightBlue;
                    this.DialogImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/information.png"));
                    this.DialogBorder.BorderBrush = System.Windows.Media.Brushes.Aqua;
                    //WindowTitle = "情報";

                    break;
                case EnumDialogType.Warning:
                    this.Background = System.Windows.Media.Brushes.Yellow;
                    this.DialogImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/warning.png"));
                    this.DialogBorder.BorderBrush = System.Windows.Media.Brushes.Orange;
                    //WindowTitle = "警告";
                    break;
                case EnumDialogType.Error:
                    this.Background = System.Windows.Media.Brushes.Red;
                    this.DialogImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/Error.png"));
                    this.DialogBorder.BorderBrush = System.Windows.Media.Brushes.Red;
                    //WindowTitle = "エラー";
                    break;
                case EnumDialogType.Question:
                    this.Background = System.Windows.Media.Brushes.LightGreen;
                    this.DialogImage.Source = new BitmapImage(new Uri("pack://application:,,,/Resources/Images/question.png"));
                    this.DialogBorder.BorderBrush = System.Windows.Media.Brushes.Green;
                    //WindowTitle = "質問";
                    break;
            }
            this.Title = this.WindowTitle;


        }

        /// <summary>
        /// ダイアログを表示し、ユーザーの操作結果を返します。
        /// </summary>
        /// <returns>ユーザーが押したボタンに対応する <see cref="EnumDialogResult"/>。</returns>
        public static EnumDialogResult Show(string message, string title = "エラー", EnumDialogButtons buttons = EnumDialogButtons.Ok, EnumDialogType enumDialogType = EnumDialogType.None, Window? owner = null)
        {
            var dialog = new Dialog(message, title, buttons, enumDialogType);
            if (owner != null)
                dialog.Owner = owner;

            dialog.ShowDialog();
            return dialog._result;
        }
    }
}
