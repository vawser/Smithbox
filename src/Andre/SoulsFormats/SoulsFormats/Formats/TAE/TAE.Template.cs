using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace SoulsFormats
{
    public partial class TAE
    {
        /// <summary>
        /// Template for the parameters in an event.
        /// </summary>
        public class Template : Dictionary<long, Template.BankTemplate>
        {
            /// <summary>
            /// The game(s) this template is for.
            /// </summary>
            public TAEFormat Game;

            /// <summary>
            /// Creates new empty template.
            /// </summary>
            public Template()
                : base()
            {

            }

            private Template(XmlDocument xml)
                : base()
            {
                XmlNode templateNode = xml.SelectSingleNode("event_template");
                Game = (TAEFormat)Enum.Parse(typeof(TAEFormat), templateNode.Attributes["game"].InnerText);

                Dictionary<BankTemplate, long> basedOnMap = new Dictionary<BankTemplate, long>();

                foreach (XmlNode bankNode in templateNode.SelectNodes("bank"))
                {
                    var newBank = new BankTemplate(bankNode, out long basedOn);
                    basedOnMap.Add(newBank, basedOn);
                    if (ContainsKey(newBank.ID))
                    {
                        throw new Exception($"TAE Template has more than one bank with ID {newBank.ID}.");
                    }
                    Add(newBank.ID, newBank);
                }

                foreach (var kvp in basedOnMap)
                {
                    if (kvp.Value != -1)
                    {
                        foreach (var importFromKvp in this[kvp.Value])
                        {
                            if (!kvp.Key.ContainsKey(importFromKvp.Key))
                            {
                                kvp.Key.Add(importFromKvp.Key, importFromKvp.Value);
                            }
                        }
                    }
                }
            }

            /// <summary>
            /// Read a TAE template from an XML file.
            /// </summary>
            public static Template ReadXMLFile(string path)
            {
                var xml = new XmlDocument();
                xml.Load(path);
                return new Template(xml);
            }

            /// <summary>
            /// Read a TAE template from an XML string.
            /// </summary>
            public static Template ReadXMLText(string text)
            {
                var xml = new XmlDocument();
                xml.LoadXml(text);
                return new Template(xml);
            }

            /// <summary>
            /// Read a TAE template from an XML document.
            /// </summary>
            public static Template ReadXMLDoc(XmlDocument xml)
            {
                return new Template(xml);
            }

            /// <summary>
            /// A template for a bank of events.
            /// </summary>
            public class BankTemplate : Dictionary<int, EventTemplate>
            {
                /// <summary>
                /// ID of this bank template.
                /// </summary>
                public long ID;

                /// <summary>
                /// Name of this bank template.
                /// </summary>
                public string Name;

                internal BankTemplate(XmlNode bankNode, out long basedOn)
                    : base()
                {
                    ID = long.Parse(bankNode.Attributes["id"].InnerText);
                    Name = bankNode.Attributes["name"].InnerText;

                    basedOn = long.Parse(bankNode.Attributes["basedon"]?.InnerText ?? "-1");

                    EventTemplate lastGoodEventTemplate = null;

                    foreach (XmlNode eventNode in bankNode.SelectNodes("event"))
                    {
                        try
                        {
                            var newEvent = new EventTemplate(ID, eventNode);
                            if (ContainsKey(newEvent.ID))
                            {
                                throw new Exception($"TAE Bank Template has more than one event with ID {newEvent.ID}.");
                            }
                            Add(newEvent.ID, newEvent);

                            lastGoodEventTemplate = newEvent;
                        }
                        catch (Exception e)
                        {
                            if (lastGoodEventTemplate == null)
                            {
                                throw new Exception($"First event template in bank template {ID} failed to read:\n\n{e}");
                            }
                            else
                            {
                                throw new Exception($"Event template in bank template {ID} failed to read.\n\nLast valid event ID read: {lastGoodEventTemplate.ID}\n\nMessage:\n{e}");
                            }
                        }
                        
                    }
                }
            }

            /// <summary>
            /// Info about a parameter supplied to a TAE event.
            /// </summary>
            public class ParameterTemplate
            {
                /// <summary>
                /// Gets the byte count of a specific value type.
                /// </summary>
                public int GetByteCount()
                {
                    switch (Type)
                    {
                        case ParamType.s8:
                        case ParamType.u8:
                        case ParamType.x8:
                        case ParamType.b:
                            return 1;
                        case ParamType.s16:
                        case ParamType.u16:
                        case ParamType.x16:
                            return 2;
                        case ParamType.s32:
                        case ParamType.u32:
                        case ParamType.x32:
                        case ParamType.f32:
                            return 4;
                        case ParamType.s64:
                        case ParamType.u64:
                        case ParamType.x64:
                        case ParamType.f64:
                            return 8;
                        case ParamType.aob:
                            return AobLength;
                        default: throw new ArgumentException("Not a real ParamType");
                    }
                }

                /// <summary>
                /// Gets the System.Type of this parameter's value.
                /// </summary>
                public System.Type GetValueObjectType()
                {
                    switch (Type)
                    {
                        case ParamType.aob: return typeof(string);
                        case ParamType.u8: case ParamType.x8: return typeof(byte);
                        case ParamType.s8: return typeof(sbyte);
                        case ParamType.u16: case ParamType.x16: return typeof(ushort);
                        case ParamType.s16: return typeof(short);
                        case ParamType.u32: case ParamType.x32: return typeof(uint);
                        case ParamType.s32: return typeof(int);
                        case ParamType.u64: case ParamType.x64: return typeof(ulong);
                        case ParamType.s64: return typeof(long);
                        case ParamType.f32: return typeof(float);
                        case ParamType.f64: return typeof(double);
                        case ParamType.b: return typeof(bool);
                        default: throw new Exception($"Invalid ParamTemplate ParamType: {Type.ToString()}");
                    }
                }

                /// <summary>
                /// Converts a string to a value based on this ParameterTemplate's type.
                /// </summary>
                public object StringToValue(string str)
                {
                    if (str == null)
                        return null;

                    IEnumerable<string> GetArrayFromSingleLineString(string s)
                    {
                        return s.Split(' ')
                            .Where(st => !string.IsNullOrWhiteSpace(st))
                            .Select(st => st.Trim());
                    }

                    List<string> GetArrayFromString(string s)
                    {
                        List<string> result = new List<string>();
                        var lines = s.Split('\n');
                        foreach (var l in lines)
                            result.AddRange(GetArrayFromSingleLineString(l.Replace("\r", "").Replace("\n", "").Replace("\t", "")));
                        return result;
                    }

                    // Convert a string enum value to the actual numeric value.
                    if (EnumEntries != null)
                    {
                        if (EnumEntries.ContainsKey(str))
                            str = EnumEntries[str].ToString();
                    }

                    switch (Type)
                    {
                        case ParamType.aob: return GetArrayFromString(str).Select(b => byte.Parse(b, System.Globalization.NumberStyles.HexNumber)).ToArray();
                        case ParamType.u8: return byte.Parse(str);
                        case ParamType.x8: return byte.Parse(str, System.Globalization.NumberStyles.HexNumber);
                        case ParamType.s8: return sbyte.Parse(str);
                        case ParamType.u16: return ushort.Parse(str);
                        case ParamType.x16: return ushort.Parse(str, System.Globalization.NumberStyles.HexNumber);
                        case ParamType.s16: return short.Parse(str);
                        case ParamType.u32: return uint.Parse(str);
                        case ParamType.x32: return uint.Parse(str, System.Globalization.NumberStyles.HexNumber);
                        case ParamType.s32: return int.Parse(str);
                        case ParamType.u64: return ulong.Parse(str);
                        case ParamType.x64: return ulong.Parse(str, System.Globalization.NumberStyles.HexNumber);
                        case ParamType.s64: return long.Parse(str);
                        case ParamType.f32: return float.Parse(str);
                        case ParamType.f64: return double.Parse(str);
                        case ParamType.b:
                            string toLower = str.ToLower().Trim();
                            if (toLower == "true")
                                return true;
                            else if (toLower == "false")
                                return false;
                            else
                                throw new FormatException("Boolean value must be either 'True' or 'False', case-insensitive.");
                        default: throw new Exception($"Invalid ParamTemplate ParamType: {Type.ToString()}");
                    }
                }

                /// <summary>
                /// Converts a value to a string based on this ParameterTemplate's type.
                /// </summary>
                public string ValueToString(object val)
                {
                    if (EnumEntries != null)
                    {
                        if (EnumEntries.Values.Contains(val))
                        {
                            return EnumEntries.First(x => x.Value.Equals(val)).Key;
                        }
                    }

                    switch (Type)
                    {
                        case ParamType.aob: return string.Join(" ", ((byte[])val).Select(b => b.ToString("X2")));
                        case ParamType.x8: return ((byte)val).ToString("X2");
                        case ParamType.x16: return ((ushort)val).ToString("X4");
                        case ParamType.x32: return ((uint)val).ToString("X8");
                        case ParamType.x64: return ((ulong)val).ToString("X16");
                        case ParamType.b: return ((bool)val) ? "True" : "False";
                        default: return val.ToString();
                    }
                }

                internal void WriteValue(BinaryWriterEx bw, object value)
                {
                    switch (Type)
                    {
                        case ParamType.aob: bw.WriteBytes((byte[])value); break;
                        case ParamType.b: bw.WriteBoolean((bool)value); break;
                        case ParamType.u8: case ParamType.x8: bw.WriteByte((byte)value); break;
                        case ParamType.s8: bw.WriteSByte((sbyte)value); break;
                        case ParamType.u16: case ParamType.x16: bw.WriteUInt16((ushort)value); break;
                        case ParamType.s16: bw.WriteInt16((short)value); break;
                        case ParamType.u32: case ParamType.x32: bw.WriteUInt32((uint)value); break;
                        case ParamType.s32: bw.WriteInt32((int)value); break;
                        case ParamType.u64: case ParamType.x64: bw.WriteUInt64((ulong)value); break;
                        case ParamType.s64: bw.WriteInt64((long)value); break;
                        case ParamType.f32: bw.WriteSingle((float)value); break;
                        case ParamType.f64: bw.WriteDouble((double)value); break;
                        default: throw new Exception($"Invalid ParamTemplate ParamType: {Type.ToString()}");
                    }
                }

                internal object ReadValue(BinaryReaderEx br)
                {
                    switch (Type)
                    {
                        case ParamType.aob: return br.ReadBytes(AobLength);
                        case ParamType.b: return br.ReadBoolean();
                        case ParamType.u8: case ParamType.x8: return br.ReadByte();
                        case ParamType.s8: return br.ReadSByte();
                        case ParamType.u16: case ParamType.x16: return br.ReadUInt16();
                        case ParamType.s16: return br.ReadInt16();
                        case ParamType.u32: case ParamType.x32: return br.ReadUInt32();
                        case ParamType.s32: return br.ReadInt32();
                        case ParamType.u64: case ParamType.x64: return br.ReadUInt64();
                        case ParamType.s64: return br.ReadInt64();
                        case ParamType.f32: return br.ReadSingle();
                        case ParamType.f64: return br.ReadDouble();
                        default: throw new Exception($"Invalid ParamTemplate ParamType: {Type.ToString()}");
                    }
                }

                internal void AssertValue(BinaryReaderEx br)
                {
                    switch (Type)
                    {
                        case ParamType.aob:
                            var assertAob = (byte[])ValueToAssert;
                            for (int i = 0; i < AobLength; i++)
                            {
                                br.AssertByte(assertAob[i]);
                            }
                            break;
                        case ParamType.b: br.AssertBoolean((bool)ValueToAssert); break;
                        case ParamType.u8: case ParamType.x8: br.AssertByte((byte)ValueToAssert); break;
                        case ParamType.s8: br.AssertSByte((sbyte)ValueToAssert); break;
                        case ParamType.u16: case ParamType.x16: br.AssertUInt16((ushort)ValueToAssert); break;
                        case ParamType.s16: br.AssertInt16((short)ValueToAssert); break;
                        case ParamType.u32: case ParamType.x32: br.AssertUInt32((uint)ValueToAssert); break;
                        case ParamType.s32: br.AssertInt32((int)ValueToAssert); break;
                        case ParamType.u64: case ParamType.x64: br.AssertUInt64((ulong)ValueToAssert); break;
                        case ParamType.s64: br.AssertInt64((long)ValueToAssert); break;
                        case ParamType.f32: br.AssertSingle((float)ValueToAssert); break;
                        case ParamType.f64: br.AssertDouble((double)ValueToAssert); break;
                        default: throw new Exception($"Invalid ParamTemplate ParamType: {Type.ToString()}");
                    }
                }

                internal void WriteAssertValue(BinaryWriterEx bw)
                {
                    switch (Type)
                    {
                        case ParamType.aob:
                            var assertAob = (byte[])ValueToAssert;
                            bw.WriteBytes(assertAob);
                            break;
                        case ParamType.b: bw.WriteBoolean((bool)ValueToAssert); break;
                        case ParamType.u8: case ParamType.x8: bw.WriteByte((byte)ValueToAssert); break;
                        case ParamType.s8: bw.WriteSByte((sbyte)ValueToAssert); break;
                        case ParamType.u16: case ParamType.x16: bw.WriteUInt16((ushort)ValueToAssert); break;
                        case ParamType.s16: bw.WriteInt16((short)ValueToAssert); break;
                        case ParamType.u32: case ParamType.x32: bw.WriteUInt32((uint)ValueToAssert); break;
                        case ParamType.s32: bw.WriteInt32((int)ValueToAssert); break;
                        case ParamType.u64: case ParamType.x64: bw.WriteUInt64((ulong)ValueToAssert); break;
                        case ParamType.s64: bw.WriteInt64((long)ValueToAssert); break;
                        case ParamType.f32: bw.WriteSingle((float)ValueToAssert); break;
                        case ParamType.f64: bw.WriteDouble((double)ValueToAssert); break;
                        default: throw new Exception($"Invalid ParamTemplate ParamType: {Type.ToString()}");
                    }
                }

                internal void WriteDefaultValue(BinaryWriterEx bw)
                {
                    if (ValueToAssert != null)
                    {
                        WriteAssertValue(bw);
                    }
                    else if (DefaultValue == null)
                    {
                        switch (Type)
                        {
                            case ParamType.aob:
                                for (int i = 0; i < AobLength; i++)
                                    bw.WriteByte(0);
                                break;
                            case ParamType.b: case ParamType.u8: case ParamType.x8: bw.WriteByte(0); break;
                            case ParamType.s8: bw.WriteSByte(0); break;
                            case ParamType.u16: case ParamType.x16: bw.WriteUInt16(0); break;
                            case ParamType.s16: bw.WriteInt16(0); break;
                            case ParamType.u32: case ParamType.x32: bw.WriteUInt32(0); break;
                            case ParamType.s32: bw.WriteInt32(0); break;
                            case ParamType.u64: case ParamType.x64: bw.WriteUInt64(0); break;
                            case ParamType.s64: bw.WriteInt64(0); break;
                            case ParamType.f32: bw.WriteSingle(0); break;
                            case ParamType.f64: bw.WriteDouble(0); break;
                            default: throw new Exception($"Invalid ParamTemplate ParamType: {Type.ToString()}");
                        }
                    }
                    else
                    {
                        switch (Type)
                        {
                            case ParamType.aob:
                                var assertAob = (byte[])DefaultValue;
                                bw.WriteBytes(assertAob);
                                break;
                            case ParamType.b: case ParamType.u8: case ParamType.x8: bw.WriteByte((byte)DefaultValue); break;
                            case ParamType.s8: bw.WriteSByte((sbyte)DefaultValue); break;
                            case ParamType.u16: case ParamType.x16: bw.WriteUInt16((ushort)DefaultValue); break;
                            case ParamType.s16: bw.WriteInt16((short)DefaultValue); break;
                            case ParamType.u32: case ParamType.x32: bw.WriteUInt32((uint)DefaultValue); break;
                            case ParamType.s32: bw.WriteInt32((int)DefaultValue); break;
                            case ParamType.u64: case ParamType.x64: bw.WriteUInt64((ulong)DefaultValue); break;
                            case ParamType.s64: bw.WriteInt64((long)DefaultValue); break;
                            case ParamType.f32: bw.WriteSingle((float)DefaultValue); break;
                            case ParamType.f64: bw.WriteDouble((double)DefaultValue); break;
                            default: throw new Exception($"Invalid ParamTemplate ParamType: {Type.ToString()}");
                        }
                    }
                    
                }

                /// <summary>
                /// The value type of this parameter.
                /// </summary>
                public ParamType Type;

                /// <summary>
                /// The name of this parameter.
                /// </summary>
                public string Name;

                /// <summary>
                /// (Optional) The value which should be asserted on this parameter.
                /// </summary>
                public object ValueToAssert = null;

                /// <summary>
                /// (Optional) The default value to set when creating a new event of
                /// this type from scratch. Otherwise a 0 value will be used in such a case.
                /// </summary>
                public object DefaultValue = null;

                /// <summary>
                /// (Only applies if Type == ParamType.aob)
                /// The length of the array of bytes.
                /// </summary>
                public int AobLength = -1;

                /// <summary>
                /// Possible values if this is an enum, otherwise it's null.
                /// </summary>
                public Dictionary<string, object> EnumEntries { get; private set; } = null;

                /// <summary>
                /// Sorts the enum entries by key.
                /// </summary>
                public void SortEnumEntries()
                {
                    EnumEntries = EnumEntries.OrderBy(kvp => kvp.Key)
                        .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
                }

                internal ParameterTemplate(long bankId, long eventId, long paramIndex, XmlNode paramNode, int offset)
                {
                    Type = (ParamType)Enum.Parse(typeof(ParamType), paramNode.Name);

                    Name = paramNode.Attributes["name"]?.InnerText ?? $"Unk{offset:X2}";

                    // Load enum entries before doing default value so you can make the default value an enum entry.
                    var enumNodes = paramNode.SelectNodes("entry");
                    if (enumNodes.Count > 0)
                    {
                        EnumEntries = new Dictionary<string, object>();
                        foreach (XmlNode entryNode in paramNode.SelectNodes("entry"))
                        {
                            var entryName = entryNode.Attributes["name"].InnerText;
                            var entryValue = StringToValue(entryNode.Attributes["value"].InnerText);
                            EnumEntries.Add(entryName, entryValue);
                        }
                    }

                    if (paramNode.HasChildNodes)
                    {
                        var valueNode = paramNode.SelectSingleNode("assert");
                        if (valueNode != null)
                        {
                            ValueToAssert = StringToValue(valueNode.InnerText);
                        }

                        var defaultValueNode = paramNode.SelectSingleNode("default");
                        if (defaultValueNode != null)
                        {
                            DefaultValue = StringToValue(defaultValueNode.InnerText);
                        }
                    }

                    try
                    {
                        if (ValueToAssert == null)
                            ValueToAssert = StringToValue(paramNode.Attributes["assert"]?.InnerText);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Bank {bankId} -> Event {eventId} -> Parameter {(Name != null ? $"'{Name}'" : $"{paramIndex}")}\n    Failed to read 'assert' attribute of parameter.\n\n\n{ex}");
                    }
                    
                    try
                    {
                        if (DefaultValue == null)
                            DefaultValue = StringToValue(paramNode.Attributes["default"]?.InnerText);
                    }
                    catch (Exception ex)
                    {
                        if (EnumEntries != null && EnumEntries.Count > 0)
                            throw new Exception($"Bank {bankId} -> Event {eventId} -> Parameter {(Name != null ? $"'{Name}'" : $"{paramIndex}")}\n    Failed to read 'default' attribute of parameter. Note: default value must be an integer on enums.\n\n\n{ex}");
                        else
                            throw new Exception($"Bank {bankId} -> Event {eventId} -> Parameter {(Name != null ? $"'{Name}'" : $"{paramIndex}")}\n    Failed to read 'default' attribute of parameter.\n\n\n{ex}");
                    }

                    var lengthAttribute = paramNode.Attributes["length"];
                    if (lengthAttribute != null)
                    {
                        try
                        {
                            AobLength = int.Parse(lengthAttribute.InnerText);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Bank {bankId} -> Event {eventId} -> Parameter {(Name != null ? $"'{Name}'" : $"{paramIndex}")}\n    Failed to read 'length' attribute of parameter.\n\n\n{ex}");
                        }
                    }
                    else
                    {
                        if (Type == ParamType.aob)
                        {
                            throw new Exception($"Bank {bankId} -> Event {eventId} -> Parameter {(Name != null ? $"'{Name}'" : $"{paramIndex}")} was an " +
                                $"array of bytes but no length was specified");
                        }
                    }

                    if (Type == ParamType.aob && ValueToAssert != null)
                    {
                        var aob = (byte[])ValueToAssert;
                        if (aob.Length != AobLength)
                        {
                            throw new Exception($"Bank {bankId} -> Event {eventId} -> Parameter {(Name != null ? $"'{Name}'" : $"{paramIndex}")}: " +
                                $"AoB assert value length was {aob.Length} but 'length' " +
                                $"attribute was set to {AobLength}.");
                        }
                    }
                    
                }
            }

            /// <summary>
            /// Info about an event in a TAE file.
            /// </summary>
            public class EventTemplate : Dictionary<string, ParameterTemplate>
            {
                /// <summary>
                /// ID of this TAE event.
                /// </summary>
                public readonly int ID;

                /// <summary>
                /// Name of this TAE event.
                /// </summary>
                public string Name;

                /// <summary>
                /// Gets the default byte array for this event, using the parameters'
                /// DefaultValue properties if applicable.
                /// </summary>
                public byte[] GetDefaultBytes(bool isBigEndian)
                {
                    using (var memStream = new System.IO.MemoryStream())
                    {
                        var bw = new BinaryWriterEx(isBigEndian, memStream);

                        foreach (var paramKvp in this)
                        {
                            var p = paramKvp.Value;
                            paramKvp.Value.WriteDefaultValue(bw);
                        }

                        return memStream.ToArray();
                    }
                }

                /// <summary>
                /// Gets the byte count of the entire list of parameters.
                /// </summary>
                public int GetAllParametersByteCount()
                {
                    int result = 0;
                    foreach (var paramKvp in this)
                    {
                        result += paramKvp.Value.GetByteCount();
                    }
                    return result;
                }

                internal EventTemplate(long bankId, XmlNode eventNode)
                    : base()
                {
                    ID = int.Parse(eventNode.Attributes["id"].InnerText);
                    Name = eventNode.Attributes["name"]?.InnerText ?? $"Event{ID}";
                    int i = 0;
                    int offset = 0;
                    foreach (XmlNode paramNode in eventNode.ChildNodes)
                    {
                        if (paramNode.Name == "#comment")
                            continue;
                        var newParam = new ParameterTemplate(bankId, ID, i++, paramNode, offset);
                        var paramSize = newParam.GetByteCount();
                        Add(newParam.Name, newParam);
                        offset += paramSize;
                    }
                }
            }

            /// <summary>
            /// Possible types for values in an event parameter.
            /// </summary>
            public enum ParamType
            {
                /// <summary>
                /// Single-byte boolean value.
                /// </summary>
                b,

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
                /// Unsigned long.
                /// </summary>
                u64,

                /// <summary>
                /// Unsigned long, display as hex.
                /// </summary>
                x64,

                /// <summary>
                /// Signed long.
                /// </summary>
                s64,

                /// <summary>
                /// Single-precision float.
                /// </summary>
                f32,

                /// <summary>
                /// Double-precision float.
                /// </summary>
                f64,

                /// <summary>
                /// Array of bytes.
                /// </summary>
                aob,
            }

        }
    }
}
