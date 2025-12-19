/*
 * =============================================================================
 * APPOINTMENT CONTROLLER
 * =============================================================================
 *
 * AÇIKLAMA:
 * `AppointmentController` kayıtlı kullanıcıların randevu oluşturma, uygunluk
 * kontrolü ve kendi randevularını görüntüleme işlemlerini yönetir.
 *
 * KULLANIM:
 * - Book, CheckAvailability, GetTrainersByService, MyAppointments gibi action'lar içerir.
 *
 * =============================================================================
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;
using FitnessCenterManagement.ViewModels;

namespace FitnessCenterManagement.Controllers
{
    [Authorize]
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Appointment/Book
        public async Task<IActionResult> Book()
        {
            var services = await _context.Services.Where(s => s.IsActive).ToListAsync();
            ViewBag.Services = new SelectList(services, "Id", "Name");
            return View();
        }

        // GET: Appointment/GetTrainersByService
        [HttpGet]
        public async Task<IActionResult> GetTrainersByService(int serviceId)
        {
            // Seçilen hizmeti veren ve aktif olan eğitmenleri listeliyoruz.
            // Sadece gerekli alanları (id, isim, biyografi) seçerek JSON olarak dönüyoruz.
            var trainers = await _context.TrainerServices
                .Where(ts => ts.ServiceId == serviceId && ts.Trainer.IsAvailable)
                .Select(ts => new
                {
                    id = ts.TrainerId,
                    name = ts.Trainer.Name,
                    biography = ts.Trainer.Biography
                })
                .ToListAsync();

            return Json(trainers);
        }

        // GET: Appointment/CheckAvailability
        [HttpGet]
        public async Task<IActionResult> CheckAvailability(int trainerId, int serviceId, DateTime date, TimeSpan time)
        {
            // Seçilen tarihin hangi gün olduğunu buluyoruz (Pazartesi, Salı vs.)
            var dayOfWeek = date.DayOfWeek;

            // Eğitmenin o gün ve saatte çalışıp çalışmadığını kontrol ediyoruz.
            // TrainerAvailabilities tablosundan gün ve saat aralığına bakıyoruz.
            var isAvailableSlot = await _context.TrainerAvailabilities
                .AnyAsync(ta => ta.TrainerId == trainerId &&
                               ta.DayOfWeek == dayOfWeek &&
                               ta.StartTime <= time &&
                               ta.EndTime >= time &&
                               ta.IsActive);

            if (!isAvailableSlot)
            {
                return Json(new { available = false, message = "Eğitmen bu saat aralığında çalışmıyor." });
            }

            // Seçilen hizmetin süresini alıyoruz (örn: 60 dk)
            var service = await _context.Services.FindAsync(serviceId);
            if (service == null)
            {
                return Json(new { available = false, message = "Hizmet bulunamadı." });
            }

            // Randevunun bitiş saatini hesaplıyoruz
            var endTime = time.Add(TimeSpan.FromMinutes(service.Duration));

            // Çakışan randevu var mı diye kontrol ediyoruz.
            // Önce o eğitmene ait o günkü iptal edilmemiş randevuları çekiyoruz.
            // Not: Veritabanında TimeSpan işlemleri bazen sorun çıkarabildiği için önce veriyi çekip sonra bellekte kontrol ediyoruz.
            var existingAppointments = await _context.Appointments
                .Where(a => a.TrainerId == trainerId &&
                           a.AppointmentDate == date &&
                           a.Status != "Cancelled")
                .Include(a => a.Service)
                .ToListAsync();

            // Bellekteki randevularla yeni talep edilen saat aralığı çakışıyor mu?
            var hasConflict = existingAppointments.Any(a =>
            {
                var appointmentEndTime = a.AppointmentTime.Add(TimeSpan.FromMinutes(a.Service.Duration));
                // İki zaman aralığının kesişim kontrolü
                return (a.AppointmentTime < endTime && appointmentEndTime > time);
            });

            if (hasConflict)
            {
                return Json(new { available = false, message = "Bu saat aralığı zaten dolu." });
            }

            return Json(new { available = true, message = "Randevu saati uygun!" });
        }

        // POST: Appointment/Book
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Book(BookAppointmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Validate availability again
                var dayOfWeek = model.AppointmentDate.DayOfWeek;
                var isAvailableSlot = await _context.TrainerAvailabilities
                    .AnyAsync(ta => ta.TrainerId == model.TrainerId &&
                                   ta.DayOfWeek == dayOfWeek &&
                                   ta.StartTime <= model.AppointmentTime &&
                                   ta.EndTime >= model.AppointmentTime &&
                                   ta.IsActive);

                if (!isAvailableSlot)
                {
                    ModelState.AddModelError("", "Trainer is not available at this time slot.");
                    var services = await _context.Services.Where(s => s.IsActive).ToListAsync();
                    ViewBag.Services = new SelectList(services, "Id", "Name");
                    return View(model);
                }

                var service = await _context.Services.FindAsync(model.ServiceId);
                var endTime = model.AppointmentTime.Add(TimeSpan.FromMinutes(service!.Duration));

                // Check for conflicting appointments - get appointments first then check in memory
                var existingAppointments = await _context.Appointments
                    .Where(a => a.TrainerId == model.TrainerId &&
                               a.AppointmentDate == model.AppointmentDate &&
                               a.Status != "Cancelled")
                    .Include(a => a.Service)
                    .ToListAsync();

                var hasConflict = existingAppointments.Any(a =>
                {
                    var appointmentEndTime = a.AppointmentTime.Add(TimeSpan.FromMinutes(a.Service.Duration));
                    return (a.AppointmentTime < endTime && appointmentEndTime > model.AppointmentTime);
                });

                if (hasConflict)
                {
                    ModelState.AddModelError("", "This time slot is already booked. Please choose another time.");
                    var services = await _context.Services.Where(s => s.IsActive).ToListAsync();
                    ViewBag.Services = new SelectList(services, "Id", "Name");
                    return View(model);
                }

                var appointment = new Appointment
                {
                    MemberId = user.Id,
                    TrainerId = model.TrainerId,
                    ServiceId = model.ServiceId,
                    AppointmentDate = model.AppointmentDate,
                    AppointmentTime = model.AppointmentTime,
                    Notes = model.Notes,
                    Status = "Pending",
                    CreatedDate = DateTime.Now
                };

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Appointment booked successfully! Waiting for confirmation.";
                return RedirectToAction(nameof(MyAppointments));
            }

            var servicesForView = await _context.Services.Where(s => s.IsActive).ToListAsync();
            ViewBag.Services = new SelectList(servicesForView, "Id", "Name");
            return View(model);
        }

        // GET: Appointment/MyAppointments
        public async Task<IActionResult> MyAppointments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var appointments = await _context.Appointments
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .Where(a => a.MemberId == user.Id)
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .ToListAsync();

            return View(appointments);
        }

        // POST: Appointment/Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == id && a.MemberId == user!.Id);

            if (appointment == null)
            {
                return NotFound();
            }

            if (appointment.Status == "Cancelled")
            {
                TempData["ErrorMessage"] = "Appointment is already cancelled.";
                return RedirectToAction(nameof(MyAppointments));
            }

            appointment.Status = "Cancelled";
            appointment.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Appointment cancelled successfully.";
            return RedirectToAction(nameof(MyAppointments));
        }
    }
}
