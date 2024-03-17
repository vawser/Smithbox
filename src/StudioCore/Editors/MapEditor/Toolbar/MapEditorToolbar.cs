using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using Microsoft.Toolkit.HighPerformance;
using StudioCore.Gui;
using StudioCore.Scene;
using StudioCore.Utilities;
using System.IO;
using System;
using DotNext;
using Veldrid.Utilities;
using SoulsFormats;
using StudioCore.Interface;
using System.Reflection;
using StudioCore.UserProject;
using StudioCore.Banks;
using StudioCore.MsbEditor;
using StudioCore.BanksMain;
using StudioCore.Platform;
using StudioCore.Editors.MapEditor.Prefabs;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using StudioCore.Editor;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public class MapEditorToolbar
    {
        public static ViewportActionManager _actionManager;
        public static RenderScene _scene;
        public static ViewportSelection _selection;
        public static Universe _universe;

        private IViewport _viewport;

        // These are used by the Prefab actions
        // Held here since they need to persist across all of them
        public static string _prefabName;
        public static string _prefabExt;
        public static string _prefabDir;

        public static string _newPrefabName;
        public static string _prefabTags;

        public static List<PrefabInfo> _prefabInfos;
        public static PrefabInfo _selectedPrefabInfo;
        public static PrefabInfo _selectedPrefabInfoCache;
        public static List<string> _selectedPrefabObjectNames;

        public static (string, ObjectContainer) _comboTargetMap;

        public MapEditorToolbar(RenderScene scene, ViewportSelection sel, ViewportActionManager manager, Universe universe, IViewport viewport, (string, ObjectContainer) comboTargetMap)
        {
            _scene = scene;
            _selection = sel;
            _actionManager = manager;
            _universe = universe;

            _viewport = viewport;

            _prefabName = "";
            _prefabExt = ".json";
            _prefabDir = "";
            _newPrefabName = "";
            _prefabTags = "";
            _comboTargetMap = comboTargetMap;

            MapEditorState.ActionManager = _actionManager;
            MapEditorState.Scene = _scene;
            MapEditorState.Universe = _universe;
            MapEditorState.Viewport = _viewport;
            MapEditorState.Toolbar = this;
        }

        public static bool IsSupportedProjectTypeForPrefabs()
        {
            if (!(Project.Type is ProjectType.ER or ProjectType.DS3 or ProjectType.SDT or ProjectType.DS2S or ProjectType.DS1 or ProjectType.DS1R))
                return false;

            return true;
        }

        public void OnProjectChanged()
        {
            _selectedPrefabObjectNames = new List<string>();
            _prefabInfos = new List<PrefabInfo>();
            _selectedPrefabInfo = null;
            _selectedPrefabInfoCache = null;
            _comboTargetMap = ("", null);
            _newPrefabName = "";
            _prefabTags = "";

            _prefabDir = $"{Project.GameModDirectory}\\.smithbox\\{Project.GetGameIDForDir()}\\prefabs\\";

            if (!Directory.Exists(_prefabDir))
            {
                Directory.CreateDirectory(_prefabDir);
            }

            RefreshPrefabList();
        }

        public void OnGui()
        {
            var scale = Smithbox.GetUIScale();

            if (Project.Type == ProjectType.Undefined)
                return;

            if (!CFG.Current.Interface_MapEditor_Toolbar)
                return;

            MapAction_GenerateNavigationData.OnTextReset();
            MapEditorState.LoadedMaps = _universe.LoadedObjectContainers.Values.Where(x => x != null);

            ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
            ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * scale, ImGuiCond.FirstUseEver);

            if (ImGui.Begin($"Toolbar##MapEditorToolbar"))
            {
                var width = ImGui.GetWindowWidth();
                var height = ImGui.GetWindowHeight();

                if (CFG.Current.Interface_MapEditor_Toolbar_HorizontalOrientation)
                {
                    ImGui.Columns(2);

                    ImGui.BeginChild("##MapEditorToolbar_Selection");

                    ShowActionList();

                    ImGui.EndChild();

                    ImGui.NextColumn();

                    ImGui.BeginChild("##MapEditorToolbar_Configuration");

                    ShowSelectedConfiguration();

                    ImGui.EndChild();
                }
                else
                {
                    ImGui.BeginChild("##MapEditorToolbar_Selection", new Vector2((width - 10), (height / 3)));

                    ShowActionList();
                    
                    ImGui.EndChild();

                    ImGui.BeginChild("##MapEditorToolbar_Configuration");

                    ShowSelectedConfiguration();

                    ImGui.EndChild();
                }
            }

            ImGui.End();
            ImGui.PopStyleColor(1);
        }

        public void ShowActionList()
        {
            ImGui.Separator();
            ImGui.AlignTextToFramePadding();
            ImGui.Text("Actions");
            ImguiUtils.ShowHoverTooltip("Click to select a toolbar action.");
            ImGui.SameLine();

            if (ImGui.Button($"{ForkAwesome.Refresh}##SwitchOrientation"))
            {
                CFG.Current.Interface_MapEditor_Toolbar_HorizontalOrientation = !CFG.Current.Interface_MapEditor_Toolbar_HorizontalOrientation;
            }
            ImguiUtils.ShowHoverTooltip("Toggle the orientation of the toolbar.");
            ImGui.SameLine();

            if (ImGui.Button($"{ForkAwesome.ExclamationTriangle}##PromptUser"))
            {
                if(CFG.Current.Interface_MapEditor_PromptUser)
                {
                    CFG.Current.Interface_MapEditor_PromptUser = false;
                    PlatformUtils.Instance.MessageBox("Map Editor Toolbar will no longer prompt the user.", "Smithbox", MessageBoxButtons.OK);
                }
                else
                {
                    CFG.Current.Interface_MapEditor_PromptUser = true;
                    PlatformUtils.Instance.MessageBox("Map Editor Toolbar will prompt user before applying certain toolbar actions.", "Smithbox", MessageBoxButtons.OK);
                }
            }
            ImguiUtils.ShowHoverTooltip("Toggle whether certain toolbar actions prompt the user before applying.");
            ImGui.Separator();

            // Contextual
            MapAction_GoToInObjectList.Select(_selection);
            MapAction_FrameInViewport.Select(_selection);
            MapAction_MoveToCamera.Select(_selection);
            MapAction_MoveToGrid.Select(_selection);

            MapAction_TogglePresence.Select(_selection);
            MapAction_ToggleVisibility.Select(_selection);

            MapAction_Duplicate.Select(_selection);
            MapAction_Rotate.Select(_selection);
            MapAction_Scramble.Select(_selection);
            MapAction_Replicate.Select(_selection);

            // Global
            MapAction_Create.Select(_selection);
            MapAction_AssignEntityGroupID.Select(_selection);
            MapAction_ToggleObjectVisibilityByTag.Select(_selection);
            MapAction_TogglePatrolRoutes.Select(_selection);
            MapAction_CheckForErrors.Select(_selection);
            MapAction_GenerateNavigationData.Select(_selection);

            // Prefabs
            //MapAction_EditPrefab.Select(_selection);
            MapAction_ImportPrefab.Select(_selection);
            MapAction_ExportPrefab.Select(_selection);
        }

        public void ShowSelectedConfiguration()
        {
            ImGui.Indent(10.0f);
            ImGui.Separator();
            ImGui.Text("Configuration");
            ImGui.Separator();

            // Shortcut: Contextual
            MapAction_GoToInObjectList.Shortcuts();
            MapAction_FrameInViewport.Shortcuts();
            MapAction_MoveToCamera.Shortcuts();
            MapAction_MoveToGrid.Shortcuts();

            MapAction_TogglePresence.Shortcuts();
            MapAction_ToggleVisibility.Shortcuts();

            MapAction_Duplicate.Shortcuts();
            MapAction_Rotate.Shortcuts();
            MapAction_Scramble.Shortcuts();
            MapAction_Replicate.Shortcuts();

            // Prefabs
            //MapAction_EditPrefab.Shortcuts();
            MapAction_ImportPrefab.Shortcuts();
            MapAction_ExportPrefab.Shortcuts();

            // Shortcut: Global
            MapAction_Create.Shortcuts();
            MapAction_AssignEntityGroupID.Shortcuts();
            MapAction_ToggleObjectVisibilityByTag.Shortcuts();
            MapAction_TogglePatrolRoutes.Shortcuts();
            MapAction_CheckForErrors.Shortcuts();
            MapAction_GenerateNavigationData.Shortcuts();

            // Configure: Contextual
            MapAction_GoToInObjectList.Configure(_selection);
            MapAction_FrameInViewport.Configure(_selection);
            MapAction_MoveToCamera.Configure(_selection);
            MapAction_MoveToGrid.Configure(_selection);

            MapAction_TogglePresence.Configure(_selection);
            MapAction_ToggleVisibility.Configure(_selection);

            MapAction_Duplicate.Configure(_selection);
            MapAction_Rotate.Configure(_selection);
            MapAction_Scramble.Configure(_selection);
            MapAction_Replicate.Configure(_selection);

            // Prefabs
            //MapAction_EditPrefab.Configure(_selection);
            MapAction_ImportPrefab.Configure(_selection);
            MapAction_ExportPrefab.Configure(_selection);

            // Configure: Global
            MapAction_Create.Configure(_selection);
            MapAction_AssignEntityGroupID.Configure(_selection);
            MapAction_ToggleObjectVisibilityByTag.Configure(_selection);
            MapAction_TogglePatrolRoutes.Configure(_selection);
            MapAction_CheckForErrors.Configure(_selection);
            MapAction_GenerateNavigationData.Configure(_selection);

            // Act: Contextual
            MapAction_GoToInObjectList.Act(_selection);
            MapAction_FrameInViewport.Act(_selection);
            MapAction_MoveToCamera.Act(_selection);
            MapAction_MoveToGrid.Act(_selection);

            MapAction_TogglePresence.Act(_selection);
            MapAction_ToggleVisibility.Act(_selection);

            MapAction_Duplicate.Act(_selection);
            MapAction_Rotate.Act(_selection);
            MapAction_Scramble.Act(_selection);
            MapAction_Replicate.Act(_selection);

            // Act: Global
            MapAction_Create.Act(_selection);
            MapAction_AssignEntityGroupID.Act(_selection);
            MapAction_ToggleObjectVisibilityByTag.Act(_selection);
            MapAction_TogglePatrolRoutes.Act(_selection);
            MapAction_CheckForErrors.Act(_selection);
            MapAction_GenerateNavigationData.Act(_selection);

            // Prefabs
            //MapAction_EditPrefab.Act(_selection);
            MapAction_ImportPrefab.Act(_selection);
            MapAction_ExportPrefab.Act(_selection);
        }

        public static void RefreshPrefabList()
        {
            _prefabInfos = GetPrefabList();
        }

        public static List<PrefabInfo> GetPrefabList()
        {
            List<PrefabInfo> infoList = new();

            string[] files = Directory.GetFiles(_prefabDir, "*.json", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var name = Path.GetFileNameWithoutExtension(file);
                PrefabInfo info = new PrefabInfo(name, file, GetPrefabTags(file));
                infoList.Add(info);
            }

            return infoList;
        }

        public static List<string> GetPrefabTags(string filepath)
        {
            var options = new JsonSerializerOptions
            {
                ReadCommentHandling = JsonCommentHandling.Skip,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            };

            List<string> tags = new List<string>();

            switch (Project.Type)
            {
                case ProjectType.ER:
                    var prefab_ER = new Prefab_ER();
                    using (var stream = File.OpenRead(filepath))
                        prefab_ER = JsonSerializer.Deserialize<Prefab_ER>(File.OpenRead(filepath), options);

                    tags = prefab_ER.TagList;
                    break;
                case ProjectType.SDT:
                    var prefab_SDT = new Prefab_SDT();
                    using (var stream = File.OpenRead(filepath))
                        prefab_SDT = JsonSerializer.Deserialize<Prefab_SDT>(File.OpenRead(filepath), options);

                    tags = prefab_SDT.TagList;
                    break;
                case ProjectType.DS3:
                    var prefab_DS3 = new Prefab_DS3();
                    using (var stream = File.OpenRead(filepath))
                        prefab_DS3 = JsonSerializer.Deserialize<Prefab_DS3>(File.OpenRead(filepath), options);

                    tags = prefab_DS3.TagList;
                    break;
                case ProjectType.DS2S:
                    var prefab_DS2 = new Prefab_DS2();
                    using (var stream = File.OpenRead(filepath))
                        prefab_DS2 = JsonSerializer.Deserialize<Prefab_DS2>(File.OpenRead(filepath), options);

                    tags = prefab_DS2.TagList;
                    break;
                case ProjectType.DS1:
                case ProjectType.DS1R:
                    var prefab_DS1 = new Prefab_DS1();
                    using (var stream = File.OpenRead(filepath))
                        prefab_DS1 = JsonSerializer.Deserialize<Prefab_DS1>(File.OpenRead(filepath), options);

                    tags = prefab_DS1.TagList;
                    break;
                default: break;
            }

            return tags;
        }
    }
}
