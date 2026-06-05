// Here you could define global logic that would affect all tests
using QuestPDF.Infrastructure;

// You can use attributes at the assembly level to apply to all tests in the assembly
[assembly: Retry(3)]
[assembly: System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]

namespace PaperlessLabelGenerator.Tests;

public class GlobalHooks
{
    [Before(TestSession)]
    public static void SetUp()
    {
        // QuestPDF requires a license to be configured before any document is generated.
        QuestPDF.Settings.License = LicenseType.Community;
    }
}
