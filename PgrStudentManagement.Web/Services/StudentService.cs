using PgrStudentManagement.Web.Data;
using PgrStudentManagement.Web.Models;

namespace PgrStudentManagement.Web.Services;

/// <summary>
/// Business layer component coordinating PGR Student Management operations, business rules, calculations, and data access.
/// </summary>
public class StudentService
{
    private readonly StudentStorage _studentStorage;

    public StudentService(StudentStorage studentStorage)
    {
        _studentStorage = studentStorage ?? throw new ArgumentNullException(nameof(studentStorage));
    }

    /// <summary>
    /// Retrieves all students from storage with business layer ordering applied (W3-UC02).
    /// Default order is ascending by StudentNumber.
    /// </summary>
    public IReadOnlyList<Student> GetStudents(string? sortBy = "number")
    {
        var students = _studentStorage.GetStudents();

        return (sortBy?.ToLowerInvariant()) switch
        {
            "name" => students.OrderBy(s => s.LastName).ThenBy(s => s.FirstName).ToList().AsReadOnly(),
            "status" => students.OrderBy(s => s.Status).ThenBy(s => s.StudentNumber).ToList().AsReadOnly(),
            "programme" => students.OrderBy(s => s.Course).ThenBy(s => s.StudentNumber).ToList().AsReadOnly(),
            _ => students.OrderBy(s => s.StudentNumber).ToList().AsReadOnly()
        };
    }

