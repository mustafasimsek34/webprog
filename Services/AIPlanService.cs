/*
 * =============================================================================
 * AI PLAN SERVICE - YAPAY ZEKA PLAN SERVİSİ
 * =============================================================================
 * 
 * AÇIKLAMA:
 * Bu servis, Google Gemini AI API'sini kullanarak kişiselleştirilmiş fitness ve 
 * diyet planları oluşturur. Kullanıcının kilo, boy, yaş, vücut tipi ve hedef 
 * bilgilerine göre AI'dan detaylı bir plan alır.
 * 
 * ÇALIŞMA PRENSİBİ:
 * 1. Kullanıcı bilgileri alınır (kilo, boy, yaş, vücut tipi, hedef)
 * 2. BMI (Vücut Kitle İndeksi) hesaplanır
 * 3. Gemini AI'ya Türkçe bir prompt gönderilir
 * 4. AI'dan gelen yanıt kullanıcıya gösterilir
 * 
 * API ENDPOİNTLERİ:
 * Birden fazla Gemini model endpoint'i denenir (lite, flash, pro modeller)
 * Bir model başarısız olursa sıradaki denenir (rate limit koruması)
 * 
 * BAĞIMLILIKLAR:
 * - IConfiguration: appsettings.json'dan API anahtarı okumak için
 * - HttpClient: Gemini API'ye HTTP istekleri göndermek için
 * 
 * =============================================================================
 */

using System.Text;
using System.Text.Json;

namespace FitnessCenterManagement.Services
{
    // ============================================
    // INTERFACE (Arayüz Tanımı)
    // ============================================
    
    /// <summary>
    /// AI Plan Servisi için arayüz tanımı.
    /// Dependency Injection ile servisin mocklanabilmesi ve test edilebilmesi için kullanılır.
    /// </summary>
    public interface IAIPlanService
    {
        /// <summary>
        /// Yapay zeka kullanarak diyet ve egzersiz planı oluşturur.
        /// </summary>
        /// <param name="weight">Kullanıcının kilosu (kg)</param>
        /// <param name="height">Kullanıcının boyu (cm)</param>
        /// <param name="goal">Fitness hedefi (Weight Loss, Muscle Gain, vb.)</param>
        /// <param name="age">Kullanıcının yaşı (isteğe bağlı)</param>
        /// <param name="bodyType">Vücut tipi (isteğe bağlı)</param>
        /// <returns>Markdown formatında fitness ve diyet planı</returns>
        Task<string> GenerateDietAndExercisePlan(double weight, double height, string goal, 
            int? age = null, string? bodyType = null);
    }

    // ============================================
    // SERVICE IMPLEMENTATION (Servis Uygulaması)
    // ============================================
    
    /// <summary>
    /// AI Plan Servisi - Google Gemini API ile çalışır.
    /// Fitness ve diyet planları oluşturmak için yapay zeka kullanır.
    /// </summary>
    public class AIPlanService : IAIPlanService
    {
        // ============================================
        // PRIVATE FIELDS (Özel Alanlar)
        // ============================================
        
        private readonly IConfiguration _configuration;  // Uygulama ayarlarına erişim
        private readonly HttpClient _httpClient;         // HTTP istekleri için client
        private readonly string? _apiKey;                // Gemini API anahtarı

        // ============================================
        // CONSTRUCTOR (Yapıcı Metod)
        // ============================================
        
        /// <summary>
        /// Servis constructor - Dependency Injection ile bağımlılıklar alınır.
        /// </summary>
        /// <param name="configuration">Uygulama ayarları (appsettings.json)</param>
        /// <param name="httpClient">HTTP client instance</param>
        public AIPlanService(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            
            // appsettings.json'dan Gemini API anahtarını oku
            // Örnek: "Gemini": { "ApiKey": "AIzaSy..." }
            _apiKey = _configuration["Gemini:ApiKey"];
        }

        // ============================================
        // ANA METOD: Plan Oluşturma
        // ============================================
        
