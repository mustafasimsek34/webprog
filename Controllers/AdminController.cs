using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FitnessCenterManagement.Data;
using Microsoft.EntityFrameworkCore;

namespace FitnessCenterManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // Dashboard için özet istatistikleri topluyoruz
            ViewBag.TotalMembers = await _context.Users.CountAsync();
            ViewBag.TotalTrainers = await _context.Trainers.CountAsync();
            ViewBag.TotalServices = await _context.Services.CountAsync();
            ViewBag.TotalAppointments = await _context.Appointments.CountAsync();
            
            // Bekleyen randevu sayısını alıyoruz (Bildirimler için önemli)
            ViewBag.PendingAppointments = await _context.Appointments
                .CountAsync(a => a.Status == "Pending");

            // Son 10 randevuyu listelemek için veritabanından çekiyoruz
            // İlişkili tabloları (Member, Trainer, Service) dahil ediyoruz ki isimleri görebilelim
            var recentAppointments = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .OrderByDescending(a => a.CreatedDate)
                .Take(10)
                .ToListAsync();

            return View(recentAppointments);
        }
    }
}
