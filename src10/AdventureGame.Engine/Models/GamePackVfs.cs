// ==============================
// AdventureGame.Engine/Models/GamePackVfs.cs
// ==============================
#nullable enable
using System.Text.Json.Serialization;

namespace AdventureGame.Engine.Models;

public sealed class GamePackVfs
{
    public List<VfsEntry> Files { get; } = new();
    [JsonIgnore] private Dictionary<string, VfsEntry>? _byPath;

    private Dictionary<string, VfsEntry> ByPath()
        => _byPath ??= Files.ToDictionary(f => Normalize(f.Path), f => f, StringComparer.OrdinalIgnoreCase);

    public bool TryGet(string path, out VfsEntry entry)
    {
        var map = ByPath();
        return map.TryGetValue(Normalize(path), out entry!);
    }

    public void AddOrReplace(string path, string contentType, ReadOnlySpan<byte> data)
    {
        var p = Normalize(path);
        var map = ByPath();
        if (map.TryGetValue(p, out var e)) { e.ContentType = contentType; e.Data = data.ToArray(); }
        else
        {
            var ne = new VfsEntry { Path = p, ContentType = contentType, Data = data.ToArray() };
            Files.Add(ne); map[p] = ne;
        }
    }

    public bool Remove(string path)
    {
        var p = Normalize(path);
        var map = ByPath();
        if (!map.TryGetValue(p, out var e)) return false;
        map.Remove(p);
        return Files.Remove(e);
    }

    public void ValidateOrThrow()
    {
        var norm = Files.Select(f => Normalize(f.Path)).ToList();
        if (norm.Count != norm.Distinct(StringComparer.OrdinalIgnoreCase).Count())
            throw new InvalidOperationException("Duplicate file paths in VFS.");
        foreach (var f in Files)
        {
            if (string.IsNullOrWhiteSpace(f.Path))
                throw new InvalidOperationException("VFS file path cannot be empty.");
            if (f.Data is null)
                throw new InvalidOperationException($"VFS file '{f.Path}' has null data.");
        }
    }

    private static string Normalize(string path) => (path ?? "").Replace('\\', '/').Trim();
}

public sealed class VfsEntry
{
    public string Path { get; set; } = "";
    public string ContentType { get; set; } = "";
    public byte[] Data { get; set; } = [];
}