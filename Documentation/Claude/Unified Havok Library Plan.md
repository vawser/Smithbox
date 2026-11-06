# Unified Havok Library Plan

Goal: replace the three Havok libraries Smithbox uses today with one library that reads and writes every Havok file version they support between them, behind one entry point.

Method: I read the three libraries and their call sites in Smithbox. I didn't build or run anything. Items marked *(verify)* need checking against game files (via VFS for ER) before they're relied on.

---

## 1. What exists today

### 1.1 The three libraries

| | **SoulsFormats `HKX`** | **HKX2** | **HKLib** |
|---|---|---|---|
| Location | `src/Andre/SoulsFormats/SoulsFormats/Formats/HKX/` (4 files, ~2,950 lines) | `src/Havok/HKX2` (+ `HKX2.Serialization` CLI, `HKX.ClassGeneration`) | `src/Havok/HKLib`, `HKLib.Reflection`, `HKLib.Serialization` (+ `HKLib.ClassGeneration`) |
| Container format | Packfile (`0x57E0E057`) | Packfile; TAG0 tagfile (read only) | TAG0 tagfile + TCM0 compendium; XML (2018 only) |
| SDK versions | Header v5 (DeS), v8 (DS1), v0x0B (hk_2014: DS2, BB, DS3) | hk_2014.1.0-r1 packfile (DS3 layouts); 2016 tagfile (Sekiro) | 20180100 (ER, NR overlay, DB/DB_NT), 20190100 (AC6) |
| Classes | ~15 hand-written: skeleton, spline/interleaved animation, binding, ref frame, hknp/hkp collision meshes, MOPP, navmesh, directed graph | 1,356 generated from `classxmlds3` dumps, each with hand-emitted `Read`/`Write`; manual partials for compressed mesh and static tree codecs; builders (BVH, navmesh, collision mesh) | ~2,950 generated per SDK from `HavokTypeRegistry*.xml`, plus generated `HavokData` accessors (~3,200 files) |
| Serialization style | Per-class hand code, switched on `HKXVariation` | Per-class generated code; tagfile reader matches fields by name | Generic, driven by the type registry (`HavokType` + `HavokData`) |
| Writing | Yes, but `Write` always passes `HKXDS3` to the sections | Packfile yes (always writes `hk_2014.1.0-r1`, 64-bit); tagfile no | Yes, binary for both SDKs, XML for 2018 |
| Pointer size / endian | 4 or 8, either endian | Header accepts both, but class code asserts 64-bit pointers (`AssertUInt64`) | 64-bit LE |

There's also a fourth, partial reader: `Smithbox.Program/Renderer/Animation/HavokTagfile.cs` (604 lines). It parses TAG0/TCM0 with no class definitions, for animation playback. It duplicates most of `HKX2.TagFileDeserializer`.

### 1.2 Who uses what

| Game | Havok Editor | Map Editor (collision/navmesh) | Renderer animation | Model Editor |
|---|---|---|---|---|
| DeS | — | SoulsFormats | SoulsFormats | — |
| DS1 / DS1R | HKX2 *(see 1.3)* | SoulsFormats / HKX2 | SoulsFormats | — |
| DS2 | — | SoulsFormats | SoulsFormats (as DS3 variation) | — |
| BB | HKX2 | SoulsFormats (`HKXBloodBorne`) | SoulsFormats | — |
| DS3 | HKX2 | HKX2 + SoulsFormats | SoulsFormats | HKX2 |
| SDT | — | HKX2 tagfile reader | `HavokTagfile` | — |
| ER, DB, DB_NT | HKLib 2018 | HKLib 2018 | `HavokTagfile` | — |
| NR | HKLib 2018 + NR registry | HKLib 2018 + NR registry | `HavokTagfile` | — |
| AC6 | HKLib 2019 (+ binder compendium) | HKLib 2019 | `HavokTagfile` | — |

