using System.Windows.Input;
using Microsoft.Maui.Controls;
using MoneyMate.Services;

namespace MoneyMate.ViewModels
{
    public class HeaderViewModel
    {
        public ICommand GoNotificationsCommand { get; }
        public ICommand LogoutCommand { get; }

        public HeaderViewModel()
        {
            GoNotificationsCommand = new Command(async () => await Shell.Current.GoToAsync("//NotificationsPage"));
            LogoutCommand = new Command(ExecuteLogout);
        }

        private async void ExecuteLogout()
        {
            AuthService.Logout();
            await Shell.Current.GoToAsync("//MainPage");
        }
    }
}
