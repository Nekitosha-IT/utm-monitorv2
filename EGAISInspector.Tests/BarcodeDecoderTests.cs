using EGAISInspector.Core.Barcodes;

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
}
