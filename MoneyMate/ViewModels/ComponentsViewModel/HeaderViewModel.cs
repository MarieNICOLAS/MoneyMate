using MoneyMate.Services;
using System.Windows.Input;

namespace MoneyMate.ViewModels.ComponentsViewModel
{
    public class HeaderViewModel : BaseViewModel
    {
        public ICommand GoNotificationsCommand { get; }
        public ICommand LogoutCommand { get; }
        public ICommand LoginCommand { get; }

        public HeaderViewModel()
        {
            GoNotificationsCommand = new Command(async () => await Shell.Current.GoToAsync("//NotificationsPage"));
            LogoutCommand = new Command(ExecuteLogout);
            LoginCommand = new Command(async () => await ExecuteLogin());
        }

        private async void ExecuteLogout()
        {
            AuthService.Logout();
            await Shell.Current.GoToAsync("//MainPage");
        }

        private async Task ExecuteLogin()
        {
            // 🚀 Navigation directe vers la page de connexion
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
