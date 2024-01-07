using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace SoulsFormats
{
    public partial class PARAM : SoulsFile<PARAM>
    {
        /// <summary>
        /// The layout of cell data within each row in a param.
        /// </summary>
        [Obsolete]
        public class Layout : List<Layout.Entry>
        {
            /// <summary>
            /// Collections of named values which may be referenced by cells.
            /// </summary>
            public Dictionary<string, Enum> Enums;

            /// <summary>
            /// The size of a row, determined automatically from the layout.
            /// </summary>
            public int Size
            {
                get
                {
                    int size = 0;

                    for (int i = 0; i < Count; i++)
                    {
                        CellType type = this[i].Type;

                        void ConsumeBools(CellType boolType, int fieldSize)
                        {
                            size += fieldSize;

                            int j;
                            for (j = 0; j < fieldSize * 8; j++)
                            {
                                if (i + j >= Count || this[i + j].Type != boolType)
                                    break;
                            }
                            i += j - 1;
                        }

                        if (type == CellType.b8)
                            ConsumeBools(type, 1);
                        else if (type == CellType.b16)
                            ConsumeBools(type, 2);
                        else if (type == CellType.b32)
                            ConsumeBools(type, 4);
                        else
                            size += this[i].Size;
                    }

                    return size;
                }
            }

            /// <summary>
            /// Read a PARAM layout from an XML file.
            /// </summary>
            public static Layout ReadXMLFile(string path)
            {
                var xml = new XmlDocument();
                xml.Load(path);
                return new Layout(xml);
            }

            /// <summary>
            /// Read a PARAM layout from an XML string.
            /// </summary>
            public static Layout ReadXMLText(string text)
            {
                var xml = new XmlDocument();
                xml.LoadXml(text);
                return new Layout(xml);
            }

            /// <summary>
            /// Read a PARAM layout from an XML document.
            /// </summary>
            public static Layout ReadXMLDoc(XmlDocument xml)
            {
                return new Layout(xml);
            }

            /// <summary>
            /// Creates a new empty layout.
            /// </summary>
            public Layout() : base()
            {
                Enums = new Dictionary<string, Enum>();
            }

            private Layout(XmlDocument xml) : base()
            {
                Enums = new Dictionary<string, Enum>();
                foreach (XmlNode node in xml.SelectNodes("/layout/enum"))
                {
                    string enumName = node.Attributes["name"].InnerText;
                    Enums[enumName] = new Enum(node);
                }

                foreach (XmlNode node in xml.SelectNodes("/layout/entry"))
                    Add(new Entry(node));
            }

            /// <summary>
            /// Write the layout to an XML file.
            /// </summary>
            public void Write(string path)
            {
                var xws = new XmlWriterSettings()
                {
                    Indent = true,
                };
                var xw = XmlWriter.Create(path, xws);
                xw.WriteStartElement("layout");

                foreach (Entry entry in this)
                    entry.Write(xw);

                xw.WriteEndElement();
                xw.Close();
            }

            /// <summary>
            /// Converts the layout to a paramdef with the given type, and all child enums to paramtdfs.
            /// </summary>
            public PARAMDEF ToParamdef(string paramType, out List<PARAMTDF> paramtdfs)
            {
                paramtdfs = new List<PARAMTDF>(Enums.Count);
                foreach (string enumName in Enums.Keys)
                    paramtdfs.Add(Enums[enumName].ToParamtdf(enumName));

                var def = new PARAMDEF { ParamType = paramType, Unicode = true, FormatVersion = 201 };
                foreach (Entry entry in this)
                {
                    PARAMDEF.DefType fieldType;
                    switch (entry.Type)
                    {
                        case CellType.dummy8: fieldType = PARAMDEF.DefType.dummy8; break;
                        case CellType.b8:
                        case CellType.u8:
                        case CellType.x8: fieldType = PARAMDEF.DefType.u8; break;
                        case CellType.s8: fieldType = PARAMDEF.DefType.s8; break;
                        case CellType.b16:
                        case CellType.u16:
                        case CellType.x16: fieldType = PARAMDEF.DefType.u16; break;
                        case CellType.s16: fieldType = PARAMDEF.DefType.s16; break;
                        case CellType.b32:
                        case CellType.u32:
                        case CellType.x32: fieldType = PARAMDEF.DefType.u32; break;
                        case CellType.s32: fieldType = PARAMDEF.DefType.s32; break;
                        case CellType.f32: fieldType = PARAMDEF.DefType.f32; break;
                        case CellType.fixstr: fieldType = PARAMDEF.DefType.fixstr; break;
                        case CellType.fixstrW: fieldType = PARAMDEF.DefType.fixstrW; break;

                        default:
                            throw new NotImplementedException($"DefType not specified for CellType {entry.Type}.");
                    }

                    var field = new PARAMDEF.Field(def, fieldType, entry.Name);
                    field.Description = entry.Description;
                    if (entry.Enum != null)
                        field.InternalType = entry.Enum;

                    if (entry.Type == CellType.s8)
                        field.Default = (sbyte)entry.Default;
                    else if (entry.Type == CellType.u8 || entry.Type == CellType.x8)
                        field.Default = (byte)entry.Default;
                    else if (entry.Type == CellType.s16)
                        field.Default = (short)entry.Default;
                    else if (entry.Type == CellType.u16 || entry.Type == CellType.x16)
                        field.Default = (ushort)entry.Default;
                    else if (entry.Type == CellType.s32)
                        field.Default = (int)entry.Default;
                    else if (entry.Type == CellType.u32 || entry.Type == CellType.x32)
                        field.Default = (uint)entry.Default;
                    else if (entry.Type == CellType.dummy8 || entry.Type == CellType.fixstr)
                        field.ArrayLength = entry.Size;
                    else if (entry.Type == CellType.fixstrW)
                        field.ArrayLength = entry.Size / 2;
                    else if (entry.Type == CellType.b8 || entry.Type == CellType.b16 || entry.Type == CellType.b32)
                    {
                        field.Default = (bool)entry.Default ? 1 : 0;
                        field.BitSize = 1;
                    }

                    def.Fields.Add(field);
                }
                return def;
            }

            /// <summary>
            /// Parse a string according to the given param type and culture.
            /// </summary>
            public static object ParseParamValue(CellType type, string value, CultureInfo culture)
            {
                if (type == CellType.fixstr || type == CellType.fixstrW)
                    return value;
                else if (type == CellType.b8 || type == CellType.b16 || type == CellType.b32)
                    return bool.Parse(value);
                else if (type == CellType.s8)
                    return sbyte.Parse(value);
                else if (type == CellType.u8)
                    return byte.Parse(value);
                else if (type == CellType.x8)
                    return Convert.ToByte(value, 16);
                else if (type == CellType.s16)
                    return short.Parse(value);
                else if (type == CellType.u16)
                    return ushort.Parse(value);
                else if (type == CellType.x16)
                    return Convert.ToUInt16(value, 16);
                else if (type == CellType.s32)
                    return int.Parse(value);
                else if (type == CellType.u32)
                    return uint.Parse(value);
                else if (type == CellType.x32)
                    return Convert.ToUInt32(value, 16);
                else if (type == CellType.f32)
                    return float.Parse(value, culture);
                else
                    throw new InvalidCastException("Unparsable type: " + type);
            }

            /// <summary>
            /// Parse a string according to the given param type and invariant culture.
            /// </summary>
            public static object ParseParamValue(CellType type, string value)
            {
                return ParseParamValue(type, value, CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// Convert a param value of the specified type to a string using the given culture.
            /// </summary>
            public static string ParamValueToString(CellType type, object value, CultureInfo culture)
            {
                if (type == CellType.x8)
                    return $"0x{value:X2}";
                else if (type == CellType.x16)
                    return $"0x{value:X4}";
                else if (type == CellType.x32)
                    return $"0x{value:X8}";
                else if (type == CellType.f32)
                    return Convert.ToString(value, culture);
                else
                    return value.ToString();
            }

            /// <summary>
            /// Convert a param value of the specified type to a string using invariant culture.
            /// </summary>
            public static string ParamValueToString(CellType type, object value)
            {
                return ParamValueToString(type, value, CultureInfo.InvariantCulture);
            }

            /// <summary>
            /// The type and name of one cell in a row.
            /// </summary>
            public class Entry
            {
                /// <summary>
                /// The type of the cell.
                /// </summary>
                public CellType Type { get; set; }

                /// <summary>
                /// The name of the cell.
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                /// Size in bytes of the entry; may only be set for fixstr, fixstrW, and dummy8.
                /// </summary>
                public int Size
                {
                    get
                    {
                        if (IsVariableSize)
                            return size;
                        else if (Type == CellType.s8 || Type == CellType.u8 || Type == CellType.x8)
                            return 1;
                        else if (Type == CellType.s16 || Type == CellType.u16 || Type == CellType.x16)
                            return 2;
                        else if (Type == CellType.s32 || Type == CellType.u32 || Type == CellType.x32 || Type == CellType.f32)
                            return 4;
                        // Not meaningful
                        else if (Type == CellType.b8 || Type == CellType.b16 || Type == CellType.b32)
                            return 0;
                        else
                            throw new InvalidCastException("Unknown type: " + Type);
                    }

                    set
                    {
                        if (IsVariableSize)
                            size = value;
                        else
                            throw new InvalidOperationException("Size may only be set for variable-width types: fixstr, fixstrW, and dummy8.");
                    }
                }
                private int size;

                /// <summary>
                /// The default value to use when creating a new row.
                /// </summary>
                public object Default
                {
                    get
                    {
                        if (Type == CellType.dummy8)
                            return new byte[Size];
                        else
                            return def;
                    }

                    set
                    {
                        if (Type == CellType.dummy8)
                            throw new InvalidOperationException("Default may not be set for dummy8.");
                        else
                            def = value;
                    }
                }
                private object def;

                /// <summary>
                /// Whether the size can be modified.
                /// </summary>
                public bool IsVariableSize => Type == CellType.fixstr || Type == CellType.fixstrW || Type == CellType.dummy8;

                /// <summary>
                /// A description of this field's purpose; may be null.
                /// </summary>
                public string Description;

                /// <summary>
                /// If not null, the enum containing possible values for this cell.
                /// </summary>
                public string Enum;

                /// <summary>
                /// Create a new entry of a fixed-width type.
                /// </summary>
                public Entry(CellType type, string name, object def)
                {
                    Type = type;
                    Name = name;
                    Default = def;
                }

                /// <summary>
                /// Create a new entry of a variable-width type. Default is ignored for dummy8.
                /// </summary>
                public Entry(CellType type, string name, int size, object def)
                {
                    Type = type;
                    Name = name;
                    Size = size;
                    this.def = Type == CellType.dummy8 ? null : def;
                }

                internal Entry(XmlNode node)
                {
                    Name = node.SelectSingleNode("name").InnerText;
                    Type = (CellType)System.Enum.Parse(typeof(CellType), node.SelectSingleNode("type").InnerText, true);

                    if (IsVariableSize)
                        size = int.Parse(node.SelectSingleNode("size").InnerText);

                    if (Type != CellType.dummy8)
                        Default = ParseParamValue(Type, node.SelectSingleNode("default").InnerText);

                    Description = node.SelectSingleNode("description")?.InnerText;
                    Enum = node.SelectSingleNode("enum")?.InnerText;
                }

                internal void Write(XmlWriter xw)
                {
                    xw.WriteStartElement("entry");
                    xw.WriteElementString("name", Name);
                    xw.WriteElementString("type", Type.ToString());

                    if (IsVariableSize)
                        xw.WriteElementString("size", Size.ToString());

                    if (Type != CellType.dummy8)
                        xw.WriteElementString("default", ParamValueToString(Type, Default));

                    if (Description != null)
                        xw.WriteElementString("description", Description);

                    if (Enum != null)
                        xw.WriteElementString("enum", Enum);

                    xw.WriteEndElement();
                }
            }
        }

        /// <summary>
        /// Possible types for values in a param.
        /// </summary>
        [Obsolete]
        public enum CellType
        {
            /// <summary>
            /// Array of bytes.
            /// </summary>
            dummy8,

            /// <summary>
            /// 1-bit bool in a 1-byte field.
            /// </summary>
            b8,

            /// <summary>
            /// 1-bit bool in a 2-byte field.
            /// </summary>
            b16,

            /// <summary>
            /// 1-bit bool in a 4-byte field.
            /// </summary>
            b32,

            /// <summary>
            /// Unsigned byte.
            /// </summary>
            u8,

            /// <summary>
            /// Unsigned byte, display as hex.
            /// </summary>
            x8,

            /// <summary>
            /// Signed byte.
            /// </summary>
            s8,

            /// <summary>
            /// Unsigned short.
            /// </summary>
            u16,

            /// <summary>
            /// Unsigned short, display as hex.
            /// </summary>
            x16,

            /// <summary>
            /// Signed short.
            /// </summary>
            s16,

            /// <summary>
            /// Unsigned int.
            /// </summary>
            u32,

            /// <summary>
            /// Unsigned int, display as hex.
            /// </summary>
            x32,

            /// <summary>
            /// Signed int.
            /// </summary>
            s32,

            /// <summary>
            /// Single-precision float.
            /// </summary>
            f32,

            /// <summary>
            /// Shift-JIS encoded string.
            /// </summary>
            fixstr,

            /// <summary>
            /// UTF-16 encoded string.
            /// </summary>
            fixstrW,
        }
    }
}
