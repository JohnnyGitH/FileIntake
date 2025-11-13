using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FileIntake.Interfaces;
using FileIntake.Models;
using Microsoft.AspNetCore.Mvc;

namespace FileIntake.Controllers;

public class FileIntakeController : Controller
{
    private readonly IFileIntakeService _fileIntakeService;

    public FileIntakeController(IFileIntakeService fileIntakeService)
    {
        _fileIntakeService = fileIntakeService;
    }

    /// <summary>
    /// Displays a list of recent file intakes with server-side sorting.
    /// </summary>
    /// <param name="sortOrder">How the files in the table will be sorted</param>
    /// <returns>View with the files</returns>
    public async Task<IActionResult> Index(string sortOrder)
    {
        ViewData["FileNameSortParam"] = string.IsNullOrEmpty(sortOrder) ? "name_desc" : "";
        ViewData["DateSortParam"] = sortOrder == "Date" ? "date_desc" : "Date";
        ViewData["UploaderSortParam"] = string.IsNullOrEmpty(sortOrder)? "uploader_desc" : "";

        var recentFiles = await _fileIntakeService.GetRecentFilesAsync(5);

        IEnumerable<FileRecord> files = recentFiles;

        IEnumerable<FileRecord> sortedFiles;
        
        switch(sortOrder)
        {
            case "name_desc":
                sortedFiles = files.OrderByDescending(f => f.FileName).ToList();
                break;
           case "Date":
                sortedFiles = files.OrderBy(f => f.UploadedAt);
                break;
            case "date_desc":
                sortedFiles = files.OrderByDescending(f => f.UploadedAt);
                break;
            case "Uploader":
                sortedFiles = files.OrderBy(f => f.UserProfile.FirstName);
                break;
            case "uploader_desc":
                sortedFiles = files.OrderByDescending(f => f.UserProfile.FirstName);
                break;
            default:
                sortedFiles = files.OrderBy(f => f.FileName);
                break;
        }

        return View(sortedFiles);
    }
}