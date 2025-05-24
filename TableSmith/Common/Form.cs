using MahApps.Metro.Controls;
using System.Windows;
using TableSmith;
using TableSmith.Views; // WPF 名前空間を追加

namespace TableSmith.Common
{
    internal class Form
    {

        /// <summary>
        /// MainWindowのインスタンス
        /// </summary>
        private MainWindow? _mainWindow { get; set; }
        public MetroWindow mainWindowOpen()
        {
            this._mainWindow = new MainWindow();
            return this._mainWindow;
        }
        public void mainWindowClose()
        {
            if (this._mainWindow != null)
            {
                this._mainWindow.Close();
                this._mainWindow = null;
            }
        }

    }
}
