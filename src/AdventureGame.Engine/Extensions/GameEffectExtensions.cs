using AdventureGame.Engine.Models.Round;
using AdventureGame.Engine.Runtime;

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
}
