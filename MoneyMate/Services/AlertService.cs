using MoneyMate.Database;
using MoneyMate.Models;
using System.Diagnostics;

namespace MoneyMate.Services
{
    public class AlertService
    {
        private readonly MoneyMateContext _db;

        public AlertService(MoneyMateContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Vérifie et crée une alerte si le seuil du budget est atteint
        /// </summary>
        public async Task CheckBudgetThresholdAsync(int budgetId, int userId, double threshold = 0.8)
        {
            var budget = await _db.GetByIdAsync<Budget>(budgetId);
            if (budget == null) return;

            double percentageUsed = budget.TotalAmount > 0 
                ? budget.SpentAmount / budget.TotalAmount 
                : 0;

            // Si seuil dépassé et pas d'alerte existante
            if (percentageUsed >= threshold)
            {
                var existingAlerts = await GetAlertsByBudgetAsync(budgetId);
                var hasAlert = existingAlerts.Any(a => 
                    a.Type == "Global" && 
                    a.ReadAt == null);

                if (!hasAlert)
                {
                    var alert = Alert.CreateThresholdAlert(
                        userId, 
                        percentageUsed * 100, 
                        "Global", 
                        budgetId
                    );
                    
                    await _db.InsertAsync(alert);
                    Debug.WriteLine($"?? Alerte créée : {alert.Message}");
                }
            }
        }

        /// <summary>
        /// Vérifie et crée une alerte si le seuil d'une catégorie est atteint
        /// </summary>
        public async Task CheckCategoryThresholdAsync(int categoryId, int userId, double threshold = 0.8)
        {
            var category = await _db.GetByIdAsync<Category>(categoryId);
            if (category == null || category.AllocatedAmount == 0) return;

            double percentageUsed = category.SpentAmount / category.AllocatedAmount;

            if (percentageUsed >= threshold)
            {
                var existingAlerts = await GetAlertsByCategoryAsync(categoryId);
                var hasAlert = existingAlerts.Any(a => 
                    a.Type == "Category" && 
                    a.ReadAt == null);

                if (!hasAlert)
                {
                    var alert = Alert.CreateThresholdAlert(
                        userId, 
                        percentageUsed * 100, 
                        category.Name, 
                        category.BudgetId,
                        categoryId
                    );
                    
                    await _db.InsertAsync(alert);
                    Debug.WriteLine($"?? Alerte catégorie créée : {alert.Message}");
                }
            }
        }

        /// <summary>
        /// Récupère toutes les alertes non lues d'un utilisateur
        /// </summary>
        public async Task<List<Alert>> GetUnreadAlertsAsync(int userId)
        {
            var alerts = await _db.GetAllAsync<Alert>();
            return alerts.Where(a => a.UserId == userId && !a.IsRead)
                        .OrderByDescending(a => a.CreatedAt)
                        .ToList();
        }

        /// <summary>
        /// Récupère les alertes liées à un budget
        /// </summary>
        public async Task<List<Alert>> GetAlertsByBudgetAsync(int budgetId)
        {
            var alerts = await _db.GetAllAsync<Alert>();
            return alerts.Where(a => a.BudgetId == budgetId).ToList();
        }

        /// <summary>
        /// Récupère les alertes liées à une catégorie
        /// </summary>
        public async Task<List<Alert>> GetAlertsByCategoryAsync(int categoryId)
        {
            var alerts = await _db.GetAllAsync<Alert>();
            return alerts.Where(a => a.CategoryId == categoryId).ToList();
        }

        /// <summary>
        /// Marque une alerte comme lue
        /// </summary>
        public async Task MarkAsReadAsync(Alert alert)
        {
            alert.MarkAsRead();
            await _db.UpdateAsync(alert);
        }

        /// <summary>
        /// Marque toutes les alertes d'un utilisateur comme lues
        /// </summary>
        public async Task MarkAllAsReadAsync(int userId)
        {
            var unreadAlerts = await GetUnreadAlertsAsync(userId);
            foreach (var alert in unreadAlerts)
            {
                alert.MarkAsRead();
                await _db.UpdateAsync(alert);
            }
        }

        /// <summary>
        /// Supprime une alerte
        /// </summary>
        public async Task DeleteAlertAsync(Alert alert)
        {
            await _db.DeleteAsync(alert);
        }

        /// <summary>
        /// Supprime toutes les alertes d'un utilisateur
        /// </summary>
        public async Task DeleteAllAlertsAsync(int userId)
        {
            var alerts = await _db.GetAllAsync<Alert>();
            var userAlerts = alerts.Where(a => a.UserId == userId).ToList();
            
            foreach (var alert in userAlerts)
            {
                await _db.DeleteAsync(alert);
            }
        }
    }
}