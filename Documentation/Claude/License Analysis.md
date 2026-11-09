# Smithbox License Analysis

Date: 2026-11-09. This is an engineering review, not legal advice.

## Summary

| # | Issue | Severity | Fix |
|---|-------|----------|-----|
| 1 | Smithbox is labelled MIT, but it compiles in GPL-3.0 code (SoulsFormats, SplineCompressedAnimation.cs). The program you ship falls under GPL-3.0. | High (mislabelled license) | Relicense the distributed program as GPL-3.0-or-later, or say clearly that it is GPL-3.0 |
| 2 | Proprietary Oodle DLLs (`oo2core_6/8/9_win64.dll`) are committed and shipped in releases | High (redistributing proprietary code) | Stop shipping them; copy or load them from the user's game install |
| 3 | Most third-party notices are missing from `Licenses/` | Medium (MIT/BSD/Apache/zlib attribution requirement) | Add a THIRD-PARTY-NOTICES file |
| 4 | `sixlabors.lic` (ImageSharp license key) is committed to a public repo and shipped | Low–Medium | Check the Six Labors key terms |
| 5 | Havok class data, game-derived assets | Low | No change; note only |

## 1. MIT label vs. GPL-3.0 components

`LICENSE` is MIT (Vawser 2025; Katalash, Meowmaritus 2018, inherited from DSMapStudio).

GPL-3.0 code linked into the program:

