using MoneyMate.Database;
using MoneyMate.Models;

namespace MoneyMate.Services
{
    public class ExpenseService
    {
        private readonly MoneyMateContext _context;

        public ExpenseService()
        {
            _context = new MoneyMateContext();
        }

        // 🧩 Ajouter une dépense
        public async Task AddExpenseAsync(Expense expense)
        {
            await _context.InsertAsync(expense);
        }

        // 🧩 Supprimer une dépense
        public async Task DeleteExpenseAsync(Expense expense)
        {
            await _context.DeleteAsync(expense);
        }

        // 🧩 Récupérer toutes les dépenses
        public async Task<List<Expense>> GetExpensesAsync()
        {
            return await _context.GetAllAsync<Expense>();
        }

        // 🧩 Modifier une dépense
        public async Task UpdateExpenseAsync(Expense expense)
        {
            await _context.UpdateAsync(expense);
        }
    }
}
