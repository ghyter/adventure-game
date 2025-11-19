# ?? Game Logic DSL System - Complete Implementation

## What Has Been Created

A full **Domain Specific Language (DSL)** system for evaluating complex game logic conditions in your Blazor/MAUI hybrid project.

### System Architecture

```
User Input (DSL Text)
        ?
    Tokenizer (DslTokenizer)
        ?
    Parser (DslParser)
        ?
    AST (Abstract Syntax Tree)
        ?
    Semantic Validator (DslSemanticValidator)
        ?
    Evaluator (DslEvaluator)
        ?
    Boolean Result
```

---

## ?? Complete File Structure

### AST Classes (`/DSL/AST/`)
```
ConditionNode.cs          ? Base node + visitor pattern
LogicalNodes.cs           ? AndNode, OrNode, NotNode
RelationNodes.cs          ? RelationNode, SubjectRef, ObjectRef, CountRelationNode, DistanceRelationNode
```

### Tokenizer (`/DSL/Tokenizer/`)
```
Token.cs                  ? Token class + TokenType enum
DslTokenizer.cs           ? Lexical analyzer
```

### Parser (`/DSL/Parser/`)
```
DslParseResult.cs         ? Parse result + error reporting
DslParser.cs              ? Recursive descent parser (recursive descent)
```

### Validation (`/DSL/Validation/`)
```
DslSemanticValidator.cs   ? Semantic validation against game elements
```

### Evaluation (`/DSL/Evaluation/`)
```
DslEvaluator.cs           ? AST evaluation against game state
```

### Facade Service (`/DSL/`)
```
DslService.cs             ? Main API for parse ? validate ? evaluate
```

### Tests (`/DSL/` in Tests project)
```
DslParserTests.cs         ? Unit tests for all DSL features
```

---

## ?? Supported DSL Expressions

### Simple Comparisons
```
player is target
target.state is open
item jade_key is_in player.inventory
```

### Numeric Comparisons
```
player.attribute constitution is_less_than 3
npc monster.health is_greater_than 50
player.level is_equal_to 5
```

### Boolean Logic
```
(player has silver_key or player has skeleton_key) and not target.isMovable is true
condition1 and condition2 or condition3
not (player is target)
```

### Count Relations
```
player.visits kitchen is_greater_than 3
player.visits bedroom is_equal_to 1
```

### Distance Relations
```
npc monster.distance_from player is_greater_than 2
player.distance_from scene guard_post is_less_than 5
```

### Complex Nested Expressions
```
(player has jade_key or player has silver_key) and 
not target.state is locked and
player.visits treasury is_greater_than 0
```

---

## ?? DSL Grammar (Formal)

```
<Expression>       ::= <OrExpr>
<OrExpr>           ::= <AndExpr> ( "or" <AndExpr> )*
<AndExpr>          ::= <UnaryExpr> ( "and" <UnaryExpr> )*
<UnaryExpr>        ::= "not" <UnaryExpr>
                     | "(" <Expression> ")"
                     | <Relation>

<Relation>         ::= <PropertyAccess> <Comparison> <Value>
                     | <CountRelation>
                     | <DistanceRelation>

<PropertyAccess>   ::= <Subject> ( "." <Identifier> )*
<Subject>          ::= "player" | "target" | "target2" | "currentScene" | 
                       "session" | "log" | "item" <Id> | "npc" <Id> | 
                       "scene" <Id> | "exit" <Id>

<CountRelation>    ::= <Subject> ".visits" <Identifier> <Comparison> <Number>
<DistanceRelation> ::= <Subject> ".distance_from" <Subject> <Comparison> <Number>

<Comparison>       ::= "is_less_than" | "is_greater_than" | "is_equal_to" |
                       "is_not_equal_to" | "is" | "is_not" | "is_in" | "is_empty"

<Value>            ::= <Identifier> | <Number> | "true" | "false"
```

---

## ?? How to Use

### 1. Parse DSL Text
```csharp
var parser = new DslParser();
var result = parser.Parse("player is target and target.state is open");

if (result.Success)
{
    Console.WriteLine("Parsed successfully!");
    // result.Ast contains the AST
}
else
{
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"Error: {error.Message}");
    }
}
```

### 2. Validate Against Game Elements
```csharp
var validator = new DslSemanticValidator(result);
validator.SetValidElements(new[] { "player", "target", "jade_key", ... });
validator.SetValidScenes(new[] { "kitchen", "bedroom", ... });
validator.SetValidAttributes(new[] { "constitution", "strength", ... });

bool isValid = validator.Validate(result.Ast);
```

### 3. Evaluate Against Game State
```csharp
var context = new MyGameEvaluationContext(gameSession, gameState);
var evaluator = new DslEvaluator(context);
bool conditionMet = evaluator.Evaluate(result.Ast);

if (conditionMet)
{
    // Trigger game logic
}
```

### 4. Or Use the Facade Service
```csharp
var service = new DslService(validator);
var result = service.ParseAndValidate(dslText);

if (result.Success)
{
    bool conditionMet = service.Evaluate(result.Ast!, gameContext);
}
```

---

## ?? Testing

### Run Unit Tests
```bash
dotnet test AdventureGame.Engine.Tests --filter "DslParserTests"
```

### Test Coverage
- ? Simple comparisons
- ? AND/OR/NOT logic
- ? Parenthesized expressions
- ? Visits clauses
- ? Distance relations
- ? Attribute comparisons
- ? Invalid syntax detection
- ? AST-to-JSON serialization
- ? Mock evaluation

---

## ?? How It Works Internally

### Tokenizer (`DslTokenizer`)
Converts raw DSL text into tokens:
```
"player is target" 
? [Identifier("player"), Is, Identifier("target"), EndOfInput]
```

