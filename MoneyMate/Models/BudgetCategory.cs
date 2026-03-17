using SQLite;

namespace MoneyMate.Models
{
    /// <summary>
    /// Table pivot entre Budget et Category.
    /// Représente une catégorie assignée à un budget donné,
    /// avec des montants spécifiques et des données propres au mois.
    /// </summary>
    [Table("BudgetCategories")]
    public class BudgetCategory
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // 🔗 Relation vers Budget
        [Indexed]
        public int BudgetId { get; set; }

        // 🔗 Relation vers Category
        [Indexed]
        public int CategoryId { get; set; }

        // Montant alloué pour cette catégorie dans CE budget
        [NotNull]
        public double AllocatedAmount { get; set; }

        // Montant dépensé sur ce mois
        [NotNull]
        public double SpentAmount { get; set; } = 0;

        // Couleur propre à CE budget (optionnelle)
        [MaxLength(10)]
        public string ColorHex { get; set; } = "#CCCCCC";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // 🔹 Non persistant — pratique en UI
        [Ignore]
        public double RemainingAmount => AllocatedAmount - SpentAmount;

        public BudgetCategory() { }

        public BudgetCategory(int budgetId, int categoryId, double allocatedAmount, string colorHex = "#CCCCCC")
        {
            BudgetId = budgetId;
            CategoryId = categoryId;
            AllocatedAmount = allocatedAmount;
            ColorHex = colorHex;
        }

        public void AddExpense(double amount)
        {
            SpentAmount += amount;
            if (SpentAmount > AllocatedAmount)
                SpentAmount = AllocatedAmount;
        }

        public void ResetForNewMonth(double newAllocatedAmount)
        {
            AllocatedAmount = newAllocatedAmount;
            SpentAmount = 0;
            CreatedAt = DateTime.Now;
        }
    }
}