The split into three type families shows up across Smithbox as triplicated code: `HavokShapeGenerator_HKX2/_HKX3/_HKX3_2019`, `HavokCompressedMeshGenerator_*`, `HKX_Helper`/`HKX2_Helper`/`HKLib_Helper`/`HKLib2019_Helper`, and `IsHKX2`/`IsHKX3`/`IsHKX3_2019` branches in `HavokBank`, `HavokCollisionResource` and others. Call sites to migrate (from a grep for each library):

- **HKLib (11 files):** `HavokTypeUtils`, `HavokShapeGenerator_HKX3*`, `HavokCompressedMeshGenerator_HKX3*`, `HavokCollisionBank`, `HavokNavmeshBank`, `HKLib_Helper`, `HKLib2019_Helper`, `DeveloperKit`, `DeveloperPanel`; plus fully-qualified use in `HavokTagSerializer` and `HKLibTagfileReader`.
- **HKX2 (12 files + qualified uses):** `HavokBank`, `ModelBank`, `HavokCollisionBank`, `HavokCollisionResource`, `HavokNavmeshResource`, `NavmeshBuilderTool`, `WallBank`, `HKX2_Helper`, `HKX_Helper`, `EntAddAction`, `EntDuplicateAction`, `HavokShapeGenerator_HKX2`, `HavokCompressedMeshGenerator_HKX2`, two Param mass-edit operations, `HavokScratchpad`.
- **SoulsFormats HKX (3 files):** `HavokAnimationData`, `HavokCollisionResource`, `HKX_Helper`.
- **Editor framework (version-agnostic, uses `System.Reflection`):** `HavokPropertyCache`, `HavokPropertyDecorators`, `HavokChangeField`, `HavokAdd/RemoveListEntry`, `HavokFileOutline`, `HavokBehaviorSymbols`.

### 1.3 Phase 0 results (2026-11-06)

Done. Full results are in [Havok Library Survey/README.md](Havok%20Library%20Survey/README.md), with per-game reports and golden file lists alongside. What the survey found, against the open questions above:

- **The version matrix is wider than 1.1 says.** DS1R is **tagfile `20150100`**, not a packfile. Original DS2 is **`hk_2012` 32-bit**, and no library reads it. DS2S is `hk_2014` 64-bit (HKX2 only). DeS also has `Havok-4.5` (v4, nothing reads it) and `Havok-5.1` packfiles.
- **`IsHKX2` is wrong for DS1 and DS1R.** HKX2 reads 0/331 DS1 files and can't read DS1R's tagfiles through the packfile path.
- **BB needs real overlay work.** HKX2 reads 382/721 BB files. The rest fail on layout differences.
- **SoulsFormats and HKX2 writers lose data.** SoulsFormats' writes don't re-read for hk_2014, and skeleton/spline writes throw. HKX2 drops `hkRelArray` contents (`m_planes`) and can't re-read 31 DS3 chr physics files after writing them.
- **DB_NT needs an overlay** like NR's. HKLib reads only 123/710 files ("Incorrect hash").
- **HKLib 2018 is never byte-identical** (but round-trips the graph exactly). HKLib 2019 is byte-identical.

Scope that follows from "support everything the three libraries support": **in** are hk_2014 packfiles (DS2S, BB, DS3, SDT), Havok-5.x and hk_2010 packfiles (DeS, DS1: read, plus write where SoulsFormats can), and 2015/2016/2018/2019 tagfiles. **Out** (no library supports them today; optional later work) are `Havok-4.5` (DeS), `hk_2012` 32-bit (DS2) and the old binary tagfile (`0xCAB00D1E`, DS3/SDT behbnd `.hkt`).

---

## 2. Target design

### 2.1 Principles

