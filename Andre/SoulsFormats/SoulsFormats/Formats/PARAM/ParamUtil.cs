using System;
using System.Collections.Generic;
using DefType = SoulsFormats.PARAMDEF.DefType;
using EditFlags = SoulsFormats.PARAMDEF.EditFlags;

namespace SoulsFormats
{
    public static class ParamUtil
    {
        public static string GetDefaultFormat(DefType type)
        {
            switch (type)
            {
                case DefType.s8: return "%d";
                case DefType.u8: return "%d";
                case DefType.s16: return "%d";
                case DefType.u16: return "%d";
                case DefType.s32: return "%d";
                case DefType.u32: return "%d";
                case DefType.b32: return "%d";
                case DefType.f32: return "%f";
                case DefType.angle32: return "%f";
                case DefType.f64: return "%f";
                case DefType.dummy8: return "";
                case DefType.fixstr: return "%d";
                case DefType.fixstrW: return "%d";

                default:
                    throw new NotImplementedException($"No default format specified for {nameof(DefType)}.{type}");
            }
        }

        private static readonly Dictionary<DefType, float> fixedDefaults = new Dictionary<DefType, float>
        {
            [DefType.s8] = 0,
            [DefType.u8] = 0,
            [DefType.s16] = 0,
            [DefType.u16] = 0,
            [DefType.s32] = 0,
            [DefType.u32] = 0,
            [DefType.b32] = 0,
            [DefType.f32] = 0,
            [DefType.angle32] = 0,
            [DefType.f64] = 0,
            [DefType.dummy8] = 0,
            [DefType.fixstr] = 0,
            [DefType.fixstrW] = 0,
        };

        private static readonly Dictionary<DefType, object> variableDefaults = new Dictionary<DefType, object>
        {
            [DefType.s8] = 0,
            [DefType.u8] = 0,
            [DefType.s16] = 0,
            [DefType.u16] = 0,
            [DefType.s32] = 0,
            [DefType.u32] = 0,
            [DefType.b32] = 0,
            [DefType.f32] = 0f,
            [DefType.angle32] = 0f,
            [DefType.f64] = 0d,
            [DefType.dummy8] = null,
            [DefType.fixstr] = null,
            [DefType.fixstrW] = null,
        };

        public static object GetDefaultDefault(PARAMDEF def, DefType type)
        {
            if (def?.VariableEditorValueTypes ?? false)
                return variableDefaults[type];
            else
                return fixedDefaults[type];
        }

        private static readonly Dictionary<DefType, float> fixedMinimums = new Dictionary<DefType, float>
        {
            [DefType.s8] = sbyte.MinValue,
            [DefType.u8] = byte.MinValue,
            [DefType.s16] = short.MinValue,
            [DefType.u16] = ushort.MinValue,
            [DefType.s32] = -2147483520, // Smallest representable float greater than int.MinValue
            [DefType.u32] = uint.MinValue,
            [DefType.b32] = 0,
            [DefType.f32] = float.MinValue,
            [DefType.angle32] = float.MinValue,
            [DefType.f64] = float.MinValue,
            [DefType.dummy8] = 0,
            [DefType.fixstr] = -1,
            [DefType.fixstrW] = -1,
        };

        private static readonly Dictionary<DefType, object> variableMinimums = new Dictionary<DefType, object>
        {
            [DefType.s8] = (int)sbyte.MinValue,
            [DefType.u8] = (int)byte.MinValue,
            [DefType.s16] = (int)short.MinValue,
            [DefType.u16] = (int)ushort.MinValue,
            [DefType.s32] = int.MinValue,
            [DefType.u32] = (int)uint.MinValue,
            [DefType.b32] = 0,
            [DefType.f32] = float.MinValue,
            [DefType.angle32] = float.MinValue,
            [DefType.f64] = double.MinValue,
            [DefType.dummy8] = null,
            [DefType.fixstr] = null,
            [DefType.fixstrW] = null,
        };

        public static object GetDefaultMinimum(PARAMDEF def, DefType type)
        {
            if (def?.VariableEditorValueTypes ?? false)
                return variableMinimums[type];
            else
                return fixedMinimums[type];
        }

        private static readonly Dictionary<DefType, float> fixedMaximums = new Dictionary<DefType, float>
        {
            [DefType.s8] = sbyte.MaxValue,
            [DefType.u8] = byte.MaxValue,
            [DefType.s16] = short.MaxValue,
            [DefType.u16] = ushort.MaxValue,
            [DefType.s32] = 2147483520, // Largest representable float less than int.MaxValue
            [DefType.u32] = 4294967040, // Largest representable float less than uint.MaxValue
            [DefType.b32] = 1,
            [DefType.f32] = float.MaxValue,
            [DefType.angle32] = float.MaxValue,
            [DefType.f64] = float.MaxValue,
            [DefType.dummy8] = 0,
            [DefType.fixstr] = 1000000000,
            [DefType.fixstrW] = 1000000000,
        };

        private static readonly Dictionary<DefType, object> variableMaximums = new Dictionary<DefType, object>
        {
            [DefType.s8] = (int)sbyte.MaxValue,
            [DefType.u8] = (int)byte.MaxValue,
            [DefType.s16] = (int)short.MaxValue,
            [DefType.u16] = (int)ushort.MaxValue,
            [DefType.s32] = int.MaxValue,
            [DefType.u32] = int.MaxValue, // Yes, u32 uses signed int too (usually)
            [DefType.b32] = 1,
            [DefType.f32] = float.MaxValue,
            [DefType.angle32] = float.MaxValue,
            [DefType.f64] = double.MaxValue,
            [DefType.dummy8] = null,
            [DefType.fixstr] = null,
            [DefType.fixstrW] = null,
        };

