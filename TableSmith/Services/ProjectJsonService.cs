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
            return JsonSerializer.Deserialize<TableSmithProject>(json, JsonOptions) ?? new TableSmithProject();
        }
    }
}
