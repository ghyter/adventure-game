using System.Collections.Generic;
using System.Linq;
using AdventureGame.Engine.Models.Elements;

namespace AdventureGame.Engine.Models
{
    /// <summary>
    /// Represents a bidirectional relationship between two linked exits.
    /// </summary>
    public class ExitPair(Exit a, Exit b)
    {
        public Exit ExitA { get; } = a;
        public Exit ExitB { get; } = b;

        /// <summary>
        /// Given any exit in the pair, returns its partner.
        /// </summary>
        public Exit? GetPartner(Exit exit)
        {
            if (exit.Id == ExitA.Id) return ExitB;
            if (exit.Id == ExitB.Id) return ExitA;
            return null;
        }

        /// <summary>
        /// Checks whether this pair contains the given exit.
        /// </summary>
        public bool Contains(ElementId id) =>
            id == ExitA.Id || id == ExitB.Id;

        /// <summary>
        /// Finds all valid exit pairs in a collection of elements.
        /// </summary>
        public static List<ExitPair> FromElements(IEnumerable<GameElement> elements)
        {
            var exits = elements.OfType<Exit>().ToList();
            var pairs = new List<ExitPair>();
            var visited = new HashSet<ElementId>();

            foreach (var ex in exits)
            {
                if (visited.Contains(ex.Id)) continue;

                if (ex.TargetExitId is ElementId tid)
                {
                    var target = exits.FirstOrDefault(e => e.Id == tid);
                    if (target != null && target.TargetExitId == ex.Id)
                    {
                        pairs.Add(new ExitPair(ex, target));
                        visited.Add(ex.Id);
                        visited.Add(target.Id);
                    }
                }
            }

            return pairs;
        }

        /// <summary>
        /// Returns the ExitPair containing this exit, if any.
        /// </summary>
        public static ExitPair? FindByExit(IEnumerable<ExitPair> pairs, Exit exit)
            => pairs.FirstOrDefault(p => p.Contains(exit.Id));
    }
}