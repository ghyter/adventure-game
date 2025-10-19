// ==============================
// AdventureGame.Engine/Validation/IGamePackValidator.cs
// ==============================
#nullable enable
using AdventureGame.Engine.Models;

namespace AdventureGame.Engine.Validation;

/// <summary>
/// Defines a contract for validating the structure and content of a GamePack.
/// </summary>
public interface IGamePackValidator
{
    /// <summary>
    /// Validates the given GamePack and throws or reports any errors.
    /// </summary>
    void Validate(GamePack pack);
}
