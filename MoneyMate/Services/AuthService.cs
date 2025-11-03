using MoneyMate.Database;
using MoneyMate.Models;
using System.Security.Cryptography;
using System.Text;

namespace MoneyMate.Services
{
    public class AuthService
    {
        private readonly MoneyMateContext _db;

        public AuthService(MoneyMateContext db)
        {
            _db = db;
        }

        public async Task<User?> LoginAsync(string email, string password)
        {
            string hash = ComputeHash(password);
            var users = await _db.GetAllAsync<User>();
            return users.FirstOrDefault(u => u.Email == email && u.PasswordHash == hash && u.IsActive);
        }

        private static string ComputeHash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}
