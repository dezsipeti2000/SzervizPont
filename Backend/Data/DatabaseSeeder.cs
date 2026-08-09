using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SzervizPont.Models;

namespace SzervizPont.Data;

public static class DatabaseSeeder
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();

        await db.Database.EnsureCreatedAsync();

        if (!await db.Users.AnyAsync(user => user.Email == "admin@szervizpont.hu"))
        {
            var admin = new User
            {
                Name = "Admin",
                Email = "admin@szervizpont.hu",
                Role = Roles.Admin
            };

            admin.PasswordHash = passwordHasher.HashPassword(admin, "Admin123!");
            db.Users.Add(admin);
        }

        if (!await db.Services.AnyAsync())
        {
            db.Services.AddRange(
                new ServiceItem
                {
                    Name = "Olajcsere",
                    Description = "Motorolaj és olajszűrő cseréje.",
                    Price = 28000,
                    EstimatedMinutes = 60
                },
                new ServiceItem
                {
                    Name = "Fékellenőrzés",
                    Description = "A fékrendszer állapotának átvizsgálása.",
                    Price = 15000,
                    EstimatedMinutes = 45
                },
                new ServiceItem
                {
                    Name = "Általános diagnosztika",
                    Description = "Hibakódolvasás és alapvető műszaki átvizsgálás.",
                    Price = 12000,
                    EstimatedMinutes = 30
                });
        }

        await db.SaveChangesAsync();
    }
}
