using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Threading.Tasks;
using System.IO;
using ImageMagick;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly string _connString;
    public DocumentsController(IConfiguration config)
    {
        _connString = config.GetConnectionString("DefaultConnection");
    }

    // POST api/documents
    // Body JSON: { "titleNumber": "123/45", "description": "..." }
    [HttpPost]
    public async Task<IActionResult> CreateDocument([FromBody] DocumentCreateDto dto)
    {
        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO documents (title_number, title_type, description, created_by)
            VALUES (@titleNumber, @titleType, @desc, @createdBy)
            RETURNING id;
        ";
        cmd.Parameters.AddWithValue("titleNumber", dto.TitleNumber ?? "");
        cmd.Parameters.AddWithValue("titleType", dto.TitleType ?? "");
        cmd.Parameters.AddWithValue("desc", dto.Description ?? "");
        cmd.Parameters.AddWithValue("createdBy", dto.CreatedBy ?? "");
        var id = (int)await cmd.ExecuteScalarAsync();
        return CreatedAtAction(nameof(GetDocument), new { id }, new { id });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetDocument(int id)
    {
        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT id, title_number, description, created_at FROM documents WHERE id = @id";
        cmd.Parameters.AddWithValue("id", id);
        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return NotFound();
        var doc = new {
            id = reader.GetInt32(0),
            titleNumber = reader.IsDBNull(1) ? null : reader.GetString(1),
            description = reader.IsDBNull(2) ? null : reader.GetString(2),
            createdAt = reader.GetDateTime(3)
        };
        return Ok(doc);
    }

    // POST api/documents/{documentId}/pages
    // multipart/form-data: file, pageNumber
    [HttpPost("{documentId}/pages")]
    public async Task<IActionResult> UploadPage(int documentId)
    {
        var form = await Request.ReadFormAsync();
        var file = form.Files.GetFile("file");
        if (file == null || file.Length == 0) return BadRequest("Fichier manquant");

        if (!int.TryParse(form["pageNumber"], out int pageNumber))
            pageNumber = 1;

        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        var loManager = new NpgsqlTypes.NpgsqlLargeObjectManager(conn);
        uint oid = loManager.Create(); // crée un large object

        // Écrire le contenu uploadé dans le LO
        await using (var loStream = loManager.OpenReadWrite(oid))
        {
            await file.CopyToAsync(loStream);
        }

        // Insérer métadonnées
        await using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO pages (document_id, page_number, oid, mime_type, size_bytes)
            VALUES (@docId, @pageNumber, @oid, @mime, @size)
            RETURNING id;
        ";
        cmd.Parameters.AddWithValue("docId", documentId);
        cmd.Parameters.AddWithValue("pageNumber", pageNumber);
        cmd.Parameters.AddWithValue("oid", (int)oid);
        cmd.Parameters.AddWithValue("mime", file.ContentType ?? "application/octet-stream");
        cmd.Parameters.AddWithValue("size", file.Length);
        var newId = (int)await cmd.ExecuteScalarAsync();
        return CreatedAtAction(nameof(GetPage), new { documentId, pageId = newId }, new { pageId = newId });
    }

    // GET api/documents/{documentId}/pages/{pageId}
    [HttpGet("{documentId}/pages/{pageId}")]
    public async Task<IActionResult> GetPage(int documentId, int pageId)
    {
        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT oid, mime_type FROM pages WHERE id = @id AND document_id = @docId";
        cmd.Parameters.AddWithValue("id", pageId);
        cmd.Parameters.AddWithValue("docId", documentId);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (!await reader.ReadAsync()) return NotFound();

        var oid = (uint)reader.GetInt32(0);
        var mime = reader.GetString(1);

        var loManager = new NpgsqlTypes.NpgsqlLargeObjectManager(conn);
        var loStream = loManager.OpenRead(oid); // stream en lecture

        // Retourner le contenu brut (TIFF possible) — navigateur peut ne pas l'afficher
        return File(loStream, mime, $"page_{pageId}");
    }

    // GET api/documents/{documentId}/pages/{pageId}/preview?pageIndex=0
    [HttpGet("{documentId}/pages/{pageId}/preview")]
    public async Task<IActionResult> GetPagePreview(int documentId, int pageId, [FromQuery] int pageIndex = 0)
    {
        await using var conn = new NpgsqlConnection(_connString);
        await conn.OpenAsync();

        await using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT oid FROM pages WHERE id = @id AND document_id = @docId";
        cmd.Parameters.AddWithValue("id", pageId);
        cmd.Parameters.AddWithValue("docId", documentId);

        var result = await cmd.ExecuteScalarAsync();
        if (result == null) return NotFound();

        var oid = (uint)(int)result;
        var loManager = new NpgsqlTypes.NpgsqlLargeObjectManager(conn);

        using (var loStream = loManager.OpenRead(oid))
        using (var memOut = new MemoryStream())
        {
            using (var images = new MagickImageCollection())
            {
                images.Read(loStream); // lit toutes les pages du TIFF si multi-pages
                if (images.Count == 0) return NotFound("Image vide");
                if (pageIndex < 0 || pageIndex >= images.Count) pageIndex = 0;
                using (var img = images[pageIndex])
                {
                    img.Format = MagickFormat.Jpeg;
                    img.Quality = 85;
                    img.Write(memOut);
                    memOut.Position = 0;
                    return File(memOut.ToArray(), "image/jpeg");
                }
            }
        }
    }
}

public class DocumentCreateDto
{
    public string? TitleNumber { get; set; }
    public string? TitleType { get; set; }
    public string? Description { get; set; }
    public string? CreatedBy { get; set; }
}
