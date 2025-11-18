using System;
using System.Linq;
using System.Threading.Tasks;
using AdventureGame.Engine.Services;
using AdventureGame.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdventureGame.Engine.Tests
{
    internal sealed class FakeEmbeddingModel : IEmbeddingModel
    {
        private readonly Func<long[], long[], long[], float[]> _impl;

        public FakeEmbeddingModel(Func<long[], long[], long[], float[]> impl) => _impl = impl;

        public Task<float[]> GetEmbeddingAsync(long[] ids, long[] mask, long[] tokenTypes)
            => Task.FromResult(_impl(ids, mask, tokenTypes));
    }

    [TestClass]
    public class EmbeddingServiceTests
    {
        [TestMethod]
        public async Task EmbedAsync_EmptyOrWhitespace_ReturnsEmpty()
        {
            var service = new EmbeddingService(new FakeEmbeddingModel((ids, m, t) => [1, 0, 0]));

            var e1 = await service.EmbedAsync("");
            var e2 = await service.EmbedAsync("   ");

            Assert.IsEmpty(e1);
            Assert.IsEmpty(e2);
        }

        [TestMethod]
        public async Task EmbedAsync_NormalizesOutput_ToUnitLength()
        {
            var service = new EmbeddingService(new FakeEmbeddingModel((ids, m, t) => [3, 4]));

            var emb = await service.EmbedAsync("any text");

            var mag = Math.Sqrt(emb.Select(x => x * x).Sum());
            Assert.AreEqual(1.0, mag, 1e-5, "Embedding should be L2-normalized");

            // The original vector [3,4] normalizes to [0.6, 0.8]
            Assert.AreEqual(0.6, emb[0], 1e-5);
            Assert.AreEqual(0.8, emb[1], 1e-5);
        }

        [TestMethod]
        public async Task EmbedAsync_CachesByText_Key()
        {
            int calls = 0;
            var service = new EmbeddingService(new FakeEmbeddingModel((ids, m, t) =>
            {
                calls++;
                return [1, 2, 2]; // norm -> [0.333..., 0.666..., 0.666...]
            }));

            var e1 = await service.EmbedAsync("same text");
            var e2 = await service.EmbedAsync("same text");

            Assert.AreEqual(1, calls, "Embedding model should be called once due to cache");
            Assert.AreSame(e1, e2, "Cache should return same reference instance");
        }

        [TestMethod]
        public async Task EmbedAsync_PassesTokenizationOutputs_ToModel()
        {
            long[] capturedIds = [];
            long[] capturedMask = [];
            long[] capturedTypes = [];

            var service = new EmbeddingService(new FakeEmbeddingModel((ids, m, t) =>
            {
                capturedIds = ids;
                capturedMask = m;
                capturedTypes = t;
                return [1, 0];
            }));

            var emb = await service.EmbedAsync("Hello world");

            Assert.IsGreaterThanOrEqualTo(3, capturedIds.Length, "Should include CLS and SEP plus words");
            Assert.HasCount(capturedIds.Length, capturedMask);
            Assert.HasCount(capturedIds.Length, capturedTypes);
            Assert.AreEqual(101L, capturedIds[0]);
            Assert.AreEqual(102L, capturedIds[^1]);

            foreach (var v in capturedMask) Assert.AreEqual(1L, v);
            foreach (var v in capturedTypes) Assert.AreEqual(0L, v);

            Assert.HasCount(2, emb);
            Assert.AreEqual(1.0, Math.Sqrt(emb.Select(x => x * x).Sum()), 1e-5);
        }
    }
}
