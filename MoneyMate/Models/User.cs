using SQLite;

namespace MoneyMate.Models
{
    /// <summary>
    /// Modèle représentant un utilisateur de l'application MoneyMate.
    /// Les données sont stockées localement dans la base SQLite.
    /// </summary>
    [Table("Users")]
    public class User
    {
        
       [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(100), NotNull]
        public string Name { get; set; } = string.Empty;

        [MaxLength(255), Unique, NotNull]
        public string Email { get; set; } = string.Empty;

        [NotNull]
        public string PasswordHash { get; set; } = string.Empty;

        [NotNull]
        public double AlertThreshold { get; set; } = 0.8;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? LastLogin { get; set; }

        public bool IsActive { get; set; } = true;

        public User() { }

        public User(string name, string email, string passwordHash, double alertThreshold = 0.8)
        {
            Name = name;
            Email = email;
            PasswordHash = passwordHash;
            AlertThreshold = alertThreshold;
            CreatedAt = DateTime.Now;
            IsActive = true;
        }

        public void UpdateLastLogin()
        {
            LastLogin = DateTime.Now;
        }

        public void Deactivate()
        {
            IsActive = false;
            Name = "Utilisateur supprimé";
            Email = $"deleted_{Id}@moneymate.local";
        }
    }
}
