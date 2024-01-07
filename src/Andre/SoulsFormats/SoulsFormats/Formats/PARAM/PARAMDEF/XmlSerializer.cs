using SoulsFormats.XmlExtensions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;

namespace SoulsFormats
{
    public partial class PARAMDEF
    {
        private static class XmlSerializer
        {
            public const int CURRENT_XML_VERSION = 3;
            private static Regex FIELD_NAME_VALIDATOR = new Regex("^\\w+$");

            public static PARAMDEF Deserialize(XmlDocument xml, bool versionAware)
            {
                var def = new PARAMDEF();
                XmlNode root = xml.SelectSingleNode("PARAMDEF");
                // In the interest of maximum compatibility, we will no longer check the XML version;
                // just try everything and hope it works.

                def.ParamType = root.SelectSingleNode("ParamType").InnerText;
                def.DataVersion = root.ReadInt16IfExist("DataVersion") ?? root.ReadInt16("Unk06");
                def.BigEndian = root.ReadBoolean("BigEndian");
                def.Unicode = root.ReadBoolean("Unicode");
                def.FormatVersion = root.ReadInt16IfExist("FormatVersion") ?? root.ReadInt16("Version");

                def.Fields = new List<Field>();
                foreach (XmlNode node in root.SelectNodes("Fields/Field"))
                {
                    var field = DeserializeField(def, node, versionAware);
                    if (field != null)
                        def.Fields.Add(field);
                }
                
                def.VersionAware = versionAware;

                return def;
            }

            public static void Serialize(PARAMDEF def, XmlWriter xw, int xmlVersion)
            {
                if (xmlVersion < 0 || xmlVersion > CURRENT_XML_VERSION)
                    throw new InvalidOperationException($"XML version {xmlVersion} not recognized.");

                xw.WriteStartDocument();
                xw.WriteStartElement("PARAMDEF");
                xw.WriteAttributeString("XmlVersion", xmlVersion.ToString());
                xw.WriteElementString("ParamType", def.ParamType);
                xw.WriteElementString(xmlVersion == 0 ? "Unk06" : "DataVersion", def.DataVersion.ToString());
                xw.WriteElementString("BigEndian", def.BigEndian.ToString());
                xw.WriteElementString("Unicode", def.Unicode.ToString());
                xw.WriteElementString(xmlVersion == 0 ? "Version" : "FormatVersion", def.FormatVersion.ToString());

                xw.WriteStartElement("Fields");
                foreach (Field field in def.Fields)
                {
                    xw.WriteStartElement("Field");
                    SerializeField(def, field, xw);
                    xw.WriteEndElement();
                }
                xw.WriteEndElement();

                xw.WriteEndElement();
            }


            private static readonly Regex defOuterRx = new Regex($@"^(?<type>\S+)\s+(?<name>.+?)(?:\s*=\s*(?<default>\S+))?$");
            private static readonly Regex defBitRx = new Regex($@"^(?<name>.+?)\s*:\s*(?<size>\d+)$");
            private static readonly Regex defArrayRx = new Regex($@"^(?<name>.+?)\s*\[\s*(?<length>\d+)\]$");

