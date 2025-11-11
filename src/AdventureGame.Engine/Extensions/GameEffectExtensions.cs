using AdventureGame.Engine.Models.Round;
using AdventureGame.Engine.Runtime;
using AdventureGame.Engine.Models;

namespace AdventureGame.Engine.Extensions;

public static class GameEffectExtensions
{

    public static void Apply(this GameEffect effect, GameSession session)
    {
        //var element = session.FindByNameOrAlias(effect.TargetId);
        //if (element is null) return;

        //switch (effect.Action)
        //{
        //    case "ChangeState":
        //        if (element.States.ContainsKey(effect.Value))
        //            element.DefaultState = effect.Value;
        //        break;

        //    case "SetVisible":
        //        element.IsVisible = bool.Parse(effect.Value);
        //        break;

        //        // add other effect types here...
        //}
    }

    /// <summary>
    /// Apply an effect given a session and a candidate scope of elements.
    /// Placeholder: delegates to legacy Apply without scope.
    /// </summary>
    public static void Apply(this GameEffect effect, GameSession session, IEnumerable<GameElement> scope)
        => Apply(effect, session);

    /// <summary>
    /// Convenience to apply a list of effects to a session with a scope.
    /// </summary>
    public static void ApplyRange(this IEnumerable<GameEffect> effects, GameSession session, IEnumerable<GameElement> scope)
    {
        foreach (var e in effects)
            e.Apply(session, scope);
    }
}
