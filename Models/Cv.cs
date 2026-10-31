using System.ComponentModel.DataAnnotations;

namespace KormoKhoj.Models;

public class Cv
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;

    [MaxLength(20)]
    public string Template { get; set; } = "classic";

    [MaxLength(120)] public string FullName { get; set; } = "";
    [MaxLength(200)] public string Email { get; set; } = "";
    [MaxLength(30)] public string Phone { get; set; } = "";
    [MaxLength(250)] public string Address { get; set; } = "";
    [MaxLength(2000)] public string Objective { get; set; } = "";
    [MaxLength(1000)] public string Skills { get; set; } = "";

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<CvEducation> Educations { get; set; } = new();
    public List<CvExperience> Experiences { get; set; } = new();
    public List<CvProject> Projects { get; set; } = new();
}

public class CvEducation
{
    public int Id { get; set; }
    public int CvId { get; set; }
    public Cv Cv { get; set; } = null!;
    [MaxLength(60)] public string Degree { get; set; } = "";
    [MaxLength(200)] public string Department { get; set; } = "";
    [MaxLength(200)] public string Institute { get; set; } = "";
    [MaxLength(30)] public string Cgpa { get; set; } = "";
    [MaxLength(30)] public string Year { get; set; } = "";
}

public class CvExperience
{
    public int Id { get; set; }
    public int CvId { get; set; }
    public Cv Cv { get; set; } = null!;
    [MaxLength(150)] public string Role { get; set; } = "";
    [MaxLength(150)] public string Organization { get; set; } = "";
    [MaxLength(60)] public string Period { get; set; } = "";
    [MaxLength(2000)] public string Details { get; set; } = "";
}

public class CvProject
{
    public int Id { get; set; }
    public int CvId { get; set; }
    public Cv Cv { get; set; } = null!;
    [MaxLength(150)] public string Title { get; set; } = "";
    [MaxLength(2000)] public string Details { get; set; } = "";
}
