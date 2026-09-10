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
        if (raw.Length == 0)
            return new(BarcodeKind.Unknown, raw, null, null, null, null, null, null);

        if (LooksLikeEgaisDataMatrix(raw))
            return DecodeEgaisDataMatrix(raw);

        return new(BarcodeKind.Pdf417, raw, null, null, null, null, null, null);
    }

    private static bool LooksLikeEgaisDataMatrix(string raw)
    {
        // Native ЕГАИС payload: type(3)+series(3)+number(8)+service(7)+signature.
        // Real UTM payloads are 150 chars, while shortened payloads are useful in tests.
        if (raw.Length >= 22 && raw.Take(14).All(char.IsDigit))
            return true;

        // GS1 DataMatrix from scanners can contain separators and application identifiers.
        return raw.StartsWith("01", StringComparison.Ordinal) || raw.Contains('\u001d');
    }

    private static BarcodeMark DecodeEgaisDataMatrix(string raw)
    {
        var normalized = raw.Replace("\u001d", "");
        var type = normalized.Length >= 3 ? normalized[..3] : null;
        var series = normalized.Length >= 6 ? normalized.Substring(3, 3) : null;
        var number = normalized.Length >= 14 ? normalized.Substring(6, 8) : null;
        var service = normalized.Length > 14 ? normalized.Substring(14, Math.Min(7, normalized.Length - 14)) : null;
        var signature = normalized.Length > 21 ? normalized[21..] : null;
        return new(BarcodeKind.DataMatrix, raw, type, series, number, service, signature, null);
    }
}
