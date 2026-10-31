using KormoKhoj.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KormoKhoj.Data;

public static class DbSeeder
{
    /// <summary>
    /// Inserts demo data on first run (only when the Users table is empty).
    /// Run the EF migration first, otherwise this just logs a warning and the app keeps running.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services, ILogger logger)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<AppUser>>();

        try
        {
            if (await db.Users.AnyAsync()) return;

            var admin = new AppUser
            {
                Role = UserRole.Admin,
                Name = "Admin",
                Email = "admin@karmo.com"
            };
            admin.PasswordHash = hasher.HashPassword(admin, "admin123");

            var rec = new AppUser
            {
                Role = UserRole.Recruiter,
                Name = "Rahim Uddin",
                Email = "recruiter@demo.com",
                Company = "Dhaka Tech Ltd",
                Website = "dhakatech.example",
                About = "Software and product company building tools for South Asia.",
                Location = "Dhaka"
            };
            rec.PasswordHash = hasher.HashPassword(rec, "demo123");

            db.Users.AddRange(admin, rec);
            await db.SaveChangesAsync();

            var today = DateTime.UtcNow.Date;
            db.Jobs.AddRange(
                new Job
                {
                    RecruiterId = rec.Id, Title = "Frontend Developer", Company = rec.Company,
                    Location = "Dhaka", Type = "Full-time", Category = "IT",
                    Salary = "40,000 – 60,000 BDT",
                    Description = "Build responsive interfaces with HTML, CSS and JavaScript.",
                    Requirements = "2+ years experience, React or vanilla JS, Git.",
                    PostedDate = today, IsOpen = true
                },
                new Job
                {
                    RecruiterId = rec.Id, Title = "Customer Support Executive", Company = rec.Company,
                    Location = "Chattogram", Type = "Full-time", Category = "Support",
                    Salary = "22,000 – 30,000 BDT",
                    Description = "Handle customer chats and calls in Bangla and English.",
                    Requirements = "Good communication, HSC or above.",
                    PostedDate = today, IsOpen = true
                },
                new Job
                {
                    RecruiterId = rec.Id, Title = "Graphic Designer (Intern)", Company = rec.Company,
                    Location = "Remote", Type = "Internship", Category = "Design",
                    Salary = "10,000 BDT",
                    Description = "Design social media posts and banners.",
                    Requirements = "Photoshop or Figma, portfolio.",
                    PostedDate = today, IsOpen = true
                },
                new Job
                {
                    RecruiterId = rec.Id, Title = "Accounts Officer", Company = rec.Company,
                    Location = "Sylhet", Type = "Part-time", Category = "Finance",
                    Salary = "18,000 BDT",
                    Description = "Maintain ledgers and prepare monthly reports.",
                    Requirements = "B.Com, Excel skills.",
                    PostedDate = today, IsOpen = true
                });
            await db.SaveChangesAsync();

            logger.LogInformation("Seeded demo admin, recruiter and jobs.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Seeding skipped. Make sure the database exists (dotnet ef database update).");
        }
    }
}
