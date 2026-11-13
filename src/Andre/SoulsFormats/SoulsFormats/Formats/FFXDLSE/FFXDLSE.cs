using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace SoulsFormats
{
    /// <summary>
    /// An SFX configuration format used in DeS and DS2. Extension: .ffx
    /// DS2 files start with "DLsE" (stream v1.3) and give every object a length;
    /// DeS files start with "DLSE" (stream v2.1), have no object lengths and use older class versions.
    /// Each object keeps the class version it was read with, so files are written back as they were.
    /// </summary>
    public partial class FFXDLSE : SoulsFile<FFXDLSE>
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public enum StreamFormat
        {
            /// <summary>DS2 / DS2S: "DLsE" 1.3, objects carry a length.</summary>
            DS2,
            /// <summary>DeS: "DLSE" 2.1, no object lengths, older class versions.</summary>
            DES,
        }

        public StreamFormat Format { get; set; } = StreamFormat.DS2;

        public FXEffect Effect { get; set; }

        public FFXDLSE()
        {
            Effect = new FXEffect();
        }

        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 4)
                return false;

            string magic = br.GetASCII(0, 4);
            return magic == "DLsE" || magic == "DLSE";
        }

        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            string magic = br.AssertASCII(["DLsE", "DLSE"]);
            Format = magic == "DLSE" ? StreamFormat.DES : StreamFormat.DS2;
            if (Format == StreamFormat.DS2)
            {
                br.AssertByte(1);
                br.AssertByte(3);
                br.AssertByte(0);
                br.AssertByte(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertByte(0);
                br.AssertInt32(1);
            }
            else
            {
                br.AssertInt16(2);
                br.AssertInt16(1);
            }
            short classNameCount = br.ReadInt16();

            var classNames = new ClassTable(Format);
            for (int i = 0; i < classNameCount; i++)
            {
                int length = br.ReadInt32();
                classNames.Add(br.ReadASCII(length));
            }

            Effect = new FXEffect(br, classNames);
        }

        protected override void Write(BinaryWriterEx bw)
        {
            var classNames = new ClassTable(Format);
            Effect.AddClassNames(classNames);

            bw.BigEndian = false;
            if (Format == StreamFormat.DS2)
            {
                bw.WriteASCII("DLsE");
                bw.WriteByte(1);
                bw.WriteByte(3);
                bw.WriteByte(0);
                bw.WriteByte(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteByte(0);
                bw.WriteInt32(1);
            }
            else
            {
                bw.WriteASCII("DLSE");
                bw.WriteInt16(2);
                bw.WriteInt16(1);
            }
            bw.WriteInt16((short)classNames.Count);

            foreach (string className in classNames)
            {
                bw.WriteInt32(className.Length);
                bw.WriteASCII(className);
            }

            Effect.Write(bw, classNames);
        }

        // Disabled to get rid of trim warnings since this isn't used in DSMS
        /*
        #region XML Serialization
        private static XmlSerializer _ffxSerializer;
        private static XmlSerializer _stateSerializer;
        private static XmlSerializer _paramSerializer;

        private static XmlSerializer MakeSerializers(int returnIndex)
        {
            XmlSerializer[] serializers = XmlSerializer.FromTypes(
                new Type[] { typeof(FFXDLSE), typeof(State), typeof(Param) });

            _ffxSerializer = serializers[0];
            _stateSerializer = serializers[1];
            _paramSerializer = serializers[2];
            return serializers[returnIndex];
        }

        private static XmlSerializer FFXSerializer => _ffxSerializer ?? MakeSerializers(0);
        private static XmlSerializer StateSerializer => _stateSerializer ?? MakeSerializers(1);
        private static XmlSerializer ParamSerializer => _paramSerializer ?? MakeSerializers(2);

        public static FFXDLSE XmlDeserialize(Stream stream)
            => (FFXDLSE)FFXSerializer.Deserialize(stream);

        public static FFXDLSE XmlDeserialize(TextReader textReader)
            => (FFXDLSE)FFXSerializer.Deserialize(textReader);

        public static FFXDLSE XmlDeserialize(XmlReader xmlReader)
            => (FFXDLSE)FFXSerializer.Deserialize(xmlReader);

        public void XmlSerialize(Stream stream)
            => FFXSerializer.Serialize(stream, this);

        public void XmlSerialize(TextWriter textWriter)
            => FFXSerializer.Serialize(textWriter, this);

        public void XmlSerialize(XmlWriter xmlWriter)
            => FFXSerializer.Serialize(xmlWriter, this);
        #endregion*/

        private static class DLVector
        {
            public static List<int> Read(BinaryReaderEx br, ClassTable classNames)
            {
                br.AssertInt16((short)(classNames.IndexOf("DLVector") + 1));
                int count = br.ReadInt32();
                return new List<int>(br.ReadInt32s(count));
            }

            public static void AddClassNames(ClassTable classNames)
            {
                if (!classNames.Contains("DLVector"))
                    classNames.Add("DLVector");
            }

            public static void Write(BinaryWriterEx bw, ClassTable classNames, List<int> vector)
            {
                bw.WriteInt16((short)(classNames.IndexOf("DLVector") + 1));
                bw.WriteInt32(vector.Count);
                bw.WriteInt32s(vector);
            }
        }

        /// <summary>
        /// The file's class name table, plus how objects are framed in this stream format.
        /// </summary>
        public sealed class ClassTable : List<string>
        {
            public StreamFormat Format { get; }

            /// <summary>DS2 objects carry an i32 length after the version; DeS objects don't.</summary>
            public bool HasLengths => Format == StreamFormat.DS2;

            /// <summary>Bytes from an object's start to its body (class index, version, length if any).</summary>
            public int HeaderSize => HasLengths ? 10 : 6;

            public ClassTable(StreamFormat format) { Format = format; }
        }

        public abstract class FXSerializable
        {
            internal abstract string ClassName { get; }

            /// <summary>The class version DS2 uses; new objects are written with it.</summary>
            internal abstract int Version { get; }

            /// <summary>Class versions DeS files use, besides <see cref="Version"/>.</summary>
            internal virtual int[] DesVersions => [1];

            private int? fileVersion;

            /// <summary>The version this object was read with, or <see cref="Version"/> for a new object.</summary>
            [System.Xml.Serialization.XmlIgnore]
            public int ClassVersion
            {
                get => fileVersion ?? Version;
                set => fileVersion = value;
            }

            internal FXSerializable() { }

            internal FXSerializable(BinaryReaderEx br, ClassTable classNames)
            {
                long start = br.Position;
                br.AssertInt16((short)(classNames.IndexOf(ClassName) + 1));
                if (classNames.HasLengths)
                {
                    ClassVersion = br.AssertInt32(Version);
                    int length = br.ReadInt32();
                    Deserialize(br, classNames);
                    if (br.Position != start + length)
                        throw new InvalidDataException("Failed to read all object data (or read too much of it).");
                }
                else
                {
                    int version = br.ReadInt32();
                    if (version != Version && Array.IndexOf(DesVersions, version) < 0)
                        throw new InvalidDataException($"{ClassName}: unexpected DeS class version {version} at 0x{start:X}.");
                    ClassVersion = version;
                    Deserialize(br, classNames);
                }
            }

            protected internal abstract void Deserialize(BinaryReaderEx br, ClassTable classNames);

            internal virtual void AddClassNames(ClassTable classNames)
            {
                if (!classNames.Contains(ClassName))
                    classNames.Add(ClassName);
            }

            internal void Write(BinaryWriterEx bw, ClassTable classNames)
            {
                long start = bw.Position;
                bw.WriteInt16((short)(classNames.IndexOf(ClassName) + 1));
                // A new object in a DeS file takes the newest DeS version of its class, so the body below matches it
                if (fileVersion == null && !classNames.HasLengths)
                    fileVersion = DesVersions[^1];
                bw.WriteInt32(ClassVersion);
                if (classNames.HasLengths)
                {
                    bw.ReserveInt32($"{start:X}Length");
                    Serialize(bw, classNames);
                    bw.FillInt32($"{start:X}Length", (int)(bw.Position - start));
                }
                else
                {
                    Serialize(bw, classNames);
                }
            }

            protected internal abstract void Serialize(BinaryWriterEx bw, ClassTable classNames);
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
