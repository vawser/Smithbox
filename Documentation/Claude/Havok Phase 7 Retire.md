# Phase 7: retiring the old Havok libraries

Part of the [Unified Havok Library Plan](Unified%20Havok%20Library%20Plan.md). Follows Phase 6 (every Smithbox path is on the unified library, checked in the app).

Status: **done 2026-11-06.** Smithbox, the unified library and the kept tools build without HKLib or HKX2. Not yet checked in the app.

## Decisions

- **P1. The old libraries and the tools that compare against them.** **Decided (2026-11-06): archive them all and port the generator.** Tools keep running on the unified library alone, and their old-vs-new columns go.
- **P2. The comparison tools.** **Decided (2026-11-06, the user): remove them.** HavokLibSurvey is kept (the user's choice), as it's the only home of the registry pipeline: `--buildregistry`, `--buildoverlay`, `--dumpclasses`, `--filldumps`, `--layoutcheck`.

## What was done

**Archived to `.archive/Havok Phase 7`:**
- **Libraries** (`Libraries/`): `HKLib`, `HKLib.ClassGeneration`, `HKLib.Reflection`, `HKLib.Serialization`, `HKX.ClassGeneration` (holds HKX2's `classxmlds3` class dumps), `HKX2`, `HKX2.Serialization`, `Havok.Formats.HKLib` (the Phase 2 bridge), `ColBakeTest`. They were also removed from `Smithbox.sln`; the pre-change solution is `Smithbox.sln.before`.
- **Comparison tools** (`Tools/`): `AnimationReadCompare`, `CollisionReadCompare`, `HavokEditorRoundTrip`, `CollisionBuilderEquivalence`, `HkTypeDiff`. Their `bin`/`obj` build output was removed rather than moved.
- **Replaced code:** HavokLibSurvey's previous `CoreCheck`, `Libraries`, `Program` and `Report` (`HavokLibSurvey/`), and AnimationAliasGenerator's `PackfileReader` (`AnimationAliasGenerator/`).

**Smithbox.Program** drops its HKLib, HKLib.Reflection, HKLib.Serialization and HKX2 references. No code used them after 6.5.

**Havok.Core** now holds every registry. `HavokTypeRegistry20180100.xml`, `_NR.xml` and `20190100.xml` moved from `HKLib.Reflection/Res` into `Havok.Core/Res`, and the link to HKLib's folder is gone. Smithbox's output still gets all of them.

**Havok.ClassGeneration no longer uses HKLib:**
- It loads registries through Havok.Core's `HavokTypeRegistry`/`HavokTypeBuilder`, which are HKLib's merged into one class.
- It takes the naming and placement of shared types from `Havok.hk2018`, its own earlier output, which has HKLib's names.
- It keeps only the four marker interfaces (`IRootContainer`, `IClipGenerator`, `IStateMachine`, `ICustomManualSelectorGenerator`). The hk2018 classes also implement the Havok.Shared views in their Manual partials, and those must not spread to other SDKs.
- `GlobalUsing.cs` supplies the namespaces HKLib's used to.
- **Regenerating:** the generator references the current hk2018 build, so write into a scratch folder, diff, then swap Autogen (the existing workflow). Havok.hk2018 keeps building while it runs.
- **Verified:** all 7 class sets (hk55, hk2010, hk2014, hk2015, hk2016, hk2018, hk2019) were regenerated with the HKLib-based generator and with the ported one, using the same arguments. **Both outputs are identical, and identical to the committed Autogen.**

**Tools, kept and moved onto the unified library:**
- **AnimationAliasGenerator:** behaviours read through `HavokFile`. Bloodborne goes through the same object walk as the other games (the unified library reads BB's real layout), replacing the hard-coded byte offsets over HKX2's fixups, so the `packprobe` mode went too. The schema-less tagfile reader links Havok.Formats' `HavokVarUInt`. **Verified:** `generate` for DS3, BB, SDT, ER, NR and AC6 writes byte-identical alias files before and after.
- **BehaviorSurvey:** behaviours read through `HavokFile`. **Verified:** ER and AC6 output identical. DS3 differs only in labels: hk2014 types flags as enums, so names print instead of numbers, and nested classes print by short name. All 225 distinct counts are identical.
- **HavokLibSurvey:**
  - The HKX2 (packfile, tagfile) and HKLib (2018, 2019) adapters are gone, along with the three HKLib-backed "Unified 2018/2019" entries. The generated class sets now read with no parity reference.
  - `--core` keeps Core's header-sniff check; the registry comparison with HKLib is gone.
  - The report's "Unified tagfile engine vs HKLib" section is now "Unified engine" (read, byte identity).
  - SoulsFormats `HKX` stays as a column (D3).
- **HavokPackfileProbe:** its unused HKX2, HKLib and bridge references were removed.
- **CollisionSurvey:** already on `HavokFile` since 6.4.
- **HavokClassDumpConvert:** unchanged. It reads class dumps from a folder given on the command line, so it can still convert HKX2's archived `classxmlds3` dumps. Its default note names the old path.

**Built:** Smithbox.Program, Havok.ClassGeneration (and with it Core, Formats, Shared and every class set), AnimationAliasGenerator, BehaviorSurvey, CollisionSurvey, HavokLibSurvey, HavokPackfileProbe, HavokClassDumpConvert, GridSettingsMigrationCheck. All have 0 errors.

**Left as they are:**
- SoulsFormats' `HKX` code stays in Andre (D3); `HKX_Helper` still reads original DS2 collision through it.
- `NavGen.dll` stays in Smithbox.Data's content: `Havok.hk2014/Builders/hkaiNavMeshBuilder` still declares it, though nothing calls it since 6.4.
- `Documentation/Backup/Smithbox_LowRequirements` and `Reference/DSAnimStudio` mention HKX2/HKLib. They're separate, unbuilt projects.

## To check in the app

1. Smithbox starts, and a project of each kind still opens Havok files: an ER behaviour in the Havok Editor, an AC6 map collision in the Map Editor, a DS3 animation playing in the Model Editor.
2. NR still reads with its overlay (the registries moved): an NR behaviour shows `m_isBasePoseAnim` on its clips.
