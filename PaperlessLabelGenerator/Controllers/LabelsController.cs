using Microsoft.AspNetCore.Mvc;
using PaperlessLabelGenerator.Contracts;
using PaperlessLabelGenerator.Core.Generators;
using PaperlessLabelGenerator.Core.Labels;

namespace PaperlessLabelGenerator.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class LabelsController : ControllerBase
{
    private readonly ILabelDesignFactory _labelDesignFactory;
    private readonly ILabelDocumentGenerator _labelDocumentGenerator;

    public LabelsController(
        ILabelDesignFactory labelDesignFactory,
        ILabelDocumentGenerator labelDocumentGenerator)
    {
        _labelDesignFactory = labelDesignFactory;
        _labelDocumentGenerator = labelDocumentGenerator;
    }

    [HttpPost]
    public async Task<IActionResult> GenerateAsync([FromBody] GenerateLabelsRequest request)
    {
        ILabelDesign design = _labelDesignFactory.Create(request.Format);

        var capacity = design.ColumnsPerRow * design.RowsPerSheet;

        var labels = new List<LabelContent>(capacity);
        var numberFormat = $"D{request.numberOfDigits}";

        for (var i = 0; i < capacity; i++)
        {
            var currentNumber = request.StartingNumber + i;

            // 1. QR Data: Prefix + Raw Number (e.g. "ASN1")
            var qrData = $"{request.LabelPrefix}{currentNumber}";

            // 2. Display Text: Prefix + Padded Number (e.g. "ASN0001")
            var displayText = $"{request.LabelPrefix}{currentNumber.ToString(numberFormat)}";

            labels.Add(new LabelContent(qrData, displayText));
        }

        var pdfBytes = await _labelDocumentGenerator.GenerateAsync(design, labels);
        return File(pdfBytes, "application/pdf", $"labels-{request.LabelPrefix}.pdf");
    }

    [HttpGet("formats")]
    public IActionResult GetFormats()
    {
        var formats = _labelDesignFactory.GetAvailableFormats()
            .Select(f => new { Id = (int)f, Name = f.ToString() });
        return Ok(formats);
    }
}
