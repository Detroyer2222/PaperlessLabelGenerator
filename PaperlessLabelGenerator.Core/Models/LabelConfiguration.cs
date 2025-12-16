namespace PaperlessLabelGenerator.Core.Models;

/// <summary>
/// Configuration for label generation with QR code support
/// </summary>
public class LabelConfiguration
{
    /// <summary>
    /// The prefix/label string (e.g., "ASN", "ASN-", "INV")
    /// </summary>
    public string LabelPrefix { get; set; } = "ASN";

    /// <summary>
    /// The starting number for incrementing labels
    /// </summary>
    public int StartingNumber { get; set; } = 1;

    /// <summary>
    /// Number of leading zeros to pad the number with (0 = no padding)
    /// </summary>
    public int PaddingZeros { get; set; } = 4;

    /// <summary>
    /// The label format type (e.g., "avery-l4731")
    /// </summary>
    public string LabelFormat { get; set; } = "avery-l4731";

    /// <summary>
    /// How many labels to generate on the PDF
    /// </summary>
    public int LabelCount { get; set; } = 189;

    /// <summary>
    /// Font size for labels in points
    /// </summary>
    public int FontSize { get; set; } = 10;

    /// <summary>
    /// Enable QR code generation on labels
    /// </summary>
    public bool IncludeQrCode { get; set; } = true;

    /// <summary>
    /// Size of QR code in millimeters
    /// </summary>
    public float QrCodeSizeMm { get; set; } = 8f;

    /// <summary>
    /// Custom QR code data generator (if null, uses label text)
    /// Examples: "{label}", "{label}:{date}", "https://paperless.local?id={label}"
    /// Placeholder {label} will be replaced with the formatted label text
    /// </summary>
    public string? QrCodeDataTemplate { get; set; }

    /// <summary>
    /// Generate the formatted label for a given number
    /// </summary>
    public string FormatLabel(int number)
    {
        var formattedNumber = PaddingZeros > 0
            ? number.ToString().PadLeft(PaddingZeros, '0')
            : number.ToString();

        return $"{LabelPrefix}{formattedNumber}";
    }

    /// <summary>
    /// Generate the QR code data for a given label
    /// </summary>
    public string GenerateQrData(string labelText)
    {
        if (string.IsNullOrEmpty(QrCodeDataTemplate))
            return labelText;

        // Replace placeholder with actual label text
        return QrCodeDataTemplate.Replace("{label}", labelText);
    }
}