# Havok Behavior Structure Edits

Follow-on to *Havok Behavior Presentation Plan.md* (Phase 4 edits). Adds:

| Edit | Where |
|---|---|
| Bind a variable to an object's member | Outline (any bindable node), Clips list |
| Remove a variable binding | Outline, Clips list |
| Duplicate / delete a CMSG | Outline, Selectors list |
| Duplicate / delete a variable | Variables list |
| Duplicate / delete a state machine | State Machines list (also Outline) |

Every edit is one undo step (`HavokCompoundAction`), built in `Utilities/HavokBehaviorOperations_Structure.cs`,
with its dialog in `HavokBehaviorActionsView`.

## Survey (2026-11-05, `Tools/BehaviorSurvey ... editsurvey`)

ER 400 binders / 799 behavior files, NR 276 / 551, DS3 100 / 298. NR matches ER on every row.

| Question | Result |
|---|---|
| Binding fields | `m_memberPath`, `m_variableIndex`, `m_bitIndex`, `m_bindingType` (hk2018 and HKX2) |
| `m_bitIndex` | Always -1 (ER 143,365 bindings) |
| Empty binding sets | None. Every set has at least one binding. |
| Shared binding sets | None: every set has exactly one owner |
| `m_indexOfBindingToEnable` | -1, except 1,224 ER sets where it points at the `enable` binding |
| Members bound on clips | `startTime` (29,188), `playbackSpeed` (57), `cropStart/EndAmountLocalTime` (12). CMSGs: `enableTae` only. |
| Other variable-index fields | Only `hkbStateMachine.m_syncVariableIndex`, always -1. No expression objects in any file. |
| CMSG holders (ER) | `StateInfo.m_generator` 191,862 · `hkbManualSelectorGenerator.m_generators[]` 50,441 · `hkbBlenderGeneratorChild.m_generator` 14,800 · script generator → state 22 · layer 12 · transition effect 13 |
| Shared CMSGs (ER) | 476 of 256,664 have more than one holder |
| State machine holders (ER) | `StateInfo.m_generator` 16,269 · `hkbBlenderGeneratorChild.m_generator` 16,029 · `hkbScriptGenerator.m_child` 3,675 (→ state 3,269, → layer 406) · `hkbLayer.m_generator` 1,755 · graph root 799. DS3 also has 3 held in a CMSG's `m_generators[]`. |
| What a state machine shares outward | 24% of ER machines share something with the rest of the file, mostly clips (15,867) and `CustomPreDeleteIndexSelector` (9,143) |

## Decisions

- **Placement** (where a duplicate goes, what a delete removes). Walk up from the object through single-field
  wrappers (`hkbScriptGenerator.m_child`, `hkbModifierGenerator.m_generator`) until one of:
  - an entry of a generator list (`hkbManualSelectorGenerator.m_generators`, `CustomManualSelectorGenerator.m_generators`):
    Duplicate appends a copy at the end (so the existing selector indices stay the same); Delete removes the entry,
    and later entries move down one.
  - a state (`StateInfo` / `hkbStateMachineStateInfo`): Duplicate adds a new state with a copy, Delete removes the
    state (decided: delete the state too).
  - anything else (blender child, layer, transition effect, graph root): not supported, and the dialog says
    what holds it. Blending inputs and layers aren't safe to add or remove blindly.
- **Duplicate** copies the object and the objects below it. Children shared with the rest of the file stay shared
  unless *Copy shared children* is ticked, as in Clone State. The chain from the placement down to the object is
  always copied, even if it's shared.
  - A duplicated state gets the next free state ID and copies the original's outgoing transitions. No transitions
    are added into it.
  - CMSG: you can change the animation ID. The CMSG's own clips are then always copied and retargeted to
    `aCCC_{new id}`. Their internal ID is that of an existing clip of the same animation, or a new
    `m_animationNames` entry, as Add Animation does. On HKX2 (DS3) the new animation must already be used by a clip.
- **Delete state** refuses if the state is its machine's start state or its machine's only state. It also removes
  every transition in that machine (state transitions and wildcards) that targets the state's ID.
  Nested-state transition targets (`m_toNestedStateId`) in other machines aren't followed.
- **Bind variable**: any object with `m_variableBindingSet`. Members offered are its own primitive, vector and
  quaternion fields (not `m_userData`); compound paths can still be edited in the property grid. If the member is
  already bound, its binding is changed instead. A new binding has bit index -1, binding type variable. An object
  without a set gets a new set (`m_indexOfBindingToEnable` -1).
- **Remove binding**: removing the last binding sets the owner's `m_variableBindingSet` to null (no vanilla set is
  empty). `m_indexOfBindingToEnable` follows the removal (-1 if it was the removed binding).
- **Duplicate variable**: name, info, bounds (if the file has bounds), and the initial value. A vector, quaternion
  or pointer variable gets its own quad or variant value. Its bindings aren't copied.
