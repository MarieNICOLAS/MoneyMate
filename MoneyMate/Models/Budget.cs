using SQLite;
using System;

namespace MoneyMate.Models
{
    /// <summary>
    /// Représente le budget mensuel d’un utilisateur.
    /// </summary>
    [Table("Budgets")]
    public class Budget
    {
        // Identifiant unique du budget (clé primaire auto-incrémentée)
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Référence à l’utilisateur propriétaire de ce budget
        [Indexed]
        public int UserId { get; set; }

        // Montant total du budget mensuel (en euros)
        [NotNull]
        public double TotalAmount { get; set; }

        // Mois du budget (exemple : 10 pour octobre)
        [NotNull]
        public int Month { get; set; }

        // Année du budget (exemple : 2025)
        [NotNull]
        public int Year { get; set; }

        // Montant déjà dépensé (sera mis à jour automatiquement)
        [NotNull]
        public double SpentAmount { get; set; } = 0;

        // Montant restant (calculé à la volée)
        [Ignore]
        public double RemainingAmount => TotalAmount - SpentAmount;

        // Date de création (utile pour historique)
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Indique si le budget est toujours actif (mois courant)
        public bool IsActive { get; set; } = true;

        // Constructeur vide (obligatoire pour SQLite)
        public Budget() { }

        // Constructeur pratique
        public Budget(int userId, double totalAmount)
        {
            UserId = userId;
            TotalAmount = totalAmount;
            Month = DateTime.Now.Month;
            Year = DateTime.Now.Year;
            CreatedAt = DateTime.Now;
            IsActive = true;
        }

        // Vérifie si le budget correspond au mois courant
        public bool IsForCurrentMonth()
        {
            return Month == DateTime.Now.Month && Year == DateTime.Now.Year;
        }

        // Réinitialise le budget pour un nouveau mois
        public void ResetForNewMonth(double newAmount)
        {
            TotalAmount = newAmount;
            SpentAmount = 0;
            Month = DateTime.Now.Month;
            Year = DateTime.Now.Year;
            IsActive = true;
            CreatedAt = DateTime.Now;
        }

        // Met à jour les dépenses
        public void AddExpense(double amount)
        {
            SpentAmount += amount;
            if (SpentAmount > TotalAmount)
                SpentAmount = TotalAmount; // sécurité pour éviter négatif
        }
    }
}
