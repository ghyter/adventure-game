# Fix: Natural Language Processing in ConditionInput

## Problem

The `ConditionInput` component was directly calling `DslParser.Parse()` instead of using `DslService.ParseAndValidate()`, which meant the natural language preprocessor (`DslCanonicalizer`) was being bypassed.

**Symptom:**
- ? `"when item desk state is open"` - Parse error
- ? `"item desk.state is open"` - Works

## Root Cause

```csharp
// ConditionInput.razor (BEFORE - broken)
private void ValidateCondition()
{
    var parser = new DslParser();  // ? No natural language preprocessing!
    parseResult = parser.Parse(conditionText);
}
```

The `DslParser` only understands the canonical DSL syntax. Natural language phrases like "when" and implicit subjects need to be processed by `DslCanonicalizer` first.

## Solution

### 1. Updated ConditionInput Component

Added `DslService` parameter:

```csharp
[Parameter]
public DslService? DslService { get; set; }

private void ValidateCondition()
{
    // Use DslService if provided (includes natural language preprocessing)
    if (DslService != null)
    {
        parseResult = DslService.ParseAndValidate(conditionText);
    }
    else
    {
        // Fallback to direct parser (no natural language support)
        var parser = new DslParser();
        parseResult = parser.Parse(conditionText);
    }
}
```

**Benefits:**
- ? Uses `DslCanonicalizer` when `DslService` is provided
- ? Backward compatible (works without DslService for simple cases)
- ? Single source of truth for parsing logic

### 2. Updated ConditionTester Page

Pass `dslService` to each `ConditionInput`:

```razor
<ConditionInput Value="@condition.ConditionText" 
              DslService="@dslService"  ? ADDED
              OnParsed="@((result) => OnConditionParsed(condition, result))" />
```

## Processing Pipeline (Now Working)

```
User Input: "when desk state is closed"
    ?
DslService.ParseAndValidate()
    ?
DslCanonicalizer.Canonicalize()
    - Strip "when" ? "desk state is closed"
    - Infer subject ? "item desk state is closed"
    - Handle possessive ? "item desk.state is closed"
    ?
DslParser.Parse()
    - Tokenize
    - Parse AST
    ?
DslSemanticValidator.Validate() (if configured)
    ?
Return DslParseResult
    ?
ConditionInput displays result
```

## What Now Works

### Natural Language Prefixes
```
? "when desk state is closed"
? "if player health is_less_than 50"
? "while target is visible"
```

### Implicit Subject Inference
```
? "desk state is closed" ? "item desk.state is closed"
? "guard health is 100" ? "npc guard health is 100"
? "kitchen is visited" ? "scene kitchen is visited"
```

### Combined Features
```
? "when desk state is closed"
   ? Strip "when"
   ? Infer "item desk"
   ? Result: "item desk.state is closed" ? PARSES!
```

## Files Modified

1. **AdventureGame\Components\Rules\ConditionInput.razor**
   - Added `DslService` parameter
   - Updated `ValidateCondition()` to use DslService when available
   - Updated placeholder text with natural language example

2. **AdventureGame\Components\Pages\Tools\ConditionTester.razor**
   - Pass `dslService` to `ConditionInput` component

## Testing

### All Tests Pass ?
- **Total**: 135 tests
- **Passed**: 135
- **Failed**: 0
- **Duration**: ~989ms

### Manual Test Cases

**Test 1: Natural Language Prefix**
```
Input: "when desk state is closed"
Expected: ? Parses successfully
Result: ? PASS
```

**Test 2: Implicit Subject**
```
Input: "desk state is closed"
Expected: ? Parses successfully (infers "item")
Result: ? PASS
```

**Test 3: Explicit Syntax (Backward Compatibility)**
```
Input: "item desk.state is closed"
Expected: ? Parses successfully
Result: ? PASS
```

**Test 4: Without DslService (Fallback)**
```
Input: "player is target"
DslService: null
Expected: ? Parses successfully (simple syntax)
Result: ? PASS
```

## Technical Details

### DslService Pipeline

The `DslService` integrates three components:

1. **DslCanonicalizer** - Natural language preprocessing
   ```csharp
   var canonical = _canonicalizer.Canonicalize(dslText, _vocab);
   ```

2. **DslParser** - Tokenization and AST generation
   ```csharp
   var result = _parser.Parse(canonical);
   ```

3. **DslSemanticValidator** - Optional semantic validation
   ```csharp
   if (result.Success && _validator != null)
       _validator.Validate(result.Ast);
   ```

### Why This Fix Works

**Before:** 
```
User Input ? DslParser.Parse() ? Parse Error ?
```

**After:**
```
User Input ? DslService.ParseAndValidate() 
           ? DslCanonicalizer (transforms natural language)
           ? DslParser.Parse() 
           ? Success ?
```

## Key Takeaway

**Always use `DslService.ParseAndValidate()` instead of `DslParser.Parse()` directly when natural language support is needed.**

The parser is a low-level component that only understands canonical DSL syntax. The service layer adds:
- Natural language preprocessing
- Vocabulary-aware canonicalization  
- Semantic validation
- Expression caching

## Related Documentation

- See `NATURAL_LANGUAGE_DSL_ENHANCEMENT.md` for details on canonicalization features
- See `CONDITION_TESTER_IMPLEMENTATION.md` for overall tester architecture
- See `CONDITION_TESTER_VERB_TRIGGER_ENHANCEMENT.md` for verb/trigger mode details

## Summary

? **Fixed**: ConditionInput now uses DslService for natural language support
? **Backward Compatible**: Fallback to DslParser when DslService not provided
? **All Tests Pass**: 135/135 tests passing
? **Natural Language Works**: "when desk state is closed" now parses correctly
