using System.ComponentModel.DataAnnotations;

namespace KormoKhoj.Models;

public enum UserRole
{
    JobSeeker = 0,
    Recruiter = 1,
    Admin = 2
}

public class AppUser
{
    public int Id { get; set; }

    public UserRole Role { get; set; }

    [Required, MaxLength(120)]
    public string Name { get; set; } = "";

    [Required, MaxLength(200)]
    public string Email { get; set; } = "";

    [Required]
    public string PasswordHash { get; set; } = "";

    [MaxLength(30)]
    public string Phone { get; set; } = "";

    [MaxLength(100)]
    public string Location { get; set; } = "";

    [MaxLength(150)]
    public string Headline { get; set; } = "";

    // Recruiter only
    [MaxLength(150)]
    public string Company { get; set; } = "";

    [MaxLength(200)]
    public string Website { get; set; } = "";

    [MaxLength(2000)]
    public string About { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Cv? Cv { get; set; }
    public List<Job> Jobs { get; set; } = new();
    public List<JobApplication> Applications { get; set; } = new();
}
