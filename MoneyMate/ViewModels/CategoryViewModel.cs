using CommunityToolkit.Mvvm.Input;
using MoneyMate.Database;
using MoneyMate.Models;
using MoneyMate.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MoneyMate.ViewModels
{
    public class CategoryViewModel : BaseViewModel
    {
        private readonly MoneyMateContext _context;
        private readonly CategoryService _categoryService;

        public ObservableCollection<Category> Categories { get; } = new();
        public ObservableCollection<Budget> Budgets { get; } = new();

        public ObservableCollection<string> ColorOptions { get; } = new(new[]
        {
            "#FFAFAD", "#FFD6A5", "#FCFEB6",
            "#CAFEBF", "#9DF3FD", "#A2C3FD",
            "#BFB3FD", "#FFC7FC"
        });

        private string _name = string.Empty;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        private Budget? _selectedBudget;
        public Budget? SelectedBudget
        {
            get => _selectedBudget;
            set
            {
                if (SetProperty(ref _selectedBudget, value))
                    OnPropertyChanged(nameof(AmountPreview));
            }
        }

        private string? _selectedColor;
        public string? SelectedColor
        {
            get => _selectedColor;
            set => SetProperty(ref _selectedColor, value);
        }

        private double _percentage = 10;
        public double Percentage
        {
            get => _percentage;
            set
            {
                if (SetProperty(ref _percentage, value))
                    OnPropertyChanged(nameof(AmountPreview));
            }
        }

        // 💶 Aperçu du montant selon budget + %
        public double AmountPreview =>
            SelectedBudget == null
                ? 0
                : SelectedBudget.TotalAmount * (Percentage / 100.0);

        // COMMANDES
        public IAsyncRelayCommand AddCategoryCommand { get; }
        public IAsyncRelayCommand UpdateCategoryCommand { get; }
        public IAsyncRelayCommand DeleteCategoryCommand { get; }
        public IRelayCommand CancelCommand { get; }

        // ⭐ Navbar commands
        public IRelayCommand GoHomeCommand { get; }
        public IRelayCommand GoStatisticsCommand { get; }
        public IRelayCommand GoAddCommand { get; }
        public IRelayCommand GoSearchCommand { get; }
        public IRelayCommand GoMenuCommand { get; }

        public CategoryViewModel()
        {
            _context = App.Database ?? new MoneyMateContext();
            _categoryService = new CategoryService(_context);

            SelectedColor = null;

            AddCategoryCommand = new AsyncRelayCommand(AddCategoryAsync);
            UpdateCategoryCommand = new AsyncRelayCommand(UpdateCategoryAsync);
            DeleteCategoryCommand = new AsyncRelayCommand(DeleteCategoryAsync);
            CancelCommand = new RelayCommand(Cancel);

            // ⭐ Navbar navigation
            GoHomeCommand = new RelayCommand(async () =>
                await Shell.Current.GoToAsync("//DashboardPage"));
            GoStatisticsCommand = new RelayCommand(async () =>
                await Shell.Current.GoToAsync("//StatisticsPage"));
            GoAddCommand = new RelayCommand(async () =>
                await Shell.Current.GoToAsync("//AddExpensePage"));
            GoSearchCommand = new RelayCommand(async () =>
                await Shell.Current.GoToAsync("//HistoryPage"));
            GoMenuCommand = new RelayCommand(async () =>
                await Shell.Current.GoToAsync("//SettingsPage"));

            _ = InitializeAsync();
        }

        // --- INITIALISATION ---
        private async Task InitializeAsync()
        {
            await LoadBudgetsAsync();
            await LoadCategoriesAsync();
        }

        private async Task LoadBudgetsAsync()
        {
            var budgets = await _context.GetAllAsync<Budget>();
            Budgets.Clear();

            foreach (var budget in budgets.OrderByDescending(b => b.Year)
                                          .ThenByDescending(b => b.Month))
                Budgets.Add(budget);

            SelectedBudget ??= Budgets.FirstOrDefault();
            OnPropertyChanged(nameof(AmountPreview));
        }

        private async Task LoadCategoriesAsync()
        {
            var categories = await _categoryService.GetCategoriesAsync();
            Categories.Clear();

            foreach (var category in categories)
                Categories.Add(category);
        }

        // --- ADD ---
        private async Task AddCategoryAsync()
        {
            if (IsBusy || SelectedBudget is null || string.IsNullOrWhiteSpace(Name))
                return;

            try
            {
                IsBusy = true;

                var category = new Category
                {
                    BudgetId = SelectedBudget.Id,
                    Name = Name.Trim(),
                    Percentage = Percentage,
                    ColorHex = string.IsNullOrWhiteSpace(SelectedColor) ? "#CCCCCC" : SelectedColor
                };

                category.CalculateAllocatedAmount(SelectedBudget.TotalAmount);

                await _categoryService.AddCategoryAsync(category);
                Categories.Add(category);

                ResetForm();
            }
            finally
            {
                IsBusy = false;
            }
        }

        // --- UPDATE ---
        private async Task UpdateCategoryAsync()
        {
            if (IsBusy || SelectedBudget is null)
                return;

            try
            {
                IsBusy = true;

                var category = Categories.FirstOrDefault(c => c.Id == EditingCategoryId);
                if (category == null) return;

                category.Name = Name;
                category.Percentage = Percentage;
                category.ColorHex = SelectedColor ?? "#CCCCCC";
                category.BudgetId = SelectedBudget.Id;

                category.CalculateAllocatedAmount(SelectedBudget.TotalAmount);

                await _categoryService.UpdateCategoryAsync(category);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // --- DELETE ---
        private async Task DeleteCategoryAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                var category = Categories.FirstOrDefault(c => c.Id == EditingCategoryId);
                if (category == null) return;

                await _categoryService.DeleteCategoryAsync(category);
                Categories.Remove(category);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // --- CANCEL ---
        private void Cancel()
        {
            ResetForm();
            Shell.Current.GoToAsync("..");
        }

        private void ResetForm()
        {
            Name = string.Empty;
            Percentage = 10;
            SelectedColor = null;
            OnPropertyChanged(nameof(AmountPreview));
        }

        // Id de la catégorie en cours d'édition
        public int EditingCategoryId { get; set; }
    }
}
