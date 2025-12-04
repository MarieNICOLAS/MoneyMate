using MoneyMate.Database;
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
        private readonly MoneyMateContext _db;

        public BudgetService(MoneyMateContext db)
        {
            _db = db ;
        }

        // 🔹 Récupérer tous les budgets
        public Task<List<Budget>> GetBudgetsAsync()
        {
            return _db.GetAllAsync<Budget>();
        }

        // 🔹 Récupérer un budget par ID
        public Task<Budget> GetBudgetByIdAsync(int id)
        {
            return _db.GetByIdAsync<Budget>(id);
        }

        // 🔹 Récupérer les budgets d’un utilisateur
        public async Task<List<Budget>> GetBudgetsByUserAsync(int userId)
        {
            var budgets = await _db.GetAllAsync<Budget>();
            return budgets.Where(b => b.UserId == userId).ToList();
        }

        // 🔹 Ajouter un budget
        public async Task<bool> AddBudgetAsync(Budget budget)
        {
            if (budget == null)
                throw new ArgumentNullException(nameof(budget));

            // Vérifier si un budget existe déjà pour le même utilisateur / mois / année
            var budgets = await _db.GetAllAsync<Budget>();
            var existing = budgets.FirstOrDefault(b =>
                b.UserId == budget.UserId &&
                b.Month == budget.Month &&
                b.Year == budget.Year);

            if (existing != null)
                return false; // doublon

            await _db.InsertAsync(budget);
            return true;
        }

        // 🔹 Modifier un budget
        public async Task<bool> UpdateBudgetAsync(Budget budget)
        {
            if (budget == null)
                throw new ArgumentNullException(nameof(budget));

            var result = await _db.UpdateAsync(budget);
            return result > 0;
        }

        // 🔹 Supprimer un budget
        public async Task<bool> DeleteBudgetAsync(Budget budget)
        {
            if (budget == null)
                throw new ArgumentNullException(nameof(budget));

            var result = await _db.DeleteAsync(budget);
            return result > 0;
        }

        // 🔹 Supprimer tous les budgets (optionnel)
        public async Task<int> ClearBudgetsAsync()
        {
            var budgets = await _db.GetAllAsync<Budget>();
            int count = 0;
            foreach (var b in budgets)
            {
                count += await _db.DeleteAsync(b);
            }
            return count;
        }
    }
}
