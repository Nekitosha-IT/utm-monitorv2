using System.Xml.Linq;

namespace EGAISInspector.Core.Xml;

public static class EgaisXmlTemplates
{
    private static readonly XNamespace Ns = "http://fsrar.ru/WEGAIS/WB_DOC_SINGLE_01";

    public static string QueryFormA(string fsrId, string informARegId) =>
        new XDocument(new XElement(Ns + "Documents",
            new XAttribute("Version", "2.0"),
            new XElement(Ns + "QueryFormA", new XAttribute("FSRAR_ID", fsrId),
                new XElement(Ns + "InformARegId", informARegId)))).ToString(SaveOptions.DisableFormatting);

    public static string QueryFormB(string fsrId, string informBRegId) =>
        new XDocument(new XElement(Ns + "Documents",
            new XAttribute("Version", "2.0"),
            new XElement(Ns + "QueryFormB", new XAttribute("FSRAR_ID", fsrId),
                new XElement(Ns + "InformBRegId", informBRegId)))).ToString(SaveOptions.DisableFormatting);

    public static string QueryFormBHistory(string fsrId, string informBRegId) =>
        new XDocument(new XElement(Ns + "Documents",
            new XAttribute("Version", "2.0"),
            new XElement(Ns + "QueryFormBHistory", new XAttribute("FSRAR_ID", fsrId),
                new XElement(Ns + "InformBRegId", informBRegId)))).ToString(SaveOptions.DisableFormatting);

    public static string QueryRests(string fsrId) =>
        new XDocument(new XElement(Ns + "Documents",
            new XAttribute("Version", "2.0"),
            new XElement(Ns + "QueryRests", new XAttribute("FSRAR_ID", fsrId)))).ToString(SaveOptions.DisableFormatting);
}
