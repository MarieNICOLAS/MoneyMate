using MoneyMate.Graphics;
using MoneyMate.Services;
using MoneyMate.ViewModels;

namespace MoneyMate.Views;

public partial class StatisticsPage : ContentPage
{
    private LineChartDrawable _lineDrawable;
    private BarChartDrawable _barDrawable;
    private StatisticsViewModel _vm;

    public StatisticsPage()
    {
        InitializeComponent();

        var expenseService = IPlatformApplication.Current.Services.GetService<ExpenseService>();
        var budgetService = IPlatformApplication.Current.Services.GetService<BudgetService>();
        var categoryService = IPlatformApplication.Current.Services.GetService<CategoryService>();

        _vm = new StatisticsViewModel(expenseService, budgetService, categoryService);
        BindingContext = _vm;

        _lineDrawable = new LineChartDrawable();
        _barDrawable = new BarChartDrawable();

        LineChartView.Drawable = _lineDrawable;
        BarChartView.Drawable = _barDrawable;

        _vm.LinePoints.CollectionChanged += (s, e) => UpdateLineChart();
        _vm.BudgetPoints.CollectionChanged += (s, e) => UpdateLineChart();
        _vm.BarMonths.CollectionChanged += (s, e) =>
        {
            if (_vm.BarMonths.Count == 12)
                UpdateBarChart();
        };
        _vm.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(_vm.BarMaxValue))
                UpdateBarChart();
        };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await Task.Delay(100);
        UpdateLineChart();
        UpdateBarChart();
    }

    private void UpdateLineChart()
    {
        _lineDrawable.Points = _vm.LinePoints.ToList();
        _lineDrawable.BudgetPoints = _vm.BudgetPoints.ToList();
        LineChartView.Invalidate();
    }

    private void UpdateBarChart()
    {
        _barDrawable.Months = _vm.BarMonths.ToList();
        _barDrawable.MaxValue = _vm.BarMaxValue;
        BarChartView.Invalidate();
    }
}