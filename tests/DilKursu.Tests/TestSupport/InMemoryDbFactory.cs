using DilKursu.DataAccess.Context;
using Microsoft.EntityFrameworkCore;

namespace DilKursu.Tests.TestSupport;

public static class InMemoryDbFactory
{
    /// <summary>
    /// Benzersiz adlı, boş bir InMemory AppDbContext örneği oluşturur.
    /// Her çağrı farklı bir veritabanı kullandığından testler birbirini etkilemez.
    /// </summary>
    /// <returns>Yeni bir InMemory veritabanı bağlamı.</returns>
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
