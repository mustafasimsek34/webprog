using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterManagement.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string MemberId { get; set; } = string.Empty;
        
        [ForeignKey("MemberId")]
        public ApplicationUser Member { get; set; } = null!;

        [Required]
        public int TrainerId { get; set; }
        
        [ForeignKey("TrainerId")]
        public Trainer Trainer { get; set; } = null!;

        [Required]
        public int ServiceId { get; set; }
        
        [ForeignKey("ServiceId")]
        public Service Service { get; set; } = null!;

        [Required]
        public DateTime AppointmentDate { get; set; }

        [Required]
        public TimeSpan AppointmentTime { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; // Pending, Confirmed, Cancelled, Completed

        [StringLength(500)]
        public string? Notes { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? UpdatedDate { get; set; }
    }
}
