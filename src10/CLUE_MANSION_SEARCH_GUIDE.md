# Clue Mansion Search Examples

This guide shows you exactly what searches work with the **Clue Mansion 1.1.10** example game.

## Quick Start

1. Load the Clue Mansion game
2. Click "Load GamePack & Generate Embeddings"
3. Try any of the searches below
4. Watch the scores to understand semantic matching

---

## Confirmed Working Searches

### ??? Locations (Scenes)

Direct searches for room names work perfectly:

| Search Query | What It Finds | Expected Score |
|---|---|---|
| `library` | "Tall shelves of dusty books line the walls." | **High (0.8+)** |
| `ballroom` | "A grand room with chandeliers and polished floors for dancing." | **High (0.8+)** |
| `kitchen` | "The scent of old cooking lingers, mingled with something metallic." | **High (0.8+)** |
| `dining` | "A long mahogany table dominates the space, set for twelve." | **High (0.8+)** |
| `study` | "A small, wood-paneled room filled with books and secrets." | **High (0.8+)** |
| `lounge` | "A cozy sitting room with plush furniture and the faint aroma of brandy." | **High (0.8+)** |
| `conservatory` | "A sunlit greenhouse filled with exotic plants." | **High (0.8+)** |

### ?? Characters (NPCs)

Search for character descriptions:

| Search Query | Matches | Expected Score |
|---|---|---|
| `academic` | Professor Plum: "An absent-minded academic in deep purple attire" | **High (0.7+)** |
| `military` | Colonel Mustard: "A retired military man with a booming voice" | **High (0.7+)** |
| `housekeeper` | Mrs. White: "The loyal housekeeper, stoic and silent but observant." | **High (0.7+)** |
| `socialite` | Miss Scarlett: "A glamorous socialite in a scarlet gown" | **High (0.7+)** |
| `businessman` | Mr. Green: "A nervous businessman, impeccably dressed" | **High (0.7+)** |
| `elegant` | Mrs. Peacock: "An elegant older woman with a love for gossip" | **High (0.7+)** |
| `purple` | Professor Plum (related to color) | **Medium (0.6+)** |
| `nervous` | Mr. Green (fidgets nervously) | **Medium (0.6+)** |

### ?? Weapons/Items

Search for weapon descriptions:

| Search Query | Item | Description Match | Score |
|---|---|---|---|
| `firearm` | Revolver | "A small but deadly firearm, polished to a shine." | **High (0.8+)** |
| `rope` | Rope | "A sturdy length of rope, frayed at one end." | **High (0.8+)** |
| `knife` | Knife | "A gleaming kitchen knife with a sharp edge." | **High (0.8+)** |
| `heavy` | Lead Pipe, Wrench, Candlestick | All described as heavy | **Medium (0.6+)** |
| `brass` | Candlestick | "A solid brass candlestick" | **High (0.7+)** |
| `metal` | Various weapons | Inherent material | **Medium (0.5+)** |

### ?? Semantic Searches (Related Concepts)

These work because they're semantically similar to the descriptions:

| Search Query | Likely Matches | Reasoning |
|---|---|---|
| `books` | Library, Study | Both mention books/studying |
| `studying` | Library, Study, Professor Plum | Academic spaces & character |
| `weapon` | All weapons/items | Semantic matching |
| `elegant` | Ballroom, Lounge, Dining Room, Mrs. Peacock | Luxury/refined spaces & character |
| `plants` | Conservatory | Explicitly mentioned |
| `games` | Billiard Room | "room for games and gossip" |
| `suspect` | All NPCs | They're all suspects |
| `clue` | All weapons | They're clues in the game |
| `wealthy` | Study, Dining Room, NPCs | Implied by descriptions |

---

## Example Search Workflow

### Step 1: Load Game & Generate Embeddings
```
1. Go to Games page
2. Select "Clue Mansion"
3. Navigate to Embedding Manager (/embedding-playground)
4. Click "Load GamePack & Generate Embeddings"
5. Wait for success notification and check logs
```

### Step 2: Try a Simple Search
```
Query: "library"
Expected Results:
- Library scene: 0.900+ (direct match)
- Study scene: 0.650+ (both have books)
- Professor Plum: 0.550+ (academic)
```

