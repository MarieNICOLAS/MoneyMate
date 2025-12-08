using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyMate.Models
{
    [Table("BudgetCategories")]
    public  class Budget_category
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        // Référence à l’utilisateur propriétaire de ce budget
        [Indexed]
        public int BudgetId { get; set; }
        [Indexed]
        public int CategoryId { get; set; }

        // Montant limite pour cette catégorie dans ce budget
        public double Percentage { get; set; }

        
        public double CalculatedAmount(double totalBudget) { return totalBudget * (Percentage / 100.0); }

    }
}
