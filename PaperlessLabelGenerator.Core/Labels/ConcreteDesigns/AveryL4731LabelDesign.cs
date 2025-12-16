using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PaperlessLabelGenerator.Core.Labels;

public class AveryL4731LabelDesign : ILabelDesign
{
    public string FormatId => "avery-l4731";
    public string FormatName => "Avery Zweckform L4731 (25.4 x 10 mm)";
    public float LabelWidthMm => 25.4f;
    public float LabelHeightMm => 10.0f;
    public float PageMarginTopBottomMm => 13.5f;
    public float PageMarginSideMm => 9.0f;
    public int ColumnsPerRow => 7;
    public int RowsPerSheet => 27;
    public float HorizontalSpacingMm => 2.0f;
    public float VerticalSpacingMm => 0.0f;

    public void ComposeLabel(IContainer container, LabelContent content)
    {
        // 1. Bigger QR (9mm fits inside 10mm height)
        const float qrSizeMm = 9f;

        // 2. Bigger Text
        const float fontSize = 7f;

        container.Row(row =>
        {
            // QR Code (Left)
            row.AutoItem()
                .AlignMiddle()
                .Element(e => RenderQrCode(e, content.QrData, qrSizeMm));

            // Text (Right)
            row.RelativeItem()
                .AlignMiddle()
                .AlignLeft()
                .PaddingLeft(0.5f, Unit.Millimetre)
                .Text(content.DisplayText ?? "")
                .FontSize(fontSize)
                .FontColor(Colors.Black)
                .SemiBold();
        });
    }

    private static void RenderQrCode(IContainer container, string data, float sizeMm)
    {
        if (string.IsNullOrEmpty(data)) return;

        using var generator = new QRCodeGenerator();
        using QRCodeData qrData = generator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
        using var svgQr = new SvgQRCode(qrData);

        // Get SVG string. The size param usually controls width/height attributes.
        // QuestPDF will scale the SVG to fit the container anyway.
        var svgString = svgQr.GetGraphic(1);

        container
            .Width(sizeMm, Unit.Millimetre)
            .Height(sizeMm, Unit.Millimetre)
            .Svg(svgString);
    }
}
