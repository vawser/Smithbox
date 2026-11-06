# Phase 6.3: the Havok Editor on the unified Havok library

Part of the [Unified Havok Library Plan](Unified%20Havok%20Library%20Plan.md), Phase 6 step 3. Follows [6.2](Havok%20Phase%206.2%20Collision%20and%20Navmesh.md).

Status:
- **6.3a done (2026-11-06):** checked with game files, and in the app (the user confirmed every item in the Acceptance list).
- **6.3b done (2026-11-06):** checked with game files, and in the app (the user confirmed every item in its list). The Havok Editor now opens every game the unified library reads: all but original DS2.

## 6.3b: what was done

- **Turned on:** BB, DS2S, DS1, DS1R and DeS, in `EditorConditions.SupportsHavokEditor`.
- **Binders by header:**
  - `HavokBank.ReadContainer` tells BND3 from BND4, and BXF3 from BXF4, by the file. The per-game `IsBND3`/`IsBXF3` lists are gone; they had missed DeS.
  - **Loose files:** DS1 and DeS map collision are loose `.hkx` files. The bank holds one as a container of one file and saves it back to its own path. File actions (paste, delete, rename, insert, compress) are refused on it with a message (`HAVOK_FileView_ContextAction_Loose_File`). Copying out of it still works.
