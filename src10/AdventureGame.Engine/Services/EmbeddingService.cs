using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using AdventureGame.Services;
using AdventureGame.Engine.Services.Tokenization;

namespace AdventureGame.Engine.Services;

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

    // TCS and event to signal readiness
    private TaskCompletionSource<bool> _readyTcs = new(TaskCreationOptions.RunContinuationsAsynchronously);
    public event EventHandler? Ready;

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
            _readyTcs.TrySetResult(true);
        }
    }

    public bool IsReady => _initialized;

    /// <summary>
    /// A task that completes when the model is fully initialized.
    /// </summary>
    public Task WhenReady => _readyTcs.Task;

    /// <summary>
    /// Trigger background loading if not already loaded.
    /// </summary>
    public void StartLoadIfNeeded()
    {
        if (_initialized) return;
        _ = InitializeAsync();
    }

    public async Task EnsureReadyAsync()
    {
        if (_initialized)
            return;

        await InitializeAsync();
    }

    private Task<string> EnsureLocalModelAsync()
    {
        // Resolve model path on a background thread to avoid blocking the caller/UI
        return Task.Run(() => ResolveModelPath());
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

            // Create the ONNX session off the UI thread.
            var session = await Task.Run(() => new InferenceSession(path)).ConfigureAwait(false);
            _session = session;
            _initialized = true;
            _readyTcs.TrySetResult(true);
            Ready?.Invoke(this, EventArgs.Empty);
        }
        catch (Exception ex)
        {
            // Propagate failure to awaiters
            _readyTcs.TrySetException(ex);
            throw;
        }
        finally
        {
            _initLock.Release();
        }
    }

    public async Task<float[]> EmbedAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return [];

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
            var inputIds = new DenseTensor<long>(ids, [1, ids.Length]);
            var attentionMask = new DenseTensor<long>(mask, [1, mask.Length]);
            var tokenTypes = new DenseTensor<long>(types, [1, types.Length]);

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



    private static string ResolveModelPath()
    {
        // 1. Unit tests / console apps
        var baseDir = AppContext.BaseDirectory;
        var testPath = Path.Combine(baseDir, "AIModels", "model.onnx");
        if (File.Exists(testPath))
        {
#if DEBUG
            var fi = new FileInfo(testPath);
            Console.WriteLine($"EmbeddingService: Loaded model from TEST path: {testPath} ({fi.Length} bytes)");
#endif
            return testPath;
        }

        // 2. MAUI Windows LocalState
        var cwd = Directory.GetCurrentDirectory();
        var mauiPath = Path.Combine(cwd, "AIModels", "model.onnx");
        if (File.Exists(mauiPath))
        {
#if DEBUG
            var fi = new FileInfo(mauiPath);
            Console.WriteLine($"EmbeddingService: Loaded model from MAUI path: {mauiPath} ({fi.Length} bytes)");
#endif
            return mauiPath;
        }

#if DEBUG
        Console.WriteLine("EmbeddingService: MODEL NOT FOUND:");
        Console.WriteLine("  Tried: " + testPath);
        Console.WriteLine("  Tried: " + mauiPath);
#endif

        throw new FileNotFoundException("model.onnx not found in either AppContext.BaseDirectory or CurrentDirectory");
    }
}
