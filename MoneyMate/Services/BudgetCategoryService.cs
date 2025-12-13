using MoneyMate.Database;
using MoneyMate.Models;

namespace MoneyMate.Services
{
    /// <summary>
    /// Service de gestion de la table de liaison BudgetCategory (Budget <-> Category).
    /// Permet :
    /// - d'obtenir les catégories d'un budget
    /// - d'obtenir les budgets associés à une catégorie
    /// - d'ajouter/modifier/supprimer une liaison
    /// - de recalculer automatiquement les montants limites
    /// </summary>
    public class BudgetCategoryService
    {
        private readonly MoneyMateContext _db;

        public BudgetCategoryService(MoneyMateContext db)
        {
            _db = db;
        }

        // ============================================================================
        // 🔹 1) CRUD DE BASE
        // ============================================================================

        /// <summary>
        /// Récupère tous les liens BudgetCategory.
        /// </summary>
        public Task<List<BudgetCategory>> GetAllAsync()
            => _db.GetAllAsync<BudgetCategory>();

        /// <summary>
        /// Récupère un lien par ID.
        /// </summary>
        public Task<BudgetCategory> GetByIdAsync(int id)
            => _db.GetByIdAsync<BudgetCategory>(id);

        /// <summary>
        /// Ajoute un lien Budget <-> Category avec recalcul automatique du montant limite.
        /// </summary>
        public async Task<int> AddAsync(BudgetCategory link, double budgetTotalAmount)
        {
            // Recalcul du montant limite
            link.RecalculateLimit(budgetTotalAmount);

            return await _db.InsertAsync(link);
        }

        /// <summary>
        /// Met à jour un lien existant avec recalcul du montant limite.
        /// </summary>
        public async Task<int> UpdateAsync(BudgetCategory link, double budgetTotalAmount)
        {
            // Recalcul du montant limite
            link.RecalculateLimit(budgetTotalAmount);

            return await _db.UpdateAsync(link);
        }

        /// <summary>
        /// Supprime un lien BudgetCategory.
        /// </summary>
        public Task<int> DeleteAsync(BudgetCategory link)
            => _db.DeleteAsync(link);


        // ============================================================================
        // 🔹 2) QUERIES AVANCÉES
        // ============================================================================

        /// <summary>
        /// Récupère toutes les catégories associées à un budget donné.
        /// </summary>
        public async Task<List<Category>> GetCategoriesForBudgetAsync(int budgetId)
        {
            var allCategories = await _db.GetAllAsync<Category>();
            var links = await GetAllAsync();

            var categoryIds = links
                .Where(l => l.BudgetId == budgetId)
                .Select(l => l.CategoryId)
                .ToHashSet();

            return allCategories
                .Where(c => categoryIds.Contains(c.Id))
                .ToList();
        }

        /// <summary>
        /// Récupère tous les budgets associés à une catégorie donnée.
        /// </summary>
        public async Task<List<Budget>> GetBudgetsForCategoryAsync(int categoryId)
        {
            var allBudgets = await _db.GetAllAsync<Budget>();
            var links = await GetAllAsync();

            var budgetIds = links
                .Where(l => l.CategoryId == categoryId)
                .Select(l => l.BudgetId)
                .ToHashSet();

            return allBudgets
                .Where(b => budgetIds.Contains(b.Id))
                .ToList();
        }

        /// <summary>
        /// Récupère un lien spécifique Budget-Category.
        /// </summary>
        public async Task<BudgetCategory?> GetLinkAsync(int budgetId, int categoryId)
        {
            var links = await GetAllAsync();
            return links.FirstOrDefault(l =>
                l.BudgetId == budgetId &&
                l.CategoryId == categoryId);
        }

        /// <summary>
        /// Ajoute une liaison Budget <-> Category (si elle n'existe pas déjà).
        /// </summary>
        public async Task<bool> AddBudgetCategoryAsync(int budgetId, int categoryId, double percentage = 10)
        {
            var existing = await GetLinkAsync(budgetId, categoryId);
            if (existing != null)
                return false;

            // Récupère le budget pour calculer le montant limite
            var budget = await _db.GetByIdAsync<Budget>(budgetId);
            if (budget == null)
                throw new InvalidOperationException($"Budget {budgetId} introuvable");

            var link = new BudgetCategory
            {
                BudgetId = budgetId,
                CategoryId = categoryId,
                Percentage = percentage,
                SpentAmount = 0
            };

            await AddAsync(link, budget.TotalAmount);
            return true;
        }

        /// <summary>
        /// Supprime toutes les liaisons pour une catégorie donnée.
        /// </summary>
        public async Task DeleteLinksForCategoryAsync(int categoryId)
        {
            var links = await GetAllAsync();
            var toDelete = links.Where(l => l.CategoryId == categoryId).ToList();

            foreach (var link in toDelete)
            {
                await DeleteAsync(link);
            }
        }

        /// <summary>
        /// Supprime toutes les liaisons pour un budget donné.
        /// </summary>
        public async Task DeleteLinksForBudgetAsync(int budgetId)
        {
            var links = await GetAllAsync();
            var toDelete = links.Where(l => l.BudgetId == budgetId).ToList();

            foreach (var link in toDelete)
            {
                await DeleteAsync(link);
            }
        }

        /// <summary>
        /// Recalcule tous les montants limites pour un budget donné
        /// (utile après modification du montant total du budget).
        /// </summary>
        public async Task RecalculateAllLimitsForBudgetAsync(int budgetId)
        {
            var budget = await _db.GetByIdAsync<Budget>(budgetId);
            if (budget == null)
                return;

            var links = await GetAllAsync();
            var budgetLinks = links.Where(l => l.BudgetId == budgetId).ToList();

            foreach (var link in budgetLinks)
            {
                await UpdateAsync(link, budget.TotalAmount);
            }
        }
    }
}