using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Logger;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

public class ModelInstanceFinder
{
    public ModelEditorView View;
    public ProjectEntry Project;

    public ModelInstanceFinder(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public string _searchInput = "";
    public List<MapModelMatch> Matches = new List<MapModelMatch>();

    public List<ResourceDescriptor> resMaps = new List<ResourceDescriptor>();
    public bool _targetProjectFiles = true;
    public bool _looseModelNameMatch = false;

    private bool SetupSearch = true;

    // Whether the maps in MapList were read from the project files
    private bool? _cachedTargetProjectFiles = null;

    public Dictionary<string, IMsb> MapList = new Dictionary<string, IMsb>();

    // The background read of the maps, and its progress
    private Task<Dictionary<string, IMsb>> _readTask;
    private int _readCount;
    private int _readTotal;
    private bool _searchPending;

    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        UpdateMapRead();

        // Model Instance Finder
        if (ToolSections.Header($"{LOC.Get("MODEL_InstanceFinder_Header")}##modelInstanceFinder"))
        {
            ImGui.BeginChild("ModelInstanceFinderToolSection", ImGuiChildFlags.Borders);

            GUI.WrappedText(LOC.Get("MODEL_InstanceFinder_Hint"));

            // Model Name
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("MODEL_InstanceFinder_Model_Name_Header"),
                LOC.Get("MODEL_InstanceFinder_Model_Name_Header_TT"));

            GUI.SinglelineTextInput("ModelNameInput", ref _searchInput);

            // Options
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("MODEL_InstanceFinder_Options_Header"),
                LOC.Get("MODEL_InstanceFinder_Options_Header_TT"));

            // Target Project Files
            ImGui.Checkbox($"{LOC.Get("MODEL_InstanceFinder_Checkbox_Target_Project")}##toggleProjectFileTarget", ref _targetProjectFiles);
            GUI.Tooltip(LOC.Get("MODEL_InstanceFinder_Checkbox_Target_Project_TT"));

            // Loose Name Match
            ImGui.Checkbox($"{LOC.Get("MODEL_InstanceFinder_Checkbox_Loose_Name_Match")}##toggleLooseNameMatch", ref _looseModelNameMatch);
            GUI.Tooltip(LOC.Get("MODEL_InstanceFinder_Checkbox_Loose_Name_Match_TT"));

