using System.Threading.Tasks;
using FileIntake.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FileIntake.Controllers;

public class FileIntakeController : Controller
{
    private readonly IFileIntakeService _fileIntakeService;

    public FileIntakeController(IFileIntakeService fileIntakeService)
    {
        _fileIntakeService = fileIntakeService;
    }

    public async Task<IActionResult> Index()
    {
        var recentFiles = await _fileIntakeService.GetRecentFilesAsync(5);
        return View(recentFiles);
    }
}