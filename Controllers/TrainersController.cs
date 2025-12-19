/*
 * =============================================================================
 * TRAINERS CONTROLLER
 * =============================================================================
 *
 * AÇIKLAMA:
 * `TrainersController` admin kullanıcıları için eğitmenlerin CRUD işlemlerini
 * ve ilişkili hizmet/uygunluk yönetimini sağlar.
 *
 * KULLANIM:
 * - Eğitmen oluşturma, düzenleme, silme ve detay görüntüleme işlemleri burada.
 *
 * =============================================================================
 */

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;

namespace FitnessCenterManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class TrainersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Trainers
        public async Task<IActionResult> Index()
        {
            // Eğitmenleri listelerken verdikleri hizmetleri de çekiyoruz
            var trainers = await _context.Trainers
                .Include(t => t.TrainerServices)
                .ThenInclude(ts => ts.Service)
                .ToListAsync();
            return View(trainers);
        }

        // GET: Trainers/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Eğitmen detayında hizmetleri ve uygunluk saatlerini de gösteriyoruz
            var trainer = await _context.Trainers
                .Include(t => t.TrainerServices)
                .ThenInclude(ts => ts.Service)
                .Include(t => t.Availabilities)
                .FirstOrDefaultAsync(m => m.Id == id);
            
            if (trainer == null)
            {
                return NotFound();
            }

            return View(trainer);
        }

        // GET: Trainers/Create
        public IActionResult Create()
        {
            // Yeni eğitmen eklerken hizmet seçimi için listeyi View'a gönderiyoruz
            ViewBag.Services = new MultiSelectList(_context.Services, "Id", "Name");
            return View();
        }

        // POST: Trainers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Email,PhoneNumber,Biography,ImageUrl,IsAvailable")] Trainer trainer, int[] selectedServices)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trainer);
                await _context.SaveChangesAsync();

                // Seçilen hizmetleri eğitmenle ilişkilendiriyoruz (Many-to-Many)
                if (selectedServices != null && selectedServices.Length > 0)
                {
                    foreach (var serviceId in selectedServices)
                    {
                        _context.TrainerServices.Add(new TrainerService
                        {
                            TrainerId = trainer.Id,
                            ServiceId = serviceId
                        });
                    }
                    await _context.SaveChangesAsync();
                }

                TempData["SuccessMessage"] = "Eğitmen başarıyla oluşturuldu!";
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Services = new MultiSelectList(_context.Services, "Id", "Name", selectedServices);
            return View(trainer);
        }

        // GET: Trainers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .Include(t => t.TrainerServices)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (trainer == null)
            {
                return NotFound();
            }

            ViewBag.Services = new MultiSelectList(_context.Services, "Id", "Name", 
                trainer.TrainerServices.Select(ts => ts.ServiceId));
            
            return View(trainer);
        }

        // POST: Trainers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Email,PhoneNumber,Biography,ImageUrl,IsAvailable")] Trainer trainer, int[] selectedServices)
        {
            if (id != trainer.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trainer);
                    
                    // Remove existing services
                    var existingServices = _context.TrainerServices.Where(ts => ts.TrainerId == id);
                    _context.TrainerServices.RemoveRange(existingServices);
                    
                    // Add new services
                    if (selectedServices != null && selectedServices.Length > 0)
                    {
                        foreach (var serviceId in selectedServices)
                        {
                            _context.TrainerServices.Add(new TrainerService
                            {
                                TrainerId = trainer.Id,
                                ServiceId = serviceId
                            });
                        }
                    }
                    
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Trainer updated successfully!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainerExists(trainer.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Services = new MultiSelectList(_context.Services, "Id", "Name", selectedServices);
            return View(trainer);
        }

        // GET: Trainers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trainer == null)
            {
                return NotFound();
            }

            return View(trainer);
        }

        // POST: Trainers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainer = await _context.Trainers.FindAsync(id);
            if (trainer != null)
            {
                _context.Trainers.Remove(trainer);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Trainer deleted successfully!";
            }

            return RedirectToAction(nameof(Index));
        }

        private bool TrainerExists(int id)
        {
            return _context.Trainers.Any(e => e.Id == id);
        }

        // GET: Trainers/ManageAvailability/5
        public async Task<IActionResult> ManageAvailability(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainer = await _context.Trainers
                .Include(t => t.Availabilities)
                .FirstOrDefaultAsync(t => t.Id == id);
            
            if (trainer == null)
            {
                return NotFound();
            }

            return View(trainer);
        }

        // POST: Trainers/AddAvailability
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAvailability(int trainerId, DayOfWeek dayOfWeek, string startTime, string endTime)
        {
            var availability = new TrainerAvailability
            {
                TrainerId = trainerId,
                DayOfWeek = dayOfWeek,
                StartTime = TimeSpan.Parse(startTime),
                EndTime = TimeSpan.Parse(endTime),
                IsActive = true
            };

            _context.TrainerAvailabilities.Add(availability);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Availability added successfully!";
            return RedirectToAction(nameof(ManageAvailability), new { id = trainerId });
        }

        // POST: Trainers/DeleteAvailability/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAvailability(int id, int trainerId)
        {
            var availability = await _context.TrainerAvailabilities.FindAsync(id);
            if (availability != null)
            {
                _context.TrainerAvailabilities.Remove(availability);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Availability removed successfully!";
            }

            return RedirectToAction(nameof(ManageAvailability), new { id = trainerId });
        }
    }
}
