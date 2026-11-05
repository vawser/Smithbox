# MSBE consumer tracing plan

Goal: name the remaining MSBE (Elden Ring MSB) fields that `eldenring.exe` reads, by following each value from the MSB
into the runtime object it is copied to, and from there to every piece of code that reads it. The name comes from what
that code does with the value: what it is compared with, which function it is passed to, what arithmetic it goes
through.

This builds on `MSBE Exe Analysis.md` and `Tools/MsbExeSurvey`. Use `Documentation/Games/eldenring.exe`.

## Where things stand

| | fields |
|---|---|
| Unknown properties the exe reads (distinct struct/offset) | 90, of which about 25 are named by the passes so far |
| Unknown properties never seen read | 94 (region type data dominates; partly a coverage gap) |

`out_fixed/destinations.tsv` (new `Destinations` pass in `Report.cs`) lists, for every getter of an unnamed field, each
call site and where the returned value goes first: a store into an object, an argument to a call, or a compare.

## Runtime objects to trace

Grouped by the object (or setup function) the values land in. "→ +X" is the destination offset in that object.

| # | object / setup function | fields landing there | notes |
|---|---|---|---|
| A | Collision runtime object, filled by `0x1403F67A0` (object in r15) | Collision `UnkT02` → `+2E1`, `UnkT4C` → `+2E5`, `UnkT03` → `+2E6`, `UnkT1C` → `+2F0`, `UnkT38`/PlayRegion → `+2F4`; LocationTextID → `+2ECh` (known, use as a control) | naming these also names AssetUnk1/AssetUnk3 counterparts |
| B | Collision scene/gparam setup `0x14062B2A0` (stack struct) | SceneGparam `Unk1C`/`Unk20`, Collision `UnkT4D` (`rbp+6F`), `UnkT4E` (`rbp+77`), `UnkT06/08` (`rbp-3D`), `UnkT14`; GparamOverride (known control) | the stack struct is passed to a callee; follow it |
| C | Asset geometry setup `0x1406C6111`, `0x14071AD80`, `0x1406C4380`, `0x1406C23E1`, `0x1406C4E70`, `0x1406C5440`, `0x1406C5C30`, `0x1406C5EC0`, `0x1406C59E0` | `UnkE3C` (`+148`, `+7C`), Asset `UnkT50`, `UnkT01` (cmp 16), `UnkT02`, `UnkT08`, `UnkT10`, `UnkT28`, `UnkT30`, `UnkT34`, `UnkStruct8_Unk00`, `UnkStruct11_Unk04` (`r14+74`) | geometry instance creation; `CSWorldGeomInfo` is partly named in fromsoftware-rs |
| D | Draw-flag setup `0x1406DAAE0`, `0x14071E030` → `CS::CSGeomModelIns +150h` | `UnkE3E`, `UnkStruct9_Unk00` (compared to 1 and 2) | flags are read through accessors of the model instance |
| E | Grass `0x14071A680`, `0x140BA1020` (`CSGrassMapPart_Geom`) | `GrassConfigStruct_Unk18`, `_Unk1a` | grass parts have RTTI names |
| F | Tile loading: 13 callers of the TileLoad getters | `TileLoadConfig_Unk04` (byte), `_Unk0C` (`0x1406E1450`, `0x1406E2570`), `_Unk10` | many callers; pick the one that branches on the value |
| G | Region consumers: `0x1401B96B0` (Message), `0x140A70130`..`0x140A73090`, `0x140D08460`, `0x140D0925E`, `0x140B391D0` | region type data `+0`, `+2`, `+4`, `+8`, `+0C`, `+10`, `+14`, `+18`, `+20`, `+30`, `+34`; `Unk4 +4/+C` | the type is ambiguous per offset: decide it per function from the `CSMsbPoint` subclass |
| H | Platoon object `0x140353CA0` (rsi) | PlatoonInfo `UnkT05` → `+7CB` | `State` → `+7C9` is the control |
| I | Character: `CS::ChrIns#8` (`0x1403F26A0`), `0x14043B090`, `0x140131CC0` | Enemy `UnkT14` (compared to 2), `UnkT15` | fromsoftware-rs names much of `ChrIns` |
| J | Asset `UnkT51` | lists at `+528h`/`+548h` of the geometry manager | trace the lists' consumers |

