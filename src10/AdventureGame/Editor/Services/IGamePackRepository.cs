using System.Collections.Generic;
using System.Threading.Tasks;
using AdventureGame.Engine.Models;

public interface IGamePackRepository
{
    Task InitializeAsync();
    Task<List<GamePack>> GetAllAsync();
    Task<GamePack?> GetByIdAsync(string id);
    Task AddAsync(GamePack pack);
    Task UpdateAsync(GamePack pack);
    Task DeleteAsync(string id);
}