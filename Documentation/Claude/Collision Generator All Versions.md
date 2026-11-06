# Collision Generator: every Havok version

Goal: the Havok Editor's Collision Generator (`CollisionGeneratorTool`) builds a replacement map/asset collision shape
for every class set the editor reads, not only DS3 (hk2014) and the 2018/2019 games.

Status: **hknp done 2026-11-06 (BB, SDT): checked with game files, not yet in the app. hkp (DeS, DS1, DS1R, DS2S) is a
later phase.**

## Done (hknp)

- **SDT**: `src/Havok/Classes/Havok.hk2016/Builders/CollisionShapeBuilder.cs` (`Havok.hk2016.Builders`), typed against
  the 2016 classes. It isn't a shared source with the DS3 generator: the 2014 and 2016 class sets name the same types
  differently (`hkcdStaticMeshTreeBaseSection` / `hkcdStaticMeshTreeBase.Section` and so on).
  - Compressed meshes only; SDT's extern meshes are only in its `f` binders, with an AABB tree the generator doesn't build.
  - The replace also swaps the shape in the system's `m_referencedObjects`, which SDT lists it in.
- **BB**: the DS3 generator with a `Variant`: flags `IS_COMPOSITE_SHAPE` without `NO_GET_SHAPE_KEYS_ON_SPU`, convex
  radius 0. Everything else matched DS3.
- **Collision Generator**: lets BB and SDT in. The mesh type choice only shows where extern meshes can be built, and
  the material picker reads 2016 shapes.

### What SDT holds (CollisionSurvey SDT, m10 h)

- Flags `IS_COMPOSITE_SHAPE`, radius 0.01, tag codec `FFFFFFFF`, `COMPRESSED_MESH`/`COMPOSITE`, no material table,
  connectivity empty, domain w 0.99/1.01. Interior bitfield is `maxKey + 1` bits.
- Sections and runs as DS3: packed `count | first << 8` words, `DataRunData` 0xFFFF.
- **SIMD tree**: a real tree, unlike DS3's 2-node stub. No leaf/compact flags: each lane's data is
  `value << 1 | isLeaf`, the value a child node index or a primitive key (one leaf per primitive). Node 0 is an empty
  sentinel, node 1 the root, empty lanes have inverted bounds and data 0. Nodes can mix leaf and child lanes; vanilla
  orders them depth first. The builder writes the shared builder's breadth-first tree in this encoding.

### Verified (rebuild every vanilla mesh, write, re-read, decode and check keys, trees, SIMD tree, materials)

| Game | Maps (h + l) | Result |
|---|---|---|
| SDT | m10, m11, m20, m25 | 1,575 / 1,576. The one is `l10_00_00_00_000001`: too many shared vertices for the builder (its limit, every SDK) |
| BB | m21, m24_01, m25, m29 | 7,862 / 7,864. The two are vanilla meshes with no triangles |
| DS3 | m30, m40 | unchanged: m30 h 87/93, the 6 empty vanilla meshes |

DS3 note: a few vanilla DS3 meshes have radius 0 rather than 0.01; the generator keeps 0.01.

### To check in the app

1. **SDT**: Havok Editor → Map Collision → an `h` binder → a file → Collision Generator. No mesh type choice, the
   material section shows. Generate a square: it draws, undo restores the old shape, and save and reload keeps the new one.
2. **BB**: the same, with both mesh types.
3. **DS3, ER**: unchanged behaviour.
4. **DS1 / DS1R / DeS / DS2S**: the "not supported" message names the older physics format.
5. In-game, if possible: walk on a generated SDT and BB collision.

## What each version ships

From the Phase 0 survey goldens (`Havok Library Survey/Golden/*.txt`): the collision shape classes of each game.