- **Locator:** DS1R and DeS `/obj/*.objbnd` are now listed as asset collision (`ShouldAddToHavokAssetFiles`; only DS1's were before). The general asset rule is unchanged.
- **Compression defaults** for files that can't be read as DCX: BB `DCX_DFLT_10000_44_9` (as DS3), DeS `DCX_EDGE`. Both used to fall through to `DCX_KRAK`.
- **Meta:** each SDK has an ordered list of meta folders, and a class takes the first that describes it.
  - hk2015 uses hk2018 then hk2014.
  - hk2010 and hk55 use hk2014 then hk2018.
  - No new JSON. Very few of these games' classes have descriptions yet (DS1 2 of 64, BB 28 of 190).
- **Collision Generator:** limited to DS3 and the 2018/2019 games (`HAVOK_CollisionGen_Unsupported_Game` elsewhere).
  - DeS, DS1 and DS1R use hkp physics, which it doesn't build.
  - BB, DS2S and SDT shapes were never checked against what it builds. BB, for example, would have had its compressed mesh swapped for an extern mesh.
- **Fix to E3 found here:** fields are now hidden by the profile of the *shown file*, not the project's (`HavokPropertyCache.ProfileSource`).
  - A DeS project holds Havok-5.5 and 5.1 files, read with `DES` and `DES_51`. Both share the hk55 classes, and 5.1 has fields 5.5 doesn't (`hkaSkeletalAnimation.numberOfTracks`, `hkaAnimationBinding.animationTrackToBoneIndices`). Under the project's profile, real 5.1 data was hidden.
  - The same applies to ER's stray 2016 files, which are now filtered by SDT's registry instead of keeping every field.
- **DS1R's 2015 registry, rebuilt from all 17,603 files** (now including cutscenes and rumble): all 416 types match and none are missing. The registry is unchanged.

### Verified with game files (6.3b)

`Tools/HavokEditorRoundTrip ALL 12 8` (now 11 games, with DS2S's `/model/*.bnd` binders and DS1/DeS loose map collision):

| Game | Read | Saved as stored | Round trip | Hidden fields holding a value |
|---|---|---|---|---|
| BB | 365 / 365 | 337 | 365 / 365 | — |
| DS2S | 133 / 133 | 133 | 133 / 133 | — |
| DS1 | 172 / 172 | 172 | 172 / 172 | — |
| DS1R | 217 / 217 | 216 | 217 / 217 | — |
| DeS | 162 / 163 | 162 | 162 / 162 | 0 of 251 |

- The one DeS file that doesn't read is `c9900.hkx` (Havok-4.5, out of scope since Phase 0). The editor logs a read failure for it.
- The 6.3a games are unchanged from the table below.
- The tool's own field cache had keyed profiles by game name, so `DES` and `DES_51` collided. It now keys by profile.

### To check in the app (6.3b)

1. **BB:** open a character (`chrbnd`) and an animation; edit, save, reopen. Open a map collision `hkxbhd`.
2. **DS2S:** open a map collision (`/model/map/*.hkxbhd`) and an object `.bnd`; save one.
3. **DS1:** open a loose map collision `.hkx` under Map Collision; edit, save. It's written to the project at the same path, and the Map Editor still draws it. Paste/delete/rename on it give the "loose file" message.
4. **DS1R:** open an animation (BND3) and a map collision (BXF3); save one. Object collision `objbnd`s are listed under Asset Collision.
5. **DeS:** open an animation from a 5.1 file and one from a 5.5 file; both show their fields (5.1 binding's track-to-bone indices included). Open a loose map collision. `c9900.chrbnd`'s `c9900.hkx` logs a read error rather than crashing.
6. **Collision Generator** in a BB or DS1 project shows the "not supported" message; in ER and DS3 it works as before.

## 6.3a: what was done

- **`Havok.Files.HavokFile`:**
  - `Write(HavokDocument)` writes in the form the file was read (container, SDK, header, compendium reference).
  - `HasField(profile, type, field)` tells whether the game's registry gives a class a field, counting parents.
  - `Read` now keeps the compendium on the document only if the file refers to one (TCRF). Otherwise a write would replace a file's own TYPE section with a compendium reference.
- **`HavokBank`** reads and saves every game through `HavokFile`. The binder's compendium is read only for files that refer to it. `HavokFileWrapper` holds a `HavokDocument` (`Root` is now its root). `HavokTagSerializer.cs` is archived in `.archive/Havok Phase 6.3`.
- **Views and tools are SDK-neutral:**
  - The behaviour views find objects through the `Havok.Shared` markers (`IStateMachine`, `IClipGenerator`, `ICustomManualSelectorGenerator`), and the behaviour graph by class name. Names are read by field (`HavokTypeUtils.GetName`).
  - The alias generator has one implementation for ER and DS3.
  - Clip flags use hk2018's `ClipFlags` for every SDK: the values are the same, and the hk2014/hk2016 registries type the field as a plain byte.
  - The property view uses `hkPropertyBag` by name and Core's `Havok.Matrix3x3`.
- **Collision Generator (E2):**
  - Its 2018/2019 generators are on `Havok.hk2018`/`hk2019`, and the DS3 ones on `Havok.hk2014` (nested names, array members, `hkcdShapeDispatchType`).
  - It dispatches on the root's class set.
  - SDT has no generator, so it logs "no replaceable shape", as it would for any unsupported game.
- **Variant fields (E3):** `HavokPropertyCache` and the Map Editor's `MapPropertyCache` take the project's profile and drop fields the registry doesn't give the class. Classes the registry doesn't know (ER's stray 2016 files) keep every field.
- **Meta:** `HavokMetaHelper.GetMeta` picks the folder from the class set (hk2014 for `Havok.hk2014`; hk2018 for 2016, 2018 and 2019). It compares names with the library prefix and nesting separators removed. That matches all three spellings in the JSON (`hkaiNavMesh+Edge`, `hkcdStaticMeshTree.Section`, HKX2's flat `hkbStateMachineTimeInterval`). The JSON files are unchanged.
- **Map Editor banks:**
  - `HavokCollisionBank.MapDocuments` and `HavokNavmeshBank.Documents` replace the HKLib containers.
  - One save path serves ER, NR and AC6. It keeps each file's DCX type, which replaces the hard-coded `DCX_KRAK`.
  - `HKLibTagfileReader.cs` is archived. Its compendium cache is now `Renderer/Resource Types/HavokCompendiumSource.cs`, unified-only.
- **SDT** is on in `EditorConditions.SupportsHavokEditor`.
- **Found and fixed:** the 2016 registry had no `hkaQuantizedAnimation`, because the survey never sampled cutscene or rumble binders. 42 SDT cutscene files couldn't be read.
  - The survey now samples `cutscenebnd`/`remobnd`/`rumblebnd`. A full `--buildregistry` over 21,030 SDT files found that one missing type and nothing else.
  - `Havok.hk2016` was regenerated. The diff is that class, its accessor and one identity-map line. The old registry and Autogen are in `.archive/Havok Phase 6.3/hk2016 before cutscene types`.
  - DS1R's 2015 registry was built the same way, so check it in 6.3b.

### Verified with game files

`Tools/HavokEditorRoundTrip ALL 12 8`, report in [Havok Library Survey/Havok Editor Round Trip.md](Havok%20Library%20Survey/Havok%20Editor%20Round%20Trip.md). For every game and category the editor opens:

| Game | Read: old → new | Same data as old | Saved as stored: old → new | Round trip | Hidden fields holding a value |
|---|---|---|---|---|---|
| ER | 339 → **343 / 343** | 339 / 339 | 0 → **342** | 343 / 343 | 0 of 61,395 |
| NR | 344 → 344 / 344 | 344 / 344 | 0 → **344** | 344 / 344 | 0 of 40,293 |
| AC6 | 310 → 310 / 310 | 310 / 310 | 309 → 309 | 310 / 310 | — |
| DB_NT | 75 → **347 / 347** | 67 / 75 | 0 → **293** | 347 / 347 | 0 of 10,012 |
| DS3 | 391 → 391 / 391 | 387 / 391 | 268 → **311** | 391 / 391 | — |
| SDT | — (new) → **397 / 397** | — | — → 396 | 397 / 397 | — |

- "Saved as stored" is a save without edits giving back the stored bytes. HKLib 2018's writer never did this. ER, NR and DB_NT now mostly do.
- The old/new differences are the new path being right:
  - **DS3:** 4 Shift-JIS body names, which HKX2 read as `?`.
  - **DB_NT rumble:** 8 files. HKLib read `hkaAnimation.type` with ER's layout and got `HK_UNKNOWN_ANIMATION`, where the new read gets `HK_INTERLEAVED_ANIMATION`.
- **Hidden fields:** 111,700 hidden field values were checked (ER, NR and DB_NT behaviours and collision; the other SDKs' class sets hold one game each, so nothing is hidden there), and none held a non-default value after a read. Every field the registry hides really is absent from the files.
- **ER's 4 files the old path couldn't read** are rumble files; the new path reads them, and they round-trip.
- **Meta:** all 56 hk2018 meta files are found. 31 of the 56 hk2014 ones occur in DS3's sampled files. Of the other 25, the 5 that named a real HKX2 class have the same hk2014 name (extern mesh shapes, `hkGeometry`, `hkaiStaticTreeNavMeshQueryMediator`). They're just not in the sampled game files. The other 20 never matched an HKX2 class either.
- **Not identical, unchanged from before:** DS3 behaviours (0/58, old and new) and some DS3 asset collision. The data sections lay out differently from the dumps (Phase 4).

### To check in the app

See the end of this file's Acceptance section.

## Before 6.3

The Havok Editor is on for ER, NR, AC6, DB_NT, DB and DS3 (`EditorConditions.SupportsHavokEditor`).

| Piece | ER, NR, DB_NT, DB | AC6 | DS3 |
|---|---|---|---|
| Read/write (`HavokBank.ReadRoot`/`WriteRoot`) | `HavokTagSerializer2018` (HKLib 2018, NR registry for NR) | `HavokTagSerializer2019` (HKLib 2019, binder compendium) | HKX2 packfile (fixed DS3 header on write) |
| Classes the views use | `HKLib.hk2018.*` | `HKLib.hk2019.*` | `HKX2.*` |
| Field descriptions (`Assets/HAVOK`) | `hk2018/*.json`, keyed `HKLib.hk2018.<type>` | shares hk2018's (prefix rewritten) | `hk2014/*.json`, keyed `HKX2.<type>` |

`HavokTypeUtils.IsHKX2` is also true for DS1, DS1R and BB, but HKX2 can't read DS1/DS1R, and none of the three has the editor on.

The map editor's collision/navmesh banks keep their own HKLib copies of ER/NR/AC6 files, which `MapHavokPropertyView` edits and the banks save (left over from 6.2):
- `HavokCollisionBank.HavokContainers` / `AC6Containers`, read by `HKLibTagfileReader`, saved by `SaveMapCollision` (HKLib 2018, always `DCX_KRAK`) and `SaveAC6MapCollision`.
- `HavokNavmeshBank.HKX3_Containers` / `AC6_Containers`, same pattern.

## What uses version-specific types (from a grep)

| Area | Files | What changes |
|---|---|---|
| Bank + serializer | `HavokBank`, `HavokTagSerializer`, `HavokTypeUtils` | One `HavokFile.Read`/`Write` path; the wrapper holds a `HavokDocument` |
| Behaviour views | `HavokBehaviorView`, `HavokStateMachineView`, `HavokAnimationSelectorView`, `HavokAnimationClipView` | `HKLib.hk2018.X` / `HKX2.X` branches → `Havok.hk2018.X` / `Havok.hk2014.X`, or one branch through `Havok.Shared` interfaces |
| Property view | `HavokPropertyView` (`hkPropertyBag`, `HKLib.ValueTypes.Matrix3x3`), `HavokPropertyDecorators` (`ClipFlags`, animation IDs) | Core's `Havok.Matrix3x3`; per-SDK `hkPropertyBag` |
| Tools | `HavokAliasGenerator`, Collision Generator (`CollisionGeneratorTool`, `CollisionMaterialPicker`, `HavokShapeGenerator_*`, `HavokCompressedMeshGenerator_*`) | They build objects into the editor's loaded files, so their types must match the editor's |
| Meta | `HavokMeta.GetMeta` + 112 JSON files | Keys carry the old namespace |
| Map banks | `HavokCollisionBank`, `HavokNavmeshBank`, `MapHavokPropertyView`, `HKLibTagfileReader` | Documents through `HavokFile`; save through `HavokFile.Write` |

The editor framework itself (`HavokPropertyCache`, `HavokChangeField`, list add/remove, `HavokFileOutline`, `HavokBehaviorSymbols`, `HavokTreeSearch`) is `System.Reflection` on `m_` fields, and the generated classes keep HKLib's names and nesting, so it should carry over unchanged. *(verify per view)*

## Design

1. **`HavokFile.Write(HavokDocument)`** in `Havok.Files`. The packfile and tagfile writers already keep what they read (header, `__types__`, TYPE section) in weak tables keyed by the root object, so a fresh serializer writes byte-faithfully. Write uses the document's profile, dialect and compendium. Byte identity is what the survey already measured: ER 667/670, NR 638/642, AC6 773/774, DB_NT 604/710, DS3 693/860 (HKX2 today: 667).
2. **`HavokFileWrapper` holds a `HavokDocument`** (root + profile + compendium). `Root` stays as the object the views reflect on. `HavokTagSerializer` and `HKLibTagfileReader` are archived.
3. **The views use the generated classes.** A per-SDK `switch` stays where the algorithm reads a typed field; where the 6.1/6.2 `Havok.Shared` interfaces already describe the data, they're used instead.
4. **Meta keys become SDK-relative** (`hkbClipGenerator`, `hkaiNavMesh+Edge`), looked up in the folder of the document's SDK. The hk2019 → hk2018 fallback stays.
5. **Game variants.** `Havok.hk2018` holds the union of ER, NR and DB_NT fields, so DB_NT's `m_additiveType` would show in ER files. The class set knows which fields the game's registry has, so the property view can hide the rest automatically (replaces `HavokField.ProjectTypes`).
6. **Map banks** cache `HavokDocument`s and save with `HavokFile.Write`, keeping each file's existing DCX type (fixes the hard-coded `DCX_KRAK`).

## Acceptance

- **Tool:** `Tools/HavokEditorRoundTrip <GAME|ALL> <containers> <files>`: for each editor category, read with HKLib/HKX2 and with `HavokFile`, compare the graphs (reusing the survey's cross-class-set compare), then check `HavokFile.Write` re-reads equal and is byte-identical where the survey says it is.
- **In the app (user):**
  1. **Havok Editor, ER:** open a `behbnd` behaviour. The state machine, clip, selector and graph views list their entries with names, and fields show their community names and descriptions. Edit a field, save, reopen: the edit is kept. NR's `m_isBasePoseAnim` shows on NR clips only, and DB_NT's `m_additiveType` nowhere outside DB_NT.
  2. **Havok Editor, AC6:** open and save an animation and a map collision. Both refer to the binder's compendium.
  3. **Havok Editor, DS3:** open a behaviour (names and descriptions show), edit, save, reopen.
  4. **Havok Editor, SDT (new):** a behaviour, an animation, a map collision and a cutscene open. Saving one writes without errors in the log.
  5. **Map Editor, ER and AC6:** select a collision part and edit a field of its collision in the Havok properties, then save the map. The `.hkxbhd`/`.hkxbdt` are written to the project. Reload the map and the collision still draws.
  6. **Collision Generator:** replace an ER map collision's shape (compressed mesh), and a DS3 one. Both draw and save.
  7. **Paste and export** of an edited, unsaved file write the edited data.

## Decisions

Asked one at a time; recorded here as they're settled.

- **E1. Which games get the editor in 6.3.** **Decided (2026-11-06): every readable game, in two steps.**
  - **6.3a:** migrate the current games (ER, NR, AC6, DB_NT, DB, DS3) and add SDT (hk2016 tagfiles).
  - **6.3b:** add the rest: BB, DS2S, DS1, DS1R and DeS (packfiles, plus DS1R's 2015 tagfiles). Each game is turned on only after its round trip passes. These need meta for hk2010/hk2015/hk55, and BND3/BXF3 handling for DS1/DS1R.
- **E2. The Collision Generator** (builds into the editor's objects). **Decided (2026-11-06): move its types in 6.3a**, to `Havok.hk2018`/`hk2019`/`hk2014`. 6.4 still merges the three copies into one.
- **E3. Game-variant fields.** **Decided (2026-11-06): hide automatically from the registry.** The property view asks the document's registry whether the class has the field. `HavokField.ProjectTypes` stays as a manual override.
