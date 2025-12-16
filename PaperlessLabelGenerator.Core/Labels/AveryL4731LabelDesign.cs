using QRCoder;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PaperlessLabelGenerator.Core.Labels;

/// <summary>
/// Label design for Avery Zweckform L4731 labels with QRCoder QR code support
/// Format: 25.4 x 10 mm (189 labels per A4 sheet)
/// Layout: QR code (left) + label text (right)
/// Uses QRCoder - zero dependency QR code generation
/// https://www.avery-zweckform.com/produkt/universal-etiketten-l4731rev-25
/// </summary>
public class AveryL4731LabelDesign : ILabelDesign
{
    public string FormatId => "avery-l4731";
    public string FormatName => "Avery Zweckform L4731 (25.4 x 10 mm with QR)";

    // Label dimensions
    public float LabelWidthMm => 25.4f;
    public float LabelHeightMm => 10f;

    // A4 layout: 3 columns, 63 rows = 189 labels per sheet
    private const int ColumnsPerRow = 3;
    private const int RowsPerSheet = 63;

    public int GetColumnsPerRow() => ColumnsPerRow;
    public int GetRowsPerSheet() => RowsPerSheet;

    /// <summary>
    /// Compose label with optional QR code
    /// Layout: [QR Code] [Text Label]
    /// </summary>
    public void ComposeLabel(IContainer container, string labelText, int fontSize,
        bool includeQrCode = true, float qrCodeSizeMm = 8f, string qrCodeData = "")
    {
        container
            .Padding(0.5f, Unit.Millimetre)
            .Width(LabelWidthMm, Unit.Millimetre)
            .Height(LabelHeightMm, Unit.Millimetre)
            .Border(0.5f)
            .Padding(0.3f, Unit.Millimetre)
            .Row(row =>
            {
                // QR Code (left side) if enabled
                if (includeQrCode)
                {
                    row.RelativeItem(2).Element(qrContainer =>
                    {
                        RenderQrCode(qrContainer, qrCodeData ?? labelText, qrCodeSizeMm);
                    });

                    // Small spacer
                    row.ConstantItem(0.5f, Unit.Millimetre);
                }

                // Text label (remaining space)
                row.RelativeItem(includeQrCode ? 3 : 5)
                    .AlignCenter()
                    .AlignMiddle()
                    .Text(labelText)
                    .FontSize(fontSize)
                    .FontColor(Colors.Black);
            });
    }

    /// <summary>
    /// Render QR code in the given container
    /// Uses QRCoder library to generate QR code from data
    /// </summary>
    private void RenderQrCode(IContainer container, string qrData, float sizeMm)
    {
        try
        {
            // Generate QR code using QRCoder (zero dependencies)
            using var qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(qrData, QRCodeGenerator.ECCLevel.M);

            // Use PngByteQRCode for pixel data
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrImage = qrCode.GetGraphic(20); // 20px per module

            // Render QR code in container
            container
                .Height(sizeMm, Unit.Millimetre)
                .Width(sizeMm, Unit.Millimetre)
                .AlignCenter()
                .AlignMiddle()
                .Image(qrImage);
        }
        catch (Exception)
        {
            // Fallback: render placeholder if QR generation fails
            // This prevents label generation from failing completely
            container
                .Height(sizeMm, Unit.Millimetre)
                .Width(sizeMm, Unit.Millimetre)
                .Border(0.5f)
                .Background(Colors.Grey.Lighten3)
                .AlignCenter()
                .AlignMiddle()
                .Text("QR Error");
        }
    }
}