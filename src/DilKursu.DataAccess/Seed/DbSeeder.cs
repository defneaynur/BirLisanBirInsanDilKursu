using DilKursu.DataAccess.Context;
using DilKursu.DataAccess.Identity;
using DilKursu.Entities;
using DilKursu.Entities.Enums;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DilKursu.DataAccess.Seed;

public static class DbSeeder
{
    /// <summary>
    /// Bekleyen migration'ları uygular ve başlangıç verilerini belirler.
    /// </summary>
    /// <param name="serviceProvider">Gerekli servislerin çözümleneceği kapsamlı (scoped) sağlayıcı.</param>
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        var context = serviceProvider.GetRequiredService<AppDbContext>();
        var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

        // 1) Veritabanını dayanıklı biçimde hazırla (soğuk başlatma toleransı + bozulma onarımı).
        await EnsureDatabaseReadyAsync(context);

        // 2) Rolleri oluştur.
        await SeedRolesAsync(roleManager);

        // 3) Varsayılan yönetici ve kayıt elemanı kullanıcılarını oluştur.
        await SeedUsersAsync(userManager);

        // 4) Örnek referans verilerini (dil, şube, derslik, öğretmen) oluştur.
        await SeedDomainDataAsync(context);

        // 5) Ek şubelerin (farklı şehir/semt) her koşulda var olmasını sağla (idempotent).
        await EnsureBranchesAsync(context);

        // 6) Kullanıcıların şube atamalarını yap (kayıt elemanı bir şubeden çalışır; yönetici merkezidir).
        await AssignUserBranchesAsync(context, userManager);

