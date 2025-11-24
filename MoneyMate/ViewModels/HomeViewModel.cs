using MoneyMate.Services;
using MoneyMate.ViewModels.Expense;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MoneyMate.ViewModels
{
    public class HomeViewModel : BaseViewModel
    {
        private readonly AuthService _authService;
        private readonly BudgetService _budgetService;
        private readonly ExpenseService _expenseService;
        private readonly CategoryService _categoryService;

        // Propriétés exposées à la vue
        private double _totalBudget = 0.0;
        public double TotalBudget
        {
            get => _totalBudget;
            set => SetProperty(ref _totalBudget, value);
        }

        private double _totalSpent = 0.0;
        public double TotalSpent
        {
            get => _totalSpent;
            set => SetProperty(ref _totalSpent, value);
        }

        public double RemainingAmount => TotalBudget - TotalSpent;

        private ObservableCollection<ExpenseSummary> _expenseBreakdown;
        public ObservableCollection<ExpenseSummary> ExpenseBreakdown
        {
            get => _expenseBreakdown;
            set => SetProperty(ref _expenseBreakdown, value);
        }

        // Propriétés utilitaires
        public string CurrentMonthYear => DateTime.Now.ToString("MMMM yyyy");

        // Commandes
        public ICommand GoToAddExpenseCommand { get; }

        // CONSTRUCTEUR
        public HomeViewModel()
        {
            // Initialisation des services avec App.Database (supposé initialisé dans App.xaml.cs)
            _authService = new AuthService(App.Database);
            _budgetService = new BudgetService(App.Database);
            _expenseService = new ExpenseService();
            _expenseBreakdown = new ObservableCollection<ExpenseSummary>();

            // Initialisation des commandes
            GoToAddExpenseCommand = new Command(async () => await Shell.Current.GoToAsync("AddExpensePage"));

            // Chargement des données au démarrage
            Task.Run(LoadDashboardDataAsync);
        }

        // ... (Début du fichier reste le même)

        public async Task LoadDashboardDataAsync()
        {
            if (IsBusy) return;
            IsBusy = true;
            ExpenseBreakdown.Clear();

            try
            {
                var user = await _authService.GetLoggedInUserAsync();
                if (user == null)
                {
                    await Shell.Current.GoToAsync("//LoginPage");
                    return;
                }

                var now = DateTime.Now;
                int userId = user.Id;
                int month = now.Month;
                int year = now.Year;

                // 1. Récupérer le budget du mois
                var currentBudget = await _budgetService.GetCurrentMonthBudgetAsync(userId, month, year);

                // Si aucun budget n'est défini pour le mois, les totaux sont 0.
                if (currentBudget == null)
                {
                    TotalBudget = 0.0;
                    TotalSpent = 0.0;
                    // L' ExpenseBreakdown restera vide, ce qui est correct
                }
                else
                {
                    int budgetId = currentBudget.Id;

                    // Récupérer les données
                    TotalBudget = currentBudget.TotalAmount;

                    // 2. Récupérer le total des dépenses en utilisant le BudgetId
                    TotalSpent = await _expenseService.GetTotalExpensesForBudgetAsync(budgetId);

                    // 3. Récupérer le détail des dépenses par catégorie en utilisant le BudgetId
                    var breakdownList = await _expenseService.GetExpensesBreakdownForBudgetAsync(budgetId);
                    foreach (var item in breakdownList)
                    {
                        ExpenseBreakdown.Add(item);
                    }
                }

                // Mettre à jour les propriétés calculées
                OnPropertyChanged(nameof(RemainingAmount));

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur de chargement du Dashboard : {ex.Message}");
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}