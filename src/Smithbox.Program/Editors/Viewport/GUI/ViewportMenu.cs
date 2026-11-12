using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.FxEditor;
using StudioCore.Editors.GparamEditor;
using StudioCore.Editors.MapEditor;
using StudioCore.Editors.ModelEditor;
using StudioCore.Editors.TimeActEditor;
using StudioCore.Keybinds;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Numerics;
using Veldrid;

namespace StudioCore.Editors.Viewport;

public class ViewportMenu
{
    public IUniverse Owner;
    public VulkanViewport Parent;

    public ViewportMenu(VulkanViewport parent)
    {
        Parent = parent;
        Owner = parent.Owner;
    }

    public void Draw()
    {
        ImGui.BeginMenuBar();

        SettingsMenu();
        OverlayMenu();
        FilterMenu();
        GizmoMenu();

        ImGui.EndMenuBar();
    }

    public void OverlayMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Header_Overlay")}##overlayMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Toggle_Controls")}##controlsToggle"))
            {
                CFG.Current.Viewport_DisplayControls = !CFG.Current.Viewport_DisplayControls;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.Viewport_DisplayControls);
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Toggle_Controls_TT"));

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Toggle_Profiling")}##profilingToggle"))
            {
                CFG.Current.Viewport_Display_Profiling = !CFG.Current.Viewport_Display_Profiling;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.Viewport_Display_Profiling);
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Toggle_Profiling_TT"));

            if (Owner is MapUniverse mapUniverse)
            {
                if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Toggle_Active_Map")}##activeMapToggle"))
                {
                    CFG.Current.Viewport_DisplayActiveMap = !CFG.Current.Viewport_DisplayActiveMap;
                    Parent.DelayPicking();
                }
                GUI.ShowActiveStatus(CFG.Current.Viewport_DisplayActiveMap);
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Toggle_Active_Map_TT"));

                if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Toggle_Translation_Increment")}##translationIncrementToggle"))
                {
                    CFG.Current.Viewport_DisplayTranslationIncrement = !CFG.Current.Viewport_DisplayTranslationIncrement;
                    Parent.DelayPicking();
                }
                GUI.ShowActiveStatus(CFG.Current.Viewport_DisplayTranslationIncrement);
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Toggle_Translation_Increment_TT"));

                if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Toggle_Rotation_Increment")}##rotationIncrementToggle"))
                {
                    CFG.Current.Viewport_DisplayRotationIncrement = !CFG.Current.Viewport_DisplayRotationIncrement;
                    Parent.DelayPicking();
                }
                GUI.ShowActiveStatus(CFG.Current.Viewport_DisplayRotationIncrement);
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Toggle_Rotation_Increment_TT"));

                if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Toggle_Viewport_Tooltip")}##viewportTooltipToggle"))
                {
                    CFG.Current.QuickView_DisplayTooltip = !CFG.Current.QuickView_DisplayTooltip;
                    Parent.DelayPicking();
                }
                GUI.ShowActiveStatus(CFG.Current.QuickView_DisplayTooltip);
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Toggle_Viewport_Tooltip_TT"));

                if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Toggle_Compass")}##compassToggle"))
                {
                    CFG.Current.DisplayCompass = !CFG.Current.DisplayCompass;
                    Parent.DelayPicking();
                }
                GUI.ShowActiveStatus(CFG.Current.DisplayCompass);
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Toggle_Compass_TT"));
            }

            ImGui.EndMenu();
        }
    }

    /// <summary>
    ///     Shows the owning editor's tool window with its Rendering category open: at its first section, or at the
    ///     Camera section when <paramref name="camera" /> is set
    /// </summary>
    private void OpenRenderingTool(bool camera = false)
    {
        ViewportRenderingTool tool = null;

        switch (Owner)
        {
            case MapUniverse map:
                CFG.Current.Interface_MapEditor_ToolWindow = true;
                CFG.Current.Interface_MapEditor_Tool_ViewportRendering = true;
                tool = map.View.RenderingTool;
                break;
            case ModelUniverse model:
                CFG.Current.Interface_ModelEditor_ToolWindow = true;
                CFG.Current.Interface_ModelEditor_Tool_ViewportRendering = true;
                tool = model.View.RenderingTool;
                break;
            case TimeActUniverse timeAct:
                CFG.Current.Interface_TimeActEditor_ToolWindow = true;
                CFG.Current.Interface_TimeActEditor_Tool_ViewportRendering = true;
                tool = timeAct.View.RenderingTool;
                break;
            case GparamUniverse gparam:
                CFG.Current.Interface_GparamEditor_ToolWindow = true;
                CFG.Current.Interface_GparamEditor_Tool_ViewportRendering = true;
                tool = gparam.View.RenderingTool;
                break;
            case FxUniverse fx:
                CFG.Current.Interface_FxEditor_ToolWindow = true;
                CFG.Current.Interface_FxEditor_Tool_ViewportRendering = true;
                tool = fx.Editor.RenderingTool;
                break;
        }

        if (tool == null)
            return;

        if (camera)
            tool.CameraFocusRequested = true;
        else
            tool.FocusRequested = true;
    }

    public void FilterMenu()
    {
        // Map Editor
        if (Owner is MapUniverse mapUniverse)
        {
            mapUniverse.View.Editor.FilterMenu();
        }

        // Model Editor
        if (Owner is ModelUniverse modelUniverse)
        {
            modelUniverse.View.ViewportFilters.Display();
        }

        // Time Act Editor
        if (Owner is TimeActUniverse timeActUniverse)
        {
            timeActUniverse.View.ViewportFilters.Display();
        }
    }

    public void GizmoMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Header_Gizmos")}##gizmosMenuHeader"))
        {
            GizmoState.OnMenu(Parent);

            ImGui.EndMenu();
        }
    }


    public void SettingsMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Header_Settings")}##settingsMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Enable_Rendering")}##renderToggle", CFG.Current.Viewport_Enable_Rendering))
            {
                CFG.Current.Viewport_Enable_Rendering = !CFG.Current.Viewport_Enable_Rendering;
                Parent.DelayPicking();
            }
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Enable_Rendering_TT"));

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Enable_Texturing")}##textureToggle", CFG.Current.Viewport_Enable_Texturing))
            {
                CFG.Current.Viewport_Enable_Texturing = !CFG.Current.Viewport_Enable_Texturing;
                Parent.DelayPicking();
            }
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Enable_Texturing_TT"));

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Enable_Culling")}##cullingToggle", CFG.Current.Viewport_Enable_Culling))
            {
                CFG.Current.Viewport_Enable_Culling = !CFG.Current.Viewport_Enable_Culling;
                Parent.DelayPicking();
            }
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Enable_Culling_TT"));

            ImGui.Separator();

            RenderModeMenu();

            ImGui.Separator();

            // The same switches as the Rendering tool's tabs; the pipeline reads them every frame. GI and map
            // lighting are only offered where the viewport has them (AC6, ER, NR). Editor mode overrides them all,
            // so they are shown but can't be changed there.
            var pipeline = Parent.ViewPipeline;

            // Disabled items show no tooltip, so say why they're greyed out
            if (!RenderMode.IsGame)
            {
                ImGui.TextDisabled(LOC.Get("VIEWPORT_Menubar_Setting_Game_Mode_Only"));
            }

            ImGui.BeginDisabled(!RenderMode.IsGame);

            if (pipeline?.UseIrradianceVolumes == true)
            {
                if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Enable_Global_Illumination")}##giToggle", CFG.Current.Viewport_GI_Enable))
                {
                    CFG.Current.Viewport_GI_Enable = !CFG.Current.Viewport_GI_Enable;
                }
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Enable_Global_Illumination_TT"));
            }

            if (pipeline?.UseMapLighting == true)
            {
                if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Enable_Lighting")}##lightingToggle", CFG.Current.Viewport_Map_Lighting))
                {
                    CFG.Current.Viewport_Map_Lighting = !CFG.Current.Viewport_Map_Lighting;
                }
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Enable_Lighting_TT"));
            }

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Enable_Shadows")}##shadowsToggle", CFG.Current.Viewport_Shadows))
            {
                CFG.Current.Viewport_Shadows = !CFG.Current.Viewport_Shadows;
            }
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Enable_Shadows_TT"));

            ImGui.EndDisabled();

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Open_Rendering_Tool")}##openRenderingTool"))
            {
                OpenRenderingTool();
            }
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Open_Rendering_Tool_TT"));

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Open_Camera_Tool")}##openCameraTool"))
            {
                OpenRenderingTool(camera: true);
            }
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Open_Camera_Tool_TT"));

            if (Owner is MapUniverse mapUniverse)
            {
                if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Enable_Model_Masks")}##modelMaskToggle", CFG.Current.Viewport_Enable_Model_Masks))
                {
                    CFG.Current.Viewport_Enable_Model_Masks = !CFG.Current.Viewport_Enable_Model_Masks;
                    Parent.DelayPicking();
                }
                GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Enable_Model_Masks_TT"));

                ImGui.Separator();

                MapModelLoadMenu();
                MapTextureLoadMenu();

                ImGui.Separator();

                if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Header_Quick_View")}##viewportTooltipMenuHeader"))
                {
                    mapUniverse.View.AutomaticPreviewTool.HandleQuickViewProperties();

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Header_Placement_Orb")}##placementOrbMenuHeader"))
                {
                    ImGui.DragFloat(
                        $"{LOC.Get("VIEWPORT_Menubar_Setting_Orb_Distance")}##orbDistance", 
                        ref CFG.Current.PlacementOrb_Distance, 0.1f, 1f, 100f);

                    GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Orb_Distance_TT"));

                    ImGui.EndMenu();
                }

                if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Header_Compass")}##compassMenuHeader"))
                {
                    ImGui.DragInt(
                        $"{LOC.Get("VIEWPORT_Menubar_Setting_Compass_Size")}##compassSize",
                        ref CFG.Current.CompassSize, 1, 16, 512);

                    GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Compass_Size_TT"));

                    ImGui.DragFloat(
                        $"{LOC.Get("VIEWPORT_Menubar_Setting_Compass_North_Offset")}##compassNorthOffset",
                        ref CFG.Current.CompassNorthOffset, 0.1f, -360.0f, 360f);

                    GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Compass_North_Offset_TT"));

                    ImGui.EndMenu();
                }
            }

            if (Owner is ModelUniverse modelUniverse)
            {
                ModelSettingsMenu(modelUniverse.View.ViewportWindow.UpdateDisplayNodes);
            }

            if (Owner is TimeActUniverse timeActUniverse)
            {
                ModelSettingsMenu(timeActUniverse.View.ViewportWindow.UpdateDisplayNodes);
            }

            ImGui.EndMenu();
        }
    }

    /// <summary>
    ///     The render mode (Editor's fullbright or Game's full lighting) and what the textured image shows, each a
    ///     pair of items of which one is checked
    /// </summary>
    private void RenderModeMenu()
    {
        var mode = RenderMode.Current;

        if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Render_Mode_Editor")}##renderModeEditor", "", mode == ViewportRenderMode.Editor))
        {
            CFG.Current.Viewport_Render_Mode = (int)ViewportRenderMode.Editor;
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Render_Mode_Editor_TT"));

        if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Render_Mode_Game")}##renderModeGame", "", mode == ViewportRenderMode.Game))
        {
            CFG.Current.Viewport_Render_Mode = (int)ViewportRenderMode.Game;
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Render_Mode_Game_TT"));

        ImGui.Separator();

        var texturing = RenderMode.Texturing;

        if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Texturing_Diffuse")}##texturingDiffuse", "", texturing == ViewportTexturingMode.Diffuse))
        {
            CFG.Current.Viewport_Texturing_Mode = (int)ViewportTexturingMode.Diffuse;
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Texturing_Diffuse_TT"));

        if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Texturing_Normal")}##texturingNormal", "", texturing == ViewportTexturingMode.Normal))
        {
            CFG.Current.Viewport_Texturing_Mode = (int)ViewportTexturingMode.Normal;
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Texturing_Normal_TT"));

        ImGui.Separator();

        WireframeMenu();
    }

    /// <summary>
    ///     The wireframe view: lines along the triangles of FLVER models, over their surfaces or alone. Drawn by the
    ///     shaders, so the settings apply at once; see MeshRenderables.cWireframePipelines.
    /// </summary>
    private void WireframeMenu()
    {
        if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Wireframe")}##wireframeToggle", "", CFG.Current.Viewport_Wireframe_Enable))
        {
            CFG.Current.Viewport_Wireframe_Enable = !CFG.Current.Viewport_Wireframe_Enable;
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Wireframe_TT"));

        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Setting_Wireframe_Settings")}##wireframeSettings"))
        {
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_Setting_Wireframe_Only")}##wireframeOnly", "", CFG.Current.Viewport_Wireframe_Only))
            {
                CFG.Current.Viewport_Wireframe_Only = !CFG.Current.Viewport_Wireframe_Only;
            }
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Wireframe_Only_TT"));

            ImGui.ColorEdit3($"{LOC.Get("VIEWPORT_Menubar_Setting_Wireframe_Color")}##wireframeColor",
                ref CFG.Current.Viewport_Wireframe_Line_Color);
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Wireframe_Color_TT"));

            ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Menubar_Setting_Wireframe_Opacity")}##wireframeOpacity",
                ref CFG.Current.Viewport_Wireframe_Line_Opacity, 0.0f, 1.0f);
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Wireframe_Opacity_TT"));

            ImGui.SliderFloat($"{LOC.Get("VIEWPORT_Menubar_Setting_Wireframe_Width")}##wireframeWidth",
                ref CFG.Current.Viewport_Wireframe_Line_Width, 0.5f, 8.0f);
            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Wireframe_Width_TT"));

            ImGui.EndMenu();
        }
    }

    private void ModelSettingsMenu(Action updateDisplayNodes)
    {
        ImGui.Separator();

        ModelModelLoadMenu();
        ModelTextureLoadMenu();

        ImGui.Separator();

        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Header_Display_Nodes")}##displayNodeMenuHeader"))
        {
            ImGui.DragFloat(
                $"{LOC.Get("VIEWPORT_Menubar_Setting_Dummy_Size")}##dummyMeshSize",
                ref CFG.Current.DummyMeshSize, 0.1f, 0.0001f, 1f);

            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Dummy_Size_TT"));

            if(ImGui.IsItemDeactivatedAfterEdit())
            {
                RenderableHelper.UpdateProxySizes();
                updateDisplayNodes();
            }

            ImGui.DragFloat(
                $"{LOC.Get("VIEWPORT_Menubar_Setting_Node_Size")}##nodeMeshSize",
                ref CFG.Current.NodeMeshSize, 0.1f, 0.0001f, 1f);

            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Node_Size_TT"));

            if (ImGui.IsItemDeactivatedAfterEdit())
            {
                RenderableHelper.UpdateProxySizes();
                updateDisplayNodes();
            }

            ImGui.DragFloat(
                $"{LOC.Get("VIEWPORT_Menubar_Setting_Label_Scale")}##labelScale",
                ref CFG.Current.ModelEditor_LabelScale, 0.05f, 0.25f, 5f);

            GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Setting_Label_Scale_TT"));

            ImGui.EndMenu();
        }
    }

    public void MapModelLoadMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Model_Load_Header")}##modelLoadMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_ML_Map_Pieces")}##mapPieceToggle"))
            {
                CFG.Current.MapEditor_ModelLoad_MapPieces = !CFG.Current.MapEditor_ModelLoad_MapPieces;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.MapEditor_ModelLoad_MapPieces);

            var name = LOC.Get("VIEWPORT_Menubar_ML_Objects");

            if (Owner is MapUniverse mapUniverse)
            {
                if (mapUniverse.Project.Descriptor.ProjectType is ProjectType.ER 
                    or ProjectType.AC6 
                    or ProjectType.NR
                    or ProjectType.DB_NT
                    or ProjectType.DB)
                {
                    name = LOC.Get("VIEWPORT_Menubar_ML_Assets");
                }
            }

            if (ImGui.MenuItem($"{name}##objectToggle"))
            {
                CFG.Current.MapEditor_ModelLoad_Objects = !CFG.Current.MapEditor_ModelLoad_Objects;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.MapEditor_ModelLoad_Objects);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_ML_Characters")}##characterToggle"))
            {
                CFG.Current.MapEditor_ModelLoad_Characters = !CFG.Current.MapEditor_ModelLoad_Characters;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.MapEditor_ModelLoad_Characters);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_ML_Collisions")}##collisionToggle"))
            {
                CFG.Current.MapEditor_ModelLoad_Collisions = !CFG.Current.MapEditor_ModelLoad_Collisions;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.MapEditor_ModelLoad_Collisions);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_ML_Navmeshes")}##navmeshToggle"))
            {
                CFG.Current.MapEditor_ModelLoad_Navmeshes = !CFG.Current.MapEditor_ModelLoad_Navmeshes;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.MapEditor_ModelLoad_Navmeshes);


            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Model_Load_Header_TT"));
    }

    public void MapTextureLoadMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Texture_Load_Header")}##textureLoadMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_TL_Map_Pieces")}##mapPieceToggle"))
            {
                CFG.Current.MapEditor_TextureLoad_MapPieces = !CFG.Current.MapEditor_TextureLoad_MapPieces;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.MapEditor_TextureLoad_MapPieces);

            var name = LOC.Get("VIEWPORT_Menubar_TL_Objects");

            if (Owner is MapUniverse mapUniverse)
            {
                if (mapUniverse.Project.Descriptor.ProjectType is ProjectType.ER 
                    or ProjectType.AC6 
                    or ProjectType.NR
                    or ProjectType.DB_NT
                    or ProjectType.DB)
                {
                    name = LOC.Get("VIEWPORT_Menubar_TL_Assets");
                }
            }

            if (ImGui.MenuItem($"{name}##objectToggle"))
            {
                CFG.Current.MapEditor_TextureLoad_Objects = !CFG.Current.MapEditor_TextureLoad_Objects;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.MapEditor_TextureLoad_Objects);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_TL_Characters")}##characterToggle"))
            {
                CFG.Current.MapEditor_TextureLoad_Characters = !CFG.Current.MapEditor_TextureLoad_Characters;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.MapEditor_TextureLoad_Characters);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_TL_Misc")}##miscToggle"))
            {
                CFG.Current.MapEditor_TextureLoad_Misc = !CFG.Current.MapEditor_TextureLoad_Misc;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.MapEditor_TextureLoad_Misc);

            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Texture_Load_Header_TT"));
    }

    public void ModelModelLoadMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Model_Load_Header")}##modelLoadMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_ML_Map_Pieces")}##mapPieceToggle"))
            {
                CFG.Current.ModelEditor_ModelLoad_MapPieces = !CFG.Current.ModelEditor_ModelLoad_MapPieces;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_ModelLoad_MapPieces);

            var name = LOC.Get("VIEWPORT_Menubar_ML_Objects");

            if (Owner is MapUniverse mapUniverse)
            {
                if (mapUniverse.Project.Descriptor.ProjectType is ProjectType.ER 
                    or ProjectType.AC6 
                    or ProjectType.NR
                    or ProjectType.DB_NT
                    or ProjectType.DB)
                {
                    name = LOC.Get("VIEWPORT_Menubar_ML_Assets");
                }
            }

            if (ImGui.MenuItem($"{name}##objectToggle"))
            {
                CFG.Current.ModelEditor_ModelLoad_Objects = !CFG.Current.ModelEditor_ModelLoad_Objects;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_ModelLoad_Objects);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_ML_Characters")}##characterToggle"))
            {
                CFG.Current.ModelEditor_ModelLoad_Characters = !CFG.Current.ModelEditor_ModelLoad_Characters;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_ModelLoad_Characters);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_ML_Parts")}##partsToggle"))
            {
                CFG.Current.ModelEditor_ModelLoad_Parts = !CFG.Current.ModelEditor_ModelLoad_Parts;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_ModelLoad_Parts);

            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Model_Load_Header_TT"));
    }

    public void ModelTextureLoadMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("VIEWPORT_Menubar_Texture_Load_Header")}##textureLoadMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_TL_Map_Pieces")}##mapPieceToggle"))
            {
                CFG.Current.ModelEditor_TextureLoad_MapPieces = !CFG.Current.ModelEditor_TextureLoad_MapPieces;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_TextureLoad_MapPieces);

            var name = LOC.Get("VIEWPORT_Menubar_TL_Objects");

            if (Owner is MapUniverse mapUniverse)
            {
                if (mapUniverse.Project.Descriptor.ProjectType is ProjectType.ER 
                    or ProjectType.AC6 
                    or ProjectType.NR
                    or ProjectType.DB_NT
                    or ProjectType.DB)
                {
                    name = LOC.Get("VIEWPORT_Menubar_TL_Assets");
                }
            }

            if (ImGui.MenuItem($"{name}##objectToggle"))
            {
                CFG.Current.ModelEditor_TextureLoad_Objects = !CFG.Current.ModelEditor_TextureLoad_Objects;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_TextureLoad_Objects);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_TL_Characters")}##characterToggle"))
            {
                CFG.Current.ModelEditor_TextureLoad_Characters = !CFG.Current.ModelEditor_TextureLoad_Characters;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_TextureLoad_Characters);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_TL_Parts")}##partsToggle"))
            {
                CFG.Current.ModelEditor_TextureLoad_Parts = !CFG.Current.ModelEditor_TextureLoad_Parts;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_TextureLoad_Parts);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Menubar_TL_Misc")}##miscToggle"))
            {
                CFG.Current.ModelEditor_TextureLoad_Misc = !CFG.Current.ModelEditor_TextureLoad_Misc;
                Parent.DelayPicking();
            }
            GUI.ShowActiveStatus(CFG.Current.ModelEditor_TextureLoad_Misc);

            ImGui.EndMenu();
        }
        GUI.Tooltip(LOC.Get("VIEWPORT_Menubar_Texture_Load_Header_TT"));
    }
}
