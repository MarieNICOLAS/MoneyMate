using MoneyMate.ViewModels.AuthViewModel;

namespace MoneyMate.Views
{
    public partial class SignupPage : ContentPage
    {
        public SignupPage(SignupViewModel viewmodel)
        {
            InitializeComponent();
            BindingContext = viewmodel;
        }
    }
}
