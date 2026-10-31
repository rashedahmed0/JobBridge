using System.ComponentModel.DataAnnotations;

namespace KormoKhoj.Models;

public class Job
{
    public int Id { get; set; }

    public int RecruiterId { get; set; }
    public AppUser Recruiter { get; set; } = null!;

    [Required, MaxLength(200)]
    public string Title { get; set; } = "";

    [MaxLength(150)]
    public string Company { get; set; } = "";

    [MaxLength(100)]
    public string Location { get; set; } = "";

    [MaxLength(30)]
    public string Type { get; set; } = "Full-time";

    [MaxLength(80)]
    public string Category { get; set; } = "";

    [MaxLength(100)]
    public string Salary { get; set; } = "";

    [MaxLength(4000)]
    public string Description { get; set; } = "";

    [MaxLength(2000)]
    public string Requirements { get; set; } = "";

    public DateTime PostedDate { get; set; } = DateTime.UtcNow.Date;

    public bool IsOpen { get; set; } = true;

    public List<JobApplication> Applications { get; set; } = new();
}
