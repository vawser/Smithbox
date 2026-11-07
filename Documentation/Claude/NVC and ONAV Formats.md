# NVC and ONAV Formats (Elden Ring)

2026-11-06. Both formats are now in SoulsFormats: `Formats/NVC.cs` and `Formats/ONAV.cs`.
Both belong to the navmesh family with NVA (`NVA_ER`).

| Format | Files | Location | Round trip |
|---|---|---|---|
| NVC | 3,674 | `/map/nvc/mAA_BB_CC_DD_mAA_BB_CC_DD.nvc.dcx` (DCX_KRAK) | 3,674 / 3,674 byte-identical |
| ONAV | 997 | `/map/onav/mAA_BB_CC_DD.onav.dcx` (DCX_KRAK) | 997 / 997 byte-identical |

The ER File Dictionary lists 14 more NVC/ONAV paths that the game does not contain.

Tool: `Tools/NavSurvey`. Run it with the working directory at `src/Smithbox.Data`. In Git Bash, set `MSYS_NO_PATHCONV=1`,
otherwise `/map/...` arguments get rewritten into Windows paths.

| Mode | What it does |
|---|---|
| `extract <dir>` | writes every decompressed file |
| `roundtrip` | reads and writes every file through SoulsFormats and compares bytes |
| `check` | checks NVC against the two maps' NVAs |
| `rawnvc` / `rawonav <dir or file>` | the format-agnostic probes used to decode the layouts |
| `exe <exe> magic / str <text> / calls <va> / fn <va> / disp <hex> <lo> <hi>` | exe scans (Exe.cs/Arxan.cs linked from MsbExeSurvey) |

---

## NVC: navmesh connections between two maps

One file per pair of neighbouring maps. The file name holds both map names, and the game parses them into packed map
numbers (`0x140721060`). The game's reader is at `0x140C90DC0`, reached through CS::NvcResCap. The format is a
standalone slice of NVA's connector sections.

### Header (0x10)
| Off | Type | Value |
|---|---|---|
| 0x0 | char[4] | `NVMC` |
| 0x4 | int | version: 3 in every file; the game accepts 1–3 |
| 0x8 | int | file size |
| 0xC | int | section count: always 5, in the order 3, 0, 1, 2, 4 |

### Section header (0x10), as in NVA
`int type, int version (=1), int length, int count`, followed by `length` bytes. The game only accepts types below 5
with version 1. For each type it stores the data pointer, and stores the count for every type except 3.

| Type | Entry | NVA equivalent | Contents |
|---|---|---|---|
| 3 | 0x10 ×1 | Section 11 (`Entry11`) | `uint MapUID, uint OppositeMapUID, 0, 0` |
| 0 | 0x20 | Section 4 `Connector` | `MainNameID, TargetNameID, navCount, graphCount, navIndex, Unk14, graphIndex, Unk1C` |
| 1 | 0x10 | Section 5 `NavMeshConnection` | `FaceIndex, EdgeIndex, OppositeFaceIndex, OppositeEdgeIndex` (hkaiStreamingSet::NavMeshConnection) |
| 2 | 0x8 | Section 6 `GraphConnection` | always empty |
| 4 | ? | ? | always empty (length 0); SoulsFormats throws if it ever isn't |

### What `check` confirmed against the NVAs
- `MainNameID` is a `Navmesh.NameID` in the first map's NVA, and `TargetNameID` one in the second map's NVA:
  3,299 of 3,299 connectors.
- `FaceIndex` < that navmesh's `FaceCount`: 83,424 of 83,456. `OppositeFaceIndex` is always in range. Edge index ≥ 3 × face index.
- `MapUID` and `OppositeMapUID` equal `Entry11.Unk00` of the two NVAs in 3,365 of 3,376 files.
  A few maps share the UID `ECBB4B55`, both within a file and across files.
- 1,326 files have no connectors at all.
- The connections of a connector are consecutive (index = the running total), so SoulsFormats rebuilds the indices on write.

The model reuses `NVA_ER.NavMeshConnection` / `NVA_ER.GraphConnection`, so connections can move between NVA and NVC.

---

## ONAV: the map's occlusion graph

The RTTI names are CS::OnavResCap, OnavFileCap and OnavRepository. The code that reads the data asserts with source
names under `Source\MSP\MSPOcclusionGraph*.cpp`, namely `MSPOcclusionGraphAreaLayerAccessor.cpp` and `…GroupedAreaLayerAccessor.cpp`.
940 of the 997 files are empty (140 bytes).

The file is a memory image: 64-bit offsets, a relocation table at the end, packed at 4-byte alignment.

