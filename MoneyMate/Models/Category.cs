using SQLite;

namespace MoneyMate.Models
{
    /// <summary>
    /// Représente une catégorie globale et réutilisable dans plusieurs budgets.
    /// Les relations Budget ↔ Catégorie sont gérées via la table BudgetCategory.
    /// </summary>
    [Table("Categories")]
    public class Category
    {
        // Identifiant unique
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Nom de la catégorie (ex : "Alimentation")
        [MaxLength(100), NotNull]
        public string Name { get; set; } = string.Empty;

        // Couleur visuelle (ex : #FFAFAD)
        [MaxLength(10)]
        public string ColorHex { get; set; } = "#f4cfff";

        // Date de création
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public Category() { }

        public Category(string name, string colorHex)
        {
            Name = name;
            ColorHex = colorHex;
            CreatedAt = DateTime.Now;
        }
    }
}
