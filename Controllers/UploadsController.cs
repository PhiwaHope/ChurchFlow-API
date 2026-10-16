using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UploadsController : ControllerBase
{
    private readonly IWebHostEnvironment _env;

    // Whitelist: only these file types are accepted.
    private static readonly string[] Allowed = { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
    private const long MaxBytes = 10 * 1024 * 1024; // 10 MB

    public UploadsController(IWebHostEnvironment env)
    {
        _env = env;
    }

    [Authorize(Roles = "Chairperson")]
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("No file uploaded");

        if (file.Length > MaxBytes)
            return BadRequest("File is too large (max 10 MB)");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!Allowed.Contains(extension))
            return BadRequest("Only JPG, PNG, WEBP and PDF files are allowed");

        // Never use the client's file name: it could contain a path
        // or overwrite an existing file. A GUID is unique and safe.
        var fileName = $"{Guid.NewGuid()}{extension}";

        var folder = Path.Combine(_env.WebRootPath, "uploads");
        Directory.CreateDirectory(folder);

        using (var stream = new FileStream(Path.Combine(folder, fileName), FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        // Relative path: this is what gets stored in the database.
        return Ok(new { path = $"/uploads/{fileName}" });
    }
}