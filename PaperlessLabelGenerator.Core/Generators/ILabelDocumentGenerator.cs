using PaperlessLabelGenerator.Core.Labels;

namespace PaperlessLabelGenerator.Core.Generators;

/// <summary>
/// Interface for generating label documents (PDFs).
/// </summary>
public interface ILabelDocumentGenerator
{
    /// <summary>
    /// Generates a PDF document containing labels.
    /// </summary>
    Task<byte[]> GenerateAsync(ILabelDesign design, IEnumerable<string> labels);
}
