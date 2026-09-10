namespace EGAISInspector.Core.Utm.Ttn;

public sealed record TtnItem(
    string AlcoholCode,
    decimal Quantity,
    decimal? Volume = null,
    decimal? Strength = null,
    string? ProductName = null,
    IReadOnlyList<string>? Marks = null);

public sealed record TtnSummary(
    string? Identity,
    string? RegId,
    string? Number,
    DateTime? Date,
    string? Shipper,
    string? Consignee,
    string? Status,
    IReadOnlyList<TtnItem> Items,
    string RawXml);

public sealed record WayBillActItem(
    string AlcoholCode,
    decimal Quantity,
    decimal? Volume = null,
    IReadOnlyList<string>? Marks = null);

public sealed record WayBillActRequest(
    string FsrarId,
    string WayBillRegId,
    string ActNumber,
    DateTime ActDate,
    bool IsAccepted,
    string? Note = null,
    IReadOnlyList<WayBillActItem>? Items = null);

public sealed record TtnOperationResult(
    bool Success,
    string? ReplyId,
    string? TransportId,
    string RawXml,
    string? Error = null);
