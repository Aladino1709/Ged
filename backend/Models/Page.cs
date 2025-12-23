using System;

public class Page
{
    public int Id { get; set; }
    public int DocumentId { get; set; }
    public int PageNumber { get; set; }
    public uint Oid { get; set; }
    public string? MimeType { get; set; }
    public long SizeBytes { get; set; }
    public DateTime CreatedAt { get; set; }
}
