using StudioCore.Application;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Captures render snapshots with nobody at the keyboard:
/// <c>Smithbox.exe --render-snapshots DES,DS1R|all --snapshot-root &lt;dir&gt; [--game-data &lt;game_data.txt&gt;]</c>.
/// For each game it opens a temporary project on the vanilla data named by game_data.txt (in
/// &lt;root&gt;/.projects/&lt;GAME&gt;, never added to the project list), runs the Model Editor's Render Snapshots
/// tool, unloads it, then closes Smithbox. The user's projects and settings are left as they were. Progress goes to
/// &lt;root&gt;/automation.log. Plan: Documentation/Shader Family Split Plan.md, S1.
/// </summary>
public static class RenderSnapshotAutomation
{
    private enum Stage { Starting, Loading, Opening, Capturing, Unloading, Done }

    private static readonly string[] AllGames =
        ["DES", "DS1", "DS1R", "DS2", "DS2S", "DS3", "BB", "SDT", "ER", "AC6", "NR", "DB_NT", "DB"];

    // Frames after startup before the first project, so the main window and the project list are up
    private const int StartupFrames = 60;

    private static readonly TimeSpan LoadTimeout = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan OpenTimeout = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan CaptureTimeout = TimeSpan.FromMinutes(15);

    private static List<string> Games = new();
    private static string Root;
    private static string GameDataPath;
    private static StreamWriter Log;

    private static Stage CurrentStage = Stage.Starting;
    private static int GameIndex;
    private static int Frames;
    private static readonly Stopwatch StageTime = new();
    private static ProjectEntry Project;
    private static Task<bool> LoadTask;
    private static bool RunStarted;
    private static int Failures;

    // Settings changed for the run, put back before Smithbox closes and saves them
    private static int SavedRenderMode;
    private static string SavedFolder;

    /// <summary>Whether Smithbox was started to capture render snapshots</summary>
    public static bool Active { get; private set; }

    /// <summary>Reads the command line. Called before Smithbox starts.</summary>
    public static void Configure(string[] args)
    {
        var games = Value(args, "--render-snapshots");
        if (games == null)
            return;

        Root = Value(args, "--snapshot-root");
        if (string.IsNullOrWhiteSpace(Root))
            return;

        Root = Path.GetFullPath(Root);
        GameDataPath = Value(args, "--game-data") ?? Path.Join(Path.GetDirectoryName(Root), "game_data.txt");

        Games = games.Equals("all", StringComparison.OrdinalIgnoreCase)
            ? AllGames.ToList()
            : games.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(g => g.Replace('-', '_').ToUpperInvariant()).ToList();

        Directory.CreateDirectory(Root);
        Log = new StreamWriter(Path.Join(Root, "automation.log"), append: false) { AutoFlush = true };
        Active = true;

        Write($"Render snapshot automation: {string.Join(", ", Games)} into {Root}, game data from {GameDataPath}");
    }

    private static string Value(string[] args, string name)
    {
        var index = Array.FindIndex(args, a => a.Equals(name, StringComparison.OrdinalIgnoreCase));
        return index >= 0 && index + 1 < args.Length ? args[index + 1] : null;
    }

    private static void Write(string line)
    {
        Log?.WriteLine($"{DateTime.Now:HH:mm:ss} {line}");
    }

    /// <summary>Advances the run. Called every frame by the project orchestrator.</summary>
    public static void Update()
    {
        if (!Active)
            return;

        try
        {
            switch (CurrentStage)
            {
                case Stage.Starting: UpdateStarting(); break;
                case Stage.Loading: UpdateLoading(); break;
                case Stage.Opening: UpdateOpening(); break;
                case Stage.Capturing: UpdateCapturing(); break;
                case Stage.Unloading: UpdateUnloading(); break;
            }
        }
        catch (Exception e)
        {
            Write($"Failed: {e}");
            Failures++;
            CurrentStage = Stage.Unloading;
        }
    }

    private static void UpdateStarting()
    {
        if (++Frames < StartupFrames)
            return;

        SavedRenderMode = CFG.Current.Viewport_Render_Mode;
        SavedFolder = CFG.Current.ModelEditor_RenderSnapshot_Folder;

        CFG.Current.Viewport_Render_Mode = (int)ViewportRenderMode.Game;
        CFG.Current.ModelEditor_RenderSnapshot_Folder = Root;

        GameIndex = 0;
        StartGame();
    }

    private static void StartGame()
    {
        while (GameIndex < Games.Count)
        {
            var game = Games[GameIndex];

            if (TryCreateProject(game, out var reason))
            {
                Write($"{game}: loading {Project.Descriptor.DataPath}");
                LoadTask = Smithbox.Orchestrator.StartupTemporaryProject(Project);
                CurrentStage = Stage.Loading;
                StageTime.Restart();
                return;
            }

            Write($"{game}: skipped, {reason}");
            Failures++;
            GameIndex++;
        }

        Finish();
    }

    private static bool TryCreateProject(string game, out string reason)
    {
        reason = null;
        Project = null;

        if (!Enum.TryParse<ProjectType>(game, out var type))
        {
            reason = "unknown game";
            return false;
        }

        var dataPath = ReadGameDataPath(game);
        if (dataPath == null)
        {
            reason = $"no path for it in {GameDataPath}";
            return false;
        }

        dataPath = ResolveDataRoot(dataPath);

        if (!Directory.Exists(dataPath))
        {
            reason = $"the data folder {dataPath} doesn't exist";
            return false;
        }

        // An empty project folder of its own, so the snapshots show the game's data and nothing a mod changed
        var projectPath = Path.Join(Root, ".projects", game);
        Directory.CreateDirectory(projectPath);

        var descriptor = new ProjectDescriptor
        {
            ProjectGUID = Guid.NewGuid(),
            ProjectName = $"Render Snapshots {game}",
            ProjectPath = projectPath,
            DataPath = dataPath,
            ProjectType = type
        };
        descriptor.EnableAllEditors();

        Project = new ProjectEntry { Descriptor = descriptor };
        return true;
    }

