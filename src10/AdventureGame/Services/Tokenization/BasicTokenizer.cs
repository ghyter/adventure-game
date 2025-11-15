using System;
using System.Linq;

namespace AdventureGame.Services.Tokenization
{
    /// <summary>
    /// Phase 1 simplistic tokenizer. NOT production ready.
    /// </summary>
    public class BasicTokenizer
    {
        public (long[] ids, long[] mask, long[] tokenTypes) Tokenize(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                long[] empty = Array.Empty<long>();
                return (empty, empty, empty);
            }

            var words = text.ToLowerInvariant().Split((char[])null, StringSplitOptions.RemoveEmptyEntries);

            long cls = 101;
            long sep = 102;

            long[] ids = new long[words.Length + 2];
            long[] mask = new long[ids.Length];
            long[] types = new long[ids.Length];

            ids[0] = cls; mask[0] = 1; types[0] = 0;

            for (int i = 0; i < words.Length; i++)
            {
                ids[i + 1] = Math.Abs(words[i].GetHashCode() % 30000) + 200;
                mask[i + 1] = 1;
                types[i + 1] = 0;
            }

            ids[^1] = sep;
            mask[^1] = 1;
            types[^1] = 0;

            return (ids, mask, types);
        }
    }
}
