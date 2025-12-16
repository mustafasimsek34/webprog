/*
 * =============================================================================
 * AI FITNESS PLAN VIEW MODEL
 * =============================================================================
 * 
 * AÇIKLAMA:
 * Bu dosya, AI Fitness Plan sayfasındaki form verilerini tutan ViewModel sınıfını içerir.
 * Kullanıcının girdiği kilo, boy, yaş, vücut tipi ve hedef bilgilerini depolar.
 * 
 * KULLANIM:
 * - GeneratePlan.cshtml view dosyasında form verileri için kullanılır
 * - AIFitnessController'da form post işlemlerinde model binding için kullanılır
 * - AIPlanService'e parametre olarak geçirilecek verileri taşır
 * 
 * DOĞRULAMA (VALIDATION):
 * - Required: Zorunlu alan kontrolü
 * - Range: Minimum ve maksimum değer kontrolü
 * - Display: Form etiketlerinde gösterilecek isimler
 * 
 * =============================================================================
 */

using System.ComponentModel.DataAnnotations;

namespace FitnessCenterManagement.ViewModels
{
    /// <summary>
    /// AI Fitness ve Diyet Planı oluşturmak için kullanıcıdan alınan verileri tutan model sınıfı.
    /// Bu model, form verilerini Controller'a taşır ve doğrulama kurallarını içerir.
    /// </summary>
    public class AIFitnessPlanViewModel
    {
        // ============================================
        // ZORUNLU ALANLAR (Required Fields)
        // ============================================
        
        /// <summary>
        /// Kullanıcının kilosu (kg cinsinden).
        /// BMI hesaplaması ve kalori hedefi için kullanılır.
        /// </summary>
        [Required(ErrorMessage = "Kilo gereklidir")]
        [Range(30, 300, ErrorMessage = "Kilo 30-300 kg arasında olmalıdır")]
        [Display(Name = "Kilo (kg)")]
        public double Weight { get; set; }

        /// <summary>
        /// Kullanıcının boyu (cm cinsinden).
        /// BMI hesaplaması ve ideal kilo aralığı için kullanılır.
        /// </summary>
        [Required(ErrorMessage = "Boy gereklidir")]
        [Range(100, 250, ErrorMessage = "Boy 100-250 cm arasında olmalıdır")]
        [Display(Name = "Boy (cm)")]
        public double Height { get; set; }

        /// <summary>
        /// Kullanıcının fitness hedefi.
        /// Değerler: "Weight Loss" (Kilo Verme), "Muscle Gain" (Kas Kazanma), 
        /// "Maintenance" (Kiloyu Koruma), "General Fitness" (Genel Fitness)
        /// </summary>
        [Required(ErrorMessage = "Hedef gereklidir")]
        [Display(Name = "Fitness Hedefi")]
        public string Goal { get; set; } = "Maintenance"; // Varsayılan: Kiloyu Koruma

        // ============================================
        // İSTEĞE BAĞLI ALANLAR (Optional Fields)
        // ============================================
        
        /// <summary>
        /// Kullanıcının yaşı (isteğe bağlı).
        /// Kalori hesaplaması ve egzersiz yoğunluğu için kullanılır.
        /// </summary>
        [Display(Name = "Yaş")]
        [Range(10, 100, ErrorMessage = "Yaş 10-100 arasında olmalıdır")]
        public int? Age { get; set; } // Nullable: Boş bırakılabilir

        /// <summary>
        /// Kullanıcının vücut tipi (isteğe bağlı).
        /// Değerler: "Ektomorf" (İnce yapılı), "Mezomorf" (Atletik yapılı), "Endomorf" (Geniş yapılı)
        /// AI'ın daha kişiselleştirilmiş öneriler vermesi için kullanılır.
        /// </summary>
        [Display(Name = "Vücut Tipi")]
        public string? BodyType { get; set; } // Nullable: Boş bırakılabilir
    }
}
