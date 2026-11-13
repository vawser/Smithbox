using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;
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

    // Follows the active map unless another is picked
    public (string, ObjectContainer) TargetMap = MapTargetPicker.ActiveMap;
    public (string, Entity) TargetBTL = ("None", null);

    public List<(string, Type)> EventClasses = new();
    public List<(string, Type)> PartsClasses = new();
    public List<(string, Type)> RegionClasses = new();

    public Type CreatePartSelectedType;
    public Type CreateRegionSelectedType;
    public Type CreateEventSelectedType;

    public CreateTargetType TargetBaseType = CreateTargetType.Part;

    public MassCreateTool MassCreate;
    public CreateAtPointTool CreateAtPoint;

    public CreateAction(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        MassCreate = new MassCreateTool(view, project, this);
        CreateAtPoint = new CreateAtPointTool(view, project, this);

        // The type flags persist in the config, so the combo has to start on the one they select
        TargetBaseType = CFG.Current.Toolbar_Create_Light ? CreateTargetType.Light
            : CFG.Current.Toolbar_Create_Region ? CreateTargetType.Region
            : CFG.Current.Toolbar_Create_Event ? CreateTargetType.Event
            : CreateTargetType.Part;
        SetTargetBaseType(TargetBaseType);

        PopulateClassNames();
    }

    private static void SetTargetBaseType(CreateTargetType type)
    {
        CFG.Current.Toolbar_Create_Part = type == CreateTargetType.Part;
        CFG.Current.Toolbar_Create_Region = type == CreateTargetType.Region;
        CFG.Current.Toolbar_Create_Event = type == CreateTargetType.Event;
        CFG.Current.Toolbar_Create_Light = type == CreateTargetType.Light;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {
        // Creating needs a target map and type, not a selection
        if (InputManager.IsPressed(KeybindID.MapEditor_Create_Map_Object))
        {
            ApplyObjectCreation();
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
        GUI.Tooltip($"Create a new map object.");
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        if (ImGui.BeginTabBar("createActionTabs"))
        {
            if (ImGui.BeginTabItem($"{LOC.Get("MAP_MassCreate_Tab_Create")}##createActionTabSingle"))
            {
                DisplayMenu();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{LOC.Get("MAP_CreateAtPoint_Tab")}##createActionTabAtPoint"))
            {
                CreateAtPoint.Display();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{LOC.Get("MAP_MassCreate_Tab_Mass_Create")}##createActionTabMass"))
            {
                DisplayMassCreate();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }
    }

    private void DisplayMassCreate()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_MassCreate_Target_Map_Header"), LOC.Get("MAP_MassCreate_Target_Map_Header_TT"));

        MapTargetPicker.Display(View, "massCreateTargetMapSelect", ref TargetMap);

        var map = View.Selection.ResolveTargetMap(TargetMap);

        if (map == null)
        {
            GUI.WrappedText(LOC.Get("MAP_MassCreate_No_Map"));
            return;
        }

        MassCreate.Display(map);
    }

    /// <summary>
    /// Menu
    /// </summary>
    public void DisplayMenu()
    {
        GUI.WrappedText("Use this to create a new map object within the target loaded map.");

        GUI.Spacer();
        GUI.SimpleHeader("Target Map", "The map to create the new map object in. New objects are placed at the placement orb.");

        MapTargetPicker.Display(View, "targetMapSelect", ref TargetMap);

        if (View.Selection.ResolveTargetMap(TargetMap) != null)
        {
            DisplayBaseTypeSelection();
            DisplaySubTypeSelection();
            DisplayActions();
        }
    }

    public void DisplayBaseTypeSelection()
    {
        var map = View.Selection.ResolveTargetMap(TargetMap);

        if (map == null)
            return;

        GUI.Spacer();
        GUI.SimpleHeader("Base Type", "The base type of the map object to create.");

        GUI.SetInputWidth();
        if (ImGui.BeginCombo("##targetBaseType", TargetBaseType.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(CreateTargetType)))
            {
                var curType = (CreateTargetType)entry;

                if (ImGui.Selectable(curType.GetDisplayName(), TargetBaseType == curType))
                {
                    TargetBaseType = curType;
                    SetTargetBaseType(curType);
                }
            }

            ImGui.EndCombo();
        }
    }

    public void DisplaySubTypeSelection()
    {
        var map = View.Selection.ResolveTargetMap(TargetMap);

        if (map == null)
            return;


        if (TargetBaseType == CreateTargetType.Light)
        {
            DisplayBtlSelection(map);
            return;
        }

        GUI.Spacer();
        GUI.SimpleHeader("Sub Type", "The sub type of the map object to create.");

        GUI.SetInputWidth();

        if (TargetBaseType == CreateTargetType.Part)
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
        else if (TargetBaseType == CreateTargetType.Region)
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
        else if (TargetBaseType == CreateTargetType.Event)
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

    /// <summary>
    /// A BTL picked for an earlier target map belongs to that map, so it only counts if this map has it.
    /// A map with a single BTL needs no pick.
    /// </summary>
    private Entity ResolveTargetBtl(MapContainer map)
    {
        if (TargetBTL.Item2 != null && map.BTLParents.Contains(TargetBTL.Item2))
            return TargetBTL.Item2;

        return map.BTLParents.Count == 1 ? map.BTLParents[0] : null;
    }

    private void DisplayBtlSelection(MapContainer map)
    {
        GUI.Spacer();
        GUI.SimpleHeader("Target Lights", "The BTL file to create the new light in.");

        if (map.BTLParents.Count == 0)
        {
            GUI.WrappedText("This map has no BTL files loaded.");
            return;
        }

        var current = ResolveTargetBtl(map);

        GUI.SetInputWidth();
        if (ImGui.BeginCombo("##targetBtl", current != null ? (string)current.WrappedObject : "None"))
        {
            foreach (Entity btl in map.BTLParents)
            {
                var btlName = (string)btl.WrappedObject;
                if (ImGui.Selectable(btlName, btl == current))
                {
                    TargetBTL = (btlName, btl);
                }
            }

            ImGui.EndCombo();
        }
    }

    public void DisplayActions()
    {
        var map = View.Selection.ResolveTargetMap(TargetMap);

        if (map == null)
            return;

        GUI.Spacer();
        GUI.SimpleHeader("Actions", "");

        GUI.MultiButtonInput("createActions",
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

        var map = View.Selection.ResolveTargetMap(TargetMap);

        if (map == null)
        {
            Smithbox.LogError<CreateAction>("No loaded map has been targeted. Pick one in the Create tool first.");
            return;
        }

        switch (TargetBaseType)
        {
            case CreateTargetType.Light:
                var btl = ResolveTargetBtl(map);
                if (btl == null)
                {
                    Smithbox.LogError<CreateAction>(map.BTLParents.Count == 0
                        ? "The target map has no BTL files loaded."
                        : "No BTL has been targeted. Pick one in the Create tool first.");
                    return;
                }

                AddNewEntity(typeof(BTL.Light), MsbEntityType.Light, map, btl);
                break;

            case CreateTargetType.Part:
                if (CreatePartSelectedType == null)
                {
                    Smithbox.LogError<CreateAction>("No part type has been selected.");
                    return;
                }

                AddNewEntity(CreatePartSelectedType, MsbEntityType.Part, map);
                break;

            case CreateTargetType.Region:
                if (CreateRegionSelectedType == null)
                {
                    Smithbox.LogError<CreateAction>("No region type has been selected.");
                    return;
                }

                AddNewEntity(CreateRegionSelectedType, MsbEntityType.Region, map);
                break;

            case CreateTargetType.Event:
                if (CreateEventSelectedType == null)
                {
                    Smithbox.LogError<CreateAction>("No event type has been selected.");
                    return;
                }

                AddNewEntity(CreateEventSelectedType, MsbEntityType.Event, map);
                break;
        }

        View.DelayPicking();
    }

    private void AddNewEntity(Type typ, MsbEntityType etype, MapContainer map, Entity parent = null)
    {
        var newent = typ.GetConstructor(Type.EmptyTypes).Invoke(new object[0]);
        MsbEntity obj = new(View.Universe, map, newent, etype);

        // The type's default name is a placeholder (ER assets are all "AEGxxx_xxx_xxxx"), or empty, so each new
        // object is given one of its own in its list, as name references need
        if (obj.SupportsName)
        {
            var baseName = string.IsNullOrEmpty(obj.Name) ? typ.Name : obj.Name;
            obj.Name = MsbNaming.MakeUnique(baseName, MsbNaming.GetNames(map, newent));
        }

        parent ??= map.RootObject;

        PlaceAtPlacementOrb(newent, parent);

        EntAddAction act = new(View, map, new List<MsbEntity> { obj }, parent);
        View.ViewportActionManager.ExecuteAction(act);
    }

    /// <summary>
    /// Puts a new object at the placement orb rather than the map's origin. The orb is in world space, so the
    /// position is made local to the parent chain (which, for tiled maps, holds the map's offset).
    /// The object isn't in a map yet, so this needs no undo of its own.
    /// </summary>
    private void PlaceAtPlacementOrb(object wrapped, Entity parent)
    {
        var positionProp = wrapped.GetType().GetProperty("Position");

        if (positionProp == null || positionProp.PropertyType != typeof(Vector3) || !positionProp.CanWrite)
            return;

        if (View.ViewportHandler.ActiveViewport?.Viewport == null)
            return;

        var world = View.ViewportWindow.GetPlacementPosition();

        var root = Transform.Default;
        for (var p = parent; p != null; p = p.Parent)
        {
            root += p.GetLocalTransform();
        }

        var local = Vector3.Transform(world - root.Position, Quaternion.Inverse(root.Rotation));

        positionProp.SetValue(wrapped, local);
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
                msbclass = typeof(MSB_NR);
                break;
            case ProjectType.DB_NT:
                msbclass = typeof(MSB_DB_NT);
                break;
            case ProjectType.DB:
                PartsClasses = new();
                RegionClasses = new();
                EventClasses = new();
                return;
            default:
                throw new ArgumentException("type must be valid");
        }

        Type partType = msbclass.GetNestedType("Part");
        PartsClasses = SavableSubclasses(msbclass, partType, msb => msb.Parts.GetEntries(),
            (msb, e) => msb.Parts.Add((IMsbPart)e));

        Type regionType = msbclass.GetNestedType("Region");
        RegionClasses = SavableSubclasses(msbclass, regionType, msb => msb.Regions.GetEntries(),
            (msb, e) => msb.Regions.Add((IMsbRegion)e));
        if (RegionClasses.Count == 0)
        {
            RegionClasses.Add(("Region", regionType));
        }

        Type eventType = msbclass.GetNestedType("Event");
        EventClasses = SavableSubclasses(msbclass, eventType, msb => msb.Events.GetEntries(),
            (msb, e) => msb.Events.Add((IMsbEvent)e));
    }

    /// <summary>
    /// The subclasses of an MSB entry type that the MSB can save. Some formats declare types their params do not
    /// handle (NR has regions its Add rejects, and an event its Add takes but never writes), so each type is
    /// added to an empty MSB and kept only if it can be built and comes back out of GetEntries.
    /// </summary>
    private static List<(string, Type)> SavableSubclasses<T>(Type msbclass, Type baseType,
        Func<IMsb, IEnumerable<T>> getEntries, Action<IMsb, object> add) where T : IMsbEntry
    {
        var types = msbclass.Assembly.GetTypes()
            .Where(type => type.IsSubclassOf(baseType) && !type.IsAbstract).ToList();

        IMsb probe = null;
        try
        {
            probe = Activator.CreateInstance(msbclass) as IMsb;
        }
        catch { }

        // Without a probe MSB nothing can be checked, so everything is offered as before
        if (probe == null)
            return types.Select(x => (x.Name, x)).ToList();

        var result = new List<(string, Type)>();
        foreach (var type in types)
        {
            try
            {
                var entry = type.GetConstructor(Type.EmptyTypes)?.Invoke(null);
                if (entry == null)
                    continue;

                add(probe, entry);

                if (getEntries(probe).Any(x => ReferenceEquals(x, entry)))
                {
                    result.Add((type.Name, type));
                }
            }
            catch { }
        }

        return result;
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