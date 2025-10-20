using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AdventureGame.Engine.Models;
using Microsoft.JSInterop;

public class IndexedDbGamePackRepository : IGamePackRepository
{
    private readonly IJSRuntime _js;
    private readonly string _dbName = "adventure-game-db";
    private readonly int _version = 1;
    private readonly string _store = "gamepacks";
    private bool _initialized;

    public IndexedDbGamePackRepository(IJSRuntime js)
    {
        _js = js;
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;
        await _js.InvokeAsync<bool>("indexedDbInterop.ensureDb", _dbName, _version, _store);
        _initialized = true;
    }

    public async Task<List<GamePack>> GetAllAsync()
    {
        await InitializeAsync();
        var jsonList = await _js.InvokeAsync<string[]>("indexedDbInterop.getAll", _dbName, _version, _store);
        if (jsonList == null) return new List<GamePack>();
        return jsonList.Select(json => GamePack.FromJson(json)).ToList();
    }

    public async Task<GamePack?> GetByIdAsync(string id)
    {
        await InitializeAsync();
        var json = await _js.InvokeAsync<string?>("indexedDbInterop.get", _dbName, _version, _store, id);
        return string.IsNullOrEmpty(json) ? null : GamePack.FromJson(json);
    }

    public async Task AddAsync(GamePack pack)
    {
        if (pack == null) throw new ArgumentNullException(nameof(pack));
        await InitializeAsync();
        pack.CreatedAt = pack.CreatedAt == default ? DateTime.UtcNow : pack.CreatedAt;
        pack.ModifiedAt = DateTime.UtcNow;
        var json = pack.ToJson();
        await _js.InvokeVoidAsync("indexedDbInterop.put", _dbName, _version, _store, pack.Id.ToString(), json);
    }

    public async Task UpdateAsync(GamePack pack)
    {
        if (pack == null) throw new ArgumentNullException(nameof(pack));
        await InitializeAsync();
        pack.ModifiedAt = DateTime.UtcNow;
        var json = pack.ToJson();
        await _js.InvokeVoidAsync("indexedDbInterop.put", _dbName, _version, _store, pack.Id.ToString(), json);
    }

    public async Task DeleteAsync(string id)
    {
        await InitializeAsync();
        await _js.InvokeVoidAsync("indexedDbInterop.remove", _dbName, _version, _store, id);
    }
}