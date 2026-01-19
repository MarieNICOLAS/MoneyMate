using MoneyMate.ViewModels;

namespace MoneyMate.Views
{
    public partial class DashboardPage : ContentPage
    {
        private DashboardViewModel ViewModel => BindingContext as DashboardViewModel;

        // ✅ Injection de dépendances
        public DashboardPage(DashboardViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // ✅ Rafraîchir les données à chaque fois qu'on revient sur la page
            if (ViewModel != null)
            {
                await ViewModel.LoadDashboardDataAsync();
            }
        }
    }
}