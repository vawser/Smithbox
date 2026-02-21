using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Preferences;

public class ViewportPrefs
{
    public static Type GetPrefType()
    {
        return typeof(ViewportPrefs);
    }

    #region General
    public static PreferenceItem Viewport_Frame_Rate()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.General,

            Title = "Frame Rate",
            Description = "Adjusts the frame rate of the viewport.",

            Draw = () =>
            {
                DPI.ApplyInputWidth();
                if (ImGui.SliderFloat("##inputValue", ref CFG.Current.Viewport_Frame_Rate, 20.0f, 240.0f))
                {
                    CFG.Current.Viewport_Frame_Rate = (float)Math.Round(CFG.Current.Viewport_Frame_Rate);
                }
            }
        };
    }


    #endregion

    #region Rendering
    public static PreferenceItem Viewport_Enable_Rendering()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = SectionCategory.Rendering,

            Title = "Enable Model Rendering",
            Description = "If enabled, model rendering will occur within the viewport.",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Viewport_Enable_Rendering);
            }
        };
    }

    public static PreferenceItem Viewport_Enable_Texturing()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = SectionCategory.Rendering,

            Title = "Enable Texture Rendering",
            Description = "If enabled, texture rendering will occur within the viewport.",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Viewport_Enable_Texturing);
            }
        };
    }

    public static PreferenceItem Viewport_Enable_Culling()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = SectionCategory.Rendering,

            Title = "Enable Frustum Culling",
            Description = "If enabled, entities outside of the camera frustum to be culled within the viewport.",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Viewport_Enable_Culling);
            }
        };
    }

    public static PreferenceItem Viewport_Limit_Buffer_Indirect_Draw()
    {
        return new PreferenceItem
        {
            OrderID = 4,
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.Rendering,

            Title = "Indirect Draw Buffer",
            Description = "This value constrains the size of the indirect draw buffer. Exceeding this value will throw an exception.",

            Draw = () =>
            {
                DPI.ApplyInputWidth();
                Utils.ImGui_InputUint("##inputValue", ref CFG.Current.Viewport_Limit_Buffer_Indirect_Draw);
            }
        };
    }

    public static PreferenceItem Viewport_Limit_Buffer_Flver_Bone()
    {
        return new PreferenceItem
        {
            OrderID = 5,
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.Rendering,

            Title = "FLVER Bone Buffer",
            Description = "This value constrains the size of the FLVER bone buffer. Exceeding this value will throw an exception.",

            Draw = () =>
            {
                DPI.ApplyInputWidth();
                Utils.ImGui_InputUint("##inputValue", ref CFG.Current.Viewport_Limit_Buffer_Flver_Bone);
            }
        };
    }

    #endregion

    #region Model Rendering
    public static PreferenceItem Viewport_Enable_Model_Masks()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = SectionCategory.Rendering,

            Title = "Enable Model Masks",
            Description = "If enabled, model masks will be accounted for when rendering viewport objects.",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Viewport_Enable_Model_Masks);
            }
        };
    }

    public static PreferenceItem Viewport_Enable_LOD_Facesets()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = SectionCategory.ModelRendering,

            Title = "Enable LOD Facesets",
            Description = "If enabled, render all facesets for all FLVER meshes, including LOD ones.",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Viewport_Enable_LOD_Facesets);
            }
        };
    }

    public static PreferenceItem Viewport_Untextured_Model_Brightness()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.ModelRendering,

            Title = "Untextured Model Brightness",
            Description = "Change the brightness of untextured models.",

            Draw = () =>
            {
                DPI.ApplyInputWidth();
                ImGui.InputFloat("##inputValue", ref CFG.Current.Viewport_Untextured_Model_Brightness);
            }
        };
    }
    public static PreferenceItem Viewport_Untextured_Model_Saturation()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.ModelRendering,

            Title = "Untextured Model Saturation",
            Description = "Change the saturation of untextured models.",

            Draw = () =>
            {
                DPI.ApplyInputWidth();
                ImGui.InputFloat("##inputValue", ref CFG.Current.Viewport_Untextured_Model_Saturation);
            }
        };
    }

    #endregion

    #region Selection
    public static PreferenceItem Viewport_Enable_Selection_Tint()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = SectionCategory.Selection,

            Title = "Enable Selection Tint",
            Description = "If enabled, selected objects will be tinted.",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Viewport_Enable_Selection_Tint);
            }
        };
    }

    public static PreferenceItem Viewport_Enable_Selection_Outline()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = SectionCategory.Selection,

            Title = "Enable Selection Outline",
            Description = "If enabled, a selection outline will display alongside the selection tint on selected viewport objects.\nNote: the outline is error-prone when hundreds (or more) of viewport objects are selected.",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Viewport_Enable_Selection_Outline);
            }
        };
    }


    public static PreferenceItem Viewport_Enable_Box_Selection()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = SectionCategory.Selection,

            Title = "Enable Box Drag Selection",
            Description = "If enabled, you can click and drag the mouse to select multiple objects. (Ctrl: Subtract, Shift: Add)",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Viewport_Enable_Box_Selection);
            }
        };
    }

    public static PreferenceItem Viewport_Frame_Distance()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = SectionCategory.Selection,

            Title = "Frame Selection Distance",
            Description = "Determine the distance the camera is placed at when framing a selection in the viewport.",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.DragFloat3("##inputValue", ref CFG.Current.Viewport_Frame_Distance);
            }
        };
    }

    public static PreferenceItem Viewport_Frame_Offset()
    {
        return new PreferenceItem
        {
            OrderID = 4,
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = SectionCategory.Selection,

            Title = "Frame Selection Offset",
            Description = "Determine the offset applied to the camera when framing a selection in the viewport.",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.DragFloat3("##inputValue", ref CFG.Current.Viewport_Frame_Offset);
            }
        };
    }

    #endregion

    #region Coloring
    public static PreferenceItem Viewport_Wireframe_Color_Variance()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.Coloring,

            Title = "Wireframe Color Variance",
            Description = "The variance factor when adjusting the coloring of wireframe viewport objects.",

            Draw = () =>
            {
                DPI.ApplyInputWidth();
                ImGui.SliderFloat("##inputValue", ref CFG.Current.Viewport_Wireframe_Color_Variance, 0.0f, 1.0f);
            }
        };
    }

    public static PreferenceItem GeneralColoring()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.Coloring,

            Title = "",
            Description = "",

            Draw = () =>
            {
                UIHelper.SimpleHeader("General", "");

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Viewport Background Color", ref CFG.Current.Viewport_Bg_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Selection Outline Color", ref CFG.Current.Viewport_Selection_Outline_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Selection Tint Color (Textured)", ref CFG.Current.Viewport_Selection_Tint_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Selection Tint Color (Untextured)", ref CFG.Current.Viewport_Untextured_Selection_Tint_Color);


                DPI.ApplyInputWidth();
                ImGui.DragFloat("Selection Tint Strength", ref CFG.Current.Viewport_Selection_Tint_Strength, 0.0f, 1.0f);
                if (ImGui.IsItemDeactivatedAfterEdit())
                {
                    if (CFG.Current.Viewport_Selection_Tint_Strength < 0.0f)
                        CFG.Current.Viewport_Selection_Tint_Strength = 0.0f;

                    if (CFG.Current.Viewport_Selection_Tint_Strength > 1.0f)
                        CFG.Current.Viewport_Selection_Tint_Strength = 1.0f;
                }
            }
        };
    }

    public static PreferenceItem GizmoColoring()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.Coloring,

            Title = "",
            Description = "",

            Draw = () =>
            {
                UIHelper.SimpleHeader("Gizmo", "");

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("X-Axis: Base Color", ref CFG.Current.Viewport_Gizmo_X_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("X-Axis: Highlight Color", ref CFG.Current.Viewport_Gizmo_X_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Y-Axis: Base Color", ref CFG.Current.Viewport_Gizmo_Y_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Y-Axis: Highlight Color", ref CFG.Current.Viewport_Gizmo_Y_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Z-Axis: Base Color", ref CFG.Current.Viewport_Gizmo_Z_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Z-Axis: Highlight Color", ref CFG.Current.Viewport_Gizmo_Z_Highlight_Color);
            }
        };
    }

    public static PreferenceItem PartsColoring()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.Coloring,

            Title = "",
            Description = "",

            Draw = () =>
            {
                UIHelper.SimpleHeader("Parts", "");

                var curColor = Utils.GetDecimalColor(CFG.Current.Viewport_Collision_Color);
                DPI.ApplyInputWidth();
                if (ImGui.ColorEdit3("Collision: Base Color", ref curColor))
                {
                    CFG.Current.Viewport_Collision_Color = Utils.GetRgbColor(curColor);
                }

                curColor = Utils.GetDecimalColor(CFG.Current.Viewport_Connect_Collision_Color);
                DPI.ApplyInputWidth();
                if (ImGui.ColorEdit3("Connect Collision: Base Color", ref curColor))
                {
                    CFG.Current.Viewport_Connect_Collision_Color = Utils.GetRgbColor(curColor);
                }

                curColor = Utils.GetDecimalColor(CFG.Current.Viewport_Navmesh_Color);
                DPI.ApplyInputWidth();
                if (ImGui.ColorEdit3("Navmesh: Base Color", ref curColor))
                {
                    CFG.Current.Viewport_Navmesh_Color = Utils.GetRgbColor(curColor);
                }

                curColor = Utils.GetDecimalColor(CFG.Current.Viewport_Navmesh_Gate_Color);
                DPI.ApplyInputWidth();
                if (ImGui.ColorEdit3("Navmesh Gate: Base Color", ref curColor))
                {
                    CFG.Current.Viewport_Navmesh_Gate_Color = Utils.GetRgbColor(curColor);
                }
            }
        };
    }

    public static PreferenceItem WireframeColoring()
    {
        return new PreferenceItem
        {
            OrderID = 4,
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = SectionCategory.Coloring,

            Title = "",
            Description = "",

            Draw = () =>
            {
                UIHelper.SimpleHeader("Wireframes", "");

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Box Region: Base Color", ref CFG.Current.Viewport_Box_Region_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Box Region: Highlight Color", ref CFG.Current.Viewport_Box_Region_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat("Box Region: Transparency", ref CFG.Current.Viewport_Box_Region_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Cylinder Region: Base Color", ref CFG.Current.Viewport_Cylinder_Region_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Cylinder Region: Highlight Color", ref CFG.Current.Viewport_Cylinder_Region_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat("Cylinder Region: Transparency", ref CFG.Current.Viewport_Cylinder_Region_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Sphere Region: Base Color", ref CFG.Current.Viewport_Sphere_Region_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Sphere Region: Highlight Color", ref CFG.Current.Viewport_Sphere_Region_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat("Sphere region: Transparency", ref CFG.Current.Viewport_Sphere_Region_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Point Region: Base Color", ref CFG.Current.Viewport_Point_Region_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Point Region: Highlight Color", ref CFG.Current.Viewport_Point_Region_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat("Point Region: Transparency", ref CFG.Current.Viewport_Point_Region_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Dummy Polygon: Base Color", ref CFG.Current.Viewport_Dummy_Polygon_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Dummy Polygon: Highlight Color", ref CFG.Current.Viewport_Dummy_Polygon_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat("Dummy Polygon: Transparency", ref CFG.Current.Viewport_Dummy_Polygon_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Bone Marker: Base Color", ref CFG.Current.Viewport_Bone_Marker_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Bone Marker: Highlight Color", ref CFG.Current.Viewport_Bone_Marker_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat("Bone Marker: Transparency", ref CFG.Current.Viewport_Bone_Marker_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Character Marker: Base Color", ref CFG.Current.Viewport_Character_Marker_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Character Marker: Highlight Color", ref CFG.Current.Viewport_Character_Marker_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat("Character Marker: Transparency", ref CFG.Current.Viewport_Character_Marker_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Object Marker: Base Color", ref CFG.Current.Viewport_Object_Marker_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Object Marker: Highlight Color", ref CFG.Current.Viewport_Object_Marker_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat("Object Marker: Transparency", ref CFG.Current.Viewport_Object_Marker_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Player Marker: Base Color", ref CFG.Current.Viewport_Player_Marker_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Player Marker: Highlight Color", ref CFG.Current.Viewport_Player_Marker_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat("Player Marker: Transparency", ref CFG.Current.Viewport_Player_Marker_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Other Marker: Base Color", ref CFG.Current.Viewport_Other_Marker_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Other Marker: Highlight Color", ref CFG.Current.Viewport_Other_Marker_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat("Other Marker: Transparency", ref CFG.Current.Viewport_Other_Marker_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Point Light: Base Color", ref CFG.Current.Viewport_Point_Light_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Point Light: Highlight Color", ref CFG.Current.Viewport_Point_Light_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat("Point Light: Transparency", ref CFG.Current.Viewport_Point_Light_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Spot Light: Base Color", ref CFG.Current.Viewport_Spot_Light_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Spot Light: Highlight Color", ref CFG.Current.Viewport_Splot_Light_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat("Spot Light: Transparency", ref CFG.Current.Viewport_Spot_Light_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Directional Light: Base Color", ref CFG.Current.Viewport_Directional_Light_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Directional Light: Highlight Color", ref CFG.Current.Viewport_Directional_Light_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat("Directional Light: Transparency", ref CFG.Current.Viewport_Directional_Light_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Auto Invade Marker: Base Color", ref CFG.Current.Viewport_Auto_Invade_Marker_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Auto Invade Marker: Highlight Color", ref CFG.Current.Viewport_Auto_Invade_Marker_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Level Connector Marker: Base Color", ref CFG.Current.Viewport_Level_Connector_Marker_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3("Level Connector Marker: Highlight Color", ref CFG.Current.Viewport_Level_Connector_Marker_Highlight_Color);
            }
        };
    }

    #endregion

    #region Viewport Filter Preset

    public static PreferenceItem ViewportFilterPresetBuilder()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = SectionCategory.FilterPresets,

            Title = "",
            Description = "",

            Draw = () =>
            {
                void ViewportFilterPresetEditor(int id, RenderFilterPreset preset)
                {
                    ImGui.PushID($"Preset {id}##PresetEdit");

                    UIHelper.SimpleHeader($"Preset {id}", "");

                    ImGui.TextUnformatted("Preset Name");
                    var nameInput = preset.Name;
                    DPI.ApplyInputWidth();
                    ImGui.InputText("##PresetName", ref nameInput, 32);
                    if (ImGui.IsItemDeactivatedAfterEdit())
                        preset.Name = nameInput;

                    ImGui.TextUnformatted("");

                    const int columns = 6;

                    ImGui.TextUnformatted("Filters to Set");
                    if (ImGui.BeginTable("RenderFilterTable", columns, ImGuiTableFlags.SizingFixedFit))
                    {
                        int columnIndex = 0;

                        foreach (RenderFilter e in Enum.GetValues(typeof(RenderFilter)))
                        {
                            if (columnIndex == 0)
                                ImGui.TableNextRow();

                            ImGui.TableSetColumnIndex(columnIndex);

                            bool ticked = preset.Filters.HasFlag(e);

                            // Use hidden label to avoid text affecting layout width
                            if (ImGui.Checkbox($"##{e}", ref ticked))
                            {
                                if (ticked)
                                    preset.Filters |= e;
                                else
                                    preset.Filters &= ~e;
                            }

                            // Draw label next to checkbox
                            ImGui.SameLine();
                            ImGui.TextUnformatted(e.ToString());

                            columnIndex = (columnIndex + 1) % columns;
                        }

                        ImGui.TextUnformatted("");

                        ImGui.EndTable();
                    }

                    ImGui.PopID();
                }

                ImGui.Text("Configure each of the six filter presets available.");

                ViewportFilterPresetEditor(1, CFG.Current.Viewport_Filter_Preset_1);
                ViewportFilterPresetEditor(2, CFG.Current.Viewport_Filter_Preset_2);
                ViewportFilterPresetEditor(3, CFG.Current.Viewport_Filter_Preset_3);
                ViewportFilterPresetEditor(4, CFG.Current.Viewport_Filter_Preset_4);
                ViewportFilterPresetEditor(5, CFG.Current.Viewport_Filter_Preset_5);
                ViewportFilterPresetEditor(6, CFG.Current.Viewport_Filter_Preset_6);
            }
        };
    }
    #endregion
}