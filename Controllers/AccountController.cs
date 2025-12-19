/*
 * =============================================================================
 * ACCOUNT CONTROLLER
 * =============================================================================
 *
 * AÇIKLAMA:
 * `AccountController` kullanıcı kimlik doğrulama, kayıt, giriş/çıkış ve erişim
 * reddi işlemlerini yönetir.
 *
 * KULLANIM:
 * - `Register`, `Login`, `Logout`, `AccessDenied` ve `Profile` action'larını içerir.
 *
 * =============================================================================
 */

using FitnessCenterManagement.Models;
using FitnessCenterManagement.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace FitnessCenterManagement.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Yeni kullanıcı nesnesini oluşturuyoruz
                var user = new ApplicationUser
                {
                    UserName = model.Email, // Kullanıcı adı olarak e-posta kullanıyoruz
                    Email = model.Email,
                    FullName = model.FullName,
                    PhoneNumber = model.PhoneNumber,
                    RegistrationDate = DateTime.Now
                };

                // Identity kütüphanesi ile kullanıcıyı veritabanına kaydediyoruz
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Varsayılan olarak her yeni kullanıcıya "Member" rolü atıyoruz
                    await _userManager.AddToRoleAsync(user, "Member");
                    
                    // Kayıt sonrası otomatik giriş yaptırıyoruz
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                // Hata varsa (örn: şifre yetersiz, email kullanımda) ekrana basıyoruz
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }

            return View(model);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (ModelState.IsValid)
            {
                // Kullanıcı adı (email) ve şifre kontrolü
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, 
                    model.Password, 
                    model.RememberMe, 
                    lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    // Giriş başarılıysa kullanıcının rolüne bakıyoruz
                    var user = await _userManager.FindByEmailAsync(model.Email);
                    
                    // Eğer Admin ise direkt Admin paneline yönlendir
                    if (user != null && await _userManager.IsInRoleAsync(user, "Admin"))
                    {
                        return RedirectToAction("Index", "Admin");
                    }
                    
                    // Değilse geldiği sayfaya veya ana sayfaya yönlendir
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Giriş başarısız. Lütfen bilgilerinizi kontrol edin.");
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            ViewData["FullName"] = user.FullName;
            ViewData["Email"] = user.Email;
            ViewData["PhoneNumber"] = user.PhoneNumber;
            ViewData["RegistrationDate"] = user.RegistrationDate.ToString("g");

            return View();
        }
    }
}
