using PaperlessLabelGenerator.Core.Labels.ConcreteDesigns;

namespace PaperlessLabelGenerator.Core.Labels;

/// <summary>
/// Defines a factory for creating label design instances from an enum.
/// </summary>
public interface ILabelDesignFactory
{
    /// <summary>
    /// Creates a label design instance for the specified format.
    /// </summary>
    ILabelDesign Create(LabelFormat format);

    /// <summary>
    /// Gets all available label format enum values.
    /// </summary>
    IEnumerable<LabelFormat> GetAvailableFormats();

    /// <summary>
    /// Gets all available label formats with their friendly names.
    /// </summary>
    IEnumerable<(LabelFormat Format, string Name)> GetAvailableFormatsWithNames();
}

