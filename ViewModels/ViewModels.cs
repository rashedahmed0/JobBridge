using System.ComponentModel.DataAnnotations;
using KormoKhoj.Models;

namespace KormoKhoj.ViewModels;

public record JobCardVm(Job Job, bool Applied);

public class JobsIndexVm
{
    public List<Job> Jobs { get; set; } = new();
    public List<string> Locations { get; set; } = new();
    public HashSet<int> AppliedJobIds { get; set; } = new();
    public string? Q { get; set; }
    public string? Location { get; set; }
    public string? Type { get; set; }
    public string Sort { get; set; } = "new";
}

public class JobDetailsVm
{
    public Job Job { get; set; } = null!;
    public bool Applied { get; set; }
}

public class LoginVm
{
    public string Role { get; set; } = "JobSeeker";

    [Required, EmailAddress]
    public string Email { get; set; } = "";

    [Required, DataType(DataType.Password)]
    public string Password { get; set; } = "";

    public string? ReturnUrl { get; set; }
}

public class RegisterVm
{
    public string Role { get; set; } = "JobSeeker";

    [Required, MaxLength(120), Display(Name = "Full name")]
    public string Name { get; set; } = "";

    [MaxLength(150), Display(Name = "Company name")]
    public string? Company { get; set; }

    [Required, EmailAddress, MaxLength(200)]
    public string Email { get; set; } = "";

    [Required, MinLength(6, ErrorMessage = "Password must be at least 6 characters."), DataType(DataType.Password)]
    public string Password { get; set; } = "";

    // Filled by JS from localStorage ("guest" CV) so it can be attached to the new account.
    public string? GuestCv { get; set; }
}

public class CvBuilderVm
{
    public bool Guest { get; set; }
    public CvData? Initial { get; set; }
}

public class ProfileVm
{
    [Required, MaxLength(120), Display(Name = "Full name")]
    public string Name { get; set; } = "";
    [MaxLength(30)] public string? Phone { get; set; }
    [MaxLength(100)] public string? Location { get; set; }
    [MaxLength(150)] public string? Headline { get; set; }
    public string? Email { get; set; }
}

public class CompanyVm
{
    [Required, MaxLength(150), Display(Name = "Company name")]
    public string Company { get; set; } = "";
    [Required, MaxLength(120), Display(Name = "Contact person")]
    public string Name { get; set; } = "";
    [MaxLength(200)] public string? Website { get; set; }
    [MaxLength(100)] public string? Location { get; set; }
    [MaxLength(2000)] public string? About { get; set; }
}

public class JobFormVm
{
    public int? Id { get; set; }

    [Required, MaxLength(200), Display(Name = "Job title")]
    public string Title { get; set; } = "";

    [Required, MaxLength(100)]
    public string Location { get; set; } = "";

    [Required, MaxLength(30)]
    public string Type { get; set; } = "Full-time";

    [Required, MaxLength(80), Display(Name = "Category")]
    public string Category { get; set; } = "";

    [Required, MaxLength(100)]
    public string Salary { get; set; } = "";

    [Required, MaxLength(4000)]
    public string Description { get; set; } = "";

    [MaxLength(2000)]
    public string? Requirements { get; set; }
}

public class RecruiterJobRow
{
    public Job Job { get; set; } = null!;
    public int ApplicantCount { get; set; }
}

public class ApplicantRow
{
    public int AppId { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Address { get; set; } = "";
    public string Education { get; set; } = "";
    public string Department { get; set; } = "";
    public string Cgpa { get; set; } = "";
    public string Year { get; set; } = "";
    public string Skills { get; set; } = "";
    public ApplicationStatus Status { get; set; }
}

public class ApplicantsVm
{
    public Job Job { get; set; } = null!;
    public List<ApplicantRow> Rows { get; set; } = new();
}

public class ApplicantCvVm
{
    public AppUser Applicant { get; set; } = null!;
    public CvData Cv { get; set; } = new();
}

public class AdminOverviewVm
{
    public int Seekers { get; set; }
    public int Recruiters { get; set; }
    public int Jobs { get; set; }
    public int Applications { get; set; }
}
