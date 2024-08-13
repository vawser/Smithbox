using DotNext.Collections.Generic;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Interface.Modals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.DRB;
using static SoulsFormats.FFXDLSE;
using static StudioCore.Editors.TimeActEditor.AnimationBank;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Handles property edits for collection fields
/// </summary>
public class TimeActCollectionPropertyHandler
{
    private ActionManager EditorActionManager;
    private TimeActEditorScreen Screen;
    private TimeActDecorator Decorator;
    private TimeActSelectionHandler SelectionHandler;

    public TimeActCollectionPropertyHandler(ActionManager editorActionManager, TimeActEditorScreen screen, TimeActDecorator decorator)
    {
        EditorActionManager = editorActionManager;
        Screen = screen;
        Decorator = decorator;
        SelectionHandler = screen.SelectionHandler;

    }

    public bool ShowCreateEventModal = false;
    private TAE.Template.EventTemplate CurrentEvent;

    public void OnGui()
    {
        if (ShowCreateEventModal)
        {
            TaskLogs.AddLog("Create Event Modal");
            ImGui.OpenPopup("Create Event");
        }

        if (ImGui.BeginPopupModal("Create Event", ref ShowCreateEventModal, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize))
        {
            var listboxSize = new Vector2(520, 400);
            var buttonSize = new Vector2(520 * 0.5f, 24);

            var curEvent = Smithbox.EditorHandler.TimeActEditor.SelectionHandler.CurrentTimeActEvent;
            var curTemplate = TimeActUtils.GetRelevantTemplate(TimeActUtils.TemplateType.Character);

            if (curEvent != null && curTemplate != null)
            {
                ImGui.Text("Event Types:");
                if (ImGui.BeginListBox("##eventTypes", listboxSize))
                {
                    foreach (var entry in curTemplate.Events)
                    {
                        var eventType = entry.Value;

                        if (ImGui.Selectable($"[{eventType.ID}] {eventType.Name}##eventEntry{eventType.ID}", eventType == CurrentEvent))
                        {
                            CurrentEvent = eventType;
                        }
                    }

                    ImGui.EndListBox();
                }

                if (ImGui.Button("Create", buttonSize))
                {
                    var newEvent = new TAE.Event(curEvent.StartTime, curEvent.EndTime, CurrentEvent.ID, curEvent.Unk04, false, CurrentEvent);
                    var animations = Smithbox.EditorHandler.TimeActEditor.SelectionHandler.CurrentTimeActAnimation;
                    var insertIdx = animations.Events.IndexOf(curEvent) + 1;

                    var action = new TimeActCreateNewEvent(newEvent, animations.Events, insertIdx);
                    EditorActionManager.ExecuteAction(action);

                    ShowCreateEventModal = false;
                }
                ImGui.SameLine();
                if (ImGui.Button("Close", buttonSize))
                {
                    ShowCreateEventModal = false;
                }
            }

            ImGui.EndPopup();
        }
    }

    // TODO: actionize

    public void DuplicateTimeAct()
    {
        if (SelectionHandler.CurrentTimeAct == null)
            return;

        if (SelectionHandler.CurrentTimeActKey == -1)
            return;

        SelectionHandler.ContainerInfo.IsModified = true;

        var curInternalFile = SelectionHandler.ContainerInfo.InternalFiles[SelectionHandler.CurrentTimeActKey];
        var newInternalFile = new InternalFileInfo(curInternalFile.Filepath, curInternalFile.TAE.Clone());

        int id = int.Parse(newInternalFile.Name.Substring(1));
        int newId = id;
        string newName = "";
        (newId, newName) = GetNewFileName(id);

        string newFilePath = newInternalFile.Filepath.Replace(newInternalFile.Name, newName);
        int newTaeID = GetNewTAEID(newInternalFile.TAE.ID);

        newInternalFile.Name = newName;
        newInternalFile.Filepath = newFilePath;
        newInternalFile.TAE.ID = newTaeID;
        newInternalFile.MarkForAddition = true;

        // Inserts the new internal file at the right position in the list
        for (int i = 0; i < SelectionHandler.ContainerInfo.InternalFiles.Count; i++)
        {
            var curFile = SelectionHandler.ContainerInfo.InternalFiles[i];
            int curId = int.Parse(curFile.Name.Substring(1));
            if (curId == (newId-1))
            {
                SelectionHandler.ContainerInfo.InternalFiles.Insert(i+1, newInternalFile);
                break;
            }
        }

        SelectionHandler.ContainerInfo.InternalFiles.Sort();

        SelectionHandler.ResetOnTimeActChange();
    }

