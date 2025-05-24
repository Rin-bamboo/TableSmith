using System.ComponentModel;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using TableSmith.Models;
using Utility.Util;
using TableSmith.Common.Enum;
using TableSmith.Page;
using static TableSmith.Common.Enum.CommonEnum;
using MahApps.Metro.Controls;

namespace TableSmith.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow // Correctly inherit from MetroWindow instead of MahApps.Metro
    {
        public TableDefinition CurrentTable { get; set; } = new TableDefinition
        {
            TableName = string.Empty,
            TableDisplayName = string.Empty,
            Columns = new List<ColumnDefinition>()
        };

        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }

        private void ButtonTableCreate_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to TableCreatePage
            //var page = new TableCreatePage(CurrentTable);
            //this.Content = page;
            MessageBox.Show("Table Create button clicked!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void ButtonTableList_Click(object sender, RoutedEventArgs e)
        {
            // Navigate to TableCreatePage
            //var page = new TableCreatePage(CurrentTable);
            //this.Content = page;
            MessageBox.Show("Table List button clicked!", "Info", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        //private void Save()
        //{
        //    string json = JsonSerializer.Serialize(CurrentTable, new JsonSerializerOptions { WriteIndented = true });
        //    File.WriteAllText("table_definition.json", json);
        //}

        //private void Load()
        //{
        //    if (File.Exists("table_definition.json"))
        //    {
        //        string json = File.ReadAllText("table_definition.json");
        //        CurrentTable = JsonSerializer.Deserialize<TableDefinition>(json) ?? new TableDefinition
        //        {
        //            TableName = string.Empty,
        //            TableDisplayName = string.Empty,
        //            Columns = new List<ColumnDefinition>()
        //        };
        //        OnPropertyChanged(nameof(CurrentTable));
        //    }
        //}
    }
}