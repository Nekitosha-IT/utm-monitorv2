using System.Net.Http;
using System.Xml;
using System.Xml.Linq;
using EGAISInspector.Core.Database;
using Microsoft.EntityFrameworkCore;

namespace EGAISInspector.Core.Utm;

public sealed record SyncResult(int Found, int Downloaded, int Saved, int Failed, IReadOnlyList<string> Errors);

public sealed class UtmInboxSyncService(UtmClient client, EgaisDbContext db)
{
    public async Task<SyncResult> SyncAsync(CancellationToken ct = default)
    {
        var errors = new List<string>();
        var found = 0;
        var downloaded = 0;
        var saved = 0;

        var queueResult = await client.GetOutputQueueAsync(ct);
        if (!queueResult.Success)
        {
            errors.Add($"Не удалось получить очередь УТМ: {queueResult.Error ?? $"HTTP {(int)queueResult.StatusCode}"}");
            return new(0, 0, 0, 1, errors);
        }

        XDocument queue;
        try
        {
            queue = XDocument.Parse(queueResult.Content, LoadOptions.PreserveWhitespace);
        }
        catch (XmlException ex)
        {
            errors.Add($"УТМ вернул некорректный XML очереди: {ex.Message}");
            return new(0, 0, 0, 1, errors);
        }

        var items = EgaisResponseParser.ParseQueue(queue);
        found = items.Count;

        foreach (var item in items)
        {
            ct.ThrowIfCancellationRequested();
            if (!Uri.TryCreate(item.Url, UriKind.Absolute, out var absolute))
                absolute = new Uri(new Uri("http://localhost"), item.Url);

            var path = absolute.AbsolutePath;
            var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
            if (segments.Length < 2)
            {
                errors.Add($"Некорректный URL очереди: {item.Url}");
                continue;
            }

            var type = Uri.UnescapeDataString(segments[^2]);
            var id = Uri.UnescapeDataString(segments[^1]);
            var externalId = !string.IsNullOrWhiteSpace(item.Id) ? item.Id! : $"{type}:{id}";

            try
            {
                var documentResult = await client.GetDocumentAsync(type, id, ct);
                if (!documentResult.Success)
                {
                    errors.Add($"{type}/{id}: {documentResult.Error ?? $"HTTP {(int)documentResult.StatusCode}"}");
                    continue;
                }

                downloaded++;

                XDocument document;
                try
                {
                    document = XDocument.Parse(documentResult.Content, LoadOptions.PreserveWhitespace);
                }
                catch (XmlException ex)
                {
                    errors.Add($"{type}/{id}: некорректный XML: {ex.Message}");
                    continue;
                }

                foreach (var parsed in EgaisDocumentParser.ParseMany(document))
                {
                    var key = externalId;
                    if (!string.IsNullOrWhiteSpace(parsed.RegId))
                        key = $"{type}:{parsed.RegId}";

                    var entity = await db.EgaisDocuments.FirstOrDefaultAsync(x => x.ExternalId == key, ct);
                    if (entity is null)
                    {
                        entity = new EgaisDocument
                        {
                            ExternalId = key,
                            FirstSeenUtc = DateTime.UtcNow
                        };
                        db.EgaisDocuments.Add(entity);
                    }

                    entity.DocumentType = parsed.DocumentType;
                    entity.RegId = parsed.RegId;
                    entity.InformARegId = parsed.InformARegId;
                    entity.InformBRegId = parsed.InformBRegId;
                    entity.Number = parsed.Number;
                    entity.DocumentDate = parsed.DocumentDate;
                    entity.Sender = parsed.Sender;
                    entity.Recipient = parsed.Recipient;
                    entity.ProductName = parsed.ProductName;
                    entity.Producer = parsed.Producer;
                    entity.Quantity = parsed.Quantity;
                    entity.Volume = parsed.Volume;
                    entity.Strength = parsed.Strength;
                    entity.RawXml = parsed.RawXml;
                    entity.LastSeenUtc = DateTime.UtcNow;
                    saved++;
                }

                await db.SaveChangesAsync(ct);

                if (!string.IsNullOrWhiteSpace(item.Id))
                {
                    var deleteResult = await client.DeleteOutputAsync(item.Id!, ct);
                    if (!deleteResult.Success)
                        errors.Add($"{type}/{id}: документ сохранён, но не удалён из очереди: {deleteResult.Error ?? $"HTTP {(int)deleteResult.StatusCode}"}");
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or InvalidOperationException)
            {
                errors.Add($"{type}/{id}: {ex.Message}");
            }
        }

        return new SyncResult(found, downloaded, saved, errors.Count, errors);
    }
}