    public void DeleteTimeAct()
    {
        if (SelectionHandler.CurrentTimeAct == null)
            return;

        if (SelectionHandler.CurrentTimeActKey == -1)
            return;

        SelectionHandler.ContainerInfo.IsModified = true;

        var curInternalFile = SelectionHandler.ContainerInfo.InternalFiles[SelectionHandler.CurrentTimeActKey];
        curInternalFile.MarkForRemoval = true;

        SelectionHandler.ResetOnTimeActChange();
    }

    public void DuplicateAnimation()
    {
        if (SelectionHandler.CurrentTimeActAnimation == null)
            return;

        if (SelectionHandler.CurrentTimeActAnimationIndex == -1)
            return;

        SelectionHandler.ContainerInfo.IsModified = true;

        var multiselect = SelectionHandler.TimeActAnimationMultiselect;

        List<TAE.Animation> targetAnims = new();

        for(int i = 0; i < SelectionHandler.CurrentTimeAct.Animations.Count; i++)
        {
            var targetAnim = SelectionHandler.CurrentTimeAct.Animations[i];
            if(multiselect._storedIndices.Contains(i))
            {
                targetAnims.Add(targetAnim);
            }
        }

        List<TAE.Animation> newAnims = new();

        foreach (var anim in targetAnims)
        {
            long newID = 0;
            int insertIdx = 0;
            (newID, insertIdx) = GetNewAnimationID(anim.ID);

            var newAnim = TimeActUtils.CloneAnimation(anim);
            newAnim.ID = newID;

            SelectionHandler.CurrentTimeAct.Animations.Insert(insertIdx, newAnim);

            newAnims.Add(newAnim);
        }

        // Re-select last row at new index
        TimeActUtils.SelectNewAnimation(newAnims.Last());
    }

    public void DeleteAnimation()
    {
        if (SelectionHandler.CurrentTimeActAnimation == null)
            return;

        if (SelectionHandler.CurrentTimeActAnimationIndex == -1)
            return;

        SelectionHandler.ContainerInfo.IsModified = true;

        var multiselect = SelectionHandler.TimeActAnimationMultiselect;

        List<TAE.Animation> targetAnims = new();

        for (int i = 0; i < SelectionHandler.CurrentTimeAct.Animations.Count; i++)
        {
            var targetAnim = SelectionHandler.CurrentTimeAct.Animations[i];
            if (multiselect._storedIndices.Contains(i))
            {
                targetAnims.Add(targetAnim);
            }
        }

        foreach (var anim in targetAnims)
        {
            SelectionHandler.CurrentTimeAct.Animations.Remove(anim);
        }

        SelectionHandler.CurrentTimeAct.Animations.Sort();
        SelectionHandler.ResetOnTimeActAnimationChange();
    }
    public void CreateEvent()
    {
        if (SelectionHandler.CurrentTimeActEvent == null)
            return;

        if (SelectionHandler.CurrentTimeActEventIndex == -1)
            return;

        SelectionHandler.ContainerInfo.IsModified = true;

        ShowCreateEventModal = true;

        // Ignore multi-select for this, create should always be one discrete event
    }

    public void DuplicateEvent()
    {
        if (SelectionHandler.CurrentTimeActEvent == null)
            return;

        if (SelectionHandler.CurrentTimeActEventIndex == -1)
            return;

        SelectionHandler.ContainerInfo.IsModified = true;

        var indices = Screen.SelectionHandler.TimeActEventMultiselect._storedIndices;
        var animations = Smithbox.EditorHandler.TimeActEditor.SelectionHandler.CurrentTimeActAnimation;

        var lastEventIdx = -1;

        // Single
        if(indices.Count <= 1)
        {
            var curEvent = animations.Events[indices.First()];
            var insertIdx = animations.Events.IndexOf(curEvent) + 1;
            var dupeEvent = curEvent.GetClone(false);

            var action = new TimeActDuplicateEvent(dupeEvent, animations.Events, insertIdx);
            EditorActionManager.ExecuteAction(action);
        }
        // Multi-Select
        else
        {
            List<int> insertIndices = new List<int>();
            List<TAE.Event> newEvents = new List<TAE.Event>();

            for (int i = 0; i < animations.Events.Count; i++)
            {
                if (indices.Contains(i))
                {
                    var curEvent = animations.Events[i];
                    var insertIdx = animations.Events.IndexOf(curEvent) + 1;
                    insertIndices.Add(insertIdx);
                    var dupeEvent = curEvent.GetClone(false);
                    newEvents.Add(dupeEvent);

                    lastEventIdx = i;
                }
            }

            var action = new TimeActMultiDuplicateEvent(newEvents, animations.Events, insertIndices);
            EditorActionManager.ExecuteAction(action);

            // Select last newly duplicated event
            if (lastEventIdx != -1)
            {
                TimeActUtils.SelectNewEvent(lastEventIdx);
            }
        }
    }

