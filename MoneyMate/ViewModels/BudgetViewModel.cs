using CommunityToolkit.Mvvm.Input;
using MoneyMate.Database;
using MoneyMate.Models;
using MoneyMate.Services;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MoneyMate.ViewModels
{
    public class BudgetViewModel : BaseViewModel
    {
        private readonly BudgetService _budgetService;

        // --- Champs privés ---
        private string totalAmount = string.Empty;
        private DateTime selectedDate = DateTime.Now;
        private string message = string.Empty;
        private Color messageColor = Colors.Transparent;

        // --- Propriétés bindées ---
        public string TotalAmount
        {
            get => totalAmount;
            set { if (totalAmount != value) { totalAmount = value; OnPropertyChanged(); } }
        }

        public DateTime SelectedDate
        {
            get => selectedDate;
            set { if (selectedDate != value) { selectedDate = value; OnPropertyChanged(); } }
        }

        public string Message
        {
            get => message;
            set { if (message != value) { message = value; OnPropertyChanged(); } }
        }

        public Color MessageColor
        {
            get => messageColor;
            set { if (messageColor != value) { messageColor = value; OnPropertyChanged(); } }
        }

        // --- Liste de budgets ---
        public ObservableCollection<Budget> Budgets { get; } = new();

        // --- Commandes ---
        public ICommand CreateBudgetCommand { get; }
        public ICommand CancelCommand { get; }

        // --- Constructeur ---
        public BudgetViewModel()
        {
            _budgetService = new BudgetService(App.Database);

            CreateBudgetCommand = new AsyncRelayCommand(CreateBudgetAsync);
            CancelCommand = new Command(async () => await Shell.Current.GoToAsync("//DashboardPage"));
        }

        // --- Méthodes ---
        private async Task CreateBudgetAsync()
        {
            // Validation
            if (!double.TryParse(TotalAmount, out double total))
            {
                Message = "Montant invalide";
                MessageColor = Colors.Red;
                return;
            }

            var budget = new Budget(
                userId: 1, // remplacer par l'ID réel de l'utilisateur connecté
                totalAmount: total
            )
            {
                Month = SelectedDate.Month,
                Year = SelectedDate.Year
            };

            try
            {
                bool success = await _budgetService.AddBudgetAsync(budget);

                if (!success)
                {
                    Message = "Un budget pour ce mois existe déjà.";
                    MessageColor = Colors.Red;
                    return;
                }

                Message = "Budget créé avec succès";
                MessageColor = Colors.Green;

                // Reset formulaire
                TotalAmount = string.Empty;
                SelectedDate = DateTime.Now;

            }
            catch (Exception ex)
            {
                Message = $"Erreur : {ex.Message}";
                MessageColor = Colors.Red;
                Debug.WriteLine(ex);
            }
        }

        public async Task LoadBudgetsAsync()
        {
            try
            {
                var budgets = await _budgetService.GetBudgetsAsync();
                Budgets.Clear();
                foreach (var b in budgets)
                    Budgets.Add(b);
            }
            catch (Exception ex)
            {
                Message = $"Erreur chargement budgets : {ex.Message}";
                MessageColor = Colors.Red;
                Debug.WriteLine(ex);
            }
        }

        public async Task DeleteBudgetAsync(Budget budget)
        {
            if (budget == null) return;

            try
            {
                bool deleted = await _budgetService.DeleteBudgetAsync(budget);
                if (deleted)
                    Budgets.Remove(budget);
            }
            catch (Exception ex)
            {
                Message = $"Erreur suppression budget : {ex.Message}";
                MessageColor = Colors.Red;
                Debug.WriteLine(ex);
            }
        }

        public async Task UpdateBudgetAsync(Budget budget)
        {
            if (budget == null) return;

            try
            {
                bool updated = await _budgetService.UpdateBudgetAsync(budget);
                if (updated)
                {
                    // recharge la liste pour refléter les changements
                    await LoadBudgetsAsync();
                }
            }
            catch (Exception ex)
            {
                Message = $"Erreur mise à jour budget : {ex.Message}";
                MessageColor = Colors.Red;
                Debug.WriteLine(ex);
            }
        }
    }
}