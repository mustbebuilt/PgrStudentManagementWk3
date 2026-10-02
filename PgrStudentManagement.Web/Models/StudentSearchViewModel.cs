namespace PgrStudentManagement.Web.Models;

/// <summary>
/// ViewModel representing search parameters and results for W3-UC04 (Search Students).
/// </summary>
public class StudentSearchViewModel
{
    public string? SearchTerm { get; set; }

    public Status? StatusFilter { get; set; }

    public string? ModeFilter { get; set; }

    public string? ProgrammeFilter { get; set; }

    public IReadOnlyList<Student> Results { get; set; } = Array.Empty<Student>();

    public bool HasSearched { get; set; }

    public int TotalResults => Results.Count;
}
