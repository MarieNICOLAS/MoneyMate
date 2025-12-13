using CommunityToolkit.Mvvm.Input;
using MoneyMate.Models;
using MoneyMate.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MoneyMate.ViewModels
{
    public class BudgetDetailViewModel : BaseViewModel
    {
        private readonly BudgetService _budgetService;
        private readonly BudgetCategoryService _pivotService;

        // -----------------------------
        // PROPRIÉTÉS BUDGET
        // -----------------------------

        private Budget? _budget;
        public Budget? Budget
        {
            get => _budget;
            set => SetProperty(ref _budget, value);
        }

        private double _totalSpent;
        public double TotalSpent
        {
            get => _totalSpent;
            set => SetProperty(ref _totalSpent, value);
        }

        public double RemainingAmount =>
            Budget == null ? 0 : Budget.TotalAmount - TotalSpent;

        // -----------------------------
        // COLLECTION DES CATÉGORIES DU BUDGET
        // -----------------------------
        public ObservableCollection<(BudgetCategory pivot, Category category)> BudgetCategories { get; } = new();

        // -----------------------------
        // COMMANDES
        // -----------------------------
        public IAsyncRelayCommand LoadBudgetDetailCommand { get; }
        public IAsyncRelayCommand<(BudgetCategory pivot, Category category)> DeletePivotCommand { get; }
        public IAsyncRelayCommand AddCategoryCommand { get; }
        public IAsyncRelayCommand<(BudgetCategory pivot, Category category)> EditCategoryCommand { get; }

        // -----------------------------
        // CONSTRUCTEUR
        // -----------------------------
        public BudgetDetailViewModel()
        {
            _budgetService = new BudgetService(App.Database);
            _pivotService = new BudgetCategoryService(App.Database);

            LoadBudgetDetailCommand = new AsyncRelayCommand(LoadBudgetDetailAsync);
            DeletePivotCommand = new AsyncRelayCommand<(BudgetCategory pivot, Category category)>(DeletePivotAsync);
            AddCategoryCommand = new AsyncRelayCommand(AddCategoryAsync);
            EditCategoryCommand = new AsyncRelayCommand<(BudgetCategory pivot, Category category)>(EditCategoryAsync);
        }

        // -----------------------------
        // INITIALISATION
        // -----------------------------
        public int BudgetId { get; set; }

        public void SetBudgetId(int id)
        {
            BudgetId = id;
        }

        // -----------------------------
        // CHARGEMENT DU BUDGET COMPLET
        // -----------------------------
        private async Task LoadBudgetDetailAsync()
        {
            if (IsBusy) return;

            try
            {
                IsBusy = true;

                var data = await _budgetService.GetBudgetWithCategoriesAsync(BudgetId);

                Budget = data.budget;

                BudgetCategories.Clear();
                foreach (var item in data.categories)
                    BudgetCategories.Add(item);

                // Calcul du total dépensé
                TotalSpent = BudgetCategories.Sum(c => c.pivot.SpentAmount);

                OnPropertyChanged(nameof(RemainingAmount));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // -----------------------------
        // AJOUT D’UNE CATÉGORIE AU BUDGET
        // -----------------------------
        private async Task AddCategoryAsync()
        {
            await Shell.Current.GoToAsync($"addbudgetcategory?budgetId={BudgetId}");
        }

        // -----------------------------
        // ÉDITION D’UNE CATÉGORIE DU BUDGET
        // -----------------------------
        private async Task EditCategoryAsync((BudgetCategory pivot, Category category) item)
        {
            var pivot = item.pivot;

            await Shell.Current.GoToAsync(
                $"editbudgetcategory?pivotId={pivot.Id}&budgetId={BudgetId}"
            );
        }

        // -----------------------------
        // SUPPRESSION D'UNE CATÉGORIE DU BUDGET
        // -----------------------------
        private async Task DeletePivotAsync((BudgetCategory pivot, Category category) item)
        {
            try
            {
                await _pivotService.DeleteBudgetCategoryAsync(item.pivot);
                BudgetCategories.Remove(item);

                TotalSpent = BudgetCategories.Sum(c => c.pivot.SpentAmount);
                OnPropertyChanged(nameof(RemainingAmount));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
