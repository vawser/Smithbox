using SoulsFormats;
using StudioCore.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor;

public class TimeActMultiselect
{
    public SortedDictionary<int, TAE> StoredTimeActs = new();
    public SortedDictionary<int, TAE.Animation> StoredAnimations = new();
    public SortedDictionary<int, TAE.Event> StoredEvents = new();

    private TimeActEditorScreen Screen;

    public TimeActMultiselect(TimeActEditorScreen screen) 
    {
        Screen = screen;
    }

    public void Reset(bool resetTimeAct = false, bool resetAnimation = false, bool resetEvent = false)
    {
        if(resetTimeAct)
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

    public void TimeActSelection(int currentSelectionIndex, int currentIndex)
    {
        var timeAct = Screen.SelectionHandler.ContainerInfo.InternalFiles[currentIndex].TAE;

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
                if (!StoredTimeActs.ContainsKey(k))
                    StoredTimeActs.Add(k, timeAct);
            }
        }
        // Multi-Select Mode
        else if (InputTracker.GetKey(KeyBindings.Current.TIMEACT_Multiselect))
        {
            if (StoredTimeActs.ContainsKey(currentIndex) && StoredTimeActs.Count > 1)
            {
                StoredTimeActs.Remove(currentIndex);
            }
            else
            {
                if (!StoredTimeActs.ContainsKey(currentIndex))
                    StoredTimeActs.Add(currentIndex, timeAct);
            }
        }
        // Reset Multi-Selection if normal selection occurs
        else
        {
            StoredTimeActs.Clear();
            StoredTimeActs.Add(currentIndex, timeAct);
        }
    }

    public void AnimationSelection(int currentSelectionIndex, int currentIndex)
    {
        var animation = Screen.SelectionHandler.CurrentTimeAct.Animations[currentIndex];

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
        var animEvent = Screen.SelectionHandler.CurrentTimeActAnimation.Events[currentIndex];

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
}
