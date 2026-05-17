using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

public class ReplicateAction
{
    public MapEditorView View;
    public ProjectEntry Project;

    public ReplicateType ReplicateType = ReplicateType.Line;
    public ReplicateLineAxis AxisType = ReplicateLineAxis.X;
    public ReplicateAction(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (View.ViewportSelection.IsSelection())
        {
            if (InputManager.IsPressed(KeybindID.MapEditor_Replicate))
            {
                ApplyReplicate();
            }
        }
    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext(Entity ent)
    {
        if (ent.WrappedObject is IMsbPart or IMsbRegion)
        {
            if (ImGui.Selectable("Replicate"))
            {
                ApplyReplicate();
            }
            UIHelper.Tooltip($"Apply the replicate configuration to the currently selected map objects.\n\nShortcut: {InputManager.GetHint(KeybindID.MapEditor_Replicate)}");
        }
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem("Replicate", InputManager.GetHint(KeybindID.MapEditor_Replicate)))
        {
            ApplyReplicate();
        }
        UIHelper.Tooltip($"Apply the replicate configuration to the currently selected map objects.");
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        UIHelper.WrappedText("Configure how the replicate action works.");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Replicate Style", "The style in which the replication occurs.");

        UIHelper.SetInputWidth();
        if (ImGui.BeginCombo("##replicateStyleSelect", ReplicateType.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(ReplicateType)))
            {
                var replicateType = (ReplicateType)entry;

                if (ImGui.Selectable(replicateType.GetDisplayName(), replicateType == ReplicateType))
                {
                    ReplicateType = replicateType;

                    switch (ReplicateType)
                    {
                        case ReplicateType.Line:
                            CFG.Current.Replicator_Mode_Line = true;
                            CFG.Current.Replicator_Mode_Circle = false;
                            CFG.Current.Replicator_Mode_Square = false;
                            CFG.Current.Replicator_Mode_Sphere = false;
                            CFG.Current.Replicator_Mode_Box = false;
                            break;
                        case ReplicateType.Circle:
                            CFG.Current.Replicator_Mode_Line = false;
                            CFG.Current.Replicator_Mode_Circle = true;
                            CFG.Current.Replicator_Mode_Square = false;
                            CFG.Current.Replicator_Mode_Sphere = false;
                            CFG.Current.Replicator_Mode_Box = false;
                            break;
                        case ReplicateType.Square:
                            CFG.Current.Replicator_Mode_Line = false;
                            CFG.Current.Replicator_Mode_Circle = false;
                            CFG.Current.Replicator_Mode_Square = true;
                            CFG.Current.Replicator_Mode_Sphere = false;
                            CFG.Current.Replicator_Mode_Box = false;
                            break;
                    }
                }
            }

            ImGui.EndCombo();
        }

