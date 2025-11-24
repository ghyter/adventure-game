# Promotion of DefaultState to Full Property with Radio Button Selection

## Overview

This update promotes `DefaultState` from being stored as a key in the `Properties` dictionary to being a full first-class property on the `GameElement` class. Additionally, it adds radio button controls to the States editor UI, allowing users to visually select which state should be the default.

## Changes Made

### 1. GameElement Model Changes

**File:** `AdventureGame.Engine\Models\GameElements.cs`

#### Before (DefaultState as Dictionary Entry)
```csharp
public abstract class GameElement : IJsonOnDeserialized
{
    [JsonInclude]
    public Dictionary<string, string?> Properties { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    protected GameElement()
    {
        Properties.TryAdd(PropertyKeys.DefaultState, "");
    }

    [JsonIgnore]
    public string DefaultState
    {
        get => Properties.TryGetValue(PropertyKeys.DefaultState, out var v) && !string.IsNullOrWhiteSpace(v)
            ? v!.Trim()
            : "";
        set => Properties[PropertyKeys.DefaultState] = value?.Trim() ?? "";
    }
}
```

#### After (DefaultState as Full Property)
```csharp
public abstract class GameElement : IJsonOnDeserialized
{
    // Default state - now a full property instead of being stored in Properties dictionary
    [JsonInclude]
    public string DefaultState { get; set; } = "default";

    [JsonInclude]
    public Dictionary<string, string?> Properties { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    protected GameElement()
    {
        // No longer need to add to Properties
        EnsureDefaultStateExists();
    }
}
```

**Key Changes:**
- ? `DefaultState` is now a `[JsonInclude]` property, serialized directly in JSON
- ? No longer uses `PropertyKeys.DefaultState` constant
- ? Simplified initialization - no dictionary manipulation needed
- ? Default value set directly: `= "default"`

### 2. Migration Logic for Backward Compatibility

Added migration in `OnDeserialized()` to handle old game packs:

```csharp
public virtual void OnDeserialized()
{
    Flags.TryAdd(FlagKeys.IsVisible, true);
    
    // Migrate old DefaultState from Properties dictionary if it exists
    if (Properties.TryGetValue(PropertyKeys.DefaultState, out var oldDefaultState) 
        && !string.IsNullOrWhiteSpace(oldDefaultState))
    {
        DefaultState = oldDefaultState.Trim();
        Properties.Remove(PropertyKeys.DefaultState); // Remove the old key
    }
    
    // Normalize DefaultState
    if (!string.IsNullOrWhiteSpace(DefaultState))
    {
        DefaultState = DefaultState.Trim();
    }

    // Ensure the default state exists after deserialization
    EnsureDefaultStateExists();
}
```

**Migration Strategy:**
1. Check if old `Properties["defaultState"]` exists
2. Copy value to new `DefaultState` property
3. Remove old dictionary entry
4. Normalize and validate

This ensures **backward compatibility** with existing game packs!

### 3. RadzenDictionaryEditor - Radio Button Support

**File:** `AdventureGame\Components\RadzenDictionaryEditor.razor`

Added support for default state selection via radio buttons when editing `GameElementState` dictionaries.

#### New Parameters
```csharp
/// <summary>
/// For GameElementState dictionaries, allows selection of default state via radio buttons.
/// Should be bound to the element's DefaultState property.
/// </summary>
[Parameter] public string? DefaultStateName { get; set; }
[Parameter] public EventCallback<string> DefaultStateNameChanged { get; set; }
```

