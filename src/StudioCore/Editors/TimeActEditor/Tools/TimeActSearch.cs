using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudioCore.Editors.TimeActEditor.Actions;
using ImGuiNET;
using StudioCore.Utilities;
using StudioCore.Interface;
using System.Numerics;
using StudioCore.Editor;
using SoulsFormats;
using HKLib.hk2018.hkAsyncThreadPool;
using static StudioCore.Editors.TimeActEditor.TimeActUtils;

namespace StudioCore.Editors.TimeActEditor.Tools;

public class TimeActSearch
{
    private TimeActEditorScreen Screen;
    public ActionHandler Handler;

    public enum SearchType
    {
        [Display(Name ="Animation ID")] AnimationID,
        [Display(Name = "Event ID")] EventID,
        [Display(Name = "Event Name")] EventName,
        [Display(Name = "Event Value")] EventValue,
    }

    public TimeActSearch(TimeActEditorScreen screen, ActionHandler handler)
    {
        Screen = screen;
        Handler = handler;
    }

    private SearchType CurrentSearchType = SearchType.AnimationID;
    private string SearchInput = "";
    private List<TimeActSearchResult> searchResults = new List<TimeActSearchResult>();
    private bool AllowPartialMatch = false;

    public void Display()
    {
        if (Screen.SelectionHandler.ContainerInfo == null)
        {
            ImguiUtils.WrappedText("You must select a File first.");
            return;
        }

        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth * 0.975f, 32);

        ImGui.PushItemWidth(windowWidth);

        ImguiUtils.WrappedText("Search Type:");
        if (ImGui.BeginCombo("##Search Type", CurrentSearchType.GetDisplayName()))
        {
            foreach(var entry in Enum.GetValues(typeof(SearchType)))
            {
                var serType = (SearchType)entry;

                if (ImGui.Selectable($"{serType.GetDisplayName()}"))
                {
                    CurrentSearchType = serType;
                }
            }

            ImGui.EndCombo();
        }
        if (CurrentSearchType is SearchType.EventValue)
        {
            ImguiUtils.WrappedText("");
            ImguiUtils.WrappedText($"Event Value will only match within the currently selected Time Act container:");
            ImguiUtils.WrappedTextColored(CFG.Current.ImGui_AliasName_Text, $"{Screen.SelectionHandler.ContainerInfo.Name}");
        }
        ImguiUtils.WrappedText("");

        ImguiUtils.WrappedText("Search Input:");
        ImGui.InputText("##searchInput", ref SearchInput, 255);

        ImGui.Checkbox("Allow Partial Matches", ref AllowPartialMatch);
        ImguiUtils.WrappedText("");

        if (ImGui.Button("Search##searchButton", defaultButtonSize))
        {
            if (Screen.SelectionHandler.ContainerIndex != -1 && Screen.SelectionHandler.CurrentTimeActKey != -1)
            {
                if (SearchInput != "")
                {
                    GenerateSearchResults();
                }
            }
        }


        ImGui.Separator();

        ImguiUtils.WrappedText("Results:");

