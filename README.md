# Bir Lisan Bir İnsan — Dil Kursu Otomasyon Yazılımı

"Bir Lisan Bir İnsan" dil kursu şirketi için geliştirilmiş; öğrenci, öğretmen, ders,
şube, kullanıcı ve ödeme (taksit) işlemlerini **merkezi tek bir sunucu** üzerinden yöneten
**ASP.NET Core MVC** tabanlı otomasyon uygulamasıdır.

Proje; **Katmanlı Mimari (Layered Architecture)**, **SOLID** prensipleri, **OOP** ve
**Code First** yaklaşımı esas alınarak, bir yazılım mimarisi disiplini içinde geliştirilmiştir.

---

## 🎯 Karşılanan Gereksinimler

| # | Gereksinim | Karşılandığı Yer |
|---|-----------|------------------|
| 1 | Öğrenci, öğretmen ve derslerin kaydı | `Student`, `Teacher`, `Course` entity + servisleri |
| 2 | Birden fazla şubenin merkezi sunucuda toplanması, her şubeden her şubeyle işlem | Tek MSSQL veritabanı; işlemlerde şube kısıtı yok (kayıt elemanı herhangi bir şubeye kaydeder) |
| 3 | Yöneticinin yeni şube ve derslik girmesi | `BranchController`, `ClassroomController` (Admin) |
| 4 | Ders açarken **uygun öğretmen + boş sınıf** önerisi | `CourseService.GetSuggestionsAsync` (Ders Açma Sihirbazı) |
| 5 | Kayıt elemanının herhangi bir şubedeki derse öğrenci kaydı | `EnrollmentController.Enroll` (Kayıt rolü) |
| 6 | Şube tanıtım bilgileri (adres, ulaşım, sosyal olanaklar) + kayıt elemanının bunları görebilmesi | `Branch` entity + kayıt elemanına salt-okunur şube **Detay** ekranı |
| 7 | Öğretmen bilgileri (dil, telefon, işe başlama, gün/saat, şubeler) | `Teacher` + `TeacherLanguage`/`TeacherBranch`/`TeacherAvailability` |
| 8 | Öğrenci bilgileri (kurs, kur, telefon, ödeme) | `Student` + `Enrollment` |
| 9 | Peşin/taksitli ödeme, ödenmemiş taksit gösterimi ve tahsilat | `EnrollmentService` + `Installment` entity |
| + | Kayıt elemanlarının şubelere atanması (personel yönetimi) | `UserService` + Kullanıcılar ekranı (Admin) |

---

## 🏛️ Mimari

Çözüm, sorumlulukların net biçimde ayrıldığı **4 katman + 1 test** projesinden oluşur.
Bağımlılıklar tek yönlüdür (dıştan içe): `Web → Business → DataAccess → Entities`.

```
DilKursu.sln
│
├── src/
│   ├── DilKursu.Entities      # Domain modelleri + enum'lar (hiçbir bağımlılığı yok — çekirdek)
│   │   ├── Common/BaseEntity  #   Ortak alanlar (Id, tarih, soft-delete)
│   │   ├── Enums/             #   KurSeviyesi, OdemeTuru, AuditLevel
│   │   └── *.cs               #   Branch, Classroom, Language, Teacher, Course, Student, Enrollment, Installment, AuditLog, ErrorLog ...
│   │
│   ├── DilKursu.DataAccess    # EF Core veri erişimi (Code First)
│   │   ├── Context/           #   AppDbContext (IdentityDbContext)
│   │   ├── Configurations/    #   Her entity için ayrı Fluent API yapılandırması (SRP)
│   │   ├── Repositories/      #   IGenericRepository + GenericRepository
│   │   ├── UnitOfWork/        #   IUnitOfWork + UnitOfWork (atomik kayıt)
│   │   ├── Identity/          #   ApplicationUser, AppRoles, AppUserClaimsPrincipalFactory (şube claim'i)
│   │   ├── Migrations/        #   Code First migration'ları
│   │   └── Seed/DbSeeder.cs   #   Rol, kullanıcı, örnek veri + dayanıklı DB hazırlığı
│   │
│   ├── DilKursu.Business      # İş kuralları (uygulamanın kalbi)
│   │   ├── Common/            #   ServiceResult, ScheduleHelper
│   │   ├── Dtos/              #   Katmanlar arası veri transfer nesneleri
│   │   └── Services/          #   Abstract (arayüzler) + Concrete (uygulamalar)
│   │
│   └── DilKursu.Web           # Sunum katmanı (ASP.NET Core MVC)
│       ├── Controllers/       #   İnce controller'lar — iş mantığı servistedir
│       ├── Infrastructure/     #   Auditing: [Audit] özniteliği, denetim filtresi, hata middleware'i
│       ├── Documents/         #   ReceiptDocument (QuestPDF ödeme makbuzu)
│       ├── Views/             #   Razor + Bootstrap + jQuery + AJAX + DataTables + SweetAlert2 + Chart.js
│       ├── Models/            #   Görünüm modelleri (Login, Dashboard)
│       └── wwwroot/           #   site.css/js, logo & favicon (SVG), kütüphaneler
│
└── tests/
    └── DilKursu.Tests         # xUnit birim testleri (InMemory EF Core)
```

