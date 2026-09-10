using System.Xml.Linq;
namespace EGAISInspector.Core.Utm;
public static class QueryBarcodeBuilder
{
 const string Ns="http://fsrar.ru/WEGAIS/WB_DOC_SINGLE_01", Qb="http://fsrar.ru/WEGAIS/QueryBarcode";
 public static XDocument Build(string fsrarId,string series,string number)=>new(new XDeclaration("1.0","UTF-8",null),new XElement(XName.Get("Documents",Ns),new XAttribute("Version","2.0"),new XAttribute(XNamespace.Xmlns+"ns",Ns),new XAttribute(XNamespace.Xmlns+"qb",Qb),new XElement(XName.Get("Owner",Ns),new XElement(XName.Get("FSRAR_ID",Ns),fsrarId)),new XElement(XName.Get("Document",Ns),new XElement(XName.Get("QueryBarcode",Ns),new XElement(XName.Get("Barcode",Qb),new XElement(XName.Get("Series",Qb),series),new XElement(XName.Get("Number",Qb),number))))));
}
