using ImGuiNET;
using StudioCore.Scene;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Interface.Settings;

public class ViewportTab
{
    public ViewportTab() { }

    public void Display()
    {
        if (ImGui.BeginTabItem("视图 Viewport"))
        {
            // General
            if (ImGui.CollapsingHeader("通用 General", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.Checkbox("启用模型纹理 Enable model texturing", ref CFG.Current.Viewport_Enable_Texturing);
                ImguiUtils.ShowHoverTooltip("启用此选项将允许DSMS在视口内渲染模型的纹理(测试版) Enabling this option will allow DSMS to render the textures of models within the viewport.\n\nNote, this feature is in an alpha state.");

                ImGui.Checkbox("启用视域剔除 Enable frustum culling", ref CFG.Current.Viewport_Frustum_Culling);
                ImguiUtils.ShowHoverTooltip("启用此选项将导致相机平截头体之外的实体被剔除\nEnabling this option will cause entities outside of the camera frustum to be culled.");

                //ImGui.ColorEdit3("Viewport Background Color", ref CFG.Current.Viewport_BackgroundColor);
                //ImguiUtils.ShowHoverTooltip("Change the background color in the viewport. Requires a restart of Smithbox to take effect.");

            }

            if (ImGui.CollapsingHeader("渲染 Rendering", ImGuiTreeNodeFlags.DefaultOpen))
            {
                ImGui.InputFloat("亮度 Default Model Render: Brightness", ref CFG.Current.Viewport_DefaultRender_Brightness);
                ImguiUtils.ShowHoverTooltip("Change the brightness modifier for the Default Model Rendering shader.");
                ImGui.InputFloat("饱和 Default Model Render: Saturation", ref CFG.Current.Viewport_DefaultRender_Saturation);
                ImguiUtils.ShowHoverTooltip("Change the saturation modifier for the Default Model Rendering shader.");


                ImGui.Checkbox("启用轮廓选择 Enable selection outline", ref CFG.Current.Viewport_Enable_Selection_Outline);
                ImguiUtils.ShowHoverTooltip("Enabling this option will cause a selection outline to appear on selected objects.");

                ImGui.ColorEdit3("选中 Selection Color", ref CFG.Current.Viewport_DefaultRender_SelectColor);

                ImGui.Checkbox("启用敌人模型面罩 Enable enemy model masks", ref CFG.Current.Viewport_Enable_Model_Masks);
                ImguiUtils.ShowHoverTooltip("尝试根据NpcParam为敌人显示正确的模型掩码\nAttempt to display the correct model masks for enemies based on NpcParam.");

                ImGui.Checkbox("绘制LOD面部设置 Draw LOD facesets", ref CFG.Current.Viewport_Enable_LOD_Facesets);
                ImguiUtils.ShowHoverTooltip("渲染所有FLVER网格的所有面集，包括LOD网格\nRender all facesets for all FLVER meshes, including LOD ones.");

                if (ImGui.Button("重置 Reset##ResetRenderProperties"))
                {
                    CFG.Current.Viewport_DefaultRender_Brightness = CFG.Default.Viewport_DefaultRender_Brightness;
                    CFG.Current.Viewport_DefaultRender_Saturation = CFG.Default.Viewport_DefaultRender_Saturation;
                    CFG.Current.Viewport_Enable_Selection_Outline = CFG.Default.Viewport_Enable_Selection_Outline;
                    CFG.Current.Viewport_DefaultRender_SelectColor = CFG.Default.Viewport_DefaultRender_SelectColor;
                    CFG.Current.Viewport_Enable_Model_Masks = CFG.Default.Viewport_Enable_Model_Masks;
                    CFG.Current.Viewport_Enable_LOD_Facesets = CFG.Default.Viewport_Enable_LOD_Facesets;
                }
                ImguiUtils.ShowHoverTooltip("重置所有到初始状态 Resets all of the values within this section to their default values.");
            }

            if (ImGui.CollapsingHeader("相机 Camera"))
            {
                if (ImGui.Button("重置 Reset##ViewportCamera"))
                {
                    CFG.Current.Viewport_Camera_FOV = CFG.Default.Viewport_Camera_FOV;

                    CFG.Current.Viewport_RenderDistance_Max = CFG.Default.Viewport_RenderDistance_Max;

                    Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Slow = CFG.Default.Viewport_Camera_MoveSpeed_Slow;
                    CFG.Current.Viewport_Camera_MoveSpeed_Slow = Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Slow;
                    CFG.Current.Viewport_Camera_Sensitivity = CFG.Default.Viewport_Camera_Sensitivity;

                    Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Normal = CFG.Default.Viewport_Camera_MoveSpeed_Normal;
                    CFG.Current.Viewport_Camera_MoveSpeed_Normal = Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Normal;

                    Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Fast = CFG.Default.Viewport_Camera_MoveSpeed_Fast;
                    CFG.Current.Viewport_Camera_MoveSpeed_Fast = Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Fast;
                }
                ImguiUtils.ShowHoverTooltip("重置所有到初始状态 Resets all of the values within this section to their default values.");

                var cam_fov = CFG.Current.Viewport_Camera_FOV;

                if (ImGui.SliderFloat("相机 Camera FOV", ref cam_fov, 40.0f, 140.0f))
                    CFG.Current.Viewport_Camera_FOV = cam_fov;
                ImguiUtils.ShowHoverTooltip("Set the field of view used by the camera within DSMS.");

                var cam_sensitivity = CFG.Current.Viewport_Camera_Sensitivity;

                if (ImGui.SliderFloat("灵敏度 Camera sensitivity", ref cam_sensitivity, 0.0f, 0.1f))
                    CFG.Current.Viewport_Camera_Sensitivity = cam_sensitivity;
                ImguiUtils.ShowHoverTooltip("Mouse sensitivty for turning the camera.");

                var farClip = CFG.Current.Viewport_RenderDistance_Max;

                if (ImGui.SliderFloat("最大渲染距离 Map max render distance", ref farClip, 10.0f, 500000.0f))
                    CFG.Current.Viewport_RenderDistance_Max = farClip;
                ImguiUtils.ShowHoverTooltip("Set the maximum distance at which entities will be rendered within the DSMS viewport.");

                if (ImGui.SliderFloat("减速值 Map camera speed (slow)",
                        ref Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Slow, 0.1f, 9999.0f))
                    CFG.Current.Viewport_Camera_MoveSpeed_Slow = Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Slow;
                ImguiUtils.ShowHoverTooltip("Set the speed at which the camera will move when the Left or Right Shift key is pressed whilst moving.");

                if (ImGui.SliderFloat("速度值 Map camera speed (normal)",
                        ref Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Normal, 0.1f, 9999.0f))
                    CFG.Current.Viewport_Camera_MoveSpeed_Normal = Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Normal;
                ImguiUtils.ShowHoverTooltip("Set the speed at which the camera will move whilst moving normally.");

                if (ImGui.SliderFloat("加速值 Map camera speed (fast)",
                        ref Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Fast, 0.1f, 9999.0f))
                    CFG.Current.Viewport_Camera_MoveSpeed_Fast = Smithbox.EditorHandler.MapEditor.Viewport.WorldView.CameraMoveSpeed_Fast;
                ImguiUtils.ShowHoverTooltip("Set the speed at which the camera will move when the Left or Right Control key is pressed whilst moving.");
            }

            // Limits
            if (ImGui.CollapsingHeader("限制 Limits"))
            {
                if (ImGui.Button("重置 Reset##MapLimits"))
                {
                    CFG.Current.Viewport_Limit_Renderables = CFG.Default.Viewport_Limit_Renderables;
                    CFG.Current.Viewport_Limit_Buffer_Indirect_Draw = CFG.Default.Viewport_Limit_Buffer_Indirect_Draw;
                    CFG.Current.Viewport_Limit_Buffer_Flver_Bone = CFG.Default.Viewport_Limit_Buffer_Flver_Bone;
                }
                ImguiUtils.ShowHoverTooltip("Reset the values within this section to their default values.");

                ImGui.Text("请重启程序以应用修改\nPlease restart the program for changes to take effect.");

                ImGui.TextColored(new Vector4(1.0f, 0.0f, 0.0f, 1.0f),
                    @"Try smaller increments (+25%%) at first, as high values will cause issues.");

                if (ImGui.InputInt("可渲染 Renderables", ref CFG.Current.Viewport_Limit_Renderables, 0, 0))
                    if (CFG.Current.Viewport_Limit_Renderables < CFG.Default.Viewport_Limit_Renderables)
                        CFG.Current.Viewport_Limit_Renderables = CFG.Default.Viewport_Limit_Renderables;
                ImguiUtils.ShowHoverTooltip("This value constrains the number of renderable entities that are allowed. Exceeding this value will throw an exception.");

                Utils.ImGui_InputUint("间接绘制缓冲区 Indirect Draw buffer", ref CFG.Current.Viewport_Limit_Buffer_Indirect_Draw);
                ImguiUtils.ShowHoverTooltip("This value constrains the size of the indirect draw buffer. Exceeding this value will throw an exception.");

                Utils.ImGui_InputUint("FLVER骨骼缓冲区 FLVER Bone buffer", ref CFG.Current.Viewport_Limit_Buffer_Flver_Bone);
                ImguiUtils.ShowHoverTooltip("This value constrains the size of the FLVER bone buffer. Exceeding this value will throw an exception.");
            }

            // Wireframes
            if (ImGui.CollapsingHeader("线框图 Wireframes"))
            {
                if (ImGui.Button("重置 Reset"))
                {
                    // Proxies
                    CFG.Current.GFX_Renderable_Box_BaseColor = Utils.GetDecimalColor(Color.Blue);
                    CFG.Current.GFX_Renderable_Box_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

                    CFG.Current.GFX_Renderable_Cylinder_BaseColor = Utils.GetDecimalColor(Color.Blue);
                    CFG.Current.GFX_Renderable_Cylinder_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

                    CFG.Current.GFX_Renderable_Sphere_BaseColor = Utils.GetDecimalColor(Color.Blue);
                    CFG.Current.GFX_Renderable_Sphere_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

                    CFG.Current.GFX_Renderable_Point_BaseColor = Utils.GetDecimalColor(Color.Yellow);
                    CFG.Current.GFX_Renderable_Point_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

                    CFG.Current.GFX_Renderable_DummyPoly_BaseColor = Utils.GetDecimalColor(Color.Yellow);
                    CFG.Current.GFX_Renderable_DummyPoly_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

                    CFG.Current.GFX_Renderable_BonePoint_BaseColor = Utils.GetDecimalColor(Color.Blue);
                    CFG.Current.GFX_Renderable_BonePoint_HighlightColor = Utils.GetDecimalColor(Color.DarkViolet);

                    CFG.Current.GFX_Renderable_ModelMarker_Chr_BaseColor = Utils.GetDecimalColor(Color.Firebrick);
                    CFG.Current.GFX_Renderable_ModelMarker_Chr_HighlightColor = Utils.GetDecimalColor(Color.Tomato);

                    CFG.Current.GFX_Renderable_ModelMarker_Object_BaseColor = Utils.GetDecimalColor(Color.MediumVioletRed);
                    CFG.Current.GFX_Renderable_ModelMarker_Object_HighlightColor = Utils.GetDecimalColor(Color.DeepPink);

                    CFG.Current.GFX_Renderable_ModelMarker_Player_BaseColor = Utils.GetDecimalColor(Color.DarkOliveGreen);
                    CFG.Current.GFX_Renderable_ModelMarker_Player_HighlightColor = Utils.GetDecimalColor(Color.OliveDrab);

                    CFG.Current.GFX_Renderable_ModelMarker_Other_BaseColor = Utils.GetDecimalColor(Color.Wheat);
                    CFG.Current.GFX_Renderable_ModelMarker_Other_HighlightColor = Utils.GetDecimalColor(Color.AntiqueWhite);

                    CFG.Current.GFX_Renderable_PointLight_BaseColor = Utils.GetDecimalColor(Color.YellowGreen);
                    CFG.Current.GFX_Renderable_PointLight_HighlightColor = Utils.GetDecimalColor(Color.Yellow);

                    CFG.Current.GFX_Renderable_SpotLight_BaseColor = Utils.GetDecimalColor(Color.Goldenrod);
                    CFG.Current.GFX_Renderable_SpotLight_HighlightColor = Utils.GetDecimalColor(Color.Violet);

                    CFG.Current.GFX_Renderable_DirectionalLight_BaseColor = Utils.GetDecimalColor(Color.Cyan);
                    CFG.Current.GFX_Renderable_DirectionalLight_HighlightColor = Utils.GetDecimalColor(Color.AliceBlue);

                    CFG.Current.GFX_Gizmo_X_BaseColor = new Vector3(0.952f, 0.211f, 0.325f);
                    CFG.Current.GFX_Gizmo_X_HighlightColor = new Vector3(1.0f, 0.4f, 0.513f);

                    CFG.Current.GFX_Gizmo_Y_BaseColor = new Vector3(0.525f, 0.784f, 0.082f);
                    CFG.Current.GFX_Gizmo_Y_HighlightColor = new Vector3(0.713f, 0.972f, 0.270f);

                    CFG.Current.GFX_Gizmo_Z_BaseColor = new Vector3(0.219f, 0.564f, 0.929f);
                    CFG.Current.GFX_Gizmo_Z_HighlightColor = new Vector3(0.407f, 0.690f, 1.0f);

                    CFG.Current.GFX_Wireframe_Color_Variance = CFG.Default.GFX_Wireframe_Color_Variance;
                }
                ImguiUtils.ShowHoverTooltip("Resets all of the values within this section to their default values.");

                ImGui.SliderFloat("线框变化 Wireframe color variance", ref CFG.Current.GFX_Wireframe_Color_Variance, 0.0f, 1.0f);

                // 代理
                ImGui.ColorEdit3("盒子区域 - 基础 Box region - base color", ref CFG.Current.GFX_Renderable_Box_BaseColor);
                ImGui.ColorEdit3("盒子区域 - 高亮 Box region - highlight color", ref CFG.Current.GFX_Renderable_Box_HighlightColor);

                ImGui.ColorEdit3("圆柱区域 - 基础 Cylinder region - base color", ref CFG.Current.GFX_Renderable_Cylinder_BaseColor);
                ImGui.ColorEdit3("圆柱区域 - 高亮 Cylinder region - highlight color", ref CFG.Current.GFX_Renderable_Cylinder_HighlightColor);

                ImGui.ColorEdit3("球体区域 - 基础 Sphere region - base color", ref CFG.Current.GFX_Renderable_Sphere_BaseColor);
                ImGui.ColorEdit3("球体区域 - 高亮 Sphere region - highlight color", ref CFG.Current.GFX_Renderable_Sphere_HighlightColor);

                ImGui.ColorEdit3("点区域 - 基础 Point region - base color", ref CFG.Current.GFX_Renderable_Point_BaseColor);
                ImGui.ColorEdit3("点区域 - 高亮 Point region - highlight color", ref CFG.Current.GFX_Renderable_Point_HighlightColor);

                ImGui.ColorEdit3("虚拟多边形 - 基础 Dummy poly - base color", ref CFG.Current.GFX_Renderable_DummyPoly_BaseColor);
                ImGui.ColorEdit3("虚拟多边形 - 高亮 Dummy poly - highlight color", ref CFG.Current.GFX_Renderable_DummyPoly_HighlightColor);

                ImGui.ColorEdit3("骨骼点 - 基础 Bone point - base color", ref CFG.Current.GFX_Renderable_BonePoint_BaseColor);
                ImGui.ColorEdit3("骨骼点 - 高亮 Bone point - highlight color", ref CFG.Current.GFX_Renderable_BonePoint_HighlightColor);

                ImGui.ColorEdit3("角色标记 - 基础 Chr marker - base color", ref CFG.Current.GFX_Renderable_ModelMarker_Chr_BaseColor);
                ImGui.ColorEdit3("角色标记 - 高亮 Chr marker - highlight color", ref CFG.Current.GFX_Renderable_ModelMarker_Chr_HighlightColor);

                ImGui.ColorEdit3("对象标记 - 基础 Object marker - base color", ref CFG.Current.GFX_Renderable_ModelMarker_Object_BaseColor);
                ImGui.ColorEdit3("对象标记 - 高亮 Object marker - highlight color", ref CFG.Current.GFX_Renderable_ModelMarker_Object_HighlightColor);

                ImGui.ColorEdit3("玩家标记 - 基础 Player marker - base color", ref CFG.Current.GFX_Renderable_ModelMarker_Player_BaseColor);
                ImGui.ColorEdit3("玩家标记 - 高亮 Player marker - highlight color", ref CFG.Current.GFX_Renderable_ModelMarker_Player_HighlightColor);

                ImGui.ColorEdit3("其他标记 - 基础 Other marker - base color", ref CFG.Current.GFX_Renderable_ModelMarker_Other_BaseColor);
                ImGui.ColorEdit3("其他标记 - 高亮 Other marker - highlight color", ref CFG.Current.GFX_Renderable_ModelMarker_Other_HighlightColor);

                ImGui.ColorEdit3("点光源 - 基础 Point light - base color", ref CFG.Current.GFX_Renderable_PointLight_BaseColor);
                ImGui.ColorEdit3("点光源 - 高亮 Point light - highlight color", ref CFG.Current.GFX_Renderable_PointLight_HighlightColor);

                ImGui.ColorEdit3("聚光灯 - 基础 Spot light - base color", ref CFG.Current.GFX_Renderable_SpotLight_BaseColor);
                ImGui.ColorEdit3("聚光灯 - 高亮 Spot light - highlight color", ref CFG.Current.GFX_Renderable_SpotLight_HighlightColor);

                ImGui.ColorEdit3("方向光 - 基础 Directional light - base color", ref CFG.Current.GFX_Renderable_DirectionalLight_BaseColor);
                ImGui.ColorEdit3("方向光 - 高亮 Directional light - highlight color", ref CFG.Current.GFX_Renderable_DirectionalLight_HighlightColor);

                ImGui.ColorEdit3("操作轴X - 基础 Gizmo - X Axis - base color", ref CFG.Current.GFX_Gizmo_X_BaseColor);
                ImGui.ColorEdit3("操作轴X - 高亮 Gizmo - X Axis - highlight color", ref CFG.Current.GFX_Gizmo_X_HighlightColor);

                ImGui.ColorEdit3("操作轴Y - 基础 Gizmo - Y Axis - base color", ref CFG.Current.GFX_Gizmo_Y_BaseColor);
                ImGui.ColorEdit3("操作轴Y - 高亮 Gizmo - Y Axis - highlight color", ref CFG.Current.GFX_Gizmo_Y_HighlightColor);

                ImGui.ColorEdit3("操作轴Z - 基础 Gizmo - Z Axis - base color", ref CFG.Current.GFX_Gizmo_Z_BaseColor);
                ImGui.ColorEdit3("操作轴Z - 高亮 Gizmo - Z Axis - highlight color", ref CFG.Current.GFX_Gizmo_Z_HighlightColor);
            }

            // Map Object Display Presets
            if (ImGui.CollapsingHeader("预处理 Display Presets"))
            {
                ImGui.Text("配置六个可用的显示预设\n Configure each of the six display presets available.");

                if (ImGui.Button("重置 Reset##DisplayPresets"))
                {
                    CFG.Current.SceneFilter_Preset_01.Name = CFG.Default.SceneFilter_Preset_01.Name;
                    CFG.Current.SceneFilter_Preset_01.Filters = CFG.Default.SceneFilter_Preset_01.Filters;
                    CFG.Current.SceneFilter_Preset_02.Name = CFG.Default.SceneFilter_Preset_02.Name;
                    CFG.Current.SceneFilter_Preset_02.Filters = CFG.Default.SceneFilter_Preset_02.Filters;
                    CFG.Current.SceneFilter_Preset_03.Name = CFG.Default.SceneFilter_Preset_03.Name;
                    CFG.Current.SceneFilter_Preset_03.Filters = CFG.Default.SceneFilter_Preset_03.Filters;
                    CFG.Current.SceneFilter_Preset_04.Name = CFG.Default.SceneFilter_Preset_04.Name;
                    CFG.Current.SceneFilter_Preset_04.Filters = CFG.Default.SceneFilter_Preset_04.Filters;
                    CFG.Current.SceneFilter_Preset_05.Name = CFG.Default.SceneFilter_Preset_05.Name;
                    CFG.Current.SceneFilter_Preset_05.Filters = CFG.Default.SceneFilter_Preset_05.Filters;
                    CFG.Current.SceneFilter_Preset_06.Name = CFG.Default.SceneFilter_Preset_06.Name;
                    CFG.Current.SceneFilter_Preset_06.Filters = CFG.Default.SceneFilter_Preset_06.Filters;
                }
                ImguiUtils.ShowHoverTooltip("Reset the values within this section to their default values.");

                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_01);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_02);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_03);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_04);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_05);
                SettingsRenderFilterPresetEditor(CFG.Current.SceneFilter_Preset_06);
            }

            ImGui.Unindent();
            ImGui.EndTabItem();
        }
    }

    private void SettingsRenderFilterPresetEditor(CFG.RenderFilterPreset preset)
    {
        ImGui.PushID($"{preset.Name}##PresetEdit");
        if (ImGui.CollapsingHeader($"{preset.Name}##Header"))
        {
            ImGui.Indent();
            var nameInput = preset.Name;
            ImGui.InputText("Preset Name", ref nameInput, 32);
            if (ImGui.IsItemDeactivatedAfterEdit())
                preset.Name = nameInput;

            foreach (RenderFilter e in Enum.GetValues(typeof(RenderFilter)))
            {
                var ticked = false;
                if (preset.Filters.HasFlag(e))
                    ticked = true;

                if (ImGui.Checkbox(e.ToString(), ref ticked))
                    if (ticked)
                        preset.Filters |= e;
                    else
                        preset.Filters &= ~e;
            }

            ImGui.Unindent();
        }

        ImGui.PopID();
    }
}
