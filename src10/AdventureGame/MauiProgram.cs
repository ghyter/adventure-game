using AdventureGame.DependencyInjection;
using AdventureGame.Editor.Services;
using AdventureGame.Engine.Services;
using Microsoft.Extensions.Logging;
using Radzen;

namespace AdventureGame
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
            builder.Services.AddRadzenComponents();


            builder.Services.AddRadzenCookieThemeService(options =>
            {
                options.Name = "MyApplicationTheme"; // The name of the cookie
                options.Duration = TimeSpan.FromDays(365); // The duration of the cookie
            });

            builder.Services.AddScoped<IThemePersistence, PreferencesThemePersistence>();

            // Replace IndexedDbGamePackRepository with file-based AppData repository
            builder.Services.AddScoped<IGamePackRepository, AppDataFileGamePackRepository>();
            builder.Services.AddSingleton<CurrentGameService>();
            builder.Services.AddSingleton<LogService>();

            // Register game element dependencies (effects, conditions, catalogs)
            builder.AddGameElementDependencies();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
