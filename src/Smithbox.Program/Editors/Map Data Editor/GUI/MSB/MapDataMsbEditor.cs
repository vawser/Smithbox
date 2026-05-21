using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static HKLib.hk2018.hkSerialize.CompatTypeParentInfo;

namespace StudioCore.Editors.MapDataEditor;

public class MapDataMsbEditor
{
    public MapDataEditorView View;
    public ProjectEntry Project;

    private string FileListFilter = "";
    private bool ExactFileListFilter = false;

    public MapDataMsbEditor(MapDataEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void DisplayHeader()
    {
        UIHelper.SimpleHeader("Maps", "");

        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild($"framedListFilter_msbEditor_FileList", searchHeight, ImGuiChildFlags.Borders);

        EditorFilters.DisplayListFilter("msbEditor_FileList",
            ref FileListFilter, ref ExactFileListFilter);

        ImGui.SameLine();

        if(ImGui.Button($"{Icons.Database}"))
        {
            CFG.Current.MapDataEditor_CacheLoadedMaps = !CFG.Current.MapDataEditor_CacheLoadedMaps;
        }

        var curCacheMode = "Loaded maps are loaded once and then retrieved from a cache.";
        if (!CFG.Current.MapDataEditor_CacheLoadedMaps)
            curCacheMode = "Loaded maps are re-loaded each time.";

        UIHelper.Tooltip($"Determines whether to re-load a map when a entry is selected, or to cache a load and use that on subsequent selections.\n\nCache Mode:\n{curCacheMode}");

        ImGui.EndChild();
    }

    public void DisplaySourceList()
    {
        var primaryBank = Project.Handler.MapDataHandler.PrimaryBank_MSB;

        foreach (var entry in primaryBank.Maps)
        {
            var mapKey = entry.Key.Filename;
            var alias = AliasHelper.GetMapNameAlias(Project, mapKey);

            var isMatch = EditorFilters.IsMatch(FileListFilter, mapKey, ExactFileListFilter, alias);

            if (!isMatch)
                continue;

            var isSelected = entry.Key == View.Selection.SelectedMapDescriptor;

            if(ImGui.Selectable($"{mapKey}", isSelected))
            {
                View.Selection.SelectedMapDescriptor = entry.Key;
                View.Selection.SelectedMap = entry.Value;

                var loadTask = primaryBank.LoadMap(entry.Key);
                if(loadTask.Result)
                {
                    Smithbox.Log<MapDataMsbEditor>($"Loaded map: {entry.Key.Filename}");
                }
            }

            if(alias != "")
            {
                UIHelper.DisplayAlias(alias);
            }

            // Arrow Selection
            if (ImGui.IsItemHovered() && View.Selection.SelectMapEntry)
            {
                View.Selection.SelectMapEntry = false;
                View.Selection.SelectedMapDescriptor = entry.Key;
                View.Selection.SelectedMap = entry.Value;

                var loadTask = primaryBank.LoadMap(entry.Key);
                if (loadTask.Result)
                {
                    Smithbox.Log<MapDataMsbEditor>($"Loaded map: {entry.Key.Filename}");
                }
            }

            if (ImGui.IsItemFocused())
            {
                if (InputManager.HasArrowSelection())
                {
                    View.Selection.SelectMapEntry = true;
                }
            }
        }
    }

    public void Draw()
    {
        UIHelper.SimpleHeader("Current Map", "");
    }
}
