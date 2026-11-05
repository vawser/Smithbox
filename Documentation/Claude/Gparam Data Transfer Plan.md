# Gparam Data Transfer – Analysis and Redesign Plan

Covers `GparamDataTransferTool` (`Editors/Gparam Editor/Tools/GparamDataTransferTool.cs`), its JSON format
(`GparamJSON.cs`), the import actions (`Actions/Import*Action.cs`, `InsertFieldValueAction.cs`) and the four list
context menus that call into it.

## 1. What exists today

The same eight operations (import/export × GPARAM/group/field/value) are reachable from **three** places, each
with its own copy of the logic and its own state:

| Entry point | Where | Overwrite vs append (values) | Export folder | File name |
|---|---|---|---|---|
| Tool section | Tools window › Data Transfer (Import / Export tabs) | `ImportType` combo — field on the tool, **not saved** | Text field + "Set" button, remembered for the session | Auto-generated |
| Menubar | Tools › Data Transfer › Import / Export | `CFG.GparamEditor_Data_Import_Overwrite` checkbox at the bottom of the Import menu | **Cleared before every export**, always prompts | Auto-generated |
| Context menus | File / Group / Field / Value lists, right click | Same CFG flag, but **not shown** in the menu | Whatever the tool currently remembers – may write silently, may prompt | Per-list "override name" text box, remembered |

Export writes one JSON file per object. Import opens a file dialog, parses it as the kind the button promised,
and runs an undoable action.

## 2. Problems

### 2.1 Inconsistent behaviour between entry points
- **Two settings for one choice.** The tool section's Overwrite/Append combo and the CFG checkbox are separate.
  Choosing "Overwrite" in the tool section has no effect on the context menu, and the context menu never says which
  mode it will use (default is *append*).
- **Shared, hidden export folder.** The menubar wipes `ExportDirectory`; the tool section keeps it. After one export
  from the tool section, a context-menu export writes straight into that folder without asking. After a menubar
  export it asks again. The user can't predict which.
- **Sticky override name.** The context-menu override name persists. Type `fog_test` once, and every later export
  from that list writes `fog_test.json`, overwriting the previous one without warning.
- **Logging differs.** Tool-section imports log success; menubar and context-menu imports discard the return value
  and log nothing.
- **Vocabulary differs.** "Param" (menubar) vs "Group" (tool section, lists); "Field Value" vs "Value".

### 2.2 The import semantics aren't what the UI says
- **Group and field import ignore the selection.** `ImportParamAction` replaces the group whose key matches the
  *JSON's* key (or appends it); the selected group is passed in but unused. The tooltip says "Replace the selected
  Param". Same for fields: the target is the selected *group*, the field is matched by key from the JSON.
  Right-clicking group A and importing group B's JSON changes B, not A.
- **No type check across kinds.** Only the value JSON carries a discriminator. A field JSON picked in "Import
  Group" deserialises as a `GPARAM.Param` (shared `Key`/`Name` properties, unknown ones ignored) and is added as a
  group with no fields. Group JSON into "Import GPARAM" behaves similarly. *(Inferred from the serializer defaults –
  worth confirming with a quick test.)*
- **No confirmation for whole-file import.** Import GPARAM replaces every group in the file. It is undoable, but
  there's no preview or prompt.
- **No filter on the open dialog.** Any file can be picked; a non-`.json` file is silently ignored.

### 2.3 The tool section is weak as a UI
- Four buttons always enabled, then a log error if nothing is selected (the styling guide asks for
  `ConditionalMultiButtonInput` or an explanatory message).
- The "Output" box only shows the JSON of the last *file* export. You can't copy to the clipboard without writing a
  file first.
- Hard-coded strings (no `LOC`), a "CSV" header for JSON data, a "clibpaord" typo, and a non-standard section
  frame (header drawn in `GparamToolView`, not an `OnToolWindow()`).
- It duplicates the context menus without adding anything — except the Overwrite/Append choice, which the context
  menus should have had anyway.

