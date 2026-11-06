# Tool UI Styling

How the sections of an editor's Tools window are laid out, so new tools look and behave like the existing ones.
The reference implementations are `ViewportGridTool` (`Editors/Viewport/Tools/ViewportGridTool.cs`) and
`TimeActSearchTool` (`Editors/Time Act Editor/Tools/TimeActSearchTool.cs`).

## Registering a tool

1. **Class**: put the tool in the editor's `Tools` folder. It takes the view and project, and has an `OnToolWindow()` method.
2. **View**: create it in the editor view's constructor, next to the other tools (for example `TimeActEditorView`).
3. **Setting**: add a `bool Interface_<Editor>_Tool_<Name> = true;` to `CFG.cs`, in the editor's block.
4. **Tool window**: in `<Editor>ToolWindow.Display`, call the tool between `Sections.Begin()` and `Sections.End()`,
   guarded by the setting, under the `ToolSections.Category(LOC.Get("EDITOR_ToolCategory_<Name>"))` call it belongs
   to. Keep each category's tools together in the code. The shared category keys are in `Editor-Common.json`
   (Actions, Search & Edit, Validation, Browse, Assets, Preview, Animation, Editing, Rows, Data, Organization,
   Game Memory, Simulation, Map Data, Viewport); reuse one before adding a new one.
5. **View menu**: add a toggle to the Tool window's View menu, followed by `GUI.ShowActiveStatus`:

```csharp
// Local Search
if (ImGui.MenuItem($"{LOC.Get("TAE_ToolWindow_View_Toggle_Local_Search")}##localSearchToolToggle"))
{
    CFG.Current.Interface_TimeActEditor_Tool_LocalSearch = !CFG.Current.Interface_TimeActEditor_Tool_LocalSearch;
}
GUI.ShowActiveStatus(CFG.Current.Interface_TimeActEditor_Tool_LocalSearch);
```

## Section frame

A Tools window lists its tools down the left side, grouped by category, and shows the selected tool in the pane
beside it. When the window is narrow, or the "Always Use Dropdown Tool Navigation" option is set, the list becomes
a combo at the top. A window with a single tool shows it without the list.

