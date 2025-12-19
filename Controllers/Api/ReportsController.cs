/*
 * =============================================================================
 * REPORTS API CONTROLLER
 * =============================================================================
 *
 * AÇIKLAMA:
 * `ReportsController` REST API endpoint'leri sağlar; örn. müsait eğitmenler,
 * randevu istatistikleri ve hizmete göre randevular gibi veri döndüren metodlar.
 *
 * KULLANIM:
 * - `api/Reports/*` rotası üzerinden JSON veri döndürür ve genelde dashboard/JS
 *   tarafında veya harici istemcilerce tüketilir.
 *
 * =============================================================================
 */

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;

namespace FitnessCenterManagement.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ReportsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Reports/AvailableTrainers
        [HttpGet("AvailableTrainers")]
        public async Task<ActionResult<IEnumerable<object>>> GetAvailableTrainers(
            [FromQuery] DateTime? date = null,
            [FromQuery] int? serviceId = null)
        {
            // Eğer tarih verilmediyse bugünü baz alıyoruz
            var targetDate = date ?? DateTime.Today;
            var dayOfWeek = targetDate.DayOfWeek;

            // Eğitmenleri, verdikleri hizmetleri ve uygunluk durumlarını joinleyerek çekiyoruz
            var query = _context.Trainers
                .Include(t => t.TrainerServices)
                .ThenInclude(ts => ts.Service)
                .Include(t => t.Availabilities)
                .Where(t => t.IsAvailable);

            // Eğer servis ID'si ile filtreleme istenmişse
            if (serviceId.HasValue)
            {
                query = query.Where(t => t.TrainerServices.Any(ts => ts.ServiceId == serviceId.Value));
            }

            // Seçilen günde uygunluğu olan eğitmenleri filtreliyoruz
            query = query.Where(t => t.Availabilities.Any(a =>
                a.DayOfWeek == dayOfWeek && a.IsActive));

            // Sonuçları anonim bir obje olarak şekillendirip dönüyoruz
            var trainers = await query
                .Select(t => new
                {
                    id = t.Id,
                    name = t.Name,
                    email = t.Email,
                    biography = t.Biography,
                    services = t.TrainerServices.Select(ts => new
                    {
                        id = ts.Service.Id,
                        name = ts.Service.Name,
                        price = ts.Service.Price,
                        duration = ts.Service.Duration
                    }).ToList(),
                    availabilities = t.Availabilities
                        .Where(a => a.DayOfWeek == dayOfWeek && a.IsActive)
                        .Select(a => new
                        {
                            dayOfWeek = a.DayOfWeek.ToString(),
                            startTime = a.StartTime.ToString(@"hh\:mm"),
                            endTime = a.EndTime.ToString(@"hh\:mm")
                        }).ToList()
                })
                .ToListAsync();

            return Ok(trainers);
        }

        // GET: api/Reports/AllAppointments
        [HttpGet("AllAppointments")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllAppointments(
            [FromQuery] DateTime? startDate = null,
            [FromQuery] DateTime? endDate = null,
            [FromQuery] string? status = null)
        {
            // Tüm randevuları ilişkili tablolarla (Üye, Eğitmen, Hizmet) birlikte çekiyoruz
            var query = _context.Appointments
                .Include(a => a.Member)
                .Include(a => a.Trainer)
                .Include(a => a.Service)
                .AsQueryable();

            // Tarih aralığı filtresi
            if (startDate.HasValue)
            {
                query = query.Where(a => a.AppointmentDate >= startDate.Value);
            }
            if (endDate.HasValue)
            {
                query = query.Where(a => a.AppointmentDate <= endDate.Value);
            }

            // Durum filtresi (Onaylı, Beklemede vs.)
            if (!string.IsNullOrEmpty(status))
            {
                query = query.Where(a => a.Status == status);
            }

            var appointments = await query
                .OrderByDescending(a => a.AppointmentDate)
                .ThenByDescending(a => a.AppointmentTime)
                .Select(a => new
                {
                    id = a.Id,
                    member = new
                    {
                        id = a.Member.Id,
                        name = a.Member.FullName,
                        email = a.Member.Email
                    },
                    trainer = new
                    {
                        id = a.Trainer.Id,
                        name = a.Trainer.Name,
                        email = a.Trainer.Email
                    },
                    service = new
                    {
                        id = a.Service.Id,
                        name = a.Service.Name,
                        price = a.Service.Price,
                        duration = a.Service.Duration
                    },
                    appointmentDate = a.AppointmentDate.ToString("yyyy-MM-dd"),
                    appointmentTime = a.AppointmentTime.ToString(@"hh\:mm"),
                    status = a.Status,
                    notes = a.Notes,
                    createdDate = a.CreatedDate.ToString("yyyy-MM-dd HH:mm:ss")
                })
                .ToListAsync();

            return Ok(new
            {
                totalCount = appointments.Count,
                data = appointments
            });
        }

        // GET: api/Reports/AppointmentStatistics
        [HttpGet("AppointmentStatistics")]
        public async Task<ActionResult<object>> GetAppointmentStatistics()
        {
            var total = await _context.Appointments.CountAsync();
            var pending = await _context.Appointments.CountAsync(a => a.Status == "Pending");
            var confirmed = await _context.Appointments.CountAsync(a => a.Status == "Confirmed");
            var cancelled = await _context.Appointments.CountAsync(a => a.Status == "Cancelled");
            var completed = await _context.Appointments.CountAsync(a => a.Status == "Completed");

            var topTrainers = await _context.Appointments
                .GroupBy(a => new { a.TrainerId, a.Trainer.Name })
                .Select(g => new
                {
                    trainerId = g.Key.TrainerId,
                    trainerName = g.Key.Name,
                    appointmentCount = g.Count()
                })
                .OrderByDescending(x => x.appointmentCount)
                .Take(5)
                .ToListAsync();

            var topServices = await _context.Appointments
                .GroupBy(a => new { a.ServiceId, a.Service.Name })
                .Select(g => new
                {
                    serviceId = g.Key.ServiceId,
                    serviceName = g.Key.Name,
                    appointmentCount = g.Count()
                })
                .OrderByDescending(x => x.appointmentCount)
                .Take(5)
                .ToListAsync();

            return Ok(new
            {
                totalAppointments = total,
                statusBreakdown = new
                {
                    pending,
                    confirmed,
                    cancelled,
                    completed
                },
                topTrainers,
                topServices
            });
        }

        // GET: api/Reports/ServiceAppointments
        [HttpGet("ServiceAppointments")]
        public async Task<ActionResult<IEnumerable<object>>> GetServiceAppointments()
        {
            var serviceAppointments = await _context.Services
                .Include(s => s.Appointments)
                .ThenInclude(a => a.Trainer)
                .Where(s => s.IsActive)
                .Select(s => new
                {
                    serviceId = s.Id,
                    serviceName = s.Name,
                    price = s.Price,
                    duration = s.Duration,
                    totalAppointments = s.Appointments.Count(),
                    pendingAppointments = s.Appointments.Count(a => a.Status == "Pending"),
                    confirmedAppointments = s.Appointments.Count(a => a.Status == "Confirmed"),
                    completedAppointments = s.Appointments.Count(a => a.Status == "Completed"),
                    totalRevenue = s.Appointments
                        .Where(a => a.Status == "Completed")
                        .Sum(a => s.Price),
                    appointments = s.Appointments
                        .OrderByDescending(a => a.AppointmentDate)
                        .Take(10)
                        .Select(a => new
                        {
                            id = a.Id,
                            trainerName = a.Trainer.Name,
                            appointmentDate = a.AppointmentDate.ToString("yyyy-MM-dd"),
                            appointmentTime = a.AppointmentTime.ToString(@"hh\:mm"),
                            status = a.Status
                        })
                        .ToList()
                })
                .OrderByDescending(s => s.totalAppointments)
                .ToListAsync();

            return Ok(new
            {
                totalServices = serviceAppointments.Count,
                services = serviceAppointments
            });
        }

        // GET: api/Reports/TrainerSchedule
        [HttpGet("TrainerSchedule")]
        public async Task<ActionResult<IEnumerable<object>>> GetTrainerSchedule(
            [FromQuery] int? trainerId = null,
            [FromQuery] DateTime? date = null)
        {
            var targetDate = date ?? DateTime.Today;
            var startOfWeek = targetDate.AddDays(-(int)targetDate.DayOfWeek);
            var endOfWeek = startOfWeek.AddDays(7);

            var query = _context.Trainers
                .Include(t => t.Appointments.Where(a => 
                    a.AppointmentDate >= startOfWeek && 
                    a.AppointmentDate < endOfWeek))
                .ThenInclude(a => a.Service)
                .Include(t => t.Appointments.Where(a => 
                    a.AppointmentDate >= startOfWeek && 
                    a.AppointmentDate < endOfWeek))
                .ThenInclude(a => a.Member)
                .Include(t => t.Availabilities)
                .Where(t => t.IsAvailable);

            if (trainerId.HasValue)
            {
                query = query.Where(t => t.Id == trainerId.Value);
            }

            var trainers = await query
                .Select(t => new
                {
                    trainerId = t.Id,
                    trainerName = t.Name,
                    email = t.Email,
                    weeklyAppointments = t.Appointments
                        .OrderBy(a => a.AppointmentDate)
                        .ThenBy(a => a.AppointmentTime)
                        .Select(a => new
                        {
                            id = a.Id,
                            memberName = a.Member.FullName,
                            serviceName = a.Service.Name,
                            date = a.AppointmentDate.ToString("yyyy-MM-dd"),
                            dayOfWeek = a.AppointmentDate.DayOfWeek.ToString(),
                            time = a.AppointmentTime.ToString(@"hh\:mm"),
                            duration = a.Service.Duration,
                            status = a.Status
                        })
                        .ToList(),
                    weeklyAvailability = t.Availabilities
                        .Where(a => a.IsActive)
                        .OrderBy(a => a.DayOfWeek)
                        .Select(a => new
                        {
                            dayOfWeek = a.DayOfWeek.ToString(),
                            startTime = a.StartTime.ToString(@"hh\:mm"),
                            endTime = a.EndTime.ToString(@"hh\:mm")
                        })
                        .ToList(),
                    totalWeeklyAppointments = t.Appointments.Count(),
                    weekRange = $"{startOfWeek:yyyy-MM-dd} - {endOfWeek:yyyy-MM-dd}"
                })
                .ToListAsync();

            return Ok(new
            {
                weekRange = $"{startOfWeek:yyyy-MM-dd} - {endOfWeek:yyyy-MM-dd}",
                totalTrainers = trainers.Count,
                trainers
            });
        }
    }
}
