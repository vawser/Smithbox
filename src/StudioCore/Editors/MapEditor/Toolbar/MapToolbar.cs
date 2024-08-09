using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using ImGuiNET;
using CommunityToolkit.HighPerformance;
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
using StudioCore.Banks;
using StudioCore.MsbEditor;
using StudioCore.Platform;
using StudioCore.Editors.MapEditor.Prefabs;
using System.Text.Json.Serialization.Metadata;
using System.Text.Json;
using StudioCore.Editor;
using StudioCore.Core;
using StudioCore.Locators;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public class MapToolbar
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

        public MapToolbar(RenderScene scene, ViewportSelection sel, ViewportActionManager manager, Universe universe, IViewport viewport, (string, ObjectContainer) comboTargetMap)
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
            if (!(Smithbox.ProjectType is ProjectType.ER or ProjectType.DS3 or ProjectType.SDT or ProjectType.DS2S or ProjectType.DS2 or ProjectType.DS1 or ProjectType.DS1R or ProjectType.AC6))
                return false;

            return true;
        }

        public void OnProjectChanged()
        {
            if (Smithbox.ProjectType == ProjectType.Undefined)
                return;

            _selectedPrefabObjectNames = new List<string>();
            _prefabInfos = new List<PrefabInfo>();
            _selectedPrefabInfo = null;
            _selectedPrefabInfoCache = null;
            _comboTargetMap = ("", null);
            _newPrefabName = "";
            _prefabTags = "";

            _prefabDir = $"{Smithbox.ProjectRoot}\\.smithbox\\{MiscLocator.GetGameIDForDir()}\\prefabs\\";

            if (!Directory.Exists(_prefabDir))
            {
                try
                {
                    Directory.CreateDirectory(_prefabDir);
                }
                catch { }
            }

            RefreshPrefabList();
        }

        public static void RefreshPrefabList()
        {
            _prefabInfos = GetPrefabList();
        }

        public static List<PrefabInfo> GetPrefabList()
        {
            List<PrefabInfo> infoList = new();

            if (Directory.Exists(_prefabDir))
            {
                string[] files = Directory.GetFiles(_prefabDir, "*.json", SearchOption.AllDirectories);
                foreach (var file in files)
                {
                    var name = Path.GetFileNameWithoutExtension(file);
                    PrefabInfo info = new PrefabInfo(name, file, GetPrefabTags(file));
                    infoList.Add(info);
                }
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

            switch (Smithbox.ProjectType)
            {
                case ProjectType.AC6:
                    var prefab_AC6 = new Prefab_AC6();
                    using (var stream = File.OpenRead(filepath))
                        prefab_AC6 = JsonSerializer.Deserialize<Prefab_AC6>(File.OpenRead(filepath), options);

                    tags = prefab_AC6.TagList;
                    break;
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
                case ProjectType.DS2:
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
