# Semantic Embedding Tests Guide

## Overview

The `SemanticEmbeddingTests.cs` file contains unit tests that validate the semantic quality of the embedding model using real data from the Clue Mansion game pack. These tests ensure that the embedding service produces semantically meaningful embeddings that correctly rank related items higher than unrelated items.

## Test Structure

All tests follow this pattern:

1. **Prepare embeddings** for various game element descriptions
2. **Calculate similarity scores** using cosine similarity
3. **Assert** that semantically related items score higher than unrelated items

## Individual Tests

### 1. `Semantics_Weapon_Query_ShouldRankWeapons_AboveNonWeapons`

**Purpose:** Validates that a "weapon" query returns weapon items higher than non-weapon items.

**Query:** `"weapon"`

**Expected Winners:**
- Revolver: "A small but deadly firearm, polished to a shine."
- Lead Pipe: "Heavy and blunt, it could crush more than plumbing."
- Rope: "A sturdy length of rope, frayed at one end."
- Candlestick: "A solid brass candlestick, elegant and heavy."
- Knife: "A gleaming kitchen knife with a sharp edge."
- Wrench: "A heavy wrench used for tightening—or bludgeoning."

**Expected Losers:**
- Ballroom, Dining Room, Hall (rooms - not weapons)
- Professor Plum, Miss Scarlett, etc. (NPCs - not weapons)

**Why This Matters:** This is the primary use case mentioned in your issue. The model should clearly distinguish weapon-related content.

---

### 2. `Semantics_Educator_Query_ShouldRankEducator_HigherThanNonEducators`

**Purpose:** Validates that "educator" query correctly identifies Professor Plum (academic character).

**Query:** `"educator"`

**Expected Winner:**
- Professor Plum: "An absent-minded academic in deep purple attire, sharp of mind and tongue."

**Expected Losers:**
- Colonel Mustard (military)
- Mr. Green (businessman)
- Mrs. White (housekeeper)
- Miss Scarlett (socialite)
- Mrs. Peacock (socialite/gossip)

**Why This Matters:** This addresses your specific issue where "educator" returned Mr. Green instead of Professor Plum. The test validates this behavior.

---

### 3. `Semantics_Firearm_Query_ShouldRankRevolver_Higher`

**Purpose:** Validates that specific weapon type queries work correctly.

**Query:** `"firearm"`

**Expected Winner:**
- Revolver: "A small but deadly firearm, polished to a shine."

**Expected Losers:**
- Lead Pipe, Rope, Knife (melee weapons, not firearms)

**Why This Matters:** Tests that the model can distinguish between weapon subtypes.

---

### 4. `Semantics_Academic_Query_Should_RelateTo_ProfessorPlum`

**Purpose:** Validates that "academic" query clearly identifies the academic NPC.

**Query:** `"academic"`

**Expected Winner:**
- Professor Plum

**Expected Losers:**
- All other NPCs

---

### 5. `Semantics_Heavy_Query_Should_Score_WeaponItems_High`

**Purpose:** Validates that adjective-based queries work correctly.

**Query:** `"heavy"`

**Expected Winners:**
- Lead Pipe: explicitly mentions "heavy"
- Candlestick: explicitly mentions "heavy"
- Wrench: explicitly mentions "heavy"

**Expected Losers:**
- Revolver: "small" firearm
- Rope: no weight implications
- Knife: no weight implications

**Why This Matters:** Tests that descriptive adjectives in embeddings are properly weighted.

---

### 6. `Semantics_Elegant_Query_Should_Relate_To_Socialites`

**Purpose:** Validates that aesthetic/style queries work.

**Query:** `"elegant"`

**Expected Winners:**
- Miss Scarlett (glamorous socialite)
- Candlestick (explicitly "elegant")
- Mrs. Peacock (elegant older woman)

**Expected Losers:**
- Lead Pipe, Wrench (utilitarian)
- Colonel Mustard (military, not elegant)

---

### 7. `Semantics_Library_Query_Should_Score_Study_AndLibrary_High`

**Purpose:** Validates that location-based queries work correctly.

**Query:** `"library"`

**Expected Winners:**
- Library: "Tall shelves of dusty books line the walls."
- Study: "A small, wood-paneled room filled with books and secrets."
- Professor Plum: academic character related to libraries

**Expected Losers:**
- Ballroom (dancing)
- Kitchen (cooking)
- Miss Scarlett (socialite, not academic)

---

### 8. `Semantics_Socialite_Query_ShouldRank_MissScarlett_High`

**Purpose:** Validates that character type queries work.

**Query:** `"socialite"`

**Expected Winner:**
- Miss Scarlett: explicitly described as "glamorous socialite"

**Expected Losers:**
- All other NPCs (military, business, housekeeping, academic)

---

### 9. `Semantics_SimilarQueries_ProduceSimilar_Embeddings`

**Purpose:** Validates that semantically similar queries produce similar embeddings.

**Queries:** 
- "weapon"
- "deadly"
- "lethal"
- "instrument of death"

**Assertion:** All embeddings should have >0.5 cosine similarity to each other.

**Why This Matters:** Tests that the model captures semantic relationships between synonymous terms.

---

### 10. `Semantics_DissimilarQueries_ProduceDifferent_Embeddings`

**Purpose:** Validates that semantically opposite queries produce different embeddings.

**Queries:**
- "weapon" vs "peaceful"