### Game code
| VA | Role |
|---|---|
| `0x1423B9240` | validate: magic `ONAV`, byte4 == 3, byte5 == 2, byte6 == 0. So the 26 files with byte5 = 1 are rejected (they have the same layout) |
| `0x14233CD40` | loader: skips a file whose u16 at 0x34 is 0, copies it, then calls the fixup |
| `0x1423B92A0` | fixup: adds the base to each offset in the relocation table (offset at 0x28, count at 0xC), then to each entry of the area-pointer array (0x20, u16 count at 0x34). Those entries are not in the table |
| `0x1423B0C00` | build: reads each area's layer count (byte +2), its layers at +0x248 (stride 0x38), and the 0x230 array at 0x10 (u16 count at 0x36) |
| `0x1423ADFE0` | transform of a 0x230 element: byte +3 is the rotation mode (0–2, other values assert), +0x10..0x18 are Euler angles (sin/cos), +0x1C is multiplied by 65535 × 0.5 |
| `0x1423B1380` | link lookup: area u16 +0x28 counts the 8-byte links at +0x240; a link's low nibble at +2 is matched against layer byte +0; layer byte +0x2A counts the 16-byte entries at layer +0x30 |

### Header (0x74)
| Off | Type | Field |
|---|---|---|
| 0x00 | char[4] | `ONAV` |
| 0x04 | u8, u8, u16 | Version 3, SubVersion 2 (or 1), 0 |
| 0x08 | int | 0 |
| 0x0C | int | relocation count |
| 0x10 | offset | UnkAreas (0x230 each) |
| 0x18 | offset | always-empty array (assumed count at 0x32) |
| 0x20 | offset | area pointer array |
| 0x28 | offset | relocation table (not itself relocated) |
| 0x30 | u16 ×4 | 0, 0, area count, UnkArea count |
| 0x38 | 0x18 zero bytes | |
| 0x50 | Vector3 | BoundsMax; an empty file has 1.17549435e-38 (smallest normal float) on each axis |
| 0x5C | Vector3 | BoundsMin; an empty file has FLT_MAX |
| 0x68 | int ×3 | grid cell counts X, Y, Z; the product is always 4096, e.g. 16×16×16, 64×2×32, 2×2048×1 |

### Body order (what the writer reproduces)
1. Each area in turn: its 0x248-byte body, then its layers (0x38 each), then each layer's entries, then its links.
   An empty list's pointer points to where the list would start.
2. The UnkAreas (offset 0x10), then the empty array (offset 0x18), then the area pointers (offset 0x20).
3. The relocation table: 0x10, 0x18, 0x20, then for each area its +0x240 site followed by each layer's +0x30 site.

### Area (0x248 + layers)
| Off | Type | Field |
|---|---|---|
| 0x00 | u16 | ID. Links refer to it. Areas and UnkAreas share the numbering, so it is not an index |
| 0x02 | u8 | layer count |
| 0x03 | u8 | Unk03 (0, 2, 4, 6, 8) |
| 0x04 | Vector3 | Position |
| 0x10 | Vector3 | Rotation (Euler, radians) |
| 0x1C | Vector3 | Scale (16-bit local quantization, ×65535) |
| 0x28 | u16 | link count |
| 0x2A–0x3F | | unknowns: Unk2A u16, Unk2C/2D u8, Unk2E, Unk30, Unk34, Unk38, Unk3C u16 (=1), Unk3E u16 (counts up) |
| 0x40 | byte[0x200] | CellMask: 4096 bits, one per header grid cell. The bit order relative to X/Y/Z is unverified |
| 0x240 | offset | links (8 bytes each: u16 AreaID, u8 Layers (two nibbles), u8, int) |

### Layer (0x38)
Byte 0 is the layer index, which links match on. Byte 1 is a flag; when set, the game adds the layer to a list.
The other fields are a Vector3 at +4, small ints at +0x14/+0x18, about 7500 at +0x1C (counts up across files),
a key at +0x24 (the game looks it up when nonzero; always 0), u8 +0x28, u8 +0x29 (=2), the entry count at +0x2A,
and the entries offset at +0x30. Each entry is 16 bytes: a u8 holding two layer nibbles, u8 0, u8 0 or 2, then zeros.

### UnkArea (0x230)
The same first 0x28 bytes as an area, with byte +2 an unknown (0/1) instead of the layer count. Then int +0x28, int +0x2C,
and the 4096-bit CellMask at +0x30. There are 25 in total, across about 20 maps.

---

## Open questions
- NVC section 4, and NVC graph connections: always empty, so their layouts are unknown.
  SoulsFormats asserts that section 4 is empty.
- ONAV: what UnkAreas are, given that they have IDs links can reach. Also the meanings of the area/layer unknowns,
  the CellMask bit order, and why `Unk03` on areas goes beyond the 0–2 rotation modes the UnkArea transform accepts.
- ONAV links and layer entries have a high nibble the game passes on (`0x1423ADA90`); presumably it is the layer in
  the target area.
- 32 NVC face indices are beyond their navmesh's NVA `FaceCount`; they need checking against the navmesh HKX itself.

## Not done
Nothing in Smithbox uses these classes yet. They are SoulsFormats-only, with no editor, file-browser or save hookup.
