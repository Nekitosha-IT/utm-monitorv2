using EGAISInspector.Core.Database;
using EGAISInspector.Core.Barcodes;
using Microsoft.EntityFrameworkCore;
namespace EGAISInspector.Core.Services;
public sealed class MarkAnalysisService(EgaisDbContext db)
{
 public async Task<Bottle?> FindAsync(string raw,CancellationToken ct=default){var mark=BarcodeDecoder.Decode(raw);if(mark is null)return null;return await db.Bottles.AsNoTracking().FirstOrDefaultAsync(x=>x.Mark==mark.Value.Raw,ct);}
 public async Task<Bottle> UpsertAsync(string raw,string? product,string? producer,string? informB,string? informA,CancellationToken ct=default){var mark=BarcodeDecoder.Decode(raw)?.Raw??raw.Trim();var b=await db.Bottles.FirstOrDefaultAsync(x=>x.Mark==mark,ct)??new Bottle{Mark=mark};b.ProductName=product??b.ProductName;b.Producer=producer??b.Producer;b.InformBRegId=informB??b.InformBRegId;b.InformARegId=informA??b.InformARegId;b.Status="Найдено";b.LastSeenUtc=DateTime.UtcNow;db.Update(b);await db.SaveChangesAsync(ct);return b;}
}
