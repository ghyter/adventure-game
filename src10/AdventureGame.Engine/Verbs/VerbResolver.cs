namespace AdventureGame.Engine.Verbs;

using AdventureGame.Engine.Models;

public class VerbResolver
{
    public Verb? ResolveVerb(
        string verbToken,
        GameElement? target1,
        GameElement? target2,
        IEnumerable<Verb> verbs)
    {
        // Count how many targets the user provided
        int providedTargetCount = 0;
        if (target1 != null) providedTargetCount++;
        if (target2 != null) providedTargetCount++;

        var candidates = verbs.Where(v =>
               v.Name.Equals(verbToken, StringComparison.OrdinalIgnoreCase)
            || v.Aliases.Contains(verbToken, StringComparer.OrdinalIgnoreCase));

        // Filter out verbs that don't match the provided target count
        var validCandidates = candidates.Where(v => v.TargetCount == providedTargetCount).ToList();

        // If no valid candidates by target count, return null
        if (validCandidates.Count == 0)
            return null;

        var scored = new List<(Verb verb, int score)>();

        foreach (var v in validCandidates)
        {
            int score = 0;
            if (target1 != null) score += v.Target1.Score(target1);
            if (target2 != null) score += v.Target2.Score(target2);
            scored.Add((v, score));
        }

        return scored.OrderByDescending(s => s.score)
                     .Select(s => s.verb)
                     .FirstOrDefault();
    }
}
