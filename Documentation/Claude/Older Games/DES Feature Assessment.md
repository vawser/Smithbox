# DES feature assessment

Which Smithbox features are blocked, non-functional or unverified for Demon's Souls projects (2026-11-08).

How this was checked:
- Every `ProjectType` gate in `src/Smithbox.Program`: the 137 `ProjectType.DES` references, the `CanUse` and `Supports*` methods, and the game lists that name DS1 but leave out DES.
- The existing DES and Havok survey docs.
- A new tool, `Tools/DesFormatRoundTrip`. It reads every DES file of each format the editors save, writes it back with SoulsFormats, and compares the bytes; where the bytes differ, it re-reads the file and compares content. Its report is `Tools/DesFormatRoundTrip/report.md`.

Nothing here was checked in the app.

## Blocked: the game has the data, Smithbox turns the feature off

| Feature | Where | Why | Evidence |
|---|---|---|---|
| ~~**Time Act Editor saving**~~ **Fixed 2026-11-08** | `TimeActData.CanSaveTimeActs` (now `true`) | It was read-only because 124 of 132 DES `.tae` lost event parameters on write. | All 128 readable DES time acts now write back byte-identical, both raw and through the editor's template path. See "TAE writer fixes" below. |
| **Animation preview** (Model, Time Act and Map viewports) | `AnimationPackage.IsSupportedGame` | DES skeletons now read, but its clips are wavelet or delta compressed, and the renderer has no decoder for those. | `Havok Phase 6.1 Renderer Animation.md` |
| **Collision Editor**, fall protection, **Collision Generator** | `CollisionEditorTool.CanUse`; Havok Editor generator | Only hknp and DS3's hk2014 hkp are supported. DES is Havok 5.5 hkp, 32-bit and big endian. | `Collision Generator All Versions.md`: "hkp (DeS, DS1, DS1R, DS2S) is a later phase" |
| **Viewport map lighting from game data** | `GparamProfile.For(DES)` = `Older` (`Supported = false`) | DES lighting lives in DrawParam (`param/drawparam/aXX_drawparam.parambnd`), and nothing reads it for the viewport. Only the DES lightmap toggle applies. | `Older Games/Analysis.md` puts DES out of scope. A DrawParam adapter is planned for DS1R only. |
| **Prefabs** | `Prefab.New` has no `MSBD` case | Creating a prefab logs "Prefabs are not supported for this project type". The Prefabs section is still shown. | Code |
| **Param → map jumps** (ItemLotParam, GameAreaParam, BulletParam, …) | `ParamMapReferences` | None of the param-to-map-entity links list DES. | Code |
| **Havok 4.5 files** (30 files, for example `c9900.hkx`) | Unified Havok reader | Packfile v4 isn't supported. The 327 Havok-5.5 files and 2 Havok-5.1 files read and roundtrip. | `Havok Library Survey/Havok Editor Round Trip.md` |

The **Gparam Editor** is turned off for DES (`SupportsGraphicsParamEditor`). That is correct, not a gap: DES has no GPARAM files. Its DrawParam binders load in the **Param Editor**.

## Non-functional on specific files

- **Four anibnds read as empty:** `c0300`, `c1040`, `c9950` and `c9951` (both the `.anibnd` and the `.anibnd.dcx` copies). Their BND3 header has a file count of 0, but they hold 43–530 KB of data.
  - SoulsFormats reads them as having no files, so the Time Act and Havok editors show nothing for these characters.
  - Any save of these binders would write a 32-byte empty binder. That would lose data if the game reads the entries some other way. **Unknown.**

## Works, with caveats

