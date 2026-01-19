using Microsoft.Extensions.Logging;
using MoneyMate.Database;
using MoneyMate.Services;
using MoneyMate.ViewModels;
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
            builder.Services.AddSingleton<MoneyMateContext>();

            builder.Services.AddSingleton<BudgetService>();
            builder.Services.AddSingleton<CategoryService>();
            builder.Services.AddSingleton<ExpenseService>();

            builder.Services.AddTransient<BudgetViewModel>();
            builder.Services.AddTransient<CategoryViewModel>();
            builder.Services.AddTransient<ExpenseViewModel>();

            builder.Services.AddTransient<AddBudgetPage>();
            builder.Services.AddTransient<AddCategoryPage>();
            builder.Services.AddTransient<AddExpensePage>();

            builder.Services.AddSingleton<HistoryViewModel>();
            builder.Services.AddSingleton<HistoryExpensePage>();


            return builder.Build();
        }
    }
}
