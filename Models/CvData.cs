namespace KormoKhoj.Models;

// Plain DTO used by the CV builder (JSON), by saved application snapshots
// and by the CV preview partial. Property names are camelCased in JSON.
public class CvData
{
    public string Tpl { get; set; } = "classic";
    public string Name { get; set; } = "";
    public string Email { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Address { get; set; } = "";
    public string Obj { get; set; } = "";
    public string Skills { get; set; } = "";
    public List<EduItem> Edu { get; set; } = new();
    public List<ExpItem> Exp { get; set; } = new();
    public List<ProjItem> Proj { get; set; } = new();
}

public class EduItem
{
    public string Deg { get; set; } = "";
    public string Dept { get; set; } = "";
    public string Inst { get; set; } = "";
    public string Cgpa { get; set; } = "";
    public string Year { get; set; } = "";
}

public class ExpItem
{
    public string Role { get; set; } = "";
    public string Org { get; set; } = "";
    public string Period { get; set; } = "";
    public string Desc { get; set; } = "";
}

public class ProjItem
{
    public string Title { get; set; } = "";
    public string Desc { get; set; } = "";
}
