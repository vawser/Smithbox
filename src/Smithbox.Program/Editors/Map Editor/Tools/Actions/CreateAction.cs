using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

public class CreateAction
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public (string, ObjectContainer) TargetMap = ("None", null);
    public (string, Entity) TargetBTL = ("None", null);

    public List<(string, Type)> EventClasses = new();
    public List<(string, Type)> PartsClasses = new();
    public List<(string, Type)> RegionClasses = new();

    public Type CreatePartSelectedType;
    public Type CreateRegionSelectedType;
    public Type CreateEventSelectedType;


    public CreateAction(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        PopulateClassNames();
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if (InputTracker.GetKeyDown(KeyBindings.Current.MAP_CreateMapObject) && Editor.ViewportSelection.IsSelection())
        {
            Editor.CreateAction.ApplyObjectCreation();
        }
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        if (ImGui.BeginMenu("Create New Object"))
        {
            DisplayMenu();

            ImGui.EndMenu();
        }
        UIHelper.Tooltip($"Create a new map object.");
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        if (ImGui.CollapsingHeader("Create"))
        {
            DisplayMenu();
        }
    }

    /// <summary>
    /// Menu
    /// </summary>
    public void DisplayMenu()
    {
        var windowSize = new Vector2(800f, 500f);
        var sectionWidth = ImGui.GetWindowWidth() * 0.95f;
        var sectionHeight = windowSize.Y * 0.25f;
        var sectionSize = new Vector2(sectionWidth * DPI.UIScale(), sectionHeight * DPI.UIScale());

        UIHelper.SimpleHeader("Target Map", "Target Map", "The target map to duplicate the current selection to.", UI.Current.ImGui_Default_Text_Color);

        ImGui.BeginChild("##mapSelectionSection", sectionSize, ImGuiChildFlags.Borders);

        foreach (var entry in Project.MapData.PrimaryBank.Maps)
        {
            var mapID = entry.Key.Filename;
            var map = entry.Value.MapContainer;

            if (map != null)
            {
                if (ImGui.Selectable(mapID, TargetMap.Item1 == mapID))
                {
                    TargetMap = (mapID, map);
                }

                var mapName = AliasHelper.GetMapNameAlias(Editor.Project, mapID);
                UIHelper.DisplayAlias(mapName);
            }
        }

        ImGui.EndChild();

        if (TargetMap.Item2 == null)
        {
            UIHelper.WrappedText("No map has been loaded or targeted.");
        }

        if (TargetMap != (null, null))
        {
            var map = (MapContainer)TargetMap.Item2;

            if (map != null)
            {
                UIHelper.SimpleHeader("Target Type", "Target Type", "", UI.Current.ImGui_Default_Text_Color);

                if (map.BTLParents.Any())
                {
                    if (ImGui.Checkbox("BTL Light", ref CFG.Current.Toolbar_Create_Light))
                    {
                        CFG.Current.Toolbar_Create_Part = false;
                        CFG.Current.Toolbar_Create_Region = false;
                        CFG.Current.Toolbar_Create_Event = false;
                    }
                    UIHelper.Tooltip("Create a BTL Light object.");
                }

                if (ImGui.Checkbox("Part", ref CFG.Current.Toolbar_Create_Part))
                {
                    CFG.Current.Toolbar_Create_Light = false;
                    CFG.Current.Toolbar_Create_Region = false;
                    CFG.Current.Toolbar_Create_Event = false;
                }
                UIHelper.Tooltip("Create a Part object.");

                if (ImGui.Checkbox("Region", ref CFG.Current.Toolbar_Create_Region))
                {
                    CFG.Current.Toolbar_Create_Light = false;
                    CFG.Current.Toolbar_Create_Part = false;
                    CFG.Current.Toolbar_Create_Event = false;
                }
                UIHelper.Tooltip("Create a Region object.");

                if (ImGui.Checkbox("Event", ref CFG.Current.Toolbar_Create_Event))
                {
                    CFG.Current.Toolbar_Create_Light = false;
                    CFG.Current.Toolbar_Create_Region = false;
                    CFG.Current.Toolbar_Create_Part = false;
                }
                UIHelper.Tooltip("Create an Event object.");
                UIHelper.WrappedText("");


                if (ImGui.Button("Create Object", DPI.WholeWidthButton(sectionWidth, 24)))
                {
                    ApplyObjectCreation();
                }

                UIHelper.WrappedText("");

                if (CFG.Current.Toolbar_Create_Light)
                {
                    // Nothing
                }

                if (CFG.Current.Toolbar_Create_Part)
                {
                    UIHelper.SimpleHeader("Part Type", "Part Type", "", UI.Current.ImGui_Default_Text_Color);

                    ImGui.BeginChild("msb_part_selection", sectionSize, ImGuiChildFlags.Borders);

                    foreach ((string, Type) p in PartsClasses)
                    {
                        if (ImGui.RadioButton(p.Item1, p.Item2 == CreatePartSelectedType))
                        {
                            CreatePartSelectedType = p.Item2;
                        }
                    }

                    ImGui.EndChild();
                }

                if (CFG.Current.Toolbar_Create_Region)
                {
                    // MSB format that only have 1 region type
                    if (RegionClasses.Count == 1)
                    {
                        CreateRegionSelectedType = RegionClasses[0].Item2;
                    }
                    else
                    {
                        UIHelper.SimpleHeader("Region Type", "Region Type", "", UI.Current.ImGui_Default_Text_Color);

                        ImGui.BeginChild("msb_region_selection", sectionSize, ImGuiChildFlags.Borders);

                        foreach ((string, Type) p in RegionClasses)
                        {
                            if (ImGui.RadioButton(p.Item1, p.Item2 == CreateRegionSelectedType))
                            {
                                CreateRegionSelectedType = p.Item2;
                            }
                        }

                        ImGui.EndChild();
                    }
                }

                if (CFG.Current.Toolbar_Create_Event)
                {
                    UIHelper.SimpleHeader("Event Type", "Event Type", "", UI.Current.ImGui_Default_Text_Color);

                    ImGui.BeginChild("msb_event_selection", sectionSize, ImGuiChildFlags.Borders);

                    foreach ((string, Type) p in EventClasses)
                    {
                        if (ImGui.RadioButton(p.Item1, p.Item2 == CreateEventSelectedType))
                        {
                            CreateEventSelectedType = p.Item2;
                        }
                    }

                    ImGui.EndChild();
                }
            }
        }
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void ApplyObjectCreation()
    {
        if (!Editor.Selection.IsAnyMapLoaded())
            return;

        if (TargetMap != (null, null))
        {
            var map = (MapContainer)TargetMap.Item2;

            if (CFG.Current.Toolbar_Create_Light)
            {
                foreach (Entity btl in map.BTLParents)
                {
                    AddNewEntity(typeof(BTL.Light), MsbEntityType.Light, map, btl);
                }
            }
            if (CFG.Current.Toolbar_Create_Part)
            {
                if (CreatePartSelectedType == null)
                    return;

                AddNewEntity(CreatePartSelectedType, MsbEntityType.Part, map);
            }
            if (CFG.Current.Toolbar_Create_Region)
            {
                if (CreateRegionSelectedType == null)
                    return;

                AddNewEntity(CreateRegionSelectedType, MsbEntityType.Region, map);
            }
            if (CFG.Current.Toolbar_Create_Event)
            {
                if (CreateEventSelectedType == null)
                    return;

                AddNewEntity(CreateEventSelectedType, MsbEntityType.Event, map);
            }
        }
    }

    private void AddNewEntity(Type typ, MsbEntityType etype, MapContainer map, Entity parent = null)
    {
        var newent = typ.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
        MsbEntity obj = new(Editor, map, newent, etype);

        parent ??= map.RootObject;

        AddMapObjectsAction act = new(Editor, map, new List<MsbEntity> { obj }, true, parent);
        Editor.EditorActionManager.ExecuteAction(act);
    }

    public void PopulateClassNames()
    {
        Type msbclass;
        switch (Editor.Project.ProjectType)
        {
            case ProjectType.DES:
                msbclass = typeof(MSBD);
                break;
            case ProjectType.DS1:
            case ProjectType.DS1R:
                msbclass = typeof(MSB1);
                break;
            case ProjectType.DS2:
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
            case ProjectType.ACFA:
                msbclass = typeof(MSBFA);
                break;
            case ProjectType.ACV:
                msbclass = typeof(MSBV);
                break;
            case ProjectType.ACVD:
                msbclass = typeof(MSBVD);
                break;
            case ProjectType.NR:
                msbclass = typeof(MSBE);
                break;
            default:
                throw new ArgumentException("type must be valid");
        }

        Type partType = msbclass.GetNestedType("Part");
        List<Type> partSubclasses = msbclass.Assembly.GetTypes()
            .Where(type => type.IsSubclassOf(partType) && !type.IsAbstract).ToList();
        PartsClasses = partSubclasses.Select(x => (x.Name, x)).ToList();

        Type regionType = msbclass.GetNestedType("Region");
        List<Type> regionSubclasses = msbclass.Assembly.GetTypes()
            .Where(type => type.IsSubclassOf(regionType) && !type.IsAbstract).ToList();
        RegionClasses = regionSubclasses.Select(x => (x.Name, x)).ToList();
        if (RegionClasses.Count == 0)
        {
            RegionClasses.Add(("Region", regionType));
        }

        Type eventType = msbclass.GetNestedType("Event");
        List<Type> eventSubclasses = msbclass.Assembly.GetTypes()
            .Where(type => type.IsSubclassOf(eventType) && !type.IsAbstract).ToList();
        EventClasses = eventSubclasses.Select(x => (x.Name, x)).ToList();
    }
}