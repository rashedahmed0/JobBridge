using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KormoKhoj.Controllers;

[Authorize]
public class DashboardController : Controller
{
    public IActionResult Index()
    {
        if (User.IsInRole("Admin")) return RedirectToAction("Index", "Admin");
        if (User.IsInRole("Recruiter")) return RedirectToAction("Index", "Recruiter");
        return RedirectToAction("Index", "Seeker");
    }
}