## Tooling to add (MsbExeSurvey)

1. **Watched fields.** Let the flow pass record accesses to chosen (class, offset) pairs as well as MSB kinds. Today
   class-kind accesses are only used for learning, not reported. Input is a list such as
   `C:CS::CSGeomModelIns +152`. Output is every read, with its uses, as for MSB fields. This is the core of "trace the
   object's consumers".
2. **Object identity.** For a destination `[reg+off]` in a setup function, find the object's class:
   * the function's own RTTI class (`this`);
   * the vtable written at the allocation site that produced `reg` (as `0x1409F0AA0` shows `CSGeomModelIns`);
   * or a fromsoftware-rs struct with the same layout (`CSWorldGeomInfo`, `ChrIns` modules).

   If no class is found, give the object a synthetic kind (`S:<function>:<frame offset>` for a stack struct) and let
   the label flow into callees through the argument registers, the way MSB kinds do now.
3. **Anchor library.** Functions whose purpose is known, so "passed to X" becomes a meaning:
   * param row lookup: find the `SoloParamRepository` get-row function and map its constant index argument to the 127
     params in fromsoftware-rs `solo_param_repository.rs`. `PartsDrawParam` via `0x140D45E80` is already known.
   * event flags: the getter and setter of `CSEventFlagMan` / `CSFD4VirtualMemoryFlag` (fromsoftware-rs
     `event_flag.rs`).
   * text: FMG lookups (`MsgRepository`), with the FMG category argument.
   * the ~100 named RVAs in fromsoftware-rs `rva_ww.rs` (SpEffect apply/remove, bullet spawn, ray cast, menu
     messages…).
   * entity lookup: the map-entity registry calls that take an entity ID (`0x140CEFBE0`-style).
   * strings referenced in the consumer (format strings such as `a%03d`, `s%d`, debug names).
4. **Shape classifier.** Rules applied to each consumer, giving a candidate name, type and evidence (below).
5. **Value checks.** Extend `MsbRoundTrip ... stats` to test a candidate against retail data:
   * param IDs exist in that param in `regulation.bin`;
   * text IDs exist in that FMG;
   * flags are event flags used in EMEVD;
   * enums take only the compared values;
   * time and angle values fall in their ranges.

## Shape rules (what the code does → what to call it)

| shape | name pattern | example already found |
|---|---|---|
| `if (a) use b else use param/other field` | `Override<X>` + `<X>` | entity draw flags; AssetUnk1/3 vs Collision |
| `a*60 + b` compared with the game clock | `<X>Hour`, `<X>Minute` | Enemy ActivateTime |
| passed to the param row lookup with index N | `<ParamName>ID` | PartsDrawParamID |
| passed to the event flag getter | `<X>EventFlagID` (Enable/Disable from the branch) | EnableFastTravelEventFlagID |
| passed to an FMG lookup of category C | `<Category>TextID` | LocationTextID |
| looked up in the entity registry; `10000` special | `<X>EntityID` | PlatoonInfo MemberEntityIDs |
| used as a part/region index into the MSB tables | `<X>PartName` / `<X>RegionName` (index property) | Asset PartNames[4/5] |
| compared with a few constants and branched (switch) | `<X>Type` enum, values from the cases | UnkT51 (0/1/2) |
| `!= 0` only, result is a bool | `Is<X>` / `Enable<X>` / `Disable<X>` from the branch taken | DisableRTAO |
| `1 << v` or `bt` | `<X>Bit` / group index | — |
| loop bound or array length | `<X>Count` | PatrolInfo UseAllWalkRegions (24 vs 64) |
| float compared with a squared distance, or used with sqrt | `<X>Distance` / `<X>Radius` | — |
| multiplied by 0.0175 (deg→rad) | `<X>Angle` | region rotation |
| multiplied by `1/30`, or compared with the frame delta | `<X>Time` / `<X>Frames` | — |
| divided by 100 | `<X>Rate` / `<X>Percent` | — |
| used to select a platform/version field | `<X>_PS4` / `_XboxOne`, version notes | ExcludeActivateRatioOverride |
| read only when the MSB version ≥ N | note "MSB version N+" in the summary | UnkT52, AssetUnk3 +0C |
| copied straight into a named fromsoftware-rs field | that field's name | — |

