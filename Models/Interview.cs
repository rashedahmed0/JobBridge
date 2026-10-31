using System.ComponentModel.DataAnnotations;

namespace KormoKhoj.Models;

public enum InterviewResult { Pending = 0, Selected = 1, Rejected = 2, OnHold = 3 }

public class Interview
{
    public int Id { get; set; }

    public int ApplicationId { get; set; }
    public JobApplication Application { get; set; } = null!;

    public DateTime ScheduledAt { get; set; }          // Bangladesh local time
    [MaxLength(500)] public string MeetLink { get; set; } = "";
    [MaxLength(1000)] public string Note { get; set; } = "";

    public InterviewResult Result { get; set; } = InterviewResult.Pending;
    [MaxLength(2000)] public string Feedback { get; set; } = "";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}