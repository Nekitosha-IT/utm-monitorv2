using System.Xml.Linq;
namespace EGAISInspector.Core.Utm;
public sealed record UtmQueueItem(string Url,string? Id,string? Type);
public sealed record EgaisReply(string? Type,string? RegId,string? InformARegId,string? InformBRegId,string? ProductName,string? Producer,string? BottlingDate,decimal? Quantity,string RawXml);
public static class EgaisResponseParser
{
 public static IReadOnlyList<UtmQueueItem> ParseQueue(XDocument doc)=>doc.Descendants().Where(x=>x.Name.LocalName.Equals("url",StringComparison.OrdinalIgnoreCase)).Select(x=>new UtmQueueItem(x.Value,x.Attribute("replyId")?.Value,x.Parent?.Name.LocalName)).ToList();
 public static EgaisReply ParseReply(XDocument doc){var root=doc.Root;var type=root?.Descendants().FirstOrDefault(x=>x.Name.LocalName.StartsWith("Reply",StringComparison.OrdinalIgnoreCase))?.Name.LocalName;string? V(params string[] names)=>names.Select(n=>root?.Descendants().FirstOrDefault(x=>x.Name.LocalName==n)?.Value).FirstOrDefault(v=>!string.IsNullOrWhiteSpace(v));decimal? D(string n){var s=V(n);return decimal.TryParse(s,System.Globalization.NumberStyles.Any,System.Globalization.CultureInfo.InvariantCulture,out var d)?d:null;}return new(type,V("RegId","FormRegId"),V("InformARegId","InformA"),V("InformBRegId","InformB"),V("FullName","ProductName","Name"),V("ProducerName","Producer"),V("BottlingDate","FillingDate","ProductionDate"),D("Quantity"),doc.ToString(SaveOptions.DisableFormatting));}
}