    /// <summary>
    /// Finds a single student by student number (W2-UC01, W2-UC02, W3-UC03).
    /// </summary>
    public Student? GetStudent(string? studentNumber)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            return null;
        }

        return _studentStorage.GetStudent(studentNumber);
    }

    /// <summary>
    /// Checks whether a student with the given student number exists.
    /// </summary>
    public bool StudentExists(string? studentNumber)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            return false;
        }

        return _studentStorage.Exists(studentNumber);
    }

    /// <summary>
    /// Creates a new student record according to W3-UC01 business rules.
    /// Validates mandatory fields and checks for duplicate student number.
    /// </summary>
    public ServiceResult<Student> CreateStudent(Student student)
    {
        if (student == null)
        {
            return ServiceResult<Student>.Fail("Student payload cannot be null.");
        }

        // Validate mandatory attributes
        if (string.IsNullOrWhiteSpace(student.StudentNumber))
        {
            return ServiceResult<Student>.Fail("Student number is mandatory.", "Required_StudentNumber");
        }

        if (string.IsNullOrWhiteSpace(student.FirstName))
        {
            return ServiceResult<Student>.Fail("First name is mandatory.", "Required_FirstName");
        }

        if (string.IsNullOrWhiteSpace(student.LastName))
        {
            return ServiceResult<Student>.Fail("Last name is mandatory.", "Required_LastName");
        }

        // Normalize student number
        student.StudentNumber = student.StudentNumber.Trim();
        student.FirstName = student.FirstName.Trim();
        student.LastName = student.LastName.Trim();
        if (student.Course != null) student.Course = student.Course.Trim();
        if (student.ModeOfStudy != null) student.ModeOfStudy = student.ModeOfStudy.Trim();
        if (student.ThesisTitle != null) student.ThesisTitle = student.ThesisTitle.Trim();

        // Check for duplicate student number
        if (_studentStorage.Exists(student.StudentNumber))
        {
            return ServiceResult<Student>.Fail(
                $"A student with student number '{student.StudentNumber}' already exists.",
                "DuplicateStudentNumber");
        }

        // Validate dates if present
        if (student.StartDate.HasValue && student.ExpectedSubmissionDate.HasValue &&
            student.ExpectedSubmissionDate.Value < student.StartDate.Value)
        {
            return ServiceResult<Student>.Fail(
                "Expected submission date cannot be earlier than the start date.",
                "Invalid_ExpectedDate");
        }

        if (student.StartDate.HasValue && student.ActualSubmissionDate.HasValue &&
            student.ActualSubmissionDate.Value < student.StartDate.Value)
        {
            return ServiceResult<Student>.Fail(
                "Actual submission date cannot be earlier than the start date.",
                "Invalid_ActualDate");
        }

        // Initialize OriginalExpectedSubmissionDate if not explicitly provided
        if (student.ExpectedSubmissionDate.HasValue && !student.OriginalExpectedSubmissionDate.HasValue)
        {
            student.OriginalExpectedSubmissionDate = student.ExpectedSubmissionDate;
        }

        // If actual submission date is supplied at creation, ensure status is Submitted
        if (student.ActualSubmissionDate.HasValue && student.Status != Status.Completed)
        {
            student.Status = Status.Submitted;
        }

        _studentStorage.AddStudent(student);
        return ServiceResult<Student>.Ok(student);
    }

    /// <summary>
    /// Updates the academic status of a student (W2-UC03).
    /// </summary>
    public ServiceResult UpdateStatus(string? studentNumber, Status status)
    {
        var student = GetStudent(studentNumber);
        if (student == null)
        {
            return ServiceResult.Fail("Student not found.", "NotFound");
        }

        if (!Enum.IsDefined(typeof(Status), status))
        {
            return ServiceResult.Fail("Invalid status value provided.", "InvalidStatus");
        }

        student.Status = status;
        _studentStorage.UpdateStudent(student);
        return ServiceResult.Ok();
    }

    /// <summary>
    /// Updates the expected thesis submission date of a student (W2-UC04).
    /// </summary>
    public ServiceResult UpdateExpectedSubmissionDate(string? studentNumber, DateTime? expectedSubmissionDate)
    {
        var student = GetStudent(studentNumber);
        if (student == null)
        {
            return ServiceResult.Fail("Student not found.", "NotFound");
        }

        if (!expectedSubmissionDate.HasValue)
        {
            return ServiceResult.Fail("Invalid expected submission date", "MissingDate");
        }

        if (student.StartDate.HasValue && expectedSubmissionDate.Value < student.StartDate.Value)
        {
            return ServiceResult.Fail("Invalid expected submission date: Expected date cannot be prior to start date.", "DateBeforeStartDate");
        }

        student.ExpectedSubmissionDate = expectedSubmissionDate.Value;
        _studentStorage.UpdateStudent(student);
        return ServiceResult.Ok();
    }

    /// <summary>
    /// Records the actual thesis submission date and atomically updates status to Submitted (W2-UC05).
    /// </summary>
    public ServiceResult RecordSubmission(string? studentNumber, DateTime? actualSubmissionDate)
    {
        var student = GetStudent(studentNumber);
        if (student == null)
        {
            return ServiceResult.Fail("Student not found.", "NotFound");
        }

        if (!actualSubmissionDate.HasValue)
        {
            return ServiceResult.Fail("Invalid submission date", "MissingDate");
        }

        if (student.StartDate.HasValue && actualSubmissionDate.Value < student.StartDate.Value)
        {
            return ServiceResult.Fail("Invalid submission date: Submission date cannot be before enrolment start date.", "DateBeforeStartDate");
        }

        // Apply atomic change: both submission date and status change together
        student.ActualSubmissionDate = actualSubmissionDate.Value;
        student.Status = Status.Submitted;

        _studentStorage.UpdateStudent(student);
        return ServiceResult.Ok();
    }

    /// <summary>
    /// Searches students matching free-text search terms and/or structured filters (W3-UC04).
    /// </summary>
    public IReadOnlyList<Student> SearchStudents(
        string? searchTerm,
        Status? statusFilter = null,
        string? modeFilter = null,
        string? programmeFilter = null)
    {
        var students = _studentStorage.GetStudents().AsEnumerable();

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var term = searchTerm.Trim();
            students = students.Where(s =>
                s.StudentNumber.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                s.FirstName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                s.LastName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                (s.Course != null && s.Course.Contains(term, StringComparison.OrdinalIgnoreCase)) ||
                (s.ThesisTitle != null && s.ThesisTitle.Contains(term, StringComparison.OrdinalIgnoreCase)));
        }

        if (statusFilter.HasValue)
        {
            students = students.Where(s => s.Status == statusFilter.Value);
        }

        if (!string.IsNullOrWhiteSpace(modeFilter))
        {
            students = students.Where(s =>
                s.ModeOfStudy != null && s.ModeOfStudy.Equals(modeFilter.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(programmeFilter))
        {
            students = students.Where(s =>
                s.Course != null && s.Course.Equals(programmeFilter.Trim(), StringComparison.OrdinalIgnoreCase));
        }

        return students.OrderBy(s => s.StudentNumber).ToList().AsReadOnly();
    }

    /// <summary>
    /// Aggregates and calculates statistics across all PGR students in storage (W3-UC05).
    /// </summary>
    public StudentStatisticsViewModel GetStatistics()
    {
        var students = _studentStorage.GetStudents();

        var stats = new StudentStatisticsViewModel
        {
            TotalStudents = students.Count,
            SubmissionsCount = students.Count(s => s.ActualSubmissionDate.HasValue || s.Status == Status.Submitted || s.Status == Status.Completed),
            WritingUpCount = students.Count(s => s.Status == Status.WritingUp),
            ResearchingCount = students.Count(s => s.Status == Status.Researching),
            WithThesisInfoCount = students.Count(s => s.HasThesisInformation)
        };

        foreach (var status in Enum.GetValues<Status>())
        {
            stats.StatusDistribution[status] = students.Count(s => s.Status == status);
        }

        foreach (var student in students)
        {
            var mode = string.IsNullOrWhiteSpace(student.ModeOfStudy) ? "Unspecified" : student.ModeOfStudy.Trim();
            stats.ModeOfStudyDistribution[mode] = stats.ModeOfStudyDistribution.GetValueOrDefault(mode, 0) + 1;

            var prog = string.IsNullOrWhiteSpace(student.Course) ? "Unspecified" : student.Course.Trim();
            stats.ProgrammeDistribution[prog] = stats.ProgrammeDistribution.GetValueOrDefault(prog, 0) + 1;
        }

        return stats;
    }
}
