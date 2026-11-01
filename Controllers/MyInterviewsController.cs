using KormoKhoj.Data;
using KormoKhoj.Services;
using KormoKhoj.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KormoKhoj.Controllers;

[Authorize(Roles = "JobSeeker")]    
public class MyInterviewsController : Controller
{
    private readonly AppDbContext _db;
    public MyInterviewsController(AppDbContext db) => _db = db;

    private int Uid => User.GetUserId();

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var rows = await _db.Interviews.AsNoTracking()
            .Where(i => i.Application.UserId == Uid)
            .OrderByDescending(i => i.ScheduledAt)
            .Select(i => new MyInterviewRow
            {
                JobTitle = i.Application.Job.Title,
                Company = i.Application.Job.Company,
                Interview = i
            })
            .ToListAsync();
        return View(rows);
    }
}