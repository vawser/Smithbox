# Map Editor Action Tabs Assessment

Assessment of the four sections under the **Actions** category of the Map Editor Tools window
(`MapToolWindow.Display`, `src/Smithbox.Program/Editors/Map Editor/GUI/MapToolWindow.cs:131`), and which of their
tabs could be lifted out into a category of their own.

## Current layout

The Tools window lists one entry per `ToolSections.Header`, grouped by `ToolSections.Category`. The Actions category
has only four list entries, but each one opens a tab bar, so 18 tools are reachable only through a second level of
navigation.

| Section | Class | Tab | What it changes | Target |
|---|---|---|---|---|
| Common Actions | `CommonActionTool` | Create | Adds a new MSB entry | Target map (no selection) |
| | | Translate | Position | Selection |
| | | Rotate | Rotation (also has Reset Rotation shortcut) | Selection |
| | | Duplicate | Adds copies of MSB entries | Selection |
| | | Duplicate to Map | Adds copies in another map / BTL | Selection |
| | | Pull to Camera | Position | Selection |
| | | Replicate | Adds copies in a line / circle / square / scatter | Selection |
| | | Scramble | Random position / rotation / scale | Selection |
| Property Actions | `PropertiesActionTool` | Transform | Reset position / rotation / scale | Selection |
| | | Display | Clear display groups, draw groups, collision masks | Selection |
| | | Logic | Clear Entity ID, Entity Group IDs | Selection |
| Selection | `SelectActionTool` | Mass Selection | The selection (by filter, type, model, ceremony) | All loaded |
| | | Collision Selection | The selection (part ↔ collision references) | Selection |
| | | Box Selection | Box-select settings only (`CFG`) | None |
| Visibility | `VisibilityActionTool` | Viewport Filters | Render filters per object kind (editor only) | Render scene |
| | | Editor Visibility | Per-object hide/show, global, tag, focus (editor only) | Selection / all |
| | | Game Visibility | Dummy type / Game Presence — **writes MSB data** | Selection |
| | | Low Detail | Hide/show/select low-detail models (editor only) | All loaded |

Sorting the tabs by *what they change* rather than by section gives four kinds of work:

1. **Make new entries** — Create, Duplicate, Duplicate to Map, Replicate
2. **Move entries** — Translate, Rotate, Pull to Camera, Scramble, Property Actions › Transform
3. **Set properties** — Property Actions › Display, Property Actions › Logic, Game Visibility
4. **Editor-only view state** — Viewport Filters, Editor Visibility, Low Detail (plus Selection, which changes the
   selection but no data)

The section boundaries cut across these: transform work is split between Common Actions and Property Actions, and
Game Visibility sits under Visibility although it is an undoable edit of the MSB that changes the saved map.

## Candidates for a separate category

### 1. Transform — strongest candidate

Translate, Rotate, Pull to Camera, Scramble and the Transform tab of Property Actions.

- All five write only `Position` / `Rotation` / `Scale` of the selected entities through the viewport action manager.
- Two of them already overlap: Rotate has a *Reset Rotation* shortcut (`KeybindID.MapEditor_Reset_Rotation`), and
  Property Actions › Transform has a *Reset Rotation* button. The three reset buttons belong next to the tools that
  set those values.
