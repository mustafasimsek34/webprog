/*
 * =============================================================================
 * AI FITNESS CONTROLLER
 * =============================================================================
 * 
 * AÇIKLAMA:
 * Bu controller, yapay zeka destekli fitness ve diyet planı oluşturma işlemlerini yönetir.
 * Kullanıcıdan alınan form verilerini AIPlanService'e gönderir ve sonucu görüntüler.
 * 
 * ROTALAR (ROUTES):
 * - GET  /AIFitness/GeneratePlan -> Boş formu gösterir
 * - POST /AIFitness/GeneratePlan -> Formu işler ve AI planı oluşturur
 * 
 * BAĞIMLILIKLAR (DEPENDENCIES):
 * - IAIPlanService: Yapay zeka ile plan oluşturma servisi
 * - AIFitnessPlanViewModel: Form verilerini taşıyan model
 * 
 * =============================================================================
 */

using Microsoft.AspNetCore.Mvc;
using FitnessCenterManagement.Services;
using FitnessCenterManagement.ViewModels;

namespace FitnessCenterManagement.Controllers
{
    /// <summary>
    /// AI Fitness ve Diyet Planı oluşturma işlemlerini yöneten controller.
    /// Kullanıcı bilgilerini alır, yapay zekaya gönderir ve sonucu gösterir.
    /// </summary>
    public class AIFitnessController : Controller
    {
        // ============================================
        // BAĞIMLILIKLAR (Dependencies)
        // ============================================
        
        /// <summary>
        /// Yapay zeka plan servisi - Dependency Injection ile enjekte edilir.
        /// Bu servis, Gemini AI API'si ile iletişim kurar.
        /// </summary>
        private readonly IAIPlanService _aiPlanService;

        // ============================================
        // CONSTRUCTOR (Yapıcı Metod)
        // ============================================
        
        /// <summary>
        /// Controller constructor - Dependency Injection ile servis alır.
        /// ASP.NET Core, Program.cs'de kayıtlı servisi otomatik olarak enjekte eder.
        /// </summary>
        /// <param name="aiPlanService">AI plan oluşturma servisi</param>
        public AIFitnessController(IAIPlanService aiPlanService)
        {
            _aiPlanService = aiPlanService;
        }

        // ============================================
        // GET METODLARI
        // ============================================
        
        /// <summary>
        /// GET: /AIFitness/GeneratePlan
        /// Boş formu gösterir. Kullanıcı bu sayfada bilgilerini girer.
        /// </summary>
        /// <returns>GeneratePlan.cshtml view'ını döndürür</returns>
        public IActionResult GeneratePlan()
        {
            return View(); // Views/AIFitness/GeneratePlan.cshtml dosyasını gösterir
        }

        // ============================================
        // POST METODLARI
        // ============================================
        
        /// <summary>
        /// POST: /AIFitness/GeneratePlan
        /// Form gönderildiğinde çalışır. AI servisini çağırıp plan oluşturur.
        /// </summary>
        /// <param name="model">Formdan gelen kullanıcı verileri (kilo, boy, yaş, hedef, vücut tipi)</param>
        /// <returns>Oluşturulan plan ile birlikte view'ı döndürür</returns>
        [HttpPost] // Bu metod sadece POST isteklerinde çalışır
        [ValidateAntiForgeryToken] // CSRF saldırılarına karşı koruma sağlar
        public async Task<IActionResult> GeneratePlan(AIFitnessPlanViewModel model)
        {
            // Model doğrulama kontrolü (Required, Range vb. kurallar)
            if (ModelState.IsValid)
            {
                // AI servisini çağır ve plan oluştur
                // await: Asenkron işlem tamamlanana kadar bekle
                var plan = await _aiPlanService.GenerateDietAndExercisePlan(
                    model.Weight,      // Kullanıcının kilosu
                    model.Height,      // Kullanıcının boyu
                    model.Goal,        // Fitness hedefi
                    model.Age,         // Yaş (isteğe bağlı)
                    model.BodyType);   // Vücut tipi (isteğe bağlı)

                // Oluşturulan planı ViewBag'e ekle (View'da @ViewBag.GeneratedPlan ile erişilir)
                ViewBag.GeneratedPlan = plan;
                
                // Aynı sayfayı model ile birlikte döndür (form verileri korunur)
                return View(model);
            }

            // Doğrulama hatası varsa, formu hata mesajlarıyla birlikte tekrar göster
            return View(model);
        }
    }
}
