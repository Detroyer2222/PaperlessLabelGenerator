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

        var labels = new List<string>(capacity);

        var numberFormat = $"D{request.numberOfDigits}";

        for (var i = 0; i < capacity; i++)
        {
            var currentNumber = request.StartingNumber + i;
            var numericPart = currentNumber.ToString(numberFormat);
            labels.Add($"{request.LabelPrefix}{numericPart}");
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
