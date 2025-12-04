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

        //  Récupérer toutes les catégories
        public Task<List<Category>> GetCategoriesAsync()
            => _db.GetAllAsync<Category>();

        //  Récupérer les catégories d'un budget
        public async Task<List<Category>> GetCategoriesByBudgetAsync(int budgetId)
        {
            var categories = await _db.GetAllAsync<Category>();
            return categories.Where(c => c.BudgetId == budgetId).ToList();
        }

        //  Récupérer une catégorie par ID
        public Task<Category> GetByIdAsync(int id)
            => _db.GetByIdAsync<Category>(id);

        //  Ajouter une nouvelle catégorie
        public async Task<int> AddCategoryAsync(Category category)
        {
            // Empêcher 2 catégories avec le même nom dans le même budget
            var categories = await GetCategoriesByBudgetAsync(category.BudgetId);
            if (categories.Any(c => c.Name == category.Name))
                throw new Exception("Une catégorie avec ce nom existe déjà dans ce budget.");

            return await _db.InsertAsync(category);
        }

        //  Mettre à jour une catégorie
        public Task<int> UpdateCategoryAsync(Category category)
            => _db.UpdateAsync(category);

        //  Supprimer une catégorie
        public Task<int> DeleteCategoryAsync(Category category)
            => _db.DeleteAsync(category);

        //  Supprimer toutes les catégories d’un budget (optionnel)
        public async Task<int> DeleteCategoriesByBudgetAsync(int budgetId)
        {
            var categories = await GetCategoriesByBudgetAsync(budgetId);
            int count = 0;

            foreach (var c in categories)
                count += await _db.DeleteAsync(c);

            return count;
        }
    }
}


