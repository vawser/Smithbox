using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

public static class MassEditPropertyHelper
{
    private static readonly Random _random = new Random();

    /// <summary>
    /// Performs the mathematical condition check
    /// </summary>
    public static bool PerformNumericComparison(Entity ent, string comparator, string targetVal, object propValue, Type valueType)
    {
        // LONG
        if (valueType == typeof(long))
        {
            var tPropValue = (long)propValue;
            var tTargetValue = (long)propValue;

            var res = long.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // UINT
        if (valueType == typeof(uint))
        {
            var tPropValue = (uint)propValue;
            var tTargetValue = (uint)propValue;

            var res = uint.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // INT
        if (valueType == typeof(int))
        {
            var tPropValue = (int)propValue;
            var tTargetValue = (int)propValue;

            var res = int.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // USHORT
        if (valueType == typeof(ushort))
        {
            var tPropValue = (ushort)propValue;
            var tTargetValue = (ushort)propValue;

            var res = ushort.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // SHORT
        if (valueType == typeof(short))
        {
            var tPropValue = (short)propValue;
            var tTargetValue = (short)propValue;

            var res = short.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // SBYTE
        if (valueType == typeof(sbyte))
        {
            var tPropValue = (sbyte)propValue;
            var tTargetValue = (sbyte)propValue;

            var res = sbyte.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // BYTE
        if (valueType == typeof(byte))
        {
            var tPropValue = (byte)propValue;
            var tTargetValue = (byte)propValue;

            var res = byte.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // FLOAT
        if (valueType == typeof(float))
        {
            var tPropValue = (float)propValue;
            var tTargetValue = (float)propValue;

            var res = float.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // VECTOR 3
        if (valueType == typeof(Vector3))
        {
            var tPropValue = (Vector3)propValue;
            var tTargetValue = (Vector3)propValue;

            var parts = targetVal.Split(",");
            if (parts.Length >= 2)
            {
                var x = parts[0];
                var y = parts[1];
                var z = parts[2];

                float tX = 0.0f;
                float tY = 0.0f;
                float tZ = 0.0f;

                var resX = float.TryParse(x, out tX);
                var resY = float.TryParse(y, out tY);
                var resZ = float.TryParse(z, out tZ);

                if (resX && resY && resZ)
                {
                    if (comparator == "<")
                    {
                        if (tPropValue.X < tX && tPropValue.Y < tY && tPropValue.Z < tZ)
                            return true;
                    }
                    if (comparator == ">")
                    {
                        if (tPropValue.X > tX && tPropValue.Y > tY && tPropValue.Z > tZ)
                            return true;
                    }
                }
            }
        }


        return false;
    }


    /// <summary>
    /// Handles the property value operation edits
    /// TODO: adjust how this is done so we don't need to duplicate the operation logic so much
    /// </summary>
    public static ViewportAction PropertyValueOperation(MapEditorScreen editor, MapContainer map, MsbEntity curEnt, string cmd,
        bool enableRandomSpread, float minRandom, float maxRandom)
    {
        var input = cmd.Replace("prop:", "");

        var segments = input.Split(" ");
        if (segments.Length >= 3)
        {
            var prop = segments[0];
            var compare = segments[1].Trim().ToLower();
            var newValue = segments[2].Trim().ToLower();

            var index = -1;

            if (prop.Contains("[") && prop.Contains("]"))
            {
                var match = new Regex(@"\[(.*?)\]").Match(prop);

                if (match.Success)
                {
                    var val = match.Value.Replace("[", "").Replace("]", "");

                    int.TryParse(val, out index);
                    prop = prop.Replace($"{match.Value}", "");
                }
            }

            Type targetObj = curEnt.WrappedObject.GetType();

            PropertyInfo targetProp = null;
            object targetProp_Value = null;

            // Get the actual property from within the array
            if (index != -1)
            {
                targetProp = targetObj.GetProperty(prop);

                if (targetProp != null)
                {
                    object collection = targetProp.GetValue(curEnt.WrappedObject);

                    if (collection is Array arr && index >= 0 && index < arr.Length)
                    {
                        targetProp_Value = arr.GetValue(index);

                        if (targetProp_Value == null)
                            return null;

                        var valueType = targetProp_Value.GetType();

                        // If numeric operation is not supported, force set operation
                        if (!MassEditUtils.IsNumericType(valueType))
                        {
                            compare = "=";
                        }

                        // LONG
                        if (valueType == typeof(long))
                        {
                            long tNewValue = 0;
                            long tExistingValue = (long)targetProp_Value;

                            var res = long.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;
                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // UINT
                        if (valueType == typeof(uint))
                        {
                            uint tNewValue = 0;
                            uint tExistingValue = (uint)targetProp_Value;

                            var res = uint.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;

                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // INT
                        if (valueType == typeof(int))
                        {
                            int tNewValue = 0;
                            int tExistingValue = (int)targetProp_Value;

                            var res = int.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;

                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // USHORT
                        if (valueType == typeof(ushort))
                        {
                            ushort tNewValue = 0;
                            ushort tExistingValue = (ushort)targetProp_Value;

                            var res = ushort.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;

                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // SHORT
                        if (valueType == typeof(short))
                        {
                            short tNewValue = 0;
                            short tExistingValue = (short)targetProp_Value;

                            var res = short.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;

                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // SBYTE
                        if (valueType == typeof(sbyte))
                        {
                            sbyte tNewValue = 0;
                            sbyte tExistingValue = (sbyte)targetProp_Value;

                            var res = sbyte.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;

                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // BYTE
                        if (valueType == typeof(byte))
                        {
                            byte tNewValue = 0;
                            byte tExistingValue = (byte)targetProp_Value;

                            var res = byte.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;

                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // FLOAT
                        if (valueType == typeof(float))
                        {
                            float tNewValue = 0;
                            float tExistingValue = (float)targetProp_Value;

                            var res = float.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;

                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // VECTOR3
                        if (valueType == typeof(Vector3))
                        {
                            Vector3 tNewValue = new Vector3();
                            Vector3 tExistingValue = (Vector3)targetProp_Value;

                            var res = VectorExtensions.TryParseVector3(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;

                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // STRING
                        if (valueType == typeof(string))
                        {
                            string result = newValue;

                            return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                        }
                    }
                }
            }
            else
            {
                targetProp = curEnt.GetProperty(prop);
                targetProp_Value = curEnt.GetPropertyValue(prop);

                if (targetProp_Value == null)
                    return null;

                var valueType = targetProp_Value.GetType();

                // If numeric operation is not supported, force set operation
                if (!MassEditUtils.IsNumericType(valueType))
                {
                    compare = "=";
                }

                // LONG
                if (valueType == typeof(long))
                {
                    long tNewValue = 0;
                    long tExistingValue = (long)targetProp_Value;

                    var res = long.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if(enableRandomSpread)
                        {
                            result = (long)GetRandomValue(result, minRandom, maxRandom, valueType);
                        }

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // UINT
                if (valueType == typeof(uint))
                {
                    uint tNewValue = 0;
                    uint tExistingValue = (uint)targetProp_Value;

                    var res = uint.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if (enableRandomSpread)
                        {
                            result = (uint)GetRandomValue(result, minRandom, maxRandom, valueType);
                        }

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // INT
                if (valueType == typeof(int))
                {
                    int tNewValue = 0;
                    int tExistingValue = (int)targetProp_Value;

                    var res = int.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if (enableRandomSpread)
                        {
                            result = (int)GetRandomValue(result, minRandom, maxRandom, valueType);
                        }

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // USHORT
                if (valueType == typeof(ushort))
                {
                    ushort tNewValue = 0;
                    ushort tExistingValue = (ushort)targetProp_Value;

                    var res = ushort.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if (enableRandomSpread)
                        {
                            result = (ushort)GetRandomValue(result, minRandom, maxRandom, valueType);
                        }

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // SHORT
                if (valueType == typeof(short))
                {
                    short tNewValue = 0;
                    short tExistingValue = (short)targetProp_Value;

                    var res = short.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if (enableRandomSpread)
                        {
                            result = (short)GetRandomValue(result, minRandom, maxRandom, valueType);
                        }

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // SBYTE
                if (valueType == typeof(sbyte))
                {
                    sbyte tNewValue = 0;
                    sbyte tExistingValue = (sbyte)targetProp_Value;

                    var res = sbyte.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if (enableRandomSpread)
                        {
                            result = (sbyte)GetRandomValue(result, minRandom, maxRandom, valueType);
                        }

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // BYTE
                if (valueType == typeof(byte))
                {
                    byte tNewValue = 0;
                    byte tExistingValue = (byte)targetProp_Value;

                    var res = byte.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if (enableRandomSpread)
                        {
                            result = (byte)GetRandomValue(result, minRandom, maxRandom, valueType);
                        }

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // FLOAT
                if (valueType == typeof(float))
                {
                    float tNewValue = 0;
                    float tExistingValue = (float)targetProp_Value;

                    var res = float.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if (enableRandomSpread)
                        {
                            result = (float)GetRandomValue(result, minRandom, maxRandom, valueType);
                        }

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // VECTOR3
                if (valueType == typeof(Vector3))
                {
                    Vector3 tNewValue = new Vector3();
                    Vector3 tExistingValue = (Vector3)targetProp_Value;

                    var res = VectorExtensions.TryParseVector3(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if (enableRandomSpread)
                        {
                            result = (Vector3)GetRandomValue(result, minRandom, maxRandom, valueType);
                        }

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new MapObjectPropertyChangeAction(editor, curEnt, targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // STRING
                if (valueType == typeof(string))
                {
                    string result = newValue;

                    return new MapObjectPropertyChangeAction(editor, curEnt, targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                }
            }
        }

        return null;
    }
    public static object GetRandomValue(object value, float valueMin, float valueMax, Type valueType)
    {
        if (value == null)
            throw new ArgumentNullException(nameof(value));

        if (valueType == null)
            throw new ArgumentNullException(nameof(valueType));

        // Vector3 support
        if (valueType == typeof(Vector3))
        {
            Vector3 mid = (Vector3)value;

            return new Vector3(
                RandomRange(mid.X + valueMin, mid.X + valueMax),
                RandomRange(mid.Y + valueMin, mid.Y + valueMax),
                RandomRange(mid.Z + valueMin, mid.Z + valueMax)
            );
        }

        // Scalar types
        float midpoint = Convert.ToSingle(value);
        float min = midpoint + valueMin;
        float max = midpoint + valueMax;

        if (min > max)
            (min, max) = (max, min);

        float randomFloat = RandomRange(min, max);

        if (valueType == typeof(float))
            return randomFloat;

        if (valueType == typeof(double))
            return (double)randomFloat;

        if (valueType == typeof(int))
            return (int)Math.Round(randomFloat);

        if (valueType == typeof(uint))
            return (uint)Math.Max(0, Math.Round(randomFloat));

        if (valueType == typeof(short))
            return (short)Math.Round(randomFloat);

        if (valueType == typeof(ushort))
            return (ushort)Math.Round(randomFloat);

        if (valueType == typeof(byte))
            return (byte)Math.Clamp(Math.Round(randomFloat), byte.MinValue, byte.MaxValue);

        if (valueType == typeof(sbyte))
            return (sbyte)Math.Clamp(Math.Round(randomFloat), sbyte.MinValue, sbyte.MaxValue);

        throw new NotSupportedException($"Unsupported value type: {valueType}");
    }

    private static float RandomRange(float min, float max)
    {
        if (min > max)
            (min, max) = (max, min);

        return (float)(_random.NextDouble() * (max - min) + min);
    }
}
