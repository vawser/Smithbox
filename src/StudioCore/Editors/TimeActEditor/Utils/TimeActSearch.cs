using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudioCore.Editors.TimeActEditor.Actions;
using ImGuiNET;
using StudioCore.Utilities;
using System.Numerics;
using StudioCore.Editor;
using SoulsFormats;
using HKLib.hk2018.hkAsyncThreadPool;
using static StudioCore.Editors.TimeActEditor.Utils.TimeActUtils;
using StudioCore.Interface;
using StudioCore.Editors.TimeActEditor.Enums;

namespace StudioCore.Editors.TimeActEditor.Utils;

public class TimeActSearch
{
    private TimeActEditorScreen Screen;
    public TimeActActionHandler ActionHandler;

    private List<TimeActSearchTerm> Terms = new();
    private List<TimeActSearchResult> Results = new();

    public TimeActSearch(TimeActEditorScreen screen)
    {
        Screen = screen;
        ActionHandler = screen.ActionHandler;
    }

    private void DisplaySearchTerm(TimeActSearchTerm term)
    {
        var windowWidth = ImGui.GetWindowWidth() * 0.975f;
        var defaultButtonSize = new Vector2(windowWidth, 24);

        UIHelper.WrappedText("Search Input:");
        ImGui.PushItemWidth(windowWidth);
        ImGui.InputText("##searchInput", ref term.SearchInput, 255);
        UIHelper.ShowHoverTooltip("The search term to match with.");

        UIHelper.WrappedText("Search Type:");
        ImGui.PushItemWidth(windowWidth);
        if (ImGui.BeginCombo("##Search Type", term.CurrentSearchType.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(TimeActSearchType)))
            {
                var serType = (TimeActSearchType)entry;

                if (ImGui.Selectable($"{serType.GetDisplayName()}"))
                {
                    term.CurrentSearchType = serType;
                }
            }

            ImGui.EndCombo();
        }
        UIHelper.ShowHoverTooltip("What type of values the search term will be compared to.");

        if (term.CurrentSearchType is TimeActSearchType.EventValue)
        {
            UIHelper.WrappedText("");
            UIHelper.WrappedText($"Event Value will only match within the currently selected Time Act container:");
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"{Screen.Selection.ContainerInfo.Name}");
        }
        UIHelper.WrappedText("");

        ImGui.Checkbox("Allow Partial Matches", ref term.AllowPartialMatch);
        UIHelper.WrappedText("");
        UIHelper.ShowHoverTooltip("Allow partial matches for this term.");

        if (ImGui.Button("Clear Term"))
        {
            Terms.Remove(term);
        }
        UIHelper.ShowHoverTooltip("Remove this term.");

        ImGui.Separator();
    }

    public void Display()
    {
        var windowWidth = ImGui.GetWindowWidth() * 0.975f;
        var defaultButtonSize = new Vector2(windowWidth, 24);

        if (Screen.Selection.ContainerInfo == null)
        {
            UIHelper.WrappedText("You must select a File first.");
            return;
        }

        ImGui.Separator();
        UIHelper.WrappedText("Search Terms");
        ImGui.Separator();

        foreach (var existingComponent in Terms)
        {
            DisplaySearchTerm(existingComponent);
        }

        // Add new Term
        if (ImGui.Button("Add New Search Term", defaultButtonSize))
        {
            Terms.Add(new TimeActSearchTerm());
        }
        UIHelper.ShowHoverTooltip("Add a new term.");

        ImGui.Separator();

        if (ImGui.Button("Search##searchButton", defaultButtonSize))
        {
            if (Screen.Selection.ContainerIndex != -1 && Screen.Selection.CurrentTimeActKey != -1)
            {
                if (Terms.Count > 0)
                {
                    GenerateSearchResults();
                }
            }
        }

        ImGui.Separator();

        UIHelper.WrappedText("Results:");

        if (Results.Count > 0)
        {
            for (int i = 0; i < Results.Count; i++)
            {
                var res = Results[i];
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
                    var containerType = Smithbox.EditorHandler.TimeActEditor.Selection.CurrentFileContainerType;

                    var command = $"timeact/select/none/{res.ContainerIndex}/{res.TimeActIndex}";

                    if (containerType is FileContainerType.Character)
                    {
                        command = $"timeact/select/chr/{res.ContainerIndex}/{res.TimeActIndex}";
                    }
                    if (containerType is FileContainerType.Object)
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
            UIHelper.WrappedText("No results.");
        }
    }

    public void GenerateSearchResults()
    {
        Results = new();

        var truth = false;

        // TODO: implement the 'overall' truth for a set of terms, and only add a result if it matches all of them.

        foreach (var term in Terms)
        {
            // Time Acts
            for (int j = 0; j < Screen.Selection.ContainerInfo.InternalFiles.Count; j++)
            {
                var timeActFile = Screen.Selection.ContainerInfo.InternalFiles[j].TAE;
                var timeActName = Screen.Selection.ContainerInfo.InternalFiles[j].Name;

                // Time Act Animations
                for (int i = 0; i < timeActFile.Animations.Count; i++)
                {
                    if (term.CurrentSearchType is TimeActSearchType.AnimationID)
                    {
                        var result = MatchAnimation(term, timeActFile, timeActName, i, j);
                    }
                }
            }
        }
    }

    public (bool, TimeActSearchResult) MatchAnimation(TimeActSearchTerm term, TAE timeActFile, string timeActName, int i, int j)
    {
        var containerIndex = Screen.Selection.ContainerIndex;
        var anim = timeActFile.Animations[i];

        if (term.AllowPartialMatch)
        {
            if (anim.ID.ToString().Contains(term.SearchInput))
            {
                var result = new TimeActSearchResult(timeActName, timeActFile, anim);
                result.ContainerIndex = containerIndex;
                result.TimeActIndex = j;
                result.AnimationIndex = i;
                return (true, result);
            }
        }
        else
        {
            if (anim.ID.ToString() == term.SearchInput)
            {
                var result = new TimeActSearchResult(timeActName, timeActFile, anim);
                result.ContainerIndex = containerIndex;
                result.TimeActIndex = j;
                result.AnimationIndex = i;
                return (true, result);
            }
        }

        return (false, null);
    }
}

