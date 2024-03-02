using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.UserProject;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.MSBB.Event.ObjAct;

namespace StudioCore.Editors.MapEditor.Toolbar
{
    public static class MapAction_Create
    {
        private static Type _createPartSelectedType;
        private static Type _createRegionSelectedType;
        private static Type _createEventSelectedType;

        private static int _createEntityMapIndex;

        private static List<(string, Type)> _eventClasses = new();
        private static List<(string, Type)> _partsClasses = new();
        private static List<(string, Type)> _regionClasses = new();

        public static void Select(ViewportSelection _selection)
        {
            if (ImGui.Selectable("Create##tool_Selection_Create", false, ImGuiSelectableFlags.AllowDoubleClick))
            {
                MapEditorState.CurrentTool = SelectedTool.Selection_Create;

                if (ImGui.IsMouseDoubleClicked(0) && _selection.IsSelection())
                {
                    Act(_selection);
                }
            }
        }

        public static void Configure(ViewportSelection _selection)
        {
            if (MapEditorState.CurrentTool == SelectedTool.Selection_Create)
            {
                ImGui.Text("Create a new object within the target map.");
                ImGui.Separator();
                ImGui.Text($"Shortcut: {ImguiUtils.GetKeybindHint(KeyBindings.Current.Toolbar_Create.HintText)}");
                ImGui.Separator();

                if (!MapEditorState.LoadedMaps.Any())
                {
                    ImGui.Text("No maps have been loaded yet.");
                }
                else
                {
                    var map = (Map)MapEditorState.LoadedMaps.ElementAt(_createEntityMapIndex);

                    ImGui.Combo("Target Map", ref _createEntityMapIndex, MapEditorState.LoadedMaps.Select(e => e.Name).ToArray(), MapEditorState.LoadedMaps.Count());

                    if (map.BTLParents.Any())
                    {
                        if (ImGui.Checkbox("BTL Light", ref CFG.Current.Toolbar_Create_Light))
                        {
                            CFG.Current.Toolbar_Create_Part = false;
                            CFG.Current.Toolbar_Create_Region = false;
                            CFG.Current.Toolbar_Create_Event = false;
                        }
                        ImguiUtils.ShowHoverTooltip("Create a BTL Light object.");
                    }

                    if (ImGui.Checkbox("Part", ref CFG.Current.Toolbar_Create_Part))
                    {
                        CFG.Current.Toolbar_Create_Light = false;
                        CFG.Current.Toolbar_Create_Region = false;
                        CFG.Current.Toolbar_Create_Event = false;
                    }
                    ImguiUtils.ShowHoverTooltip("Create a Part object.");

                    if (ImGui.Checkbox("Region", ref CFG.Current.Toolbar_Create_Region))
                    {
                        CFG.Current.Toolbar_Create_Light = false;
                        CFG.Current.Toolbar_Create_Part = false;
                        CFG.Current.Toolbar_Create_Event = false;
                    }
                    ImguiUtils.ShowHoverTooltip("Create a Region object.");

                    if (ImGui.Checkbox("Event", ref CFG.Current.Toolbar_Create_Event))
                    {
                        CFG.Current.Toolbar_Create_Light = false;
                        CFG.Current.Toolbar_Create_Region = false;
                        CFG.Current.Toolbar_Create_Part = false;
                    }
                    ImguiUtils.ShowHoverTooltip("Create an Event object.");

                    ImGui.Separator();

                    if (CFG.Current.Toolbar_Create_Light)
                    {
                        // Nothing
                    }

                    if (CFG.Current.Toolbar_Create_Part)
                    {
                        ImGui.Text("Part Type:");
                        ImGui.Separator();
                        ImGui.BeginChild("msb_part_selection");

                        foreach ((string, Type) p in _partsClasses)
                        {
                            if (ImGui.Selectable(p.Item1, p.Item2 == _createPartSelectedType))
                            {
                                _createPartSelectedType = p.Item2;
                            }
                        }

                        ImGui.EndChild();
                    }

                    if (CFG.Current.Toolbar_Create_Region)
                    {
                        // MSB format that only have 1 region type
                        if (_regionClasses.Count == 1)
                        {
                            _createRegionSelectedType = _regionClasses[0].Item2;
                        }
                        else
                        {
                            ImGui.Text("Region Type:");
                            ImGui.Separator();
                            ImGui.BeginChild("msb_region_selection");

                            foreach ((string, Type) p in _regionClasses)
                            {
                                if (ImGui.Selectable(p.Item1, p.Item2 == _createRegionSelectedType))
                                {
                                    _createRegionSelectedType = p.Item2;
                                }
                            }

                            ImGui.EndChild();
                        }
                    }

                    if (CFG.Current.Toolbar_Create_Event)
                    {
                        ImGui.Text("Event Type:");
                        ImGui.Separator();
                        ImGui.BeginChild("msb_event_selection");

                        foreach ((string, Type) p in _eventClasses)
                        {
                            if (ImGui.Selectable(p.Item1, p.Item2 == _createEventSelectedType))
                            {
                                _createEventSelectedType = p.Item2;
                            }
                        }

                        ImGui.EndChild();
                    }

                    ImGui.Separator();
                }
            }
        }

