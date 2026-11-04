# Time Act Editor — Mass Edit Plan

A Tools-window section for the Time Act Editor that finds animations and events with the same filters as Local and
Global Search, then **edits**, **creates** or **deletes** across every match in one undoable step. The edit is
built from pickers (target → field → operation → value), not typed as a command like the Param Mass Edit.

## 1. What exists to build on

| Piece | Where | What we reuse |
|---|---|---|
| `TimeActSearchTool` (Local + Global modes) | `Tools/TimeActSearchTool.cs` | Source gathering, background run, matching (`MatchAnimation`, `MatchFields`, `MatchValue`, `MatchText`), field listing (`GetAnimationProperties`, `GetEventFields`), result list and Jump |
| Event actions | `Framework/TimeActEventActions.cs` | `TimeActMoveEventAction`, `TimeActSetParameterAction`, `TimeActAddEventsAction`, `TimeActDeleteEventsAction` |
| Animation actions | `Framework/TimeActAnimationActions.cs` | `TimeActAddAnimationAction` (with `isDelete`) |
| Property action | `Framework/TimeActEntryActions.cs` | `TimeActSetPropertyAction<T>` for animation properties |
| Compound undo | `Viewport/Action/ViewportCompoundAction.cs` | One undo for the whole mass edit |
| Event creation | `TimeActGraphWindow.AddEvent` | `new TAE.Event(start, end, type, unk04, bigEndian, template)` and the Unk04 rule |
| Parameter typing | `TimeActPropertiesWindow.DisplayParameter` | `ParameterTemplate.StringToValue/ValueToString`, `EnumEntries`, `TimeActField.IsBool`, `DefaultValue` |
| Template loading | `TimeActEntry.ApplyTemplate` / `GetReadableEvent` | Exact-fit templating before a parameter can be edited |
| UI conventions | `Documentation/Claude/Tool UI Styling.md` | Section frame, headers, inputs, buttons, progress, LOC keys |

### Constraints found in the code

1. **The time act list is the source of truth on save.** `TimeActContainerWrapper.UpdateTimeActFiles` removes
   binder `.tae` files not in `TimeActs`. A global edit of an unopened source must therefore adopt the *whole*
   list read by `ReadTimeActs()`, never just the matched entries, or saving would delete the rest.
2. **Global search reads copies.** Unopened sources are searched through `ReadTimeActs()` copies. Those copies
   are full `TimeActEntry` objects, so they can be adopted as `source.TimeActs` when the edit is applied — if the
   source is still unopened at that moment. Otherwise results are remapped by ID, as `Jump` already does.
3. **Parameters only exist after templating.** `ev.Parameters` is null until `ApplyTemplate` runs on its
   animation, and only exact-fit templates are applied. Events that don't fit can't have parameters edited; they
   are reported as skipped, not changed.
4. **Index-based actions don't compose.** `TimeActAddEventAction(isDelete)` captures its index when constructed,
   and `ViewportCompoundAction.Undo` runs forwards. Deletes and adds must be grouped **one action per animation**
   (`TimeActDeleteEventsAction`, `TimeActAddEventsAction`), which keep their own index bookkeeping.
5. **Undo history is cleared when the source changes.** `TimeActModelWrapper.Unload` clears both action managers.
   A global mass edit's undo is lost once the user selects another source. (See decision D1.)
6. **DES can't save** (`TimeActData.CanSaveTimeActs`). The tool shows a message and stops, per the UI guide.
7. **Animation ID edits re-sort.** Changing `Animation.ID` needs `ResortAnimation`; ID edits are left out of v1.

## 2. Architecture

```
Tools/
  Query/                      (built in Phase 0)
    TimeActQuery.cs           TimeActQuery (inputs), TimeActQuerySource, TimeActQueryMatch, TimeActQueryField
    TimeActQueryMatcher.cs    matching logic moved out of TimeActSearchTool
    TimeActQueryRunner.cs     TimeActQueryRun + source gathering, background run (MaxResults, 0 = none),
                              Jump, and TryResolve (remaps a match found in a read copy)
  TimeActSearchTool.cs        now a thin UI over Query/*
  MassEdit/
    TimeActMassEditTool.cs        UI: Scope → Filter → Operation → Preview → Apply
    TimeActMassEditOperation.cs   the built operation (Edit/Create/Delete + target + steps)
    TimeActMassEditPlanner.cs     matches + operation → list of planned changes (old/new, or skip reason)
    TimeActMassEditApplier.cs     planned changes → grouped actions → one compound action
```

