using System;
using System.IO;
using System.Threading.Tasks;
using FileIntake.Data;
using FileIntake.Interfaces;
using FileIntake.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UglyToad.PdfPig;
using UglyToad.PdfPig.Content;


namespace FileIntake.Controllers;

public class FileIntakeController : Controller
{
    private readonly IFileIntakeService _fileIntakeService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _context;
    private const int DefaultRecentFileCount = 5;

    public FileIntakeController(IFileIntakeService fileIntakeService, 
                                UserManager<IdentityUser> userManager,
                                ApplicationDbContext context)
    {
        _fileIntakeService = fileIntakeService;
        _userManager = userManager;
        _context = context;
    }

    /// <summary>
    /// Displays the main upload page along with a sortable list of recent uploads.
    /// </summary>
    /// <param name="sortOrder">The requested sort order for the file list.</param>
    /// <returns>The File Upload view and recent file data.</returns>
    public async Task<IActionResult> Index(string? sortOrder)
    {
        ViewData["FileNameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["DateSortParam"] = sortOrder == "Date" ? "date_desc" : "Date";
        ViewData["UploaderSortParam"] = string.IsNullOrEmpty(sortOrder)? "uploader_desc" : "Uploader";

        FileRecord? uploaded = null;
        string checkSort = sortOrder ?? "";

        if (TempData["UploadedFileId"] is string raw && int.TryParse(raw, out int id))
        {
            uploaded = await _fileIntakeService.GetFileByIdAsync(id);
        }

        var model = new FileUploadViewModel
        {
            FileRecords = await _fileIntakeService.GetRecentFilesAsync(DefaultRecentFileCount, checkSort),
            UploadedFileRecord = uploaded
        };

        return View(model);
    }

    /// <summary>
    /// Handles a file upload request, validates the user, and persists file metadata.
    /// </summary>
    /// <param name="file">The uploaded file from the client.</param>
    /// <returns>A redirect back to the Index page.</returns>
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if(file == null || file.Length == 0)
        {
            TempData["Error"] = "No file selected for upload.";
            return RedirectToAction(nameof(Index));
        }

        var identityUser = await _userManager.GetUserAsync(User);

        if (identityUser == null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction("Login", "Account");
        }

        var userProfile = await _context.UserProfiles
            .FirstOrDefaultAsync(u => u.IdentityUserId == identityUser.Id);

        if (userProfile == null)
        {
            TempData["Error"] = "User profile not found.";
            return RedirectToAction("Index", "Home");
        }

        var fileBytes = await GetByteArrayFromIFormFile(file);
        string extractedText = "";

        using (PdfDocument document = PdfDocument.Open(fileBytes))
        {
            foreach (Page page in document.GetPages())
            {
                extractedText = page.Text + "\n\n";
                Console.WriteLine($"Document info: {extractedText}");
            }
        }

        Console.WriteLine("Final: "+ extractedText);

        var fileRecord = new FileRecord{
            Id = 0,
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            UploadedAt = DateTime.UtcNow,
            UserProfileId = userProfile.Id,
            FileText = extractedText,
        };

        try
        {
            Console.WriteLine($"Starting file upload: {file.FileName}, Size: {file.Length} bytes");
            await _fileIntakeService.AddFileAsync(fileRecord);
            TempData["Success"] = "File uploaded successfully.";
            TempData["UploadedFileId"] = fileRecord.Id.ToString();
        } 
        catch (Exception ex)
        {
            Console.WriteLine($"Error uploading file: {ex.Message}");
            TempData["Error"] = "Error uploading file.";
        }

        return RedirectToAction(nameof(Index));
    }

    // Convert IFormFile to byte array for UglyToad PDFPig library
    private async Task<byte[]> GetByteArrayFromIFormFile(IFormFile file)
    {
        if(file == null || file.Length == 0)
        {
            return Array.Empty<byte>();
        }

        using (var memoryStream = new MemoryStream())
        {
            await file.CopyToAsync(memoryStream);

            return memoryStream.ToArray();
        }
    }
}