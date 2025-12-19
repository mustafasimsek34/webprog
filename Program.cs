/*
 * =============================================================================
 * PROGRAM (UYGULAMA GİRİŞ NOKTASI)
 * =============================================================================
 *
 * AÇIKLAMA:
 * `Program.cs` uygulamanın giriş noktasıdır. Web uygulamasını oluşturur,
 * bağımlılıkları (services) kaydeder, middleware pipeline'ını yapılandırır ve
 * uygulamayı başlatır.
 *
 * KULLANIM:
 * - Hizmet kayıtları (DbContext, Identity, HttpClient vb.) burada yapılır.
 * - `app.Run()` ile HTTP sunucusu başlatılır.
 *
 * =============================================================================
 */

using FitnessCenterManagement.Data;
using FitnessCenterManagement.Models;
using FitnessCenterManagement.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Veritabanı bağlantı ayarını appsettings.json'dan alıyoruz
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Entity Framework Core ile SQL Server kullanacağımızı belirtiyoruz
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions =>
        sqlOptions.EnableRetryOnFailure()));

// Identity (Üyelik) sistemini yapılandırıyoruz
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    // Şifre kurallarını geliştirme aşamasında kolaylık olsun diye basitleştirdim
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequiredLength = 3;
    
    // Her e-posta adresiyle sadece bir kayıt olunabilir
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Çerez (Cookie) ayarları
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
});

// Yapay Zeka Servisini sisteme tanıtıyoruz (Dependency Injection)
builder.Services.AddHttpClient();
builder.Services.AddScoped<IAIPlanService, AIPlanService>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

// Uygulama başlarken veritabanını otomatik oluştur ve örnek verileri ekle (Seed)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate(); // Bekleyen migrationları uygula

        await DbSeeder.SeedAsync(services); // Admin ve örnek verileri ekle
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı oluşturulurken bir hata meydana geldi.");
    }
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
