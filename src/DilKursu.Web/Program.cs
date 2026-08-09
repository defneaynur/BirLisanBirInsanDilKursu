using DilKursu.Business;
using DilKursu.DataAccess;
using DilKursu.DataAccess.Context;
using DilKursu.DataAccess.Identity;
using DilKursu.DataAccess.Seed;
using DilKursu.Web.Infrastructure.Auditing;
using Microsoft.AspNetCore.Identity;
using Serilog;
using Serilog.Events;

// QuestPDF topluluk (Community) lisansı: ücretsiz kullanım için uygulama başlamadan ayarlanır.
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

// Kod tarafı (teknik) derin loglama: Serilog ile konsol + günlük döngülü (rolling) dosya.
// İstekler, hatalar ve denetim olayları buraya yapılandırılmış (structured) biçimde yazılır.
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/dilkursu-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 14,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

// Tüm .NET loglamasını Serilog üzerinden yönet.
builder.Host.UseSerilog();

// -----------------------------------------------------------------------------
// 1) Servis kayıtları (Dependency Injection konteynerinin yapılandırılması)
// -----------------------------------------------------------------------------

// MSSQL bağlantı dizesi appsettings.json'dan okunur.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("'DefaultConnection' bağlantı dizesi bulunamadı.");

// Veri erişim katmanı (EF Core DbContext) kaydı.
builder.Services.AddDataAccess(connectionString);

// İş katmanı servisleri (Unit of Work + tüm uygulama servisleri) kaydı.
builder.Services.AddBusinessServices();

// ASP.NET Core Identity yapılandırması: kullanıcı, rol ve parola politikaları.
builder.Services
    .AddIdentity<ApplicationUser, IdentityRole>(options =>
    {
        // Parola politikası (örnek proje için makul düzeyde).
        options.Password.RequiredLength = 6;
        options.Password.RequireNonAlphanumeric = false;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireDigit = true;

        // E-posta doğrulaması bu senaryoda zorunlu tutulmaz.
        options.SignIn.RequireConfirmedAccount = false;

        // Kullanıcı adı kurallarına e-posta karakterlerine izin ver.
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<AppDbContext>()
    .AddClaimsPrincipalFactory<AppUserClaimsPrincipalFactory>()
    .AddDefaultTokenProviders();

// Kimlik doğrulama çerezi ayarları: giriş yapılmamışsa Login sayfasına yönlendir.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// MVC controller ve view desteği + [Audit] işaretli action'lar için merkezî denetim filtresi.
builder.Services.AddControllersWithViews(options =>
{
    options.Filters.Add<AuditActionFilter>();
});

// AJAX isteklerinde CSRF (antiforgery) token'ının HTTP başlığından okunmasını sağlar.
// JSON gövdeli POST isteklerinde token, "RequestVerificationToken" başlığıyla gönderilir.
builder.Services.AddAntiforgery(options => options.HeaderName = "RequestVerificationToken");

var app = builder.Build();

// -----------------------------------------------------------------------------
// 2) HTTP istek işleme hattı (middleware pipeline)
// -----------------------------------------------------------------------------

if (!app.Environment.IsDevelopment())
{
    // Üretimde ayrıntılı hata yerine kullanıcı dostu hata sayfası gösterilir.
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

// HTTP isteklerini tek satırlık özet olarak loglar (yöntem, yol, durum kodu, süre).
app.UseSerilogRequestLogging();

// Hattın tamamındaki yakalanmamış (beklenmeyen) hataları ayrı ErrorLogs tablosuna (yığın izi dahil) yazar.
// Hata gösterim katmanından sonra, kimlik doğrulamadan önce konumlandırılır ki kayıt kullanıcıya bağlansın.
app.UseMiddleware<ErrorLoggingMiddleware>();

app.UseRouting();

// Sıralama önemlidir: önce kimlik doğrulama, sonra yetkilendirme.
app.UseAuthentication();
app.UseAuthorization();

// Varsayılan rota. Giriş yapılmamışsa Identity çerez ayarı Login'e yönlendirir.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// -----------------------------------------------------------------------------
// 3) Uygulama başlarken veritabanını migrate et ve başlangıç verilerini tohumla
// -----------------------------------------------------------------------------
using (var scope = app.Services.CreateScope())
{
    try
    {
        await DbSeeder.SeedAsync(scope.ServiceProvider);
    }
    catch (Exception ex)
    {
        // Tohumlama hatası kritiktir; loglanır ancak uygulamanın açılışını bilinçli olarak durdurmaz.
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "Veritabanı tohumlama sırasında bir hata oluştu.");
    }
}

app.Run();
