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
                if (ImGui.MenuItem($"{LOC.Get("MAP_MapInfo_Window_Title")}##mapInformationWindow"))
                {
                    View.WorldMapTool.ToggleWindow();
                }
                GUI.Tooltip(LOC.Get("MAP_MapInfo_Open_TT", InputManager.GetHint(KeybindID.MapEditor_Toggle_World_Map_Menu)));
                GUI.ShowActiveStatus(CFG.Current.Interface_MapEditor_MapInformation);
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

        Sections.Begin();

        //*****************************
        // General
        //*****************************
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_General"));

        if (ToolSections.Header($"{LOC.Get("MAP_Ruler_Header")}##rulerToolHeader"))
        {
            View.RulerTool.OnToolWindow();
        }

        View.HistoryTool.OnToolWindow();

        ActionSection("MAP_ToolWindow_Viewport_Filters", "mapViewportFiltersSection", View.ViewportFiltersAction.OnToolWindow);

        ActionSection("MAP_ToolWindow_Low_Detail", "mapLowDetailSection", View.LowDetailVisibilityAction.OnToolWindow);

        //*****************************
        // Authoring
        //*****************************
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Authoring"));

        if (View.CollisionEditorTool.CanUse())
        {
            if (ToolSections.Header($"{LOC.Get("MAP_CollisionEditor_Header")}##collisionEditorHeader"))
            {
                View.CollisionEditorTool.OnToolWindow();
            }
        }

        if (View.NavigationTool.CanUse())
        {
            if (ToolSections.Header($"{LOC.Get("MAP_Navigation_Header")}##navigationToolHeader"))
            {
                View.NavigationTool.OnToolWindow();
            }
        }

        if (View.DestructionGenerationTool.CanUse())
        {
            if (ToolSections.Header($"{LOC.Get("MAP_DestructableAssets_Header")}##destructableAssetsHeader"))
            {
                View.DestructionGenerationTool.OnToolWindow();
            }
        }

        if (View.Project.Descriptor.ProjectType is ProjectType.SDT)
        {
            if (ToolSections.Header($"{LOC.Get("MAP_WallEditor_Header")}##wallEditorHeader"))
            {
                View.WallPaintTool.OnToolWindow();
            }
        }

        if (View.EdgeEditorTool.CanUse())
        {
            if (ToolSections.Header($"{LOC.Get("MAP_EdgeEditor_Header")}##edgeEditorHeader"))
            {
                View.EdgeEditorTool.OnToolWindow();
            }
        }

        if (View.MfrEditorTool.CanUse())
        {
            if (ToolSections.Header($"{LOC.Get("MAP_MfrEditor_Header")}##mfrEditorHeader"))
            {
                View.MfrEditorTool.OnToolWindow();
            }
        }

        if (View.EntryFileListTool.CanUse())
        {
            if (ToolSections.Header($"{LOC.Get("MAP_EntryFileLists_Header")}##entryFileListsHeader"))
            {
                View.EntryFileListTool.OnToolWindow();
            }
        }

        if (ToolSections.Header($"{LOC.Get("MAP_LoadLists_Header")}##loadListsHeader"))
        {
            View.LoadListTool.OnToolWindow();
        }

        if (View.PartsTokenIdListTool.CanUse())
        {
            if (ToolSections.Header($"{LOC.Get("MAP_PartTokenLists_Header")}##partTokenListsHeader"))
            {
                View.PartsTokenIdListTool.OnToolWindow();
            }
        }

        if (View.AiThreadInfoTool.CanUse())
        {
            if (ToolSections.Header($"{LOC.Get("MAP_AiThreadInfo_Header")}##aiThreadInfoHeader"))
            {
                View.AiThreadInfoTool.OnToolWindow();
            }
        }

        //*****************************
        // Preview
        //*****************************
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Preview"));

        if (ToolSections.Header($"{LOC.Get("MAP_AnimationBrowser_Header")}##animationBrowserHeader"))
        {
            View.MapAnimationTool.OnToolWindow();
        }

        if (ToolSections.Header($"{LOC.Get("MAP_ModelBrowser_Header")}##modelBrowserHeader"))
        {
            View.ModelBrowserTool.OnToolWindow();
        }

        if (View.FirstPersonTool.CanUse())
        {
            if (ToolSections.Header($"{LOC.Get("MAP_FirstPerson_Header")}##firstPersonToolHeader"))
            {
                View.FirstPersonTool.OnToolWindow();
            }
        }

        //*****************************
        // Creation
        //*****************************
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Creation"));

        ActionSection("MAP_ToolWindow_Create", "mapCreateSection", View.CreateAction.OnToolWindow);
        ActionSection("MAP_ToolWindow_Duplicate", "mapDuplicateSection", View.DuplicateAction.OnToolWindow);
        ActionSection("MAP_ToolWindow_Duplicate_To_Map", "mapDuplicateToMapSection", View.DuplicateToMapAction.OnToolWindow);
        ActionSection("MAP_ToolWindow_Replicate", "mapReplicateSection", View.ReplicateAction.OnToolWindow);

        if (ToolSections.Header($"{LOC.Get("MAP_Prefabs_Header")}##prefabsToolHeader"))
        {
            View.PrefabTool.OnToolWindow();
        }

        //*****************************
        // Transform
        //*****************************
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Transform"));

        ActionSection("MAP_ToolWindow_Translate", "mapTranslateSection", View.TranslateAction.OnToolWindow);
        ActionSection("MAP_ToolWindow_Rotate", "mapRotateSection", View.RotateAction.OnToolWindow);
        ActionSection("MAP_ToolWindow_Pull_To_Camera", "mapPullToCameraSection", View.PullToCameraAction.OnToolWindow);
        ActionSection("MAP_ToolWindow_Snap_To_Mesh", "mapSnapToMeshSection", View.SnapToMeshAction.OnToolWindow);
        ActionSection("MAP_ToolWindow_Scramble", "mapScrambleSection", View.ScrambleAction.OnToolWindow);
        ActionSection("MAP_ToolWindow_Reset_Transform", "mapResetTransformSection", PropertiesActionTool.DisplayResetTransform);

        ActionSection("MAP_ToolWindow_Editor_Visibility", "mapEditorVisibilitySection", View.EditorVisibilityAction.OnToolWindow);

        ActionSection("MAP_DisplayGroups_Header", "mapDisplayGroupsSection", View.DisplayGroupTool.OnToolWindow);

        ActionSection("MAP_ToolWindow_Property_Actions", "mapPropertyActionsSection", PropertiesActionTool.Display);

        //*****************************
        // Selection
        //*****************************
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Selection"));

        ActionSection("MAP_ToolWindow_Selection", "mapSelectionSection", SelectActionTool.Display);

        // Draws its own header and section
        View.SelectionGroupsTool.OnToolWindow();

        //*****************************
        // Search & Edit
        //*****************************
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Search"));

        // Each draws its own header and section
        View.LocalSearchTool.OnToolWindow();

        View.GlobalSearchTool.OnToolWindow();

        View.MassEditTool.OnToolWindow();

        //*****************************
        // Data
        //*****************************
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Data"));

        // Draws its own header and section
        View.MapExtractionTool.OnToolWindow();

        // Draws its own header and section
        if (View.PartTransferTool.CanUse())
        {
            View.PartTransferTool.OnToolWindow();
        }

        // Draws its own header and section
        if (View.TextureTransferTool.CanUse())
        {
            View.TextureTransferTool.OnToolWindow();
        }

        // Draws its own header and section
        if (View.MapPortTool.CanUse())
        {
            View.MapPortTool.OnToolWindow();
        }

        //*****************************
        // Validation
        //*****************************
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Validation"));

        // Save Check does this now
        //ActionSection("MAP_ToolWindow_Msb_Validation", "mapMsbValidationSection", View.MapValidatorTool.OnToolWindow);

        ActionSection("MAP_ToolWindow_Vanilla_Compare", "mapVanillaCompareSection", View.VanillaCompareTool.OnToolWindow);

        ActionSection(!(View.Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S),
            "MAP_ToolWindow_Entity_Ids", "mapEntityIdsSection", View.EntityIdentifierTool.OnToolWindow);

        if (ToolSections.Header($"{LOC.Get("MAP_AI_Visualization_Header")}##aiVisToolHeader"))
        {
            View.EnemyAIVisualizationTool.OnToolWindow();
        }

        //if (View.IrradianceVolumeBank.CanUse())
        //{
        //    // The Viewport Rendering section's Global Illumination tab asks for this tool
        //    if (View.IrradianceVolumeIdTool.FocusRequested)
        //    {
        //        ToolSections.Focus();
        //        View.IrradianceVolumeIdTool.FocusRequested = false;
        //    }

        //    ActionSection("MAP_GI_Tool_Tab", "mapIrradianceVolumeIdsSection", View.IrradianceVolumeIdTool.OnToolWindow);
        //}

        //*****************************
        // Game Memory
        //*****************************
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Game_Memory"));

        ActionSection(View.WorldReloaderTool.CanUse(),
            "MAP_WorldReloader_Header", "mapWorldReloaderSection", View.WorldReloaderTool.OnToolWindow);

        //*****************************
        // Viewport
        //*****************************
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Viewport"));


        View.GridTool.OnToolWindow();

        View.ResourceListTool.Display("mapEditor", View.Universe);

        //*****************************
        // Rendering
        //*****************************
        View.RenderingTool.OnToolWindow();

        Sections.End();
    }

    /// <summary>
    /// Registers one tool in the list and, while it is the selected tool, draws it in its own bordered child.
    /// </summary>
    private static void ActionSection(string locKey, string childId, Action draw)
    {
        if (!ToolSections.Header($"{LOC.Get(locKey)}##{childId}Header"))
            return;

        ImGui.BeginChild(childId, ImGuiChildFlags.Borders);

        draw();

        ImGui.EndChild();
    }

    /// <summary>
    /// As <see cref="ActionSection(string, string, Action)"/>, for a tool only some projects can use.
    /// </summary>
    private static void ActionSection(bool canUse, string locKey, string childId, Action draw)
    {
        if (canUse)
            ActionSection(locKey, childId, draw);
    }
}