- **Delete variable**: removes the name, info, bounds, word value and its quad or variant value. Indices above it
  move down in every binding and every `VariableIndex`-tagged field. Its bindings are removed, and an index field
  pointing at it becomes -1. The other variables' quad and variant indices are re-numbered. The dialog warns that
  scripts (HKS) and other files may use the name.

## Clips, shortcuts, copy and paste (2026-11-06)

Survey (clips): every ER clip (1,150,658) is held by a `CustomManualSelectorGenerator.m_generators[]`; DS3 has 2 in
blender children. A selector's clip animation names are unique (all but 2 of 256,664 ER selectors) and usually in
category order.

- **Duplicate Clip**: a copy in the same selector for another category, `aCCC_{selector animId}`, built by Add Animation
  with the clip as its template (name, bindings and settings come from the clip; internal ID resolved the same way).
  The category starts at the selector's highest + 1. A shared clip picks which selector.
- **Delete Clip**: removed from every selector that holds it. The dialog warns if a selector is left with no generators.
- **Delete** takes the whole list selection (Ctrl+click) and is one undo step: `DescribeDeleteGenerators` / `DeleteGenerators`.
- **Copy / Paste** (decided: generators paste only into the file they came from; variables paste into any file):
  - Clips → the selected selector, or the selected clip's selector. They keep their category, or take the next free one
    if it's taken.
  - Selectors / state machines → where the selected selector or state machine is held, as Duplicate places them. A
    state placement copies the target's state with the pasted object in the target's place, named after the pasted object.
  - Variables → the end of the list, as snapshots taken at Copy (renamed `_Copy` if the name is taken).
  - Several items paste as one undo step: `HavokBehaviorOperations.Sequence` builds each edit after applying the ones
    before (so they see each other), then undoes them and hands them over as one compound action.
- **Shortcuts**: `HavokShortcuts.Monitor` → `HavokBehaviorActionsView.Shortcuts()` while the Contents window has focus.
  Duplicate, Delete, Copy and Paste act on the selection of the mode shown (Outline node, Animations row, Selectors /
  State Machines / Clips selection, Variables selection). Not while a dialog is open or a text box has focus. The menu
  items show the key hints.

## Status (2026-11-05)

Implemented. `Smithbox.Program` builds. Not yet checked in the app.

- Operations: `Utilities/HavokBehaviorOperations_Structure.cs` (`HavokBehaviorOperations` is now partial).
- `HavokBehaviorCloner.Clone` has an `alwaysCopy` overload that also returns the original → copy map.
- Menus: `HavokBehaviorActionsView.DisplayGeneratorItems` / `DisplayBindingItems` / `DisplayVariableItems`, used by the
  Outline node menu and the right-click menus of the Clips, Selectors, State Machines and Variables lists. The Variables
  header also has Duplicate / Delete buttons for the selection.
- The Outline also offers Duplicate / Delete State Machine (same menu code).

## To check in the app (ER c0000.hkx)

1. **Bind**: Clips → right-click a clip without bindings → Bind Variable → `playbackSpeed` + a REAL variable → Apply.
   The property grid shows `← variable` on playbackSpeed, and the Variables tab lists the binding. Undo removes the new set.
2. **Remove binding**: right-click a clip → Remove Binding → its `startTime` binding. Its binding set goes (it was the only
   binding). Undo brings it back.
3. **Duplicate CMSG (state)**: Selectors → right-click an attack CMSG held by a state → Duplicate. Change the animation ID,
   Apply. A new state appears in the same machine (next ID) with a CMSG whose clips are `aXXX_{new id}`. The Problems tab
   stays empty.
4. **Duplicate CMSG (list)**: one held by a manual selector. The copy is the selector's last generator.
5. **Delete CMSG**: one held by a non-start state. The dialog names the state and the transitions into it. After Apply,
   the state is gone and the Problems tab shows no missing state IDs.
6. **Duplicate / Delete state machine**: a state-held machine (most `*_SM` under `Master_SM`). A blender-held one says why
   it can't be done.
7. **Variables**: duplicate a VECTOR4 variable (its own quad value), then delete it again. Delete a variable used by
   bindings: the bindings to later variables still show the right names.
8. Save, reload the file, and check it reads and the edits are still there.
9. **Clips**: right-click a clip → Duplicate Clip → category = next free → Apply. The new clip is selected; its name has
   the new category. Ctrl+click two clips → Delete → both go, one undo step.
10. **Shortcuts**: with the Contents window focused, Ctrl+D / Delete / Ctrl+C / Ctrl+V (or your bindings) in each mode.
    Typing in the search box must not trigger them.
11. **Paste**: copy two clips, select another CMSG, paste: both are retargeted to that CMSG's ID. Copy a CMSG, select
    a state-held CMSG, paste: a new state appears. Copy a variable in c0000, open c9997, paste in Variables.
