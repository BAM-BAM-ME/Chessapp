using System.Threading.Tasks;
using Core;
using Xunit;

namespace Core.Tests
{
    public class InsightsServiceTests
    {
        [Fact]
        public async Task InsertAndRead()
        {
            var svc = new InsightsService("Data Source=:memory:");
            await svc.AppendAsync("fen", 12, 34, null, 1000, "e2e4 e7e5");
            var rows = await svc.GetLatestAsync(10);
            Assert.Single(rows);
            Assert.Equal("fen", rows[0].Fen);
            Assert.Equal(12, rows[0].Depth);
            Assert.Equal(34, rows[0].ScoreCp);
            Assert.Equal("e2e4 e7e5", rows[0].Pv);
        }
    }
}
