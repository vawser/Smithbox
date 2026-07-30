using Hexa.NET.ImGui;
using Microsoft.Extensions.Logging;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Logger;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ModelEditor;

public class ModelInstanceFinder
{
    public ModelEditorView View;
    public ProjectEntry Project;

    public ModelInstanceFinder(ModelEditorView view, ProjectEntry project)
    {
        View = view;
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

        // Model Instance Finder
        if (ImGui.CollapsingHeader($"{LOC.Get("MODEL_InstanceFinder_Header")}##modelInstanceFinder"))
        {
            ImGui.BeginChild("ModelInstanceFinderToolSection", ImGuiChildFlags.Borders);

            GUI.WrappedText(LOC.Get("MODEL_InstanceFinder_Hint"));

            // Model Name
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("MODEL_InstanceFinder_Model_Name_Header"),
                LOC.Get("MODEL_InstanceFinder_Model_Name_Header_TT"));

            GUI.SinglelineTextInput("ModelNameInput", ref _searchInput);

            // Options
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("MODEL_InstanceFinder_Options_Header"),
                LOC.Get("MODEL_InstanceFinder_Options_Header_TT"));

            // Target Project Files
            ImGui.Checkbox($"{LOC.Get("MODEL_InstanceFinder_Checkbox_Target_Project")}##toggleProjectFileTarget", ref _targetProjectFiles);
            GUI.Tooltip(LOC.Get("MODEL_InstanceFinder_Checkbox_Target_Project_TT"));

            // Loose Name Match
            ImGui.Checkbox($"{LOC.Get("MODEL_InstanceFinder_Checkbox_Loose_Name_Match")}##toggleLooseNameMatch", ref _looseModelNameMatch);
            GUI.Tooltip(LOC.Get("MODEL_InstanceFinder_Checkbox_Loose_Name_Match_TT"));

            // Actions
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("MODEL_InstanceFinder_Actions_Header"),
                LOC.Get("MODEL_InstanceFinder_Actions_Header_TT"));

            GUI.MultiButtonInput("instanceActions",
                "search",
                LOC.Get("MODEL_InstanceFinder_Search_Action"),
                LOC.Get("MODEL_InstanceFinder_Search_Action_TT"),
                SearchMaps);

            // Results
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("MODEL_InstanceFinder_Results_Header"),
                LOC.Get("MODEL_InstanceFinder_Results_Header_TT"));

            DisplayInstances();

            ImGui.EndChild();
        }
    }

    public void DisplayInstances()
    {
        if (Matches.Count > 0)
        {
            ImGui.BeginChild("ModelInstanceList");

            foreach (var entry in Matches)
            {
                if (ImGui.Selectable($"{entry.MapName} [{entry.Count}]"))
                {
                    EditorCommandQueue.AddCommand($"map/load/{entry.MapName}");
                    EditorCommandQueue.AddCommand($"map/select/{entry.MapName}/{entry.EntityName}/Part");
                }
                var aliasName = AliasHelper.GetMapNameAlias(View.Project, entry.MapName);
                GUI.DisplayAlias(aliasName);
                GUI.Tooltip(LOC.Get("MODEL_InstanceFinder_Instances_TT"));
            }

            ImGui.EndChild();
        }
        else
        {
            GUI.WrappedText(LOC.Get("MODEL_InstanceFinder_No_Results"));
        }
    }

    public void SearchMaps()
    {
        if (SetupSearch)
        {
            SetupSearch = false;

            var targetFS = View.Project.VFS.VanillaFS;
            if(_targetProjectFiles)
            {
                targetFS = View.Project.VFS.FS;
            }

            var maps = View.Project.Locator.MapFiles;

            switch (View.Project.Descriptor.ProjectType)
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
                            Smithbox.LogError(this, LOC.Get("MODEL_InstanceFinder_Log_Failed_MSB_Read", entry.Path), e);
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
                            Smithbox.LogError(this, LOC.Get("MODEL_InstanceFinder_Log_Failed_MSB_Read", entry.Path), e);
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
                            Smithbox.LogError(this, LOC.Get("MODEL_InstanceFinder_Log_Failed_MSB_Read", entry.Path), e);
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
                            Smithbox.LogError(this, LOC.Get("MODEL_InstanceFinder_Log_Failed_MSB_Read", entry.Path), e);
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
                            Smithbox.LogError(this, LOC.Get("MODEL_InstanceFinder_Log_Failed_MSB_Read", entry.Path), e);
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
                            Smithbox.LogError(this, LOC.Get("MODEL_InstanceFinder_Log_Failed_MSB_Read", entry.Path), e);
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
                            Smithbox.LogError(this, LOC.Get("MODEL_InstanceFinder_Log_Failed_MSB_Read", entry.Path), e);
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
                            Smithbox.LogError(this, LOC.Get("MODEL_InstanceFinder_Log_Failed_MSB_Read", entry.Path), e);
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
                            Smithbox.LogError(this, LOC.Get("MODEL_InstanceFinder_Log_Failed_MSB_Read", entry.Path), e);
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