        public static object GetDefaultMaximum(PARAMDEF def, DefType type)
        {
            if (def?.VariableEditorValueTypes ?? false)
                return variableMaximums[type];
            else
                return fixedMaximums[type];
        }

        private static readonly Dictionary<DefType, float> fixedIncrements = new Dictionary<DefType, float>
        {
            [DefType.s8] = 1,
            [DefType.u8] = 1,
            [DefType.s16] = 1,
            [DefType.u16] = 1,
            [DefType.s32] = 1,
            [DefType.u32] = 1,
            [DefType.b32] = 1,
            [DefType.f32] = 0.01f,
            [DefType.angle32] = 0.01f,
            [DefType.f64] = 0.01f,
            [DefType.dummy8] = 0,
            [DefType.fixstr] = 1,
            [DefType.fixstrW] = 1,
        };

        private static readonly Dictionary<DefType, object> variableIncrements = new Dictionary<DefType, object>
        {
            [DefType.s8] = 1,
            [DefType.u8] = 1,
            [DefType.s16] = 1,
            [DefType.u16] = 1,
            [DefType.s32] = 1,
            [DefType.u32] = 1,
            [DefType.b32] = 1,
            [DefType.f32] = 0.01f,
            [DefType.angle32] = 0.01f,
            [DefType.f64] = 0.01d,
            [DefType.dummy8] = null,
            [DefType.fixstr] = null,
            [DefType.fixstrW] = null,
        };

        public static object GetDefaultIncrement(PARAMDEF def, DefType type)
        {
            if (def?.VariableEditorValueTypes ?? false)
                return variableIncrements[type];
            else
                return fixedIncrements[type];
        }

        public static EditFlags GetDefaultEditFlags(DefType type)
        {
            switch (type)
            {
                case DefType.s8: return EditFlags.Wrap;
                case DefType.u8: return EditFlags.Wrap;
                case DefType.s16: return EditFlags.Wrap;
                case DefType.u16: return EditFlags.Wrap;
                case DefType.s32: return EditFlags.Wrap;
                case DefType.u32: return EditFlags.Wrap;
                case DefType.b32: return EditFlags.Wrap;
                case DefType.f32: return EditFlags.Wrap;
                case DefType.angle32: return EditFlags.Wrap;
                case DefType.f64: return EditFlags.Wrap;
                case DefType.dummy8: return EditFlags.None;
                case DefType.fixstr: return EditFlags.Wrap;
                case DefType.fixstrW: return EditFlags.Wrap;

                default:
                    throw new NotImplementedException($"No default edit flags specified for {nameof(DefType)}.{type}");
            }
        }

        public static bool IsArrayType(DefType type)
        {
            switch (type)
            {
                case DefType.dummy8:
                case DefType.fixstr:
                case DefType.fixstrW:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsBitType(DefType type)
        {
            switch (type)
            {
                case DefType.s8:
                case DefType.u8:
                case DefType.s16:
                case DefType.u16:
                case DefType.s32:
                case DefType.u32:
                case DefType.dummy8:
                    return true;

                default:
                    return false;
            }
        }

        public static bool IsSignedBitType(DefType type)
        {
            switch (type)
            {
                case DefType.s8:
                case DefType.s16:
                case DefType.s32:
                    return true;
                default:
                    return false;
            }
        }

        public static int GetValueSize(DefType type)
        {
            switch (type)
            {
                case DefType.s8: return 1;
                case DefType.u8: return 1;
                case DefType.s16: return 2;
                case DefType.u16: return 2;
                case DefType.s32: return 4;
                case DefType.u32: return 4;
                case DefType.b32: return 4;
                case DefType.f32: return 4;
                case DefType.angle32: return 4;
                case DefType.f64: return 8;
                case DefType.dummy8: return 1;
                case DefType.fixstr: return 1;
                case DefType.fixstrW: return 2;

                default:
                    throw new NotImplementedException($"No value size specified for {nameof(DefType)}.{type}");
            }
        }

        public static object ConvertDefaultValue(PARAMDEF.Field field)
        {
            switch (field.DisplayType)
            {
                case DefType.s8: return Convert.ToSByte(field.Default);
                case DefType.u8: return Convert.ToByte(field.Default);
                case DefType.s16: return Convert.ToInt16(field.Default);
                case DefType.u16: return Convert.ToUInt16(field.Default);
                case DefType.s32: return Convert.ToInt32(field.Default);
                case DefType.u32: return Convert.ToUInt32(field.Default);
                case DefType.b32: return Convert.ToInt32(field.Default);
                case DefType.f32: return Convert.ToSingle(field.Default);
                case DefType.angle32: return Convert.ToSingle(field.Default);
                case DefType.f64: return Convert.ToDouble(field.Default);
                case DefType.fixstr: return "";
                case DefType.fixstrW: return "";
                case DefType.dummy8:
                    if (field.BitSize == -1)
                        return new byte[field.ArrayLength];
                    else
                        return (byte)0;

                default:
                    throw new NotImplementedException($"Default not implemented for type {field.DisplayType}");
            }
        }

        public static int GetBitLimit(DefType type)
        {
            switch (type)
            {
                case DefType.s8: return 8;
                case DefType.u8: return 8;
                case DefType.s16: return 16;
                case DefType.u16: return 16;
                case DefType.s32: return 32;
                case DefType.u32: return 32;
                case DefType.dummy8: return 8;
                default:
                    throw new InvalidOperationException($"Type {type} cannot be a bitfield.");
            }
        }
    }
}
