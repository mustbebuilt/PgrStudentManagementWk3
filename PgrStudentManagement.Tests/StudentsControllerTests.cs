using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Moq;
using PgrStudentManagement.Web.Controllers;
using PgrStudentManagement.Web.Data;
using PgrStudentManagement.Web.Models;
using PgrStudentManagement.Web.Services;
using Xunit;
using Xunit.Abstractions;

namespace PgrStudentManagement.Tests;

[Collection("Sequential")]
public class StudentsControllerTests
{
    private readonly ITestOutputHelper _output;
    private readonly StudentStorage _storage;
    private readonly StudentService _service;

    public StudentsControllerTests(ITestOutputHelper output)
    {
        _output = output;
        _storage = new StudentStorage();
        _storage.ResetDefaultStudents();
        _service = new StudentService(_storage);
    }

    private StudentsController CreateControllerWithTempData()
    {
        var controller = new StudentsController(_service);
        var tempData = new TempDataDictionary(
            new Microsoft.AspNetCore.Http.DefaultHttpContext(),
            Mock.Of<ITempDataProvider>());
        controller.TempData = tempData;
        return controller;
    }

    #region W3-UC02: List Students

    [Fact(DisplayName = "W3-UC02 [Success]: Index returns view with all students")]
    [Trait("UseCase", "W3-UC02")]
    public void W3_UC02_Index_ReturnsViewWithStudents()
    {
        // Arrange
        _output.WriteLine("[GIVEN] A population of students in storage");
        var controller = CreateControllerWithTempData();

        // Act
        _output.WriteLine("[WHEN] Index action is invoked");
        var result = controller.Index();

        // Assert
        _output.WriteLine("[THEN] View is returned with student collection");
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsAssignableFrom<IEnumerable<Student>>(viewResult.Model);
        Assert.Equal(2, model.Count());
    }

    #endregion

    #region W3-UC01: Create Student

    [Fact(DisplayName = "W3-UC01 [Success]: Create GET returns view with initialized model")]
    [Trait("UseCase", "W3-UC01")]
    public void W3_UC01_Create_Get_ReturnsView()
    {
        // Arrange
        var controller = CreateControllerWithTempData();

        // Act
        var result = controller.Create();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Student>(viewResult.Model);
        Assert.Equal(Status.Researching, model.Status);
    }

