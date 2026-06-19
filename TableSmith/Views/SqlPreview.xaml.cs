using System.Windows;
using MahApps.Metro.Controls;

namespace TableSmith.Views
{
    /// <summary>
    /// 生成したCREATE TABLE文を表示する画面です。
    /// </summary>
    public partial class SqlPreview : MetroWindow
    {
        public string SqlText { get; }

        public SqlPreview(string sqlText, string? windowTitle = null)
        {
            InitializeComponent();
            this.SqlText = sqlText;
            if (!string.IsNullOrWhiteSpace(windowTitle))
            {
                this.Title = windowTitle;
            }
            this.DataContext = this;
        }

        /// <summary>
        /// SQLプレビュー画面を閉じます。
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
