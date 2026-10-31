using KormoKhoj.Data;
using KormoKhoj.Models;
using KormoKhoj.Services;
using KormoKhoj.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KormoKhoj.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly AppDbContext _db;
    public AdminController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        return View(new AdminOverviewVm
        {
            Seekers = await _db.Users.CountAsync(u => u.Role == UserRole.JobSeeker),
            Recruiters = await _db.Users.CountAsync(u => u.Role == UserRole.Recruiter),
            Jobs = await _db.Jobs.CountAsync(),
            Applications = await _db.Applications.CountAsync()
        });
    }

    [HttpGet]
    public async Task<IActionResult> Users()
        => View(await _db.Users.AsNoTracking().OrderBy(u => u.Id).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        if (user == null || user.Role == UserRole.Admin || id == User.GetUserId())
            return RedirectToAction(nameof(Users));

        // Applications made BY this user (FK is NoAction), then the user.
        // Their jobs, those jobs' applications and their CV are removed by cascade.
        await _db.Applications.Where(a => a.UserId == id).ExecuteDeleteAsync();
        await _db.Users.Where(u => u.Id == id).ExecuteDeleteAsync();

        TempData["Msg"] = "User deleted.";
        return RedirectToAction(nameof(Users));
    }

    [HttpGet]
    public async Task<IActionResult> Jobs()
        => View(await _db.Jobs.AsNoTracking().OrderByDescending(j => j.Id).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> ToggleJob(int id)
    {
        var j = await _db.Jobs.FirstOrDefaultAsync(x => x.Id == id);
        if (j != null)
        {
            j.IsOpen = !j.IsOpen;
            await _db.SaveChangesAsync();
        }
        return RedirectToAction(nameof(Jobs));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteJob(int id)
    {
        await _db.Jobs.Where(j => j.Id == id).ExecuteDeleteAsync();
        TempData["Msg"] = "Job deleted.";
        return RedirectToAction(nameof(Jobs));
    }
}
