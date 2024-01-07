using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace SoulsFormats
{
    public partial class PARAMDEF
    {
        /// <summary>
        /// Supported primitive field types.
        /// </summary>
        public enum DefType
        {
            /// <summary>
            /// Signed 1-byte integer.
            /// </summary>
            s8,

            /// <summary>
            /// Unsigned 1-byte integer.
            /// </summary>
            u8,

            /// <summary>
            /// Signed 2-byte integer.
            /// </summary>
            s16,

            /// <summary>
            /// Unsigned 2-byte integer.
            /// </summary>
            u16,

            /// <summary>
            /// Signed 4-byte integer.
            /// </summary>
            s32,

            /// <summary>
            /// Unsigned 4-byte integer.
            /// </summary>
            u32,

            /// <summary>
            /// 4-byte integer representing a boolean.
            /// </summary>
            b32,

            /// <summary>
            /// Single-precision floating point value.
            /// </summary>
            f32,

            /// <summary>
            /// Single-precision floating point value representing an angle.
            /// </summary>
            angle32,

            /// <summary>
            /// Double-precision floating point value.
            /// </summary>
            f64,

            /// <summary>
            /// Byte or array of bytes used for padding or placeholding.
            /// </summary>
            dummy8,

            /// <summary>
            /// Fixed-width Shift-JIS string.
            /// </summary>
            fixstr,

            /// <summary>
            /// Fixed-width UTF-16 string.
            /// </summary>
            fixstrW,
        }

        /// <summary>
        /// Flags that control editor behavior for a field.
        /// </summary>
        [Flags]
        public enum EditFlags
        {
            /// <summary>
            /// Value is editable and does not wrap.
            /// </summary>
            None = 0,

            /// <summary>
            /// Value wraps around when scrolled past the minimum or maximum.
            /// </summary>
            Wrap = 1,

            /// <summary>
            /// Value may not be edited.
            /// </summary>
            Lock = 4,
        }

        /// <summary>
        /// Information about a field present in each row in a param.
        /// </summary>
        public class Field
        {
            /// <summary>
            /// Owning PARAMDEF
            /// </summary>
            public PARAMDEF Parent { get; }

            /// <summary>
            /// Name to display in the editor.
            /// </summary>
            public string DisplayName { get; set; }

            /// <summary>
            /// Type of value to display in the editor.
            /// </summary>
            public DefType DisplayType { get; set; }

            /// <summary>
            /// Printf-style format string to apply to the value in the editor.
            /// </summary>
            public string DisplayFormat { get; set; }

            /// <summary>
            /// Default value for new rows.
            /// </summary>
            public object Default { get; set; }

            /// <summary>
            /// Minimum valid value.
            /// </summary>
            public object Minimum { get; set; }

            /// <summary>
            /// Maximum valid value.
            /// </summary>
            public object Maximum { get; set; }

            /// <summary>
            /// Amount of increase or decrease per step when scrolling in the editor.
            /// </summary>
            public object Increment { get; set; }

            /// <summary>
            /// Flags determining behavior of the field in the editor.
            /// </summary>
            public EditFlags EditFlags { get; set; }

            /// <summary>
            /// Number of elements for array types; only supported for dummy8, fixstr, and fixstrW.
            /// </summary>
            public int ArrayLength { get; set; }

            /// <summary>
            /// Optional description of the field; may be null.
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            /// Type of the value in the engine; may be an enum type.
            /// </summary>
            public string InternalType { get; set; }

            /// <summary>
            /// Name of the value in the engine; not present before version 102.
            /// </summary>
            public string InternalName { get; set; }

            /// <summary>
            /// Number of bits used by a bitfield; only supported for unsigned types, -1 when not used.
            /// </summary>
            public int BitSize { get; set; }

            /// <summary>
            /// Fields are ordered by this value in the editor; not present before version 104.
            /// </summary>
            public int SortID { get; set; }

            /// <summary>
            /// Unknown; appears to be an identifier. May be null, only supported in versions >= 200, only present in version 202 so far.
            /// </summary>
            public string UnkB8 { get; set; }

            /// <summary>
            /// Unknown; appears to be a param type. May be null, only supported in versions >= 200, only present in version 202 so far.
            /// </summary>
            public string UnkC0 { get; set; }

            /// <summary>
            /// Unknown; appears to be a display string. May be null, only supported in versions >= 200, only present in version 202 so far.
            /// </summary>
            public string UnkC8 { get; set; }

            private static readonly Regex arrayLengthRx = new Regex(@"^\s*(?<name>.+?)\s*\[\s*(?<length>\d+)\s*\]\s*$");
            private static readonly Regex bitSizeRx = new Regex(@"^\s*(?<name>.+?)\s*\:\s*(?<size>\d+)\s*$");

            /// <summary>
            /// The first regulation version this field was introduced in, or 0 if the field has always existed. This
            /// is not part of the binary paramdef format and only exists in XML paramdefs.
            /// </summary>
            public ulong FirstRegulationVersion { get; set; } = 0;

            /// <summary>
            /// The first regulation version that this field is removed from/no longer exists in, or 0 if the field is
            /// never removed. This is not part of the binary paramdef format and only exists in XML paramdefs.
            /// </summary>
            public ulong RemovedRegulationVersion { get; set; } = 0;
            
            /// <summary>
            /// Creates a Field with placeholder values.
            /// </summary>
            public Field() : this(null, DefType.f32, "placeholder") { }

            /// <summary>
            /// Creates a Field with the given type, name, and appropriate default values.
            /// </summary>
            public Field(PARAMDEF def, DefType displayType, string internalName)
            {
                Parent = def;
                DisplayName = internalName;
                DisplayType = displayType;
                DisplayFormat = ParamUtil.GetDefaultFormat(DisplayType);
                Default = ParamUtil.GetDefaultDefault(def, DisplayType);
                Minimum = ParamUtil.GetDefaultMinimum(def, DisplayType);
                Maximum = ParamUtil.GetDefaultMaximum(def, DisplayType);
                Increment = ParamUtil.GetDefaultIncrement(def, DisplayType);
                EditFlags = ParamUtil.GetDefaultEditFlags(DisplayType);
                ArrayLength = 1;
                InternalType = DisplayType.ToString();
                InternalName = internalName;
                BitSize = -1;
            }

            internal Field(BinaryReaderEx br, PARAMDEF def)
            {
                Parent = def;
                if (def.FormatVersion >= 202 || def.FormatVersion >= 106 && def.FormatVersion < 200)
                    DisplayName = br.GetUTF16(br.ReadVarint());
                else if (def.Unicode)
                    DisplayName = br.ReadFixStrW(0x40);
                else
                    DisplayName = br.ReadFixStr(0x40);

                DisplayType = (DefType)Enum.Parse(typeof(DefType), br.ReadFixStr(8));
                DisplayFormat = br.ReadFixStr(8);

                if (def.FormatVersion >= 203)
                {
                    br.AssertPattern(0x10, 0x00);
                }
                else
                {
                    Default = br.ReadSingle();
                    Minimum = br.ReadSingle();
                    Maximum = br.ReadSingle();
                    Increment = br.ReadSingle();
                }

                EditFlags = (EditFlags)br.ReadInt32();

                int byteCount = br.ReadInt32();
                if (!ParamUtil.IsArrayType(DisplayType) && byteCount != ParamUtil.GetValueSize(DisplayType)
                    || ParamUtil.IsArrayType(DisplayType) && byteCount % ParamUtil.GetValueSize(DisplayType) != 0)
                    throw new InvalidDataException($"Unexpected byte count {byteCount} for type {DisplayType}.");
                ArrayLength = byteCount / ParamUtil.GetValueSize(DisplayType);

                long descriptionOffset = br.ReadVarint();
                if (descriptionOffset != 0)
                {
                    if (def.Unicode)
                        Description = br.GetUTF16(descriptionOffset);
                    else
                        Description = br.GetShiftJIS(descriptionOffset);
                }

                if (def.FormatVersion >= 202 || def.FormatVersion >= 106 && def.FormatVersion < 200)
                    InternalType = br.GetASCII(br.ReadVarint()).Trim();
                else
                    InternalType = br.ReadFixStr(0x20).Trim();

                BitSize = -1;
                if (def.FormatVersion >= 102)
                {
                    if (def.FormatVersion >= 202 || def.FormatVersion >= 106 && def.FormatVersion < 200)
                        InternalName = br.GetASCII(br.ReadVarint()).Trim();
                    else
                        InternalName = br.ReadFixStr(0x20).Trim();

                    Match match = bitSizeRx.Match(InternalName);
                    if (match.Success)
                    {
                        InternalName = match.Groups["name"].Value;
                        BitSize = int.Parse(match.Groups["size"].Value);
                    }

                    if (ParamUtil.IsArrayType(DisplayType))
                    {
                        match = arrayLengthRx.Match(InternalName);
                        int length = match.Success ? int.Parse(match.Groups["length"].Value) : 1;
                        if (length != ArrayLength)
                            throw new InvalidDataException($"Mismatched array length in {InternalName} with byte count {byteCount}.");
                        if (match.Success)
                            InternalName = match.Groups["name"].Value;
                    }
                }

                if (def.FormatVersion >= 104)
                    SortID = br.ReadInt32();

                if (def.FormatVersion >= 200)
                {
                    br.AssertInt32(0);
                    long unkB8Offset = br.ReadInt64();
                    long unkC0Offset = br.ReadInt64();
                    long unkC8Offset = br.ReadInt64();

                    if (unkB8Offset != 0)
                        UnkB8 = br.GetASCII(unkB8Offset);
                    if (unkC0Offset != 0)
                        UnkC0 = br.GetASCII(unkC0Offset);
                    if (unkC8Offset != 0)
                        UnkC8 = br.GetUTF16(unkC8Offset);
                }
                else if (def.FormatVersion >= 106)
                {
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                    br.AssertInt32(0);
                }

                if (def.FormatVersion >= 203)
                {
                    object readVariableValue()
                    {
                        object value;
                        switch (DisplayType)
                        {
                            case DefType.s8:
                            case DefType.u8:
                            case DefType.s16:
                            case DefType.u16:
                            case DefType.s32:
                            case DefType.u32:
                            case DefType.b32:
                                value = br.ReadInt32();
                                br.AssertInt32(0);
                                break;

                            case DefType.f32:
                            case DefType.angle32:
                                value = br.ReadSingle();
                                br.AssertInt32(0);
                                break;

                            case DefType.f64:
                                value = br.ReadDouble();
                                break;

                            // Given that there are 8 bytes available, these could possibly be offsets
                            case DefType.dummy8:
                            case DefType.fixstr:
                            case DefType.fixstrW:
                                value = null;
                                br.AssertInt64(0);
                                break;

                            default:
                                throw new NotImplementedException($"Missing variable read for type: {DisplayType}");
                        }
                        return value;
                    }

                    Default = readVariableValue();
                    Minimum = readVariableValue();
                    Maximum = readVariableValue();
                    Increment = readVariableValue();
                }
            }

            internal void Write(BinaryWriterEx bw, PARAMDEF def, int index)
            {
                if (def.FormatVersion >= 202 || def.FormatVersion >= 106 && def.FormatVersion < 200)
                    bw.ReserveVarint($"DisplayNameOffset{index}");
                else if (def.Unicode)
                    bw.WriteFixStrW(DisplayName, 0x40, (byte)(def.FormatVersion >= 104 ? 0x00 : 0x20));
                else
                    bw.WriteFixStr(DisplayName, 0x40, (byte)(def.FormatVersion >= 104 ? 0x00 : 0x20));

                byte padding = (byte)(def.FormatVersion >= 106 ? 0x00 : 0x20);
                bw.WriteFixStr(DisplayType.ToString(), 8, padding);
                bw.WriteFixStr(DisplayFormat, 8, padding);

                if (def.FormatVersion >= 203)
                {
                    bw.WritePattern(0x10, 0x00);
                }
                else
                {
                    bw.WriteSingle(Convert.ToSingle(Default));
                    bw.WriteSingle(Convert.ToSingle(Minimum));
                    bw.WriteSingle(Convert.ToSingle(Maximum));
                    bw.WriteSingle(Convert.ToSingle(Increment));
                }

                bw.WriteInt32((int)EditFlags);
                bw.WriteInt32(ParamUtil.GetValueSize(DisplayType) * (ParamUtil.IsArrayType(DisplayType) ? ArrayLength : 1));
                bw.ReserveVarint($"DescriptionOffset{index}");

                if (def.FormatVersion >= 202 || def.FormatVersion >= 106 && def.FormatVersion < 200)
                    bw.ReserveVarint($"InternalTypeOffset{index}");
                else
                    bw.WriteFixStr(InternalType, 0x20, padding);

                if (def.FormatVersion >= 202 || def.FormatVersion >= 106 && def.FormatVersion < 200)
                    bw.ReserveVarint($"InternalNameOffset{index}");
                else if (def.FormatVersion >= 102)
                    bw.WriteFixStr(MakeInternalName(), 0x20, padding);

                if (def.FormatVersion >= 104)
                    bw.WriteInt32(SortID);

                if (def.FormatVersion >= 200)
                {
                    bw.WriteInt32(0);
                    bw.ReserveInt64($"UnkB8Offset{index}");
                    bw.ReserveInt64($"UnkC0Offset{index}");
                    bw.ReserveInt64($"UnkC8Offset{index}");
                }
                else if (def.FormatVersion >= 106)
                {
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                    bw.WriteInt32(0);
                }

                if (def.FormatVersion >= 203)
                {
                    void writeVariableValue(object value)
                    {
                        switch (DisplayType)
                        {
                            case DefType.s8:
                            case DefType.u8:
                            case DefType.s16:
                            case DefType.u16:
                            case DefType.s32:
                            case DefType.u32:
                            case DefType.b32:
                                bw.WriteInt32(Convert.ToInt32(value));
                                bw.WriteInt32(0);
                                break;

                            case DefType.f32:
                            case DefType.angle32:
                                bw.WriteSingle(Convert.ToSingle(value));
                                bw.WriteInt32(0);
                                break;

                            case DefType.f64:
                                bw.WriteDouble(Convert.ToDouble(value));
                                break;

                            case DefType.dummy8:
                            case DefType.fixstr:
                            case DefType.fixstrW:
                                bw.WriteInt64(0);
                                break;

                            default:
                                throw new NotImplementedException($"Missing variable write for type: {DisplayType}");
                        }
                    }

                    writeVariableValue(Default);
                    writeVariableValue(Minimum);
                    writeVariableValue(Maximum);
                    writeVariableValue(Increment);
                }
            }

            internal void WriteStrings(BinaryWriterEx bw, PARAMDEF def, int index, Dictionary<string, long> sharedStringOffsets)
            {
                if (def.FormatVersion >= 202 || def.FormatVersion >= 106 && def.FormatVersion < 200)
                {
                    bw.FillVarint($"DisplayNameOffset{index}", bw.Position);
                    bw.WriteUTF16(DisplayName, true);
                }

                long descriptionOffset = 0;
                if (Description != null)
                {
                    descriptionOffset = bw.Position;
                    if (def.Unicode)
                        bw.WriteUTF16(Description, true);
                    else
                        bw.WriteShiftJIS(Description, true);
                }
                bw.FillVarint($"DescriptionOffset{index}", descriptionOffset);

                if (def.FormatVersion >= 202 || def.FormatVersion >= 106 && def.FormatVersion < 200)
                {
                    bw.FillVarint($"InternalTypeOffset{index}", bw.Position);
                    bw.WriteASCII(InternalType, true);

                    bw.FillVarint($"InternalNameOffset{index}", bw.Position);
                    bw.WriteASCII(MakeInternalName(), true);
                }

                if (def.FormatVersion >= 200)
                {
                    long writeSharedStringMaybe(string str, bool unicode)
                    {
                        if (str == null)
                            return 0;

                        if (!sharedStringOffsets.ContainsKey(str))
                        {
                            sharedStringOffsets[str] = bw.Position;
                            if (unicode)
                                bw.WriteUTF16(str, true);
                            else
                                bw.WriteASCII(str, true);
                        }
                        return sharedStringOffsets[str];
                    }

                    bw.FillInt64($"UnkB8Offset{index}", writeSharedStringMaybe(UnkB8, false));
                    bw.FillInt64($"UnkC0Offset{index}", writeSharedStringMaybe(UnkC0, false));
                    bw.FillInt64($"UnkC8Offset{index}", writeSharedStringMaybe(UnkC8, true));
                }
            }

            /// <summary>
            /// If the paramdef is version aware, this returns if this paramdef field is valid for a given regulation
            /// version
            /// </summary>
            /// <param name="version">The regulation version to check</param>
            /// <returns>True if the field is valid for this regulation version</returns>
            public bool IsValidForRegulationVersion(ulong version)
            {
                return version >= FirstRegulationVersion && 
                       (RemovedRegulationVersion == 0 || version < RemovedRegulationVersion);
            }

            private string MakeInternalName()
            {
                // This formatting is almost 100% accurate in DS1, less so in BB, and a complete crapshoot in DS3
                // C'est la vie.
                if (BitSize != -1)
                    return $"{InternalName}:{BitSize}";
                else if (ParamUtil.IsArrayType(DisplayType))
                    return $"{InternalName}[{ArrayLength}]";
                else
                    return InternalName;
            }

            /// <summary>
            /// Returns a string representation of the field.
            /// </summary>
            public override string ToString()
            {
                if (ParamUtil.IsBitType(DisplayType) && BitSize != -1)
                    return $"{DisplayType} {InternalName}:{BitSize}";
                else if (ParamUtil.IsArrayType(DisplayType))
                    return $"{DisplayType} {InternalName}[{ArrayLength}]";
                else
                    return $"{DisplayType} {InternalName}";
            }
        }
    }
}
