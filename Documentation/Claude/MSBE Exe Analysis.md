# MSBE (Elden Ring MSB) checked against eldenring.exe

Checked against `Documentation/Games/eldenring.exe` (87,011,328 bytes) using `Tools/MsbExeSurvey`. That file is the
same build as the Steam exe (identical section layout, `.data`, `.pdata`) with about 1,200 Arxan-encrypted functions
(~25 KB) decrypted. The Steam file has garbage at e.g. `0x140DE9390` and `0x140CB7980`, the dump has real code. Use
the dump for any further exe work. All addresses are VAs and match both files.

## Summary

* The exe reads **286 of 511** unknown leaf properties in SoulsFormats' MSBE (counted per entry type). The other 225 were
  never seen read. That makes them likely dead in retail, though the scan has gaps (see *Limits*). The first pass, on the
  Steam exe without stub expansion, found 247. The extra 39 are region `Unk4 +0Ch`, which is read for every region type.
* Several SoulsFormats layouts are wrong in ways that matter for naming:
  * **Part entity data `+0Ch..+19h` is six (override, value) pairs** on top of `PARTS_DRAW_PARAM_ST`. They are not twelve
    independent flags, and most of the current names are on the wrong byte (§2).
  * **`DisplayDataStruct` is six 8-int arrays, not Display[8] + Draw[8] + CollisionMask[32]** (§1). SoulsFormats'
    `DrawGroups` is really an *alternate* display-group set. The real draw groups sit at `+60h`, in what SoulsFormats
    reads as `CollisionMask[8..15]`.
  * Many `int`/`short` unknowns are read by the game as separate bytes (§6), and some offsets SoulsFormats treats as padding
    are read (§7).
* Asset `PartIndices[4]`/`[5]` (`+48h`/`+4Ch`) point at a **Collision** part. The loader forces them to `-2` on
  open-world tiles (§4).
* The loader fills version-gated defaults into SoundRegion and WeatherOverride type data (§5). That shows those bytes
  were added in later MSB versions.

## How it was done

`Tools/MsbExeSurvey` (C#, Iced). Run `flow <exe> <outDir> [iterations]` (14 is enough) and it writes:

| file | content |
|---|---|
| `report.tsv` | every MSB field the exe reads: kind, offset, size, SoulsFormats property, translation unit, functions, what the value is used for |
| `unknowns.tsv` | every `Unk*` property and whether it is `READ` |
| `getters.tsv` | the small MSB accessors, the fields they read, and their callers (RTTI class and referenced strings) |
| `mismatches.tsv` | reads that do not line up with a SoulsFormats property (`SPLIT`, `NO PROPERTY`) |
| `layout.tsv` | the offset of every SoulsFormats property, found by changing it and diffing the written MSB |
| `accesses.tsv` | the raw reads, one per instruction |

Current results are in `Tools/MsbExeSurvey/out_dump` (the dump, with stubs expanded). `out_steam` keeps the first pass on
the Steam exe, for comparison.

Other modes: `diff <exeA> <exeB>` (differing ranges per section), `stub <exe> <va>`, `disp <exe> <hex>` (every
instruction with that memory displacement, in both `.text` sections), `classes`, `vtable`, `strings`, `xref`, `dis`, `fdis` (whole function via `.pdata`), `idiom`,
`paramdef <xml>` (field offsets of a paramdef, so param row reads can be named).

What makes it work:

* **The loader turns offsets into pointers.** `MsbResCap` loading (`0x140CF92E0`, `..\..\Source\Res\Msb\MsbResCap.cpp`)
  rewrites every offset in every entry into an absolute pointer, in place. The game then reads a field as
  `[[entry+sub]+field]`, which can be followed statically.
* **Wrapper classes** (RTTI): `CSMsbParts` (+ `Ene`, `Geom`, `Hit`, `Map`, `Player`) and `CSMsbPoint` keep the raw
  entry at `+18h`. `CSMsbEvent` keeps it at `+10h`. All of them hold the `MsbResCap` at `+8h`. `MsbResCap` holds the
  param tables (`+80h` model, `+88h` event, `+90h` point, `+98h` part, `+A0h` layer, `+A8h` route; entries at
  `[table + i*8 + 10h]`) and per-type first-entry slots (`+D0h`/`+180h`/`+320h`/`+6A0h`/`+780h`/`+7F0h` + 16·type).
* A register dataflow pass labels these structs and carries the labels across calls (arguments, returns, owning-object
  fields). It rejects any function whose reads don't fit the struct (out-of-range offsets, or writes to read-only MSB
  data).
