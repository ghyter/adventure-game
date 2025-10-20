using System;
using System.Threading.Tasks;
using AdventureGame.Engine.Models;
using AdventureGame.Engine.Runtime;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace AdventureGame.Editor.Services;

public sealed class CurrentGameService
{
    private const string LocalStorageKey = "adventure_current_game_id";

    private readonly IServiceProvider _services;

    public event Action? OnChange;

    public GamePack? CurrentPack { get; private set; }
    public GameSession? Session { get; private set; }

    public bool HasCurrent => CurrentPack is not null;

    public CurrentGameService(IServiceProvider services)
    {
        _services = services ?? throw new ArgumentNullException(nameof(services));
    }

    /// <summary>
    /// Initialize the service: restores previously selected GamePack Id from localStorage
    /// (if present) and attempts to load it from the repository (IndexedDB).
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            using var scope = _services.CreateScope();
            var js = scope.ServiceProvider.GetRequiredService<IJSRuntime>();
            var repo = scope.ServiceProvider.GetRequiredService<IGamePackRepository>();

            await repo.InitializeAsync();

            var savedId = await js.InvokeAsync<string?>("localStorage.getItem", LocalStorageKey);
            if (!string.IsNullOrWhiteSpace(savedId))
            {
                var pack = await repo.GetByIdAsync(savedId);
                if (pack is not null)
                {
                    // Use clone to avoid accidental mutation of stored item
                    CurrentPack = pack.Clone();
                    Session = GameSession.NewGame(CurrentPack);
                    NotifyChanged();
                }
                else
                {
                    // Stale entry - remove it
                    await js.InvokeVoidAsync("localStorage.removeItem", LocalStorageKey);
                }
            }
        }
        catch
        {
            // Swallow JS/IndexedDB errors during initialization - functionality is optional.
        }
    }

    public void SetCurrent(GamePack pack)
    {
        if (pack is null) throw new ArgumentNullException(nameof(pack));
        // Use a clone so later edits in the editor/list don't unintentionally mutate the runtime copy.
        CurrentPack = pack.Clone();
        Session = GameSession.NewGame(CurrentPack);
        // Persist selection to localStorage (fire-and-forget)
        _ = SaveCurrentIdAsync(CurrentPack.Id.ToString());
        NotifyChanged();
    }

    public void ClearCurrent()
    {
        CurrentPack = null;
        Session = null;
        // Remove persisted selection (fire-and-forget)
        _ = SaveCurrentIdAsync(null);
        NotifyChanged();
    }

    private async Task SaveCurrentIdAsync(string? id)
    {
        try
        {
            using var scope = _services.CreateScope();
            var js = scope.ServiceProvider.GetRequiredService<IJSRuntime>();
            if (string.IsNullOrWhiteSpace(id))
            {
                await js.InvokeVoidAsync("localStorage.removeItem", LocalStorageKey);
            }
            else
            {
                await js.InvokeVoidAsync("localStorage.setItem", LocalStorageKey, id);
            }
        }
        catch
        {
            // Ignore failures to persist selection.
        }
    }

    private void NotifyChanged() => OnChange?.Invoke();
}