A name needs at least one shape plus a retail value check that agrees. Where only a shape is known (say, a bool with
no named branch), the summary says what it does and the property keeps its `Unk` name.

## Work order

1. **Tooling 1–2** (watched fields, object identity), tested on known controls: LocationTextID `+2EC` (A) and
   `State` `+7C9` (H) must trace to FMG text and a platoon state branch.
2. **A (Collision runtime object)** first: five fields, and each one also names an Asset field.
3. **B, then C** (geometry setup; the biggest group). Use the anchor library heavily.
4. **G (regions):** first improve coverage. Seed every `CSMsbPoint` subclass's virtuals with its own region type, so
   type data is labelled per type, then rerun. This should also move many of the 94 "never seen read" fields.
5. **D, E, F, H, I, J** as smaller jobs.
6. After each group: rename in SoulsFormats (with summaries), update both meta copies, round-trip all ER MSBs (must
   equal `Tools/MsbRoundTrip/baseline.tsv`), rerun the survey, and record the evidence in `MSBE Exe Analysis.md`.

## Progress

### 2026-11-05: tooling 1–2 done (`Tools/MsbExeSurvey/Objects.cs`)

Two new modes, independent of `flow` (about 2 s each):

* `identity <exe> <fn> [arg]`: the fields `fn` touches through its argument (offset/size), its callers with
  their known classes, and the RTTI classes ranked by how many of those fields they also touch (fn's own accesses
  excluded).
* `watch <exe> <class> "<off,off..>" [fn:arg=Name ..]`: every access to those fields. Quote the offsets in
  PowerShell (it reads `2E1` as the number 20). The output has three parts:
  * **Attributed**: through `this` of the class's virtuals, carried into callees through the argument registers
    (4 levels; a callee called with more than 6 classes is dropped). Alias state is joined at forward branches.
  * **Unattributed** (`?R`/`?W`): the same displacement and size anywhere else, scored `n/m` = how many other
    fields of the class the same base register touches in that function. Sorted by score. Noisy for common
    offsets (`+2F0`), useful for bytes.
  * **Getter/setter hops**: a read that only returns lists the getter's callers and what they do with `rax`,
    recursively for 2 levels. A small function that stores an argument lists its callers and the constant passed.
* A seed `fn:arg=S:Name` gives an object with no RTTI class a synthetic kind.
* **Anchors (start of tooling 3)**: 651 script bindings (`lea rax,[fn]` … `lea r8,"Name"`, the
  `DLConcreteMethodInvoker` registrations for Lua/ESD functions) are shown as `fn<Name>`.

Saved runs: `out_fixed/watch_A.txt`, `out_fixed/watch_H.txt`.

### Controls

* **A, LocationTextID `+2EC`**: `identity 0x1403F67E0` gives **`CS::ChrIns`** (34/34 fields; callers are
  `ChrIns#10` and `CSRideNode#4`). So "the Collision runtime object" is the character: `1403F67E0` copies the
  data of the collision it stands on, or of an asset, into `ChrIns +2E0..+2F8`. The asset branch uses the
  `CSMsbPartsGeom` getters, the collision branch the `CSMsbPartsHit` ones. The read of `+2EC` is unattributed (the
  object reaches `14062B2A0`, a `CSEventWorldAreaTimeCtrl` step, through a field). There `-1` falls back to a text
  ID built from the map number (`(area*-100 - block)*10`), which is then compared with the cached `[rdi+E0h]`:
  location-name logic, as expected. **Pass.**
