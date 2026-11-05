# Havok Editor: staging file actions until Save (U1)

Decision (2026-11-05): file actions (Paste, Delete, Rename, Insert, Compress, Change Compression) change an in-memory copy of the binder, can be undone, and are written to disk on Save, like field edits. Export stays immediate, since it only reads.

## Model

- **Working binder:** `HavokBank.WorkingBinders[binderEntry]` is the `IBinder` read from disk the first time a file action touches the binder. File actions, loading, Paste/Export reads and Save all use it while it exists, so staged files (pasted or inserted, not yet saved) can be opened, edited and copied.
- **Staged binders:** `HavokBank.StagedBinders` holds the binders whose file list or stored bytes differ from disk. `IsBinderDirty` includes them, so the binder list shows `*`, they aren't evicted, Save All writes them, and the unsaved-changes prompt lists them.
- **`HavokBinderAction`:** one undoable action per file action. Its first `Execute` captures a snapshot of the binder (each `BinderFile` object with its name, ID, flags and compression, plus the name → wrapper map), runs the change, re-syncs the wrappers to the binder files by object identity (so a renamed file keeps its wrapper, edits and undo history), and captures the after-snapshot. Redo restores the after-snapshot and Undo restores the before-snapshot.
  - Bytes are only restored for files whose bytes the action itself changed (compression). Otherwise an undo after a Save would put back the bytes from before a saved field edit. When bytes are restored for a loaded file, that file is marked dirty, so Save re-serializes its current objects with the restored compression.
- **Selections:** the action runs each change through `HavokEditorScreen.PreserveSelections`. That captures every view's selected file and multi-selection as wrapper objects before the change, and re-maps them by the wrappers' new keys afterwards (dropping files that are gone). This replaces the rename map from the second pass.
- **Pinning:** the screen pins the binders and wrappers referenced by binder actions in any view's history, as it already does for field edits. A pinned binder keeps its working binder and wrappers through refreshes.
- **Save:** `SaveBinder` uses the working binder when there is one, writes the dirty loaded files into it, writes the container and clears the staged flag. The working binder is kept, so later undos still apply. An unpinned, unstaged binder drops its working binder on the next refresh and reads from disk again.

## Limits

- While a binder has a working binder (staged changes, or file-action history), another tool writing the same binder file on disk isn't picked up, and the next Havok save overwrites it. Before this change the binder was re-read on every save.
- Each view has its own undo stack, so a file action is undone from the view where it was done.
