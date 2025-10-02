using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Interface;
using StudioCore.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Tools;

public class ReplicateAction
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public ReplicateAction(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_ReplicateSelection) && Editor.ViewportSelection.IsSelection())
        {
            ApplyReplicate();
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
            UIHelper.Tooltip($"Apply the replicate configuration to the currently selected map objects.\n\nShortcut: {KeyBindings.Current.MAP_ReplicateSelection.HintText}");
        }
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.MenuItem("Replicate", KeyBindings.Current.MAP_ReplicateSelection.HintText))
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
        var windowWidth = ImGui.GetWindowWidth();

        if (ImGui.CollapsingHeader("Replicate"))
        {
            UIHelper.SimpleHeader("Replicate Style", "Replicate Style", "The style in which the replication occurs.", UI.Current.ImGui_Default_Text_Color);

            if (ImGui.Checkbox("Line", ref CFG.Current.Replicator_Mode_Line))
            {
                CFG.Current.Replicator_Mode_Circle = false;
                CFG.Current.Replicator_Mode_Square = false;
                CFG.Current.Replicator_Mode_Sphere = false;
                CFG.Current.Replicator_Mode_Box = false;
            }
            UIHelper.Tooltip("Replicate the first selection in the Line shape.");

            ImGui.SameLine();
            if (ImGui.Checkbox("Circle", ref CFG.Current.Replicator_Mode_Circle))
            {
                CFG.Current.Replicator_Mode_Line = false;
                CFG.Current.Replicator_Mode_Square = false;
                CFG.Current.Replicator_Mode_Sphere = false;
                CFG.Current.Replicator_Mode_Box = false;
            }
            UIHelper.Tooltip("Replicate the first selection in the Circle shape.");

            ImGui.SameLine();
            if (ImGui.Checkbox("Square", ref CFG.Current.Replicator_Mode_Square))
            {
                CFG.Current.Replicator_Mode_Circle = false;
                CFG.Current.Replicator_Mode_Line = false;
                CFG.Current.Replicator_Mode_Sphere = false;
                CFG.Current.Replicator_Mode_Box = false;
            }
            UIHelper.Tooltip("Replicate the first selection in the Square shape.");
            UIHelper.WrappedText("");

            // Line
            if (CFG.Current.Replicator_Mode_Line)
            {
                UIHelper.SimpleHeader("Line - Amount to Replicate", "Amount to Replicate", "The amount of replication instances to apply.", UI.Current.ImGui_Default_Text_Color);
                DPI.ApplyInputWidth(windowWidth);
                ImGui.InputInt("##Amount", ref CFG.Current.Replicator_Line_Clone_Amount);
                UIHelper.Tooltip("The amount of new entities to create (from the first selection).");

                UIHelper.SimpleHeader("Line - Replicate Offset", "Offset per Replicate", "The offset to apply per each replication instance.", UI.Current.ImGui_Default_Text_Color);
                DPI.ApplyInputWidth(windowWidth);
                ImGui.InputInt("##Offset", ref CFG.Current.Replicator_Line_Position_Offset);
                UIHelper.Tooltip("The distance between each newly created entity.");

                UIHelper.SimpleHeader("Line - Replicate Direction", "Replicate Direction", "The direction in which to replicate.", UI.Current.ImGui_Default_Text_Color);

                if (ImGui.Checkbox("X", ref CFG.Current.Replicator_Line_Position_Offset_Axis_X))
                {
                    CFG.Current.Replicator_Line_Position_Offset_Axis_Y = false;
                    CFG.Current.Replicator_Line_Position_Offset_Axis_Z = false;
                }
                UIHelper.Tooltip("Replicate on the X-axis.");

                ImGui.SameLine();

                if (ImGui.Checkbox("Y", ref CFG.Current.Replicator_Line_Position_Offset_Axis_Y))
                {
                    CFG.Current.Replicator_Line_Position_Offset_Axis_X = false;
                    CFG.Current.Replicator_Line_Position_Offset_Axis_Z = false;
                }
                UIHelper.Tooltip("Replicate on the Y-axis.");

                ImGui.SameLine();

                if (ImGui.Checkbox("Z", ref CFG.Current.Replicator_Line_Position_Offset_Axis_Z))
                {
                    CFG.Current.Replicator_Line_Position_Offset_Axis_X = false;
                    CFG.Current.Replicator_Line_Position_Offset_Axis_Y = false;
                }
                UIHelper.Tooltip("Replicate on the Z-axis.");
            }

            // Circle
            if (CFG.Current.Replicator_Mode_Circle)
            {
                UIHelper.SimpleHeader("Circle - Size", "Size", "The amount of replication instances to apply to form the circle.", UI.Current.ImGui_Default_Text_Color);

                DPI.ApplyInputWidth(windowWidth);
                ImGui.InputInt("##Size", ref CFG.Current.Replicator_Circle_Size);
                UIHelper.Tooltip("The number of points within the circle on which the entities are placed.");

                UIHelper.SimpleHeader("Circle - Radius", "Circle Radius", "The circle radius to use during the replication process.", UI.Current.ImGui_Default_Text_Color);

                DPI.ApplyInputWidth(windowWidth);
                ImGui.SliderFloat("##Radius", ref CFG.Current.Replicator_Circle_Radius, 0.1f, 100);
                UIHelper.Tooltip("Press Ctrl+Left Click to input directly.\nThe radius of the circle on which to place the entities.");

                if (CFG.Current.Replicator_Circle_Size < 1)
                    CFG.Current.Replicator_Circle_Size = 1;

            }

            // Square
            if (CFG.Current.Replicator_Mode_Square)
            {
                UIHelper.SimpleHeader("Square - Size", "Size", "The amount of replication instances to apply to form the square.", UI.Current.ImGui_Default_Text_Color);

                DPI.ApplyInputWidth(windowWidth);
                ImGui.InputInt("##Size", ref CFG.Current.Replicator_Square_Size);
                UIHelper.Tooltip("The number of points on one side of the square on which the entities are placed.");

                UIHelper.SimpleHeader("Square - Width", "Width", "The amount of replication instances to that form the width of the square.", UI.Current.ImGui_Default_Text_Color);

                DPI.ApplyInputWidth(windowWidth);
                ImGui.InputFloat("##Width", ref CFG.Current.Replicator_Square_Width);
                UIHelper.Tooltip("The width of the square on which to place the entities.");

                UIHelper.SimpleHeader("Square - Height", "Height", "The amount of replication instances to that form the height of the square.", UI.Current.ImGui_Default_Text_Color);

                DPI.ApplyInputWidth(windowWidth);
                ImGui.InputFloat("##Depth", ref CFG.Current.Replicator_Square_Depth);
                UIHelper.Tooltip("The depth of the square on which to place the entities.");

                if (CFG.Current.Replicator_Square_Width < 1)
                    CFG.Current.Replicator_Square_Width = 1;

                if (CFG.Current.Replicator_Square_Size < 2)
                    CFG.Current.Replicator_Square_Size = 2;

                if (CFG.Current.Replicator_Square_Depth < 1)
                    CFG.Current.Replicator_Square_Depth = 1;
            }

            UIHelper.WrappedText("");

            UIHelper.SimpleHeader("Options", "Options", "The options for the replication process.", UI.Current.ImGui_Default_Text_Color);

            if (CFG.Current.Replicator_Mode_Line)
            {
                ImGui.Checkbox("Flip Offset Direction", ref CFG.Current.Replicator_Line_Offset_Direction_Flipped);
                UIHelper.Tooltip("When enabled, the position offset will be applied in the opposite direction.");
            }

            ImGui.Checkbox("Apply Scramble Configuration", ref CFG.Current.Replicator_Apply_Scramble_Configuration);
            UIHelper.Tooltip("When enabled, the Scramble configuration settings will be applied to the newly duplicated entities.");

            if (Editor.Project.ProjectType != ProjectType.DS2S && Editor.Project.ProjectType != ProjectType.DS2 && Editor.Project.ProjectType != ProjectType.AC6)
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

            if (Editor.Project.ProjectType == ProjectType.ER || Editor.Project.ProjectType == ProjectType.AC6)
            {
                ImGui.Checkbox("Increment Instance ID", ref CFG.Current.Replicator_Increment_InstanceID);
                UIHelper.Tooltip("When enabled, the duplicated entities will be given a new valid Instance ID.");
            }

            if (Editor.Project.ProjectType == ProjectType.ER || Editor.Project.ProjectType == ProjectType.AC6)
            {
                ImGui.Checkbox("Increment Part Names for Assets", ref CFG.Current.Replicator_Increment_PartNames);
                UIHelper.Tooltip("When enabled, the duplicated Asset entities PartNames property will be updated.");
            }

            if (Editor.Project.ProjectType != ProjectType.DS2S && Editor.Project.ProjectType != ProjectType.DS2)
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

            UIHelper.WrappedText("");

            if (ImGui.Button("Replicate Selection", DPI.WholeWidthButton(windowWidth, 24)))
            {
                ApplyReplicate();
            }
        }
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void ApplyReplicate()
    {
        if (Editor.ViewportSelection.IsSelection())
        {
            ReplicateMapObjectsAction action = new(Editor, Editor.ViewportSelection.GetFilteredSelection<MsbEntity>().ToList());
            Editor.EditorActionManager.ExecuteAction(action);
        }
        else
        {
            PlatformUtils.Instance.MessageBox("No object selected.", "Smithbox", MessageBoxButtons.OK);
        }
    }
}
