using Hexa.NET.ImGui;
using StudioCore.Editors.Viewport;
using StudioCore.Keybinds;

namespace StudioCore.Editors.MapEditor;

public class MapToolWindow
{
    private MapEditorView View;
    private ProjectEntry Project;

    public PropertiesActionTool PropertiesActionTool;
    public SelectActionTool SelectActionTool;
    public MapDataTransferTool DataTransferTool;

    private ToolSections Sections = new();

    public MapToolWindow(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        PropertiesActionTool = new(view, project);
        SelectActionTool = new(view, project);
        DataTransferTool = new(view, project);
    }

    public void DisplayDropdown()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            DataTransferTool.DisplayDropdown();

            if (Project.Descriptor.ProjectType is ProjectType.ER 
                or ProjectType.NR)
            {
                if (ImGui.MenuItem("World Map"))
                {
                    View.WorldMapTool.DisplayMenuOption();
                }
                GUI.Tooltip($"Open a world map with a visual representation of the map tiles.\nShortcut: {InputManager.GetHint(KeybindID.MapEditor_Toggle_World_Map_Menu)}");
            }

            if (ImGui.BeginMenu("Miscellaneous"))
            {
                View.EditorVisibilityAction.OnToolMenu();

                ///--------------------
                /// Generate Navigation Data
                ///--------------------
                if (View.Editor.Project.Descriptor.ProjectType is ProjectType.DES || View.Editor.Project.Descriptor.ProjectType is ProjectType.DS1 || View.Editor.Project.Descriptor.ProjectType is ProjectType.DS1R)
                {
                    if (ImGui.BeginMenu("Navigation Data"))
                    {
                        if (ImGui.MenuItem("Generate"))
                        {
                            View.ActionHandler.GenerateNavigationData();
                        }

                        ImGui.EndMenu();
                    }
                }

                ///--------------------
                /// Generate Break Data
                ///--------------------
                if (Project.Handler.MapData?.BreakDataGenerator?.CanUse() == true)
                {
                    if (ImGui.BeginMenu("Break Data"))
                    {
                        if (ImGui.MenuItem("Generate for Loaded Maps"))
                        {
                            View.ActionHandler.GenerateBreakData(loadedMapsOnly: true);
                        }
                        GUI.Tooltip("Rebuild the break data (BREAKOBJ/BREAKGEOM) of the loaded maps from their saved MSBs.\nObjects unchanged from vanilla keep their vanilla break data; new breakable objects get a copy from a vanilla instance of their model.\nSave the maps first: unsaved changes are not included.");

                        if (ImGui.MenuItem("Generate for All Maps"))
                        {
                            View.ActionHandler.GenerateBreakData(loadedMapsOnly: false);
                        }
                        GUI.Tooltip("Rebuild the break data of every map in the project. Only files that change are written.");

                        ImGui.EndMenu();
                    }
                }

                ///--------------------
                /// Entity ID Checker
                ///--------------------
                if (View.Editor.Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
                {
                    View.EntityIdCheckAction.OnToolMenu();
                }

                ///--------------------
                /// Name Map Objects
                ///--------------------
                // Tool for AC6 since its maps come with unnamed Regions and Events
                if (View.Editor.Project.Descriptor.ProjectType is ProjectType.AC6)
                {
                    View.EntityRenameAction.OnToolMenu();
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    public void Display()
    {
        // The action popups are drawn by MapEditorView every frame, so they also open with this window hidden

        if (ImGui.BeginMenuBar())
        {
            ViewMenu();

            ToolSections.OptionsMenu();

            ImGui.EndMenuBar();
        }

        Sections.Begin();

        // Create: tools that add new map objects
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Create"));

        ActionSection(CFG.Current.Interface_MapEditor_Tool_Create, "MAP_ToolWindow_Create", "mapCreateSection", View.CreateAction.OnToolWindow);
        ActionSection(CFG.Current.Interface_MapEditor_Tool_Duplicate, "MAP_ToolWindow_Duplicate", "mapDuplicateSection", View.DuplicateAction.OnToolWindow);
        ActionSection(CFG.Current.Interface_MapEditor_Tool_DuplicateToMap, "MAP_ToolWindow_Duplicate_To_Map", "mapDuplicateToMapSection", View.DuplicateToMapAction.OnToolWindow);
        ActionSection(CFG.Current.Interface_MapEditor_Tool_Replicate, "MAP_ToolWindow_Replicate", "mapReplicateSection", View.ReplicateAction.OnToolWindow);

        // Transform: tools that change the position, rotation or scale of the selection
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Transform"));

        ActionSection(CFG.Current.Interface_MapEditor_Tool_Translate, "MAP_ToolWindow_Translate", "mapTranslateSection", View.TranslateAction.OnToolWindow);
        ActionSection(CFG.Current.Interface_MapEditor_Tool_Rotate, "MAP_ToolWindow_Rotate", "mapRotateSection", View.RotateAction.OnToolWindow);
        ActionSection(CFG.Current.Interface_MapEditor_Tool_PullToCamera, "MAP_ToolWindow_Pull_To_Camera", "mapPullToCameraSection", View.PullToCameraAction.OnToolWindow);
        ActionSection(CFG.Current.Interface_MapEditor_Tool_SnapToMesh, "MAP_ToolWindow_Snap_To_Mesh", "mapSnapToMeshSection", View.SnapToMeshAction.OnToolWindow);
        ActionSection(CFG.Current.Interface_MapEditor_Tool_Scramble, "MAP_ToolWindow_Scramble", "mapScrambleSection", View.ScrambleAction.OnToolWindow);
        ActionSection(CFG.Current.Interface_MapEditor_Tool_ResetTransform, "MAP_ToolWindow_Reset_Transform", "mapResetTransformSection", PropertiesActionTool.DisplayResetTransform);

        // Actions: property writes and selection changes
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Actions"));

        ActionSection(CFG.Current.Interface_MapEditor_Tool_Properties_Action, "MAP_ToolWindow_Property_Actions", "mapPropertyActionsSection", PropertiesActionTool.Display);
        ActionSection(CFG.Current.Interface_MapEditor_Tool_Select_Action, "MAP_ToolWindow_Selection", "mapSelectionSection", SelectActionTool.Display);

        // Draws its own header and section
        if (CFG.Current.Interface_MapEditor_Tool_SelectionGroups)
        {
            View.SelectionGroupsTool.OnToolWindow();
        }

        // Search & Edit
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Search"));

        // Each draws its own header and section
        if (CFG.Current.Interface_MapEditor_Tool_LocalSearch)
        {
            View.LocalSearchTool.OnToolWindow();
        }

        if (CFG.Current.Interface_MapEditor_Tool_GlobalSearch)
        {
            View.GlobalSearchTool.OnToolWindow();
        }

        if (CFG.Current.Interface_MapEditor_Tool_MassEdit)
        {
            View.MassEditTool.OnToolWindow();
        }

        // Validation
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Validation"));

        ActionSection(CFG.Current.Interface_MapEditor_Tool_MsbValidation, "MAP_ToolWindow_Msb_Validation", "mapMsbValidationSection", View.MapValidatorTool.OnToolWindow);

        ActionSection(CFG.Current.Interface_MapEditor_Tool_EntityIds && !(View.Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S),
            "MAP_ToolWindow_Entity_Ids", "mapEntityIdsSection", View.EntityIdentifierTool.OnToolWindow);

        if (CFG.Current.Interface_MapEditor_Tool_IrradianceVolumeIds && View.IrradianceVolumeBank.CanUse())
        {
            // The Viewport Rendering section's Global Illumination tab asks for this tool
            if (View.IrradianceVolumeIdTool.FocusRequested)
            {
                ToolSections.Focus();
                View.IrradianceVolumeIdTool.FocusRequested = false;
            }

            ActionSection(true, "MAP_GI_Tool_Tab", "mapIrradianceVolumeIdsSection", View.IrradianceVolumeIdTool.OnToolWindow);
        }

        // Browse
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Browse"));

        if (CFG.Current.Interface_MapEditor_Tool_Animation)
        {
            if (ToolSections.Header("Animation Browser"))
            {
                View.MapAnimationTool.OnToolWindow();
            }
        }

        if (CFG.Current.Interface_MapEditor_Tool_ModelBrowser)
        {
            if (ToolSections.Header("Model Browser"))
            {
                View.ModelBrowserTool.OnToolWindow();
            }
        }

        if (CFG.Current.Interface_MapEditor_Tool_Prefab)
        {
            if (ToolSections.Header("Prefabs"))
            {
                View.PrefabTool.OnToolWindow();
            }
        }

        // Map Data
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Map_Data"));

        if (CFG.Current.Interface_MapEditor_Tool_WallPaint && View.Project.Descriptor.ProjectType is ProjectType.SDT)
        {
            if (ToolSections.Header("Wall Painting"))
            {
                View.WallPaintTool.OnToolWindow();
            }
        }

        if (CFG.Current.Interface_MapEditor_Tool_CollisionEditor && View.CollisionEditorTool.CanUse())
        {
            if (ToolSections.Header($"{LOC.Get("MAP_CollisionEditor_Header")}##collisionEditorHeader"))
            {
                View.CollisionEditorTool.OnToolWindow();
            }
        }

        if (CFG.Current.Interface_MapEditor_Tool_Navigation && View.NavigationTool.CanUse())
        {
            if (ToolSections.Header($"{LOC.Get("MAP_Navigation_Header")}##navigationToolHeader"))
            {
                View.NavigationTool.OnToolWindow();
            }
        }

        if (CFG.Current.Interface_MapEditor_Tool_DestructionGeneration && View.DestructionGenerationTool.CanUse())
        {
            if (ToolSections.Header(View.DestructionGenerationTool.Title))
            {
                View.DestructionGenerationTool.OnToolWindow();
            }
        }

        if (CFG.Current.Interface_MapEditor_Tool_LoadList)
        {
            if (ToolSections.Header("Load Lists"))
            {
                View.LoadListTool.OnToolWindow();
            }
        }

        if (CFG.Current.Interface_MapEditor_Tool_EntryFileList && View.EntryFileListTool.CanUse())
        {
            if (ToolSections.Header("Entry File Lists"))
            {
                View.EntryFileListTool.OnToolWindow();
            }
        }

        if (CFG.Current.Interface_MapEditor_Tool_PartsTokenIdList && View.PartsTokenIdListTool.CanUse())
        {
            if (ToolSections.Header("Parts Token ID Lists"))
            {
                View.PartsTokenIdListTool.OnToolWindow();
            }
        }

        // Viewport
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Viewport"));

        // Editor-only visibility: these change what the viewport draws, not the map data
        ActionSection(CFG.Current.Interface_MapEditor_Tool_ViewportFilters, "MAP_ToolWindow_Viewport_Filters", "mapViewportFiltersSection", View.ViewportFiltersAction.OnToolWindow);
        ActionSection(CFG.Current.Interface_MapEditor_Tool_EditorVisibility, "MAP_ToolWindow_Editor_Visibility", "mapEditorVisibilitySection", View.EditorVisibilityAction.OnToolWindow);
        ActionSection(CFG.Current.Interface_MapEditor_Tool_LowDetail, "MAP_ToolWindow_Low_Detail", "mapLowDetailSection", View.LowDetailVisibilityAction.OnToolWindow);

        if (CFG.Current.Interface_MapEditor_Tool_DisplayGroups)
        {
            if (ToolSections.Header("Render Groups"))
            {
                View.DisplayGroupTool.OnToolWindow();
            }
        }

        if (CFG.Current.Interface_MapEditor_Tool_EnemyAIVisualization)
        {
            if (ToolSections.Header("Detection Visualization"))
            {
                View.EnemyAIVisualizationTool.OnToolWindow();
            }
        }

        if (CFG.Current.Interface_MapEditor_Tool_Ruler)
        {
            if (ToolSections.Header($"{LOC.Get("MAP_Ruler_Header")}##rulerToolHeader"))
            {
                View.RulerTool.OnToolWindow();
            }
        }

        if (CFG.Current.Interface_MapEditor_Tool_GridConfiguration)
        {
            View.GridTool.OnToolWindow();
        }

        if (CFG.Current.Interface_MapEditor_ResourceList)
        {
            View.ResourceListTool.Display("mapEditor", View.Universe);
        }

        // Rendering: sets its own category, so it comes last
        if (CFG.Current.Interface_MapEditor_Tool_ViewportRendering)
        {
            View.RenderingTool.OnToolWindow();
        }

        Sections.End();
    }

    /// <summary>
    /// Registers one tool in the list and, while it is the selected tool, draws it in its own bordered child.
    /// </summary>
    private static void ActionSection(bool enabled, string locKey, string childId, Action draw)
    {
        if (!enabled)
            return;

        if (!ToolSections.Header($"{LOC.Get(locKey)}##{childId}Header"))
            return;

        ImGui.BeginChild(childId, ImGuiChildFlags.Borders);

        draw();

        ImGui.EndChild();
    }

    /// <summary>
    /// View menu toggle for a tool, labelled with the same text as its list entry.
    /// </summary>
    private static void ToolToggle(string locKey, ref bool setting)
    {
        if (ImGui.MenuItem($"{LOC.Get(locKey)}##{locKey}Toggle"))
        {
            setting = !setting;
        }
        GUI.ShowActiveStatus(setting);
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            // Create
            ToolToggle("MAP_ToolWindow_Create", ref CFG.Current.Interface_MapEditor_Tool_Create);
            ToolToggle("MAP_ToolWindow_Duplicate", ref CFG.Current.Interface_MapEditor_Tool_Duplicate);
            ToolToggle("MAP_ToolWindow_Duplicate_To_Map", ref CFG.Current.Interface_MapEditor_Tool_DuplicateToMap);
            ToolToggle("MAP_ToolWindow_Replicate", ref CFG.Current.Interface_MapEditor_Tool_Replicate);

            // Transform
            ToolToggle("MAP_ToolWindow_Translate", ref CFG.Current.Interface_MapEditor_Tool_Translate);
            ToolToggle("MAP_ToolWindow_Rotate", ref CFG.Current.Interface_MapEditor_Tool_Rotate);
            ToolToggle("MAP_ToolWindow_Pull_To_Camera", ref CFG.Current.Interface_MapEditor_Tool_PullToCamera);
            ToolToggle("MAP_ToolWindow_Snap_To_Mesh", ref CFG.Current.Interface_MapEditor_Tool_SnapToMesh);
            ToolToggle("MAP_ToolWindow_Scramble", ref CFG.Current.Interface_MapEditor_Tool_Scramble);
            ToolToggle("MAP_ToolWindow_Reset_Transform", ref CFG.Current.Interface_MapEditor_Tool_ResetTransform);

            // Actions
            ToolToggle("MAP_ToolWindow_Property_Actions", ref CFG.Current.Interface_MapEditor_Tool_Properties_Action);
            ToolToggle("MAP_ToolWindow_Selection", ref CFG.Current.Interface_MapEditor_Tool_Select_Action);
            ToolToggle("MAP_ToolWindow_Selection_Groups", ref CFG.Current.Interface_MapEditor_Tool_SelectionGroups);

            // Viewport visibility
            ToolToggle("MAP_ToolWindow_Viewport_Filters", ref CFG.Current.Interface_MapEditor_Tool_ViewportFilters);
            ToolToggle("MAP_ToolWindow_Editor_Visibility", ref CFG.Current.Interface_MapEditor_Tool_EditorVisibility);
            ToolToggle("MAP_ToolWindow_Low_Detail", ref CFG.Current.Interface_MapEditor_Tool_LowDetail);

            // Validation
            ToolToggle("MAP_ToolWindow_Msb_Validation", ref CFG.Current.Interface_MapEditor_Tool_MsbValidation);

            if (!(View.Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S))
            {
                ToolToggle("MAP_ToolWindow_Entity_Ids", ref CFG.Current.Interface_MapEditor_Tool_EntityIds);
            }

            if (View.IrradianceVolumeBank.CanUse())
            {
                ToolToggle("MAP_GI_Tool_Tab", ref CFG.Current.Interface_MapEditor_Tool_IrradianceVolumeIds);
            }

            ImGui.Separator();

            if (ImGui.MenuItem("Viewport"))
            {
                CFG.Current.Interface_MapEditor_Tool_Viewport = !CFG.Current.Interface_MapEditor_Tool_Viewport;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_Viewport);

            if (ImGui.MenuItem("Data Transfer"))
            {
                CFG.Current.Interface_MapEditor_Tool_Data_Transfer = !CFG.Current.Interface_MapEditor_Tool_Data_Transfer;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_Data_Transfer);

            if (ImGui.MenuItem("Prefabs"))
            {
                CFG.Current.Interface_MapEditor_Tool_Prefab = !CFG.Current.Interface_MapEditor_Tool_Prefab;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_Prefab);

            ToolToggle("MAP_SearchTool_Local_Header", ref CFG.Current.Interface_MapEditor_Tool_LocalSearch);
            ToolToggle("MAP_SearchTool_Global_Header", ref CFG.Current.Interface_MapEditor_Tool_GlobalSearch);
            ToolToggle("MAP_MassEditTool_Header", ref CFG.Current.Interface_MapEditor_Tool_MassEdit);

            // Ruler
            if (ImGui.MenuItem($"{LOC.Get("MAP_ToolWindow_View_Toggle_Ruler")}##rulerToolToggle"))
            {
                CFG.Current.Interface_MapEditor_Tool_Ruler = !CFG.Current.Interface_MapEditor_Tool_Ruler;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_Ruler);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_GridConfig_Header")}##toggleGridConfigTool"))
            {
                CFG.Current.Interface_MapEditor_Tool_GridConfiguration = !CFG.Current.Interface_MapEditor_Tool_GridConfiguration;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_GridConfiguration);

            if (ImGui.MenuItem($"{LOC.Get("VIEWPORT_Rendering_Tool_Header")}##toggleViewportRenderingTool"))
            {
                CFG.Current.Interface_MapEditor_Tool_ViewportRendering = !CFG.Current.Interface_MapEditor_Tool_ViewportRendering;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_ViewportRendering);

            if (ImGui.MenuItem("Model Browser"))
            {
                CFG.Current.Interface_MapEditor_Tool_ModelBrowser = !CFG.Current.Interface_MapEditor_Tool_ModelBrowser;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_ModelBrowser);

            if (ImGui.MenuItem("Animation Browser"))
            {
                CFG.Current.Interface_MapEditor_Tool_Animation = !CFG.Current.Interface_MapEditor_Tool_Animation;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_Animation);

            if (ImGui.MenuItem("Display Groups"))
            {
                CFG.Current.Interface_MapEditor_Tool_DisplayGroups = !CFG.Current.Interface_MapEditor_Tool_DisplayGroups;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_DisplayGroups);

            if (ImGui.MenuItem("Detection Visualization"))
            {
                CFG.Current.Interface_MapEditor_Tool_EnemyAIVisualization = !CFG.Current.Interface_MapEditor_Tool_EnemyAIVisualization;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_EnemyAIVisualization);

            if (View.Project.Descriptor.ProjectType is ProjectType.SDT)
            {
                if (ImGui.MenuItem("Wall Painting"))
                {
                    CFG.Current.Interface_MapEditor_Tool_WallPaint = !CFG.Current.Interface_MapEditor_Tool_WallPaint;
                }
                GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_WallPaint);
            }

            // Collision Editor
            if (View.CollisionEditorTool.CanUse())
            {
                if (ImGui.MenuItem($"{LOC.Get("MAP_ToolWindow_View_Toggle_Collision_Editor")}##collisionEditorToolToggle"))
                {
                    CFG.Current.Interface_MapEditor_Tool_CollisionEditor = !CFG.Current.Interface_MapEditor_Tool_CollisionEditor;
                }
                GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_CollisionEditor);
            }