**Assertion:** Cosine similarity should be <0.8 (indicating difference).

**Why This Matters:** Ensures the model doesn't produce overly similar embeddings for completely unrelated concepts.

---

## Running the Tests

### In Visual Studio
1. Open Test Explorer (Test ? Windows ? Test Explorer)
2. Run all tests: Click "Run All Tests"
3. Run specific test: Right-click test and select "Run"

### From Command Line
```bash
dotnet test AdventureGame.Engine.Tests
```

### Run Only Semantic Tests
```bash
dotnet test AdventureGame.Engine.Tests --filter "ClassName=SemanticEmbeddingTests"
```

## Expected Test Results

With a properly trained semantic embedding model (like all-MiniLM-L6-v2):

- ? **Should PASS:** All tests listed above
- ?? **Might FAIL:** If your ONNX model is:
  - Not properly loaded
  - Not a semantic model (e.g., just using hash-based tokenization)
  - Corrupted or incorrectly exported

## Debugging Failed Tests

### If tests fail, check:

1. **Is the ONNX model loaded?**
   ```csharp
   var service = new EmbeddingService();
   var emb = await service.EmbedAsync("test");
   Debug.WriteLine($"Embedding length: {emb.Length}"); // Should be 384 for MiniLM
   ```

2. **Are embeddings actually different?**
   Add debug output to see actual similarity scores:
   ```csharp
   System.Diagnostics.Debug.WriteLine($"Score for 'weapon': {score:F6}");
   ```

3. **Check the model file:**
   - Verify `model.onnx` exists at:
     - `AppContext.BaseDirectory\AIModels\model.onnx`
     - `Directory.GetCurrentDirectory()\AIModels\model.onnx`

4. **Run integration test:**
   Load the game and manually search - output logs will show actual scores.

## Understanding Cosine Similarity Scores

For normalized embeddings (which the service provides):

| Similarity Score | Meaning |
|------------------|---------|
| 1.0 | Identical embeddings |
| 0.8-0.9 | Very closely related |
| 0.6-0.8 | Related but distinct |
| 0.4-0.6 | Loosely related |
| 0.2-0.4 | Weakly related |
| 0.0-0.2 | Unrelated |
| -1.0 | Completely opposite |

## Examples from Clue Mansion

### Good Semantic Ranking (Expected)
```
Query: "weapon"
Results:
  1. Revolver (0.73) ? Directly weapon-related
  2. Candlestick (0.67) ? Weapon in Clue
  3. Lead Pipe (0.65) ? Weapon in Clue
  4. Ballroom (0.50) ? Not weapon
  5. Hall (0.48) ? Not weapon
```

### Poor Semantic Ranking (Problem)
```
Query: "weapon"
Results:
  1. Ballroom (0.68) ? Room, NOT weapon!
  2. Revolver (0.67) ? Should be #1
  3. Hall (0.66) ? Room, NOT weapon
```

## How to Extend These Tests

To add a new semantic test for your own game:

```csharp
[TestMethod]
public async Task Semantics_YourQuery_Should_Rank_ExpectedItem()
{
    var expectedDescriptions = new Dictionary<string, string>
    {
        { "ItemName", "Item description..." },
    };
    
    var unexpectedDescriptions = new Dictionary<string, string>
    {
        { "OtherItem", "Other description..." },
    };

    var queryEmbedding = await _service.EmbedAsync("your query");

    // Calculate scores for expected items
    var expectedScores = new List<double>();
    foreach (var (name, desc) in expectedDescriptions)
    {
        var emb = await _service.EmbedAsync(desc);
        var score = CosineSimilarity(emb, queryEmbedding);
        expectedScores.Add(score);
    }

    // Calculate scores for unexpected items
    var unexpectedScores = new List<double>();
    foreach (var (name, desc) in unexpectedDescriptions)
    {
        var emb = await _service.EmbedAsync(desc);
        var score = CosineSimilarity(emb, queryEmbedding);
        unexpectedScores.Add(score);
    }

    var minExpected = expectedScores.Min();
    var maxUnexpected = unexpectedScores.Max();

    Assert.IsTrue(minExpected > maxUnexpected,
        $"Expected items should score higher than unexpected items");
}
```

## Interpreting Results

When you run these tests:

1. **All tests pass** ? Your embedding model is working well! ?
2. **Some tests fail** ? Your model needs improvement or verification
3. **Debug output shows low scores everywhere** ? Model not loading correctly
4. **Debug output shows similar scores for different items** ? Model not semantic

## Next Steps

If tests fail:

1. **Verify model is loaded correctly:**
   - Check `EmbeddingService.ResolveModelPath()`
   - Ensure `model.onnx` is in the correct location
   - Verify file isn't corrupted

2. **Consider model quality:**
   - The all-MiniLM-L6-v2 model is good but not perfect
   - Consider using a larger model like all-mpnet-base-v2 for better accuracy
   - Fine-tune a model on your specific domain if needed

3. **Augment descriptions:**
   - Add more descriptive terms to game element descriptions
   - The model will perform better with richer context
   - Example: Instead of "A weapon", use "A deadly firearm, small but lethal"

## References

- **ONNX Model:** https://huggingface.co/onnx-models/all-MiniLM-L6-v2-onnx
- **Sentence Transformers:** https://www.sbert.net/
- **Cosine Similarity:** https://en.wikipedia.org/wiki/Cosine_similarity
