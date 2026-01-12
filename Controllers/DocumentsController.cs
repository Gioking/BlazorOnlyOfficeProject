using Microsoft.AspNetCore.Mvc;
using System.IO.Compression;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;

namespace OnlyOfficeBlazor.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly ILogger<DocumentsController> _logger;
    private readonly IConfiguration _configuration;
    private readonly string _documentsPath;

    public DocumentsController(ILogger<DocumentsController> logger, IConfiguration configuration, IWebHostEnvironment env)
    {
        _logger = logger;
        _configuration = configuration;
        _documentsPath = Path.Combine(env.WebRootPath, "documents");

        _logger.LogInformation("DocumentsController inizializzato. Percorso documenti: {Path}", _documentsPath);

        if (!Directory.Exists(_documentsPath))
        {
            Directory.CreateDirectory(_documentsPath);
            _logger.LogInformation("Creata cartella documenti: {Path}", _documentsPath);
        }
    }

    [HttpGet]
    public IActionResult GetDocuments()
    {
        try
        {
            _logger.LogInformation("Richiesta lista documenti dalla cartella wwwroot/documents");

            var files = Directory.GetFiles(_documentsPath, "*.docx")
                .Select(f => new
                {
                    name = Path.GetFileName(f),
                    size = new FileInfo(f).Length,
                    modified = System.IO.File.GetLastWriteTime(f)
                })
                .OrderByDescending(f => f.modified)
                .ToList();

            _logger.LogInformation("Trovati {Count} documenti nella cartella wwwroot/documents", files.Count);
            return Ok(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel recupero dei documenti da wwwroot/documents");
            return StatusCode(500, "Error retrieving documents");
        }
    }

    [HttpGet("{filename}")]
    public IActionResult GetDocument(string filename)
    {
        try
        {
            _logger.LogInformation("Richiesta download documento: {Filename}", filename);
            var filePath = Path.Combine(_documentsPath, filename);

            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning("Documento non trovato: {Filename}", filename);
                return NotFound($"Document '{filename}' not found");
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            _logger.LogInformation("Download documento completato: {Filename}, Size: {Size} bytes", filename, fileBytes.Length);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", filename);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel download del documento: {Filename}", filename);
            return StatusCode(500, "Error retrieving document");
        }
    }

    [HttpPost("create")]
    public IActionResult CreateDocument([FromBody] CreateDocumentRequest request)
    {
        try
        {
            var filename = string.IsNullOrWhiteSpace(request.Filename)
                ? $"Document_{DateTime.Now:yyyyMMdd_HHmmss}.docx"
                : request.Filename;

            if (!filename.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            {
                filename += ".docx";
            }

            var filePath = Path.Combine(_documentsPath, filename);

            _logger.LogInformation("Creazione nuovo documento in wwwroot/documents: {Filename}", filename);

            // Create a minimal valid DOCX file (empty document)
            CreateEmptyDocx(filePath);

            _logger.LogInformation("Documento creato con successo in wwwroot/documents: {Filename}", filename);
            return Ok(new { filename, message = "Document created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante la creazione del documento");
            return StatusCode(500, "Error creating document");
        }
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                _logger.LogWarning("Tentativo di upload senza file");
                return BadRequest("No file uploaded");
            }

            var filename = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(_documentsPath, filename);

            _logger.LogInformation("Upload documento in wwwroot/documents: {Filename}, Dimensione: {Size} bytes", filename, file.Length);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            _logger.LogInformation("Documento caricato con successo in wwwroot/documents: {Filename}", filename);
            return Ok(new { filename, message = "Document uploaded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante l'upload del documento");
            return StatusCode(500, "Error uploading document");
        }
    }

    [HttpDelete("{filename}")]
    public IActionResult DeleteDocument(string filename)
    {
        try
        {
            var filePath = Path.Combine(_documentsPath, filename);

            _logger.LogInformation("Richiesta eliminazione documento da wwwroot/documents: {Filename}", filename);

            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning("Documento non trovato: {Filename}", filename);
                return NotFound($"Document '{filename}' not found");
            }

            System.IO.File.Delete(filePath);
            _logger.LogInformation("Documento eliminato con successo da wwwroot/documents: {Filename}", filename);
            return Ok(new { message = "Document deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante l'eliminazione del documento: {Filename}", filename);
            return StatusCode(500, "Error deleting document");
        }
    }

    [HttpGet("config/{filename}")]
    public IActionResult GetEditorConfig(string filename)
    {
        try
        {
            _logger.LogInformation("Richiesta configurazione editor per documento: {Filename}", filename);

            var filePath = Path.Combine(_documentsPath, filename);

            if (!System.IO.File.Exists(filePath))
            {
                _logger.LogWarning("Documento non trovato per configurazione editor: {Filename}", filename);
                return NotFound($"Document '{filename}' not found");
            }

            var documentServerUrl = _configuration["OnlyOffice:DocumentServerUrl"] ?? "http://localhost:8080";

            // CRITICAL: OnlyOffice container deve raggiungere il tuo PC host
            // host.docker.internal risolve all'IP del PC host da dentro il container
            var documentUrl = $"http://192.168.2.10:5000/api/documents/{filename}";
            var callbackUrl = $"http://192.168.2.10:5000/api/documents/save";

            var key = $"{filename}_{DateTime.Now.Ticks}";

            // Struttura config OnlyOffice
            var configPayload = new
            {
                documentType = "word",
                document = new
                {
                    fileType = "docx",
                    key = key,
                    title = filename,
                    url = documentUrl
                },
                editorConfig = new
                {
                    mode = "edit",
                    callbackUrl = callbackUrl,
                    user = new
                    {
                        id = "user1",
                        name = "User"
                    }
                }
            };

            // Genera JWT token
            var token = GenerateJwtToken(configPayload);

            // Config finale con token
            var config = new
            {
                documentType = configPayload.documentType,
                document = configPayload.document,
                editorConfig = configPayload.editorConfig,
                token = token,
                apiUrl = _configuration["OnlyOffice:ApiUrl"] ?? $"{documentServerUrl}/web-apps/apps/api/documents/api.js"
            };

            _logger.LogInformation("Configurazione editor con JWT generata. DocumentUrl: {Url}", documentUrl);
            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nella generazione della configurazione editor per: {Filename}", filename);
            return StatusCode(500, "Error generating editor configuration");
        }
    }

    [HttpPost("save")]
    public async Task<IActionResult> SaveDocument()
    {
        try
        {
            _logger.LogInformation("Callback save ricevuto da OnlyOffice");

            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            _logger.LogDebug("Callback OnlyOffice body: {Body}", body);

            return Ok(new { error = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore nel salvataggio del documento da callback OnlyOffice");
            return StatusCode(500, "Error saving document");
        }
    }

    private void CreateEmptyDocx(string filePath)
    {
        _logger.LogDebug("Creazione documento DOCX vuoto: {Path}", filePath);

        // Minimal DOCX structure
        using (var zip = ZipFile.Open(filePath, ZipArchiveMode.Create))
        {
            // [Content_Types].xml
            var contentTypes = zip.CreateEntry("[Content_Types].xml");
            using (var writer = new StreamWriter(contentTypes.Open()))
            {
                writer.Write("""
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                        <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                        <Default Extension="xml" ContentType="application/xml"/>
                        <Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>
                    </Types>
                    """);
            }

            // _rels/.rels
            var rels = zip.CreateEntry("_rels/.rels");
            using (var writer = new StreamWriter(rels.Open()))
            {
                writer.Write("""
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                        <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
                    </Relationships>
                    """);
            }

            // word/document.xml
            var document = zip.CreateEntry("word/document.xml");
            using (var writer = new StreamWriter(document.Open()))
            {
                writer.Write("""
                    <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                    <w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">
                        <w:body>
                            <w:p>
                                <w:r>
                                    <w:t>Nuovo documento</w:t>
                                </w:r>
                            </w:p>
                        </w:body>
                    </w:document>
                    """);
            }
        }

        _logger.LogDebug("Documento DOCX vuoto creato con successo");
    }

    private string GenerateJwtToken(object payload)
    {
        var secret = _configuration["OnlyOffice:JwtSecret"];
        if (string.IsNullOrEmpty(secret))
        {
            throw new InvalidOperationException("OnlyOffice:JwtSecret non configurato");
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var payloadJson = JsonSerializer.Serialize(payload);

        var claims = new[]
        {
        new Claim("payload", payloadJson)
    };

        var token = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddHours(5),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public class CreateDocumentRequest
    {
        public string Filename { get; set; } = "";
    }
}
