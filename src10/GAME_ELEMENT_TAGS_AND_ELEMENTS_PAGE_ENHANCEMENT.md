# Game Element Tags & Enhanced Elements Page

## Summary of Changes

This update adds tagging support to GameElements and creates an enhanced Elements page with tile/grid view modes, filtering, and sorting capabilities.

## 1. Added Tags to GameElement

### GameElement.cs Changes

Added a new `Tags` property to the `GameElement` base class:

```csharp
[JsonInclude]
public HashSet<string> Tags { get; set; } = new(StringComparer.OrdinalIgnoreCase);
```

**Benefits:**
- Tags are searchable metadata for organizing elements
- Case-insensitive storage
- JSON serialized with game packs
- Separate from Aliases (which are for gameplay references)

## 2. Created StringHashSetEditor Component

### New Reusable Component

**File:** `AdventureGame.Engine\Components\StringHashSetEditor.razor`

A reusable component for editing HashSet<string> collections:

**Features:**
- Add multiple items via comma-separated input
- Remove items with × button
- Enter key support for quick adding
- Empty state message
- Sorted display

**Usage:**
```razor
<StringHashSetEditor Value="@element.Aliases" 
                     ValueChanged="@((newVal) => { element.Aliases = newVal; })" 
                     Placeholder="Add aliases..." />
```

**Styling:** Includes comprehensive CSS with:
- Tag-style display with badges
- Hover effects
- Responsive layout
- Theme variable integration

## 3. Updated GameElementEditor

### Integrated StringHashSetEditor for Both Aliases and Tags

**Before:**
```razor
<RadzenFormField Text="Aliases">
    <HashMapTagList @bind-Value="Element.Aliases" />
</RadzenFormField>
```

**After:**
```razor
<RadzenFormField Text="Aliases">
    <StringHashSetEditor Value="@Element.Aliases" 
                        ValueChanged="@((newVal) => { Element.Aliases = newVal; CurrentGameService.MarkDirty(); })" 
                        Placeholder="Add aliases... (comma-separated)" />
</RadzenFormField>

<RadzenFormField Text="Tags">
    <StringHashSetEditor Value="@Element.Tags" 
                        ValueChanged="@((newVal) => { Element.Tags = newVal; CurrentGameService.MarkDirty(); })" 
                        Placeholder="Add tags... (comma-separated)" />
</RadzenFormField>
```

## 4. Enhanced Elements Page

### New Features

**File:** `AdventureGame\Components\Pages\GameElementsPage.razor`

#### A. View Modes

- **Tile View** (default): Card-based layout with rich visual display
- **Grid View**: Traditional data grid with sorting and filtering

#### B. Search Functionality

Global search across:
- Element names
- Descriptions
- Aliases
- Tags

#### C. Tile View Features

**Compact Card Layout:**
```
????????????????????????????????????
? [item]              [clone][del] ?
? Wooden Desk                      ?
? ?? Study (2, 3)                  ?
?                                  ?
? Aliases:                         ?
? [desk] [table] [furniture] +2    ?
?                                  ?
? Tags:                            ?
? [movable] [interactive] +1       ?
????????????????????????????????????
```

**Card Elements:**
- Element kind badge (top-left)
- Action buttons (top-right: clone, delete)
- Large element name
- Location indicator with emoji
- Aliases section (blue tags)
- Tags section (green tags)
- "+N" indicator for overflow items

**Styling:**
- Responsive grid layout
- Hover effects (lift + shadow)
- Click to edit
- Stop propagation on action buttons

#### D. Grid View Features

**Enhanced DataGrid:**
- Sortable columns (Name, Kind)
- Filterable columns
- Paging (20 items per page)
- Compact tag display in columns
- Full action button suite

**Columns:**
1. **Name**: Bold, sortable, filterable
2. **Kind**: Element type, sortable, filterable
3. **Location**: Parent/coordinates display
4. **Aliases**: Tag chips (max 3 + overflow)
5. **Tags**: Tag chips (max 3 + overflow)
6. **Actions**: Edit, Clone, Delete buttons

### Styling

**File:** `AdventureGame\Components\Pages\GameElementsPage.razor.css`

**Key Styles:**
- `.elements-tiles`: Responsive grid (auto-fill, min 300px)
- `.element-tile`: Card with hover effects
- `.element-tile-kind-badge`: Uppercase badge
- `.alias-tag`: Blue tags for aliases
- `.tag-tag`: Green tags for tags
- `.grid-tags`: Compact tags for grid view

**Responsive Breakpoints:**
- Mobile: 1 column
- Desktop: Auto-fill (300px min)
- Large screens: 350px tiles

### UI Flow

```
[Type Dropdown?] [Add Scene] ????????????? [Search...] [??][?]
                                                        ?     ?
                                                      Tile  Grid
```

