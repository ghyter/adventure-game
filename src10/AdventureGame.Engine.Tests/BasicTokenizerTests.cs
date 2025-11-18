using AdventureGame.Engine.Services.Tokenization;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace AdventureGame.Engine.Tests
{
    [TestClass]
    public class BasicTokenizerTests
    {
        [TestMethod]
        public void Tokenize_EmptyString_ReturnsEmptyArrays()
        {
            var tokenizer = new BasicTokenizer();

            var (ids, mask, types) = tokenizer.Tokenize("");

            Assert.IsEmpty(ids);
            Assert.IsEmpty(mask);
            Assert.IsEmpty(types);
        }

        [TestMethod]
        public void Tokenize_Whitespace_ReturnsEmptyArrays()
        {
            var tokenizer = new BasicTokenizer();

            var (ids, mask, types) = tokenizer.Tokenize("   \t  \n");

            Assert.IsEmpty(ids);
            Assert.IsEmpty(mask);
            Assert.IsEmpty(types);
        }

        [TestMethod]
        public void Tokenize_SingleWord_IncludesClsAndSepAndMasks()
        {
            var tokenizer = new BasicTokenizer();

            var (ids, mask, types) = tokenizer.Tokenize("Hello");

            Assert.HasCount(3, ids, "ids length should include [CLS] and [SEP]");
            Assert.HasCount(3, mask);
            Assert.HasCount(3, types);

            Assert.AreEqual(101L, ids[0], "First token should be CLS");
            Assert.AreEqual(102L, ids[^1], "Last token should be SEP");

            foreach (var m in mask) Assert.AreEqual(1L, m, "All mask values should be 1");
            foreach (var t in types) Assert.AreEqual(0L, t, "All token types should be 0");
        }

        [TestMethod]
        public void Tokenize_MultiWord_RangesAndDedupHashes()
        {
            var tokenizer = new BasicTokenizer();

            var (ids, mask, types) = tokenizer.Tokenize("Hello   WORLD  test");

            // words: hello, world, test
            Assert.HasCount(5, ids);
            Assert.HasCount(5, mask);
            Assert.HasCount(5, types);

            Assert.AreEqual(101L, ids[0]);
            Assert.AreEqual(102L, ids[^1]);

            // inner tokens are hash-based: [200, 30199]
            for (int i = 1; i < ids.Length - 1; i++)
            {
                Assert.IsTrue(ids[i] >= 200 && ids[i] <= 30199, $"Token id {ids[i]} out of expected range");
                Assert.AreEqual(1L, mask[i]);
                Assert.AreEqual(0L, types[i]);
            }
        }

        [TestMethod]
        public void Tokenize_RepeatedWords_ProduceSameIdWithinRun()
        {
            var tokenizer = new BasicTokenizer();

            var (ids, mask, types) = tokenizer.Tokenize("foo foo");

            Assert.HasCount(4, ids); // CLS, foo, foo, SEP
            Assert.AreEqual(ids[1], ids[2], "Repeated words should hash to same id within a process");
        }
    }
}
