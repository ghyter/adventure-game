using System;
using System.Collections.Generic;
using System.Linq;
using AdventureGame.Engine.Models;

namespace AdventureGame.Editor.Services
{
    // Enumeration of concrete GameElement classes available in the editor.
    public enum ElementKind
    {
        Scene,
        Item,
        Npc,
        Player,
        Exit
    }

    // Simple info record for populating UI lists
    public sealed record ElementKindItem(ElementKind Kind, string Name, Type Type);

    // Factory for creating GameElement instances in a way that's AOT/linker friendly.
    public static class ElementFactory
    {
        private static readonly List<ElementKindItem> _all = new()
        {
            new ElementKindItem(ElementKind.Scene, nameof(Scene), typeof(Scene)),
            new ElementKindItem(ElementKind.Item, nameof(Item), typeof(Item)),
            new ElementKindItem(ElementKind.Npc, nameof(Npc), typeof(Npc)),
            new ElementKindItem(ElementKind.Player, nameof(Player), typeof(Player)),
            new ElementKindItem(ElementKind.Exit, nameof(Exit), typeof(Exit)),
        };

        public static IEnumerable<ElementKindItem> All => _all;

        public static GameElement Create(ElementKind kind)
            => kind switch
            {
                ElementKind.Scene => new Scene(),
                ElementKind.Item => new Item(),
                ElementKind.Npc => new Npc(),
                ElementKind.Player => new Player(),
                ElementKind.Exit => new Exit(),
                _ => new Item()
            };

        public static Type TypeOf(ElementKind kind)
            => _all.First(i => i.Kind == kind).Type;

        public static string DisplayName(ElementKind kind)
            => _all.First(i => i.Kind == kind).Name;
    }
}