**Search Behavior:**
- Real-time filtering
- Searches across name, description, aliases, tags
- Case-insensitive
- Preserves kind filter

**View Toggle:**
- Persistent during session
- Remembers search state
- Instant switching

## Usage Examples

### Adding Tags to an Element

1. Open Elements page
2. Click on an element tile (or Edit in grid)
3. Navigate to "Details" tab
4. Find "Tags" field
5. Type tags comma-separated: `interactive, puzzle, required`
6. Click "Add"
7. Tags appear as green badges
8. Click Save

### Searching by Tags

1. Type in search box: `interactive`
2. All elements with "interactive" in name, description, aliases, or tags appear
3. Works in both Tile and Grid views

### Using Tile View

1. Click grid icon (??) to switch to Tile view
2. See rich cards with all metadata
3. Click card to edit
4. Click clone/delete buttons without editing
5. Scroll to see more (responsive grid)

### Using Grid View

1. Click list icon (?) to switch to Grid view
2. Click column headers to sort
3. Use built-in filters
4. Page through results
5. See condensed tag display

## Migration Notes

### Existing GamePacks

- **Backward Compatible**: Elements without Tags will have empty HashSet
- **No Data Loss**: Existing Aliases unchanged
- **Automatic Initialization**: Tags property auto-initialized on deserialization

### Component Replacement

- `HashMapTagList` still exists and works
- `StringHashSetEditor` is the new recommended component
- Both support same `HashSet<string>` binding
- `StringHashSetEditor` has better styling and features

## Benefits

### 1. Better Organization
- Tags provide flexible categorization
- Search across all metadata
- Filter by multiple criteria

### 2. Improved UX
- Visual tile view for browsing
- Grid view for bulk operations
- Responsive design
- Consistent styling

### 3. Developer Experience
- Reusable `StringHashSetEditor` component
- Clean separation of concerns
- Type-safe bindings
- Well-documented code

### 4. Future-Proof
- Tags can drive features like:
  - Tag-based queries in DSL
  - Auto-grouping in UI
  - Export/import by tags
  - Tag-based permissions

## Files Modified

1. `AdventureGame.Engine\Models\GameElements.cs`
   - Added `Tags` property

2. `AdventureGame.Engine\Components\StringHashSetEditor.razor`
   - New reusable component

3. `AdventureGame.Engine\Components\StringHashSetEditor.razor.css`
   - Component styling

4. `AdventureGame\Components\GameElementEditor.razor`
   - Integrated StringHashSetEditor for Aliases and Tags

5. `AdventureGame\Components\Pages\GameElementsPage.razor`
   - Complete redesign with Tile/Grid views
   - Added search functionality
   - Enhanced layout and UX

6. `AdventureGame\Components\Pages\GameElementsPage.razor.css`
   - Comprehensive styling for new features

## Testing Checklist

- [ ] Create element with tags
- [ ] Edit tags (add/remove)
- [ ] Search by tag name
- [ ] Switch between Tile and Grid views
- [ ] Clone element (tags copied)
- [ ] Save and reload (tags persisted)
- [ ] Filter by element kind + search
- [ ] Sort in Grid view
- [ ] Test responsive layout (mobile/desktop)
- [ ] Verify aliases still work
- [ ] Check tag display overflow (+N)

## Future Enhancements

### Short Term
- Tag suggestions/autocomplete
- Tag color coding
- Bulk tag operations
- Tag statistics/clouds

### Long Term
- Tag-based DSL conditions: `"player has_tag explorer"`
- Tag inheritance (parent tags to children)
- Tag hierarchies (categories)
- Tag-based scene generation
- Tag templates for quick element creation

## Screenshots

### Tile View
```
[item]              [clone][del]
Ancient Key
?? Castle Entrance

Aliases:
[key] [old-key] [rusty-key]

Tags:
[puzzle] [required] [metal]
```

### Grid View
```
Name         | Kind | Location         | Aliases          | Tags
------------------------------------------------------------------------
Ancient Key  | item | Castle Entrance  | [key] [old-key]  | [puzzle] [required]
Magic Sword  | item | Treasury         | [sword] +2       | [weapon] [magic] +1
```

## Performance Considerations

- **Search**: O(n) linear search, acceptable for typical game pack sizes (<10,000 elements)
- **Tags**: HashSet ensures O(1) lookups, efficient for gameplay queries
- **View Rendering**: Virtual scrolling could be added for >1000 elements
- **Responsive Grid**: CSS Grid is performant, no JavaScript needed

## Conclusion

This update significantly enhances the Elements page with modern UI patterns, better organization through tags, and improved user experience with dual view modes. The reusable `StringHashSetEditor` component provides a solid foundation for future HashMap editing needs throughout the application.
