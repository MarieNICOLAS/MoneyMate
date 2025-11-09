using MoneyMate.Database;
using MoneyMate.Models;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Maui.Storage;

namespace MoneyMate.Services
{
    public class AuthService
    {
        private readonly MoneyMateContext _db;

        public AuthService(MoneyMateContext db)
        {
            _db = db;
        }

        // ✅ Connexion avec option "Se souvenir de moi"
        public async Task<User?> LoginAsync(string email, string password, bool rememberMe = false)
        {
            string hash = ComputeHash(password);
            var users = await _db.GetAllAsync<User>();
            var user = users.FirstOrDefault(u => u.Email == email && u.PasswordHash == hash && u.IsActive);

            if (user != null && rememberMe)
            {
                Preferences.Set("IsLoggedIn", true);
                Preferences.Set("UserEmail", user.Email);
            }

            return user;
        }

        // ✅ Vérifie si un utilisateur est déjà connecté
        public static bool IsUserLoggedIn()
        {
            return Preferences.Get("IsLoggedIn", false);
        }

        // ✅ Déconnexion (efface les préférences)
        public static void Logout()
        {
            Preferences.Clear();
        }

        // ✅ Inscription d’un nouvel utilisateur
        public async Task<bool> RegisterAsync(string email, string password, string name)
        {
            var users = await _db.GetAllAsync<User>();
            if (users.Any(u => u.Email == email))
                return false;

            string hash = ComputeHash(password);

            var newUser = new User
            {
                Name = name,
                Email = email,
                PasswordHash = hash,
                CreatedAt = DateTime.Now
            };

            await _db.InsertAsync(newUser);
            return true;
        }

        // ✅ Hachage sécurisé du mot de passe
        private static string ComputeHash(string input)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
            return Convert.ToHexString(bytes);
        }
    }
}