* **Arxan control-flow stubs are undone** (`Arxan.cs`). Many functions jump into the second `.text` (`0x144C16000+`),
  or to a stub in the main one. There the original instructions are mixed with tricks: push a constant address,
  choose one with `cmovcc`, then `jmp [rsp-8]`. A small emulator that tracks only the stack and constant registers
  recovers the original instructions plus the real `jcc`/`jmp`/`call`/`ret`, and the dataflow follows them inline. For
  example, `CSMsbPoint#7` (`0x140CFC450`) is `rcx = [rcx+48h]; if (rcx) goto 0x140D01870 else 0x1400CA5F7`. The
  `stub <exe> <va>` mode prints the expansion of a jump.
* **Type data is assigned by translation unit.** The code for each wrapper class is contiguous:
  `CSMsbPartsEne` 0x140CF1500–0x140CF22C0 (Enemy/DummyEnemy), `CSMsbPartsGeom` –0x140CF4D50 (Asset/DummyAsset),
  `CSMsbPartsHit` –0x140CF52F0 (Collision/ConnectCollision), `CSMsbPartsMap` –0x140CF5560 (MapPiece), `Player` after that.
  A per-function `cmp [part+0Ch], N` is *not* reliable, because asset code checks the type of a *referenced* collision
  part.

## 1. DisplayDataStruct (part `+50h`) is six group arrays

The loader (`0x140CF9B62..0x140CF9E3D`, for part types MapPiece/Enemy/Collision/Asset: bitmask `0x2023`) treats it as:

| offset | loader use | SoulsFormats today |
|---|---|---|
| `00h` [8] | display groups | `DisplayGroups` ✓ |
| `20h` [8] | **alternate** display groups | `DrawGroups` ✗ |
| `40h` [8] | mask: which display-group bits keep the `00h` value | `CollisionMask[0..7]` |
| `60h` [8] | **draw groups** | `CollisionMask[8..15]` |
| `80h` [8] | alternate draw groups | `CollisionMask[16..23]` |
| `A0h` [8] | mask for the draw groups | `CollisionMask[24..31]` |
| `C0h` u8 | gate for the replace/merge (`Condition1`) | |
| `C1h` u8 | "has alternate groups" (`Condition2`) | |

Logic, per array element:

* If the global at `0x140679A90` is true and `C1h` is set, `disp = alt` (straight copy of `20h..3Ch` into `00h..1Ch`).
* Else if `0x140679AB0` is false, `C1h` is set and `C0h` is clear:
  `disp = (disp & mask40) | (alt20 & ~mask40)` and `draw60 = (draw60 & maskA0) | (alt80 & ~maskA0)`.
* For **Collision** parts (type 5) only, afterwards: `draw60 |= disp00`. A collision always draws in the groups it
  displays.
* An editor/debug mode (`0x140E66000() == 12`, which always returns 0 in retail) would reset everything to all-on.

Both are checks on one byte of **`CS::GameMan`** (the singleton at `0x143D709A8`, built by `0x14067AB80` with vtable
`0x142A84210`):

* `0x140679A90` (an Arxan stub) returns `GameMan+B71h == 1`.
* `0x140679AB0` returns `GameMan+B71h == 0xFF`.

