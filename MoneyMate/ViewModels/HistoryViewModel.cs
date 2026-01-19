using MoneyMate.Models;
using MoneyMate.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MoneyMate.ViewModels
{
    public class HistoryViewModel : BaseViewModel
    {
        private readonly ExpenseService _expenseService;
        private readonly CategoryService _categoryService;


        public ObservableCollection<Expense> Expenses { get; } = new();
        public ObservableCollection<Category> Categories { get; } = new();


        // Recherche par nom
        private string _searchQuery;
        public string SearchQuery
        {
            get => _searchQuery;
            set
            {
                _searchQuery = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        // Filtre date
        private DateTime _selectedDate = DateTime.Today;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set
            {
                _selectedDate = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        // Filtre catégorie
        private Category _selectedCategory;
        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }


        // Commandes
        public ICommand ResetFiltersCommand { get; }

        public ICommand LoadCategoriesCommand { get; }  
        public ICommand LoadExpensesCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand SelectExpenseCommand { get; }

        private List<Expense> _allExpenses = new();

        public HistoryViewModel(ExpenseService expenseService,CategoryService categoryService)
        {
            _expenseService = expenseService;
            _categoryService = categoryService; 


            LoadExpensesCommand = new Command(async () => await LoadExpenses());
            LoadCategoriesCommand = new Command(async () => await LoadCategories());
            SearchCommand = new Command(ApplyFilters);
            SelectExpenseCommand = new Command<Expense>(OnExpenseSelected);
            ResetFiltersCommand = new Command(ResetFilters);

            //_ = LoadCategories();
            //_ = LoadExpenses();
            _ = InitializeAsync();

        }
        private async Task InitializeAsync()
        {
            await LoadCategories();  // charge les catégories
            await LoadExpenses();    // charge les dépenses
            SetDefaultFilters();     // applique les filtres par défaut
        }


        private void ApplyFilters()
        {
            if (_allExpenses == null)
                return;

            var query = _allExpenses.AsEnumerable();

            //  Filtre par nom
            if (!string.IsNullOrWhiteSpace(SearchQuery))
            {
                query = query.Where(e =>
                    !string.IsNullOrEmpty(e.Description) &&
                    e.Description.Contains(SearchQuery, StringComparison.OrdinalIgnoreCase));
            }

            //  Filtre par mois
            query = query.Where(e =>
                e.Date.Month == SelectedDate.Month &&
                e.Date.Year == SelectedDate.Year);

            //  Filtre par catégorie (sauf si "Toutes")
            if (SelectedCategory != null && SelectedCategory.Id != 0)
            {
                query = query.Where(e => e.CategoryId == SelectedCategory.Id);
            }

            Expenses.Clear();
            foreach (var expense in query.OrderByDescending(e => e.Date))
                Expenses.Add(expense);
        }
        private void SetDefaultFilters()
        {
            // Catégorie "Toutes"
            SelectedCategory = Categories.FirstOrDefault(c => c.Id == 0);

            // Date = mois courant
            SelectedDate = DateTime.Today;

            // Recherche vide
            SearchQuery = string.Empty;

            // Appliquer les filtres
            ApplyFilters();
        }

        private void ResetFilters()
        {
            SetDefaultFilters();
        }

        private void OnExpenseSelected(Expense expense)
        {
            if (expense == null)
                return;
        }

        private async Task LoadCategories()
        {
            Categories.Clear();

            // Ajouter "Toutes" en premier
            var allCategory = new Category { Id = 0, Name = "Toutes" };
            Categories.Add(allCategory);

            // Charger les catégories depuis le service
            var categories = await _categoryService.GetCategoriesAsync();

            // Ajouter uniquement celles qui ne sont pas déjà dans la liste
            foreach (var category in categories)
            {
                if (!Categories.Any(c => c.Id == category.Id))
                    Categories.Add(category);
            }
        }

        private async Task LoadExpenses()
        {
            _allExpenses = await _expenseService.GetExpensesAsync();
            var categories = await _categoryService.GetCategoriesAsync();

            foreach (var expense in _allExpenses)
            {
                var category = categories.FirstOrDefault(c => c.Id == expense.CategoryId);
                expense.CategoryName = category?.Name ?? "Inconnu"; // tu ajoutes CategoryName dans Expense
            }

            Expenses.Clear();
            foreach (var expense in _allExpenses.OrderByDescending(e => e.Date))
                Expenses.Add(expense);
        }


    }
}
