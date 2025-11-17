using System;
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
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _context;
    const int DefaultRecentFileCount = 5;

    public FileIntakeController(IFileIntakeService fileIntakeService, 
                                UserManager<IdentityUser> userManager,
                                ApplicationDbContext context)
    {
        _fileIntakeService = fileIntakeService;
        _userManager = userManager;
        _context = context;
    }

    /// <summary>
    /// Basic UI to view the recent file intakes.
    /// Displays a list of recent file intakes with server-side sorting.
    /// </summary>
    /// <param name="sortOrder">How the files in the table will be sorted</param>
    /// <returns>View with the files</returns>
    public async Task<IActionResult> Index(string sortOrder)
    {
        ViewData["FileNameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["DateSortParam"] = sortOrder == "Date" ? "date_desc" : "Date";
        ViewData["UploaderSortParam"] = string.IsNullOrEmpty(sortOrder)? "uploader_desc" : "";

        FileRecord? uploaded = null;

        if (TempData["UploadedFileId"] is int id)
        {
            uploaded = await _fileIntakeService.GetFileByIdAsync(id);
        }

        var model = new FileUploadViewModel
        {
            FileRecords = await _fileIntakeService.GetRecentFilesAsync(DefaultRecentFileCount, sortOrder),
            UploadedFileRecord = uploaded ?? null
        };

        return View(model);
    }

    /// <summary>
    /// This method handles file uploads from the user.
    /// </summary>
    /// <param name="file">File uploaded from the user</param>
    /// <returns>Returns the index view</returns>
    [HttpPost]
    public async Task<IActionResult> Upload(IFormFile file)
    {
        if(file == null || file.Length == 0)
        {
            TempData["Error"] = "No file selected for upload.";
            return View();
        }

        var identityUser = _userManager.GetUserAsync(User).Result;

        if (identityUser == null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction("Login", "Account");
        }

        var userProfile = await _context.UserProfiles.FirstOrDefaultAsync(u => u.IdentityUserId == identityUser.Id);

        if (userProfile == null)
        {
            TempData["Error"] = "User profile not found.";
            return RedirectToAction("Index", "Home");
        }

        var fileRecord = new FileRecord{
            Id = 0,
            FileName = file.FileName,
            ContentType = file.ContentType,
            FileSize = file.Length,
            UploadedAt = DateTime.UtcNow,
            UserProfileId = userProfile.Id,
        };

        try
        {
            Console.WriteLine($"Starting file upload: {file.FileName}, Size: {file.Length} bytes");
            await _fileIntakeService.AddFileAsync(fileRecord);
            TempData["Success"] = "File uploaded successfully.";
        } catch (Exception ex)
        {
            Console.WriteLine($"Error uploading file: {ex.Message}");
            TempData["Error"] = "Error uploading file.";
        }

        var recentFiles = await _fileIntakeService.GetRecentFilesAsync(DefaultRecentFileCount, null);

        var model = new FileUploadViewModel
        {
            FileRecords = recentFiles,
            UploadedFileRecord = fileRecord
        };

        TempData["UploadedFileId"] = fileRecord.Id;

        return RedirectToAction(nameof(Index));
    }
}