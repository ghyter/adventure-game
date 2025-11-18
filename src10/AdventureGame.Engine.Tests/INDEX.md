# ?? Semantic Embedding Tests - Complete Index

## ?? Files Created

All files are in: `AdventureGame.Engine.Tests/`

### Test Implementation
- **`SemanticEmbeddingTests.cs`** - Main test file with 10 comprehensive unit tests

### Documentation (Read in This Order)
1. **`README.md`** ? **START HERE** - Overview and quick start
2. **`QUICK_REFERENCE.md`** - One-page cheat sheet for running tests
3. **`SEMANTIC_TESTS_GUIDE.md`** - Detailed explanation of each test
4. **`TEST_RESULTS_GUIDE.md`** - Debugging and failure pattern analysis
5. **`VISUAL_GUIDE.md`** - ASCII diagrams and visual explanations
6. **`IMPLEMENTATION_SUMMARY.md`** - What was created and why
7. **`README.md`** (this file) - Navigation guide

## ?? What's Included

### 10 Unit Tests

| # | Test Name | What It Tests | Your Issues |
|---|-----------|---------------|------------|
| 1 | `Weapon_Query` | Weapons vs non-weapons | Detects ballroom ranking |
| 2 | `Educator_Query` | Professor vs other NPCs | **Detects your issue** |
| 3 | `Firearm_Query` | Firearm weapons | Weapon sub-types |
| 4 | `Academic_Query` | Academic character | Alternative to educator |
| 5 | `Heavy_Query` | Heavy vs light items | Adjective search |
| 6 | `Elegant_Query` | Elegant vs ordinary | Style search |
| 7 | `Library_Query` | Library/Study locations | Location search |
| 8 | `Socialite_Query` | Socialite character | Character type |
| 9 | `Similar_Queries` | Synonym similarity | Query equivalence |
| 10 | `Dissimilar_Queries` | Opposite meanings | Semantic contrast |

### Documentation Set

- **5 markdown guides** with 100+ pages of documentation
- **Visual diagrams** showing test architecture
- **Troubleshooting strategies** for debugging failures
- **Code examples** for common issues

## ?? Quick Start (3 Steps)

### Step 1: Run Tests
```bash
dotnet test AdventureGame.Engine.Tests --filter "SemanticEmbeddingTests"
```

### Step 2: Check Results
Look for ? FAILED tests and note actual scores in debug output

### Step 3: Read Guide
Based on what fails:
- All pass? ? Celebrate! ?
- Some fail? ? Read `QUICK_REFERENCE.md`
- All fail? ? Read `TEST_RESULTS_GUIDE.md`

## ?? Documentation Guide

### For Different Readers

**I want to just run tests:**
? Read `QUICK_REFERENCE.md` (1 page)

**I want to understand my issues:**
? Read `README.md` then `VISUAL_GUIDE.md`

**Tests are failing, help me debug:**
? Read `TEST_RESULTS_GUIDE.md` (very detailed)

**I want all the details:**
? Read `SEMANTIC_TESTS_GUIDE.md` (comprehensive)

**I need the big picture:**
? Read `IMPLEMENTATION_SUMMARY.md`

### By Purpose

| Need | Read |
|------|------|
| Overview | README.md |
| Quick ref | QUICK_REFERENCE.md |
| Run tests | QUICK_REFERENCE.md |
| Test details | SEMANTIC_TESTS_GUIDE.md |
| Debugging | TEST_RESULTS_GUIDE.md |
| Visuals | VISUAL_GUIDE.md |
| Architecture | VISUAL_GUIDE.md |
| Summary | IMPLEMENTATION_SUMMARY.md |

## ?? Learning Path

### Beginner (Just want it working)
1. README.md (overview)
2. Run tests
3. QUICK_REFERENCE.md (if something fails)

### Intermediate (Want to understand)
1. README.md
2. QUICK_REFERENCE.md
3. SEMANTIC_TESTS_GUIDE.md
4. Run tests and verify results

### Advanced (Full mastery)
1. VISUAL_GUIDE.md (understand architecture)
2. SEMANTIC_TESTS_GUIDE.md (test details)
3. SemanticEmbeddingTests.cs (code)
4. TEST_RESULTS_GUIDE.md (debugging)

## ? Key Information

### What Tests Measure
- ? Semantic quality of embeddings
- ? Weapon search accuracy
- ? Character classification accuracy
- ? Location search accuracy
- ? Adjective-based search
- ? Synonym recognition

### What Tests Show You
- Exact similarity scores (0.72, 0.58, etc.)
- Which items rank highest/lowest
- Why search results are wrong
- How much improvement you've made

### Using Test Results

```
Test Output:
  ? Educator test FAILS with scores:
     Professor: 0.58
     Mr. Green: 0.62  ? Higher (PROBLEM!)

Next Step:
  1. Read QUICK_REFERENCE.md (section "Educator Query")
  2. Or read TEST_RESULTS_GUIDE.md (section "Educator Problem")
  3. Either fix Professor description OR debug model
  4. Re-run test to see improvement
```

