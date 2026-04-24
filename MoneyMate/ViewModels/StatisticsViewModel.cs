using MoneyMate.Services;
using MoneyMate.Models;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MoneyMate.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        private readonly ExpenseService _expenseService;
        private readonly BudgetService _budgetService;
        private readonly CategoryService _categoryService;

        private string _selectedPeriod = "Year";
        public string SelectedPeriod
        {
            get => _selectedPeriod;
            set { _selectedPeriod = value; OnPropertyChanged(); _ = LoadLineChartAsync(); }
        }

        private string _selectedYear;
        public string SelectedYear
        {
            get => _selectedYear;
            set { _selectedYear = value; OnPropertyChanged(); _ = LoadAllAsync(); }
        }

        public List<string> YearOptions { get; } = new();

        public ObservableCollection<ChartPoint> LinePoints { get; } = new();

        public ObservableCollection<BarMonth> BarMonths { get; } = new();

        private double _barMaxValue = 1;
        public double BarMaxValue
        {
            get => _barMaxValue;
            set { _barMaxValue = value; OnPropertyChanged(); OnPropertyChanged(nameof(BarMidValue)); }
        }
        public double BarMidValue => Math.Round(BarMaxValue / 2);

        private string _lineTotal = "0 €";
        public string LineTotal
        {
            get => _lineTotal;
            set { _lineTotal = value; OnPropertyChanged(); }
        }

        private string _totalSpent = "0 €";
        public string TotalSpent
        {
            get => _totalSpent;
            set { _totalSpent = value; OnPropertyChanged(); }
        }

        private string _totalBudget = "0 €";
        public string TotalBudget
        {
            get => _totalBudget;
            set { _totalBudget = value; OnPropertyChanged(); }
        }

        private string _totalRemaining = "0 €";
        public string TotalRemaining
        {
            get => _totalRemaining;
            set { _totalRemaining = value; OnPropertyChanged(); }
        }

        private string _currentMonthTotal = "0 €";
        public string CurrentMonthTotal
        {
            get => _currentMonthTotal;
            set { _currentMonthTotal = value; OnPropertyChanged(); }
        }

        private string _previousMonthTotal = "0 €";
        public string PreviousMonthTotal
        {
            get => _previousMonthTotal;
            set { _previousMonthTotal = value; OnPropertyChanged(); }
        }

        private string _monthEvolution = "0%";
        public string MonthEvolution
        {
            get => _monthEvolution;
            set { _monthEvolution = value; OnPropertyChanged(); }
        }

        private bool _isPositiveEvolution = true;
        public bool IsPositiveEvolution
        {
            get => _isPositiveEvolution;
            set { _isPositiveEvolution = value; OnPropertyChanged(); OnPropertyChanged(nameof(EvolutionColor)); }
        }

        public Color EvolutionColor => IsPositiveEvolution
            ? Color.FromArgb("#6CC57C")
            : Color.FromArgb("#E57373");

        public ObservableCollection<StatCategoryItem> TopCategories { get; } = new();

        public ObservableCollection<PieSlice> PieSlices { get; } = new();

        private bool _hasNoData = false;
        public bool HasNoData
        {
            get => _hasNoData;
            set { _hasNoData = value; OnPropertyChanged(); }
        }

        public ICommand SelectPeriodCommand { get; }
        public ICommand SelectYearCommand { get; }

        public StatisticsViewModel(ExpenseService expenseService, BudgetService budgetService, CategoryService categoryService)
        {
            _expenseService = expenseService;
            _budgetService = budgetService;
            _categoryService = categoryService;

            SelectPeriodCommand = new Command<string>(p => SelectedPeriod = p);
            SelectYearCommand = new Command<string>(y => SelectedYear = y);

            int currentYear = DateTime.Now.Year;
            for (int y = currentYear - 2; y <= currentYear; y++)
                YearOptions.Add(y.ToString());

            _selectedYear = currentYear.ToString();

            _ = LoadAllAsync();
        }

        private async Task LoadAllAsync()
        {
            await LoadLineChartAsync();
            await LoadBarChartAsync();
            await LoadSummaryAsync();
            await LoadComparisonAsync();
            await LoadTopCategoriesAsync();
        }

        private async Task LoadLineChartAsync()
        {
            LinePoints.Clear();
            var allExpenses = await _expenseService.GetExpensesAsync();

            if (SelectedPeriod == "Year")
            {
                int year = int.Parse(SelectedYear);
                string[] labels = { "Jan","Feb","Mar","Apr","May","Jun",
                                    "Jul","Aug","Sep","Oct","Nov","Dec" };

                var values = new double[12];
                double total = 0;
                for (int m = 1; m <= 12; m++)
                {
                    values[m - 1] = allExpenses
                        .Where(e => e.Date.Year == year && e.Date.Month == m)
                        .Sum(e => e.Amount);
                    total += values[m - 1];
                }

                double max = values.Max();
                if (max <= 0) max = 1;

                for (int m = 0; m < 12; m++)
                    LinePoints.Add(new ChartPoint(labels[m], values[m], values[m] / max * 140));

                LineTotal = $"{total:0.00} €";
            }
            else
            {
                var now = DateTime.Now;
                var expenses = allExpenses
                    .Where(e => e.Date.Year == now.Year && e.Date.Month == now.Month)
                    .ToList();

                var values = new double[5];
                for (int w = 0; w < 5; w++)
                {
                    int dayStart = w * 7 + 1;
                    int dayEnd = Math.Min((w + 1) * 7, DateTime.DaysInMonth(now.Year, now.Month));
                    values[w] = expenses
                        .Where(e => e.Date.Day >= dayStart && e.Date.Day <= dayEnd)
                        .Sum(e => e.Amount);
                }

                double max = values.Max();
                if (max <= 0) max = 1;

                for (int w = 0; w < 5; w++)
                    LinePoints.Add(new ChartPoint($"W{w + 1}", values[w], values[w] / max * 140));

                LineTotal = $"{expenses.Sum(e => e.Amount):0.00} €";
            }

            OnPropertyChanged(nameof(LinePoints));
        }

        private async Task LoadBarChartAsync()
        {
            BarMonths.Clear();

            int year = int.Parse(SelectedYear);
            var allBudgets = await _budgetService.GetBudgetsAsync();
            var allExpenses = await _expenseService.GetExpensesAsync();

            string[] labels = { "Jan","Feb","Mar","Apr","May","Jun",
                                 "Jul","Aug","Sep","Oct","Nov","Dec" };

            double max = 1;
            var rawData = new (double budget, double spent)[12];

            for (int m = 1; m <= 12; m++)
            {
                var budget = allBudgets.FirstOrDefault(b => b.Year == year && b.Month == m);
                double budgetAmt = budget?.TotalAmount ?? 0;
                double spent = allExpenses
                    .Where(e => e.Date.Year == year && e.Date.Month == m)
                    .Sum(e => e.Amount);

                rawData[m - 1] = (budgetAmt, spent);
                if (budgetAmt > max) max = budgetAmt;
                if (spent > max) max = spent;
            }

            BarMaxValue = Math.Ceiling(max / 1000) * 1000;
            if (BarMaxValue < 1) BarMaxValue = 1;

            const double maxBarHeight = 180;
            bool anyData = false;

            for (int m = 0; m < 12; m++)
            {
                var (budgetAmt, spent) = rawData[m];
                if (budgetAmt > 0 || spent > 0) anyData = true;

                double budgetHeight = budgetAmt / BarMaxValue * maxBarHeight;
                double spentHeight = spent / BarMaxValue * maxBarHeight;
                bool overBudget = spent > budgetAmt && budgetAmt > 0;

                BarMonths.Add(new BarMonth(labels[m], budgetAmt, spent, budgetHeight, spentHeight, overBudget));
            }

            HasNoData = !anyData;
            OnPropertyChanged(nameof(BarMonths));
        }

        private async Task LoadSummaryAsync()
        {
            var now = DateTime.Now;
            var allExpenses = await _expenseService.GetExpensesAsync();
            var allBudgets = await _budgetService.GetBudgetsAsync();

            double spent = allExpenses
                .Where(e => e.Date.Year == now.Year && e.Date.Month == now.Month)
                .Sum(e => e.Amount);

            var budget = allBudgets
                .FirstOrDefault(b => b.Year == now.Year && b.Month == now.Month);
            double total = budget?.TotalAmount ?? 0;

            TotalSpent = $"{spent:0.00} €";
            TotalBudget = $"{total:0.00} €";
            TotalRemaining = $"{Math.Max(0, total - spent):0.00} €";
        }

        private async Task LoadComparisonAsync()
        {
            var now = DateTime.Now;
            var allExpenses = await _expenseService.GetExpensesAsync();

            double currentMonth = allExpenses
                .Where(e => e.Date.Year == now.Year && e.Date.Month == now.Month)
                .Sum(e => e.Amount);

            var prevDate = now.AddMonths(-1);
            double previousMonth = allExpenses
                .Where(e => e.Date.Year == prevDate.Year && e.Date.Month == prevDate.Month)
                .Sum(e => e.Amount);

            CurrentMonthTotal = $"{currentMonth:0.00} €";
            PreviousMonthTotal = $"{previousMonth:0.00} €";

            if (previousMonth > 0)
            {
                double evolution = ((currentMonth - previousMonth) / previousMonth) * 100;
                IsPositiveEvolution = evolution <= 0; // positif = on dépense moins
                MonthEvolution = $"{evolution:+0.0;-0.0}%";
            }
            else
            {
                MonthEvolution = "N/A";
                IsPositiveEvolution = true;
            }
        }

        private async Task LoadTopCategoriesAsync()
        {
            TopCategories.Clear();
            PieSlices.Clear();

            var now = DateTime.Now;
            var allExpenses = await _expenseService.GetExpensesAsync();
            var allCategories = await _categoryService.GetCategoriesAsync();

            var monthExpenses = allExpenses
                .Where(e => e.Date.Year == now.Year && e.Date.Month == now.Month)
                .ToList();

            double totalSpent = monthExpenses.Sum(e => e.Amount);
            if (totalSpent <= 0) return;

            var grouped = monthExpenses
                .GroupBy(e => e.CategoryId)
                .Select(g =>
                {
                    var cat = allCategories.FirstOrDefault(c => c.Id == g.Key);
                    return new
                    {
                        Name = cat?.Name ?? $"Catégorie {g.Key}",
                        Color = cat?.ColorHex ?? "#CCCCCC",
                        Amount = g.Sum(e => e.Amount)
                    };
                })
                .OrderByDescending(x => x.Amount)
                .Take(5)
                .ToList();

            foreach (var item in grouped.Take(3))
            {
                double percentage = totalSpent > 0 ? (item.Amount / totalSpent) * 100 : 0;
                TopCategories.Add(new StatCategoryItem(
                    item.Name,
                    item.Amount,
                    percentage,
                    item.Color
                ));
            }

            double startAngle = 0;
            foreach (var item in grouped)
            {
                double percentage = (item.Amount / totalSpent) * 100;
                double sweepAngle = (item.Amount / totalSpent) * 360;
                PieSlices.Add(new PieSlice(item.Name, item.Amount, percentage, item.Color, startAngle, sweepAngle));
                startAngle += sweepAngle;
            }

            OnPropertyChanged(nameof(TopCategories));
            OnPropertyChanged(nameof(PieSlices));
        }
    }

    public class ChartPoint
    {
        public string Label { get; }
        public double Value { get; }
        public double NormalizedHeight { get; }
        public string FormattedValue => $"{Value:0.##} €";

        public ChartPoint(string label, double value, double normalizedHeight)
        {
            Label = label;
            Value = value;
            NormalizedHeight = normalizedHeight;
        }
    }

    public class BarMonth
    {
        public string Month { get; }
        public double BudgetAmount { get; }
        public double SpentAmount { get; }
        public double BudgetBarHeight { get; }
        public double SpentBarHeight { get; }
        public bool IsOverBudget { get; }

        public Color SpentBarColor => IsOverBudget
            ? Color.FromArgb("#E57373")
            : Color.FromArgb("#D4D8DE");

        public BarMonth(string month, double budgetAmount, double spentAmount,
                        double budgetBarHeight, double spentBarHeight, bool isOverBudget)
        {
            Month = month;
            BudgetAmount = budgetAmount;
            SpentAmount = spentAmount;
            BudgetBarHeight = budgetBarHeight;
            SpentBarHeight = spentBarHeight;
            IsOverBudget = isOverBudget;
        }
    }

    public class StatCategoryItem
    {
        public string Name { get; }
        public double Amount { get; }
        public double Percentage { get; }
        public string Color { get; }
        public string FormattedAmount => $"{Amount:0.00} €";
        public string FormattedPercentage => $"{Percentage:0.0}%";
        public double BarWidth => Percentage / 100 * 200; // largeur max 200px

        public StatCategoryItem(string name, double amount, double percentage, string color)
        {
            Name = name;
            Amount = amount;
            Percentage = percentage;
            Color = color;
        }
    }

    public class PieSlice
    {
        public string Name { get; }
        public double Amount { get; }
        public double Percentage { get; }
        public string Color { get; }
        public double StartAngle { get; }
        public double SweepAngle { get; }
        public string FormattedPercentage => $"{Percentage:0.0}%";

        public PieSlice(string name, double amount, double percentage, string color, double startAngle, double sweepAngle)
        {
            Name = name;
            Amount = amount;
            Percentage = percentage;
            Color = color;
            StartAngle = startAngle;
            SweepAngle = sweepAngle;
        }
    }
}
