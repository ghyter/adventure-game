using Microsoft.JSInterop;
using Radzen;

namespace AdventureGame.Editor.Services;

public interface IThemePersistence
{
    Task InitializeAsync();                 // read saved theme and apply
    Task PersistOnChangeAsync();            // hook ThemeService.ThemeChanged
}

public sealed class PreferencesThemePersistence(ThemeService themeService) : IThemePersistence
{
    private const string Key = "app.theme";

    public Task InitializeAsync()
    {
        var saved = Preferences.Default.Get<string?>(Key, null);
        if (!string.IsNullOrWhiteSpace(saved))
        {
            // false = don't raise ThemeChanged again when restoring
            themeService.SetTheme(saved!, false);
        }
        return Task.CompletedTask;
    }

    public Task PersistOnChangeAsync()
    {
        themeService.ThemeChanged += () =>
        {
            Preferences.Default.Set(Key, themeService.Theme);
            //return Task.CompletedTask;
        };
        return Task.CompletedTask;
    }
}