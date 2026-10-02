using PgrStudentManagement.Web.Models;

namespace PgrStudentManagement.Web.Data;

/// <summary>
/// Data layer component responsible for managing in-memory storage of Student records.
/// Registered as a Singleton in dependency injection so the collection persists across HTTP requests.
/// </summary>
public class StudentStorage
{
    private readonly List<Student> _students;

    public StudentStorage()
    {
        _students = new List<Student>();
        InitialiseDefaultData();
    }

    private void InitialiseDefaultData()
    {
        _students.Clear();
        _students.AddRange(new List<Student>
        {
            new Student
            {
                StudentNumber = "S100001",
                FirstName = "John",
                LastName = "Doe",
                Course = "PhD Computing",
                ModeOfStudy = "Full-time",
                StartDate = new DateTime(2025, 10, 1),
                Status = Status.Researching,
                ThesisTitle = "Explainable Models for Clinical Decision Support",
                ExpectedSubmissionDate = new DateTime(2028, 9, 30),
                OriginalExpectedSubmissionDate = new DateTime(2028, 9, 30),
                ActualSubmissionDate = null
            },
            new Student
            {
                StudentNumber = "S100002",
                FirstName = "Jane",
                LastName = "Smith",
                Course = "Professional Doctorate",
                ModeOfStudy = "Part-time",
                StartDate = new DateTime(2024, 10, 1),
                Status = Status.WritingUp,
                ThesisTitle = null,
                ExpectedSubmissionDate = null,
                OriginalExpectedSubmissionDate = null,
                ActualSubmissionDate = null
            }
        });
    }

    /// <summary>
    /// Resets the in-memory student collection to the default initial dataset.
    /// Useful for test isolation and application reset.
    /// </summary>
    public void ResetDefaultStudents()
    {
        InitialiseDefaultData();
    }

    /// <summary>
    /// Clears all students from storage.
    /// </summary>
    public void Clear()
    {
        _students.Clear();
    }

    /// <summary>
    /// Returns a read-only list of all students currently in storage.
    /// </summary>
    public IReadOnlyList<Student> GetStudents()
    {
        return _students.ToList().AsReadOnly();
    }

    /// <summary>
    /// Finds a student by their unique student number (case-insensitive).
    /// </summary>
    public Student? GetStudent(string? studentNumber)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            return null;
        }

        return _students.FirstOrDefault(s =>
            s.StudentNumber.Equals(studentNumber.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks whether a student with the given student number already exists in storage.
    /// </summary>
    public bool Exists(string? studentNumber)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            return false;
        }

        return _students.Any(s =>
            s.StudentNumber.Equals(studentNumber.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Adds a new student to storage.
    /// </summary>
    public void AddStudent(Student student)
    {
        ArgumentNullException.ThrowIfNull(student);
        _students.Add(student);
    }

    /// <summary>
    /// Updates an existing student's data in storage.
    /// </summary>
    public void UpdateStudent(Student student)
    {
        ArgumentNullException.ThrowIfNull(student);
        var index = _students.FindIndex(s =>
            s.StudentNumber.Equals(student.StudentNumber.Trim(), StringComparison.OrdinalIgnoreCase));

        if (index >= 0)
        {
            _students[index] = student;
        }
    }
}
