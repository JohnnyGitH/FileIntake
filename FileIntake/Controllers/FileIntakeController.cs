using System.Threading.Tasks;
using FileIntake.Data;
using FileIntake.Interfaces;
using FileIntake.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FileIntake.Controllers;

public class FileIntakeController : Controller
{
    private readonly IFileIntakeService _fileIntakeService;
    private readonly IFileProcessingService _fileProcessingService;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _context;
    private const int DefaultRecentFileCount = 5;

    public FileIntakeController(IFileIntakeService fileIntakeService, 
                                IFileProcessingService fileProcessingService,
                                UserManager<IdentityUser> userManager,
                                ApplicationDbContext context)
    {
        _fileIntakeService = fileIntakeService;
        _fileProcessingService = fileProcessingService;
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

        var userId = userProfile.Id;

        var record = await _fileProcessingService.ProcessFile(file, userId);

        if (record.success)
        {
            TempData["Success"] = "File uploaded successfully.";
            TempData["UploadedFileId"] = record.FileRecord.Id.ToString();
        } else
        {
            TempData["Error"] = $"{record.ErrorMessage}";
        }

        return RedirectToAction(nameof(Index));
    }
}