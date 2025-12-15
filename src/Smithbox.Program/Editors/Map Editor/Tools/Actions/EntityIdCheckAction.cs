using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Generic;


namespace StudioCore.Editors.MapEditor;

public class EntityIdCheckAction
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public (string, ObjectContainer) TargetMap = ("None", null);

    public EntityIdCheckAction(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {

    }

    /// <summary>
    /// Context Menu
    /// </summary>
    public void OnContext()
    {
        // Not shown here
    }

    /// <summary>
    /// Edit Menu
    /// </summary>
    public void OnMenu()
    {
        // Not shown here
    }

    /// <summary>
    /// Tool Menu
    /// </summary>
    public void OnToolMenu()
    {
        if (ImGui.BeginMenu("Entity ID Check"))
        {
            if (Editor.Selection.IsAnyMapLoaded())
            {
                if (ImGui.BeginCombo("##Targeted Map", TargetMap.Item1))
                {
                    foreach (var entry in Editor.Project.MapData.PrimaryBank.Maps)
                    {
                        var mapID = entry.Key.Filename;
                        var container = entry.Value.MapContainer;

                        if (container != null)
                        {
                            if (ImGui.Selectable(mapID))
                            {
                                TargetMap = (mapID, container);
                                break;
                            }
                        }
                    }
                    ImGui.EndCombo();
                }

                if (ImGui.MenuItem("Check"))
                {
                    ApplyEntityChecker();
                }
            }

            ImGui.EndMenu();
        }
    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        // Not shown here
    }

    /// <summary>
    /// Effect
    /// </summary>
    public void ApplyEntityChecker()
    {
        if (!Editor.Selection.IsAnyMapLoaded())
            return;

        HashSet<uint> vals = new();
        bool hasError = false;

        if (TargetMap != (null, null))
        {
            var loadedMap = (MapContainer)TargetMap.Item2;

            // Entity ID
            foreach (var e in loadedMap?.Objects)
            {
                var val = PropFinderUtil.FindPropertyValue("EntityID", e.WrappedObject);

                if (val == null)
                    continue;

                uint entUint;

                if (val is int entInt)
                    entUint = (uint)entInt;
                else
                    entUint = (uint)val;

                if (entUint == 0 || entUint == uint.MaxValue)
                    continue;

                if (!vals.Add(entUint))
                {
                    vals.Add(entUint);

                    hasError = true;
                    TaskLogs.AddLog($"Duplicate Entity ID: {entUint.ToString()} in {e.Name}");
                }
            }

            // Entity Group ID
            foreach (var e in loadedMap?.Objects)
            {
                if (Editor.Project.ProjectType == ProjectType.AC6)
                {
                    if (e.WrappedObject is MSB_AC6.Part)
                    {
                        MSB_AC6.Part part = (MSB_AC6.Part)e.WrappedObject;

                        List<uint> checkedEntityGroups = new List<uint>();

                        for (int i = 0; i < part.EntityGroupIDs.Length; i++)
                        {
                            if (part.EntityGroupIDs[i] == 0)
                                continue;

                            if (checkedEntityGroups.Count > 0)
                            {
                                foreach (var group in checkedEntityGroups)
                                {
                                    if (part.EntityGroupIDs[i] == group)
                                    {
                                        hasError = true;
                                        TaskLogs.AddLog($"Duplicate Entity Group ID: {part.EntityGroupIDs[i].ToString()} in {e.Name}");
                                    }
                                }
                            }

                            checkedEntityGroups.Add(part.EntityGroupIDs[i]);
                        }
                    }
                }
                if (Editor.Project.ProjectType == ProjectType.ER)
                {
                    if (e.WrappedObject is MSBE.Part)
                    {
                        MSBE.Part part = (MSBE.Part)e.WrappedObject;

                        List<uint> checkedEntityGroups = new List<uint>();

                        for (int i = 0; i < part.EntityGroupIDs.Length; i++)
                        {
                            if (part.EntityGroupIDs[i] == 0)
                                continue;

                            if (checkedEntityGroups.Count > 0)
                            {
                                foreach (var group in checkedEntityGroups)
                                {
                                    if (part.EntityGroupIDs[i] == group)
                                    {
                                        hasError = true;
                                        TaskLogs.AddLog($"Duplicate Entity Group ID: {part.EntityGroupIDs[i].ToString()} in {e.Name}");
                                    }
                                }
                            }

                            checkedEntityGroups.Add(part.EntityGroupIDs[i]);
                        }
                    }
                }
                if (Editor.Project.ProjectType == ProjectType.SDT)
                {
                    if (e.WrappedObject is MSBS.Part)
                    {
                        MSBS.Part part = (MSBS.Part)e.WrappedObject;

                        List<int> checkedEntityGroups = new List<int>();

                        for (int i = 0; i < part.EntityGroupIDs.Length; i++)
                        {
                            if (part.EntityGroupIDs[i] == -1)
                                continue;

                            if (checkedEntityGroups.Count > 0)
                            {
                                foreach (var group in checkedEntityGroups)
                                {
                                    if (part.EntityGroupIDs[i] == group)
                                    {
                                        hasError = true;
                                        TaskLogs.AddLog($"Duplicate Entity Group ID: {part.EntityGroupIDs[i].ToString()} in {e.Name}");
                                    }
                                }
                            }

                            checkedEntityGroups.Add(part.EntityGroupIDs[i]);
                        }
                    }
                }
                if (Editor.Project.ProjectType == ProjectType.DS3)
                {
                    if (e.WrappedObject is MSB3.Part)
                    {
                        MSB3.Part part = (MSB3.Part)e.WrappedObject;

                        List<int> checkedEntityGroups = new List<int>();

                        for (int i = 0; i < part.EntityGroups.Length; i++)
                        {
                            if (part.EntityGroups[i] == -1)
                                continue;

                            if (checkedEntityGroups.Count > 0)
                            {
                                foreach (var group in checkedEntityGroups)
                                {
                                    if (part.EntityGroups[i] == group)
                                    {
                                        hasError = true;
                                        TaskLogs.AddLog($"Duplicate Entity Group ID: {part.EntityGroups[i].ToString()} in {e.Name}");
                                    }
                                }
                            }

                            checkedEntityGroups.Add(part.EntityGroups[i]);
                        }
                    }
                }
            }
        }

        if (!hasError)
        {
            TaskLogs.AddLog($"No errors found.");
        }
    }
}