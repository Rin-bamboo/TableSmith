using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using MahApps.Metro.Controls;
using Microsoft.Win32;
using TableSmith.Models;
using TableSmith.Services;

namespace TableSmith.Views
{
    /// <summary>
    /// ER図をプレビューし、SVGまたはPNGへ保存する画面です。
    /// </summary>
    public partial class ErDiagramPreview : MetroWindow
    {
        private readonly ErDiagramService _erDiagramService = new();
        private readonly string _projectName;
        private readonly ObservableCollection<TableDefinition> _tables;

        public BitmapSource PreviewImage { get; }

        public ErDiagramPreview(string projectName, ObservableCollection<TableDefinition> tables)
        {
            this._projectName = projectName;
            this._tables = tables;
            this.PreviewImage = _erDiagramService.CreatePreview(projectName, tables);

            InitializeComponent();
            this.DataContext = this;
        }

        /// <summary>
        /// スライダーの値に合わせてプレビューの表示倍率を変更します。
        /// </summary>
        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (this.DiagramScaleTransform == null)
            {
                return;
            }

            var scale = e.NewValue / 100d;
            this.DiagramScaleTransform.ScaleX = scale;
            this.DiagramScaleTransform.ScaleY = scale;
        }

        /// <summary>
        /// ER図をSVG形式で保存します。
        /// </summary>
        private void SaveSvgButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "ER図をSVG形式で保存",
                Filter = "SVG Image (*.svg)|*.svg",
                FileName = $"{CreateSafeFileName(this._projectName)}_ER図.svg",
                AddExtension = true,
                DefaultExt = ".svg"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            ExecuteExport(
                () => _erDiagramService.ExportSvg(dialog.FileName, this._projectName, this._tables),
                "ER図をSVG形式で保存しました。");
        }

        /// <summary>
        /// ER図をPNG形式で保存します。
        /// </summary>
        private void SavePngButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "ER図をPNG形式で保存",
                Filter = "PNG Image (*.png)|*.png",
                FileName = $"{CreateSafeFileName(this._projectName)}_ER図.png",
                AddExtension = true,
                DefaultExt = ".png"
            };

            if (dialog.ShowDialog(this) != true)
            {
                return;
            }

            ExecuteExport(
                () => _erDiagramService.ExportPng(dialog.FileName, this._projectName, this._tables),
                "ER図をPNG形式で保存しました。");
        }

        /// <summary>
        /// 出力処理を実行し、成功またはエラーのメッセージを表示します。
        /// </summary>
        private void ExecuteExport(Action exportAction, string successMessage)
        {
            try
            {
                exportAction();
                MessageBox.Show(successMessage, "Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"ER図の保存に失敗しました。\n{ex.Message}", "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// プロジェクト名をファイル名として利用できる文字列へ変換します。
        /// </summary>
        private static string CreateSafeFileName(string projectName)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var safeName = new string(projectName
                .Select(character => invalidChars.Contains(character) ? '_' : character)
                .ToArray())
                .Trim();

            return string.IsNullOrWhiteSpace(safeName) ? "TableSmith" : safeName;
        }

        /// <summary>
        /// ER図プレビュー画面を閉じます。
        /// </summary>
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