1. **One serialization engine per container format, driven by type metadata.** HKLib's approach (registry + generic format handlers) wins over HKX2's (1,356 hand-emitted `Read`/`Write` methods). It's the only one of the three that scales to new SDKs without regenerating code paths, and it already supports reading and writing.
2. **Typed classes stay per SDK.** Layouts differ too much between hk_2010, hk_2014, 2016, 2018 and 2019 to merge into one class set. Smithbox code relies on typed access (`m_variant`, `m_systemDatas`…), so the generated classes stay. Field names already match across libraries (`m_` prefix), so moving most call sites is a namespace change.
3. **Game differences are registry overlays, not code switches.** HKLib already does this for NR (`HavokTypeRegistry20180100_NR.xml` on top of the base 2018 registry). Generalise it: BB over hk_2014, DS2 over hk_2014, DS1R over hk_2010, and so on.
4. **Sniff, don't trust the project type.** The format and SDK come from the file (packfile header version + `ContentsVersionString`, tagfile `SDKV`). The game profile only picks the overlay and the write defaults.

### 2.2 Proposed layout

```
src/Havok/
  Havok.Core/            IO primitives, type model, registry + overlay loading, profiles, facade
  Havok.Formats/         Packfile, Tagfile (TAG0/TCM0), XML serializers — all registry-driven
  Havok.Classes/         Generated classes per SDK: hk2010, hk2014, hk2016, hk2018, hk2019
                         + Manual/ partials (compressed mesh decode, tree codecs, hkHashMap, hkPropertyBag…)
  Havok.Classes.Reflection/  Generated HavokData accessors per SDK (from HKLib.Reflection)
  Havok.Builders/        BVH, collision mesh, navmesh builders (from HKX2/Builders), per SDK
  Havok.ClassGeneration/ One generator for every SDK (extends HKLib.ClassGeneration)
  Havok.Shared/          Cross-version interfaces (existing: IRootContainer, IClipGenerator…; extended)
```

Namespaces: `Havok.hk2010`, `Havok.hk2014`, `Havok.hk2016`, `Havok.hk2018`, `Havok.hk2019`. Splitting classes from formats keeps the slow-to-compile generated code out of the projects that change often.

### 2.3 Public API (sketch)

```csharp
public enum HavokContainer { Packfile, Tagfile, Xml }
public enum HavokSdk { Hk5x, Hk2010, Hk2014, Hk2015, Hk2016, Hk2018, Hk2019 }

public sealed record HavokProfile(string Game, HavokSdk Sdk, string? Overlay,
                                  int PointerSize, bool BigEndian, HavokContainer DefaultContainer)
{
    public static HavokProfile DeS, DS1, DS1R, DS2, BB, DS3, SDT, ER, NR, AC6, DB, DB_NT;
}

public sealed class HavokDocument
{
    public IHavokObject Root { get; set; }
    public HavokProfile Profile { get; }
    public HavokContainer Container { get; }
    public HavokFormatInfo Source { get; }      // header fields kept for byte-faithful rewrite
    public HavokCompendium? Compendium { get; } // the compendium it was read against, if any
}

public static class HavokFile
{
    public static HavokDocument Read(ReadOnlyMemory<byte> data, HavokReadOptions options);
    public static void Write(HavokDocument doc, Stream output, HavokWriteOptions? options = null);
    public static HavokCompendium ReadCompendium(ReadOnlyMemory<byte> data, HavokSdk sdk);
    public static HavokFileInfo Sniff(ReadOnlySpan<byte> data); // container, SDK, pointer size
}

public sealed class HavokReadOptions
{
    public HavokProfile Profile;          // picks the overlay; SDK is still checked against the file
    public HavokCompendium? Compendium;   // for TAG0 files with no TYPE section
    public bool Untyped;                  // read into HavokDynamicObject (replaces Renderer/HavokTagfile)
    public bool Lazy;                     // keep raw bytes for classes nobody touches (perf, see 4)
}
```

`Untyped` mode reads any tagfile, or a packfile with a registry, into a `HavokDynamicObject` (type + name→value map). That covers the renderer's schema-less animation reader and the Havok Editor's need to show types the generator doesn't know.

### 2.4 Format engines

