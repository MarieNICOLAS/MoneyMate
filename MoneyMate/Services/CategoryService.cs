using MoneyMate.Database;
using MoneyMate.Models;

namespace MoneyMate.Services
{
    public class CategoryService
    {
        private readonly MoneyMateContext _db;

        public CategoryService(MoneyMateContext db)
        {
            _db = db;
        }

        /*---------------------------------------------------------
         * 1️⃣  CATEGORIES (CRUD)
         *---------------------------------------------------------*/

        // Récupérer toutes les catégories existantes
        public Task<List<Category>> GetAllCategoriesAsync()
            => _db.GetAllAsync<Category>();


        // Récupérer une catégorie par ID
        public Task<Category> GetByIdAsync(int id)
            => _db.GetByIdAsync<Category>(id);


        // Ajouter une catégorie (sans budget obligatoire)
        public Task<int> AddCategoryAsync(Category category)
            => _db.InsertAsync(category);


        // Mettre à jour une catégorie
        public Task<int> UpdateCategoryAsync(Category category)
            => _db.UpdateAsync(category);


        // Supprimer une catégorie + ses liens BudgetCategory
        public async Task<int> DeleteCategoryAsync(int categoryId)
        {
            // Supprimer les liens d'association
            var links = await _db.GetAllAsync<BudgetCategory>();
            var toRemove = links.Where(bc => bc.CategoryId == categoryId);

            foreach (var link in toRemove)
                await _db.DeleteAsync(link);

            // Supprimer la catégorie
            return await _db.DeleteAsync(new Category { Id = categoryId });
        }


        /*---------------------------------------------------------
         * 2️⃣  GESTION DES LIENS BUDGET ↔ CATEGORY
         *---------------------------------------------------------*/

        // Associer une catégorie à un budget
        public async Task AddCategoryToBudgetAsync(int categoryId, int budgetId)
        {
            var existing = await GetCategoriesByBudgetAsync(budgetId);

            if (existing.Any(c => c.Id == categoryId))
                return; // déjà lié

            var link = new BudgetCategory
            {
                BudgetId = budgetId,
                CategoryId = categoryId
            };

            await _db.InsertAsync(link);
        }


        // Retirer une catégorie d’un budget
        public async Task RemoveCategoryFromBudgetAsync(int categoryId, int budgetId)
        {
            var links = await _db.GetAllAsync<BudgetCategory>();
            var link = links.FirstOrDefault(bc =>
                bc.CategoryId == categoryId && bc.BudgetId == budgetId);

            if (link != null)
                await _db.DeleteAsync(link);
        }


        /*---------------------------------------------------------
         * 3️⃣  QUERIES AVANCÉES
         *---------------------------------------------------------*/

        // Récupérer les catégories d’un budget
        public async Task<List<Category>> GetCategoriesByBudgetAsync(int budgetId)
        {
            var categories = await _db.GetAllAsync<Category>();
            var links = await _db.GetAllAsync<BudgetCategory>();

            var filteredIds = links
                .Where(l => l.BudgetId == budgetId)
                .Select(l => l.CategoryId);

            return categories
                .Where(c => filteredIds.Contains(c.Id))
                .ToList();
        }


        // Récupérer les budgets d’une catégorie
        public async Task<List<Budget>> GetBudgetsOfCategoryAsync(int categoryId)
        {
            var budgets = await _db.GetAllAsync<Budget>();
            var links = await _db.GetAllAsync<BudgetCategory>();

            var matchedIds = links
                .Where(l => l.CategoryId == categoryId)
                .Select(l => l.BudgetId);

            return budgets
                .Where(b => matchedIds.Contains(b.Id))
                .ToList();
        }
    }
}
