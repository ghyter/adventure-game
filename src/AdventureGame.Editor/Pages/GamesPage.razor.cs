using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AdventureGame.Engine.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using NUlid;
using Radzen;

namespace AdventureGame.Editor.Pages;

public partial class GamesPage : ComponentBase
{
    [Inject] private DialogService DialogService { get; set; } = null!;
    [Inject] private NotificationService NotificationService { get; set; } = null!;
    [Inject] private IGamePackRepository Repo { get; set; } = null!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = null!;
    [Inject] private AdventureGame.Editor.Services.CurrentGameService CurrentGameService { get; set; } = null!;

    private readonly List<GamePack> gamePacks = new();

    private record SortItem(string Value, string Text);
    private readonly List<SortItem> sortOptions = new()
    {
        new SortItem("name", "Sort: Name (A→Z)"),
        new SortItem("recent", "Sort: Recent (Modified)"),
        new SortItem("created", "Sort: Created")
    };

    private const string SortStorageKey = "AdventureGame.GamesPage.SelectedSort";

    private string selectedSort = "recent";

    protected override async Task OnInitializedAsync()
    {
        await Repo.InitializeAsync();
        var list = await Repo.GetAllAsync();
        gamePacks.Clear();
        gamePacks.AddRange(list);

        // Attempt to restore saved sort selection from localStorage (if available)
        try
        {
            var stored = await JSRuntime.InvokeAsync<string>("localStorage.getItem", SortStorageKey);
            if (!string.IsNullOrWhiteSpace(stored) && sortOptions.Any(s => s.Value == stored))
            {
                selectedSort = stored!;
            }
        }
        catch
        {
            // ignore if localStorage isn't available or fails
        }
    }

    private async Task OnSortChanged(object value)
    {
        selectedSort = value?.ToString() ?? "recent";

        // Persist the user's choice to localStorage so it survives reloads
        try
        {
            await JSRuntime.InvokeVoidAsync("localStorage.setItem", SortStorageKey, selectedSort);
        }
        catch
        {
            // ignore storage failures
        }

        await InvokeAsync(StateHasChanged);
    }

    private IEnumerable<GamePack> OrderedPacks()
        => selectedSort switch
        {
            "name" => gamePacks.OrderBy(g => SortKey(g.Name), StringComparer.OrdinalIgnoreCase),
            "created" => gamePacks.OrderByDescending(g => g.CreatedAt),
            _ => gamePacks.OrderByDescending(g => g.ModifiedAt) // recent by default
        };

    // Helper to produce a sort key that ignores leading articles like "a", "an", "the".
    private static string SortKey(string? title)
    {
        if (string.IsNullOrWhiteSpace(title)) return string.Empty;
        var s = title.Trim();
        var lower = s.ToLowerInvariant();
        // check common English articles
        if (lower.StartsWith("a ")) return s.Substring(2).TrimStart();
        if (lower.StartsWith("an ")) return s.Substring(3).TrimStart();
        if (lower.StartsWith("the ")) return s.Substring(4).TrimStart();
        return s;
    }

    private async void NewGamePack()
    {
        var model = new GamePack();
        await OpenEditorAsync(model, true);
    }

    private async void EditGamePack(GamePack gp)
    {
        var model = gp.Clone();
        await OpenEditorAsync(model, false);
    }

