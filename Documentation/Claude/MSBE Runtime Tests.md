# MSBE runtime tests: UnkE3E, UnkE3C, Asset UnkT51

Static analysis of `eldenring.exe` ends in unnamed accessors for these three fields (see `MSBE Exe Analysis.md`). These
tests change each field on a few retail parts and look for the difference in game. Each test edits one map, so keep a
backup of the map's `.msb.dcx` (or use a mod folder) and restore it afterwards.

What the exe shows, and what retail data shows, for each field:

| field | exe | retail |
|---|---|---|
| Part `UnkE3E` (byte) | becomes bit 0 of byte 2 of the part's render flags (`CSGeomModelIns +152h`) | 1 on 16,692 assets and 2,331 map pieces; mostly in the `_02`/`_12` LOD-overview maps (12,074 there, against 2,531 in `_00` maps) |
| Part `UnkE3C` (u16) | copied into the geometry setup next to the PartsDrawParam flags; `-1` = none | -1 on all but 143 parts; values like 3, 10, 20, 50, 400–437, 800, 829–837, 880, 900–999 |
| Asset `UnkT51` (byte) | 1 and 2 put the asset into two separate instance lists of the geometry manager, checked next to DisableRTAO | 0 on all but 99 assets: `AEG010_*` and a few others in `m60_09_12_02`, `m61_11_11_02`, `m61_11_11_12` |

Working guesses to confirm or reject: `UnkE3E` is a distant-view/LOD render flag; `UnkT51` selects a distant-view or
ray-tracing list. `UnkE3C` has no guess yet.

## Test 1: UnkE3E

Map `m11_05_00_00` (Leyndell, Ashen Capital). These assets have `UnkE3E = 1`:
`AEG022_010_4000` … `AEG022_010_4011`, `AEG022_011_4000` … `AEG022_011_4005`, `AEG022_000_4000`, `AEG004_543_4000`.

1. In Smithbox's map editor, set `UnkE3E` to 0 on the `AEG022_010_*` assets. Save.
2. In game, stand where they are visible near and far (from close up, then from a distance and from a high point).
3. Compare with the unedited map: visibility at distance, LOD switching, shadows, reflections, whether they draw through
   fog or the far clip.
4. Optional reverse check: set `UnkE3E = 1` on a nearby asset that has 0, and see whether it gains the same behaviour.

## Test 2: UnkE3C

Parts with a value:
* `m15_00_00_00` MapPiece `m100018_3000` = 998, Asset `AEG097_000_1000` = 999
* `m10_00_00_00` MapPiece `m025000_2000` = 901, `m025002_2000` = 902
* `m13_00_00_00` MapPiece `m100003_5500` = 900

1. Set `UnkE3C` to -1 on `m10_00_00_00` `m025000_2000` and `m025002_2000` (Stormveil). Save.
2. In game, look at both map pieces near and far, and walk through the area around them.
3. Compare with the unedited map: lighting/light probes, draw distance, sound, or whether they appear at all.

## Test 3: Asset UnkT51

Map `m60_09_12_02` (open-world LOD level 2). The `m60_3x_4x_00-AEG010_0xx_0000` assets there have `UnkT51` 1 or 2.

1. Set `UnkT51` to 0 on two of them, one with 1 and one with 2: `m60_38_50_00-AEG010_010_0000` (1) and
   `m60_38_50_00-AEG010_011_0000` (2). Save.
2. In game, view their area of the open world from far away (they belong to the distant-view map), with ray tracing on
   and off.
3. Compare with the unedited map: whether they vanish, change shading, or change only with ray tracing on.

## What to report back

For each test: what changed (or that nothing did), with ray tracing on or off, at what distance, and screenshots if
possible. If a test shows nothing, say so; that rules out the guesses above.
