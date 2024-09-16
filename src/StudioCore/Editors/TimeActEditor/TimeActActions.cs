using DotNext.Collections.Generic;
using HKLib.hk2018.hkHashMapDetail;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Editors.TimeActEditor.Bank;
using StudioCore.GraphicsEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using static SoulsFormats.GPARAM;
using static StudioCore.Editors.TimeActEditor.Bank.TimeActBank;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Action: TAE.Event.Parameters change
/// </summary>
public class TaeEventParametersChange : EditorAction
{
    private Dictionary<string, object> Parameters;
    private string ParamName;
    private object OldValue;
    private object NewValue;

    public TaeEventParametersChange(Dictionary<string, object> parameters, string paramName, object oldValue, object newValue, Type propertyType)
    {
        Parameters = parameters;
        ParamName = paramName;

        if(propertyType == typeof(string) )
        {
            OldValue = (string)oldValue;

            NewValue = (string)newValue;
        }
        else if (propertyType == typeof(byte))
        {
            OldValue = (byte)oldValue;

            if (newValue.GetType() != typeof(byte))
            {
                byte tValue;
                byte.TryParse(newValue.ToString(), out tValue);
                NewValue = tValue;
            }
            else
            {
                NewValue = (byte)newValue;
            }
        }
        else if (propertyType == typeof(sbyte))
        {
            OldValue = (sbyte)oldValue;

            if (newValue.GetType() != typeof(sbyte))
            {
                sbyte tValue;
                sbyte.TryParse(newValue.ToString(), out tValue);
                NewValue = tValue;
            }
            else
            {
                NewValue = (sbyte)newValue;
            }
        }
        else if (propertyType == typeof(short))
        {
            OldValue = (short)oldValue;

            if (newValue.GetType() != typeof(short))
            {
                short tValue;
                short.TryParse(newValue.ToString(), out tValue);
                NewValue = tValue;
            }
            else
            {
                NewValue = (short)newValue;
            }
        }
        else if (propertyType == typeof(ushort))
        {
            OldValue = (ushort)oldValue;

            if (newValue.GetType() != typeof(ushort))
            {
                ushort tValue;
                ushort.TryParse(newValue.ToString(), out tValue);
                NewValue = tValue;
            }
            else
            {
                NewValue = (ushort)newValue;
            }
        }
        else if (propertyType == typeof(int))
        {
            OldValue = (int)oldValue;

            if (newValue.GetType() != typeof(int))
            {
                int tValue;
                int.TryParse(newValue.ToString(), out tValue);
                NewValue = tValue;
            }
            else
            {
                NewValue = (int)newValue;
            }
        }
        else if (propertyType == typeof(uint))
        {
            OldValue = (uint)oldValue;

            if (newValue.GetType() != typeof(uint))
            {
                uint tValue;
                uint.TryParse(newValue.ToString(), out tValue);
                NewValue = tValue;
            }
            else
            {
                NewValue = (uint)newValue;
            }
        }
        else if (propertyType == typeof(long))
        {
            OldValue = (long)oldValue;

            if (newValue.GetType() != typeof(long))
            {
                long tValue;
                long.TryParse(newValue.ToString(), out tValue);
                NewValue = tValue;
            }
            else
            {
                NewValue = (long)newValue;
            }
        }
        else if (propertyType == typeof(ulong))
        {
            OldValue = (ulong)oldValue;

            if (newValue.GetType() != typeof(ulong))
            {
                ulong tValue;
                ulong.TryParse(newValue.ToString(), out tValue);
                NewValue = tValue;
            }
            else
            {
                NewValue = (ulong)newValue;
            }
        }
        else if (propertyType == typeof(float))
        {
            OldValue = (float)oldValue;

            if (newValue.GetType() != typeof(float))
            {
                float tValue;
                float.TryParse(newValue.ToString(), out tValue);
                NewValue = tValue;
            }
            else
            {
                NewValue = (float)newValue;
            }
        }
        else if (propertyType == typeof(double))
        {
            OldValue = (double)oldValue;

            if (newValue.GetType() != typeof(double))
            {
                double tValue;
                double.TryParse(newValue.ToString(), out tValue);
                NewValue = tValue;
            }
            else
            {
                NewValue = (double)newValue;
            }
        }
        else if (propertyType == typeof(bool))
        {
            OldValue = (bool)oldValue;

            if (newValue.GetType() != typeof(bool))
            {
                bool tValue;
                bool.TryParse(newValue.ToString(), out tValue);
                NewValue = tValue;
            }
            else
            {
                NewValue = (bool)newValue;
            }
        }
        else
        {
            NewValue = newValue;
        }
    }

