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
            builder.Services.AddSingleton<BudgetService>();
            builder.Services.AddTransient<BudgetViewModel>();
            builder.Services.AddTransient<AddBudgetPage>();

            builder.Services.AddSingleton<CategoryService>();
            builder.Services.AddTransient<CategoryViewModel>();

            builder.Services.AddSingleton<MoneyMateContext>();

            return builder.Build();
        }
    }
}
