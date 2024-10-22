using System.Collections.Generic;
using System.IO;

namespace SoulsFormats.Other.PlayStation3
{
    public class PARAMSFO : SoulsFile<PARAMSFO>
    {
        public Dictionary<string, Parameter> Parameters { get; set; }
        public FormatVersion Version { get; set; }

        protected override bool Is(BinaryReaderEx br)
        {
            br.BigEndian = true;
            if (br.Length < 20)
            {
                return false;
            }
            string magic = br.ReadASCII(4);
            return magic == "\0PSF";
        }

        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertASCII("\0PSF");
            Version = new FormatVersion(br);
            uint keyTableStart = br.ReadUInt32();
            uint dataTableStart = br.ReadUInt32();
            uint tableEntryCount = br.ReadUInt32();

            Parameters = new Dictionary<string, Parameter>((int)tableEntryCount);
            for (int i = 0; i < tableEntryCount; i++)
            {
                _ = new Parameter(br, Parameters, keyTableStart, dataTableStart);
            }
        }

        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = true;
            bw.WriteASCII("\0PSF", false);
            Version.Write(bw);
            bw.ReserveUInt32("KeyTableStart");
            bw.ReserveUInt32("DataTableStart");

            uint count = (uint)Parameters.Count;
            bw.WriteUInt32(count);

            List<string> keys = new List<string>(Parameters.Keys);
            List<Parameter> parameters = new List<Parameter>(Parameters.Values);

            for (int i = 0; i < count; i++)
            {
                parameters[i].WriteEntry(bw, i);
            }

            long keyTableStart = bw.Position;
            bw.FillUInt32("KeyTableStart", (uint)keyTableStart);
            for (int i = 0; i < count; i++)
            {
                bw.FillUInt16($"KeyOffset_{i}", (ushort)(bw.Position - keyTableStart));
                bw.WriteASCII(keys[i], true);
            }
            bw.Pad(4);

            long dataTableStart = bw.Position;
            bw.FillUInt32("DataTableStart", (uint)dataTableStart);
            for (int i = 0; i < count; i++)
            {
                bw.FillUInt32($"DataOffset_{i}", (uint)(bw.Position - dataTableStart));
                Parameter parameter = parameters[i];
                DataFormat format = parameter.Format;
                switch (format)
                {
                    case DataFormat.UTF8S:
                        bw.WriteASCII(parameter.Data, false);
                        bw.WritePattern((int)(parameter.DataMaxLength - parameter.Data.Length), 0);
                        break;
                    case DataFormat.UTF8:
                        bw.WriteASCII(parameter.Data, true);
                        bw.WritePattern((int)(parameter.DataMaxLength - (parameter.Data.Length + 1)), 0);
                        break;
                    case DataFormat.UInt32:
                        bw.WriteUInt32(uint.Parse(parameter.Data));
                        break;
                    default:
                        throw new InvalidDataException($"{nameof(DataFormat)} {format} is not supported or implemented.");
                }
            }
        }

        public class Parameter
        {
            public string Data { get; set; }
            public DataFormat Format { get; set; }
            public uint DataMaxLength { get; set; }

            public Parameter(string value)
            {
                Data = value;
                Format = DataFormat.UTF8;
            }

            public Parameter(string value, DataFormat dataFormat)
            {
                Data = value;
                Format = dataFormat;
                DataMaxLength = (uint)value.Length;
            }

            public Parameter(string value, DataFormat dataFormat, uint dataMaxLength)
            {
                Data = value;
                Format = dataFormat;
                DataMaxLength = dataMaxLength;
            }

            internal Parameter(BinaryReaderEx br, Dictionary<string, Parameter> dictionary, uint keyTableStart, uint dataTableStart)
            {
                ushort keyOffset = br.ReadUInt16();
                Format = br.ReadEnum16<DataFormat>();
                uint dataLength = br.ReadUInt32();
                DataMaxLength = br.ReadUInt32();
                uint dataOffset = br.ReadUInt32();

                long end = br.Position;
                br.Position = keyTableStart + keyOffset;
                string key = br.ReadASCII();

                br.Position = dataTableStart + dataOffset;
                switch (Format)
                {
                    case DataFormat.UTF8S:
                        Data = br.ReadASCII((int)dataLength);
                        break;
                    case DataFormat.UTF8:
                        Data = br.ReadASCII();
                        break;
                    case DataFormat.UInt32:
                        Data = br.ReadUInt32().ToString();
                        break;
                    default:
                        throw new InvalidDataException($"{nameof(DataFormat)} {Format} is not supported or implemented.");
                }

                dictionary.Add(key, this);
                br.Position = end;
            }

            internal void WriteEntry(BinaryWriterEx bw, int index)
            {
                bw.ReserveUInt16($"KeyOffset_{index}");
                bw.WriteUInt16((ushort)Format);
                
                if (Format == DataFormat.UInt32)
                {
                    bw.WriteUInt32(4);
                    bw.WriteUInt32(4);
                }
                else
                {
                    bw.WriteUInt32((uint)Data.Length);
                    bw.WriteUInt32(DataMaxLength);
                }

                bw.ReserveUInt32($"DataOffset_{index}");
            }
        }

        public class FormatVersion
        {
            public byte Major { get; set; }
            public byte Minor { get; set; }
            public byte Unk03 { get; set; }
            public byte Unk04 { get; set; }

            public FormatVersion()
            {
                Major = 1;
                Minor = 1;
                Unk03 = 0;
                Unk04 = 0;
            }

            public FormatVersion(byte major, byte minor)
            {
                Major = major;
                Minor = minor;
                Unk03 = 0;
                Unk04 = 0;
            }

            public FormatVersion(byte major, byte minor, byte unk03, byte unk04)
            {
                Major = major;
                Minor = minor;
                Unk03 = unk03;
                Unk04 = unk04;
            }

            internal FormatVersion(BinaryReaderEx br)
            {
                Major = br.ReadByte();
                Minor = br.ReadByte();
                Unk03 = br.ReadByte();
                Unk04 = br.ReadByte();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteByte(Major);
                bw.WriteByte(Minor);
                bw.WriteByte(Unk03);
                bw.WriteByte(Unk04);
            }
        }

        public enum DataFormat : ushort
        {
            /// <summary>
            /// UTF8 without null termination.
            /// </summary>
            UTF8S = 0x0004, 

            /// <summary>
            /// UTF8.
            /// </summary>
            UTF8 = 0x0204,

            /// <summary>
            /// UInt32.
            /// </summary>
            UInt32 = 0x0404
        }
    }
}
