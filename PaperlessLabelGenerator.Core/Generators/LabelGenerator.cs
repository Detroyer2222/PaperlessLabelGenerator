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
                    page.MarginHorizontal(design.PageMarginSideMm, Unit.Millimetre);
                    page.MarginVertical(design.PageMarginTopBottomMm, Unit.Millimetre);
                    page.PageColor(Colors.White);

                    page.Content().Table(table =>
                    {
                        // Define columns and gutters explicitly
                        table.ColumnsDefinition(columns =>
                        {
                            for (var i = 0; i < design.ColumnsPerRow; i++)
                            {
                                // Content column
                                columns.RelativeColumn();

                                // Add spacer column AFTER every column except the last one
                                if (i < design.ColumnsPerRow - 1)
                                {
                                    columns.ConstantColumn(design.HorizontalSpacingMm, Unit.Millimetre);
                                }
                            }
                        });

                        var labelList = labels.ToList();
                        var index = 0;

                        for (var row = 0; row < design.RowsPerSheet; row++)
                        {
                            for (var col = 0; col < design.ColumnsPerRow; col++)
                            {
                                // 1. Determine label text
                                var labelText = index < labelList.Count ? labelList[index] : string.Empty;

                                // 2. Render the label cell
                                table.Cell().Element(cell =>
                                {
                                    // Ensure exact height constraint per row
                                    // 10mm height per row (Avery L4731)
                                    cell.Height(10, Unit.Millimetre).Element(c => design.ComposeLabel(c, labelText));
                                });

                                // 3. Add spacer cell if this isn't the last column
                                if (col < design.ColumnsPerRow - 1)
                                {
                                    // Empty cell for the 2mm gutter
                                    table.Cell();
                                }

                                index++;
                            }

                            // If there is vertical spacing (e.g. 0mm), it's handled here. 
                            // Since it's 0mm for this format, we don't need a spacer row.
                        }
                    });
                });
            });

            return document.GeneratePdf();
        });
    }
}
