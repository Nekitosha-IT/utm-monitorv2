using EGAISInspector.Core.Barcodes;

namespace EGAISInspector.Tests;

public sealed class BarcodeDecoderTests
{
    [Fact]
    public void DecodesDataMatrixStructure()
    {
        var raw = "187331488658160225001HN2DV4TEST";
        var result = new BarcodeDecoder().Decode(raw);
        Assert.True(result.IsDataMatrix);
        Assert.Equal("187", result.Type);
        Assert.Equal("331", result.Series);
        Assert.Equal("48865816", result.Number);
    }
}
