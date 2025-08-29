using System;
using System.Collections.Generic;
using System.Linq;

namespace Core
{
    public record Candidate(string Move, int Score, int Rank);

    public static class CandidateSelector
    {
        public static Candidate? Select(IEnumerable<Candidate> candidates, MovePolicy policy, int seed)
        {
            if (candidates == null) return null;
            var list = candidates.OrderBy(c => c.Rank).ToList();
            if (list.Count == 0) return null;

            int k = Math.Max(1, Math.Min(policy.TopK, list.Count));
            if (policy.Deterministic || k == 1)
            {
                return list[0];
            }

            var rnd = new Random(seed);
            int idx = rnd.Next(k);
            return list[idx];
        }
    }
}
