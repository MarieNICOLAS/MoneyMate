namespace MoneyMate.Views
{
    public partial class StatisticsPage : ContentPage
    {
        private readonly ViewModels.StatisticsViewModel _viewModel;

        public StatisticsPage()
        {
            InitializeComponent();
            _viewModel = new ViewModels.StatisticsViewModel();
            BindingContext = _viewModel;
        }

        private void OnRefreshClicked(object sender, EventArgs e)
        {
            _viewModel.LoadCharts();
        }
    }
}
