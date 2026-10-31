using KormoKhoj.Data;
using KormoKhoj.Models;
using KormoKhoj.Services;
using KormoKhoj.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KormoKhoj.Controllers;

[Authorize(Roles = "Recruiter")]
public class RecruiterController : Controller
{
    private readonly AppDbContext _db;
    public RecruiterController(AppDbContext db) => _db = db;

    private int Uid => User.GetUserId();

    // ---------- My jobs ----------
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var rows = await _db.Jobs.AsNoTracking()
            .Where(j => j.RecruiterId == Uid)
            .OrderByDescending(j => j.PostedDate).ThenByDescending(j => j.Id)
            .Select(j => new RecruiterJobRow { Job = j, ApplicantCount = j.Applications.Count })
            .ToListAsync();
        return View(rows);
    }

    // ---------- Post / edit a job ----------
    [HttpGet]
    public async Task<IActionResult> Post(int? id)
    {
        if (id == null) return View(new JobFormVm());

        var j = await _db.Jobs.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id && x.RecruiterId == Uid);
        if (j == null) return NotFound();

        return View(new JobFormVm
        {
            Id = j.Id, Title = j.Title, Location = j.Location, Type = j.Type, Category = j.Category,
            Salary = j.Salary, Description = j.Description, Requirements = j.Requirements
        });
    }

    [HttpPost]
    public async Task<IActionResult> Post(JobFormVm vm)
    {
        if (!Lookups.JobTypes.Contains(vm.Type))
            ModelState.AddModelError(nameof(vm.Type), "Invalid job type.");
        if (!ModelState.IsValid) return View(vm);

        Job? job;
        if (vm.Id != null)
        {
            job = await _db.Jobs.FirstOrDefaultAsync(x => x.Id == vm.Id && x.RecruiterId == Uid);
            if (job == null) return NotFound();
        }
        else
        {
            var company = await _db.Users.Where(u => u.Id == Uid).Select(u => u.Company).FirstAsync();
            job = new Job { RecruiterId = Uid, Company = company, PostedDate = DateTime.UtcNow.Date, IsOpen = true };
            _db.Jobs.Add(job);
        }

        job.Title = vm.Title.Trim();
        job.Location = vm.Location.Trim();
        job.Type = vm.Type;
        job.Category = vm.Category.Trim();
        job.Salary = vm.Salary.Trim();
        job.Description = vm.Description.Trim();
        job.Requirements = (vm.Requirements ?? "").Trim();

        await _db.SaveChangesAsync();
        TempData["Msg"] = "Job saved and live in the job feed.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Toggle(int id)
    {
        var j = await _db.Jobs.FirstOrDefaultAsync(x => x.Id == id && x.RecruiterId == Uid);
        if (j == null) return NotFound();
        j.IsOpen = !j.IsOpen;
        await _db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var j = await _db.Jobs.FirstOrDefaultAsync(x => x.Id == id && x.RecruiterId == Uid);
        if (j == null) return NotFound();
        _db.Jobs.Remove(j); // applications are removed by cascade
        await _db.SaveChangesAsync();
        TempData["Msg"] = "Job deleted.";
        return RedirectToAction(nameof(Index));
    }

    // ---------- Company profile ----------
    [HttpGet]
    public async Task<IActionResult> Company()
    {
        var u = await _db.Users.AsNoTracking().FirstAsync(x => x.Id == Uid);
        return View(new CompanyVm
        {
            Company = u.Company, Name = u.Name, Website = u.Website, Location = u.Location, About = u.About
        });
    }

    [HttpPost]
    public async Task<IActionResult> Company(CompanyVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var u = await _db.Users.FirstAsync(x => x.Id == Uid);
        u.Company = vm.Company.Trim();
        u.Name = vm.Name.Trim();
        u.Website = (vm.Website ?? "").Trim();
        u.Location = (vm.Location ?? "").Trim();
        u.About = (vm.About ?? "").Trim();
        await _db.SaveChangesAsync();

        TempData["Msg"] = "Profile saved";
        return RedirectToAction(nameof(Company));
    }

    // ---------- Applicants ----------
    //[HttpGet]
    //public async Task<IActionResult> Applicants(int id)
    //{
    //    var job = await _db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == id && j.RecruiterId == Uid);
    //    if (job == null) return NotFound();

    //    var apps = await _db.Applications.AsNoTracking()
    //        .Include(a => a.Applicant)
    //        .Where(a => a.JobId == id)
    //        .OrderByDescending(a => a.AppliedDate)
    //        .ToListAsync();

    //    var rows = apps.Select(a =>
    //    {
    //        var cv = CvMapper.TryParse(a.CvSnapshot) ?? new CvData();
    //        var edu = cv.Edu?.FirstOrDefault() ?? new EduItem();
    //        return new ApplicantRow
    //        {
    //            AppId = a.Id,
    //            Name = string.IsNullOrWhiteSpace(a.Applicant.Name) ? cv.Name : a.Applicant.Name,
    //            Phone = !string.IsNullOrWhiteSpace(cv.Phone) ? cv.Phone : a.Applicant.Phone,
    //            Email = !string.IsNullOrWhiteSpace(a.Applicant.Email) ? a.Applicant.Email : cv.Email,
    //            Address = !string.IsNullOrWhiteSpace(cv.Address) ? cv.Address : a.Applicant.Location,
    //            Education = edu.Deg,
    //            Department = edu.Dept,
    //            Cgpa = edu.Cgpa,
    //            Year = edu.Year,
    //            Skills = cv.Skills,
    //            Status = a.Status
    //        };
    //    }).ToList();

    //    return View(new ApplicantsVm { Job = job, Rows = rows });
    //}

    [HttpGet]
    public async Task<IActionResult> Applicants(int id)
    {
        var job = await _db.Jobs.AsNoTracking().FirstOrDefaultAsync(j => j.Id == id && j.RecruiterId == Uid);
        if (job == null) return NotFound();

        var apps = await _db.Applications.AsNoTracking()
            .Include(a => a.Applicant)
            .Where(a => a.JobId == id)
            .OrderByDescending(a => a.AppliedDate)
            .ToListAsync();

        // CvEducations table theke shob applicant er education ekbare load
        var applicantIds = apps.Select(a => a.Applicant.Id).Distinct().ToList();
        var cvs = await _db.Cvs.AsNoTracking()
            .Include(c => c.Educations)
            .Where(c => applicantIds.Contains(c.UserId))
            .ToListAsync();
        var eduMap = cvs
            .GroupBy(c => c.UserId)
            .ToDictionary(g => g.Key, g => g.First().Educations.ToList());

        var rows = apps.Select(a =>
        {
            var cv = CvMapper.TryParse(a.CvSnapshot) ?? new CvData();
            var edu = cv.Edu?.FirstOrDefault() ?? new EduItem();

            // CvEducations table er Department column theke value
            string? dept = null;
            if (eduMap.TryGetValue(a.Applicant.Id, out var list))
            {
                var m = list.FirstOrDefault(x =>
                            x.Degree == edu.Deg && x.Institute == edu.Inst && x.Year == edu.Year)
                        ?? list.FirstOrDefault();
                dept = m?.Department;
            }

            return new ApplicantRow
            {
                AppId = a.Id,
                Name = string.IsNullOrWhiteSpace(a.Applicant.Name) ? cv.Name : a.Applicant.Name,
                Phone = !string.IsNullOrWhiteSpace(cv.Phone) ? cv.Phone : a.Applicant.Phone,
                Email = !string.IsNullOrWhiteSpace(a.Applicant.Email) ? a.Applicant.Email : cv.Email,
                Address = !string.IsNullOrWhiteSpace(cv.Address) ? cv.Address : a.Applicant.Location,
                Education = edu.Deg,
                Department = !string.IsNullOrWhiteSpace(dept) ? dept : edu.Dept, // table value age, na thakle snapshot
                Cgpa = edu.Cgpa,
                Year = edu.Year,
                Skills = cv.Skills,
                Status = a.Status
            };
        }).ToList();

        return View(new ApplicantsVm { Job = job, Rows = rows });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStatus(int id, ApplicationStatus status)
    {
        var app = await _db.Applications
            .FirstOrDefaultAsync(a => a.Id == id && a.Job.RecruiterId == Uid);
        if (app == null) return NotFound();
        app.Status = status;
        await _db.SaveChangesAsync();
        return Ok();
    }

    // Returns an HTML fragment that is injected into a modal by site.js
    [HttpGet]
    public async Task<IActionResult> ApplicantCv(int id)
    {
        var app = await _db.Applications.AsNoTracking()
            .Include(a => a.Applicant)
            .FirstOrDefaultAsync(a => a.Id == id && a.Job.RecruiterId == Uid);
        if (app == null) return NotFound();

        return PartialView("_ApplicantCv", new ApplicantCvVm
        {
            Applicant = app.Applicant,
            Cv = CvMapper.TryParse(app.CvSnapshot) ?? new CvData()
        });
    }
}
