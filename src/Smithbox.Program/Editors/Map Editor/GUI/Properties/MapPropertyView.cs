using Andre.Formats;
using CsvHelper;
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.HavokEditor;
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
    public MapHavokPropertyView HavokPropertyView;

    public string MapPropFilter = "";
    public bool ExactMapPropFilter = false;

    private MapCollisionViewMode CollisionViewMode = MapCollisionViewMode.MSB;
    private MapNavmeshViewMode NavmeshViewMode = MapNavmeshViewMode.NVA;

    public MapPropertyView(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        MsbPropertyView = new(view, project);
        HavokPropertyView = new(view, project);
    }

    public void Display()
    {
        DisplayCommonHeader();

        var supportsMultEditModes = IsCollisionType();

        if (IsCollisionType())
        {
            DisplayCollisionViewModeSelect();

            if(CollisionViewMode is MapCollisionViewMode.MSB)
            {
                MsbPropertyView.Display();
            }

            if (CollisionViewMode is MapCollisionViewMode.CollisionHKX)
            {
                DisplayCollisionTypeHeader();

                HavokPropertyView.SetPropertyMode(HavokPropertyMode.Collision);
                HavokPropertyView.Display();
            }
        }
        else if (IsNavmeshType())
        {
            DisplayNavmeshViewModeSelect();

            if (NavmeshViewMode is MapNavmeshViewMode.NVA)
            {
                MsbPropertyView.Display();
            }

            if (NavmeshViewMode is MapNavmeshViewMode.NavmeshHKX)
            {
                DisplayNavmeshTypeHeader();

                HavokPropertyView.SetPropertyMode(HavokPropertyMode.Navmesh);
                HavokPropertyView.Display();
            }
        }
        else
        {
            MsbPropertyView.Display();
        }
    }

    public void DisplayCommonHeader()
    {
        ImGui.BeginChild($"framedList_MapProperties", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("MapPropSearch", ref MapPropFilter, ref ExactMapPropFilter);

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

        if (IsCollisionType() || IsNavmeshType())
        {
            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Calculator}##toggleTypeCol"))
            {
                CFG.Current.MapEditor_HavokEdit_Display_Type_Column = !CFG.Current.MapEditor_HavokEdit_Display_Type_Column;
            }

            var typeColumnVis = "Internal";
            if (CFG.Current.MapEditor_HavokEdit_Display_Type_Column)
                typeColumnVis = "Community";

            GUI.Tooltip($"Toggle the visibilty of the field type column.\nCurrent Mode: {typeColumnVis}");

            ImGui.SameLine();

            if (ImGui.Button($"{Icons.Database}##toggleRawDataFields"))
            {
                CFG.Current.MapEditor_CollisionEdit_Display_Raw_Data_Fields = !CFG.Current.MapEditor_CollisionEdit_Display_Raw_Data_Fields;
            }

            var rawDataVis = "Hide Mesh Data";
            if (CFG.Current.MapEditor_CollisionEdit_Display_Raw_Data_Fields)
                rawDataVis = "Show Mesh Data";

            GUI.Tooltip($"Toggle the visibilty of fields tagged as 'mesh data'.\nCurrent Mode: {rawDataVis}");
        }
        else
        {
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
        }

        ImGui.EndChild();
    }

    public void DisplayCollisionTypeHeader()
    {
        ImGui.BeginChild("editTypeCollisionSection", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        var previewName = LOC.Get(HavokPropertyView.CollisionEditType.GetDisplayName());

        if (ImGui.BeginCombo("##editTypeCollisionSelect", previewName))
        {
            foreach (var entry in Enum.GetValues(typeof(MapCollisionEditType)))
            {
                var curType = (MapCollisionEditType)entry;

                var displayName = LOC.Get(curType.GetDisplayName());

                if (ImGui.Selectable(displayName, curType == HavokPropertyView.CollisionEditType))
                {
                    HavokPropertyView.CollisionEditType = curType;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip("The type of collision file to edit.");

        ImGui.EndChild();
    }

    public void DisplayNavmeshTypeHeader()
    {
        ImGui.BeginChild("editTypeNavmeshSection", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        var previewName = LOC.Get(HavokPropertyView.NavmeshEditType.GetDisplayName());

        if (ImGui.BeginCombo("##editTypeNavmeshSelect", previewName))
        {
            foreach (var entry in Enum.GetValues(typeof(MapNavmeshEditType)))
            {
                var curType = (MapNavmeshEditType)entry;

                var displayName = LOC.Get(curType.GetDisplayName());

                if (ImGui.Selectable(displayName, curType == HavokPropertyView.NavmeshEditType))
                {
                    HavokPropertyView.NavmeshEditType = curType;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip("The type of navmesh file to edit.");

        ImGui.EndChild();
    }

    public void DisplayCollisionViewModeSelect()
    {
        ImGui.BeginChild("collisionViewModeSection", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        var previewName = LOC.Get(CollisionViewMode.GetDisplayName());

        if (ImGui.BeginCombo("##collisionViewModeSelect", previewName))
        {
            foreach (var entry in Enum.GetValues(typeof(MapCollisionViewMode)))
            {
                var curType = (MapCollisionViewMode)entry;

                var displayName = LOC.Get(curType.GetDisplayName());

                if (ImGui.Selectable(displayName, curType == CollisionViewMode))
                {
                    CollisionViewMode = curType;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip("Determines which property editor to display.");

        ImGui.EndChild();
    }

    public void DisplayNavmeshViewModeSelect()
    {
        ImGui.BeginChild("navmeshViewModeSection", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        var previewName = LOC.Get(NavmeshViewMode.GetDisplayName());

        if (ImGui.BeginCombo("##navmeshViewModeSelect", previewName))
        {
            foreach (var entry in Enum.GetValues(typeof(MapNavmeshViewMode)))
            {
                var curType = (MapNavmeshViewMode)entry;

                var displayName = LOC.Get(curType.GetDisplayName());

                if (ImGui.Selectable(displayName, curType == NavmeshViewMode))
                {
                    NavmeshViewMode = curType;
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip("Determines which property editor to display.");

        ImGui.EndChild();
    }

    public bool IsCollisionType()
    {
        HashSet<Entity> entSelection = View.ViewportSelection.GetFilteredSelection<Entity>();

        if (View.Universe.HasProcessedMapLoad && entSelection.Count > 0)
        {
            Entity firstEnt = entSelection.First();

            // Is Collision Map Object
            if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
            {
                if (EntityHelper.IsPartCollision(firstEnt))
                {
                    return true;
                }
            }
        }

        return false;
    }
    public bool IsNavmeshType()
    {
        HashSet<Entity> entSelection = View.ViewportSelection.GetFilteredSelection<Entity>();

        if (View.Universe.HasProcessedMapLoad && entSelection.Count > 0)
        {
            Entity firstEnt = entSelection.First();

            // Is Navmesh Map Object
            if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
            {
                if (EntityHelper.IsNavmesh(firstEnt))
                {
                    return true;
                }
            }
        }

        return false;
    }
}
