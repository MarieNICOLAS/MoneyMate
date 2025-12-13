using SQLite;
using System;

namespace MoneyMate.Models
{
    /// <summary>
    /// Représente le budget mensuel d’un utilisateur.
    /// Les dépenses sont calculées à partir des BudgetCategory, pas stockées ici.
    /// </summary>
    [Table("Budgets")]
    public class Budget
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed]
        public int UserId { get; set; }

        [NotNull]
        public double TotalAmount { get; set; }

        [NotNull]
        public int Month { get; set; }

        [NotNull]
        public int Year { get; set; }

        // Calculés à l’usage, pas stockés dans la DB
        [Ignore]
        public double SpentAmount { get; set; }

        [Ignore]
        public double RemainingAmount => TotalAmount - SpentAmount;

        [Ignore]
        public string DisplayName => $"{TotalAmount} € - {Month}/{Year}";

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsActive { get; set; } = true;

        public Budget() { }

        public Budget(int userId, double totalAmount)
        {
            UserId = userId;
            TotalAmount = totalAmount;
            Month = DateTime.Now.Month;
            Year = DateTime.Now.Year;
            CreatedAt = DateTime.Now;
            IsActive = true;
        }

        public bool IsForCurrentMonth()
        {
            return Month == DateTime.Now.Month && Year == DateTime.Now.Year;
        }
    }
}
