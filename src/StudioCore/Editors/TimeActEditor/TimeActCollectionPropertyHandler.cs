using DotNext.Collections.Generic;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Editors.TimeActEditor.Bank;
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

    public bool ShowCreateEventModal = false;
    private TAE.Template.EventTemplate CurrentEvent;

    public TimeActCollectionPropertyHandler(ActionManager editorActionManager, TimeActEditorScreen screen, TimeActDecorator decorator)
    {
        EditorActionManager = editorActionManager;
        Screen = screen;
        Decorator = decorator;
        SelectionHandler = screen.SelectionHandler;

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

            TAE.Event curEvent = Smithbox.EditorHandler.TimeActEditor.SelectionHandler.CurrentTimeActEvent;
            TAE.Template curTemplate = TimeActUtils.GetRelevantTemplate(TimeActUtils.TemplateType.Character);

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

                    TAE.Animation animation = Smithbox.EditorHandler.TimeActEditor.SelectionHandler.CurrentTimeActAnimation;
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
        if (SelectionHandler.CurrentTimeAct == null)
            return;

        if (SelectionHandler.CurrentTimeActKey == -1)
            return;

        SelectionHandler.ContainerInfo.IsModified = true;

        InternalTimeActWrapper curInternalFile = SelectionHandler.ContainerInfo.InternalFiles[SelectionHandler.CurrentTimeActKey];
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
        for (int i = 0; i < SelectionHandler.ContainerInfo.InternalFiles.Count; i++)
        {
            InternalTimeActWrapper curFile = SelectionHandler.ContainerInfo.InternalFiles[i];
            int curId = int.Parse(curFile.Name.Substring(1));

            if (curId == (newId-1))
            {
                EditorActionManager.ExecuteAction(new TimeActDuplicate(newInternalFile, SelectionHandler.ContainerInfo.InternalFiles, i + 1));
                break;
            }
        }

        SelectionHandler.ContainerInfo.InternalFiles.Sort();

        SelectionHandler.ResetOnTimeActChange();
    }

    /// <summary>
    /// Delete currently selected Time Act
    /// </summary>
    public void DeleteTimeAct()
    {
        if (SelectionHandler.CurrentTimeAct == null)
            return;

        if (SelectionHandler.CurrentTimeActKey == -1)
            return;

        SelectionHandler.ContainerInfo.IsModified = true;

        InternalTimeActWrapper curInternalFile = SelectionHandler.ContainerInfo.InternalFiles[SelectionHandler.CurrentTimeActKey];

        EditorActionManager.ExecuteAction(new TimeActDelete(curInternalFile));

        SelectionHandler.ResetOnTimeActChange();
    }

    /// <summary>
    /// Duplicate currently selected TAE.Animations
    /// </summary>
    public void DuplicateAnimation()
    {
        if (SelectionHandler.CurrentTimeActAnimation == null)
            return;

        if (SelectionHandler.CurrentTimeActAnimationIndex == -1)
            return;

        SelectionHandler.ContainerInfo.IsModified = true;

        TAE timeact = Smithbox.EditorHandler.TimeActEditor.SelectionHandler.CurrentTimeAct;
        SortedDictionary<int, TAE.Animation> storedAnims = Screen.SelectionHandler.TimeActMultiselect.StoredAnimations;
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
        if (SelectionHandler.CurrentTimeActAnimation == null)
            return;

        if (SelectionHandler.CurrentTimeActAnimationIndex == -1)
            return;

        SelectionHandler.ContainerInfo.IsModified = true;

        SortedDictionary<int, TAE.Animation> storedAnims = Screen.SelectionHandler.TimeActMultiselect.StoredAnimations;
        TAE timeact = Smithbox.EditorHandler.TimeActEditor.SelectionHandler.CurrentTimeAct;

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

        Screen.SelectionHandler.TimeActMultiselect.Reset(false, true, true);
    }

    /// <summary>
    /// Create new TAE.Event
    /// </summary>
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

    /// <summary>
    /// Duplicate currently selected TAE.Events
    /// </summary>
    public void DuplicateEvent()
    {
        if (SelectionHandler.CurrentTimeActEvent == null)
            return;

        if (SelectionHandler.CurrentTimeActEventIndex == -1)
            return;

        SelectionHandler.ContainerInfo.IsModified = true;

        SortedDictionary<int, TAE.Event> storedEvents = Screen.SelectionHandler.TimeActMultiselect.StoredEvents;
        TAE.Animation animations = Smithbox.EditorHandler.TimeActEditor.SelectionHandler.CurrentTimeActAnimation;

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
        if (SelectionHandler.CurrentTimeActEvent == null)
            return;

        if (SelectionHandler.CurrentTimeActEventIndex == -1)
            return;

        SelectionHandler.ContainerInfo.IsModified = true;

        SortedDictionary<int, TAE.Event> storedEvents = Screen.SelectionHandler.TimeActMultiselect.StoredEvents;
        TAE.Animation animations = Smithbox.EditorHandler.TimeActEditor.SelectionHandler.CurrentTimeActAnimation;

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

        Screen.SelectionHandler.TimeActMultiselect.Reset(false, false, true);
    }

    /// <summary>
    /// Order currently selected TAE.Animations
    /// </summary>
    public void OrderAnimation()
    {
        if (SelectionHandler.CurrentTimeActAnimation == null)
            return;

        if (SelectionHandler.CurrentTimeActAnimationIndex == -1)
            return;

        SelectionHandler.ContainerInfo.IsModified = true;
    }

    /// <summary>
    /// Order currently selected TAE.Events
    /// </summary>
    public void OrderEvent()
    {
        if (SelectionHandler.CurrentTimeActEvent == null)
            return;

        if (SelectionHandler.CurrentTimeActEventIndex == -1)
            return;

        SelectionHandler.ContainerInfo.IsModified = true;
    }

    /// <summary>
    /// Return new Time Act filename based on current Time Acts.
    /// </summary>
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
        foreach (InternalTimeActWrapper file in SelectionHandler.ContainerInfo.InternalFiles)
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
        for (int i = 0; i < SelectionHandler.CurrentTimeAct.Animations.Count; i++)
        {
            TAE.Animation anim = SelectionHandler.CurrentTimeAct.Animations[i];

            if (anim.ID == newID)
            {
                insertIdx = i;
                newID = newID + 1;
            }
        }

        return (newID, insertIdx);
    }

    public enum OrderType
    {
        Up,
        Down,
        Top,
        Bottom,
        Sort
    }
}

