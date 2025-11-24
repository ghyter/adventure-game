# Fix: Add Button Closing Edit Dialog

## Problem

The "Add" button in the `StringHashSetEditor` component was closing the GameElementEditor dialog when clicked. This affected both the Aliases and Tags fields.

## Root Cause

The `<button>` element in the `StringHashSetEditor` component did not have an explicit `type` attribute. In HTML, buttons inside a `<form>` element default to `type="submit"`, which triggers form submission.

Since the `GameElementEditor` uses a `<RadzenTemplateForm>`, clicking the Add button was submitting the form, which was interpreted as a "Save" action, closing the dialog.

## Solution

Added `type="button"` to the Add button to prevent it from triggering form submission:

### Before (Broken)
```html
<button class="hashset-add-button" 
        @onclick="AddItem"
        disabled="@string.IsNullOrWhiteSpace(NewItem)">
    Add
</button>
```

### After (Fixed)
```html
<button type="button"
        class="hashset-add-button" 
        @onclick="AddItem"
        disabled="@string.IsNullOrWhiteSpace(NewItem)">
    Add
</button>
```

## Why This Works

**HTML Button Types:**
- `type="submit"` (default): Submits the form
- `type="button"`: Does nothing unless you add event handlers
- `type="reset"`: Resets form fields

By explicitly setting `type="button"`, we tell the browser that this button should NOT submit the form, allowing it to only execute the `@onclick` handler.

## Comparison with HashMapTagList

The older `HashMapTagList` component uses `<RadzenButton>`, which automatically handles this correctly. Radzen buttons don't default to submit behavior, so they don't have this issue.

```razor
<!-- HashMapTagList (no issue) -->
<RadzenButton Text="Add"
              Icon="add"
              Click="AddKey"
              ButtonStyle="ButtonStyle.Light" />
```

## Testing

After the fix:
1. ? Open GameElementEditor
2. ? Add aliases via the Add button ? Dialog stays open
3. ? Add tags via the Add button ? Dialog stays open
4. ? Press Enter in the textbox ? Adds item, dialog stays open
5. ? Click Save button ? Dialog closes (expected behavior)
6. ? Remove tags/aliases ? Works correctly

## File Modified

**AdventureGame.Engine\Components\StringHashSetEditor.razor**
- Added `type="button"` to the Add button (line 29)

## Best Practice Reminder

**When using plain HTML buttons in Blazor forms:**
- Always explicitly set the `type` attribute
- Use `type="button"` for action buttons
- Use `type="submit"` only for form submission buttons
- This prevents unexpected form submission behavior

**Alternative Approaches:**
1. Use `<RadzenButton>` which handles this automatically
2. Use `<input type="button">` instead of `<button>`
3. Place the button outside the `<form>` element (not recommended)

## Why We Use Plain HTML Button

The `StringHashSetEditor` uses a plain HTML button instead of `RadzenButton` because:
- It's a low-level, reusable component
- Doesn't depend on Radzen UI library
- Lighter weight
- Custom styled via CSS
- More control over appearance

The trade-off is we need to be more careful about HTML semantics.

## Related Components

Other components that might need similar attention:
- Any custom input components with action buttons
- Form components with inline actions
- Multi-step wizards with Next/Previous buttons

Always check if action buttons inside forms have explicit `type="button"`.
