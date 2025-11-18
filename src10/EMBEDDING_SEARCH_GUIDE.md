# Embedding Search Guide

## How Semantic Search Works

The embedding search uses AI to find semantically similar items based on meaning, not just exact word matches. It compares the semantic meaning of your search query against all game element descriptions.

## Step-by-Step Usage

### 1. **Load GamePack & Generate Embeddings**
   - Navigate to `/embedding-playground`
   - Click "Load GamePack & Generate Embeddings" button
   - This creates vector embeddings for all:
     - Scenes (with descriptions)
     - Items (with descriptions)
     - NPCs (with descriptions)
   - **Status:** You'll see a success notification when complete
   - **Logs:** Check the "Logs" section at the bottom to verify embeddings were created

### 2. **Enter a Search Query**
   - Type keywords or phrases related to what you're looking for
   - Examples of good search queries:
     - For a "dark cave" scene: search `"cave"`, `"dark"`, `"underground"`, `"tunnel"`
     - For a "healing potion" item: search `"potion"`, `"heal"`, `"medicine"`, `"cure"`
     - For a "royal guard" NPC: search `"guard"`, `"royal"`, `"soldier"`, `"knight"`

### 3. **Click Search**
   - Press the "Search" button or hit Enter
   - **Visual Feedback:** Results clear immediately when you click Search, showing progress
   - The search processes:
     - Tokenizes your query
     - Generates an embedding for your query
     - Compares against all indexed embeddings
     - Returns top 20 matches sorted by similarity score

### 4. **Review Results**
   - **Score Column:** Ranges from 0.0 to 1.0
     - **1.0** = Perfect match (only possible if searching exact same text as embedding)
     - **0.7+** = Very strong match
     - **0.5-0.7** = Good match
     - **0.0-0.5** = Weak match
   - **Type Column:** Indicates if it's a Scene, Item, or NPC
   - **ID Column:** Unique identifier for the element
   - **Text Column:** The description that was embedded

## What Makes a Good Search

### ? Good Searches
- Single concept: `"cave"`, `"poison"`, `"treasure"`
- Related words: `"healing"` (finds health/cure-related items)
- Descriptive terms: `"dark"`, `"valuable"`, `"dangerous"`
- Synonyms work: Searching `"guard"` finds items described as `"soldier"` or `"protector"`

### ? Poor Searches
- Too generic: `"the"`, `"is"`, `"a"` (common words have weak embeddings)
- Wrong concept: Searching `"sword"` won't find items described as `"potion"`
- Multiple unrelated concepts: `"sword and potion and castle"` (splits focus)

## Debugging

### Check the Logs Section
Each search logs:
- **`Searching for: '<query>'`** - Your search text
- **`Query embedding generated with length: <N>`** - Confirms embedding was created (typically 384 dimensions)
- **`Search complete. Found <N> results.`** - How many matches were found
- **`Top result score: 0.XXX`** - The highest similarity score achieved

### If You Get No Results
1. Check if embeddings were generated (see logs for "Embeddings Created" message)
2. Verify your GamePack has items with descriptions
3. Try a different search term - ensure it relates to actual descriptions in your game
4. Check the logs for any error messages

### If Scores Seem Wrong
- Scores should range 0.0 to 1.0
- If all scores are 0.0, vectors may not be compatible
- Check embeddings are being created properly in logs
- Verify both embeddings and query embeddings have correct dimensions (should both be 384)

## Example Workflow

**Setup:**
```
GamePack: "Fantasy Adventure"
- Scene: "Dark Mountain Cave" 
  Description: "A deep cave beneath the mountains, dark and foreboding"
- Item: "Health Elixir"
  Description: "A glowing potion that restores health"
- NPC: "Mountain Guard"
  Description: "A grizzled soldier who guards the cave entrance"
```

**Searches and Expected Results:**
| Query | Expected Top Matches | Why |
|-------|---------------------|-----|
| `"cave"` | Mountain Cave, Mountain Guard | Both related to cave |
| `"healing"` | Health Elixir, Mountain Guard | Healing is about health; Guard relates to protection/wellness |
| `"potion"` | Health Elixir | Direct match to description |
| `"dark place"` | Mountain Cave | Semantic match to "dark and foreboding" |
| `"mountain guardian"` | Mountain Guard | Semantic match to "soldier who guards" |

## Performance Notes

- First search takes ~1-2 seconds (embedding generation + comparison)
- Subsequent identical searches are cached (< 100ms)
- Maximum 20 results displayed
- Logs keep last 500 entries for performance

## Troubleshooting Checklist

- [ ] GamePack is selected (check header)
- [ ] Clicked "Load GamePack & Generate Embeddings"
- [ ] Embeddings were created (check logs for success message)
- [ ] Query is not empty
- [ ] Check logs for error messages
- [ ] Verify game elements have non-empty descriptions
- [ ] Try simpler, more specific search terms
