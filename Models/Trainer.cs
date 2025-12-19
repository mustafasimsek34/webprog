/*
 * =============================================================================
 * TRAINER MODEL
 * =============================================================================
 *
 * AÇIKLAMA:
 * `Trainer` uygulamadaki eğitmenleri temsil eden model sınıfıdır. İsim, iletişim
 * bilgileri, biyografi ve ilişkili servis/uygunluk bilgilerini içerir.
 *
 * KULLANIM:
 * - Admin arayüzünde eğitmen yönetimi, randevu ilişkileri ve API raporlamada kullanılır.
 *
 * =============================================================================
 */

using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class Trainer
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [StringLength(15)]
        public string? PhoneNumber { get; set; }

        [StringLength(1000)]
        public string? Biography { get; set; }

        [StringLength(200)]
        public string? ImageUrl { get; set; }

        public bool IsAvailable { get; set; } = true;

        // Navigation properties
        public ICollection<TrainerService> TrainerServices { get; set; } = new List<TrainerService>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<TrainerAvailability> Availabilities { get; set; } = new List<TrainerAvailability>();
    }
}