* **H, PlatoonInfo `State` `+7C9`**: the platoon object has no RTTI class (best match 5/19), so it is seeded as
  `S:Platoon`. Its setter `140353B40` is reached only from the Lua binding **`SetReflectState`**
  (`CS::CSAiPlatoonFunc`). Getter `140353560` feeds `140360230`, which is only called indirectly; the trail stops
  there. **Pass**, and it gives a name: `State` is the platoon's initial *reflect state*.

### Found on the way (group H)

* **PlatoonInfo `UnkT05` `+7CB`**: its setter `140353B10` is reached only from Lua **`SetEnableDeactiveMove`**.
  Readers in the platoon update (`140354DA0`, `1403556D0`) compare it with 0 / a bool. Candidate name
  `EnableDeactiveMove` (bool). Retail: 1 on two platoons. Not renamed yet.

### Group A first look (from `watch_A.txt`)

| field | ChrIns | readers seen (unattributed, best score) |
|---|---|---|
| Collision `UnkT02` | `+2E1` byte | `1403F3D10`: `cmp 255`, test; `140DB1D5B`: test, `sub 1`, `cmp 1` (enum, 255 = none) |
| Collision `UnkT4C` | `+2E5` byte | only `140ABB970`/`140ABB720` (probably another class: it writes `-1.0f` at `+2EC`) |
| Collision `UnkT03` | `+2E6` byte | `140436C30`, `140439696`, `140DB11D0` → stored to `[x+318h]`, arg1 of `140DC7300` |
| Collision `UnkT1C` | `+2F0` dword | too common an offset; needs the `ChrIns` field path instead |
| `UnkT38`/PlayRegion | `+2F4` dword | same |

Next for A: find the pointer path to `ChrIns` that `14062B2A0` uses (`r15`), add it as a field kind so those
readers become attributed, and follow `+318h` (`UnkT03`) and `140DC7300`.

### 2026-11-05: group A done

* Tooling: declared object paths, `G:<global>[+off]=Class` and `F:<class>+off=Class`. Functions that read
  the global are scanned from their real start (`Owner`), so the consumers of `ChrIns +2E0..` became attributed
  through the main player (`G:143D6D018+1E508=CS::PlayerIns`). New `raw` (linear disassembly) and `owner` modes.
* Renamed (SoulsFormats, both meta copies; round trip identical to the baseline; survey rerun):
  Collision `UnkT02` → `OutdoorIndoorType`, `UnkT03` (bool) → `EasyOcclusionID` (byte), `UnkT38` →
  `LegacyPlayRegionID`; AssetUnk4 `Unk00/02/03` → `OverrideCollisionSoundSettings` / `OutdoorIndoorType` /
  `EasyOcclusionID`. `UnkT1C` and `UnkT4C` keep their names, with summaries of what the code does.
* Evidence: `MSBE Exe Analysis.md`, "Collision values on the character". Stats: `out_fixed/stats_A.txt`.
* Correction to the table above: the asset counterparts of these fields are in **AssetUnk4**, not AssetUnk1/3.
  And `+2F4` comes from a version switch (`LegacyPlayRegionID` before parts version 0x33, PlayRegionID after).

### 2026-11-05: group H done

PlatoonInfo `UnkT05` → `EnableDeactiveMove` (byte, `IsBool` in meta); `State` keeps its name, its summary now
says it is the initial reflect state (Lua `SetReflectState`). Both meta copies updated; round trip identical to the
baseline; survey rerun. Evidence: `MSBE Exe Analysis.md`, "PlatoonInfo flags named from Lua bindings".

### 2026-11-05: group B done