#### UI Changes
```razor
@if (typeof(TValue) == typeof(GameElementState))
{
    <RadzenStack Orientation="Orientation.Vertical" Gap="12" Style="margin-top:8px;">
      @foreach (var item in items)
      {
        <RadzenCard Class="rde-card" Style="padding:8px;">
          <RadzenRow>
            <RadzenColumn Size="12">
              <div style="display:flex; align-items:flex-start; justify-content:space-between; gap:8px; width:100%;">
                <div style="display:flex; align-items:center; gap:12px; flex:1;">
                  @if (DefaultStateName != null)
                  {
                    <!-- Radio button for selecting default state -->
                    <RadzenRadioButtonList @bind-Value="@DefaultStateName" 
                                          TValue="string" 
                                          Change="@OnDefaultStateChanged"
                                          Orientation="Orientation.Horizontal">
                      <Items>
                        <RadzenRadioButtonListItem Text="" Value="@item.KeyString" />
                      </Items>
                    </RadzenRadioButtonList>
                  }
                  <RadzenFormField Text="State Key" Class="rde-formfield" Style="flex:1; width:100%;">
                    <RadzenTextBox @bind-Value="item.KeyString" ... />
                  </RadzenFormField>
                </div>
                <div style="display:flex; align-items:flex-start;">
                  <RadzenButton Icon="delete" ... />
                </div>
              </div>
            </RadzenColumn>
          </RadzenRow>
          ...
        </RadzenCard>
      }
    </RadzenStack>
}
```

**Visual Layout:**
```
????????????????????????????????????????????????????????????
? (?) State Key: [open_________________________]     [×]   ?
?                                                           ?
?     Description:                                          ?
?     [The desk is open and accessible_________]           ?
?     [_______________________________________]            ?
?     [_______________________________________]            ?
????????????????????????????????????????????????????????????

????????????????????????????????????????????????????????????
? ( ) State Key: [closed_______________________]     [×]   ?
?                                                           ?
?     Description:                                          ?
?     [The desk is closed__________________]               ?
?     [_______________________________________]            ?
?     [_______________________________________]            ?
????????????????????????????????????????????????????????????
```

The filled radio button (?) indicates the default state!

### 4. GameElementEditor Integration

**File:** `AdventureGame\Components\GameElementEditor.razor`

Updated the States tab to pass DefaultState binding:

#### Before
```razor
<RadzenTabsItem Text="States">
    <RadzenDictionaryEditor TKey="string" 
                           TValue="GameElementState" 
                           Dictionary="@Element.States" 
                           OnChange="@(async () => CurrentGameService.MarkDirty())" 
                           Style="width:100%;" />
</RadzenTabsItem>
```

#### After
```razor
<RadzenTabsItem Text="States">
    <RadzenDictionaryEditor TKey="string" 
                           TValue="GameElementState" 
                           Dictionary="@Element.States" 
                           DefaultStateName="@Element.DefaultState"
                           DefaultStateNameChanged="@((newState) => { Element.DefaultState = newState; CurrentGameService.MarkDirty(); })"
                           OnChange="@(async () => CurrentGameService.MarkDirty())" 
                           Style="width:100%;" />
</RadzenTabsItem>
```

**Bindings:**
- `DefaultStateName="@Element.DefaultState"` - Current default state (one-way)
- `DefaultStateNameChanged="..."` - Updates Element.DefaultState when user clicks radio button
- Also marks the game pack as dirty so save indicator appears

## Benefits

### 1. ? Cleaner Data Model
- DefaultState is now a direct property, not hidden in a dictionary
- JSON serialization is explicit and clear
- Less indirection when accessing default state

### 2. ? Better JSON Structure

**Before (DefaultState in Properties):**
```json
{
  "$type": "item",
  "Name": "desk",
  "Properties": {
    "defaultState": "closed",  ? Hidden in dictionary
    "weight": "50"
  },
  "States": {
    "open": { "Description": "..." },
    "closed": { "Description": "..." }
  }
}
```

**After (DefaultState as Property):**
```json
{
  "$type": "item",
  "Name": "desk",
  "DefaultState": "closed",  ? Clear, first-class property
  "Properties": {
    "weight": "50"
  },
  "States": {
    "open": { "Description": "..." },
    "closed": { "Description": "..." }
  }
}
```

