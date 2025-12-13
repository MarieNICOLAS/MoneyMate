using SQLite;

namespace MoneyMate.Models
{
    /// <summary>
    /// Table d’association entre un Budget et une Category.
    /// - Une catégorie peut être liée à plusieurs budgets.
    /// - Un budget peut contenir plusieurs catégories.
    /// 
    /// Cette table stocke les données spécifiques AU COUPLE Budget–Catégorie :
    /// → Percentage : pourcentage du budget alloué à cette catégorie.
    /// → LimitAmount : montant maximum autorisé pour cette catégorie dans ce budget.
    /// → SpentAmount : montant dépensé dans ce budget pour cette catégorie.
    /// </summary>
    [Table("BudgetCategories")]
    public class BudgetCategory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // FK vers Budget.Id
        [Indexed]
        public int BudgetId { get; set; }

        // FK vers Category.Id
        [Indexed]
        public int CategoryId { get; set; }

        /// <summary>
        /// Pourcentage du budget attribué à cette catégorie.
        /// Exemple : 25 = 25%.
        /// </summary>
        [NotNull]
        public double Percentage { get; set; } = 0;

        /// <summary>
        /// Montant maximum autorisé pour cette catégorie
        /// dans CE budget.
        /// </summary>
        [NotNull]
        public double LimitAmount { get; set; } = 0;

        /// <summary>
        /// Montant réellement dépensé dans CE budget pour cette catégorie.
        /// </summary>
        [NotNull]
        public double SpentAmount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Mise à jour automatique à chaque recalcul.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Recalcule le montant limite (en euros)
        /// à partir du Percentage et du total du budget.
        /// </summary>
        public void RecalculateLimit(double budgetTotal)
        {
            if (Percentage < 0) Percentage = 0;
            if (Percentage > 100) Percentage = 100;

            LimitAmount = Math.Round(budgetTotal * (Percentage / 100.0), 2);
            UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// Montant restant disponible pour cette catégorie.
        /// </summary>
        public double RemainingAmount => LimitAmount - SpentAmount;
    }
}
