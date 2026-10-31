using System.Security.Claims;
using KormoKhoj.Data;
using KormoKhoj.Models;
using KormoKhoj.Services;
using KormoKhoj.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KormoKhoj.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _db;
    private readonly IPasswordHasher<AppUser> _hasher;

    public AccountController(AppDbContext db, IPasswordHasher<AppUser> hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    private static bool TryRole(string? s, bool allowAdmin, out UserRole role)
    {
        role = UserRole.JobSeeker;
        if (!Enum.TryParse(s, true, out UserRole r) || !Enum.IsDefined(r)) return false;
        if (r == UserRole.Admin && !allowAdmin) return false;
        role = r;
        return true;
    }

    [HttpGet]
    public IActionResult Login(string? role, string? returnUrl)
    {
        TryRole(role, true, out var r);
        return View(new LoginVm { Role = r.ToString(), ReturnUrl = returnUrl });
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginVm vm)
    {
        if (!TryRole(vm.Role, true, out var role)) role = UserRole.JobSeeker;
        vm.Role = role.ToString();
        if (!ModelState.IsValid) return View(vm);

        var email = vm.Email.Trim().ToLowerInvariant();
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email && u.Role == role);

        var ok = user != null &&
                 _hasher.VerifyHashedPassword(user, user.PasswordHash, vm.Password) != PasswordVerificationResult.Failed;
        if (!ok)
        {
            ModelState.AddModelError("", "Wrong email, password or role tab.");
            return View(vm);
        }

        await SignInAsync(user!);
        TempData["Msg"] = "Welcome, " + user!.Name;

        if (!string.IsNullOrEmpty(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
            return Redirect(vm.ReturnUrl);
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpGet]
    public IActionResult Register(string? role)
    {
        TryRole(role, false, out var r);
        return View(new RegisterVm { Role = r.ToString() });
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!TryRole(vm.Role, false, out var role)) role = UserRole.JobSeeker;
        vm.Role = role.ToString();

        if (role == UserRole.Recruiter && string.IsNullOrWhiteSpace(vm.Company))
            ModelState.AddModelError(nameof(vm.Company), "Company name is required.");

        var email = (vm.Email ?? "").Trim().ToLowerInvariant();
        if (ModelState.IsValid && await _db.Users.AnyAsync(u => u.Email == email))
            ModelState.AddModelError(nameof(vm.Email), "Email already registered.");

        if (!ModelState.IsValid) return View(vm);

        var user = new AppUser
        {
            Role = role,
            Name = vm.Name.Trim(),
            Email = email,
            Company = role == UserRole.Recruiter ? (vm.Company ?? "").Trim() : ""
        };
        user.PasswordHash = _hasher.HashPassword(user, vm.Password);
        _db.Users.Add(user);

        // Attach the CV the visitor built in guest mode (kept in the browser's localStorage)
        if (role == UserRole.JobSeeker && vm.GuestCv is { Length: > 0 and < 30000 })
        {
            var data = CvMapper.TryParse(vm.GuestCv);
            if (data != null)
            {
                var cv = new Cv { User = user };
                CvMapper.Apply(cv, data);
                _db.Cvs.Add(cv);
            }
        }

        await _db.SaveChangesAsync();

        await SignInAsync(user);
        TempData["Msg"] = "Welcome, " + user.Name;
        TempData["ClearGuest"] = true;
        return RedirectToAction("Index", "Dashboard");
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Msg"] = "Logged out";
        return RedirectToAction("Index", "Jobs");
    }

    [HttpGet]
    public IActionResult Denied() => View();

    private Task SignInAsync(AppUser user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        return HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = true });
    }
}