### 3. ? Improved UI/UX
- Visual radio buttons make it obvious which state is default
- Single click to change default state
- No need to manually edit Properties dictionary
- Clearer intent - users understand what they're selecting

### 4. ? Backward Compatible
- Old game packs automatically migrate on load
- Old `Properties["defaultState"]` entries are detected and migrated
- No data loss
- Seamless upgrade path

### 5. ? Runtime State Tracking Unchanged
- `Properties["CurrentState"]` is still used for runtime state during gameplay
- `DefaultState` is the **initial** state (from game pack definition)
- `CurrentState` is the **runtime** state (changes during play)
- Clear separation of concerns

## Current vs Default State

### DefaultState (New Full Property)
- **Purpose:** Initial state when element is created
- **Storage:** First-class `[JsonInclude]` property
- **Set By:** Game designer in editor (via radio buttons)
- **Used By:** Game initialization, validation, fallback
- **Changes:** Rarely (only by designer)

### CurrentState (Still in Properties Dictionary)
- **Purpose:** Runtime state during active gameplay
- **Storage:** `Properties["CurrentState"]` dictionary entry
- **Set By:** Game effects, verb outcomes, triggers
- **Used By:** Condition evaluator, game logic
- **Changes:** Frequently (during gameplay)

### State Resolution Logic
```csharp
// In DslEvaluator.GetGameElementProperty()
if (propertyName.Equals("state", StringComparison.OrdinalIgnoreCase))
{
    // Check CurrentState first (runtime), then DefaultState (initial)
    if (element.Properties.TryGetValue("CurrentState", out var currentState) 
        && !string.IsNullOrWhiteSpace(currentState))
    {
        return currentState;  // ? Runtime state wins
    }
    return element.DefaultState;  // ? Fallback to default
}
```

**Example Flow:**
1. Designer creates desk with `DefaultState = "closed"`
2. Game starts, desk state is "closed" (from DefaultState)
3. Player opens desk ? `Properties["CurrentState"] = "open"`
4. Condition "desk state is open" checks CurrentState first ? returns "open" ?
5. Game resets ? CurrentState cleared, back to DefaultState "closed"

## Migration Testing

### Test Case 1: New Game Pack
```json
{
  "Name": "New Pack",
  "Elements": [
    {
      "$type": "item",
      "Name": "chest",
      "DefaultState": "locked",  ? Already promoted
      "States": {
        "locked": { "Description": "..." },
        "unlocked": { "Description": "..." }
      }
    }
  ]
}
```
? Works immediately, no migration needed

### Test Case 2: Old Game Pack (Before Update)
```json
{
  "Name": "Old Pack",
  "Elements": [
    {
      "$type": "item",
      "Name": "chest",
      "Properties": {
        "defaultState": "locked"  ? Old location
      },
      "States": {
        "locked": { "Description": "..." },
        "unlocked": { "Description": "..." }
      }
    }
  ]
}
```

**After Loading:**
```json
{
  "Name": "Old Pack",
  "Elements": [
    {
      "$type": "item",
      "Name": "chest",
      "DefaultState": "locked",  ? Migrated!
      "Properties": {},  ? Cleaned up
      "States": {
        "locked": { "Description": "..." },
        "unlocked": { "Description": "..." }
      }
    }
  ]
}
```
? Migrated automatically via `OnDeserialized()`

### Test Case 3: Mixed Pack
```json
{
  "Name": "Mixed Pack",
  "Elements": [
    {
      "$type": "item",
      "Name": "desk",
      "DefaultState": "closed"  ? New style
    },
    {
      "$type": "item",
      "Name": "door",
      "Properties": {
        "defaultState": "locked"  ? Old style
      }
    }
  ]
}
```
? Both work correctly, old entries migrated on load

## UI Workflow

