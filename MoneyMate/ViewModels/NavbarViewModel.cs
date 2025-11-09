using System.Windows.Input;

namespace MoneyMate.ViewModels;

public class NavbarViewModel : ContentView
{
    public ICommand GoMenuCommand { get; }
    public ICommand GoHomeCommand { get; }
    public ICommand GoStatisticsCommand { get; }
    public ICommand GoAddCommand { get; }
    public ICommand GoSearchCommand { get; }

    public NavbarViewModel()
    {
        GoHomeCommand = new Command(async () => await Shell.Current.GoToAsync("//DashboardPage"));
        GoStatisticsCommand = new Command(async () => await Shell.Current.GoToAsync("//StatisticsPage"));
        GoAddCommand = new Command(async () => await Shell.Current.GoToAsync("//AddExpensePage"));
        GoSearchCommand = new Command(async () => await Shell.Current.GoToAsync("//SearchPage"));
        GoMenuCommand = new Command(async () => await Shell.Current.GoToAsync("//MenuPage"));
    }

}