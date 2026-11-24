using StudioCore.Application;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.TextureViewer;

/// <summary>
/// The Scaleform Reloader (Texture Viewer, GFX): makes the running game (ER, AC6: any game with offsets in
/// Assets/Scaleform Reloader) load edited menu movies without a restart. A native port of gfx-reloader (MIT, lugia19):
/// the hooks (ScaleformHooks) do the game-thread work; everything else is done from here. One per project.
/// See Claude/Documentation/Scaleform.md §5 Scaleform Reloader.
/// </summary>
public class ScaleformReloader
{
    private readonly ProjectEntry Project;
    public List<ScaleformReloadOffsets> Offsets { get; }

    /// <summary>Lines for the tool and the log, made on the worker and shown on the main thread.</summary>
    private readonly ConcurrentQueue<(bool Error, string Text)> Pending = new();
    public List<(DateTime Time, bool Error, string Text)> History { get; } = new();

    public bool IsBusy => Worker != null && !Worker.IsCompleted;
    private Task Worker;

    /// <summary>Reload the saved movies after every save (GFX mode).</summary>
    public bool ReloadOnSave;

    /// <summary>Movies saved since the last reload, by project path.</summary>
    public HashSet<string> SavedSinceReload { get; } = new(StringComparer.OrdinalIgnoreCase);

    // The attached game
    private ScaleformProcess Process;
    private int ProcessId;
    private DateTime ProcessStart;
    private ulong ImageBase;
    private ScaleformReloadOffsets Active;
    private ScaleformHooks.Install Hooks;

    public string Status { get; private set; } = "";
    public bool IsAttached => Hooks != null;

    public ScaleformReloader(ProjectEntry project, List<ScaleformReloadOffsets> offsets)
    {
        Project = project;
        Offsets = offsets;
    }

    #region Messages
    private void Info(string text) => Pending.Enqueue((false, text));
    private void Error(string text) => Pending.Enqueue((true, text));

    /// <summary>Main thread: moves the worker's lines to the history and the log.</summary>
    public void Drain()
    {
        while (Pending.TryDequeue(out var line))
        {
            History.Add((DateTime.Now, line.Error, line.Text));
            if (line.Error)
                Smithbox.LogError(this, line.Text);
            else
                Smithbox.Log(this, line.Text);
        }

        if (History.Count > 200)
            History.RemoveRange(0, History.Count - 200);
    }
    #endregion

    /// <summary>The game process of one of the offsets' exes, or null.</summary>
    private static (System.Diagnostics.Process Process, ScaleformReloadOffsets Offsets) FindGame(List<ScaleformReloadOffsets> offsets)
    {
        foreach (var exe in offsets.Select(o => o.ExeName).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            var running = System.Diagnostics.Process.GetProcessesByName(Path.GetFileNameWithoutExtension(exe)).FirstOrDefault();
            if (running == null)
                continue;

            string version = "";
            try
            {
                version = running.MainModule?.FileVersionInfo.FileVersion ?? "";
            }
            catch
            {
            }

            var match = offsets.FirstOrDefault(o => o.ExeName.Equals(exe, StringComparison.OrdinalIgnoreCase) && o.ExeVersion == version);
            return (running, match);
        }
        return (null, null);
    }

