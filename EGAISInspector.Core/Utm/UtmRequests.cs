using System.Text;
using System.Xml.Linq;
namespace EGAISInspector.Core.Utm;
public sealed class UtmRequests(UtmClient client)
{
    public Task<HttpResponseMessage> QueryFormAAsync(string fsrarId,string reg,CancellationToken ct=default)=>PostAsync("QueryFormA",EgaisXml.QueryFormA(fsrarId,reg),ct);
    public Task<HttpResponseMessage> QueryFormBAsync(string fsrarId,string reg,CancellationToken ct=default)=>PostAsync("QueryFormB",EgaisXml.QueryFormB(fsrarId,reg),ct);
    public Task<HttpResponseMessage> QueryFormBHistoryAsync(string fsrarId,string reg,CancellationToken ct=default)=>PostAsync("QueryFormBHistory",EgaisXml.QueryFormBHistory(fsrarId,reg),ct);
    public Task<HttpResponseMessage> QueryRestsAsync(string fsrarId,CancellationToken ct=default)=>PostAsync("QueryRests",EgaisXml.QueryRests(fsrarId),ct);
    async Task<HttpResponseMessage> PostAsync(string endpoint,XDocument xml,CancellationToken ct){using var content=new StringContent(xml.ToString(SaveOptions.DisableFormatting),Encoding.UTF8,"application/xml");return await client.PostAsync($"/opt/in/{endpoint}",content,ct);}
}