### 2.4 Missing the common workflow
The usual task is "take *this* group / these values from map A's GPARAM and put them in map B's" — inside one
session. Today that's: export → pick folder → switch file → select target → import → find the file again. There's
no clipboard path, although the editor already has `KeybindID.Copy` / `KeybindID.Paste`, and the value list already
has multi-selection that export ignores (only the clicked value is exported).

## 3. Proposed design

**One transfer model, two transports.** Every transfer is a *payload* (a kind + objects) that moves through either
the clipboard (fast, in-app) or a file (for sharing / keeping). Both use the same JSON, so a file can be opened in a
text editor, copied, and pasted back in, and vice versa.

### 3.1 Payload envelope
Wrap every export in one envelope (new `GparamTransferPayload` in `GparamJSON.cs`):

```json
{
  "Format": "Smithbox.Gparam",
  "Version": 1,
  "Kind": "Group",            // Gparam | Group | Field | Values
  "Game": "ER",
  "Source": { "File": "m60_42_36_00", "Group": "LightSet ...", "Field": "..." },
  "FieldType": "float",       // Values only
  "Items": [ ... ]            // one GPARAM, or 1+ groups / fields / values
}
```

- Import reads `Kind` first, so there's **one** Import command that routes the payload to the right target and
  gives a clear error for a mismatch ("This is a Field; select a group to paste it into").
- `Items` is a list, so multi-selection (values now; groups/fields if they get multi-select later) travels in one
  payload.
- **Back-compat:** if `Format` is missing, fall back to today's bare-object parsing, keyed by which import was
  requested (old files keep working through the per-kind path).

### 3.2 Entry points

| Where | Commands |
|---|---|
| **Edit menu** (main menubar) | Copy *(Ctrl+C)*, Paste *(Ctrl+V)*, Paste Special… — act on the focused list's selection |
| **Context menus** (each list) | Copy, Paste, Paste Special…, separator, Export to File…, Import from File… |
| **File menu** | Import JSON…, Export Selected GPARAM as JSON… |
| Tools › Data Transfer menubar dropdown | **Removed** (duplicate of the above) |
| Tools window › Data Transfer section | Replaced by a **Transfer** staging panel (3.4) |

Copy/Paste follow the focused list, like Delete and Duplicate already do in `GparamEditorScreen.EditMenu`.

### 3.3 Paste semantics (explicit and shown)
Each target gets a short, named set of modes; the default is shown in the menu item, and **Paste Special…** opens a
small popup to pick another:

| Payload | Target | Modes |
|---|---|---|
| Gparam | File | Replace all groups *(confirm)* · Merge (replace matching groups, add missing) |
| Group | GPARAM | Replace matching key, else add *(default)* · Add as new only · Replace **selected** group |
| Field | Group | Replace matching key, else add *(default)* · Replace **selected** field |
| Values | Field | Insert after selection *(default)* · Replace selection · Replace by ID, add missing |

- One persisted CFG setting per payload kind replaces both `ImportType` and `GparamEditor_Data_Import_Overwrite`.
- "Replace selected" is the mode the current tooltips *promise*; offering it explicitly fixes 2.2 without changing
  what the default does.
- Values: type check (`AcceptsValue`) and ID collision handling (`NextFreeValueId`) stay as in
  `InsertFieldValueAction`, extended to a list. Cross-game paste logs a warning but is allowed.
- Every paste is one undoable action (a `CompoundAction` for multi-item payloads) and logs one line:
  "Pasted 3 values into *FieldKey* (inserted after ID 2)".

### 3.4 Transfer section in the Tools window
It becomes a **staging panel** instead of a set of blind buttons:

- **Payload** header: what's on the clipboard / last opened file – kind, source file, item count, game. "Load from
  File…", "Load from Clipboard".
- **Preview**: a short list of the items (keys/IDs), not the raw JSON.
- **Target**: what the current selection would receive it ("→ Group *LightSet* in m60_42_36_00"), or a
  `WrappedText` saying what to select.
