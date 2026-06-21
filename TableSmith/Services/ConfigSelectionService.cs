using System.Configuration;
using TableSmith.Models;

namespace TableSmith.Services
{
    /// <summary>
    /// App.configから画面の選択肢を読み込むサービスです。
    /// </summary>
    public static class ConfigSelectionService
    {
        private static readonly ConfigSelectionOption[] DefaultCharacterSets =
        {
            new() { Value = "utf8mb4", DisplayName = "UTF-8（utf8mb4）" },
            new() { Value = "utf8", DisplayName = "UTF-8（utf8）" },
            new() { Value = "latin1", DisplayName = "Latin-1" },
            new() { Value = "ujis", DisplayName = "EUC-JP" },
            new() { Value = "sjis", DisplayName = "Shift_JIS" }
        };

        private static readonly ConfigSelectionOption[] DefaultCollations =
        {
            new() { Value = "utf8mb4_ja_0900_as_cs", DisplayName = "日本語・大文字小文字区別" },
            new() { Value = "utf8mb4_ja_0900_as_ci", DisplayName = "日本語・大文字小文字無視" },
            new() { Value = "utf8mb4_unicode_ci", DisplayName = "Unicode・大文字小文字無視" },
            new() { Value = "utf8mb4_bin", DisplayName = "バイナリ比較" },
            new() { Value = "Japanese_CI_AS", DisplayName = "SQL Server 日本語・大文字小文字無視" },
            new() { Value = "Japanese_CS_AS", DisplayName = "SQL Server 日本語・大文字小文字区別" }
        };

        private static readonly SqlFileEncodingOption[] DefaultSqlFileEncodings =
        {
            new() { Value = SqlFileEncoding.Utf8Bom, DisplayName = "UTF-8（BOM付き）" },
            new() { Value = SqlFileEncoding.Utf8NoBom, DisplayName = "UTF-8（BOMなし）" },
            new() { Value = SqlFileEncoding.ShiftJis, DisplayName = "Shift_JIS" },
            new() { Value = SqlFileEncoding.Utf16, DisplayName = "UTF-16" }
        };

        /// <summary>
        /// DB文字コードの選択肢を取得します。
        /// </summary>
        public static IReadOnlyList<ConfigSelectionOption> GetCharacterSetOptions(
            string? currentValue = null,
            bool includeEmpty = false,
            string emptyDisplayName = "プロジェクト既定")
        {
            return BuildStringOptions(
                "CharacterSetOptions",
                DefaultCharacterSets,
                currentValue,
                includeEmpty,
                emptyDisplayName);
        }

        /// <summary>
        /// 照合順序の選択肢を取得します。
        /// </summary>
        public static IReadOnlyList<ConfigSelectionOption> GetCollationOptions(
            string? currentValue = null,
            bool includeEmpty = false,
            string emptyDisplayName = "プロジェクト既定")
        {
            return BuildStringOptions(
                "CollationOptions",
                DefaultCollations,
                currentValue,
                includeEmpty,
                emptyDisplayName);
        }

        /// <summary>
        /// SQLファイル文字コードの選択肢を取得します。
        /// </summary>
        public static IReadOnlyList<SqlFileEncodingOption> GetSqlFileEncodingOptions(
            SqlFileEncoding currentValue)
        {
            var configured = ParsePairs(ConfigurationManager.AppSettings["SqlFileEncodingOptions"])
                .Select(item => Enum.TryParse<SqlFileEncoding>(item.Value, true, out var encoding)
                    ? new SqlFileEncodingOption { Value = encoding, DisplayName = item.DisplayName }
                    : null)
                .Where(item => item != null)
                .Cast<SqlFileEncodingOption>()
                .GroupBy(item => item.Value)
                .Select(group => group.First())
                .ToList();

            if (configured.Count == 0)
            {
                configured.AddRange(DefaultSqlFileEncodings);
            }

            // コンフィグから削除された値でも、プロジェクトJSONの保存値を正として表示します。
            if (configured.All(item => item.Value != currentValue))
            {
                configured.Add(new SqlFileEncodingOption
                {
                    Value = currentValue,
                    DisplayName = "保存済み設定"
                });
            }

            return configured;
        }

        private static IReadOnlyList<ConfigSelectionOption> BuildStringOptions(
            string configKey,
            IEnumerable<ConfigSelectionOption> fallback,
            string? currentValue,
            bool includeEmpty,
            string emptyDisplayName)
        {
            var options = ParsePairs(ConfigurationManager.AppSettings[configKey]).ToList();
            if (options.Count == 0)
            {
                options.AddRange(fallback);
            }

            options = options
                .Where(item => !string.IsNullOrWhiteSpace(item.Value))
                .GroupBy(item => item.Value, StringComparer.OrdinalIgnoreCase)
                .Select(group => group.First())
                .ToList();

            if (includeEmpty)
            {
                options.Insert(0, new ConfigSelectionOption
                {
                    Value = string.Empty,
                    DisplayName = emptyDisplayName
                });
            }

            // ComboBoxのSelectedValueは文字列の表記差も区別するため、完全一致しない保存値を追加します。
            // これにより、コンフィグ未登録値や大文字小文字が異なる値もJSONどおり選択表示されます。
            if (!string.IsNullOrWhiteSpace(currentValue)
                && options.All(item =>
                    !item.Value.Equals(currentValue, StringComparison.Ordinal)))
            {
                options.Add(new ConfigSelectionOption
                {
                    Value = currentValue,
                    DisplayName = "保存済み設定"
                });
            }

            return options;
        }

        /// <summary>
        /// 「内部値|表示名;...」形式の設定文字列を選択肢へ変換します。
        /// </summary>
        private static IEnumerable<ConfigSelectionOption> ParsePairs(string? configuredValue)
        {
            if (string.IsNullOrWhiteSpace(configuredValue))
            {
                yield break;
            }

            foreach (var entry in configuredValue.Split(
                         ';',
                         StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var pair = entry.Split('|', 2, StringSplitOptions.TrimEntries);
                if (pair.Length == 0 || string.IsNullOrWhiteSpace(pair[0]))
                {
                    continue;
                }

                yield return new ConfigSelectionOption
                {
                    Value = pair[0],
                    DisplayName = pair.Length > 1 && !string.IsNullOrWhiteSpace(pair[1])
                        ? pair[1]
                        : pair[0]
                };
            }
        }
    }
}
