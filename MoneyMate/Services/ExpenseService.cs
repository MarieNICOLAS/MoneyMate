using MoneyMate.Database;
using MoneyMate.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyMate.Services
{
    public class ExpenseService
    {
        private readonly MoneyMateContext _db;

        public ExpenseService(MoneyMateContext db)
        {
            _db = db;
        }

        //  Récupérer toutes les dépenses
        public Task<List<Expense>> GetExpensesAsync()
            => _db.GetAllAsync<Expense>();

        //  Récupérer les dépenses par catégorie
        public async Task<List<Expense>> GetExpensesByCategoryAsync(int categoryId)
        {
            var expenses = await _db.GetAllAsync<Expense>();
            return expenses.Where(e => e.CategoryId == categoryId).ToList();
        }

        //  Récupérer les dépenses par budget
        public async Task<List<Expense>> GetExpensesByBudgetAsync(int budgetId)
        {
            var expenses = await _db.GetAllAsync<Expense>();
            return expenses.Where(e => e.BudgetId == budgetId).ToList();
        }

        //  Récupérer une dépense par ID
        public Task<Expense> GetByIdAsync(int id)
            => _db.GetByIdAsync<Expense>(id);

        //  Ajouter une dépense
        public async Task<int> AddExpenseAsync(Expense expense)
        {
            if (expense.Amount <= 0)
                throw new Exception("Le montant de la dépense doit être supérieur à 0.");

            expense.CreatedAt = DateTime.Now;
            return await _db.InsertAsync(expense);
        }

        //  Mettre à jour une dépense
        public Task<int> UpdateExpenseAsync(Expense expense)
            => _db.UpdateAsync(expense);

        //  Supprimer une dépense
        public Task<int> DeleteExpenseAsync(Expense expense)
            => _db.DeleteAsync(expense);
    }
}
