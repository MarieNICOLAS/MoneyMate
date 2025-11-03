using System.Windows.Input;
using MoneyMate.Models;
using Microsoft.Maui.Graphics;

namespace MoneyMate.ViewModels
{
    public class SignupViewModel /*: BaseViewModel*/
    {
        //private readonly AuthService _authService;

        //public string FullName { get; set; } = "";
        //public string Email { get; set; } = "";
        //public string Password { get; set; } = "";
        //public string Message { get; set; } = "";
        //public Color MessageColor { get; set; } = Colors.Black;

        //public ICommand SignupCommand { get; }

        //public SignupViewModel()
        //{
        //    _authService = new AuthService(App.Database);
        //    SignupCommand = new Command(async () => await SignupAsync());
        //}

        //private async Task SignupAsync()
        //{
        //    Message = "";

        //    if (string.IsNullOrWhiteSpace(Email) ||
        //        string.IsNullOrWhiteSpace(Password) ||
        //        string.IsNullOrWhiteSpace(FullName))
        //    {
        //        Message = "Veuillez remplir tous les champs.";
        //        MessageColor = Colors.Red;
        //        OnPropertyChanged(nameof(Message));
        //        OnPropertyChanged(nameof(MessageColor));
        //        return;
        //    }

        //    var success = await _authService.RegisterAsync(Email, Password, FullName);

        //    if (success)
        //    {
        //        Message = "Inscription réussie ! Vous pouvez maintenant vous connecter.";
        //        MessageColor = Colors.Green;
        //        OnPropertyChanged(nameof(Message));
        //        OnPropertyChanged(nameof(MessageColor));
        //    }
        //    else
        //    {
        //        Message = "Cet email est déjà utilisé.";
        //        MessageColor = Colors.Red;
        //        OnPropertyChanged(nameof(Message));
        //        OnPropertyChanged(nameof(MessageColor));
        //    }
        //}
    }
}
