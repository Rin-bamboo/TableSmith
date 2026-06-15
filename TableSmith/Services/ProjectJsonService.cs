using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using TableSmith.Models;

namespace TableSmith.Services
{
    public class ProjectJsonService
    {
        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

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

        public TableSmithProject Load(string filePath)
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<TableSmithProject>(json, JsonOptions) ?? new TableSmithProject();
        }
    }
}
