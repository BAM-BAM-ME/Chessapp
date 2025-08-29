using System.Collections.Generic;
using Core;
using Xunit;

namespace Chessapp.Tests
{
    public class CandidateSelectorTests
    {
        private static List<Candidate> BuildCandidates(int count)
        {
            var list = new List<Candidate>();
            for (int i = 1; i <= count; i++)
            {
                list.Add(new Candidate($"m{i}", score: i * 10, rank: i));
            }
            return list;
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void Deterministic_PicksFirst(int k)
        {
            var candidates = BuildCandidates(3);
            var policy = new MovePolicy { Deterministic = true, TopK = k };
            var sel = CandidateSelector.Select(candidates, policy, seed: 42);
            Assert.Equal("m1", sel?.Move);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        public void NonDeterministic_UsesSeed(int k)
        {
            var candidates = BuildCandidates(3);
            var policy = new MovePolicy { Deterministic = false, TopK = k };
            int idx = new System.Random(1234).Next(System.Math.Min(k, candidates.Count));
            var expected = candidates[idx].Move;
            var sel = CandidateSelector.Select(candidates, policy, seed: 1234);
            Assert.Equal(expected, sel?.Move);
        }
    }
}
