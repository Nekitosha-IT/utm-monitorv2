namespace EGAISInspector.Core.Barcodes;

public enum BarcodeKind { Unknown, DataMatrix, Pdf417 }

public sealed record BarcodeMark(
    BarcodeKind Kind,
    string Raw,
    string? Type,
    string? Series,
    string? Number,
    string? ServicePart,
    string? Signature,
    string? AlcoholCode);

public sealed class BarcodeDecoder
{
    public BarcodeMark Decode(string raw)
    {
        raw = raw.Trim();
        if (raw.Length == 0) return new(BarcodeKind.Unknown, raw, null, null, null, null, null, null);

        if (raw.StartsWith("01", StringComparison.Ordinal) || raw.Contains("", StringComparison.Ordinal))
            return DecodeDataMatrix(raw);

        return new(BarcodeKind.Pdf417, raw, null, null, null, null, null, null);
    }

    private static BarcodeMark DecodeDataMatrix(string raw)
    {
        var normalized = raw.Replace("\u001d", "");
        string? type = normalized.Length >= 3 ? normalized[..3] : null;
        string? series = normalized.Length >= 6 ? normalized.Substring(3, 3) : null;
        string? number = normalized.Length >= 14 ? normalized.Substring(6, 8) : null;
        string? service = normalized.Length > 14 ? normalized.Substring(14, Math.Min(7, normalized.Length - 14)) : null;
        string? signature = normalized.Length > 21 ? normalized[21..] : null;
        return new(BarcodeKind.DataMatrix, raw, type, series, number, service, signature, null);
    }
}
