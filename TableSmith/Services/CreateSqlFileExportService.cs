using System.IO;
using System.Text;
using TableSmith.Models;

namespace TableSmith.Services
{
    /// <summary>
    /// CREATE TABLE文をテーブルごとのSQLファイルとして出力します。
    /// </summary>
    public class CreateSqlFileExportService
    {
        private readonly CreateTableSqlService _createTableSqlService = new();

        /// <summary>
        /// 指定されたフォルダへ、1テーブルにつき1つのSQLファイルを出力します。
        /// </summary>
        /// <returns>出力したファイル数。</returns>
        public int Export(
            string outputDirectory,
            IEnumerable<TableDefinition> tables,
            SqlDialect dialect,
            DatabaseSettings? databaseSettings = null)
        {
            var tableList = tables.ToList();
            if (tableList.Count == 0)
            {
                throw new InvalidOperationException("CREATE文を出力するテーブルがありません。");
            }

            Directory.CreateDirectory(outputDirectory);
            var usedFileNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var table in tableList)
            {
                var fileName = CreateUniqueFileName(table.TableName, usedFileNames);
                var filePath = Path.Combine(outputDirectory, fileName);
                var sql = _createTableSqlService.Generate(table, dialect, databaseSettings);

                File.WriteAllText(
                    filePath,
                    sql,
                    ResolveEncoding(databaseSettings?.SqlFileEncoding ?? SqlFileEncoding.Utf8Bom));
            }

            return tableList.Count;
        }

        /// <summary>
        /// プロジェクト設定からSQLファイル出力用の文字コードを生成します。
        /// </summary>
        private static Encoding ResolveEncoding(SqlFileEncoding fileEncoding)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return fileEncoding switch
            {
                SqlFileEncoding.Utf8NoBom => new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
                SqlFileEncoding.ShiftJis => Encoding.GetEncoding(932),
                SqlFileEncoding.Utf16 => Encoding.Unicode,
                _ => new UTF8Encoding(encoderShouldEmitUTF8Identifier: true)
            };
        }

        /// <summary>
        /// OSで使用できない文字を置換し、重複しないSQLファイル名を作成します。
        /// </summary>
        private static string CreateUniqueFileName(string tableName, ISet<string> usedFileNames)
        {
            var invalidChars = Path.GetInvalidFileNameChars();
            var safeName = new string(tableName
                .Select(character => invalidChars.Contains(character) ? '_' : character)
                .ToArray())
                .Trim();

            if (string.IsNullOrWhiteSpace(safeName))
            {
                safeName = "table";
            }

            var candidate = $"{safeName}.sql";
            var suffixNumber = 2;
            while (usedFileNames.Contains(candidate))
            {
                candidate = $"{safeName}_{suffixNumber++}.sql";
            }

            usedFileNames.Add(candidate);
            return candidate;
        }
    }
}
