using PaperlessLabelGenerator.Core.Labels.ConcreteDesigns;

namespace PaperlessLabelGenerator.Contracts;

public sealed record GenerateLabelsRequest(
    LabelFormat Format = 0,       // e.g. AveryL4731
    string LabelPrefix = "ASN",   // e.g. "ASN"
    int numberOfDigits = 5,       // e.g. 4 (generates 0001)
    int StartingNumber = 1        // e.g. 1
);