            private static Field DeserializeField(PARAMDEF def, XmlNode node, bool versionAware)
            {
                // Check regulation version info for the field if it exists
                ulong firstVersion = 0;
                ulong removedVersion = 0;
                var firstVersionAttribute = node.Attributes?["FirstVersion"];
                var removedVersionAttribute = node.Attributes?["RemovedVersion"];
                if (firstVersionAttribute != null && !ulong.TryParse(firstVersionAttribute.InnerText, out firstVersion))
                    throw new Exception("FirstVersion attribute is not a valid integer");
                if (removedVersionAttribute != null && !ulong.TryParse(removedVersionAttribute.InnerText, out removedVersion))
                    throw new Exception("RemovedVersion attribute is not a valid integer");
                
                // If we are not reading paramdefs in a version aware way, we implicitly are loading paramdefs for the
                // latest defined version and will throw away any fields that are ever removed
                if (!versionAware && removedVersion != 0)
                    return null;
                
                var field = new Field();
                string fieldDef = node.Attributes["Def"].InnerText;
                Match outerMatch = defOuterRx.Match(fieldDef);
                field.DisplayType = (DefType)Enum.Parse(typeof(DefType), outerMatch.Groups["type"].Value.Trim());
                if (outerMatch.Groups["default"].Success)
                    field.Default = float.Parse(outerMatch.Groups["default"].Value, CultureInfo.InvariantCulture);
                else
                    field.Default = ParamUtil.GetDefaultDefault(def, field.DisplayType);

                string internalName = outerMatch.Groups["name"].Value.Trim();
                Match bitMatch = defBitRx.Match(internalName);
                Match arrayMatch = defArrayRx.Match(internalName);
                field.BitSize = -1;
                field.ArrayLength = 1;
                if (ParamUtil.IsBitType(field.DisplayType) && bitMatch.Success)
                {
                    field.BitSize = int.Parse(bitMatch.Groups["size"].Value);
                    internalName = bitMatch.Groups["name"].Value;
                }
                else if (ParamUtil.IsArrayType(field.DisplayType))
                {
                    field.ArrayLength = int.Parse(arrayMatch.Groups["length"].Value);
                    internalName = arrayMatch.Groups["name"].Value;
                }
                field.InternalName = internalName;

                field.DisplayName = node.ReadStringOrDefault("DisplayName", field.InternalName);
                field.InternalType = node.ReadStringOrDefault("Enum", field.DisplayType.ToString());
                field.Description = node.ReadStringIfExist("Description");
                field.DisplayFormat = node.ReadStringOrDefault("DisplayFormat", ParamUtil.GetDefaultFormat(field.DisplayType));
                field.EditFlags = (EditFlags)Enum.Parse(typeof(EditFlags),
                    node.ReadStringOrDefault("EditFlags", ParamUtil.GetDefaultEditFlags(field.DisplayType).ToString()));
                field.Minimum = ReadVariableValueOrDefault(def, node, field.DisplayType, "Minimum", ParamUtil.GetDefaultMinimum(def, field.DisplayType));
                field.Maximum = ReadVariableValueOrDefault(def, node, field.DisplayType, "Maximum", ParamUtil.GetDefaultMaximum(def, field.DisplayType));
                field.Increment = ReadVariableValueOrDefault(def, node, field.DisplayType, "Increment", ParamUtil.GetDefaultIncrement(def, field.DisplayType));
                field.SortID = node.ReadInt32OrDefault("SortID", 0);

                field.UnkB8 = node.ReadStringIfExist("UnkB8");
                field.UnkC0 = node.ReadStringIfExist("UnkC0");
                field.UnkC8 = node.ReadStringIfExist("UnkC8");

                if (versionAware)
                {
                    field.FirstRegulationVersion = firstVersion;
                    field.RemovedRegulationVersion = removedVersion;
                }

                if (!FIELD_NAME_VALIDATOR.IsMatch(internalName))
                    throw new Exception("Disallowed field name found in paramdef: " + def.ParamType + ", name: " + field.InternalName);
                // Check same name, and if version aware, check they aren't replacing eachother
                bool matchingFieldTest(Field ifield) => string.Equals(field.InternalName, ifield.InternalName)
                    && (!versionAware || (
                        (field.RemovedRegulationVersion == 0 || field.RemovedRegulationVersion > ifield.FirstRegulationVersion)
                        && (ifield.RemovedRegulationVersion == 0 || field.FirstRegulationVersion < ifield.RemovedRegulationVersion)));
                Field field2 = def.Fields.Find(matchingFieldTest);
                if (field2 != null)
                    throw new Exception("Repeated field name found in paramdef: " + def.ParamType + ", name: " + field.InternalName);
                
                return field;
            }

            private static object ParseVariableValue(PARAMDEF def, DefType type, string text)
            {
                if (def.VariableEditorValueTypes)
                {
                    switch (type)
                    {
                        case DefType.s8:
                        case DefType.u8:
                        case DefType.s16:
                        case DefType.u16:
                        case DefType.s32:
                        case DefType.u32:
                        case DefType.b32:
                            return int.Parse(text);

                        case DefType.f32:
                        case DefType.angle32:
                            return float.Parse(text, CultureInfo.InvariantCulture);

                        case DefType.f64:
                            return double.Parse(text, CultureInfo.InvariantCulture);

                        case DefType.dummy8:
                        case DefType.fixstr:
                        case DefType.fixstrW:
                            return null;

                        default:
                            throw new NotImplementedException($"Missing variable parse for type: {type}");
                    }
                }
                else
                {
                    return float.Parse(text, CultureInfo.InvariantCulture);
                }
            }

            private static object ReadVariableValueOrDefault(PARAMDEF def, XmlNode node, DefType type, string xpath, object defaultValue)
            {
                if (def.VariableEditorValueTypes)
                {
                    switch (type)
                    {
                        case DefType.s8:
                        case DefType.u8:
                        case DefType.s16:
                        case DefType.u16:
                        case DefType.s32:
                        case DefType.u32:
                        case DefType.b32:
                            return node.ReadInt32OrDefault(xpath, (int)defaultValue);

                        case DefType.f32:
                        case DefType.angle32:
                            return node.ReadSingleOrDefault(xpath, (float)defaultValue, CultureInfo.InvariantCulture);

                        case DefType.f64:
                            return node.ReadDoubleOrDefault(xpath, (double)defaultValue, CultureInfo.InvariantCulture);

                        case DefType.dummy8:
                        case DefType.fixstr:
                        case DefType.fixstrW:
                            return null;

                        default:
                            throw new NotImplementedException($"Missing variable read for type: {type}");
                    }
                }
                else
                {
                    return node.ReadSingleOrDefault(xpath, (float)defaultValue, CultureInfo.InvariantCulture);
                }
            }

