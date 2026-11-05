# Map Editor Usability Plan

Date: 2026-11-05. This implements U1–U10 from `Map Editor Review.md`, plus applying Map List filters when an input is deactivated.

## Decisions

- **Per-map undo (U3)**, chosen by the user. While an action executes, undoes or redoes, `ViewportActionManager` keeps the action as the "running" one. `MapContainer.MarkEntityDirty` records the map's ID on that action. Unloading a map removes only the actions that recorded it. Actions that recorded no map (for example `PropArrayCopyAction`, which never marks anything dirty) are dropped on any unload, because nothing says which map they belong to.
- **Map List filter apply-on-deactivate**. The Edit menu currently edits the live filter's entry list in place, so the active filter changes with every keystroke, and its Name and Match Type are reset every frame. It will edit a working copy taken when the menu opens. Each input writes to the live filter when it is deactivated, and the Edit button saves the whole set. Toggling the search bar's exact-match checkbox also re-filters now (B15).
- **Context menu (U7)**. The `ent` passed to Scramble, Replicate, Frame, Pull and Select All only decides whether the item is shown; the actions already use the whole selection. So the fix is a better representative entity (the first part or region, otherwise the first entity), not "(first selected)" labels.
- **Paste placement (U4)**. A new option, "Paste at Placement Orb", on by default, in the Duplicate options. Pasted objects keep their layout, with the group's centre moved to the orb. The move is part of the paste's undo step. Created objects are always placed at the orb.
- **Reference clearing (U2)**. A reference is cleared by setting it to `null`. SoulsFormats writes `null`/`""` as index -1.
- **Window labels (U9)**. The labels go through `LOC.Get`, keeping the `##id` suffix. The English text is unchanged, so existing dock layouts keep working.

## Steps

1. Map List filter apply-on-deactivate; exact-match toggle refreshes the list.
2. U6: dictionary lookup for `GetMapContainerFromMapID`; cached sorted content-tree categories; Map List entries rebuilt only when their inputs change; clipped flat object lists.
3. U1: the active map follows the selection; viewport overlay line showing the active map.
4. U3: per-map undo; Save / Discard / Cancel on unload and view close; every unload path goes through the prompt; Global Mass Edit only unloads the maps it loaded.
5. U2: shared MSB reference scanning (also used by rename); delete and move-to-map warn and offer to clear broken references; the save-failure dialog names the field and offers to clear it.
6. U4: Create at the placement orb and without needing a selection; target defaults to the active map; Paste at the orb; "Place at List End" honoured.
7. U5: map load queue; Loading/Queued markers in the Map List; load commands select the map.
8. U7: context menu with an empty selection (Create, Paste, Frame, Show All); a better representative entity.
9. U8: multi-selection reorder; Reorder items disabled when nothing in the selection can be reordered.
10. U9: shared target-map picker with an "Active Map" entry; LOC labels; consistent Duplicate-to-Map labels.
11. U10: autosave never opens a dialog; a map whose save failed is marked in the Map List until it saves.
