using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StudioCore.Editors.MapEditor;
public class EntityRenameAction
{
    public MapEditorScreen Editor;
    public ProjectEntry Project;

    public (string, ObjectContainer) TargetMap = ("None", null);

    public EntityRenameAction(MapEditorScreen editor, ProjectEntry project)
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
        if (ImGui.BeginMenu("Rename Map Objects"))
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

                if (ImGui.MenuItem("Apply Japanese Names"))
                {
                    DialogResult result = PlatformUtils.Instance.MessageBox(
                    $"This will apply the developer map object names (in Japanese) for this map.\nNote, this will not work if you have edited the map as the name list is based on the index of the map object",
                    "Warning",
                    MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        ApplyMapObjectNames(true);
                    }
                }

                if (ImGui.MenuItem("Apply English Names"))
                {
                    DialogResult result = PlatformUtils.Instance.MessageBox(
                    $"This will apply the developer map object names (in machine translated English) for this map.\nNote, this will not work if you have edited the map as the name list is based on the index of the map object",
                    "Warning",
                    MessageBoxButtons.YesNo);

                    if (result == DialogResult.Yes)
                    {
                        ApplyMapObjectNames(false);
                    }
                }
            }

            ImGui.EndMenu();
        }
        UIHelper.Tooltip("Applies descriptive name for map objects from developer name list.");
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
    public void ApplyMapObjectNames(bool useJapaneseNames)
    {
        if (!Editor.Selection.IsAnyMapLoaded())
            return;

        if (TargetMap != (null, null))
        {
            var actionList = new List<ViewportAction>();

            var loadedMap = (MapContainer)TargetMap.Item2;
            var mapid = TargetMap.Item1;

            List<MapObjectNameInfo> partNames = GetNameListInfo(mapid, "Part");

            // Parts
            for (int i = 0; i < loadedMap.Parts.Count; i++)
            {
                var ent = loadedMap.Parts[i];

                var name = partNames.Where(e => e.Index == i).FirstOrDefault();

                if (name != null)
                {
                    var newName = name.EnglishName;

                    if (useJapaneseNames)
                    {
                        newName = name.JapaneseName;
                    }

                    actionList.Add(new RenameObjectsAction(
                        new List<MsbEntity> { ent as MsbEntity },
                        new List<string> { $"{ent.Name} -- {newName}" },
                        true
                    ));
                }
            }

            List<MapObjectNameInfo> regionNames = GetNameListInfo(mapid, "Point");

            // Regions
            for (int i = 0; i < loadedMap.Regions.Count; i++)
            {
                var ent = loadedMap.Regions[i];

                var name = regionNames.Where(e => e.Index == i).FirstOrDefault();

                if (name != null)
                {
                    var newName = name.EnglishName;

                    if (useJapaneseNames)
                    {
                        newName = name.JapaneseName;
                    }

                    actionList.Add(new RenameObjectsAction(
                        new List<MsbEntity> { ent as MsbEntity },
                        new List<string> { $"{ent.Name} -- {newName}" },
                        true
                    ));
                }
            }

            List<MapObjectNameInfo> eventNames = GetNameListInfo(mapid, "Event");

            // Events
            for (int i = 0; i < loadedMap.Events.Count; i++)
            {
                var ent = loadedMap.Events[i];

                var name = eventNames.Where(e => e.Index == i).FirstOrDefault();

                if (name != null)
                {
                    var newName = name.EnglishName;

                    if (useJapaneseNames)
                    {
                        newName = name.JapaneseName;
                    }

                    actionList.Add(new RenameObjectsAction(
                        new List<MsbEntity> { ent as MsbEntity },
                        new List<string> { $"{ent.Name} -- {newName}" },
                        true
                    ));
                }
            }

            var compoundAction = new ViewportCompoundAction(actionList);
            Editor.EditorActionManager.ExecuteAction(compoundAction);
        }
    }

    private List<MapObjectNameInfo> GetNameListInfo(string mapId, string type)
    {
        var list = new List<MapObjectNameInfo>();

        var dir = Path.Join(AppContext.BaseDirectory, "Assets", "MSB", ProjectUtils.GetGameDirectory(Editor.Project), "namelist.csv");

        if (File.Exists(dir))
        {
            var file = File.ReadAllLines(dir);

            for (int i = 0; i < file.Length; i++)
            {
                var entry = file[i];

                if (i == 0)
                    continue;

                var line = entry.Split(",");

                var curMapId = line[0];
                var typeName = line[1];
                var index = line[2];
                var japaneseName = line[3];
                var translatedName = line[4];

                if (mapId == curMapId && typeName == type)
                {
                    var newInfo = new MapObjectNameInfo();
                    newInfo.Index = int.Parse(index);
                    newInfo.JapaneseName = japaneseName;
                    newInfo.EnglishName = translatedName;

                    list.Add(newInfo);
                }
            }
        }

        return list;
    }
}

public class MapObjectNameInfo
{
    public int Index { get; set; }
    public string JapaneseName { get; set; }
    public string EnglishName { get; set; }

    public MapObjectNameInfo() { }
}