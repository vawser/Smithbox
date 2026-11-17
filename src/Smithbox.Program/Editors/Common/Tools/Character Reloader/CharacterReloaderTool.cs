using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.Common;

/// <summary>
/// Reloads characters in the running game, so changes to their files (behavior, animations, scripts, etc.)
/// can be seen without restarting. Editor-agnostic: each editor view that shows it owns an instance (and its
/// file watchers), and disposes it with the view.
/// </summary>
public class CharacterReloaderTool : IDisposable
{
    public ProjectEntry Project;

    private Dictionary<FileDictionaryEntry, bool> InclusionCache = new();
    private bool BuildInclusionCache = true;

    private bool DetectFileChanges = false;
    private bool WatchersDirty = false;
    private Dictionary<FileDictionaryEntry, List<FileSystemWatcher>> Watchers = new();
    private ConcurrentDictionary<FileDictionaryEntry, DateTime> PendingReloads = new();
    private static readonly TimeSpan ReloadDebounce = TimeSpan.FromMilliseconds(300);

    public CharacterReloaderTool(ProjectEntry project)
    {
        Project = project;
    }

    public void Display()
    {
        UpdateFileChangeDetection();

        if (ToolSections.Header($"{LOC.Get("COMMON_CharacterReloader_Title")}##characterReloaderTool"))
        {
            ImGui.BeginChild("CharacterReloaderSection", ImGuiChildFlags.Borders);

            GUI.WrappedText(LOC.Get("COMMON_CharacterReloader_Hint"));

            if (SupportsReloader())
            {
                DisplayReloader();
            }
            else
            {
                GUI.Spacer();
                GUI.WrappedText(LOC.Get("COMMON_CharacterReloader_Not_Supported"));
            }

            ImGui.EndChild();
        }

        if (BuildInclusionCache)
        {
            // A character can appear more than once (e.g. in a DLC folder), but it is reloaded by name
            foreach (var entry in Project.Locator.ChrFiles.Entries
                .GroupBy(e => e.Filename)
                .Select(g => g.First())
                .OrderBy(e => e.Filename))
            {
                InclusionCache.Add(entry, false);
            }

            BuildInclusionCache = false;
        }
    }

    public bool SupportsReloader()
    {
        if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.DS3)
            return true;

