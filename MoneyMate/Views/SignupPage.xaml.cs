namespace MoneyMate.Views;

public partial class SignupPage : ContentPage
{
    public SignupPage()
    {
        InitializeComponent();
    }

    private async void OnBackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//MainPage");
    }
}
