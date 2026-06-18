using ClosedXML.Excel;
using TableSmith.Models;

namespace TableSmith.Services
{
    /// <summary>
    /// テーブル定義をExcel形式のテーブル定義書として出力します。
    /// </summary>
    public class TableDefinitionExcelService
    {
        private static readonly string[] ColumnHeaders =
        {
            "No",
            "カラム物理名",
            "カラム論理名",
            "データ型",
            "サイズ",
            "PK",
            "FK",
            "Not Null",
            "既定値",
            "参照テーブル",
            "参照カラム",
            "説明"
        };

        /// <summary>
        /// 指定されたテーブル一覧を1つのExcelファイルへ出力します。
        /// </summary>
        public void Export(string filePath, string projectName, IEnumerable<TableDefinition> tables)
        {
            var tableList = tables.ToList();
            if (tableList.Count == 0)
            {
                throw new InvalidOperationException("出力するテーブルがありません。");
            }

            using var workbook = new XLWorkbook();
            AddTableListSheet(workbook, projectName, tableList);

            var usedSheetNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "テーブル一覧"
            };

            foreach (var table in tableList)
            {
                AddTableDefinitionSheet(workbook, table, usedSheetNames);
            }

            workbook.SaveAs(filePath);
        }

        /// <summary>
        /// 表紙を兼ねたテーブル一覧シートを作成します。
        /// </summary>
        private static void AddTableListSheet(
            XLWorkbook workbook,
            string projectName,
            IReadOnlyCollection<TableDefinition> tables)
        {
            var sheet = workbook.Worksheets.Add("テーブル一覧");

            var title = string.IsNullOrWhiteSpace(projectName)
                ? "TableSmith テーブル定義書"
                : $"{projectName} テーブル定義書";
            sheet.Cell("A1").Value = title;
            sheet.Range("A1:D1").Merge();
            sheet.Range("A1:D1").Style
                .Font.SetBold()
                .Font.SetFontSize(18)
                .Font.SetFontColor(XLColor.White);
            sheet.Range("A1:D1").Style.Fill.SetBackgroundColor(XLColor.FromHtml("#1F4E78"));
            sheet.Range("A1:D1").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            sheet.Cell("A3").Value = "プロジェクト名";
            sheet.Cell("B3").Value = projectName;
            sheet.Cell("A4").Value = "出力日時";
            sheet.Cell("B4").Value = DateTime.Now;
            sheet.Cell("B4").Style.DateFormat.Format = "yyyy/MM/dd HH:mm:ss";
            sheet.Cell("A5").Value = "テーブル数";
            sheet.Cell("B5").Value = tables.Count;

            var headerRow = 7;
            sheet.Cell(headerRow, 1).Value = "No";
            sheet.Cell(headerRow, 2).Value = "テーブル物理名";
            sheet.Cell(headerRow, 3).Value = "テーブル論理名";
            sheet.Cell(headerRow, 4).Value = "説明";
            ApplyHeaderStyle(sheet.Range(headerRow, 1, headerRow, 4));

            var row = headerRow + 1;
            var no = 1;
            foreach (var table in tables)
            {
                sheet.Cell(row, 1).Value = no++;
                sheet.Cell(row, 2).Value = table.TableName;
                sheet.Cell(row, 3).Value = table.TableDisplayName;
                sheet.Cell(row, 4).Value = table.Description;
                row++;
            }

            var dataRange = sheet.Range(headerRow, 1, row - 1, 4);
            ApplyTableBorder(dataRange);
            sheet.SheetView.FreezeRows(headerRow);
            sheet.Column(1).Width = 8;
            sheet.Column(2).Width = 28;
            sheet.Column(3).Width = 28;
            sheet.Column(4).Width = 60;
            sheet.Column(4).Style.Alignment.WrapText = true;
        }

