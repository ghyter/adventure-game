using AdventureGame.Engine.Models;
using AdventureGame.Engine.Runtime;

namespace AdventureGame.Editor.Services
{
    public interface ICurrentGameService
    {
        GamePack? CurrentPack { get; }
        bool HasCurrent { get; }
        bool IsDirty { get; }
        GameSession? Session { get; }

        event Action? OnChange;

        void ClearCurrent();
        Task InitializeAsync();
        void MarkDirty();
        Task<bool> SaveCurrentPackAsync();
        void SetCurrent(GamePack pack);
    }
}