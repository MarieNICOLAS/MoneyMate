using CommunityToolkit.Mvvm.Input;
using MoneyMate.Services;

namespace MoneyMate.ViewModels.ComponentsViewModel
{
    public partial class HeaderViewModel : BaseViewModel
    {
        // CONSTRUCTEUR ➜ Injection complète
        public HeaderViewModel(
            AuthService authService,
            BudgetService budgetService,
            ExpenseService expenseService,
            CategoryService categoryService)
            : base(authService, budgetService, expenseService, categoryService)
        {
        }

        // 1️. Aller aux notifications
        [RelayCommand]
        private async Task GoNotificationsAsync()
        {
            await Shell.Current.GoToAsync("//NotificationsPage");
        }


        // 3️. Aller au login
        [RelayCommand]
        private async Task GoLoginAsync()
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
