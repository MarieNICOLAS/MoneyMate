using CommunityToolkit.Mvvm.Input;
using MoneyMate.Models;
using MoneyMate.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;

namespace MoneyMate.ViewModels
{
    [QueryProperty(nameof(BudgetId), "budgetId")]
    [QueryProperty(nameof(ReturnMessage), "returnMsg")]
    public partial class BudgetViewModel : BaseViewModel
    {
        // ID du budget passé via Shell
        public int BudgetId { get; set; }

        // Champs privés
        private string totalAmount;
        private DateTime selectedDate = DateTime.Now;
        private string message;
        private Color messageColor = Colors.Transparent;
        private Budget selectedBudget;

        // --------- Propriétés bindées ---------

        public string TotalAmount
        {
            get => totalAmount;
            set => SetProperty(ref totalAmount, value);
        }

        public DateTime SelectedDate
        {
            get => selectedDate;
            set => SetProperty(ref selectedDate, value);
        }

        public string Message
        {
            get => message;
            set => SetProperty(ref message, value);
        }

        public Color MessageColor
        {
            get => messageColor;
            set => SetProperty(ref messageColor, value);
        }

        public Budget SelectedBudget
        {
            get => selectedBudget;
            set => SetProperty(ref selectedBudget, value);
        }

        public ObservableCollection<Budget> Budgets { get; } = new();


        // --------- Commandes ---------

        public ICommand CreateBudgetCommand { get; }
        public ICommand UpdateBudgetCommand { get; }
        public ICommand DeleteBudgetCommand { get; }
        public ICommand CancelCommand { get; }


        // --------- Constructeur CORRIGÉ ---------

        public BudgetViewModel(
            AuthService authService,
            BudgetService budgetService,
            ExpenseService expenseService,
            CategoryService categoryService
        ) : base(authService, budgetService, expenseService, categoryService)
        {
            CreateBudgetCommand = new AsyncRelayCommand(CreateBudgetAsync);
            UpdateBudgetCommand = new AsyncRelayCommand(UpdateBudgetAsync);
            DeleteBudgetCommand = new AsyncRelayCommand(DeleteBudgetAsync);
            CancelCommand = new AsyncRelayCommand(OnCancelAsync);
        }


        // --------- Charger tous les budgets ---------

        public async Task LoadBudgetsAsync()
        {
            await RunSafeAsync(async () =>
            {
                var list = await _budgetService.GetBudgetsAsync();
                Budgets.Clear();

                foreach (var b in list)
                    Budgets.Add(b);
            });
        }


        // --------- Charger un budget ---------

        public async Task LoadBudgetAsync()
        {
            if (BudgetId <= 0) return;

            SelectedBudget = await _budgetService.GetBudgetByIdAsync(BudgetId);

            if (SelectedBudget != null)
            {
                TotalAmount = SelectedBudget.TotalAmount.ToString();
                SelectedDate = new DateTime(SelectedBudget.Year, SelectedBudget.Month, 1);
            }
        }


        // --------- Création d'un budget ---------

        private async Task CreateBudgetAsync()
        {
            if (!double.TryParse(TotalAmount, out double total))
            {
                SetMessage("Montant invalide.", Colors.Red);
                return;
            }

            var budget = new Budget(CurrentUserId, total)
            {
                Month = SelectedDate.Month,
                Year = SelectedDate.Year
            };

            await RunSafeAsync(async () =>
            {
                bool ok = await _budgetService.AddBudgetAsync(budget);

                if (!ok)
                {
                    SetMessage("Budget déjà existant pour ce mois.", Colors.Red);
                    return;
                }

                await Shell.Current.GoToAsync("///BudgetListPage?returnMsg=Budget créé avec succès");
            });
        }


        // --------- Modification d'un budget ---------

        private async Task UpdateBudgetAsync()
        {
            if (SelectedBudget == null)
            {
                SetMessage("Aucun budget à modifier.", Colors.Red);
                return;
            }

            if (!double.TryParse(TotalAmount, out double total))
            {
                SetMessage("Montant invalide.", Colors.Red);
                return;
            }

            SelectedBudget.TotalAmount = total;
            SelectedBudget.Month = SelectedDate.Month;
            SelectedBudget.Year = SelectedDate.Year;

            await RunSafeAsync(async () =>
            {
                bool ok = await _budgetService.UpdateBudgetAsync(SelectedBudget);

                if (ok)
                    await Shell.Current.GoToAsync("///BudgetListPage?returnMsg=Budget modifié");
                else
                    SetMessage("Erreur lors de la modification.", Colors.Red);
            });
        }


        // --------- Suppression ---------

        private async Task DeleteBudgetAsync()
        {
            if (SelectedBudget == null)
            {
                SetMessage("Aucun budget à supprimer.", Colors.Red);
                return;
            }

            await RunSafeAsync(async () =>
            {
                bool ok = await _budgetService.DeleteBudgetAsync(SelectedBudget);

                if (ok)
                    await Shell.Current.GoToAsync("///BudgetListPage?returnMsg=Budget supprimé");
                else
                    SetMessage("Erreur suppression.", Colors.Red);
            });
        }


        // --------- Annuler ---------

        private async Task OnCancelAsync()
        {
            await Shell.Current.GoToAsync("///BudgetListPage?returnMsg=Annulé");
        }


        // --------- Messages utilitaires ---------

        private void SetMessage(string txt, Color c)
        {
            Message = txt;
            MessageColor = c;
        }


        // --------- Return Message (Shell) ---------

        public string ReturnMessage
        {
            get => message;
            set
            {
                if (!string.IsNullOrEmpty(value))
                    SetMessage(value, Colors.Green);
            }
        }
    }
}
