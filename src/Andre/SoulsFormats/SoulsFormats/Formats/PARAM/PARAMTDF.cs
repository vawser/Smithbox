using System;
using System.Collections.Generic;
using System.Text;

namespace SoulsFormats
{
    /// <summary>
    /// A companion format to PARAM and PARAMDEF that provides friendly names for enumerated value types. Extension: .tdf
    /// </summary>
    public class PARAMTDF
    {
        /// <summary>
        /// The identifier of this type.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The type of values in this TDF; must be an integral type.
        /// </summary>
        public PARAMDEF.DefType Type
        {
            get => type;
            set
            {
                if (value != PARAMDEF.DefType.s8 && value != PARAMDEF.DefType.u8
                    && value != PARAMDEF.DefType.s16 && value != PARAMDEF.DefType.u16
                    && value != PARAMDEF.DefType.s32 && value != PARAMDEF.DefType.u32)
                    throw new ArgumentException($"TDF type may only be s8, u8, s16, u16, s32, or u32, but {value} was given.");
                type = value;
            }
        }
        private PARAMDEF.DefType type;

        /// <summary>
        /// Named values in this TDF.
        /// </summary>
        public List<Entry> Entries { get; set; }

        /// <summary>
        /// Returns the value of the entry with the given name.
        /// </summary>
        public object this[string name] => Entries.Find(e => e.Name == name).Value;

        /// <summary>
        /// Returns the name of the entry with the given value.
        /// </summary>
        public string this[object value] => Entries.Find(e => e.Value == value).Name;

        /// <summary>
        /// Creates an empty TDF.
        /// </summary>
        public PARAMTDF()
        {
            Name = "UNSPECIFIED";
            Type = PARAMDEF.DefType.s32;
            Entries = new List<Entry>();
        }

        /// <summary>
        /// Reads a TDF from From's plaintext format.
        /// </summary>
        public PARAMTDF(string text)
        {
            string[] lines = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            Name = lines[0].Trim('"');
            Type = (PARAMDEF.DefType)Enum.Parse(typeof(PARAMDEF.DefType), lines[1].Trim('"'));

            Entries = new List<Entry>(lines.Length - 2);
            for (int i = 2; i < lines.Length; i++)
            {
                string[] elements = lines[i].Split(',');
                string valueStr = elements[1].Trim('"');
                object value;
                switch (Type)
                {
                    case PARAMDEF.DefType.s8: value = sbyte.Parse(valueStr); break;
                    case PARAMDEF.DefType.u8: value = byte.Parse(valueStr); break;
                    case PARAMDEF.DefType.s16: value = short.Parse(valueStr); break;
                    case PARAMDEF.DefType.u16: value = ushort.Parse(valueStr); break;
                    case PARAMDEF.DefType.s32: value = int.Parse(valueStr); break;
                    case PARAMDEF.DefType.u32: value = uint.Parse(valueStr); break;

                    default:
                        throw new NotImplementedException($"Parsing not implemented for type {Type}.");
                }

                if (elements[0] == "")
                    Entries.Add(new Entry(null, value));
                else
                    Entries.Add(new Entry(elements[0].Trim('"'), value));
            }
        }

        /// <summary>
        /// Writes the TDF in From's plaintext format.
        /// </summary>
        public string Write()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"\"{Name}\"");
            sb.AppendLine($"\"{Type}\"");
            foreach (Entry entry in Entries)
            {
                if (entry.Name != null)
                    sb.Append($"\"{entry.Name}\"");
                sb.AppendLine($",\"{entry.Value}\"");
            }
            return sb.ToString();
        }

        /// <summary>
        /// Returns a string representation of the TDF.
        /// </summary>
        public override string ToString()
        {
            return $"{Type} {Name}";
        }

        /// <summary>
        /// A named enumerator.
        /// </summary>
        public class Entry
        {
            /// <summary>
            /// Name given to this value.
            /// </summary>
            public string Name { get; set; }

            /// <summary>
            /// Value of this entry, of the same type as the parent TDF.
            /// </summary>
            public object Value { get; set; }

            /// <summary>
            /// Creates a new Entry with the given values.
            /// </summary>
            public Entry(string name, object value)
            {
                Name = name;
                Value = value;
            }

            /// <summary>
            /// Returns a string representation of the Entry.
            /// </summary>
            public override string ToString()
            {
                return $"{Name ?? "<null>"} = {Value}";
            }
        }
    }
}
