namespace PaperlessLabelGenerator.Core.Exceptions;

/// <summary>
/// Thrown when label generation encounters an error
/// </summary>
public class LabelGenerationException : Exception
{
    public LabelGenerationException(string message) : base(message) { }

    public LabelGenerationException(string message, Exception innerException)
        : base(message, innerException) { }
}