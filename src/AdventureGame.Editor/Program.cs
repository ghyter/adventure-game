using AdventureGame.Editor;
using AdventureGame.Editor.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Radzen;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddRadzenComponents();

builder.Services.AddRadzenCookieThemeService(options =>
{
    options.Name = "MyApplicationTheme"; // The name of the cookie
    options.Duration = TimeSpan.FromDays(365); // The duration of the cookie
});

builder.Services.AddScoped<IThemePersistence, LocalStorageThemePersistence>();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IGamePackRepository, IndexedDbGamePackRepository>();
builder.Services.AddSingleton<CurrentGameService>();

var app = builder.Build();

var persist = app.Services.GetRequiredService<IThemePersistence>();
await persist.InitializeAsync();
await persist.PersistOnChangeAsync();

// Initialize CurrentGameService so it can restore persisted current game from localStorage/IndexedDB
var currentGameService = app.Services.GetRequiredService<CurrentGameService>();
await currentGameService.InitializeAsync();

await app.RunAsync();



