/*
 * =============================================================================
 * SERVICE MODEL
 * =============================================================================
 *
 * AÇIKLAMA:
 * `Service` spor salonunda sunulan hizmetleri tanımlayan model sınıfıdır
 * (isim, açıklama, süre, fiyat ve aktiflik durumu).
 *
 * KULLANIM:
 * - Hizmet yönetimi, randevu rezervasyonları ve fiyatlandırma için kullanılır.
 *
 * =============================================================================
 */

using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class Service
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        [Range(15, 180)]
        public int Duration { get; set; } // Duration in minutes

        [Required]
        [Range(0.01, 10000.00)]
        public decimal Price { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation properties
        public ICollection<TrainerService> TrainerServices { get; set; } = new List<TrainerService>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
