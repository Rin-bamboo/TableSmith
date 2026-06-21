using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using TableSmith.Models;

namespace TableSmith.Services
{
    /// <summary>
    /// テーブル定義からER図を生成し、プレビュー・SVG・PNGとして出力します。
    /// </summary>
    public class ErDiagramService
    {
        private const double TableWidth = 320;
        private const double HeaderHeight = 58;
        private const double RowHeight = 25;
        private const double PageMargin = 60;
        private const double HorizontalGap = 120;
        private const double VerticalGap = 90;

        /// <summary>
        /// ER図のプレビュー用画像を生成します。
        /// </summary>
        public BitmapSource CreatePreview(string projectName, IEnumerable<TableDefinition> tables)
        {
            var layout = BuildLayout(tables);
            var visual = CreateDrawingVisual(projectName, layout);
            return RenderBitmap(visual, layout.Width, layout.Height);
        }

        /// <summary>
        /// ER図をPNGファイルへ出力します。
        /// </summary>
        public void ExportPng(string filePath, string projectName, IEnumerable<TableDefinition> tables)
        {
            var layout = BuildLayout(tables);
            var visual = CreateDrawingVisual(projectName, layout);
            var bitmap = RenderBitmap(visual, layout.Width, layout.Height);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = File.Create(filePath);
            encoder.Save(stream);
        }

        /// <summary>
        /// ER図を拡大しても劣化しないSVGファイルへ出力します。
        /// </summary>
        public void ExportSvg(string filePath, string projectName, IEnumerable<TableDefinition> tables)
        {
            var layout = BuildLayout(tables);
            File.WriteAllText(filePath, CreateSvg(projectName, layout), Encoding.UTF8);
        }

        /// <summary>
        /// テーブル数と各カラム数からER図内の配置を計算します。
        /// </summary>
        private static ErDiagramLayout BuildLayout(IEnumerable<TableDefinition> tables)
        {
            var tableList = tables.ToList();
            if (tableList.Count == 0)
            {
                throw new InvalidOperationException("ER図を作成するテーブルがありません。");
            }

            var columnCount = Math.Max(1, (int)Math.Ceiling(Math.Sqrt(tableList.Count)));
            var rowHeights = new Dictionary<int, double>();
            var positions = new List<ErTableLayout>();

            for (var index = 0; index < tableList.Count; index++)
            {
                var row = index / columnCount;
                var tableHeight = HeaderHeight + Math.Max(1, tableList[index].Columns.Count) * RowHeight;
                rowHeights[row] = Math.Max(rowHeights.GetValueOrDefault(row), tableHeight);
            }

            var rowOffsets = new Dictionary<int, double>();
            var currentY = PageMargin + 50;
            foreach (var row in rowHeights.Keys.OrderBy(value => value))
            {
                rowOffsets[row] = currentY;
                currentY += rowHeights[row] + VerticalGap;
            }

            for (var index = 0; index < tableList.Count; index++)
            {
                var row = index / columnCount;
                var column = index % columnCount;
                var table = tableList[index];
                var tableHeight = HeaderHeight + Math.Max(1, table.Columns.Count) * RowHeight;

                positions.Add(new ErTableLayout(
                    table,
                    PageMargin + column * (TableWidth + HorizontalGap),
                    rowOffsets[row],
                    TableWidth,
                    tableHeight));
            }

            var width = PageMargin * 2 + columnCount * TableWidth + Math.Max(0, columnCount - 1) * HorizontalGap;
            var diagramHeight = currentY - VerticalGap + PageMargin;
            return new ErDiagramLayout(positions, width, diagramHeight);
        }

        /// <summary>
        /// WPF描画命令としてER図を生成します。
        /// </summary>
        private static DrawingVisual CreateDrawingVisual(string projectName, ErDiagramLayout layout)
        {
            var visual = new DrawingVisual();
            using var drawing = visual.RenderOpen();

            drawing.DrawRectangle(Brushes.White, null, new Rect(0, 0, layout.Width, layout.Height));
            DrawText(
                drawing,
                string.IsNullOrWhiteSpace(projectName) ? "ER Diagram" : $"{projectName} ER Diagram",
                new Point(PageMargin, 18),
                22,
                Brushes.Black,
                FontWeights.Bold,
                layout.Width - PageMargin * 2);

            DrawRelationships(drawing, layout);
            foreach (var table in layout.Tables)
            {
                DrawTable(drawing, table);
            }

            return visual;
        }

