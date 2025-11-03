namespace MoneyMate
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnLoginClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("///LoginPage");
        }

        private async void OnSignupClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("///SignupPage");
        }

    }
}