    /// <summary>Attaches to the game and installs (or finds) the hooks. Worker thread.</summary>
    private bool Attach()
    {
        var (running, offsets) = FindGame(Offsets);
        if (running == null)
        {
            Error(LOC.Get("TEXVIEW_ScaleformReloader_No_Game", string.Join(", ", Offsets.Select(o => o.ExeName).Distinct())));
            Detach();
            return false;
        }

        if (offsets == null)
        {
            string version = "?";
            try { version = running.MainModule?.FileVersionInfo.FileVersion ?? "?"; } catch { }
            Error(LOC.Get("TEXVIEW_ScaleformReloader_Unsupported_Version", running.ProcessName, version,
                string.Join(", ", Offsets.Select(o => o.ExeVersion))));
            Detach();
            return false;
        }

        if (Hooks != null && ProcessId == running.Id)
            return true;

        Detach();

        try
        {
            var module = running.MainModule;
            Process = new ScaleformProcess(running.Id);
            ProcessId = running.Id;
            ProcessStart = running.StartTime;
            ImageBase = (ulong)(long)module.BaseAddress;
            Active = offsets;

            Hooks = ScaleformHooks.InstallOrFind(Process, ImageBase, (ulong)module.ModuleMemorySize, offsets);
            Process.WriteU32(Hooks.Control + ScaleformHooks.Enabled, 1);

            var me3 = running.Modules.Cast<ProcessModule>().Any(m => m.ModuleName.Contains("me3", StringComparison.OrdinalIgnoreCase));
            Info(LOC.Get(Hooks.Reused ? "TEXVIEW_ScaleformReloader_Hooks_Found" : "TEXVIEW_ScaleformReloader_Hooks_Installed", offsets.Game));
            if (!me3)
                Info(LOC.Get("TEXVIEW_ScaleformReloader_No_Me3"));

            Status = LOC.Get("TEXVIEW_ScaleformReloader_Status_Attached", offsets.Game, running.Id);
            return true;
        }
        catch (Exception e)
        {
            Error(LOC.Get("TEXVIEW_ScaleformReloader_Attach_Failed", e.Message));
            Detach();
            return false;
        }
    }

    private void Detach()
    {
        Process?.Dispose();
        Process = null;
        Hooks = null;
        Active = null;
        ProcessId = 0;
        Status = "";
    }

    /// <summary>Turns the hooks on or off in the game (they stay installed).</summary>
    public void SetEnabled(bool enabled)
    {
        Run(() =>
        {
            if (!Attach())
                return;
            Process.WriteU32(Hooks.Control + ScaleformHooks.Enabled, enabled ? 1u : 0u);
            Info(LOC.Get(enabled ? "TEXVIEW_ScaleformReloader_Enabled" : "TEXVIEW_ScaleformReloader_Disabled"));
        });
    }

    private void Run(Action work)
    {
        if (IsBusy)
            return;

        Worker = Task.Run(() =>
        {
            try
            {
                work();
            }
            catch (Exception e)
            {
                Error(LOC.Get("TEXVIEW_ScaleformReloader_Failed", e.Message));
                Detach();
            }
        });
    }

    #region Game data
    private List<string> MenuNames()
    {
        var table = ImageBase + ScaleformReloadOffsets.Hex(Active.MenuTable);
        var names = new List<string>();
        for (int i = 0; i < Active.MenuCount; i++)
            names.Add(Process.WideString(Process.TryU64(table + (ulong)i * 16 + 8)));
        return names;
    }

    private ulong ScaleformSystem()
    {
        var holder = Process.TryU64(ImageBase + ScaleformReloadOffsets.Hex(Active.ScaleformHolder));
        return holder == 0 ? 0 : Process.TryU64(holder + 8);
    }

    private string NodeKey(ulong node)
    {
        var text = ScaleformReloadOffsets.Hex(Active.MapKeyText);
        var length = (int)Math.Min(Process.TryU64(node + ScaleformReloadOffsets.Hex(Active.MapKeyLength)), 256);

        if (Active.MapKeyKind == "WString")
        {
            var inline = Process.TryU64(node + ScaleformReloadOffsets.Hex(Active.MapKeyCapacity)) < 8;
            var chars = inline ? node + text : Process.TryU64(node + text);
            return Process.WideString(chars, length, true);
        }

        return Process.WideString(Process.TryU64(node + text), length, true);
    }

    /// <summary>The movie cache's node for a menu (MSVC std::map: left +0, parent +8, right +0x10, is-nil +0x19).</summary>
    private ulong CacheNode(ulong system, string name)
    {
        var head = Process.TryU64(system + ScaleformReloadOffsets.Hex(Active.CacheMapHead));
        if (head == 0)
            return 0;

        var stack = new Stack<ulong>();
        stack.Push(Process.TryU64(head + 8));
        int visited = 0;
        while (stack.Count > 0 && visited < 1000)
        {
            var node = stack.Pop();
            if (node == 0 || node == head || !Process.TryRead(node + 0x19, 1, out var nil) || nil[0] != 0)
                continue;
            visited++;
            if (NodeKey(node) == name)
                return node;
            stack.Push(Process.TryU64(node));
            stack.Push(Process.TryU64(node + 0x10));
        }
        return 0;
    }

