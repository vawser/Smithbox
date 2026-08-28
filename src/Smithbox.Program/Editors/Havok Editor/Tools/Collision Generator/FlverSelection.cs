using Hexa.NET.ImGui;
using Octokit;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public class FlverSelection
{
    public HavokEditorView View;
    public ProjectEntry Project;

    public string FlverPath = "";
    public FLVER2 SourceFlver = null;
    public bool[] FlverMeshSelection = Array.Empty<bool>();

    private bool UpdateFlverSelectionList = true;
    private ModelListType CurrentTab = ModelListType.Asset;
    private ModelListType _previousTab = ModelListType.Asset;
    private Dictionary<ModelListType, HashSet<string>> CachedSearchMatches = new();
    private string FlverListFilter = "";
    private bool ExactFlverListFilter = false;
    private bool _arrowKeyPressed = false;

    public FlverSelection(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void DisplayFlverSelectionTabs()
    {
        DisplayFlverSelectionHeader();

        ImGui.BeginChild("ContainerList", new Vector2(0, 400) * DPI.UIScale(), ImGuiChildFlags.Borders);

        ImGui.BeginTabBar("sourceTabs");

        // Objects / Assets
        var name = LOC.Get("HAVOK_FlverSelection_Tab_Objects");
        if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
        {
            name = LOC.Get("HAVOK_FlverSelection_Tab_Assets");
        }

        if (ImGui.BeginTabItem($"{name}##objectTab"))
        {
            CurrentTab = ModelListType.Asset;

            ImGui.BeginChild($"assetSourceList", new Vector2(0, 0), ImGuiChildFlags.Borders);

            DisplayModelSourceList(ModelListType.Asset, Project.Locator.AssetFiles);

            ImGui.EndChild();
            ImGui.EndTabItem();
        }

        // Map Pieces
        if (ImGui.BeginTabItem($"{LOC.Get("HAVOK_FlverSelection_MapPieces")}##mapPieceTab"))
        {
            CurrentTab = ModelListType.MapPiece;

            ImGui.BeginChild($"mapPieceSourceList", new Vector2(0, 0), ImGuiChildFlags.Borders);

            DisplayModelSourceList(ModelListType.MapPiece, Project.Locator.MapPieceFiles);

            ImGui.EndChild();
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }

    private static readonly Dictionary<ModelListType, ProjectAliasType> AliasTypeMap = new()
    {
        { ModelListType.Asset,     ProjectAliasType.Assets     },
        { ModelListType.MapPiece,  ProjectAliasType.MapPieces  },
    };

    public void DisplayFlverSelectionHeader()
    {
        ImGui.BeginChild($"havokFlverSelectionHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("flverSelection", ref FlverListFilter, ref ExactFlverListFilter);

        bool filterChanged = ImGui.IsItemDeactivatedAfterEdit();
        bool tabChanged = _previousTab != CurrentTab;

        GUI.Tooltip(LOC.Get("HAVOK_FlverSelection_Filter_TT"));

        ImGui.EndChild();

        if (filterChanged)
        {
            UpdateFlverSelectionList = true;
        }

        if (tabChanged)
        {
            _previousTab = CurrentTab;
            UpdateFlverSelectionList = true;
        }

        if (!UpdateFlverSelectionList)
            return;

        UpdateFlverSelectionList = false;

        // Get the right file dictionary for the current tab
        var fileDict = CurrentTab switch
        {
            ModelListType.Asset => Project.Locator.AssetFiles,
            ModelListType.MapPiece => Project.Locator.MapPieceFiles,
            _ => null
        };
        if (fileDict == null) return;

        var matches = new HashSet<string>();

        foreach (var entry in fileDict.Entries)
        {
            var modelName = entry.Filename;
            var nameAlias = "";

            if (CFG.Current.ModelEditor_Containers_IncludeAliasInSearch
                && AliasTypeMap.TryGetValue(CurrentTab, out var aliasType))
            {
                nameAlias = AliasHelper.GetAlias(Project, modelName, CurrentTab); // collapsed below
            }

            if (EditorFilters.IsMatch(FlverListFilter, modelName, ExactFlverListFilter, nameAlias, true, true))
            {
                matches.Add(modelName);
            }
        }

        CachedSearchMatches[CurrentTab] = matches;
    }

    public void DisplayModelSourceList(ModelListType modelListType, FileDictionary fileDictionary)
    {
        if (!CachedSearchMatches.TryGetValue(modelListType, out var matches))
            return;

        if (InputManager.HasArrowSelection())
        {
            _arrowKeyPressed = true;
        }

        var filteredEntries = fileDictionary.Entries
            .Where(e => matches.Contains(e.Filename))
            .ToList();

        var clipper = new ImGuiListClipper();
        clipper.Begin(filteredEntries.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var fileEntry = filteredEntries[i];

                bool selected = FlverPath == fileEntry.Path;

                var displayedName = $"{fileEntry.Filename}";

                var alias = ModelEditorUtils.GetAliasForSourceListEntry(Project,
                    displayedName, modelListType);

                var flags = ImGuiSelectableFlags.None;
                if (CFG.Current.ModelEditor_ModelSourceList_RequireDoubleClick)
                {
                    flags = ImGuiSelectableFlags.AllowDoubleClick;
                }

                if (ImGui.Selectable($"{displayedName}##modelSourceListEntry{modelListType.ToString()}{i}", selected,
                    flags))
                {
                    FlverPath = fileEntry.Path;
                    LoadInternalFlver(FlverPath);
                }

                if (_arrowKeyPressed && ImGui.IsItemFocused() && !selected)
                {
                    FlverPath = fileEntry.Path;
                    LoadInternalFlver(FlverPath);

                    _arrowKeyPressed = false;
                }

                if (alias != "")
                {
                    GUI.DisplayAlias(alias, CFG.Current.Interface_Alias_Wordwrap_Model_Editor);
                }
            }
        }

        clipper.End();
    }

    public void LoadInternalFlver(string path)
    {
        var binderType = ModelEditorUtils.GetContainerTypeFromRelativePath(Project, path);

        if (binderType is ResourceContainerType.None)
        {
            ReadDirect();
        }
        else if (binderType is ResourceContainerType.BND)
        {
            if (Project.Descriptor.ProjectType is ProjectType.DS1 or ProjectType.DS1R or ProjectType.DES)
            {
                ReadBND3();
            }
            else
            {
                ReadBND4();
            }
        }
        else if (binderType is ResourceContainerType.BXF)
        {
            ReadBXF();
        }
    }

    public void ReadDirect()
    {

    }

    public void ReadBND3()
    {

    }

    public void ReadBND4()
    {

    }

    public void ReadBXF()
    {

    }
}