So the loader copies the alternate display groups when the byte is `1`, and merges them when it isn't `0xFF`. The byte
sits right after `new_game_plus_requested` (`+B70h`, named in fromsoftware-rs; `+B71h` is unnamed there). The
constructor zeroes it (`0x14067744C`), and no code stores to it directly, so its setter is hidden (through a pointer,
or a wider store). It is also compared to `0` at `0x1454706DC`. Watching that byte at runtime in a map known to use
`Condition2` would name it.

**Impact for Smithbox:** the map editor's draw-group view should use `+60h`, not `+20h`.

## 2. Part entity data (part `+60h`)

From the getters at `0x140CF0210..0x140CF08A0`. Each getter returns `value != 0` when `enable != 0`. Otherwise it
falls back to the part's draw param row, from virtual `CSMsbParts#3` (`0x140CF0B10`): `PartsDrawParam[PartsDrawParamID]`
when `isUsePartsDrawParamID` is set; Asset and Enemy override `#3`.

| bytes (enable, value) | SoulsFormats names today | param fallback (`PARTS_DRAW_PARAM_ST`) | default with no row | render flag bit |
|---|---|---|---|---|
| `0C`, `0D` | IsShadowSrc, IsStaticShadowSrc | `+48h isPointLightShadowSrc` | false | byte0 bit2 |
| `0E`, `0F` | IsCascade3ShadowSrc, UnkE0F | `+49h isDirLightShadowSrc` | true | byte0 bit1 |
| `10`, `11` | UnkE10, IsShadowDest | `+4Ah isShadowDst` | true | byte0 bit3 |
| `14`, `15` | DrawOnlyReflectCam, EnableOnAboveShadow | `+4Bh isShadowOnly` | false | byte0 bit4 |
| `16`, `17` | DisablePointLightEffect, UnkE17 | `+4Ch drawByReflectCam` | false | byte0 bit6 |
| `18`, `19` | UnkE18 (int, `18h..1Bh`) | `+4Dh drawOnlyReflectCam` | false | byte0 bit7 |

* `0C/0D` is also forced on by the debug toggle `Draw.IsEnableAllPartsShadowSrc` (`0x140E562A0`).
* The flags are packed in the draw setup at `0x1406DB0B7..0x1406DB130` (also built at `0x1404CF590` and
  `0x14071E030`).
* `+3Eh` **UnkE3E** (bool, getter `0x140CF07C0`) goes into the same draw setup as **byte2 bit0**. Its meaning is still
  unnamed.
* `+3Ch` **UnkE3C** (u16, default `0xFFFF`, getter `0x140CF0330`) is read by `0x1406C6111` and `0x14071AD80`.
* `+3Fh` DisableRTAO (getter `0x140CF07A0`) has six callers, including `ChrIns#29`.
* **Never read:** `0Ah` (IsPointLightShadowSrc, sbyte), `0Bh` (UnkE0B), `12h`, `13h` (IsShadowOnly, DrawByReflectCam),
  `1Ah`, `1Bh` (rest of UnkE18). These look vestigial in ER. The `0Ah`/`0Bh` and `12h`/`13h` pairs have no getter.
* `+00h` EntityID and `+1Ch` EntityGroupIDs (indexed getter `0x140CF0450`) are read as expected.

Suggested renames: `0C` `OverridePointLightShadowSrc` / `0D` `IsPointLightShadowSrc`, `0E` `OverrideDirLightShadowSrc` /
`0F` `IsDirLightShadowSrc`, `10`/`11` ShadowDst, `14`/`15` ShadowOnly, `16`/`17` DrawByReflectCam, `18`/`19`
DrawOnlyReflectCam, and split `UnkE18` into bytes.

## 3. Other shared part sub-structs (getters in `CSMsbParts`)

