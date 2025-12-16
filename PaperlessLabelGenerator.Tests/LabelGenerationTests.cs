using PaperlessLabelGenerator.Core.Generators;
using PaperlessLabelGenerator.Core.Labels;
using PaperlessLabelGenerator.Core.Models;

namespace PaperlessLabelGenerator.Tests;

public class LabelGenerationTests
{
    [Test]
    public async Task TestLabelConfiguration_FormatLabel_WithPadding()
    {
        // Arrange
        var config = new LabelConfiguration
        {
            LabelPrefix = "ASN-",
            PaddingZeros = 4
        };

        // Act
        var result = config.FormatLabel(1);

        // Assert
        await Assert.That(result).IsEqualTo("ASN-0001");
    }

    [Test]
    public async Task TestLabelConfiguration_FormatLabel_WithoutPadding()
    {
        // Arrange
        var config = new LabelConfiguration
        {
            LabelPrefix = "INV",
            PaddingZeros = 0
        };

        // Act
        var result = config.FormatLabel(189);

        // Assert
        await Assert.That(result).IsEqualTo("INV189");
    }

    [Test]
    public async Task TestLabelConfiguration_FormatLabel_DifferentStartNumbers()
    {
        // Arrange
        var config = new LabelConfiguration
        {
            LabelPrefix = "DOC-",
            PaddingZeros = 5
        };

        // Act
        var result1 = config.FormatLabel(1);
        var result2 = config.FormatLabel(100);
        var result3 = config.FormatLabel(10000);

        // Assert
        await Assert.That(result1).IsEqualTo("DOC-00001");
        await Assert.That(result2).IsEqualTo("DOC-00100");
        await Assert.That(result3).IsEqualTo("DOC-10000");
    }

    [Test]
    public async Task TestGenerateQrData_DefaultTemplate()
    {
        // Arrange
        var config = new LabelConfiguration
        {
            QrCodeDataTemplate = null
        };

        // Act
        var result = config.GenerateQrData("ASN-0001");

        // Assert
        await Assert.That(result).IsEqualTo("ASN-0001");
    }

    [Test]
    public async Task TestGenerateQrData_WithCustomTemplate()
    {
        // Arrange
        var config = new LabelConfiguration
        {
            QrCodeDataTemplate = "https://paperless.local/?id={label}"
        };

        // Act
        var result = config.GenerateQrData("ASN-0001");

        // Assert
        await Assert.That(result).IsEqualTo("https://paperless.local/?id=ASN-0001");
    }

    [Test]
    public async Task TestGenerateQrData_WithJsonTemplate()
    {
        // Arrange
        var config = new LabelConfiguration
        {
            QrCodeDataTemplate = "{\"label\":\"{label}\",\"type\":\"asn\"}"
        };

        // Act
        var result = config.GenerateQrData("ASN-0042");

        // Assert
        await Assert.That(result).IsEqualTo("{\"label\":\"ASN-0042\",\"type\":\"asn\"}");
    }

    [Test]
    public async Task TestLabelDesignFactory_CreateValidFormat()
    {
        // Act
        ILabelDesign design = LabelDesignFactory.CreateLabelDesign("avery-l4731");

        // Assert
        await Assert.That(design).IsNotNull();
        await Assert.That(design.FormatId).IsEqualTo("avery-l4731");
    }

    [Test]
    public async Task TestLabelDesignFactory_GetAvailableFormats()
    {
        // Act
        var formats = LabelDesignFactory.GetAvailableFormats().ToList();

        // Assert
        await Assert.That(formats.Count).IsGreaterThanOrEqualTo(1);
        await Assert.That(formats).Contains("avery-l4731");
    }

    [Test]
    [Arguments("avery-invalid")]
    [Arguments("non-existent")]
    [Arguments("")]
    public async Task TestLabelDesignFactory_InvalidFormat_ThrowsException(string formatId)
    {
        // Act & Assert
        ArgumentException? exception = await Assert.ThrowsAsync<ArgumentException>(
            () => (Task)LabelDesignFactory.CreateLabelDesign(formatId)
        );

        await Assert.That(exception.Message).Contains("not supported");
    }

    [Test]
    public async Task TestAveryL4731Design_Properties()
    {
        // Arrange
        var design = new AveryL4731LabelDesign();

        // Assert
        await Assert.That(design.FormatId).IsEqualTo("avery-l4731");
        await Assert.That(design.LabelWidthMm).IsEqualTo(25.4f);
        await Assert.That(design.LabelHeightMm).IsEqualTo(10f);
        await Assert.That(design.GetColumnsPerRow()).IsEqualTo(3);
        await Assert.That(design.GetRowsPerSheet()).IsEqualTo(63);
        await Assert.That(design.GetColumnsPerRow() * design.GetRowsPerSheet()).IsEqualTo(189);
    }

    [Test]
    public async Task TestLabelGenerator_BasicConfiguration_GeneratesPdf()
    {
        // Arrange
        var config = new LabelConfiguration
        {
            LabelPrefix = "TEST-",
            StartingNumber = 1,
            PaddingZeros = 4,
            LabelCount = 10
        };

        var generator = new LabelGenerator(config);

        // Act
        var pdf = generator.GenerateLabelsPdf();

        // Assert
        await Assert.That(pdf).IsNotNull();
        await Assert.That(pdf.Length).IsGreaterThan(0);
        // PDF signature: %PDF
        await Assert.That(pdf[0]).IsEqualTo((byte)0x25);
        await Assert.That(pdf[1]).IsEqualTo((byte)0x50);
        await Assert.That(pdf[2]).IsEqualTo((byte)0x44);
        await Assert.That(pdf[3]).IsEqualTo((byte)0x46);
    }

