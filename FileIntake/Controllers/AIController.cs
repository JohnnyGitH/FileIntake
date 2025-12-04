using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace FileIntake.Controllers;

public class AIController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}