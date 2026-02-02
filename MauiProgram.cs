using CommunityToolkit.Maui;
using controle_ja_mobile.Services;
using controle_ja_mobile.ViewModels;
using controle_ja_mobile.Views.Privates;
using controle_ja_mobile.Views.Publics;
using Microcharts.Maui;
using Microsoft.Extensions.Logging;

namespace controle_ja_mobile
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseMicrocharts()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });
            builder.Services.AddSingleton<BiometricAuthService>();
            builder.Services.AddSingleton<AccountService>();
            builder.Services.AddSingleton<ApiService>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<CategoryService>();
            builder.Services.AddSingleton<CreditCardService>();
            builder.Services.AddSingleton<DashboardService>();
            builder.Services.AddSingleton<TransactionService>();
            builder.Services.AddSingleton<VehicleService>();

            //ViewModels
            builder.Services.AddTransient<CreditCardsViewModel>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<TransactionAddViewModel>();
            builder.Services.AddTransient<VehiclesViewModel>();

            //Views
            builder.Services.AddTransient<CreditCardAddPage>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<TransactionAddPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<RegisterPage>(); 
            builder.Services.AddTransient<VehicleAddPage>();
            builder.Services.AddTransient<WelcomePage>();



#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
