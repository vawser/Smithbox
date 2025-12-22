using Andre.Formats;
using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public class MapValidatorTool
{
    private MapEditorScreen Editor;
    private ProjectEntry Project;

    private List<MapValidationEntry> ValidationEntries;

    private bool FirstValidate = false;

    public MapValidatorTool(MapEditorScreen screen, ProjectEntry project)
    {
        Editor = screen;
        Project = project;

        ValidationEntries = new();
    }

    public void OnToolWindow()
    {
        if (ImGui.CollapsingHeader("Map Validator"))
        {
            var windowWidth = ImGui.GetWindowWidth();

            var windowSize = DPI.GetWindowSize(Editor.BaseEditor._context);
            var sectionWidth = ImGui.GetWindowWidth() * 0.95f;
            var sectionHeight = windowSize.Y * 0.25f;
            var sectionSize = new Vector2(sectionWidth * DPI.UIScale(), sectionHeight * DPI.UIScale());

            UIHelper.WrappedText("Validate the currently loaded map.");
            UIHelper.WrappedText("");

            if (ImGui.Button("Validate", DPI.WholeWidthButton(sectionWidth, 24)))
            {
                ValidationEntries = new();

                var mapContainer = Editor.Selection.SelectedMapContainer;

                if(mapContainer != null)
                {
                    // Entity ID
                    ValidateEntityID(mapContainer);

                    // Collision Name
                    if (Project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
                    {
                        ValidateReferenceProperty(mapContainer, "CollisionPartName", MapValidationType.CollisionName, true);

                        ValidateReferenceProperty(mapContainer, "CollisionName", MapValidationType.CollisionName, true);
                    }
                    else
                    {
                        ValidateReferenceProperty(mapContainer, "CollisionName", MapValidationType.CollisionName, true);

                        ValidateReferenceProperty(mapContainer, "UnkHitName", MapValidationType.CollisionName, true);
                    }

                    // Walk Route
                    ValidateReferenceProperty(mapContainer, "WalkRouteName", 
                        MapValidationType.WalkRoute, true);

                    // Part Names
                    if (Project.ProjectType is ProjectType.ER or ProjectType.AC6 or ProjectType.NR)
                    {
                        ValidateStringArrayReferenceProperty(mapContainer, "PartNames", MapValidationType.PartNames, true);

                        ValidateReferenceProperty(mapContainer, "UnkT54PartName",
                            MapValidationType.PartNames, true);
                    }

                    // Params
                    ValidateParamProperty(mapContainer, "NpcThinkParam", "ThinkParamID",
                    MapValidationType.ThinkParamID, [-1]);

                    ValidateParamProperty(mapContainer, "NpcParam", "NPCParamID",
                        MapValidationType.NPCParamID, [-1]);

                    ValidateParamProperty(mapContainer, "CharaInitParam", "CharaInitID",
                        MapValidationType.CharaInitID, [-1]);

                    ValidateParamProperty(mapContainer, "MapMimicryEstablishmentParam", "ChameleonParamID",
                        MapValidationType.ChameleonParamID, [-1]);
                }

                FirstValidate = true;
            }

            if (ValidationEntries.Count > 0)
            {
                ImGui.BeginChild("ValidationTabs");

                ImGui.BeginTabBar("##mapValidationTabBar");

                DisplayEntityID();

                DisplayCommonMatches(
                    "Collision Name", "collisionName", 
                    "refers to a collision name that doesn't exist:",
                    MapValidationType.CollisionName);

                DisplayCommonMatches(
                    "Walk Route", "walkRoute",
                    "refers to a walk route name that doesn't exist:",
                    MapValidationType.WalkRoute);

                DisplayCommonMatches(
                    "Part Name", "partName",
                    "refers to a part name entry that doesn't exist:",
                    MapValidationType.PartNames);

                DisplayCommonMatches(
                    "NPC Think", "npcThink",
                    "refers to a NpcThinkParam entry that doesn't exist:",
                    MapValidationType.ThinkParamID);

                DisplayCommonMatches(
                    "NPC Param", "npcParam",
                    "refers to a NpcParam entry that doesn't exist:",
                    MapValidationType.NPCParamID);

                DisplayCommonMatches(
                    "Character Init", "charaInit",
                    "refers to a CharaInitParam entry that doesn't exist:",
                    MapValidationType.CharaInitID);

                DisplayCommonMatches(
                    "Chameleon Param", "chameleonParam",
                    "refers to a MapMimicryEstablishmentParam entry that doesn't exist:",
                    MapValidationType.ChameleonParamID);

                ImGui.EndTabBar();

                ImGui.EndChild();
            }
            else if(FirstValidate && ValidationEntries.Count == 0)
            {
                UIHelper.WrappedText("No issues found.");
            }
        }
    }

    public void DisplayEntityID()
    {
        if (!ValidationEntries.Any(e => e.Type is MapValidationType.EntityID))
            return;

        if (ImGui.BeginTabItem("Entity ID##entityIdValidationList"))
        {
            var entries = ValidationEntries.ToLookup(e => e.Type is MapValidationType.EntityID);

            int i = 0;
            foreach (var entry in entries[true])
            {
                var message = $"{entry.Name} has a duplicate entity ID of {entry.Value}";

                if(ImGui.Selectable($"{message}##validationEntry_entityID_{i}"))
                {
                    FocusEntry(entry);
                }

                i++;
            }

            ImGui.EndTabItem();
        }
    }

    public void DisplayCommonMatches(string title, string id, string msg, MapValidationType validationType)
    {
        if (!ValidationEntries.Any(e => e.Type == validationType))
            return;

        if (ImGui.BeginTabItem($"{title}##{id}ValidationList"))
        {
            var entries = ValidationEntries.ToLookup(e => e.Type == validationType);

            int i = 0;
            foreach (var entry in entries[true])
            {
                var message = $"{entry.Name} {msg} {entry.Value}";

                if (ImGui.Selectable($"{message}##{id}_validationEntry_{i}"))
                {
                    FocusEntry(entry);
                }

                i++;
            }

            ImGui.EndTabItem();
        }
    }

    private void FocusEntry(MapValidationEntry entry)
    {
        Editor.ViewportSelection.ClearSelection(Editor);
        Editor.MapViewportView.Viewport.FramePosition(entry.AssociatedEntity.GetLocalTransform().Position, 10f);
        Editor.ViewportSelection.AddSelection(Editor, entry.AssociatedEntity);
    }

    public void ValidateEntityID(MapContainer map)
    {
        Dictionary<int, string> entityIDList = new();
        foreach (Entity obj in map.Objects)
        {
            var objType = obj.WrappedObject.GetType().ToString();
            PropertyInfo entityIdProperty = obj.GetProperty("EntityID");

            if (entityIdProperty != null)
            {
                var idObj = entityIdProperty.GetValue(obj.WrappedObject);
                if (idObj is not int entityID)
                {
                    // EntityID is uint in Elden Ring. Only <2^31 is used in practice.
                    // If really desired, a separate routine could be created.
                    if (idObj is uint uID)
                    {
                        entityID = unchecked((int)uID);
                    }
                    else
                    {
                        continue;
                    }
                }

                if (entityID > 0)
                {
                    var entryExists = entityIDList.TryGetValue(entityID, out var name);
                    if (entryExists)
                    {
                        var validationEntry = new MapValidationEntry();
                        validationEntry.AssociatedEntity = obj;
                        validationEntry.Type = MapValidationType.EntityID;
                        validationEntry.Name = obj.Name;
                        validationEntry.Value = $"{entityID}";

                        ValidationEntries.Add(validationEntry);
                    }
                    else
                    {
                        entityIDList.Add(entityID, obj.PrettyName);
                    }
                }
            }
        }
    }

    public void ValidateReferenceProperty(MapContainer map, string propName, MapValidationType type, bool ignoreEmpty = false)
    {
        foreach (Entity obj in map.Objects)
        {
            var prop = PropFinderUtil.FindProperty(propName, obj.WrappedObject);

            if (prop == null)
                continue;

            var entResult = (string)PropFinderUtil.FindPropertyValue(prop, obj.WrappedObject);

            if (ignoreEmpty)
            {
                if (entResult == null || entResult == "")
                    continue;
            }

            var colNameEnt = map.GetObjectByName(entResult);

            if (colNameEnt == null)
            {
                var validationEntry = new MapValidationEntry();
                validationEntry.AssociatedEntity = obj;
                validationEntry.Type = type;
                validationEntry.Name = obj.Name;
                validationEntry.Value = $"{entResult}";

                ValidationEntries.Add(validationEntry);
            }
        }
    }

    public void ValidateStringArrayReferenceProperty(MapContainer map, string propName, MapValidationType type, bool ignoreEmpty = false)
    {
        foreach (Entity obj in map.Objects)
        {
            var prop = PropFinderUtil.FindProperty(propName, obj.WrappedObject);

            if (prop == null)
                continue;

            var entResult = (string[])PropFinderUtil.FindPropertyValue(prop, obj.WrappedObject);

            foreach (var entry in entResult)
            {
                if (ignoreEmpty)
                {
                    if (entry == null || entry == "")
                        continue;
                }

                var colNameEnt = map.GetObjectByName(entry);

                if (colNameEnt == null)
                {
                    var validationEntry = new MapValidationEntry();
                    validationEntry.AssociatedEntity = obj;
                    validationEntry.Type = type;
                    validationEntry.Name = obj.Name;
                    validationEntry.Value = $"{entry}";

                    ValidationEntries.Add(validationEntry);
                }
            }
        }
    }

    public void ValidateParamProperty(MapContainer map, string paramName, string propName, MapValidationType type, int[] defaultValues)
    {
        if (Project.ParamEditor == null)
            return;

        if (!Project.ParamData.PrimaryBank.Params.ContainsKey(paramName))
            return;


        foreach (Entity obj in map.Objects)
        {
            var prop = PropFinderUtil.FindProperty(propName, obj.WrappedObject);

            if (prop == null)
                continue;

            var entResult = PropFinderUtil.FindPropertyValue(prop, obj.WrappedObject);

            var success = int.TryParse(entResult.ToString(), out var id);

            if(success)
            {
                var skip = false;
                for (int i = 0; i < defaultValues.Length; i++)
                {
                    var defaultValue = defaultValues[i];

                    if (id == defaultValue)
                    {
                        skip = true;
                    }
                }

                if (skip)
                {
                    continue;
                }

                if (!Project.ParamData.PrimaryBank.Params[paramName].ContainsRow(id))
                {
                    var validationEntry = new MapValidationEntry();
                    validationEntry.AssociatedEntity = obj;
                    validationEntry.Type = type;
                    validationEntry.Name = obj.Name;
                    validationEntry.Value = $"{entResult}";

                    ValidationEntries.Add(validationEntry);
                }
            }
        }
    }
}

public class MapValidationEntry
{
    public Entity AssociatedEntity { get; set; }

    public MapValidationType Type { get; set; }

    public string Name { get; set; }

    public string Value { get; set; }

    public MapValidationEntry() { }
}
