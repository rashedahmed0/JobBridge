namespace KormoKhoj.Models;

public enum ApplicationStatus
{
    Pending = 0,
    Shortlisted = 1,
    Rejected = 2,
    Hired = 3
}

public class JobApplication
{
    public int Id { get; set; }

    public int JobId { get; set; }
    public Job Job { get; set; } = null!;

    public int UserId { get; set; }
    public AppUser Applicant { get; set; } = null!;

    public DateTime AppliedDate { get; set; } = DateTime.UtcNow;

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;

    /// <summary>JSON copy of the applicant's CV at the time of applying.</summary>
    public string CvSnapshot { get; set; } = "{}";
    public Interview? Interview { get; set; }
}
