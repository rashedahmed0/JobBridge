using System.ComponentModel.DataAnnotations;
using KormoKhoj.Models;

namespace KormoKhoj.ViewModels;

public class InterviewRow
{
    public int AppId { get; set; }
    public string ApplicantName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string JobTitle { get; set; } = "";
    public Interview? Interview { get; set; }
}

public class InterviewFormVm
{
    public int ApplicationId { get; set; }
    public string ApplicantName { get; set; } = "";
    public string JobTitle { get; set; } = "";

    [Required(ErrorMessage = "Date o time dien")]
    public DateTime? ScheduledAt { get; set; }

    [Required(ErrorMessage = "Meet link dien"), MaxLength(500)]
    public string MeetLink { get; set; } = "";

    [MaxLength(1000)]
    public string? Note { get; set; }
}

public class InterviewResultVm
{
    public int InterviewId { get; set; }
    public InterviewResult Result { get; set; }

    [Required(ErrorMessage = "Feedback likhun"), MaxLength(2000)]
    public string Feedback { get; set; } = "";
}

public class MyInterviewRow
{
    public string JobTitle { get; set; } = "";
    public string Company { get; set; } = "";
    public Interview Interview { get; set; } = null!;
}