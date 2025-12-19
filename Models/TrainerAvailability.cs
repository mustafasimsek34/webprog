/*
 * =============================================================================
 * TRAINER AVAILABILITY MODEL
 * =============================================================================
 *
 * AÇIKLAMA:
 * `TrainerAvailability` eğitmenlerin hangi gün ve saatlerde müsait olduğunu
 * temsil eden model sınıfıdır (gün, başlangıç/bitiş saatleri, aktiflik durumu).
 *
 * KULLANIM:
 * - Randevu rezervasyon kontrolü ve eğitmen takvimi görüntülemede kullanılır.
 *
 * =============================================================================
 */

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterManagement.Models
{
    public class TrainerAvailability
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TrainerId { get; set; }
        
        [ForeignKey("TrainerId")]
        public Trainer Trainer { get; set; } = null!;

        [Required]
        public DayOfWeek DayOfWeek { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
