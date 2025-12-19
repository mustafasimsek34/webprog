/*
 * =============================================================================
 * HOME CONTROLLER
 * =============================================================================
 *
 * AÇIKLAMA:
 * `HomeController` uygulamanın genel (public) sayfalarını yönetir: ana sayfa,
 * gizlilik politikası ve hata sayfası gibi işlemler burada tanımlıdır.
 *
 * KULLANIM:
 * - `Index`, `Privacy`, `Error` action'ları içerir.
 *
 * =============================================================================
 */

using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using FitnessCenterManagement.Models;

namespace FitnessCenterManagement.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    // Ana sayfa
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