        /// <summary>
        /// Ana metod: Kullanıcı bilgilerine göre AI destekli fitness ve diyet planı oluşturur.
        /// </summary>
        public async Task<string> GenerateDietAndExercisePlan(double weight, double height, string goal,
            int? age = null, string? bodyType = null)
        {
            // -----------------------------------------
            // ADIM 1: BMI Hesaplama
            // -----------------------------------------
            // BMI Formülü: kilo / (boy_metre)^2
            // Örnek: 70kg / (1.75m)^2 = 22.86
            double bmi = weight / Math.Pow(height / 100, 2);
            
            // Hata ayıklama bilgileri için değişken
            string debugInfo = "";

            // -----------------------------------------
            // ADIM 2: Ek Bilgileri Hazırla
            // -----------------------------------------
            // AI'ya gönderilecek ek kullanıcı bilgilerini string olarak oluştur
            var additionalInfo = new System.Text.StringBuilder();
            if (age.HasValue) 
                additionalInfo.AppendLine($"- Yaş: {age} yıl");
            if (!string.IsNullOrEmpty(bodyType)) 
                additionalInfo.AppendLine($"- Vücut Tipi: {bodyType}");
            
            // -----------------------------------------
            // ADIM 3: Gemini API ile Plan Oluştur
            // -----------------------------------------
            // Eğer API anahtarı varsa gerçek AI'yı kullan
            if (!string.IsNullOrEmpty(_apiKey))
            {
                try
                {
                    // AI'ya gönderilecek prompt (istek metni)
                    // Türkçe olarak detaylı bir plan isteniyorilacak şekilde hazırlanır
                    var prompt = $@"Sen profesyonel bir fitness koçu ve diyetisyensin. Türkçe olarak kişiselleştirilmiş bir fitness ve diyet planı oluştur.

Kullanıcı Bilgileri:
- Kilo: {weight} kg
- Boy: {height} cm
- BMI: {bmi:F2}
- Hedef: {goal}
{additionalInfo}

Lütfen aşağıdaki 4 bölümü içeren bir plan hazırla:

## 1. VÜCUT ANALİZİ
- BMI değerlendirmesi ve kategorisi
- İdeal kilo aralığı önerisi

## 2. HAFTALIK EGZERSİZ PROGRAMI
Her gün için kısa program (Pazartesi-Pazar)
- Antrenman türü ve süresi
- Temel hareketler

## 3. BESLENME PLANI
- Günlük kalori hedefi
- Örnek kahvaltı, öğle, akşam yemeği
- Ara öğün önerileri

## 4. ÖNEMLİ NOTLAR
- Su tüketimi
- Uyku önerisi
- Dikkat edilmesi gerekenler

Planı markdown formatında, başlıklar ve listeler ile düzenli şekilde sun.";

                    // -----------------------------------------
                    // ADIM 4: API İsteği Hazırla
                    // -----------------------------------------
                    // Gemini API'nin beklediği JSON formatında istek gövdesi
                    var requestBody = new
                    {
                        contents = new[]
                        {
                            new
                            {
                                parts = new[]
                                {
                                    new { text = prompt }
                                }
                            }
                        }
                    };

                    // -----------------------------------------
                    // ADIM 5: Model Endpoint'lerini Dene
                    // -----------------------------------------
                    // Birden fazla model denenir, biri başarısız olursa diğeri denenir
                    // Bu sayede rate limit (kota aşımı) hatalarından kaçınılır
                    var endpointsToTry = new[]
                    {
                        // En yeni modeller (Gemini 3 Preview)
                        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-3-pro-preview:generateContent?key={_apiKey}",

                        // Lite modeller - Genellikle daha yüksek kota limiti vardır
                        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-lite-preview-02-05:generateContent?key={_apiKey}",
                        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-lite-001:generateContent?key={_apiKey}",
                        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-lite-latest:generateContent?key={_apiKey}",
                        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite:generateContent?key={_apiKey}",
                        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-lite-preview-09-2025:generateContent?key={_apiKey}",
                        
                        // Flash modeller - Hızlı ve genel amaçlı
                        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}",
                        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-001:generateContent?key={_apiKey}",
                        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}",
                        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-preview-09-2025:generateContent?key={_apiKey}",
                        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-flash-latest:generateContent?key={_apiKey}",
                        
                        // Experimental ve Pro modeller - En yetenekli ama sınırlı kota
                        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash-exp:generateContent?key={_apiKey}",
                        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-exp-1206:generateContent?key={_apiKey}",
                        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-pro:generateContent?key={_apiKey}",
                        $"https://generativelanguage.googleapis.com/v1beta/models/gemini-pro-latest:generateContent?key={_apiKey}"
                    };

                    // -----------------------------------------
                    // ADIM 6: Her Endpoint'i Sırayla Dene
                    // -----------------------------------------
                    foreach (var endpoint in endpointsToTry)
                    {
                        try 
                        {
                            // JSON içeriği oluştur (her istek için yeni StringContent gerekli)
                            var jsonContent = new StringContent(
                                JsonSerializer.Serialize(requestBody),
                                Encoding.UTF8,
                                "application/json");

                            // API'ye POST isteği gönder
                            var response = await _httpClient.PostAsync(endpoint, jsonContent);

                            // Başarılı yanıt kontrolü (HTTP 200)
                            if (response.IsSuccessStatusCode)
                            {
                                // Yanıt içeriğini oku
                                var responseString = await response.Content.ReadAsStringAsync();
                                
                                // JSON yanıtını parse et
                                using var doc = JsonDocument.Parse(responseString);
                                
                                // Gemini API yanıt yapısı:
                                // { "candidates": [{ "content": { "parts": [{ "text": "..." }] } }] }
                                if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                                {
                                    var firstCandidate = candidates[0];
                                    if (firstCandidate.TryGetProperty("content", out var contentObj) && 
                                        contentObj.TryGetProperty("parts", out var parts) && 
                                        parts.GetArrayLength() > 0)
                                    {
                                        // AI'dan gelen metni al
                                        var text = parts[0].GetProperty("text").GetString();
                                        if (!string.IsNullOrEmpty(text))
                                        {
                                            return text; // BAŞARILI! Planı döndür
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Hata durumunda detayları logla
                                var errorContent = await response.Content.ReadAsStringAsync();
                                debugInfo += $"\nModel ({endpoint}) Hatası: {errorContent}";
                                
                                // HTTP 429: Too Many Requests (Kota aşımı)
                                // Bu durumda biraz bekleyip sonraki modeli dene
                                if ((int)response.StatusCode == 429)
                                {
                                    await Task.Delay(2000); // 2 saniye bekle
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            // İstek sırasında hata oluştuysa logla ve devam et
                            debugInfo += $"\nİstek Hatası: {ex.Message}";
                            await Task.Delay(1000); // 1 saniye bekle
                        }
                    }

                    // -----------------------------------------
                    // ADIM 7: Model Listesini Kontrol Et (Debug)
                    // -----------------------------------------
                    // Eğer hiçbir model çalışmadıysa, erişilebilir modelleri listele
                    try 
                    {
                        var listModelsResponse = await _httpClient.GetAsync($"https://generativelanguage.googleapis.com/v1beta/models?key={_apiKey}");
                        var listModelsContent = await listModelsResponse.Content.ReadAsStringAsync();
                        debugInfo += $"\n\nErişilebilir Modeller Listesi (Debug): {listModelsContent}";
                    }
                    catch (Exception ex)
                    {
                        debugInfo += $"\nModel Listesi Çekilemedi: {ex.Message}";
                    }
                    
                    // Hata bilgilerini konsola yaz (geliştirici için)
                    Console.WriteLine(debugInfo);
                }
                catch (Exception ex)
                {
                    // Genel sistem hatası
                    debugInfo = $"Sistem Hatası: {ex.Message}";
                    Console.WriteLine($"Gemini API Error: {ex.Message}");
                }
            }
            else
            {
                // API anahtarı bulunamadı
                debugInfo = "API Anahtarı bulunamadı.";
            }

            // ============================================
            // FALLBACK PLAN (Yedek Plan)
            // ============================================
            // AI'ya erişilemediğinde gösterilecek standart plan
            // Bu sayede kullanıcı tamamen boş bir sayfa görmez
            
            await Task.Delay(500); // İşlem yapılıyormuş gibi kısa bekleme (UX için)

            // Standart plan şablonu (Markdown formatında)
            var plan = $@"
## Kişiselleştirilmiş Fitness & Diyet Planı (Otomatik Oluşturuldu)

**Profiliniz:**
- Kilo: {weight} kg
- Boy: {height} cm
- BMI: {bmi:F2}
- Hedef: {goal}

### BMI Kategorisi:
{GetBMICategory(bmi)}

### Önerilen Egzersiz Planı:

**Kardiyo (Haftada 3-4 gün):**
- Koşu/Yürüyüş: 30 dakika
- Bisiklet: 45 dakika
- Yüzme: 30 dakika

**Kuvvet Antrenmanı (Haftada 3 gün):**
- Şınav: 3 set x 12 tekrar
- Squat (Çömelme): 3 set x 15 tekrar
- Plank: 3 set x 30 saniye
- Dambıl egzersizleri: 3 set x 10 tekrar

**Esneklik (Günlük):**
- Yoga: 15-20 dakika
- Esneme Hareketleri: 10 dakika

### Önerilen Beslenme Planı:

**Kahvaltı:**
- Meyveli ve kuruyemişli yulaf ezmesi
- Ballı süzme yoğurt
- Yeşil çay

**Öğle Yemeği:**
- Izgara tavuk göğsü veya balık
- Esmer pirinç veya kinoa
- Karışık sebzeler

**Akşam Yemeği:**
- Yağsız protein (hindi, balık)
- Tatlı patates
- Büyük bir kase salata

**Ara Öğünler:**
- Taze meyveler
- Kuruyemiş (badem, ceviz)
- Protein shake

### Günlük Kalori Hedefi:
{GetCalorieTarget(weight, height, goal)} kalori

### Su Tüketimi:
- Günde 8-10 bardak su için
- Şekerli içeceklerden kaçının

### Önemli Notlar:
- Yeni bir fitness programına başlamadan önce bir sağlık uzmanına danışın
- Aşamalı olarak ilerleyin ve vücudunuzu dinleyin
- Yeterli uyku alın (geceleri 7-9 saat)
- İlerlemenizi haftalık olarak takip edin

---
*Bu plan yapay zeka tarafından oluşturulmuştur ve genel bir rehber olarak kullanılmalıdır. Kişiselleştirilmiş tavsiyeler için sertifikalı fitness eğitmenleri ve beslenme uzmanlarına danışın.*
";

            return plan;
        }

        // ============================================
        // YARDIMCI METODLAR (Helper Methods)
        // ============================================
        
        /// <summary>
        /// BMI değerine göre kategori belirler.
        /// WHO (Dünya Sağlık Örgütü) standartlarına göre sınıflandırma yapılır.
        /// </summary>
        /// <param name="bmi">Vücut Kitle İndeksi değeri</param>
        /// <returns>BMI kategorisi ve önerisi</returns>
        private string GetBMICategory(double bmi)
        {
            // BMI Kategorileri (WHO Standartları):
            // < 18.5: Zayıf (Underweight)
            // 18.5 - 24.9: Normal (Normal weight)
            // 25 - 29.9: Fazla kilolu (Overweight)
            // >= 30: Obez (Obese)
            
            if (bmi < 18.5) 
                return "Zayıf - Kas kazanımına ve kalori alımını artırmaya odaklanın";
            if (bmi < 25) 
                return "Normal kilo - Dengeli beslenmeyi ve düzenli egzersizi koruyun";
            if (bmi < 30) 
                return "Fazla kilolu - Kardiyo ve kalori açığına odaklanın";
            return "Obez - Bir sağlık uzmanına danışmanız önerilir. Aşamalı kilo vermeye odaklanın";
        }

        /// <summary>
        /// Hedefe göre günlük kalori ihtiyacını hesaplar.
        /// Mifflin-St Jeor denklemi kullanılarak bazal metabolizma hızı (BMR) hesaplanır.
        /// </summary>
        /// <param name="weight">Kilo (kg)</param>
        /// <param name="height">Boy (cm)</param>
        /// <param name="goal">Fitness hedefi</param>
        /// <returns>Günlük kalori hedefi</returns>
        private int GetCalorieTarget(double weight, double height, string goal)
        {
            // Mifflin-St Jeor Denklemi (Ortalama yetişkin için basitleştirilmiş):
            // BMR = 10 × kilo + 6.25 × boy - 5 × yaş + 5 (erkek) veya -161 (kadın)
            // Burada yaş 30 olarak varsayılıyor
            int baseBMR = (int)(10 * weight + 6.25 * height - 5 * 30 + 5);

            // Hedefe göre kalori ayarlaması:
            // Kilo verme: BMR - 500 (haftada ~0.5 kg kayıp)
            // Kas kazanma: BMR + 500 (anabolik durum için fazlalık)
            // Koruma: BMR (denge)
            return goal.ToLower() switch
            {
                "weight loss" => baseBMR - 500,   // Kalori açığı
                "muscle gain" => baseBMR + 500,   // Kalori fazlası
                "maintenance" => baseBMR,          // Denge
                _ => baseBMR                       // Varsayılan: Denge
            };
        }
    }
}