**Tagfile (TAG0/TCM0).** Start from HKLib `hk2018/Binary` and `hk2019/Binary`. With version names normalised, `HavokBinaryReader`, `RecordFormatHandler` and `HavokSerializer` are identical between the two. The differences are in `HavokBinarySerializer` (TYPE section / compendium handling, ~490 changed lines each way), `BinarySerializeContext` (~30) and `ArrayFormatHandler` (~25). Merge into one serializer with a small `TagfileDialect` (2015 / 2016 / 2018 / 2019) that holds the differences. Add 2015 (DS1R) and 2016 (Sekiro) on the same path: they replace `HKX2.TagFileDeserializer`, which matches fields by name into DS3 classes, and gain writing. Compendiums are resolved by id (`TCRF` → `TCID`), because a binder can hold several.

**Packfile.** New, registry-driven. Reuse the container code (header v5/v8/v0x0B, the three sections, local/global/virtual fixups, class-name section, the extra 16-byte padding for v0x0B) from HKX2 `PackFileCommon`/`PackFileDeserializer`/`PackFileSerializer`. Those already handle header variants. The per-field encoding comes from the registry's member offsets, sizes and kinds instead of generated code. It needs:
- **Layout per pointer size.** Registries describe one layout. For 32-bit hk_2010 (DS1/DeS), either the registry stores 32-bit offsets directly, or a layout engine recomputes offsets from member kinds and alignment. Storing them directly is safer; the engine is only needed to *create* a registry.
- **Endianness** (DeS is big-endian PS3).
- **Write-order fidelity.** HKX2's serializer order (objects, then arrays and strings depth-first, then fixups) defines what "byte-identical" means for DS3. Copy that order exactly.

**XML.** Port HKLib `hk2018/Xml` onto the shared engine. It's a free win for all SDKs once the type model is shared, but nothing needs it beyond 2018 today.

### 2.5 Type registries

| SDK | Source for the registry | Overlays |
|---|---|---|
| Havok-5.x (DeS), hk_2010 (DS1) | Hand-written for the ~15 SoulsFormats classes, 32-bit; widen later from game files | DeS (big-endian, 5.x layouts) |
| 2015 (DS1R) | Pull TYPE sections from DS1R tagfiles (same survey as 2016) | — |
| hk_2014 (DS2S, BB, DS3, a few SDT) | Convert `HKX.ClassGeneration/classxmlds3` (1,362 dumps) into HKLib registry XML | BB (HKX2 reads only 382/721 BB files with DS3 layouts); DS2S (HKX2 reads all 227 with DS3 layouts, so possibly none needed) |
| 2016 (SDT) | Pull TYPE sections from Sekiro compendiums/tagfiles (a `Tools/` survey, like `HkTypeDiff`) | — |
| 2018 (ER, NR, DB, DB_NT) | Existing `HavokTypeRegistry20180100.xml` | NR (existing `_NR.xml`); **DB_NT (new: HKLib reads only 123/710 DB_NT files today)** |
| 2019 (AC6) | Existing `HavokTypeRegistry20190100.xml` | — |

Registries and overlays ship in `Smithbox.Data/Assets/HAVOK` (or stay in `Res/`, as now).

### 2.6 Cross-version surface

Extend `Havok.Shared` beyond the four marker interfaces, but only where Smithbox already has triplicated code: collision mesh access (vertices, triangles, materials), navmesh faces/edges, skeleton bones, animation track sampling. Each SDK's manual partials implement them. This collapses `HavokShapeGenerator_*`, `HavokCompressedMeshGenerator_*` and the `*_Helper` classes into one implementation each, wherever the underlying algorithm really is the same. Where it isn't (hknp 2014 compressed mesh vs 2018), keep separate implementations behind the shared interface.

---

## 3. Phases

Each phase ends with Smithbox building and the round-trip tool (Phase 0) passing for the formats it touched. Old libraries stay in place until Phase 7, so every phase can ship alone.

