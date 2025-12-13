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

        // 🔹 Récupère toutes les catégories globales
        public Task<List<Category>> GetCategoriesAsync()
            => _db.GetAllAsync<Category>();

        // 🔹 Récupère une catégorie par son ID
        public Task<Category> GetByIdAsync(int id)
            => _db.GetByIdAsync<Category>(id);

        // 🔹 Ajoute une catégorie globale
        public async Task<int> AddCategoryAsync(Category category)
        {
            // Vérification : éviter les doublons globaux
            var categories = await GetCategoriesAsync();
            if (categories.Any(c => c.Name.Trim().ToLower() == category.Name.Trim().ToLower()))
                throw new Exception("Une catégorie portant ce nom existe déjà.");

            return await _db.InsertAsync(category);
        }

        // 🔹 Met à jour une catégorie
        public Task<int> UpdateCategoryAsync(Category category)
            => _db.UpdateAsync(category);

        // 🔹 Supprime une catégorie
        public Task<int> DeleteCategoryAsync(Category category)
            => _db.DeleteAsync(category);
    }
}
