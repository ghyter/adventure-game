# Fix: Session State Inspector Hierarchy - Show Contained Items

## Problem

The Session State Inspector's Hierarchy tab was not showing items that are contained within other items. For example, in the Clue game, if a lead pipe is inside a desk, it would not appear in the hierarchy.

**Before:**
```
?? Library
  ??? Study
    ?? Desk
    ?? Table
    ?? Colonel Mustard
```

The lead pipe inside the desk would not be shown.

## Root Cause

The original `GetHierarchyData()` method only looked at direct children of scenes:
```csharp
var items = Session.Pack.Elements.OfType<Item>()
    .Where(i => i.ParentId == scene.Id)  // Only scene-level items!
    .ToList();
```

This approach ignored items where `ParentId` pointed to another item instead of a scene.

## Solution

### 1. Created Recursive Item Builder

Added `BuildItemNode()` method that recursively builds item hierarchies:

```csharp
private HierarchyNode BuildItemNode(Item item, List<GameElement> allElements)
{
    var itemNode = new HierarchyNode { Text = $"?? {item.Name}" };

    // Find items contained within this item
    var containedItems = allElements.OfType<Item>()
        .Where(i => i.ParentId == item.Id)  // Items inside this item
        .ToList();

    if (containedItems.Any())
    {
        itemNode.Children = new List<HierarchyNode>();
        foreach (var containedItem in containedItems)
        {
            var childNode = BuildItemNode(containedItem, allElements);  // Recurse!
            itemNode.Children.Add(childNode);
        }
    }

    return itemNode;
}
```

### 2. Updated Scene Item Processing

Changed hierarchy building to use the recursive builder:

```csharp
foreach (var item in directItems)
{
    var itemNode = BuildItemNode(item, Session.Pack.Elements);  // Builds full tree
    sceneNode.Children.Add(itemNode);
}
```

### 3. Added NPC Inventory Support

NPCs can also contain items, so added support for that too:

```csharp
foreach (var npc in npcs)
{
    var npcNode = new HierarchyNode { Text = $"?? {npc.Name}" };
    
    // Add items contained in this NPC (inventory)
    var containedItems = Session.Pack.Elements.OfType<Item>()
        .Where(i => i.ParentId == npc.Id)
        .ToList();
    
    if (containedItems.Any())
    {
        npcNode.Children = new List<HierarchyNode>();
        foreach (var item in containedItems)
        {
            var itemNode = BuildItemNode(item, Session.Pack.Elements);
            npcNode.Children.Add(itemNode);
        }
    }
}
```

## What Now Works

### Example: Clue Game Hierarchy

```
?? Library
  ??? Study
    ?? Desk
      ?? Lead Pipe         ? Shows contained items!
      ?? Dagger
    ?? Table
    ?? Colonel Mustard
      ?? Revolver          ? NPC inventory!
      ?? Keys
```

### Multi-Level Nesting

Items can be nested multiple levels deep:

```
?? Kitchen
  ??? Pantry
    ?? Drawer
      ?? Envelope
        ?? Letter         ? 3 levels deep!
```

### Player Inventory

Items held by the player show under the player NPC:

```
?? Player (Colonel Mustard)
  ?? Candlestick
  ?? Rope
  ?? Knife
```

## How It Works

### ParentId Assignment

The hierarchy is built using `ParentId` relationships:

- **Scene Item**: `item.ParentId == scene.Id`
  ```csharp
  var leadPipe = new Item { Name = "Lead Pipe", ParentId = desk.Id };
  ```

- **Contained Item**: `item.ParentId == containerItem.Id`
  ```csharp
  var dagger = new Item { Name = "Dagger", ParentId = leadPipe.Id };
  ```

- **NPC Inventory**: `item.ParentId == npc.Id`
  ```csharp
  var revolver = new Item { Name = "Revolver", ParentId = npc.Id };
  ```

### Recursive Building Process

1. **Get Direct Scene Items** - Items where `ParentId == scene.Id`
2. **For Each Item**:
   - Create item node
   - Find contained items (where `ParentId == item.Id`)
   - Recursively build nodes for contained items
   - Attach to parent node
3. **Process NPCs** - Similar process for NPC inventory

## Code Changes

### File: `AdventureGame\Components\SessionAudit\SessionAudit.razor`

**Methods Added:**
- `BuildItemNode()` - Recursive helper to build item hierarchies

**Methods Updated:**
- `GetHierarchyData()` - Now calls recursive builder for items and handles NPC inventory

**Key Changes:**
- Items are now processed through `BuildItemNode()` instead of direct `Select()`
- Added NPC inventory support
- Recursive structure properly reflects containment relationships

## Visual Representation

### Tree Structure Icons

```
?? Level/Folder
??? Scene/Location
?? Item/Container
?? NPC/Character
```

Nested items show full containment hierarchy:
```
?? Study
  ??? Hallway
    ?? Desk (container)
      ?? Envelope (contained in desk)
        ?? Letter (contained in envelope)
```

## Testing

### Build Status ?
- Build succeeded with 0 errors
- No warnings

### Test Results ?
- Total: 135 tests
- Passed: 135
- Failed: 0

## Performance Considerations

The recursive approach is efficient because:
- Each item is processed once
- Recursion depth is shallow (typically 2-3 levels)
- Uses LINQ efficiently for lookups
- No circular reference issues (ParentId must form a tree, not a cycle)

## User Benefits

? **Complete Visibility** - All items are now visible in the hierarchy
? **Intuitive Structure** - Shows actual containment relationships
? **Inventory Support** - NPC and player inventory displayed
? **Nested Containers** - Can drill down through multiple levels
? **Better Debugging** - Easier to verify game state during testing

## Example Use Cases

### Clue Game - Finding Murder Weapons
```
Study
  ?? Desk
  ?  ?? Lead Pipe ? Visible!
  ?? Candlestick (on table)
  ?? Colonel Mustard
     ?? Revolver ? In inventory!
```

### Adventure Game - Quest Items
```
Dungeon
  ?? Treasure Chest
  ?  ?? Golden Amulet
  ?  ?? Ancient Map
  ?  ?  ?? Decryption Key
  ?  ?? Ruby
  ?? Goblin
     ?? Magic Scroll
```

### Text Adventure - World Building
```
House
  ?? Living Room
  ?  ?? Bookshelf
  ?  ?  ?? Book
  ?  ?  ?? Scroll
  ?  ?? Armchair
  ?     ?? Coin
  ?? Player
     ?? Sword
     ?? Torch
     ?? Backpack
        ?? Rope
        ?? Grapple
```

## Summary

? **Fixed**: Hierarchy now shows items contained within other items
? **Added**: Recursive item building with multi-level nesting support
? **Added**: NPC inventory display
? **Tested**: All 135 tests passing
? **Benefit**: Complete game state visibility in inspector

The Session State Inspector now properly reflects the full containment hierarchy of your game world!