## ?? File Organization

```
AdventureGame.Engine.Tests/
?
??? Test Code
?   ??? SemanticEmbeddingTests.cs    (10 tests)
?
??? Quick Start
?   ??? README.md                     (Main entry point)
?   ??? QUICK_REFERENCE.md            (Cheat sheet)
?
??? Detailed Guides
?   ??? SEMANTIC_TESTS_GUIDE.md       (Test details)
?   ??? TEST_RESULTS_GUIDE.md         (Debugging)
?   ??? VISUAL_GUIDE.md               (Diagrams)
?
??? Meta
    ??? IMPLEMENTATION_SUMMARY.md     (What was created)
    ??? README.md                     (This file)
```

## ?? Finding Answers

### How do I run the tests?
? `QUICK_REFERENCE.md` (section "Running the Tests")

### Why did my test fail?
? `TEST_RESULTS_GUIDE.md` (section "Failure Patterns")

### What do the scores mean?
? `QUICK_REFERENCE.md` or `SEMANTIC_TESTS_GUIDE.md` (both have score table)

### How do I fix the weapon issue?
? `VISUAL_GUIDE.md` (section "Your Problem #1")

### How do I fix the educator issue?
? `VISUAL_GUIDE.md` (section "Your Problem #2")

### What does this test actually do?
? `SEMANTIC_TESTS_GUIDE.md` (detailed breakdown for each test)

### My model isn't loading, help!
? `TEST_RESULTS_GUIDE.md` (section "All Tests Fail With Same Pattern")

### How do I know if I'm done fixing things?
? `QUICK_REFERENCE.md` (section "Success Criteria")

## ?? Workflow

### Initial Assessment
1. Run: `dotnet test ... --filter "SemanticEmbeddingTests"`
2. Count passing vs failing tests
3. Note actual scores from debug output
4. Document current state

### Identify Issues
1. Read `QUICK_REFERENCE.md` for failing tests
2. Understand what each test expects
3. See actual vs expected scores
4. Identify root cause

### Fix Problems
1. For description issues: Improve game element descriptions
2. For model issues: Debug ONNX loading
3. For vocabulary issues: Try different query words

### Validate Improvement
1. Make one fix at a time
2. Re-run tests
3. Compare scores before/after
4. Document improvement

### Celebrate Success
1. All tests passing ?
2. Semantic search working correctly
3. Ready for production

## ?? Addressing Your Issues

### Issue 1: "weapon" returns wrong items
- **Test:** `Semantics_Weapon_Query_ShouldRankWeapons_AboveNonWeapons`
- **Guide:** `TEST_RESULTS_GUIDE.md` ? "FAILURE PATTERN #2"
- **Quick Fix:** Add more weapon-specific keywords to descriptions

### Issue 2: "educator" returns Mr. Green
- **Test:** `Semantics_Educator_Query_ShouldRankEducator_HigherThanNonEducators`
- **Guide:** `VISUAL_GUIDE.md` ? "Your Problem #2"
- **Quick Fix:** Add professor/educator keywords, make Mr. Green less academic

## ? Success Indicators

- [ ] Tests run without errors
- [ ] Understand which tests pass/fail
- [ ] Can interpret similarity scores
- [ ] Know how to fix issues
- [ ] See improvement after first fix
- [ ] All (or most) tests passing

## ?? Related Files

In your project:
- `AdventureGame.Engine/Services/EmbeddingService.cs` - The service being tested
- `AdventureGame.Engine.Tests/EmbeddingServiceTests.cs` - Existing basic tests
- `AdventureGame.Engine/wwwroot/ExampleGames/ClueMansion1.0.json` - Test data source

## ?? Pro Tips

1. **Run tests in Release mode** for more accurate results
2. **Run tests with `-v detailed`** to see more output
3. **Add breakpoints** in tests to inspect embeddings
4. **Export scores to CSV** to visualize in Excel
5. **Run single test** at a time when debugging

## ?? Need Help?

Check these in order:
1. `README.md` - Overview
2. `QUICK_REFERENCE.md` - Quick answers
3. `TEST_RESULTS_GUIDE.md` - Debugging help
4. `SEMANTIC_TESTS_GUIDE.md` - Detailed info
5. `VISUAL_GUIDE.md` - Diagrams and examples

## Summary

You have:
- ? 10 comprehensive unit tests
- ? 5 detailed documentation files
- ? Clear debugging strategies
- ? Exact issue identification capability
- ? Improvement measurement tools

**Ready to get started?** Read `README.md` then run the tests! ??

---

**Last Updated:** 2025-01-14  
**Test Count:** 10 comprehensive tests  
**Documentation:** 7 guides + this index  
**Status:** Ready to use  
