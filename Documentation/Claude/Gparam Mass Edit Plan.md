# Gparam Mass Edit Plan

A Mass Edit tool for the Gparam Editor, modelled on the Time Act Editor's (`Editors/Time Act Editor/Tools/MassEdit`,
see `TAE Mass Edit Plan.md`). It reuses the Gparam search's query runner (`Tools/Search`).

## Shape

Same two tabs as TAE:

- **Find**: Scope, Target, Filter (file / group / field text and value conditions), Find, then the matches
  (progress, a count, a stale warning and a filterable list where each row jumps to its match).
- **Edit**: the action (Edit / Delete / Duplicate), its inputs, a preview of every change, and Apply. Applying is
  one undo step. Once applied, the plan is dropped until the next Find.

## Mapping from TAE

| TAE                         | Gparam                                                  |
|-----------------------------|---------------------------------------------------------|
| Target: Events / Animations | Target: Values / Fields / Groups                        |
| Scope: Selected Events      | Selected Values (the value list's multi-selection)      |
| Scope: Current Animation    | Selected Field                                          |
| Scope: Current Time Act     | Selected Group                                          |
| Scope: Current Source       | Selected File                                           |
| Scope: All Sources          | All Files                                               |
| Event type picker           | Group and Field text inputs                             |
| Field conditions            | Value conditions: Value / ID / Time of Day + comparison |
| Steps on event fields       | Steps on Value, a component of it, ID or Time of Day    |
| Create events / Duplicate   | Duplicate values with an ID offset                      |
| Delete events / animations  | Delete values / fields / groups                         |

Values is the only target that can be edited or duplicated. Fields and Groups can only be deleted.

## Runner

The search runner takes an `IGparamQueryFilter` instead of a `GparamQuery`, so the search's query and the mass
edit's filter share the same background run, matcher and jump. The filter decides the level found (file, group,
field, value) and what is matched at each level. The mass edit's filter adds:

- several value conditions, all of which must match,
- a limit to the selected value rows (the Selected Values scope).

## Unopened files

The search reads unopened files through copies. The mass edit doesn't adopt those copies (TAE does). Instead, Apply
opens each file of a match in the bank (`LoadGraphicsParamNow`), and looks the match up again by its group key,
field key and value ID, as a jump does. Every planned row is checked against the values it had when the plan was
made. A row that has changed since is left alone and counted as stale, so the edit never builds on values it
didn't show.

## Steps

`Set [Field] [Operator] [Value]`, applied in order to each row:

- Fields: Value, Value component 1 to 4 (X/Y/Z/W, or R/G/B/A for colours), ID, Time of Day.
- Operators: Set, Add, Subtract, Multiply By, Divide By, At Least, At Most, Toggle.
- Vectors and colours: an operand of one number applies to every component. An operand with one number per
  component applies component by component. Set takes the whole value.
- Whole numbers are rounded. A result outside the type's range skips the row with a reason (as TAE does).
- Text can only be set. Bools can be set or toggled. An enum field takes the option's name or value.
- IDs must stay unique within a field. A row whose new ID would be taken in the field's final state is skipped.

A row is changed by every step or by none.

## Undo

- Edit: one `GparamMassSetValuesAction` sets each row's value, ID and time of day, keeping the old ones. It rebuilds
  the `UnkParamExtras` of a GPARAM whose IDs changed, and snapshots them for the undo.
- Delete: a `CompoundAction` of the existing `DeleteValueAction` / `DeleteFieldAction` / `DeleteGroupAction`, one
  per field, group or file.
- Duplicate: a `CompoundAction` of `AddValueAction` (duplicate form, ID offset). It moves a colliding ID up to the
  next free one.

The view's undo history isn't cleared when another file is selected, so an edit across files can be undone as one.
Save writes only the selected file, so the result tells the user to use Save All after an edit across files.

## Search hand-off

The Local and Global Search get a "Send to Mass Edit" button. It loads the query into the Find tab with the
matching scope (Selected File / Group / Field, or All Files) and runs Find.
