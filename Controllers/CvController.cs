using KormoKhoj.Data;
using KormoKhoj.Models;
using KormoKhoj.Services;
using KormoKhoj.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KormoKhoj.Controllers;

public class CvController : Controller
{
    private readonly AppDbContext _db;
    public CvController(AppDbContext db) => _db = db;

    // Open to everyone. Visitors who are not logged-in job seekers work in "guest mode"
    // (the draft lives in the browser's localStorage).
    [HttpGet]
    public async Task<IActionResult> Builder()
    {
        var vm = new CvBuilderVm { Guest = true };

        if (User.IsInRole("JobSeeker"))
        {
            vm.Guest = false;
            var uid = User.GetUserId();
            var cv = await _db.Cvs
                .Include(c => c.Educations).Include(c => c.Experiences).Include(c => c.Projects)
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == uid);

            if (cv != null)
            {
                vm.Initial = CvMapper.ToData(cv);
            }
            else
            {
                var u = await _db.Users.AsNoTracking().FirstAsync(x => x.Id == uid);
                vm.Initial = new CvData { Name = u.Name, Email = u.Email, Phone = u.Phone, Address = u.Location };
            }
        }
        return View(vm);
    }

    [HttpPost, Authorize(Roles = "JobSeeker")]
    public async Task<IActionResult> Save([FromBody] CvData data)
    {
        if (data == null) return BadRequest();

        var uid = User.GetUserId();
        var cv = await _db.Cvs
            .Include(c => c.Educations).Include(c => c.Experiences).Include(c => c.Projects)
            .FirstOrDefaultAsync(c => c.UserId == uid);

        if (cv == null)
        {
            cv = new Cv { UserId = uid };
            _db.Cvs.Add(cv);
        }

        CvMapper.Apply(cv, data);
        await _db.SaveChangesAsync();
        return Json(new { ok = true });
    }
}
