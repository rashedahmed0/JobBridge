using System.Text.Json;
using KormoKhoj.Models;

namespace KormoKhoj.Services;

public static class CvMapper
{
    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

    public static CvData ToData(Cv cv) => new()
    {
        Tpl = cv.Template,
        Name = cv.FullName,
        Email = cv.Email,
        Phone = cv.Phone,
        Address = cv.Address,
        Obj = cv.Objective,
        Skills = cv.Skills,
        Edu = (cv.Educations ?? new()).OrderBy(x => x.Id)
            .Select(x => new EduItem
            {
                Deg = x.Degree,
                Dept = x.Department,
                Inst = x.Institute,
                Cgpa = x.Cgpa,
                Year = x.Year
            }).ToList(),
        Exp = (cv.Experiences ?? new()).OrderBy(x => x.Id)
            .Select(x => new ExpItem
            {
                Role = x.Role,
                Org = x.Organization,
                Period = x.Period,
                Desc = x.Details
            }).ToList(),
        Proj = (cv.Projects ?? new()).OrderBy(x => x.Id)
            .Select(x => new ProjItem { Title = x.Title, Desc = x.Details }).ToList()
    };

    public static string Snapshot(Cv cv) => Serialize(ToData(cv));

    public static void Apply(Cv cv, CvData d)
    {
        cv.Template = d.Tpl is "modern" or "minimal" ? d.Tpl : "classic";
        cv.FullName = Cut(d.Name, 120);
        cv.Email = Cut(d.Email, 200);
        cv.Phone = Cut(d.Phone, 30);
        cv.Address = Cut(d.Address, 250);
        cv.Objective = Cut(d.Obj, 2000);
        cv.Skills = Cut(d.Skills, 1000);
        cv.UpdatedAt = DateTime.UtcNow;

        cv.Educations.Clear();
        foreach (var e in (d.Edu ?? new()).Where(x => x != null && Any(x.Deg, x.Dept, x.Inst, x.Cgpa, x.Year)).Take(20))
            cv.Educations.Add(new CvEducation
            {
                Degree = Cut(e.Deg, 60),
                Department = Cut(e.Dept, 200),
                Institute = Cut(e.Inst, 200),
                Cgpa = Cut(e.Cgpa, 30),
                Year = Cut(e.Year, 30)
            });

        cv.Experiences.Clear();
        foreach (var e in (d.Exp ?? new()).Where(x => x != null && Any(x.Role, x.Org, x.Period, x.Desc)).Take(20))
            cv.Experiences.Add(new CvExperience
            {
                Role = Cut(e.Role, 150),
                Organization = Cut(e.Org, 150),
                Period = Cut(e.Period, 60),
                Details = Cut(e.Desc, 2000)
            });

        cv.Projects.Clear();
        foreach (var p in (d.Proj ?? new()).Where(x => x != null && Any(x.Title, x.Desc)).Take(20))
            cv.Projects.Add(new CvProject { Title = Cut(p.Title, 150), Details = Cut(p.Desc, 2000) });
    }

    public static CvData? TryParse(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;
        try
        {
            var d = JsonSerializer.Deserialize<CvData>(json, Json);
            if (d == null) return null;
            d.Edu ??= new();
            d.Exp ??= new();
            d.Proj ??= new();
            return d;
        }
        catch { return null; }
    }

    public static string Serialize(CvData d) => JsonSerializer.Serialize(d, Json);

    private static bool Any(params string?[] v) => v.Any(s => !string.IsNullOrWhiteSpace(s));

    private static string Cut(string? s, int max)
    {
        s = (s ?? "").Trim();
        return s.Length > max ? s[..max] : s;
    }
}


//using System.Text.Json;
//using KormoKhoj.Models;

//namespace KormoKhoj.Services;

//public static class CvMapper
//{
//    public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

