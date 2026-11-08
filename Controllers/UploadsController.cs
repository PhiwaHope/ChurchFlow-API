using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class UploadsController : ControllerBase
{
    private readonly Cloudinary _cloudinary;

    private static readonly string[] Allowed = { ".jpg", ".jpeg", ".png", ".webp", ".pdf" };
    private const long MaxBytes = 10 * 1024 * 1024; // 10 MB

    public UploadsController(Cloudinary cloudinary)
    {
        _cloudinary = cloudinary;
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

        using var stream = file.OpenReadStream();

        // AutoUploadParams lets Cloudinary detect images vs PDFs.
        var uploadParams = new AutoUploadParams
        {
            File = new FileDescription($"{Guid.NewGuid()}{extension}", stream),
            Folder = "churchflow"
        };

        var result = await _cloudinary.UploadAsync(uploadParams);
        if (result.Error != null)
            return BadRequest($"Upload failed: {result.Error.Message}");

        // Full public URL: this is what gets stored in the database.
        return Ok(new { path = result.SecureUrl.ToString() });
    }
}