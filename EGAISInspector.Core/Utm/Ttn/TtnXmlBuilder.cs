using System.Xml.Linq;

namespace EGAISInspector.Core.Utm.Ttn;

public static class TtnXmlBuilder
{
    private static readonly XNamespace Ns = "http://fsrar.ru/WEGAIS/WB_DOC_SINGLE_01";
    private static readonly XNamespace Qp = "http://fsrar.ru/WEGAIS/QueryParameters";
    private static readonly XNamespace Wa = "http://fsrar.ru/WEGAIS/ActTTNSingle_v2";

    public static XDocument BuildQueryResendDoc(string fsrarId, string wbRegId)
    {
        Require(fsrarId, nameof(fsrarId));
        Require(wbRegId, nameof(wbRegId));
        return Documents(fsrarId,
            new XElement(Ns + "QueryResendDoc",
                Parameters(("WBREGID", wbRegId))));
    }

    public static XDocument BuildWayBillAct(WayBillActRequest request)
    {
        Require(request.FsrarId, nameof(request.FsrarId));
        Require(request.WayBillRegId, nameof(request.WayBillRegId));
        Require(request.ActNumber, nameof(request.ActNumber));

        var header = new XElement(Wa + "Header",
            new XElement(Wa + "IsAccept", request.IsAccepted ? "Accepted" : "Rejected"),
            new XElement(Wa + "ACTNUMBER", request.ActNumber),
            new XElement(Wa + "ActDate", request.ActDate.ToString("yyyy-MM-dd")),
            new XElement(Wa + "WBRegId", request.WayBillRegId));

        if (!string.IsNullOrWhiteSpace(request.Note))
            header.Add(new XElement(Wa + "Note", request.Note));

        var content = new XElement(Wa + "Content");
        foreach (var item in request.Items ?? Array.Empty<WayBillActItem>())
        {
            Require(item.AlcoholCode, nameof(item.AlcoholCode));
            var position = new XElement(Wa + "Position",
                new XElement(Wa + "Identity", item.AlcoholCode),
                new XElement(Wa + "Quantity", item.Quantity.ToString(System.Globalization.CultureInfo.InvariantCulture)));

            if (item.Volume is not null)
                position.Add(new XElement(Wa + "Volume", item.Volume.Value.ToString(System.Globalization.CultureInfo.InvariantCulture)));

            foreach (var mark in item.Marks ?? Array.Empty<string>())
                position.Add(new XElement(Wa + "MarkCode", mark));

            content.Add(position);
        }

        return Documents(request.FsrarId,
            new XElement(Ns + "WayBillAct",
                header,
                content));
    }

    private static XElement Parameters(params (string Name, string Value)[] values) =>
        new(Qp + "Parameters",
            values.Select(x => new XElement(Qp + "Parameter",
                new XElement(Qp + "Name", x.Name),
                new XElement(Qp + "Value", x.Value))));

    private static XDocument Documents(string fsrarId, XElement document) =>
        new(new XDeclaration("1.0", "UTF-8", null),
            new XElement(Ns + "Documents",
                new XAttribute("Version", "1.0"),
                new XAttribute(XNamespace.Xmlns + "ns", Ns),
                new XAttribute(XNamespace.Xmlns + "qp", Qp),
                new XAttribute(XNamespace.Xmlns + "wa", Wa),
                new XElement(Ns + "Owner",
                    new XElement(Ns + "FSRAR_ID", fsrarId)),
                new XElement(Ns + "Document", document)));

    private static void Require(string value, string name)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Значение обязательно.", name);
    }
}