- Translate and Rotate share the same design (five configurable increments, a cycle shortcut, a "display increment
  in viewport" option), which reads more clearly as siblings in the list than as tabs 2 and 3 of eight.
- Moving these out leaves Common Actions as purely *creation* tools, which is a coherent group by itself.

Proposed entries: **Translate**, **Rotate**, **Pull to Camera**, **Scramble**, **Reset Transform**.

### 2. Create — strong candidate (falls out of 1)

Create, Duplicate, Duplicate to Map, Replicate.

- All add MSB entries; three of them copy the selection.
- Duplicate and Replicate repeat the same option block — Increment Entity ID, Increment Instance ID, Increment Part
  Names, Clear Entity ID, Clear Entity Group IDs — on separate `CFG` keys (`Toolbar_Duplicate_*` vs
  `Replicator_*`). Side by side in one category, the repetition is visible and could later share one helper.
- Replicate already depends on Scramble ("Apply Scramble Configuration"), so its tab should say where to set that.

### 3. Properties — moderate candidate

Property Actions › Display, Property Actions › Logic, and Visibility › Game Visibility.

- All are one-click, undoable property writes to the selection.
- Game Visibility is the misfit in the current layout: Dummy conversion rewrites the entity type (with property data
  loss) and Game Presence edits saved data. Users looking for "hide in editor" under Visibility can mistake it for
  an editor-only toggle. It belongs with the other property edits.
- With Transform moved out, this would be the remaining contents of Property Actions, so it could stay one
  section with three tabs (Display, Logic, Game Visibility) rather than becoming its own category.

### 4. Editor visibility → existing Viewport category — moderate candidate

Viewport Filters, Editor Visibility, Low Detail.

- None of them change map data; they change what the viewport draws.
- The **Viewport** category already holds the same kind of tool: Render Groups, Detection Visualization, Grid,
  Viewport Rendering. Viewport Filters in particular is the per-type counterpart of Render Groups.
- Per the Tool UI Styling guide, an existing category should be reused before adding a new one, so these fit
  better there than in a new "Visibility" category.

### Selection — keep as is

Mass, Collision and Box Selection only change the selection (or its settings) and form a clean group already.
Low Detail's *Select detected* button is a selection action, but it depends on that tab's scan, so it should stay
with Low Detail.

## Proposed structure

```
Create            Create · Duplicate · Duplicate to Map · Replicate
Transform         Translate · Rotate · Pull to Camera · Scramble · Reset Transform
Actions           Property Actions (Display · Logic · Game Visibility) · Selection (Mass · Collision · Box)
Search & Edit     (unchanged)
...
Viewport          Viewport Filters · Editor Visibility · Low Detail · Render Groups · Detection Visualization · Grid · Rendering
```

Each tab in Create and Transform becomes its own `ToolSections.Header`, so it gets a direct list entry, can be
hidden from the View menu, and can be focused from elsewhere with `ToolSections.Focus()`.

## Implementation (2026-11-06)

Implemented as proposed:

- `MapToolWindow.Display` registers each Create / Transform tool as its own header through `ActionSection`, which
  wraps the tool's existing `OnToolWindow()` in a bordered child. The View menu uses `ToolToggle` for each one.
- New categories `EDITOR_ToolCategory_Create` / `_Transform` (`Editor-Common.json`); header and toggle labels in
  `Map-Editor-Tool-Window.json`.
- New settings `Interface_MapEditor_Tool_{Create, Duplicate, DuplicateToMap, Replicate, Translate, Rotate,
  PullToCamera, Scramble, ResetTransform, ViewportFilters, EditorVisibility, LowDetail}`; `_Common_Action` and
  `_Visbility_Action` removed.
- `PropertiesActionTool`: the Transform tab became `DisplayResetTransform()`; Game Visibility joined as a tab.
- `CommonActionTool.cs` and `VisibilityActionTool.cs` moved to `.archive`.

The undo bug in the Clear Display / Draw Groups / Collision Masks buttons is still open.

## Validation section

One header (`Interface_MapEditor_Tool_Validation`) with a tab bar, in `MapToolWindow.Display`.

| Tab | Class | What it does | Changes data |
|---|---|---|---|
| Entity ID | `EntityIdentifierTool` | Lists the Entity IDs of the selected map, with a filter, a hide-unassigned toggle and block separators; click to select | No |
| MSB Validation | `MapValidatorTool` | Checks the selected map for duplicate Entity IDs and broken references (collision, walk route, part names, NpcThink / Npc / CharaInit / Chameleon params); results in sub-tabs, click to select | No |
| Irradiance Volumes | `IrradianceVolumeIdTool` | Per loaded map, how the EnvMapEffectBoxes link to baked volumes; **Fill** writes volume IDs from box names | Yes (undoable) |

Related, outside the Tools window: **Tools › Miscellaneous › Entity ID Check** (`EntityIdCheckAction`) checks for duplicate
Entity IDs, which MSB Validation already does, plus duplicate Entity Group IDs within one part (ER / AC6 only). It
writes its results to the log rather than a list.

Assessment:

- The three tabs are unrelated tools that share a frame. None depends on another, so the tab bar only hides them.
- **Entity ID** is a browser for picking free IDs, not a check. It sits here because Entity IDs are what
  MSB Validation most often reports.
- **Irradiance Volumes** is a check *and* a fix. It is the only one that writes data, and the Viewport Rendering
  Global Illumination tab links to it (`IrradianceVolumeIdTool.FocusRequested`). That link has to force the tab with
  `ImGuiTabItemFlags.SetSelected` because the tool is a tab rather than a header.
- Entity IDs are checked in three places (Entity ID tab, MSB Validation's Entity ID results, the Entity ID Check
  menu). The menu item's one extra check, duplicate group IDs, would fit as another MSB Validation result type.

**Recommendation:** keep the Validation category, but make each tab its own header: **MSB Validation**,
**Entity IDs**, **Irradiance Volume IDs**. The GI link then becomes a plain `ToolSections.Focus()` before its
header, and the `SetSelected` workaround goes away. Moving the group-ID check into MSB Validation and retiring the
menu item is a follow-up.

This doesn't call for a new category. Three headers are a small enough group, and "validate the map" describes all
three.

## Viewport Rendering section

`ViewportRenderingTool` (`Editors/Viewport/Tools/ViewportRenderingTool.cs`) is **shared by the Map, Model, Time Act
and Gparam editors**. It is one header with up to seven tabs, and some tabs appear only in some editors:

| Tab | Shown in | What it sets |
|---|---|---|
| Map Lighting | Map; Model / Time Act (borrowed map); Gparam (selected GPARAM) | GPARAM lighting on/off, exposure scale, source file, weather, time of day, debug view, resolved values |
| Scene Lighting | All | Fallback sun (disabled while map lighting is active), light multipliers, camera light |
| Global Illumination | Map; Model / Time Act; Gparam | Irradiance volume enable, intensity, auto exposure, bakes and blending, link status |
| BTL Lights | Map, Gparam | Which BTL lights are gathered, max count, intensity (from the map's exposure when map lighting is on) |
| Shadows | All | Cascaded sun shadows: on/off, resolution, strength, bias, MSB shadow flags |
| Post Effects | All | Tone map, then the GPARAM post effects (image, AA, atmosphere, surfaces) and resolved values |
| Materials | All | Shader feature toggles, shader types, wind, plus grass density and collision opacity |

The tab bar uses `FittingPolicyScroll | ListPopupButton` because a narrow tool window can't fit the tabs. That is the
same problem the Actions sections had.

Grouping:

1. **Lighting**: Map Lighting, Scene Lighting, Global Illumination, BTL Lights, Shadows. They depend on each other.
   Map lighting turns off the Scene Lighting sun, sets the GI and BTL exposure, and drives the bake selection through
   time of day, and the GPARAM's Shadows group drives the cascades. A user changing one usually needs another.
2. **Image**: Post Effects with the tone map. It is the end of the chain and stands alone.
3. **Materials**: shader feature toggles. Its *Display* group (grass density, collision opacity) is a misfit.
   Grass density rebuilds geometry, and collision opacity is a display setting. In the Map Editor both fit beside
   Viewport Filters, but the Model, Time Act and Gparam editors have no Viewport Filters, so they stay here.

**Recommendation: a new shared Rendering category** (`EDITOR_ToolCategory_Rendering`), with one header per tab:
Map Lighting, Scene Lighting, Global Illumination, BTL Lights, Shadows, Post Effects, Materials. Seven headers is a
category of its own; inside the Viewport category they would outnumber the other tools.

Because the tool is shared, the change belongs in `ViewportRenderingTool`, not in each tool window:

- `OnToolWindow()` emits `ToolSections.Category(Rendering)` and then one header per available tab, using the same
  per-editor conditions `DisplayTabs` uses now. Each editor's tool window calls it last, after its Viewport category,
  so the four editors get the change at once.
- The setup that is shared between tabs (`mapView`, `gparamView`, `environment`, `postBank`, `postMaps`) is worked
  out once at the top of `OnToolWindow()`, as now.
- `FocusRequested` (set by the viewport's Settings menu in all four editors) focuses the first header, Map Lighting
  where there is one and Scene Lighting otherwise.
- The "no viewport" message has to be shown once, under a single header, since there are no tabs to show without a
  viewport.

Toggles: decided to keep one View-menu toggle per editor (the existing `Interface_*_Tool_ViewportRendering`
keys), which shows or hides the whole Rendering category.

## Implementation, Validation and Rendering (2026-11-06)

- **Validation**: `MapToolWindow.Display` draws MSB Validation, Entity IDs (not DS2) and Irradiance Volume IDs
  (when the bank is usable) as separate headers through `ActionSection`. `Interface_MapEditor_Tool_Validation` is
  replaced by `_MsbValidation`, `_EntityIds` and `_IrradianceVolumeIds`, each with a View-menu toggle (there was no
  toggle before). The GI tab's "open tool" button sets `_IrradianceVolumeIds`, and `FocusRequested` now only calls
  `ToolSections.Focus()`; the `SetSelected` tab flag is gone.
- **Rendering**: `ViewportRenderingTool.OnToolWindow()` sets `EDITOR_ToolCategory_Rendering` and draws each
  former tab as a header through `Section(ref focus, ...)`. The first header drawn takes `FocusRequested`. Without a
  viewport there is one header with the "no viewport" message.
- `ToolSections` groups entries by the category current when each is drawn, so the Rendering call moved to the end
  of the Viewport block in all four tool windows (after the Resource List in Map / Model / Time Act, after Grid in
  Gparam), or the tools after it would be listed under Rendering.
- Not done: moving the Entity ID Check menu's group-ID check into MSB Validation.

## Side findings

- The View menu has no toggles for **Selection** (`Interface_MapEditor_Tool_Select_Action`) or **Visibility**
  (`Interface_MapEditor_Tool_Visbility_Action`); once hidden from config they can't be turned back on from the UI.
  The latter key is also misspelled (`Visbility`).
- The View menu has toggles for **Viewport**, **Data Transfer**, **Selection Groups** and **Viewport Configuration**
  (`Interface_MapEditor_Tool_Viewport`, `_Data_Transfer`, `_SelectionGroups`, `_ViewportConfig`) that no section in
  `Display()` reads, so they do nothing.
- `PropertiesActionTool.ClearDisplayGroups` / `ClearDrawGroups` / `ClearCollisionMasks` zero the live array and then
  copy it onto itself (`new PropArrayCopyAction(arr, sel.Arr)` with `arr == sel.Arr`), so undo has nothing to restore.
  `ClearEntityGroupIDs` clones first and is correct. They also execute one action per entity rather than one
  compound action, so undo steps through entities one at a time.
