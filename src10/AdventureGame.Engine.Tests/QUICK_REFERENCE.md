# Semantic Embedding Tests - Quick Reference

## What These Tests Do

These 10 unit tests validate that your embedding model produces **semantically meaningful** results. They use data from Clue Mansion to ensure searches work correctly.

## Running the Tests

### Visual Studio
- **Test ? Run All Tests** (or Ctrl+R, Ctrl+A)
- **Test Explorer ? Right-click ? Run** for specific tests

### Command Line
```bash
# Run all semantic tests
dotnet test AdventureGame.Engine.Tests --filter "SemanticEmbeddingTests"

# Run specific test
dotnet test AdventureGame.Engine.Tests --filter "Semantics_Weapon_Query"

# Run with detailed output
dotnet test AdventureGame.Engine.Tests -v detailed
```

## Test Quick Reference

| Test Name | Query | Should Find | Why It Matters |
|-----------|-------|-------------|---|
| **Weapon_Query** | "weapon" | Revolver, Lead Pipe, Rope, etc. | Main use case from your issue |
| **Educator_Query** | "educator" | Professor Plum | Your specific "educator" problem |
| **Firearm_Query** | "firearm" | Revolver (not pipe/rope) | Weapon sub-typing |
| **Academic_Query** | "academic" | Professor Plum | Similar to educator |
| **Heavy_Query** | "heavy" | Lead Pipe, Candlestick, Wrench | Adjective-based search |
| **Elegant_Query** | "elegant" | Miss Scarlett, Candlestick, Mrs. Peacock | Aesthetic queries |
| **Library_Query** | "library" | Library, Study, Professor Plum | Location/topic search |
| **Socialite_Query** | "socialite" | Miss Scarlett | Character type search |
| **Similar_Queries** | "weapon", "deadly", "lethal", etc. | All should be ~0.5+ similar | Query synonymy |
| **Dissimilar_Queries** | "weapon" vs "peaceful" | Low similarity (<0.8) | Semantic contrast |

## Expected Results

### ? All Tests Pass
- Model is working correctly
- Semantic search will work well
- Ready for production

### ?? Some Tests Fail
```
Weapon test: ? PASS
Educator test: ? FAIL    ? Focus here
Academic test: ? PASS
```
**Action:** Improve descriptions, try synonyms, check model vocabulary

### ? All Tests Fail
```
All tests show scores ~0.50 with no variation
```
**Action:** Model not loading - check file path and ONNX Runtime

## Score Interpretation

For normalized embeddings:

- **0.85+** = Very similar (like synonyms)
- **0.65-0.85** = Related (good match for search)
- **0.45-0.65** = Loosely related (acceptable hit)
- **0.25-0.45** = Weak connection (random-ish)
- **<0.25** = Different (should not rank)

## Understanding Your "Educator" Issue

```
Current behavior (BAD):
  Query: "educator"
  Results:
    1. Mr. Green (0.62)        ? Not an educator!
    2. Professor Plum (0.58)   ? IS the educator!

Expected behavior (GOOD):
  Query: "educator"
  Results:
    1. Professor Plum (0.75)   ? Educator!
    2. Mr. Green (0.48)        ? Not educator
```

**Why it happens:**
- Word overlap: "businessman" and "educator" might share similar context in model
- Insufficient semantic distinction between NPC descriptions
- Model limitations with profession/role classification

**How tests help:**
- `Semantics_Educator_Query_ShouldRankEducator_HigherThanNonEducators` will FAIL
- Shows exactly what's wrong: educator is NOT scoring higher
- Helps you measure improvement as you fix it

## Debugging Checklist

```
? Model file exists?
  Path: YourProject/AIModels/model.onnx
  Size: ~33MB for all-MiniLM-L6-v2

? Embedding service initialized?
  Check: EmbeddingService._initialized == true
  Debug: Add logging to ResolveModelPath()

? Embeddings have variation?
  Check: Different texts ? different vectors
  Debug: Print first 5 values of different embeddings

? Scores make sense?
  Check: Similar items score higher than different items
  Debug: Log individual similarity scores

? Test environment correct?
  Check: Run in Release mode?
  Check: Working directory includes AIModels folder?
```

## Quick Test for Model Health

Add this test to verify model is working:

```csharp
[TestMethod]
public async Task Health_Check_ModelLoads()
{
    var service = new EmbeddingService();
    await Task.Delay(1000); // Wait for loading
    
    Assert.IsTrue(service.IsReady, "Model should be loaded");
    
    var emb = await service.EmbedAsync("test");
    Assert.AreEqual(384, emb.Length, "Embedding should be 384 dimensions");
    Assert.IsTrue(emb.Any(x => Math.Abs(x) > 0.01), "Embedding should have non-zero values");
}
```

## Model Files

**Current Model:** all-MiniLM-L6-v2
- **Dimensions:** 384
- **Size:** ~33MB
- **Speed:** Very fast (good for real-time)
- **Quality:** Good general-purpose

**URL:** https://huggingface.co/onnx-models/all-MiniLM-L6-v2-onnx

## Common Issues & Fixes

| Issue | Solution |
|-------|----------|
| All scores ~0.50 | Model not loading - check path |
| No clear winner | Descriptions need more keywords |
| Only some tests fail | Model vocabulary gap - try synonyms |
| Scores too low overall | Model fine, expected behavior |
| One query consistently fails | Word not in model vocab |

## Next Steps

1. **Run all tests** to see current state
2. **Note which tests fail** (if any)
3. **Run debug test** to check model loading
4. **If tests fail** ? improve descriptions or debug model
5. **Re-run tests** to verify improvement
6. **Document results** for team

## Success Criteria

```
Target: At least 8/10 tests passing
Stretch: All 10/10 tests passing

Current Status (your issue):
  - Weapon tests: Failing (wrong ranking)
  - Educator tests: Failing (Mr. Green > Professor Plum)
  - Need to: Investigate model quality & improve descriptions
```

## Resources

- **Test Guide:** `SEMANTIC_TESTS_GUIDE.md`
- **Results Guide:** `TEST_RESULTS_GUIDE.md`
- **This File:** `QUICK_REFERENCE.md`
- **Test Code:** `SemanticEmbeddingTests.cs`

---

**Last Updated:** 2025-01-14
**Test Count:** 10 comprehensive semantic tests
**Coverage:** Clue Mansion items, NPCs, and queries
