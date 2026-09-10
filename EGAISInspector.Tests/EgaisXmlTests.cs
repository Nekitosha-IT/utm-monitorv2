using EGAISInspector.Core.Utm;
using Xunit;
namespace EGAISInspector.Tests;
public class EgaisXmlTests
{
 [Fact] public void QueryFormB_UsesDocumentsV2(){var x=EgaisXml.QueryFormB("3463047","FB-123");Assert.Equal("2.0",x.Root?.Attribute("Version")?.Value);Assert.Equal("3463047",x.Descendants().First(e=>e.Name.LocalName=="FSRAR_ID").Value);Assert.Equal("FB-123",x.Descendants().First(e=>e.Name.LocalName=="FormRegId").Value);}
 [Fact] public void QueryBarcode_ContainsSeriesAndNumber(){var x=QueryBarcodeBuilder.Build("3463047","331","48865816");Assert.Equal("331",x.Descendants().First(e=>e.Name.LocalName=="Series").Value);Assert.Equal("48865816",x.Descendants().First(e=>e.Name.LocalName=="Number").Value);}
}
