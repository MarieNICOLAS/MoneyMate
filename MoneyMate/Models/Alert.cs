using SQLite;
using System;

namespace MoneyMate.Models
{
    /// <summary>
    /// Représente une alerte ou notification générée automatiquement
    /// lorsque certaines conditions budgétaires sont atteintes.
    /// </summary>
    [Table("Alerts")]
    public class Alert
    {
        // Identifiant unique
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Référence à l'utilisateur concerné
        [Indexed]
        public int UserId { get; set; }

        // Référence optionnelle à un budget ou une catégorie
        public int? BudgetId { get; set; }
        public int? CategoryId { get; set; }

        // Type d'alerte (ex: "Global", "Category", "Info")
        [MaxLength(50), NotNull]
        public string Type { get; set; } = "Global";

        // Message à afficher à l'utilisateur
        [MaxLength(255), NotNull]
        public string Message { get; set; } = string.Empty;

        // Date de création de l'alerte
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Date de lecture (null = non lue)
        public DateTime? ReadAt { get; set; }

        // Indique si l'alerte a été vue / lue
        [Ignore]
        public bool IsRead => ReadAt != null;

        // Constructeur vide pour SQLite
        public Alert() { }

        // Constructeur pratique
        public Alert(int userId, string type, string message, int? budgetId = null, int? categoryId = null)
        {
            UserId = userId;
            Type = type;
            Message = message;
            BudgetId = budgetId;
            CategoryId = categoryId;
            CreatedAt = DateTime.Now;
        }

        // Marque l'alerte comme lue
        public void MarkAsRead()
        {
            ReadAt = DateTime.Now;
        }

        // Méthode statique utile pour générer des alertes standardisées
        public static Alert CreateThresholdAlert(int userId, double percent, string scope, int? budgetId = null, int? categoryId = null)
        {
            string message = percent >= 100
                ? $"🚨 {scope} : budget dépassé !"
                : $"⚠️ {scope} : {percent:0}% du budget consommé.";

            string type = scope == "Global" ? "Global" : "Category";

            return new Alert(userId, type, message, budgetId, categoryId);
        }
    }
}
