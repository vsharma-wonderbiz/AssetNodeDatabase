using AssetNode.Models.Entities;
using AssetNode.Services.Sql; // For HashPassword method

namespace AssetNode.Data
{
    public class SeedAdmin
    {
        public static void Initialize(AssetDbContext context)
        {
            context.Database.EnsureCreated(); // Create DB if not exists

            if (!context.Users.Any(u => u.Role == "Admin"))
            {
                Console.WriteLine("No admin found. Creating default admin...");

                var admin = new User
                {
                    Username = "admin",
                    Email = "admin@example.com",
                    Role = "Admin",
                    PasswordHash = HashPassword("Admin@123") // Use same hash as normal users
                };

                context.Users.Add(admin);
                context.SaveChanges();

                Console.WriteLine("✅ Default admin created!");
            }
            else
            {
                Console.WriteLine("⚡ Admin already exists. Skipping seed.");
            }
        }

        // Use same method as in CreateUser
        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}
