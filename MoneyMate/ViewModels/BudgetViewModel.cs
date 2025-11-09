using MoneyMate.Database;
using MoneyMate.Models;
using MoneyMate.Services;
using SQLite;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;

namespace MoneyMate.ViewModels
{
    public class BudgetViewModel : BaseViewModel
    {
        private readonly BudgetService _budgetService;

        public ObservableCollection<Budget> Budgets { get; } = new();
        public ICommand CreateBudgetCommand { get; }
        private double _totalAmount;
        public double TotalAmount
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

        private async Task CreateBudgetAsync()
        {
            var budget = new Budget(
                userId: 1, // remplacer par l'ID réel de l'utilisateur connecté
                totalAmount: TotalAmount
            )
            {
                Month = SelectedDate.Month,
                Year = SelectedDate.Year
            };

            await AddBudget(budget);

            // Optionnel : reset du formulaire
            TotalAmount = 0;
            SelectedDate = DateTime.Now;
        }
        public BudgetViewModel(BudgetService budgetService)
        {
            _budgetService = budgetService;
            CreateBudgetCommand = new AsyncRelayCommand(CreateBudgetAsync);

            LoadBudgets();
        }

        public async Task LoadBudgets()
        {
            var budgets = await _budgetService.GetBudgetsAsync();
            Budgets.Clear();
            foreach (var b in budgets)
                Budgets.Add(b);
        }

        public async Task AddBudget(Budget budget)
        {
            await _budgetService.AddBudgetAsync(budget);
            await LoadBudgets(); // recharge la liste
        }

        public async Task UpdateBudget(Budget budget)
        {
            await _budgetService.UpdateBudgetAsync(budget);
            await LoadBudgets();
        }

        public async Task DeleteBudget(Budget budget)
        {
            await _budgetService.DeleteBudgetAsync(budget);
            await LoadBudgets();
        }


    }
}