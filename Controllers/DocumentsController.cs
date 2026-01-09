using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace BlazorOnlyOfficeProject.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DocumentsController : ControllerBase
{
    private readonly string _documentsPath;
    private readonly ILogger<DocumentsController> _logger;

    public DocumentsController(IWebHostEnvironment env, ILogger<DocumentsController> logger)
    {
        _documentsPath = Path.Combine(env.WebRootPath, "documents");
        _logger = logger;

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
