using KormoKhoj.Data;
using KormoKhoj.Services;
using KormoKhoj.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KormoKhoj.Controllers;

[Authorize(Roles = "JobSeeker")]
public class SeekerController : Controller
{
    private readonly AppDbContext _db;
    public SeekerController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var u = await _db.Users.AsNoTracking().FirstAsync(x => x.Id == User.GetUserId());
        return View(new ProfileVm
        {
            Name = u.Name, Phone = u.Phone, Location = u.Location, Headline = u.Headline, Email = u.Email
        });
    }

    [HttpPost]
    public async Task<IActionResult> Index(ProfileVm vm)
    {
        var u = await _db.Users.FirstAsync(x => x.Id == User.GetUserId());
        vm.Email = u.Email;
        if (!ModelState.IsValid) return View(vm);

        u.Name = vm.Name.Trim();
        u.Phone = (vm.Phone ?? "").Trim();
        u.Location = (vm.Location ?? "").Trim();
        u.Headline = (vm.Headline ?? "").Trim();
        await _db.SaveChangesAsync();

        TempData["Msg"] = "Profile saved";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Cv()
    {
        var uid = User.GetUserId();
        var cv = await _db.Cvs
            .Include(c => c.Educations).Include(c => c.Experiences).Include(c => c.Projects)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == uid);
        return View(cv == null ? null : CvMapper.ToData(cv));
    }

    [HttpGet]
    public async Task<IActionResult> Applications()
    {
        var uid = User.GetUserId();
        var apps = await _db.Applications.AsNoTracking()
            .Include(a => a.Job)
            .Where(a => a.UserId == uid)
            .OrderByDescending(a => a.AppliedDate)
            .ToListAsync();
        return View(apps);
    }
}
