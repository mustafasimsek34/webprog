using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class Gym
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Location { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string WorkingHoursStart { get; set; } = "08:00";

        [Required]
        [StringLength(50)]
        public string WorkingHoursEnd { get; set; } = "22:00";

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(15)]
        public string? ContactPhone { get; set; }
    }
}
