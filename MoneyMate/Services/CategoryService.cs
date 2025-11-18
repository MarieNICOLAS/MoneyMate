using MoneyMate.Database;
using MoneyMate.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MoneyMate.Services
{
    /// <summary>
    /// Service responsable de la gestion des catégories (CRUD).
    /// </summary>
    public class CategoryService
    {
        private readonly MoneyMateContext _context;

        public CategoryService(MoneyMateContext? context = null)
        {
            _context = context ?? new MoneyMateContext();
        }

        public Task<List<Category>> GetCategoriesAsync()
            => _context.GetAllAsync<Category>();

        public Task<Category> GetCategoryByIdAsync(int id)
            => _context.GetByIdAsync<Category>(id);

        public Task<int> AddCategoryAsync(Category category)
            => _context.InsertAsync(category);

        public Task<int> UpdateCategoryAsync(Category category)
            => _context.UpdateAsync(category);

        public Task<int> DeleteCategoryAsync(Category category)
            => _context.DeleteAsync(category);
    }
}


