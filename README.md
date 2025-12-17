# webprog # Spor Salonu (Fitness Center) Yönetim ve Randevu Sistemi

**Kısa Açıklama**
Bu proje, ASP.NET Core MVC ile geliştirilmiş bir Spor Salonu Yönetim ve Randevu sistemidir. Antrenör yönetimi, hizmetler, randevu sistemi ve Google Gemini tabanlı AI fitness/diyet planı oluşturucu içerir.

## Öne Çıkan Özellikler
- Antrenör (Trainer) CRUD ve müsaitlik yönetimi
- Hizmet (Service) CRUD (süre, fiyat) yönetimi
- Üye randevu sistemi (çakışma kontrolü, onay mekanizması)
- Rol tabanlı yetkilendirme (Admin / Member)
- REST API endpoint'leri ve LINQ ile filtreleme
- Yapay zeka entegrasyonu: AI Fitness & Diyet Planı (Google Gemini API)

## Teknolojiler
- .NET 8 (ASP.NET Core MVC)
- C#
- Entity Framework Core
- SQL Server (veya SQLite)
- Bootstrap 5, jQuery
- Google Gemini AI
  

## Dosya Yapısı (Önemli Klasörler)
- `Controllers/` - MVC controller'lar
- `Models/` - Entity modelleri (Trainer, Service, Appointment, Gym, vb.)
- `Views/` - Razor görünüm dosyaları
- `Data/` - `ApplicationDbContext`, `DbSeeder`
- `Services/` - AI servis entegrasyonu
- `PROJE_RAPORU.md` - Proje raporu şablonu (doldurulmaya hazır)

Varsayılan admin e-posta (seed tarafından ekleniyse): `b241210383@sakarya.edu.tr` (parola: `sau`).

## Ortam Değişkenleri / Konfigürasyon
- `appsettings.json` içinde `ConnectionStrings` bölümünü veritabanınıza göre ayarlayın.
- AI entegrasyonu için `Gemini:ApiKey` (veya proje ayar adı) `appsettings.json` içine eklenmiş olabilir. Gizli anahtarları doğrudan repoya koymayın; GitHub için `secrets` kullanın.

## AI Fitness Planı (Kısa Açıklama)
- Kullanıcı, kilo, boy, yaş, vücut tipi ve hedef vererek AI'dan kişiselleştirilmiş plan alır.
- AI çağrıları bir servis sınıfı (`AIPlanService`) aracılığıyla yapılır; birden fazla endpoint/fallback mantığı vardır.

## API Örnekleri
- `GET /api/trainers` - Tüm antrenörleri listeler
- `GET /api/trainers/available?date=2025-12-20` - Belirli tarihte müsait antrenörleri getirir
- `POST /api/appointments` - Yeni randevu oluşturur



---

Hazırlayan: mustafa şimşek  
Öğrenci No: B241210383