        /// <summary>
        /// FKカラムから参照先PKカラムへリレーション線を描画します。
        /// </summary>
        private static void DrawRelationships(DrawingContext drawing, ErDiagramLayout layout)
        {
            var tableMap = layout.Tables.ToDictionary(
                item => item.Table.TableName,
                StringComparer.OrdinalIgnoreCase);
            var relationPen = new Pen(new SolidColorBrush(Color.FromRgb(55, 96, 135)), 1.8);

            foreach (var sourceTable in layout.Tables)
            {
                foreach (var foreignKey in sourceTable.Table.Columns.Where(column => column.IsForeignKey))
                {
                    if (!tableMap.TryGetValue(foreignKey.ReferenceTableName, out var targetTable))
                    {
                        continue;
                    }

                    var sourceRow = GetColumnIndex(sourceTable.Table, foreignKey.ColumnName);
                    var targetRow = GetColumnIndex(targetTable.Table, foreignKey.ReferenceColumnName);
                    if (sourceRow < 0 || targetRow < 0)
                    {
                        continue;
                    }

                    var sourceOnRight = targetTable.X >= sourceTable.X;
                    var start = new Point(
                        sourceOnRight ? sourceTable.X + sourceTable.Width : sourceTable.X,
                        sourceTable.Y + HeaderHeight + sourceRow * RowHeight + RowHeight / 2);
                    var end = new Point(
                        sourceOnRight ? targetTable.X : targetTable.X + targetTable.Width,
                        targetTable.Y + HeaderHeight + targetRow * RowHeight + RowHeight / 2);
                    var middleX = (start.X + end.X) / 2;

                    var geometry = new StreamGeometry();
                    using (var context = geometry.Open())
                    {
                        context.BeginFigure(start, false, false);
                        context.PolyLineTo(
                            new[]
                            {
                                new Point(middleX, start.Y),
                                new Point(middleX, end.Y),
                                end
                            },
                            true,
                            false);
                    }

                    drawing.DrawGeometry(null, relationPen, geometry);
                    DrawArrow(drawing, end, sourceOnRight, relationPen.Brush);

                    if (!foreignKey.IsNotNull)
                    {
                        drawing.DrawEllipse(
                            Brushes.White,
                            relationPen,
                            new Point(start.X + (sourceOnRight ? 8 : -8), start.Y),
                            4,
                            4);
                    }
                }
            }
        }

