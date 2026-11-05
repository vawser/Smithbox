using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.GparamEditor;

/// <summary>
/// The Tools window section that picks the map the viewport shows, and the time of day it is lit at
/// </summary>
public class GparamViewportPreviewTool
{
    private GparamEditorView View;
    private ProjectEntry Project;

    // The map list's filter, and the list it last gave
    private string MapFilter = "";
    private bool MapFilterExact = false;
    private string CachedFilter;
    private bool CachedExact;
    private int CachedMapCount = -1;
    private List<string> FilteredMaps = new();

    // The map picked in the list, which is loaded with the Load button
    private string ChosenMap;

    public GparamViewportPreviewTool(GparamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("GPARAM_ViewportPreview_Header")}##gparamViewportPreviewHeader"))
            return;

        ImGui.BeginChild($"GparamViewportPreviewSection_{View.ViewIndex}", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        var universe = View.Universe;

        GUI.WrappedText(LOC.Get("GPARAM_ViewportPreview_Hint"));

        if (!universe.CanLoadMaps())
        {
            GUI.Spacer();
            GUI.WrappedText(LOC.Get("GPARAM_ViewportPreview_No_Maps"));
            return;
        }

        var selectedFile = View.ViewportWindow.GetSelectedFile();
        var fileMap = universe.FindMapForFile(selectedFile);

        // Status
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("GPARAM_ViewportPreview_Status_Header"), LOC.Get("GPARAM_ViewportPreview_Status_Header_TT"));

        if (universe.LoadingMapId != null)
        {
            GUI.WrappedText(LOC.Get("GPARAM_ViewportPreview_Status_Loading", universe.LoadingMapId));
        }
        else if (universe.LoadedMapId != null)
        {
            GUI.WrappedText(LOC.Get("GPARAM_ViewportPreview_Status_Loaded", universe.LoadedMapId, universe.PartCount));

            // What lights it besides the GPARAM; the volumes and lights only in Game render mode
            var environment = universe.Environment;

            GUI.WrappedText(LOC.Get("GPARAM_ViewportPreview_Status_Environment",
                environment.ShadedVolumeCount,
                environment.ShadedLightCount, environment.LightCount,
                environment.LightmappedPartCount));
        }
        else
        {
            GUI.WrappedText(LOC.Get("GPARAM_ViewportPreview_Status_None"));
        }

        if (selectedFile != null)
        {
            GUI.WrappedText(fileMap != null
                ? LOC.Get("GPARAM_ViewportPreview_File_Map", selectedFile, fileMap)
                : LOC.Get("GPARAM_ViewportPreview_File_No_Map", selectedFile));
        }

        if (!View.ViewportWindow.LightingBank.CanUse())
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("GPARAM_ViewportPreview_Lighting_Unsupported"));
        }

        // Map
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("GPARAM_ViewportPreview_Map_Header"), LOC.Get("GPARAM_ViewportPreview_Map_Header_TT"));

        DisplayMapList();

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("GPARAM_ViewportPreview_Actions_Header"), LOC.Get("GPARAM_ViewportPreview_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput("gparamViewportPreviewActions",
            "gparamViewportLoadFileMap", LOC.Get("GPARAM_ViewportPreview_Load_File_Map"), LOC.Get("GPARAM_ViewportPreview_Load_File_Map_TT"),
            () => universe.LoadMap(fileMap), fileMap != null,

            "gparamViewportLoadChosenMap", LOC.Get("GPARAM_ViewportPreview_Load_Chosen_Map"), LOC.Get("GPARAM_ViewportPreview_Load_Chosen_Map_TT"),
            () => universe.LoadMap(ChosenMap), ChosenMap != null,

            "gparamViewportFrameMap", LOC.Get("GPARAM_ViewportPreview_Frame_Map"), LOC.Get("GPARAM_ViewportPreview_Frame_Map_TT"),
            universe.FrameMap, universe.LoadedMapId != null,

            "gparamViewportUnloadMap", LOC.Get("GPARAM_ViewportPreview_Unload_Map"), LOC.Get("GPARAM_ViewportPreview_Unload_Map_TT"),
            universe.UnloadMap, universe.LoadedMapId != null || universe.LoadingMapId != null);

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("GPARAM_ViewportPreview_Options_Header"), LOC.Get("GPARAM_ViewportPreview_Options_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("GPARAM_ViewportPreview_Auto_Load")}##gparamViewportAutoLoad", ref CFG.Current.GparamEditor_Viewport_Auto_Load_Map);
        GUI.Tooltip(LOC.Get("GPARAM_ViewportPreview_Auto_Load_TT"));

        ImGui.Checkbox($"{LOC.Get("GPARAM_ViewportPreview_Load_Assets")}##gparamViewportLoadAssets", ref CFG.Current.GparamEditor_Viewport_Load_Assets);
        GUI.Tooltip(LOC.Get("GPARAM_ViewportPreview_Load_Assets_TT"));

        // Time of Day: the Map Editor's, set in the Viewport Rendering section's Map Lighting tab, unless following
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("GPARAM_ViewportPreview_Time_Header"), LOC.Get("GPARAM_ViewportPreview_Time_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("GPARAM_ViewportPreview_Follow_Value_Time")}##gparamViewportFollowTime", ref CFG.Current.GparamEditor_Viewport_Follow_Value_Time);
        GUI.Tooltip(LOC.Get("GPARAM_ViewportPreview_Follow_Value_Time_TT"));

        GUI.WrappedText(LOC.Get("GPARAM_ViewportPreview_Lit_At", View.ViewportWindow.GetHours().ToString("0.00")));
    }

    private void DisplayMapList()
    {
        var maps = View.Universe.ListMaps();

        EditorFilters.DisplayFramedListFilter($"gparamViewportMapFilter_{View.ViewIndex}", ref MapFilter, ref MapFilterExact);

        if (CachedFilter != MapFilter || CachedExact != MapFilterExact || CachedMapCount != maps.Count)
        {
            CachedFilter = MapFilter;
            CachedExact = MapFilterExact;
            CachedMapCount = maps.Count;

            FilteredMaps = maps.Where(m => EditorFilters.IsMatch(MapFilter, m, MapFilterExact)).ToList();
        }

        var height = 160.0f * DPI.UIScale();

        ImGui.BeginChild("gparamViewportMapList", new Vector2(-1, height), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(FilteredMaps.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var map = FilteredMaps[i];
                var isLoaded = string.Equals(map, View.Universe.LoadedMapId, StringComparison.OrdinalIgnoreCase);

                if (ImGui.Selectable($"{map}##gparamViewportMap_{map}", map == ChosenMap || (ChosenMap == null && isLoaded),
                    ImGuiSelectableFlags.AllowDoubleClick))
                {
                    ChosenMap = map;

                    if (ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
                    {
                        View.Universe.LoadMap(map);
                    }
                }

                var alias = AliasHelper.GetMapNameAlias(Project, map);

                if (!string.IsNullOrEmpty(alias))
                {
                    GUI.DisplayAlias(alias);
                }
            }
        }

        clipper.End();

        ImGui.EndChild();
    }
}
