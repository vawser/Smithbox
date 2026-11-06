# MSB Search and Mass Edit Plan

Replaces the Map Editor's Search (Local / Global tabs) and Mass Edit tools with versions modelled on the Time Act
Editor's (`Editors/Time Act Editor/Tools`, see `TAE Mass Edit Plan.md`) and the Gparam Editor's port of them
(`Gparam Mass Edit Plan.md`).

## What is replaced

| Old | Where | New |
|---|---|---|
| `LocalSearchTool` | `Tools/Search` | `MsbSearchTool` (Local mode) |
| `GlobalSearchTool`, `MapQueryBank`, `IMapQueryEngine`, `MapObjectMatch` | `Tools/Search` | `MsbSearchTool` (Global mode) + `Tools/Query` |
| `MassEditTool`, `MassEditPropertyHelper`, `MassEditUtils`, `MsbMassEditLog` | `Tools/Mass Edit` | `MsbMassEditTool` (+ Filter, Operation, Planner, Applier) |
| `MassEditGroupAction`, `MapActionGroup` | `Actions`, `Tools/Search` | `ViewportCompoundAction` of `PropChangeAction`s, `EntDeleteAction` |

The old files are moved to `.archive/Map Editor Search and Mass Edit`. `QuerySelectionConditionLogic` stays, as
`SelectAllAction` uses it.

Old behaviour that changes:

- The old Global Search loaded every MSB into memory up front ("Load Map Data") and kept them. The new one reads the
  MSBs on a background task when Search is pressed, and keeps only the matches.
- Regex filters are replaced by the shared filter syntax (`EditorFilters.IsMatch`: `|` for or, `+` for and), as
  the selection windows use.
- The typed command rows of the old Mass Edit (`prop:EntityID > 1000`, `Position + 0,3,0`) are replaced by pickers:
  condition rows and step rows.
- The old "random spread" (variance) becomes the *Add Random* and *Set Random* operators.
- The copy-to-clipboard column options are replaced by one Copy Results button that copies the listed labels.

## Shape

### Query (`Tools/Query`)

- `MsbQuery.cs`: `MsbQueryComparison`, `MsbQueryCategory` (All / Parts / Regions / Events), `MsbQueryLevel`
  (Entry / Field), `IMsbQueryFilter`, `MsbQuery` (the search inputs), `MsbQuerySource`, `MsbQueryEntry`,
  `MsbQueryField`, `MsbQueryMatch`.
- `MsbQueryMatcher.cs`: walks an MSB entry's properties into fields, as the Properties window does (nested classes,
  arrays and lists of values or classes, shapes). A field has a path (`Unk1.CharaInitID`, `EntityGroupIDs[3]`),
  its name, its community name from the MSB meta, its value as text and the enum name. Matches text through
  `EditorFilters.IsMatch`, compares values as Gparam does (numbers as numbers, vectors component-wise).
- `MsbQueryRunner.cs`: gathers the sources on the main thread (loaded maps as their entities, unloaded ones as a
  file to read), runs the filter on a background task, and jumps to a match through `map/select/{map}/{name}/{type}`,
  which loads the map first if needed.

A loaded map is queried as it is in the editor; an unloaded one is read from the project's `TargetFS`.

### Search (`Tools/Search/MsbSearchTool.cs`)

Local and Global modes, as TAE. Inputs: Scope (Local: Selected Map / Loaded Maps), Map (Global only), Category,
Type, Name, Field, Value (comparison combo + value, Between takes two), Options (Exact, Ignore Dummies). A query
with a field or value lists each matching field (`m10_00_00_00  c1000_9000  EntityID = 10001000`); without one it
lists the entries. Results: count, Send to Mass Edit, Copy Results, a filterable clipped list with a jump button.

### Mass Edit (`Tools/Mass Edit`)

Two tabs, as TAE:

- **Find**: Scope (Selected Objects / Selected Map / Loaded Maps / All Maps), Map (All Maps only), Category, Type,
  Name, Conditions (rows of Field / Comparison / Value, all must match), Options, Find, then the matches.
- **Edit**: Action (Edit / Delete).
  - *Edit*: steps `Set [field] [operator] [value]`. The fields are the ones found on the matches, with
    "(in N objects)" on those only some have. Operators by kind:
    numbers and vectors: Set, Add, Subtract, Multiply By, Divide By, At Least, At Most, Set Random, Add Random;
    bools: Set, Toggle; enums: Set; text: Set, Prefix, Suffix, Replace.
    A vector operand of one number applies to every component; one per component applies component by component.
    Whole numbers are rounded; a result outside the type's range skips the object with a reason. An object is
    changed by every step or none. Random values are drawn when the plan is made, so the preview shows the values
    that are applied. `Name` can't be edited (other entries refer to objects by name; the Properties window hides it
    too).
  - *Delete*: removes the matched objects, after a confirm.
  - Apply is enabled only while the matches aren't stale; after it the plan is dropped until the next Find.

