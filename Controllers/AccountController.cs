using Microsoft.AspNetCore.Mvc;

namespace FileIntake.Controllers;

public class AccountController : Controller
{
    public IActionResult Login()
    {
        return View();
    }
}
