using MoneyMate.Database;
using MoneyMate.Models;

namespace MoneyMate.Services
{
    public class BudgetService
    {
        private readonly MoneyMateContext _db;

        public BudgetService(MoneyMateContext db)
        {
            _db = db;
        }

        // ============================================================================
        // 🔹 1) BUDGETS : CRUD CLASSIQUE
        // ============================================================================

        // ➤ Récupérer tous les budgets
        public Task<List<Budget>> GetBudgetsAsync()
            => _db.GetAllAsync<Budget>();

        // ➤ Récupérer un budget par ID
        public Task<Budget> GetBudgetByIdAsync(int id)
            => _db.GetByIdAsync<Budget>(id);

        // ➤ Récupérer budgets d’un utilisateur
        public async Task<List<Budget>> GetBudgetsByUserAsync(int userId)
        {
            var budgets = await _db.GetAllAsync<Budget>();
            return budgets.Where(b => b.UserId == userId).ToList();
        }

        // ➤ Ajouter un budget
        public async Task<bool> AddBudgetAsync(Budget budget)
        {
            if (budget == null)
                throw new ArgumentNullException(nameof(budget));

            // Pas de doublon user + mois + année
            var all = await _db.GetAllAsync<Budget>();
            bool exists = all.Any(b =>
                b.UserId == budget.UserId &&
                b.Month == budget.Month &&
                b.Year == budget.Year
            );

            if (exists)
                return false;

            await _db.InsertAsync(budget);
            return true;
        }

        // ➤ Modifier un budget
        public async Task<bool> UpdateBudgetAsync(Budget budget)
        {
            if (budget == null)
                throw new ArgumentNullException(nameof(budget));

            int result = await _db.UpdateAsync(budget);
            return result > 0;
        }

        // ➤ Supprimer un budget
        public async Task<bool> DeleteBudgetAsync(Budget budget)
        {
            if (budget == null)
                throw new ArgumentNullException(nameof(budget));

            int result = await _db.DeleteAsync(budget);
            return result > 0;
        }

        // ============================================================================
        // 🔹 2) ASSOCIATIONS BUDGET ↔ CATÉGORIE (table BudgetCategory)
        // ============================================================================

        // ➤ Associer une catégorie à un budget
        public async Task<bool> AddCategoryToBudgetAsync(int budgetId, int categoryId)
        {
            var existing = await GetLinkAsync(budgetId, categoryId);
            if (existing != null)
                return false; // déjà lié

            var link = new BudgetCategory
            {
                BudgetId = budgetId,
                CategoryId = categoryId
            };

            await _db.InsertAsync(link);
            return true;
        }

        // ➤ Désassocier une catégorie d’un budget
        public async Task<bool> RemoveCategoryFromBudgetAsync(int budgetId, int categoryId)
        {
            var existing = await GetLinkAsync(budgetId, categoryId);
            if (existing == null)
                return false;

            int result = await _db.DeleteAsync(existing);
            return result > 0;
        }

        // ➤ Récupérer toutes les catégories associées à un budget
        public async Task<List<Category>> GetCategoriesForBudgetAsync(int budgetId)
        {
            var links = await _db.GetAllAsync<BudgetCategory>();
            var categories = await _db.GetAllAsync<Category>();

            var categoryIds = links
                .Where(l => l.BudgetId == budgetId)
                .Select(l => l.CategoryId)
                .ToList();

            return categories
                .Where(c => categoryIds.Contains(c.Id))
                .ToList();
        }

        // ➤ Récupérer tous les budgets associés à une catégorie
        public async Task<List<Budget>> GetBudgetsForCategoryAsync(int categoryId)
        {
            var links = await _db.GetAllAsync<BudgetCategory>();
            var budgets = await _db.GetAllAsync<Budget>();

            var budgetIds = links
                .Where(l => l.CategoryId == categoryId)
                .Select(l => l.BudgetId)
                .ToList();

            return budgets
                .Where(b => budgetIds.Contains(b.Id))
                .ToList();
        }

        // ============================================================================
        // 🔹 3) Méthode interne : récupérer un lien unique
        // ============================================================================
        private async Task<BudgetCategory?> GetLinkAsync(int budgetId, int categoryId)
        {
            var links = await _db.GetAllAsync<BudgetCategory>();
            return links
                .FirstOrDefault(l => l.BudgetId == budgetId && l.CategoryId == categoryId);
        }
    }
}