| field | read by | notes |
|---|---|---|
| Gparam `+0` LightSetID, `+4` FogParamID | `0x140CF08F0`, `0x140CF0350`, 7 callers each | the LightSet getter also checks the part type |
| Grass `+0`, `+0Ch`, `+18h`, `+1Ah` | `0x140CF03E0`, `0x140CEF870`, `0x140CF03C0`, `0x140CF03A0` | `+0` reaches `CSGrassMapPart_Geom#8`. **`GrassConfigStruct_Unk18`/`Unk1a` are read** next to it by `0x14071A680` and `0x140BA1020` (grass setup) |
| SceneGparam `+10h` TransitionTime, `+1Ch`, `+1Dh`, `+20h` | `CSMsbPartsHit` | **only read for collisions.** `Unk1C`, `Unk1D`, `Unk20` are sub-ID slots next to the base/override IDs. `Unk21` is never read |
| TileLoad `+0` MapID, `+4` Unk04 (byte), `+0Ch` Unk0C, `+10h` Unk10, `+14h` CullingHeightBehavior | 13–21 callers each | `Unk04` is read as a byte. `Unk0C` is read only by Geom (assets) |
| Unk8 `+0` | `0x140CF10E0`, 4 callers | read as a byte |
| Unk9 `+0` | Geom/Map getters | read as a byte, compared to 1/2/3. `Unk9 +4` is read by `MsbResCap` (no SoulsFormats property) |
| Unk11 `+0` (Hit), `+4` (Geom, Map) | getters | |
| DisplayGroupStruct `+0` Condition, `+4..+20h` groups | `MsbResCap`, Hit | groups copied out whole. **`Condition` is read as separate bytes `+0`, `+1`, `+2`** (not one int). `Unk24`/`Unk26` are never read |

## 4. Part type data

### Asset (`CSMsbPartsGeom`)

* **`PartIndices[4]` (`+48h`) / `[5]` (`+4Ch`): a Collision part.** The getters (`0x140CF2800` and others, 9 callers)
  take `[5]` if `>= 0`, else `[4]`. They look the index up in the part table, require part type 5, then read the
  collision's type data. For example, `+36h` DisableTorrent supports Asset `DisableTorrentAssetOnly`.
* The loader sets both to `-2` when the map's area byte is 50–88 (open-world tiles) and either is already set. It always
  sets `+54h` to `-1` (`0x140CF9B06`).
* Read unknowns: `UnkT00` (as byte `+1`), `UnkT02`, `UnkT10`, `UnkT24` (byte `+26h`), `UnkT28`, `UnkT30`, `UnkT34`
  (bytes `+36h`/`+37h`), `UnkT50`, `UnkT51` (4 callers), `UnkT53`, `UnkModelMaskAndAnimID`.
* Read where SoulsFormats has **no property**: `+08h` (qword, tested), `+13h` (byte getter `0x140CF3490`), `+52h`
  (byte getter `0x140CF34D0`, 3 callers).
* Nested structs: `AssetUnk1 +06h/+0Ch/+10h/+20h`, `AssetUnk2 +08h` and `AssetUnk3 +08h` are read with no property.
  `AssetUnk3 +0Eh/+0Fh` and `AssetUnk1 +00h` are read as bytes. `AssetUnk3 +24h`/`+25h` share a getter.

### Enemy (`CSMsbPartsEne`)

* **`UnkT28` (int) is four byte fields:** `+28h`, `+29h`, `+2Ah`, `+2Bh` each have their own getter (`0x140CF16A0`,
  `0x140CF16C0`, `0x140CF1660`, `0x140CF1680`), next to `ChrActivateCondParamID` (`+2Ch`, `0x140CF1640`).
* `UnkT15` (byte) has two getters. `+14h` (byte) and `+20h` (u16, default `-1`) are read and stored to `[obj+15Ch]` in
  `0x1403664F0`, but SoulsFormats has no property at either.
* The loader fixes up a pointer at `+78h`. That is a nested struct SoulsFormats should expose.

### Collision (`CSMsbPartsHit`)

* Read unknowns: `UnkT02`, `UnkT03`, `UnkT04`, `UnkT06/08` (`+8`), `UnkT14`, `UnkT1C`, `UnkT34`, `UnkT4E`.
* `UnkT4C` (short) is **two byte fields**, `+4Ch` and `+4Dh`, with separate getters.
* `+35h` GparamOverride and `+36h` DisableTorrent are confirmed. `+20h` PlayRegionID shares a getter with `+38h`.