        // Line
        if (CFG.Current.Replicator_Mode_Line)
        {
            UIHelper.Spacer();
            UIHelper.SimpleHeader("Line - Amount to Replicate", "Amount to Replicate", "The amount of replication instances to apply.", UI.Current.ImGui_Default_Text_Color);

            UIHelper.SetInputWidth();
            ImGui.InputInt("##Amount", ref CFG.Current.Replicator_Line_Clone_Amount);
            UIHelper.Tooltip("The amount of new entities to create (from the first selection).");

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Line - Replicate Offset", "Offset per Replicate", "The offset to apply per each replication instance.", UI.Current.ImGui_Default_Text_Color);

            UIHelper.SetInputWidth();
            ImGui.InputInt("##Offset", ref CFG.Current.Replicator_Line_Position_Offset);
            UIHelper.Tooltip("The distance between each newly created entity.");

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Line - Replicate Direction", "Replicate Direction", "The direction in which to replicate.", UI.Current.ImGui_Default_Text_Color);

            UIHelper.SetInputWidth();
            if (ImGui.BeginCombo("##replicateLineAxisSelect", AxisType.GetDisplayName()))
            {
                foreach (var entry in Enum.GetValues(typeof(ReplicateLineAxis)))
                {
                    var axisType = (ReplicateLineAxis)entry;

                    if (ImGui.Selectable(axisType.GetDisplayName(), axisType == AxisType))
                    {
                        AxisType = axisType;

                        switch (AxisType)
                        {
                            case ReplicateLineAxis.X:
                                CFG.Current.Replicator_Line_Position_Offset_Axis_X = true;
                                CFG.Current.Replicator_Line_Position_Offset_Axis_Y = false;
                                CFG.Current.Replicator_Line_Position_Offset_Axis_Z = false;
                                break;
                            case ReplicateLineAxis.Y:
                                CFG.Current.Replicator_Line_Position_Offset_Axis_X = false;
                                CFG.Current.Replicator_Line_Position_Offset_Axis_Y = true;
                                CFG.Current.Replicator_Line_Position_Offset_Axis_Z = false;
                                break;
                            case ReplicateLineAxis.Z:
                                CFG.Current.Replicator_Line_Position_Offset_Axis_X = false;
                                CFG.Current.Replicator_Line_Position_Offset_Axis_Y = false;
                                CFG.Current.Replicator_Line_Position_Offset_Axis_Z = true;
                                break;
                        }
                    }
                }

                ImGui.EndCombo();
            }
        }

        // Circle
        if (CFG.Current.Replicator_Mode_Circle)
        {
            UIHelper.Spacer();
            UIHelper.SimpleHeader("Circle - Size", "Size", "The amount of replication instances to apply to form the circle.", UI.Current.ImGui_Default_Text_Color);

            UIHelper.SetInputWidth();
            ImGui.InputInt("##Size", ref CFG.Current.Replicator_Circle_Size);
            UIHelper.Tooltip("The number of points within the circle on which the entities are placed.");

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Circle - Radius", "Circle Radius", "The circle radius to use during the replication process.", UI.Current.ImGui_Default_Text_Color);

            UIHelper.SetInputWidth();
            ImGui.SliderFloat("##Radius", ref CFG.Current.Replicator_Circle_Radius, 0.1f, 100);
            UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nThe radius of the circle on which to place the entities.");

            if (CFG.Current.Replicator_Circle_Size < 1)
                CFG.Current.Replicator_Circle_Size = 1;

        }

        // Square
        if (CFG.Current.Replicator_Mode_Square)
        {
            UIHelper.Spacer();
            UIHelper.SimpleHeader("Square - Size", "Size", "The amount of replication instances to apply to form the square.", UI.Current.ImGui_Default_Text_Color);

            UIHelper.SetInputWidth();
            ImGui.InputInt("##Size", ref CFG.Current.Replicator_Square_Size);
            UIHelper.Tooltip("The number of points on one side of the square on which the entities are placed.");

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Square - Width", "Width", "The amount of replication instances to that form the width of the square.", UI.Current.ImGui_Default_Text_Color);

            UIHelper.SetInputWidth();
            ImGui.InputFloat("##Width", ref CFG.Current.Replicator_Square_Width);
            UIHelper.Tooltip("The width of the square on which to place the entities.");

            UIHelper.Spacer();
            UIHelper.SimpleHeader("Square - Height", "Height", "The amount of replication instances to that form the height of the square.", UI.Current.ImGui_Default_Text_Color);

            UIHelper.SetInputWidth();
            ImGui.InputFloat("##Depth", ref CFG.Current.Replicator_Square_Depth);
            UIHelper.Tooltip("The depth of the square on which to place the entities.");

            if (CFG.Current.Replicator_Square_Width < 1)
                CFG.Current.Replicator_Square_Width = 1;

            if (CFG.Current.Replicator_Square_Size < 2)
                CFG.Current.Replicator_Square_Size = 2;

            if (CFG.Current.Replicator_Square_Depth < 1)
                CFG.Current.Replicator_Square_Depth = 1;
        }

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Options", "Options", "The options for the replication process.", UI.Current.ImGui_Default_Text_Color);