Each tool starts with `ToolSections.Header(label)`, which registers the tool in the list and returns true only
while it is the selected tool. Its visible label (before `##`) is the name in the list. To open a tool from
elsewhere (as the viewport's Settings menu opens Viewport Rendering), call `ToolSections.Focus()` just before its
`Header`; don't use `ImGui.SetNextItemOpen`. Put the content in a bordered child window:

```csharp
public void OnToolWindow()
{
    if (!ToolSections.Header($"{LOC.Get("TAE_SearchTool_Local_Header")}##timeActSearchToolHeader_local"))
        return;

    ImGui.BeginChild("TimeActSearchToolSection_local", ImGuiChildFlags.Borders);

    Display();

    ImGui.EndChild();
}
```

- The child's name must be unique to the tool, and to each instance if a tool is used twice. Within that child,
  IDs only need to be unique to the section.
- Start with a short `GUI.WrappedText` hint saying what the tool does, if it needs one.
- If the tool can't be used yet (no source selected, wrong game), show a `GUI.WrappedText` saying why and return.
  Don't show disabled inputs instead.

## Groups within a section

Each group of controls, even a single input, gets a titled box made with `GUI.SimpleHeader(title, tooltip)`.
Put a `GUI.Spacer()` before each one to separate the groups:

```csharp
// Square Size
GUI.Spacer();
GUI.SimpleHeader(
    LOC.Get("VIEWPORT_GridConfig_Square_Size_Header"),
    LOC.Get("VIEWPORT_GridConfig_Square_Size_Header_TT"));

ImGui.InputFloat("##SquareSize", ref grid.SectionSize);
GUI.Tooltip(LOC.Get("VIEWPORT_GridConfig_Square_Size_Input_TT"));
```

- Write a one-line comment naming each group above it (`// Square Size`), so the layout can be read from the code.
- For a group whose body is a long list, use `GUI.ConditionalHeader(title, tooltip, ref bool show)` instead. It adds
  an eye toggle that hides the list. Keep the counts, warnings and action buttons outside the toggle, so hiding the
  list never hides what the user needs to act.
- A tool with stages that each need the full height (for example Find, then Edit, in `TimeActMassEditTool`) can
  split them into an `ImGui.BeginTabBar` inside its section child. Work that runs in the background, such as
  picking up results, must run before the tab bar, so it keeps going while another tab is open. A tab that depends
  on another one says what to do there, instead of showing empty.
- Use `GUI.TitleHeader` for the title of a whole editor window, not inside a tool. It is hidden when title
  headers are turned off.

## Inputs

- **Width**: text inputs and combos fill the width: `ImGui.SetNextItemWidth(-1)` before them, with a `##id` label.
  The title comes from the `SimpleHeader` above them.
- **Checkboxes and sliders** under a header can carry their own label: `$"{LOC.Get(key)}##id"`.
- **Tooltips**: every input gets a `GUI.Tooltip` straight after it.
- **Text inputs**: don't use `ImGuiInputTextFlags.EnterReturnsTrue` with the `ref string` overloads. The binding only
  writes the text back on frames where `InputText` returns true, so typed text is lost when the user clicks out of
  the field. Use a plain `InputText`, and act on `ImGui.IsItemDeactivatedAfterEdit()`. Enter also takes focus off
  the field, so it still triggers.

```csharp
ImGui.SetNextItemWidth(-1);
ImGui.InputText(id, ref text, 255);
GUI.Tooltip(LOC.Get($"{nameKey}_TT"));

return ImGui.IsItemDeactivatedAfterEdit();
```

- For a list filter, use `EditorFilters.DisplayFramedListFilter(id, ref filter, ref exact)` and match items with
  `EditorFilters.IsMatch(filter, text, exact)`. This gives the same search bar, exact-match toggle and filter
  syntax as the selection windows.

## Buttons

Action buttons are full width, one per row, each with a tooltip, using `GUI.MultiButtonInput`. Use
`GUI.ConditionalMultiButtonInput` when some of the buttons can be disabled. Both have overloads for one button
and up. The callbacks are `Action`s, so a button that sets a flag can use a lambda:

```csharp
// Actions
GUI.Spacer();
GUI.SimpleHeader(LOC.Get("TAE_SearchTool_Actions_Header"), LOC.Get("TAE_SearchTool_Actions_Header_TT"));

GUI.ConditionalMultiButtonInput("timeActSearchActions",
    "timeActSearchButton", LOC.Get("TAE_SearchTool_Search"), LOC.Get("TAE_SearchTool_Search_TT"),
    () => search = true, hasQuery,

    "timeActSearchClear", LOC.Get("TAE_SearchTool_Clear"), LOC.Get("TAE_SearchTool_Clear_TT"),
    ClearInputs, true);
```

Small inline buttons, such as an icon button on a list row, use `ImGui.Button($"{Icons.LocationArrow}##id{index}")`
with a tooltip.

## Lists

- Draw lists in a bordered child (`ImGuiChildFlags.Borders`). Lists that can be long use `ImGuiListClipper`.
- Cache filtered lists. Rebuild the cache when the filter text, the exact toggle or the source list changes.
  The selection windows check a version counter. A list that grows while a background task fills it can check
  just the new items.
- To scroll a clipped list to an item, store a scroll target and call `clipper.IncludeItemByIndex(index)` after
  `clipper.Begin`. Then call `ImGui.SetScrollHereY()` when that row is drawn, and clear the target. Clear it too if
  the filter hides the item.
- Show an alias under an entry with `GUI.DisplayAlias`.

## Text and colours

- Body text: `GUI.WrappedText`. Coloured text: `GUI.WrappedTextColored(UI.Current.<Color>, text)`. Keep the warning
  colour (bright red) for real problems, not for notes.
- Take colours from `UI.Current` rather than hard-coding them. Scale pixel sizes with `DPI.UIScale()`.

## Modals

- A modal with wrapped text needs a set width. With `ImGuiWindowFlags.AlwaysAutoResize` alone, the window sizes
  itself to its content while `GUI.WrappedText` wraps at the window's width, so the text never wraps and the window
  runs off screen (worst with file paths). Set the width every frame and leave the height at 0, which still fits the
  content:

```csharp
ImGui.SetNextWindowSize(new Vector2(460, 0) * DPI.UIScale(), ImGuiCond.Always);

if (ImGui.BeginPopupModal(title, ref open, ImGuiWindowFlags.AlwaysAutoResize))
```

- A modal that the user may want to resize (e.g. one with a list) drops `AlwaysAutoResize` and sets its size with
  `ImGuiCond.Appearing` instead.
- Open a modal from a context menu by setting a flag, and call `ImGui.OpenPopup` and `BeginPopupModal` together,
  outside the menu, in a place drawn every frame (see `GparamFileList.DisplayDeleteConfirm`,
  `GparamTransferCommands.DisplayPopups`). The `###id` part of the title is the ID, so the visible title can change.

## Long work

Work that takes noticeable time runs on a background task (`Task.Run`), never in the draw loop:

- Read everything the task needs from the editor on the main thread first: list snapshots (`.ToList()`), lazily
  loaded data (such as templates), and the inputs as an immutable record.
- Pass results back through a `ConcurrentQueue`, and empty it while the tool is drawn.
- Track progress in `volatile` fields. Show it with `ImGui.ProgressBar(fraction, new Vector2(-1, 0), text)` under a
  "Progress" `SimpleHeader`, followed by a status line and a Cancel button (`CancellationTokenSource`).
- Starting a new run, or clearing, cancels the run in progress.

## Localization

- Every string the user sees goes through `LOC.Get(key)`, with arguments as `{0}`, `{1}`.
- Add the entries to `src/Smithbox.Data/Assets/Localization/English/<Editor>.json`. The copy under
  `Smithbox.Release/Output` is build output.
- Key names: `<EDITOR>_<Tool>_<Thing>`, with `_TT` for its tooltip and `_Header` for a group title. For example
  `TAE_SearchTool_Results_Header` and `TAE_SearchTool_Results_Header_TT`. The View menu toggle is
  `<EDITOR>_ToolWindow_View_Toggle_<Tool>`.
- Check the JSON still parses after editing it, for example with PowerShell's `ConvertFrom-Json`.

## IDs

- Every widget label carries a `##id` suffix that is unique within its window, even when the visible text is unique.
  Titles get localized, so they can't be relied on as IDs.
- For a tool used more than once, such as Local and Global Search, add an instance suffix to every ID
  (`##timeActSearchValue_{Id}`).
- `GUI.SimpleHeader` uses a hash of its title as the table ID, so identical titles need to be in different child
  windows.