## 5. Regions and events

* **SoundRegion (type 30):** for MSB version < `0x32`, the loader sets `+03h` UnkT03 to `0xFF` and `+04h`
  WwiseEnvPlaceTypeID to `-1`. For version < `0x34`, it sets `+08h` UnkT08 and `+0Ah` CommonPlaceType to `-1`. So those
  are "unset = -1" IDs added in later versions.
* **WeatherOverride (type 35):** for version < `0x42`, `+04h` is set to `-1` (SoulsFormats has no property there). For
  version < `0x4A`, `+08h`/`+09h`/`+0Ah` (UnkT08/09/0A) are set to `-1`.
* Region `+44h` MapStudioLayer is read (`CSMsbPoint` init, `0x140CFFB7C`). It is passed to a layer lookup, so the name is
  right. Region `+10h` (shape type) is compared to 5/6. Rotation is converted from degrees to radians at load.
* Region and event `Unk4`/`Unk3` `+0` MapID are read and compared to `-1`. `+4` (UnkS04) is read as a byte. `+0Ch`
  (UnkS0C) is read for both: the event getter also returns EventID, and the region getter is `0x140CFDFB0` (found
  only once stubs were expanded).
* Region header `+2Ch` RegionID is read (also found via stub expansion).
* The region "shape data" reads at `+20h`, `+81h`, `+82h` and `+86h` (`0x140CFEA70`, `0x140CFEA90`) are probably
  `CSMsbPoint +48h` (a wrapper object) mislabelled as the entry's shape pointer. Don't rely on them.
* Event type data: Generator/ObjAct/PseudoMultiplayer bytes `+3`, `+4`, `+5`, `+7` are read as single bytes. Several of
  these sit inside fields SoulsFormats treats as `short`/`int` (see `mismatches.tsv`). Each needs a per-type check.

## 6. Read narrower than the SoulsFormats property (`SPLIT`)

Entity `UnkE18`, TileLoad `Unk04`, Unk8 `Unk00`, Unk9 `Unk00`, Asset `UnkT00`/`UnkT24`/`UnkT34`/`AssetUnk1.Unk00`/
`AssetUnk2.Unk00`/`AssetUnk3.Unk00`/`AssetUnk3.Unk0E`, Enemy `UnkT28`, Collision `UnkT4C`, region/event
`Unk4/Unk3.UnkS04`. In most of these the game reads byte 0 only and the rest is padding. Splitting them makes the
editor show the real value range.

## 7. Read with no SoulsFormats property (`NO PROPERTY`)

Asset type `+08h`, `+13h`, `+52h`; Enemy type `+14h`, `+20h`; AssetUnk1 `+06h`, `+0Ch`, `+10h`, `+20h`;
AssetUnk2 `+08h`; AssetUnk3 `+08h`; Unk9 `+04h`; WeatherOverride `+04h`. SoulsFormats asserts these as zero or skips
them. They should become properties so values survive a round trip and can be edited.

(Region `Entity +0` and Event `Entity +4` also show up here. They are the private part/region index fields, so they are
fine.)

## Limits

* Coverage is incomplete. Arxan stubs are now followed, but a stub the emulator can't resolve (one with a real
  conditional branch, an indirect call, or an unknown target) is still a dead end. Many stubbed `CSMsbPoint` virtuals
  go through the wrapper's own `+48h` object rather than the MSB entry, so they add little. Region type data is the
  weakest area (1–7 of each type's unknowns confirmed). "Not read" means **not seen**, not proven unused.
* The hypothesis pass breaks ties in favour of the unit's own wrapper kind (`W.Point` in the `CSMsbPoint` range, and
  so on). Without that, stub expansion made some getters ambiguous.
