using MoneyMate.ViewModels.AuthViewModel;

namespace MoneyMate.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginViewModel viewmodel)
        {
            InitializeComponent();
            BindingContext = viewmodel;
        }
    }
}
