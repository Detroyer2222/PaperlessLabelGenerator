using QuestPDF.Infrastructure;

namespace PaperlessLabelGenerator.Core.Labels;

/// <summary>
/// Interface for label design implementations
/// Each label format (Avery L4731, etc.) implements this to define its specific layout
/// Now supports QR code rendering
/// </summary>
public interface ILabelDesign
{
    /// <summary>
    /// The unique identifier for this label format
    /// </summary>
    string FormatId { get; }

    /// <summary>
    /// Friendly name for the label format
    /// </summary>
    string FormatName { get; }

    /// <summary>
    /// Width of a single label in millimeters
    /// </summary>
    float LabelWidthMm { get; }

    /// <summary>
    /// Height of a single label in millimeters
    /// </summary>
    float LabelHeightMm { get; }

    /// <summary>
    /// Compose the layout for this label format in the container
    /// This method is called for each label with the text to render and optional QR code data
    /// </summary>
    /// <param name="container">The QuestPDF container to render into</param>
    /// <param name="labelText">The main label text to display</param>
    /// <param name="fontSize">Font size in points</param>
    /// <param name="includeQrCode">Whether to render QR code</param>
    /// <param name="qrCodeSizeMm">Size of QR code in millimeters</param>
    /// <param name="qrCodeData">Data to encode in QR code</param>
    void ComposeLabel(IContainer container, string labelText, int fontSize,
        bool includeQrCode = true, float qrCodeSizeMm = 8f, string qrCodeData = "");

    /// <summary>
    /// Get the optimal number of columns for an A4 sheet
    /// </summary>
    int GetColumnsPerRow();

    /// <summary>
    /// Get the optimal number of rows for an A4 sheet
    /// </summary>
    int GetRowsPerSheet();
}