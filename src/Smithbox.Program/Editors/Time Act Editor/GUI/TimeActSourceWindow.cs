using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActSourceWindow
{
    public TimeActEditorView View;
    public ProjectEntry Project;

    private string SourceListFilter = "";
    private bool ExactSourceListFilter = false;

    private bool _arrowKeyPressed = false;

    // Rebuilt only when the filter or the sources change, as the aliases are matched as well
    private Dictionary<ModelListType, List<TimeActContainerWrapper>> CachedSources = new();
    private Dictionary<ModelListType, int> CachedSourceCounts = new();
    private string CachedFilter = "";
    private bool CachedExactFilter = false;

    public TimeActSourceWindow(TimeActEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display(float width, float height)
    {
        GUI.TitleHeader(
            LOC.Get("TAE_SourceWindow_List_Header"),
            LOC.Get("TAE_SourceWindow_List_Header_TT"));

        EditorFilters.DisplayFramedListFilter("timeActEditor_SourceList_Filter",
            ref SourceListFilter, ref ExactSourceListFilter);

        var bank = Project.Handler.TimeActData.PrimaryBank;

        if (bank.IsScanning)
        {
            ImGui.TextDisabled(LOC.Get("TAE_SourceWindow_Scanning", bank.ScanProgress, bank.ScanTotal));
        }

        ImGui.BeginChild($"SourceListSection", new Vector2(0, 0), ImGuiChildFlags.Borders);

        DisplaySourceList();

        ImGui.EndChild();
    }

    public void DisplaySourceList()
    {
        var bank = Project.Handler.TimeActData.PrimaryBank;

        ImGui.BeginTabBar("sourceTabs");

        // Characters
        if (ImGui.BeginTabItem($"{LOC.Get("TAE_SourceWindow_Tab_Characters")}##characterTab"))
        {
            ImGui.BeginChild($"characterSourceList", new Vector2(0, 0), ImGuiChildFlags.Borders);

            DisplaySources(ModelListType.Character, bank.GetCharacters());

            ImGui.EndChild();
            ImGui.EndTabItem();
        }

        // Objects / Assets
        var name = LOC.Get("TAE_SourceWindow_Tab_Objects");
        if (Project.Descriptor.ProjectType is ProjectType.ER
            or ProjectType.AC6
            or ProjectType.NR
            or ProjectType.DB_NT
            or ProjectType.DB)
        {
            name = LOC.Get("TAE_SourceWindow_Tab_Assets");
        }

        if (ImGui.BeginTabItem($"{name}##objectTab"))
        {
            ImGui.BeginChild($"assetSourceList", new Vector2(0, 0), ImGuiChildFlags.Borders);

            DisplaySources(ModelListType.Asset, bank.GetAssets());

            ImGui.EndChild();
            ImGui.EndTabItem();
        }

        ImGui.EndTabBar();
    }

    private List<TimeActContainerWrapper> GetFilteredSources(ModelListType listType, List<TimeActContainerWrapper> sources)
    {
        if (CachedFilter != SourceListFilter || CachedExactFilter != ExactSourceListFilter)
        {
            CachedFilter = SourceListFilter;
            CachedExactFilter = ExactSourceListFilter;

            CachedSources.Clear();
        }

        if (CachedSources.TryGetValue(listType, out var filtered) &&
            CachedSourceCounts[listType] == sources.Count)
        {
            return filtered;
        }

        filtered = sources
            .Where(e => EditorFilters.IsMatch(SourceListFilter, e.Name, ExactSourceListFilter,
                ModelEditorUtils.GetAliasForSourceListEntry(Project, e.Name, listType), true, true))
            .OrderBy(e => e.Name)
            .ToList();

        CachedSources[listType] = filtered;
        CachedSourceCounts[listType] = sources.Count;

        return filtered;
    }

    private void DisplaySources(ModelListType listType, List<TimeActContainerWrapper> sources)
    {
        var filteredSources = GetFilteredSources(listType, sources);

        if (InputManager.HasArrowSelection())
        {
            _arrowKeyPressed = true;
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(filteredSources.Count);

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var source = filteredSources[i];

                var selected = View.Selection.SelectedTimeActContainerWrapper == source;

                var alias = ModelEditorUtils.GetAliasForSourceListEntry(Project, source.Name, listType);

                if (ImGui.Selectable($"{source.Name}##timeActSourceEntry{listType}{i}", selected))
                {
                    SelectSource(source);
                }

                if (_arrowKeyPressed && ImGui.IsItemFocused() && !selected)
                {
                    SelectSource(source);

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

    /// <summary>
    /// Lists the time acts of the source, and displays its model in the viewport.
    /// </summary>
    private void SelectSource(TimeActContainerWrapper source)
    {
        var selection = View.Selection;

        if (selection.SelectedTimeActContainerWrapper == source)
            return;

        if (selection.SelectedTimeActContainerWrapper != null)
        {
            selection.SelectedTimeActContainerWrapper.TimeActs.Clear();
        }

        selection.SelectedTimeActModelWrapper?.Unload();
        selection.SelectedTimeActModelWrapper = null;

        selection.SelectedTimeActContainerWrapper = source;
        selection.SelectedTimeAct = null;

        try
        {
            source.PopulateTimeActList();
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("TAE_Data_Failed_To_Read_Source", source.SourcePath), e);
        }

        source.PopulateModelList();

        var model = source.GetPrimaryModel();

        if (model != null)
        {
            selection.SelectedTimeActModelWrapper = model;

            model.Load();
        }
    }
}
