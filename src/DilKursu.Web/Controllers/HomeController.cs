using System.Diagnostics;
using DilKursu.Business.Services.Abstract;
using DilKursu.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DilKursu.Web.Controllers;

[Authorize]
public class HomeController(IBranchService branchService, ITeacherService teacherService, IStudentService studentService, ICourseService courseService) : Controller
{
    /// <summary>
    /// Kontrol panelini; şube, öğretmen, öğrenci ve ders sayıları gibi özet verilerle görüntüler.
    /// </summary>
    public async Task<IActionResult> Index()
    {
        // Özet sayımlar ilgili servislerden toplanır.
        var branches = await branchService.GetAllAsync();
        var teachers = await teacherService.GetAllAsync();
        var students = await studentService.GetAllAsync();
        var courses = await courseService.GetAllAsync();

        var model = new DashboardViewModel
        {
            BranchCount = branches.Data?.Count ?? 0,
            TeacherCount = teachers.Data?.Count ?? 0,
            StudentCount = students.Data?.Count ?? 0,
            CourseCount = courses.Data?.Count ?? 0
        };

        return View(model);
    }

    /// <summary>
    /// Beklenmeyen hatalarda gösterilen hata sayfası.
    /// </summary>
    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
