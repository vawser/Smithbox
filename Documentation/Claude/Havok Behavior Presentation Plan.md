# Havok Behavior Presentation Plan

*2026-11-05. Follows on from `Havok Editor Review.md` (all usability items there are done).*

## Goal

A behavior file is a graph of objects held together by references and integer indices. The editor shows that graph as it's stored, so a modder has to work out by hand:

- which event `m_eventId = 47` is,
- which state `m_toStateId = 3` goes to,
- which variable drives a field through a `hkbVariableBindingSet` `m_memberPath` string,
- where animation `a000_003000` is wired, and what else uses that generator.

This plan moves the editor from showing **how the file is stored** to showing **what the behavior does**. It's split into phases so each one is useful on its own.

## Current state

- **Behavior View** (`GUI/Behavior View/`): flat, filterable lists of clip generators, selectors (CMSG), state machines and variables. The Variables tab is commented out. Each entry opens the generic property grid (`HavokPropertyView.HavokPropEditGeneric`).
- **Decorators** (`Framework/HavokPropertyDecorators.cs`), driven by flags on `HavokField` in the meta JSON (`JSON/HavokMeta.cs`):
  - `ParamRef`: param link
  - `ClipGeneratorFlags`: flag editor
  - `AnimationInternalID`: a "Set to Free ID" button only, with no name resolution.
- **Shared interfaces** (`Havok/Havok.Shared/Interfaces.cs`): `IRootContainer`, `IClipGenerator`, `IStateMachine`, `ICustomManualSelectorGenerator`. These are empty markers. The views still branch on `HKLib.hk2018` vs `HKX2` and duplicate the code for each.
- `Hexa.NET.ImNodes` 2.2.7 is already referenced by `Smithbox.Program.csproj` (not used by the Havok Editor yet).

### Where the names live (hk2018, ER/AC6/NR)

| Index field | Name source |
|---|---|
| `hkbStateMachine.TransitionInfo.m_eventId`, `hkbEventBase.m_id` (enter/exit notify events, `m_eventToSendWhenStateOrTransitionChanges`), `hkbStateMachine.m_returnToPreviousStateEventId` / `m_randomTransitionEventId` / `m_transitionToNext*StateEventId` | `hkbBehaviorGraphStringData.m_eventNames` (parallel to `hkbBehaviorGraphData.m_eventInfos`) |
| `hkbVariableBindingSet.Binding.m_variableIndex` (`m_bindingType` = variable), `hkbStateMachine.m_syncVariableIndex` | `m_variableNames` (parallel to `m_variableInfos`, `m_variableBounds`, `m_variableInitialValues`) |
| binding with `m_bindingType` = character property | `m_characterPropertyNames` (parallel to `m_characterPropertyInfos`) |
| `TransitionInfo.m_toStateId`, `m_fromNestedStateId`, `m_toNestedStateId`, `hkbStateMachine.m_startStateId` | `StateInfo.m_stateId` / `m_name` within the owning (or nested) state machine |
| `hkbClipGenerator.m_animationInternalId` | `hkbBehaviorGraphStringData.m_animationNames` *(to verify: index vs. name pairing per game)* |
| `CustomManualSelectorGenerator.m_animId`, `hkbClipGenerator.m_animationName` | TAE / anim entry IDs (FromSoftware convention `aXXX_YYYYYY`) |

HKX2 (`HKX2.hkbBehaviorGraphStringData`) has event, attribute, variable and character-property names, but **no `m_animationNames`**. Animation-name resolution is hk2018-only unless another source turns up.

---

## Phase 1: Symbol table and name resolution

**Highest value for the effort. Every later phase reads from this.**

> **Status (2026-11-05): implemented, not yet checked in the app. `Smithbox.Program` builds.**
> - `Data/HavokBehaviorSymbols.cs` reads everything by field name, so it covers hk2018, hk2019 (AC6) and HKX2 (DS3) with no version branches. Names are read from the file each time, so renames show at once. The parent map and binding-set list are rebuilt on structural edits (with the other behavior tab lists). Only built for the Behavior category.
> - Field roles use meta JSON tags (decided): `EventID`, `VariableIndex`, `StateID` on `HavokField`. New meta files for `hkbStateMachine`, `hkbStateMachine+TransitionInfo`, `hkbEvent`, `hkbEventProperty` and `hkbVariableBindingSet+Binding` (hk2018), plus their HKX2 counterparts (hk2014). `CharacterPropertyIndex` wasn't needed: a binding's `VariableIndex` resolves to a character property when its binding type says so.
> - Corrected after the game-data survey (below): binding rows now also match compound paths (`field:index/field`), animation names show as the file name, and five more event fields are tagged.
> - `m_fromNestedStateId` / `m_toNestedStateId` aren't resolved. They are 0 on every transition in ER and DS3, so this doesn't matter in practice.

