using System.Xml.Linq;
namespace EGAISInspector.Core.Utm;
public sealed class UtmRequests(UtmClient client)
{
    public Task<XDocument> QueryFormAAsync(string fsrarId,string reg,CancellationToken ct=default)=>client.PostXmlAsync("/opt/in/QueryFormA",EgaisXml.QueryFormA(fsrarId,reg),ct);
    public Task<XDocument> QueryFormBAsync(string fsrarId,string reg,CancellationToken ct=default)=>client.PostXmlAsync("/opt/in/QueryFormB",EgaisXml.QueryFormB(fsrarId,reg),ct);
    public Task<XDocument> QueryFormBHistoryAsync(string fsrarId,string reg,CancellationToken ct=default)=>client.PostXmlAsync("/opt/in/QueryFormBHistory",EgaisXml.QueryFormBHistory(fsrarId,reg),ct);
    public Task<XDocument> QueryRestsAsync(string fsrarId,CancellationToken ct=default)=>client.PostXmlAsync("/opt/in/QueryRests",EgaisXml.QueryRests(fsrarId),ct);
}
