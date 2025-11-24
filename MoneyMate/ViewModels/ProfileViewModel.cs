using MoneyMate.Models;
using MoneyMate.Services;

namespace MoneyMate.ViewModels
{
    public class ProfileViewModel : BaseViewModel
    {
        private readonly AuthService _authService;

        private User? _currentUser;
        public User? CurrentUser
        {
            get => _currentUser;
            set => SetProperty(ref _currentUser, value);
        }

        public string DisplayName => CurrentUser?.Name ?? "N/A";
        public string DisplayEmail => CurrentUser?.Email ?? "N/A";

        public string DisplayCreationDate => CurrentUser?.CreatedAt.ToString("dd MMMM yyyy") ?? "N/A";
        public string DisplayLastLogin => CurrentUser?.LastLogin?.ToString("dd MMMM yyyy à HH:mm") ?? "Jamais";

        // ❌ RÉTIRER : La commande de déconnexion est dans HeaderViewModel
        // public ICommand LogoutCommand { get; } 

        public ProfileViewModel()
        {
            _authService = new AuthService(App.Database);

            // ❌ RÉTIRER : L'assignation de la commande
            // LogoutCommand = new Command(async () => await LogoutAsync()); 

            Task.Run(LoadUserData);
        }

        public async Task LoadUserData()
        {
            IsBusy = true;
            try
            {
                CurrentUser = await _authService.GetLoggedInUserAsync();

                OnPropertyChanged(nameof(DisplayName));
                OnPropertyChanged(nameof(DisplayEmail));
                OnPropertyChanged(nameof(DisplayCreationDate));
                OnPropertyChanged(nameof(DisplayLastLogin));
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}