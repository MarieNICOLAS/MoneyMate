using System.Collections.ObjectModel;
using Microsoft.Maui.Graphics;
using MoneyMate.Services;
using MoneyMate.Models;
using System.Diagnostics;
using CommunityToolkit.Mvvm.Input;

namespace MoneyMate.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        private readonly BudgetService _budgetService;
        private readonly CategoryService _categoryService;
        private readonly ExpenseService _expenseService;
        private readonly AlertService _alertService;

        // --- Champs privés ---
        private double totalBudget;
        private double totalSpent;
        private Budget currentBudget;
        private int unreadAlertsCount;

        // --- Propriétés bindées ---
        public double TotalBudget
        {
            get => totalBudget;
            set
            {
                if (SetProperty(ref totalBudget, value))
                {
                    OnPropertyChanged(nameof(CurrentBalance));
                    OnPropertyChanged(nameof(CurrentBalanceFormatted));
                    OnPropertyChanged(nameof(BudgetProgress));
                    OnPropertyChanged(nameof(BudgetProgressText));
                }
            }
        }

        public double TotalSpent
        {
            get => totalSpent;
            set
            {
                if (SetProperty(ref totalSpent, value))
                {
                    OnPropertyChanged(nameof(CurrentBalance));
                    OnPropertyChanged(nameof(CurrentBalanceFormatted));
                    OnPropertyChanged(nameof(BudgetProgress));
                    OnPropertyChanged(nameof(BudgetProgressText));
                }
            }
        }

        public int UnreadAlertsCount
        {
            get => unreadAlertsCount;
            set => SetProperty(ref unreadAlertsCount, value);
        }

        public double CurrentBalance => TotalBudget - TotalSpent;
        public string CurrentBalanceFormatted => $"{CurrentBalance:0.00} €";
        public double BudgetProgress => TotalBudget > 0 ? TotalSpent / TotalBudget : 0;
        public string BudgetProgressText => $"{TotalSpent:0.##} / {TotalBudget:0.##} €";

        public ObservableCollection<CategoryStat> Categories { get; set; }

        // --- Commandes ---
        public IAsyncRelayCommand RefreshCommand { get; }

        // ✅ Constructeur avec injection de dépendances
        public DashboardViewModel(
            BudgetService budgetService, 
            CategoryService categoryService, 
            ExpenseService expenseService,
            AlertService alertService)
        {
            _budgetService = budgetService;
            _categoryService = categoryService;
            _expenseService = expenseService;
            _alertService = alertService;

            Categories = new ObservableCollection<CategoryStat>();
            RefreshCommand = new AsyncRelayCommand(LoadDashboardDataAsync);

            // Chargement initial
            _ = LoadDashboardDataAsync();
        }

        // --- Méthodes ---
        public async Task LoadDashboardDataAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                // 1️⃣ Récupérer le budget du mois en cours
                var budgets = await _budgetService.GetBudgetsAsync();
                currentBudget = budgets.FirstOrDefault(b => b.IsForCurrentMonth());

                if (currentBudget == null)
                {
                    TotalBudget = 0;
                    TotalSpent = 0;
                    Categories.Clear();
                    UnreadAlertsCount = 0;
                    return;
                }

                // 2️⃣ Mettre à jour les totaux
                TotalBudget = currentBudget.TotalAmount;
                TotalSpent = currentBudget.SpentAmount;

                // 3️⃣ Charger les alertes non lues
                var alerts = await _alertService.GetUnreadAlertsAsync(currentBudget.UserId);
                UnreadAlertsCount = alerts.Count;

                // 4️⃣ Charger les catégories avec leurs dépenses
                var categories = await _categoryService.GetCategoriesByBudgetAsync(currentBudget.Id);
                var expenses = await _expenseService.GetExpensesByBudgetAsync(currentBudget.Id);

                Categories.Clear();
                foreach (var category in categories)
                {
                    var categoryExpenses = expenses.Where(e => e.CategoryId == category.Id).Sum(e => e.Amount);
                    
                    var trend = category.AllocatedAmount > 0 
                        ? ((categoryExpenses - category.AllocatedAmount) / category.AllocatedAmount) * 100 
                        : 0;

                    Categories.Add(new CategoryStat(
                        category.Name,
                        categoryExpenses,
                        trend
                    ));
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"❌ Erreur lors du chargement du dashboard : {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    public class CategoryStat
    {
        public string Name { get; set; }
        public double Amount { get; set; }
        public double Trend { get; set; }

        public string AmountFormatted => $"{Amount:0.00} €";
        public string TrendFormatted => $"{Trend:+0.0;-0.0}%";
        public Color TrendColor => Trend < 0 ? Colors.Green : (Trend > 0 ? Colors.Red : Colors.Gray);

        public CategoryStat(string name, double amount, double trend)
        {
            Name = name;
            Amount = amount;
            Trend = trend;
        }
    }
}
