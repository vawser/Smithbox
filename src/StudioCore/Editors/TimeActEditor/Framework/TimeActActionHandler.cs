using DotNext.Collections.Generic;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Editors.TimeActEditor.Actions;
using StudioCore.Editors.TimeActEditor.Bank;
using StudioCore.Editors.TimeActEditor.Enums;
using StudioCore.Editors.TimeActEditor.Utils;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.DRB;
using static SoulsFormats.FFXDLSE;
using static StudioCore.Editors.TimeActEditor.Bank.TimeActBank;
using static StudioCore.Editors.TimeActEditor.TimeActSelectionManager;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Handles property edits for collection fields
/// </summary>
public class TimeActActionHandler
{
    private ActionManager EditorActionManager;
    private TimeActEditorScreen Screen;

    public bool ShowCreateEventModal = false;
    private TAE.Template.EventTemplate CurrentEvent;

    public TimeActActionHandler(TimeActEditorScreen screen)
    {
        Screen = screen;
        EditorActionManager = screen.EditorActionManager;
    }

    /// <summary>
    /// GUI loop
    /// </summary>
    public void OnGui()
    {
        if (ShowCreateEventModal)
        {
            ImGui.OpenPopup("Create Event");
        }

        CreateEventModal();
    }
    public void DetermineCreateTarget()
    {
        var handler = Screen.ActionHandler;
        var context = Screen.Selection.CurrentWindowContext;

        switch (context)
        {
            case TimeActEditorContext.File: 
                break;
            case TimeActEditorContext.TimeAct:
                break;
            case TimeActEditorContext.Animation:
                break;
            case TimeActEditorContext.AnimationProperty:
                break;
            case TimeActEditorContext.Event:
                Screen.ActionHandler.CreateEvent();
                break;
            case TimeActEditorContext.EventProperty:
                break;
        }
    }

    public void DetermineDuplicateTarget()
    {
        var handler = Screen.ActionHandler;
        var context = Screen.Selection.CurrentWindowContext;

        switch (context)
        {
            case TimeActEditorContext.File: 
                break;
            case TimeActEditorContext.TimeAct:
                Screen.ActionHandler.DuplicateTimeAct();
                break;
            case TimeActEditorContext.Animation:
                Screen.ActionHandler.DuplicateAnimation();
                break;
            case TimeActEditorContext.AnimationProperty:
                break;
            case TimeActEditorContext.Event:
                Screen.ActionHandler.DuplicateEvent();
                break;
            case TimeActEditorContext.EventProperty:
                break;
        }
    }
    public void DetermineDeleteTarget()
    {
        var handler = Screen.ActionHandler;
        var context = Screen.Selection.CurrentWindowContext;

        switch (context)
        {
            case TimeActEditorContext.File: 
                break;
            case TimeActEditorContext.TimeAct:
                Screen.ActionHandler.DeleteTimeAct();
                break;
            case TimeActEditorContext.Animation:
                Screen.ActionHandler.DeleteAnimation();
                break;
            case TimeActEditorContext.AnimationProperty:
                break;
            case TimeActEditorContext.Event:
                Screen.ActionHandler.DeleteEvent();
                break;
            case TimeActEditorContext.EventProperty:
                break;
        }
    }

    private string _eventTypeCreateSearchStr = "";