        public static void Act(ViewportSelection _selection)
        {
            var map = (Map)MapEditorState.LoadedMaps.ElementAt(_createEntityMapIndex);

            if (CFG.Current.Toolbar_Create_Light)
            {
                foreach (Entity btl in map.BTLParents)
                {
                    AddNewEntity(typeof(BTL.Light), MsbEntity.MsbEntityType.Light, map, btl);
                }
            }
            if (CFG.Current.Toolbar_Create_Part)
            {
                AddNewEntity(_createPartSelectedType, MsbEntity.MsbEntityType.Part, map);
            }
            if (CFG.Current.Toolbar_Create_Region)
            {
                AddNewEntity(_createRegionSelectedType, MsbEntity.MsbEntityType.Region, map);
            }
            if (CFG.Current.Toolbar_Create_Event)
            {
                AddNewEntity(_createEventSelectedType, MsbEntity.MsbEntityType.Event, map);
            }
        }

        /// <summary>
        /// Adds a new entity to the targeted map. If no parent is specified, RootObject will be used.
        /// </summary>
        private static void AddNewEntity(Type typ, MsbEntity.MsbEntityType etype, Map map, Entity parent = null)
        {
            var newent = typ.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
            MsbEntity obj = new(map, newent, etype);
            parent ??= map.RootObject;

            AddMapObjectsAction act = new(MapEditorState.Universe, map, MapEditorState.Scene, new List<MsbEntity> { obj }, true, parent);
            MapEditorState.ActionManager.ExecuteAction(act);
        }

        public static void PopulateClassNames()
        {
            Type msbclass;
            switch (Project.Type)
            {
                case ProjectType.DES:
                    msbclass = typeof(MSBD);
                    break;
                case ProjectType.DS1:
                case ProjectType.DS1R:
                    msbclass = typeof(MSB1);
                    break;
                case ProjectType.DS2S:
                    msbclass = typeof(MSB2);
                    break;
                case ProjectType.DS3:
                    msbclass = typeof(MSB3);
                    break;
                case ProjectType.BB:
                    msbclass = typeof(MSBB);
                    break;
                case ProjectType.SDT:
                    msbclass = typeof(MSBS);
                    break;
                case ProjectType.ER:
                    msbclass = typeof(MSBE);
                    break;
                case ProjectType.AC6:
                    msbclass = typeof(MSB_AC6);
                    break;
                default:
                    throw new ArgumentException("type must be valid");
            }

            Type partType = msbclass.GetNestedType("Part");
            List<Type> partSubclasses = msbclass.Assembly.GetTypes()
                .Where(type => type.IsSubclassOf(partType) && !type.IsAbstract).ToList();
            _partsClasses = partSubclasses.Select(x => (x.Name, x)).ToList();

            Type regionType = msbclass.GetNestedType("Region");
            List<Type> regionSubclasses = msbclass.Assembly.GetTypes()
                .Where(type => type.IsSubclassOf(regionType) && !type.IsAbstract).ToList();
            _regionClasses = regionSubclasses.Select(x => (x.Name, x)).ToList();
            if (_regionClasses.Count == 0)
            {
                _regionClasses.Add(("Region", regionType));
            }

            Type eventType = msbclass.GetNestedType("Event");
            List<Type> eventSubclasses = msbclass.Assembly.GetTypes()
                .Where(type => type.IsSubclassOf(eventType) && !type.IsAbstract).ToList();
            _eventClasses = eventSubclasses.Select(x => (x.Name, x)).ToList();
        }
    }
}
