using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// One map part that uses a model
/// </summary>
public sealed record ModelInstance(string MapName, string PartName, string PartType);

/// <summary>
/// Model Instance Finder: which map parts use a model. Every map is read once in the background into an index of
/// model name → parts, and the maps themselves are let go. Results list every part, grouped by map; clicking one
/// loads the map in the Map Editor and selects it. Plan: Documentation/Model Editor Second Review.md, R10.
/// </summary>
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
    public bool _targetProjectFiles = true;
    public bool _looseModelNameMatch = false;

    // The index: lower-case model name → the parts using it, built from the files chosen by _targetProjectFiles
    private Dictionary<string, List<ModelInstance>> Index;
    private bool? IndexFromProjectFiles;
    private int IndexedMaps;
    private int IndexedParts;

    // The background build and its progress
    private Task<(Dictionary<string, List<ModelInstance>> Index, int Maps, int Parts)> _buildTask;
    private int _readCount;
    private int _readTotal;
    private bool _searchPending;

    // The last search: matches grouped by map, and the model names matched
    private List<(string Map, List<ModelInstance> Parts)> Results = new();
    private List<string> MatchedModels = new();
    private int ResultCount;
    private string SearchedFor;

    private bool IsBuilding => _buildTask != null && !_buildTask.IsCompleted;

    public void OnToolWindow()
    {
        UpdateBuild();

        // Model Instance Finder
        if (!ToolSections.Header($"{LOC.Get("MODEL_InstanceFinder_Header")}##modelInstanceFinder"))
            return;

        ImGui.BeginChild("ModelInstanceFinderToolSection", ImGuiChildFlags.Borders);

        GUI.WrappedText(LOC.Get("MODEL_InstanceFinder_Hint"));

        // Model Name
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_InstanceFinder_Model_Name_Header"),
            LOC.Get("MODEL_InstanceFinder_Model_Name_Header_TT"));

        GUI.SinglelineTextInput("ModelNameInput", ref _searchInput);

        var loaded = View.Selection.SelectedModelWrapper;

        GUI.ConditionalMultiButtonInput("instanceLoadedModel",
            "useLoaded", LOC.Get("MODEL_InstanceFinder_Use_Loaded"),
            LOC.Get("MODEL_InstanceFinder_Use_Loaded_TT", loaded != null ? GetMsbModelName(loaded) : "-"),
            () =>
            {
                _searchInput = GetMsbModelName(loaded);
                SearchMaps();
            },
            loaded != null);

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

        if (Index != null && !IsBuilding)
        {
            GUI.WrappedText(LOC.Get("MODEL_InstanceFinder_Index_Summary", IndexedParts, IndexedMaps, Index.Count,
                IndexFromProjectFiles == true ? LOC.Get("MODEL_InstanceFinder_Index_Project") : LOC.Get("MODEL_InstanceFinder_Index_Vanilla")));
        }

        // Results
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("MODEL_InstanceFinder_Results_Header"),
            LOC.Get("MODEL_InstanceFinder_Results_Header_TT"));

        DisplayResults();

        ImGui.EndChild();
    }

    #region Model names
    // ER and later map pieces: the file m10_00_00_00_001000 is m001000 in the MSB
    private static readonly Regex MapPieceFile = new(@"^m\d\d_\d\d_\d\d_\d\d_(\d{6})$", RegexOptions.IgnoreCase);

    // DS1: the file m1000B0A10 is m1000B0 in the MSB (the area is added to the file name)
    private static readonly Regex Ds1MapPieceFile = new(@"^(m.+)A\d\d$", RegexOptions.IgnoreCase);

    /// <summary>
    /// The name map parts give a model: the file name, except for map pieces, whose MSB names drop the map
    /// </summary>
    public string GetMsbModelName(ModelWrapper wrapper)
    {
        var name = wrapper.Name;

        if (wrapper.Parent?.MapID == null || !name.StartsWith('m') && !name.StartsWith('M'))
            return name;

        switch (Project.Descriptor.ProjectType)
        {
            case ProjectType.DS1:
            case ProjectType.DS1R:
                var ds1 = Ds1MapPieceFile.Match(name);
                return ds1.Success ? ds1.Groups[1].Value : name;

            case ProjectType.DES:
            case ProjectType.DS2:
            case ProjectType.DS2S:
                return name;

            default:
                // The reverse of ModelLocator.MapModelNameToAssetName: {mapid}_{rest} is m{rest} in the MSB
                var prefix = $"{wrapper.Parent.MapID}_";

                if (name.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    return $"m{name[prefix.Length..]}";

                var m = MapPieceFile.Match(name);
                return m.Success ? $"m{m.Groups[1].Value}" : name;
        }
    }
    #endregion

    #region Index
    /// <summary>
    /// Builds the index again before searching, to pick up maps changed since
    /// </summary>
    public void RefreshAndSearchMaps()
    {
        Index = null;

        SearchMaps();
    }

    /// <summary>
    /// Searches the index for the model, building it first (in the background) when there is none for the files
    /// chosen
    /// </summary>
    public void SearchMaps()
    {
        if (IsBuilding)
        {
            _searchPending = true;
            return;
        }

        if (Index == null || IndexFromProjectFiles != _targetProjectFiles)
        {
            StartBuild();
            _searchPending = true;
            return;
        }

        Search();
    }

    /// <summary>
    /// Reads every map on a background thread and keeps only which parts use which model; each MSB is let go once
    /// read, so the larger games don't hold every map in memory
    /// </summary>
    private void StartBuild()
    {
        var fromProject = _targetProjectFiles;
        var targetFS = fromProject ? Project.VFS.FS : Project.VFS.VanillaFS;
        var entries = Project.Locator.MapFiles.Entries.ToList();
        var projectType = Project.Descriptor.ProjectType;

        IndexFromProjectFiles = fromProject;

        _readCount = 0;
        _readTotal = entries.Count;

        _buildTask = Task.Run(() =>
        {
            var index = new Dictionary<string, List<ModelInstance>>(StringComparer.Ordinal);
            var maps = 0;
            var parts = 0;

            foreach (var entry in entries)
            {
                try
                {
                    var msbData = targetFS.ReadFile(entry.Path);
                    var msb = msbData != null ? ReadMsb(projectType, msbData.Value) : null;

                    if (msb != null)
                    {
                        maps++;

                        foreach (var part in msb.Parts.GetEntries())
                        {
                            if (string.IsNullOrEmpty(part.ModelName))
                                continue;

                            var key = part.ModelName.ToLowerInvariant();

                            if (!index.TryGetValue(key, out var list))
                                index[key] = list = new List<ModelInstance>();

                            list.Add(new ModelInstance(entry.Filename, part.Name, part.GetType().Name));
                            parts++;
                        }
                    }
                }
                catch (Exception e)
                {
                    Smithbox.LogError(this, LOC.Get("MODEL_InstanceFinder_Log_Failed_MSB_Read", entry.Path), e);
                }

                Interlocked.Increment(ref _readCount);
            }

            return (index, maps, parts);
        });
    }

    /// <summary>Picks up the index once built, and runs the search waiting on it. Called every frame.</summary>
    private void UpdateBuild()
    {
        if (_buildTask == null || !_buildTask.IsCompleted)
            return;

        if (_buildTask.IsCompletedSuccessfully)
        {
            (Index, IndexedMaps, IndexedParts) = _buildTask.Result;
        }

        _buildTask = null;

        if (_searchPending)
        {
            _searchPending = false;
            Search();
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
            _ => null
        };
    }
    #endregion

    #region Results
    private void Search()
    {
        Results = new();
        MatchedModels = new();
        ResultCount = 0;
        SearchedFor = _searchInput;

        if (Index == null || string.IsNullOrWhiteSpace(_searchInput))
            return;

        var input = _searchInput.Trim().ToLowerInvariant();

        MatchedModels = _looseModelNameMatch
            ? Index.Keys.Where(k => k.Contains(input)).OrderBy(k => k, StringComparer.Ordinal).ToList()
            : Index.ContainsKey(input) ? new List<string> { input } : new List<string>();

        var instances = MatchedModels.SelectMany(m => Index[m]).ToList();

        ResultCount = instances.Count;

        Results = instances
            .GroupBy(i => i.MapName)
            .OrderBy(g => g.Key, StringComparer.Ordinal)
            .Select(g => (g.Key, g.OrderBy(i => i.PartName, StringComparer.Ordinal).ToList()))
            .ToList();
    }

    private void DisplayResults()
    {
        if (IsBuilding)
        {
            GUI.WrappedText(LOC.Get("MODEL_InstanceFinder_Reading_Maps", Volatile.Read(ref _readCount), _readTotal));
            return;
        }

        if (SearchedFor == null)
        {
            GUI.WrappedText(LOC.Get("MODEL_InstanceFinder_Not_Searched"));
            return;
        }

        if (Results.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MODEL_InstanceFinder_No_Results"));
            return;
        }

        GUI.WrappedText(LOC.Get("MODEL_InstanceFinder_Result_Summary", ResultCount, Results.Count, MatchedModels.Count));

        ImGui.BeginChild("ModelInstanceList");

        for (var m = 0; m < Results.Count; m++)
        {
            var (map, parts) = Results[m];
            var alias = AliasHelper.GetMapNameAlias(View.Project, map);

            var open = ImGui.TreeNodeEx($"{map} ({parts.Count})##instanceMap{m}", ImGuiTreeNodeFlags.SpanAvailWidth);
            GUI.Tooltip(LOC.Get("MODEL_InstanceFinder_Map_TT"));

            // Load the map without selecting a part
            if (ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
            {
                EditorCommandQueue.AddCommand($"map/load/{map}");
            }

            if (!string.IsNullOrEmpty(alias))
            {
                ImGui.SameLine();
                GUI.DisplayAlias(alias);
            }

            if (!open)
                continue;

            for (var p = 0; p < parts.Count; p++)
            {
                var part = parts[p];

                // Several models matched (loose match): say which part is which
                var label = MatchedModels.Count > 1 || _looseModelNameMatch
                    ? $"{part.PartName}  [{part.PartType}]"
                    : part.PartName;

                if (ImGui.Selectable($"{label}##instancePart{m}_{p}"))
                {
                    EditorCommandQueue.AddCommand($"map/load/{map}");
                    EditorCommandQueue.AddCommand($"map/select/{map}/{part.PartName}/Part");
                }
                GUI.Tooltip(LOC.Get("MODEL_InstanceFinder_Part_TT", part.PartType));
            }

            ImGui.TreePop();
        }

        ImGui.EndChild();
    }
    #endregion
}
