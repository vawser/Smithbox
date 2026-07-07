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

            Title = "PREF_Viewport_Frame_Rate",
            Description = "PREF_Viewport_Frame_Rate_TT",

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

            Title = "PREF_Viewport_Enable_Rendering",
            Description = "PREF_Viewport_Enable_Rendering_TT",

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

            Title = "PREF_Viewport_Enable_Texturing",
            Description = "PREF_Viewport_Enable_Texturing_TT",

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

            Title = "PREF_Viewport_Enable_Culling",
            Description = "PREF_Viewport_Enable_Culling_TT",

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

            Title = "PREF_Viewport_Limit_Buffer_Indirect_Draw",
            Description = "PREF_Viewport_Limit_Buffer_Indirect_Draw_TT",

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

            Title = "PREF_Viewport_Limit_Buffer_Flver_Bone",
            Description = "PREF_Viewport_Limit_Buffer_Flver_Bone_TT",

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

            Title = "PREF_Viewport_Enable_Model_Masks",
            Description = "PREF_Viewport_Enable_Model_Masks_TT",

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

            Title = "PREF_Viewport_Enable_LOD_Facesets",
            Description = "PREF_Viewport_Enable_LOD_Facesets_TT",

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

            Title = "PREF_Viewport_Flat_Model_Brightness",
            Description = "PREF_Viewport_Flat_Model_Brightness_TT",

            Draw = () =>
            {
                DPI.ApplyInputWidth();
                ImGui.InputFloat("##inputValue", ref CFG.Current.Viewport_Flat_Model_Brightness);
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

            Title = "PREF_Viewport_Flat_Model_Saturation",
            Description = "PREF_Viewport_Flat_Model_Saturation_TT",

            Draw = () =>
            {
                DPI.ApplyInputWidth();
                ImGui.InputFloat("##inputValue", ref CFG.Current.Viewport_Flat_Model_Saturation);
            }
        };
    }

    #endregion

    #region Selection

    public static PreferenceItem Viewport_Enable_Selection_Outline()
    {
        return new PreferenceItem
        {
            OrderID = 0,
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = SectionCategory.Selection,

            Title = "PREF_Viewport_Enable_Selection_Outline",
            Description = "PREF_Viewport_Enable_Selection_Outline_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Viewport_Enable_Selection_Outline);
            }
        };
    }
    public static PreferenceItem Viewport_Enable_Selection_Tint()
    {
        return new PreferenceItem
        {
            OrderID = 1,
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = SectionCategory.Selection,

            Title = "PREF_Viewport_Enable_Selection_Tint",
            Description = "PREF_Viewport_Enable_Selection_Tint_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Viewport_Enable_Selection_Tint);
            }
        };
    }

    public static PreferenceItem Viewport_Enable_Selection_Dithering()
    {
        return new PreferenceItem
        {
            OrderID = 2,
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = SectionCategory.Selection,

            Title = "PREF_Viewport_Enable_Selection_Dithering",
            Description = "PREF_Viewport_Enable_Selection_Dithering_TT",

            Draw = () =>
            {
                ImGui.Checkbox("##inputValue", ref CFG.Current.Viewport_Enable_Selection_Dithering);
            }
        };
    }
    public static PreferenceItem Viewport_Selection_Dither_Opacity()
    {
        return new PreferenceItem
        {
            OrderID = 3,
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = SectionCategory.Selection,

            Title = "PREF_Viewport_Selection_Dither_Opacity",
            Description = "PREF_Viewport_Selection_Dither_Opacity_TT",

            Draw = () => {
                DPI.ApplyInputWidth();
                ImGui.DragFloat("##inputValue", ref CFG.Current.Viewport_Selection_Dither_Opacity);
            }
        };
    }

    public static PreferenceItem Viewport_Enable_Box_Selection()
    {
        return new PreferenceItem
        {
            OrderID = 4,
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = SectionCategory.Selection,

            Title = "PREF_Viewport_Enable_Box_Selection",
            Description = "PREF_Viewport_Enable_Box_Selection_TT",

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
            OrderID = 5,
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = SectionCategory.Selection,

            Title = "PREF_Viewport_Frame_Distance",
            Description = "PREF_Viewport_Frame_Distance_TT",

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
            OrderID = 6,
            Category = PreferenceCategory.Viewport,
            Spacer = true,

            Section = SectionCategory.Selection,

            Title = "PREF_Viewport_Frame_Offset",
            Description = "PREF_Viewport_Frame_Offset_TT",

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

            Title = "PREF_Viewport_Wireframe_Color_Variance",
            Description = "PREF_Viewport_Wireframe_Color_Variance_TT",

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

            Title = "SYS_Blank",
            Description = "SYS_Blank",

            Draw = () =>
            {
                GUI.SimpleHeader(
                    LOC.Get("PREF_Viewport_Header_General"),
                    LOC.Get("PREF_Viewport_Header_General_TT"));

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Bg_Color")}##Viewport_Bg_Color", 
                    ref CFG.Current.Viewport_Bg_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Selection_Outline_Color")}##Viewport_Selection_Outline_Color",
                    ref CFG.Current.Viewport_Selection_Outline_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Selection_Tint_Color")}##Viewport_Selection_Tint_Color",
                    ref CFG.Current.Viewport_Selection_Tint_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Untextured_Selection_Tint_Color")}##Viewport_Untextured_Selection_Tint_Color",
                    ref CFG.Current.Viewport_Untextured_Selection_Tint_Color);


                DPI.ApplyInputWidth();
                ImGui.DragFloat(
                    $"{LOC.Get("PREF_Viewport_Selection_Tint_Strength")}##Viewport_Selection_Tint_Strength",
                    ref CFG.Current.Viewport_Selection_Tint_Strength, 0.0f, 1.0f);

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

            Title = "SYS_Blank",
            Description = "SYS_Blank",

            Draw = () =>
            {
                GUI.SimpleHeader(
                    LOC.Get("PREF_Viewport_Header_Gizmo"),
                    LOC.Get("PREF_Viewport_Header_Gizmo_TT"));

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Gizmo_X_Base_Color")}##Viewport_Gizmo_X_Base_Color",
                    ref CFG.Current.Viewport_Gizmo_X_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Gizmo_X_Highlight_Color")}##Viewport_Gizmo_X_Highlight_Color",
                    ref CFG.Current.Viewport_Gizmo_X_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Gizmo_Y_Base_Color")}##Viewport_Gizmo_Y_Base_Color",
                    ref CFG.Current.Viewport_Gizmo_Y_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Gizmo_Y_Highlight_Color")}##Viewport_Gizmo_Y_Highlight_Color",
                    ref CFG.Current.Viewport_Gizmo_Y_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Gizmo_Z_Base_Color")}##Viewport_Gizmo_Z_Base_Color",
                    ref CFG.Current.Viewport_Gizmo_Z_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Gizmo_Z_Highlight_Color")}##Viewport_Gizmo_Z_Highlight_Color",
                    ref CFG.Current.Viewport_Gizmo_Z_Highlight_Color);
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

            Title = "SYS_Blank",
            Description = "SYS_Blank",

            Draw = () =>
            {
                GUI.SimpleHeader(
                    LOC.Get("PREF_Viewport_Header_Parts"),
                    LOC.Get("PREF_Viewport_Header_Parts_TT"));

                var curColor = Utils.GetDecimalColor(CFG.Current.Viewport_Collision_Color);
                DPI.ApplyInputWidth();
                if (ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Collision_Color")}##Viewport_Collision_Color",
                    ref curColor))
                {
                    CFG.Current.Viewport_Collision_Color = Utils.GetRgbColor(curColor);
                }

                curColor = Utils.GetDecimalColor(CFG.Current.Viewport_Connect_Collision_Color);
                DPI.ApplyInputWidth();
                if (ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Connect_Collision_Color")}##Viewport_Connect_Collision_Color",
                    ref curColor))
                {
                    CFG.Current.Viewport_Connect_Collision_Color = Utils.GetRgbColor(curColor);
                }

                curColor = Utils.GetDecimalColor(CFG.Current.Viewport_Navmesh_Color);
                DPI.ApplyInputWidth();
                if (ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Navmesh_Color")}##Viewport_Navmesh_Color",
                    ref curColor))
                {
                    CFG.Current.Viewport_Navmesh_Color = Utils.GetRgbColor(curColor);
                }

                curColor = Utils.GetDecimalColor(CFG.Current.Viewport_Navmesh_Gate_Color);
                DPI.ApplyInputWidth();
                if (ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Navmesh_Gate_Color")}##Viewport_Navmesh_Gate_Color",
                    ref curColor))
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

            Title = "SYS_Blank",
            Description = "SYS_Blank",

            Draw = () =>
            {
                GUI.SimpleHeader(
                    LOC.Get("PREF_Viewport_Header_Wireframes"),
                    LOC.Get("PREF_Viewport_Header_Wireframes_TT"));

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Box_Region_Base_Color")}##Viewport_Box_Region_Base_Color",
                    ref CFG.Current.Viewport_Box_Region_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Box_Region_Highlight_Color")}##Viewport_Box_Region_Highlight_Color",
                    ref CFG.Current.Viewport_Box_Region_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat(
                    $"{LOC.Get("PREF_Viewport_Box_Region_Alpha")}##Viewport_Box_Region_Alpha",
                    ref CFG.Current.Viewport_Box_Region_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Cylinder_Region_Base_Color")}##Viewport_Cylinder_Region_Base_Color",
                    ref CFG.Current.Viewport_Cylinder_Region_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Cylinder_Region_Highlight_Color")}##Viewport_Cylinder_Region_Highlight_Color",
                    ref CFG.Current.Viewport_Cylinder_Region_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat(
                    $"{LOC.Get("PREF_Viewport_Cylinder_Region_Alpha")}##Viewport_Cylinder_Region_Alpha",
                    ref CFG.Current.Viewport_Cylinder_Region_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Sphere_Region_Base_Color")}##Viewport_Sphere_Region_Base_Color",
                    ref CFG.Current.Viewport_Sphere_Region_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Sphere_Region_Highlight_Color")}##Viewport_Sphere_Region_Highlight_Color",
                    ref CFG.Current.Viewport_Sphere_Region_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat(
                    $"{LOC.Get("PREF_Viewport_Sphere_Region_Alpha")}##Viewport_Sphere_Region_Alpha",
                    ref CFG.Current.Viewport_Sphere_Region_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Point_Region_Base_Color")}##Viewport_Point_Region_Base_Color",
                    ref CFG.Current.Viewport_Point_Region_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Point_Region_Highlight_Color")}##Viewport_Point_Region_Highlight_Color",
                    ref CFG.Current.Viewport_Point_Region_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat(
                    $"{LOC.Get("PREF_Viewport_Point_Region_Alpha")}##Viewport_Point_Region_Alpha",
                    ref CFG.Current.Viewport_Point_Region_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Dummy_Polygon_Base_Color")}##Viewport_Dummy_Polygon_Base_Color",
                    ref CFG.Current.Viewport_Dummy_Polygon_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Dummy_Polygon_Highlight_Color")}##Viewport_Dummy_Polygon_Highlight_Color",
                    ref CFG.Current.Viewport_Dummy_Polygon_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat(
                    $"{LOC.Get("PREF_Viewport_Dummy_Polygon_Alpha")}##Viewport_Dummy_Polygon_Alpha",
                    ref CFG.Current.Viewport_Dummy_Polygon_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Bone_Marker_Base_Color")}##Viewport_Bone_Marker_Base_Color",
                    ref CFG.Current.Viewport_Bone_Marker_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Bone_Marker_Highlight_Color")}##Viewport_Bone_Marker_Highlight_Color",
                    ref CFG.Current.Viewport_Bone_Marker_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat(
                    $"{LOC.Get("PREF_Viewport_Bone_Marker_Alpha")}##Viewport_Bone_Marker_Alpha",
                    ref CFG.Current.Viewport_Bone_Marker_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Character_Marker_Base_Color")}##Viewport_Character_Marker_Base_Color",
                    ref CFG.Current.Viewport_Character_Marker_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Character_Marker_Highlight_Color")}##Viewport_Character_Marker_Highlight_Color",
                    ref CFG.Current.Viewport_Character_Marker_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat(
                    $"{LOC.Get("PREF_Viewport_Character_Marker_Alpha")}##Viewport_Character_Marker_Alpha",
                    ref CFG.Current.Viewport_Character_Marker_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Object_Marker_Base_Color")}##Viewport_Object_Marker_Base_Color",
                    ref CFG.Current.Viewport_Object_Marker_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Object_Marker_Highlight_Color")}##Viewport_Object_Marker_Highlight_Color",
                    ref CFG.Current.Viewport_Object_Marker_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat(
                    $"{LOC.Get("PREF_Viewport_Object_Marker_Alpha")}##Viewport_Object_Marker_Alpha",
                    ref CFG.Current.Viewport_Object_Marker_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Player_Marker_Base_Color")}##Viewport_Player_Marker_Base_Color",
                    ref CFG.Current.Viewport_Player_Marker_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Player_Marker_Highlight_Color")}##Viewport_Player_Marker_Highlight_Color",
                    ref CFG.Current.Viewport_Player_Marker_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat(
                    $"{LOC.Get("PREF_Viewport_Player_Marker_Alpha")}##Viewport_Player_Marker_Alpha",
                    ref CFG.Current.Viewport_Player_Marker_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Other_Marker_Base_Color")}##Viewport_Other_Marker_Base_Color",
                    ref CFG.Current.Viewport_Other_Marker_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Other_Marker_Highlight_Color")}##Viewport_Other_Marker_Highlight_Color",
                    ref CFG.Current.Viewport_Other_Marker_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat(
                    $"{LOC.Get("PREF_Viewport_Other_Marker_Alpha")}##Viewport_Other_Marker_Alpha",
                    ref CFG.Current.Viewport_Other_Marker_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Point_Light_Base_Color")}##Viewport_Point_Light_Base_Color",
                    ref CFG.Current.Viewport_Point_Light_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Point_Light_Highlight_Color")}##Viewport_Point_Light_Highlight_Color",
                    ref CFG.Current.Viewport_Point_Light_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat(
                    $"{LOC.Get("PREF_Viewport_Point_Light_Alpha")}##Viewport_Point_Light_Alpha",
                    ref CFG.Current.Viewport_Point_Light_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Spot_Light_Base_Color")}##Viewport_Spot_Light_Base_Color",
                    ref CFG.Current.Viewport_Spot_Light_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Spot_Light_Highlight_Color")}##Viewport_Splot_Light_Highlight_Color",
                    ref CFG.Current.Viewport_Spot_Light_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat(
                    $"{LOC.Get("PREF_Viewport_Spot_Light_Alpha")}##Viewport_Spot_Light_Alpha",
                    ref CFG.Current.Viewport_Spot_Light_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Directional_Light_Base_Color")}##Viewport_Directional_Light_Base_Color",
                    ref CFG.Current.Viewport_Directional_Light_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Directional_Light_Highlight_Color")}##Viewport_Directional_Light_Highlight_Color",
                    ref CFG.Current.Viewport_Directional_Light_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.DragFloat(
                    $"{LOC.Get("PREF_Viewport_Directional_Light_Alpha")}##Viewport_Directional_Light_Alpha",
                    ref CFG.Current.Viewport_Directional_Light_Alpha, 1.0f, 1.0f, 100.0f);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Auto_Invade_Marker_Base_Color")}##Viewport_Auto_Invade_Marker_Base_Color",
                    ref CFG.Current.Viewport_Auto_Invade_Marker_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Auto_Invade_Marker_Highlight_Color")}##Viewport_Auto_Invade_Marker_Highlight_Color",
                    ref CFG.Current.Viewport_Auto_Invade_Marker_Highlight_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Level_Connector_Marker_Base_Color")}##Viewport_Level_Connector_Marker_Base_Color",
                    ref CFG.Current.Viewport_Level_Connector_Marker_Base_Color);

                DPI.ApplyInputWidth();
                ImGui.ColorEdit3(
                    $"{LOC.Get("PREF_Viewport_Level_Connector_Marker_Highlight_Color")}##Viewport_Level_Connector_Marker_Highlight_Color",
                    ref CFG.Current.Viewport_Level_Connector_Marker_Highlight_Color);
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

            Title = "SYS_Blank",
            Description = "SYS_Blank",

            Draw = () =>
            {
                void ViewportFilterPresetEditor(int id, RenderFilterPreset preset)
                {
                    ImGui.PushID($"Preset {id}##PresetEdit");

                    GUI.SimpleHeader(
                        LOC.Get("PREF_FilterPreset_Header", id),
                        LOC.Get("PREF_FilterPreset_Header_TT"));

                    ImGui.TextUnformatted(LOC.Get("PREF_FilerPreset_Name"));
                    var nameInput = preset.Name;
                    DPI.ApplyInputWidth();
                    ImGui.InputText("##PresetName", ref nameInput, 32);
                    if (ImGui.IsItemDeactivatedAfterEdit())
                        preset.Name = nameInput;

                    ImGui.TextUnformatted("");

                    const int columns = 6;

                    ImGui.TextUnformatted(LOC.Get("PREF_FilterPreset_Filters_to_Set"));

                    if (ImGui.BeginTable("RenderFilterTable", columns, ImGuiTableFlags.SizingFixedFit))
                    {
                        int columnIndex = 0;

                        foreach (RenderFilter e in Enum.GetValues(typeof(RenderFilter)))
                        {
                            if (columnIndex == 0)
                                ImGui.TableNextRow();

                            ImGui.TableSetColumnIndex(columnIndex);

                            bool ticked = preset.Filters.HasFlag(e);

                            var displayName = LOC.Get(e.GetDisplayName());

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
                            ImGui.TextUnformatted(displayName);

                            columnIndex = (columnIndex + 1) % columns;
                        }

                        ImGui.TextUnformatted("");

                        ImGui.EndTable();
                    }

                    ImGui.PopID();
                }

                ImGui.Text(LOC.Get("PREF_FilterPreset_Configure_Filters"));

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