### Game data survey (2026-11-05)

`Tools/BehaviorSurvey` reads behavior files through the VFS and checks these assumptions. Run it with the working directory at `src/Smithbox.Data`: `BehaviorSurvey <GAME> <binder regex> [max binders]`. Covered: ER (400 binders, 5.9M objects), DS3 (60 binders), with no read failures.

| Assumption | Result |
|---|---|
| `TransitionInfo.m_eventId` indexes `m_eventNames` | All 305,769 in range (ER); all 43,669 (DS3) |
| `m_toStateId` is a state ID in the owning state machine (via `StateInfo.m_transitions` or `m_wildcardTransitions`) | All resolve (ER and DS3) |
| `m_startStateId` is a state in the same machine | All resolve |
| `m_fromNestedStateId` / `m_toNestedStateId` | Always 0 |
| `hkbEvent` / `hkbEventProperty.m_id`, the state machine's `…EventId` fields, `m_syncVariableIndex` | Always -1 in ER and DS3 (tagged anyway; mods may set them) |
| Other event fields | Found and tagged: `TimeInterval.m_enterEventId` / `m_exitEventId`, `hkbManualSelectorGenerator.m_endOfClipEventId`, `hkbEventDrivenBlendingObject.m_onEventId` / `m_offEventId` (DS3: on `hkbLayer`). Some are set in ER `c0000`. |
| `m_animationInternalId` indexes `m_animationNames` | Yes, but the names are **file paths** (`..\..\..\..\Model\chr\c0000\hkx\a000\a000_003000.hkx`). The file stem equals the clip's `m_animationName` in all 1,095,705 ER clips. DS3 (HKX2) has no animation names. |
| Binding `m_memberPath` format | The field name without `m_`, with case ignored (`selectedGeneratorIndex`, `DownLimitAngle`), or compound: `field:index/field` from the bindable down (`twistParam:0/onGain`, `hands:1/controlData/targetHandle`). All 54,317 ER and 13,244 DS3 compound paths resolve to a real field. Only `isActive` / `currentStateId` on `hkbStateMachine` (runtime members) match no serialised field. |
| Binding indices in range, with type 1 = character property | All in range. Type 1 is common (ER: 62,977 of 143,365). |
| Parallel arrays (`eventInfos`, `variableInfos`, `variableBounds`, initial word values) | Always the same length as the names, except `variableBounds`, which is empty in 113 ER files |
| Duplicate state IDs | None |
| Sharing | Transition effects are heavily shared (one `CustomTransitionEffect` in 1,163 places). Clip generators are often shared in ER `c0000` (4,839); CMSGs sometimes (360 in ER). |
| Outline reachability (Phase 2 rules) | Every generator is reachable from the `hkbBehaviorGraph` root in ER and DS3, once `hkbGeneratorTransitionEffect` is treated as a node and `hkbManualSelectorTransitionEffect` is looked through |

Not covered:
- **AC6:** its behbnds hold FromSoftware `.bhv` files and a `.tae`, not Havok behavior graphs, so AC6 isn't a target for this work.
- **NR:** ~~HKLib can't read NR's main behaviors~~ Fixed (2026-11-05). NR's `CustomManualSelectorGenerator` adds `isBasePoseAnim` (hkBool, offset 198, hash 624531992); every other type in NR's 551 behavior files matches the ER registry (`Tools/HkTypeDiff`). HKLib now has `HavokTypeRegistry.Nightreign` (base registry + `Res/HavokTypeRegistry20180100_NR.xml` overlay), which `HavokTagSerializer.Create` uses for NR projects. All 551 NR and 817 ER files read, write and reread with every CMSG field intact; 181,075 of NR's 182,959 CMSGs have `isBasePoseAnim` set, so it isn't padding. The NR survey now runs with no read failures.

1. **`HavokBehaviorSymbols`** (new, `Data/`): built in `HavokBehaviorView.SetupBehaviorView` from the graph root, and rebuilt where that already reruns (file change, structural edit). It holds:
   - event names, variable names, character property names and animation names (each as index → name, plus reverse lookups)
   - for each state machine: `stateId → StateInfo`
   - a parent map, object → list of (parent object, field path), built in the same `HavokTreeSearch` walk. Phase 2's "Used by" panel needs this.
   - the variable bindings for each `hkbBindable`: memberPath → (kind, index)
   - one version per game (hk2018 / HKX2), behind a single API, so the UI code has no version branches.