            // Actions
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("MODEL_InstanceFinder_Actions_Header"),
                LOC.Get("MODEL_InstanceFinder_Actions_Header_TT"));

            GUI.MultiButtonInput("instanceActions",
                "search",
                LOC.Get("MODEL_InstanceFinder_Search_Action"),
                LOC.Get("MODEL_InstanceFinder_Search_Action_TT"),
                SearchMaps,

                "refresh",
                LOC.Get("MODEL_InstanceFinder_Refresh_Action"),
                LOC.Get("MODEL_InstanceFinder_Refresh_Action_TT"),
                RefreshAndSearchMaps);

            // Results
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("MODEL_InstanceFinder_Results_Header"),
                LOC.Get("MODEL_InstanceFinder_Results_Header_TT"));

            DisplayInstances();

            ImGui.EndChild();
        }
    }

    public void DisplayInstances()
    {
        if (IsReadingMaps)
        {
            GUI.WrappedText(LOC.Get("MODEL_InstanceFinder_Reading_Maps", Volatile.Read(ref _readCount), _readTotal));
            return;
        }

        if (Matches.Count > 0)
        {
            ImGui.BeginChild("ModelInstanceList");

            foreach (var entry in Matches)
            {
                if (ImGui.Selectable($"{entry.MapName} [{entry.Count}]"))
                {
                    EditorCommandQueue.AddCommand($"map/load/{entry.MapName}");
                    EditorCommandQueue.AddCommand($"map/select/{entry.MapName}/{entry.EntityName}/Part");
                }
                var aliasName = AliasHelper.GetMapNameAlias(View.Project, entry.MapName);
                GUI.DisplayAlias(aliasName);
                GUI.Tooltip(LOC.Get("MODEL_InstanceFinder_Instances_TT"));
            }

            ImGui.EndChild();
        }
        else
        {
            GUI.WrappedText(LOC.Get("MODEL_InstanceFinder_No_Results"));
        }
    }

    /// <summary>
    /// Reads the maps again before searching, to pick up maps changed since they were last read
    /// </summary>
    public void RefreshAndSearchMaps()
    {
        SetupSearch = true;

        SearchMaps();
    }

    /// <summary>
    /// Searches the maps for the model. The maps are read once, in the background, and kept for later searches.
    /// </summary>
    public void SearchMaps()
    {
        // A search asked for while the maps are being read runs once they have been
        if (IsReadingMaps)
        {
            _searchPending = true;
            return;
        }

        // The maps are read once and kept, so they have to be read again from the other files when the target changes
        if (_cachedTargetProjectFiles != _targetProjectFiles)
        {
            SetupSearch = true;
        }

        if (SetupSearch)
        {
            SetupSearch = false;

            StartReadingMaps();

            _searchPending = true;
            return;
        }

        CompileAllResults();
    }

    private bool IsReadingMaps => _readTask != null && !_readTask.IsCompleted;

    /// <summary>
    /// Reads every map on a background thread, as reading them all takes a while for the larger games
    /// </summary>
    private void StartReadingMaps()
    {
        _cachedTargetProjectFiles = _targetProjectFiles;

        var targetFS = _targetProjectFiles ? View.Project.VFS.FS : View.Project.VFS.VanillaFS;
        var entries = View.Project.Locator.MapFiles.Entries.ToList();
        var projectType = View.Project.Descriptor.ProjectType;

        _readCount = 0;
        _readTotal = entries.Count;

        _readTask = Task.Run(() =>
        {
            var maps = new Dictionary<string, IMsb>();

            foreach (var entry in entries)
            {
                try
                {
                    var msbData = targetFS.ReadFile(entry.Path);

                    if (msbData != null)
                    {
                        var msb = ReadMsb(projectType, msbData.Value);

                        if (msb != null)
                        {
                            maps.TryAdd(entry.Filename, msb);
                        }
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_InstanceFinder_Log_Failed_MSB_Read", entry.Path), e);
                }

                Interlocked.Increment(ref _readCount);
            }

            return maps;
        });
    }

    /// <summary>
    /// Picks up the maps once they have been read, and runs the search waiting on them. Called every frame.
    /// </summary>
    private void UpdateMapRead()
    {
        if (_readTask == null || !_readTask.IsCompleted)
            return;

        if (_readTask.IsCompletedSuccessfully)
        {
            MapList = _readTask.Result;
        }

        _readTask = null;

        if (_searchPending)
        {
            _searchPending = false;

            CompileAllResults();
        }
    }

    private void CompileAllResults()
    {
        Matches = new();

        foreach (KeyValuePair<string, IMsb> entry in MapList)
        {
            CompileResults(entry.Key, entry.Value);
        }
    }

    private static IMsb ReadMsb(ProjectType projectType, Memory<byte> data)
    {
        return projectType switch
        {
            ProjectType.DES => MSBD.Read(data),
            ProjectType.DS1 or ProjectType.DS1R => MSB1.Read(data),
            ProjectType.DS2 or ProjectType.DS2S => MSB2.Read(data),
            ProjectType.DS3 => MSB3.Read(data),
            ProjectType.BB => MSBB.Read(data),
            ProjectType.SDT => MSBS.Read(data),
            ProjectType.ER => MSBE.Read(data),
            ProjectType.AC6 => MSB_AC6.Read(data),
            ProjectType.NR => MSB_NR.Read(data),
            ProjectType.DB_NT => MSB_DB_NT.Read(data),
            // TODO: Duskbloods
            _ => null
        };
    }

    public void CompileResults(string mapName, IMsb map)
    {
        var searchInput = _searchInput.ToLower();

        foreach (var entry in map.Parts.GetEntries())
        {
            var modelName = entry.ModelName;

            if (modelName != null)
            {
                modelName = modelName.ToLower();

                if (_looseModelNameMatch)
                {
                    if (modelName.Contains(searchInput))
                    {
                        if (!Matches.Any(e => e.MapName == mapName))
                        {
                            var match = new MapModelMatch(mapName, modelName, entry.Name);
                            match.Count++;
                            Matches.Add(match);
                        }
                        else
                        {
                            var curMatch = Matches.FirstOrDefault(e => e.MapName == mapName);
                            curMatch.Count++;
                        }
                    }
                }
                else
                {
                    if (modelName == searchInput)
                    {
                        if (!Matches.Any(e => e.MapName == mapName))
                        {
                            var match = new MapModelMatch(mapName, modelName, entry.Name);
                            match.Count++;
                            Matches.Add(match);
                        }
                        else
                        {
                            var curMatch = Matches.FirstOrDefault(e => e.MapName == mapName);
                            curMatch.Count++;
                        }
                    }
                }
            }
        }
    }
}

public class MapModelMatch
{
    public string MapName;
    public string ModelName;
    public string EntityName;

    public int Count = 0;

    public MapModelMatch(string mapname, string modelname, string entityName)
    {
        MapName = mapname;
        ModelName = modelname;
        EntityName = entityName;
    }
}