The planner is pure (no ImGui, no action manager), so the preview and the apply use exactly the same results.

### Flow

1. **Find** — the runner gathers sources on the main thread and matches in the background (as Search does now).
2. **Plan** — on completion, the planner turns matches into `PlannedChange` rows: target, field, old value, new
   value, or a skip reason ("no template", "value out of range for u8", "end before start").
3. **Preview** — the rows are shown with a clipper, with counts per source and per skip reason.
4. **Apply** — on the main thread: adopt read copies for unopened sources (constraint 1–2), template the touched
   animations, build one action per animation, wrap them in a `ViewportCompoundAction`, execute once.

## 3. The UI (guided builder)

One `ToolSections.Header` section, groups as `GUI.SimpleHeader` boxes in this order:

**Scope** — combo: *Selected Events* · *Current Animation* · *Current Time Act* · *Current Source* · *All Sources*.
"All Sources" shows the scanning status like Global Search does.

**Target** — combo: *Events* · *Animations*. Decides which filters and fields appear below.

**Filter** — the Search inputs, with pickers where text is ambiguous:
- Animation: text (name, alias, properties) — as Search.
- Event type: searchable combo built from the source's `BankTemplate` (`[ID] Name`), plus free text as fallback.
- Conditions: rows of *Field* (combo of the chosen event type's parameters, or text) · *Comparison*
  (`contains`, `=`, `≠`, `<`, `≤`, `>`, `≥`, `between`) · *Value* (typed input: enum combo for enum parameters,
  checkbox for bool / `IsBool`, number otherwise). Add/remove rows; all rows must match.
- ~~Time range~~: dropped in Phase 1. *Start Time* and *End Time* are pickable condition fields, so
  `Start Time between 1 and 2` covers it without a separate input.

**Action** — three-way selector: *Edit* · *Create* · *Delete*. The section below it changes with the choice.

*Edit* — rows of steps, each a sentence assembled from combos:

> Set **[field ▾]** **[operation ▾]** **[value]**

- Field list comes from the chosen event type's template (with annotated names per `TimeActFieldNameMode`),
  plus *Start Time*, *End Time*, *Both times*. For animations: *Anim File Name*, *Import HKX Source*, *Import From*.
- Operations offered depend on the field type:
  - numbers: *set to*, *add*, *subtract*, *multiply by*, *divide by*, *clamp to min/max*, *reset to default*
  - enums: *set to* (combo of `EnumEntries`), *reset to default*
  - bool: *set to*, *toggle*
  - text: *set to*, *replace … with …*, *prefix*, *suffix*
  - times: *set to*, *shift by*, *scale by*, *set length to*; option *snap to frames* (`Playback.FrameDuration`)
- Values are parsed with `ParameterTemplate.StringToValue`; a result outside the type's range is a skip, not a wrap.
- The header line under the steps reads back the whole edit in plain words
  ("Set `DamageLevel` to 3 and shift times by +2 frames on 412 events in 37 animations").

*Create*
- Create **[event type ▾]** in each matched **animation** / alongside each matched **event**.
- Times: absolute start/end, or (alongside an event) its times plus an offset.
- Parameters: template default → `TimeActField.DefaultValue` → any *Set* steps from the Edit builder.
- Option: *skip animations that already have this event type*.
- Unk04 follows the graph window's rule (copy from an existing event of that type, else 0).
- For the *Animations* target: *duplicate matched animations at ID + offset* via `CloneAnimation` +
  `GetFreeAnimationID` (phase 6).

*Delete*
- Events: grouped per animation into `TimeActDeleteEventsAction`.
- Animations: `TimeActAddAnimationAction(isDelete: true)`, guarded by a confirm showing the count.

**Preview** — progress bar while matching, then the change list (`ImGuiListClipper`, framed list filter):
`Source  TimeAct  Animation  Event  Field  old → new`, with a Jump button per row (reuse `Jump`). Skipped rows are
grouped under their reason, collapsed by default.

**Actions** — *Find*, *Apply* (disabled until a preview exists and is current), *Clear*. Editing any input marks
the preview stale, so Apply always does exactly what was shown.

## 4. Phases

| # | Phase | Done when |
|---|---|---|
| 0 | **Extract the query engine** from `TimeActSearchTool` into `Tools/Query`, no behaviour change — **done 2026-11-04**, awaiting check | Local/Global Search behave exactly as before |
| 1 | **Tool skeleton**: CFG flag `Interface_TimeActEditor_Tool_MassEdit`, View menu toggle, section, Scope/Target/Filter, Find + match list — **done 2026-11-04**, awaiting check. See *Phase 1 notes* below | Matches list and jump, for local scopes |
| 2 | **Edit** for events: builder, planner, preview old → new, apply as one compound action — **done 2026-11-04**, awaiting check. See *Phase 2 notes* | Edits undo/redo in one step; skipped rows explained |
| 3 | **Delete** for events and animations — **done 2026-11-04**, awaiting check. See *Phase 3 notes* | Undo restores order and selection |
| 4 | **Create** events (per animation / alongside event) | New events have correct groups, defaults and Unk04 |
| 5 | **All Sources** scope: adopt read copies, dirty marking, Save All path, undo caveat. The runner discards the `ReadTimeActs()` list after querying a source; it needs an option to hand that full list back per source (constraint 1) | Edited unopened sources save without losing other time acts |
| 6 | Extras: animation property edits, duplicate animations, "Send to Mass Edit" button on Search results, comparison operators back-ported to Search | — |

### Phase 1 notes

- **Runner filter interface.** `TimeActQueryRunner.Start` takes an `ITimeActQueryFilter`. `TimeActQuery` (Search)
  implements it with its old behaviour; `TimeActMassEditFilter` (`Tools/MassEdit`) is the Mass Edit one. The event
  check is split into `MatchEventType` (cheap) and `MatchEvent` (reads fields), so events are only copied for
  templating once their type has matched, as before.
- **Field names.** Picked fields store the parameter's `Name` (what `GetEventFields` reports), not its key in the
  `EventTemplate` dictionary.
- **`TimeActData` overloads.** `GetFieldMeta`, `GetFieldAnnotation` and `GetFieldDisplayName` now also take an
  `EventTemplate`, so the field picker can show annotated names without an event.
- **Empty filter is allowed.** Find with nothing filled in matches everything in the scope (e.g. every event of
  the current animation), which is a valid mass-edit target.
- **Stale matches** are flagged when any input changes, or when the selection the scope covers changes
  (time act, animation, selected events). Phase 2's Apply will use the same check (D3).
- **Comparisons** use ASCII (`!=`, `<=`, `>=`) since the UI font may not have `≠ ≤ ≥`.
- **Animations target** matches animation properties in conditions, and the event-type picker means
  "has an event of this type". *Selected Events* scope is hidden for it.

### Phase 2 notes

- **Files.** `TimeActMassEditOperation.cs` (operators, field kinds, step record), `TimeActMassEditPlanner.cs`
  (pure: matches + steps → `TimeActMassEditPlan`), `TimeActMassEditApplier.cs` (re-check + one compound action),
  `TimeActMassEditTool.Edit.cs` (partial: Action / Steps / Changes UI).
- **Flow, refined.** Find gives the matches; the Changes preview is then worked out again on the main thread
  every time the steps change, so there's no separate preview button. Apply is enabled only when the matches
  aren't stale (D3), and always applies the plan on screen.
- **After Apply** the plan is dropped and Apply stays off until Find is pressed again. The old values in the plan
  are out of date by then, and re-planning an *add 1* step would offer to add it a second time.
- **Step fields** come from the matches: the times, then every parameter of the event types among them, with
  "(in N events)" on those only some matches have. Events of a type without the field are skipped with a reason.
- **All or nothing per event.** If any step can't apply to an event, none of them do; the event is listed under
  its skip reason.
- **Values.** *Set* parses with the template like the Properties window (hex for x-types). Enum entries are looked
  up by name first, because `StringToValue` turns an entry name into its decimal text and then parses that as hex
  for x-types. The arithmetic operators take plain decimal numbers; whole-number results are rounded, and a result
  outside the type's range is a skip, not a wrap.
- **Times** can be given in frames (on by default) at a fixed 30 fps, `TimeActMassEditFields.FrameDuration`.
  An end before its start is a skip.
- **Templating on apply.** The plan reads untemplated events through cached copies (`GetReadableEvent`); the
  applier calls `ApplyTemplate` on the real animation before setting parameters, then checks the live values
  equal the planned old values.
- **Undo order.** Every Phase 2 action sets values (`TimeActMoveEventAction`, `TimeActSetParameterAction`), so
  the forward-order undo of `ViewportCompoundAction` is safe. Phases 3–4 (index-based) still need grouping per
  animation (constraint 4).
- **Deferred:** the plain-words read-back of the whole edit, and *snap to frames*. The summary line gives the counts.

### Phase 3 notes

- **New actions** in `Framework/TimeActMassEditActions.cs`: `TimeActMassDeleteEventsAction` and
  `TimeActMassDeleteAnimationsAction`. Each holds one group per animation (or per time act), removes from the back
  and reinserts from the front, so constraint 4 holds in a single action rather than a compound.
- **Why not reuse the single-delete actions.** `TimeActDeleteEventsAction.Undo` sets the selection to its own
  events; with one per animation, the last undone group would win and leave events of an unselected animation
  selected. The mass actions snapshot the selection when they execute and restore exactly that.
- **Selected animation deleted:** the animation now in its place is selected (as the single delete does), or the
  selection is cleared if none are left. Undo selects it again, with its events.
- **Confirm:** animations only, via a modal opened outside the button callback (the popup must be opened and
  drawn under the same ID stack). Event deletes go straight through; undo covers them.
- **Re-check:** each match is resolved again with `TryResolve`; anything already gone is counted as stale.
- **Preview** is the matches themselves, sharing the Find tab's list filter. Event groups are left in place, as the
  single delete does.
- After a delete, both Edit and Delete stay disabled until Find is pressed again (`HasApplied`).

## 5. Verification (for the user, per phase)

Suggested models, per the verification-targets convention:
- **Local edit**: ER `c0000`, filter event type `[1] InvokeAttackBehavior`, edit a parameter by +1 → preview counts
  match Local Search's result count; one Ctrl+Z restores every value.
- **Delete**: an ER enemy character (one with a single time act keeps it quick), delete one event type from the
  current time act → graph updates; undo puts events back in their original order.
- **Create**: the same character, add an event to every animation matching `a000_00` with *skip if present* →
  no duplicates.
- **Global**: ER, *All Sources*, a narrow filter touching 2–3 unopened characters → those sources show `*`; Save All;
  reopen and confirm every `.tae` of those anibnds is still present (constraint 1).
- **Skip reasons**: set a `u8` parameter to 300 → every row is skipped as out of range, nothing changes.
- **DES project**: the section shows the "can't save" message.

## 6. Decisions

Asked one at a time; the recommended option is assumed in this plan until answered.

- **D1 — Undo for All Sources edits. DECIDED (2026-11-04): allow + warn.** Undo is cleared on source switch;
  global edits are allowed, and the preview warns that undo lasts only until another source is selected.
- **D2 — Filter inputs. DECIDED (2026-11-04): pickers + text.** Event type combo (from the `BankTemplate`), field
  combo (from that type's parameters), comparison combo and a typed value input, with free text kept as a
  fallback for each. Free text matches across event types that share a field name; a picked field is exact.
  Comparisons on numbers are numeric, so `= 1` never matches 10 or 100.
- **D3 — Preview requirement. DECIDED (2026-11-04): require a current preview.** Find builds the old → new
  preview; Apply is disabled until one exists, and editing any input (scope, target, filter, action, steps) marks
  it stale. Apply executes the planned changes as previewed, not a fresh match. The one confirm popup kept is the
  animation delete in §3. The data itself can still change after the preview (an edit in the graph or properties
  window), so the applier re-checks each row's current value against its planned old value and skips rows that
  no longer match, reporting the count in the log.
- **D4 — Result cap. DECIDED (2026-11-04): no cap.** Every match is planned and applied, and counts are always
  exact. The preview list draws through `ImGuiListClipper`, so its length doesn't affect drawing, and its filter
  cache is built incrementally as Search's is.
  - Phase 0: the shared query runner takes the cap as a parameter: Search passes 10 000, Mass Edit passes none.
  - Memory: a global preview holds the full `ReadTimeActs()` list of every source it touches (constraint 1).
    Clearing or re-running the preview releases the lists of sources that weren't applied. Applied sources keep
    theirs until saved, as the editor already does for unsaved sources.
  - Planned changes store references and old/new values, not labels; a row's label is built the first
    time it is needed (drawn by the clipper, or checked by the list filter) and cached.
