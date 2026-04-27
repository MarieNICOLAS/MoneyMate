using MoneyMate.Models;
using MoneyMate.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MoneyMate.ViewModels
{
    public class HistoryTransactionViewModel : BaseViewModel
    {
        private readonly ExpenseService _expenseService;
        private readonly CategoryService _categoryService;

        // ─── Données brutes ────────────────────────────────────
        private List<ExpenseDisplayItem> _allItems = new();

        // ─── Liste affichée (filtrée) ──────────────────────────
        public ObservableCollection<ExpenseDisplayItem> Transactions { get; } = new();

        // ─── Recherche textuelle ───────────────────────────────
        private string _searchQuery = string.Empty;
        public string SearchQuery
        {
            get => _searchQuery;
            set { _searchQuery = value; OnPropertyChanged(); ApplyFilters(); }
        }

        // ─── Filtres toggle ────────────────────────────────────
        private bool _filterByCategory;
        public bool FilterByCategory
        {
            get => _filterByCategory;
            set { _filterByCategory = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private bool _filterByName;
        public bool FilterByName
        {
            get => _filterByName;
            set { _filterByName = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private bool _filterByPrice;
        public bool FilterByPrice
        {
            get => _filterByPrice;
            set { _filterByPrice = value; OnPropertyChanged(); ApplyFilters(); }
        }

        // ─── Filtre date ───────────────────────────────────────
        private DateTime _selectedDate = DateTime.Now;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set { _selectedDate = value; OnPropertyChanged(); ApplyFilters(); }
        }

        private bool _filterByDate;
        public bool FilterByDate
        {
            get => _filterByDate;
            set { _filterByDate = value; OnPropertyChanged(); ApplyFilters(); }
        }

        // ─── États UI ──────────────────────────────────────────
        private bool _hasNoData;
        public bool HasNoData
        {
            get => _hasNoData;
            set { _hasNoData = value; OnPropertyChanged(); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        // ─── Résumé ────────────────────────────────────────────
        private string _totalAmount = "0.00 €";
        public string TotalAmount
        {
            get => _totalAmount;
            set { _totalAmount = value; OnPropertyChanged(); }
        }

        private string _transactionCount = "0 transaction";
        public string TransactionCount
        {
            get => _transactionCount;
            set { _transactionCount = value; OnPropertyChanged(); }
        }

        // ─── Commandes ─────────────────────────────────────────
        public ICommand SearchCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ToggleDateFilterCommand { get; }

        // ──────────────────────────────────────────────────────
        public HistoryTransactionViewModel(ExpenseService expenseService, CategoryService categoryService)
        {
            _expenseService = expenseService;
            _categoryService = categoryService;

            SearchCommand = new Command(ApplyFilters);
            DeleteCommand = new Command<ExpenseDisplayItem>(async item => await DeleteExpenseAsync(item));
            RefreshCommand = new Command(async () => await LoadAsync());
            ToggleDateFilterCommand = new Command(() => FilterByDate = !FilterByDate);

            _ = LoadAsync();
        }

        // ──────────────────────────────────────────────────────
        //  CHARGEMENT
        // ──────────────────────────────────────────────────────
        public async Task LoadAsync()
        {
            IsLoading = true;
            try
            {
                var expenses = await _expenseService.GetExpensesAsync();
                var categories = await _categoryService.GetCategoriesAsync();

                _allItems = expenses
                    .OrderByDescending(e => e.Date)
                    .Select(e =>
                    {
                        var cat = categories.FirstOrDefault(c => c.Id == e.CategoryId);
                        return new ExpenseDisplayItem
                        {
                            Id = e.Id,
                            Expense = e,
                            Description = string.IsNullOrWhiteSpace(e.Description) ? "Sans description" : e.Description,
                            Amount = e.Amount,
                            FormattedAmount = $"{e.Amount:0.00} €",
                            Date = e.Date,
                            FormattedDate = e.Date.ToString("dd/MM/yyyy"),
                            CategoryName = cat?.Name ?? "Autre",
                            CategoryColor = cat?.ColorHex ?? "#AAAAAA",
                            BudgetId = e.BudgetId
                        };
                    })
                    .ToList();

                ApplyFilters();
            }
            finally
            {
                IsLoading = false;
            }
        }

        // ──────────────────────────────────────────────────────
        //  FILTRES
        // ──────────────────────────────────────────────────────
        private void ApplyFilters()
        {
            var result = _allItems.AsEnumerable();

            // Filtre recherche textuelle
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                var q = SearchQuery.ToLower();

                if (FilterByName)
                    result = result.Where(i => i.Description.ToLower().Contains(q));
                else if (FilterByCategory)
                    result = result.Where(i => i.CategoryName.ToLower().Contains(q));
                else
                    result = result.Where(i =>
                        i.Description.ToLower().Contains(q) ||
                        i.CategoryName.ToLower().Contains(q));
            }

            // Filtre prix (tri décroissant)
            if (FilterByPrice)
                result = result.OrderByDescending(i => i.Amount);

            // Filtre date (jour exact)
            if (FilterByDate)
                result = result.Where(i => i.Date.Date == SelectedDate.Date);

            var list = result.ToList();

            Transactions.Clear();
            foreach (var item in list)
                Transactions.Add(item);

            double total = list.Sum(i => i.Amount);
            TotalAmount = $"{total:0.00} €";
            int count = list.Count;
            TransactionCount = $"{count} transaction{(count > 1 ? "s" : "")}";
            HasNoData = count == 0;
        }

        // ──────────────────────────────────────────────────────
        //  SUPPRESSION
        // ──────────────────────────────────────────────────────
        private async Task DeleteExpenseAsync(ExpenseDisplayItem item)
        {
            if (item == null) return;

            bool confirmed = await Shell.Current.DisplayAlert(
                "Supprimer",
                $"Supprimer \"{item.Description}\" ({item.FormattedAmount}) ?",
                "Supprimer", "Annuler");

            if (!confirmed) return;

            await _expenseService.DeleteExpenseAsync(item.Expense);
            _allItems.Remove(item);
            ApplyFilters();
        }
    }

    // ──────────────────────────────────────────────────────────
    //  MODÈLE D'AFFICHAGE
    // ──────────────────────────────────────────────────────────
    public class ExpenseDisplayItem
    {
        public int Id { get; set; }
        public Expense Expense { get; set; }
        public string Description { get; set; }
        public double Amount { get; set; }
        public string FormattedAmount { get; set; }
        public DateTime Date { get; set; }
        public string FormattedDate { get; set; }
        public string CategoryName { get; set; }
        public string CategoryColor { get; set; }
        public int BudgetId { get; set; }

        // Première lettre de la catégorie pour l'icône
        public string CategoryInitial => CategoryName?.Length > 0
            ? CategoryName[0].ToString().ToUpper() : "?";
    }
}
