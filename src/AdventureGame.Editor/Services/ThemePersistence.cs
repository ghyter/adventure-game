using Microsoft.JSInterop;
using Radzen;

namespace AdventureGame.Editor.Services;

public interface IThemePersistence
{
    Task InitializeAsync();                 // read saved theme and apply
    Task PersistOnChangeAsync();            // hook ThemeService.ThemeChanged
}

public sealed class LocalStorageThemePersistence(
    IJSRuntime js, ThemeService themeService) : IThemePersistence
{
    const string Key = "app.theme";

    public async Task InitializeAsync()
    {
        var saved = await js.InvokeAsync<string?>("localStorage.getItem", Key);
        if (!string.IsNullOrWhiteSpace(saved))
        {
            // false = don't raise ThemeChanged again when restoring
            themeService.SetTheme(saved!, false);
        }
    }

    public async Task PersistOnChangeAsync()
    {
        themeService.ThemeChanged += async () =>
        {
            await js.InvokeVoidAsync("localStorage.setItem", Key, themeService.Theme);
        };
        await Task.CompletedTask;
    }
}
