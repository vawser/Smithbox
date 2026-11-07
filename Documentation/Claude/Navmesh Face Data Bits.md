# Navmesh Face Data Bits (Elden Ring)

2026-11-07. What `hkaiNavMesh.m_faceData` (one u32 per face) and user-edge `m_edgeData` mean, read from
`Documentation/Games/eldenring.exe`. Tools: `Tools/NavSurvey` `exe` modes `vt` (RTTI virtuals), `bits` (clusters of bit
tests), `fn`, `win` and `jt`. The names are now shown by the Map Editor's Navigation tool (`NavFaceDataBits`).

## Where the game reads them

The navmesh cost modifier is **`CS::CSChrCostModifier`** (vtable `0x142A07F58`). There is a variant for sealed worlds,
`CSChrCostModifier_SealdWorld`.

| Function | Role |
|---|---|
| `0x1402E33B0` | Reads a face's data from the navmesh instance: `[inst+0x48]` original faces, `[inst+0x140]` owned faces, `[inst+0x120]` cut faces, all scaled by the striding at `[inst+0x50]` |
| `0x140BD9100` | Runtime per-face flags of the navmesh (`[obj+0x30][face]`), ORed into the face data before any test |
| `0x1402E2550` (vfunc 1) | **Passability.** If `flags & 0x1115401` is 0, the face is passable. Otherwise the result depends on the character (`0x1402E2A60`), unless the query context byte `+0x14` bit 0 overrides it |
| `0x1402E2A60` | The per-bit rules, below. The sealed-world copy is `0x1402E2BC0` |
| `0x1402E1C70` (vfunc 3) | **Cost.** Multipliers for some face bits, plus penalties from the user-edge data. Sealed-world copy: `0x1402E20C0`, which has no 0x8 case |

`[module+0x30]` in these functions is the character's **NpcThinkParam** row. The byte at `+0x64` is
`enableNaviFlg_*`, so bit 2 = Ladder and bit 6 = Lava. Recomputed from the paramdef: `dummy8 x:7` and `dummy8 pad8:5` share
their byte with the u8 bits before them.

## Face data bits

| Bit | Name | What the code does | n faces / o faces in a sample of ER maps |
|---|---|---|---|
| `0x1` | Impassable | Never passable | 149+ / 26+ |
| `0x8` | High cost | Cost × 100 (not in sealed worlds) | none |
| `0x40` | ? | Not read by the cost modifier | 201 / 37 |
| `0x400` | Checked | In the passability mask `0x1115401`, but no rule of its own | none |
| `0x1000` | Lava | Passable only with NpcThinkParam `enableNaviFlg_Lava`, or for characters with a runtime state (`chr+0xDCA0`), who pay × `[world+0x9808]` | 5,417 / 1,461 |
| `0x4000` | Ladder | Passable only with NpcThinkParam `enableNaviFlg_Ladder` | none |
| `0x8000` | Opens 0x10000 | Never in the files; set at runtime with the dynamic face flags | none |
| `0x10000` | Gated | Not passable unless 0x8000 is also set: something in game opens it, such as a door | 94 / 35 |
| `0x20000` | ? | Not read by the cost modifier | 958 / none |
| `0x40000` | Huge enemy area | Cost × `[world+0x9804]`, unless NpcThinkParam `isNoAvoidHugeEnemy` | none in the sample |
| `0x80000` | Opens 0x100000 | Never in the files; set at runtime | none |
| `0x100000` | Gated | Not passable unless 0x80000 is also set | 519 / 108 |
| `0x200000` | High cost | Cost × 100 | none |
| `0x1000000` | Character limit | Not passable when `CSChrPhysicsModule +0x340` (unnamed in fromsoftware-rs; set to −π/2 at init, so probably an angle) is above a float of the navmesh section (`[section+0xC0]`, via `0x140C3C210`), or when the query asks for it | 44,393 / none |

`[world]` is the singleton at `0x143D7C908`, which also hands out the navmesh sections (`0x140C3C1A0`). Its multipliers
come from data not traced here. If passability fails during costing, the cost is 100 rather than infinite.

## User edge data

`0x1402E1C70` reads the edge's data and adds a penalty after the multipliers:

| Bit | Name | Penalty |
|---|---|---|
| `0x2` | Jump | NpcThinkParam `enableJumpMove` (in battle: `enableJumpMove_onBattle`): 1 → +20, 3 (battle) → +1, else none |
| `0x4` | Fall | `useFall_onNormalCaution` (in battle: `useFall_onSearchBattle`): the same values |
| `0x10` | Extra cost | +4 on a jump, +2 on a fall |

The game files' values (`0x0X04`, `0x0X14`, `0x02`, `0x12`) fit this. The high byte (`X` = 3..F) isn't read here; perhaps it is
the drop height.

## False leads (not navigation)

The `bits` scan also turned up:
- `0x140D48030`: builds four 32-bit masks from EquipParamProtector `invisibleFlag*` bits.
- `0x140F4EC08`: UI text layout (kerning).

Clusters of the same constants aren't enough on their own.

## Open

- Bits `0x40` and `0x20000`, which are common in the files: their readers are elsewhere, perhaps AI movement or path
  following rather than costing.
- Who sets the runtime flags (0x8000, 0x80000): the writers of the per-face array that `0x140BD9100` reads. Candidates are
  MSB `NavmeshCutting` regions and `Event.Navmesh`, and assets with `AssetGeometryParam.navimeshFlag` (Obstacle / Impassable /
  High Cost, which look like 0x1 and 0x8).
- The navmesh float `[section+0xC0]` behind the character limit: possibly the NVA navmesh's `Unk44` (2.4).