/*
Results = new();

var containerIndex = Screen.Selection.ContainerIndex;

for (int j = 0; j < Screen.Selection.ContainerInfo.InternalFiles.Count; j++)
{
    var timeActFile = Screen.Selection.ContainerInfo.InternalFiles[j].TAE;
    var timeActName = Screen.Selection.ContainerInfo.InternalFiles[j].Name;

    for (int i = 0; i < timeActFile.Animations.Count; i++)
    {
        var anim = timeActFile.Animations[i];

        if (CurrentSearchType is TimeActSearchType.AnimationID)
        {
            if (AllowPartialMatch)
            {
                if (anim.ID.ToString().Contains(SearchInput))
                {
                    var result = new TimeActSearchResult(timeActName, timeActFile, anim);
                    result.ContainerIndex = containerIndex;
                    result.TimeActIndex = j;
                    result.AnimationIndex = i;
                    Results.Add(result);
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
                    Results.Add(result);
                }
            }
        }
        else
        {
            for (int k = 0; k < anim.Events.Count; k++)
            {
                var evt = anim.Events[k];

                if (CurrentSearchType is TimeActSearchType.EventID)
                {
                    if (AllowPartialMatch)
                    {
                        if (evt.Type.ToString().Contains(SearchInput))
                        {
                            TimeActUtils.ApplyTemplate(timeActFile, Screen.Selection.CurrentTimeActType);

                            var result = new TimeActSearchResult(timeActName, timeActFile, anim, evt);
                            result.ContainerIndex = containerIndex;
                            result.TimeActIndex = j;
                            result.AnimationIndex = i;
                            result.EventIndex = k;
                            Results.Add(result);
                        }
                    }
                    else
                    {
                        if (evt.Type.ToString() == SearchInput)
                        {
                            TimeActUtils.ApplyTemplate(timeActFile, Screen.Selection.CurrentTimeActType);

                            var result = new TimeActSearchResult(timeActName, timeActFile, anim, evt);
                            result.ContainerIndex = containerIndex;
                            result.TimeActIndex = j;
                            result.AnimationIndex = i;
                            result.EventIndex = k;
                            Results.Add(result);
                        }
                    }
                }

                if (CurrentSearchType is TimeActSearchType.EventValue)
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
                                    Results.Add(result);
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
                                    Results.Add(result);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
*/
