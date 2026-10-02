using Microsoft.AspNetCore.Mvc;
using PgrStudentManagement.Web.Models;
using PgrStudentManagement.Web.Services;

namespace PgrStudentManagement.Web.Controllers;

/// <summary>
/// Presentation layer controller for Postgraduate Research Student Management.
/// Receives HTTP requests, calls StudentService operations, and selects views and redirects.
/// Contains no direct data access or in-memory collections.
/// </summary>
public class StudentsController : Controller
{
    private readonly StudentService _studentService;

    public StudentsController(StudentService studentService)
    {
        _studentService = studentService ?? throw new ArgumentNullException(nameof(studentService));
    }

    #region W3-UC02: List Students

    // GET: /Students or /Students/Index
    public IActionResult Index(string? sortBy = "number")
    {
        var students = _studentService.GetStudents(sortBy);
        return View(students);
    }

    #endregion

    #region W3-UC03: View Student Details

    // GET: /Students/Details?studentNumber=S100001
    public IActionResult Details(string? studentNumber)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            ViewBag.StudentNumber = studentNumber ?? string.Empty;
            return View("StudentNotFound");
        }

        var student = _studentService.GetStudent(studentNumber);
        if (student == null)
        {
            ViewBag.StudentNumber = studentNumber;
            return View("StudentNotFound");
        }

        return View(student);
    }

    #endregion

    #region W3-UC01: Create Student

    // GET: /Students/Create
    [HttpGet]
    public IActionResult Create()
    {
        var model = new Student
        {
            Status = Status.Researching,
            StartDate = DateTime.Today
        };
        return View(model);
    }

    // POST: /Students/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Create(Student student)
    {
        if (student == null)
        {
            return BadRequest();
        }

        var result = _studentService.CreateStudent(student);

        if (!result.Success)
        {
            if (result.ErrorCode == "DuplicateStudentNumber")
            {
                ModelState.AddModelError(nameof(student.StudentNumber), result.ErrorMessage!);
            }
            else if (result.ErrorCode == "Required_StudentNumber")
            {
                ModelState.AddModelError(nameof(student.StudentNumber), result.ErrorMessage!);
            }
            else if (result.ErrorCode == "Required_FirstName")
            {
                ModelState.AddModelError(nameof(student.FirstName), result.ErrorMessage!);
            }
            else if (result.ErrorCode == "Required_LastName")
            {
                ModelState.AddModelError(nameof(student.LastName), result.ErrorMessage!);
            }
            else if (result.ErrorCode == "Invalid_ExpectedDate")
            {
                ModelState.AddModelError(nameof(student.ExpectedSubmissionDate), result.ErrorMessage!);
            }
            else if (result.ErrorCode == "Invalid_ActualDate")
            {
                ModelState.AddModelError(nameof(student.ActualSubmissionDate), result.ErrorMessage!);
            }
            else
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Unable to create student.");
            }

            return View(student);
        }

        TempData["SuccessMessage"] = $"Student {student.StudentNumber} ({student.FullName}) created successfully.";
        return RedirectToAction(nameof(Details), new { studentNumber = student.StudentNumber });
    }

    #endregion

    #region W3-UC04: Search Students

    // GET: /Students/Search
    [HttpGet]
    public IActionResult Search(string? searchTerm, Status? statusFilter, string? modeFilter, string? programmeFilter)
    {
        var hasSearched = !string.IsNullOrWhiteSpace(searchTerm) ||
                          statusFilter.HasValue ||
                          !string.IsNullOrWhiteSpace(modeFilter) ||
                          !string.IsNullOrWhiteSpace(programmeFilter);

        var results = hasSearched
            ? _studentService.SearchStudents(searchTerm, statusFilter, modeFilter, programmeFilter)
            : Array.Empty<Student>();

        var viewModel = new StudentSearchViewModel
        {
            SearchTerm = searchTerm,
            StatusFilter = statusFilter,
            ModeFilter = modeFilter,
            ProgrammeFilter = programmeFilter,
            Results = results,
            HasSearched = hasSearched
        };

        return View(viewModel);
    }

    #endregion

    #region W3-UC05: View Student Statistics

    // GET: /Students/Statistics
    [HttpGet]
    public IActionResult Statistics()
    {
        var statistics = _studentService.GetStatistics();
        return View(statistics);
    }

    #endregion

    #region W2-UC01: View Enrolment Details

    // GET: /Students/EnrolmentDetails?studentNumber=S100001
    public IActionResult EnrolmentDetails(string? studentNumber)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            ViewBag.StudentNumber = studentNumber ?? string.Empty;
            return View("StudentNotFound");
        }

        var student = _studentService.GetStudent(studentNumber);
        if (student == null)
        {
            ViewBag.StudentNumber = studentNumber;
            return View("StudentNotFound");
        }

        return View(student);
    }

    #endregion

    #region W2-UC02: View Thesis Details

    // GET: /Students/ThesisDetails?studentNumber=S100001
    public IActionResult ThesisDetails(string? studentNumber)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            ViewBag.StudentNumber = studentNumber ?? string.Empty;
            return View("StudentNotFound");
        }

        var student = _studentService.GetStudent(studentNumber);
        if (student == null)
        {
            ViewBag.StudentNumber = studentNumber;
            return View("StudentNotFound");
        }

        return View(student);
    }

    #endregion

    #region W2-UC03: Update Student Status

    // GET: /Students/EditStatus?studentNumber=S100001
    [HttpGet]
    public IActionResult EditStatus(string? studentNumber)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            ViewBag.StudentNumber = studentNumber ?? string.Empty;
            return View("StudentNotFound");
        }

        var student = _studentService.GetStudent(studentNumber);
        if (student == null)
        {
            ViewBag.StudentNumber = studentNumber;
            return View("StudentNotFound");
        }

        return View(student);
    }

    // POST: /Students/EditStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditStatus(string? studentNumber, Status status)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            ViewBag.StudentNumber = studentNumber ?? string.Empty;
            return View("StudentNotFound");
        }

        var student = _studentService.GetStudent(studentNumber);
        if (student == null)
        {
            ViewBag.StudentNumber = studentNumber;
            return View("StudentNotFound");
        }

        if (!Enum.IsDefined(typeof(Status), status))
        {
            ModelState.AddModelError(nameof(status), "Invalid status value provided.");
            return View(student);
        }

        var result = _studentService.UpdateStatus(studentNumber, status);
        if (!result.Success)
        {
            ModelState.AddModelError(nameof(status), result.ErrorMessage ?? "Failed to update status.");
            return View(student);
        }

        TempData["SuccessMessage"] = $"Status updated to {status} for student {student.StudentNumber}.";
        return RedirectToAction(nameof(EnrolmentDetails), new { studentNumber = student.StudentNumber });
    }

    #endregion

    #region W2-UC04: Update Expected Thesis Submission Date

    // GET: /Students/EditExpectedSubmissionDate?studentNumber=S100001
    [HttpGet]
    public IActionResult EditExpectedSubmissionDate(string? studentNumber)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            ViewBag.StudentNumber = studentNumber ?? string.Empty;
            return View("StudentNotFound");
        }

        var student = _studentService.GetStudent(studentNumber);
        if (student == null)
        {
            ViewBag.StudentNumber = studentNumber;
            return View("StudentNotFound");
        }

        return View(student);
    }

    // POST: /Students/EditExpectedSubmissionDate
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult EditExpectedSubmissionDate(string? studentNumber, DateTime? expectedSubmissionDate)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            ViewBag.StudentNumber = studentNumber ?? string.Empty;
            return View("StudentNotFound");
        }

        var student = _studentService.GetStudent(studentNumber);
        if (student == null)
        {
            ViewBag.StudentNumber = studentNumber;
            return View("StudentNotFound");
        }

        if (!expectedSubmissionDate.HasValue)
        {
            ModelState.AddModelError("expectedSubmissionDate", "Invalid expected submission date");
            return View(student);
        }

        var result = _studentService.UpdateExpectedSubmissionDate(studentNumber, expectedSubmissionDate);
        if (!result.Success)
        {
            ModelState.AddModelError("expectedSubmissionDate", result.ErrorMessage ?? "Failed to update expected date.");
            return View(student);
        }

        TempData["SuccessMessage"] = $"Expected submission date updated to {expectedSubmissionDate.Value:yyyy-MM-dd} for student {student.StudentNumber}.";
        return RedirectToAction(nameof(ThesisDetails), new { studentNumber = student.StudentNumber });
    }

    #endregion

    #region W2-UC05: Record Actual Thesis Submission

    // GET: /Students/RecordSubmission?studentNumber=S100001
    [HttpGet]
    public IActionResult RecordSubmission(string? studentNumber)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            ViewBag.StudentNumber = studentNumber ?? string.Empty;
            return View("StudentNotFound");
        }

        var student = _studentService.GetStudent(studentNumber);
        if (student == null)
        {
            ViewBag.StudentNumber = studentNumber;
            return View("StudentNotFound");
        }

        return View(student);
    }

    // POST: /Students/RecordSubmission
    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult RecordSubmission(string? studentNumber, DateTime? actualSubmissionDate)
    {
        if (string.IsNullOrWhiteSpace(studentNumber))
        {
            ViewBag.StudentNumber = studentNumber ?? string.Empty;
            return View("StudentNotFound");
        }

        var student = _studentService.GetStudent(studentNumber);
        if (student == null)
        {
            ViewBag.StudentNumber = studentNumber;
            return View("StudentNotFound");
        }

        if (!actualSubmissionDate.HasValue)
        {
            ModelState.AddModelError("actualSubmissionDate", "Invalid submission date");
            return View(student);
        }

        var result = _studentService.RecordSubmission(studentNumber, actualSubmissionDate);
        if (!result.Success)
        {
            ModelState.AddModelError("actualSubmissionDate", result.ErrorMessage ?? "Failed to record submission.");
            return View(student);
        }

        TempData["SuccessMessage"] = $"Actual thesis submission recorded on {actualSubmissionDate.Value:yyyy-MM-dd} and status updated to Submitted for student {student.StudentNumber}.";
        return RedirectToAction(nameof(ThesisDetails), new { studentNumber = student.StudentNumber });
    }

    #endregion
}
