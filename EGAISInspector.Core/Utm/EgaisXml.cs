using System.Xml.Linq;
namespace EGAISInspector.Core.Utm;
public static class EgaisXml
{
    const string Ns="http://fsrar.ru/WEGAIS/WB_DOC_SINGLE_01", Qf="http://fsrar.ru/WEGAIS/QueryFormAB";
    static XDocument Wrap(string id,XElement doc)=>new(new XDeclaration("1.0","UTF-8",null),new XElement(XName.Get("Documents",Ns),new XAttribute("Version","2.0"),new XAttribute(XNamespace.Xmlns+"ns",Ns),new XAttribute(XNamespace.Xmlns+"qf",Qf),new XElement(XName.Get("Owner",Ns),new XElement(XName.Get("FSRAR_ID",Ns),id)),new XElement(XName.Get("Document",Ns),doc)));
    static XDocument Query(string id,string name,string reg)=>Wrap(id,new XElement(XName.Get(name,Ns),new XElement(XName.Get("FormRegId",Qf),reg)));
    public static XDocument QueryFormA(string id,string reg)=>Query(id,"QueryFormA",reg);
    public static XDocument QueryFormB(string id,string reg)=>Query(id,"QueryFormB",reg);
    public static XDocument QueryFormBHistory(string id,string reg)=>Query(id,"QueryFormBHistory",reg);
    public static XDocument QueryRests(string id)=>Wrap(id,new XElement(XName.Get("QueryRests",Ns)));
}