    /// <summary>The windows that step in about 150 ms (live and closed), by pointer.</summary>
    private List<ulong> Survey()
    {
        var control = Hooks.Control;
        Process.WriteU32(control + ScaleformHooks.SurveyIndex, 0);
        Process.WriteU32(control + ScaleformHooks.SurveyFlag, 1);
        Thread.Sleep(150);
        Process.WriteU32(control + ScaleformHooks.SurveyFlag, 0);

        var count = (int)Math.Min(Process.U32(control + ScaleformHooks.SurveyIndex), ScaleformHooks.SurveySize);
        var ring = Process.Read(control + ScaleformHooks.SurveyList, ScaleformHooks.SurveySize * 8);
        var windows = new HashSet<ulong>();
        for (int i = 0; i < count; i++)
        {
            var w = BitConverter.ToUInt64(ring, i * 8);
            if (w != 0)
                windows.Add(w);
        }
        return windows.ToList();
    }

    private string WindowName(ulong window) => Process.WideString(Process.TryU64(window + ScaleformReloadOffsets.Hex(Active.WindowName)));

    private bool IsLive(ulong window)
    {
        return Process.TryU64(window + ScaleformReloadOffsets.Hex(Active.WindowController)) != 0
            && Process.TryU64(window + ScaleformReloadOffsets.Hex(Active.WindowFactory)) != 0;
    }
    #endregion

    /// <summary>Lists the menus on screen (their windows stepping now) in the history.</summary>
    public void ListOnScreen()
    {
        Run(() =>
        {
            if (!Attach())
                return;

            var heartbeat = Process.U32(Hooks.Control + ScaleformHooks.Heartbeat);
            var windows = Survey();
            if (Process.U32(Hooks.Control + ScaleformHooks.Heartbeat) == heartbeat)
            {
                Error(LOC.Get("TEXVIEW_ScaleformReloader_No_Heartbeat"));
                return;
            }

            var names = windows.Where(IsLive).Select(WindowName).Where(n => n.Length > 0).Distinct().OrderBy(n => n).ToList();
            Info(LOC.Get("TEXVIEW_ScaleformReloader_On_Screen", names.Count == 0 ? "-" : string.Join(", ", names)));
        });
    }

