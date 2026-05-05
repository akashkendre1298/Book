using BookTracker.Api.Data;
using BookTracker.Api.Models;
using BookTracker.Api.Services;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BookTracker.Api.Data
{
    public static class Seeder
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();

            // 1. Wipe DB
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            // 2. Create Users
            // Note: Bypassing AuthService's password rules (akash123 lacks uppercase) since we inject the hash directly.
            var adminUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "akash@gmail.com",
                PasswordHash = authService.HashPassword("akash123"),
                Role = "Admin",
                IsActive = true
            };

            var normalUser = new User
            {
                Id = Guid.NewGuid(),
                Email = "suraj@gmail.com",
                PasswordHash = authService.HashPassword("suraj123"),
                Role = "User",
                IsActive = true
            };

            context.Users.AddRange(adminUser, normalUser);
            await context.SaveChangesAsync();

            // 3. Create Books with covers
            var uploadsPath = Path.Combine(env.ContentRootPath, "uploads", "covers");
            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var books = new List<Book>();
            using var httpClient = new HttpClient();

            var adminBookData = new[]
            {
                new { Title = "The Shining", Author = "Stephen King", Genre = "Horror", Year = 1977 },
                new { Title = "1984", Author = "George Orwell", Genre = "Science Fiction", Year = 1949 },
                new { Title = "The Hobbit", Author = "J.R.R. Tolkien", Genre = "Fantasy", Year = 1937 },
                new { Title = "To Kill a Mockingbird", Author = "Harper Lee", Genre = "Fiction", Year = 1960 },
                new { Title = "Dracula", Author = "Bram Stoker", Genre = "Horror", Year = 1897 },
                new { Title = "Dune", Author = "Frank Herbert", Genre = "Science Fiction", Year = 1965 },
                new { Title = "Pride and Prejudice", Author = "Jane Austen", Genre = "Romance", Year = 1813 },
                new { Title = "The Catcher in the Rye", Author = "J.D. Salinger", Genre = "Fiction", Year = 1951 },
                new { Title = "Frankenstein", Author = "Mary Shelley", Genre = "Horror", Year = 1818 },
                new { Title = "The Great Gatsby", Author = "F. Scott Fitzgerald", Genre = "Fiction", Year = 1925 }
            };

            var userBookData = new[]
            {
                new { Title = "It", Author = "Stephen King", Genre = "Horror", Year = 1986 },
                new { Title = "Neuromancer", Author = "William Gibson", Genre = "Science Fiction", Year = 1984 },
                new { Title = "The Name of the Wind", Author = "Patrick Rothfuss", Genre = "Fantasy", Year = 2007 },
                new { Title = "Fahrenheit 451", Author = "Ray Bradbury", Genre = "Science Fiction", Year = 1953 },
                new { Title = "The Haunting of Hill House", Author = "Shirley Jackson", Genre = "Horror", Year = 1959 }
            };

            foreach (var data in adminBookData)
            {
                var book = await CreateBookAsync(httpClient, adminUser, data.Title, data.Author, data.Genre, data.Year, "Public", uploadsPath);
                books.Add(book);
            }

            foreach (var data in userBookData)
            {
                var book = await CreateBookAsync(httpClient, normalUser, data.Title, data.Author, data.Genre, data.Year, "Private", uploadsPath);
                books.Add(book);
            }

            context.Books.AddRange(books);
            await context.SaveChangesAsync();
            
            Console.WriteLine("Database seeded successfully.");
        }

        private static async Task<Book> CreateBookAsync(HttpClient client, User user, string title, string author, string genre, int year, string visibility, string uploadsPath)
        {
            var fileName = $"{Guid.NewGuid()}_{title.Replace(" ", "_")}.jpg";
            var filePath = Path.Combine(uploadsPath, fileName);
            
            // Download a random image
            var imageBytes = await client.GetByteArrayAsync($"https://picsum.photos/seed/{Guid.NewGuid()}/400/600");
            await File.WriteAllBytesAsync(filePath, imageBytes);

            return new Book
            {
                Id = Guid.NewGuid(),
                Title = title,
                Author = author,
                Visibility = visibility == "Public" ? BookVisibility.Public : BookVisibility.Private,
                UserId = user.Id,
                OwnerRole = user.Role,
                IsApproved = visibility == "Public",
                CoverImageUrl = $"/uploads/covers/{fileName}",
                Status = ReadingStatus.WantToRead,
                Genre = genre,
                PublicationYear = year,
                TotalPages = 300,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }
    }
}
