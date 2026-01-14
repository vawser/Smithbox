using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

public class ModelInstanceFinder
{
    public ModelEditorScreen Editor;
    public ProjectEntry Project;

    public ModelInstanceFinder(ModelEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public string _searchInput = "";
    public List<MapModelMatch> Matches = new List<MapModelMatch>();

    public List<ResourceDescriptor> resMaps = new List<ResourceDescriptor>();
    public bool _targetProjectFiles = true;
    public bool _looseModelNameMatch = false;

    private bool SetupSearch = true;

    public Dictionary<string, IMsb> MapList = new Dictionary<string, IMsb>();

    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (ImGui.CollapsingHeader("Model Instance Finder"))
        {
            UIHelper.WrappedText("Search through all maps for usage of the specificed model name.");
            UIHelper.WrappedText("");

            UIHelper.WrappedText("Model Name:");
            ImGui.InputText("##modelNameInput", ref _searchInput, 255);

            UIHelper.WrappedText("");
            ImGui.Checkbox("Target Project Files", ref _targetProjectFiles);
            UIHelper.Tooltip("Uses the project map files instead of game root.");
            ImGui.Checkbox("Loose Name Match", ref _looseModelNameMatch);
            UIHelper.Tooltip("Only require the Model Name field to contain the search string, instead of requiring an exact match.");

            UIHelper.WrappedText("");

            if (ImGui.Button("Search", DPI.WholeWidthButton(windowWidth, 24)))
            {
                SearchMaps();
            }
            UIHelper.Tooltip("Initial usage will be slow as all maps have to be loaded. Subsequent usage will be instant.");

            UIHelper.WrappedText("");

            DisplayInstances();
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
                if (ImGui.Selectable($"{entry.MapName} [{entry.Count}]"))
                {
                    EditorCommandQueue.AddCommand($"map/load/{entry.MapName}");
                    EditorCommandQueue.AddCommand($"map/select/{entry.MapName}/{entry.EntityName}/Part");
                }
                var aliasName = AliasHelper.GetMapNameAlias(Editor.Project, entry.MapName);
                UIHelper.DisplayAlias(aliasName);
                UIHelper.Tooltip("The value in the [] is the number of instances with the map.");
            }
        }
    }

    public void SearchMaps()
    {
        if (SetupSearch)
        {
            SetupSearch = false;

            var targetFS = Editor.Project.VFS.VanillaFS;
            if(_targetProjectFiles)
            {
                targetFS = Editor.Project.VFS.FS;
            }

            var maps = Editor.Project.Handler.MapData.MapFiles;

            switch (Editor.Project.Descriptor.ProjectType)
            {
                case ProjectType.DES:
                    foreach (var entry in maps.Entries)
                    {
                        try
                        {
                            var msbData = targetFS.ReadFile(entry.Path);
                            var msb = MSBD.Read(msbData.Value);

                            if (!MapList.ContainsKey(entry.Filename))
                                MapList.Add(entry.Filename, msb);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[Smithbox] Failed to read MSB: {entry.Path}", LogLevel.Error, LogPriority.High, e);
                        }
                    }
                    break;
                case ProjectType.DS1:
                case ProjectType.DS1R:
                    foreach (var entry in maps.Entries)
                    {
                        try
                        {
                            var msbData = targetFS.ReadFile(entry.Path);
                            var msb = MSB1.Read(msbData.Value);

                            if (!MapList.ContainsKey(entry.Filename))
                                MapList.Add(entry.Filename, msb);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[Smithbox] Failed to read MSB: {entry.Path}", LogLevel.Error, LogPriority.High, e);
                        }
                    }
                    break;
                case ProjectType.DS2:
                case ProjectType.DS2S:
                    foreach (var entry in maps.Entries)
                    {
                        try
                        {
                            var msbData = targetFS.ReadFile(entry.Path);
                            var msb = MSB2.Read(msbData.Value);

                            if (!MapList.ContainsKey(entry.Filename))
                                MapList.Add(entry.Filename, msb);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[Smithbox] Failed to read MSB: {entry.Path}", LogLevel.Error, LogPriority.High, e);
                        }
                    }
                    break;
                case ProjectType.DS3:
                    foreach (var entry in maps.Entries)
                    {
                        try
                        {
                            var msbData = targetFS.ReadFile(entry.Path);
                            var msb = MSB3.Read(msbData.Value);

                            if (!MapList.ContainsKey(entry.Filename))
                                MapList.Add(entry.Filename, msb);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[Smithbox] Failed to read MSB: {entry.Path}", LogLevel.Error, LogPriority.High, e);
                        }
                    }
                    break;
                case ProjectType.BB:
                    foreach (var entry in maps.Entries)
                    {
                        try
                        {
                            var msbData = targetFS.ReadFile(entry.Path);
                            var msb = MSBB.Read(msbData.Value);

                            if (!MapList.ContainsKey(entry.Filename))
                                MapList.Add(entry.Filename, msb);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[Smithbox] Failed to read MSB: {entry.Path}", LogLevel.Error, LogPriority.High, e);
                        }
                    }
                    break;
                case ProjectType.SDT:
                    foreach (var entry in maps.Entries)
                    {
                        try
                        {
                            var msbData = targetFS.ReadFile(entry.Path);
                            var msb = MSBS.Read(msbData.Value);

                            if (!MapList.ContainsKey(entry.Filename))
                                MapList.Add(entry.Filename, msb);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[Smithbox] Failed to read MSB: {entry.Path}", LogLevel.Error, LogPriority.High, e);
                        }
                    }
                    break;
                case ProjectType.ER:
                    foreach (var entry in maps.Entries)
                    {
                        try
                        {
                            var msbData = targetFS.ReadFile(entry.Path);
                            var msb = MSBE.Read(msbData.Value);

                            if (!MapList.ContainsKey(entry.Filename))
                                MapList.Add(entry.Filename, msb);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[Smithbox] Failed to read MSB: {entry.Path}", LogLevel.Error, LogPriority.High, e);
                        }
                    }
                    break;
                case ProjectType.AC6:
                    foreach (var entry in maps.Entries)
                    {
                        try
                        {
                            var msbData = targetFS.ReadFile(entry.Path);
                            var msb = MSB_AC6.Read(msbData.Value);

                            if (!MapList.ContainsKey(entry.Filename))
                                MapList.Add(entry.Filename, msb);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[Smithbox] Failed to read MSB: {entry.Path}", LogLevel.Error, LogPriority.High, e);
                        }
                    }
                    break;
                case ProjectType.NR:
                    foreach (var entry in maps.Entries)
                    {
                        try
                        {
                            var msbData = targetFS.ReadFile(entry.Path);
                            var msb = MSB_NR.Read(msbData.Value);

                            if (!MapList.ContainsKey(entry.Filename))
                                MapList.Add(entry.Filename, msb);
                        }
                        catch (Exception e)
                        {
                            TaskLogs.AddLog($"[Smithbox] Failed to read MSB: {entry.Path}", LogLevel.Error, LogPriority.High, e);
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
                            match.Count++;
                            Matches.Add(match);
                        }
                        else
                        {
                            var curMatch = Matches.FirstOrDefault(e => e.MapName == mapName);
                            curMatch.Count++;
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
                            match.Count++;
                            Matches.Add(match);
                        }
                        else
                        {
                            var curMatch = Matches.FirstOrDefault(e => e.MapName == mapName);
                            curMatch.Count++;
                        }
                    }
                }
            }
        }
    }
}

public class MapModelMatch
{
    public string MapName;
    public string ModelName;
    public string EntityName;

    public int Count = 0;

    public MapModelMatch(string mapname, string modelname, string entityName)
    {
        MapName = mapname;
        ModelName = modelname;
        EntityName = entityName;
    }
}
