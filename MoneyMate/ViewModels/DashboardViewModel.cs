using System.Collections.ObjectModel;
using Microsoft.Maui.Graphics;
using MoneyMate.Services;

namespace MoneyMate.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        // --- Propriétés simulées en attendant la vraie logique métier ---
        public double TotalBudget { get; set; } = 2000;
        public double TotalSpent { get; set; } = 1350;
        public double CurrentBalance => TotalBudget - TotalSpent;

        public string CurrentBalanceFormatted => $"{CurrentBalance:0.00} €";
        public double BudgetProgress => TotalSpent / TotalBudget;
        public string BudgetProgressText => $"{TotalSpent:0.##} / {TotalBudget:0.##} €";

        public ObservableCollection<CategoryStat> Categories { get; } = new();

        // --- Constructeur CORRECT AVEC DI ---
        public DashboardViewModel(
            AuthService authService,
            BudgetService budgetService,
            ExpenseService expenseService,
            CategoryService categoryService)
            : base(authService, budgetService, expenseService, categoryService)
        {
            LoadMockData();
        }

        // --- Données factices en attendant la vraie DB ---
        private void LoadMockData()
        {
            Categories.Clear();
            Categories.Add(new CategoryStat("Category 1", 823.2, +8.2));
            Categories.Add(new CategoryStat("Category 2", 438.76, +7.0));
            Categories.Add(new CategoryStat("Category 3", 345.58, +2.5));
            Categories.Add(new CategoryStat("Category 4", 240.32, -6.5));
            Categories.Add(new CategoryStat("Category 5", 56.89, +1.7));
        }
    }

    public class CategoryStat
    {
        public string Name { get; set; }
        public double Amount { get; set; }
        public double Trend { get; set; }

        public string AmountFormatted => $"{Amount:0.00} €";
        public string TrendFormatted => $"{Trend:+0.0;-0.0}%";
        public Color TrendColor => Trend < 0 ? Colors.Gray : Color.FromArgb("#393781");

        public CategoryStat(string name, double amount, double trend)
        {
            Name = name;
            Amount = amount;
            Trend = trend;
        }
    }
}
