
# EmbeddingService — Full Implementation Specification (Phase 1)

This document defines the full specification for building a complete EmbeddingService for the AdventureGame MAUI + Blazor Hybrid application using ONNX Runtime and a local MiniLM/MPNet embedding model.

This file is intended for **GitHub Copilot**, so it provides:

- Explicit structure  
- Clear requirements  
- Idiomatic .NET 10 patterns  
- Implementation constraints  
- File organization  
- Future-phase expansion points  

Copilot should use this file as the authoritative source when generating the embedding infrastructure.

---

# ================================
# 1. GOAL OF THE SERVICE
# ================================

Create a reusable, dependency-injectable **EmbeddingService** that allows the application to generate semantic vector embeddings using an ONNX model shipped inside the MAUI package.

This will enable:

- Semantic condition parsing
- Natural-language command resolution
- Similarity search
- Vector database integration (SQLite)
- AI-powered tooling in the game editor

The service must:

1. Run ONNX inference reliably in MAUI (.NET 10)
2. Extract the model from Resources/Raw to a real file
3. Load the model only once (lazy, thread-safe)
4. Tokenize input text
5. Produce normalized embeddings (768-dim or similar)
6. Cache results
7. Provide an async API for embedding text
8. Operate offline with no internet

---

# ================================
# 2. FILE STRUCTURE
# ================================

Copilot must generate the following files:

```
AdventureGame/
    Services/
        EmbeddingService.cs
        Tokenization/
            BasicTokenizer.cs
```

The service is a **singleton**.

---

# ================================
# 3. MODEL HANDLING REQUIREMENTS
# ================================

The ONNX model file (`model.onnx`) is distributed inside:

```
Resources/Raw/model.onnx
```

MAUI flattens packaged assets, but they must be accessed via:

```csharp
FileSystem.OpenAppPackageFileAsync("model.onnx")
```

You **cannot** load the model directly from the package.  
ONNX Runtime requires a **real filesystem path**, so:

1. Open the packaged file as a stream  
2. Copy it to:

```
FileSystem.AppDataDirectory
```

3. Load the model from that copied path

If the file already exists in AppDataDirectory, do not recopy.

### Copilot must implement:

```csharp
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
```

---

# ================================
# 4. ONNX SESSION INITIALIZATION
# ================================

The service must implement **lazy, thread-safe initialization** using `SemaphoreSlim`:

```csharp
private readonly SemaphoreSlim _initLock = new(1, 1);
private InferenceSession? _session;
private bool _initialized;
```

Initialization method:

```csharp
private async Task InitializeAsync()
{
    if (_initialized)
        return;

    await _initLock.WaitAsync();
    try
    {
        if (_initialized)
            return;

        var path = await EnsureLocalModelAsync();
        _session = new InferenceSession(path);
        _initialized = true;
    }
    finally
    {
        _initLock.Release();
    }
}
```

Session must only be created once.

---

# ================================
# 5. BASIC TOKENIZER (PHASE 1)
# ================================

Copilot must create:

```
BasicTokenizer.cs
```

Inside `Tokenization/`.

This tokenizer can be simplistic because Phase 2 adds HuggingFace real tokenization.

### Required behavior:

- Convert input to lowercase
- Split on whitespace
- Prepend `[CLS]` = 101
- Append `[SEP]` = 102
- Convert each word to a stable hash-based token ID
- Produce:
  - input_ids
  - attention_mask
  - token_type_ids (must be ALL zeros)

### Required signature:

```csharp
public (long[] ids, long[] mask, long[] tokenTypes) Tokenize(string text)
```

### Copilot must generate:

```csharp
long cls = 101;
long sep = 102;

long[] ids = new long[words.Length + 2];
long[] mask = new long[ids.Length];
long[] types = new long[ids.Length];

ids[0] = cls; mask[0] = 1; types[0] = 0;

for (int i = 0; i < words.Length; i++)
{
    ids[i+1] = Math.Abs(words[i].GetHashCode() % 30000) + 200;
    mask[i+1] = 1;
    types[i+1] = 0;
}

ids[^1] = sep;
mask[^1] = 1;
types[^1] = 0;
```

This is NOT production tokenization, but it ensures inference functions correctly.

---

# ================================
# 6. EMBEDDING PIPELINE
# ================================

Copilot must implement:

```csharp
public async Task<float[]> EmbedAsync(string text)
```

Full pipeline:

1. If text is null/empty, return empty float[]  
2. Call InitializeAsync()  
3. If cached, return cached value  
4. Tokenize text  
5. Create DenseTensor<long> for:
   - input_ids
   - attention_mask
   - token_type_ids  
6. Run inference via:

```csharp
_session!.Run(inputs)
```

7. Extract first output tensor  
8. Normalize vector  
9. Store in cache  
10. Return result

---

# ================================
# 7. CACHE REQUIREMENTS
# ================================

Service must contain:

```csharp
private readonly Dictionary<string, float[]> _cache = new();
```

Cache is per-text embedding.

---

# ================================
# 8. VECTOR NORMALIZATION
# ================================

Copilot must generate:

```csharp
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
```

Normalization is mandatory.

---

# ================================
# 9. DEPENDENCY INJECTION
# ================================

`EmbeddingService` must be registered in `MauiProgram.cs`:

```csharp
builder.Services.AddSingleton<EmbeddingService>();
```

Tokenizer does NOT need DI.

---

# ================================
# 10. TESTING REQUIREMENTS
# ================================

Copilot must generate xUnit or NUnit tests verifying:

1. Model loads without exception  
2. Embedding length is correct (typically 768)  
3. Values are not NaN  
4. Magnitude is approx. 1.0 after normalization  
5. Second call is faster (cache check)  
6. Tokenizer produces legal shapes  

Tests live under:

```
AdventureGame.Tests/Services/EmbeddingServiceTests.cs
```

---

# ================================
# 11. DESIGN CONSTRAINTS & RULES
# ================================

- Do NOT load the ONNX model from inside packaged assets directly  
- Do NOT run inference synchronously  
- Do NOT block UI thread  
- Do NOT allocate new InferenceSession repeatedly  
- Do NOT require large LLMs  
- Do NOT use unsafe code  
- Use only ONNX Runtime DirectML or Managed  
- Support Windows only (for now)  

---

# ================================
# 12. FUTURE PHASES (DO NOT IMPLEMENT YET)
# ================================

Copilot must NOT implement these now, but must prepare clean architecture that allows:

Phase 2: Full HuggingFace tokenizer  
Phase 3: SQLite vector database  
Phase 4: KNN vector search  
Phase 5: Semantic alias resolver  
Phase 6: Natural-language action parser  
Phase 7: Semantic condition builder  
Phase 8: Contextual command disambiguation  

---

# ================================
# 13. ACCEPTANCE CRITERIA
# ================================

Copilot must satisfy ALL of the following:

- Embeddings are generated reliably  
- Model loads exactly once  
- Tokenization works  
- token_type_ids input is supplied  
- Vectors are normalized  
- Vectors match expected dimension  
- No deadlocks or blocking awaits  
- Code compiles under .NET 10 MAUI  
- Code is idiomatic C#  
- Service is DI-friendly  
- Tests pass  

---

# END OF SPECIFICATION
