# Startup Flow Analysis

Date: 2026-11-05. Scope: `Smithbox.Vulkan` / `Smithbox.OpenGL` `Program.Main` through the first frames of `Smithbox.Run()`. Static review only; nothing was launched.

## Fix status (2026-11-05)

| Finding | Status |
|---|---|
| 1 Vulkan fallback | Fixed – `Smithbox.CreateGraphicsContext` wraps `Initialize()`, disposes and closes the Vulkan window on failure |
| 2 Logging before logger exists | Fixed – `Smithbox.SetupLogging` runs first in the constructor (log windows and popup subscription included) |
| 3 Languages.json | Fixed – built-in English `FallbackLanguage`; missing language folders are skipped, not created |
| 4 Null Startup.json | Fixed – null treated as a read failure; saves during `Startup.Load` can't throw |
| 5 Crash handler order | Fixed – registered first in all four `Program.cs`; dialog failures no longer escape the handler |
| 6 Crash log folder | Fixed – uses `AppContext.BaseDirectory` |
| 7 Release crash recovery | Fixed – Release also runs `AttemptSaveOnCrash`/`CrashShutdown`; a project loading at crash time loses `AutoSelect` (`ProjectOrchestrator.LoadingProject`) |
| 8 Auto-load task | Fixed – `StartupProject`/`ReloadProject` share `LoadProject`, which catches and logs failures (high priority). `Init` still runs in the background, but selecting the project (and the auto-load's project-screen update) is queued with `RunOnMainThread` and applied at the start of the next `Orchestrator.Update`. Auto-load uses a snapshot and loads one project |
| 9 Double shutdown | Fixed |
| 10 Theme overwrites Interface.json | Fixed – missing or corrupt theme falls back to `LoadDefault()` |
| 11 Soapstone flag / Startup save / frame rate / LOC folders / passive-task wording | Fixed |

Not covered: `Smithbox.Tests` still calls the removed `ProjectManager` API and doesn't build; it predates this work.

## Startup sequence

| # | Step | Where |
|---|------|-------|
| 1 | `Startup.Setup/Load` – reads `%LOCALAPPDATA%\Smithbox\Configuration\Startup.json` | `Application/Startup.cs` |
| 2 | `LOC.Setup/Load` – reads `Assets/Localization/Languages.json` and language folders | `Interface/Localization.cs` |
| 3 | `Profiler.Startup` – Tracy (always on in DEBUG) | `Tracy/Tracy.cs` |
| 4 | Register `AppDomain.UnhandledException` crash handler | `Program.cs:41` |
| 5 | `SetCurrentDirectory(exe dir)`, read version | `Program.cs:44` |
| 6 | `new Smithbox(...)`: title via `LOC.Get`, `RestoreImguiIfMissing`, `PlatformUtils.InitializeWindows(null)` | `Smithbox.cs:67` |
| 7 | `Setup()`: `CFG`, `UI`, `InputManager.Init`, `CFG.Load`, `UI.Load`, `PreferencesUtil.Setup` | `Smithbox.cs:207` |
| 8 | Create graphics context (Vulkan, "fallback" to OpenGL) | `Smithbox.cs:104` |
| 9 | Logger factory created; `InputManager.FlushPendingLogs` | `Smithbox.cs:126` |
| 10 | `_context.Initialize()` – window + device creation | `Smithbox.cs:159` |
| 11 | `ProjectOrchestrator`, DPI, ImGui, fonts, menus, Soapstone, TextureManager, OpenGL warning | `Smithbox.cs:164-204` |
| 12 | `Run()`: Soapstone passive task, frame loop | `Smithbox.cs:383` |
| 13 | Frame 2+: `Orchestrator.LoadExistingProjects()` (fire-and-forget) + auto-load | `ProjectOrchestrator.cs:67` |

## Findings (most severe first)

### 1. Vulkan → OpenGL fallback can never happen, and would crash if it did — High
`Smithbox.cs:111-122`. The `try` only wraps `new VulkanGraphicsContext()`, which has no constructor and cannot throw. All Vulkan failure happens in `Initialize()` (`VeldridStartup.CreateWindowAndGraphicsDevice`, `Smithbox.cs:159`), outside the try. A machine without a working Vulkan driver crashes instead of falling back.

Second bug in the same block: the catch calls `Smithbox.LogWarning`, but `SbLoggerFactory` is only created at line 126, so `Logger<T>()` throws `NullReferenceException` inside the catch.

Fix: move logger setup above context creation, and wrap `_context.Initialize()` (disposing the partially-created Vulkan context/window) in the fallback.

### 2. Corrupt Configuration.json / Interface.json / theme crashes instead of recovering — High
`CFG.cs:1676`, `UI.cs:211`, `UI.cs:244`. The recovery `catch` blocks call `Smithbox.LogError<...>` during `Setup()` (step 7), before the logger exists (step 9). Result: `NullReferenceException`, defaults are never restored, and every subsequent launch crashes the same way until the user deletes the file. `InputManager` already solved this with `LogLater`/`FlushPendingLogs` — CFG and UI need the same treatment (or create the logger first).

The same applies to `LOC.Get` in DEBUG builds: a missing key logs via `Smithbox.LogError`, so any missing key used before step 9 (e.g. `PROGRAM_TITLE`, `SYS_CFG_Failed_*`) crashes debug builds.

### 3. Missing/invalid Languages.json crashes before the crash handler exists — High
`Localization.cs:28-57, 67-73`.
- File missing → `Languages` is empty → `LOC.Load` finds no language, falls back to `LanguageList.FirstOrDefault("English")` which is also null → `curLang.Name` NRE.
- File unparsable → the catch leaves `Languages == null` → NRE at line 50.
- JSON literal `null` → same.

Because this runs before step 4, no crash log and no message box: the app just exits. Fallback should be a hard-coded `{ Name = "English", Folder = "English" }` entry.

### 4. Startup.json with `null` content → NRE — Medium
`Startup.cs:49`. `Deserialize` can return null without throwing; `Startup.Current` becomes null and `LOC.Setup` crashes on `Startup.Current.SetProgramLanguage`. CFG/UI already guard with a null check; Startup does not. Also `Save()` in the catch is unguarded (read-only/locked LocalAppData → crash before handler).

### 5. Everything before the crash handler is unlogged — Medium
`Program.cs:30-42` (both Vulkan and OpenGL). Startup, LOC and Tracy (`TracyClient.dll` missing in a DEBUG build → `DllNotFoundException`) run before `UnhandledException` is registered. Move the handler registration to the first line of `Main`.

Related: `CrashHandler` → `ExportCrashLog` → `PlatformUtils.Instance.MessageBox`. `PlatformUtils.Instance` is null until `Smithbox.cs:100`, and `MessageBox` calls `LOC.Get`, so a crash in steps 1–6 throws a second exception inside the handler.

### 6. Crash log folder probably resolves against the launch directory — Low/Plausible
`Program.cs:107`. `CrashLogPath` is a `static readonly` initializer built from `Directory.GetCurrentDirectory()`. `Program` has no static constructor (beforefieldinit), so the initializer is likely run when `Main` first touches a static field (`IsDebug` in DEBUG) — before `SetCurrentDirectory` at line 44. Launching from a shortcut/other CWD can write `Crash Logs` somewhere unexpected (or fail with access denied). Use `AppContext.BaseDirectory` instead.

### 7. Crash recovery never runs in Release builds — Medium
`Program.cs:50-77`. `AttemptSaveOnCrash` / `CrashShutdown` are only called on the `IsDebug` path; Release just calls `Run()` and relies on the AppDomain handler, which never calls them. In addition `AttemptSaveOnCrash` only re-saves config — its comment says it clears the recent/auto-load project but nothing does that. So a project that crashes on auto-load (step 13) crashes on every launch (see #8).

`Smithbox.OpenGL/Program.cs:60-61` also lacks the `try` around `CrashShutdown` that the Vulkan entry point has, so a shutdown failure there hides the original exception.

### 8. Auto-load is fire-and-forget; failures are swallowed and leave state stuck — Medium
`ProjectOrchestrator.cs:71, 161-224`. `_ = LoadExistingProjects()` discards the task:
- Any exception from `StartupProject` / `ProjectEntry.Init` is never observed (no crash log, no message). `IsProjectLoading` remains `true` forever, so `CreateProject` and auto-load skip loading silently and the loading UI may stay up.
- After the first `await`, the continuation runs on a thread-pool thread (no sync context) and mutates `SelectedProject`, `ProjectEditor.*` while the UI thread reads them each frame — a data race.
- It iterates `Projects` with `foreach` across an `await`; if the user creates a project meanwhile (`CreateProject` → `Projects.Add`), the enumeration throws `InvalidOperationException` (also swallowed).
- No `break` after the first auto-select project: with `Project_Enable_Automatic_Auto_Load_Assignment` off and several descriptors marked `AutoSelect`, each is loaded in turn, disposing the previous one.

### 9. Shutdown runs twice — Low/Medium
`Smithbox.cs:447-462`. When the window closes during `Update`, the loop calls `Orchestrator.Exit(); Exit(); break;` and then the code after the loop calls `Orchestrator.Exit(); Exit();` again — every project and config is serialized twice, and `Profiler.Shutdown` is called twice (guarded by `EnableTracy`, so harmless there). If a save fails midway the second pass repeats the failure. Remove the in-loop calls.

### 10. Missing theme file overwrites Interface.json — Low/Medium (data loss)
`UI.cs:224-228`. If `Interface_Selected_Theme` names a theme that no longer exists in `Assets/Themes` (e.g. after updating Smithbox), `LoadTheme` sets defaults and `Save()` writes them to **Interface.json**, discarding the user's customised default UI. Should fall back to `LoadDefault()` instead.

### 11. Smaller issues
- `Startup.System_Enable_Soapstone_Server` is never read; the server always starts (`Smithbox.cs:183, 390`).
- `Startup.json` is not saved in `Exit()` (only in `SaveConfiguration`), so the language auto-detection result in `LOC.Setup` isn't persisted on a normal exit.
- `Viewport_Frame_Rate` is only clamped by the slider; a hand-edited `0` makes `_desiredFrameLengthSeconds` infinite and the frame limiter spins forever (`Smithbox.cs:422-433`).
- `LOC.Load` calls `Directory.CreateDirectory` under `AppContext.BaseDirectory`; installed in a protected folder (Program Files) this throws `UnauthorizedAccessException` before the crash handler.
- The Soapstone passive task logs "running" both before and after its action (`TaskManager.cs:194-210`); after the server stops it reports the *completed* (`RUNNING`) message rather than a stopped one.
- `_initialLoadComplete` becomes true on the first frame with no active tasks, i.e. before `LoadExistingProjects` even starts, so the "crashed on initial load" branch in `AttemptSaveOnCrash` effectively never covers project auto-load.

## Suggested fix order
1. Create the logger factory first thing in the `Smithbox` constructor (fixes #1b, #2, DEBUG `LOC.Get`).
2. Register the crash handler first in `Main`; make `ExportCrashLog` tolerate a null `PlatformUtils.Instance` (#5).
3. Harden `LOC.Setup/Load` and `Startup.Load` against missing/null data (#3, #4).
4. Move the Vulkan fallback around `Initialize()` (#1a).
5. Observe the auto-load task, marshal results to the UI thread, and clear `AutoSelect` on crash (#7, #8).
6. Remove double shutdown, fix theme fallback (#9, #10).
