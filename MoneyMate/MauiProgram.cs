using Microsoft.Extensions.Logging;
using MoneyMate.Database;
using MoneyMate.Services;
using MoneyMate.ViewModels;
using MoneyMate.ViewModels.AuthViewModel;
using MoneyMate.Views;

namespace MoneyMate
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                    fonts.AddFont("MaterialIcons-Regular.ttf", "MaterialIcons");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // ✅ 1. Database Context (Singleton)
            builder.Services.AddSingleton<MoneyMateContext>();

            // ✅ 2. Services (Singleton car ils gèrent des données partagées)
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<BudgetService>();
            builder.Services.AddSingleton<CategoryService>();
            builder.Services.AddSingleton<ExpenseService>();
            builder.Services.AddSingleton<AlertService>();

            // ✅ 3. ViewModels (Transient car instanciés à chaque navigation)
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<BudgetViewModel>();
            builder.Services.AddTransient<CategoryViewModel>();
            builder.Services.AddTransient<ExpenseViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<SignupViewModel>();

            // ✅ 4. Pages (Transient)
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<AddBudgetPage>();
            builder.Services.AddTransient<AddCategoryPage>();
            builder.Services.AddTransient<AddExpensePage>();

            return builder.Build();
        }
    }
}
