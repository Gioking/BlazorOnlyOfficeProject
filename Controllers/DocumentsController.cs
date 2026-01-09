using Microsoft.AspNetCore.Mvc;
using System.Text;
using System.Text.Json;
using System.Security.Cryptography;

namespace BlazorOnlyOfficeProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly string _documentsPath;
    private readonly ILogger<DocumentsController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _env;

    public DocumentsController(IWebHostEnvironment env, ILogger<DocumentsController> logger, IConfiguration configuration)
    {
        _documentsPath = Path.Combine(env.WebRootPath, "documents");
        _logger = logger;
        _configuration = configuration;
        _env = env;

        // Ensure documents directory exists
        if (!Directory.Exists(_documentsPath))
        {
            Directory.CreateDirectory(_documentsPath);
        }
    }

    [HttpGet]
    public IActionResult GetDocuments()
    {
        try
        {
            var files = Directory.GetFiles(_documentsPath, "*.docx")
                .Select(f => new
                {
                    name = Path.GetFileName(f),
                    size = new FileInfo(f).Length,
                    modified = System.IO.File.GetLastWriteTime(f)
                })
                .OrderByDescending(f => f.modified)
                .ToList();

            return Ok(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting documents");
            return StatusCode(500, "Error retrieving documents");
        }
    }

    [HttpGet("{filename}")]
    public IActionResult GetDocument(string filename)
    {
        try
        {
            var filePath = Path.Combine(_documentsPath, filename);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound($"Document '{filename}' not found");
            }

            var fileBytes = System.IO.File.ReadAllBytes(filePath);
            return File(fileBytes, "application/vnd.openxmlformats-officedocument.wordprocessingml.document", filename);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting document {filename}");
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

            // Create a minimal valid DOCX file (empty document)
            CreateEmptyDocx(filePath);

            return Ok(new { filename, message = "Document created successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating document");
            return StatusCode(500, "Error creating document");
        }
    }

    [HttpPost("save")]
    public async Task<IActionResult> SaveDocument()
    {
        try
        {
            var filename = Request.Form["filename"].ToString();
            var file = Request.Form.Files["file"];

            if (string.IsNullOrEmpty(filename) || file == null)
            {
                return BadRequest("Filename and file are required");
            }

            var filePath = Path.Combine(_documentsPath, filename);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { message = "Document saved successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving document");
            return StatusCode(500, "Error saving document");
        }
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadDocument(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No file uploaded");
            }

            var filename = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(_documentsPath, filename);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { filename, message = "Document uploaded successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading document");
            return StatusCode(500, "Error uploading document");
        }
    }

    [HttpDelete("{filename}")]
    public IActionResult DeleteDocument(string filename)
    {
        try
        {
            var filePath = Path.Combine(_documentsPath, filename);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound($"Document '{filename}' not found");
            }

            System.IO.File.Delete(filePath);
            return Ok(new { message = "Document deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting document {filename}");
            return StatusCode(500, "Error deleting document");
        }
    }

    [HttpGet("config/{filename}")]
    public IActionResult GetEditorConfig(string filename)
    {
        try
        {
            var filePath = Path.Combine(_documentsPath, filename);

            if (!System.IO.File.Exists(filePath))
            {
                return NotFound($"Document '{filename}' not found");
            }

            // Use host.docker.internal for OnlyOffice to reach the host machine from Docker container
            var host = Request.Host.Host;
            var port = Request.Host.Port ?? (Request.Scheme == "https" ? 443 : 80);

            // Replace localhost with host.docker.internal for Docker compatibility
            if (host == "localhost" || host == "127.0.0.1")
            {
                host = "host.docker.internal";
            }

            var baseUrl = port == 80 || port == 443
                ? $"{Request.Scheme}://{host}"
                : $"{Request.Scheme}://{host}:{port}";

            var documentUrl = $"{baseUrl}/api/documents/{filename}";
            var callbackUrl = $"{baseUrl}/api/documents/callback";

            // Generate unique key for document (based on filename and last modified time)
            var fileInfo = new FileInfo(filePath);
            var key = GenerateDocumentKey(filename, fileInfo.LastWriteTimeUtc);

            var config = new
            {
                documentType = "word",
                fileType = "docx",
                key = key,
                title = filename,
                documentUrl = documentUrl,
                callbackUrl = callbackUrl,
                mode = "edit",
                userId = "user1",
                userName = "User",
                apiUrl = _configuration["OnlyOffice:ApiUrl"] ?? "http://localhost:8080/web-apps/apps/api/documents/api.js"
            };

            _logger.LogInformation($"OnlyOffice Config - DocumentUrl: {documentUrl}, CallbackUrl: {callbackUrl}, Key: {key}");

            return Ok(config);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting editor config for {filename}");
            return StatusCode(500, "Error getting editor configuration");
        }
    }

    [HttpPost("callback")]
    public async Task<IActionResult> OnlyOfficeCallback()
    {
        try
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            _logger.LogInformation($"OnlyOffice Callback received: {body}");

            var callback = JsonSerializer.Deserialize<OnlyOfficeCallback>(body, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (callback == null)
            {
                return BadRequest("Invalid callback data");
            }

            // Status values:
            // 0 - Document not found
            // 1 - Document editing
            // 2 - Document ready for saving
            // 3 - Document saving error
            // 4 - Document closed with no changes
            // 6 - Document being edited, force save requested
            // 7 - Error force saving the document

            if (callback.Status == 2 || callback.Status == 6)
            {
                // Document is ready to be saved
                if (!string.IsNullOrEmpty(callback.Url))
                {
                    var filename = callback.Key?.Split('_')[0] + ".docx";
                    var filePath = Path.Combine(_documentsPath, filename);

                    using var httpClient = new HttpClient();
                    var documentBytes = await httpClient.GetByteArrayAsync(callback.Url);
                    await System.IO.File.WriteAllBytesAsync(filePath, documentBytes);

                    _logger.LogInformation($"Document saved: {filename}");
                }
            }

            return Ok(new { error = 0 });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing OnlyOffice callback");
            return Ok(new { error = 1 });
        }
    }

    private string GenerateDocumentKey(string filename, DateTime lastModified)
    {
        var input = $"{filename}_{lastModified:yyyyMMddHHmmss}";
        using var md5 = MD5.Create();
        var hash = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
        return BitConverter.ToString(hash).Replace("-", "").ToLower();
    }

    private void CreateEmptyDocx(string filePath)
    {
        // Create a minimal DOCX file using the Open XML format
        // This is a base64 encoded minimal DOCX file
        var minimalDocxBase64 = "UEsDBBQABgAIAAAAIQDfpNJsWgEAACAFAAATAAgCW0NvbnRlbnRfVHlwZXNdLnhtbCCiBAIooAAC" +
                               "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                               "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                               "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                               "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                               "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                               "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                               "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" +
                               "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAACsVMluwjAQ" +
                               "vVfqP0S+Vomhh6qqCBy6HFsk6AeYZAJRk1hyXEL/nglJCKWXNkKcbDPzvPfmTVznqlbqTuEXsrE+" +
                               "ZjlJQ8E+TbMt+KTfzLKGJVAoVVpMJxWFhVNY0GulqHcG5iDwWPzhPlLKW7Lkr+8jzgWr/C1XsICQ" +
                               "P8+4SaF0eo7TlMiJtmDK0XzCfOXFRNbZ9BvXHRPQJdKdR4dC9mKjBn7HJvVcLWgiFBpQlDm8l5ZM" +
                               "BXWM7K3YbD/k7eGvZSGrC/XGCGNl+i7vEYwczxqJWKcOXFb7qGG7KBQAAA==";

        var docxBytes = Convert.FromBase64String(minimalDocxBase64);
        System.IO.File.WriteAllBytes(filePath, docxBytes);
    }
}

public class CreateDocumentRequest
{
    public string Filename { get; set; } = "";
}

public class OnlyOfficeCallback
{
    public int Status { get; set; }
    public string? Url { get; set; }
    public string? Key { get; set; }
    public List<string>? Users { get; set; }
}