        if (searchResults.Count > 0)
        {
            for (int i = 0; i < searchResults.Count; i++)
            {
                var res = searchResults[i];
                var displayName = $"{res.TimeActName}";

                if (res.ResultAnim != null)
                {
                    displayName = displayName + " - " + $"{res.ResultAnim.ID}";

                    if (res.ResultEvent != null)
                    {
                        displayName = displayName + " - " + $"{res.ResultEvent.Type}({res.ResultEvent.TypeName})";


                        if (res.EventPropertyValue != null)
                        {
                            displayName = displayName + " - " + $"{res.EventPropertyValue}";
                        }
                    }
                }

                if (ImGui.Selectable($"{displayName}##{displayName}_{i}"))
                {
                    var containerType = Smithbox.EditorHandler.TimeActEditor.SelectionHandler.CurrentFileContainerType;

                    var command = $"timeact/select/none/{res.ContainerIndex}/{res.TimeActIndex}";

                    if (containerType is TimeActSelectionHandler.FileContainerType.Character)
                    {
                        command = $"timeact/select/chr/{res.ContainerIndex}/{res.TimeActIndex}";
                    }
                    if (containerType is TimeActSelectionHandler.FileContainerType.Object)
                    {
                        command = $"timeact/select/obj/{res.ContainerIndex}/{res.TimeActIndex}";
                    }

                    if (res.ResultAnim != null)
                    {
                        command = command + $"/{res.AnimationIndex}";

                        if (res.ResultEvent != null)
                        {
                            command = command + $"/{res.EventIndex}";
                        }
                    }

                    EditorCommandQueue.AddCommand(command);
                }
            }
        }
        else
        {
            ImguiUtils.WrappedText("No results.");
        }
    }

    public void GenerateSearchResults()
    {
        searchResults = new();

        var containerIndex = Screen.SelectionHandler.ContainerIndex;

        for (int j = 0; j < Screen.SelectionHandler.ContainerInfo.InternalFiles.Count; j++)
        {
            var timeActFile = Screen.SelectionHandler.ContainerInfo.InternalFiles[j].TAE;
            var timeActName = Screen.SelectionHandler.ContainerInfo.InternalFiles[j].Name;

            for (int i = 0; i < timeActFile.Animations.Count; i++)
            {
                var anim = timeActFile.Animations[i];

                if (CurrentSearchType is SearchType.AnimationID)
                {
                    if (AllowPartialMatch)
                    {
                        if (anim.ID.ToString().Contains(SearchInput))
                        {
                            var result = new TimeActSearchResult(timeActName, timeActFile, anim);
                            result.ContainerIndex = containerIndex;
                            result.TimeActIndex = j;
                            result.AnimationIndex = i;
                            searchResults.Add(result);
                        }
                    }
                    else
                    {
                        if (anim.ID.ToString() == SearchInput)
                        {
                            var result = new TimeActSearchResult(timeActName, timeActFile, anim);
                            result.ContainerIndex = containerIndex;
                            result.TimeActIndex = j;
                            result.AnimationIndex = i;
                            searchResults.Add(result);
                        }
                    }
                }
                else
                {
                    for (int k = 0; k < anim.Events.Count; k++)
                    {
                        var evt = anim.Events[k];

                        if (CurrentSearchType is SearchType.EventID)
                        {
                            if (AllowPartialMatch)
                            {
                                if (evt.Type.ToString().Contains(SearchInput))
                                {
                                    TimeActUtils.ApplyTemplate(timeActFile, Screen.SelectionHandler.CurrentTimeActType);

                                    var result = new TimeActSearchResult(timeActName, timeActFile, anim, evt);
                                    result.ContainerIndex = containerIndex;
                                    result.TimeActIndex = j;
                                    result.AnimationIndex = i;
                                    result.EventIndex = k;
                                    searchResults.Add(result);
                                }
                            }
                            else
                            {
                                if (evt.Type.ToString() == SearchInput)
                                {
                                    TimeActUtils.ApplyTemplate(timeActFile, Screen.SelectionHandler.CurrentTimeActType);

                                    var result = new TimeActSearchResult(timeActName, timeActFile, anim, evt);
                                    result.ContainerIndex = containerIndex;
                                    result.TimeActIndex = j;
                                    result.AnimationIndex = i;
                                    result.EventIndex = k;
                                    searchResults.Add(result);
                                }
                            }
                        }

                        if (CurrentSearchType is SearchType.EventValue)
                        {
                            if (evt.Parameters != null)
                            {
                                if (AllowPartialMatch)
                                {
                                    foreach (var entry in evt.Parameters.ParameterValues)
                                    {
                                        var valueStr = entry.Value.ToString();

                                        if (valueStr.Contains(SearchInput))
                                        {
                                            var result = new TimeActSearchResult(timeActName, timeActFile, anim, evt);
                                            result.ContainerIndex = containerIndex;
                                            result.TimeActIndex = j;
                                            result.AnimationIndex = i;
                                            result.EventIndex = k;
                                            result.EventPropertyValue = valueStr;
                                            searchResults.Add(result);
                                        }
                                    }
                                }
                                else
                                {
                                    foreach (var entry in evt.Parameters.ParameterValues)
                                    {
                                        var valueStr = entry.Value.ToString();

                                        if (valueStr == SearchInput)
                                        {
                                            var result = new TimeActSearchResult(timeActName, timeActFile, anim, evt);
                                            result.ContainerIndex = containerIndex;
                                            result.TimeActIndex = j;
                                            result.AnimationIndex = i;
                                            result.EventIndex = k;
                                            result.EventPropertyValue = valueStr;
                                            searchResults.Add(result);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}


public class TimeActSearchResult
{
    public string TimeActName;
    public TAE ResultTAE;
    public TAE.Animation ResultAnim;
    public TAE.Event ResultEvent;

    public int ContainerIndex = -1;
    public int TimeActIndex = -1;
    public int AnimationIndex = -1;
    public int EventIndex = -1;
    public string EventPropertyValue = "";

    public TimeActSearchResult(string taeName, TAE tae, TAE.Animation anim, TAE.Event evt = null) 
    {
        TimeActName = taeName;
        ResultTAE = tae;
        ResultAnim = anim;
        ResultEvent = evt;
    }
}