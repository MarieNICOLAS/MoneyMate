using System;

namespace MoneyMate.ViewModels.Expense
{
    /// <summary>
    /// Modèle utilisé pour contenir les résultats agrégés 
    /// des dépenses (Somme totale par catégorie).
    /// </summary>
    public class ExpenseSummary
    {
        public int CategoryId { get; set; }

        // Nom de la catégorie
        public string? CategoryName { get; set; }

        // Alias de la colonne ColorHex du modèle Category
        public string? CategoryColor { get; set; }

        // Colonne non existante dans le nouveau modèle Category, mais ajoutée pour compatibilité DTO
        public string? CategoryIcon { get; set; }

        // Somme des dépenses pour cette catégorie
        public double TotalAmount { get; set; }

        /// <summary>
        /// Propriété utilitaire pour formater le montant (BindingContext)
        /// </summary>
        public string FormattedTotalAmount => $"{TotalAmount:0.00} €";
    }
}