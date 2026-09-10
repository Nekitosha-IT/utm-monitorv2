using System.Xml.Linq;

namespace EGAISInspector.Core.Utm;

public static class QueryBarcodeBuilder
{
    private const string Ns = "http://fsrar.ru/WEGAIS/WB_DOC_SINGLE_01";
    private const string Bk = "http://fsrar.ru/WEGAIS/QueryBarcode";
    private const string Ce = "http://fsrar.ru/WEGAIS/CommonEnum";

    public static XDocument Build(string fsrarId, string type, string series, string number, string queryNumber = "1", DateTime? date = null)
    {
        if (string.IsNullOrWhiteSpace(fsrarId)) throw new ArgumentException("FSRAR_ID is required.", nameof(fsrarId));
        if (string.IsNullOrWhiteSpace(type)) throw new ArgumentException("Mark type is required.", nameof(type));
        if (string.IsNullOrWhiteSpace(series)) throw new ArgumentException("Mark series is required.", nameof(series));
        if (string.IsNullOrWhiteSpace(number)) throw new ArgumentException("Mark number is required.", nameof(number));

        var when = (date ?? DateTime.Now).ToString("yyyy-MM-dd'T'HH:mm:ss");
        return new XDocument(
            new XDeclaration("1.0", "UTF-8", null),
            new XElement(XName.Get("Documents", Ns),
                new XAttribute("Version", "2.0"),
                new XAttribute(XNamespace.Xmlns + "ns", Ns),
                new XAttribute(XNamespace.Xmlns + "bk", Bk),
                new XAttribute(XNamespace.Xmlns + "ce", Ce),
                new XElement(XName.Get("Owner", Ns),
                    new XElement(XName.Get("FSRAR_ID", Ns), fsrarId)),
                new XElement(XName.Get("Document", Ns),
                    new XElement(XName.Get("QueryBarcode", Ns),
                        new XElement(XName.Get("QueryNumber", Bk), queryNumber),
                        new XElement(XName.Get("Date", Bk), when),
                        new XElement(XName.Get("Marks", Bk),
                            new XElement(XName.Get("Mark", Bk),
                                new XElement(XName.Get("Identity", Bk), "1"),
                                new XElement(XName.Get("Type", Bk), type),
                                new XElement(XName.Get("Rank", Bk), series),
                                new XElement(XName.Get("Number", Bk), number)))))));
    }
}