        return false;
    }

    public void DisplayReloader()
    {
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("COMMON_CharacterReloader_Inclusion_List_Header"),
            LOC.Get("COMMON_CharacterReloader_Inclusion_List_Header_TT"));

        DisplayInclusionList();

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("COMMON_CharacterReloader_Actions_Header"),
            LOC.Get("COMMON_CharacterReloader_Actions_Header_TT"));

        GUI.MultiButtonInput("directActions",
            "reloadSelected",
            LOC.Get("COMMON_CharacterReloader_Reload_Characters"),
            LOC.Get("COMMON_CharacterReloader_Reload_Characters_TT"),
            ReloadIncludedCharacters);

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("COMMON_CharacterReloader_Options_Header"),
            LOC.Get("COMMON_CharacterReloader_Options_Header_TT"));

        if (ImGui.Checkbox($"{LOC.Get("COMMON_CharacterReloader_Detect_File_Changes")}##detectFileChanges", ref DetectFileChanges))
        {
            WatchersDirty = true;
        }
        GUI.Tooltip(LOC.Get("COMMON_CharacterReloader_Detect_File_Changes_TT"));
    }

    private string ListFilter = "";
    private bool ExactListFilter = false;

    private void DisplayInclusionList()
    {
        ImGui.BeginChild($"framedList_TabListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        ImGui.PushItemWidth(150f * DPI.UIScale());
        EditorFilters.DisplaySearchbar("inclusionHeader", ref ListFilter, ref ExactListFilter);

        ImGui.SameLine();

        // Toggle All
        if (ImGui.Button($"{Icons.Plus}##inclusionToggleAll", DPI.IconButtonSize))
        {
            foreach (var entry in InclusionCache)
            {
                InclusionCache[entry.Key] = true;
            }

            WatchersDirty = true;
        }
        GUI.Tooltip(LOC.Get("COMMON_CharacterReloader_Inclusion_Toggle_All_TT"));

        ImGui.SameLine();

        // Clear All
        if (ImGui.Button($"{Icons.Minus}##inclusionClearAll", DPI.IconButtonSize))
        {
            foreach (var entry in InclusionCache)
            {
                InclusionCache[entry.Key] = false;
            }

            WatchersDirty = true;
        }
        GUI.Tooltip(LOC.Get("COMMON_CharacterReloader_Inclusion_Clear_All_TT"));

        ImGui.EndChild();

        ImGui.BeginChild($"##inclusionSection", new Vector2(0, 200) * DPI.UIScale(), ImGuiChildFlags.Borders);

        foreach (var entry in InclusionCache)
        {
            var alias = AliasHelper.GetCharacterAlias(Project, entry.Key.Filename);

            var isMatch = EditorFilters.IsMatch(ListFilter, entry.Key.Filename, ExactListFilter, alias);

            if (!isMatch)
                continue;

            var curValue = entry.Value;
            ImGui.Checkbox($"{entry.Key.Filename}##toggleField_{entry.Key.Filename}", ref curValue);
            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                InclusionCache[entry.Key] = curValue;
                WatchersDirty = true;
            }

            // Alias
            GUI.DisplayAlias(alias);
        }

        ImGui.EndChild();
    }

    private void ReloadIncludedCharacters()
    {
        var anySelected = false;

        foreach (var entry in InclusionCache)
        {
            if (entry.Value)
            {
                anySelected = true;
                ReloadCharacter(entry.Key);
            }
        }

        if (!anySelected)
        {
            Smithbox.LogError(this, LOC.Get("COMMON_CharacterReloader_Error_No_Characters_Enabled"));
        }
    }

    private void ReloadCharacter(FileDictionaryEntry entry)
    {
        var name = entry.Filename;
        var success = CharacterReload.RequestReloadChr(Project, name);

        if (success)
        {
            Smithbox.Log(this, LOC.Get("COMMON_CharacterReloader_Log_Reloaded_Character", name));
        }
    }

    private void UpdateFileChangeDetection()
    {
        if (WatchersDirty)
        {
            SyncWatchers();
        }

        ProcessPendingReloads();
    }

    private void SyncWatchers()
    {
        WatchersDirty = false;

        var stale = new List<FileDictionaryEntry>();
        foreach (var entry in Watchers.Keys)
        {
            if (!DetectFileChanges || !InclusionCache.TryGetValue(entry, out var included) || !included)
            {
                stale.Add(entry);
            }
        }

        foreach (var entry in stale)
        {
            foreach (var watcher in Watchers[entry])
            {
                watcher.Dispose();
            }

            Watchers.Remove(entry);
            PendingReloads.TryRemove(entry, out _);
        }

        if (!DetectFileChanges)
            return;

        foreach (var kvp in InclusionCache)
        {
            if (!kvp.Value || Watchers.ContainsKey(kvp.Key))
                continue;

            var watchers = CreateWatchers(kvp.Key);
            if (watchers.Count > 0)
            {
                Watchers.Add(kvp.Key, watchers);
            }
        }
    }

    /// <summary>
    /// Watch every file of the character in the project: the files beside its chrbnd that start with
    /// its name (chrbnd, anibnd, behbnd, texbnd, etc.) and its Havok script.
    /// </summary>
    private List<FileSystemWatcher> CreateWatchers(FileDictionaryEntry entry)
    {
        var watchers = new List<FileSystemWatcher>();

        var chrWatcher = CreateWatcher(entry, entry.Path, $"{entry.Filename}*");
        if (chrWatcher != null)
        {
            watchers.Add(chrWatcher);
        }

        var scriptEntry = Project.Locator.HavokScriptFiles.Entries
            .FirstOrDefault(e => string.Equals(e.Filename, entry.Filename, StringComparison.OrdinalIgnoreCase));

        if (scriptEntry != null)
        {
            var scriptWatcher = CreateWatcher(entry, scriptEntry.Path, null);
            if (scriptWatcher != null)
            {
                watchers.Add(scriptWatcher);
            }
        }

        return watchers;
    }

    /// <summary>
    /// Watch the project's copy of a file. The filter defaults to the file's own name.
    /// </summary>
    private FileSystemWatcher CreateWatcher(FileDictionaryEntry entry, string path, string filter)
    {
        try
        {
            var relativePath = path.TrimStart('/', '\\');
            var fullPath = Path.GetFullPath(Path.Combine(Project.Descriptor.ProjectPath, relativePath));

            var directory = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return null;

            var watcher = new FileSystemWatcher(directory, filter ?? Path.GetFileName(fullPath))
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
                IncludeSubdirectories = false
            };

            void OnChanged(object sender, FileSystemEventArgs e)
            {
                PendingReloads[entry] = DateTime.UtcNow;
            }

            watcher.Changed += OnChanged;
            watcher.Created += OnChanged;
            watcher.Renamed += (sender, e) => OnChanged(sender, e);

            watcher.EnableRaisingEvents = true;

            return watcher;
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("COMMON_CharacterReloader_Error_Failed_Create_Watcher"), ex);
            return null;
        }
    }

    private void ProcessPendingReloads()
    {
        if (PendingReloads.IsEmpty)
            return;

        var now = DateTime.UtcNow;

        foreach (var pending in PendingReloads)
        {
            if (now - pending.Value < ReloadDebounce)
                continue;

            if (!PendingReloads.TryRemove(pending))
                continue;

            if (DetectFileChanges && InclusionCache.TryGetValue(pending.Key, out var included) && included)
            {
                ReloadCharacter(pending.Key);
            }
        }
    }

    public void Dispose()
    {
        foreach (var watchers in Watchers.Values)
        {
            foreach (var watcher in watchers)
            {
                watcher.Dispose();
            }
        }

        Watchers.Clear();
        PendingReloads.Clear();
    }
}
