using EGAISInspector.Core.Barcodes;
using EGAISInspector.Core.Utm;
using Xunit;

namespace EGAISInspector.Tests;

public class EgaisXmlTests
{
    [Fact]
    public void QueryFormB_UsesDocumentsV2()
    {
        var x = EgaisXml.QueryFormB("3463047", "FB-123");
        Assert.Equal("2.0", x.Root?.Attribute("Version")?.Value);
        Assert.Equal("3463047", x.Descendants().First(e => e.Name.LocalName == "FSRAR_ID").Value);
        Assert.Equal("FB-123", x.Descendants().First(e => e.Name.LocalName == "FormRegId").Value);
    }

    [Fact]
    public void QueryBarcode_ContainsSchemaFields()
    {
        var x = QueryBarcodeBuilder.Build("3463047", "187", "331", "48865816", "EGMON-TEST-001");
        Assert.Equal("2.0", x.Root?.Attribute("Version")?.Value);
        Assert.Equal("187", x.Descendants().First(e => e.Name.LocalName == "Type").Value);
        Assert.Equal("331", x.Descendants().First(e => e.Name.LocalName == "Rank").Value);
        Assert.Equal("48865816", x.Descendants().First(e => e.Name.LocalName == "Number").Value);
        Assert.Equal("EGMON-TEST-001", x.Descendants().First(e => e.Name.LocalName == "QueryNumber").Value);
        Assert.NotNull(x.Descendants().FirstOrDefault(e => e.Name.LocalName == "Date"));
    }

    [Fact]
    public void NativeEgaisDataMatrix_IsDecoded()
    {
        var raw = "18733148865816" + new string('X', 136);
        var mark = new BarcodeDecoder().Decode(raw);
        Assert.Equal(BarcodeKind.DataMatrix, mark.Kind);
        Assert.Equal("187", mark.Type);
        Assert.Equal("331", mark.Series);
        Assert.Equal("48865816", mark.Number);
    }
}
