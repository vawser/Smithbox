using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StudioCore.Editors.TimeActEditor.Actions;
using Hexa.NET.ImGui;
using StudioCore.Utilities;
using System.Numerics;
using StudioCore.Editor;
using SoulsFormats;
using HKLib.hk2018.hkAsyncThreadPool;
using static StudioCore.Editors.TimeActEditor.Utils.TimeActUtils;
using StudioCore.Interface;
using StudioCore.Editors.TimeActEditor.Enums;
using StudioCore.Configuration;
using StudioCore.Core;
using Microsoft.AspNetCore.Components.Forms;

namespace StudioCore.Editors.TimeActEditor.Utils;

public class TimeActSearch
{
    public TimeActEditorScreen Editor;
    public ProjectEntry Project;

    public TimeActSearch(TimeActEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    private TimeActSearchType CurrentSearchType = TimeActSearchType.AnimationID;
    private string SearchInput = "";
    private List<TimeActSearchResult> searchResults = new List<TimeActSearchResult>();
    private bool AllowPartialMatch = false;

    public void Display()
    {
        if (Editor.Selection.SelectedBinder == null)
        {
            UIHelper.WrappedText("You must select a File first.");
            return;
        }

        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth * 0.975f, 32);

        ImGui.PushItemWidth(windowWidth);

        UIHelper.WrappedText("Search Type:");
        if (ImGui.BeginCombo("##Search Type", CurrentSearchType.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(TimeActSearchType)))
            {
                var serType = (TimeActSearchType)entry;

                if (ImGui.Selectable($"{serType.GetDisplayName()}"))
                {
                    CurrentSearchType = serType;
                }
            }

            ImGui.EndCombo();
        }
        if (CurrentSearchType is TimeActSearchType.EventValue)
        {
            UIHelper.WrappedText("");
            UIHelper.WrappedText($"Event Value will only match within the currently selected Time Act container:");
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, $"{Editor.Selection.SelectedFileKey}");
        }
        UIHelper.WrappedText("");

        UIHelper.WrappedText("Search Input:");
        ImGui.InputText("##searchInput", ref SearchInput, 255);

        ImGui.Checkbox("Allow Partial Matches", ref AllowPartialMatch);
        UIHelper.WrappedText("");

        if (ImGui.Button("Search##searchButton", defaultButtonSize))
        {
            if (Editor.Selection.CurrentTimeAct != null)
            {
                if (SearchInput != "")
                {
                    GenerateSearchResults();
                }
            }
        }


        ImGui.Separator();

        UIHelper.WrappedText("Results:");

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
                    var command = $"timeact/select/chr/{res.FileKey}/{res.TimeActKey}";

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
        searchResults = new();

        for (int j = 0; j < Editor.Selection.SelectedBinder.Files.Count; j++)
        {
            var curFile = Editor.Selection.SelectedBinder.Files.ElementAt(j);
            var binderFile = curFile.Key;
            var taeEntry = curFile.Value;

            for (int i = 0; i < taeEntry.Animations.Count; i++)
            {
                var anim = taeEntry.Animations[i];

                if (CurrentSearchType is TimeActSearchType.AnimationID)
                {
                    if (AllowPartialMatch)
                    {
                        if (anim.ID.ToString().Contains(SearchInput))
                        {
                            var result = new TimeActSearchResult(binderFile.Name, taeEntry, anim);
                            result.FileKey = Editor.Selection.SelectedFileKey;
                            result.TimeActKey = binderFile.Name;
                            result.AnimationIndex = i;
                            searchResults.Add(result);
                        }
                    }
                    else
                    {
                        if (anim.ID.ToString() == SearchInput)
                        {
                            var result = new TimeActSearchResult(binderFile.Name, taeEntry, anim);
                            result.FileKey = Editor.Selection.SelectedFileKey;
                            result.TimeActKey = binderFile.Name;
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

                        if (CurrentSearchType is TimeActSearchType.EventID)
                        {
                            if (AllowPartialMatch)
                            {
                                if (evt.Type.ToString().Contains(SearchInput))
                                {
                                    TimeActUtils.ApplyTemplate(Editor, taeEntry, Editor.Selection.CurrentTimeActType);

                                    var result = new TimeActSearchResult(binderFile.Name, taeEntry, anim, evt);
                                    result.FileKey = Editor.Selection.SelectedFileKey;
                                    result.TimeActKey = binderFile.Name;
                                    result.AnimationIndex = i;
                                    result.EventIndex = k;
                                    searchResults.Add(result);
                                }
                            }
                            else
                            {
                                if (evt.Type.ToString() == SearchInput)
                                {
                                    TimeActUtils.ApplyTemplate(Editor, taeEntry, Editor.Selection.CurrentTimeActType);

                                    var result = new TimeActSearchResult(binderFile.Name, taeEntry, anim, evt);
                                    result.FileKey = Editor.Selection.SelectedFileKey;
                                    result.TimeActKey = binderFile.Name;
                                    result.AnimationIndex = i;
                                    result.EventIndex = k;
                                    searchResults.Add(result);
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
                                            var result = new TimeActSearchResult(binderFile.Name, taeEntry, anim, evt);
                                            result.FileKey = Editor.Selection.SelectedFileKey;
                                            result.TimeActKey = binderFile.Name;
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
                                            var result = new TimeActSearchResult(binderFile.Name, taeEntry, anim, evt);
                                            result.FileKey = Editor.Selection.SelectedFileKey;
                                            result.TimeActKey = binderFile.Name;
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


