using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdventureGame.Services.Tokenization;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.Maui.Storage;

namespace AdventureGame.Services
{
    /// <summary>
    /// Provides semantic embeddings via ONNX model inference. Singleton service.
    /// </summary>
    public class EmbeddingService
    {
        private readonly SemaphoreSlim _initLock = new(1, 1);
        private InferenceSession? _session;
        private bool _initialized;
        private readonly Dictionary<string, float[]> _cache = new();
        private readonly BasicTokenizer _tokenizer = new();

        // Optional injection point for testing - when provided, the service will use it instead of creating an ONNX session.
        private readonly IEmbeddingModel? _model;

        // Default constructor - production usage
        public EmbeddingService()
        {
        }

        // Constructor for tests / DI - allows supplying a fake model
        public EmbeddingService(IEmbeddingModel? model)
        {
            _model = model;
            if (_model != null)
            {
                // mark initialized so we don't try to load an onnx model
                _initialized = true;
            }
        }

        private async Task<string> EnsureLocalModelAsync()
        {
            var localPath = Path.Combine(FileSystem.AppDataDirectory, "model.onnx");
            if (!File.Exists(localPath))
            {
                using var src = await FileSystem.OpenAppPackageFileAsync("model.onnx");
                using var dest = File.Create(localPath);
                await src.CopyToAsync(dest);
            }
            return localPath;
        }

        private async Task InitializeAsync()
        {
            if (_initialized)
                return;

            await _initLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (_initialized)
                    return;

                var path = await EnsureLocalModelAsync().ConfigureAwait(false);
                _session = new InferenceSession(path);
                _initialized = true;
            }
            finally
            {
                _initLock.Release();
            }
        }

        public async Task<float[]> EmbedAsync(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return Array.Empty<float>();

            if (_cache.TryGetValue(text, out var cached))
                return cached;

            // Tokenize
            var (ids, mask, types) = _tokenizer.Tokenize(text);

            float[] embedding;

            if (_model != null)
            {
                // Use the injected model (tests)
                embedding = await _model.GetEmbeddingAsync(ids, mask, types).ConfigureAwait(false);
            }
            else
            {
                await InitializeAsync().ConfigureAwait(false);

                // Prepare tensors
                var inputIds = new DenseTensor<long>(ids, new int[] { 1, ids.Length });
                var attentionMask = new DenseTensor<long>(mask, new int[] { 1, mask.Length });
                var tokenTypes = new DenseTensor<long>(types, new int[] { 1, types.Length });

                var inputs = new List<NamedOnnxValue>
                {
                    NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
                    NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask),
                    NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypes),
                };

                using var results = _session!.Run(inputs);
                var first = results.First();
                // Expecting first output to be a 2D tensor [1, D]
                var outputTensor = first.AsEnumerable<float>().ToArray();

                // If shape includes batch dimension flatten accordingly
                embedding = outputTensor;
                if (first is DisposableNamedOnnxValue dnov && dnov.Value is Tensor<float> t && t.Rank == 2 && t.Dimensions[0] == 1)
                {
                    embedding = t.ToArray();
                }
            }

            var normalized = Normalize(embedding);
            _cache[text] = normalized;
            return normalized;
        }

        private float[] Normalize(float[] v)
        {
            float sum = 0;
            for (int i = 0; i < v.Length; i++)
                sum += v[i] * v[i];

            float mag = (float)Math.Sqrt(sum);
            if (mag == 0) return v;

            float[] result = new float[v.Length];
            for (int i = 0; i < v.Length; i++)
                result[i] = v[i] / mag;

            return result;
        }
    }
}
