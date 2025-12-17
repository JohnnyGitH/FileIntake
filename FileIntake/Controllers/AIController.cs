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

    [HttpGet]
    public async Task<IActionResult> Index(int? id, AiPageViewModel vmodel)
    {
        Console.WriteLine("Inside AiController GET Index Action method");
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

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Index(AiPageViewModel vmodel)
    {
        Console.WriteLine("Inside AiController POST Index Action method");
        if(vmodel.UploadedFileRecord?.FileText == null || vmodel.UploadedFileRecord.FileText?.Length == 0)
        {
            TempData["Error"] = "Request file does not exist.";
            return RedirectToAction("Index","FileIntake");
        }

        try
        {
            AiProcessingResult aiPromptResponse = new AiProcessingResult();
            aiPromptResponse = await _aiProcessingService.AiProcessAsync(vmodel.UploadedFileRecord.FileText, vmodel.SelectedQueryType.ToString());
            Console.WriteLine($"Response : {aiPromptResponse.aiResponse}");

            vmodel.AIPromptResponse = aiPromptResponse.aiResponse;
            vmodel.QueryTypes = Enum.GetValues<AiQueryType>()
                    .Select(q => new SelectListItem
                    {
                        Value = q.ToString(),
                        Text = q.ToString()
                    })
                    .ToList();
            
            ModelState.Clear();

            return View(vmodel);
        }
        catch (Exception ex)
        {
            TempData["Error"] = $"Ai Processing Failed. Exception : {ex}";
            return View();
        }
    }
}