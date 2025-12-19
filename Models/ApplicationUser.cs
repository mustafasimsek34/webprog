/*
 * =============================================================================
 * APPLICATION USER (IDENTITY)
 * =============================================================================
 *
 * AÇIKLAMA:
 * `ApplicationUser` IdentityUser'dan türetilmiş kullanıcı modelidir. Ek alanlar
 * (FullName, RegistrationDate, Appointments) içerir ve uygulama kullanıcılarını
 * temsil eder.
 *
 * KULLANIM:
 * - ASP.NET Identity ile kullanıcı yönetimi ve ilişkili verilerde kullanılır.
 *
 * =============================================================================
 */

using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [StringLength(15)]
        public new string? PhoneNumber { get; set; }

        public DateTime RegistrationDate { get; set; } = DateTime.Now;

        // Navigation property
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}
