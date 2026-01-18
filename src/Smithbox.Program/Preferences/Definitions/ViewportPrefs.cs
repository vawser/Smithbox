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
    public static PreferenceItem System_Frame_Rate()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = "General",

            Title = "Frame Rate",
            Description = "Adjusts the frame rate of the viewport.",

            Draw = () =>
            {
                if (ImGui.SliderFloat("##inputValue", ref CFG.Current.System_Frame_Rate, 20.0f, 240.0f))
                {
                    CFG.Current.System_Frame_Rate = (float)Math.Round(CFG.Current.System_Frame_Rate);
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
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = "Rendering",

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
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = "Rendering",

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
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = "Rendering",

            Title = "Enable Frustum Culling",
            Description = "If enabled, entities outside of the camera frustum to be culled within the viewport.",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Viewport_Enable_Culling);
            }
        };
    }

    public static PreferenceItem Viewport_Limit_Renderables()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = "Rendering",

            Title = "Renderable Limit",
            Description = "This value constrains the number of renderable entities that are allowed. Exceeding this value will throw an exception.",

            Draw = () =>
            {
                if (ImGui.InputInt("##inputValue", ref CFG.Current.Viewport_Limit_Renderables, 0, 0))
                {
                    if (CFG.Current.Viewport_Limit_Renderables < CFG.Default.Viewport_Limit_Renderables)
                        CFG.Current.Viewport_Limit_Renderables = CFG.Default.Viewport_Limit_Renderables;
                }
            }
        };
    }

    public static PreferenceItem Viewport_Limit_Buffer_Indirect_Draw()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = "Rendering",

            Title = "Indirect Draw Buffer",
            Description = "This value constrains the size of the indirect draw buffer. Exceeding this value will throw an exception.",

            Draw = () =>
            {
                Utils.ImGui_InputUint("##inputValue", ref CFG.Current.Viewport_Limit_Buffer_Indirect_Draw);
            }
        };
    }

    public static PreferenceItem Viewport_Limit_Buffer_Flver_Bone()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = "Rendering",

            Title = "FLVER Bone Buffer",
            Description = "This value constrains the size of the FLVER bone buffer. Exceeding this value will throw an exception.",

            Draw = () =>
            {
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
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = "Model Rendering",

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
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = "Model Rendering",

            Title = "Enable LOD Facesets",
            Description = "If enabled, render all facesets for all FLVER meshes, including LOD ones.",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Viewport_Enable_LOD_Facesets);
            }
        };
    }

    public static PreferenceItem Viewport_DefaultRender_Brightness()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = "Model Rendering",

            Title = "Untextured Model Brightness",
            Description = "Change the brightness of untextured models.",

            Draw = () =>
            {
                ImGui.InputFloat("##inputValue", ref CFG.Current.Viewport_DefaultRender_Brightness);
            }
        };
    }
    public static PreferenceItem Viewport_DefaultRender_Saturation()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = "Model Rendering",

            Title = "Untextured Model Saturation",
            Description = "Change the saturation of untextured models.",

            Draw = () =>
            {
                ImGui.InputFloat("##inputValue", ref CFG.Current.Viewport_DefaultRender_Saturation);
            }
        };
    }

    #endregion

    #region Selection
    public static PreferenceItem Viewport_Enable_Selection_Outline()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = "Selection",

            Title = "Enable Selection Outline",
            Description = "If enabled, a selection outline will appear on selected objects.",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Viewport_Enable_Selection_Outline);
            }
        };
    }

    public static PreferenceItem Viewport_Enable_BoxSelection()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = "Selection",

            Title = "Enable Box Drag Selection",
            Description = "If enabled, you can click and drag the mouse to select multiple objects. (Ctrl: Subtract, Shift: Add)",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Viewport_Enable_BoxSelection);
            }
        };
    }

    public static PreferenceItem Viewport_BS_DistThresFactor()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = "Selection",

            Title = "Box Drag Distance Threshold",
            Description = "The distance threshold to use for the box drag. Lower means 'select objects closer to each other', Higher means 'select objects farther from each other'.",

            Draw = () =>
            {
                ImGui.SliderFloat("##inputValue", ref CFG.Current.Viewport_BS_DistThresFactor, 1.0f, 2.0f);
            }
        };
    }

    #endregion

    #region Coloring
    public static PreferenceItem GFX_Wireframe_Color_Variance()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = "Coloring",

            Title = "Wireframe Color Variance",
            Description = "The variance factor when adjusting the coloring of wireframe viewport objects.",

            Draw = () =>
            {
                ImGui.SliderFloat("##inputValue", ref CFG.Current.GFX_Wireframe_Color_Variance, 0.0f, 1.0f);
            }
        };
    }

    public static PreferenceItem GeneralColoring()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = "Coloring",

            Title = "",
            Description = "",

            Draw = () =>
            {
                UIHelper.SimpleHeader("General", "");
                ImGui.ColorEdit3("Viewport Background Color", ref CFG.Current.Viewport_Background_Color);
                ImGui.ColorEdit3("Selection Color", ref CFG.Current.Viewport_DefaultRender_SelectColor);
            }
        };
    }

    public static PreferenceItem GizmoColoring()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = "Coloring",

            Title = "",
            Description = "",

            Draw = () =>
            {
                UIHelper.SimpleHeader("Gizmo", "");

                ImGui.ColorEdit3("X-Axis: Base Color", ref CFG.Current.GFX_Gizmo_X_BaseColor);
                ImGui.ColorEdit3("X-Axis: Highlight Color", ref CFG.Current.GFX_Gizmo_X_HighlightColor);

                ImGui.ColorEdit3("Y-Axis: Base Color", ref CFG.Current.GFX_Gizmo_Y_BaseColor);
                ImGui.ColorEdit3("Y-Axis: Highlight Color", ref CFG.Current.GFX_Gizmo_Y_HighlightColor);

                ImGui.ColorEdit3("Z-Axis: Base Color", ref CFG.Current.GFX_Gizmo_Z_BaseColor);
                ImGui.ColorEdit3("Z-Axis: Highlight Color", ref CFG.Current.GFX_Gizmo_Z_HighlightColor);
            }
        };
    }

    public static PreferenceItem PartsColoring()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = "Coloring",

            Title = "",
            Description = "",

            Draw = () =>
            {
                UIHelper.SimpleHeader("Parts", "");

                var curColor = Utils.GetDecimalColor(CFG.Current.GFX_Renderable_Collision_Color);
                if (ImGui.ColorEdit3("Collision: Base Color", ref curColor))
                {
                    CFG.Current.GFX_Renderable_Collision_Color = Utils.GetRgbColor(curColor);
                }

                curColor = Utils.GetDecimalColor(CFG.Current.GFX_Renderable_ConnectCollision_Color);
                if (ImGui.ColorEdit3("Connect Collision: Base Color", ref curColor))
                {
                    CFG.Current.GFX_Renderable_ConnectCollision_Color = Utils.GetRgbColor(curColor);
                }

                curColor = Utils.GetDecimalColor(CFG.Current.GFX_Renderable_Navmesh_Color);
                if (ImGui.ColorEdit3("Navmesh: Base Color", ref curColor))
                {
                    CFG.Current.GFX_Renderable_Navmesh_Color = Utils.GetRgbColor(curColor);
                }

                curColor = Utils.GetDecimalColor(CFG.Current.GFX_Renderable_NavmeshGate_Color);
                if (ImGui.ColorEdit3("Navmesh Gate: Base Color", ref curColor))
                {
                    CFG.Current.GFX_Renderable_NavmeshGate_Color = Utils.GetRgbColor(curColor);
                }
            }
        };
    }

    public static PreferenceItem WireframeColoring()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Viewport,
            Spacer = true,
            InlineName = false,

            Section = "Coloring",

            Title = "",
            Description = "",

            Draw = () =>
            {
                UIHelper.SimpleHeader("Wireframes", "");

                ImGui.ColorEdit3("Box Region: Base Color", ref CFG.Current.GFX_Renderable_Box_BaseColor);
                ImGui.ColorEdit3("Box Region: Highlight Color", ref CFG.Current.GFX_Renderable_Box_HighlightColor);
                ImGui.DragFloat("Box Region: Transparency", ref CFG.Current.GFX_Renderable_Box_Alpha, 1.0f, 1.0f, 100.0f);

                ImGui.ColorEdit3("Cylinder Region: Base Color", ref CFG.Current.GFX_Renderable_Cylinder_BaseColor);
                ImGui.ColorEdit3("Cylinder Region: Highlight Color", ref CFG.Current.GFX_Renderable_Cylinder_HighlightColor);
                ImGui.DragFloat("Cylinder Region: Transparency", ref CFG.Current.GFX_Renderable_Cylinder_Alpha, 1.0f, 1.0f, 100.0f);

                ImGui.ColorEdit3("Sphere Region: Base Color", ref CFG.Current.GFX_Renderable_Sphere_BaseColor);
                ImGui.ColorEdit3("Sphere Region: Highlight Color", ref CFG.Current.GFX_Renderable_Sphere_HighlightColor);
                ImGui.DragFloat("Sphere region: Transparency", ref CFG.Current.GFX_Renderable_Sphere_Alpha, 1.0f, 1.0f, 100.0f);

                ImGui.ColorEdit3("Point Region: Base Color", ref CFG.Current.GFX_Renderable_Point_BaseColor);
                ImGui.ColorEdit3("Point Region: Highlight Color", ref CFG.Current.GFX_Renderable_Point_HighlightColor);
                ImGui.DragFloat("Point Region: Transparency", ref CFG.Current.GFX_Renderable_Point_Alpha, 1.0f, 1.0f, 100.0f);

                ImGui.ColorEdit3("Dummy Polygon: Base Color", ref CFG.Current.GFX_Renderable_DummyPoly_BaseColor);
                ImGui.ColorEdit3("Dummy Polygon: Highlight Color", ref CFG.Current.GFX_Renderable_DummyPoly_HighlightColor);
                ImGui.DragFloat("Dummy Polygon: Transparency", ref CFG.Current.GFX_Renderable_DummyPoly_Alpha, 1.0f, 1.0f, 100.0f);

                ImGui.ColorEdit3("Bone Marker: Base Color", ref CFG.Current.GFX_Renderable_BonePoint_BaseColor);
                ImGui.ColorEdit3("Bone Marker: Highlight Color", ref CFG.Current.GFX_Renderable_BonePoint_HighlightColor);
                ImGui.DragFloat("Bone Marker: Transparency", ref CFG.Current.GFX_Renderable_BonePoint_Alpha, 1.0f, 1.0f, 100.0f);

                ImGui.ColorEdit3("Character Marker: Base Color", ref CFG.Current.GFX_Renderable_ModelMarker_Chr_BaseColor);
                ImGui.ColorEdit3("Character Marker: Highlight Color", ref CFG.Current.GFX_Renderable_ModelMarker_Chr_HighlightColor);
                ImGui.DragFloat("Character Marker: Transparency", ref CFG.Current.GFX_Renderable_ModelMarker_Chr_Alpha, 1.0f, 1.0f, 100.0f);

                ImGui.ColorEdit3("Object Marker: Base Color", ref CFG.Current.GFX_Renderable_ModelMarker_Object_BaseColor);
                ImGui.ColorEdit3("Object Marker: Highlight Color", ref CFG.Current.GFX_Renderable_ModelMarker_Object_HighlightColor);
                ImGui.DragFloat("Object Marker: Transparency", ref CFG.Current.GFX_Renderable_ModelMarker_Object_Alpha, 1.0f, 1.0f, 100.0f);

                ImGui.ColorEdit3("Player Marker: Base Color", ref CFG.Current.GFX_Renderable_ModelMarker_Player_BaseColor);
                ImGui.ColorEdit3("Player Marker: Highlight Color", ref CFG.Current.GFX_Renderable_ModelMarker_Player_HighlightColor);
                ImGui.DragFloat("Player Marker: Transparency", ref CFG.Current.GFX_Renderable_ModelMarker_Player_Alpha, 1.0f, 1.0f, 100.0f);

                ImGui.ColorEdit3("Other Marker: Base Color", ref CFG.Current.GFX_Renderable_ModelMarker_Other_BaseColor);
                ImGui.ColorEdit3("Other Marker: Highlight Color", ref CFG.Current.GFX_Renderable_ModelMarker_Other_HighlightColor);
                ImGui.DragFloat("Other Marker: Transparency", ref CFG.Current.GFX_Renderable_ModelMarker_Other_Alpha, 1.0f, 1.0f, 100.0f);

                ImGui.ColorEdit3("Point Light: Base Color", ref CFG.Current.GFX_Renderable_PointLight_BaseColor);
                ImGui.ColorEdit3("Point Light: Highlight Color", ref CFG.Current.GFX_Renderable_PointLight_HighlightColor);
                ImGui.DragFloat("Point Light: Transparency", ref CFG.Current.GFX_Renderable_PointLight_Alpha, 1.0f, 1.0f, 100.0f);

                ImGui.ColorEdit3("Spot Light: Base Color", ref CFG.Current.GFX_Renderable_SpotLight_BaseColor);
                ImGui.ColorEdit3("Spot Light: Highlight Color", ref CFG.Current.GFX_Renderable_SpotLight_HighlightColor);
                ImGui.DragFloat("Spot Light: Transparency", ref CFG.Current.GFX_Renderable_SpotLight_Alpha, 1.0f, 1.0f, 100.0f);

                ImGui.ColorEdit3("Directional Light: Base Color", ref CFG.Current.GFX_Renderable_DirectionalLight_BaseColor);
                ImGui.ColorEdit3("Directional Light: Highlight Color", ref CFG.Current.GFX_Renderable_DirectionalLight_HighlightColor);
                ImGui.DragFloat("Directional Light: Transparency", ref CFG.Current.GFX_Renderable_DirectionalLight_Alpha, 1.0f, 1.0f, 100.0f);

                ImGui.ColorEdit3("Auto Invade Marker: Base Color", ref CFG.Current.GFX_Renderable_AutoInvadeSphere_BaseColor);
                ImGui.ColorEdit3("Auto Invade Marker: Highlight Color", ref CFG.Current.GFX_Renderable_AutoInvadeSphere_HighlightColor);

                ImGui.ColorEdit3("Level Connector Marker: Base Color", ref CFG.Current.GFX_Renderable_LevelConnectorSphere_BaseColor);
                ImGui.ColorEdit3("Level Connector Marker: Highlight Color", ref CFG.Current.GFX_Renderable_LevelConnectorSphere_HighlightColor);
            }
        };
    }

    #endregion

    #region Viewport Filter Preset

    public static PreferenceItem ViewportFilterPresetBuilder()
    {
        return new PreferenceItem
        {
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = "Viewport Filter Preset",

            Title = "",
            Description = "",

            Draw = () =>
            {
                void SettingsRenderFilterPresetEditor(int id, RenderFilterPreset preset)
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

                ImGui.Text("Configure each of the six display presets available.");

                SettingsRenderFilterPresetEditor(1, CFG.Current.SceneFilter_Preset_01);
                SettingsRenderFilterPresetEditor(2, CFG.Current.SceneFilter_Preset_02);
                SettingsRenderFilterPresetEditor(3, CFG.Current.SceneFilter_Preset_03);
                SettingsRenderFilterPresetEditor(4, CFG.Current.SceneFilter_Preset_04);
                SettingsRenderFilterPresetEditor(5, CFG.Current.SceneFilter_Preset_05);
                SettingsRenderFilterPresetEditor(6, CFG.Current.SceneFilter_Preset_06);
            }
        };
    }
    #endregion
}