### Step 3: Try a Semantic Search
```
Query: "plants"
Expected Results:
- Conservatory: 0.800+ (explicitly mentions "exotic plants")
- Library: 0.400+ (has plant-like growth from dust)
```

### Step 4: Try a Weapon Search
```
Query: "firearm"
Expected Results:
- Revolver: 0.900+ (explicitly "firearm")
- Lead Pipe: 0.500+ (also a weapon)
- Knife: 0.480+ (also a weapon)
```

---

## Scoring Reference

Your search results will show scores between **0.0 and 1.0**:

- **0.9 - 1.0** = Excellent match (exact or very similar words)
- **0.7 - 0.9** = Strong match (related concepts)
- **0.5 - 0.7** = Good match (semantic relationship)
- **0.3 - 0.5** = Weak match (distant relationship)
- **0.0 - 0.3** = Very weak or no match

---

## Why Some Searches Don't Return Results

If a search returns nothing or very weak scores, it's because:

1. **No matching descriptions**: The search term doesn't relate to any element description
2. **Wrong semantic domain**: E.g., searching for "outdoor" won't find anything (mansion is indoors)
3. **Too generic**: Common words like "the", "a", "room" are too generic to embed well
4. **Wrong game**: Make sure you've loaded Clue Mansion (not a different game pack)

---

## Debugging Checklist

If searches aren't working as expected:

- [ ] GamePack "Clue Mansion" is selected (check header)
- [ ] You clicked "Load GamePack & Generate Embeddings"
- [ ] Logs show "Embeddings Created" message with count
- [ ] Query is not empty
- [ ] Search term relates to actual game descriptions
- [ ] Check logs for error messages
- [ ] Verify embedding length is consistent (should be 384)

---

## Pro Tips

### 1. Use Actual Description Words
Best searches use words directly from element descriptions:
- ? `"dusty"` (Library has "dusty books")
- ? `"mahogany"` (Dining Room has "mahogany table")
- ? `"exotic"` (Conservatory has "exotic plants")

### 2. Single Concepts Work Better
- ? `"book"` - Clear concept
- ? `"book and weapon and plant"` - Splits focus

### 3. Look for Synonyms
- Embedding model understands:
  - `"gun"` ? `"firearm"` ? `"revolver"`
  - `"study"` ? `"academic"` ? `"scholar"`
  - `"dance"` ? `"ballroom"` ? `"elegant"`

### 4. Attribute-Based Searches
- `"ornate"` ? Finds elegant/fancy items
- `"cold"` ? Finds freezer, kitchen
- `"mysterious"` ? Finds secret passages

---

## Complete Game Element List

### Scenes (with key keywords)
- Hall: "grand", "pillars", "staircase", "formal"
- Study: "wood-paneled", "books", "secrets", "papers"
- Library: "dusty", "books", "old paper", "tobacco"
- Lounge: "plush furniture", "fireplace", "brandy"
- Billiard Room: "games", "green felt", "cues"
- Conservatory: "plants", "greenhouse", "humid"
- Dining Room: "mahogany table", "candles"
- Kitchen: "cooking", "range", "metallic"
- Ballroom: "chandeliers", "dancing", "elegant"
- Porch: "entrance", "outside"

### NPCs (with keywords)
- Professor Plum: "academic", "purple", "spectacles"
- Colonel Mustard: "military", "medals", "booming"
- Mr. Green: "businessman", "nervous", "tie"
- Mrs. White: "housekeeper", "maid", "rag"
- Miss Scarlett: "socialite", "scarlet", "glamorous"
- Mrs. Peacock: "elegant", "gossip", "jewels", "blue"

### Weapons/Items (with keywords)
- Revolver: "firearm", "deadly", "polished", "six-chamber"
- Lead Pipe: "heavy", "blunt", "plumbing", "bent"
- Rope: "sturdy", "frayed", "coiled"
- Candlestick: "brass", "solid", "elegant", "wax"
- Knife: "gleaming", "blade", "sharp", "kitchen"
- Wrench: "heavy", "grease", "metal", "mechanical"