        /// <summary>
        /// テーブル名とカラム一覧をボックスとして描画します。
        /// </summary>
        private static void DrawTable(DrawingContext drawing, ErTableLayout layout)
        {
            var borderPen = new Pen(new SolidColorBrush(Color.FromRgb(70, 91, 112)), 1.3);
            var headerBrush = new SolidColorBrush(Color.FromRgb(31, 78, 121));
            var alternateBrush = new SolidColorBrush(Color.FromRgb(238, 244, 251));

            drawing.DrawRoundedRectangle(
                Brushes.White,
                borderPen,
                new Rect(layout.X, layout.Y, layout.Width, layout.Height),
                4,
                4);
            drawing.DrawRectangle(
                headerBrush,
                null,
                new Rect(layout.X, layout.Y, layout.Width, HeaderHeight));

            DrawText(
                drawing,
                layout.Table.TableDisplayName,
                new Point(layout.X + 12, layout.Y + 8),
                14,
                Brushes.White,
                FontWeights.Bold);
            DrawText(
                drawing,
                layout.Table.TableName,
                new Point(layout.X + 12, layout.Y + 31),
                11,
                new SolidColorBrush(Color.FromRgb(220, 234, 247)),
                FontWeights.Normal);

            var columns = layout.Table.Columns.OrderBy(column => column.No).ToList();
            if (columns.Count == 0)
            {
                DrawText(drawing, "(カラムなし)", new Point(layout.X + 12, layout.Y + HeaderHeight + 5), 11, Brushes.Gray, FontWeights.Normal);
                return;
            }

            for (var index = 0; index < columns.Count; index++)
            {
                var column = columns[index];
                var rowY = layout.Y + HeaderHeight + index * RowHeight;
                if (index % 2 == 1)
                {
                    drawing.DrawRectangle(
                        alternateBrush,
                        null,
                        new Rect(layout.X + 1, rowY, layout.Width - 2, RowHeight));
                }

                drawing.DrawLine(
                    new Pen(new SolidColorBrush(Color.FromRgb(220, 226, 232)), 0.7),
                    new Point(layout.X, rowY),
                    new Point(layout.X + layout.Width, rowY));

                var keyLabel = column.IsPrimaryKey ? "PK" : column.IsForeignKey ? "FK" : string.Empty;
                var typeLabel = BuildTypeLabel(column);

                DrawText(drawing, keyLabel, new Point(layout.X + 8, rowY + 5), 10, new SolidColorBrush(Color.FromRgb(190, 62, 62)), FontWeights.Bold);
                DrawText(drawing, column.ColumnName, new Point(layout.X + 40, rowY + 5), 10.5, Brushes.Black, FontWeights.Normal);
                DrawText(drawing, typeLabel, new Point(layout.X + 205, rowY + 5), 9.5, Brushes.DimGray, FontWeights.Normal);
            }
        }

        /// <summary>
        /// リレーション線の参照先へ矢印を描画します。
        /// </summary>
        private static void DrawArrow(DrawingContext drawing, Point end, bool pointsRight, Brush brush)
        {
            var direction = pointsRight ? 1 : -1;
            var geometry = new StreamGeometry();
            using var context = geometry.Open();
            context.BeginFigure(end, true, true);
            context.LineTo(new Point(end.X - direction * 9, end.Y - 5), true, false);
            context.LineTo(new Point(end.X - direction * 9, end.Y + 5), true, false);
            drawing.DrawGeometry(brush, null, geometry);
        }

        /// <summary>
        /// テーブル内で指定カラムが何行目かを取得します。
        /// </summary>
        private static int GetColumnIndex(TableDefinition table, string columnName)
        {
            return table.Columns
                .OrderBy(column => column.No)
                .Select((column, index) => new { column, index })
                .FirstOrDefault(item => item.column.ColumnName.Equals(columnName, StringComparison.OrdinalIgnoreCase))
                ?.index ?? -1;
        }

        /// <summary>
        /// WPFの文字描画を共通化します。
        /// </summary>
        private static void DrawText(
            DrawingContext drawing,
            string text,
            Point point,
            double fontSize,
            Brush brush,
            FontWeight fontWeight,
            double maxTextWidth = TableWidth - 24)
        {
            var formattedText = new FormattedText(
                text ?? string.Empty,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, fontWeight, FontStretches.Normal),
                fontSize,
                brush,
                1.0)
            {
                MaxTextWidth = maxTextWidth,
                Trimming = TextTrimming.CharacterEllipsis
            };

            drawing.DrawText(formattedText, point);
        }

        /// <summary>
        /// DrawingVisualをPNG出力可能なBitmapSourceへ変換します。
        /// </summary>
        private static BitmapSource RenderBitmap(DrawingVisual visual, double width, double height)
        {
            var pixelWidth = Math.Max(1, (int)Math.Ceiling(width));
            var pixelHeight = Math.Max(1, (int)Math.Ceiling(height));
            var bitmap = new RenderTargetBitmap(pixelWidth, pixelHeight, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);
            bitmap.Freeze();
            return bitmap;
        }

