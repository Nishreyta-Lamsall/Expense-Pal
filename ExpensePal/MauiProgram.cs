using Microsoft.Extensions.Logging;
using ExpensePal.Services.Interface;
using ExpensePal.Services;

namespace ExpensePal
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
                });

                builder.Services.AddMauiBlazorWebView();
                builder.Services.AddScoped<DebtService>();
                builder.Services.AddScoped<TransactionService>();
                builder.Services.AddScoped<DashboardService>();



#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            // Register the IUser service with its implementation
            builder.Services.AddScoped<IUser, UserService>();

            return builder.Build();
        }
    }
}
