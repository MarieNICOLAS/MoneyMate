using MoneyMate.Models;
using MoneyMate.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace MoneyMate.ViewModels
{
    public class ExpenseViewModel : BaseViewModel
    {
        private readonly ExpenseService _expenseService;

        public ObservableCollection<Expense> Expenses { get; set; } = new();
        public Expense NewExpense { get; set; } = new();

        public ICommand AddExpenseCommand { get; }
        public ICommand DeleteExpenseCommand { get; }
        public ICommand LoadExpensesCommand { get; }

        public ExpenseViewModel()
        {
            _expenseService = new ExpenseService();

            AddExpenseCommand = new Command(async () => await AddExpense());
            DeleteExpenseCommand = new Command<Expense>(async (e) => await DeleteExpense(e));
            LoadExpensesCommand = new Command(async () => await LoadExpenses());
        }

        private async Task AddExpense()
        {
            if (string.IsNullOrWhiteSpace(NewExpense.Description) || NewExpense.Amount <= 0)
                return;

            await _expenseService.AddExpenseAsync(NewExpense);
            Expenses.Add(NewExpense);
            NewExpense = new Expense();
            OnPropertyChanged(nameof(NewExpense));
        }

        private async Task DeleteExpense(Expense expense)
        {
            await _expenseService.DeleteExpenseAsync(expense);
            Expenses.Remove(expense);
        }

        private async Task LoadExpenses()
        {
            Expenses.Clear();
            var list = await _expenseService.GetExpensesAsync();
            foreach (var e in list) Expenses.Add(e);
        }
    }
}
