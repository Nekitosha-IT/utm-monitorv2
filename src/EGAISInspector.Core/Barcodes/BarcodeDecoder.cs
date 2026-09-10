using System.Text;
using ZXing;
using ZXing.Common;

namespace EGAISInspector.Core.Barcodes;

public sealed record EgaisMark(
    string Raw,
    string Format,
    string? Type,
    string? Series,
    string? Number,
    string? ServicePart,
    string? Signature,
    string? AlcoholCode,
    bool IsDataMatrix,
    bool IsValid);

public interface IBarcodeDecoder
{
    EgaisMark Decode(string raw);
    EgaisMark Decode(ReadOnlySpan<byte> imageBytes);
}

public sealed class BarcodeDecoder : IBarcodeDecoder
{
    public EgaisMark Decode(string raw)
    {
        raw = raw.Trim();
        if (raw.Length >= 20 && raw.Length <= 256)
        {
            var type = raw[..3];
            var series = raw.Length >= 6 ? raw[3..6] : null;
            var number = raw.Length >= 14 ? raw[6..14] : null;
            var service = raw.Length >= 21 ? raw[14..21] : null;
            var signature = raw.Length > 21 ? raw[21..] : null;
            return new EgaisMark(raw, "DataMatrix", type, series, number, service, signature, null, true, raw.Length >= 20);
        }

        return new EgaisMark(raw, "PDF417/Unknown", null, null, null, null, null, DecodeLegacyAlcoholCode(raw), false, !string.IsNullOrWhiteSpace(raw));
    }

    public EgaisMark Decode(ReadOnlySpan<byte> imageBytes)
    {
        var luminance = new RGBLuminanceSource(imageBytes.ToArray(), 1, imageBytes.Length, RGBLuminanceSource.BitmapFormat.Gray8);
        var bitmap = new BinaryBitmap(new HybridBinarizer(luminance));
        var result = new MultiFormatReader().decode(bitmap);
        return Decode(result?.Text ?? Encoding.UTF8.GetString(imageBytes));
    }

    private static string? DecodeLegacyAlcoholCode(string raw)
    {
        if (raw.Length < 8) return null;
        var token = raw.Split(new[] { ' ', '\t', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
        return string.IsNullOrWhiteSpace(token) ? null : token;
    }
}