### Parser (`DslParser`) - Recursive Descent
Builds an Abstract Syntax Tree (AST) from tokens:
```
player is target and target.state is open

? AndNode(
    left: RelationNode(player IS target),
    right: RelationNode(target.state IS open)
  )
```

### Semantic Validator (`DslSemanticValidator`)
Checks that all referenced elements exist:
- ? "player" is valid
- ? "target" is valid
- ? "jade_key" exists in game
- ? "unknown_element" ? Warning

### Evaluator (`DslEvaluator`)
Recursively evaluates the AST against game state:
```
AND(
  Relation(player IS target) ? true,
  Relation(target.state IS open) ? false
) ? false
```

---

## ?? Class Diagram

```
ConditionNode (abstract)
??? AndNode
??? OrNode
??? NotNode
??? RelationNode
??? CountRelationNode
??? DistanceRelationNode

DslService (Facade)
??? DslParser
??? DslSemanticValidator
??? DslEvaluator

DslEvaluationContext (abstract)
??? [Your game-specific implementation]
```

---

## ?? Next Steps for Integration

### 1. Implement Your Evaluation Context
```csharp
public class YourGameEvaluationContext : DslEvaluationContext
{
    private readonly GameSession _session;
    
    public override object? GetPlayer() => _session.Player;
    public override object? GetTarget() => _session.CurrentTarget;
    // ... implement other methods
}
```

### 2. Use in Game Triggers/Conditions
```csharp
public class GameTrigger
{
    public string ConditionDsl { get; set; }  // Your DSL text
    public string Action { get; set; }
    
    public bool ShouldFire(GameSession session)
    {
        var service = new DslService(validator);
        var result = service.ParseAndValidate(ConditionDsl);
        
        if (!result.Success) return false;
        
        var context = new YourGameEvaluationContext(session);
        return service.Evaluate(result.Ast!, context);
    }
}
```

### 3. Create Blazor Editor Component (Next Phase)
Could create a Blazor component with:
- DSL text editor
- Real-time error highlighting
- AST visualization
- Evaluation tester

---

## ?? Example DSL Expressions

### Quest Logic
```
player has quest_crystal or player has ancient_rune
```

### Room Access Control
```
target.state is open and not npc guard.is_blocking is true
```

### Inventory Check
```
item gold_key is_in player or item silver_key is_in player
```

### Attribute-Based Combat
```
player.attribute strength is_greater_than 10 and npc enemy.health is_less_than 20
```

### Multi-Part Quest Condition
```
player.visits first_location is_greater_than 0 and 
player.visits second_location is_greater_than 0 and
not player.has failed_quest is true
```

### Complex Distance-Based Logic
```
(npc guard.distance_from player is_less_than 3 and target.state is locked) or
player.attribute stealth is_greater_than npc guard.perception
```

---

## ?? Key Features

? **Full Expression Support**
- Logical operators (and, or, not)
- Comparison operators (is, is_not, is_less_than, etc.)
- Parentheses and precedence

? **Flexible Property Access**
- Simple: `player is target`
- Nested: `target.state is open`
- Attributes: `player.attribute constitution is_less_than 3`

? **Game-Specific Relations**
- Visits tracking: `player.visits scene is_greater_than N`
- Distance calculation: `entity.distance_from player is_less_than N`

? **Rich Error Reporting**
- Syntax errors with position info
- Semantic warnings for unknown elements
- Clear error messages

? **AST Serialization**
- Convert AST to JSON for visualization
- Useful for debugging and UI display

? **Extensible Design**
- Implement your own `DslEvaluationContext`
- Add custom relations as needed
- Visitor pattern for AST traversal

---

## ?? Files Summary

| File | Lines | Purpose |
|------|-------|---------|
| ConditionNode.cs | 20 | Base class + visitor pattern |
| LogicalNodes.cs | 50 | AND, OR, NOT nodes |
| RelationNodes.cs | 130 | Relation and ref classes |
| Token.cs | 40 | Token definition |
| DslTokenizer.cs | 150 | Lexical analyzer |
| DslParseResult.cs | 40 | Parse result class |
| DslParser.cs | 350 | Recursive descent parser |
| DslSemanticValidator.cs | 90 | Semantic validation |
| DslEvaluator.cs | 120 | AST evaluator |
| DslService.cs | 80 | Facade service |
| DslParserTests.cs | 100 | Unit tests |
| **TOTAL** | **~1100** | **Complete DSL System** |

---

## ? Status

- ? Tokenizer implemented and tested
- ? Recursive descent parser fully functional
- ? AST classes complete with visitor pattern
- ? Semantic validator working
- ? Evaluator operational
- ? Unit tests passing
- ? Build successful
- ? Blazor editor component (next phase)
- ? Playground page (next phase)

---

## ?? What's Ready to Use

The entire DSL pipeline is ready:
1. **Parse** DSL text into AST
2. **Validate** against game elements
3. **Serialize** to JSON for display
4. **Evaluate** against game state

All with comprehensive error handling and extensibility points.

---

## ?? Next Phase Options

If you want to continue:

1. **Create Blazor Editor Component**
   - Real-time syntax highlighting
   - Error underlines
   - AST visualization

2. **Build Playground Page**
   - Test DSL expressions
   - Visualize AST
   - Mock evaluations

3. **Integrate with Game Engine**
   - Use for triggers
   - Use for quest conditions
   - Use for NPC behaviors

---

## ?? Ready to Go!

The DSL system is **production-ready** and fully **extensible**. You can:
- Parse any DSL expression
- Get detailed error reports
- Validate against your game data
- Evaluate conditions against game state
- Extend with custom logic

Happy gamedev! ??
