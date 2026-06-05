using PaperlessLabelGenerator.Core.Generators;
using PaperlessLabelGenerator.Core.Labels;
using PaperlessLabelGenerator.Core.Labels.ConcreteDesigns;
using PaperlessLabelGenerator.Core.Models;

namespace PaperlessLabelGenerator.Tests;

public class LabelGenerationTests
{
    // ---------------------------------------------------------------------
    // LabelDesignFactory
    // ---------------------------------------------------------------------

    [Test]
    public async Task Factory_Create_AveryL4731_ReturnsMatchingDesign()
    {
        var factory = new LabelDesignFactory();

        ILabelDesign design = factory.Create(LabelFormat.AveryL4731);

        await Assert.That(design).IsTypeOf<AveryL4731LabelDesign>();
        await Assert.That(design.FormatId).IsEqualTo("avery-l4731");
    }

    [Test]
    public async Task Factory_Create_UnsupportedFormat_Throws()
    {
        var factory = new LabelDesignFactory();

        await Assert.That(() => factory.Create((LabelFormat)999))
            .Throws<ArgumentOutOfRangeException>();
    }

    [Test]
    public async Task Factory_GetAvailableFormats_ContainsAveryL4731()
    {
        var factory = new LabelDesignFactory();

        var formats = factory.GetAvailableFormats().ToList();

        await Assert.That(formats).Contains(LabelFormat.AveryL4731);
    }

    [Test]
    public async Task Factory_GetAvailableFormatsWithNames_ReturnsFriendlyName()
    {
        var factory = new LabelDesignFactory();

        var named = factory.GetAvailableFormatsWithNames().ToList();

        await Assert.That(named).Contains(x =>
            x.Format == LabelFormat.AveryL4731 &&
            x.Name == "Avery Zweckform L4731 (25.4 x 10 mm)");
    }

    // ---------------------------------------------------------------------
    // AveryL4731LabelDesign — physical geometry of the L4731 sheet.
    // These values must match the real die-cut sheet or printing misaligns.
    // ---------------------------------------------------------------------

    [Test]
    public async Task AveryL4731_LabelDimensions_Match()
    {
        var design = new AveryL4731LabelDesign();

        await Assert.That(design.LabelWidthMm).IsEqualTo(25.4f);
        await Assert.That(design.LabelHeightMm).IsEqualTo(10f);
    }

    [Test]
    public async Task AveryL4731_Grid_Is7By27_For189Labels()
    {
        var design = new AveryL4731LabelDesign();

        await Assert.That(design.ColumnsPerRow).IsEqualTo(7);
        await Assert.That(design.RowsPerSheet).IsEqualTo(27);
        await Assert.That(design.ColumnsPerRow * design.RowsPerSheet).IsEqualTo(189);
    }

    [Test]
    public async Task AveryL4731_PageMargins_Are8And14()
    {
        var design = new AveryL4731LabelDesign();

        await Assert.That(design.PageMarginSideMm).IsEqualTo(8f);
        await Assert.That(design.PageMarginTopBottomMm).IsEqualTo(14f);
    }

    // ---------------------------------------------------------------------
    // LabelDocumentGenerator — produces a valid PDF for the design.
    // ---------------------------------------------------------------------

    private static List<LabelContent> BuildLabels(int count, string prefix = "ASN", int startNumber = 1, int digits = 5)
    {
        var labels = new List<LabelContent>(count);
        var format = $"D{digits}";
        for (var i = 0; i < count; i++)
        {
            var number = startNumber + i;
            labels.Add(new LabelContent($"{prefix}{number}", $"{prefix}{number.ToString(format)}"));
        }
        return labels;
    }

    private static bool HasPdfSignature(byte[] bytes) =>
        bytes.Length >= 4 && bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46; // %PDF

    [Test]
    public async Task Generator_SingleLabel_ProducesValidPdf()
    {
        var generator = new LabelDocumentGenerator();
        var design = new AveryL4731LabelDesign();

        var pdf = await generator.GenerateAsync(design, BuildLabels(1));

        await Assert.That(pdf).IsNotNull();
        await Assert.That(HasPdfSignature(pdf)).IsTrue();
    }

