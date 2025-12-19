/*
 * =============================================================================
 * LOGIN VIEWMODEL
 * =============================================================================
 *
 * AÇIKLAMA:
 * `LoginViewModel` kullanıcı giriş (login) formu için gerekli alanları ve
 * doğrulama kurallarını tutar.
 *
 * KULLANIM:
 * - `AccountController.Login` action'ında model binding ve doğrulama için kullanılır.
 *
 * =============================================================================
 */

using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }
}
