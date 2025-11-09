using System.Collections.ObjectModel;
using Microsoft.Maui.Graphics;

namespace MoneyMate.ViewModels
{
    public class DashboardViewModel : BaseViewModel
    {
        public double TotalBudget { get; set; } = 2000;
        public double TotalSpent { get; set; } = 1350;
        public double CurrentBalance => TotalBudget - TotalSpent;

        public string CurrentBalanceFormatted => $"{CurrentBalance:0.00} €";
        public double BudgetProgress => TotalSpent / TotalBudget;
        public string BudgetProgressText => $"{TotalSpent:0.##} / {TotalBudget:0.##} €";

        public ObservableCollection<CategoryStat> Categories { get; set; }

        public DashboardViewModel()
        {
            Categories = new ObservableCollection<CategoryStat>
            {
                new("Category 1", 823.2, +8.2),
                new("Category 2", 438.76, +7.0),
                new("Category 3", 345.58, +2.5),
                new("Category 4", 240.32, -6.5),
                new("Category 5", 56.89, +1.7)
            };
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