## Applying (decided 2026-11-06: write unloaded maps directly)

- **Loaded maps** (owned by this view): one `ViewportCompoundAction` of `PropChangeAction`s, one per changed object
  (the entry or a nested class of it), so the property view's refreshes (model, grass, name cache, dirty flag) run
  as for a single edit; deletes go through one `EntDeleteAction`. One undo takes the whole edit back.
- **Unloaded maps**: the MSB is read again at apply, each match is found again by category, index and name, and
  checked against the value the plan was made from (a changed one is counted as stale and left alone). The edit is
  made, then the MSB is written straight to the project, the way `MapUniverse.SaveMSB` does
  (`MsbGameAdapter.CreateForWrite`, parts / regions / events / part poses added back, models rebuilt with
  `AddModels`). This can't be undone, so Apply asks first when any unloaded map is touched, naming the count.
- **Maps loaded in another view** are skipped with a reason.
- A map whose write fails (for example a delete that leaves a reference to a removed object) is reported and left
  as it was.

## Hooks

- Properties window context menu: *Search* fills the Local Search with the field and its current value, *Add to
  Property Filter* fills the Global Search's field.
- Map list / content tree context menu: *Add to Map Filter* adds the map to the Global Search's map input (with `|`).
- `map/propsearch/{field}/{value}` (Soapstone) fills the Local Search with an equals query and runs it.

## Settings and localization

- CFG: `Interface_MapEditor_Tool_LocalSearch`, `Interface_MapEditor_Tool_GlobalSearch`,
  `Interface_MapEditor_Tool_MassEdit` replace `Interface_MapEditor_Tool_Search` and
  `Interface_MapEditor_Tool_PropertyMassEdit`. The `GlobalMapSearch_CopyResults_*` settings are removed.
- LOC: `Map-Editor-Tools-Search.json` (`MAP_SearchTool_*`) and `Map-Editor-Tools-Mass_Edit.json`
  (`MAP_MassEditTool_*`).

## Status

Implemented 2026-11-06, awaiting check in the app. Notes from the implementation:

- **Files.** `Tools/Query/MsbQuery.cs`, `MsbQueryMatcher.cs`, `MsbQueryRunner.cs`, `MsbFieldPicker.cs`;
  `Tools/Search/MsbSearchTool.cs`; `Tools/Mass Edit/MsbMassEditTool.cs` (Find), `MsbMassEditTool.Edit.cs` (Edit,
  confirms), `MsbMassEditOperation.cs`, `MsbMassEditFilter.cs`, `MsbMassEditPlanner.cs`, `MsbMassEditApplier.cs`
  (with `MsbDirectWriter`). LOC: `Map-Editor-Tools-Search.json`, `Map-Editor-Tools-Mass_Edit.json`.
- **Field picker.** The Field input of the search and of each condition has a button listing the fields of the
  selected map object, with their values, to pick from.
- **Kept fields.** The mass edit's find keeps each object's fields as read (`MsbQueryMatch.Fields`), so the step
  field list and the plan don't walk every object again on each keystroke.
- **Selected Objects** goes stale when the selection changes, as TAE's Selected Events does, so jumping to a match
  from that scope needs a Find before Apply.
- **`SelectAllAction`** used the old Mass Edit's `PropertyNameFilter` / `PropertyValueFilter`; it now has its own
  copies (same `prop:` syntax, `<` / `>` compare as numbers).
- **csproj.** `Enums/QueryMapListType.cs` was listed explicitly in `Smithbox.Program.csproj`; its entry is removed.
- **Not done:** an edit of `EntityID` doesn't refresh the Entity ID tool's cache (the Properties window does, per
  object); the cache is rebuilt when that tool next scans.

## Verification (for the user)

- **Local Search**: ER `m10_00_00_00` loaded, Field `EntityID`, `>= 10001000` → results list entity IDs; clicking one
  selects and frames it.
- **Global Search**: ER, Type `Enemy`, Field `ModelName`, Value `c4290` → matches across many maps; jumping to an
  unloaded one loads it and selects the enemy.
- **Mass Edit, loaded**: `m10_00_00_00`, Selected Map, Category Parts, Type `Asset`, Edit `Position` Add `0, 1, 0`
  → preview shows old → new; Apply moves them; one Ctrl+Z restores them.
- **Mass Edit, unloaded**: All Maps, Map `m60_42_36_00`, Type `Enemy`, Edit `NPCParamID` Add 1 → confirm names one
  map; after Apply, load the map and check the values, and that the map still loads (models intact).
- **Skips**: set a `byte` field to 300 → every object is skipped as out of range.
- **Delete**: Selected Objects, delete three regions → gone; Ctrl+Z restores them in place.
