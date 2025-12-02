using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FileIntake.Controllers;

public class AIController : Controller
{
    public AIController()
    {
        
    }

    public async Task<IActionResult> Index()
    {
        return View();
    }
}