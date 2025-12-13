using MoneyMate.Database;
using MoneyMate.Models;

namespace MoneyMate.Services
{
    public class BudgetCategoryService
    {
        private readonly MoneyMateContext _db;

        public BudgetCategoryService(MoneyMateContext db)
        {
            _db = db;
        }

        // ------------------------------------------
        // 🔹 Récupérer toutes les catégories d’un budget
        // ------------------------------------------
        public async Task<List<(BudgetCategory pivot, Category category)>> GetCategoriesForBudgetAsync(int budgetId)
        {
            var pivots = await _db.GetAllAsync<BudgetCategory>();
            var categories = await _db.GetAllAsync<Category>();

            var filtered = pivots.Where(p => p.BudgetId == budgetId).ToList();

            var result = filtered
                .Join(
                    categories,
                    pivot => pivot.CategoryId,
                    cat => cat.Id,
                    (pivot, cat) => (pivot, cat)
                )
                .ToList();

            return result;
        }

        // ------------------------------------------
        // 🔹 Ajouter une catégorie à un budget
        // ------------------------------------------
        public async Task<int> AddCategoryToBudgetAsync(int budgetId, int categoryId, double percentage, string colorHex = "#CCCCCC")
        {
            // 1. Charger le budget
            var budget = await _db.GetByIdAsync<Budget>(budgetId);
            if (budget == null)
                throw new Exception("Budget introuvable.");

            // 2. Calcul du montant alloué
            var allocatedAmount = budget.TotalAmount * (percentage / 100.0);

            // 3. Vérifier si la catégorie est déjà assignée à ce budget
            var existing = await _db.GetAllAsync<BudgetCategory>();
            if (existing.Any(p => p.BudgetId == budgetId && p.CategoryId == categoryId))
                throw new Exception("Cette catégorie est déjà assignée à ce budget.");

            // 4. Création
            var pivot = new BudgetCategory
            {
                BudgetId = budgetId,
                CategoryId = categoryId,
                AllocatedAmount = allocatedAmount,
                SpentAmount = 0,
                ColorHex = colorHex
            };

            return await _db.InsertAsync(pivot);
        }

        // ------------------------------------------
        // 🔹 Mettre à jour une catégorie d’un budget
        // ------------------------------------------
        public async Task<int> UpdateBudgetCategoryAsync(BudgetCategory pivot, double newPercentage)
        {
            var budget = await _db.GetByIdAsync<Budget>(pivot.BudgetId);
            if (budget == null)
                throw new Exception("Budget introuvable.");

            // recalcul du montant
            pivot.AllocatedAmount = budget.TotalAmount * (newPercentage / 100.0);

            return await _db.UpdateAsync(pivot);
        }

        // ------------------------------------------
        // 🔹 Supprimer une catégorie d’un budget
        // ------------------------------------------
        public Task<int> DeleteBudgetCategoryAsync(BudgetCategory pivot)
            => _db.DeleteAsync(pivot);

        // ------------------------------------------
        // 🔹 Ajouter une dépense à une BudgetCategory
        // ------------------------------------------
        public async Task AddExpenseToBudgetCategoryAsync(int pivotId, double amount)
        {
            var pivot = await _db.GetByIdAsync<BudgetCategory>(pivotId);
            if (pivot == null)
                throw new Exception("Catégorie introuvable pour ce budget.");

            pivot.SpentAmount += amount;

            // sécurité
            if (pivot.SpentAmount > pivot.AllocatedAmount)
                pivot.SpentAmount = pivot.AllocatedAmount;

            await _db.UpdateAsync(pivot);
        }
    }
}
