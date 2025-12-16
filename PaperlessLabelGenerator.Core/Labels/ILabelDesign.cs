using QuestPDF.Infrastructure;

namespace PaperlessLabelGenerator.Core.Labels;

public interface ILabelDesign
{
    string FormatId { get; }
    string FormatName { get; }

    // Physical Page Layout
    float PageMarginTopBottomMm { get; }
    float PageMarginSideMm { get; }

    // Label Grid
    int ColumnsPerRow { get; }
    int RowsPerSheet { get; }

    // Spacing (The gutters)
    float HorizontalSpacingMm { get; } // Gap between columns (e.g., 2mm)
    float VerticalSpacingMm { get; }   // Gap between rows (e.g., 0mm)

    // Render Logic
    void ComposeLabel(IContainer container, string labelText);
}
