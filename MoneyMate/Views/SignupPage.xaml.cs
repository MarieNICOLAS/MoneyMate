using MoneyMate.ViewModels;

namespace MoneyMate.Views
{
    public partial class SignupPage : ContentPage
    {
        public SignupPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            if (BindingContext is SignupViewModel vm)
            {
                vm.Name = string.Empty;
                vm.Email = string.Empty;
                vm.Password = string.Empty;
                vm.Message = string.Empty;
                vm.MessageColor = Colors.Transparent;
            }

            // Reset UI
            SignupStrengthBar.Progress = 0;
            SignupStrengthBar.ProgressColor = Colors.Red;

            SignupRequirementLabel.Text = string.Empty;

            PasswordEntry.IsPassword = true;
            EyeBtn.Source = "open_eye_icon.png";
        }
    }
}