    public void DeleteEvent()
    {
        if (SelectionHandler.CurrentTimeActEvent == null)
            return;

        if (SelectionHandler.CurrentTimeActEventIndex == -1)
            return;

        SelectionHandler.ContainerInfo.IsModified = true;

        var indices = Screen.SelectionHandler.TimeActEventMultiselect._storedIndices;
        var animations = Smithbox.EditorHandler.TimeActEditor.SelectionHandler.CurrentTimeActAnimation;
        var editorActions = new List<EditorAction>();

        TAE.Event firstEvent = null;
        var firstEventIdx = -1;

        for (int i = 0; i < animations.Events.Count; i++)
        {
            if (indices.Contains(i))
            {
                var curEvent = animations.Events[i];

                if (firstEventIdx == -1)
                {
                    firstEvent = curEvent;
                    firstEventIdx = i;
                }

                var removeIdx = animations.Events.IndexOf(curEvent);
                var storedEvent = curEvent.GetClone(false);

                var action = new TimeActDeleteEvent(storedEvent, animations.Events, removeIdx);
                editorActions.Add(action);
            }
        }

        var compoundAction = new CompoundAction(editorActions);
        EditorActionManager.ExecuteAction(compoundAction);

        if(firstEventIdx != -1 && firstEventIdx != 0)
        {
            var newIndex = firstEventIdx - 1;
            Screen.SelectionHandler.CurrentTimeActEvent = firstEvent;
            Screen.SelectionHandler.CurrentTimeActEventIndex = newIndex;
            Screen.SelectionHandler.TimeActEventMultiselect._storedIndices.Add(newIndex);
        }
    }

    public void OrderAnimation()
    {
        if (SelectionHandler.CurrentTimeActAnimation == null)
            return;

        if (SelectionHandler.CurrentTimeActAnimationIndex == -1)
            return;

        SelectionHandler.ContainerInfo.IsModified = true;
    }

    public void OrderEvent()
    {
        if (SelectionHandler.CurrentTimeActEvent == null)
            return;

        if (SelectionHandler.CurrentTimeActEventIndex == -1)
            return;

        SelectionHandler.ContainerInfo.IsModified = true;
    }

    public enum OrderType
    {
        Up,
        Down,
        Top,
        Bottom,
        Sort
    }

    // Utility
    public (int, string) GetNewFileName(int id)
    {
        var trackedId = id;
        string newName = $"a{PadFileName(trackedId)}";

        // If there are matches, keep incrementing
        foreach (var file in SelectionHandler.ContainerInfo.InternalFiles)
        {
            if (file.Name == newName)
            {
                trackedId = trackedId + 1;
                newName = $"a{PadFileName(trackedId)}";
            }
        }

        return (trackedId, newName);
    }

    public string PadFileName(int id)
    {
        var str = "";

        if(id < 10)
        {
            str = "0";
        }

        return $"{str}{id}";
    }

    public int GetNewTAEID(int id)
    {
        int newID = id + 1;

        // If there are matches, keep incrementing
        foreach (var file in SelectionHandler.ContainerInfo.InternalFiles)
        {
            if (file.TAE.ID == newID)
            {
                newID = newID + 1;
            }
        }

        return newID;
    }

    public (long, int) GetNewAnimationID(long id)
    {
        long newID = id + 1;
        int insertIdx = 0;

        // If there are matches, keep incrementing
        for (int i = 0; i < SelectionHandler.CurrentTimeAct.Animations.Count; i++)
        {
            var anim = SelectionHandler.CurrentTimeAct.Animations[i];

            if (anim.ID == newID)
            {
                insertIdx = i;
                newID = newID + 1;
            }
        }

        return (newID, insertIdx);
    }
}

