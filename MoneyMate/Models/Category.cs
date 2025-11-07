using SQLite;
using System;

namespace MoneyMate.Models
{
    /// <summary>
    /// Représente une catégorie de dépense associée à un budget mensuel.
    /// Exemple : Alimentation (30%), Logement (40%), Loisirs (10%), etc.
    /// </summary>
    [Table("Categories")]
    public class Category
    {
        // Identifiant unique de la catégorie
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Référence au budget auquel cette catégorie appartient
        [Indexed]
        public int BudgetId { get; set; }

        // Nom de la catégorie (ex : Alimentation, Logement)
        [MaxLength(100), NotNull]
        public string Name { get; set; } = string.Empty;

        // Couleur hexadécimale pour affichage (ex : #FF6B6B)
        [MaxLength(10)]
        public string ColorHex { get; set; } = "#CCCCCC";

        // Pourcentage du budget global (ex : 25 = 25%)
        [NotNull]
        public double Percentage { get; set; }

        // Montant alloué automatiquement (calculé à partir du pourcentage)
        [NotNull]
        public double AllocatedAmount { get; set; }

        // Montant dépensé dans cette catégorie
        [NotNull]
        public double SpentAmount { get; set; } = 0;

        // Montant restant dans cette catégorie
        [Ignore]
        public double RemainingAmount => AllocatedAmount - SpentAmount;

        // Date de création
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Constructeur vide (obligatoire pour SQLite)
        public Category() { }

        // Constructeur pratique
        public Category(int budgetId, string name, double percentage, string colorHex = "#CCCCCC")
        {
            BudgetId = budgetId;
            Name = name;
            Percentage = percentage;
            ColorHex = colorHex;
            CreatedAt = DateTime.Now;
        }

        // Calcule le montant alloué à cette catégorie selon le budget global
        public void CalculateAllocatedAmount(double totalBudget)
        {
            AllocatedAmount = totalBudget * (Percentage / 100.0);
        }

        // Ajoute une dépense à cette catégorie
        public void AddExpense(double amount)
        {
            SpentAmount += amount;
            if (SpentAmount > AllocatedAmount)
                SpentAmount = AllocatedAmount; // sécurité
        }

        // Réinitialise les dépenses (au changement de mois)
        public void ResetSpentAmount()
        {
            SpentAmount = 0;
        }
    }
}
