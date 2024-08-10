using DotNext.Collections.Generic;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.GraphicsEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.TimeActEditor;

public class EventPropertyChange : EditorAction
{
    private Dictionary<string, object> Parameters;
    private string ParamName;
    private object OldValue;
    private object NewValue;

    public EventPropertyChange(Dictionary<string, object> parameters, string paramName, object oldValue, object newValue, Type propertyType)
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

public class TimeActStartTimePropertyChange : EditorAction
{
    private TAE.Event Event;
    private object OldValue;
    private object NewValue;

    public TimeActStartTimePropertyChange(TAE.Event entry, object oldValue, object newValue)
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

public class TimeActEndTimePropertyChange : EditorAction
{
    private TAE.Event Event;
    private object OldValue;
    private object NewValue;

    public TimeActEndTimePropertyChange(TAE.Event entry, object oldValue, object newValue)
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

public class TimeActEndAnimHeaderPropertyChange : EditorAction
{
    private TAE.Animation Animation;
    private object OldValue;
    private object NewValue;
    private TemporaryAnimHeader OldTempHeader;

    public TimeActEndAnimHeaderPropertyChange(TAE.Animation entry, object oldValue, object newValue, TemporaryAnimHeader tempHeader)
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

public class TimeActEndAnimIDPropertyChange : EditorAction
{
    private TAE.Animation Animation;
    private object OldValue;
    private object NewValue;

    public TimeActEndAnimIDPropertyChange(TAE.Animation entry, object oldValue, object newValue)
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

public class TimeActEndAnimNamePropertyChange : EditorAction
{
    private TAE.Animation Animation;
    private object OldValue;
    private object NewValue;

    public TimeActEndAnimNamePropertyChange(TAE.Animation entry, object oldValue, object newValue)
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
