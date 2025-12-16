using QuestPDF.Infrastructure;

namespace PaperlessLabelGenerator.Core.Labels;

public interface ILabelDesign
{
    string FormatId { get; }
    string FormatName { get; }

    // Label Dimensions
    float LabelWidthMm { get; }
    float LabelHeightMm { get; }

    // Physical Page Layout
    float PageMarginTopBottomMm { get; }
    float PageMarginSideMm { get; }

    // Label Grid
    int ColumnsPerRow { get; }
    int RowsPerSheet { get; }

    // Spacing (The gutters)
    float HorizontalSpacingMm { get; }
    float VerticalSpacingMm { get; }

    void ComposeLabel(IContainer container, string labelText);
}