- **Mode**: the paste-mode combo for that kind (the same CFG setting as 3.3).
- **Actions**: Apply · Save Payload to File… · Copy Payload JSON (via `ConditionalMultiButtonInput`).

This is where a user goes when they want to see what will happen first, or to apply one file to several targets in
a row. Everything else lives in the context menus.

### 3.5 Exporting to files
- Replace the folder picker + override-name box with a **Save File** dialog (`PlatformUtils.SaveFileDialog`, JSON
  filter). The user names the file in the dialog, so there's no sticky name and no silent overwrite (the OS dialog
  asks). The NativeFileDialogSharp wrapper can't pre-fill a file name, only a folder, so the name starts empty
  (decision 2). If the chosen path has no `.json` extension, add it.
- Remember the last folder in CFG (`GparamEditor_Transfer_LastFolder`) and open the dialog there.
- Open dialog uses the `json` filter.

## 4. Implementation phases

1. **Payload + core service** – `GparamTransferPayload`, envelope read/write with legacy fallback, and a
   `GparamTransferService` (no UI) holding `BuildPayload(selection)`, `Validate(payload, target)` and
   `Apply(payload, target, mode)` → action. Moves the logic out of the tool class so every entry point calls the
   same code.
   **Status: done (2026-11-05).**
   - `Tools/Transfer/GparamTransferPayload.cs` – envelope (`Format`, `Version`, `Kind`, `Game`, `Source`,
     `FieldType`, `Items`). Items are kept as raw JSON and turned into fresh objects on every `Get…` call, so one
     payload can be applied many times. `Parse` reads the envelope, or a legacy export told apart by its properties
     (`FieldType`+`Value` → values, `$type` → field, `Params` → GPARAM, `Fields` → group).
   - `Tools/Transfer/GparamTransferService.cs` – `GparamTransferTarget.FromSelection`, `BuildPayload`, `Check`
     (error / warning / one-line description, changes nothing), `Apply` (one undo step, `CompoundAction` for
     several items, logs the outcome), `ReadFile` / `WriteFile` / `SuggestFileName`. Values: insert-after keeps
     the payload order by chaining each insert after the previous one; replace needs as many selected values as
     the payload holds.
   - `GparamDataTransferTool` – every import/export now goes through the service. The per-kind commands refuse a
     file of another kind. Value import/export acts on the whole multi-selection when the clicked value is in it.
     The old public methods use the items they are passed, not the selection. Exports now write the envelope.
   - Strings in `Localization/English/Gparam-Editor-Tools-Transfer.json`.
   - Not yet changed: the UI (two overwrite settings, folder export, override-name boxes) – phases 3–5.
2. **Actions** – extend the existing import actions with the new modes ("replace selected", merge), and a
   multi-value version of `InsertFieldValueAction`. Keep Param-extra rebuilding as today.

   **Status: done (2026-11-05).**
   - `Tools/Transfer/GparamPasteModes.cs` – `GparamFilePasteMode` (Replace All, Merge Groups),
     `GparamKeyedPasteMode` for groups and fields (Match Key, Add Missing Only, Replace Selected),
     `GparamValuePasteMode` (Insert After Selection, Replace Selection, Match ID), and the `GparamPasteModes`
     record passed to `Check`/`Apply`. One CFG setting per kind (`GparamEditor_Transfer_*_Mode`), read by
     `GparamPasteModes.FromConfig()`; nothing sets them yet (phase 3 UI).
   - Fields also got Add Missing Only, so groups and fields share one mode set.
   - `ImportParamAction` / `ImportFieldAction`: optional `replaceTarget`, replaced in place whatever its key; does
     nothing if that entry is gone. Replace Selected refuses a payload of more than one entry, or one whose key
     another entry already uses.
   - `InsertFieldValueAction`: optional `insertBefore`. Match ID replaces the value with the same ID, or inserts
     before the first higher ID (else at the end), in ID order; refuses payloads with repeated IDs.
   - Merge Groups runs one `ImportParamAction` per group; the file's header data (Unk0D, Data30…) is kept.
   - No separate multi-value action: a `CompoundAction` of `InsertFieldValueAction`s is one undo step.
   - The old Data Transfer commands pass their old modes explicitly, so they behave as in phase 1.
   - Replace All still has no confirmation – that belongs to the paste UI (phase 3).