The "stack struct" of `14062B2A0` is the locals of `14062C110`. Each Collision value overrides one per-map param
value (MapDefaultInfoParam, MapGdRegionInfoParam, MapGridCreateHeightDetailLimitInfo). Renamed: `UnkT06` →
`MapGridCreateHeightDetailLimitInfoID`, `UnkT14` → `PlayerLightScale`, `UnkT4D` →
`TimezonePlayerLightScaleOverride`, `UnkT4E` → `OpenChrActivateThreshold`, plus their asset copies. SceneGparam
`Unk1C/Unk20` and `GparamOverride` were not renamed: retail values disagree with what the code suggests
(mapRegionId; WeatherCreateAssetLimitId). Open question for later: the two share a value set (50, 55, 65, 75, 78),
so they probably index the same thing. Evidence: `MSBE Exe Analysis.md`, "Collision overrides of per-map params".

Anchor added: SoloParamRepository is `[143D88F78h]`, get-row `140D500F0(repo, index, …)`. The constant in `edx`
is the fromsoftware-rs index.

### 2026-11-05: group C done

Renamed: Asset `UnkT01` → `CollisionTypeOverride`, `UnkT30` → `PickUpItemLotParamID`, bytes `UnkT34`–`T37` →
`int BreakItemLotParamID` (merged). Eight more fields got summaries of the observed behaviour but keep `Unk` names;
most trails end inside the geometry manager's lists or the Wwise SDK. Evidence: `MSBE Exe Analysis.md`, "Asset
geometry setup". Survey caveat confirmed: Geom getters that fall back to the referenced collision make collision
offsets show up as Asset reads (`+35`–`+37`); check the getter before trusting an Asset row in `report.tsv`.

### 2026-11-05: groups D, E, F done

One rename: TileLoadConfig `_Unk04` → `NamePrefixLength` (the getter returns the part name past that many
characters; 13 = the `mAA_BB_CC_DD-` prefix on all 97,838 retail uses). Summaries for TileLoad `_Unk0C` (an index into
the source map's `CS::HitIns` list), `_Unk10`, UnkStruct9 `_Unk00` (render-flag bits) and the grass shorts.
Remaining open groups: G (regions, needs the coverage fix first), I, J.

### 2026-11-05: group G, first part

The planned coverage fix (seed `CSMsbPoint` subclass virtuals) doesn't fit how regions are read: only 9 of ~40 types
have a subclass, and type data is read by small (often indexed) getters whose type is fixed by the caller. Done:
GroupPlacementPoint, formerly GroupDefeatReward (`RandomAppearParamID`, `PlatoonEventNames[4]`, `UnkPartNames[8]`, replacing UnkT00/T04/T08/T34/T38
and eight asserted -1s) and a Message `UnkT02` summary. **Next:** attribute region getters through the 22 typed casts
(`mov edx,<type>; jmp 140CFE230`) and their callers, then work through the other region types the same way.
Remaining groups: rest of G, I, J.

### Known limits

* `Extra.FuncStart` merges chained `.pdata` fragments, so leaf functions and tail-called neighbours are reported
  under the wrong start. `Objects.Owner` corrects this for the watch output: the nearest call target / binding /
  virtual, or a 16-aligned instruction after a `ret`/`jmp` that no earlier branch targets.
* Objects reached through a field of another object (`[mgr+X]` → ChrIns) are not attributed yet; they appear only
  in the unattributed list.

## Stop rules

* Time-box each field to about three hops from the MSB read (getter → setup → first consumer → its consumer). If the
  trail goes through Arxan stubs the emulator can't resolve, or into generic containers, stop. Record the furthest
  point reached.
* A field read only to be copied into another unnamed field gets a cross-reference ("same as X"), not a guess.
* Don't rename on shape alone when retail data disagrees.

## Estimates

* Tooling 1–3: about a day of work in the survey tool.
* Groups A, B, H, I: quick once the tooling exists (a few fields each, mostly one hop to an anchor).
* Group C: the longest; geometry setup is large and passes through several layers.
* Group G: depends on the coverage fix. Expect region type fields to be the biggest remaining gain.

Some fields will stay unnamed: those whose consumers only feed rendering or physics internals with no named anchor.
For those, the summary will describe the observed behaviour (render flag bit, list selection, and so on).
