using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.TimeActEditor.Bank;
using StudioCore.Editors.TimeActEditor.Enums;
using StudioCore.Editors.TimeActEditor.Utils;
using StudioCore.Formats.JSON;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActSelection
{
    public TimeActEditorScreen Editor;
    public ProjectEntry Project;

    public FileDictionaryEntry SelectedFileEntry;
    public BinderContents SelectedBinder;
    public string SelectedFileKey;

    public string CurrentTimeActKey;
    public TAE CurrentTimeAct;
    public int CurrentTimeActIndex = -1;

    public TAE.Animation CurrentTimeActAnimation;
    public TransientAnimHeader CurrentTemporaryAnimHeader;
    public int CurrentTimeActAnimationIndex = -1;

    public TAE.Event CurrentTimeActEvent;
    public int CurrentTimeActEventIndex = -1;

    public string CurrentTimeActEventProperty;
    public int CurrentTimeActEventPropertyIndex = -1;

    public TimeActTemplateType CurrentTimeActType = TimeActTemplateType.Character;

    public TimeActEditorContext CurrentWindowContext = TimeActEditorContext.None;

    public bool FocusContainer = false;
    public bool FocusTimeAct = false;
    public bool FocusAnimation = false;
    public bool FocusEvent = false;

    public SortedDictionary<int, TAE> StoredTimeActs = new();
    public SortedDictionary<int, TAE.Animation> StoredAnimations = new();
    public SortedDictionary<int, TAE.Event> StoredEvents = new();

    public bool SelectFile = false;
    public bool SelectObjContainer = false;
    public bool SelectTimeAct = false;
    public bool SelectAnimation = false;
    public bool SelectEvent = false;
    public bool SelectFirstEvent = false;

    public TimeActSelection(TimeActEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void ResetSelection()
    {
        SelectedFileEntry = null;
        SelectedFileKey = "";
        SelectedBinder = null;

        CurrentTimeActKey = "";
        CurrentTimeAct = null;

        CurrentTimeActAnimation = null;
        CurrentTimeActAnimationIndex = -1;
        CurrentTemporaryAnimHeader = null;

        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;

        Reset(false, false, true);
    }

    public void Reset(bool resetTimeAct = false, bool resetAnimation = false, bool resetEvent = false)
    {
        if (resetTimeAct)
        {
            StoredTimeActs.Clear();
        }

        if (resetAnimation)
        {
            StoredAnimations.Clear();
        }

        if (resetEvent)
        {
            StoredEvents.Clear();
        }
    }

    public void FileContainerChange(FileDictionaryEntry entry, BinderContents binder)
    {
        SelectedFileEntry = entry;
        SelectedFileKey = entry.Filename;
        SelectedBinder = binder;

        CurrentTimeActKey = "";
        CurrentTimeAct = null;

        CurrentTimeActAnimation = null;
        CurrentTimeActAnimationIndex = -1;
        CurrentTemporaryAnimHeader = null;

        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;

        Reset(true, true, true);

        if(SelectedBinder != null && SelectedBinder.Files.Count > 0)
        {
            var firstEntry = SelectedBinder.Files.First();

            TimeActChange(firstEntry.Key.Name, firstEntry.Value, 0);
        }
    }

    public void ResetOnTimeActChange()
    {
        CurrentTimeActKey = "";
        CurrentTimeAct = null;

        CurrentTimeActAnimation = null;
        CurrentTimeActAnimationIndex = -1;
        CurrentTemporaryAnimHeader = null;

        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;

        Reset(true, true, true);
    }

    public void TimeActChange(string filename, TAE entry, int index)
    {
        CurrentTimeActKey = filename;
        CurrentTimeAct = entry;
        CurrentTimeActIndex = index;

        CurrentTimeActAnimation = null;
        CurrentTemporaryAnimHeader = null;
        CurrentTimeActAnimationIndex = -1;

        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;

        Reset(false, true, true);

        TimeActUtils.ApplyTemplate(Editor, CurrentTimeAct, CurrentTimeActType);

        // Auto-Select first Animation if not empty
        if (CurrentTimeAct.Animations.Count > 0)
        {
            var anim = CurrentTimeAct.Animations[0];
            TimeActAnimationChange(anim, 0);
        }
    }

    public void ResetOnTimeActAnimationChange()
    {
        CurrentTimeActAnimation = null;
        CurrentTimeActAnimationIndex = -1;
        CurrentTemporaryAnimHeader = null;

        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;

        Reset(false, true, true);
    }

    public void TimeActAnimationChange(TAE.Animation entry, int index)
    {
        AnimationSelection(CurrentTimeActAnimationIndex, index);

        CurrentTimeActAnimation = entry;
        CurrentTimeActAnimationIndex = index;
        CurrentTemporaryAnimHeader = null;

        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;

        // If a filter is active, auto-select first result (if any), since this is more user-friendly
        if(Editor.Filters._timeActEventFilterString != "")
        {
            SelectFirstEvent = true;
        }

        Reset(false, false, true);

        // Auto-Select first Event if not empty
        if (CurrentTimeActAnimation.Events.Count > 0)
        {
            var evt = CurrentTimeActAnimation.Events[0];
            TimeActEventChange(evt, 0);
        }
    }

    public void ResetOnTimeActEventChange()
    {
        CurrentTimeActEvent = null;
        CurrentTimeActEventIndex = -1;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;

        Reset(false, false, true);
    }

    public void TimeActEventChange(TAE.Event entry, int index)
    {
        EventSelection(CurrentTimeActEventIndex, index);

        CurrentTimeActEvent = entry;
        CurrentTimeActEventIndex = index;

        CurrentTimeActEventProperty = null;
        CurrentTimeActEventPropertyIndex = -1;
    }

    public void TimeActEventPropertyChange(string entry, int index)
    {
        CurrentTimeActEventProperty = entry;
        CurrentTimeActEventPropertyIndex = index;
    }

    public bool HasSelectedBinder()
    {
        return SelectedBinder != null;
    }

    public bool HasSelectedTimeAct()
    {
        return CurrentTimeAct != null;
    }

    public bool HasSelectedTimeActAnimation()
    {
        return CurrentTimeActAnimation != null;
    }

    public bool HasSelectedTimeActEvent()
    {
        return CurrentTimeActEvent != null;
    }

    public bool IsTimeActSelected(int index)
    {
        if (StoredTimeActs.ContainsKey(index))
            return true;

        return false;
    }
    public bool IsAnimationSelected(int index)
    {
        if (StoredAnimations.ContainsKey(index))
            return true;

        return false;
    }
    public bool IsEventSelected(int index)
    {
        if (StoredEvents.ContainsKey(index))
            return true;

        return false;
    }

    public void AnimationSelection(int currentSelectionIndex, int currentIndex)
    {
        var animation = Editor.Selection.CurrentTimeAct.Animations[currentIndex];

        // Multi-Select: Range Select
        if (InputTracker.GetKey(Veldrid.Key.LShift))
        {
            var start = currentSelectionIndex;
            var end = currentIndex;

            if (end < start)
            {
                start = currentIndex;
                end = currentSelectionIndex;
            }

            for (int k = start; k <= end; k++)
            {
                if (!StoredAnimations.ContainsKey(k))
                    StoredAnimations.Add(k, animation);
            }
        }
        // Multi-Select Mode
        else if (InputTracker.GetKey(KeyBindings.Current.TIMEACT_Multiselect))
        {
            if (StoredAnimations.ContainsKey(currentIndex) && StoredAnimations.Count > 1)
            {
                StoredAnimations.Remove(currentIndex);
            }
            else
            {
                if (!StoredAnimations.ContainsKey(currentIndex))
                    StoredAnimations.Add(currentIndex, animation);
            }
        }
        // Reset Multi-Selection if normal selection occurs
        else
        {
            StoredAnimations.Clear();
            StoredAnimations.Add(currentIndex, animation);
        }
    }

    public void EventSelection(int currentSelectionIndex, int currentIndex)
    {
        var animEvent = Editor.Selection.CurrentTimeActAnimation.Events[currentIndex];

        // Multi-Select: Range Select
        if (InputTracker.GetKey(Veldrid.Key.LShift))
        {
            var start = currentSelectionIndex;
            var end = currentIndex;

            if (end < start)
            {
                start = currentIndex;
                end = currentSelectionIndex;
            }

            for (int k = start; k <= end; k++)
            {
                if (!StoredEvents.ContainsKey(k))
                {
                    StoredEvents.Add(k, animEvent);
                }
            }
        }
        // Multi-Select Mode
        else if (InputTracker.GetKey(KeyBindings.Current.TIMEACT_Multiselect))
        {
            if (StoredEvents.ContainsKey(currentIndex) && StoredEvents.Count > 1)
            {
                StoredEvents.Remove(currentIndex);
            }
            else
            {
                if (!StoredEvents.ContainsKey(currentIndex))
                    StoredEvents.Add(currentIndex, animEvent);
            }
        }
        // Reset Multi-Selection if normal selection occurs
        else
        {
            StoredEvents.Clear();
            StoredEvents.Add(currentIndex, animEvent);
        }
    }
    /// <summary>
    /// Switches the focus context to the passed value.
    /// Use this on all windows (e.g. both Begin and BeginChild)
    /// </summary>
    public void SwitchWindowContext(TimeActEditorContext newContext)
    {
        if (ImGui.IsWindowHovered())
        {
            CurrentWindowContext = newContext;
            //TaskLogs.AddLog($"Context: {newContext.GetDisplayName()}");
        }
    }
}
