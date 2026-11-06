# Compressed Mesh Format (fsnpCustomParamCompressedMeshShape)

What the Collision Generator's compressed mesh builder (`HavokCompressedMeshBuilder`) writes, and the game data it
was checked against. Surveyed with `Tools/CollisionSurvey` on ER m10_00_00_00, m11_00_00_00, m60_42_36_00 and AC6
m10_00_00_00. ER and AC6 match on every field below.

Run the survey from `src/Smithbox.Data`:

```
CollisionSurvey <GAME> <mapId> [max files] [h|l] [file regex] [detail|rebuild]
```

`rebuild` decodes each vanilla mesh, rebuilds it with the builder, writes and re-reads it, and checks the result.
All 313 ER meshes in the three maps pass.

## Shape

| Field | Value |
|---|---|
| class | always `fsnpCustomParamCompressedMeshShape` in map collision |
| `m_flags` | `IS_COMPOSITE_SHAPE` |
| `m_convexRadius` | 0.015 (a few 0.01 or 0) |
| `m_shapeTagCodecInfo` | 0xFFFFFFFF |
| `m_materialTable` | null (absent in 2019) |
| `m_numShapeKeyBits` | = tree `m_bitsPerKey` |
| `m_triangleIsInterior` | bit field of `maxKeyValue + 1` bits, indexed by shape key. Welding hint; the builder leaves it zero |

## Mesh tree (`hknpCompressedMeshShapeTree`)

- `m_domain`: the triangle AABB expanded by the convex radius, with W = 1 - r and 1 + r. Section domains do the same.
- `m_nodes`: binary 5-byte tree over the sections, `2 * sections - 1` nodes. The left child follows its parent. An
  internal node has the high bit of `m_hiData` set and stores half the offset to its right child; a leaf stores its
  section index. The root's box bytes are 0 (its box is the domain).
- Child boxes are quantized against the parent's decoded box per axis: high nibble `a`, low nibble `b`, min =
  pmin + a² / 226 × extent, max = pmax - b² / 226 × extent.
- Shape key = section << 8 | primitive << 1 | quad half. `m_maxKeyValue` is the largest key, `m_bitsPerKey` its bit
  length, `m_numPrimitiveKeys` the triangle count (a quad counts twice).
- `m_primitiveDataRuns`: one per section, value 0xFFFF, index 0, count = the section's primitives.
- Shared vertices: 21/21/22 bits over the mesh domain. Used by vertices that appear in more than one section, and by
  all vertices of a section too large to pack precisely.

## Sections

- At most 127 primitives (7 bits of key) and 255 vertices (byte indices).
- `m_codecParms`: offset XYZ and scale XYZ of the packed vertices, 11/11/10 bits; the codec covers the packed
  vertices only, not the shared ones.
- Primitive indices below `m_numPackedVertices` are packed vertices, the rest index `m_sharedVerticesIndex` from
  `m_firstSharedVertexIndex`. A primitive is a triangle when index 3 equals index 2; 0xDE 0xAD 0xDE 0xAD marks an
  empty one.
- `m_nodes`: binary 4-byte tree over the primitives, `2 * prims - 1` nodes. Internal nodes store the offset to the
  right child with the low bit set; leaves store primitive × 2. Root box bytes are 0.
- `m_leafIndex`: the section's leaf in the mesh tree. `m_page`, `m_layerData`, `m_flags`: 0. `m_numDataRuns`: 1.

## SIMD tree and connectivity

- `m_hasSimdTree` true, `m_isCompact` true. Node 0 is an empty non-leaf sentinel, node 1 the root, which is internal
  even for one leaf. Nodes are in breadth-first order; internal lanes hold child node indices, leaf lanes hold shape
  keys with the unexpanded triangle bounds. Empty lanes have inverted (max/-max) bounds and data 0.
- `m_connectivity` is empty.

## FromSoftware parameters (`m_pParam`)

- `m_version` 0x100000, `m_vertexDataStride` 1 (some 0), `m_triangleDataStride` 0.
- One `PrimitiveData` per collision material: `m_materialNameData` is the material ID, `m_vertexData` one 0xFF byte per
  vertex of the group, triangle and primitive data empty.
- One `TriangleData` per triangle in authoring order: group index, index within the group, and group-local vertex
  indices, numbered as each triangle's corners are first met from the last corner to the first.
- `m_triangleIndexToShapeKey`: per triangle, its shape key top-aligned in 32 bits with the unused low bits set.
- Material IDs follow the OBJ2HKX naming (`Reference/OBJ2HKX/mats.xml`): name ID + property ID, e.g. 429 =
  Without (29) + Slide1 (400).

## HKX2 (DS3, Havok 2014)

`HavokCompressedMeshGenerator_HKX2` fills the HKX2 classes from the same build. Surveyed on DS3 m30_00_00_00,
m31_00_00_00 and m40_00_00_00; the `rebuild` mode passes on all 3080 meshes with triangles (a few vanilla meshes
hold only 0xDEAD primitives and are skipped). The differences from HKX3:

| Field | Value |
|---|---|
| `m_flags` | 516 (IS_COMPOSITE_SHAPE \| NO_GET_SHAPE_KEYS_ON_SPU); one vanilla mesh had 4 |
| `m_convexRadius` | 0.01, domain W 0.99 / 1.01 |
| `m_edgeWeldingMap` | mask 0xFFFFFFFF, 0 bits, empty lists |
| `m_quadIsFlat` | `(maxKey >> 1) + 1` bits (per primitive key) |
| `m_triangleIsInterior` | `maxKey + 1` bits |
| SIMD tree | two empty nodes; unused |
| section `m_sharedVertices.m_data` | packed vertex count \| first shared index << 8 |
| section `m_primitives.m_data` | count \| first primitive << 8 |
| section `m_dataRuns.m_data` | count \| first run << 8. Vanilla shares identical runs between sections; one per section is also valid |
| section `m_numSharedIndices` | the section's shared vertex count |

`HKX2.Builders.hknpCollisionMeshBuilder` is an older DS3 builder that needs the native NavGen.dll BVH; the
Collision Generator does not use it.

## Not done

- Quads: vanilla merges triangle pairs into quads. The builder only writes triangles, which is valid but larger.
- Interior triangle and flat quad bits are left clear.
