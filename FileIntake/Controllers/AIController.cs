using FileIntake.Services;
using Microsoft.AspNetCore.Mvc;

namespace FileIntake.Controllers;

public class AIController : Controller
{
    private readonly FileIntakeService _fileIntakeService;

    public AIController(FileIntakeService fileIntakeService)
    {
        _fileIntakeService = fileIntakeService;
        
    }

    public IActionResult Index(int? id)
    {
        if(id == null)
        {
            TempData["Error"] = "No file selected for upload.";
            return RedirectToAction(nameof(Index));
        }

        // Get the record to display
        var file = _fileIntakeService.GetFileByIdAsync(id.Value);

        return View();
    }
}