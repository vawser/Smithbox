using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor;

public static class DataFinderUtil
{
    public static (bool, string) IsValueMatch(PARAMDEF.DefType type, Andre.Formats.Param.Cell field, string searchValue, bool isRangeSearch, string startValue, string endValue)
    {
        var success = false;
        var startSuccess = false;
        var endSuccess = false;

        switch (type)
        {
            case PARAMDEF.DefType.s8:
                if (isRangeSearch)
                {
                    sbyte sbyteStartVal;
                    sbyte sbyteEndVal;

                    startSuccess = sbyte.TryParse(startValue, out sbyteStartVal);
                    endSuccess = sbyte.TryParse(endValue, out sbyteEndVal);

                    if (startSuccess && endSuccess)
                    {
                        if (((sbyte)field.Value >= sbyteStartVal) &&
                            ((sbyte)field.Value <= sbyteEndVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                else
                {
                    sbyte sbyteVal;

                    success = sbyte.TryParse(searchValue, out sbyteVal);

                    if (success)
                    {
                        if (((sbyte)field.Value == sbyteVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                break;
            case PARAMDEF.DefType.u8:
                if (isRangeSearch)
                {
                    byte byteStartVal;
                    byte byteEndVal;

                    startSuccess = byte.TryParse(startValue, out byteStartVal);
                    endSuccess = byte.TryParse(endValue, out byteEndVal);

                    if (startSuccess && endSuccess)
                    {
                        if (((byte)field.Value >= byteStartVal) &&
                            ((byte)field.Value <= byteEndVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                else
                {
                    byte byteVal;

                    success = byte.TryParse(searchValue, out byteVal);

                    if (success)
                    {
                        if (((byte)field.Value == byteVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                break;
            case PARAMDEF.DefType.s16:
                if (isRangeSearch)
                {
                    short shortStartVal;
                    short shortEndVal;

                    startSuccess = short.TryParse(startValue, out shortStartVal);
                    endSuccess = short.TryParse(endValue, out shortEndVal);

                    if (startSuccess && endSuccess)
                    {
                        if (((short)field.Value >= shortStartVal) &&
                            ((short)field.Value <= shortEndVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                else
                {
                    short shortVal;

                    success = short.TryParse(searchValue, out shortVal);

                    if (success)
                    {
                        if (((short)field.Value == shortVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                break;
            case PARAMDEF.DefType.u16:
                if (isRangeSearch)
                {
                    ushort ushortStartVal;
                    ushort ushortEndVal;

                    startSuccess = ushort.TryParse(startValue, out ushortStartVal);
                    endSuccess = ushort.TryParse(endValue, out ushortEndVal);

                    if (startSuccess && endSuccess)
                    {
                        if (((ushort)field.Value >= ushortStartVal) &&
                            ((ushort)field.Value <= ushortEndVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                else
                {
                    ushort ushortVal;

                    success = ushort.TryParse(searchValue, out ushortVal);

                    if (success)
                    {
                        if (((ushort)field.Value == ushortVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                break;
            case PARAMDEF.DefType.s32:
                if (isRangeSearch)
                {
                    int intStartVal;
                    int intEndVal;

                    startSuccess = int.TryParse(startValue, out intStartVal);
                    endSuccess = int.TryParse(endValue, out intEndVal);

                    if (startSuccess && endSuccess)
                    {
                        if (((int)field.Value >= intStartVal) &&
                            ((int)field.Value <= intEndVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                else
                {
                    int intVal;

                    success = int.TryParse(searchValue, out intVal);

                    if (success)
                    {
                        if (((int)field.Value == intVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                break;
            case PARAMDEF.DefType.u32:
                if (isRangeSearch)
                {
                    uint uintStartVal;
                    uint uintEndVal;

                    startSuccess = uint.TryParse(startValue, out uintStartVal);
                    endSuccess = uint.TryParse(endValue, out uintEndVal);

                    if (startSuccess && endSuccess)
                    {
                        if (((uint)field.Value >= uintStartVal) &&
                            ((uint)field.Value <= uintEndVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                else
                {
                    uint uintVal;

                    success = uint.TryParse(searchValue, out uintVal);

                    if (success)
                    {
                        if (((uint)field.Value == uintVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                break;
            case PARAMDEF.DefType.f32:
                if (isRangeSearch)
                {
                    double floatStartVal;
                    double floatEndVal;

                    startSuccess = double.TryParse(startValue, out floatStartVal);
                    endSuccess = double.TryParse(endValue, out floatEndVal);

                    if (startSuccess && endSuccess)
                    {
                        if (((double)field.Value >= floatStartVal) &&
                            ((double)field.Value <= floatEndVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                else
                {
                    float floatVal;

                    success = float.TryParse(searchValue, out floatVal);

                    if (success)
                    {
                        if (((float)field.Value == floatVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                break;
            case PARAMDEF.DefType.b32:
                if (isRangeSearch)
                {
                    bool boolStartVal;
                    bool boolEndVal;

                    startSuccess = bool.TryParse(startValue, out boolStartVal);
                    endSuccess = bool.TryParse(endValue, out boolEndVal);

                    if (startSuccess && endSuccess)
                    {
                        if (((bool)field.Value == boolStartVal) ||
                            ((bool)field.Value == boolEndVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                else
                {
                    bool boolVal;

                    success = bool.TryParse(searchValue, out boolVal);

                    if (success)
                    {
                        if (((bool)field.Value == boolVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                break;
            case PARAMDEF.DefType.fixstr:
            case PARAMDEF.DefType.fixstrW:
                if (isRangeSearch)
                {
                    if (((string)field.Value == startValue) ||
                    ((string)field.Value == endValue))
                    {
                        return (true, $"{field.Value}");
                    }
                }
                else
                {
                    if ((string)field.Value == searchValue)
                    {
                        return (true, $"{field.Value}");
                    }
                }
                break;
            default: break;
        }

        return (false, "");
    }
}
