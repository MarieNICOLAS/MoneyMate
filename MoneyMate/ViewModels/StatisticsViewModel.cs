using MoneyMate.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MoneyMate.ViewModels
{
    public class StatisticsViewModel : BaseViewModel
    {
        private readonly ExpenseService _expenseService;
        private readonly BudgetService _budgetService;

        // ─── Filtre période (courbe) ───────────────────────────
        private string _selectedPeriod = "Year";
        public string SelectedPeriod
        {
            get => _selectedPeriod;
            set { _selectedPeriod = value; OnPropertyChanged(); _ = LoadLineChartAsync(); }
        }

        // ─── Filtre année (barres) ─────────────────────────────
        private string _selectedYear;
        public string SelectedYear
        {
            get => _selectedYear;
            set { _selectedYear = value; OnPropertyChanged(); _ = LoadBarChartAsync(); }
        }

        public List<string> YearOptions { get; } = new();

        // ─── Données graphique courbes ─────────────────────────
        public ObservableCollection<ChartPoint> LinePoints { get; } = new();

        // ─── Données graphique barres ──────────────────────────
        public ObservableCollection<BarMonth> BarMonths { get; } = new();

        private double _barMaxValue = 1;
        public double BarMaxValue
        {
            get => _barMaxValue;
            set { _barMaxValue = value; OnPropertyChanged(); OnPropertyChanged(nameof(BarMidValue)); }
        }

        public double BarMidValue => Math.Round(BarMaxValue / 2);

        // ─── Total affiché courbe ──────────────────────────────
        private string _lineTotal = "0 €";
        public string LineTotal
        {
            get => _lineTotal;
            set { _lineTotal = value; OnPropertyChanged(); }
        }

        // ─── Pas de données ────────────────────────────────────
        private bool _hasNoData = false;
        public bool HasNoData
        {
            get => _hasNoData;
            set { _hasNoData = value; OnPropertyChanged(); }
        }

        // ─── Commandes ─────────────────────────────────────────
        public ICommand SelectPeriodCommand { get; }
        public ICommand SelectYearCommand { get; }

        // ──────────────────────────────────────────────────────
        public StatisticsViewModel(ExpenseService expenseService, BudgetService budgetService)
        {
            _expenseService = expenseService;
            _budgetService = budgetService;

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
        }

        // ──────────────────────────────────────────────────────
        //  GRAPHIQUE EN COURBES
        // ──────────────────────────────────────────────────────
        private async Task LoadLineChartAsync()
        {
            LinePoints.Clear();

            var allExpenses = await _expenseService.GetExpensesAsync();

            if (SelectedPeriod == "Year")
            {
                int year = int.Parse(SelectedYear);
                string[] labels = { "Jan","Feb","Mar","Apr","May","Jun",
                                    "Jul","Aug","Sep","Oct","Nov","Dec" };

                // Calculer les valeurs brutes
                var values = new double[12];
                double total = 0;
                for (int m = 1; m <= 12; m++)
                {
                    values[m - 1] = allExpenses
                        .Where(e => e.Date.Year == year && e.Date.Month == m)
                        .Sum(e => e.Amount);
                    total += values[m - 1];
                }

                // Normaliser pour la hauteur des barres (max = 140px)
                double max = values.Max();
                if (max <= 0) max = 1;

                for (int m = 0; m < 12; m++)
                    LinePoints.Add(new ChartPoint(labels[m], values[m], values[m] / max * 140));

                LineTotal = $"{total:0.00} €";
            }
            else // Month → semaines
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

        // ──────────────────────────────────────────────────────
        //  GRAPHIQUE EN BARRES — Budget vs Dépenses
        // ──────────────────────────────────────────────────────
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
    }

    // ──────────────────────────────────────────────────────────
    //  MODÈLES DE DONNÉES
    // ──────────────────────────────────────────────────────────

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
            ? Color.FromArgb("#D9534F")
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
}