using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace SoulsFormats
{
    /// <summary>
    /// An SFX configuration format used in DeS and DS2; only DS2 is supported. Extension: .ffx
    /// </summary>
    public partial class FFXDLSE : SoulsFile<FFXDLSE>
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
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
            return magic == "DLsE";
        }

        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;
            br.AssertASCII("DLsE");
            br.AssertByte(1);
            br.AssertByte(3);
            br.AssertByte(0);
            br.AssertByte(0);
            br.AssertInt32(0);
            br.AssertInt32(0);
            br.AssertByte(0);
            br.AssertInt32(1);
            short classNameCount = br.ReadInt16();

            var classNames = new List<string>(classNameCount);
            for (int i = 0; i < classNameCount; i++)
            {
                int length = br.ReadInt32();
                classNames.Add(br.ReadASCII(length));
            }

            Effect = new FXEffect(br, classNames);
        }

        protected override void Write(BinaryWriterEx bw)
        {
            var classNames = new List<string>();
            Effect.AddClassNames(classNames);

            bw.BigEndian = false;
            bw.WriteASCII("DLsE");
            bw.WriteByte(1);
            bw.WriteByte(3);
            bw.WriteByte(0);
            bw.WriteByte(0);
            bw.WriteInt32(0);
            bw.WriteInt32(0);
            bw.WriteByte(0);
            bw.WriteInt32(1);
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
            public static List<int> Read(BinaryReaderEx br, List<string> classNames)
            {
                br.AssertInt16((short)(classNames.IndexOf("DLVector") + 1));
                int count = br.ReadInt32();
                return new List<int>(br.ReadInt32s(count));
            }

            public static void AddClassNames(List<string> classNames)
            {
                if (!classNames.Contains("DLVector"))
                    classNames.Add("DLVector");
            }

            public static void Write(BinaryWriterEx bw, List<string> classNames, List<int> vector)
            {
                bw.WriteInt16((short)(classNames.IndexOf("DLVector") + 1));
                bw.WriteInt32(vector.Count);
                bw.WriteInt32s(vector);
            }
        }

        public abstract class FXSerializable
        {
            internal abstract string ClassName { get; }

            internal abstract int Version { get; }

            internal FXSerializable() { }

            internal FXSerializable(BinaryReaderEx br, List<string> classNames)
            {
                long start = br.Position;
                br.AssertInt16((short)(classNames.IndexOf(ClassName) + 1));
                br.AssertInt32(Version);
                int length = br.ReadInt32();
                Deserialize(br, classNames);
                if (br.Position != start + length)
                    throw new InvalidDataException("Failed to read all object data (or read too much of it).");
            }

            protected internal abstract void Deserialize(BinaryReaderEx br, List<string> classNames);

            internal virtual void AddClassNames(List<string> classNames)
            {
                if (!classNames.Contains(ClassName))
                    classNames.Add(ClassName);
            }

            internal void Write(BinaryWriterEx bw, List<string> classNames)
            {
                long start = bw.Position;
                bw.WriteInt16((short)(classNames.IndexOf(ClassName) + 1));
                bw.WriteInt32(Version);
                bw.ReserveInt32($"{start:X}Length");
                Serialize(bw, classNames);
                bw.FillInt32($"{start:X}Length", (int)(bw.Position - start));
            }

            protected internal abstract void Serialize(BinaryWriterEx bw, List<string> classNames);
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