    [Test]
    public async Task TestLabelGenerator_WithQrCode_GeneratesPdf()
    {
        // Arrange
        var config = new LabelConfiguration
        {
            LabelPrefix = "QR-",
            LabelCount = 5,
            IncludeQrCode = true,
            QrCodeSizeMm = 8
        };

        var generator = new LabelGenerator(config);

        // Act
        var pdf = generator.GenerateLabelsPdf();

        // Assert
        await Assert.That(pdf).IsNotNull();
        await Assert.That(pdf.Length).IsGreaterThan(1000); // Should be larger with QR codes
    }

    [Test]
    public async Task TestLabelGenerator_WithoutQrCode_GeneratesPdf()
    {
        // Arrange
        var config = new LabelConfiguration
        {
            LabelPrefix = "TEXT-",
            LabelCount = 5,
            IncludeQrCode = false
        };

        var generator = new LabelGenerator(config);

        // Act
        var pdf = generator.GenerateLabelsPdf();

        // Assert
        await Assert.That(pdf).IsNotNull();
        await Assert.That(pdf.Length).IsGreaterThan(0);
    }

    [Test]
    public async Task TestLabelGenerator_LargeQrTemplate_GeneratesPdf()
    {
        // Arrange
        var config = new LabelConfiguration
        {
            LabelPrefix = "ASN-",
            LabelCount = 5,
            IncludeQrCode = true,
            QrCodeDataTemplate = "https://paperless.local/search?q={label}"
        };

        var generator = new LabelGenerator(config);

        // Act
        var pdf = generator.GenerateLabelsPdf();

        // Assert
        await Assert.That(pdf).IsNotNull();
        await Assert.That(pdf.Length).IsGreaterThan(1000);
    }

    [Test]
    public async Task TestLabelGenerator_FullSheet_GeneratesPdf()
    {
        // Arrange - Generate a full sheet (189 labels)
        var config = new LabelConfiguration
        {
            LabelPrefix = "ASN-",
            LabelCount = 189
        };

        var generator = new LabelGenerator(config);

        // Act
        var pdf = generator.GenerateLabelsPdf();

        // Assert
        await Assert.That(pdf).IsNotNull();
        await Assert.That(pdf.Length).IsGreaterThan(10000); // Full sheet should be decent size
    }

    [Test]
    public async Task TestLabelGenerator_SingleLabel_GeneratesPdf()
    {
        // Arrange - Generate just one label for testing
        var config = new LabelConfiguration
        {
            LabelPrefix = "TEST-",
            LabelCount = 1
        };

        var generator = new LabelGenerator(config);

        // Act
        var pdf = generator.GenerateLabelsPdf();

        // Assert
        await Assert.That(pdf).IsNotNull();
        await Assert.That(pdf.Length).IsGreaterThan(0);
    }

    [Test]
    public async Task TestLabelGenerator_ContinuationSheet_GeneratesPdf()
    {
        // Arrange - Simulate continuation from a previous sheet
        var config = new LabelConfiguration
        {
            LabelPrefix = "ASN-",
            StartingNumber = 190,  // Continue from previous sheet
            LabelCount = 100
        };

        var generator = new LabelGenerator(config);

        // Act
        var pdf = generator.GenerateLabelsPdf();

        // Assert
        await Assert.That(pdf).IsNotNull();
        await Assert.That(pdf.Length).IsGreaterThan(0);
    }

    [Test]
    [Arguments(5)]
    [Arguments(50)]
    [Arguments(189)]
    public async Task TestLabelGenerator_VariousLabelCounts_GeneratesPdf(int labelCount)
    {
        // Arrange
        var config = new LabelConfiguration
        {
            LabelPrefix = "ASN-",
            LabelCount = labelCount
        };

        var generator = new LabelGenerator(config);

        // Act
        var pdf = generator.GenerateLabelsPdf();

        // Assert
        await Assert.That(pdf).IsNotNull();
        await Assert.That(pdf.Length).IsGreaterThan(0);
    }

    [Test]
    public async Task TestLabelGenerator_NullConfiguration_ThrowsException()
    {
        // Act & Assert
        ArgumentNullException? exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => Task.Run(() => new LabelGenerator(null!))
        );

        await Assert.That(exception.ParamName).IsEqualTo("config");
    }

    [Test]
    public async Task TestLabelConfiguration_DefaultValues()
    {
        // Arrange & Act
        var config = new LabelConfiguration();

        // Assert
        await Assert.That(config.LabelPrefix).IsEqualTo("ASN");
        await Assert.That(config.StartingNumber).IsEqualTo(1);
        await Assert.That(config.PaddingZeros).IsEqualTo(4);
        await Assert.That(config.LabelFormat).IsEqualTo("avery-l4731");
        await Assert.That(config.LabelCount).IsEqualTo(189);
        await Assert.That(config.FontSize).IsEqualTo(10);
        await Assert.That(config.IncludeQrCode).IsTrue();
        await Assert.That(config.QrCodeSizeMm).IsEqualTo(8f);
        await Assert.That(Math.Abs(config.QrCodeSizeMm - 8f) < 0.01f).IsTrue();
        await Assert.That(config.QrCodeDataTemplate).IsNull();
    }
}