        /// <summary>
        /// ER図をSVG文字列として生成します。
        /// </summary>
        private static string CreateSvg(string projectName, ErDiagramLayout layout)
        {
            var builder = new StringBuilder();
            builder.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            builder.AppendLine($"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"{layout.Width:0}\" height=\"{layout.Height:0}\" viewBox=\"0 0 {layout.Width:0} {layout.Height:0}\">");
            builder.AppendLine("<rect width=\"100%\" height=\"100%\" fill=\"#ffffff\"/>");
            builder.AppendLine($"<text x=\"{PageMargin}\" y=\"38\" font-family=\"Segoe UI, sans-serif\" font-size=\"22\" font-weight=\"700\">{EscapeXml(string.IsNullOrWhiteSpace(projectName) ? "ER Diagram" : $"{projectName} ER Diagram")}</text>");
            builder.AppendLine("<defs><marker id=\"arrow\" viewBox=\"0 0 10 10\" refX=\"9\" refY=\"5\" markerWidth=\"7\" markerHeight=\"7\" orient=\"auto-start-reverse\"><path d=\"M 0 0 L 10 5 L 0 10 z\" fill=\"#376087\"/></marker></defs>");

            AppendSvgRelationships(builder, layout);
            foreach (var table in layout.Tables)
            {
                AppendSvgTable(builder, table);
            }

            builder.AppendLine("</svg>");
            return builder.ToString();
        }

        /// <summary>
        /// SVGへリレーション線を追加します。
        /// </summary>
        private static void AppendSvgRelationships(StringBuilder builder, ErDiagramLayout layout)
        {
            var tableMap = layout.Tables.ToDictionary(item => item.Table.TableName, StringComparer.OrdinalIgnoreCase);
            foreach (var sourceTable in layout.Tables)
            {
                foreach (var foreignKey in sourceTable.Table.Columns.Where(column => column.IsForeignKey))
                {
                    if (!tableMap.TryGetValue(foreignKey.ReferenceTableName, out var targetTable))
                    {
                        continue;
                    }

                    var sourceRow = GetColumnIndex(sourceTable.Table, foreignKey.ColumnName);
                    var targetRow = GetColumnIndex(targetTable.Table, foreignKey.ReferenceColumnName);
                    if (sourceRow < 0 || targetRow < 0)
                    {
                        continue;
                    }

                    var sourceOnRight = targetTable.X >= sourceTable.X;
                    var startX = sourceOnRight ? sourceTable.X + sourceTable.Width : sourceTable.X;
                    var startY = sourceTable.Y + HeaderHeight + sourceRow * RowHeight + RowHeight / 2;
                    var endX = sourceOnRight ? targetTable.X : targetTable.X + targetTable.Width;
                    var endY = targetTable.Y + HeaderHeight + targetRow * RowHeight + RowHeight / 2;
                    var middleX = (startX + endX) / 2;

                    builder.AppendLine($"<polyline points=\"{startX:0},{startY:0} {middleX:0},{startY:0} {middleX:0},{endY:0} {endX:0},{endY:0}\" fill=\"none\" stroke=\"#376087\" stroke-width=\"2\" marker-end=\"url(#arrow)\"/>");
                    if (!foreignKey.IsNotNull)
                    {
                        var circleX = startX + (sourceOnRight ? 8 : -8);
                        builder.AppendLine($"<circle cx=\"{circleX:0}\" cy=\"{startY:0}\" r=\"4\" fill=\"#ffffff\" stroke=\"#376087\" stroke-width=\"2\"/>");
                    }
                }
            }
        }

