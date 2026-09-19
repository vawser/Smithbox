using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.HavokEditor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public class HavokScriptReloader : IDisposable
{
    public HavokEditorView View;
    public ProjectEntry Project;

    private Dictionary<FileDictionaryEntry, bool> InclusionCache = new();
    private bool BuildInclusionCache = true;

    private bool DetectFileChanges = false;
    private bool WatchersDirty = false;
    private Dictionary<FileDictionaryEntry, FileSystemWatcher> Watchers = new();
    private ConcurrentDictionary<FileDictionaryEntry, DateTime> PendingReloads = new();
    private static readonly TimeSpan ReloadDebounce = TimeSpan.FromMilliseconds(300);


    public HavokScriptReloader(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        UpdateFileChangeDetection();

        if (!CFG.Current.HavokEditor_ToolVisibility_ScriptReloader)
            return;

        if (ImGui.CollapsingHeader($"{LOC.Get("HAVOK_ScriptReloader_Title")}##scriptReloaderTool"))
        {
            ImGui.BeginChild("ScriptReloaderSection", ImGuiChildFlags.Borders);

            GUI.WrappedText(LOC.Get("HAVOK_ScriptReloader_Hint"));

            if (SupportsReloader())
            {
                DisplayReloader();
            }
            else
            {
                GUI.Spacer();
                GUI.WrappedText(LOC.Get("HAVOK_ScriptReloader_Not_Supported"));
            }

            ImGui.EndChild();
        }

        if(BuildInclusionCache)
        {
            foreach(var entry in Project.Locator.HavokScriptFiles.Entries)
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
            LOC.Get("HAVOK_ScriptReloader_Inclusion_List_Header"),
            LOC.Get("HAVOK_ScriptReloader_Inclusion_List_Header_TT"));

        DisplayInclusionList();

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("HAVOK_ScriptReloader_Actions_Header"),
            LOC.Get("HAVOK_ScriptReloader_Actions_Header_TT"));

        GUI.MultiButtonInput("directActions",
            "reloadSelected",
            LOC.Get("HAVOK_ScriptReloader_Reload_Scripts"),
            LOC.Get("HAVOK_ScriptReloader_Reload_Scripts_TT"),
            ReloadIncludedScripts);

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("HAVOK_ScriptReloader_Options_Header"),
            LOC.Get("HAVOK_ScriptReloader_Options_Header_TT"));

        if (ImGui.Checkbox($"{LOC.Get("HAVOK_ScriptReloader_Detect_File_Changes")}##detectFileChanges", ref DetectFileChanges))
        {
            WatchersDirty = true;
        }
        GUI.Tooltip(LOC.Get("HAVOK_ScriptReloader_Detect_File_Changes_TT"));

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
        }
        GUI.Tooltip(LOC.Get("HAVOK_ScriptReloader_Inclusion_Toggle_All_TT"));

        ImGui.SameLine();

        // Clear All
        if (ImGui.Button($"{Icons.Minus}##inclusionClearAll", DPI.IconButtonSize))
        {
            foreach (var entry in InclusionCache)
            {
                InclusionCache[entry.Key] = false;
            }
        }
        GUI.Tooltip(LOC.Get("HAVOK_ScriptReloader_Inclusion_Clear_All_TT"));

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
            }

            // Alias
            GUI.DisplayAlias(alias);
        }

        ImGui.EndChild();
    }

    private void ReloadIncludedScripts()
    {
        var anySelected = false;

        foreach(var entry in InclusionCache)
        {
            if(entry.Value)
            {
                anySelected = true;
                var name = entry.Key.Filename;
                var success = HavokReload.RequestReloadChr(Project, name);

                if (success)
                {
                    Smithbox.Log(this, LOC.Get("HAVOK_ScriptReloader_Log_Reloaded_Script", name));
                }
            }
        }

        if(!anySelected)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_ScriptReloader_Error_No_Scripts_Enabled"));
        }
    }

    private void ReloadScript(FileDictionaryEntry entry)
    {
        var name = entry.Filename;
        var success = HavokReload.RequestReloadChr(Project, name);

        if (success)
        {
            Smithbox.Log(this, LOC.Get("HAVOK_ScriptReloader_Log_Reloaded_Script", name));
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
            Watchers[entry].Dispose();
            Watchers.Remove(entry);
            PendingReloads.TryRemove(entry, out _);
        }

        if (!DetectFileChanges)
            return;

        foreach (var kvp in InclusionCache)
        {
            if (!kvp.Value || Watchers.ContainsKey(kvp.Key))
                continue;

            var watcher = CreateWatcher(kvp.Key);
            if (watcher != null)
            {
                Watchers.Add(kvp.Key, watcher);
            }
        }
    }

    private FileSystemWatcher CreateWatcher(FileDictionaryEntry entry)
    {
        try
        {
            var relativePath = entry.Path.TrimStart('/', '\\');
            var fullPath = Path.GetFullPath(Path.Combine(Project.Descriptor.ProjectPath, relativePath));

            var directory = Path.GetDirectoryName(fullPath);
            if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                return null;

            var watcher = new FileSystemWatcher(directory, Path.GetFileName(fullPath))
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

            watcher.Renamed += (sender, e) =>
            {
                if (string.Equals(e.FullPath, fullPath, StringComparison.OrdinalIgnoreCase))
                {
                    PendingReloads[entry] = DateTime.UtcNow;
                }
            };

            watcher.EnableRaisingEvents = true;

            return watcher;
        }
        catch (Exception ex)
        {
            Smithbox.LogError(this, LOC.Get("HAVOK_ScriptReloader_Error_Failed_Create_Watcher", ex));
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
                ReloadScript(pending.Key);
            }
        }
    }

    public void Dispose()
    {
        foreach (var watcher in Watchers.Values)
        {
            watcher.Dispose();
        }

        Watchers.Clear();
        PendingReloads.Clear();
    }
}
