using System.Threading.Tasks;
using FileIntake.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FileIntake.Controllers;

public class AIController : Controller
{
    private readonly IFileIntakeService _fileIntakeService;

    public AIController(IFileIntakeService fileIntakeService)
    {
        _fileIntakeService = fileIntakeService;
        }

    public async Task<IActionResult> Index(int? id)
    {
        if(id == null)
        {
            TempData["Error"] = "No file selected for upload.";
            return RedirectToAction("Index","FileIntake");
        }

        // Get the record to display
        var file = await _fileIntakeService.GetFileByIdAsync(id.Value);

        if(file == null || file.FileText?.Length == 0)
        {
            TempData["Error"] = "Request file does not exist.";
            return RedirectToAction("Index","FileIntake");
        }

        var model = new AiPageViewModel
        {
            UploadedFileRecord = file,
        };

        return View(model);
    }
}