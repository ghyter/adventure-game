using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AdventureGame.Engine.Models;
using Microsoft.Maui.Storage;

public sealed class AppDataFileGamePackRepository : IGamePackRepository
{
    private const string FolderName = "GamePacks";
    private bool _initialized;
    private string _root = string.Empty;

    public Task InitializeAsync()
    {
        if (_initialized) return Task.CompletedTask;

        // Use MAUI's cross-platform AppData directory
        var appData = FileSystem.AppDataDirectory;
        _root = Path.Combine(appData, FolderName);
        Directory.CreateDirectory(_root);

        _initialized = true;
        return Task.CompletedTask;
    }

    public async Task<List<GamePack>> GetAllAsync()
    {
        await InitializeAsync();
        var list = new List<GamePack>();
        if (!Directory.Exists(_root)) return list;

        foreach (var file in Directory.EnumerateFiles(_root, "*.json", SearchOption.TopDirectoryOnly))
        {
            try
            {
                var json = await File.ReadAllTextAsync(file).ConfigureAwait(false);
                var pack = GamePack.FromJson(json);
                if (pack != null)
                {
                    list.Add(pack);
                }
            }
            catch
            {
                // Ignore invalid/corrupt files
            }
        }
        return list;
    }

    public async Task<GamePack?> GetByIdAsync(string id)
    {
        await InitializeAsync();
        // First try exact id.json, then pattern id-*.json
        var primary = Path.Combine(_root, $"{Sanitize(id)}.json");
        if (File.Exists(primary))
        {
            try
            {
                var json = await File.ReadAllTextAsync(primary).ConfigureAwait(false);
                return GamePack.FromJson(json);
            }
            catch { return null; }
        }

        var match = Directory.EnumerateFiles(_root, $"{Sanitize(id)}-*.json").FirstOrDefault();
        if (match is not null)
        {
            try
            {
                var json = await File.ReadAllTextAsync(match).ConfigureAwait(false);
                return GamePack.FromJson(json);
            }
            catch { }
        }
        return null;
    }

    public async Task AddAsync(GamePack pack)
    {
        if (pack == null) throw new ArgumentNullException(nameof(pack));
        await InitializeAsync();

        pack.CreatedAt = pack.CreatedAt == default ? DateTime.UtcNow : pack.CreatedAt;
        pack.ModifiedAt = DateTime.UtcNow;

        var path = BuildFilePath(pack);
        var json = pack.ToJson();
        await File.WriteAllTextAsync(path, json).ConfigureAwait(false);
    }

    public async Task UpdateAsync(GamePack pack)
    {
        if (pack == null) throw new ArgumentNullException(nameof(pack));
        await InitializeAsync();

        pack.ModifiedAt = DateTime.UtcNow;
        var path = BuildFilePath(pack);
        var json = pack.ToJson();
        await File.WriteAllTextAsync(path, json).ConfigureAwait(false);
    }

    public async Task DeleteAsync(string id)
    {
        await InitializeAsync();
        var primary = Path.Combine(_root, $"{Sanitize(id)}.json");
        try
        {
            if (File.Exists(primary))
            {
                File.Delete(primary);
                return;
            }
        }
        catch { }

        foreach (var f in Directory.EnumerateFiles(_root, $"{Sanitize(id)}-*.json"))
        {
            try { File.Delete(f); } catch { }
        }
    }

    private string BuildFilePath(GamePack pack)
    {
        var safeName = SanitizeFileName(string.IsNullOrWhiteSpace(pack.Name) ? "gamepack" : pack.Name);
        var fileName = $"{pack.Id}-{safeName}.json"; // include name for easier browsing, but start with Id for stable lookup
        return Path.Combine(_root, fileName);
    }

    private static string Sanitize(string value)
        => new string((value ?? string.Empty).Where(ch => char.IsLetterOrDigit(ch) || ch == '-' || ch == '_').ToArray());

    private static string SanitizeFileName(string name)
    {
        var invalid = Path.GetInvalidFileNameChars();
        return string.Concat(name.Select(c => invalid.Contains(c) ? '_' : c));
    }
}
