using PgrStudentManagement.Web.Data;
using PgrStudentManagement.Web.Models;
using PgrStudentManagement.Web.Services;
using Xunit;

namespace PgrStudentManagement.Tests;

public class StudentServiceTests
{
    private readonly StudentStorage _storage;
    private readonly StudentService _service;

    public StudentServiceTests()
    {
        _storage = new StudentStorage();
        _service = new StudentService(_storage);
    }

    #region W3-UC01: Create Student Tests

    [Fact(DisplayName = "W3-UC01 [Success]: Creates new student when valid data supplied")]
    public void CreateStudent_Succeeds_WhenDataIsValid()
    {
        // Arrange
        var newStudent = new Student
        {
            StudentNumber = "S100003",
            FirstName = "Alice",
            LastName = "Walker",
            Course = "PhD Engineering",
            ModeOfStudy = "Full-time",
            StartDate = new DateTime(2025, 1, 1),
            Status = Status.Researching,
            ExpectedSubmissionDate = new DateTime(2028, 1, 1)
        };

        // Act
        var result = _service.CreateStudent(newStudent);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("S100003", result.Data.StudentNumber);
        Assert.True(_storage.Exists("S100003"));
        Assert.Equal(new DateTime(2028, 1, 1), result.Data.OriginalExpectedSubmissionDate);
    }

    [Fact(DisplayName = "W3-UC01 [Alt Flow - Duplicate]: Rejects creation if student number already exists")]
    public void CreateStudent_Fails_WhenStudentNumberIsDuplicate()
    {
        // Arrange
        var duplicateStudent = new Student
        {
            StudentNumber = "S100001", // Already in default storage
            FirstName = "Duplicate",
            LastName = "Person"
        };

        // Act
        var result = _service.CreateStudent(duplicateStudent);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("DuplicateStudentNumber", result.ErrorCode);
        Assert.Contains("already exists", result.ErrorMessage);
    }

    [Theory(DisplayName = "W3-UC01 [Alt Flow - Required Fields]: Rejects creation when mandatory fields are missing")]
    [InlineData("", "John", "Doe", "Required_StudentNumber")]
    [InlineData("S100009", "", "Doe", "Required_FirstName")]
    [InlineData("S100009", "John", "", "Required_LastName")]
    public void CreateStudent_Fails_WhenMandatoryFieldsMissing(string id, string first, string last, string expectedError)
    {
        // Arrange
        var student = new Student
        {
            StudentNumber = id,
            FirstName = first,
            LastName = last
        };

        // Act
        var result = _service.CreateStudent(student);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(expectedError, result.ErrorCode);
    }

    [Fact(DisplayName = "W3-UC01 [Validation]: Rejects expected submission date before start date")]
    public void CreateStudent_Fails_WhenExpectedDateBeforeStartDate()
    {
        // Arrange
        var student = new Student
        {
            StudentNumber = "S100099",
            FirstName = "Test",
            LastName = "Student",
            StartDate = new DateTime(2025, 10, 1),
            ExpectedSubmissionDate = new DateTime(2025, 1, 1) // Before start date
        };

        // Act
        var result = _service.CreateStudent(student);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("Invalid_ExpectedDate", result.ErrorCode);
    }

    #endregion

    #region W3-UC02: List Students Tests

    [Fact(DisplayName = "W3-UC02 [Success]: Retrieves all students with default ordering by student number")]
    public void GetStudents_ReturnsOrderedList()
    {
        // Act
        var students = _service.GetStudents();

        // Assert
        Assert.Equal(2, students.Count);
        Assert.Equal("S100001", students[0].StudentNumber);
        Assert.Equal("S100002", students[1].StudentNumber);
    }

    [Fact(DisplayName = "W3-UC02 [Alt Flow]: Returns empty list when no students in storage")]
    public void GetStudents_ReturnsEmptyList_WhenNoRecords()
    {
        // Arrange
        _storage.Clear();

        // Act
        var students = _service.GetStudents();

        // Assert
        Assert.Empty(students);
    }

    #endregion

    #region W3-UC03: View Student Details Tests

    [Fact(DisplayName = "W3-UC03 [Success]: Retrieves student details by student number")]
    public void GetStudent_ReturnsCorrectStudent()
    {
        // Act
        var student = _service.GetStudent("S100001");

        // Assert
        Assert.NotNull(student);
        Assert.Equal("John", student.FirstName);
        Assert.Equal("PhD Computing", student.Course);
    }

    [Fact(DisplayName = "W3-UC03 [Alt Flow]: Returns null when student number does not exist")]
    public void GetStudent_ReturnsNull_WhenDoesNotExist()
    {
        // Act
        var student = _service.GetStudent("S999999");

        // Assert
        Assert.Null(student);
    }

    #endregion

    #region W2-UC03: Update Status Tests

    [Fact(DisplayName = "W2-UC03 [Success]: Updates status of existing student")]
    public void UpdateStatus_Succeeds_ForValidStudent()
    {
        // Act
        var result = _service.UpdateStatus("S100001", Status.Completed);

        // Assert
        Assert.True(result.Success);
        var updated = _storage.GetStudent("S100001");
        Assert.Equal(Status.Completed, updated?.Status);
    }

