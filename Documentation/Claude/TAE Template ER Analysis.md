# Elden Ring TAE template: analysis against the game

Changes to `src/Smithbox.Data/Assets/TAE/TAE.Template.ER.xml`, and the evidence for each, from
`Documentation/Games/eldenring.exe` and every time act in the game.

## Method

The tool is `Tools/TaeTemplateSurvey`. Run it from `src/Smithbox.Data` for `dump`; the rest run anywhere.

| Mode | What it does |
|---|---|
| `dump ER <cache>` | Reads every `.anibnd` through the VFS and caches the raw events (928 time acts, 1,630,738 events, 124 types) |
| `report <cache> <template> <out>` | Value histograms per field, broken asserts, enum values not listed, ints that look like floats, data past the template |
| `exe <exe> <out> <cache>` | Finds every MSVC jump table in `.text` and scores it against the event types the game uses |
| `trace <exe> <dispatchers> <template> <cache> <out>` | Follows each event type through the dispatchers (see below) and records every read of its parameters |
| `compare <template> <other>` | Fields that a second template (ERNR) names or types differently |
| `roundtrip <cache> <template>` | Applies the template to every event with asserts on, and checks the bytes come back unchanged |
| `sizecheck <before> <after>` | Events whose size changed, and a parse with SoulsFormats' own reader |

### Finding the dispatcher

Three switches next to each other have cases that are exactly event IDs. The function at **`0x14042E780`** switches on
the event type (0–238, 302–522, 601–800, 904–911). A second one at **`0x14042F730`** handles a subset
(0, 1, 2, 5, 14, 64, 65, 112, 121, 123, 129, 130, 132–134, 137–139, 144, 145, 229). Both take `rdx` = an event context:

| Context offset | Meaning |
|---|---|
| `+00` | event type (s32) |
| `+08` | pointer to the parameters |
| `+18` | byte, checked by most handlers (likely begin vs. update) |
| `+1A` | u16 **version of the time act**. Handlers gate newer fields on it, e.g. 129 reads `+14/+18` only if `>= 0x17`, 2 reads `+10` only if `>= 0x25` |

ChrActionFlag (event 0) has its own switch on `FlagType` at `0x140427C54` (cases 0–142).

### Tracing

`ParamTrace` starts at a dispatcher with the type known. It folds compares on the type (signed and unsigned) and takes
only that type's switch cases. It follows the parameter pointer through moves, `lea`, stack spills, direct calls and tail
calls, using `.pdata` for function bounds. Every memory read through the pointer is logged with its width and how it's
read (`movss` → float, `movsx` → signed, `movzx`/`cmp byte` → unsigned or bool). Values loaded from a field are tagged
too, so the tracer logs which function and argument they reach and which constants they're compared to. When a named
field and an unnamed one reach the same function argument, the unnamed one has a candidate name.

A read that is `subss` then `addss` at `+N`, with a `movss` at `+N+4`, is a lerp from a start value to an end value over
the event. 601's `WeightAtEventStart/End` reads this way, so other fields with the same pattern are named like it.

## Changes

