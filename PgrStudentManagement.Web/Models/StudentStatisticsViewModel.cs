namespace PgrStudentManagement.Web.Models;

/// <summary>
/// ViewModel representing aggregated student data and metrics for W3-UC05 (View Student Statistics).
/// </summary>
public class StudentStatisticsViewModel
{
    public int TotalStudents { get; set; }

    public Dictionary<Status, int> StatusDistribution { get; set; } = new();

    public Dictionary<string, int> ModeOfStudyDistribution { get; set; } = new();

    public Dictionary<string, int> ProgrammeDistribution { get; set; } = new();

    public int SubmissionsCount { get; set; }

    public int WritingUpCount { get; set; }

    public int ResearchingCount { get; set; }

    public int WithThesisInfoCount { get; set; }

    public double SubmissionRatePercentage =>
        TotalStudents > 0 ? Math.Round((double)SubmissionsCount / TotalStudents * 100, 1) : 0;

    public bool HasData => TotalStudents > 0;
}
