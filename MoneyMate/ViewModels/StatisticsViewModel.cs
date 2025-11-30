using System;
using System.Collections.ObjectModel;
using Microcharts;
using SkiaSharp;
using System.ComponentModel;

namespace MoneyMate.ViewModels
{
    public class StatisticsViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        public ObservableCollection<string> Periods { get; set; } = new() { "Mois", "Année" };

        private string _selectedPeriod;
        public string SelectedPeriod
        {
            get => _selectedPeriod;
            set
            {
                if (_selectedPeriod != value)
                {
                    _selectedPeriod = value;
                    OnPropertyChanged(nameof(SelectedPeriod));
                    LoadCharts();
                }
            }
        }

        private Chart _lineChart;
        public Chart LineChart
        {
            get => _lineChart;
            set { _lineChart = value; OnPropertyChanged(nameof(LineChart)); }
        }

        private Chart _barChart;
        public Chart BarChart
        {
            get => _barChart;
            set { _barChart = value; OnPropertyChanged(nameof(BarChart)); }
        }

        private Chart _donutChart;
        public Chart DonutChart
        {
            get => _donutChart;
            set { _donutChart = value; OnPropertyChanged(nameof(DonutChart)); }
        }

        private string _totalIncome;
        public string TotalIncome
        {
            get => _totalIncome;
            set { _totalIncome = value; OnPropertyChanged(nameof(TotalIncome)); }
        }

        private string _totalExpense;
        public string TotalExpense
        {
            get => _totalExpense;
            set { _totalExpense = value; OnPropertyChanged(nameof(TotalExpense)); }
        }

        private string _remainingBudget;
        public string RemainingBudget
        {
            get => _remainingBudget;
            set { _remainingBudget = value; OnPropertyChanged(nameof(RemainingBudget)); }
        }

        public ObservableCollection<CategorySummary> TopCategories { get; set; } = new();

        public StatisticsViewModel()
        {
            SelectedPeriod = "Mois";
            LoadCharts();
        }

        public void LoadCharts()
        {
            // Données fictives (à remplacer plus tard)
            LineChart = new LineChart
            {
                Entries = new[]
                {
                    new ChartEntry(300){ Label = "Jan", ValueLabel = "300", Color = SKColor.Parse("#6CC57C") },
                    new ChartEntry(450){ Label = "Fév", ValueLabel = "450", Color = SKColor.Parse("#6CC57C") },
                    new ChartEntry(500){ Label = "Mar", ValueLabel = "500", Color = SKColor.Parse("#E57373") },
                    new ChartEntry(480){ Label = "Avr", ValueLabel = "480", Color = SKColor.Parse("#E57373") },
                    new ChartEntry(520){ Label = "Mai", ValueLabel = "520", Color = SKColor.Parse("#E57373") },
                },
                LabelTextSize = 28,
                LineSize = 8,
                BackgroundColor = SKColors.Transparent
            };

            BarChart = new BarChart
            {
                Entries = new[]
                {
                    new ChartEntry(350){ Label = "Jan", ValueLabel = "350", Color = SKColor.Parse("#9DBAD5") },
                    new ChartEntry(400){ Label = "Fév", ValueLabel = "400", Color = SKColor.Parse("#9DBAD5") },
                    new ChartEntry(450){ Label = "Mar", ValueLabel = "450", Color = SKColor.Parse("#9DBAD5") },
                    new ChartEntry(500){ Label = "Avr", ValueLabel = "500", Color = SKColor.Parse("#9DBAD5") },
                },
                LabelTextSize = 28,
                BackgroundColor = SKColors.Transparent
            };

            DonutChart = new DonutChart
            {
                Entries = new[]
                {
                    new ChartEntry(320){ Label = "Alimentation", ValueLabel = "320€", Color = SKColor.Parse("#EB9362") },
                    new ChartEntry(150){ Label = "Transport", ValueLabel = "150€", Color = SKColor.Parse("#9DBAD5") },
                    new ChartEntry(180){ Label = "Loisirs", ValueLabel = "180€", Color = SKColor.Parse("#6CC57C") },
                },
                LabelTextSize = 28,
                BackgroundColor = SKColors.Transparent
            };

            TotalIncome = "2250 €";
            TotalExpense = "1200 €";
            RemainingBudget = "1050 €";

            TopCategories.Clear();
            TopCategories.Add(new CategorySummary { Name = "Alimentation", Amount = 320 });
            TopCategories.Add(new CategorySummary { Name = "Transport", Amount = 150 });
            TopCategories.Add(new CategorySummary { Name = "Loisirs", Amount = 180 });
        }
    }

    public class CategorySummary
    {
        public string Name { get; set; }
        public float Amount { get; set; }
    }
}
