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
            "#FFAFAD",
            "#FFD6A5",
            "#FCFEB6",
            "#CAFEBF",
            "#9DF3FD",
            "#A2C3FD",
            "#BFB3FD",
            "#FFC7FC"
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
                {
                    OnPropertyChanged(nameof(AmountPreview));
                }
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
                {
                    OnPropertyChanged(nameof(AmountPreview));
                }
            }
        }

        // 💶 Montant dynamique en fonction du % et du budget sélectionné
        public double AmountPreview =>
            SelectedBudget == null
                ? 0
                : SelectedBudget.TotalAmount * (Percentage / 100.0);

        public IAsyncRelayCommand AddCategoryCommand { get; }

        public CategoryViewModel()
        {
            _context = App.Database ?? new MoneyMateContext();
            _categoryService = new CategoryService(_context);

            // ❌ Avant : SelectedColor = ColorOptions.FirstOrDefault();
            // ✔ Maintenant : aucune couleur sélectionnée au début (pas de checkmark)
            SelectedColor = null;

            AddCategoryCommand = new AsyncRelayCommand(AddCategoryAsync);

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            await LoadBudgetsAsync();
            await LoadCategoriesAsync();
        }

        private async Task LoadBudgetsAsync()
        {
            var budgets = await _context.GetAllAsync<Budget>();
            Budgets.Clear();

            foreach (var budget in budgets
                         .OrderByDescending(b => b.Year)
                         .ThenByDescending(b => b.Month))
            {
                Budgets.Add(budget);
            }

            SelectedBudget ??= Budgets.FirstOrDefault();
            OnPropertyChanged(nameof(AmountPreview));
        }

        private async Task LoadCategoriesAsync()
        {
            var categories = await _categoryService.GetCategoriesAsync();
            Categories.Clear();

            foreach (var category in categories)
            {
                Categories.Add(category);
            }
        }

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
                    ColorHex = string.IsNullOrWhiteSpace(SelectedColor)
                        ? "#CCCCCC"
                        : SelectedColor
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

        private void ResetForm()
        {
            Name = string.Empty;
            Percentage = 10;
            SelectedColor = null;
            OnPropertyChanged(nameof(AmountPreview));
        }
    }
}
