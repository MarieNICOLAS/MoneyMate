using System.Windows.Input;
using Microsoft.Maui.Storage;

namespace MoneyMate.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        public ICommand GoAddBudgetCommand { get; }
        public ICommand GoEditBudgetCommand { get; }
        public ICommand GoAddCategoryCommand { get; }
        public ICommand GoEditCategoryCommand { get; }
        public ICommand GoAddExpenseCommand { get; }
        public ICommand GoHistoryCommand { get; }
        public ICommand GoNotificationsCommand { get; }
        public ICommand GoStatisticsCommand { get; }
        public ICommand LogoutCommand { get; }

        public MenuViewModel()
        {
            GoAddBudgetCommand = new Command(async () => await Shell.Current.GoToAsync("///AddBudgetPage"));
            GoEditBudgetCommand = new Command(async () => await Shell.Current.GoToAsync("///EditBudgetPage"));
            GoAddCategoryCommand = new Command(async () => await Shell.Current.GoToAsync("///AddCategoryPage"));
            GoEditCategoryCommand = new Command(async () => await Shell.Current.GoToAsync("///EditCategoryPage"));
            GoAddExpenseCommand = new Command(async () => await Shell.Current.GoToAsync("///AddExpensePage"));
            GoHistoryCommand = new Command(async () => await Shell.Current.GoToAsync("///HistoryTransactionPage"));
            GoNotificationsCommand = new Command(async () => await Shell.Current.GoToAsync("///NotificationsPage"));
            GoStatisticsCommand = new Command(async () => await Shell.Current.GoToAsync("///StatisticsPage"));
            LogoutCommand = new Command(async () =>
            {
                Preferences.Clear();
                await Shell.Current.GoToAsync("///LoginPage");
            });
        }
    }
}