    [Test]
    [Arguments(5)]
    [Arguments(50)]
    [Arguments(189)]
    public async Task Generator_VariousCounts_ProduceValidPdf(int count)
    {
        var generator = new LabelDocumentGenerator();
        var design = new AveryL4731LabelDesign();

        var pdf = await generator.GenerateAsync(design, BuildLabels(count));

        await Assert.That(HasPdfSignature(pdf)).IsTrue();
        await Assert.That(pdf.Length).IsGreaterThan(0);
    }

    [Test]
    public async Task Generator_FullSheet_ProducesDecentlySizedPdf()
    {
        var generator = new LabelDocumentGenerator();
        var design = new AveryL4731LabelDesign();

        var pdf = await generator.GenerateAsync(design, BuildLabels(189));

        await Assert.That(pdf.Length).IsGreaterThan(10_000);
    }

    [Test]
    public async Task Generator_ContinuationSheet_StartsAtGivenNumber()
    {
        // A second sheet continues numbering from the previous one (ASN190..).
        var generator = new LabelDocumentGenerator();
        var design = new AveryL4731LabelDesign();

        var pdf = await generator.GenerateAsync(design, BuildLabels(189, startNumber: 190));

        await Assert.That(HasPdfSignature(pdf)).IsTrue();
    }

    [Test]
    public async Task Generator_EmptyLabelList_StillProducesPdf()
    {
        var generator = new LabelDocumentGenerator();
        var design = new AveryL4731LabelDesign();

        var pdf = await generator.GenerateAsync(design, []);

        await Assert.That(HasPdfSignature(pdf)).IsTrue();
    }

    // ---------------------------------------------------------------------
    // LabelConfiguration — prefix + start number + zero padding formatting.
    // ---------------------------------------------------------------------

    [Test]
    public async Task Config_FormatLabel_WithPadding()
    {
        var config = new LabelConfiguration { LabelPrefix = "ASN-", PaddingZeros = 4 };

        await Assert.That(config.FormatLabel(1)).IsEqualTo("ASN-0001");
    }

    [Test]
    public async Task Config_FormatLabel_WithoutPadding()
    {
        var config = new LabelConfiguration { LabelPrefix = "INV", PaddingZeros = 0 };

        await Assert.That(config.FormatLabel(189)).IsEqualTo("INV189");
    }

    [Test]
    public async Task Config_FormatLabel_DifferentNumbers()
    {
        var config = new LabelConfiguration { LabelPrefix = "DOC-", PaddingZeros = 5 };

        await Assert.That(config.FormatLabel(1)).IsEqualTo("DOC-00001");
        await Assert.That(config.FormatLabel(100)).IsEqualTo("DOC-00100");
        await Assert.That(config.FormatLabel(10000)).IsEqualTo("DOC-10000");
    }

    [Test]
    public async Task Config_GenerateQrData_DefaultTemplate_ReturnsLabelText()
    {
        var config = new LabelConfiguration { QrCodeDataTemplate = null };

        await Assert.That(config.GenerateQrData("ASN-0001")).IsEqualTo("ASN-0001");
    }

    [Test]
    public async Task Config_GenerateQrData_CustomTemplate_SubstitutesLabel()
    {
        var config = new LabelConfiguration { QrCodeDataTemplate = "https://paperless.local/?id={label}" };

        await Assert.That(config.GenerateQrData("ASN-0001"))
            .IsEqualTo("https://paperless.local/?id=ASN-0001");
    }

    [Test]
    public async Task Config_DefaultValues()
    {
        var config = new LabelConfiguration();

        await Assert.That(config.LabelPrefix).IsEqualTo("ASN");
        await Assert.That(config.StartingNumber).IsEqualTo(1);
        await Assert.That(config.PaddingZeros).IsEqualTo(4);
        await Assert.That(config.LabelFormat).IsEqualTo("avery-l4731");
        await Assert.That(config.LabelCount).IsEqualTo(189);
        await Assert.That(config.IncludeQrCode).IsTrue();
        await Assert.That(config.QrCodeDataTemplate).IsNull();
    }
}