* Values copied out whole (DisplayGroups, DispGroups) are counted as read, but what consumes the copy isn't traced.
* The flow labels can still be wrong in large "game" functions. Verify a row by looking at its listed function with
  `dis`/`fdis` before relying on it. Rows from the MSB translation units (`CSMsbParts*`, `CSMsbPoint`, `CSMsbEvent`,
  `MsbResCap`) are reliable.

## Applied to SoulsFormats (MSBE) and Smithbox

Renamed to the exe meaning; old ER names are not kept. Round trip (`Tools/MsbRoundTrip`, run from `src/Smithbox.Data`):
1,346 of 1,347 ER MSBs are byte-perfect before and after. `m61_51_45_00` already differed before (8 bytes longer at
`0xB3C0`) and is unchanged.

* **Part entity:** `PartsDrawParamID` is now `int` (it covers the old `IsPointLightShadowSrc` sbyte and `UnkE0B`).
  `0C..19h` are six `Override*` / value byte pairs (`OverridePointLightShadowSrc`/`IsPointLightShadowSrc`,
  `OverrideDirLightShadowSrc`/`IsDirLightShadowSrc`, `OverrideShadowDest`/`IsShadowDest`, `UnkE12`/`UnkE13`,
  `OverrideShadowOnly`/`IsShadowOnly`, `OverrideDrawByReflectCam`/`DrawByReflectCam`,
  `OverrideDrawOnlyReflectCam`/`DrawOnlyReflectCam`, `UnkE1A`/`UnkE1B`). Removed: `IsShadowSrc`, `IsStaticShadowSrc`,
  `IsCascade3ShadowSrc`, `UnkE0F`, `UnkE10`, `EnableOnAboveShadow`, `DisablePointLightEffect`, `UnkE17`, `UnkE18`.
* **DisplayDataStruct:** `DisplayGroups`, `AltDisplayGroups`, `AltDisplayGroupsMask`, `DrawGroups` (now `+60h`),
  `AltDrawGroups`, `AltDrawGroupsMask`, `NoAltGroupMerge` (was Condition1), `UseAltGroups` (was Condition2).
  `CollisionMask` is gone.
* **Splits** into bytes: TileLoad `Unk04..07`, `UnkStruct8_Unk00` (byte), `UnkStruct9_Unk00..03`, Asset
  `UnkT00/01`, `UnkT24..27`, `UnkT34..37`, `AssetStruct1_Unk00/01`, `AssetUnkStruct2_Unk00..03`,
  `AssetUnkStruct3_Unk00..03`, `AssetUnkStruct3_Unk0E/0F`, Enemy `UnkT28..2B`, Collision `UnkT4C/4D`, region/event
  `UnkS04..07`.
* **New properties** (were asserted constants, with the same defaults): `UnkStruct9_Unk04`, Asset `UnkT08` (long),
  `UnkT13`, `UnkT52`, `AssetStruct1_Unk06` (-1), `_Unk0C`, `_Unk10` (-1), `_Unk20`, `AssetUnkStruct2_Unk08` (-1),
  `AssetUnkStruct3_Unk08` (-1), Enemy `UnkT14`, WeatherOverride `UnkT04` (-1).
