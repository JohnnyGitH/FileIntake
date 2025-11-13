using System.Threading.Tasks;
using FileIntake.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FileIntake.Controllers;

public class FileIntakeController : Controller
{
    private readonly IFileIntakeService _fileIntakeService;
    const int DefaultRecentFileCount = 5;

    public FileIntakeController(IFileIntakeService fileIntakeService)
    {
        _fileIntakeService = fileIntakeService;
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

        var files = await _fileIntakeService.GetRecentFilesAsync(DefaultRecentFileCount, sortOrder);

        return View(files);
    }
}