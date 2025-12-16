using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FitnessCenterManagement.Models
{
    // Many-to-Many relationship between Trainer and Service
    public class TrainerService
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int TrainerId { get; set; }
        
        [ForeignKey("TrainerId")]
        public Trainer Trainer { get; set; } = null!;

        [Required]
        public int ServiceId { get; set; }
        
        [ForeignKey("ServiceId")]
        public Service Service { get; set; } = null!;

        public DateTime AssignedDate { get; set; } = DateTime.Now;
    }
}