    private static string ReadGameDataPath(string game)
    {
        if (!File.Exists(GameDataPath))
            return null;

        foreach (var line in File.ReadAllLines(GameDataPath))
        {
            var parts = line.Split(':', 2);

            if (parts.Length == 2 && parts[0].Trim().Equals(game, StringComparison.OrdinalIgnoreCase))
                return parts[1].Trim().Trim('"').Trim();
        }

        return null;
    }

    // A game folder can hold the data one level down (Bloodborne's Loose/dvdroot_ps4)
    private static string ResolveDataRoot(string root)
    {
        foreach (var candidate in new[] { root, Path.Join(root, "dvdroot_ps4"), Path.Join(root, "Loose", "dvdroot_ps4"), Path.Join(root, "Loose") })
        {
            if (Directory.Exists(Path.Join(candidate, "map")) || Directory.Exists(Path.Join(candidate, "model")) ||
                Directory.Exists(Path.Join(candidate, "chr")))
                return candidate;
        }

        return root;
    }

    private static void UpdateLoading()
    {
        var game = Games[GameIndex];

        if (StageTime.Elapsed > LoadTimeout)
        {
            Fail(game, "project load timed out");
            return;
        }

        if (!LoadTask.IsCompleted)
            return;

        if (LoadTask.IsFaulted || !LoadTask.Result)
        {
            Fail(game, "the project failed to load (see the Smithbox log)");
            return;
        }

        if (!Project.Initialized || Project.Handler?.ModelEditor == null)
            return;

        Write($"{game}: loaded in {StageTime.Elapsed:mm\\:ss}");
        CurrentStage = Stage.Opening;
        StageTime.Restart();
    }

    /// <summary>Brings the Model Editor in front until its model list is read and its viewport is drawn</summary>
    private static void UpdateOpening()
    {
        var game = Games[GameIndex];

        Project.Handler.FocusEditorWindow(Project.Handler.ModelEditor);

        var view = Project.Handler.ModelEditor.ViewHandler?.ActiveView;
        var models = Project.Handler.ModelData?.PrimaryBank?.Models;

        if (StageTime.Elapsed > OpenTimeout)
        {
            var viewport = view?.ViewportWindow?.Viewport as VulkanViewport;
            Fail(game, $"the Model Editor didn't open: view {(view == null ? "missing" : "found")}, " +
                $"{models?.Count.ToString() ?? "no"} models listed, viewport {(viewport == null ? "missing" : viewport.Visible ? "visible" : "hidden")}");
            return;
        }

        if (view?.RenderSnapshotTool == null || models == null || models.Count == 0)
            return;

        if (view.ViewportWindow?.Viewport is not VulkanViewport { Visible: true })
            return;

        RunStarted = view.RenderSnapshotTool.Start(out var reason);

        if (!RunStarted)
        {
            Fail(game, $"the capture couldn't start: {reason}");
            return;
        }

        Write($"{game}: capturing");
        CurrentStage = Stage.Capturing;
        StageTime.Restart();
    }

    private static void UpdateCapturing()
    {
        var game = Games[GameIndex];
        var tool = Project.Handler.ModelEditor.ViewHandler?.ActiveView?.RenderSnapshotTool;

        // The editor is kept in front, as the snapshot is only drawn while the viewport is on screen
        Project.Handler.FocusEditorWindow(Project.Handler.ModelEditor);

        if (tool == null)
        {
            Fail(game, "the Model Editor's view closed");
            return;
        }

        if (StageTime.Elapsed > CaptureTimeout)
        {
            tool.Cancel();
            Fail(game, "the capture timed out");
            return;
        }

        if (tool.IsRunning)
            return;

        var (ok, total) = tool.LastRunCounts;
        Write($"{game}: {tool.LastStatus} ({ok} of {total}, {StageTime.Elapsed:mm\\:ss})");

        foreach (var failure in tool.LastRunFailures)
            Write($"{game}:   {failure}");

        if (ok < total)
            Failures++;

        CurrentStage = Stage.Unloading;
    }

    private static void Fail(string game, string reason)
    {
        Write($"{game}: {reason}");
        Failures++;
        CurrentStage = Stage.Unloading;
    }

    private static void UpdateUnloading()
    {
        if (Project != null && (Project.Initialized || Project.IsInitializing))
        {
            // A load still running is left to finish first, as disposing it midway would race its tasks
            if (LoadTask != null && !LoadTask.IsCompleted)
                return;

            Smithbox.Orchestrator.UnloadProject(Project);
        }

        Project = null;
        LoadTask = null;
        GameIndex++;
        StartGame();
    }

    private static void Finish()
    {
        if (CurrentStage == Stage.Done)
            return;

        CurrentStage = Stage.Done;

        CFG.Current.Viewport_Render_Mode = SavedRenderMode;
        CFG.Current.ModelEditor_RenderSnapshot_Folder = SavedFolder;

        Write(Failures == 0 ? "Done" : $"Done, with {Failures} failure(s)");
        Log?.Dispose();
        Log = null;

        Environment.ExitCode = Failures == 0 ? 0 : 1;

        // Nothing was edited, so there is nothing to ask about
        Smithbox.Instance._context.Window.Close();
    }
}
