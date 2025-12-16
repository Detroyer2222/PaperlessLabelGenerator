using PaperlessLabelGenerator.Core.Labels;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PaperlessLabelGenerator.Core.Generators;

public class LabelDocumentGenerator : ILabelDocumentGenerator
{
    public Task<byte[]> GenerateAsync(ILabelDesign design, IEnumerable<string> labels)
    {
        return Task.Run(() =>
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);

                    // Use margins defined by the specific label design
                    page.MarginHorizontal(design.PageMarginSideMm, Unit.Millimetre);
                    page.MarginVertical(design.PageMarginTopBottomMm, Unit.Millimetre);

                    page.PageColor(Colors.White);

                    page.Content().Table(table =>
                    {
                        // 1. Define Columns (Content + Spacers)
                        table.ColumnsDefinition(columns =>
                        {
                            for (var i = 0; i < design.ColumnsPerRow; i++)
                            {
                                // Content column
                                columns.RelativeColumn();

                                // Horizontal Spacer (Gutter)
                                if (i < design.ColumnsPerRow - 1 && design.HorizontalSpacingMm > 0)
                                {
                                    columns.ConstantColumn(design.HorizontalSpacingMm, Unit.Millimetre);
                                }
                            }
                        });

                        var labelList = labels.ToList();
                        var index = 0;

                        // 2. Iterate Rows
                        for (var row = 0; row < design.RowsPerSheet; row++)
                        {
                            // 2a. Iterate Columns
                            for (var col = 0; col < design.ColumnsPerRow; col++)
                            {
                                var labelText = index < labelList.Count ? labelList[index] : string.Empty;

                                table.Cell().Element(cell =>
                                {
                                    // GENERALIZED: Use design.LabelHeightMm instead of hardcoded 10
                                    cell.Height(design.LabelHeightMm, Unit.Millimetre)
                                        .Element(c => design.ComposeLabel(c, labelText));
                                });

                                // Horizontal Spacer Cell
                                if (col < design.ColumnsPerRow - 1 && design.HorizontalSpacingMm > 0)
                                {
                                    table.Cell(); // Empty cell for spacing
                                }

                                index++;
                            }

                            // 3. Vertical Spacer Row (after every row except the last one)
                            if (row < design.RowsPerSheet - 1 && design.VerticalSpacingMm > 0)
                            {
                                // Add a full row of empty cells to create vertical gap
                                var totalTableCols = design.ColumnsPerRow + (design.ColumnsPerRow - 1);


                                for (var k = 0; k < totalTableCols; k++)
                                {
                                    table.Cell().Height(design.VerticalSpacingMm, Unit.Millimetre);
                                }
                            }
                        }
                    });
                });
            });

            return document.GeneratePdf();
        });
    }
}
