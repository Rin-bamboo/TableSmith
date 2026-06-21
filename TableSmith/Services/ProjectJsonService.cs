using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using TableSmith.Models;

namespace TableSmith.Services
{
    /// <summary>
    /// TableSmithプロジェクトをJSONファイルとして保存・読み込みするサービスです。
    /// </summary>
    public class ProjectJsonService
    {
        private readonly ProjectMigrationService _migrationService = new();

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        /// <summary>
        /// 指定されたファイルパスへプロジェクト情報を書き込みます。
        /// </summary>
        public void Save(string filePath, TableSmithProject project)
        {
            _migrationService.Migrate(project);
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(project, JsonOptions);
            File.WriteAllText(filePath, json);
        }

        /// <summary>
        /// 指定されたJSONファイルからプロジェクト情報を読み込みます。
        /// </summary>
        public TableSmithProject Load(string filePath)
        {
            var json = File.ReadAllText(filePath);
            var project = JsonSerializer.Deserialize<TableSmithProject>(json, JsonOptions)
                ?? throw new InvalidDataException("プロジェクトJSONを読み込めませんでした。");
            return _migrationService.Migrate(project);
        }
    }
}
