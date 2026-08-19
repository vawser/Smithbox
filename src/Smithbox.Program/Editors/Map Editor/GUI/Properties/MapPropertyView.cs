using Andre.Formats;
using CsvHelper;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Editors.ParamEditor;
using StudioCore.Editors.Viewport;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Reflection;

namespace StudioCore.Editors.MapEditor;

public class MapPropertyView
{
    private MapEditorView View;
    private ProjectEntry Project;

    public MapMsbPropertyView MsbPropertyView;
    public MapCollisionPropertyView MapCollisionPropertyView;

    public string MapPropFilter = "";
    public bool ExactMapPropFilter = false;

    private MapPropertyViewMode ViewMode = MapPropertyViewMode.MSB;

    public MapPropertyView(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        MsbPropertyView = new(view, project);
        MapCollisionPropertyView = new(view, project);
    }

    public void Display()
    {
        DisplayCommonHeader();

        var supportsMultEditModes = SupportsMultipleEditModes();

        if (supportsMultEditModes)
        {
            DisplayViewModeHeader();

            if(ViewMode is MapPropertyViewMode.MSB)
            {
                MsbPropertyView.Display();
            }

            if (ViewMode is MapPropertyViewMode.CollisionHKX)
            {
                MapCollisionPropertyView.DisplayTypeHeader();
                MapCollisionPropertyView.Display();
            }
        }
        else
        {
            MsbPropertyView.Display();
        }
    }

    public void DisplayCommonHeader()
    {
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild($"framedList_MapProperties", searchHeight, ImGuiChildFlags.Borders);

        EditorFilters.DisplayListFilter("MapPropSearch", ref MapPropFilter, ref ExactMapPropFilter);

        // Toggle Community Field Names
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Book}", DPI.IconButtonSize))
        {
            CFG.Current.MapEditor_Properties_Enable_Commmunity_Names = !CFG.Current.MapEditor_Properties_Enable_Commmunity_Names;
        }

        var communityFieldNameMode = "Internal";
        if (CFG.Current.MapEditor_Properties_Enable_Commmunity_Names)
            communityFieldNameMode = "Community";

        GUI.Tooltip($"Toggle field name display type between Internal and Community.\nCurrent Mode: {communityFieldNameMode}");

        // Toggle Unknown Properties
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Eye}", DPI.IconButtonSize))
        {
            CFG.Current.MapEditor_Properties_Display_Unknown_Properties = !CFG.Current.MapEditor_Properties_Display_Unknown_Properties;
        }

        var unkFieldDisplayMode = "Hidden";

        if (CFG.Current.MapEditor_Properties_Display_Unknown_Properties)
            unkFieldDisplayMode = "Visible";

        GUI.Tooltip($"Toggle the display of unknown fields.\nCurrent Mode: {unkFieldDisplayMode}");

        // Toggle Field Padding
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Hubzilla}"))
        {
            CFG.Current.MapEditor_Field_List_Display_Padding = !CFG.Current.MapEditor_Field_List_Display_Padding;
        }

        var fieldPaddingMode = "Hidden";
        if (!CFG.Current.MapEditor_Field_List_Display_Padding)
            fieldPaddingMode = "Visible";

        GUI.Tooltip($"Toggle the display of padding field.\nCurrent Mode: {fieldPaddingMode}");

        ImGui.EndChild();
    }

    public void DisplayViewModeHeader()
    {
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild("viewModeSection", searchHeight, ImGuiChildFlags.Borders);

        var previewName = LOC.Get(ViewMode.GetDisplayName());

        if (ImGui.BeginCombo("##propertyEditMode", previewName))
        {
            foreach (var entry in Enum.GetValues(typeof(MapPropertyViewMode)))
            {
                var curType = (MapPropertyViewMode)entry;

                var displayName = LOC.Get(curType.GetDisplayName());

                if (ImGui.Selectable(displayName, curType == ViewMode))
                {
                    ViewMode = curType;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip("Determines which property editor to display.");

        ImGui.EndChild();
    }

    public bool SupportsMultipleEditModes()
    {
        HashSet<Entity> entSelection = View.ViewportSelection.GetFilteredSelection<Entity>();

        if (View.Universe.HasProcessedMapLoad && entSelection.Count > 0)
        {
            Entity firstEnt = entSelection.First();

            // Is Collision Map Object
            if (Project.Descriptor.ProjectType is ProjectType.ER)
            {
                if (EntityHelper.IsPartCollision(firstEnt))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