        // 7) Örnek öğrenci ve kursların var olmasını sağla (idempotent).
        await EnsureSampleStudentsAndCoursesAsync(context);
    }

    /// <summary>
    /// Kullanıcıların bağlı olduğu şubeleri ayarlar (idempotent).
    /// Kayıt elemanı belirli bir şubeden (ör. Kadıköy) çalışır; sistem yöneticisi merkezidir (şubesiz).
    /// Not: Merkezi sunucu mantığı gereği, kullanıcı bir şubeye bağlı olsa dahi tüm şubelerle
    /// ilgili işlem yapabilir; bu atama yalnızca "hangi şubeden giriş yapıldığı" bilgisini taşır.
    /// </summary>
    /// <param name="context">Veritabanı bağlamı.</param>
    /// <param name="userManager">Identity kullanıcı yöneticisi.</param>
    private static async Task AssignUserBranchesAsync(AppDbContext context, UserManager<ApplicationUser> userManager)
    {
        var kadikoy = await context.Branches.FirstOrDefaultAsync(b => b.Name == "Kadıköy Şubesi");
        if (kadikoy is null)
        {
            return;
        }

        // Kayıt elemanı Kadıköy şubesine bağlanır (henüz bağlı değilse).
        var kayit = await userManager.FindByEmailAsync("kayit@birlisanbirinsan.com");
        if (kayit is not null && kayit.BranchId != kadikoy.Id)
        {
            kayit.BranchId = kadikoy.Id;
            await userManager.UpdateAsync(kayit);
        }
    }

    /// <summary>
    /// Veritabanını uygulama açılışında güvenilir biçimde hazırlar.
    /// Bekleyen migration'ları uygular (veritabanı yoksa oluşturur, varsa günceller).
    /// Geçici bağlantı hatalarında (ör. LocalDB soğuk başlatma) kısa aralıklarla birkaç kez
    /// yeniden dener. ÖNEMLİ: Mevcut veriler asla silinmez; kalıcı bir hata olursa istisna
    /// yukarı fırlatılır (kullanıcı verisinin korunması önceliklidir).
    /// </summary>
    /// <param name="context">Veritabanı bağlamı.</param>
    private static async Task EnsureDatabaseReadyAsync(AppDbContext context)
    {
        const int maxAttempts = 5;

        for (var attempt = 1; attempt < maxAttempts; attempt++)
        {
            try
            {
                await context.Database.MigrateAsync();
                return;
            }
            catch
            {
                // Geçici hata olabilir (örnek henüz başlamadı); kısa bekleyip yeniden dene.
                await Task.Delay(TimeSpan.FromSeconds(3));
            }
        }

        // Son deneme: hâlâ başarısızsa hata yukarı fırlar (veri güvenliği için otomatik silme YOK).
        await context.Database.MigrateAsync();
    }

    /// <summary>
    /// Sistemde örnek öğrenci ve kursların bulunmasını garanti eden idempotent tohumlama.
    /// Her açılışta çalışır; ilgili tablo zaten doluysa tekrar eklemez.
    /// Böylece uygulama açıldığında listeler ve grafikler örnek verilerle dolu görünür.
    /// </summary>
    /// <param name="context">Veritabanı bağlamı.</param>
    private static async Task EnsureSampleStudentsAndCoursesAsync(AppDbContext context)
    {
        // Örnek öğrenciler (hiç öğrenci yoksa eklenir).
        if (!await context.Students.AnyAsync())
        {
            context.Students.AddRange(
                new Student { FullName = "Aylin Defne Açar", MobilePhone = "0555 111 22 33", HomePhone = "0216 111 22 33" },
                new Student { FullName = "Aynur Açar", MobilePhone = "0555 222 33 44" },
                new Student { FullName = "Can Yıldız", MobilePhone = "0555 333 44 55" });
            await context.SaveChangesAsync();
        }

        // Örnek kurs (hiç kurs yoksa, tohumlanan öğretmen/şube/derslik/dil kullanılarak eklenir).
        if (!await context.Courses.AnyAsync())
        {
            var english = await context.Languages.FirstOrDefaultAsync(l => l.Name == "İngilizce");
            var kadikoy = await context.Branches.FirstOrDefaultAsync(b => b.Name == "Kadıköy Şubesi");
            var teacher = await context.Teachers.FirstOrDefaultAsync();
            var classroom = kadikoy == null
                ? null
                : await context.Classrooms.FirstOrDefaultAsync(c => c.BranchId == kadikoy.Id);

            // Tüm gerekli referanslar mevcutsa örnek kurs oluşturulur.
            if (english != null && kadikoy != null && teacher != null && classroom != null)
            {
                var course = new Course
                {
                    LanguageId = english.Id,
                    BranchId = kadikoy.Id,
                    TeacherId = teacher.Id,
                    ClassroomId = classroom.Id,
                    Level = KurSeviyesi.A1,
                    Day = DayOfWeek.Monday,
                    StartTime = new TimeSpan(10, 0, 0),
                    EndTime = new TimeSpan(12, 0, 0),
                    StartDate = DateTime.Today,
                    Quota = 12,
                    Fee = 6000
                };
                context.Courses.Add(course);
                await context.SaveChangesAsync();

                // Örnek bir öğrenciyi bu kursa taksitli kaydet (3 taksit) — ödeme/rapor ekranlarını doldurur.
                var firstStudent = await context.Students.OrderBy(s => s.Id).FirstOrDefaultAsync();
                if (firstStudent != null)
                {
                    var enrollment = new Enrollment
                    {
                        StudentId = firstStudent.Id,
                        CourseId = course.Id,
                        EnrollmentDate = DateTime.Now,
                        PaymentType = OdemeTuru.Taksitli,
                        TotalAmount = course.Fee
                    };
                    // 3 eşit taksit; ilk taksit ödenmiş olarak işaretlenir (makbuz örneği için).
                    for (var i = 1; i <= 3; i++)
                    {
                        enrollment.Installments.Add(new Installment
                        {
                            SequenceNo = i,
                            DueDate = DateTime.Today.AddMonths(i - 1),
                            Amount = course.Fee / 3,
                            IsPaid = i == 1,
                            PaidDate = i == 1 ? DateTime.Now : null
                        });
                    }
                    context.Enrollments.Add(enrollment);
                    await context.SaveChangesAsync();
                }
            }
        }
    }

    /// <summary>
    /// Sistemde birden fazla şube bulunmasını garanti eden idempotent tohumlama.
    /// Her açılışta çalışır; ismi zaten kayıtlı olan şubeyi tekrar eklemez.
    /// Böylece hem sıfırdan kurulumda hem de mevcut veritabanında en az üç şube olur.
    /// </summary>
    /// <param name="context">Veritabanı bağlamı.</param>
    private static async Task EnsureBranchesAsync(AppDbContext context)
    {
        // Eklenmesi istenen ek şubeler ve derslikleri (ad bazında benzersiz).
        var desiredBranches = new[]
        {
            new Branch
            {
                Name = "Beşiktaş Şubesi",
                Address = "Sinanpaşa Mah. Barbaros Bulvarı No:24, Beşiktaş/İstanbul",
                PublicTransportInstructions = "Beşiktaş vapur iskelesi ve metrobüs durağına yürüme mesafesindedir.",
                CarTransportInstructions = "Barbaros Bulvarı üzerinde vale hizmetli otopark mevcuttur.",
                SocialFacilities = "Kütüphane, kafeterya ve grup çalışma odaları bulunmaktadır.",
                Classrooms =
                {
                    new Classroom { Name = "B-201", Capacity = 18 },
                    new Classroom { Name = "B-202", Capacity = 12 }
                }
            },
            new Branch
            {
                Name = "Ankara Kızılay Şubesi",
                Address = "Kızılay Mah. Atatürk Bulvarı No:98, Çankaya/Ankara",
                PublicTransportInstructions = "Kızılay metro istasyonuna 3 dk yürüme mesafesindedir.",
                CarTransportInstructions = "Bina altında saatlik ücretli kapalı otopark vardır.",
                SocialFacilities = "Ücretsiz Wi-Fi, çay/kahve ikramı ve sessiz çalışma salonu.",
                Classrooms =
                {
                    new Classroom { Name = "K-101", Capacity = 20 },
                    new Classroom { Name = "K-102", Capacity = 16 },
                    new Classroom { Name = "K-103", Capacity = 10 }
                }
            }
        };

        var added = false;
        foreach (var branch in desiredBranches)
        {
            // Aynı isimde şube yoksa ekle (idempotent kontrol).
            if (!await context.Branches.AnyAsync(b => b.Name == branch.Name))
            {
                context.Branches.Add(branch);
                added = true;
            }
        }

        if (added)
        {
            await context.SaveChangesAsync();
        }
    }

    /// <summary>
    /// Tanımlı tüm rolleri, yoksa oluşturur.
    /// </summary>
    /// <param name="roleManager">Identity rol yöneticisi.</param>
    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in AppRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    /// <summary>
    /// Varsayılan yönetici ve kayıt elemanı kullanıcılarını, yoklarsa oluşturur ve rollerine atar.
    /// </summary>
    /// <param name="userManager">Identity kullanıcı yöneticisi.</param>
    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        // Varsayılan yönetici.
        await CreateUserIfMissingAsync(userManager, "admin@birlisanbirinsan.com", "Admin123!",
            "Sistem Yöneticisi", AppRoles.Admin);

        // Varsayılan kayıt elemanı.
        await CreateUserIfMissingAsync(userManager, "kayit@birlisanbirinsan.com", "Kayit123!",
            "Kayıt Elemanı", AppRoles.Kayit);
    }

    /// <summary>
    /// Belirtilen e-posta ile kullanıcı yoksa oluşturur ve verilen role atar (DRY yardımcı).
    /// </summary>
    /// <param name="userManager">Identity kullanıcı yöneticisi.</param>
    /// <param name="email">Kullanıcı e-postası (aynı zamanda kullanıcı adı).</param>
    /// <param name="password">Başlangıç parolası.</param>
    /// <param name="fullName">Görünen ad.</param>
    /// <param name="role">Atanacak rol.</param>
    private static async Task CreateUserIfMissingAsync(UserManager<ApplicationUser> userManager,
        string email, string password, string fullName, string role)
    {
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FullName = fullName
        };

        var result = await userManager.CreateAsync(user, password);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(user, role);
        }
    }

    /// <summary>
    /// Sistemin boş açılmaması için birkaç örnek dil, şube, derslik ve öğretmen ekler.
    /// Yalnızca ilgili tablolar boşsa çalışır.
    /// </summary>
    /// <param name="context">Veritabanı bağlamı.</param>
    private static async Task SeedDomainDataAsync(AppDbContext context)
    {
        if (await context.Languages.AnyAsync())
        {
            // Veri zaten tohumlanmış; tekrar ekleme.
            return;
        }

        // Örnek diller.
        var english = new Language { Name = "İngilizce" };
        var german = new Language { Name = "Almanca" };
        var french = new Language { Name = "Fransızca" };
        context.Languages.AddRange(english, german, french);

        // Örnek şube ve derslikler.
        var branch = new Branch
        {
            Name = "Kadıköy Şubesi",
            Address = "Caferağa Mah. Moda Cad. No:10, Kadıköy/İstanbul",
            PublicTransportInstructions = "Kadıköy metro/vapur iskelesine 5 dk yürüme mesafesindedir.",
            CarTransportInstructions = "Bina altında ücretsiz kapalı otopark mevcuttur.",
            SocialFacilities = "Kafeterya, ücretsiz Wi-Fi ve sessiz çalışma salonu bulunmaktadır."
        };
        branch.Classrooms.Add(new Classroom { Name = "A-101", Capacity = 15 });
        branch.Classrooms.Add(new Classroom { Name = "A-102", Capacity = 20 });
        context.Branches.Add(branch);

        // Örnek öğretmen (İngilizce, Kadıköy, Pazartesi 09:00-18:00 müsait).
        var teacher = new Teacher
        {
            FullName = "Ayşe Yılmaz",
            HomePhone = "0216 000 00 00",
            MobilePhone = "0532 000 00 00",
            StartDate = new DateTime(2022, 9, 1)
        };
        teacher.TeacherLanguages.Add(new TeacherLanguage { Language = english });
        teacher.TeacherBranches.Add(new TeacherBranch { Branch = branch });
        teacher.Availabilities.Add(new TeacherAvailability
        {
            Day = DayOfWeek.Monday,
            StartTime = new TimeSpan(9, 0, 0),
            EndTime = new TimeSpan(18, 0, 0)
        });
        context.Teachers.Add(teacher);

        await context.SaveChangesAsync();
    }
}