//    public static CvData ToData(Cv cv) => new()
//    {
//        Tpl = cv.Template,
//        Name = cv.FullName,
//        Email = cv.Email,
//        Phone = cv.Phone,
//        Address = cv.Address,
//        Obj = cv.Objective,
//        Skills = cv.Skills,
//        Edu = (cv.Educations ?? new()).OrderBy(x => x.Id)
//            .Select(x => new EduItem
//            {
//                Deg = x.Degree,
//                Dept = x.Department,
//                Inst = x.Institute,
//                Cgpa = x.Cgpa,
//                Year = x.Year
//            }).ToList(),
//        Exp = (cv.Experiences ?? new()).OrderBy(x => x.Id)
//            .Select(x => new ExpItem
//            {
//                Role = x.Role,
//                Org = x.Organization,
//                Period = x.Period,
//                Desc = x.Details
//            }).ToList(),
//        Proj = (cv.Projects ?? new()).OrderBy(x => x.Id)
//            .Select(x => new ProjItem { Title = x.Title, Desc = x.Details }).ToList()
//    };

//    /// <summary>One call for building an application snapshot from a loaded CV.</summary>
//    public static string Snapshot(Cv cv) => Serialize(ToData(cv));

//    /// <summary>Copies the DTO into the (tracked) entity, replacing all child rows.</summary>
//    public static void Apply(Cv cv, CvData d)
//    {
//        cv.Template = d.Tpl is "modern" or "minimal" ? d.Tpl : "classic";
//        cv.FullName = Cut(d.Name, 120);
//        cv.Email = Cut(d.Email, 200);
//        cv.Phone = Cut(d.Phone, 30);
//        cv.Address = Cut(d.Address, 250);
//        cv.Objective = Cut(d.Obj, 2000);
//        cv.Skills = Cut(d.Skills, 1000);
//        cv.UpdatedAt = DateTime.UtcNow;

//        cv.Educations.Clear();
//        foreach (var e in (d.Edu ?? new()).Where(x => x != null && Any(x.Deg, x.Dept, x.Inst, x.Cgpa, x.Year)).Take(20))
//            cv.Educations.Add(new CvEducation
//            {
//                Degree = Cut(e.Deg, 60),
//                Department = Cut(e.Dept, 200),
//                Institute = Cut(e.Inst, 200),
//                Cgpa = Cut(e.Cgpa, 30),
//                Year = Cut(e.Year, 30)
//            });

//        cv.Experiences.Clear();
//        foreach (var e in (d.Exp ?? new()).Where(x => x != null && Any(x.Role, x.Org, x.Period, x.Desc)).Take(20))
//            cv.Experiences.Add(new CvExperience
//            {
//                Role = Cut(e.Role, 150),
//                Organization = Cut(e.Org, 150),
//                Period = Cut(e.Period, 60),
//                Details = Cut(e.Desc, 2000)
//            });

//        cv.Projects.Clear();
//        foreach (var p in (d.Proj ?? new()).Where(x => x != null && Any(x.Title, x.Desc)).Take(20))
//            cv.Projects.Add(new CvProject { Title = Cut(p.Title, 150), Details = Cut(p.Desc, 2000) });
//    }

//    public static CvData? TryParse(string? json)
//    {
//        if (string.IsNullOrWhiteSpace(json)) return null;
//        try
//        {
//            var d = JsonSerializer.Deserialize<CvData>(json, Json);
//            if (d == null) return null;
//            d.Edu ??= new();
//            d.Exp ??= new();
//            d.Proj ??= new();
//            return d;
//        }
//        catch { return null; }
//    }

//    public static string Serialize(CvData d) => JsonSerializer.Serialize(d, Json);

//    private static bool Any(params string?[] v) => v.Any(s => !string.IsNullOrWhiteSpace(s));

//    private static string Cut(string? s, int max)
//    {
//        s = (s ?? "").Trim();
//        return s.Length > max ? s[..max] : s;
//    }
//}