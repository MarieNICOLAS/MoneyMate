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

        // Nom de la catégorie (ex : Alimentation, Transport…)
        [MaxLength(100), NotNull]
        public string Name { get; set; } = string.Empty;

        // Couleur pour l'affichage dans l'app (ex : #FF6B6B)
        [MaxLength(10)]
        public string ColorHex { get; set; } = "#CCCCCC";

        // Date de création
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Constructeur vide (obligatoire pour SQLite)
        public Category() { }

        // Constructeur pratique
        public Category(string name, string colorHex = "#CCCCCC")
        {
            Name = name;
            ColorHex = colorHex;
            CreatedAt = DateTime.Now;
        }
    }
}
