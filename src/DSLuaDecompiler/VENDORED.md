# DSLuaDecompiler (vendored)

Source: DSLuaDecompiler by katalash, MIT (see `LICENSE`). https://github.com/katalash/DSLuaDecompiler

Copied on 2026-11-19 from the local reference copy `F:\Programming\C#\# Reference\DSLuaDecompiler`. That copy isn't a git
checkout, so there is no commit hash: its newest source files are dated 2023-09-25.

Only `LuaDecompilerCore` is vendored. The upstream CLI (`DSLuaDecompiler`), `DecompilerTester`, the test projects and
the `SoulsFormats` submodule are left out. So is `LuaCompiler`: FromSoftware's games load plain-text Lua from the
binders, so Smithbox writes decompiled source as text and doesn't compile it. (`LuaCompiler` was briefly vendored and is
kept in `.archive/DSLuaDecompiler/LuaCompiler`. `Claude/Tools/LuaAiSurvey` uses the reference copy to check that
decompiled text parses and matches the original bytecode.)

## Local changes

| File | Change | Why |
|---|---|---|
| `LuaDecompilerCore/LuaDecompilerCore.csproj` | net7.0 → net10.0; `AllowUnsafeBlocks` set for every configuration; Smithbox's configuration list added | Upstream only allowed unsafe code in configurations named exactly `Debug`/`Release` |
| `LuaDecompilerCore/Passes/ExpressionPropagationPass.cs`, `LuaDecompilerCore/Utilities/JaggedArray.cs` | Removed upstream's leftover `Debugger.Break()` calls (on block 20, and on arrays over 1,000,000 elements) | They stop Smithbox whenever it runs under a debugger and a script hits them |
| `LuaDecompilerCore/Utilities/BinaryReaderEx.cs` | `ShiftJIS` is now `Encoding.GetEncoding("shift_jis")` (with `CodePagesEncodingProvider` registered); upstream used `Encoding.UTF8` | The Japanese strings in Lua 5.0 files decoded as U+FFFD. Distinct labels became equal strings, which also shifted constant indices on recompile (10 ER aicommon files) |