- **SoulsFormats** (`src/Andre/SoulsFormats`, built into `Andre.SoulsFormats.dll`). Upstream SoulsFormats (JKAnderson) is GPL-3.0, and `Licenses/SoulsFormats/LICENSE.txt` is GPL-3.0. Every editor references it.
- **`src/Smithbox.Program/Utilities/Havok/SplineCompressedAnimation.cs`**: adapted from PredatorCZ's HavokLib, with a GPL-3.0-or-later header. This file sits directly in Smithbox.Program, the MIT-labelled code.
- (Reference only, not compiled: DSAnimStudio's SoulsAssetPipeline is also GPL-3.0. Any further port from it brings the same obligation.)

GPL-3.0 is copyleft. Distributing a work that combines GPL code with your own code, whether through static linking or a ProjectReference into one .NET application, means distributing the whole under GPL-3.0. MIT code may be included in a GPL work (it is GPL-compatible), but the reverse doesn't hold. So the release binaries are GPL-3.0 whatever `LICENSE` says. DSMapStudio has the same mismatch, and Smithbox inherited it.

**Status (2026-11-09): done, option A.** `LICENSE` is now the verbatim GPL-3.0 text. `Licenses/Smithbox/LICENSE.txt` holds the GPL-3.0-or-later notice and the licensing history. README has a License section, and CONTRIBUTING says contributions are GPL-3.0-or-later.

**Do you need to update the license? Yes.** Options:

- **A (recommended): relicense Smithbox as GPL-3.0-or-later.** MIT lets you sublicense, so the Katalash/Meowmaritus and contributor MIT code can be distributed under GPL. Keep their MIT copyright notices (e.g. keep `Licenses/DSMS/LICENSE.txt`). Contributors don't need to sign off, because MIT already grants this. It matches what you actually ship.
- **B: dual statement.** Keep MIT for Smithbox's own source files and add to `LICENSE`/README: "The Smithbox application as distributed includes GPL-3.0 components (SoulsFormats, HavokLib-derived code); the combined work is licensed under GPL-3.0." This is accurate, but people will get confused about which files are which.
- **C: remove the GPL code.** Not realistic: SoulsFormats is the format layer for the whole program.

GPL-3.0 obligations you already mostly meet: the source is public on GitHub, and the GPL text ships in `Licenses/`. Also: each release should match a tagged commit, so the "corresponding source" is clear.

## 2. Oodle DLLs (proprietary, Epic Games / RAD)

**Status (2026-11-09): done.** The four DLLs are untracked, moved to `.archive/Oodle`, and gitignored (`oo2core_*.dll`, `liboo2core*`). `Utilities/OodleSetup.cs` copies any oo2core found in the game folder on project load, for every project type (replacing the per-game `SetupDLLs` blocks). `Oodle.cs` now looks in `AppContext.BaseDirectory` instead of the working directory, and its error message explains where to get the DLL.

Committed and shipped:
- `src/Smithbox.Data/oo2core_9_win64.dll` (since 2025-08)
- `Smithbox.Release/Output/oo2core_6_win64.dll`, `oo2core_8_win64.dll`, `oo2core_9_win64.dll`

Oodle is commercial software. Only Oodle licensees (the game developers) may redistribute it, and Smithbox has no such grant. This is a real licensing problem whatever license Smithbox uses. It also conflicts with GPL-3.0: a proprietary non-system library shipped together with GPL code.

`SoulsFormats/Util/Oodle.cs` already looks for the DLL in the working directory at runtime. The usual practice in modding tools is:
- Don't commit or ship `oo2core_*.dll`. Move them to `.archive` and add them to `.gitignore`.
- On project load, copy `oo2core_X_win64.dll` from the game's install folder (Sekiro/ER/AC6/NR all ship it next to the exe), or load it directly from that path with `NativeLibrary.Load`.
- Note: the DLLs remain in git history. Removing them from the current tree stops further distribution, which is what matters most.

## 3. Missing third-party notices

**Status (2026-11-09): done.** Added `src/Smithbox.Data/Licenses/THIRD-PARTY-NOTICES.txt`, built from the nuspec and LICENSE files in the NuGet cache and upstream repos. Native library copyright lines come from upstream and could be checked against the exact versions. Also added `src/Veldrid/LICENSE` and `src/native/NavGen/bvh/LICENSE.txt`. **New finding:** DrSwizzler (Shadowth117) has no published license (the repo license is null and the nuspec has none), so its redistribution rights are unclear. Ask the author to add one (e.g. MIT).

MIT, BSD, Apache-2.0 and zlib-with-binary terms all require keeping the copyright/license notice in copies you distribute. `Licenses/` has only: Smithbox, DSMS, SoulsFormats, HKLib, ESDLang, coremats-templates, Tracy.

Shipped without a notice (license from upstream; check the exact versions):

| Component | License | Source |
|-----------|---------|--------|
| Veldrid (vendored in `src/Veldrid`, no LICENSE file in the folder) | MIT | vendored source |
| Dear ImGui / cimgui, ImGuizmo, imnodes, ImPlot (+ cimgui wrappers) | MIT | native DLLs |
| Hexa.NET.* , HexaGen.Runtime | MIT | NuGet |
| SDL2 | zlib | `SDL2.dll` |
| Silk.NET | MIT | NuGet |
| Vortice.Vulkan / VulkanMemoryAllocator (`vma.dll`) | MIT | NuGet |
| libveldrid-spirv (SPIRV-Cross, shaderc, glslang) | Apache-2.0 / BSD-3 | native DLL |
| DirectXTex / DirectXTexNet / Hexa.NET.DirectXTex | MIT | NuGet + DLL |
| SixLabors.ImageSharp | Six Labors Split License (Apache-2.0 for OSS) | NuGet |
| Pfim, DrSwizzler, ProcessMemoryUtilities.Net, NativeFileDialogSharp (+ `nfd.dll`, zlib) | MIT / zlib | NuGet |
| ZstdNet + libzstd | BSD-3 | NuGet + DLL |
| BouncyCastle.Cryptography | MIT | NuGet |
| CommunityToolkit.HighPerformance, DotNext | MIT | NuGet |
| CsvHelper | MS-PL / Apache-2.0 | NuGet |
| Octokit | MIT | NuGet |
| Grpc.* | Apache-2.0 | NuGet |
| Google.Protobuf | BSD-3 | NuGet |
| McMaster.Extensions.CommandLineUtils | Apache-2.0 | NuGet |
| DotRecast | zlib | NuGet |
| Recast (in `src/native/NavGen`, built into `NavGen.dll`) | zlib | vendored |
| bvh (`src/native/NavGen/bvh`, madmann91/bvh assumed; no header) | MIT | vendored |
| SoapstoneLib (thefifthmatt) | MIT, has a LICENSE in `src/Andre/SoapstoneLib` but not in `Licenses/` | vendored |
| bhd5-decrypt-rust crates (rsa deps, rayon, num-bigint…) | MIT / Apache-2.0 | `bhd5_decrypt_rust.dll` |
| .NET runtime (self-contained: coreclr, ASP.NET Core, msquic) | MIT | runtime |

Test-only packages (xunit, Moq, coverlet) are not distributed and need nothing.

Fix: add `Licenses/THIRD-PARTY-NOTICES.txt` (one section per component: name, URL, copyright line, license text or SPDX id + full text once per license). For the NuGet list, a tool such as `dotnet-project-licenses` or `nuget-license` can generate the list from `obj/project.assets.json`. Also add Veldrid's MIT LICENSE to `src/Veldrid`.

Apache-2.0 items (ESDLang, coremats-templates, Grpc, SPIRV tooling) also require passing on any upstream NOTICE file, and modified files must be marked as changed.

## 4. sixlabors.lic

**Status (2026-11-09): decided to leave it committed.**

ImageSharp 4.x checks for a `sixlabors.lic` key. The file is committed in three places (`src/Smithbox.Program`, `src/Smithbox.Data/Licenses`, `Smithbox.Release/Output/Licenses`) and so is public. Six Labors gives open-source projects keys for free under the Split License. Check the terms of your key grant on whether it may sit in a public repo. If not, move it to a CI secret or an ignored local file. If you aren't sure, ask Six Labors. This doesn't affect what license Smithbox can use: the Split License applies Apache-2.0 terms to OSI-licensed projects, and Apache-2.0 is GPL-3.0-compatible.

## 5. Lower-risk items (no action)

- **Havok class definitions** (`src/Havok/Classes/*/Autogen`, `ClassDumps`): generated from reflection data embedded in game files, not from the Havok SDK source. Same practice as HKLib/HKX2. Low risk; don't import SDK headers.
- **FXR code** (new, untracked): based on fxr-main, which is public domain (Unlicense). No obligation.
- **fromsoftware-rs, DSAnimStudio, MonoGame, fsbankcl** in `Reference/`: gitignored, not distributed. Only matters if code is ported, and SoulsAssetPipeline is GPL-3.0.
- **bhd5_decrypt_rust**: holds RSA public keys from the games. Common in modding tools; low risk.
- **Game data assets** (param defs, names, enums): community-produced metadata, standard for FromSoft tooling.

## Suggested order

1. Remove the Oodle DLLs from the tree and add a runtime copy/load from the game directory.
2. Change `LICENSE` to GPL-3.0-or-later (option A), keep the old MIT notices under `Licenses/`, and update README.
3. Add THIRD-PARTY-NOTICES.
4. Resolve the sixlabors.lic question.
