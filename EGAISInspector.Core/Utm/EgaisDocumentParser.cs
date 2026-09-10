using System.Globalization;
using System.Xml.Linq;
using EGAISInspector.Core.Database;

namespace EGAISInspector.Core.Utm;

public sealed record ParsedDocument(
    string DocumentType,
    string? RegId,
    string? InformARegId,
    string? InformBRegId,
    string? Number,
    DateTime? DocumentDate,
    string? Sender,
    string? Recipient,
    string? ProductName,
    string? Producer,
    decimal? Quantity,
    decimal? Volume,
    decimal? Strength,
    string? RawXml);

public static class EgaisDocumentParser
{
    public static ParsedDocument Parse(XDocument document)
    {
        var root = document.Root;
        var type = FindDocumentType(root) ?? "Unknown";
        string? Value(params string[] names) => FirstValue(root, names);

        return new ParsedDocument(
            type,
            Value("RegId", "FormRegId", "WayBillRegId", "TTNRegId"),
            Value("InformARegId", "InformF1RegId", "InformA"),
            Value("InformBRegId", "InformF2RegId", "InformB"),
            Value("Number", "WayBillNumber", "TTNNumber", "WBNumber"),
            ParseDate(Value("Date", "DocumentDate", "WayBillDate", "TTNDate")),
            FirstPartyName(root, "Shipper", "Sender", "Consignor", "Producer"),
            FirstPartyName(root, "Consignee", "Recipient", "Receiver", "Buyer"),
            Value("FullName", "ProductName", "Name"),
            FirstPartyName(root, "Producer", "Manufacturer", "ManufacturerName"),
            ParseDecimal(Value("Quantity", "Qty", "Amount")),
            ParseDecimal(Value("Volume", "VolumeAlc", "Capacity")),
            ParseDecimal(Value("AlcCode", "Strength", "AlcoholByVolume")),
            document.ToString(SaveOptions.DisableFormatting));
    }

    public static IEnumerable<ParsedDocument> ParseMany(XDocument document)
    {
        var documents = document.Descendants()
            .Where(x => IsKnownDocument(x.Name.LocalName))
            .ToList();

        if (documents.Count == 0)
        {
            yield return Parse(document);
            yield break;
        }

        foreach (var node in documents)
            yield return Parse(new XDocument(new XElement(node)));
    }

    private static bool IsKnownDocument(string name) => name is
        "ReplyFormA" or "ReplyFormB" or "ReplyFormBHistory" or
        "WayBill" or "WayBillAct" or "WayBillTicket" or
        "TTNInformBReg" or "ReplyRests" or "QueryRests";

    private static string? FindDocumentType(XElement? root)
    {
        if (root is null) return null;
        return root.DescendantsAndSelf()
            .Select(x => x.Name.LocalName)
            .FirstOrDefault(IsKnownDocument);
    }

    private static string? FirstValue(XElement? root, params string[] names)
    {
        if (root is null) return null;
        foreach (var name in names)
        {
            var node = root.DescendantsAndSelf()
                .FirstOrDefault(x => x.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (!string.IsNullOrWhiteSpace(node?.Value))
                return node.Value.Trim();
        }
        return null;
    }

    private static string? FirstPartyName(XElement? root, params string[] containers)
    {
        if (root is null) return null;
        foreach (var container in containers)
        {
            var node = root.Descendants().FirstOrDefault(x =>
                x.Name.LocalName.Equals(container, StringComparison.OrdinalIgnoreCase));
            if (node is null) continue;
            var value = node.DescendantsAndSelf().FirstOrDefault(x =>
                x.Name.LocalName is "FullName" or "Name" or "ClientName" or "ShortName");
            if (!string.IsNullOrWhiteSpace(value?.Value)) return value.Value.Trim();
        }
        return null;
    }

    private static DateTime? ParseDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal, out var date))
            return date;
        return null;
    }

    private static decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;
        if (decimal.TryParse(value, NumberStyles.Any, CultureInfo.GetCultureInfo("ru-RU"), out result))
            return result;
        return null;
    }
}
