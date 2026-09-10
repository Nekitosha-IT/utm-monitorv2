using EGAISInspector.Core.Barcodes;
using Xunit;

namespace EGAISInspector.Tests;

public class BarcodeDecoderTests
{
    [Fact]
    public void Decode_DataMatrix_ReturnsExpectedParts()
    {
        var decoder = new BarcodeDecoder();
        var result = decoder.Decode("187331488658160225001HN2DV4TEST");
        Assert.Equal(BarcodeKind.DataMatrix, result.Kind);
        Assert.Equal("187", result.Type);
        Assert.Equal("331", result.Series);
        Assert.Equal("48865816", result.Number);
    }

    [Fact]
    public void Decode_Empty_ReturnsUnknown()
    {
        var result = new BarcodeDecoder().Decode("   ");
        Assert.Equal(BarcodeKind.Unknown, result.Kind);
    }

    [Fact]
    public void Decode_OrdinaryPayload_RemainsPdf417()
    {
        var result = new BarcodeDecoder().Decode("1234567890");
        Assert.Equal(BarcodeKind.Pdf417, result.Kind);
    }
}
