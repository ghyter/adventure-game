# PrintElementStateEffect Usage Guide

## Overview

`PrintElementStateEffect` prints the current state description of a game element. It outputs the element's name, description, and current state text to the game output. This is particularly useful for "look" commands and automatic room entry descriptions.

## Parameters

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `element` | gameElement | Yes | - | The element to describe (supports names, aliases, or special variables) |
| `includeStateText` | boolean | No | `true` | Whether to include the current state's description |
| `format` | string | No | `"{name}\n{description}\n{state}"` | Template for output formatting |

## Format Placeholders

The `format` parameter supports the following placeholders:

- `{name}` - Element's name
- `{description}` - Element's description
- `{state}` - Current state's description text
- `{stateName}` - Name of the current state
- `\n` - Line break
- `\t` - Tab character

## Usage Examples

### Example 1: Look at Current Scene

**Use Case**: Player enters a room or uses "look" command

```json
{
  "action": "print_element_state",
  "parameters": {
    "element": "currentScene"
  }
}
```

**Output**:
```
Great Hall
A vast chamber with vaulted ceilings and ornate columns.
The room is dimly lit by flickering torches on the walls.
```

### Example 2: Examine a Target Object

**Use Case**: Player examines an item

```json
{
  "action": "print_element_state",
  "parameters": {
    "element": "target"
  }
}
```

**Output**:
```
Ancient Chest
A weathered oak chest bound with iron straps.
The chest is securely locked with a rusty padlock.
```

### Example 3: Custom Format

**Use Case**: Brief description without state text

```json
{
  "action": "print_element_state",
  "parameters": {
    "element": "target",
    "includeStateText": "false",
    "format": "{name}: {description}"
  }
}
```

**Output**:
```
Golden Key: A small brass key with an ornate handle.
```

### Example 4: Detailed Format with State Name

**Use Case**: Show current state name for debugging or status

```json
{
  "action": "print_element_state",
  "parameters": {
    "element": "main_door",
    "format": "=== {name} ===\n{description}\n\nState: {stateName}\n{state}"
  }
}
```

**Output**:
```
=== Main Door ===
A heavy wooden door with iron hinges.

State: locked
The door is securely locked and won't budge.
```

## Common Use Cases

### 1. Room Entry (Trigger)

When player enters a new scene:

```json
{
  "type": "Trigger",
  "name": "Room Entry Description",
  "conditionGroups": [
    {
      "operator": "And",
      "conditions": [
        {
          "operator": "element_state_equals",
          "parameters": {
            "element": "currentScene",
            "stateName": "first_visit"
          }
        }
      ]
    }
  ],
  "effectGroups": [
    {
      "effects": [
        {
          "action": "print_element_state",
          "parameters": {
            "element": "currentScene"
          }
        },
        {
          "action": "set_element_state",
          "parameters": {
            "element": "currentScene",
            "stateName": "visited"
          }
        }
      ]
    }
  ]
}
```

### 2. Look Command (Verb)

Basic look command:

```json
{
  "type": "Verb",
  "verbPhrase": "look",
  "targetCount": 0,
  "effectGroups": [
    {
      "effects": [
        {
          "action": "print_element_state",
          "parameters": {
            "element": "currentScene"
          }
        }
      ]
    }
  ]
}
```

### 3. Examine Command (Verb)

Examine a specific object:

```json
{
  "type": "Verb",
  "verbPhrase": "examine",
  "targetCount": 1,
  "target1": {
    "mode": "All"
  },
  "effectGroups": [
    {
      "effects": [
        {
          "action": "print_element_state",
          "parameters": {
            "element": "target"
          }
        }
      ]
    }
  ]
}
```

### 4. Item Inspection with State Changes

Show different descriptions based on item state:

```json
{
  "type": "Verb",
  "verbPhrase": "open",
  "targetCount": 1,
  "conditionGroups": [
    {
      "operator": "And",
      "conditions": [
        {
          "operator": "element_state_equals",
          "parameters": {
            "element": "target",
            "stateName": "closed"
          }
        }
      ]
    }
  ],
  "effectGroups": [
    {
      "effects": [
        {
          "action": "set_element_state",
          "parameters": {
            "element": "target",
            "stateName": "open"
          }
        },
        {
          "action": "print_element_state",
          "parameters": {
            "element": "target"
          }
        }
      ]
    }
  ]
}
```

## Integration with Game Flow

### Typical Room Entry Flow

1. Player moves through exit
2. `MovePlayerThroughExitEffect` changes player location
3. Trigger fires when entering new scene
4. `PrintElementStateEffect` describes the new room
5. Player sees room description in output

### Typical Examine Flow

1. Player types "examine desk"
2. Parser resolves "desk" as target
3. Verb action executes
4. `PrintElementStateEffect` prints desk's current state
5. If desk is "closed", shows closed state description
6. If desk is "open", shows open state description

## Tips

**For Authors:**
- Use descriptive state names that match game events ("locked", "broken", "burning")
- Write engaging state descriptions that give players useful information
- Consider using custom formats for special items or locations
- Set `includeStateText="false"` for brief mentions in conversation

**For Room Descriptions:**
- Include atmosphere and mood in scene descriptions
- Mention obvious exits in the state text
- Describe lighting, sounds, and smells
- Update state when room conditions change (lights on/off, fire started, etc.)

**For Item Descriptions:**
- Base description should be unchanging (what the item looks like)
- State description should reflect current condition (open/closed, lit/unlit)
- Use states to show damage, wear, or transformation
- Consider multiple states for complex items (chest: locked ? closed ? open ? empty)

## Performance Notes

- Very lightweight effect, suitable for frequent use
- String formatting is efficient with minimal allocations
- No database or external calls
- Safe to use in every room entry and examine command

## Related Effects

- `PrintEffect` - Simple text output without element context
- `SetElementStateEffect` - Change element state
- `MovePlayerThroughExitEffect` - Move player to new scene
- `ElementStateEqualsCondition` - Check current state before printing

## Summary

`PrintElementStateEffect` is essential for creating immersive game descriptions. Use it whenever you need to show players what they see, whether entering a room, examining an object, or responding to their actions. The flexible format system allows you to customize output for any situation while keeping your game content organized and maintainable.
