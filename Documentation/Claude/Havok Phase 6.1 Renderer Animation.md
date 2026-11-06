# Phase 6.1: renderer animation on the unified Havok library

Part of the [Unified Havok Library Plan](Unified%20Havok%20Library%20Plan.md), Phase 6 step 1.

## Today

`Smithbox.Program/Renderer/Animation/HavokAnimationData.cs` (`HavokAnimationReader`) reads skeletons and clips:
- **Packfiles** (DS1, BB, DS3) through SoulsFormats `HKX` (`HKASkeleton`, `HKASplineCompressedAnimation`, `HKAInterleavedUncompressedAnimation`, `HKAAnimationBinding`), with a `HKXVariation` per game.
- **Tagfiles** (DS1R, SDT, ER, NR, AC6) through `Renderer/Animation/HavokTagfile.cs`, a schema-less TAG0/TCM0 reader (604 lines) that looks fields up by name.

`AnimationPackage` hands it raw bytes, plus the anibnd's compendium (id 7000000) where there is one. The spline decode (`SoulsFormats.SplineCompressedAnimation.ReadSplineCompressedAnimByteBlock`) works on the clip's `data` bytes and is container-independent.

## Design (decided 2026-11-06: shared interfaces, plan §2.6)

1. **`Havok.Shared` interfaces**, with no dependencies:
   - `HavokQsTransform`: translation, rotation, scale.
   - `IHavokSkeleton`: bone count, names, parents, reference pose.
   - `IHavokAnimationBinding`: the animation, track → bone map, blend hint.
   - `IHavokAnimation`: duration, transform track count.
   - `IHavokSplineAnimation : IHavokAnimation`: frames, blocks, frames per block, frame duration, data.
   - `IHavokInterleavedAnimation : IHavokAnimation`: transforms.
2. **Manual partials** implement them in every class set that has the class:

   | | skeleton | binding | spline | interleaved |
   |---|---|---|---|---|
   | hk55 (DeS) | ✓ | ✓ | — | ✓ (`hkaInterleavedSkeletalAnimation`) |
   | hk2010 (DS1) | ✓ | ✓ | ✓ | — |
   | hk2014 (DS3, BB, DS2S) | ✓ | ✓ | ✓ | ✓ |
   | hk2015 (DS1R) | ✓ | ✓ | ✓ | — |
   | hk2016 (SDT) | ✓ | ✓ | ✓ | ✓ |
   | hk2018 (ER, NR, DB) | ✓ | ✓ | ✓ | ✓ |
   | hk2019 (AC6) | ✓ | ✓ | ✓ | ✓ |

   The classes are generated as `partial` by regenerating each set with them on the generator's partial list. Each regeneration is diffed against the archived Autogen: only the `partial` keyword may change.
3. **`src/Havok/Havok.Files`**: the `HavokFile` facade from plan §2.3. It's a new project because the facade has to reference every class set, and they reference Core.
   - `Sniff`.
   - `ReadCompendium(data, profile)`.
   - `Read(data, profile, compendium)` → `HavokDocument`, with `Objects` (every object read).

   The file's SDK picks the engine, dialect and class set. A file whose SDK isn't its game's (ER's stray 2016 files) is read with that SDK's own profile.
4. **Renderer.** `HavokAnimationReader` reads through `HavokFile` and the interfaces: one path for every game and container. `AnimationPackage` parses each binder's compendium once, not per clip. `HavokTagfile.cs` moves to `.archive`.

Unchanged: which games animate (`IsSupportedGame`). DeS stays off: its skeletons now read, but its clips are wavelet or delta compressed, and the renderer has no decoder for those.

## Verification

- `Tools/AnimationReadCompare` (C#): copies of the old reader (SoulsFormats `HKX` and `HavokTagfile`) against the new one, over the anibnds of each animated game (ER via VFS). For every skeleton it compares names, parents and reference pose. For every clip it compares the type, counts, durations, track → bone map, blend mode, and the spline data bytes or interleaved transforms. Floats must match bit for bit.
- Smithbox builds (`Smithbox.Program` project only).
- In the app: play character and asset animations in the Model Editor or Map Editor for DS1, DS1R, DS3, BB, SDT, ER, NR and AC6.