    public override ActionEvent Execute()
    {
        Parameters[ParamName] = NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Parameters[ParamName] = OldValue;

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Action: TAE.Event.StartTime property change
/// </summary>
public class TaeEventStartTimeChange : EditorAction
{
    private TAE.Event Event;
    private object OldValue;
    private object NewValue;

    public TaeEventStartTimeChange(TAE.Event entry, object oldValue, object newValue)
    {
        Event = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute()
    {
        Event.StartTime = (float)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Event.StartTime = (float)OldValue;

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Action: TAE.Event.EndTime property change
/// </summary>
public class TaeEventEndTimeChange : EditorAction
{
    private TAE.Event Event;
    private object OldValue;
    private object NewValue;

    public TaeEventEndTimeChange(TAE.Event entry, object oldValue, object newValue)
    {
        Event = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute()
    {
        Event.EndTime = (float)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Event.EndTime = (float)OldValue;

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Action: TAE.Animation.EndTime property change
/// </summary>
public class TaeAnimEndTimeChange : EditorAction
{
    private TAE.Animation Animation;
    private object OldValue;
    private object NewValue;
    private TransientAnimHeader OldTempHeader;

    public TaeAnimEndTimeChange(TAE.Animation entry, object oldValue, object newValue, TransientAnimHeader tempHeader)
    {
        Animation = entry;
        OldValue = oldValue;
        NewValue = newValue;
        OldTempHeader = tempHeader;
    }

    public override ActionEvent Execute()
    {
        Animation.MiniHeader = (TAE.Animation.AnimMiniHeader)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Animation.MiniHeader = (TAE.Animation.AnimMiniHeader)OldValue;
        Smithbox.EditorHandler.TimeActEditor.SelectionHandler.CurrentTemporaryAnimHeader = OldTempHeader;

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Action: TAE.Animation.ID property change
/// </summary>
public class TaeAnimIdChange : EditorAction
{
    private TAE.Animation Animation;
    private object OldValue;
    private object NewValue;

    public TaeAnimIdChange(TAE.Animation entry, object oldValue, object newValue)
    {
        Animation = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute()
    {
        Animation.ID = (long)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Animation.ID = (long)OldValue;

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Action: TAE.Animation.AnimFileName property change
/// </summary>
public class TaeAnimFileNameChange : EditorAction
{
    private TAE.Animation Animation;
    private object OldValue;
    private object NewValue;

    public TaeAnimFileNameChange(TAE.Animation entry, object oldValue, object newValue)
    {
        Animation = entry;
        OldValue = oldValue;
        NewValue = newValue;
    }

    public override ActionEvent Execute()
    {
        Animation.AnimFileName = (string)NewValue;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        Animation.AnimFileName = (string)OldValue;

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Action: TAE.Animation duplicate
/// </summary>
public class TaeAnimDuplicate : EditorAction
{
    private TAE.Animation NewAnim;
    private List<TAE.Animation> AnimationList;
    private int InsertionIndex;

    public TaeAnimDuplicate(TAE.Animation newAnimation, List<TAE.Animation> animList, int index)
    {
        InsertionIndex = index;
        NewAnim = newAnimation;
        AnimationList = animList;
    }

    public override ActionEvent Execute()
    {
        AnimationList.Insert(InsertionIndex, NewAnim);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        AnimationList.RemoveAt(InsertionIndex);

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Action: TAE.Animation duplicate (multiple)
/// </summary>
public class TaeAnimMultiDuplicate : EditorAction
{
    private List<TAE.Animation> NewAnims;
    private List<TAE.Animation> AnimationList;
    private List<int> InsertionIndexes;

    public TaeAnimMultiDuplicate(List<TAE.Animation> newAnims, List<TAE.Animation> animList, List<int> indexList)
    {
        InsertionIndexes = indexList;
        NewAnims = newAnims;
        AnimationList = animList;
    }

    public override ActionEvent Execute()
    {
        for (int i = 0; i < InsertionIndexes.Count; i++)
        {
            TAE.Animation curNewAnim = NewAnims[i];
            int curIndex = InsertionIndexes[i];

            AnimationList.Insert(curIndex, curNewAnim);
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach (TAE.Animation entry in NewAnims)
        {
            AnimationList.Remove(entry);
        }

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Action: TAE.Animation delete
/// </summary>
public class TaeAnimDelete : EditorAction
{
    private TAE.Animation StoredAnim;
    private List<TAE.Animation> AnimationList;
    private int RemovalIndex;

    public TaeAnimDelete(TAE.Animation storedAnim, List<TAE.Animation> animList, int index)
    {
        StoredAnim = storedAnim;
        AnimationList = animList;
        RemovalIndex = index;
    }

    public override ActionEvent Execute()
    {
        AnimationList.RemoveAt(RemovalIndex);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        AnimationList.Insert(RemovalIndex, StoredAnim);

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Action: TAE.Animation delete (multiple)
/// </summary>
public class TaeAnimMultiDelete : EditorAction
{
    private List<TAE.Animation> StoredAnims;
    private List<TAE.Animation> AnimationList;
    private List<int> RemovedIndices;

    public TaeAnimMultiDelete(List<TAE.Animation> storedAnims, List<TAE.Animation> animList, List<int> indices)
    {
        StoredAnims = storedAnims;
        AnimationList = animList;
        RemovedIndices = indices;
    }

    public override ActionEvent Execute()
    {
        for (int i = RemovedIndices.Count - 1; i >= 0; i--)
        {
            int curIndex = RemovedIndices[i];
            AnimationList.RemoveAt(curIndex);
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < RemovedIndices.Count; i++)
        {
            TAE.Animation storedAnim = StoredAnims[i];
            int curIndex = RemovedIndices[i];

            AnimationList.Insert(curIndex, storedAnim);
        }

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Action: TAE.Event create
/// </summary>
public class TaeEventCreate : EditorAction
{
    private TAE.Event NewEvent;
    private List<TAE.Event> EventList;
    private int InsertionIndex;

    public TaeEventCreate(TAE.Event entryNewEvent, List<TAE.Event> eventList, int index)
    {
        InsertionIndex = index;
        NewEvent = entryNewEvent;
        EventList = eventList;
    }

    public override ActionEvent Execute()
    {
        EventList.Insert(InsertionIndex, NewEvent);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        EventList.RemoveAt(InsertionIndex);

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Action: TAE.Event duplicate
/// </summary>
public class TaeEventDuplicate : EditorAction
{
    private TAE.Event NewEvent;
    private List<TAE.Event> EventList;
    private int InsertionIndex;

    public TaeEventDuplicate(TAE.Event entryNewEvent, List<TAE.Event> eventList, int index)
    {
        InsertionIndex = index;
        NewEvent = entryNewEvent;
        EventList = eventList;
    }

    public override ActionEvent Execute()
    {
        EventList.Insert(InsertionIndex, NewEvent);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        EventList.RemoveAt(InsertionIndex);

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Action: TAE.Event duplicate (multiple)
/// </summary>
public class TaeEventMultiDuplicate : EditorAction
{
    private List<TAE.Event> NewEvents;
    private List<TAE.Event> EventList;
    private List<int> InsertionIndexes;

    public TaeEventMultiDuplicate(List<TAE.Event> newEvents, List<TAE.Event> eventList, List<int> indexList)
    {
        InsertionIndexes = indexList;
        NewEvents = newEvents;
        EventList = eventList;
    }

    public override ActionEvent Execute()
    {
        for(int i = 0; i < InsertionIndexes.Count; i++)
        {
            TAE.Event curNewEvent = NewEvents[i];
            int curIndex = InsertionIndexes[i];

            EventList.Insert(curIndex, curNewEvent);
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        foreach(TAE.Event entry in NewEvents)
        {
            EventList.Remove(entry);
        }

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Action: TAE.Event delete
/// </summary>
public class TaeEventDelete : EditorAction
{
    private TAE.Event StoredEvent;
    private List<TAE.Event> EventList;
    private int RemovalIndex;

    public TaeEventDelete(TAE.Event entryOldEvent, List<TAE.Event> eventList, int index)
    {
        RemovalIndex = index;
        StoredEvent = entryOldEvent;
        EventList = eventList;
    }

    public override ActionEvent Execute()
    {
        EventList.RemoveAt(RemovalIndex);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        EventList.Insert(RemovalIndex, StoredEvent);

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Action: TAE.Event delete (multiple)
/// </summary>
public class TaeEventMultiDelete : EditorAction
{
    private List<TAE.Event> StoredEvents;
    private List<TAE.Event> EventList;
    private List<int> RemovalIndices;
    private List<int> InsertIndices;

    public TaeEventMultiDelete(List<TAE.Event> storedEvents, List<TAE.Event> eventList, List<int> removalIndices)
    {
        RemovalIndices = removalIndices;
        StoredEvents = storedEvents;
        EventList = eventList;
    }

    public override ActionEvent Execute()
    {
        for (int i = RemovalIndices.Count - 1; i >= 0; i--)
        {
            int curIndex = RemovalIndices[i];
            EventList.RemoveAt(curIndex);
        }

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        for (int i = 0; i < RemovalIndices.Count; i++)
        {
            TAE.Event storedEvent = StoredEvents[i];
            int curIndex = RemovalIndices[i];

            EventList.Insert(curIndex, storedEvent);
        }

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Action: TAE duplicate
/// </summary>
public class TimeActDuplicate : EditorAction
{
    private InternalTimeActWrapper TargetTimeAct;
    private List<InternalTimeActWrapper> FileList;
    private int InsertionIndex;

    public TimeActDuplicate(InternalTimeActWrapper newInfo, List<InternalTimeActWrapper> fileList, int index)
    {
        InsertionIndex = index;
        FileList = fileList;
        TargetTimeAct = newInfo;
    }

    public override ActionEvent Execute()
    {
        FileList.Insert(InsertionIndex, TargetTimeAct);

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        FileList.RemoveAt(InsertionIndex);

        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Action: TAE delete
/// </summary>
public class TimeActDelete : EditorAction
{
    private InternalTimeActWrapper TargetTimeAct;

    public TimeActDelete(InternalTimeActWrapper info)
    {
        TargetTimeAct = info;
    }

    public override ActionEvent Execute()
    {
        TargetTimeAct.MarkForRemoval = true;

        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        TargetTimeAct.MarkForRemoval = false;

        return ActionEvent.NoEvent;
    }
}