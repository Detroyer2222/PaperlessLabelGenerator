using Microsoft.AspNetCore.Mvc;
using PaperlessLabelGenerator.Core.Generators;
using PaperlessLabelGenerator.Core.Labels;
using PaperlessLabelGenerator.Core.Models;

namespace PaperlessLabelGenerator.Api.Controllers;

/// <summary>
/// API endpoints for generating ASN labels
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class LabelsController : ControllerBase
{
    private readonly ILogger<LabelsController> _logger;

    public LabelsController(ILogger<LabelsController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Generate a PDF with ASN labels based on the provided configuration
    /// </summary>
    /// <param name="config">Label configuration</param>
    /// <returns>PDF file download</returns>
    /// <example>
    /// POST /api/labels
    /// {
    ///   "labelPrefix": "ASN-",
    ///   "startingNumber": 1,
    ///   "paddingZeros": 4,
    ///   "labelFormat": "avery-l4731",
    ///   "labelCount": 189,
    ///   "fontSize": 10
    /// }
    /// </example>
    [HttpPost]
    [Produces("application/pdf")]
    public IActionResult GenerateLabels([FromBody] LabelConfiguration? config)
    {
        if (config == null)
        {
            _logger.LogWarning("Generate labels request received with null configuration");
            return BadRequest(new { error = "Configuration cannot be null" });
        }

        try
        {
            _logger.LogInformation(
                "Generating labels: Prefix={Prefix}, Start={Start}, Padding={Padding}, Format={Format}, Count={Count}",
                config.LabelPrefix,
                config.StartingNumber,
                config.PaddingZeros,
                config.LabelFormat,
                config.LabelCount);

            var generator = new LabelGenerator(config);
            var pdfBytes = generator.GenerateLabelsPdf();

            var fileName = $"labels_{config.LabelPrefix}_{config.StartingNumber:D4}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            return File(pdfBytes, "application/pdf", fileName);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid label configuration provided");
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating labels");
            return StatusCode(500, new { error = "An error occurred while generating labels", details = ex.Message });
        }
    }

    /// <summary>
    /// Get information about available label formats
    /// </summary>
    /// <returns>List of supported label formats</returns>
    [HttpGet("formats")]
    public IActionResult GetAvailableFormats()
    {
        try
        {
            var formats = LabelDesignFactory
                .GetAvailableFormatsWithNames()
                .Select(f => new { id = f.Id, name = f.Name })
                .ToList();

            return Ok(new { formats });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving available formats");
            return StatusCode(500, new { error = "An error occurred while retrieving formats" });
        }
    }

    /// <summary>
    /// Get example configuration for label generation
    /// </summary>
    /// <returns>Example LabelConfiguration</returns>
    [HttpGet("example")]
    public IActionResult GetExampleConfiguration()
    {
        var example = new LabelConfiguration
        {
            LabelPrefix = "ASN-",
            StartingNumber = 1,
            PaddingZeros = 4,
            LabelFormat = "avery-l4731",
            LabelCount = 189,
            FontSize = 10
        };

        return Ok(example);
    }
}