        /// <summary>
        /// SVGへテーブルボックスを追加します。
        /// </summary>
        private static void AppendSvgTable(StringBuilder builder, ErTableLayout layout)
        {
            builder.AppendLine($"<rect x=\"{layout.X:0}\" y=\"{layout.Y:0}\" width=\"{layout.Width:0}\" height=\"{layout.Height:0}\" rx=\"4\" fill=\"#ffffff\" stroke=\"#465b70\" stroke-width=\"1.5\"/>");
            builder.AppendLine($"<path d=\"M {layout.X + 4:0} {layout.Y:0} H {layout.X + layout.Width - 4:0} Q {layout.X + layout.Width:0} {layout.Y:0} {layout.X + layout.Width:0} {layout.Y + 4:0} V {layout.Y + HeaderHeight:0} H {layout.X:0} V {layout.Y + 4:0} Q {layout.X:0} {layout.Y:0} {layout.X + 4:0} {layout.Y:0} Z\" fill=\"#1f4e79\"/>");
            builder.AppendLine($"<text x=\"{layout.X + 12:0}\" y=\"{layout.Y + 23:0}\" font-family=\"Segoe UI, sans-serif\" font-size=\"14\" font-weight=\"700\" fill=\"#ffffff\">{EscapeXml(layout.Table.TableDisplayName)}</text>");
            builder.AppendLine($"<text x=\"{layout.X + 12:0}\" y=\"{layout.Y + 44:0}\" font-family=\"Segoe UI, sans-serif\" font-size=\"11\" fill=\"#dceaf7\">{EscapeXml(layout.Table.TableName)}</text>");

            var columns = layout.Table.Columns.OrderBy(column => column.No).ToList();
            if (columns.Count == 0)
            {
                builder.AppendLine($"<text x=\"{layout.X + 12:0}\" y=\"{layout.Y + HeaderHeight + 17:0}\" font-family=\"Segoe UI, sans-serif\" font-size=\"11\" fill=\"#777777\">(カラムなし)</text>");
                return;
            }

            for (var index = 0; index < columns.Count; index++)
            {
                var column = columns[index];
                var rowY = layout.Y + HeaderHeight + index * RowHeight;
                if (index % 2 == 1)
                {
                    builder.AppendLine($"<rect x=\"{layout.X + 1:0}\" y=\"{rowY:0}\" width=\"{layout.Width - 2:0}\" height=\"{RowHeight:0}\" fill=\"#eef4fb\"/>");
                }
                builder.AppendLine($"<line x1=\"{layout.X:0}\" y1=\"{rowY:0}\" x2=\"{layout.X + layout.Width:0}\" y2=\"{rowY:0}\" stroke=\"#dce2e8\" stroke-width=\"0.7\"/>");

                var keyLabel = column.IsPrimaryKey ? "PK" : column.IsForeignKey ? "FK" : string.Empty;
                var typeLabel = BuildTypeLabel(column);
                builder.AppendLine($"<text x=\"{layout.X + 8:0}\" y=\"{rowY + 17:0}\" font-family=\"Segoe UI, sans-serif\" font-size=\"10\" font-weight=\"700\" fill=\"#be3e3e\">{keyLabel}</text>");
                builder.AppendLine($"<text x=\"{layout.X + 40:0}\" y=\"{rowY + 17:0}\" font-family=\"Segoe UI, sans-serif\" font-size=\"10.5\" fill=\"#111111\">{EscapeXml(column.ColumnName)}</text>");
                builder.AppendLine($"<text x=\"{layout.X + 205:0}\" y=\"{rowY + 17:0}\" font-family=\"Segoe UI, sans-serif\" font-size=\"9.5\" fill=\"#555555\">{EscapeXml(typeLabel)}</text>");
            }
        }

        private static string EscapeXml(string value)
        {
            return WebUtility.HtmlEncode(value ?? string.Empty);
        }

        /// <summary>
        /// ER図表示用にdecimalの精度・小数桁数を含む型名を作成します。
        /// </summary>
        private static string BuildTypeLabel(ColumnDefinition column)
        {
            if (column.DataType.Equals("decimal", StringComparison.OrdinalIgnoreCase)
                && column.Precision.HasValue)
            {
                return column.Scale.HasValue
                    ? $"{column.DataType}({column.Precision.Value},{column.Scale.Value})"
                    : $"{column.DataType}({column.Precision.Value})";
            }

            return column.DataSize.HasValue
                ? $"{column.DataType}({column.DataSize.Value})"
                : column.DataType;
        }

        private sealed record ErDiagramLayout(
            IReadOnlyList<ErTableLayout> Tables,
            double Width,
            double Height);

        private sealed record ErTableLayout(
            TableDefinition Table,
            double X,
            double Y,
            double Width,
            double Height);
    }
}