| Event | Field | Before | After | Evidence |
|---|---|---|---|---|
| 0 ChrActionFlag | FlagType enum | 125 entries | + 13, 43, 126, 127, 129, 130, 133, 136–138, 140–142 | Used by the game (e.g. 129 ×869, 142 ×683) and handled by the flag switch, except 13. 126/136 call the same toggle with `false`, 129/135 with `true`. 127/130 call the physics module (`chr+190→+68`) |
| 1 AttackBehavior | AttackType enum | 0,1,2,62,64 | + 3, 63, 65, 66 | The game compares against 1,2,3,62,63,65,66. 3 is used ×61. Names with `?` follow ERNR and aren't confirmed |
| 1 | DirectionType | u8 | s8 | `movsx` |
| 2 BulletBehavior | +10 Offset | s16 | s8 + u8 | `movsx byte`, read only if time act version ≥ 0x25 |
| 5 CommonBehavior | +08 | s32 assert 0 | b + 3 pad | read as a byte and compared to 1 |
| 112 SpawnFFX_FloorDetermined | DummyPolyID | s16 + s16 | s32 | read as a dword at two sites |
| 114/115 SpawnFFX_GoodsAndMagic(EX) | SFXIndexID enum | 0–2 | + 3, 4 | used by the game |
| 116 SpawnFFX_Throw, 119 | RepeatType (enum) | s32 enum | **SlotID** | `cmp dword [p+8],0 / jge`, then copied to slot 0 of the spawn struct, the same as SlotID in 96/120. Values are 1–5 and −1 |
| 119 | DummyPolyID | s16 + s16 | s32 | dword read |
| 119 | +0D | u8 assert 0 | u8 | the game reads it |
| 129 Wwise_PlaySound_BySlot | +13 | u8 | b | `cmp` / `setne` |
| 129 | +14, +18 | s32, s32 | **OverrideSoundType** (SoundType enum), **OverrideSoundID** | Gated on version ≥ 0x17. Passed to the same resolver (`0x140455620` / `0x140455560`) as SoundType/SoundID of 132–134. Compared to 8, 9 (material-determined types). Used in place of the main sound when it resolves |
| 152 CameraZoomOut | +00 | f32 | CameraDistance | ERNR name. Stored to the camera at `+58` |
| 155 SetLockCamParam_Target | LockCamParamID | u16 + 2×u8 assert 0 | s32 | dword read, the same as 150. One event has `FF FF` in the high bytes |
| 196 [DS3]DebugString… | +00..+1B | s64, s32×4, b | f32×6, u16 | `movss` ×6, `movzx word`. Size unchanged; not used in ER |
| 228 RagdollReviveTime | +00/+04 | ReviveTimerExtra/ReviveTimer | WeightAtEventStart/End | lerp pattern, and ERNR names it this way |
| 231 RequestMsgMapList | enum | | + 100, 1000 | used by the game |
| 233 ChangeChrDrawMask | Mask0–31 | u8 | s8 + enum (−1 No Change, 0 Off, 1 On) | the data is only 0, 1 and 255. ERNR has the same enum |
| 341 ChrSlotSys_341 | +00 | s32 assert 0 | s8 Offset + 3 pad | `movsx byte`. The assert broke 122 events. ERNR name |
| 344 havok_344 | 4×f32 | unnamed | Value1/Value2 AtEventStart/End | two lerps, passed together to `0x1403C8000` |
| 601 SetAdditiveAnim | +0C | s32 | **SpEffectID_Exist** | passed to the same SpEffect check (`0x1404FA7F0`) as SpEffectID_Exist of 607/609 |
| 602, 607, 609 | +04/+08 | f32 unnamed | WeightAtEventStart/End | lerp pattern, the same as 601. 602 `+08` reaches the same function as 601's WeightAtEventEnd |
| 604 [ExePatch]TestParam | +00..+08 | s32 ×3 | f32 ×3, +04/+08 ValueAtEventStart/End | `movss` and the lerp pattern. Data is 1.0 / 0.3 / 0.1 |
| 700 EnableTwistModifier | size | 0x34 | **0x30** | Every game event is 0x30, and the last read is the 16 byte `movups` at `+18`. The old template grew each event by 4 bytes |
| 700 | +15 | u8 assert 0 | s8 TwistPriority | read and copied with the modifier. Values are −1, 0, −2, 100. The assert broke **43,822** events. ERNR name |
| 700 | TargetType enum | 0–7 | + 8, 9 | used by the game |
| 903 Havok_ThrowUnk903 | +00 | s32 | f32 | `movss` ×6 |
| 910 Unk910 | Unk1/2/3 | | LockonDummyPolyID, LookatDummyPolyIDStart/End | ERNR names, and the data fits: `+00` is 220–226, the usual lock-on dummy polys |

All other event sizes are unchanged (checked with `sizecheck`).

## Verification done

* `roundtrip` with the **old** template: 43,945 events change when loaded and saved (the 700, 341 and 155 asserts
  overwrite real values).
* `roundtrip` with the **new** template: 0 of 1,630,738 events change.
* `report` with the new template: no broken asserts, no enum values missing, no ints that look like floats.
* SoulsFormats `TAE.Template.ReadXMLFile` reads all 153 events and builds their default bytes.

## Still unnamed

These are read by the game, but nothing found names them. The trace (`out/trace.txt`) has the reads, constants and sinks
for each.

* 2 `+12` (u8: 255, 0, 1, 2…; packed next to AttachmentType into the bullet spawn struct)
* 14: all of it (a jump table with six floats; `+04/+08/+14/+18` are passed together to `0x1403C5750`)
* 64 `+04 +05 +09 +0A +0C +0D`, 65 `+00 +09`
* 133 `+0D`, 134 `+08 +0C +10`
* 152 `+04 +08 +0C +11` (stored to the camera at `+5C`, `+64`, `+68`)
* 193 `+08`, 198 (copied whole into a model parameter modifier), 300 `+0C` (not read)
* 304 `+00`, 307 `+0C`, 310, 311, 339, 340, 342, 343, 511, 702, 706 `+04`, 714
* 717 `+00 +05`, 730, 731, 787 `+00..+0C` (stored to the ActionFlag module at `+224..+230`), 907, 911

Event names were left alone, as `TimeActEventResolver` matches on `FFX` and `PlaySound` in them.

## Follow-ups

* The same tool should work for Nightreign (`nightreign.exe`) and AC6 (`armoredcore6.exe`). Only the dispatcher
  addresses would differ, and `exe` mode finds them.
* Several fields could get names by reading the functions they reach (listed in the trace).
