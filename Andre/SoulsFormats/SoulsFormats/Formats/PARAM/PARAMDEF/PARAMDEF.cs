using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace SoulsFormats
{
    /// <summary>
    /// A companion format to params that describes each field present in the rows. Extension: .def, .paramdef
    /// </summary>
    public partial class PARAMDEF : SoulsFile<PARAMDEF>
    {
        /// <summary>
        /// Indicates a revision of the row data structure.
        /// </summary>
        public short DataVersion { get; set; }

        /// <summary>
        /// Identifies corresponding params and paramdefs.
        /// </summary>
        public string ParamType { get; set; }

        /// <summary>
        /// True for PS3 and X360 games, otherwise false.
        /// </summary>
        public bool BigEndian { get; set; }

        /// <summary>
        /// If true, certain strings are written as UTF-16; if false, as Shift-JIS.
        /// </summary>
        public bool Unicode { get; set; }

        /// <summary>
        /// Determines format of the file.
        /// </summary>
        // 101 - Enchanted Arms, Chromehounds, Armored Core 4/For Answer/V/Verdict Day, Shadow Assault: Tenchu
        // 102 - Demon's Souls
        // 103 - Ninja Blade, Another Century's Episode: R
        // 104 - Dark Souls, Steel Battalion: Heavy Armor
        // 106 - Elden Ring (deprecated ObjectParam)
        // 201 - Bloodborne
        // 202 - Dark Souls 3
        // 203 - Elden Ring
        public short FormatVersion { get; set; }

        /// <summary>
        /// Whether field default, minimum, maximum, and increment may be variable type. If false, they are always floats.
        /// </summary>
        public bool VariableEditorValueTypes => FormatVersion >= 203;

        /// <summary>
        /// Fields in each param row, in order of appearance.
        /// </summary>
        public List<Field> Fields { get; set; }

        /// <summary>
        /// PARAMDEF is "regulation version aware" and can be applied to older regulation params that may have a
        /// different layout that the latest params if the XML paramdef supports it.
        /// </summary>
        public bool VersionAware { get; set; } = false;

        /// <summary>
        /// Creates a PARAMDEF formatted for DS1.
        /// </summary>
        public PARAMDEF()
        {
            ParamType = "";
            FormatVersion = 104;
            Fields = new List<Field>();
        }

        /// <summary>
        /// Deserializes file data from a stream.
        /// </summary>
        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = BigEndian = br.GetSByte(0x2C) == -1;
            FormatVersion = br.GetInt16(0x2E);
            br.VarintLong = FormatVersion >= 200;

            br.ReadInt32(); // File size
            short headerSize = br.AssertInt16([0x30, 0xFF]);
            DataVersion = br.ReadInt16();
            short fieldCount = br.ReadInt16();
            short fieldSize = br.AssertInt16([0x48, 0x68, 0x6C, 0x88, 0x8C, 0xAC, 0xB0, 0xD0]);

            if (FormatVersion >= 202)
            {
                br.AssertInt32(0);
                // Is there a reason that I used GetShiftJIS instead of GetASCII here?
                ParamType = br.GetShiftJIS(br.ReadInt64());
                br.AssertInt64(0);
                br.AssertInt64(0);
                br.AssertInt32(0);
            }
            else if (FormatVersion >= 106 && FormatVersion < 200)
            {
                ParamType = br.GetShiftJIS(br.ReadInt32());
                br.AssertInt64(0);
                br.AssertInt64(0);
                br.AssertInt64(0);
                br.AssertInt32(0);
            }
            else
            {
                ParamType = br.ReadFixStr(0x20);
            }

            br.AssertSByte([0, -1]); // Big-endian
            Unicode = br.ReadBoolean();
            br.AssertInt16([101, 102, 103, 104, 106, 201, 202, 203]); // Format version
            if (FormatVersion >= 200)
                br.AssertInt64(0x38);

            if (!(FormatVersion < 200 && headerSize == 0x30 || FormatVersion >= 200 && headerSize == 0xFF))
                throw new InvalidDataException($"Unexpected header size 0x{headerSize:X} for version {FormatVersion}.");

            // Please note that for version 103 this value is wrong.
            if (!(FormatVersion == 101 && fieldSize == 0x8C 
                || FormatVersion == 102 && fieldSize == 0xAC 
                || FormatVersion == 103 && fieldSize == 0x6C
                || FormatVersion == 104 && fieldSize == 0xB0
                || FormatVersion == 106 && fieldSize == 0x48
                || FormatVersion == 201 && fieldSize == 0xD0 
                || FormatVersion == 202 && fieldSize == 0x68
                || FormatVersion == 203 && fieldSize == 0x88))
                throw new InvalidDataException($"Unexpected field size 0x{fieldSize:X} for version {FormatVersion}.");

            Fields = new List<Field>(fieldCount);
            for (int i = 0; i < fieldCount; i++)
                Fields.Add(new Field(br, this));
        }

        /// <summary>
        /// Verifies that the file can be written safely.
        /// </summary>
        public override bool Validate(out Exception ex)
        {
            if (!(FormatVersion == 101 || FormatVersion == 102 || FormatVersion == 103 || FormatVersion == 104 || FormatVersion == 106
                || FormatVersion == 201 || FormatVersion == 202 || FormatVersion == 203))
            {
                ex = new InvalidDataException($"Unknown version: {FormatVersion}");
                return false;
            }

            if (!ValidateNull(ParamType, $"{nameof(ParamType)} may not be null.", out ex)
                || !ValidateNull(Fields, $"{nameof(Fields)} may not be null.", out ex))
                return false;

            for (int i = 0; i < Fields.Count; i++)
            {
                Field field = Fields[i];
                string which = $"{nameof(Fields)}[{i}]";
                if (!ValidateNull(field, $"{which}: {nameof(Field)} may not be null.", out ex)
                    || !ValidateNull(field.DisplayName, $"{which}: {nameof(Field.DisplayName)} may not be null.", out ex)
                    || !ValidateNull(field.DisplayFormat, $"{which}: {nameof(Field.DisplayFormat)} may not be null.", out ex)
                    || !ValidateNull(field.InternalType, $"{which}: {nameof(Field.InternalType)} may not be null.", out ex)
                    || FormatVersion >= 102 && !ValidateNull(field.InternalName, $"{which}: {nameof(Field.InternalName)} may not be null on version {FormatVersion}.", out ex))
                    return false;
            }

            ex = null;
            return true;
        }

        /// <summary>
        /// Serializes file data to a stream.
        /// </summary>
        protected override void Write(BinaryWriterEx bw)
        {
            if (VersionAware)
                throw new Exception("Version aware PARAMDEFs cannot be written as binary.");
            
            bw.BigEndian = BigEndian;
            bw.VarintLong = FormatVersion >= 200;

            bw.ReserveInt32("FileSize");
            bw.WriteInt16((short)(FormatVersion >= 200 ? 0xFF : 0x30));
            bw.WriteInt16(DataVersion);
            bw.WriteInt16((short)Fields.Count);

            if (FormatVersion == 101)
                bw.WriteInt16(0x8C);
            else if (FormatVersion == 102)
                bw.WriteInt16(0xAC);
            else if (FormatVersion == 103)
                bw.WriteInt16(0x6C);
            else if (FormatVersion == 104)
                bw.WriteInt16(0xB0);
            else if (FormatVersion == 106)
                bw.WriteInt16(0x48);
            else if (FormatVersion == 201)
                bw.WriteInt16(0xD0);
            else if (FormatVersion == 202)
                bw.WriteInt16(0x68);
            else if (FormatVersion == 203)
                bw.WriteInt16(0x88);

            if (FormatVersion >= 202)
            {
                bw.WriteInt32(0);
                bw.ReserveVarint("ParamTypeOffset");
                bw.WriteInt64(0);
                bw.WriteInt64(0);
                bw.WriteInt32(0);
            }
            else if (FormatVersion >= 106 && FormatVersion < 200)
            {
                bw.ReserveVarint("ParamTypeOffset");
                bw.WriteInt64(0);
                bw.WriteInt64(0);
                bw.WriteInt64(0);
                bw.WriteInt32(0);
            }
            else
            {
                bw.WriteFixStr(ParamType, 0x20, (byte)(FormatVersion >= 200 ? 0x00 : 0x20));
            }

            bw.WriteSByte((sbyte)(BigEndian ? -1 : 0));
            bw.WriteBoolean(Unicode);
            bw.WriteInt16(FormatVersion);
            if (FormatVersion >= 200)
                bw.WriteInt64(0x38);

            for (int i = 0; i < Fields.Count; i++)
                Fields[i].Write(bw, this, i);

            if (FormatVersion >= 202 || FormatVersion >= 106 && FormatVersion < 200)
            {
                bw.FillVarint("ParamTypeOffset", bw.Position);
                bw.WriteShiftJIS(ParamType, true);
            }

            long fieldStringsStart = bw.Position;
            var sharedStringOffsets = new Dictionary<string, long>();
            for (int i = 0; i < Fields.Count; i++)
                Fields[i].WriteStrings(bw, this, i, sharedStringOffsets);

            // This entire heuristic seems extremely dubious
            if (FormatVersion == 104 || FormatVersion == 201)
            {
                long fieldStringsLength = bw.Position - fieldStringsStart;
                if (fieldStringsLength % 0x10 != 0)
                    bw.WritePattern((int)(0x10 - fieldStringsLength % 0x10), 0x00);
            }
            else
            {
                if (FormatVersion >= 202 && bw.Position % 0x10 == 0)
                    bw.WritePattern(0x10, 0x00);
                bw.Pad(0x10);
            }
            bw.FillInt32("FileSize", (int)bw.Position);
        }

        /// <summary>
        /// Calculates the size of cell data for each row.
        /// </summary>
        public int GetRowSize(ulong version = ulong.MaxValue)
        {
            return GetFieldsSize(Fields.Count);
        }

        internal int GetFieldsSize(int fieldCount)
        {
            if (fieldCount < 0 || fieldCount > Fields.Count)
                throw new ArgumentException("Count must be from 0 to total fields count.", nameof(fieldCount));
            int size = 0;
            for (int i = 0; i < fieldCount; i++)
            {
                Field field = Fields[i];
                DefType type = field.DisplayType;
                if (ParamUtil.IsArrayType(type))
                    size += ParamUtil.GetValueSize(type) * field.ArrayLength;
                else
                    size += ParamUtil.GetValueSize(type);

                if (ParamUtil.IsBitType(type) && field.BitSize != -1)
                {
                    int bitOffset = field.BitSize;
                    int bitLimit = ParamUtil.GetBitLimit(type);

                    for (; i < fieldCount - 1; i++)
                    {
                        Field nextField = Fields[i + 1];
                        DefType nextType = nextField.DisplayType;
                        if (!ParamUtil.IsBitType(nextType) || nextField.BitSize == -1 || ParamUtil.GetBitLimit(nextType) != bitLimit
                            || bitOffset + nextField.BitSize > bitLimit)
                            break;
                        bitOffset += nextField.BitSize;
                    }
                }
            }
            return size;
        }

        /// <summary>
        /// If this PARAMDEF is version aware, returns a filtered PARAMDEF with only the fields that are valid for a
        /// specific regulation version. This is useful for binary or XML serialization for a specific regulation version.
        /// Note that the underlying fields are NOT cloned for the new PARAMDEF.
        /// </summary>
        /// <param name="version">The version to filter the fields for</param>
        /// <returns>A new PARAMDEF with only the filtered fields</returns>
        public PARAMDEF GetFilteredParamdefForRegulationVersion(ulong version)
        {
            if (!VersionAware)
                throw new Exception("Version aware PARAMDEF required for filtering");
            var ret = new PARAMDEF
            {
                DataVersion = DataVersion,
                ParamType = ParamType,
                BigEndian = BigEndian,
                Unicode = Unicode,
                FormatVersion = FormatVersion,
                Fields = new List<Field>()
            };
            foreach (var field in Fields)
            {
                if (field.IsValidForRegulationVersion(version))
                    ret.Fields.Add(field);
            }

            return ret;
        }

        /// <summary>
        /// Returns a string representation of the PARAMDEF.
        /// </summary>
        public override string ToString()
        {
            return $"{ParamType} v{DataVersion}";
        }

        /// <summary>
        /// Reads an XML-formatted PARAMDEF from a file.
        /// </summary>
        /// <param name="path">The path to read the PARAMDEF XML file from</param>
        /// <param name="versionAware">If versionAware is enabled and the PARAMDEFs support it, additional data will be
        /// read such that the PARAMDEFs can be used on older regulations if the regulation version is known. Otherwise,
        /// the PARAMDEFs will be read to support the latest supported regulation version only.</param>
        /// <returns></returns>
        public static PARAMDEF XmlDeserialize(string path, bool versionAware = false)
        {
            var xml = new XmlDocument();
            xml.Load(path);
            return XmlSerializer.Deserialize(xml, versionAware);
        }

        /// <summary>
        /// Writes an XML-formatted PARAMDEF to a file using the current XML version.
        /// </summary>
        public void XmlSerialize(string path, bool includeOffsets = false)
        {
            XmlSerialize(path, XmlSerializer.CURRENT_XML_VERSION, includeOffsets);
        }

        /// <summary>
        /// Writes an XML-formatted PARAMDEF to a file using the given XML version.
        /// </summary>
        public void XmlSerialize(string path, int xmlVersion, bool includeOffsets = true)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            var xws = new XmlWriterSettings { Indent = true };
            using (var xw = XmlWriter.Create(path, xws))
                XmlSerializer.Serialize(this, xw, xmlVersion, includeOffsets);
        }
    }
}
