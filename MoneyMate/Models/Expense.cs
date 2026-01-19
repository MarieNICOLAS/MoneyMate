using SQLite;
using System;

namespace MoneyMate.Models
{
    /// <summary>
    /// Représente une dépense enregistrée par l'utilisateur.
    /// Chaque dépense est liée à une catégorie et un budget mensuel.
    /// </summary>
    [Table("Expenses")]
    public class Expense
    {
        // Identifiant unique de la dépense
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Référence au budget concerné
        [Indexed]
        public int BudgetId { get; set; }

        // Référence à la catégorie concernée
        [Indexed]
        public int CategoryId { get; set; }

        // Montant de la dépense (en euros)
        [NotNull]
        public double Amount { get; set; }

        // Date de la dépense
        [NotNull]
        public DateTime Date { get; set; } = DateTime.Now;

        // Description facultative (ex : "Courses Carrefour")
        [MaxLength(255)]
        public string Description { get; set; } = string.Empty;

        // Date d’enregistrement (utile pour tri ou historique)
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Constructeur vide pour SQLite
        public Expense() { }

        // Constructeur pratique
        public Expense(int budgetId, int categoryId, double amount, string description = "")
        {
            BudgetId = budgetId;
            CategoryId = categoryId;
            Amount = amount;
            Description = description;
            Date = DateTime.Now;
            CreatedAt = DateTime.Now;
        }

        // Vérifie si la dépense appartient au mois du budget actif
        public bool IsInCurrentMonth()
        {
            return Date.Month == DateTime.Now.Month && Date.Year == DateTime.Now.Year;
        }

        // Méthode utilitaire pour formater le montant (utile dans les vues)
        [Ignore]
        public string FormattedAmount => $"{Amount:0.00} €";

        // Méthode utilitaire pour formater la date (utile dans les vues)
        [Ignore]
        public string FormattedDate => Date.ToString("dd/MM/yyyy");
        [Ignore] // SQLite ignore cette propriété
        public string CategoryName { get; set; }
    }
}