            private static void SerializeField(PARAMDEF def, Field field, XmlWriter xw)
            {
                string fieldDef = $"{field.DisplayType} {field.InternalName}";
                if (ParamUtil.IsBitType(field.DisplayType) && field.BitSize != -1)
                    fieldDef += $":{field.BitSize}";
                else if (ParamUtil.IsArrayType(field.DisplayType))
                    fieldDef += $"[{field.ArrayLength}]";

                if (!Equals(field.Default, ParamUtil.GetDefaultDefault(def, field.DisplayType)))
                    fieldDef += $" = {VariableValueToString(def, field.DisplayType, field.Default)}";

                xw.WriteAttributeString("Def", fieldDef);
                if (def.VersionAware && field.FirstRegulationVersion != 0)
                    xw.WriteAttributeString("FirstVersion", $"{field.FirstRegulationVersion}");
                if (def.VersionAware && field.RemovedRegulationVersion != 0)
                    xw.WriteAttributeString("RemovedVersion", $"{field.RemovedRegulationVersion}");
                xw.WriteDefaultElement("DisplayName", field.DisplayName, field.InternalName);
                xw.WriteDefaultElement("Enum", field.InternalType, field.DisplayType.ToString());
                xw.WriteDefaultElement("Description", field.Description, null);
                xw.WriteDefaultElement("DisplayFormat", field.DisplayFormat, ParamUtil.GetDefaultFormat(field.DisplayType));
                xw.WriteDefaultElement("EditFlags", field.EditFlags.ToString(), ParamUtil.GetDefaultEditFlags(field.DisplayType).ToString());
                WriteVariableValue(def, xw, field.DisplayType, "Minimum", field.Minimum, ParamUtil.GetDefaultMinimum(def, field.DisplayType));
                WriteVariableValue(def, xw, field.DisplayType, "Maximum", field.Maximum, ParamUtil.GetDefaultMaximum(def, field.DisplayType));
                WriteVariableValue(def, xw, field.DisplayType, "Increment", field.Increment, ParamUtil.GetDefaultIncrement(def, field.DisplayType));
                xw.WriteDefaultElement("SortID", field.SortID, 0);

                xw.WriteDefaultElement("UnkB8", field.UnkB8, null);
                xw.WriteDefaultElement("UnkC0", field.UnkC0, null);
                xw.WriteDefaultElement("UnkC8", field.UnkC8, null);
            }

            private static string VariableValueToString(PARAMDEF def, DefType type, object value)
            {
                if (def.VariableEditorValueTypes)
                {
                    switch (type)
                    {
                        case DefType.s8:
                        case DefType.u8:
                        case DefType.s16:
                        case DefType.u16:
                        case DefType.s32:
                        case DefType.u32:
                        case DefType.b32:
                            return Convert.ToInt32(value).ToString();

                        case DefType.f32:
                        case DefType.angle32:
                            return Convert.ToSingle(value).ToString();

                        case DefType.f64:
                            return Convert.ToDouble(value).ToString();

                        case DefType.dummy8:
                        case DefType.fixstr:
                        case DefType.fixstrW:
                            return "null";

                        default:
                            throw new NotImplementedException($"Missing variable tostring for type: {type}");
                    }
                }
                else
                {
                    return Convert.ToSingle(value).ToString("R", CultureInfo.InvariantCulture);
                }
            }

            private static void WriteVariableValue(PARAMDEF def, XmlWriter xw, DefType type, string localName, object value, object defaultValue)
            {
                if (def.VariableEditorValueTypes)
                {
                    switch (type)
                    {
                        case DefType.s8:
                        case DefType.u8:
                        case DefType.s16:
                        case DefType.u16:
                        case DefType.s32:
                        case DefType.u32:
                        case DefType.b32:
                            xw.WriteDefaultElement(localName, Convert.ToInt32(value), (int)defaultValue);
                            break;

                        case DefType.f32:
                        case DefType.angle32:
                            xw.WriteDefaultElement(localName, Convert.ToSingle(value), (float)defaultValue);
                            break;

                        case DefType.f64:
                            xw.WriteDefaultElement(localName, Convert.ToDouble(value), (double)defaultValue);
                            break;

                        case DefType.dummy8:
                        case DefType.fixstr:
                        case DefType.fixstrW:
                            break;

                        default:
                            break;
                    }
                }
                else
                {
                    xw.WriteDefaultElement(localName, Convert.ToSingle(value), Convert.ToSingle(defaultValue), "R", CultureInfo.InvariantCulture);
                }
            }
        }
    }
}
