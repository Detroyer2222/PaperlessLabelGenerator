using PaperlessLabelGenerator.Core.Exceptions;
using PaperlessLabelGenerator.Core.Labels;
using PaperlessLabelGenerator.Core.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PaperlessLabelGenerator.Core.Generators;

/// <summary>
/// Main service for generating label PDFs with QR code support
/// Orchestrates label design selection and PDF document creation
/// </summary>
public class LabelGenerator
{
    private readonly ILabelDesign _labelDesign;
    private readonly LabelConfiguration _config;

    /// <summary>
    /// Initialize a new label generator
    /// </summary>
    /// <param name="config">Label generation configuration</param>
    /// <exception cref="ArgumentNullException">If config is null</exception>
    /// <exception cref="LabelGenerationException">If label format is invalid</exception>
    public LabelGenerator(LabelConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));

        try
        {
            _labelDesign = LabelDesignFactory.CreateLabelDesign(_config.LabelFormat);
        }
        catch (ArgumentException ex)
        {
            throw new LabelGenerationException($"Invalid label format: {_config.LabelFormat}", ex);
        }
    }

    /// <summary>
    /// Generate a PDF document containing labels with optional QR codes
    /// </summary>
    /// <returns>Byte array containing the PDF document</returns>
    /// <exception cref="LabelGenerationException">If PDF generation fails</exception>
    public byte[] GenerateLabelsPdf()
    {
        try
        {
            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    ConfigurePage(page);
                    RenderLabels(page);
                });
            });

            return document.GeneratePdf();
        }
        catch (Exception ex)
        {
            throw new LabelGenerationException("Failed to generate PDF document", ex);
        }
    }

    /// <summary>
    /// Configure the A4 page settings
    /// </summary>
    private void ConfigurePage(PageDescriptor page)
    {
        page.Size(PageSizes.A4);
        page.Margin(5, Unit.Millimetre);
        page.PageColor(Colors.White);
    }

    /// <summary>
    /// Render all labels on the page with optional QR codes
    /// </summary>
    private void RenderLabels(PageDescriptor page)
    {
        var currentNumber = _config.StartingNumber;

        page.Content().Column(column =>
        {
            // Render labels row by row
            for (var row = 0; row < _labelDesign.GetRowsPerSheet(); row++)
            {
                if (currentNumber > _config.StartingNumber + _config.LabelCount - 1)
                    break;

                // Render one row of labels
                column.Item().Row(rowContainer =>
                {
                    for (var col = 0; col < _labelDesign.GetColumnsPerRow(); col++)
                    {
                        if (currentNumber > _config.StartingNumber + _config.LabelCount - 1)
                            break;

                        var labelText = _config.FormatLabel(currentNumber);
                        var qrData = _config.GenerateQrData(labelText);

                        rowContainer
                            .RelativeItem()
                            .Element(container =>
                            {
                                _labelDesign.ComposeLabel(
                                    container,
                                    labelText,
                                    _config.FontSize,
                                    _config.IncludeQrCode,
                                    _config.QrCodeSizeMm,
                                    qrData);
                            });

                        currentNumber++;
                    }

                    // Fill remaining columns in the row if less than full row
                    var renderedColumns = Math.Min(
                        _labelDesign.GetColumnsPerRow(),
                        _config.LabelCount - (currentNumber - _config.StartingNumber)
                    );

                    for (var i = renderedColumns; i < _labelDesign.GetColumnsPerRow(); i++)
                    {
                        rowContainer.RelativeItem().Element(container =>
                        {
                            container
                                .Width(_labelDesign.LabelWidthMm, Unit.Millimetre)
                                .Height(_labelDesign.LabelHeightMm, Unit.Millimetre);
                        });
                    }
                });
            }
        });
    }
}