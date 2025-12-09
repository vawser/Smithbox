using SoulsFormats;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hexa.NET.ImGui;
using System.Text.RegularExpressions;
using StudioCore.Editor;
using StudioCore.Utilities;
using StudioCore.Interface;
using StudioCore.Resource.Locators;
using StudioCore.Core;

namespace StudioCore.Editors.ModelEditor.Utils;

public class GlobalModelSearch
{
    private ModelEditorScreen Editor;

    public string _searchInput = "";
    public List<MapModelMatch> Matches = new List<MapModelMatch>();

    public List<ResourceDescriptor> resMaps = new List<ResourceDescriptor>();
    public bool _targetProjectFiles = false;
    public bool _looseModelNameMatch = false;

    private bool SetupSearch = true;

    public Dictionary<string, IMsb> MapList = new Dictionary<string, IMsb>();

    public GlobalModelSearch(ModelEditorScreen screen)
    {
        Editor = screen;
    }

    public void SearchMaps()
    {
        if (SetupSearch)
        {
            SetupSearch = false;

            var maps = Editor.Project.MapData.MapFiles;

            switch (Editor.Project.ProjectType)
            {
                case ProjectType.DES:
                    foreach (var entry in maps.Entries)
                    {
                        try
                        {
                            var msbData = Editor.Project.FS.ReadFile(entry.Path);
                            var msb = MSBD.Read(msbData.Value);

                            if (!MapList.ContainsKey(entry.Filename))
                                MapList.Add(entry.Filename, msb);
                        }
                        catch(Exception e)
                        {
                            TaskLogs.AddLog($"[Smithbox] Failed to read MSB: {entry.Path}");
                        }
                    }
                    break;
                case ProjectType.DS1:
                case ProjectType.DS1R:
                    foreach (var entry in maps.Entries)
                    {
                        try
                        {
                            var msbData = Editor.Project.FS.ReadFile(entry.Path);
                            var msb = MSB1.Read(msbData.Value);

                            if (!MapList.ContainsKey(entry.Filename))
                                MapList.Add(entry.Filename, msb);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[Smithbox] Failed to read MSB: {entry.Path}");
                        }
                    }
                    break;
                case ProjectType.DS2:
                case ProjectType.DS2S:
                    foreach (var entry in maps.Entries)
                    {
                        try
                        {
                            var msbData = Editor.Project.FS.ReadFile(entry.Path);
                            var msb = MSB2.Read(msbData.Value);

                            if (!MapList.ContainsKey(entry.Filename))
                                MapList.Add(entry.Filename, msb);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[Smithbox] Failed to read MSB: {entry.Path}");
                        }
                    }
                    break;
                case ProjectType.DS3:
                    foreach (var entry in maps.Entries)
                    {
                        try
                        {
                            var msbData = Editor.Project.FS.ReadFile(entry.Path);
                            var msb = MSB3.Read(msbData.Value);

                            if (!MapList.ContainsKey(entry.Filename))
                                MapList.Add(entry.Filename, msb);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[Smithbox] Failed to read MSB: {entry.Path}");
                        }
                    }
                    break;
                case ProjectType.BB:
                    foreach (var entry in maps.Entries)
                    {
                        try
                        {
                            var msbData = Editor.Project.FS.ReadFile(entry.Path);
                            var msb = MSBB.Read(msbData.Value);

                            if (!MapList.ContainsKey(entry.Filename))
                                MapList.Add(entry.Filename, msb);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[Smithbox] Failed to read MSB: {entry.Path}");
                        }
                    }
                    break;
                case ProjectType.SDT:
                    foreach (var entry in maps.Entries)
                    {
                        try
                        {
                            var msbData = Editor.Project.FS.ReadFile(entry.Path);
                            var msb = MSBS.Read(msbData.Value);

                            if (!MapList.ContainsKey(entry.Filename))
                                MapList.Add(entry.Filename, msb);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[Smithbox] Failed to read MSB: {entry.Path}");
                        }
                    }
                    break;
                case ProjectType.ER:
                    foreach (var entry in maps.Entries)
                    {
                        try
                        {
                            var msbData = Editor.Project.FS.ReadFile(entry.Path);
                            var msb = MSBE.Read(msbData.Value);

                            if (!MapList.ContainsKey(entry.Filename))
                                MapList.Add(entry.Filename, msb);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[Smithbox] Failed to read MSB: {entry.Path}");
                        }
                    }
                    break;
                case ProjectType.AC6:
                    foreach (var entry in maps.Entries)
                    {
                        try
                        {
                            var msbData = Editor.Project.FS.ReadFile(entry.Path);
                            var msb = MSB_AC6.Read(msbData.Value);

                            if (!MapList.ContainsKey(entry.Filename))
                                MapList.Add(entry.Filename, msb);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[Smithbox] Failed to read MSB: {entry.Path}");
                        }
                    }
                    break;
                case ProjectType.NR:
                    foreach (var entry in maps.Entries)
                    {
                        try
                        {
                            var msbData = Editor.Project.FS.ReadFile(entry.Path);
                            var msb = MSB_NR.Read(msbData.Value);

                            if (!MapList.ContainsKey(entry.Filename))
                                MapList.Add(entry.Filename, msb);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[Smithbox] Failed to read MSB: {entry.Path}");
                        }
                    }
                    break;
            }
        }

        Matches = new();

        foreach (KeyValuePair<string, IMsb> entry in MapList)
        {
            CompileResults(entry.Key, entry.Value);
        }
    }

    public void CompileResults(string mapName, IMsb map)
    {
        var searchInput = _searchInput.ToLower();

        foreach (var entry in map.Parts.GetEntries())
        {
            var modelName = entry.ModelName;

            if (modelName != null)
            {
                modelName = modelName.ToLower();

                if (_looseModelNameMatch)
                {
                    if (modelName.Contains(searchInput))
                    {
                        if (!Matches.Any(e => e.MapName == mapName))
                        {
                            var match = new MapModelMatch(mapName, modelName, entry.Name);
                            Matches.Add(match);
                        }
                    }
                }
                else
                {
                    if (modelName == searchInput)
                    {
                        if (!Matches.Any(e => e.MapName == mapName))
                        {
                            var match = new MapModelMatch(mapName, modelName, entry.Name);
                            Matches.Add(match);
                        }
                    }
                }
            }
        }
    }

    public void DisplayInstances()
    {
        if (Matches.Count > 0)
        {
            ImGui.Separator();
            UIHelper.WrappedText($"Instances of {_searchInput}:");
            ImGui.Separator();
            foreach (var entry in Matches)
            {
                if (ImGui.Selectable($"{entry.MapName}"))
                {
                    EditorCommandQueue.AddCommand($"map/load/{entry.MapName}");
                    EditorCommandQueue.AddCommand($"map/select/{entry.MapName}/{entry.EntityName}/Part");
                }
                var aliasName = AliasUtils.GetMapNameAlias(Editor.Project, entry.MapName);
                UIHelper.DisplayAlias(aliasName);
            }
        }
    }
}

public class MapModelMatch
{
    public string MapName;
    public string ModelName;
    public string EntityName;

    public MapModelMatch(string mapname, string modelname, string entityName)
    {
        MapName = mapname;
        ModelName = modelname;
        EntityName = entityName;
    }
}
