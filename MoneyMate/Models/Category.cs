using SQLite;

namespace MoneyMate.Models
{
    /// <summary>
    /// Représente une catégorie GLOBALE (Alimentation, Logement, Transport...).
    /// Indépendante des budgets. La logique d’allocation appartient à BudgetCategory.
    /// </summary>
    [Table("Categories")]
    public class Category
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [MaxLength(100), NotNull]
        public string Name { get; set; } = string.Empty;

        // Couleur par défaut de la catégorie (modifiable par budget via BudgetCategory)
        [MaxLength(10)]
        public string ColorHex { get; set; } = "#CCCCCC";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Category() { }

        public Category(string name, string colorHex = "#CCCCCC")
        {
            Name = name;
            ColorHex = colorHex;
            CreatedAt = DateTime.Now;
        }
    }
}