    [Fact(DisplayName = "W3-UC01 [Success]: Create POST adds student and redirects to Details")]
    [Trait("UseCase", "W3-UC01")]
    public void W3_UC01_Create_Post_RedirectsToDetails_WhenValid()
    {
        // Arrange
        var controller = CreateControllerWithTempData();
        var newStudent = new Student
        {
            StudentNumber = "S100003",
            FirstName = "Alice",
            LastName = "Walker",
            Course = "PhD Engineering",
            ModeOfStudy = "Full-time",
            StartDate = new DateTime(2025, 10, 1),
            Status = Status.Researching
        };

        // Act
        var result = controller.Create(newStudent);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(StudentsController.Details), redirect.ActionName);
        Assert.Equal("S100003", redirect.RouteValues?["studentNumber"]);
        Assert.True(_storage.Exists("S100003"));
    }

    [Fact(DisplayName = "W3-UC01 [Alt Flow]: Create POST returns view with error when student number is duplicate")]
    [Trait("UseCase", "W3-UC01")]
    public void W3_UC01_Create_Post_ReturnsView_WhenDuplicateStudentNumber()
    {
        // Arrange
        var controller = CreateControllerWithTempData();
        var duplicateStudent = new Student
        {
            StudentNumber = "S100001", // Duplicate ID
            FirstName = "Duplicate",
            LastName = "User"
        };

        // Act
        var result = controller.Create(duplicateStudent);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.False(controller.ModelState.IsValid);
        Assert.True(controller.ModelState.ContainsKey(nameof(Student.StudentNumber)));
    }

    #endregion

    #region W3-UC03: View Student Details

    [Fact(DisplayName = "W3-UC03 [Success]: Details returns view with student")]
    [Trait("UseCase", "W3-UC03")]
    public void W3_UC03_Details_ReturnsView_WhenStudentExists()
    {
        // Arrange
        var controller = CreateControllerWithTempData();

        // Act
        var result = controller.Details("S100001");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Student>(viewResult.Model);
        Assert.Equal("S100001", model.StudentNumber);
    }

    [Fact(DisplayName = "W3-UC03 [Alt Flow]: Details returns StudentNotFound when student does not exist")]
    [Trait("UseCase", "W3-UC03")]
    public void W3_UC03_Details_ReturnsNotFound_WhenDoesNotExist()
    {
        // Arrange
        var controller = CreateControllerWithTempData();

        // Act
        var result = controller.Details("UNKNOWN");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("StudentNotFound", viewResult.ViewName);
    }

    #endregion

    #region W3-UC04: Search Students

    [Fact(DisplayName = "W3-UC04 [Success]: Search returns matching results")]
    [Trait("UseCase", "W3-UC04")]
    public void W3_UC04_Search_ReturnsMatchingResults()
    {
        // Arrange
        var controller = CreateControllerWithTempData();

        // Act
        var result = controller.Search("John", null, null, null);

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<StudentSearchViewModel>(viewResult.Model);
        Assert.True(model.HasSearched);
        Assert.Single(model.Results);
        Assert.Equal("S100001", model.Results.First().StudentNumber);
    }

    #endregion

    #region W3-UC05: View Student Statistics

    [Fact(DisplayName = "W3-UC05 [Success]: Statistics returns view with metrics")]
    [Trait("UseCase", "W3-UC05")]
    public void W3_UC05_Statistics_ReturnsViewWithMetrics()
    {
        // Arrange
        var controller = CreateControllerWithTempData();

        // Act
        var result = controller.Statistics();

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<StudentStatisticsViewModel>(viewResult.Model);
        Assert.Equal(2, model.TotalStudents);
    }

    #endregion

    #region Reused W2-UC01 to W2-UC05 Verification

    [Fact(DisplayName = "W2-UC01 [Success]: Displays enrolment details for existing student")]
    public void W2_UC01_EnrolmentDetails_ReturnsViewWithStudent_WhenStudentExists()
    {
        // Arrange
        var controller = CreateControllerWithTempData();

        // Act
        var result = controller.EnrolmentDetails("S100001");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Student>(viewResult.Model);
        Assert.Equal("S100001", model.StudentNumber);
        Assert.Equal("PhD Computing", model.Course);
    }

    [Fact(DisplayName = "W2-UC01 [Alt Flow]: Displays StudentNotFound when ID does not match")]
    public void W2_UC01_EnrolmentDetails_ReturnsStudentNotFound_WhenStudentDoesNotExist()
    {
        // Arrange
        var controller = CreateControllerWithTempData();

        // Act
        var result = controller.EnrolmentDetails("UNKNOWN999");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal("StudentNotFound", viewResult.ViewName);
    }

    [Fact(DisplayName = "W2-UC02 [Success]: Displays thesis details for existing student")]
    public void W2_UC02_ThesisDetails_ReturnsViewWithStudent_WhenThesisInfoExists()
    {
        // Arrange
        var controller = CreateControllerWithTempData();

        // Act
        var result = controller.ThesisDetails("S100001");

        // Assert
        var viewResult = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<Student>(viewResult.Model);
        Assert.True(model.HasThesisInformation);
    }

    [Fact(DisplayName = "W2-UC03 [Success]: Updates status and redirects to EnrolmentDetails")]
    public void W2_UC03_EditStatus_Post_UpdatesStatusAndRedirects()
    {
        // Arrange
        var controller = CreateControllerWithTempData();

        // Act
        var result = controller.EditStatus("S100001", Status.Completed);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(StudentsController.EnrolmentDetails), redirect.ActionName);
        Assert.Equal(Status.Completed, _storage.GetStudent("S100001")?.Status);
    }

    [Fact(DisplayName = "W2-UC04 [Success]: Updates expected date and redirects to ThesisDetails")]
    public void W2_UC04_EditExpectedSubmissionDate_Post_UpdatesDateAndRedirects()
    {
        // Arrange
        var controller = CreateControllerWithTempData();
        var newDate = new DateTime(2029, 6, 30);

        // Act
        var result = controller.EditExpectedSubmissionDate("S100001", newDate);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(StudentsController.ThesisDetails), redirect.ActionName);
        Assert.Equal(newDate, _storage.GetStudent("S100001")?.ExpectedSubmissionDate);
    }

    [Fact(DisplayName = "W2-UC05 [Success]: Atomically updates actual submission date and status to Submitted")]
    public void W2_UC05_RecordSubmission_Post_AtomicallyUpdatesSubmissionDateAndStatus()
    {
        // Arrange
        var controller = CreateControllerWithTempData();
        var submissionDate = new DateTime(2028, 9, 15);

        // Act
        var result = controller.RecordSubmission("S100001", submissionDate);

        // Assert
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal(nameof(StudentsController.ThesisDetails), redirect.ActionName);
        var student = _storage.GetStudent("S100001");
        Assert.Equal(submissionDate, student?.ActualSubmissionDate);
        Assert.Equal(Status.Submitted, student?.Status);
    }

    #endregion
}
