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
        }
    }
}
