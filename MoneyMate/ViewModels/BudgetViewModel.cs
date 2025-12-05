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
        private readonly BudgetService _budgetService;

        // ID du budget passé via Shell
        public int BudgetId { get; set; }

        // Champs privés
        private string totalAmount;
        private DateTime selectedDate = DateTime.Now;
        private string message;
        private Color messageColor = Colors.Transparent;
        private Budget selectedBudget;

        // ------------- PROPRIETES BINDÉES ----------------

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


        // ------------- COMMANDES ----------------

        public ICommand CreateBudgetCommand { get; }
        public ICommand UpdateBudgetCommand { get; }
        public ICommand DeleteBudgetCommand { get; }
        public ICommand CancelCommand { get; }

        // ------------- CONSTRUCTEUR ----------------

        public BudgetViewModel()
        {
            _budgetService = new BudgetService(App.Database);

            CreateBudgetCommand = new AsyncRelayCommand(CreateBudgetAsync);
            UpdateBudgetCommand = new AsyncRelayCommand(UpdateBudgetAsync);
            DeleteBudgetCommand = new AsyncRelayCommand(DeleteBudgetAsync);
            CancelCommand = new AsyncRelayCommand(OnCancelAsync);
        }


        // ------------- CHARGER TOUS LES BUDGETS ----------------

        public async Task LoadBudgetsAsync()
        {
            try
            {
                var list = await _budgetService.GetBudgetsAsync();
                Budgets.Clear();

                foreach (var b in list)
                    Budgets.Add(b);
            }
            catch (Exception ex)
            {
                SetMessage($"Erreur chargement budgets : {ex.Message}", Colors.Red);
            }
        }


        // ------------- CHARGER 1 BUDGET ----------------

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


        // ------------- CREATION ----------------

        private async Task CreateBudgetAsync()
        {
            if (!double.TryParse(TotalAmount, out double total))
            {
                SetMessage("Montant invalide.", Colors.Red);
                return;
            }

            var budget = new Budget(1, total)
            {
                Month = SelectedDate.Month,
                Year = SelectedDate.Year
            };

            try
            {
                bool ok = await _budgetService.AddBudgetAsync(budget);

                if (!ok)
                {
                    SetMessage("Budget déjà existant pour ce mois.", Colors.Red);
                    return;
                }

                await Shell.Current.GoToAsync("///BudgetListPage?returnMsg=Budget créé avec succès");
            }
            catch (Exception ex)
            {
                SetMessage(ex.Message, Colors.Red);
            }
        }


        // ------------- UPDATE ----------------

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

            bool ok = await _budgetService.UpdateBudgetAsync(SelectedBudget);

            if (ok)
            {
                await Shell.Current.GoToAsync("///BudgetListPage?returnMsg=Budget modifié");
            }
            else
            {
                SetMessage("Erreur lors de la modification.", Colors.Red);
            }
        }


        // ------------- DELETE ----------------

        private async Task DeleteBudgetAsync()
        {
            if (SelectedBudget == null)
            {
                SetMessage("Aucun budget à supprimer.", Colors.Red);
                return;
            }

            bool ok = await _budgetService.DeleteBudgetAsync(SelectedBudget);

            if (ok)
            {
                await Shell.Current.GoToAsync("///BudgetListPage?returnMsg=Budget supprimé");
            }
            else
            {
                SetMessage("Erreur suppression.", Colors.Red);
            }
        }


        // ------------- CANCEL ----------------

        private async Task OnCancelAsync()
        {
            await Shell.Current.GoToAsync("///BudgetListPage?returnMsg=Annulé");
        }


        // ------------- MESSAGE UTILITAIRE ----------------

        private void SetMessage(string txt, Color c)
        {
            Message = txt;
            MessageColor = c;
        }


        // Message renvoyé depuis EditBudgetPage
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
