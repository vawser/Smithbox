# Nightreign TAE template: analysis against the game

Changes to `src/Smithbox.Data/Assets/TAE/TAE.Template.ERNR.xml`, checked against `Documentation/Games/nightreign.exe`
and every NR time act. This uses the method in [TAE Template ER Analysis.md](TAE%20Template%20ER%20Analysis.md) and
the tools in `Tools/TaeTemplateSurvey`.

## Nightreign's exe

| | Elden Ring | Nightreign |
|---|---|---|
| Event dispatchers | `0x14042E780`, `0x14042F730` | `0x140410CA0`, `0x140414820` (most handlers inlined) |
| Event ranges | 0–238, 302–522, 601–800, 904–911, 10096 | 0–238, 302–530, 601–800, 904–1029, 10096, 10130, 10150 |
| ChrActionFlag switch | `0x140427C54`, 1–143 | `0x14040D434`, 1–163 (both `dec eax` first) |
| Module bag | `chr+190h` | **`chr+1B8h`** |
| ChrCtrl | `chr+58h` | `chr+60h` |
| ActionFlag turn speeds | `+84/+88/+8C`, priorities `+91..+93` | `+CC/+D0/+D4`, priorities `+D9..+DB` (the same struct, moved by 48h) |

The context passed to the handlers is the same: `+0` type, `+8` parameters, `+18` a flag, `+1A` the time act version.

### Module bag

Read from NR's two builders (`0x1403DE8C0` and `0x140673220`, setter `0x1404CDA00`). Slots `+00..+118` are ER's
modules, with player subclasses where they exist (`CSPlayerSuperArmorModule`, `CSPlayerDamageModule`, …). NR adds:

`+118` GrassHit, `+128` CliffWind, `+138` Hookshot, `+140` WallEdgeLanding, `+148` Skill, `+150` PlayerResonance,
`+158` JamaisVu, `+160` NecroBuddy, `+168` RebornBuddy, `+170` BirdMove, `+178` ThrowCounter, `+180` MixcraftMagic,
`+188` PostureControl, `+190` Majyuu, `+1A0` FatalDamageDodge, `+1A8` PlayerAnalyzeCam, `+1B0` PlayerJoint,
`+1B8` Trans, `+1C0` BonePile.

`TaeTemplateSurvey modules <exe> <builder> <setter> <skip,...>` reads them, and `trace … NR` uses this layout.

## Method

1. **Port from ER** (`TaeTemplateSurvey port`). An event takes the new ER definition when NR's trace reads no parameter
   that ER's doesn't. NR keeps its own enum entries, and its event name where ER's is still a placeholder. 141 events
   were ported. ChrActionFlag (0) was left out, as NR's flag switch is larger and its labels can't be assumed to match.
2. **NR's own fixes** (`nr_names.txt`), for the 11 events whose NR reads go beyond ER's, and for NR data.
3. **Names for NR-only fields** (`nr_names2.txt`, generated from `evidence`, with overrides) and event names
   (`nr_names3.txt`).

## Changes beyond the ER port

| Event | Change | Evidence |
|---|---|---|
| 1 AttackBehavior | **Template 0x10 → 0x20**, `+10` `b Unk10` | Every NR event is 0x20 bytes, and the game compares `+10` to 1. The old template would have cut 16 bytes off each event on save |
| 2 BulletBehavior | `s16 Offset` → `b Unk10` + `s8 Offset`; `+14` assert → `b Unk14` | NR compares `+10` to 1 and reads `+11` signed (data: 256, 512… = byte `+11` 1..7). `+14` broke its assert in 106 events |
| 152 CameraZoomOut | ER's GameMan names (`TargetDistMultiplier`, `LerpFactor`, …, `CheckCollisions`, Type enum) | the same setters as ER, storing to `+504/+508/+510/+514` |
| 193 SetOpacityKeyframe | `+09` assert → `b Unk09` | read by NR, and broke the assert in 3 events |
| 231 RequestMsgMapList | + 300, 400, 600 | used by NR |
| 304 ThrowAttackBehavior | `s16` → `u8 Source` + `u8 Unused03` (**ER too**) | `+02` goes to `0x140416D10`, the same function as the Source fields of 2/64 and 129's SoundIDOffsetType. 768 in the data is `+03` = 3, which isn't read |
| 710 HideEquippedWeapon | `+04` assert → `b Unk04/05/06` | read by NR, and broke the assert in 4 events |
| ChrActionFlag | + 43, 126, 127, 130, 133, 136, 137 | NR's handlers match ER's (127/137 toggle off, 130/136 toggle on, 133 → bag `+120`). 43 and 126 aren't in the switch |
| 10096 | `Sfx?` → `SpawnOneShotFFX_10096`, + `StateInfo` at `+10` | handled (`cmp eax,2770h`). The same spawner as 96, and NR reads `+10` the way 96 does |
| 344 / 907 / 911 | `…Unk350` / `…Unk3A8` / `…Unk32C` | the ER names carried ER offsets. NR writes ChrCtrl `+350`, Physics `+3A8`, ChrCtrl `+32C` |
| 784, 1016, 1020, 154, 1013 | named from their stores | 784 → Physics `+3B0/+3B4`; 1016 → BirdMove `+250`; 1020 `+00` → the StateInfo function; 154 → TimeActModule `+110..+118`; 1013 → Majyuu `+58` |

### Events added (used by NR, missing from the template)

| ID | Name | Evidence |
|---|---|---|
| 131 | Event131 | not in either dispatcher. Data: `+00` always 1, `+04` 9-digit IDs, `+08` −1 or small numbers |
| 313 | ChrClothState_313 | compares a byte to 32/64/96 and passes it on, as 310/311 do |
| 708, 1017 | SetActionFlag58_708 / _1017 | OR a flag into ActionFlag `+58`; no parameters read |
| 1014 | SetBirdMoveState | `+00` → BirdMove `+50` (data 0..2) |
| 1015, 1018 | SetBirdMoveFlag24C / 24D | set BirdMove `+24C` / `+24D` |
| 1019 | SetPhysicsFlag4FC | sets Physics `+4FC` |
| 1022 | Event1022 | only calls `0x1403D4180` on the character |

## Duskbloods

Smithbox also used this template for Duskbloods (`DB`, `DB_NT`). Duskbloods' 335,790 events didn't fit it before this
work (26,959 change on a round trip), and the NR-checked layouts fit it slightly worse (27,205). Duskbloods now has
`TAE.Template.DB.xml`, a copy of the old shared template, so it behaves exactly as before (`TimeActData.GetTemplateFilename`).
It needs an analysis of its own.

## Verified

* `roundtrip` on NR: **0 of 1,323,734** events change (113 changed with the old template: 2, 155, 193, 710).
* `report` on NR: no broken asserts, no unlisted enum values, no data past the template, no missing event types.
* `unnamed`: no fields left without a name, apart from asserted padding.
* Every NR event's template is now at least as big as its data, which `roundtrip` alone can't see.
* ER's template still round-trips 0 of 1,630,738 after the 304 fix. Duskbloods' template is byte-identical to the old one.
* `Smithbox.Program` builds.

## Follow-ups

* Duskbloods: run the same method on its own exe.
* NR's ChrActionFlag 144–163 aren't checked against its handlers.
* 131's handler isn't found. Its type isn't compared against directly, so it's probably looked up through a helper.