### Creating a New Element
1. User creates new Item
2. Adds states: "open", "closed", "broken"
3. Radio buttons appear next to each state
4. User clicks radio button next to "closed"
5. `Element.DefaultState = "closed"`
6. Editor marks pack as dirty
7. User saves ? DefaultState persisted in JSON

### Editing Existing Element
1. User opens element with existing states
2. Radio button shows which state is currently default (?)
3. User clicks different radio button to change default
4. `Element.DefaultState` updated immediately
5. Pack marked dirty
6. User saves ? new default persisted

### Deleting States
**Protection:** Cannot delete the default state
```csharp
public bool RemoveStateSafely(string name)
{
    var key = (name ?? "").Trim();
    if (!States.ContainsKey(key)) return false;
    if (States.Count <= 1) return false;
    if (string.Equals(DefaultState, key, StringComparison.OrdinalIgnoreCase)) 
        return false;  // ? Prevent deletion of default state
    return States.Remove(key);
}
```

**Workflow:**
1. User tries to delete state marked as default
2. Delete button should be disabled (UI enforcement)
3. User must first change default to another state
4. Then deletion is allowed

## Files Modified

### 1. AdventureGame.Engine\Models\GameElements.cs
- **Changed:** Promoted `DefaultState` to `[JsonInclude]` property
- **Added:** Migration logic in `OnDeserialized()`
- **Removed:** Dictionary manipulation for DefaultState in constructor
- **Simplified:** `EnsureDefaultStateExists()` method

### 2. AdventureGame\Components\RadzenDictionaryEditor.razor
- **Added:** `DefaultStateName` parameter
- **Added:** `DefaultStateNameChanged` event callback
- **Added:** Radio button UI for GameElementState dictionaries
- **Enhanced:** Layout to show radio buttons next to state keys

### 3. AdventureGame\Components\GameElementEditor.razor
- **Updated:** States tab to bind `DefaultStateName`
- **Added:** Event handler for `DefaultStateNameChanged`
- **Integrated:** Marks game pack dirty when default state changes

## Constants Cleanup

The `PropertyKeys.DefaultState` constant is now **obsolete** but kept for migration:

**File:** `AdventureGame.Engine\Helpers\GameElementHelpers.cs`

```csharp
public static class PropertyKeys
{
    // OBSOLETE: DefaultState is now a full property, not stored in dictionary
    // Kept for backward compatibility during migration
    public const string DefaultState = "defaultState";
    
    public const string DefaultAlias = "defaultAlias";
}
```

Can be removed in a future version once all packs are migrated.

## Testing Checklist

- [x] Create new element with states ? default selection works
- [x] Edit existing element ? radio button shows current default
- [x] Change default state ? updates immediately
- [x] Save and reload ? default state persists
- [x] Load old game pack ? migration works
- [x] CurrentState vs DefaultState ? both work correctly
- [x] Condition evaluator ? still checks CurrentState first
- [x] Session Audit ? displays correct state
- [x] Cannot delete default state ? validation works
- [x] Radio buttons ? only one selected at a time

## Summary

### What Changed
? **DefaultState** promoted from `Properties` dictionary to full property  
? **Radio buttons** added to States editor for visual selection  
? **Migration logic** added for backward compatibility  
? **JSON structure** improved - cleaner, more explicit  
? **UI/UX** enhanced - more intuitive state management  

### What Stayed the Same
? **CurrentState** still uses `Properties["CurrentState"]` for runtime tracking  
? **State resolution** logic unchanged (CurrentState ? DefaultState fallback)  
? **Evaluator** works the same way  
? **All existing features** continue to work  

### Why This Matters
- **Clearer Intent:** Default state is now obvious in both code and JSON
- **Better Tooling:** IDE autocomplete works better with properties vs dictionary keys
- **Type Safety:** Compiler can validate DefaultState exists
- **User Experience:** Radio buttons make selection intuitive and error-free
- **Maintenance:** Easier to understand and modify code

This is a **quality of life improvement** that makes the editor more professional and the data model more maintainable!
