using StudioCore.Editor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor.Actions;

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

        if (propertyType == typeof(string))
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