2. **New meta tags** on `HavokField`: `EventID`, `VariableIndex`, `CharacterPropertyIndex`, `StateID`, `AnimationName`. Add them to the existing ER/AC6/NR meta JSON for the fields in the table above. Matching by meta tag (rather than hard-coding field names) follows the existing `ParamRef` / `AnimationInternalID` pattern.
3. **Decorators** for those tags:
   - Show the resolved name next to the integer (`47  W_Attack`), in a dimmed colour, or red if the index is out of range.
   - Edit through a searchable combo of names. Keep the raw integer editable (raw-value toggle, like the quaternion editor from U9).
   - `AnimationInternalID`: also show the resolved animation name.
4. **Inline bindings:** when a field of a `hkbBindable` has a binding, show `← VariableName` (or `← CharProp:Name`) on that row, with a tooltip giving the memberPath, the bit index and the binding type. Clicking it selects the variable.
5. **Turn the Variables tab back on**, built on the symbol table: name, type, initial value, bounds, and how many bindings use it.

**Done when:** opening `c0000.hkx` (ER) shows event, variable and state names on every transition, notify event and binding without opening any other node.

## Phase 2: Structure views

> **Status (2026-11-05): implemented, not yet checked in the app. `Smithbox.Program` builds.**
> - **Outline** tab (`GUI/Behavior View/HavokOutlineView.cs`), first in the behavior tabs. Nodes are generators and states (`HavokBehaviorSymbols.GetNodeKind`). Children are found by looking through up to 3 intermediate objects. Transition effects, modifiers, conditions, binding sets, property bags and graph data are skipped, except `hkbGeneratorTransitionEffect` (a node) and `hkbManualSelectorTransitionEffect` (looked through). The survey shows these rules reach every generator. Children are cached per node in the symbol table, so they're rebuilt with it on structural edits.
> - Labels: a state adds `[id N]`, a CMSG `[anim N]`, a clip its animation if it differs from the name, a behavior reference its behavior name. Nodes with more than one parent show `shared xN`. The search filter matches labels, opening the nodes above a match.
> - **Used By**: a collapsible header above the property grid for the Outline, State Machines, Selectors and Clips tabs. It lists each referencing object with the field (`m_generators:12`), and clicking one shows it in the Outline. When the referencer isn't a node (a TransitionInfo, a blender child), the Outline selects the node above it. Other tabs get **Show in Outline**.
> - **Back / Forward**: arrow buttons in the Outline header, plus new keybinds `HavokEditor_Navigate_Back` / `_Forward` (Alt+Left / Alt+Right, new *Havok Editor* category), ignored while typing. History holds up to 100 entries, and entries removed by an edit are dropped.

1. **Generator outline** (new tab, or a mode of the State Machines tab): a tree showing only behavior nodes, `StateMachine › State › Selector / Layer / Blender / Script › Clip`. It skips `hkRootLevelContainer`, `hkbBehaviorGraphData`, string data, binding sets and transition arrays.
   - The label is `m_name`, with a short type tag (`SM`, `CMSG`, `Clip`, `Blend` …). For a CMSG, add its `m_animId`.
   - Selecting a node shows its properties in the existing property panel.
   - A shared generator appears under each parent, marked with a "shared ×N" tag.
2. **"Used by" panel**, under the property panel: every parent that references the selected object, with its field path. Click one to jump there. This uses the Phase 1 parent map.
3. **Back/forward navigation** across jumps (Alt+Left / Alt+Right), so following references doesn't lose your place.

**Done when:** you can go from the root state machine to a clip and back without opening a raw `m_states` list, and you can see before editing whether a generator is shared.

## Phase 3: Task views and validation

1. **Animation table** (FromSoftware specific): one row per CMSG. Columns: `m_animId`, CMSG name, owning state (and its state machine), child clip names, the TAE it resolves to, `m_enableTae` / `m_enableScript`. It can be searched and sorted. Double-click jumps to the CMSG in the outline. A "Open TAE" link appears if the TAE editor can be targeted (see decision D3).
2. **Validation pass** (`Utilities/HavokBehaviorValidator.cs`): run it on demand and after Save, with results in a list you can click to jump to the problem. Checks:
   - event, variable and character-property indices out of range
   - parallel arrays with different lengths (`eventNames` / `eventInfos`, `variableNames` / `variableInfos` / `variableBounds` / initial values)
   - duplicate `m_stateId` within one state machine
   - `m_toStateId` / `m_startStateId` that don't match any state
   - clip generators no generator references
   - duplicate `m_animationInternalId`
   - a CMSG `m_animId` or clip `m_animationName` with no anim/TAE entry *(only if D3 gives us the anim list)*
   - binding memberPaths that don't match a field on the owning type.

