using MoneyMate.Models;
using SQLite;

namespace MoneyMate.Database
{
    public class MoneyMateContext
    {
        private readonly SQLiteAsyncConnection _database;

        public MoneyMateContext()
        {
            var dbPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "MoneyMate.db3"
            );

            _database = new SQLiteAsyncConnection(dbPath);
        }

        // -----------------------------------------
        //   INITIALISATION DE LA BASE
        // -----------------------------------------
        public async Task InitializeAsync()
        {
            // ⚠️ ORDRE IMPORTANT : les tables référencées doivent exister AVANT
            await _database.CreateTableAsync<User>();
            await _database.CreateTableAsync<Budget>();
            await _database.CreateTableAsync<Category>();

            // 🆕 Nouvelle table pivot
            await _database.CreateTableAsync<BudgetCategory>();

            await _database.CreateTableAsync<Expense>();
            await _database.CreateTableAsync<Alert>();
        }

        // -----------------------------------------
        //   CRUD GÉNÉRIQUE
        // -----------------------------------------
        public Task<int> InsertAsync<T>(T entity) where T : new()
            => _database.InsertAsync(entity);

        public Task<int> UpdateAsync<T>(T entity) where T : new()
            => _database.UpdateAsync(entity);

        public Task<int> DeleteAsync<T>(T entity) where T : new()
            => _database.DeleteAsync(entity);

        public Task<List<T>> GetAllAsync<T>() where T : new()
            => _database.Table<T>().ToListAsync();

        public Task<T> GetByIdAsync<T>(int id) where T : new()
            => _database.FindAsync<T>(id);

        // -----------------------------------------
        //   RESET COMPLET DE LA BASE (DEV MODE)
        // -----------------------------------------
        public async Task ClearDatabaseAsync()
        {
            await _database.DeleteAllAsync<User>();
            await _database.DeleteAllAsync<Budget>();
            await _database.DeleteAllAsync<Category>();
            await _database.DeleteAllAsync<BudgetCategory>();
            await _database.DeleteAllAsync<Expense>();
            await _database.DeleteAllAsync<Alert>();
        }
    }
}
