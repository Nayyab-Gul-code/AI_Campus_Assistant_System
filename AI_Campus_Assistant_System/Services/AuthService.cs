using AI_Campus_Assistant.Data;
using AI_Campus_Assistant.Models;
using MongoDB.Driver;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace AI_Campus_Assistant.Services
{
    public class AuthService
    {
        private readonly MongoDbContext _db;

        public AuthService(MongoDbContext db)
        {
            _db = db;
        }

        public async Task<User?> ValidateLoginAsync(string email, string password)
        {
            var user = await _db.Users
                .Find(u => u.Email == email.ToLower().Trim() && u.IsActive)
                .FirstOrDefaultAsync();

            if (user == null) return null;
            if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash)) return null;

            // Update last login
            var update = Builders<User>.Update.Set(u => u.LastLogin, DateTime.UtcNow);
            await _db.Users.UpdateOneAsync(u => u.Id == user.Id, update);

            return user;
        }

        public async Task<(bool success, string message)> RegisterAsync(
            string name, string email, string password, string role,
            string? phone = null, string? program = null,
            int semesterNo = 1, string? designation = null)
        {
            // Check duplicate email
            var exists = await _db.Users.Find(u => u.Email == email.ToLower().Trim()).AnyAsync();
            if (exists) return (false, "Email already registered.");

            // Only student/teacher can self-register
            if (role == "admin") return (false, "Admin registration not allowed.");

            var user = new User
            {
                Name         = name.Trim(),
                Email        = email.ToLower().Trim(),
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                Role         = role,
                Phone        = phone,
                Program      = program,
                SemesterNo   = semesterNo,
                Designation  = designation,
                IsActive     = true,
                CreatedAt    = DateTime.UtcNow
            };

            await _db.Users.InsertOneAsync(user);
            return (true, "Account created successfully.");
        }

        public ClaimsPrincipal BuildPrincipal(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id!),
                new Claim(ClaimTypes.Name,  user.Name),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role,  user.Role),
            };
            var identity  = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            return new ClaimsPrincipal(identity);
        }

        public async Task<User?> GetUserByIdAsync(string id)
        {
            return await _db.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
        }

        public async Task<bool> UpdateProfileAsync(string id, string name, string? phone, string? bio, string? designation)
        {
            var update = Builders<User>.Update
                .Set(u => u.Name,        name)
                .Set(u => u.Phone,       phone)
                .Set(u => u.Bio,         bio)
                .Set(u => u.Designation, designation);
            var result = await _db.Users.UpdateOneAsync(u => u.Id == id, update);
            return result.ModifiedCount > 0;
        }

        public async Task<bool> ChangePasswordAsync(string id, string currentPassword, string newPassword)
        {
            var user = await _db.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null) return false;
            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash)) return false;

            var update = Builders<User>.Update
                .Set(u => u.PasswordHash, BCrypt.Net.BCrypt.HashPassword(newPassword));
            await _db.Users.UpdateOneAsync(u => u.Id == id, update);
            return true;
        }
    }
}
