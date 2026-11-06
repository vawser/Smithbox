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
> - **Phase 3c: hk2018/hk2019 in `Havok.*` and DB_NT. Done 2026-11-06**, see below.

**Phase 3c: hk2018/hk2019 classes and DB_NT.**

> **Status (2026-11-06): done.** Nothing in Smithbox uses these classes yet (Phase 6).
> - **`src/Havok/Classes/Havok.hk2018`** (2,279 declarations) and **`Havok.hk2019`** (1,629) are generated from HKLib's registries. hk2019 keeps HKLib's hand-written compressed-mesh decode helpers in `Manual/`. `hkPropertyBag` is hand-shaped in every SDK, as in HKLib: an empty class, since its one field isn't serialized. hk2015/hk2016 were regenerated with this too.
> - **Game variants in one class set.** The generator takes `--overlay` once per game. It loads the base registry with each overlay applied, and gives every class the union of all variants' fields. One `Havok.hk2018` therefore holds ER, NR (`CustomManualSelectorGenerator.isBasePoseAnim`) and DB_NT (`additiveType`, `hkcdSimdTree::Node.parent`, `CustomDockingGenerator`…). Each game's registry still describes its own layout, so reads and writes follow it.
> - **DB_NT overlay.** Survey `--buildoverlay` builds `Havok.Core/Res/HavokTypeRegistry20180100_DB_NT.xml` from every DB_NT file:
>   - **45 changed types** replace the base type with the same Id. They keep the base's runtime-only optionals bits, enum names and, where the files have none, hashes.
>   - **6 new types** are added. Overlays now accept `New="true"` types, on Ids continuing from the base's highest. Both Core and the generator load them.
>
>   DB_NT's 1,143 described types all match the DB_NT registry (`--typediff`). DB_NT's new enum `AdditiveType` has no names in any file, so `m_additiveType` is a `byte`.
> - **Verified** (`HavokLibSurvey <game> 30 8`, "Generated 2018/2019" rows):
>
>   | Game | Read: HKLib | Read: generated | Same data as HKLib | Same bytes as HKLib-backed engine | Byte-identical to original |
>   |---|---|---|---|---|---|
>   | ER | 670 / 670 | 670 / 670 | all | all | 667 / 670 |
>   | NR | 642 / 642 | 642 / 642 | all | all | 638 / 642 |
>   | AC6 | 774 / 774 | 774 / 774 | all | all | 773 / 774 |
>   | **DB_NT** | **123 / 710** | **710 / 710** | all comparable | all comparable | **604 / 710** |
>
>   Every write round-trips: graph equal and fixed point. DB_NT's 106 non-identical files are almost all value-level: same section sizes, with the first difference a single byte inside DATA. That's probably non-0/1 booleans written back as 1, the same pattern as the ER/NR holdouts. *(verify)*
> - **Two generator bugs found by the survey:** HKLib's `HavokType` equality is by identity, so game variants of a type must be told apart by reference; and generated classes may hold extra variant fields, so the cross-class-set compare accepts them.

**Phase 4: packfile engine (hk2014).** Registry-driven packfile reader/writer. Acceptance: every DS3 golden file reads into `hk2014` and gives the same graph as HKX2. Writes match the original file wherever HKX2's do today (667/859 DS3 files). The new writer must *also* re-read cleanly where HKX2 doesn't: HKX2 drops `hkRelArray` data (`m_planes`) and fails to re-read 31 chr physics files, so HKX2 output is not the reference for those classes. Then the BB overlay, then DS2S (check whether it needs one).