    [Fact(DisplayName = "W2-UC03 [Alt Flow]: Fails if student not found")]
    public void UpdateStatus_Fails_WhenStudentNotFound()
    {
        // Act
        var result = _service.UpdateStatus("NONEXISTENT", Status.Completed);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("NotFound", result.ErrorCode);
    }

    #endregion

    #region W2-UC04: Update Expected Submission Date Tests

    [Fact(DisplayName = "W2-UC04 [Success]: Updates expected date for existing student")]
    public void UpdateExpectedSubmissionDate_Succeeds_WhenValid()
    {
        // Arrange
        var newDate = new DateTime(2029, 6, 30);

        // Act
        var result = _service.UpdateExpectedSubmissionDate("S100001", newDate);

        // Assert
        Assert.True(result.Success);
        var student = _storage.GetStudent("S100001");
        Assert.Equal(newDate, student?.ExpectedSubmissionDate);
    }

    [Fact(DisplayName = "W2-UC04 [Alt Flow]: Fails when date is prior to start date")]
    public void UpdateExpectedSubmissionDate_Fails_WhenDateBeforeStartDate()
    {
        // Act (S100001 start date is 2025-10-01)
        var result = _service.UpdateExpectedSubmissionDate("S100001", new DateTime(2024, 1, 1));

        // Assert
        Assert.False(result.Success);
        Assert.Equal("DateBeforeStartDate", result.ErrorCode);
    }

    [Fact(DisplayName = "W2-UC04 [Alt Flow]: Fails when date is null")]
    public void UpdateExpectedSubmissionDate_Fails_WhenDateIsNull()
    {
        // Act
        var result = _service.UpdateExpectedSubmissionDate("S100001", null);

        // Assert
        Assert.False(result.Success);
        Assert.Equal("MissingDate", result.ErrorCode);
    }

    #endregion

    #region W2-UC05: Record Actual Submission Tests

    [Fact(DisplayName = "W2-UC05 [Success]: Atomically sets submission date and status to Submitted")]
    public void RecordSubmission_AtomicallyUpdatesDateAndStatus()
    {
        // Arrange
        var submissionDate = new DateTime(2028, 9, 15);

        // Act
        var result = _service.RecordSubmission("S100001", submissionDate);

        // Assert
        Assert.True(result.Success);
        var student = _storage.GetStudent("S100001");
        Assert.Equal(submissionDate, student?.ActualSubmissionDate);
        Assert.Equal(Status.Submitted, student?.Status);
    }

    [Fact(DisplayName = "W2-UC05 [Alt Flow]: Fails when date is null without mutating state")]
    public void RecordSubmission_Fails_WhenDateIsNull()
    {
        // Arrange
        var originalStatus = _storage.GetStudent("S100001")!.Status;

        // Act
        var result = _service.RecordSubmission("S100001", null);

        // Assert
        Assert.False(result.Success);
        Assert.Equal(originalStatus, _storage.GetStudent("S100001")!.Status);
    }

    #endregion

    #region W3-UC04: Search Students Tests

    [Fact(DisplayName = "W3-UC04 [Success]: Searches by student name, number, and course case-insensitively")]
    public void SearchStudents_ReturnsMatches()
    {
        // Act
        var resultsByName = _service.SearchStudents("john");
        var resultsById = _service.SearchStudents("100002");
        var resultsByCourse = _service.SearchStudents("computing");

        // Assert
        Assert.Single(resultsByName);
        Assert.Equal("S100001", resultsByName[0].StudentNumber);

        Assert.Single(resultsById);
        Assert.Equal("S100002", resultsById[0].StudentNumber);

        Assert.Single(resultsByCourse);
        Assert.Equal("S100001", resultsByCourse[0].StudentNumber);
    }

    [Fact(DisplayName = "W3-UC04 [Alt Flow]: Returns empty when no students match criteria")]
    public void SearchStudents_ReturnsEmpty_WhenNoMatch()
    {
        // Act
        var results = _service.SearchStudents("NonExistentKeywordXYZ");

        // Assert
        Assert.Empty(results);
    }

    #endregion

    #region W3-UC05: View Statistics Tests

    [Fact(DisplayName = "W3-UC05 [Success]: Calculates accurate cohort metrics")]
    public void GetStatistics_CalculatesAccurateMetrics()
    {
        // Act
        var stats = _service.GetStatistics();

        // Assert
        Assert.True(stats.HasData);
        Assert.Equal(2, stats.TotalStudents);
        Assert.Equal(1, stats.StatusDistribution[Status.Researching]);
        Assert.Equal(1, stats.StatusDistribution[Status.WritingUp]);
        Assert.Equal(1, stats.ModeOfStudyDistribution["Full-time"]);
        Assert.Equal(1, stats.ModeOfStudyDistribution["Part-time"]);
    }

    [Fact(DisplayName = "W3-UC05 [Alt Flow]: Handles empty database gracefully")]
    public void GetStatistics_HandlesEmptyData()
    {
        // Arrange
        _storage.Clear();

        // Act
        var stats = _service.GetStatistics();

        // Assert
        Assert.False(stats.HasData);
        Assert.Equal(0, stats.TotalStudents);
        Assert.Equal(0, stats.SubmissionRatePercentage);
    }

    #endregion
}
