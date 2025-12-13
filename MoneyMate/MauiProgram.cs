using Microsoft.Extensions.Logging;
using MoneyMate.Database;
using MoneyMate.Services;
using MoneyMate.ViewModels;
using MoneyMate.ViewModels.AuthViewModel;
using MoneyMate.ViewModels.ComponentsViewModel;
using MoneyMate.Views;
using MoneyMate.Views.BudgetViews;
using MoneyMate.Views.ExpensesViews;
using MoneyMate.Views.CategoriesViews;
//using MoneyMate.Views.AuthViews;
using CommunityToolkit.Maui;
using MoneyMate.Components;

namespace MoneyMate
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();

            // -------------------------------------------------
            // 1. CONFIGURATION DE L’APPLICATION
            // -------------------------------------------------
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                })
                .UseMauiCommunityToolkit(); // Toasts, Alerts, etc.

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // 2. BASE DE DONNÉES SQLITE (Singleton)
            builder.Services.AddSingleton<MoneyMateContext>();


            // 3. SERVICES MÉTIER (Singleton)
            // Chaque service doit être unique → 1 seule connexion SQLite
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<BudgetService>();
            builder.Services.AddSingleton<CategoryService>();
            builder.Services.AddSingleton<ExpenseService>();


            
            // 4. VIEWMODELS (Transient)
            // Une instance par page → normal (données différentes)
            
            // Base
            builder.Services.AddTransient<BaseViewModel>();

            // Auth
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<SignupViewModel>();

            // Categories
            builder.Services.AddTransient<CategoryViewModel>();

            // Budgets
            builder.Services.AddTransient<BudgetViewModel>();

            // Expenses
            builder.Services.AddTransient<ExpenseViewModel>();

            // Components ViewModels
            builder.Services.AddTransient<HeaderViewModel>();
            builder.Services.AddTransient<NavbarViewModel>();


            // 5. VIEWS (Pages XAML) — Transient
            
            // Auth Views
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<SignupPage>();

            //Header Views
            builder.Services.AddTransient<Header>();
            builder.Services.AddTransient<HeaderAuthenticated>();

            // Categories Views
            builder.Services.AddTransient<AddCategoryPage>();
            builder.Services.AddTransient<EditCategoryPage>();

            // Budget Views
            builder.Services.AddTransient<AddBudgetPage>();
            builder.Services.AddTransient<BudgetListPage>();
            builder.Services.AddTransient<EditBudgetPage>();

            // Expense Views
            builder.Services.AddTransient<AddExpensePage>();
            builder.Services.AddTransient<EditExpensePage>();
            //builder.Services.AddTransient<HistoryExpensePage>();

            // Dashboard Views
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<NotificationsPage>();



            // 6. CONSTRUCTION FINALE
            return builder.Build();
        }
    }
}