        /// <summary>
        /// 1テーブル分の定義書シートを作成します。
        /// </summary>
        private static void AddTableDefinitionSheet(
            XLWorkbook workbook,
            TableDefinition table,
            ISet<string> usedSheetNames)
        {
            var sheetName = CreateUniqueSheetName(table.TableName, usedSheetNames);
            var sheet = workbook.Worksheets.Add(sheetName);

            sheet.Cell("A1").Value = "テーブル定義書";
            sheet.Range("A1:L1").Merge();
            sheet.Range("A1:L1").Style
                .Font.SetBold()
                .Font.SetFontSize(18)
                .Font.SetFontColor(XLColor.White);
            sheet.Range("A1:L1").Style.Fill.SetBackgroundColor(XLColor.FromHtml("#1F4E78"));
            sheet.Range("A1:L1").Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            sheet.Cell("A3").Value = "テーブル物理名";
            sheet.Cell("B3").Value = table.TableName;
            sheet.Cell("E3").Value = "テーブル論理名";
            sheet.Cell("F3").Value = table.TableDisplayName;
            sheet.Cell("A4").Value = "説明";
            sheet.Range("B4:L4").Merge();
            sheet.Cell("B4").Value = table.Description;
            sheet.Range("A3:A4").Style.Font.SetBold();
            sheet.Cell("E3").Style.Font.SetBold();
            sheet.Range("A3:L4").Style.Fill.SetBackgroundColor(XLColor.FromHtml("#D9EAF7"));
            ApplyTableBorder(sheet.Range("A3:L4"));

            var headerRow = 6;
            for (var index = 0; index < ColumnHeaders.Length; index++)
            {
                sheet.Cell(headerRow, index + 1).Value = ColumnHeaders[index];
            }
            ApplyHeaderStyle(sheet.Range(headerRow, 1, headerRow, ColumnHeaders.Length));

            var row = headerRow + 1;
            foreach (var column in table.Columns.OrderBy(column => column.No))
            {
                sheet.Cell(row, 1).Value = column.No;
                sheet.Cell(row, 2).Value = column.ColumnName;
                sheet.Cell(row, 3).Value = column.ColumnDisplayName;
                sheet.Cell(row, 4).Value = column.DataType;
                if (column.DataSize.HasValue)
                {
                    sheet.Cell(row, 5).Value = column.DataSize.Value;
                }
                sheet.Cell(row, 6).Value = column.IsPrimaryKey ? "○" : string.Empty;
                sheet.Cell(row, 7).Value = column.IsForeignKey ? "○" : string.Empty;
                sheet.Cell(row, 8).Value = column.IsNotNull ? "○" : string.Empty;
                sheet.Cell(row, 9).Value = column.DefaultValue;
                sheet.Cell(row, 10).Value = column.ReferenceTableName;
                sheet.Cell(row, 11).Value = column.ReferenceColumnName;
                sheet.Cell(row, 12).Value = column.Description;
                row++;
            }

            var lastRow = Math.Max(headerRow, row - 1);
            var dataRange = sheet.Range(headerRow, 1, lastRow, ColumnHeaders.Length);
            ApplyTableBorder(dataRange);
            dataRange.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Top);
            if (row > headerRow + 1)
            {
                sheet.Range(headerRow + 1, 6, row - 1, 8).Style.Alignment
                    .SetHorizontal(XLAlignmentHorizontalValues.Center);
            }

            sheet.SheetView.FreezeRows(headerRow);
            SetDefinitionColumnWidths(sheet);
        }

        /// <summary>
        /// Excelの制約を満たす重複しないシート名を作成します。
        /// </summary>
        private static string CreateUniqueSheetName(string tableName, ISet<string> usedSheetNames)
        {
            var invalidChars = new[] { ':', '\\', '/', '?', '*', '[', ']' };
            var sanitized = new string(tableName
                .Select(character => invalidChars.Contains(character) ? '_' : character)
                .ToArray())
                .Trim();

            if (string.IsNullOrWhiteSpace(sanitized))
            {
                sanitized = "テーブル";
            }

            sanitized = sanitized[..Math.Min(sanitized.Length, 31)];
            var candidate = sanitized;
            var suffixNumber = 2;

            while (usedSheetNames.Contains(candidate))
            {
                var suffix = $"_{suffixNumber++}";
                var baseLength = Math.Min(sanitized.Length, 31 - suffix.Length);
                candidate = sanitized[..baseLength] + suffix;
            }

            usedSheetNames.Add(candidate);
            return candidate;
        }

        /// <summary>
        /// 表のヘッダーへ共通スタイルを適用します。
        /// </summary>
        private static void ApplyHeaderStyle(IXLRange range)
        {
            range.Style.Font.SetBold();
            range.Style.Font.SetFontColor(XLColor.White);
            range.Style.Fill.SetBackgroundColor(XLColor.FromHtml("#4472C4"));
            range.Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);
            range.Style.Alignment.SetVertical(XLAlignmentVerticalValues.Center);
            range.Style.Alignment.WrapText = true;
            ApplyTableBorder(range);
        }

        /// <summary>
        /// 表へ細い罫線を適用します。
        /// </summary>
        private static void ApplyTableBorder(IXLRange range)
        {
            range.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
            range.Style.Border.OutsideBorderColor = XLColor.FromHtml("#A6A6A6");
            range.Style.Border.InsideBorderColor = XLColor.FromHtml("#D9D9D9");
        }

        /// <summary>
        /// テーブル定義シートの列幅と折り返しを設定します。
        /// </summary>
        private static void SetDefinitionColumnWidths(IXLWorksheet sheet)
        {
            var widths = new[] { 7d, 24d, 24d, 14d, 10d, 8d, 8d, 10d, 22d, 24d, 24d, 50d };
            for (var index = 0; index < widths.Length; index++)
            {
                sheet.Column(index + 1).Width = widths[index];
            }

            sheet.Columns(2, 12).Style.Alignment.WrapText = true;
        }
    }
}