* Left as they are: `DisplayGroupStruct.Condition` (read both as an int and as bytes) and the event type-data bytes
  (they differ per event type and haven't been checked per type).
* Smithbox: the ER meta (`src/Smithbox.Data` and the Output copy) has entries for all of the above. Sun-shadow
  display (`MsbEntity.RefreshShadowFlags`, `GparamUniverse`) uses the override pairs for ER. The map editor's render
  groups now read the real draw groups, because `DrawGroups` is at `+60h`.

## Named from their callers (second pass)

Retail value counts come from `MsbRoundTrip <out> ER stats`. All renames still round-trip identically to the baseline.

* **Enemy `UnkT28..2B` → `ActivateTimeStartHour`, `ActivateTimeStartMinute`, `ActivateTimeEndHour`,
  `ActivateTimeEndMinute`.** `0x140438A80` stores `T28*60+T29` and `T2A*60+T2B` as start and end minutes of the in-game
  clock. The defaults come from the `ChrActivateConditionParam` row (`ChrActivateCondParamID`), whose
  `timeStartHour/Min` and `timeEndHour/Min` sit at `+2..+5`. The MSB values replace them when any of the four is
  non-zero. Retail: 34,618 are `00:00-00:00`; the rest are `20:00-05:00`, `20:00-05:30`, `19:30-04:30` (night-only
  enemies) or `06:00-19:59`.
* **Asset `UnkT13` → `ExcludeActivateRatioOverride_PS4`, `UnkT52` → `ExcludeActivateRatioOverride_XboxOne`.**
  `0x1406CAF30` picks the AssetGeometryParam's `excludeActivateRatio_Xboxone_Grid/Legacy` (`+11Ch`/`+11Dh`) or
  `_PS4_Grid/Legacy` (`+11Eh`/`+11Fh`) by platform. A non-zero MSB byte replaces the param value. `UnkT52` is only
  read for MSB version `>= 0x45`. Neither applies on PC; both are 0 in retail.
* **PlatoonInfo `State` (int) → `State`, `UnkT05`, `UnkT06`, `UseMemberEntityIDs` (bytes) plus a new
  `MemberEntityIDs[2]` at `+8`.**
  * `0x140353CA0` reads `+4` and `+5` as flags. It loops over `GroupPartsNames` and requires each part to be an Enemy.
  * When `+7` is set, it also adds the characters whose entity IDs are at `+8`/`+0Ch`. The value `10000` (the player)
    sets a flag on the platoon instead.
  * Retail: `State` 0/1, `UnkT05` 1 on two platoons, everything else 0.
* **PatrolInfo `+3` → `UseAllWalkRegions`** (default 1, always 1 in retail). The walk-region loop (`0x14036BB90`) reads
  all 64 region indices when it is set, otherwise only the first 24.
* **Asset `UnkT51`:** a 0/1/2 category (0 on 624,888 of 624,987 assets). 1 and 2 put the asset into separate instance
  lists of the geometry manager (`+528h`/`+548h`), checked next to `DisableRTAO`. Not named.
* **Entity `UnkE3E`:** becomes bit 0 of byte 2 of the part's render flags. Those flags are copied into
  `CS::CSGeomModelIns +150h` (`0x1409F0AA0`), and their readers are generic accessors. Not named.
* **Entity `UnkE3C`:** a `u16`, `-1` = none. It's copied into geometry setup structs (`+148h` in `0x1406C6111`, `+7Ch`
  in `0x14071AD80`) right after the PartsDrawParam flags are applied, under a condition on the map's block data. Not
  named.

* **`DisplayGroupStruct.Condition` stays an int.** The byte reads at `0x140CF890E` belong to a different struct.
  `0x140CF8880` walks `MsbResCap +468h/+470h`, which is the per-type slot for region type 21 (Connection), and reads
  `[region+58h]`, the region's type data. Its callers (`CSMsbPointConnectionRefe`) compare
  `TargetMapID[0..2]` with three bytes of a map ID. SoulsFormats already has `TargetMapID` as bytes. The survey had
  labelled that region as a part. The only real read of `Condition` is the Hit getter, which reads the whole int (1–201
  in byte 0 on collisions, -1 otherwise).

Retail patterns for the three unnamed fields (from `MsbRoundTrip ... stats`):
* `UnkE3E = 1` on 16,692 assets and 2,331 map pieces, concentrated in the LOD-overview maps (`_02`/`_12`: 12,074
  assets; `_00`: 2,531).
* `UnkE3C` is set on 143 parts, with values in clusters (400s, 800s, 900–999).
* `UnkT51` is non-zero only on 99 large `AEG010_*`-style assets in `m60_09_12_02` and `m61_11_11_02`/`_12`.

## Status

Static analysis is finished. UnkE3E, UnkE3C and Asset UnkT51 stay unnamed; their meta descriptions give the retail
patterns above. `MSBE Runtime Tests.md` has the in-game tests that would name them. They were not run (skipped,
2026-11-05).