**Phase 0: baseline and round-trip harness.** `Tools/HavokLibSurvey` (C#). For every game in `game_data.txt` (ER via VFS), enumerate every `.hkx`/`.hkt`/compendium inside binders. Record container, SDK string, pointer size, endian and class names. Check whether each current library reads it, and whether a write → re-read gives the same bytes and object graph. Output: a support matrix in this folder (answers 1.3), plus a golden set of files per game. This harness is the acceptance test for every later phase. **Done 2026-11-06; see 1.3.**

**Phase 1: `Havok.Core`.** Move the type model (`HavokType`, `HavokTypeRegistry`, `HavokTypeBuilder`) out of `HKLib.Reflection/hk2018|hk2019` into one SDK-agnostic copy, with overlay loading generalised from the NR case. One set of binary IO helpers: HKLib.Serialization carries its own 2,000-line `BinaryReaderEx`/`BinaryWriterEx`. Pick either that or SoulsFormats' and drop the other. Add `HavokProfile`, `Sniff`, and the `HavokDocument` shell.

> **Status (2026-11-06): done, built alongside HKLib** (decision: HKLib isn't switched over until Phase 2). `src/Havok/Havok.Core`, in the solution, referenced only by `Tools/HavokLibSurvey`:
> - `Havok.Reflection.HavokType` / `HavokTypeBuilder` / `HavokTypeRegistry`: one SDK-agnostic copy. HKLib's abstract base plus per-SDK subclass are merged into one sealed class. C# binding is a resolver the registry is given (null for now). `Load(path, resolver, params overlays)` applies any number of overlays, checks each overlay's `Base` against the registry version, and records the overlay games.
> - **Per-SDK difference found:** the runtime → serialized `Optionals` bit map differs between 2018 and 2019 (the 2019 runtime moved five bits down by 3). Core picks the map from the registry's `Version`. 2015/2016 registries must check which map they need.
> - `HavokFileInfo.Read` (header sniffing: container, SDK, header version, pointer size, endian, class names, `TCRF`/`TCID` compendium ids). `HavokSdk`/`HavokContainer` enums with `IsInScope`. `HavokProfile` for all 13 games, with cached registries (DB_NT and DB use the ER registry until the DB_NT overlay exists; DB is unverified). `HavokDocument`/`HavokCompendium` shells. `IHavokObject` marker.
> - **Verified** by `HavokLibSurvey ALL 30 8 --core`: Core's ER, NR and AC6 registries match HKLib's type for type (4,734 / 4,734 / 2,969: name, size, alignment, flags, optionals, format, version, hash, parent, subtype, template, fields, properties, interfaces, presets), and `HavokFileInfo.Read` agrees with the survey's sniffer on all 7,213 files across the 12 games.
> - **Moved to Phase 2:** choosing the binary IO helpers. Nothing in Core reads or writes objects yet, so the choice is made where the merged tagfile serializer needs it.

**Phase 2: unified tagfile engine (2018 + 2019).** Merge HKLib's two binary serializers into one with a dialect. Point HKLib's 2018/2019 classes at it unchanged. Acceptance: ER, NR, AC6, DB_NT golden sets read, and the graph round-trips exactly. AC6 stays byte-identical (as today). ER/NR become byte-identical as a goal: today HKLib 2018 never is, so find what the 2019 path does differently. Add the DB_NT overlay here. This is the lowest-risk phase, because it's mostly de-duplication.

> **Status (2026-11-06): done, apart from DB_NT (moved to Phase 3).** Nothing in Smithbox uses the new engine yet (Phase 6).
> - **`src/Havok/Havok.Formats`** (Core + SoulsFormats, no HKLib): `HavokTagfileSerializer` reads and writes TAG0/TCM0 for any SDK through a `TagfileDialect`, on Core `HavokType`s, SoulsFormats' `BinaryReaderEx`/`BinaryWriterEx` (decision: SoulsFormats' IO) and any `IHavokClassSet` (registry, object creation, field access, the 3x3 matrix type). It also has `ReadTypeDescriptions` (a file's own types, without the registry) for type diffs and the planned untyped mode.
> - **`src/Havok/Havok.Formats.HKLib`**: `HKLib2018ClassSet`/`HKLib2019ClassSet` bridge the engine to HKLib's classes unchanged (HKLib's `IdentityTypeMap` for binding, its `HavokData` for field access). Retire it in Phase 7.
> - **Dialects.** The real 2018/2019 format differences are the SDKV string, `TSTR`/`FSTR` vs `TST1`/`FST1`, and 0x00 vs 0xFF string padding. Reading accepts every variant: partial type bodies, the 0x80 member extra value, an absent PTCH. The rest are writer switches: TCRF compendium reference, reusing the TYPE section read, SDK item order, 16-byte array alignment, DATA padding, compact PTCH. HKLib's 2019 copy had them; 2018's didn't.
> - **Two more 2018 behaviours found by the survey:** 2018 files store equal strings as separate items (`MergeEqualStrings = false`; reads give one string object per item, so a rewrite shares exactly what the original shared), and they patch `hkRelArray` pointers (`PatchRelArrays`). `Hk2018Legacy` reproduces HKLib 2018 exactly, as a parity check.
> - **Latent SoulsFormats bug:** `BinaryReaderEx.ReadBytes(buffer, index, count)` copies without advancing `Position`. The tagfile reader goes around it. SoulsFormats itself is unchanged.
> - **Verified** with `HavokLibSurvey ALL 30 8 --core` (the "Unified tagfile engine vs HKLib" section of each report):
>   - **Parity:** for every file HKLib reads, the unified engine reads an equal object graph. `Hk2018Legacy` writes the same bytes as HKLib 2018 on all of ER, NR and DB_NT. `Hk2019` writes the same bytes as HKLib 2019 on all 774 AC6 files.
>   - **Byte identity with the original files**, HKLib before → unified now: ER 0 → 667/670, NR 0 → 638/642, DB_NT 0 → 123/123 (of the 123 HKLib can read), AC6 773 → 773/774. Every write re-reads to an equal graph and is a fixed point. The 3 ER and 4 NR holdouts are chr `.hkx` files with every section the same size, so it's a value-level difference. Possibly non-0/1 booleans, which read as `true` and write as 1 (HKLib notes they exist). *(verify)*
> - **DB_NT, moved to Phase 3** (decision). DB_NT's types differ from ER's registry in 45 layouts, because `hkReferencedObject` is 4 bytes smaller. They also have renamed fields (`CustomManualSelectorGenerator.rideSync` → `additiveType`) and 6 new types, which HKLib's classes can't hold. Full list: [Havok Library Survey/DB_NT Types.md](Havok%20Library%20Survey/DB_NT%20Types.md). Phase 3 generates the overlay from the files' types and gives the generated classes DB_NT's fields.
> - **Not ported:** HKLib's 2018 XML serializer (out of scope here, as §2.4 says).

**Phase 3: generator and hk2014/2016 classes.** Extend `HKLib.ClassGeneration` to emit `Havok.hk2014` from the converted DS3 registry and `Havok.hk2016` from the Sekiro survey. Also emit `Havok.hk2015` for DS1R. Port HKX2's `Manual/` partials (static tree codecs, compressed mesh decode) and `Builders/` onto `hk2014`. Add the 2015 and 2016 dialects to the tagfile engine. **Also DB_NT (moved here from Phase 2):** generate its overlay from the files' own types (`ReadTypeDescriptions`, survey `--typediff`), and give `Havok.hk2018` the DB_NT-only fields and types. Acceptance: Sekiro and DS1R golden sets read into `hk2016`/`hk2015`, write, and re-read to an equal graph. The map renderer shows the same geometry as with `HKX2.TagFileDeserializer`.

> **Status (2026-11-06): hk2015 and hk2016 done. The rest is split off (decision): hk2014, hk2018/hk2019 in `Havok.*`, and DB_NT become separate later steps (Phase 3b and 3c below).** Nothing in Smithbox uses the new classes yet.
> - **Registries from game files.** Survey `--buildregistry` reads every type section of every SDT and DS1R tagfile and compendium (20,529 + 17,196 files). It writes `Havok.Core/Res/HavokTypeRegistry20160200.xml` (1,247 types, 441 hashes) and `HavokTypeRegistry20150100.xml` (466 types, 108 hashes), with no conflicting descriptions. These registries store serialized optionals (root `Optionals="Serialized"`) and property counts. Both Core and the generator load them. They have no enum value names, because files don't store them.
> - **2015 format:** the 2018 layout with `TNAM`/`TBOD` for type names and bodies. **2016:** exactly the 2018 layout. Reading accepts both names. The new dialects are `TagfileDialect.Hk2015`/`Hk2016`. Sekiro also stores empty strings separately (`ShareEmptyStrings = false`).
> - **`src/Havok/Havok.ClassGeneration`**: HKLib's generator made SDK-neutral. Target namespace is a parameter. Naming and placement are still borrowed from HKLib's 2018 classes, so SDT/DS1R code reads like ER code. Enums without presets take their values from the matching 2018 enum. Each SDK gets its own `IHavokObject` marker derived from Core's. Accessors are generated per class (decision) on `Havok.Formats.HavokFieldAccess<T>`. The 2015/2016 math spellings (`hkMatrix3f`, `hkMatrix4f`, `…Impl<float>`) are bound to `Matrix3x3`/`Matrix4x4`.
> - **`src/Havok/Classes/Havok.hk2016`** (527 declarations) and **`Havok.hk2015`** (174) are generated, and compile without hand edits. `GeneratedClassSet` (Havok.Formats) finds an assembly's accessors and creates them through compiled constructors. `Havok.Matrix3x3` is now in Core.
> - **Builder fix:** a type that only names its parent (e.g. 2016's `hkaiIndex<hkInt32>`) lost the parent's format and size when the parent was built first. Core's builder now inherits from built parents. HKLib's ER/NR/AC6 registries still load identically (`--core`).
> - **Verified** (`HavokLibSurvey SDT|DS1R 30 8`, against HKX2's name-matching reader, the only one that reads these SDKs today):
>   - **SDT:** 744/752 read, the same files HKX2 reads (the other 8 use a compendium in another binder). Graph and fixed point 744/744, and **744/744 byte-identical**.
>   - **DS1R:** 482/482 read, graph and fixed point all, **479/482 byte-identical**. The 3 holdouts lose `hkViewPtr` PTCH entries, probably null view pointers that 2015 patches. *(verify)*
> - **Regression caught and fixed:** adding `Havok.Matrix3x3` to Core silently changed what the HKLib bridge's unqualified `Matrix3x3` meant. Code inside `namespace Havok.*` resolves names from enclosing namespaces before `using` directives. The bridge now qualifies HKLib's. A full `ALL 30 8 --core` run after the fix matches Phase 2 exactly (ER 667/670, NR 638/642, AC6 773/774, DB_NT 123/123, full HKLib parity). The stray 2016 files in ER/AC6 now also read through hk2016. Code under `Havok.*` must qualify HKLib types.
> - **Phase 3b (later): hk2014.** Convert `HKX.ClassGeneration/classxmlds3` to a registry and generate `Havok.hk2014`. Port HKX2's `Manual/` and `Builders/`. Best done with Phase 4, whose packfile engine is what can read those classes.
> - **Phase 3c (later): hk2018/hk2019 in `Havok.*` and DB_NT.** Run the generator on the 2018/2019 registries, generate DB_NT's overlay from its files' types, add DB_NT's fields, and check against HKLib on the survey.

**Phase 4: packfile engine (hk2014).** Registry-driven packfile reader/writer. Acceptance: every DS3 golden file reads into `hk2014` and gives the same graph as HKX2. Writes match the original file wherever HKX2's do today (667/859 DS3 files). The new writer must *also* re-read cleanly where HKX2 doesn't: HKX2 drops `hkRelArray` data (`m_planes`) and fails to re-read 31 chr physics files, so HKX2 output is not the reference for those classes. Then the BB overlay, then DS2S (check whether it needs one).

**Phase 5: 32-bit packfiles (DeS Havok-5.x, DS1 hk_2010).** Hand-built registry for the classes SoulsFormats handles, 32-bit layout, big-endian for DeS. SoulsFormats only reads 61/329 DeS files and can't write skeletons or spline animations, so parity is the floor, not the target. Acceptance: skeletons, animations, collision and navmesh decode the same as SoulsFormats. Writing should at least be as good as SoulsFormats today (round trip re-reads equal).

**Phase 6: migrate Smithbox.** In order of risk, smallest first:
1. Renderer animation (`HavokAnimationData`, `HavokTagfile` → `Untyped`/typed reads).
2. Renderer collision/navmesh resources and Map Editor banks (`HavokCollisionResource`, `HavokNavmeshResource`, `HavokCollisionBank`, `HavokNavmeshBank`, `WallBank`, the `*_Helper` classes).
3. Havok Editor (`HavokBank.ReadRoot`/`WriteRoot` → `HavokFile`; `HavokTagSerializer` and `HKLibTagfileReader` go away; `HavokFileWrapper` holds a `HavokDocument`). Turn on Havok Editor support for the games that are newly readable *only* after their round trip passes.
4. Collision Generator and NavmeshBuilder, collapsing the triplicates through the 2.6 interfaces.
5. Model Editor, Param mass edit, developer tools.

**Phase 7: retire.** Move `HKX2`, `HKX2.Serialization`, `HKX.ClassGeneration`, `HKLib*` and `ColBakeTest` (if unused) to `.archive`, and remove their project references. Leave SoulsFormats `HKX` in place but unreferenced by Smithbox (see decision D3).

---

## 4. Risks

- **Performance.** HKX2's straight-line generated code is fast. A registry-driven engine adds a dispatch per field. HKLib already uses generated `HavokData` accessors (not `System.Reflection`) to keep this cheap. Keep that pattern for hk2014/2016, and measure map-load time for DS3 collision in Phase 4 against HKX2.
- **Byte fidelity.** Packfile writers differ in padding, string pooling and object order. Phase 0 must record which existing writers are already faithful, so "identical to before" is a fair bar.
- **Registry quality for hk_2014.** `classxmlds3` came from reflection dumps that HKX2 trusted. Any gaps already show up as HKX2 bugs. Phase 0's graph comparison will expose them.
- **32-bit layouts.** Nothing in the repo describes hk_2010 layouts beyond SoulsFormats' hand code. Keep the hk2010 registry to what that code proves.
- **Generated code volume.** Adding hk2014 (~1,350) and hk2016 classes plus accessors roughly doubles the generated code. Keep them in separate projects so they only rebuild when the generator runs. Build single projects, not the `.sln` (it rewrites `Smithbox.Release/Output`).
- **Editor reflection.** The property view uses `System.Reflection` on fields. That keeps working across SDKs, as long as generated classes keep public `m_` fields.

---

## 5. Decisions needed

All five were decided on 2026-11-06. The plan above already reflects them. Next step: Phase 0 (`Tools/HavokLibSurvey`).

- **D1. Typed classes per SDK vs one dynamic object model.** **Decided (2026-11-06): typed per SDK** (2.1 #2), with `Untyped` mode for unknown types. A dynamic-only model would mean rewriting every Smithbox call site.
- **D2. Scope of hk2010 writing.** **Decided (2026-11-06): match SoulsFormats** (round-trip re-reads equal), not a full hk2010 class set, until a Smithbox editor needs it.
- **D3. SoulsFormats `HKX`.** **Decided (2026-11-06): leave it in Andre, unreferenced**, because SoulsFormats is shared upstream code. The alternative is archiving it.
- **D4. Sekiro writing.** **Decided (2026-11-06): include it in Phase 3** (acceptance adds: SDT golden files write → re-read to an equal graph), because the unified tagfile engine makes it nearly free. The alternative is keeping SDT read-only.
- **D5. Package naming.** **Decided (2026-11-06): `Havok.*` namespaces** (`Havok.hk2018`). The alternative is keeping `HKLib.*` names to shrink the Phase 6 diff for ER/AC6 code.
