using PaperlessLabelGenerator.Core.Labels.ConcreteDesigns;

namespace PaperlessLabelGenerator.Core.Labels;

/// <summary>
/// Factory for creating label design instances based on the LabelFormat enum.
/// </summary>
public class LabelDesignFactory : ILabelDesignFactory
{
    /// <summary>
    /// Creates a label design instance for the specified format.
    /// </summary>
    /// <param name="format">The label format enum value.</param>
    /// <returns>An instance implementing ILabelDesign.</returns>
    /// <exception cref="ArgumentOutOfRangeException">If the format is not supported.</exception>
    public ILabelDesign Create(LabelFormat format)
    {
        // Use a switch expression for clean, type-safe mapping from enum to instance
        return format switch
        {
            LabelFormat.AveryL4731 => new AveryL4731LabelDesign(),
            _ => throw new ArgumentOutOfRangeException(nameof(format), format, "Unsupported label format.")
        };
    }

    /// <summary>
    /// Gets all available label format enum values.
    /// </summary>
    /// <returns>An enumerable of all supported LabelFormat values.</returns>
    public IEnumerable<LabelFormat> GetAvailableFormats()
    {
        return Enum.GetValues<LabelFormat>();
    }

    /// <summary>
    /// Gets all available label formats with their friendly names.
    /// </summary>
    /// <returns>An enumerable of tuples containing the format enum and its name.</returns>
    public IEnumerable<(LabelFormat Format, string Name)> GetAvailableFormatsWithNames()
    {
        return GetAvailableFormats().Select(format =>
        {
            ILabelDesign design = Create(format);
            return (Format: format, design.FormatName);
        });
    }
}
