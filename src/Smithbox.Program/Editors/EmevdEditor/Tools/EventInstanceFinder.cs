using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.EmevdEditor;
using StudioCore.Formats.JSON;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.EventScriptEditorNS;

public class EventInstanceFinder
{
    public EmevdEditorScreen Editor;
    public ProjectEntry Project;

    public EventInstanceFinder(EmevdEditorScreen editor, ProjectEntry project)
    {
        Editor = editor; 
        Project = project;
    }

    public void Display()
    {
        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth, 32);

        if (ImGui.CollapsingHeader("Event Finder"))
        {
            ImGui.Text("Search Term");
            ImGui.SetNextItemWidth(windowWidth * 0.75f);
            ImGui.InputText("##eventSearch", ref SearchTerm, 255);

            ImGui.SameLine();

            ImGui.Checkbox("##eventMatchExact", ref MatchExact);
            UIHelper.Tooltip("If enabled, the search term must be an exact match.");

            if (ImGui.Button("Search##eventSearch", defaultButtonSize))
            {
                FindEventInstances();
            }

            UIHelper.SimpleHeader("##eventInstances", "Event Instances", "", UI.Current.ImGui_AliasName_Text);

            ImGui.BeginChild("eventResults", new Vector2(windowWidth, 600));
            if (Results.Count > 0)
            {
                for(int i = 0; i < Results.Count; i++)
                {
                    var entry = Results[i];

                    var displayName = $"{entry.FileEntry.Filename}: {entry.Event.ID}";

                    if(ImGui.Selectable($"{displayName}##eventInstance{i}", entry == SelectedResult))
                    {
                        SelectedResult = entry;
                        Editor.Selection.SelectLoadedFile(entry.FileEntry);
                        Editor.Selection.SelectEvent(entry.Event, entry.EventIndex);
                    }
                    
                    // Arrow Selection
                    if (ImGui.IsItemHovered() && SelectNextResult)
                    {
                        SelectNextResult = false;
                        SelectedResult = entry;
                        Editor.Selection.SelectLoadedFile(entry.FileEntry);
                        Editor.Selection.SelectEvent(entry.Event, entry.EventIndex);
                    }
                    if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                    {
                        SelectNextResult = true;
                    }
                }
            }
            ImGui.EndChild();
        }
    }

    public string SearchTerm = "";
    public bool MatchExact = false;
    public List<EmevdSearchResult> Results = new();
    public EmevdSearchResult SelectedResult;
    public bool SelectNextResult = false;

    public void FindEventInstances()
    {
        Results = new();

        List<Task> tasks = new();

        // Load all scripts
        foreach (var entry in Project.EmevdData.PrimaryBank.Scripts)
        {
            var newTask = Project.EmevdData.PrimaryBank.LoadScript(entry.Key);
            tasks.Add(newTask);
        }

        Task.WaitAll(tasks);

        foreach (var entry in Project.EmevdData.PrimaryBank.Scripts)
        {
            int index = 0;
            foreach (var evt in entry.Value.Events)
            {
                var addResult = false;

                var eventID = $"{evt.ID}";

                if (MatchExact)
                {
                    if(SearchTerm == eventID)
                    {
                        addResult = true;
                    }
                }
                else
                {
                    if (eventID.ToLower().Contains(SearchTerm.ToLower()))
                    {
                        addResult = true;
                    }
                }

                if (addResult)
                {
                    var newResult = new EmevdSearchResult();
                    newResult.FileEntry = entry.Key;
                    newResult.EMEVD = entry.Value;
                    newResult.Event = evt;
                    newResult.EventIndex = index;

                    Results.Add(newResult);
                }

                index++;
            }
        }
    }
}

