# FXR Format Plan (C# read/write)

Source analysed: `Reference/fxr-main` (EvenTorset's `@cccode/fxr` JS library, DS3 / SDT / ER / AC6 / NR).
Existing Smithbox code: `src/Andre/SoulsFormats/SoulsFormats/Formats/FXR3.cs` (old TKGP-era structural reader, not used anywhere in `src`).

---

## 1. What fxr-main contains

| Part | Location | Role for us |
|---|---|---|
| Binary read/write | `src/fxr.ts` 6374-7570, 9634-10650 | The format spec. Port to C#. |
| Action definitions | `src/actions/*.yml` (80 files) | Per-game field/property layout of every action type. Turn into data. |
| Action slots | `src/action_slots.yml`, `ConfigActionSlots` in fxr.ts 6174 | Which action types go in which config slot. Validation only. |
| Enums | `src/enums/*.yml` (17) | BlendMode, AttachMode, EmitterShape, … Labels for int fields. |
| Hand-written conversions | `ActionDataConversion`, fxr.ts 9162-9634 | 23 actions with read/write transforms. Port by hand. |
| Generated code | `ActionData` (2257-6130) and 80 `DataAction` classes (13707-33009) | Produced by `build/build.js` from the YAML. Do not port; regenerate from YAML. |
| Editing helpers | scale, recolor, Recolor palettes, interpolation | Out of scope for format work. |

About 30k of the 37k lines in `fxr.ts` are generated or editing helpers. The binary part is roughly 1.5k lines.

---

## 2. Binary format

All little endian. Offsets are absolute file offsets stored as 64-bit (the reader subtracts `memOffset`, so files dumped from memory also load). Counts are int32.

### 2.1 Header

```
0x00 char[4] "FXR\0"
0x04 i16     minor        0, or 1 for Nightreign
0x06 i16     version      4 = DS3, 5 = SDT/ER/AC6/NR
0x08 i32     1
0x0C i32     id           FXR id, 1..999,999,999, must match filename
0x10 i32     stateListOffset
0x14 i32     1            state machine count
0x18 i32 statesOffset,     stateCount
0x20 i32 conditionOffset,  conditionCount
0x28 i32 nodeOffset,       nodeCount
0x30 i32 configOffset,     configCount
0x38 i32 actionOffset,     actionCount
0x40 i32 propertyOffset,   propertyCount        (action properties)
0x48 i32 modifierOffset,   modifierCount        ("section 8")
0x50 i32 modPropOffset,    modPropCount         ("section 9": properties owned by modifiers)
0x58 i32 section10Offset,  section10Count       (int lists owned by actions)
0x60 i32 fieldOffset,      fieldCount           (shared 4-byte field pool)
0x68 i32 1, i32 0
-- version 5 only --
0x70 i32 sfxRefOffset,  sfxRefCount             sorted unique ids from Proxy nodes
0x78 i32 extVal1Offset, extVal1Count            sorted unique external values used
0x80 i32 extVal2Offset, extVal2Count            ExternalValue2 modifier values
0x88 i32 0, i32 0                               "unk empty" list, always empty
```

The header lists are lookup tables. fxr-main rebuilds them on write (`getReferences`, fxr.ts 10714). Rule for extVal2: an ExternalValue2 modifier value also goes into extVal1, except 10000 (blood visibility).

### 2.2 Records

| Record | Size | Layout |
|---|---|---|
| StateList | 0x10 | `i32 0, i32 count, i64 statesPtr` |
| State | 0x10 | `i32 0, i32 conditionCount, i64 conditionsOffset` |
| StateCondition | 0x60 | `i16 bits (op & 3, unk1 << 2), u8 0, u8 1, i32 0, i32 targetState, i32 0`, then two operand blocks of 0x28 each: `i16 operandType (-4 literal, -3 external, -2 unk, -1 stateTime), u8 0, u8 1, i32 0, i32 hasValue, i32 0, i64 valueOffset, 4 × i32 0` |
| Node | 0x30 | `i16 type, u8 0, u8 1, i32 0, i32 configCount, i32 actionCount, i32 childCount, i32 0, i64 configs, i64 actions, i64 children` |
| Config | 0x20 | `i16 type, u8 0, u8 1, i32 0, i32 0, i32 actionCount, i32 0, i32 0, i64 actions` |
| Action | 0x40 | `i16 type, u8 unk02, u8 unk03, i32 unk04, i32 fields1Count, i32 section10Count, i32 props1Count, i32 fields2Count, i32 0, i32 props2Count, i64 fields, i64 section10s, i64 properties, i64 unk (0)` |
| Property (action) | 0x28 | `i16 typeEnumA, u8 0, u8 1, i32 typeEnumB, i32 fieldCount, i32 0, i64 fields, i64 modifiers, i32 modifierCount, i32 0` |
| Property (modifier) | 0x18 | Same without the modifier tail |
| Modifier | 0x20 | `u16 typeEnumA, u8 0, u8 1, i32 typeEnumB, i32 fieldCount, i32 propCount, i64 fields, i64 properties` |
| Section10 | 0x10 | `i64 offset, i32 count, i32 0` → `count × i32` in the field pool |
| Field | 4 | int32 / float32 / bool (0/1). Vectors are consecutive floats. Fields carry no type tag. |

Field counts in actions count **4-byte words**, not logical fields. A vec3 counts 3.

### 2.3 Enum packing

- Property `typeEnumA = valueType | function << 4 | loop << 12`
  - valueType: 0 scalar, 1 vec2, 2 vec3, 3 vec4
  - function: 0 Zero, 1 One, 2 Constant, 3 Stepped, 4 Linear, 5 Bezier, 6 Hermite, 7 ComponentHermite (AC6 only)
- Property `typeEnumB = valueType + (function << 2) + (loop << 4)` for DS3/SDT/ER. This is **addition, not OR**: vanilla looped Linear scalar stores 0x20, but fxr-main's `|` gives 0x10 (Phase 0, §8). NR always stores 0. AC6 stores 0 for most properties and the packed value for about 5.7k; keep it raw.
- Modifier `typeEnumA = ((type >> 4) | 0b1100) << 12 | (type & 0xF) << 4 | valueType`. Types: 21 RandomDelta, 24 RandomRange, 38 ExternalValue1, 39 ExternalValue2, 53 RandomFraction.
- Modifier `typeEnumB = perGameTable[type] | valueType`. Tables differ by game:
  - DS3: RandomDelta 0, ExternalValue1 4, RandomFraction 8
  - SDT: RandomDelta 0, RandomRange 4, ExternalValue1 8, RandomFraction 12
  - ER/AC6/NR: RandomDelta 0, RandomRange 4, ExternalValue1 8, ExternalValue2 12, RandomFraction 16

### 2.4 Property field layouts (c = valueType + 1)

| Function | Fields |
|---|---|
| Zero / One | none |
| Constant | `c` floats |
| Stepped / Linear | `int n, min[c], max[c], pos[n], value[n·c]` |
| Bezier | Linear, then `p1[n·c], p2[n·c]` |
| Hermite | Linear, then `t1[n·c], t2[n·c]` |
| ComponentHermite | `float duration, int n[c], min[c], max[c]`, then per component `pos[n], value[n], t1[n], t2[n]` |

Modifier fields: RandomDelta `seed int[c], max float[c]`. RandomRange `seed int[c], min[c], max[c]`. RandomFraction `seed int[c], max[c]`. ExternalValue1/2 `int externalValue` plus one modifier property (the factor curve).

### 2.5 Write order (needed for byte-identical output)

Sections in this order, each padded to 16: header → StateList → States → Conditions → Nodes → Configs → Actions → Properties → Modifiers → ModProps → Section10s → Fields → (v5) sfxRefs, extVal1, extVal2.

> **Corrected in Phase 1 (§9).** The order below is the one verified against all 36,723 vanilla files. fxr-main's order (the first version of this section) is wrong for configs, actions, section10s and the field pool.

- **Nodes**: root first, then each node's children as one contiguous block, recursing block by block.
- **Configs and Actions**: depth-first *pre-order* of the node tree, which differs from the Nodes section order. Per node: its own actions, then each config's actions.
- **Properties and Modifiers**: follow the action order. ModProps follow the modifier order.
- **Section10 records**: *field order*: per node in pre-order, each config's actions first, then the node's own actions.
- **Field pool**: condition values, then for each action in field order: its fields (fields1 + fields2), its properties' fields (each property followed by its modifiers' fields and their properties' fields), then its section10 values. NR puts the section10 values *before* the properties' fields.
- **Empty lists**: see §9.2.


### 2.6 Node, config and action semantics

- Node types (`enums/NodeType.yml`): Root 2000, Proxy 2001, LevelsOfDetail 2002, Basic 2200, NodeEmitter 2202.
  - Root: 0 configs, 3 actions (DS3/SDT) or 4 actions (ER+).
  - Proxy: one action 132 (SFXReference).
  - Other nodes: one action 199 (StateConfigMap, which config to use per state).
- Config types: LevelsOfDetail 2002 (one action 128), Basic 2200 (≤ 15 slots), NodeEmitter 2202 (≤ 10 slots).
- Action layout per game comes from the YAML `games:` block. Each game has five ordered name lists: `fields1`, `fields2`, `properties1`, `properties2`, `section10s`. A game entry can be an alias (`AC6: ER`), an alias per list (`properties1: DS3`), or `-2` (unsupported, use the fallback conversion). Field types are `bool`, `int`, `float`, `vec3`, `vec4`, and can differ per game (`field: {DS3: int, ER: float}`). The same property can be a field in one game and a property in another (PointSprite `texture`).
- **Read fallback**: fxr-main reads an action with the schema only when every count is ≤ the schema's count for the game. Otherwise it keeps a raw generic action. We need the same rule.

### 2.7 Game detection (fxr-main heuristic)

- minor == 1 → NR
- version == 4 → DS3
- Otherwise read actions and look for markers: 10014 → AC6; 10500 with props1Count == 0 → SDT; 128 with 6 fields → AC6; 603/604/605/10012 field2 counts 46/47/39/42 → AC6; LensFlare and RichModel field counts.
- Anything else defaults to ER.

This is reliable for DS3/SDT/NR and weak for ER vs AC6. Smithbox always knows the project game, so pass it in. Keep the heuristic only for loose files.

### 2.8 Quirks to keep

- DS3 Tracer (10012) sometimes has an extra int `-2` at the start of fields2. Skip it when reading; keep it in the raw layer.
- State conditions with operand type -2/-1 can have a stray value written by old tools. fxr-main drops it with a warning. The raw layer should keep it.
- Generic field typing (no schema): fxr-main guesses float when `1e-4 ≤ |f| < 1e6`, otherwise int. Only use this guess for display. Store raw bits.

---

## 3. What fxr-main changes when it reads (and why our core must not)

The JS object model is meant for authoring, so reading a file and writing it back does not give the same bytes:

1. `StaticNodeTransform` / `RandomNodeTransform` / `PointLight` / … negate X (handedness) and change other values in `ActionDataConversion`.
2. State condition operands are sorted (`sortOperands`). GreaterThan is stored by swapping operands.
3. Root nodes get default actions (`SimulateTermination`) and BasicConfig slots get default actions filled in on write.
4. DS3 writes divide state-time literals by the root rate-of-time.
5. ComponentHermite is combined into Hermite for non-AC6 games.
6. Header reference lists are recomputed. Action `unk02/unk03/unk04` and the unknown offset are written as 0.
7. Floats can be rounded (`round` option).

**Conclusion**: Smithbox needs a **lossless structural layer** as the base. It should round-trip vanilla files byte for byte. Named/typed access and conversions go **on top** as views, and are applied only to what the user edits.

---

## 4. Proposed C# design

### Layer 1: structural (lossless)

Namespace `SoulsFormats`, folder `Formats/FXR/`, class name to be decided (§7).

```
FXR : SoulsFile<FXR>
  FxrGame GameHint; short Minor; FXRVersion Version; int Id
  List<FxrState> States
  FxrNode Root
  List<int> SfxReferences, ExternalValues1, ExternalValues2   // kept as read; RebuildReferences() helper
FxrState      { List<FxrStateCondition> Conditions }
FxrStateCondition { byte Operator, Unk1; int TargetState; FxrOperand Left, Right }
FxrOperand    { short Type; bool HasValue; uint RawValue }
FxrNode       { short Type; List<FxrConfig> Configs; List<FxrAction> Actions; List<FxrNode> Children }
FxrConfig     { short Type; List<FxrAction> Actions }
FxrAction     { short Type; byte Unk02, Unk03; int Unk04; long UnkOffset;
                List<uint> Fields1, Fields2;            // raw words
                List<FxrProperty> Properties1, Properties2;
                List<List<int>> Section10s }
FxrProperty   { ValueType; PropertyFunction; bool Loop; int TypeEnumBRaw;
                List<uint> Fields; List<FxrModifier> Modifiers }
FxrModifier   { ushort TypeEnumA; int TypeEnumB; List<uint> Fields; List<FxrProperty> Properties }
```

- Fields are stored as raw `uint`, with `AsInt/AsFloat/AsBool` helpers. This avoids any int/float guessing in the core.
- Writer uses `BinaryWriterEx.Reserve/Fill` in the order in §2.5. It does not share offsets. If the survey finds vanilla offset sharing, add dedupe.
- Read is recursive with `StepIn/StepOut` (already in `BinaryReaderEx`).
- Typed property helpers: `FxrProperty.AsKeyframes()` decodes and encodes the §2.4 layouts so editors can show curves.

### Layer 2: schema (named, typed access)

- **Tools/FxrSchemaGen** (C#, YamlDotNet in the tool only): reads `actions/*.yml`, `enums/*.yml`, `action_slots.yml`. Resolves aliases per game and writes one JSON file, e.g. `src/Smithbox.Data/Assets/FXR/fxr_schema.json`:
  ```
  actions[type] = { name, slot, isAppearance, isParticle,
    props: { name: { field?: type|perGame, default, enum?, resource?, textureType?, argument?, desc(en-US) } },
    games: { DS3: { fields1:[...], fields2:[...], properties1:[...], properties2:[...], section10s:[...] }, ... } }
  enums, configSlots
  ```
- **FxrSchema** runtime class: loads the JSON and gives `FxrActionView(action, game)` with `Get/Set` by name. The name resolves to (list, index, field type, word offset). It also gives `Fits(action, game)`, the §2.6 fallback rule, which decides whether the named view or a raw grid is shown.
- Data-driven, like param meta. When fxr-main updates, rerun the generator and nothing else changes in code.

### Layer 3: conversions and helpers (later, opt-in)

- Port the 23 `ActionDataConversion` entries as `IFxrActionConversion` (read/write value transforms, DS3 fallbacks). Apply them only in the named view, so display values match fxr-main docs. Raw bytes stay untouched until a value is edited.
- `RebuildReferences()` (port of `getReferences`), `GetResources()` (textures/models/anibnd/sounds, using `resource` tags from the schema), slot validation against `configSlots`.
- Optional: fxrjson-compatible JSON import/export, and cross-game conversion. Both are large; only build them if an editor needs them.

---

## 5. Verification tooling

**Tools/FxrRoundTrip** (C#, references Andre.IO / Andre.Formats / SoulsFormats like `Tools/GparamRoundTrip`):

1. Enumerate `sfx/*.ffxbnd*` for DS3, SDT, ER, AC6, NR. Use `ArchiveBinderVirtualFileSystem` for ER/NR/AC6, as GparamRoundTrip does. Check the exact bnd names during this step.
2. For every `.fxr` in each binder: read → write → byte compare. Report the first differing offset and the section it falls in.
3. Survey counters (results go in a `report.md`):
   - Are `unk02/unk03/unk04` and the action unknown offset ever non-zero?
   - Is `typeEnumB` always 0 on AC6/NR?
   - Do the stored header reference lists equal `RebuildReferences()` output?
   - Any offset sharing or out-of-order sections?
   - DS3 Tracer `-2` occurrences.
   - Schema coverage: action types per game that fail `Fits()`, and unknown action types.
   - ComponentHermite usage outside AC6.
4. Named-view check (C# only): for every converted action, apply the display conversion and then its reverse, and assert the raw words come back unchanged.

---

## 6. Phases

| Phase | Work | Done when |
|---|---|---|
| 0 | FxrRoundTrip skeleton + survey on existing `FXR3` — **done**, see §8 | Report lists failures per game |
| 1 | Layer 1 reader/writer — **done**, see §9 | 100 % byte round trip on all five games (or every exception explained) |
| 2 | FxrSchemaGen + `fxr_schema.json` + `FxrSchema`/`FxrActionView` | Coverage report: every vanilla action of a supported type `Fits()` its game's schema |
| 3 | Keyframe codec, conversions, `RebuildReferences`, resources, slot validation | Edit-then-save round trip (change a value via the view, reload, value persists, other bytes unchanged) |
| 4 | (Optional) JSON interop / cross-game conversion | Matches fxrjson on samples |
| 5 | Editor integration (separate plan) | — |

---

## 7. Open decisions

1. ~~Placement and the old `FXR3` class.~~ **Decided:** new `FXR` class in `Formats/FXR/`. FXR3 moves to `.archive` once Phase 1 passes.
2. ~~Schema form.~~ **Decided:** JSON data (`fxr_schema.json`) generated by Tools/FxrSchemaGen and loaded at runtime by `FxrSchema`.
3. ~~Conversions in named view.~~ **Decided:** the named view shows fxr-main's converted values (e.g. negated X). The conversion runs only in the view; raw bytes stay unchanged until a value is edited.
4. ~~Node oracle.~~ **Decided:** no. All verification is in C# (Tools/FxrRoundTrip). The optional fxrjson step in §5 and the fxrjson check in Phase 4 are dropped.

---

## 8. Phase 0 results (2026-11-09)

Tool: `Tools/FxrRoundTrip`. Run it from `src/Smithbox.Data`: `dotnet ../../Tools/FxrRoundTrip/bin/Release/net10.0/FxrRoundTrip.dll <outDir> [GAME...]`. Full output is in `Tools/FxrRoundTrip/out/report.txt`.
For each game it reads every `.fxr` in `/sfx/*.ffxbnd.dcx` (through the VFS for ER/AC6/NR). Each file gets an FXR3 read → write → byte compare, plus a raw walker that doesn't use FXR3.

### 8.1 Corpus

| Game | ffxbnds | .fxr | Header | Max node depth | Action types |
|---|---|---|---|---|---|
| DS3 | 42 | 5,194 | minor 0, v4 | 8 | 61 |
| SDT | 8 | 2,913 | minor 0, v5 | 9 | 64 |
| ER | 220 | 12,155 | minor 0, v5 | 13 | 71 |
| AC6 | 48 | 5,279 | minor 0, v5 | 19 | 74 |
| NR | 197 | 11,182 | minor 1, v5 | 15 | — |

### 8.2 FXR3 (existing class)

**0 of 36,723 files are byte-perfect.**
- NR: every read fails (it asserts minor == 0).
- Other games: the first difference is in the Actions, Nodes or Configs sections. In ER/AC6 some files also come out 16 bytes shorter (first diff at 0x64, the field count).
- FXR3 can't be fixed in place; this confirms the decision to replace it.

### 8.3 Format facts (these change the Phase 1 design)

1. **Action `unk02/unk03` is not padding.** It is `0/1` in most actions and `1/0` in about 6 %; AC6 has 28 cases of `0/0`. fxr-main writes `0/0`. Store it raw.
2. **Action `unk04` is a small int that depends on the action type** in DS3/SDT/ER, e.g. 105 → 5, 106 → 6, 399 → 2, 404 → 4, 501 → 1. It is rare in AC6 and never set in NR. Store it raw.
3. **Empty lists still point at a real offset in DS3/SDT/ER.** In those games, about 85 % of actions with no fields, properties or section10s store a nonzero offset (probably the write position), and properties with no modifiers always do. AC6 mostly stores 0; NR always stores 0. fxr-main always writes 0. The Phase 1 writer needs a per-game rule; check in Phase 1 that the offset equals the position the block would be written at.
4. **Identical field content is not deduplicated.** In every game, most duplicate field blocks are written again rather than shared.
5. **ER/AC6/NR sometimes share field blocks.** About 13k properties per game reuse an earlier property's field offset with the same length, and about 27k with a *different* length (a prefix, e.g. a scalar `1` inside a vec4 `[1,1,1,1]`). A few hundred modifier blocks do the same. DS3/SDT never share. Phase 1 needs to find the rule, or record the aliasing on read and write it out again while the property is unchanged.
6. **`typeEnumB` packing is addition** (§2.3). There are about 3k looped properties per game where fxr-main's OR gives a different value.
7. **Modifier `typeEnumB` in AC6 is mixed:** 0 for most, the ER table value (4/8/16) for a few hundred. DS3/SDT/ER always match the per-game tables in §2.3. NR is all 0, like its property typeEnumB.
8. **The header reference lists can be rebuilt on save**, with one exception. SDT/ER/AC6 match the rebuilt lists exactly in every file. In 10 NR files, ext-value list 1 contains 10000 where the rule leaves it out. Keep the lists as read and rebuild only after an edit.
9. No stray condition operand values, no shared action/property records, the state machine count is always 1, and memOffset is always 0. The DS3 Tracer `-2` quirk appears 219 times.
10. Root nodes don't always have 3 or 4 actions. Every game has some roots with 0 actions; ER also has roots with 3 and 6, and AC6 has roots with 11. Read these as generic nodes.
11. ComponentHermite (function 7) appears only in AC6: about 35k properties.

### 8.4 Consequences for Phase 1

- The Layer 1 records get `Unk02`, `Unk03`, `Unk04` and `TypeEnumBRaw` (property and modifier) as stored fields, not computed.
- The writer needs a game profile: how empty offsets are written, how typeEnumB is written, and how field blocks are shared. Start with DS3/SDT (no sharing) to get the section order exactly right, then add the ER+ sharing.
- A first Phase 1 step: dump a few ER files that share field blocks and work out the pattern before writing the ER writer.

---

## 9. Phase 1 results (2026-11-09)

Code: `src/Andre/SoulsFormats/SoulsFormats/Formats/FXR/` (`FXR.cs` model, `FXR.Read.cs`, `FXR.Write.cs`). FXR3 moved to `.archive/FXR3.cs`; nothing used it.
Verified with `Tools/FxrRoundTrip --new --edit` (run from `src/Smithbox.Data`).

| Game | .fxr | Byte-perfect | Edit check |
|---|---|---|---|
| DS3 | 5,194 | 5,194 | 5,194 |
| SDT | 2,913 | 2,913 | 2,913 |
| ER | 12,155 | 12,155 | 12,155 |
| AC6 | 5,279 | 5,279 | 5,279 |
| NR | 11,182 | 11,182 | 11,182 |

What the edit check does: it removes the modifiers of every other property and gives every Constant property its own value. Then it writes, re-reads and checks that each property reads back its own value and modifier count. This exercises the share, alias and reserved logic below, which must step aside once data is edited.

### 9.1 Model

- `FXR`: `Minor`, `Version`, `Id`, `States`, `Root`, the three v5 header lists (kept as read), and `ZeroEmptyOffsets` (per-file empty-list style, §9.2).
- `FxrState`, `FxrStateCondition`, `FxrOperand` (raw value word).
- `FxrNode`, `FxrConfig`.
- `FxrAction`: `Unk02/03/04` and raw `Fields1/Fields2` words.
- `FxrProperty`: value type, function, loop, raw `TypeEnumB`, raw fields, modifiers.
- `FxrModifier`: raw `TypeEnumA/B`, fields, properties.
- Everything is raw words; typed access is Layer 2.

The writer is two-pass: Layout gives every record and field block its offset, then Emit fills one byte array.

### 9.2 Writer rules found against vanilla

1. **Empty lists.** For each list kind, the file stores either 0 or a position. The reader picks the style per file from a majority vote (`ZeroEmptyOffsets`). Node-level actions (`unk03 == 0`, e.g. 199 StateConfigMap and 132 SFXReference) always store 0.
2. **Empty condition operand** (AC6/NR): points at the other operand's value in the same condition.
3. **Shared field blocks** (ER/AC6/NR). A block's record can point inside an *earlier* block when both came from the same source object; for example, action 129's LOD property shares 128's.
   - The block's own slot is still reserved and written. Its slot contents can differ from what the pointer reads, so the reader keeps them raw (`FxrFieldShare.Copy`).
   - The writer keeps the share only while the contents still match.
   - The header field count counts each block's words, including shared ones.
4. **Shared modifier records**: two properties can point at the same modifier records. The reader reuses the same `FxrModifier` objects, and the writer lays them out once.
5. **Left-over pointers** from source objects that were later simplified:
   - A Zero/One property can still have counted words reserved after it (`_reserved`). If no empty property is pending, gap words go on the next block (`GapBefore`) or the pool tail.
   - A property with no modifiers can still point at another property's modifier list (`_modsAlias`), or, when it shares fields, at its owner's modifier pointer.
6. **NR order**: an action's section10 values come before its properties' fields (`S10ValuesBeforeProperties`, i.e. `Minor == 1`).

### 9.3 Open items for later phases

- `ZeroEmptyOffsets` defaults to `All` for a new `FXR()`. Phase 3 should pick a default per game (AC6/NR store 0; DS3/SDT/ER store positions for most list kinds).
- The share/alias/reserved data is internal. It disappears on edit, which keeps edits correct but means an edited file is not laid out the way FromSoftware's tools would. Whether the game loads edited files correctly is **not yet tested in-game**.
- `FXR.Read` does no game detection; `Minor`/`Version` are enough for the writer. Layer 2 still needs the game (from the project) to choose the schema.
