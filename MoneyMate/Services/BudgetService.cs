using MoneyMate.Database;
using MoneyMate.Models;

namespace MoneyMate.Services
{
    public class BudgetService
    {
        private readonly MoneyMateContext _db;
        private readonly BudgetCategoryService _pivotService;
        private readonly CategoryService _categoryService;

        public BudgetService(MoneyMateContext db)
        {
            _db = db;
            _pivotService = new BudgetCategoryService(db);
            _categoryService = new CategoryService(db);
        }

        // -----------------------------------------------------------
        // 🔹 Récupérer tous les budgets (triés par année/mois)
        // -----------------------------------------------------------
        public async Task<List<Budget>> GetBudgetsAsync()
        {
            var budgets = await _db.GetAllAsync<Budget>();
            return budgets
                .OrderByDescending(b => b.Year)
                .ThenByDescending(b => b.Month)
                .ToList();
        }

        // -----------------------------------------------------------
        // 🔹 Récupérer un budget simple
        // -----------------------------------------------------------
        public Task<Budget> GetBudgetByIdAsync(int id)
        {
            return _db.GetByIdAsync<Budget>(id);
        }

        // -----------------------------------------------------------
        // 🔹 Récupérer les budgets d’un utilisateur
        // -----------------------------------------------------------
        public async Task<List<Budget>> GetBudgetsByUserAsync(int userId)
        {
            var budgets = await _db.GetAllAsync<Budget>();
            return budgets.Where(b => b.UserId == userId).ToList();
        }

        // -----------------------------------------------------------
        // 🔹 Récupérer un budget AVEC ses catégories (pivot + category)
        // -----------------------------------------------------------
        public async Task<(Budget budget, List<(BudgetCategory pivot, Category category)> categories)>
            GetBudgetWithCategoriesAsync(int budgetId)
        {
            var budget = await _db.GetByIdAsync<Budget>(budgetId);
            if (budget == null)
                throw new Exception("Budget introuvable.");

            // jointure pivot + category
            var categories = await _pivotService.GetCategoriesForBudgetAsync(budgetId);

            // calcul des dépenses totales
            budget.SpentAmount = categories.Sum(c => c.pivot.SpentAmount);

            return (budget, categories);
        }

        // -----------------------------------------------------------
        // 🔹 Ajouter un budget
        // -----------------------------------------------------------
        public async Task<bool> AddBudgetAsync(Budget budget)
        {
            if (budget == null)
                throw new ArgumentNullException(nameof(budget));

            // Vérifier doublon mois/année/user
            var list = await GetBudgetsByUserAsync(budget.UserId);
            if (list.Any(b => b.Month == budget.Month && b.Year == budget.Year))
                return false;

            await _db.InsertAsync(budget);
            return true;
        }

        // -----------------------------------------------------------
        // 🔹 Modifier un budget (montant, etc.)
        // -----------------------------------------------------------
        public async Task<bool> UpdateBudgetAsync(Budget budget)
        {
            if (budget == null)
                throw new ArgumentNullException(nameof(budget));

            return await _db.UpdateAsync(budget) > 0;
        }

        // -----------------------------------------------------------
        // 🔹 Supprimer un budget (avec ses pivots)
        // -----------------------------------------------------------
        public async Task<bool> DeleteBudgetAsync(Budget budget)
        {
            if (budget == null)
                throw new ArgumentNullException(nameof(budget));

            // supprimer pivots associés
            var pivots = await _pivotService.GetCategoriesForBudgetAsync(budget.Id);
            foreach (var (pivot, _) in pivots)
                await _pivotService.DeleteBudgetCategoryAsync(pivot);

            // supprimer le budget
            return await _db.DeleteAsync(budget) > 0;
        }

        // -----------------------------------------------------------
        // 🔹 Ajouter une catégorie à un budget
        // -----------------------------------------------------------
        public Task<int> AddCategoryToBudgetAsync(int budgetId, int categoryId, double percentage, string colorHex)
        {
            return _pivotService.AddCategoryToBudgetAsync(budgetId, categoryId, percentage, colorHex);
        }

        // -----------------------------------------------------------
        // 🔹 Modifier pourcentage (recalcul automatique)
        // -----------------------------------------------------------
        public Task<int> UpdateBudgetCategoryPercentageAsync(BudgetCategory pivot, double newPercentage)
        {
            return _pivotService.UpdateBudgetCategoryAsync(pivot, newPercentage);
        }

        // -----------------------------------------------------------
        // 🔹 Supprimer une catégorie du budget
        // -----------------------------------------------------------
        public Task<int> RemoveCategoryFromBudgetAsync(BudgetCategory pivot)
        {
            return _pivotService.DeleteBudgetCategoryAsync(pivot);
        }
    }
}