    private async Task OpenEditorAsync(GamePack model, bool isNew)
    {
        var parameters = new Dictionary<string, object?> { ["Model"] = model, ["IsNew"] = isNew };
        var result = await DialogService.OpenSideAsync<AdventureGame.Editor.Components.GamePackSideEditor>(
            title: isNew ? "New GamePack" : "Edit GamePack",
            parameters: parameters,
            options: new SideDialogOptions() { Width = "500px" }
        );

        if (result is GamePack saved)
        {
            if (gamePacks.Any(g => g.Id == saved.Id))
            {
                await Repo.UpdateAsync(saved);
                var idx = gamePacks.FindIndex(g => g.Id == saved.Id);
                gamePacks[idx] = saved.Clone();
            }
            else
            {
                await Repo.AddAsync(saved);
                gamePacks.Insert(0, saved.Clone());
            }

            NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Success, Summary = isNew ? "Created" : "Saved", Detail = $"GamePack '{saved.Name}' persisted.", Duration = 3000 });
            await InvokeAsync(StateHasChanged);
        }
    }

    private async Task ConfirmDelete(GamePack gp)
    {
        var confirmed = await DialogService.Confirm($"Delete '{gp.Name}'? This action cannot be undone.", "Confirm Delete",
            new ConfirmOptions() { OkButtonText = "Delete", CancelButtonText = "Cancel" });

        if (confirmed == true)
        {
            await DeleteGamePack(gp);
            StateHasChanged();
        }
    }

    private async Task DeleteGamePack(GamePack gp)
    {
        await Repo.DeleteAsync(gp.Id.ToString());
        var removed = gamePacks.RemoveAll(g => g.Id == gp.Id);
        if (removed > 0)
        {
            // If deleted pack was current, clear selection.
            if (CurrentGameService.CurrentPack is not null && CurrentGameService.CurrentPack.Id == gp.Id)
            {
                CurrentGameService.ClearCurrent();
            }

            NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Success, Summary = "Deleted", Detail = $"GamePack '{gp.Name}' deleted.", Duration = 3000 });
        }
    }

    private async Task ExportGamePack(GamePack gp)
    {
        var json = gp.ToJson();
        var bytes = Encoding.UTF8.GetBytes(json);
        var base64 = Convert.ToBase64String(bytes);

        var safeName = SanitizeFileName(string.IsNullOrWhiteSpace(gp.Name) ? "gamepack" : gp.Name);
        var fileName = $"{safeName}-{gp.Id}.json";

        await JSRuntime.InvokeVoidAsync("blazorDownloadFile", fileName, "application/json;charset=utf-8", base64);

        NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Info, Summary = "Exported", Detail = $"GamePack '{gp.Name}' downloaded ({json.Length} chars).", Duration = 3000 });
    }

    // Clone an existing GamePack as a new pack (new Ulid + timestamps), persist and insert into the list.
    private async Task CloneGamePack(GamePack gp)
    {
        if (gp == null) return;

        // Deep clone then assign new identity/timestamps
        var clone = gp.Clone();
        clone.Id = Ulid.NewUlid();
        clone.CreatedAt = DateTime.UtcNow;
        clone.ModifiedAt = DateTime.UtcNow;

        // Append "(Clone)" to the cloned pack name if not already present
        clone.Name = AppendCloneSuffix(clone.Name);

        await Repo.AddAsync(clone);
        gamePacks.Insert(0, clone.Clone()); // insert a fresh copy for UI list

        NotificationService.Notify(new NotificationMessage
        {
            Severity = NotificationSeverity.Success,
            Summary = "Cloned",
            Detail = $"GamePack '{gp.Name}' cloned.",
            Duration = 3000
        });

        await InvokeAsync(StateHasChanged);
    }

    private static string AppendCloneSuffix(string? name)
    {
        var n = (name ?? "").Trim();
        if (n.EndsWith(" (Clone)", StringComparison.OrdinalIgnoreCase))
            return n;
        return string.IsNullOrEmpty(n) ? "(Clone)" : $"{n} (Clone)";
    }

    // Import handler: reads uploaded JSON, deserializes and validates, then saves.
    private async Task ImportFile(InputFileChangeEventArgs e)
    {
        var file = e.File;
        if (file == null)
        {
            return;
        }

        try
        {
            // Limit to 5 MB
            const long maxBytes = 5 * 1024 * 1024;
            using var stream = file.OpenReadStream(maxBytes);
            using var reader = new System.IO.StreamReader(stream);
            var json = await reader.ReadToEndAsync();

            GamePack? pack;
            try
            {
                pack = GamePack.FromJson(json);
            }
            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = "Import failed", Detail = $"Invalid JSON: {ex.Message}", Duration = 5000 });
                return;
            }

            if (pack == null || string.IsNullOrWhiteSpace(pack.Name))
            {
                NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Warning, Summary = "Invalid GamePack", Detail = "Uploaded file is not a valid GamePack (missing name).", Duration = 5000 });
                return;
            }

            // If pack already exists, prompt for overwrite or clone
            var exists = gamePacks.Any(g => g.Id == pack.Id);
            if (exists)
            {
                // First: ask overwrite
                var overwrite = await DialogService.Confirm($"A GamePack with the same Id already exists: '{pack.Name}'. Overwrite it?", "Overwrite GamePack",
                    new ConfirmOptions() { OkButtonText = "Overwrite", CancelButtonText = "Cancel" });

                if (overwrite == true)
                {
                    // Overwrite existing
                    await Repo.UpdateAsync(pack);
                    var idx = gamePacks.FindIndex(g => g.Id == pack.Id);
                    if (idx >= 0) gamePacks[idx] = pack.Clone();
                    NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Success, Summary = "Imported", Detail = $"GamePack '{pack.Name}' overwritten.", Duration = 3000 });
                    await InvokeAsync(StateHasChanged);
                    return;
                }

                // If user cancelled overwrite, offer cloning
                var cloneConfirm = await DialogService.Confirm($"Create a cloned copy of '{pack.Name}' instead? The clone will get a new Id.", "Clone GamePack",
                    new ConfirmOptions() { OkButtonText = "Clone", CancelButtonText = "Cancel" });

                if (cloneConfirm == true)
                {
                    // Make a cloned new pack with a fresh Ulid
                    pack.Id = Ulid.NewUlid();
                    pack.CreatedAt = DateTime.UtcNow;
                    pack.ModifiedAt = DateTime.UtcNow;

                    await Repo.AddAsync(pack);
                    gamePacks.Insert(0, pack.Clone());
                    NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Success, Summary = "Imported", Detail = $"GamePack '{pack.Name}' cloned and added.", Duration = 3000 });
                    await InvokeAsync(StateHasChanged);
                    return;
                }

                // User chose not to overwrite or clone
                NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Info, Summary = "Import cancelled", Detail = "Import was cancelled.", Duration = 3000 });
                return;
            }

            // Persist: update if exists in repo list, otherwise add (normal path)
            if (gamePacks.Any(g => g.Id == pack.Id))
            {
                await Repo.UpdateAsync(pack);
                var idx = gamePacks.FindIndex(g => g.Id == pack.Id);
                gamePacks[idx] = pack.Clone();
                NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Success, Summary = "Imported", Detail = $"GamePack '{pack.Name}' updated.", Duration = 3000 });
            }
            else
            {
                await Repo.AddAsync(pack);
                gamePacks.Insert(0, pack.Clone());
                NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Success, Summary = "Imported", Detail = $"GamePack '{pack.Name}' added.", Duration = 3000 });
            }

            await InvokeAsync(StateHasChanged);
        }
        catch (System.IO.IOException)
        {
            NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = "Import failed", Detail = "File too large or could not be read.", Duration = 5000 });
        }
        catch (Exception ex)
        {
            NotificationService.Notify(new NotificationMessage { Severity = NotificationSeverity.Error, Summary = "Import failed", Detail = ex.Message, Duration = 5000 });
        }
    }

    private static string SanitizeFileName(string name)
    {
        var invalid = System.IO.Path.GetInvalidFileNameChars();
        var sb = new StringBuilder(name.Length);
        foreach (var c in name)
        {
            if (Array.IndexOf(invalid, c) < 0)
                sb.Append(c);
            else
                sb.Append('_');
        }
        return sb.ToString();
    }
}