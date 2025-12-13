using CommunityToolkit.Mvvm.Input;
using MoneyMate.Models;
using MoneyMate.Services;
using System.Collections.ObjectModel;
using System.Diagnostics;

namespace MoneyMate.ViewModels
{
    public class BudgetViewModel : BaseViewModel
    {
        private readonly BudgetService _budgetService;

        // -------------------------
        // PROPRIÉTÉS
        // -------------------------

        private string _totalAmount = string.Empty;
        public string TotalAmount
        {
            get => _totalAmount;
            set => SetProperty(ref _totalAmount, value);
        }

        private DateTime _selectedDate = DateTime.Now;
        public DateTime SelectedDate
        {
            get => _selectedDate;
            set => SetProperty(ref _selectedDate, value);
        }

        private string _message = string.Empty;
        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        private Color _messageColor = Colors.Transparent;
        public Color MessageColor
        {
            get => _messageColor;
            set => SetProperty(ref _messageColor, value);
        }

        public ObservableCollection<Budget> Budgets { get; } = new();

        // -------------------------
        // COMMANDES
        // -------------------------

        public IAsyncRelayCommand CreateBudgetCommand { get; }
        public IAsyncRelayCommand LoadBudgetsCommand { get; }
        public IAsyncRelayCommand<Budget> DeleteBudgetCommand { get; }
        public IAsyncRelayCommand<Budget> OpenBudgetDetailCommand { get; }

        // -------------------------
        // CONSTRUCTEUR
        // -------------------------

        public BudgetViewModel()
        {
            _budgetService = new BudgetService(App.Database);

            CreateBudgetCommand = new AsyncRelayCommand(CreateBudgetAsync);
            LoadBudgetsCommand = new AsyncRelayCommand(LoadBudgetsAsync);
            DeleteBudgetCommand = new AsyncRelayCommand<Budget>(DeleteBudgetAsync);
            OpenBudgetDetailCommand = new AsyncRelayCommand<Budget>(OpenBudgetDetailAsync);
        }

        // -------------------------
        // CHARGER LES BUDGETS
        // -------------------------

        private async Task LoadBudgetsAsync()
        {
            try
            {
                IsBusy = true;

                var budgets = await _budgetService.GetBudgetsAsync();

                Budgets.Clear();
                foreach (var b in budgets)
                    Budgets.Add(b);
            }
            catch (Exception ex)
            {
                Message = $"Erreur chargement budgets : {ex.Message}";
                MessageColor = Colors.Red;
            }
            finally
            {
                IsBusy = false;
            }
        }

        // -------------------------
        // CRÉER UN BUDGET
        // -------------------------

        private async Task CreateBudgetAsync()
        {
            if (!double.TryParse(TotalAmount, out double total))
            {
                ShowMessage("Montant invalide.", Colors.Red);
                return;
            }

            var budget = new Budget(
                userId: 1, // TODO : remplacer par l’ID utilisateur réel
                totalAmount: total
            )
            {
                Month = SelectedDate.Month,
                Year = SelectedDate.Year
            };

            try
            {
                IsBusy = true;

                bool success = await _budgetService.AddBudgetAsync(budget);
                if (!success)
                {
                    ShowMessage("Un budget existe déjà pour ce mois.", Colors.Red);
                    return;
                }

                ShowMessage("Budget créé avec succès !", Colors.Green);

                Budgets.Insert(0, budget);

                TotalAmount = string.Empty;
                SelectedDate = DateTime.Now;
            }
            catch (Exception ex)
            {
                ShowMessage("Erreur : " + ex.Message, Colors.Red);
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // -------------------------
        // SUPPRIMER UN BUDGET
        // -------------------------

        private async Task DeleteBudgetAsync(Budget? budget)
        {
            if (budget == null) return;

            try
            {
                IsBusy = true;

                bool deleted = await _budgetService.DeleteBudgetAsync(budget);
                if (deleted)
                    Budgets.Remove(budget);
            }
            catch (Exception ex)
            {
                ShowMessage("Erreur suppression : " + ex.Message, Colors.Red);
            }
            finally
            {
                IsBusy = false;
            }
        }

        // -------------------------
        // OUVRIR LE DÉTAIL D’UN BUDGET
        // -------------------------

        private async Task OpenBudgetDetailAsync(Budget? budget)
        {
            if (budget == null) return;

            // Navigation MAUI Shell
            await Shell.Current.GoToAsync($"budgetdetail?budgetId={budget.Id}");
        }

        // -------------------------
        // OUTILS
        // -------------------------

        private void ShowMessage(string text, Color color)
        {
            Message = text;
            MessageColor = color;
        }
    }
}
