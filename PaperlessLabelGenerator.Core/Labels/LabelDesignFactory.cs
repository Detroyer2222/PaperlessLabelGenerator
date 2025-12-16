namespace PaperlessLabelGenerator.Core.Labels;

/// <summary>
/// Factory for creating label design instances
/// Manages all available label formats and their instantiation
/// </summary>
public class LabelDesignFactory
{
    private static readonly Dictionary<string, Func<ILabelDesign>> LabelDesigns =
        new(StringComparer.OrdinalIgnoreCase)
        {
            { "avery-l4731", () => new AveryL4731LabelDesign() },
            // Future label formats can be added here easily
            // { "avery-l7651", () => new AveryL7651LabelDesign() },
            // { "custom-format", () => new CustomLabelDesign() },
        };

    /// <summary>
    /// Create a label design instance by format ID
    /// </summary>
    /// <param name="formatId">The format identifier (e.g., "avery-l4731")</param>
    /// <returns>An instance implementing ILabelDesign</returns>
    /// <exception cref="ArgumentException">If format ID is not found</exception>
    public static ILabelDesign CreateLabelDesign(string formatId)
    {
        if (string.IsNullOrWhiteSpace(formatId))
            throw new ArgumentException("Format ID cannot be null or empty", nameof(formatId));

        if (LabelDesigns.TryGetValue(formatId, out Func<ILabelDesign>? factory))
            return factory();

        throw new ArgumentException(
            $"Label format '{formatId}' is not supported. Available formats: {string.Join(", ", LabelDesigns.Keys)}",
            nameof(formatId));
    }

    /// <summary>
    /// Get all available label format IDs
    /// </summary>
    public static IEnumerable<string> GetAvailableFormats() => LabelDesigns.Keys;

    /// <summary>
    /// Get all available label formats with their details
    /// </summary>
    public static IEnumerable<(string Id, string Name)> GetAvailableFormatsWithNames()
    {
        return LabelDesigns.Keys.Select(id =>
        {
            ILabelDesign design = CreateLabelDesign(id);
            return (id, design.FormatName);
        });
    }
}