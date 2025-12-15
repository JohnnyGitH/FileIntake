using System.Threading.Tasks;
using FileIntake.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FileIntake.Controllers;

public class AIController : Controller
{
    private readonly IFileIntakeService _fileIntakeService;
    private readonly IAiProcessingService _aiProcessingService;

    public AIController(IFileIntakeService fileIntakeService, IAiProcessingService aiProcessingService)
    {
        _fileIntakeService = fileIntakeService;
        _aiProcessingService = aiProcessingService;
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

        // UI Portion, need to get the enum value to send it as the prompt parameter

        //var aiPromptResponse = await _aiProcessingService.AiProcessAsync(file.FileText,);

        var model = new AiPageViewModel
        {
            UploadedFileRecord = file,
        };

        return View(model);
    }
}