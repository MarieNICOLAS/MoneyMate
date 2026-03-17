using MoneyMate.Database;
using MoneyMate.Models;

namespace MoneyMate.Services
{
    public class ExpenseService
    {
        private readonly MoneyMateContext _db;
        private readonly BudgetCategoryService _pivotService;

        public ExpenseService(MoneyMateContext db)
        {
            _db = db;
            _pivotService = new BudgetCategoryService(db);
        }

        // -----------------------------------------
        // 🔹 AJOUT D’UNE DÉPENSE
        // -----------------------------------------
        public async Task<int> AddExpenseAsync(Expense expense)
        {
            if (expense.Amount <= 0)
                throw new Exception("Le montant doit être supérieur à 0.");

            // 1️⃣ Enregistrer la dépense en base
            var id = await _db.InsertAsync(expense);

            // 2️⃣ Mettre à jour le pivot (BudgetCategory)
            await _pivotService.AddExpenseToBudgetCategoryAsync(
                expense.BudgetCategoryId,
                expense.Amount
            );

            return id;
        }

        // -----------------------------------------
        // 🔹 SUPPRESSION D’UNE DÉPENSE
        // -----------------------------------------
        public async Task DeleteExpenseAsync(Expense expense)
        {
            // Diminuer le SpentAmount de la catégorie
            var pivot = await _db.GetByIdAsync<BudgetCategory>(expense.BudgetCategoryId);

            if (pivot != null)
            {
                pivot.SpentAmount -= expense.Amount;
                if (pivot.SpentAmount < 0)
                    pivot.SpentAmount = 0;

                await _db.UpdateAsync(pivot);
            }

            // Supprimer la dépense
            await _db.DeleteAsync(expense);
        }

        // -----------------------------------------
        // 🔹 MODIFICATION D’UNE DÉPENSE
        // -----------------------------------------
        public async Task UpdateExpenseAsync(Expense oldExpense, Expense updatedExpense)
        {
            // 1️⃣ Calcul différence montant
            double difference = updatedExpense.Amount - oldExpense.Amount;

            // 2️⃣ Gérer l’impact sur la BudgetCategory
            var pivot = await _db.GetByIdAsync<BudgetCategory>(oldExpense.BudgetCategoryId);

            if (pivot == null)
                throw new Exception("Impossible de mettre à jour la catégorie du budget.");

            pivot.SpentAmount += difference;

            // Sécurité
            if (pivot.SpentAmount < 0) pivot.SpentAmount = 0;
            if (pivot.SpentAmount > pivot.AllocatedAmount) pivot.SpentAmount = pivot.AllocatedAmount;

            await _db.UpdateAsync(pivot);

            // 3️⃣ Mettre à jour l’Expense
            await _db.UpdateAsync(updatedExpense);
        }

        // -----------------------------------------
        // 🔹 RÉCUPÉRER TOUTES LES DÉPENSES D’UN BUDGET
        // -----------------------------------------
        public async Task<List<Expense>> GetExpensesForBudgetAsync(int budgetId)
        {
            var all = await _db.GetAllAsync<Expense>();
            return all.Where(e => e.BudgetId == budgetId)
                      .OrderByDescending(e => e.Date)
                      .ToList();
        }

        // -----------------------------------------
        // 🔹 RÉCUPÉRER LES DÉPENSES PAR BUDGETCATEGORY
        // -----------------------------------------
        public async Task<List<Expense>> GetExpensesForCategoryAsync(int pivotId)
        {
            var all = await _db.GetAllAsync<Expense>();
            return all.Where(e => e.BudgetCategoryId == pivotId)
                      .OrderByDescending(e => e.Date)
                      .ToList();
        }

        // -----------------------------------------
        // 🔹 RÉCUPÉRER UNE DÉPENSE PAR ID
        // -----------------------------------------
        public Task<Expense> GetExpenseByIdAsync(int id)
            => _db.GetByIdAsync<Expense>(id);
    }
}