        if (CFG.Current.Replicator_Mode_Line)
        {
            ImGui.Checkbox("Flip Offset Direction", ref CFG.Current.Replicator_Line_Offset_Direction_Flipped);
            UIHelper.Tooltip("When enabled, the position offset will be applied in the opposite direction.");
        }

        ImGui.Checkbox("Apply Scramble Configuration", ref CFG.Current.Replicator_Apply_Scramble_Configuration);
        UIHelper.Tooltip("When enabled, the Scramble configuration settings will be applied to the newly duplicated entities.");

        if (View.Project.Descriptor.ProjectType != ProjectType.DS2S && View.Project.Descriptor.ProjectType != ProjectType.DS2 && View.Project.Descriptor.ProjectType != ProjectType.AC6)
        {
            if (ImGui.Checkbox("Increment Entity ID", ref CFG.Current.Replicator_Increment_Entity_ID))
            {
                if (CFG.Current.Replicator_Increment_Entity_ID)
                {
                    CFG.Current.Replicator_Clear_Entity_ID = false;
                }
            }
            UIHelper.Tooltip("When enabled, the replicated entities will be given new Entity ID. If disabled, the replicated entity ID will be set to 0.");
        }

        if (View.Project.Descriptor.ProjectType == ProjectType.ER || View.Project.Descriptor.ProjectType == ProjectType.AC6)
        {
            ImGui.Checkbox("Increment Instance ID", ref CFG.Current.Replicator_Increment_InstanceID);
            UIHelper.Tooltip("When enabled, the duplicated entities will be given a new valid Instance ID.");
        }

        if (View.Project.Descriptor.ProjectType == ProjectType.ER || View.Project.Descriptor.ProjectType == ProjectType.AC6)
        {
            ImGui.Checkbox("Increment Part Names for Assets", ref CFG.Current.Replicator_Increment_PartNames);
            UIHelper.Tooltip("When enabled, the duplicated Asset entities PartNames property will be updated.");
        }

        if (View.Project.Descriptor.ProjectType != ProjectType.DS2S && View.Project.Descriptor.ProjectType != ProjectType.DS2)
        {
            if (ImGui.Checkbox("Clear Entity ID", ref CFG.Current.Replicator_Clear_Entity_ID))
            {
                if (CFG.Current.Replicator_Clear_Entity_ID)
                {
                    CFG.Current.Replicator_Increment_Entity_ID = false;
                }
            }
            UIHelper.Tooltip("When enabled, the Entity ID assigned to the duplicated entities will be set to 0");

            ImGui.Checkbox("Clear Entity Group IDs", ref CFG.Current.Replicator_Clear_Entity_Group_IDs);
            UIHelper.Tooltip("When enabled, the Entity Group IDs assigned to the duplicated entities will be set to 0");
        }

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Actions", "");

        UIHelper.MultiButtonInput("replicateActions",
            "replicate", "Replicate Selection", "", ApplyReplicate);
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void ApplyReplicate()
    {
        if (View.ViewportSelection.IsSelection())
        {
            ReplicateMapObjectsAction action = new(View, View.ViewportSelection.GetFilteredSelection<MsbEntity>().ToList());

            View.ViewportActionManager.ExecuteAction(action);
        }
        else
        {
            Smithbox.LogError<ReplicateAction>("No object selected.");
        }

        View.DelayPicking();
    }
}

public enum ReplicateType
{
    [Display(Name = "Line")]
    Line,
    [Display(Name = "Circle")]
    Circle,
    [Display(Name = "Square")]
    Square
}
public enum ReplicateLineAxis
{
    [Display(Name = "X")]
    X,
    [Display(Name = "Y")]
    Y,
    [Display(Name = "Z")]
    Z
}