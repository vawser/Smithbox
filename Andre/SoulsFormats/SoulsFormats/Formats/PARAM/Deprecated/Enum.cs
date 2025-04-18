using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace SoulsFormats
{
    public partial class PARAM
    {
        /// <summary>
        /// A collection of named values.
        /// </summary>
        [Obsolete]
        public class Enum : List<Enum.Item>
        {
            /// <summary>
            /// The type of values in this enum, which should match the types of the cells using it.
            /// </summary>
            public CellType Type { get; }

            /// <summary>
            /// Creates a new empty Enum with the given type.
            /// </summary>
            public Enum(CellType type) : base()
            {
                Type = type;
            }

            internal Enum(XmlNode node)
            {
                Type = (CellType)System.Enum.Parse(typeof(CellType), node.Attributes["type"].InnerText);
                foreach (XmlNode itemNode in node.SelectNodes("item"))
                {
                    string itemName = itemNode.Attributes["name"].InnerText;
                    object itemValue = Layout.ParseParamValue(Type, itemNode.Attributes["value"].InnerText);
                    Add(new Item(itemName, itemValue));
                }
            }

            /// <summary>
            /// Converts the enum to a paramtdf with the given name.
            /// </summary>
            public PARAMTDF ToParamtdf(string name)
            {
                PARAMDEF.DefType tdfType;
                switch (Type)
                {
                    case CellType.u8:
                    case CellType.x8: tdfType = PARAMDEF.DefType.u8; break;
                    case CellType.s8: tdfType = PARAMDEF.DefType.s8; break;
                    case CellType.u16:
                    case CellType.x16: tdfType = PARAMDEF.DefType.u16; break;
                    case CellType.s16: tdfType = PARAMDEF.DefType.s16; break;
                    case CellType.u32:
                    case CellType.x32: tdfType = PARAMDEF.DefType.u32; break;
                    case CellType.s32: tdfType = PARAMDEF.DefType.s32; break;

                    default:
                        throw new InvalidDataException($"Layout.Enum type {Type} may not be used in a TDF.");
                }

                var tdf = new PARAMTDF { Name = name, Type = tdfType };
                foreach (Item item in this)
                {
                    tdf.Entries.Add(new PARAMTDF.Entry(item.Name, item.Value));
                }
                return tdf;
            }

            /// <summary>
            /// A value and corresponding name in an Enum.
            /// </summary>
            public class Item
            {
                /// <summary>
                /// The name of the value.
                /// </summary>
                public string Name { get; }

                /// <summary>
                /// The value of the value.
                /// </summary>
                public object Value { get; }

                /// <summary>
                /// Creates a new Item with the given properties.
                /// </summary>
                public Item(string name, object value)
                {
                    Name = name;
                    Value = value;
                }
            }
        }
    }
}
