using MoneyMate.Models;
using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoneyMate.Services
{
    public class BudgetService
    {
        private readonly SQLiteAsyncConnection _database;

        public BudgetService(SQLiteAsyncConnection database)
        {
            _database = database;
        }

        // 🔹 Récupérer tous les budgets
        public Task<List<Budget>> GetBudgetsAsync()
        {
            return _database.Table<Budget>().ToListAsync();
        }

        // 🔹 Récupérer un budget spécifique
        public Task<Budget> GetBudgetByIdAsync(int id)
        {
            return _database.Table<Budget>()
                            .Where(b => b.Id == id)
                            .FirstOrDefaultAsync();
        }

        // 🔹 Récupérer les budgets d’un utilisateur
        public Task<List<Budget>> GetBudgetsByUserAsync(int userId)
        {
            return _database.Table<Budget>()
                            .Where(b => b.UserId == userId)
                            .ToListAsync();
        }

        // 🔹 Ajouter un budget
        public async Task<int> AddBudgetAsync(Budget budget)
        {
            // Empêche les doublons de mois/année pour le même utilisateur
            var existing = await _database.Table<Budget>()
                                          .Where(b => b.UserId == budget.UserId &&
                                                      b.Month == budget.Month &&
                                                      b.Year == budget.Year)
                                          .FirstOrDefaultAsync();

            if (existing != null)
                throw new System.Exception("Un budget pour ce mois existe déjà.");

            return await _database.InsertAsync(budget);
        }

        // 🔹 Modifier un budget
        public Task<int> UpdateBudgetAsync(Budget budget)
        {
            return _database.UpdateAsync(budget);
        }

        // 🔹 Supprimer un budget
        public Task<int> DeleteBudgetAsync(Budget budget)
        {
            return _database.DeleteAsync(budget);
        }

        // 🔹 Supprimer tous les budgets (optionnel)
        public Task<int> ClearBudgetsAsync()
        {
            return _database.DeleteAllAsync<Budget>();
        }
    }
}