3. **Clipboard** – Copy / Paste / Paste Special in context menus and the Edit menu, keybinds via `Shortcuts`.

   **Status: done (2026-11-05).**
   - `Tools/Transfer/GparamTransferCommands.cs` (owned by `GparamDataTransferTool.Commands`): Copy (payload JSON to
     the system clipboard), Paste (default modes from CFG), Paste Special window, Replace All confirmation, and
     the menu-item and keybind helpers the lists call.
   - **Paste goes by what the clipboard holds, not by the list it's done from**: a GPARAM or groups into the
     selected file, fields into the selected group, values into the selected field. (Replaces the earlier idea of
     refusing a field paste from the group list – see section 5.)
   - Context menus: Copy / Paste / Paste Special in the file, group, field and value menus; Paste / Paste Special
     in the empty-list menus (no groups shown, no fields shown, field with no values). Value Copy takes the same
     values as Duplicate and Delete.
   - Edit menu: Copy GPARAM, Paste, Paste Special at the top level; Copy Group / Field / Values in each submenu.
   - Keys: Copy / Paste (Ctrl+C / Ctrl+V by default) in each list while it has focus (mouse over it). Ignored
     while a text box is being typed in.
   - Paste Special: clipboard summary, mode combo for its kind (with per-mode tooltips), "Use this mode for
     Paste" (saves to CFG), live result line that follows the selection (cached per mode + selection, so a large
     GPARAM isn't re-read every frame), Paste / Cancel.
   - Replace All asks first, for Paste, Paste Special and the old Import GPARAM command alike.
   - Popups are drawn by `GparamEditorView.Display` after the list windows.
