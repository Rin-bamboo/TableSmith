using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using MahApps.Metro.Controls;

namespace TableSmith.Views
{
    /// <summary>
    /// アプリケーションの製品情報と実行環境を表示する画面です。
    /// </summary>
    public partial class VersionInfo : MetroWindow
    {
        public string ProductName { get; }
        public string VersionText { get; }
        public string Description { get; }
        public string AssemblyName { get; }
        public string RuntimeText { get; }
        public string Copyright { get; }

        public VersionInfo()
        {
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
    }
}
