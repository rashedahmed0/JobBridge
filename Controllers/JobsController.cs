using KormoKhoj.Data;
using KormoKhoj.Models;
using KormoKhoj.Services;
using KormoKhoj.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KormoKhoj.Controllers;

public class JobsController : Controller
{
    private readonly AppDbContext _db;
    public JobsController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> Index(string? q, string? location, string? type, string sort = "new")
    {
        var open = _db.Jobs.AsNoTracking().Where(j => j.IsOpen);

        var vm = new JobsIndexVm
        {
            Q = q, Location = location, Type = type, Sort = sort,
            Locations = await open.Select(j => j.Location).Distinct().OrderBy(x => x).ToListAsync()
        };

        var query = open;
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            query = query.Where(j => j.Title.Contains(term) || j.Company.Contains(term) ||
                                     j.Description.Contains(term) || j.Requirements.Contains(term) ||
                                     j.Category.Contains(term));
        }
        if (!string.IsNullOrEmpty(location)) query = query.Where(j => j.Location == location);
        if (!string.IsNullOrEmpty(type)) query = query.Where(j => j.Type == type);

        query = sort == "az"
            ? query.OrderBy(j => j.Title)
            : query.OrderByDescending(j => j.PostedDate).ThenByDescending(j => j.Id);

        vm.Jobs = await query.ToListAsync();

        if (User.IsInRole("JobSeeker"))
        {
            var uid = User.GetUserId();
            vm.AppliedJobIds = (await _db.Applications.Where(a => a.UserId == uid)
                .Select(a => a.JobId).ToListAsync()).ToHashSet();
        }

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var job = await _db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == id);
        if (job == null) return NotFound();

        var applied = false;
        if (User.IsInRole("JobSeeker"))
        {
            var uid = User.GetUserId();
            applied = await _db.Applications.AnyAsync(a => a.JobId == id && a.UserId == uid);
        }
        return View(new JobDetailsVm { Job = job, Applied = applied });
    }

    [HttpPost, Authorize(Roles = "JobSeeker")]
    public async Task<IActionResult> Apply(int id, string? returnUrl)
    {
        var uid = User.GetUserId();
        var job = await _db.Jobs.FirstOrDefaultAsync(j => j.Id == id);
        if (job == null) return NotFound();

        IActionResult Back() =>
            !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)
                ? Redirect(returnUrl)
                : RedirectToAction(nameof(Details), new { id });

        if (!job.IsOpen)
        {
            TempData["Msg"] = "This job is closed.";
            return Back();
        }

        if (await _db.Applications.AnyAsync(a => a.JobId == id && a.UserId == uid))
        {
            TempData["Msg"] = "You already applied to this job.";
            return Back();
        }

        var cv = await _db.Cvs
            .Include(c => c.Educations).Include(c => c.Experiences).Include(c => c.Projects)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == uid);

        if (cv == null)
        {
            TempData["Msg"] = "Create and save your CV first.";
            return RedirectToAction("Builder", "Cv");
        }

        _db.Applications.Add(new JobApplication
        {
            JobId = id,
            UserId = uid,
            AppliedDate = DateTime.UtcNow,
            Status = ApplicationStatus.Pending,
            CvSnapshot = CvMapper.Serialize(CvMapper.ToData(cv))
        });
        await _db.SaveChangesAsync();

        TempData["Msg"] = "Application submitted with your saved CV.";
        return Back();
    }
}