> **Status (2026-11-06): done for DS3, DS2S, SDT and BB, with Phase 3b (hk2014 classes) folded in.** Nothing in Smithbox uses this yet (Phase 6).
> - **hk2014 registry.** `Tools/HavokClassDumpConvert` turns HKX2's 1,362 DS3 class dumps into `Havok.Core/Res/HavokTypeRegistry20140100.xml` (2,516 types). Packfile reflection (`TYPE_ARRAY` of `TYPE_STRUCT`, `TYPE_ENUM` + `etype`…) becomes tagfile-style types with the 2018 registry's formats (`hkArray<T>`, `T*`, `hkEnum<E, S>`, `hkVector4`, `hkQsTransform`…). Class signatures become hashes. Nested classes take HKLib's 2018 identity (470 of them, e.g. `hkRootLevelContainer::NamedVariant`). Enums come with their names from the dumps. 3 enums stay raw ints, because no class declares them uniquely. 2 classes have no dump, so empty records stand in for them. The DS3/BB/DS2S profiles use it.
> - **`src/Havok/Classes/Havok.hk2014`** (1,573 declarations), generated like the other SDKs, so DS3 code reads like ER code (`hkaSkeleton.m_referencePose` is a `List<hkQsTransform>`, etc.).
>   - The generator now drops an empty class whose name is also a namespace (`hkaiHierarchyUtils` & co.).
>   - HKX2's `Manual/` (static tree codecs, compressed mesh decode) and `Builders/` (BVH, collision mesh, navmesh) are ported into it. Their numbered fields (`m_x_0`) are now arrays (`m_x[0]`), and their flat names are now nested.
> - **`Havok.Formats/Packfile/HavokPackfileSerializer`**, driven by the registry:
>   - **Reading** follows the fixups: virtual (class), global (pointers), local (arrays, strings). It also reads `hkRelArray` and `hkSimpleArray`, which HKX2 ignores. It handles 4/8-byte pointers and either byte order, ready for Phase 5.
>   - **Writing** ports HKX2's queue algorithm literally (object order, nested local queues, padding, fixup order), so HKX2's byte output is reproduced. It also writes rel and simple arrays.
>   - **`PreserveSource`** (default) writes back the header and `__types__` section a root was read with. When off, it writes HKX2's fixed DS3 header and an empty `__types__` section.
>   - HKX2's stale-queue-index crash on one DS3 behaviour file is fixed: the queue list grows instead.
> - **Bugs the survey caught along the way:** `hkSimpleArray` is 12 bytes, not 16 (the next member can follow at +12). And a C# switch over `float`/`double` arms silently widened every float to `double`.
> - **Verified** (`HavokLibSurvey <game> 30 8`):
>
>   | Game | Read: HKX2 → engine | Writes | Round trip (graph): HKX2 → engine | Byte-identical: HKX2 → engine |
>   |---|---|---|---|---|
>   | DS3 | 860 → 860 | 859 → **860** | 810/829 → **860/860** | 667 → **668** |
>   | SDT (packfiles) | 9 → 9 | 9 → 9 | 9 → 9 | 6 → 6 |
>   | DS2S | 227 → 227 | 227 → 227 | 227 → 227 | 0 → 0 |
>   | BB | 382 → **648** | 382 → 648 | 382 → 648 | 0 → **40** |
>
>   - **Parity with HKX2:** the engine reads the same data as HKX2 on all 860 DS3 files. Comparing across the two class sets bridges flat and nested names, HKX2's `Matrix4x4` stand-ins for `hkQsTransform`/`hkMatrix3`, its raw-`Int16` `hkHalf`, and enums typed as ints. In HKX2-header mode the engine writes HKX2's exact bytes wherever HKX2 handles the data (DS3 810/859, BB 382/382, SDT 9/9). The other DS3 files carry rel/simple arrays, which HKX2 writes as zeros.
>   - **Compressed mesh decode** (the hk2014 port vs HKX2's): 4,827,782 triangles identical across 239 DS3 files. On 4 more, the decode throws inside HKX2 (both copies run the same code, so the port probably throws there too). *(verify)*
>   - **DS2S** files carry about 51 KB of `hkClass` reflection in `__types__`. It's now preserved, but DS2S rewrites still differ, at an odd offset in DATA (value-level). DS2S reads differ from HKX2 only because HKX2 drops `CustomMeshParameter`'s simple arrays.
>   - **DS3's remaining 192 non-identical files** mostly differ in the data section's length (objects laid out differently from the dumps). A few differ in a section tag's padding byte.
> - **BB overlay: done (2026-11-06).** BB's packfiles carry no type sections, so the overlay is computed rather than read:
>   - **Cause.** BB (PS4) and DB_NT (Switch) are clang builds, which follow the Itanium C++ ABI: a derived class's first members go into the tail padding of its base, where MSVC (PC) starts them after the base's full size. That is the whole of DB_NT's "`hkReferencedObject` is 4 bytes smaller", and of BB's shifted fields (`hkaAnimation.type` at 12, as SoulsFormats' hand-written BB code already had it).
>   - **`Havok.Reflection.HavokLayout`** (Core) recomputes a registry's record layouts under `HavokLayoutRules` (MSVC or Itanium). Gaps the source layout can't explain from its members (vtables, unlisted members) are kept as opaque blocks. Survey `--layoutcheck` writes [Havok Library Survey/Layout Check.md](Havok%20Library%20Survey/Layout%20Check.md):
>     - **MSVC recompute is exact** for every registry (DS3, DS1R, SDT, ER, NR, AC6, DB_NT).
>     - **Itanium on ER's registry predicts DB_NT's files.** Survey `--layoutpredict` checks every record layout DB_NT's 2,175 surveyed files describe: 393 match (355 unchanged, 38 moved as predicted), **0 mispredicted**. The only differences are 3 types with new members. That needs two rules the files prove: a class's declared alignment counts (`hkCompressedMassProperties` is 8-aligned), and `hknpExternMeshShape`'s 8 unlisted PC bytes don't exist (`SourceOnlyGaps`, probably an empty secondary base MSVC gives storage).
>   - **`Havok.Core/Res/HavokTypeRegistry20140100_BB.xml`** is written by survey `--buildlayoutoverlay BB`: the DS3 registry under `HavokLayoutRules.ItaniumBB`. 229 records change. It's the BB profile's overlay. Two hk_2014-specific findings, each proven from BB bytes:
>     - The HKX2 class dumps under-report alignment. A record whose size is its members' end rounded up to 16 is 16-aligned, not hiding members (`inferAlign16`, hk2014 only). This is what puts `hknpConvexPolytopeShape.planes` at 44 in BB.
>     - `hknpShape`'s members stay at DS3's offsets (flags@16, not 12; `NoBaseTailReuse`).
>   - **`Tools/HavokPackfileProbe --layout`** checks a type-less packfile against its profile's registry. Every global/local fixup in every object, and in every element of its arrays of records, must sit on a pointer/array member. It also dumps a class's bytes (`PROBE_DUMP=<class>`) and prints the read's stack. It now reads `.bnd` containers too.
>   - **Verified** (`HavokLibSurvey BB 30 8`): read 648 → **721/721**, byte-identical 40 → **619/721**, graph round trip and fixed point 721/721. DS3 and DS2S are unchanged (they don't use the overlay). "Same read as HKX2" is now 53/382, as it should be: HKX2 reads BB with DS3 layouts. The 102 non-identical BB files look like DS3's: a few behaviour and ragdoll files whose data section length differs, and same-size value-level differences in animations. *(verify)*
>   - **For Phase 5:** the same engine is how hk2010 32-bit layouts can be derived (pointer size is the next rule), checked the same way against SoulsFormats' hand code.

**Phase 5: 32-bit packfiles (DeS Havok-5.x, DS1 hk_2010).** Hand-built registry for the classes SoulsFormats handles, 32-bit layout, big-endian for DeS. SoulsFormats only reads 61/329 DeS files and can't write skeletons or spline animations, so parity is the floor, not the target. Acceptance: skeletons, animations, collision and navmesh decode the same as SoulsFormats. Writing should at least be as good as SoulsFormats today (round trip re-reads equal).

> **Status (2026-11-06): done for DS1, DeS 5.5 and DeS 5.1. The acceptance test passes: every object SoulsFormats decodes, the engine decodes the same (or SoulsFormats is wrong), over every DS1 and DeS file. Every DS1 and DeS file reads, and every DS1 file rewrites byte-identical. The two open fixes are closed. What's left is three DeS 5.5 oddities and the 5.1 animations (see the end of this section).** Nothing in Smithbox uses this yet (Phase 6).
> - **Decision (2026-11-06): the registries come from the games' own files, and the class sets are generated** (revises D2's "no full hk2010 class set"). DeS packfiles, and DS1's chr physics files, carry Havok's `hkClass` reflection in `__types__`. The layout is the same in Havok 5.5 and hk_2010, 32-bit:
>   - `hkClass` 48 bytes, `hkClassMember` 24, `hkClassEnum` 20, enum items 8.
>   - Type codes are `hkClassMember::Type`.
> - **Survey `--dumpclasses`** writes that reflection, from every file of DS1 (11,166) and DeS (12,439), as class dumps in HKX2's `classxmlds3` format: `src/Havok/ClassDumps/<GAME>_<SDK>/`, plus `_used.txt` (the classes the files use, with their signatures).
>   - Described: DS1 131 classes, DeS 5.5 107, 5.1 82 (and 4.1/4.5, out of scope).
>   - **Exporters only embedded the classes they didn't consider built in**, so the core ones are never described: DS1's spline animation, reference frame and collision shapes; DeS's MOPP and mesh shapes.
> - **Survey `--filldumps`** borrows each used-but-undescribed class from the nearest SDK that has it, laid out again for the target (pointer size, the target's parents, Itanium tail reuse for PS3). Results in [Havok Library Survey/Dump Fill.md](Havok%20Library%20Survey/Dump%20Fill.md):
>   - **How far it can be trusted.** For classes both sets describe with the same members, the donor predicts the target exactly: DS3 → DS1 95, DS1 → DeS 5.5 47, DeS 5.5 → 5.1 67. The only misses are multiple-inheritance classes (`hkpCollisionFilter`, `hkpShapeCollection`), whose secondary-base vtables no member lists. Target classes therefore keep their recorded layouts, and a borrowed class whose donor layout doesn't recompute is flagged and kept verbatim.
>   - Donors: DS1 ← DS3; DeS 5.5 ← DeS 5.1, DS1, DS3; DeS 5.1 ← DeS 5.5, DS1, DS3. Borrowed dumps carry `borrowed="…"` and the target's signature. Stale ones from an earlier fill go to `.archive`.
>   - **Found nowhere:** DS1 `hkaWaveletCompressedAnimation`; DeS `hkaDeltaCompressedSkeletalAnimation` (DeS animations, which no library reads), `CustomMeshParameter::DATA_ELEMENT` and some `hkxVertex*` formats.
> - **Registries:** `Tools/HavokClassDumpConvert` now takes a pointer size, version and game. It writes `HavokTypeRegistry20100200.xml` (DS1, 312 types), `05050000` (DeS 5.5, 342) and `05010000` (DeS 5.1). Survey `--registryoverlay` turns 5.1 into an overlay on 5.5 (`…05050000_DES_51.xml`, 27 changed types). Profiles: `DS1`, `DES`, `DES_51` (`HavokProfile.For(game, sdk)` picks it).
>   - `hkVariant.object` is untyped on 32-bit, because Havok 5.x roots aren't `hkReferencedObject`s.
>   - `TYPE_HOMOGENEOUSARRAY` (5.5 `hkxVertexBuffer`) is kept as space only.
> - **Classes:** `src/Havok/Classes/Havok.hk2010` (167 declarations) and `Havok.hk55` (174, holding 5.5 and 5.1), generated and compiling without hand edits. Both are in the solution.
> - **Packfile engine** (`HavokPackfileSerializer`). Writing now follows the source file:
>   - Pointer size, byte order and section order (5.1 puts `__data__` before `__types__`).
>   - Its class-name section (old SDKs list every class), so names keep their offsets.
>   - Its fixup-table lengths (Havok 5.5 leaves empty rows).
>   - Each `hkArray`'s capacity/flags word (5.5 uses `0xC0000000`).
>   - The bytes of unserialized members, where they weren't zero (DeS keeps a subpart's material stride in one).
>   - `hkVariant` class pointers into the kept `__types__` section.
>   - Unserialized `hkArray`s are written as empty arrays (`0x80000000`), as Havok writes them. This also helps DS3.
>
>   Reading: `hkUlong` is pointer-sized, and strings decode as Shift-JIS where that round-trips (DeS and DS3 hold Japanese names), else Latin-1. All the fidelity features apply only with `PreserveSource`, so the HKX2-header mode still checks HKX2 parity.
> - **DeS 5.5 mesh shapes, hand-fitted (2026-11-06).** No DeS file describes `hkpExtendedMeshShape` and its storage classes, and DS1's 2010 versions are different classes, so DeS map collision lost its mesh storage on read. Their 5.5 layouts were fitted to the bytes of DeS map collision (`l0000b0`, `l0042b0`, `h5000b1`, `o5985`, `o3270`). They're written as dumps with `manual="…"` (the evidence), which later fills and extractions leave alone.
>   - **`hkpExtendedMeshShape`** (192 bytes): `numBitsForSubpartIndex`@24, `scaling`@32 (as in DeS 5.1's `hkpMeshShape`), `aabbHalfExtents`@48, `aabbCenter`@64, `trianglesSubparts`/`shapesSubparts` as simple arrays @80/88, `weldingInfo`@96, `weldingType`@108 (6 = none), an embedded `TrianglesSubpart`@112, then four zero words.
>   - **`Subpart`** is 16 bytes, with no userData. Its `materialStriding` is 4, the size of DeS's `hkpMeshMaterial`.
>   - **`TrianglesSubpart`** is 64 bytes, with no transform and 32-bit strides. `IndexStridingType` uses the 5.x numbering (INT16 = 1).
>   - **`hkpStorageExtendedMeshShape`** has `meshstorage`@192 and `shapestorage`@204, size 224, data size 216. `CustomParamStorageExtendedMeshShape.materialArray` is at 216, which is PS3 tail reuse.
>   - **`MeshSubpartStorage`** is 80 bytes with no `indices8` (as in SoulsFormats' DeS reader), and its materials are 4-byte `hkpMeshMaterial`s.
>   - **Cross-checks:** the subpart's triangle count equals the storage's material-index count, its vertex count the vertex count, and the 16-bit index count is 4 × the triangle count.
>   - **Unverified:** `ShapesSubpart` and `ShapeSubpartStorage` (no surveyed file has one), and the empty array at `MeshSubpartStorage`+68 (assumed `materialIndices16`).
>   - The fill now also borrows what the target's own classes refer to (`hkpMeshMaterial`).
>   - **Verified:** every surveyed DeS map file (28 loose) and object file (56 objbnd) rewrites byte-identical, and every fixup lands on a member (`--layout`). DeS 5.5 byte identity is 267 → **303/327**. The other 24 are chr ragdolls (22 chrbnd, 2 loose). Survey reports now have a "Byte-identical writes by category" table, which names the files when a category has 5 or fewer misses.
> - **Probes** (`Tools/HavokPackfileProbe`): `--types` dumps a file's reflection, `--diff` compares a rewrite part by part (naming the objects of lost fixups), and `--layout` now handles 32-bit big-endian files. Loose files: pass `-` as the inner name.
> - **Verified** (`HavokLibSurvey <game> 30 8`):
>
>   | Game / SDK | Read: SoulsFormats → engine | Write | Graph round trip | Byte-identical: SoulsFormats → engine |
>   |---|---|---|---|---|
>   | DS1 hk_2010 | 328 → **331/331** | 331 | 331 | 38 → **298** |
>   | DeS Havok-5.5 | 59 → **327/327** | 327 | 327 | 0 → **303** |
>   | DeS Havok-5.1 | 2 → 2/2 | 2 | 2 | 0 → **2** |
>
>   The fidelity changes also raised the hk_2014 games: DS3 668 → **693/860**, BB 619 → **636/721**, DS2S 0 → **227/227** byte-identical. DS3 reads now differ from HKX2 in 25 files, all Shift-JIS names that HKX2 reads as `?` (`オブジェクト100093` vs `?I?u?W?F?N?g100093`).
> - **Decode comparison with SoulsFormats (the acceptance test), 2026-11-06.** The survey (`DecodeCompare`, in every DS1/DeS report) matches each object SoulsFormats decodes into a class of its own to the engine's object at the same data-section offset (the engine now exposes `LastObjects`), then compares them value by value, floats bit-exact. Over **every** file (`HavokLibSurvey DS1|DES 3000 40`: 11,166 DS1 and 12,439 DeS):
>
>   | Class | DS1 equal | DeS equal |
>   |---|---|---|
>   | skeleton (names, parents, bones, reference pose) | 692/692 | — |
>   | spline animation (counts, durations, block and track offsets, data) | 6,926/6,926 | — |
>   | animation binding (track maps, skeleton name) | 6,926/6,926 | — |
>   | collision mesh storage (vertices, indices, material indices) | 6,945/6,945 | 4,920/4,920 |
>
>   - **SoulsFormats is wrong in two places, and the engine right:**
>     - **DS1 bindings' `originalSkeletonName`:** SoulsFormats reads it inline after `blendHint`, where only padding is, so it gets "". The name is the string the binding's first member points at (`master`, `o3321`…).
>     - **Two Shift-JIS bone names:** SoulsFormats decodes them as ASCII (`master ?G?N?X?g??01Nub` for `master エクストラ01Nub`).
>   - **DeS has nothing else to compare:** SoulsFormats reads no DeS skeleton or animation, and its DeS MOPP objects are generic.
>   - **Found and fixed at full scale:**
>     - The fill laid borrowed classes out on a verbatim-kept parent by recomputing that parent, which lost `hkpShapeCollection`'s hidden vtable pointer. 44 DeS `hkpSimpleMeshShape` writes failed; they now use the recorded layout.
>     - DeS 5.5's `hkpBallAndSocketConstraintData` has no `setupStabilization` atom (hand-fitted, 80 bytes), so 2 objbnd reads failed.
>   - **Full-scale totals (before the animation fit):**
>     - DS1: 11,151/11,166 read, all of them written, round-tripped and at a fixed point, and 10,830 byte-identical.
>     - DeS 5.5: 11,080/11,086 read, all written, 10,809 byte-identical.
>     - DeS 5.1: 266/266 read and written, 219 byte-identical.
>   - **The last two animation formats, hand-fitted (2026-11-06).** No file describes them, and no library read them. Their fields cross-check against each other:
>     - **DS1 `hkaWaveletCompressedAnimation`** (112 bytes, on `hkaAnimation`'s 40): DeS 5.5's wavelet members plus static/dynamic DOF counts, and `dataBuffer` as an `hkArray`@100. Checks: block index + 4 × its size = quantized data index; quantized index + size = the buffer's length; dynamic DOFs = the format's `numD`.
>     - **DeS `hkaDeltaCompressedSkeletalAnimation`** (104 bytes): poses and block size, the 20-byte quantization format, then quantized data, static mask and DOFs, total and last block sizes, and `dataBuffer` as a simple array@96. Checks: 11 × `totalBlockSize` + `lastBlockSize` = the quantized size (12 blocks of 8 poses), and quantized index + size = the buffer's count. `staticMaskSize`/`staticDOFsSize` hold pointer-like values (`0x1E65…`), apparently left uninitialized by the exporter; they're kept as plain 32-bit values.
>     - **Result over every file:** DS1 reads **11,166/11,166** (10,845 byte-identical) and DeS 5.5 **11,086/11,086** (10,815). All 21 formerly unreadable files rewrite byte-identical. `HavokPackfileProbe` now takes `outer>inner` patterns for binders inside binders.
>   - **Regenerating** after a dump change:
>     1. Build the survey.
>     2. `--filldumps`.
>     3. `HavokClassDumpConvert` for each SDK.
>     4. `--registryoverlay` (5.1 on 5.5).
>     5. Move both `Autogen` folders to `.archive`.
>     6. `Havok.ClassGeneration` for hk2010 and hk55 (5.5 with `--overlay` 5.1).
>     7. Build the survey and probe.
> - **The two open fixes, closed (2026-11-06):**
>   - **DS1 `hkpConvexVerticesShape`, hand-fitted** (`o2770_1.hkx`). hk_2010.2 has 4 unnamed, always-zero bytes after `useSpuBuffer`, so `planeEquations` is at 88 and `connectivity` at 100 (DS3's relaid layout had 84/96, which dropped both connectivity objects and shifted everything after). Size 112 stays. The translate-shape chain was only a knock-on effect. Checks: `rotatedVertices` count = ⌈numVertices / 4⌉, and the connectivity's face count matches the planes (68/68, or 56 + 6 bevel planes = 66). `--filldumps` now reports the class as not recomputing (the unlisted bytes), so it keeps the fitted layout.
>   - **DeS ragdolls were global fixup *order*, not a misplaced pointer.** Havok 5.5 writes the global fixup table in member order. An array's element pointers come at the array member, so `hkaRagdollInstance.skeleton` (+44) comes after every `rigidBodies`/`constraints` element. Sorting by offset only agrees when no pointer member follows an array of pointers. The writer now records member order (one node per object, with each array's contents as a child node). It uses that order when the source's table wasn't sorted by offset (`GlobalsInMemberOrder`, detected on read). Sorted sources, and HKX2-header mode, still sort. `HavokPackfileProbe --diff` now shows where two tables' orders part (`PROBE_ROWS=<n>` sets how many rows).
> - **Full-scale result** (`HavokLibSurvey DS1|DES 3000 40`). Every file reads, writes, round-trips and is at a fixed point, and the decode comparison is still equal on every object:
>
>   | SDK | Byte-identical before → now |
>   |---|---|
>   | DS1 hk_2010 | 10,845 → **11,166 / 11,166** |
>   | DeS Havok-5.5 | 10,815 → **11,083 / 11,086** |
>   | DeS Havok-5.1 | 219 → 219 / 266 |
>
>   The committed 30×8 reports: DS1 298 → **331/331**, DeS 5.5 303 → **327/327**. The writer change leaves the hk_2014 games exactly as they were (DS3 693/860, BB 636/721, DS2S 227/227, SDT 6/9): their sources' tables are sorted.
>
> - **Still not byte-identical** (all newly identified; none is a ragdoll):
>   - DeS 5.5 `m99_99_98_10/l0000b0.hkx` (a test map): its `CustomMeshParameter` (borrowed from DS3) has a different layout there, and 2,768 bytes are lost.
>   - DeS 5.5 `o2999.hkx` (stored twice, `.objbnd` and `.objbnd.dcx`): an exporter leftover with `hkxMesh`/`hkxVertexBuffer` scene data. 1.25 MB is lost, because `TYPE_HOMOGENEOUSARRAY` is kept only as space.
>   - ~~DeS 5.1: 47 files~~ **Fixed in 6.1: all 266/266 are now byte-identical.** The cause wasn't the files. `GeneratedClassSet` cached class sets by game name, so the `DES` and `DES_51` profiles shared one entry, and in a full run the 5.1 files were read with the 5.5 registry. (`--diff` still assumes `__data__` is the third section, which 5.1 breaks.)

**Phase 6: migrate Smithbox.** In order of risk, smallest first:
1. Renderer animation (`HavokAnimationData`, `HavokTagfile` → `Untyped`/typed reads). **Done 2026-11-06**, see [Havok Phase 6.1 Renderer Animation.md](Havok%20Phase%206.1%20Renderer%20Animation.md) and below.
2. Renderer collision/navmesh resources and Map Editor banks (`HavokCollisionResource`, `HavokNavmeshResource`, `HavokCollisionBank`, `HavokNavmeshBank`, `WallBank`, the `*_Helper` classes). **Rendering done 2026-11-06**, see [Havok Phase 6.2 Collision and Navmesh.md](Havok%20Phase%206.2%20Collision%20and%20Navmesh.md) and below. The ER/AC6 bank containers (edited and saved) move with 6.3.
3. Havok Editor (`HavokBank.ReadRoot`/`WriteRoot` → `HavokFile`; `HavokTagSerializer` and `HKLibTagfileReader` go away; `HavokFileWrapper` holds a `HavokDocument`). Turn on Havok Editor support for the games that are newly readable *only* after their round trip passes.
4. Collision Generator and NavmeshBuilder, collapsing the triplicates through the 2.6 interfaces.
5. Model Editor, Param mass edit, developer tools.

> **6.1 status (2026-11-06): done, and checked in the app** (the user confirmed animations play correctly). The renderer's animation reads go through the unified library. Smithbox's first use of it.
> - **Decision: shared interfaces** (§2.6). `Havok.Shared/Animation.cs` has the views:
>   - `IHavokSkeleton`, `IHavokAnimationBinding`, `IHavokAnimation`.
>   - `IHavokSplineAnimation`, `IHavokInterleavedAnimation`.
>   - `HavokQsTransform`.
>
>   Each class set implements them in `Manual/Animation.cs`. All seven sets were regenerated with those classes on the generator's partial list. Each regeneration diffs from the old Autogen (now in `.archive/Havok Phase 6.1 Autogen`) by the `partial` keyword only.
> - **`src/Havok/Havok.Files`: the `HavokFile` facade** (§2.3). It's a project of its own because it references every class set, and they reference Core.
>   - `Sniff`, `ReadCompendium` and `Read`. `Read` returns a `HavokDocument`, now with `Objects` and `ObjectsOf<T>()`.
>   - The file's SDK picks the engine, dialect and class set.
>   - A file of another SDK (ER's stray 2016 files) is read with that SDK's own game profile, and DeS 5.1 files with `DES_51`.
> - **Renderer.** `HavokAnimationReader` has one path for every game. `AnimationPackage` parses each binder's compendium once, not per clip. `Renderer/Animation/HavokTagfile.cs` (the schema-less reader) is archived. The renderer no longer uses SoulsFormats `HKX` for animation. It still uses SoulsFormats' spline block decoder, which works on the clip's bytes.
> - **Verified** by `Tools/AnimationReadCompare` (the old reader's code against the new one, 40 anibnds × 40 clips per game): [Havok Library Survey/Animation Read Compare.md](Havok%20Library%20Survey/Animation%20Read%20Compare.md).
>   - Every skeleton and clip is equal, floats bit for bit, in DS1, DS1R, DS3, SDT, ER, NR and AC6.
>   - The one BB difference is the new reader being right: a Shift-JIS bone name (`不要_Weapon_01`), which SoulsFormats decoded as ASCII.
>   - DeS now reads (32 skeletons, 1,001 clip files; the old reader read none). But only 90 of its clips are interleaved. The rest are wavelet or delta compressed, and the renderer has no decoder for those, so DeS stays off in `AnimationPackage.IsSupportedGame`.
> - **Bug found on the way (6.1):** `GeneratedClassSet` cached class sets by game name, so `DES` and `DES_51` shared one entry, and whichever loaded first served the other's files. It's now keyed by game and overlays. Full DeS run: 5.1 byte identity 219 → **266/266**, and 5.5 is unchanged at 11,083/11,086.
>
> **6.2 status (2026-11-06): rendering done** (decision: rendering only). The ER/AC6 bank containers stay on HKLib until 6.3, because `MapHavokPropertyView` edits them and the banks save them.
> - **Views.** `Havok.Shared/Physics.cs` describes data, not algorithms:
>   - physics scene, system, body;
>   - compressed mesh (sections normalised: hk2014/2016 pack their counts into `m_data`);
>   - triangle mesh, convex polygons, compound shape, hkp mesh storage;
>   - navmesh, cost graph.
>
>   They're implemented in each class set's `Manual/Physics.cs`. The sets were regenerated again; each diff is `partial` only, and the old Autogen is in `.archive/Havok Phase 6.2 Autogen`.
> - **One algorithm.** `Havok.Shared/CollisionGeometry.cs` is the union of the four old builders:
>   - every shape kind;
>   - a primitive with an unresolved vertex is dropped;
>   - a zero rotation is identity;
>   - bodies come from the first physics scene, or from loose physics systems (BB);
>   - hkp files get one untransformed submesh per mesh storage;
>   - navmesh fans and graph segments;
>   - wall shape keys.
> - **Smithbox.**
>   - `HavokCollisionResource` and `HavokNavmeshResource` read a `HavokDocument` through `HavokFile`. `HavokCompendiumSource.Unified(profile)` gives them the binder's compendium.
>   - `HavokCollisionMeshBuilder` replaces `HKX2_Helper`, `HKLib_Helper` and `HKLib2019_Helper` (archived). `HKX_Helper` stays for DS2 only (hk_2012, which nothing else reads).
>   - The Sekiro bank cache and `WallBank` use documents.
>   - The navmesh builder's HKX2 output goes through small view adapters until 6.4.
>   - Each game keeps its navmesh colour (configured for DS3/BB, fixed for the rest).
> - **Verified** by `Tools/CollisionReadCompare` (the old builders' geometry code against the new, 20 containers × 12 files per category): [Havok Library Survey/Collision Read Compare.md](Havok%20Library%20Survey/Collision%20Read%20Compare.md).
>   - Equal or within 1e-4 everywhere both paths draw. "Close" comes from a matrix instead of quaternion-then-translate, and the float decode instead of HKLib 2018's `double` one.
>   - Navmeshes are equal bit for bit (DS3, BB, ER, NR, AC6).
>   - Every Sekiro wall shape key matches (`--walls`: 144,353 keys in 73 files).
> - **Differences, all the new path being right:**
>   - DS3 `h37_00_00_00_009900`: HKX2 cast packed vertex indices to `ushort`, and that mesh has 82,799 packed vertices.
>   - BB `o291270`: the old path never drew convex polytopes.
> - **Newly drawn** (the old path failed or drew nothing):
>   - DS1R's tagfile collision. Before, it only worked with the "use PTDE collision" option.
>   - 15 of the 21 sampled BB object collisions, and a DeS object.
>   - Sekiro navmeshes, which the old resource never read.
> - **Guards widened, PTDE fallback removed (2026-11-06):**
>   - DS1R's map collision is located in its own `h`/`l` `.hkxbhd` binders, as DS3's is. `ModelLocator` and `PathBuilder` used to point it at PTDE's loose layout.
>   - Sekiro object collision is shown.
>   - DB_NT and DB navmeshes pass `HavokNavmeshBank.CanUse`.
>   - The "Use PTDE collisions in DS1R projects" option is removed (user's decision), along with the PTDE data path preference, its auto-detection in project setup and `ProjectVFS.PTDE_FS`. DS1R projects only read their own files.
> - **Unchanged failures:**
>   - One ER file, `f60_51_59_00`: neither library's 2018 registry has one array type it uses.
>   - AC6 loose copies in an unpacked `-hkxbdt` folder, away from their compendium.

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
