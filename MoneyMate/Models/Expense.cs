using SQLite;

namespace MoneyMate.Models
{
    /// <summary>
    /// Représente une dépense liée à un budget et à une catégorie
    /// via le pivot BudgetCategory.
    /// </summary>
    [Table("Expenses")]
    public class Expense
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // 🔹 Référence du budget
        [Indexed]
        public int BudgetId { get; set; }

        // 🔹 Référence du pivot BudgetCategory
        [Indexed]
        public int BudgetCategoryId { get; set; }

        // 🔹 Référence de la Category globale (utile pour affichage)
        [Indexed]
        public int CategoryId { get; set; }

        // 🔹 Montant de la dépense
        [NotNull]
        public double Amount { get; set; }

        // 🔹 Description facultative
        public string Description { get; set; } = string.Empty;

        // 🔹 Date réelle de la dépense
        public DateTime Date { get; set; } = DateTime.Now;

        // 🔹 Date de création en base
        public DateTime CreatedAt { get; set; } = DateTime.Now;


        // ------------------------------
        //      UTILS / FORMATTING
        // ------------------------------

        [Ignore]
        public string FormattedAmount =>
            $"{Amount:0.00} €";

        [Ignore]
        public string FormattedDate =>
            Date.ToString("dd/MM/yyyy");
    }
}
