using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.ViewModels
{
    public class BookAppointmentViewModel
    {
        [Required(ErrorMessage = "Please select a service")]
        [Display(Name = "Service")]
        public int ServiceId { get; set; }

        [Required(ErrorMessage = "Please select a trainer")]
        [Display(Name = "Trainer")]
        public int TrainerId { get; set; }

        [Required(ErrorMessage = "Please select an appointment date")]
        [Display(Name = "Appointment Date")]
        [DataType(DataType.Date)]
        public DateTime AppointmentDate { get; set; } = DateTime.Today.AddDays(1);

        [Required(ErrorMessage = "Please select an appointment time")]
        [Display(Name = "Appointment Time")]
        public TimeSpan AppointmentTime { get; set; }

        [StringLength(500)]
        [Display(Name = "Notes (Optional)")]
        [DataType(DataType.MultilineText)]
        public string? Notes { get; set; }
    }
}