    /// <summary>
    /// Reloads the movies at these project paths (/menu/xx.gfx) in the running game. The files must be saved; the
    /// game reads them from the project through me3.
    /// </summary>
    public void Reload(IEnumerable<string> paths)
    {
        var list = paths.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        if (list.Count == 0)
        {
            Info(LOC.Get("TEXVIEW_ScaleformReloader_Nothing"));
            return;
        }

        Run(() =>
        {
            if (!Attach())
                return;

            var system = ScaleformSystem();
            if (system == 0)
            {
                Error(LOC.Get("TEXVIEW_ScaleformReloader_No_System"));
                return;
            }

            // Menu names for the files (the table's own spelling, for the cache keys)
            var menus = MenuNames();
            var names = new List<string>();
            foreach (var path in list)
            {
                var stem = Path.GetFileNameWithoutExtension(path);
                var name = menus.FirstOrDefault(m => m.Equals(stem, StringComparison.OrdinalIgnoreCase));
                if (name == null)
                {
                    Info(LOC.Get("TEXVIEW_ScaleformReloader_Not_A_Menu", Path.GetFileName(path)));
                    continue;
                }

                // me3 serves only the files that existed when the game started
                var file = Path.Combine(Project.Descriptor.ProjectPath, path.TrimStart('/', '\\'));
                if (File.Exists(file) && File.GetCreationTime(file) > ProcessStart)
                    Info(LOC.Get("TEXVIEW_ScaleformReloader_New_File", Path.GetFileName(path)));

                names.Add(name);
            }

            if (names.Count == 0)
                return;

            var control = Hooks.Control;
            if (Process.U32(control + ScaleformHooks.Enabled) == 0)
            {
                Error(LOC.Get("TEXVIEW_ScaleformReloader_Is_Disabled"));
                return;
            }

            // The live windows showing these menus
            var heartbeat = Process.U32(control + ScaleformHooks.Heartbeat);
            var windows = Survey();
            if (Process.U32(control + ScaleformHooks.Heartbeat) == heartbeat)
            {
                Error(LOC.Get("TEXVIEW_ScaleformReloader_No_Heartbeat"));
                return;
            }

            var targets = windows
                .Select(w => (Window: w, Name: WindowName(w)))
                .Where(w => names.Any(n => n.Equals(w.Name, StringComparison.OrdinalIgnoreCase)) && IsLive(w.Window))
                .Take(ScaleformHooks.MaxList)
                .ToList();

            // The cached movies to release
            var release = names.Select(n => CacheNode(system, n)).Where(n => n != 0)
                .Select(n => n + ScaleformReloadOffsets.Hex(Active.NodeValue))
                .Take(ScaleformHooks.MaxList).ToList();

            // A new round: Scaleform won't reuse a movie loaded at an older one
            Process.WriteU64(control + ScaleformHooks.Round, Process.U64(control + ScaleformHooks.Round) + 1);

            Process.WriteU32(control + ScaleformHooks.ReloadedCount, 0);
            var releaseBytes = new byte[ScaleformHooks.MaxList * 8];
            for (int i = 0; i < release.Count; i++)
                BitConverter.GetBytes(release[i]).CopyTo(releaseBytes, i * 8);
            Process.Write(control + ScaleformHooks.ReleaseList, releaseBytes);
            Process.WriteU32(control + ScaleformHooks.ReleaseCount, (uint)release.Count);
            Process.WriteU32(control + ScaleformHooks.ReleaseFlag, 1);

            // Then the windows: the game thread releases first, then reopens them as they step
            var targetBytes = new byte[ScaleformHooks.MaxList * 8];
            for (int i = 0; i < targets.Count; i++)
                BitConverter.GetBytes(targets[i].Window).CopyTo(targetBytes, i * 8);
            Process.Write(control + ScaleformHooks.TargetList, targetBytes);
            Process.WriteU32(control + ScaleformHooks.TargetCount, (uint)targets.Count);

            // Wait for the game thread (up to 2 s)
            var watch = Stopwatch.StartNew();
            while (watch.ElapsedMilliseconds < 2000)
            {
                var done = Process.U32(control + ScaleformHooks.ReleaseFlag) == 0
                    && (targets.Count == 0 || Process.Read(control + ScaleformHooks.TargetList, targets.Count * 8).All(b => b == 0));
                if (done)
                    break;
                Thread.Sleep(20);
            }

            var released = Process.U32(control + ScaleformHooks.ReleaseFlag) == 0;
            Process.WriteU32(control + ScaleformHooks.TargetCount, 0);
            if (!released)
            {
                // Not released in time: don't leave a release for a later, unexpected moment
                Process.WriteU32(control + ScaleformHooks.ReleaseFlag, 0);
                Error(LOC.Get("TEXVIEW_ScaleformReloader_Timeout"));
                return;
            }

            var reloadedCount = (int)Math.Min(Process.U32(control + ScaleformHooks.ReloadedCount), ScaleformHooks.MaxList);
            var reloadedBytes = Process.Read(control + ScaleformHooks.ReloadedList, ScaleformHooks.MaxList * 8);
            var reloaded = Enumerable.Range(0, reloadedCount).Select(i => BitConverter.ToUInt64(reloadedBytes, i * 8)).ToHashSet();

            foreach (var name in names)
            {
                var mine = targets.Where(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)).ToList();
                if (mine.Count == 0)
                {
                    Info(LOC.Get("TEXVIEW_ScaleformReloader_Off_Screen", name));
                    continue;
                }

                foreach (var (window, _) in mine)
                {
                    if (!reloaded.Contains(window))
                        Info(LOC.Get("TEXVIEW_ScaleformReloader_Closed_Meanwhile", name));
                    else if (!IsLive(window))
                        Error(LOC.Get("TEXVIEW_ScaleformReloader_Not_Reopened", name));
                    else
                        Info(LOC.Get("TEXVIEW_ScaleformReloader_Reloaded_Live", name));
                }
            }

            lock (SavedSinceReload)
            {
                foreach (var path in list)
                    SavedSinceReload.Remove(path);
            }
        });
    }
}