| Feature | Roundtrip result | Caveat |
|---|---|---|
| **Map Editor save** (MSBD) | 40 of 42 exact; 2 differ in bytes with equal parts, models, regions and events | The content check is shallow: counts, plus part names, positions and models. |
| **Param Editor save** (gameparam and drawparam) | **0 of 185 exact**; all re-read with equal rows, IDs, names and cells | The header and string table layout differ (sizes change by −692 to +4 bytes). The content is equal. Whether the game accepts this is assumed from long use, not tested. |
| **Text Editor save** (FMG) | 37 exact; 29 differ by 2 bytes (header size field) | Content is equal. |
| **Material Editor save** (MTD) | 40 exact; 266 differ at 0x4D, size the same | Content is equal (parameter and texture counts). The byte at 0x4D is probably a marker or padding byte. Not investigated. |
| **Binders** (chrbnd, parambnd, anibnd) | Exact, apart from the 4 empty anibnds above | Re-compressing a `.dcx` writes different bytes (the compressor differs); the content is equal. |
| **TPF** | 9 exact; 122 differ in bytes, texture count equal | Only the texture count was compared. Texture writing for DES isn't used by any editor (the Texture Viewer is read-only). |
| **Model Editor save** | 3272 of 3519 exact through the editor path | Built 2026-11-08 (`DES Model Save.md`). The other 247 differ only in the two header face counts, by a few faces. Not checked in the app. |
| **Model display fixes** | — | Rigid placement, backface culling, textures and lightmaps were built 2026-11-07 (`DES Model Display.md`). Not checked in the app. |
| **`X` / `X.dcx` twins** | — | Only the `.dcx` copy is listed, and saves are mirrored to the plain copy. Which copy the game reads is unproven. |

## Not applicable: DES has no such data

These features are off for DES because the game doesn't have the format. They aren't gaps:
- fog and irradiance volumes, MATBIN, light probes and atlases;
- ER navigation (NVA/NVC/ONAV), Havok navmesh (DES uses NVM, which is drawn, and MCP/MCG generation is supported);
- multiplayer walls, map limits, asset configuration, auto-invade, prefab assets, parts token lists;
- param upgrader and regulation merge;
- param reloader and memory tools (the PS3 has no process to attach to);
- Havok script reloader and behaviors (DES has none);
- param icon configurations (none are defined for DES or DS1).

The DES data assets cover the same ground as DS1's: paramdefs, meta, row names, MSB meta, TAE templates and aliases.

## TAE writer fixes (2026-11-08)

What was wrong, all in `SoulsFormats/Formats/TAE`:
- **Event data header (Event.cs, Animation.cs):** DES event data is `type, paramOffset, 0, 0` (16 bytes).
  - The writer added an `Unk04` field the read never reads. That shifted every DES event's parameters by 4 bytes.
  - The read sized each event's parameters assuming DS1's 8-byte header, so they ran 8 bytes into the next event.
- **Event groups (EventGroup.cs):** DES group headers are 0x20 bytes (four fields, then 16 zero bytes).
  - The reader stepped 16 bytes, so a second group was read from the zero padding and the real second group was lost.
  - The type struct is padded to 0x10, and DES cutscene-entity groups have no data struct. The writer added one anyway.
- **Header (TAE.cs):**
  - The flags at 0x48 are now written as read (some DES files have both clear).
  - Names and the anims table are padded to 0x10.
  - DES points the skeleton name at the anims table when there is no name. The reader took the table as a garbage UTF-16 name, which was then written out.
- **Interleaved anim bodies (TAE.cs):** files whose first anim offset is 0 put each animation's body right before its own data. They are now written that way.
- **Mini header (Animation.cs):** the loop, import and delay-load bytes hold stale exporter memory in DES (e.g. `0x2E`, `0xEEEF`). They are written back as read while the flag is unchanged.
- **Template (`Assets/TAE/DES/Defs/Character.xml`):**
  - `Blend` (16) had no fields, but every file gives it 16 zero bytes.
  - `QueueTransitionAnimation` (24) had 16 bytes, but every file gives it 32.
  - Saving through the editor would have shortened each such event, so both entries are now padded with unnamed `s32` fields.

Other games are unchanged. DS1R, PTDE and Bloodborne TAEs give the same roundtrip counts before (commit `04a3df329`) and after these changes (run with the `taegame` mode of `Tools/DesFormatRoundTrip`).

## Most worth fixing

1. ~~**TAE writing**~~: fixed (see above). Not yet checked in the app or in the game.
2. **The four empty anibnds**: find out how the game reads them before any editor saves one.
3. **Param byte layout**: make the 185 params roundtrip exactly. Content is equal, but it is the most-used DES save path and none of it is byte-identical.
4. **DrawParam lighting in the viewport** and **hkp collision editing**: larger pieces of work.
