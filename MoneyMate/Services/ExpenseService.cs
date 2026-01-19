using MoneyMate.Database;
using MoneyMate.Models;

namespace MoneyMate.Services
{
    public class ExpenseService
    {
        private readonly MoneyMateContext _db;
        private readonly AlertService _alertService;

        // ✅ Injection de AlertService
        public ExpenseService(MoneyMateContext db, AlertService alertService)
        {
            _db = db;
            _alertService = alertService;
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

        //  Ajouter une dépense avec vérification des alertes
        public async Task<int> AddExpenseAsync(Expense expense, int userId)
        {
            if (expense.Amount <= 0)
                throw new Exception("Le montant de la dépense doit être supérieur à 0.");

            expense.CreatedAt = DateTime.Now;
            
            // ✅ 1️⃣ Insérer la dépense
            var result = await _db.InsertAsync(expense);

            // ✅ 2️⃣ Mettre à jour le budget
            var budget = await _db.GetByIdAsync<Budget>(expense.BudgetId);
            if (budget != null)
            {
                budget.SpentAmount += expense.Amount;
                await _db.UpdateAsync(budget);
                
                // ✅ Vérifier les alertes budget
                await _alertService.CheckBudgetThresholdAsync(budget.Id, userId);
            }

            // ✅ 3️⃣ Mettre à jour la catégorie
            var category = await _db.GetByIdAsync<Category>(expense.CategoryId);
            if (category != null)
            {
                category.SpentAmount += expense.Amount;
                await _db.UpdateAsync(category);
                
                // ✅ Vérifier les alertes catégorie
                await _alertService.CheckCategoryThresholdAsync(category.Id, userId);
            }

            return result;
        }

        //  Mettre à jour une dépense
        public async Task<int> UpdateExpenseAsync(Expense expense, int userId)
        {
            // Récupérer l'ancienne dépense pour calculer la différence
            var oldExpense = await _db.GetByIdAsync<Expense>(expense.Id);
            if (oldExpense == null)
                throw new Exception("Dépense introuvable.");

            var difference = expense.Amount - oldExpense.Amount;

            // Mettre à jour la dépense
            var result = await _db.UpdateAsync(expense);

            // Mettre à jour le budget
            var budget = await _db.GetByIdAsync<Budget>(expense.BudgetId);
            if (budget != null)
            {
                budget.SpentAmount += difference;
                await _db.UpdateAsync(budget);
                
                // ✅ Vérifier les alertes
                await _alertService.CheckBudgetThresholdAsync(budget.Id, userId);
            }

            // Mettre à jour la catégorie
            var category = await _db.GetByIdAsync<Category>(expense.CategoryId);
            if (category != null)
            {
                category.SpentAmount += difference;
                await _db.UpdateAsync(category);
                
                // ✅ Vérifier les alertes
                await _alertService.CheckCategoryThresholdAsync(category.Id, userId);
            }

            return result;
        }

        //  Supprimer une dépense
        public async Task<int> DeleteExpenseAsync(Expense expense)
        {
            // Mettre à jour le budget avant suppression
            var budget = await _db.GetByIdAsync<Budget>(expense.BudgetId);
            if (budget != null)
            {
                budget.SpentAmount -= expense.Amount;
                if (budget.SpentAmount < 0) budget.SpentAmount = 0; // Sécurité
                await _db.UpdateAsync(budget);
            }

            // Mettre à jour la catégorie avant suppression
            var category = await _db.GetByIdAsync<Category>(expense.CategoryId);
            if (category != null)
            {
                category.SpentAmount -= expense.Amount;
                if (category.SpentAmount < 0) category.SpentAmount = 0; // Sécurité
                await _db.UpdateAsync(category);
            }

            return await _db.DeleteAsync(expense);
        }
    }
}