            // Navigation
            if (View.NavigationTool.CanUse())
            {
                if (ImGui.MenuItem($"{LOC.Get("MAP_ToolWindow_View_Toggle_Navigation")}##navigationToolToggle"))
                {
                    CFG.Current.Interface_MapEditor_Tool_Navigation = !CFG.Current.Interface_MapEditor_Tool_Navigation;
                }
                GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_Navigation);
            }

            if (View.DestructionGenerationTool.CanUse())
            {
                if (ImGui.MenuItem(View.DestructionGenerationTool.Title))
                {
                    CFG.Current.Interface_MapEditor_Tool_DestructionGeneration = !CFG.Current.Interface_MapEditor_Tool_DestructionGeneration;
                }
                GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_DestructionGeneration);
            }

            if (ImGui.MenuItem("Load Lists"))
            {
                CFG.Current.Interface_MapEditor_Tool_LoadList = !CFG.Current.Interface_MapEditor_Tool_LoadList;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_LoadList);

            if (View.EntryFileListTool.CanUse())
            {
                if (ImGui.MenuItem("Entry File Lists"))
                {
                    CFG.Current.Interface_MapEditor_Tool_EntryFileList = !CFG.Current.Interface_MapEditor_Tool_EntryFileList;
                }
                GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_EntryFileList);
            }

            if (View.PartsTokenIdListTool.CanUse())
            {
                if (ImGui.MenuItem("Parts Token ID Lists"))
                {
                    CFG.Current.Interface_MapEditor_Tool_PartsTokenIdList = !CFG.Current.Interface_MapEditor_Tool_PartsTokenIdList;
                }
                GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_PartsTokenIdList);
            }

            if (ImGui.MenuItem("Viewport Configuration"))
            {
                CFG.Current.Interface_MapEditor_Tool_ViewportConfig = !CFG.Current.Interface_MapEditor_Tool_ViewportConfig;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_ViewportConfig);

            if (ImGui.MenuItem("Resource List"))
            {
                CFG.Current.Interface_MapEditor_ResourceList = !CFG.Current.Interface_MapEditor_ResourceList;
            }
            GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_ResourceList);

            ImGui.EndMenu();
        }
    }
}