4. **Files** – Export to File… / Import from File… on the same payload; File-menu entries; remove the Tools menubar
   dropdown, the override-name boxes and the shared `ExportDirectory`.

   **Status: done (2026-11-05).**
   - `GparamTransferCommands.ExportToFile(kind, target)`: Save File dialog (JSON filter), opened in
     `CFG.GparamEditor_Transfer_Last_Folder`; adds `.json` if left off; remembers the folder.
     `PlatformUtils` got `OpenFileDialog` / `SaveFileDialog` overloads that take a start folder.
   - `ImportFromFile()`: open dialog in the same folder, any kind accepted, and the file opens **in the Paste
     Special window** (titled "Import from File") so the mode can be chosen and the result seen before applying.
     The window now shows where the data was read from.
   - Context menus: one block per list – Copy, Paste, Paste Special, then Export … to File, Import from File. The
     per-list Import/Export submenus and override-name boxes are gone.
   - File menu: Import from File…, Export GPARAM to File…. Edit menu keeps Copy / Paste / Paste Special only.
   - Removed: the Tools menubar dropdown (`GparamToolView.DisplayDropdown`), `GparamDataTransferTool`'s per-kind
     import/export methods, `ExportDirectory`, `GparamImportType`, `CFG.GparamEditor_Data_Import_Overwrite`, and
     `GparamTransferService.SuggestFileName`.
   - The Tools-window section is a stop-gap until phase 5: Import tab (Import from File, Paste Special), Export tab
     (four export buttons, disabled when the selection has nothing of that kind; last export's JSON).
   - Not yet cleaned up (phase 6): the old per-list `…_Context_Import/Export…` strings and `GPARAM_Tools_Menu`,
     and the old single-object helpers in `GparamJSON.cs`.
5. **Tools section** – rebuild as the staging panel per *Tool UI Styling.md* (`OnToolWindow()`, own section
   child, `ConditionalMultiButtonInput`, LOC strings); retire
   `GparamImportType` and `GparamEditor_Data_Import_Overwrite`.

   **Status: done (2026-11-05).** (The two settings were already retired in phase 4.)
   - `GparamDataTransferTool` is now the staging panel, with its own `OnToolWindow()` (header + section child);
     `GparamToolView` just calls it.
   - **Data**: what's held and where it was read from; Load from Clipboard, Load from File…, Clear. The payload is
     held independently of the clipboard until cleared or replaced.
   - **Items (n)**: one line per item (group name/key and field count; field name/key, type and value count; value
     ID and value), built once on load, clipped list up to 8 rows, eye toggle to hide.
   - **Mode**: the combo for the payload's kind – it *is* the Paste default (`GparamEditor_Transfer_*_Mode`), so
     there's one setting per kind everywhere.
   - **Result**: the check against the current selection, cached per payload + mode + selection, and refreshed
     after any edit or undo in the view (`ActionManager.ActionPerformed`).
   - **Actions**: Apply (goes through `Commands.Apply`, so Replace All still asks), Save to File…, Copy to Clipboard.
     The payload stays held after Apply, to apply to further targets.
   - `GparamTransferCommands` gained `SaveToFile(payload)`, `ReadFile(out origin)`, `ReadClipboardPayload(out origin)`;
     `LastExportJson` and the stop-gap section's strings are gone.
6. **Localization** – all new strings in `Gparam-Editor-GUI-Tools.json` and the list JSONs; remove dead keys.

   **Status: done (2026-11-05).**
   - The transfer strings live in their own file, `Localization/English/Gparam-Editor-Tools-Transfer.json`
     (125 entries), rather than in the GUI-Tools and list files. The loader reads every file in the folder.
   - Removed 29 keys this work left unused: each list's `…_Context_Import_Action(_TT)` and `…_Context_Export_…`
     strings (File, Group, Field, Value lists) and `GPARAM_Tools_Menu`. English only – no other language had them.
   - Not touched: older GPARAM keys that no code reads (`GPARAM_Data_Failed_*`, `GPARAM_Fields_Duplicate(_TT)`,
     `GPARAM_MassEditTool_{File,Group,Field}_TT`, `GPARAM_SearchTool_{File,Group,Field}_TT`,
     `GPARAM_ValueList_Row_*_TT`). They predate this work.
   - `GparamJSON.cs`: removed the single-object `ToJson` / `FromJson` / `ParamFromJson` / `FieldFromJson` /
     `FieldValueFromJson` helpers and `FieldValueWrapper`; kept `Options`, the type map, `GetFieldValueType`,
     `GetFieldTypeDiscriminator` and the converters.
   - *Tool UI Styling.md* gained a **Modals** section (set a width when text wraps; open from menus by flag).

All six phases are done.

## 5. Verification (for the user)
- Copy a group in file A, paste into file B: group appears/replaces by key; undo restores B.
- Multi-select 3 values, copy, paste into another field of the same type: 3 values inserted after the selection,
  colliding IDs renumbered with a log line. Paste into a field of another type: refused with a message.
- Paste a Field payload with no group selected: clear error, nothing changes. (With a group selected, it goes
  into that group from whichever list it's pasted in.)
- Open an export made by the **current** build (no envelope) through Import from File…: still imports.
- Export to File… twice with different names: two files, no silent overwrite.

## 6. Decisions
1. ~~Keep the Tools-window section as a staging panel, or remove it?~~ **Decided: keep it as the staging panel
   described in 3.4.** Phase 5 rebuilds it; it is not removed.

2. ~~Export naming?~~ **Decided: Save File dialog** (3.5), opened in the last-used folder from CFG. No auto-name
   export, no override-name box. The name field starts empty because the dialog wrapper can't pre-fill it.

3. ~~Default paste mode for groups and fields?~~ **Decided: match by key** – replace the group/field with the
   same key, else add it (today's behaviour). "Replace selected" is offered only under Paste Special. Fix the
   old tooltips that say "Replace the selected …" so they describe key matching.

All decisions are made; the plan is ready to implement from phase 1.
