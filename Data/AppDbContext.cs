using KormoKhoj.Models;
using Microsoft.EntityFrameworkCore;

namespace KormoKhoj.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<JobApplication> Applications => Set<JobApplication>();
    public DbSet<Cv> Cvs => Set<Cv>();
    public DbSet<CvEducation> CvEducations => Set<CvEducation>();
    public DbSet<CvExperience> CvExperiences => Set<CvExperience>();
    public DbSet<CvProject> CvProjects => Set<CvProject>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<AppUser>(e =>
        {
            e.ToTable("Users");
            e.HasIndex(x => x.Email).IsUnique();
            e.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
        });

        b.Entity<Job>(e =>
        {
            e.HasOne(j => j.Recruiter)
                .WithMany(u => u.Jobs)
                .HasForeignKey(j => j.RecruiterId)
                .OnDelete(DeleteBehavior.Cascade);
            e.Property(j => j.PostedDate).HasColumnType("date");
            e.HasIndex(j => new { j.IsOpen, j.PostedDate });
        });

        b.Entity<JobApplication>(e =>
        {
            e.ToTable("Applications");
            e.HasOne(a => a.Job)
                .WithMany(j => j.Applications)
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Cascade);
            // NoAction avoids SQL Server "multiple cascade paths" (User -> Job -> Application).
            // Applications of a deleted user are removed explicitly in code.
            e.HasOne(a => a.Applicant)
                .WithMany(u => u.Applications)
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.NoAction);
            e.Property(a => a.Status).HasConversion<string>().HasMaxLength(20);
            e.HasIndex(a => new { a.JobId, a.UserId }).IsUnique();
        });

        b.Entity<Cv>(e =>
        {
            e.HasOne(c => c.User)
                .WithOne(u => u.Cv)
                .HasForeignKey<Cv>(c => c.UserId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(c => c.UserId).IsUnique();
        });
    }
}