### SOLID Prensiplerinin Uygulanışı

- **S — Single Responsibility:** Her servis tek bir alanla ilgilenir; her entity kendi `IEntityTypeConfiguration` dosyasına sahiptir; `ScheduleHelper` yalnızca zaman hesaplarını yapar.
- **O — Open/Closed:** Yeni bir servis/entity, mevcut kod değiştirilmeden eklenebilir; `IGenericRepository` yeni varlıklara açıktır.
- **L — Liskov Substitution:** Tüm somut servisler, arayüzlerinin yerine sorunsuzca geçebilir (testlerde InMemory ile kanıtlanmıştır).
- **I — Interface Segregation:** `IBranchService`, `ICourseService` gibi küçük, amaca özel arayüzler kullanılır; tek bir "God interface" yoktur.
- **D — Dependency Inversion:** Üst katmanlar somut sınıflara değil arayüzlere (`IUnitOfWork`, `I...Service`) bağımlıdır; bağımlılıklar DI ile enjekte edilir.

### Kullanılan Desenler ve Modern C#
- **Repository + Unit of Work:** Veri erişimi soyutlanır, değişiklikler atomik kaydedilir.
- **DTO:** Entity'ler doğrudan dışarı açılmaz; katmanlar arası veri DTO ile taşınır.
- **Result Pattern (`ServiceResult`):** İstisna yerine öngörülebilir sonuç; AJAX/SweetAlert2 ile uyumlu.
- **Soft Delete:** Kayıtlar fiziksel silinmez, `IsActive=false` yapılır (veri bütünlüğü ve geçmiş korunur).
- **Primary Constructor (C# 12):** Servis, controller, repository ve UoW sınıflarında sade bağımlılık enjeksiyonu.

---

## 🛠️ Teknoloji Yığını

**Backend**
- C# / .NET 9 · ASP.NET Core MVC
- Entity Framework Core 9 (Code First) · MSSQL
- ASP.NET Core Identity (kullanıcı girişi + rol bazlı yetkilendirme)
- Serilog (yapılandırılmış kod tarafı loglama — konsol + döngülü dosya)

**Frontend**
- HTML, CSS, Bootstrap 5
- jQuery + AJAX (tüm veri gönderme/alma işlemleri)
- SweetAlert2 (bildirim ve onay kutuları)
- jQuery DataTables (listeleme/tablolama, Türkçe dil desteği)
- Chart.js (doluluk ve dağılım grafikleri)

**Ek Özellikler**
- **PDF Ödeme Makbuzu** — ödenen taksitler için QuestPDF ile markalı, indirilebilir PDF makbuz
- **Raporlar & Doluluk** — ders doluluğu (kayıt/kontenjan), şube ve dil bazlı ders dağılımı grafikleri
- **Kullanıcı (Personel) Yönetimi** — yönetici, kayıt elemanı oluşturur ve şubeye atar
- **Şube bağlamı** — kullanıcının hangi şubeden giriş yaptığı üst menüde rozetle gösterilir
- **Derin Loglama & Denetim** — kullanıcı işlemleri `[Modül][Aksiyon]` biçiminde denetim (audit) tablosunda, beklenmeyen hatalar ise yığın izleriyle ayrı bir hata tablosunda; her ikisi de Serilog ile dosyaya da yazılır (ayrıntı için [Loglama ve Denetim](#-loglama-ve-denetim))

**Test**
- xUnit · EF Core InMemory · Moq

---

## 🔐 Roller ve Yetkiler

| Rol | Yetkiler |
|-----|----------|
| **Admin** (Sistem Yöneticisi — merkezi) | Şube, derslik, dil, öğretmen tanımlama; ders açma sihirbazı; **kullanıcı yönetimi**; raporlar; öğrenci ve kayıt işlemleri |
| **Kayit** (Kayıt Elemanı — bir şubeye bağlı) | Şubeleri **görüntüleme** (tanıtım için); öğrenci tanımlama; **herhangi bir şubedeki** derse kayıt; taksit görüntüleme ve tahsilat |

### Varsayılan Demo Hesaplar (ilk açılışta otomatik oluşturulur)

| Rol | E-posta | Parola | Şube |
|-----|---------|--------|------|
| Admin | `admin@birlisanbirinsan.com` | `Admin123!` | Merkez (Tüm Şubeler) |
| Kayıt Elemanı | `kayit@birlisanbirinsan.com` | `Kayit123!` | Kadıköy Şubesi |

---

## 🏢 Merkezi Sunucu & Şube Mantığı

- Tüm şubeler **tek bir veritabanı** altında toplanır; herhangi bir şubeden her şubeyle ilgili işlem yapılabilir (işlemlerde şube kısıtı **yoktur**).
- Her kullanıcı bir şubeye bağlanabilir; **yönetici merkezidir** (şubesiz), **kayıt elemanı** bir şubeden çalışır.
- Giriş yapılan şube, `AppUserClaimsPrincipalFactory` ile bir claim olarak eklenir ve üst menüde **📍 rozet** olarak gösterilir.
- Bağlı olunan şube yalnızca "nereden giriş yapıldığı" bilgisidir; kayıt elemanı yine **tüm şubelerin** derslerine öğrenci kaydedebilir.

### Kullanıcı & Şube Yönetimi Akışı
1. **Admin** → *Tanımlamalar → Şubeler*'den şubeleri açar, derslikleri girer.
2. **Admin** → *Yönetim → Kullanıcılar*'dan kayıt elemanı oluşturur ve bir **şubeye atar**.
3. **Kayıt elemanı** o şubeden giriş yapar (menüde şubesi görünür), öğrencileri herhangi bir şubedeki derse kaydeder.

---

## 🚀 Kurulum ve Çalıştırma

### Gereksinimler
- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- MSSQL (LocalDB, Express veya tam sürüm)

### Adımlar

1. **Bağlantı dizesini kontrol edin** — `src/DilKursu.Web/appsettings.json`
   Varsayılan olarak SQL Server LocalDB kullanılır:
   ```json
   "DefaultConnection": "Server=(localdb)\\MSSQLLocalDB;Database=DilKursuDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True;Connect Timeout=60"
   ```
   Farklı bir sunucu için bu değeri güncelleyin.

2. **Uygulamayı çalıştırın:**
   ```bash
   dotnet run --project src/DilKursu.Web
   ```
   Uygulama ilk açılışta **veritabanını otomatik oluşturur** (migration uygular) ve rolleri,
   demo kullanıcıları ile örnek verileri (şubeler, öğretmen, öğrenciler, örnek kurs) **otomatik tohumlar**.
   Elle `dotnet ef` çalıştırmanıza gerek yoktur.

3. Tarayıcıdan çıkan adrese gidin ve demo hesapla giriş yapın.

> **Dayanıklılık:** Açılıştaki veritabanı hazırlığı, geçici bağlantı hatalarında (ör. LocalDB soğuk
> başlatma) kısa aralıklarla yeniden dener. Mevcut veriler **asla otomatik silinmez**; kalıcı hata
> olursa istisna yükseltilir (veri güvenliği önceliklidir).

### Migration'ı elle yönetmek (opsiyonel)
```bash
dotnet ef migrations add <Ad> --project src/DilKursu.DataAccess --startup-project src/DilKursu.Web
dotnet ef database update --project src/DilKursu.DataAccess --startup-project src/DilKursu.Web
```

---

## 🧪 Testler

Kritik iş kuralları xUnit ile test edilmiştir (gerçek veritabanı olmadan, EF Core InMemory ile):

```bash
dotnet test
```

Kapsanan senaryolar:
- **`ScheduleHelperTests`** — zaman çakışması (`Overlaps`) ve müsaitlik kapsama (`Covers`) kuralları.
- **`CourseServiceTests`** — ders açma sihirbazı: dil/şube/müsaitlik/çakışma kriterlerine göre öğretmen ve boş derslik önerisi.
- **`EnrollmentServiceTests`** — peşin/taksitli ödeme planı üretimi (yuvarlama artığı dahil toplamın korunması), kontenjan ve çift kayıt kuralları, taksit tahsilatı, **PDF makbuz verisi**.
- **`BranchServiceTests`** — CRUD, benzersiz ad kuralı, ilişkili ders koruması.

> Toplam **21 test**, tamamı başarılı.

---

## ⭐ Öne Çıkan İş Kuralı — Ders Açma Sihirbazı

Sistem yöneticisi bir ders açmak istediğinde (dil + şube + gün + saat girer),
sistem otomatik olarak şu koşulları sağlayan seçenekleri sunar:

**Uygun Öğretmen:**
1. Seçilen dili öğretebiliyor (`TeacherLanguage`),
2. Seçilen şubede ders verebiliyor (`TeacherBranch`),
3. O gün/saatte müsait (`TeacherAvailability` penceresi dersi tamamen kapsıyor),
4. O gün/saatte başka bir derse atanmamış (çakışma yok).

**Boş Derslik:**
1. Seçilen şubeye ait,
2. O gün/saatte başka bir derse tahsis edilmemiş.

Böylece yönetici, hatalı atama yapmadan yalnızca gerçekten uygun seçenekler arasından seçim yapar.

---

## 📊 Loglama ve Denetim

Sistem, **iki farklı kaygıyı bilinçli olarak ayırarak** çok katmanlı bir loglama sunar: *"kim ne yaptı"*
(denetim) ile *"ne çöktü"* (teknik hata) ayrı tablolarda, ayrı ekranlarda ve ayrı yaşam döngüleriyle tutulur.

### Üç kanal

| Kanal | İçerik | Nerede | Kim okur |
|-------|--------|--------|----------|
| **Denetim (`AuditLogs`)** | Kullanıcı işlemleri — `[Modül][Aksiyon]` | SQL tablo + ekran | Yönetici / denetçi |
| **Hata (`ErrorLogs`)** | Beklenmeyen istisnalar + **yığın izi (stack trace)** | SQL tablo + ekran | Geliştirici |
| **Serilog dosyası** | Tüm teknik günlük (istek özetleri, hatalar) | `logs/dilkursu-*.log` | Derin teşhis / arşiv |

### Denetim loglaması (kullanıcı işlemleri)

- İşlemler **`[Modül][Aksiyon]`** biçiminde tutulur (ör. `[Ders][Ekleme]`, `[Öğrenci][Silme]`, `[Kimlik][Giriş]`).
- **AOP yaklaşımı:** Yazma action'larına eklenen `[Audit("Ders","Ekleme")]` özniteliği, merkezî bir
  `AuditActionFilter` tarafından okunur; her action içinde tek tek loglama çağrısı yapmaya gerek kalmaz (DRY).
- **Seviyeler:** `Bilgi` (başarılı) · `Uyarı` (iş kuralı reddi) · `Kritik` (ör. hesap kilitlenmesi).
- Giriş/çıkış/kilitlenme olayları `AccountController` içinde ayrıca kayda alınır.
- Ekran: **Yönetim → İşlem Logları** (yalnızca Admin) — seviye filtreli, renkli rozetli DataTable.

### Hata loglaması (sistem istisnaları)

- İstek hattındaki **tüm yakalanmamış istisnalar**, bir `ErrorLoggingMiddleware` tarafından yakalanır;
  yığın izi, istisna türü, HTTP yöntemi/yolu, kullanıcı ve IP ile birlikte `ErrorLogs` tablosuna yazılır.
- Hatanın **hangi modülden geldiği** otomatik çözümlenir: action'ın `[Audit]` modülü → controller adı → `Sistem`.
- Middleware hatayı kaydettikten sonra **yeniden fırlatır**; böylece geliştirme/üretim hata sayfası davranışı değişmez.
- Ekran: **Yönetim → Hata Logları** (yalnızca Admin) — satır başına 🔍 ile **yığın izini** açan detay penceresi.

> **Neden ayrı tablolar?** Denetim izi ile teknik hata farklı şemaya (biri aktör/aksiyon, diğeri yığın izi),
> farklı hacme (bir bozuk ekran binlerce hata satırı üretebilir) ve farklı saklama ömrüne sahiptir.
> Ayırmak, denetim izinin bütünlüğünü ve okunabilirliğini korur.

---

## 🗄️ Veritabanı Notları

- **Code First** — şema entity'lerden ve Fluent API yapılandırmalarından üretilir.
- **Identity tablo adları sadeleştirildi:** varsayılan `AspNet...` önekleri yerine projedeki diğer
  tablolarla tutarlı adlar kullanılır → `Users`, `Roles`, `UserRoles`, `UserClaims`, `UserLogins`,
  `UserTokens`, `RoleClaims`.
- Parasal alanlar `decimal(18,2)`, benzersizlik/indeks kuralları ve silme davranışları (Cascade/Restrict)
  ilgili `*Configuration` dosyalarında tanımlıdır.
- **Loglama tabloları:** `AuditLogs` (kullanıcı işlemleri) ve `ErrorLogs` (istisnalar + yığın izi) ayrı
  tutulur; sık sorgulanan alanlara (tarih, modül, seviye/istisna türü) indeks eklenmiştir.

---

## 📄 Kod Standartları

- **Metotlar** anlamlı Türkçe XML açıklama satırlarına sahiptir; sınıf/özellik düzeyinde gereksiz
  açıklama tekrarı bulunmaz (okunabilir, sade kod).
- İsimlendirme, katman ayrımı ve bağımlılık yönetimi yazılım standartlarına uygundur.
- İş mantığı controller'larda değil **servis katmanındadır** (ince controller ilkesi).
- Bağımlılıklar **primary constructor** ile enjekte edilir; sınıflar arayüzlere bağımlıdır (DIP).
