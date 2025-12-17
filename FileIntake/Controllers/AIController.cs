using System;
using System.Linq;
using System.Threading.Tasks;
using FileIntake.Interfaces;
using FileIntake.Models.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FileIntake.Controllers;

[Authorize]
public class AIController : Controller
{
    private readonly IFileIntakeService _fileIntakeService;
    private readonly IAiProcessingService _aiProcessingService;

    public AIController(IFileIntakeService fileIntakeService, IAiProcessingService aiProcessingService)
    {
        _fileIntakeService = fileIntakeService;
        _aiProcessingService = aiProcessingService;
    }

    public async Task<IActionResult> Index(int? id, AiPageViewModel vmodel)
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

        var query = vmodel.SelectedQueryType.ToString();

        var model = new AiPageViewModel
        {
            UploadedFileRecord = file,
            QueryTypes = Enum.GetValues<AiQueryType>()
                            .Select(q => new SelectListItem
                            {
                                Value = q.ToString(),
                                Text = q.ToString()
                            })
                            .ToList()
        };

        var aiPromptResponse = await _aiProcessingService.AiProcessAsync(file.FileText, query);

        return View(model);
    }
}