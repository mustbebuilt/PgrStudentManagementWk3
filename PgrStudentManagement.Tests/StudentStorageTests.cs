using PgrStudentManagement.Web.Data;
using PgrStudentManagement.Web.Models;
using Xunit;

namespace PgrStudentManagement.Tests;

public class StudentStorageTests
{
    [Fact(DisplayName = "StudentStorage: Initialises with default student records")]
    public void StudentStorage_InitialisesWithDefaultRecords()
    {
        // Arrange
        var storage = new StudentStorage();

        // Act
        var students = storage.GetStudents();

        // Assert
        Assert.NotNull(students);
        Assert.Equal(2, students.Count);
        Assert.Contains(students, s => s.StudentNumber == "S100001");
        Assert.Contains(students, s => s.StudentNumber == "S100002");
    }

    [Fact(DisplayName = "StudentStorage: GetStudent returns student when matching student number")]
    public void GetStudent_ReturnsStudent_WhenFound()
    {
        // Arrange
        var storage = new StudentStorage();

        // Act
        var student = storage.GetStudent("s100001"); // Case-insensitive test

        // Assert
        Assert.NotNull(student);
        Assert.Equal("S100001", student.StudentNumber);
        Assert.Equal("John", student.FirstName);
    }

    [Fact(DisplayName = "StudentStorage: GetStudent returns null when student not found")]
    public void GetStudent_ReturnsNull_WhenNotFound()
    {
        // Arrange
        var storage = new StudentStorage();

        // Act
        var student = storage.GetStudent("NONEXISTENT");

        // Assert
        Assert.Null(student);
    }

    [Fact(DisplayName = "StudentStorage: Exists returns true for existing student and false for unknown")]
    public void Exists_ReturnsCorrectBoolean()
    {
        // Arrange
        var storage = new StudentStorage();

        // Act & Assert
        Assert.True(storage.Exists("S100001"));
        Assert.True(storage.Exists("s100002"));
        Assert.False(storage.Exists("S999999"));
        Assert.False(storage.Exists(null));
        Assert.False(storage.Exists(""));
    }

    [Fact(DisplayName = "StudentStorage: AddStudent appends record to in-memory collection")]
    public void AddStudent_AddsRecord()
    {
        // Arrange
        var storage = new StudentStorage();
        var newStudent = new Student
        {
            StudentNumber = "S100003",
            FirstName = "Alice",
            LastName = "Walker",
            Status = Status.Researching
        };

        // Act
        storage.AddStudent(newStudent);

        // Assert
        Assert.Equal(3, storage.GetStudents().Count);
        Assert.True(storage.Exists("S100003"));
        Assert.Equal("Alice", storage.GetStudent("S100003")?.FirstName);
    }

    [Fact(DisplayName = "StudentStorage: UpdateStudent updates existing record")]
    public void UpdateStudent_ModifiesExistingRecord()
    {
        // Arrange
        var storage = new StudentStorage();
        var student = storage.GetStudent("S100001")!;
        student.Status = Status.WritingUp;

        // Act
        storage.UpdateStudent(student);

        // Assert
        var updated = storage.GetStudent("S100001");
        Assert.NotNull(updated);
        Assert.Equal(Status.WritingUp, updated.Status);
    }

    [Fact(DisplayName = "StudentStorage: ResetDefaultStudents restores initial state")]
    public void ResetDefaultStudents_RestoresInitialState()
    {
        // Arrange
        var storage = new StudentStorage();
        storage.AddStudent(new Student { StudentNumber = "S999999", FirstName = "Temp", LastName = "User" });
        Assert.Equal(3, storage.GetStudents().Count);

        // Act
        storage.ResetDefaultStudents();

        // Assert
        Assert.Equal(2, storage.GetStudents().Count);
        Assert.False(storage.Exists("S999999"));
    }
}