| Class set | Games | Physics | Map collision mesh | Generator today |
|---|---|---|---|---|
| hk55 | DeS | hkp | `hkpMoppBvTreeShape` → `hkpStorageExtendedMeshShape` | none |
| hk2010 | DS1 | hkp | `hkpMoppBvTreeShape` → `hkpStorageExtendedMeshShape` | none |
| hk2015 | DS1R | hkp | `hkpMoppBvTreeShape` → `hkpStorageExtendedMeshShape` | none |
| hk2014 | DS2S | hkp | `hkpMoppBvTreeShape` → `hkpStorageExtendedMeshShape` | none |
| hk2014 | BB | hknp | `fsnpCustomParamCompressedMeshShape` | none (DS3's would fit: same classes, BB layout overlay) |
| hk2014 | DS3 | hknp | `fsnpCustomParamCompressedMeshShape` | `HavokCompressedMeshGenerator_hk2014`, `HavokShapeGenerator_hk2014` |
| hk2016 | SDT | hknp | `fsnpCustomParamCompressedMeshShape` (+ one extern mesh seen) | none |
| hk2018 | ER, NR, DB, DB_NT | hknp | `fsnpCustomParamCompressedMeshShape` | shared `CollisionShapeBuilder` |
| hk2019 | AC6 | hknp | `fsnpCustomParamCompressedMeshShape` | shared `CollisionShapeBuilder` |

DS2 (hk2012) has no class set, so the editor can't open it; it stays out.

## The two families

### hknp (BB, SDT): small

- **BB** uses the hk2014 classes, as DS3 does. The DS3 generator should work unchanged; `SupportsGame` just has to
  let BB in. To check: the BB fields the generator fills against BB files (`CollisionSurvey BB … rebuild`).
- **SDT (hk2016)** is a mix, from diffing the class sets:
  - The mesh tree is DS3's: `hkcdStaticMeshTreeBase.Section` with the packed `m_sharedVertices`/`m_primitives`/
    `m_dataRuns` words, `DataRunData` run values, 5-byte/4-byte codec nodes.
  - The shape header is 2018's: `hknpShape.m_type`, the dispatch type as an enum, `hknpCompositeShape.m_materialTable`,
    no edge welding map. `hknpCompressedMeshShape` has no `m_quadIsFlat`, and `m_data` is typed `hkReferencedObject`.
  - Bodies are `hknpPhysicsSystemData.bodyCinfoWithAttachment`.
  - Plan: make the DS3 generator a shared source compiled into `Havok.hk2014` and `Havok.hk2016` (as
    `CollisionShapeBuilder` is for 2018/2019), with `#if HK2016` for the differences above. Then survey SDT's own
    values (shape flags, convex radius, SIMD tree stub or real) with `CollisionSurvey SDT … rebuild` before trusting it.

### hkp (DeS, DS1, DS1R, DS2S): large

These games have no compressed mesh. A map collision is an `hkpMoppBvTreeShape` whose child is an
`hkpStorageExtendedMeshShape` (triangle subparts holding vertices, indices and per-triangle material indices). The
MOPP tree is Havok's own byte code for the bounding volume tree, and nothing in the repo builds it.

Supporting these needs:
1. **A MOPP compiler** in `Havok.Shared/Builders`: quantise the mesh into the MOPP's 0..255 integer space (from
   `hkpMoppCode.m_info.m_offset`, scale in `.w`), and emit a split tree of the documented opcodes (axis splits,
   bound checks, terminals with offsets). Checked by a decoder: every triangle must be reachable by a query
   over its own AABB, and vanilla MOPPs must decode to the same triangle sets.
2. **The extended mesh shape** per class set (hk55, hk2010, hk2014-hkp, hk2015): one triangle subpart, materials
   through the FromSoftware material subclass the games use (to survey: the material class and its fields in each
   game's files).
3. **The body side**: `hkpRigidBody` in `hkpPhysicsSystem`, unlike hknp's body cinfos. Replacing the shape of the first
   rigid body's collidable, as `ReplaceMeshShape` does for hknp.
4. An in-game check per game, as the MOPP and FromSoftware's material lookups can't be confirmed from files alone.

DeS is big-endian and 32-bit; the packfile writer already handles it, but it's the least tested.

## Decisions

- **S1. Scope.** **Decided (2026-11-06): hknp (BB, SDT) now, the hkp games as a later phase.**