    /// <summary>
    /// Display TAE.Event creation modal.
    /// </summary>
    private void CreateEventModal()
    {
        // Create Event Popup
        if (ImGui.BeginPopupModal("Create Event", ref ShowCreateEventModal, ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.AlwaysAutoResize))
        {
            Vector2 listboxSize = new Vector2(520, 400);
            Vector2 buttonSize = new Vector2(520 * 0.5f, 24);

            TAE.Event curEvent = Screen.Selection.CurrentTimeActEvent;
            TAE.Template curTemplate = TimeActUtils.GetRelevantTemplate(TimeActTemplateType.Character);

            if (curEvent != null && curTemplate != null)
            {
                ImGui.Text("Event Types:");

                ImGui.SetNextItemWidth(listboxSize.X);
                ImGui.InputText("##eventTypeSearch", ref _eventTypeCreateSearchStr, 255);

                if (ImGui.BeginListBox("##eventTypes", listboxSize))
                {
                    foreach (var entry in curTemplate.Events)
                    {
                        TAE.Template.EventTemplate eventType = entry.Value;

                        if (SearchFilters.IsBasicMatch(_eventTypeCreateSearchStr, eventType.Name) ||
                           SearchFilters.IsBasicMatch(_eventTypeCreateSearchStr, $"{eventType.ID}"))
                        {
                            if (ImGui.Selectable($"[{eventType.ID}] {eventType.Name}##eventEntry{eventType.ID}", eventType == CurrentEvent))
                            {
                                CurrentEvent = eventType;
                            }
                        }
                    }

                    ImGui.EndListBox();
                }

                if (ImGui.Button("Create", buttonSize))
                {
                    TAE.Event newEvent = new TAE.Event(curEvent.StartTime, curEvent.EndTime, CurrentEvent.ID, curEvent.Unk04, false, CurrentEvent);
                    newEvent.Group = curEvent.Group;

                    TAE.Animation animation = Screen.Selection.CurrentTimeActAnimation;
                    int insertIdx = animation.Events.IndexOf(curEvent) + 1;

                    EditorActionManager.ExecuteAction(new TaeEventCreate(newEvent, animation.Events, insertIdx));

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

    /// <summary>
    /// Duplicate currently selected Time Act
    /// </summary>
    public void DuplicateTimeAct()
    {
        if (Screen.Selection == null)
            return;

        if (Screen.Selection.CurrentTimeAct == null)
            return;

        if (Screen.Selection.CurrentTimeActKey == -1)
            return;

        Screen.Selection.ContainerInfo.IsModified = true;

        InternalTimeActWrapper curInternalFile = Screen.Selection.ContainerInfo.InternalFiles[Screen.Selection.CurrentTimeActKey];
        InternalTimeActWrapper newInternalFile = new InternalTimeActWrapper(curInternalFile.Filepath, curInternalFile.TAE.Clone());

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
        for (int i = 0; i < Screen.Selection.ContainerInfo.InternalFiles.Count; i++)
        {
            InternalTimeActWrapper curFile = Screen.Selection.ContainerInfo.InternalFiles[i];
            int curId = int.Parse(curFile.Name.Substring(1));

            if (curId == newId)
            {
                EditorActionManager.ExecuteAction(new TimeActDuplicate(newInternalFile, Screen.Selection.ContainerInfo.InternalFiles, i + 1));
                break;
            }
        }

        Screen.Selection.ContainerInfo.InternalFiles.Sort();

        Screen.Selection.ResetOnTimeActChange();
    }

    /// <summary>
    /// Delete currently selected Time Act
    /// </summary>
    public void DeleteTimeAct()
    {
        if (Screen.Selection == null)
            return;

        if (Screen.Selection.CurrentTimeAct == null)
            return;

        if (Screen.Selection.CurrentTimeActKey == -1)
            return;

        Screen.Selection.ContainerInfo.IsModified = true;

        InternalTimeActWrapper curInternalFile = Screen.Selection.ContainerInfo.InternalFiles[Screen.Selection.CurrentTimeActKey];

        EditorActionManager.ExecuteAction(new TimeActDelete(curInternalFile));

        Screen.Selection.ResetOnTimeActChange();
    }

    /// <summary>
    /// Duplicate currently selected TAE.Animations
    /// </summary>
    public void DuplicateAnimation()
    {
        if (Screen.Selection == null)
            return;

        if (Screen.Selection.CurrentTimeActAnimation == null)
            return;

        if (Screen.Selection.CurrentTimeActAnimationIndex == -1)
            return;

        Screen.Selection.ContainerInfo.IsModified = true;

        TAE timeact = Screen.Selection.CurrentTimeAct;
        SortedDictionary<int, TAE.Animation> storedAnims = Screen.Selection.StoredAnimations;
        int lastAnimIdx = -1;

        if (storedAnims.Count <= 1)
        {
            TAE.Animation curAnim = timeact.Animations[storedAnims.First().Key];
            int insertIdx = timeact.Animations.IndexOf(curAnim);
            TAE.Animation dupeAnim = TimeActUtils.CloneAnimation(curAnim);

            EditorActionManager.ExecuteAction(new TaeAnimDuplicate(dupeAnim, timeact.Animations, insertIdx));

            timeact.Animations.Sort();
        }
        else
        {
            List<int> insertIndices = new List<int>();
            List<TAE.Animation> newAnims = new List<TAE.Animation>();

            for (int i = 0; i < timeact.Animations.Count; i++)
            {
                if (storedAnims.ContainsKey(i))
                {
                    TAE.Animation curAnim = timeact.Animations[i];
                    int insertIdx = timeact.Animations.IndexOf(curAnim);
                    TAE.Animation dupeAnim = TimeActUtils.CloneAnimation(curAnim);

                    long newID = 0;
                    (newID, insertIdx) = GetNewAnimationID(dupeAnim.ID);
                    dupeAnim.ID = newID;

                    insertIndices.Add(insertIdx);
                    newAnims.Add(dupeAnim);

                    lastAnimIdx = insertIdx;
                }
            }

            EditorActionManager.ExecuteAction(new TaeAnimMultiDuplicate(newAnims, timeact.Animations, insertIndices));

            // Select last newly duplicated event
            if (lastAnimIdx != -1)
            {
                TimeActUtils.SelectNewAnimation(lastAnimIdx);
            }

            timeact.Animations.Sort();
        }
    }

    /// <summary>
    /// Delete currently selected TAE.Animations
    /// </summary>
    public void DeleteAnimation()
    {
        if (Screen.Selection == null)
            return;

        if (Screen.Selection.CurrentTimeActAnimation == null)
            return;

        if (Screen.Selection.CurrentTimeActAnimationIndex == -1)
            return;

        Screen.Selection.ContainerInfo.IsModified = true;

        SortedDictionary<int, TAE.Animation> storedAnims = Screen.Selection.StoredAnimations;
        TAE timeact = Screen.Selection.CurrentTimeAct;

        // Single
        if (storedAnims.Count <= 1)
        {
            TAE.Animation curAnim = timeact.Animations[storedAnims.First().Key];
            int removeIdx = timeact.Animations.IndexOf(curAnim);
            TAE.Animation storedAnim = TimeActUtils.CloneAnimation(curAnim);

            EditorActionManager.ExecuteAction(new TaeAnimDelete(storedAnim, timeact.Animations, removeIdx));
        }
        // Multi-Select
        else
        {
            List<int> removeIndices = new List<int>();
            List<TAE.Animation> removedAnims = new List<TAE.Animation>();

            for (int i = 0; i < timeact.Animations.Count; i++)
            {
                if (storedAnims.ContainsKey(i))
                {
                    TAE.Animation curAnim = timeact.Animations[i];
                    int removeIdx = timeact.Animations.IndexOf(curAnim);
                    TAE.Animation storedAnim = TimeActUtils.CloneAnimation(curAnim);

                    removeIndices.Add(removeIdx);
                    removedAnims.Add(storedAnim);
                }
            }

            EditorActionManager.ExecuteAction(new TaeAnimMultiDelete(removedAnims, timeact.Animations, removeIndices));
        }

        Screen.Selection.Reset(false, true, true);
    }

    /// <summary>
    /// Create new TAE.Event
    /// </summary>
    public void CreateEvent()
    {
        if (Screen.Selection == null)
            return;

        if (Screen.Selection.CurrentTimeActEvent == null)
            return;

        if (Screen.Selection.CurrentTimeActEventIndex == -1)
            return;

        Screen.Selection.ContainerInfo.IsModified = true;

        ShowCreateEventModal = true;

        // Ignore multi-select for this, create should always be one discrete event
    }

    /// <summary>
    /// Duplicate currently selected TAE.Events
    /// </summary>
    public void DuplicateEvent()
    {
        if (Screen.Selection == null)
            return;

        if (Screen.Selection.CurrentTimeActEvent == null)
            return;

        if (Screen.Selection.CurrentTimeActEventIndex == -1)
            return;

        Screen.Selection.ContainerInfo.IsModified = true;

        SortedDictionary<int, TAE.Event> storedEvents = Screen.Selection.StoredEvents;
        TAE.Animation animations = Screen.Selection.CurrentTimeActAnimation;

        int lastEventIdx = -1;

        // Single
        if(storedEvents.Count <= 1)
        {
            TAE.Event curEvent = animations.Events[storedEvents.First().Key];
            int insertIdx = animations.Events.IndexOf(curEvent);
            TAE.Event dupeEvent = curEvent.GetClone(false);

            EditorActionManager.ExecuteAction(new TaeEventDuplicate(dupeEvent, animations.Events, insertIdx));
        }
        // Multi-Select
        else
        {
            List<int> insertIndices = new List<int>();
            List<TAE.Event> newEvents = new List<TAE.Event>();

            for (int i = 0; i < animations.Events.Count; i++)
            {
                if (storedEvents.ContainsKey(i))
                {
                    TAE.Event curEvent = animations.Events[i];
                    int insertIdx = animations.Events.IndexOf(curEvent);
                    insertIndices.Add(insertIdx);
                    TAE.Event dupeEvent = curEvent.GetClone(false);
                    newEvents.Add(dupeEvent);

                    lastEventIdx = insertIdx;
                }
            }

            EditorActionManager.ExecuteAction(new TaeEventMultiDuplicate(newEvents, animations.Events, insertIndices));

            // Select last newly duplicated event
            if (lastEventIdx != -1)
            {
                TimeActUtils.SelectNewEvent(lastEventIdx);
            }
        }
    }

    /// <summary>
    /// Delete currently selected TAE.Events
    /// </summary>
    public void DeleteEvent()
    {
        if (Screen.Selection == null)
            return;

        if (Screen.Selection.CurrentTimeActEvent == null)
            return;

        if (Screen.Selection.CurrentTimeActEventIndex == -1)
            return;

        Screen.Selection.ContainerInfo.IsModified = true;

        SortedDictionary<int, TAE.Event> storedEvents = Screen.Selection.StoredEvents;
        TAE.Animation animations = Screen.Selection.CurrentTimeActAnimation;

        // Single
        if (storedEvents.Count <= 1)
        {
            TAE.Event curEvent = animations.Events[storedEvents.First().Key];

            int removeIdx = animations.Events.IndexOf(curEvent);
            TAE.Event storedEvent = curEvent.GetClone(false);

            EditorActionManager.ExecuteAction(new TaeEventDelete(storedEvent, animations.Events, removeIdx));
        }
        // Multi-Select
        else
        {
            List<int> removeIndices = new List<int>();
            List<TAE.Event> removedEvents = new List<TAE.Event>();

            for (int i = 0; i < animations.Events.Count; i++)
            {
                if (storedEvents.ContainsKey(i))
                {
                    TAE.Event curEvent = animations.Events[i];
                    int removeIdx = animations.Events.IndexOf(curEvent);
                    TAE.Event storedEvent = curEvent.GetClone(false);

                    removeIndices.Add(removeIdx);
                    removedEvents.Add(storedEvent);
                }
            }

            EditorActionManager.ExecuteAction(new TaeEventMultiDelete(removedEvents, animations.Events, removeIndices));
        }

        Screen.Selection.Reset(false, false, true);
    }

    /// <summary>
    /// Order currently selected TAE.Animations
    /// </summary>
    public void OrderAnimation()
    {
        if (Screen.Selection.CurrentTimeActAnimation == null)
            return;

        if (Screen.Selection.CurrentTimeActAnimationIndex == -1)
            return;

        Screen.Selection.ContainerInfo.IsModified = true;
    }

    /// <summary>
    /// Order currently selected TAE.Events
    /// </summary>
    public void OrderEvent()
    {
        if (Screen.Selection.CurrentTimeActEvent == null)
            return;

        if (Screen.Selection.CurrentTimeActEventIndex == -1)
            return;

        Screen.Selection.ContainerInfo.IsModified = true;
    }

    /// <summary>
    /// Return new Time Act filename based on current Time Acts.
    /// </summary>
    public (int, string) GetNewFileName(int id)
    {
        var trackedId = id;
        string newName = $"a{PadFileName(trackedId)}";

        // If there are matches, keep incrementing
        foreach (var file in Screen.Selection.ContainerInfo.InternalFiles)
        {
            if (file.Name == newName)
            {
                trackedId = trackedId + 1;
                newName = $"a{PadFileName(trackedId)}";
            }
        }

        return (trackedId, newName);
    }

    /// <summary>
    /// Returns ID padded with zeros to the start if below 10.
    /// </summary>
    public string PadFileName(int id)
    {
        var str = "";

        if(id < 10)
        {
            str = "0";
        }

        return $"{str}{id}";
    }

    /// <summary>
    /// Return new TAE ID based on the IDs within the current container.
    /// </summary>
    public int GetNewTAEID(int id)
    {
        int newID = id + 1;

        // If there are matches, keep incrementing
        foreach (InternalTimeActWrapper file in Screen.Selection.ContainerInfo.InternalFiles)
        {
            if (file.TAE.ID == newID)
            {
                newID = newID + 1;
            }
        }

        return newID;
    }

    /// <summary>
    /// Return new TAE.Animation ID based on current TAE.Animations
    /// </summary>
    public (long, int) GetNewAnimationID(long id)
    {
        long newID = id + 1;
        int insertIdx = 0;

        // If there are matches, keep incrementing
        for (int i = 0; i < Screen.Selection.CurrentTimeAct.Animations.Count; i++)
        {
            TAE.Animation anim = Screen.Selection.CurrentTimeAct.Animations[i];

            if (anim.ID == newID)
            {
                insertIdx = i;
                newID = newID + 1;
            }
        }

        return (newID, insertIdx);
    }
}

