using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessCenterManagement.Data;

namespace FitnessCenterManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Appointments (Admin view)
        public async Task<IActionResult> Index()
        {
            // Admin panelinde tüm randevuları listeliyoruz
            // Üye, Eğitmen ve Hizmet bilgilerini de dahil ediyoruz
            var appointments = await _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
            
            return View(appointments);
        }

        // POST: Appointments/UpdateStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            // Randevu durumunu güncelliyoruz (Onaylandı, İptal Edildi vb.)
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null)
            {
                appointment.Status = status;
                appointment.UpdatedDate = DateTime.Now;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Randevu durumu güncellendi: {status}";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