**Done when:** a deliberately broken file (an event index past the end, a removed state) gives one clear entry per problem.

## Phase 4: Composite actions

Each of these is **one undoable step**, built from existing `HavokChangeField` / `HavokAddListEntry` / `HavokRemoveListEntry` actions grouped together. They're staged until Save, like every other edit (review U1).

1. **Register event / variable / character property:** appends to every parallel array together. Variables get a type, initial value and bounds.
2. **Add animation to selector:** creates an `hkbClipGenerator` (name, free internal ID via the existing logic, default playback), registers the animation name, and appends it to the CMSG's `m_generators`.
3. **Clone state:** copies a `StateInfo` and its generator subtree, with a new `m_stateId` and name. The user chooses whether shared generators are deep-copied or kept shared (D2). Transitions out of the state are copied too; transitions into it are not.
4. **Add transition:** pick the from state, an event (combo) and the to state (combo), then add it to the state's transition array, creating the array if there isn't one. Includes wildcard transitions.
5. **Safe delete:** removes a node only after showing its "Used by" references, and offers to unlink them.

**Done when:** you can add a new attack (event, state, CMSG and clip) without editing a raw list.

## Phase 5: State machine graph

A node view of the selected state machine, using `Hexa.NET.ImNodes`:

- Nodes are states (name, stateId, a generator summary). Edges are transitions, labelled with the event name, plus condition and flag markers. Wildcard transitions are drawn from a single "Any" node. The start state is highlighted.
- Selecting a node or edge shows its properties in the property panel. Double-clicking a state whose generator is a nested state machine opens that machine.
- Node positions are stored in a sidecar file (D4), because the hkx file has nowhere to keep them. Without saved positions, use an automatic layered layout.
- Creating an edge by dragging opens Phase 4's *Add transition*.

**Done when:** the ER player root state machine is readable as a graph, and adding an edge produces the same result as the Phase 4 action.

---

## Cross-cutting

- **Remove the hk2018/HKX2 branching.** Either give the marker interfaces in `Havok.Shared/Interfaces.cs` the members the views need (`Name`, `States`, `AnimId` …), implemented by hand in partial classes next to the autogen files, or put a small adapter layer in `HavokBehaviorSymbols`. **D1** decides which.
- **Localisation:** all new UI text goes through `LOC.Get` keys under `HAVOK_BehaviorView_*`, following the existing views.
- **Tool UI styling:** follow `Documentation/Claude/Tool UI Styling.md` for the validator and table views.
- **Performance:** ER player behavior graphs are large. Build the symbol table and parent map in a single walk, cache them in `HavokPropertyCache`'s lifetime, and keep them out of the draw loop.

## Decisions to make

- **D1. Removing the version branching:** add members to the shared interfaces with hand-written partials (cleaner calling code, touches the Havok library) *or* use an adapter layer inside Smithbox.Program (keeps the library untouched).
- **D2. Clone state with shared generators:** deep-copy by default, *or* keep them shared by default with an option to deep-copy.
- **D3. TAE link:** read anim IDs from the anibnd in the same chr folder (enables the validation check and the link), *or* show the IDs only and leave the link out for now.
- **D4. Graph layout storage:** a sidecar JSON in the project folder for each behavior file, *or* auto-layout only, with nothing saved.

## Order and size

| Phase | Size | Depends on |
|---|---|---|
| 1 Symbols and resolution | M | none |
| 2 Outline / Used by / navigation | M | 1 (parent map) |
| 3 Animation table and validator | M | 1 |
| 4 Composite actions | L | 1, 2 (safe delete) |
| 5 Graph view | L | 1, 4 (edge creation) |

Phases 2 and 3 don't depend on each other.

## What to check in the app (per phase)

- **1:** in ER `c0000.hkx`, transitions show event and state names, and a binding row shows `← variable`. Change an event through the combo, then undo. An out-of-range ID shows in red.
- **2:** the outline tree matches the raw state machine contents. A shared CMSG shows "shared ×N", and "Used by" lists the same number of parents.
- **3:** the animation table row count equals the number of CMSGs. Break an event index on purpose and the validator reports it.
- **4:** after *Add animation*, saving, reopening and the validator all come back clean. Undo removes every part of the action.
- **5:** the graph edges match the transition arrays, and node positions are kept after a reopen (if D4 = sidecar).
