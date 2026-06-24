using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

public class CreateAction
{
    public MapEditorView View;
    public ProjectEntry Project;

    public (string, ObjectContainer) TargetMap = ("None", null);
    public (string, Entity) TargetBTL = ("None", null);

    public List<(string, Type)> EventClasses = new();
    public List<(string, Type)> PartsClasses = new();
    public List<(string, Type)> RegionClasses = new();

    public Type CreatePartSelectedType;
    public Type CreateRegionSelectedType;
    public Type CreateEventSelectedType;

    public CreateTargetType TargetBaseType = CreateTargetType.Part;

    public CreateAction(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        PopulateClassNames();
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        if(View.ViewportSelection.IsSelection())
        {
            if (InputManager.IsPressed(KeybindID.MapEditor_Create_Map_Object))
            {
                ApplyObjectCreation();
            }
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
        DisplayMenu();
    }

    /// <summary>
    /// Menu
    /// </summary>
    public void DisplayMenu()
    {
        UIHelper.WrappedText("Use this to create a new map object within the target loaded map.");

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Target Map", "The target map to duplicate the current selection to.");

        UIHelper.SetInputWidth();
        if (ImGui.BeginCombo("##targetMapSelect", TargetMap.Item1))
        {
            foreach (var entry in Project.Handler.MapData.PrimaryBank.Maps)
            {
                var map = entry.Value.MapContainer;

                if (map == null)
                    continue;

                var mapID = entry.Key.Filename;
                var mapName = AliasHelper.GetMapNameAlias(View.Project, mapID);
                var displayName = $"{mapID}: {mapName}";

                if (ImGui.Selectable(displayName, TargetMap.Item1 == mapID))
                {
                    TargetMap = (mapID, map);
                }
            }

            ImGui.EndCombo();
        }

        if (TargetMap != (null, null))
        {
            DisplayBaseTypeSelection();
            DisplaySubTypeSelection();
            DisplayActions();
        }
    }

    public void DisplayBaseTypeSelection()
    {
        var map = (MapContainer)TargetMap.Item2;

        if (map == null)
            return;

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Base Type", "The base type of the map object to create.");

        UIHelper.SetInputWidth();
        if (ImGui.BeginCombo("##targetBaseType", TargetBaseType.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(CreateTargetType)))
            {
                var curType = (CreateTargetType)entry;

                if (ImGui.Selectable(curType.GetDisplayName(), TargetBaseType == curType))
                {
                    TargetBaseType = curType;

                    switch(TargetBaseType)
                    {
                        case CreateTargetType.Part:
                            CFG.Current.Toolbar_Create_Part = true;
                            CFG.Current.Toolbar_Create_Region = false;
                            CFG.Current.Toolbar_Create_Event = false;
                            CFG.Current.Toolbar_Create_Light = false;
                            break;
                        case CreateTargetType.Region:
                            CFG.Current.Toolbar_Create_Part = false;
                            CFG.Current.Toolbar_Create_Region = true;
                            CFG.Current.Toolbar_Create_Event = false;
                            CFG.Current.Toolbar_Create_Light = false;
                            break;
                        case CreateTargetType.Event:
                            CFG.Current.Toolbar_Create_Part = false;
                            CFG.Current.Toolbar_Create_Region = false;
                            CFG.Current.Toolbar_Create_Event = true;
                            CFG.Current.Toolbar_Create_Light = false;
                            break;
                        case CreateTargetType.Light:
                            CFG.Current.Toolbar_Create_Part = false;
                            CFG.Current.Toolbar_Create_Region = false;
                            CFG.Current.Toolbar_Create_Event = false;
                            CFG.Current.Toolbar_Create_Light = true;
                            break;
                    }
                }
            }

            ImGui.EndCombo();
        }
    }

    public void DisplaySubTypeSelection()
    {
        var map = (MapContainer)TargetMap.Item2;

        if (map == null)
            return;


        if (CFG.Current.Toolbar_Create_Light)
            return;

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Sub Type", "The sub type of the map object to create.");

        UIHelper.SetInputWidth();

        if (CFG.Current.Toolbar_Create_Part)
        {
            var previewName = "None";
            if(CreatePartSelectedType != null)
            {
                previewName = CreatePartSelectedType.Name;
            }

            if (ImGui.BeginCombo("##targetSubType_Part", previewName))
            {
                foreach ((string, Type) p in PartsClasses)
                {
                    var curType = (Type)p.Item2;

                    if (ImGui.Selectable(curType.Name, CreatePartSelectedType == curType))
                    {
                        CreatePartSelectedType = curType;
                    }
                }

                ImGui.EndCombo();
            }
        }
        else if (CFG.Current.Toolbar_Create_Region)
        {
            var previewName = "None";
            if (CreateRegionSelectedType != null)
            {
                previewName = CreateRegionSelectedType.Name;
            }

            if (ImGui.BeginCombo("##targetSubType_Region", previewName))
            {
                foreach ((string, Type) p in RegionClasses)
                {
                    var curType = (Type)p.Item2;

                    if (ImGui.Selectable(curType.Name, CreateRegionSelectedType == curType))
                    {
                        CreateRegionSelectedType = curType;
                    }
                }

                ImGui.EndCombo();
            }
        }
        else if (CFG.Current.Toolbar_Create_Event)
        {
            var previewName = "None";
            if (CreateEventSelectedType != null)
            {
                previewName = CreateEventSelectedType.Name;
            }

            if (ImGui.BeginCombo("##targetSubType_Event", previewName))
            {
                foreach ((string, Type) p in EventClasses)
                {
                    var curType = (Type)p.Item2;

                    if (ImGui.Selectable(curType.Name, CreateEventSelectedType == curType))
                    {
                        CreateEventSelectedType = curType;
                    }
                }

                ImGui.EndCombo();
            }
        }
    }

    public void DisplayActions()
    {
        var map = (MapContainer)TargetMap.Item2;

        if (map == null)
            return;

        UIHelper.Spacer();
        UIHelper.SimpleHeader("Actions", "");

        UIHelper.MultiButtonInput("createActions",
            "createObject", "Create Map Object", "", ApplyObjectCreation);
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void ApplyObjectCreation()
    {
        if (!View.Selection.IsAnyMapLoaded())
        {
            Smithbox.LogError<CreateAction>("No map has been loaded.");
            return;
        }

        if (TargetMap == (null, null))
        {
            Smithbox.LogError<CreateAction>("Selected map is not loaded.");
            return;
        }

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
            {
                Smithbox.LogError<CreateAction>("No part type has been selected.");
                return;
            }

            AddNewEntity(CreatePartSelectedType, MsbEntityType.Part, map);
        }
        if (CFG.Current.Toolbar_Create_Region)
        {
            if (CreateRegionSelectedType == null)
            {
                Smithbox.LogError<CreateAction>("No region type has been selected.");
                return;
            }

            AddNewEntity(CreateRegionSelectedType, MsbEntityType.Region, map);
        }
        if (CFG.Current.Toolbar_Create_Event)
        {
            if (CreateEventSelectedType == null)
            {
                Smithbox.LogError<CreateAction>("No event type has been selected.");
                return;
            }

            AddNewEntity(CreateEventSelectedType, MsbEntityType.Event, map);
        }

        View.DelayPicking();
    }

    private void AddNewEntity(Type typ, MsbEntityType etype, MapContainer map, Entity parent = null)
    {
        var newent = typ.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
        MsbEntity obj = new(View.Universe, map, newent, etype);

        parent ??= map.RootObject;

        EntAddAction act = new(View, map, new List<MsbEntity> { obj }, parent);
        View.ViewportActionManager.ExecuteAction(act);
    }

    public void PopulateClassNames()
    {
        Type msbclass;
        switch (View.Project.Descriptor.ProjectType)
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

public enum CreateTargetType
{
    [Display(Name = "Part")]
    Part,
    [Display(Name = "Region")]
    Region,
    [Display(Name = "Event")]
    Event,
    [Display(Name = "Light")]
    Light
}