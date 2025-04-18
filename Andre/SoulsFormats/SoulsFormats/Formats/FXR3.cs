using System.Collections.Generic;
using System.Xml.Serialization;

namespace SoulsFormats
{
    /// <summary>
    /// An SFX definition file used in DS3 and Sekiro. Extension: .fxr
    /// </summary>
    public class FXR3 : SoulsFile<FXR3>
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        public FXRVersion Version { get; set; }

        public int ID { get; set; }

        public Section1 Section1Tree { get; set; }

        public Section4 Section4Tree { get; set; }

        public List<int> Section12s { get; set; }

        public List<int> Section13s { get; set; }

        public FXR3()
        {
            Version = FXRVersion.DarkSouls3;
            Section1Tree = new Section1();
            Section4Tree = new Section4();
            Section12s = new List<int>();
            Section13s = new List<int>();
        }

        protected override bool Is(BinaryReaderEx br)
        {
            if (br.Length < 8)
                return false;

            string magic = br.GetASCII(0, 4);
            short version = br.GetInt16(6);
            return magic == "FXR\0" && (version == 4 || version == 5);
        }

        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            br.AssertASCII("FXR\0");
            br.AssertInt16(0);
            Version = br.ReadEnum16<FXRVersion>();
            br.AssertInt32(1);
            ID = br.ReadInt32();
            int section1Offset = br.ReadInt32();
            br.AssertInt32(1); // Section 1 count
            br.ReadInt32(); // Section 2 offset
            br.ReadInt32(); // Section 2 count
            br.ReadInt32(); // Section 3 offset
            br.ReadInt32(); // Section 3 count
            int section4Offset = br.ReadInt32();
            br.ReadInt32(); // Section 4 count
            br.ReadInt32(); // Section 5 offset
            br.ReadInt32(); // Section 5 count
            br.ReadInt32(); // Section 6 offset
            br.ReadInt32(); // Section 6 count
            br.ReadInt32(); // Section 7 offset
            br.ReadInt32(); // Section 7 count
            br.ReadInt32(); // Section 8 offset
            br.ReadInt32(); // Section 8 count
            br.ReadInt32(); // Section 9 offset
            br.ReadInt32(); // Section 9 count
            br.ReadInt32(); // Section 10 offset
            br.ReadInt32(); // Section 10 count
            br.ReadInt32(); // Section 11 offset
            br.ReadInt32(); // Section 11 count
            br.AssertInt32(1);
            br.AssertInt32(0);

            if (Version == FXRVersion.Sekiro)
            {
                int section12Offset = br.ReadInt32();
                int section12Count = br.ReadInt32();
                int section13Offset = br.ReadInt32();
                int section13Count = br.ReadInt32();
                br.ReadInt32(); // Section 14 offset
                br.AssertInt32(0); // Section 14 count
                br.AssertInt32(0);
                br.AssertInt32(0);

                Section12s = new List<int>(br.GetInt32s(section12Offset, section12Count));
                Section13s = new List<int>(br.GetInt32s(section13Offset, section13Count));
            }
            else
            {
                Section12s = new List<int>();
                Section13s = new List<int>();
            }

            br.Position = section1Offset;
            Section1Tree = new Section1(br);

            br.Position = section4Offset;
            Section4Tree = new Section4(br);
        }

        protected override void Write(BinaryWriterEx bw)
        {
            bw.WriteASCII("FXR\0");
            bw.WriteInt16(0);
            bw.WriteUInt16((ushort)Version);
            bw.WriteInt32(1);
            bw.WriteInt32(ID);
            bw.ReserveInt32("Section1Offset");
            bw.WriteInt32(1);
            bw.ReserveInt32("Section2Offset");
            bw.WriteInt32(Section1Tree.Section2s.Count);
            bw.ReserveInt32("Section3Offset");
            bw.ReserveInt32("Section3Count");
            bw.ReserveInt32("Section4Offset");
            bw.ReserveInt32("Section4Count");
            bw.ReserveInt32("Section5Offset");
            bw.ReserveInt32("Section5Count");
            bw.ReserveInt32("Section6Offset");
            bw.ReserveInt32("Section6Count");
            bw.ReserveInt32("Section7Offset");
            bw.ReserveInt32("Section7Count");
            bw.ReserveInt32("Section8Offset");
            bw.ReserveInt32("Section8Count");
            bw.ReserveInt32("Section9Offset");
            bw.ReserveInt32("Section9Count");
            bw.ReserveInt32("Section10Offset");
            bw.ReserveInt32("Section10Count");
            bw.ReserveInt32("Section11Offset");
            bw.ReserveInt32("Section11Count");
            bw.WriteInt32(1);
            bw.WriteInt32(0);

            if (Version == FXRVersion.Sekiro)
            {
                bw.ReserveInt32("Section12Offset");
                bw.WriteInt32(Section12s.Count);
                bw.ReserveInt32("Section13Offset");
                bw.WriteInt32(Section13s.Count);
                bw.ReserveInt32("Section14Offset");
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
            }

            bw.FillInt32("Section1Offset", (int)bw.Position);
            Section1Tree.Write(bw);
            bw.Pad(0x10);

            bw.FillInt32("Section2Offset", (int)bw.Position);
            Section1Tree.WriteSection2s(bw);
            bw.Pad(0x10);

            bw.FillInt32("Section3Offset", (int)bw.Position);
            List<Section2> section2s = Section1Tree.Section2s;
            var section3s = new List<Section3>();
            for (int i = 0; i < section2s.Count; i++)
                section2s[i].WriteSection3s(bw, i, section3s);
            bw.FillInt32("Section3Count", section3s.Count);
            bw.Pad(0x10);

            bw.FillInt32("Section4Offset", (int)bw.Position);
            var section4s = new List<Section4>();
            Section4Tree.Write(bw, section4s);
            Section4Tree.WriteSection4s(bw, section4s);
            bw.FillInt32("Section4Count", section4s.Count);
            bw.Pad(0x10);

            bw.FillInt32("Section5Offset", (int)bw.Position);
            int section5Count = 0;
            for (int i = 0; i < section4s.Count; i++)
                section4s[i].WriteSection5s(bw, i, ref section5Count);
            bw.FillInt32("Section5Count", section5Count);
            bw.Pad(0x10);

            bw.FillInt32("Section6Offset", (int)bw.Position);
            section5Count = 0;
            var section6s = new List<FFXDrawEntityHost>();
            for (int i = 0; i < section4s.Count; i++)
                section4s[i].WriteSection6s(bw, i, ref section5Count, section6s);
            bw.FillInt32("Section6Count", section6s.Count);
            bw.Pad(0x10);

            bw.FillInt32("Section7Offset", (int)bw.Position);
            var section7s = new List<FFXProperty>();
            for (int i = 0; i < section6s.Count; i++)
                section6s[i].WriteSection7s(bw, i, section7s);
            bw.FillInt32("Section7Count", section7s.Count);
            bw.Pad(0x10);

            bw.FillInt32("Section8Offset", (int)bw.Position);
            var section8s = new List<Section8>();
            for (int i = 0; i < section7s.Count; i++)
                section7s[i].WriteSection8s(bw, i, section8s);
            bw.FillInt32("Section8Count", section8s.Count);
            bw.Pad(0x10);

            bw.FillInt32("Section9Offset", (int)bw.Position);
            var section9s = new List<Section9>();
            for (int i = 0; i < section8s.Count; i++)
                section8s[i].WriteSection9s(bw, i, section9s);
            bw.FillInt32("Section9Count", section9s.Count);
            bw.Pad(0x10);

            bw.FillInt32("Section10Offset", (int)bw.Position);
            var section10s = new List<Section10>();
            for (int i = 0; i < section6s.Count; i++)
                section6s[i].WriteSection10s(bw, i, section10s);
            bw.FillInt32("Section10Count", section10s.Count);
            bw.Pad(0x10);

            bw.FillInt32("Section11Offset", (int)bw.Position);
            int section11Count = 0;
            for (int i = 0; i < section3s.Count; i++)
                section3s[i].WriteSection11s(bw, i, ref section11Count);
            for (int i = 0; i < section6s.Count; i++)
                section6s[i].WriteSection11s(bw, i, ref section11Count);
            for (int i = 0; i < section7s.Count; i++)
                section7s[i].WriteSection11s(bw, i, ref section11Count);
            for (int i = 0; i < section8s.Count; i++)
                section8s[i].WriteSection11s(bw, i, ref section11Count);
            for (int i = 0; i < section9s.Count; i++)
                section9s[i].WriteSection11s(bw, i, ref section11Count);
            for (int i = 0; i < section10s.Count; i++)
                section10s[i].WriteSection11s(bw, i, ref section11Count);
            bw.FillInt32("Section11Count", section11Count);
            bw.Pad(0x10);

            if (Version == FXRVersion.Sekiro)
            {
                bw.FillInt32("Section12Offset", (int)bw.Position);
                bw.WriteInt32s(Section12s);
                bw.Pad(0x10);

                bw.FillInt32("Section13Offset", (int)bw.Position);
                bw.WriteInt32s(Section13s);
                bw.Pad(0x10);

                bw.FillInt32("Section14Offset", (int)bw.Position);
            }
        }

        public enum FXRVersion : ushort
        {
            DarkSouls3 = 4,
            Sekiro = 5,
        }

        public class Section1
        {
            public List<Section2> Section2s { get; set; }

            public Section1()
            {
                Section2s = new List<Section2>();
            }

            internal Section1(BinaryReaderEx br)
            {
                br.AssertInt32(0);
                int section2Count = br.ReadInt32();
                int section2Offset = br.ReadInt32();
                br.AssertInt32(0);

                br.StepIn(section2Offset);
                {
                    Section2s = new List<Section2>(section2Count);
                    for (int i = 0; i < section2Count; i++)
                        Section2s.Add(new Section2(br));
                }
                br.StepOut();
            }

            internal void Write(BinaryWriterEx bw)
            {
                bw.WriteInt32(0);
                bw.WriteInt32(Section2s.Count);
                bw.ReserveInt32("Section1Section2sOffset");
                bw.WriteInt32(0);
            }

            internal void WriteSection2s(BinaryWriterEx bw)
            {
                bw.FillInt32("Section1Section2sOffset", (int)bw.Position);
                for (int i = 0; i < Section2s.Count; i++)
                    Section2s[i].Write(bw, i);
            }
        }

        public class Section2
        {
            public List<Section3> Section3s { get; set; }

            public Section2()
            {
                Section3s = new List<Section3>();
            }

            internal Section2(BinaryReaderEx br)
            {
                br.AssertInt32(0);
                int section3Count = br.ReadInt32();
                int section3Offset = br.ReadInt32();
                br.AssertInt32(0);

                br.StepIn(section3Offset);
                {
                    Section3s = new List<Section3>(section3Count);
                    for (int i = 0; i < section3Count; i++)
                        Section3s.Add(new Section3(br));
                }
                br.StepOut();
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.WriteInt32(0);
                bw.WriteInt32(Section3s.Count);
                bw.ReserveInt32($"Section2Section3sOffset[{index}]");
                bw.WriteInt32(0);
            }

            internal void WriteSection3s(BinaryWriterEx bw, int index, List<Section3> section3s)
            {
                bw.FillInt32($"Section2Section3sOffset[{index}]", (int)bw.Position);
                foreach (Section3 section3 in Section3s)
                    section3.Write(bw, section3s);
            }
        }

        public class Section3
        {
            public int Unk08 { get; set; }

            public int Unk10 { get; set; }

            public int Unk38 { get; set; }

            public int Section11Data1 { get; set; }

            public int Section11Data2 { get; set; }

            public Section3() { }

            internal Section3(BinaryReaderEx br)
            {
                br.AssertInt16(11);
                br.AssertByte(0);
                br.AssertByte(1);
                br.AssertInt32(0);
                Unk08 = br.ReadInt32();
                br.AssertInt32(0);
                Unk10 = br.AssertInt32([0x100FFFC, 0x100FFFD]);
                br.AssertInt32(0);
                br.AssertInt32(1);
                br.AssertInt32(0);
                int section11Offset1 = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                Unk38 = br.AssertInt32([0x100FFFC, 0x100FFFD]);
                br.AssertInt32(0);
                br.AssertInt32(1);
                br.AssertInt32(0);
                int section11Offset2 = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                Section11Data1 = br.GetInt32(section11Offset1);
                Section11Data2 = br.GetInt32(section11Offset2);
            }

            internal void Write(BinaryWriterEx bw, List<Section3> section3s)
            {
                int index = section3s.Count;
                bw.WriteInt16(11);
                bw.WriteByte(0);
                bw.WriteByte(1);
                bw.WriteInt32(0);
                bw.WriteInt32(Unk08);
                bw.WriteInt32(0);
                bw.WriteInt32(Unk10);
                bw.WriteInt32(0);
                bw.WriteInt32(1);
                bw.WriteInt32(0);
                bw.ReserveInt32($"Section3Section11Offset1[{index}]");
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(Unk38);
                bw.WriteInt32(0);
                bw.WriteInt32(1);
                bw.WriteInt32(0);
                bw.ReserveInt32($"Section3Section11Offset2[{index}]");
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                section3s.Add(this);
            }

            internal void WriteSection11s(BinaryWriterEx bw, int index, ref int section11Count)
            {
                bw.FillInt32($"Section3Section11Offset1[{index}]", (int)bw.Position);
                bw.WriteInt32(Section11Data1);
                bw.FillInt32($"Section3Section11Offset2[{index}]", (int)bw.Position);
                bw.WriteInt32(Section11Data2);
                section11Count += 2;
            }
        }

        public class Section4
        {
            [XmlAttribute]
            public short Unk00 { get; set; }

            public List<Section4> Section4s { get; set; }

            public List<Section5> Section5s { get; set; }

            public List<FFXDrawEntityHost> Section6s { get; set; }

            public Section4()
            {
                Section4s = new List<Section4>();
                Section5s = new List<Section5>();
                Section6s = new List<FFXDrawEntityHost>();
            }

            internal Section4(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt16();
                br.AssertByte(0);
                br.AssertByte(1);
                br.AssertInt32(0);
                int section5Count = br.ReadInt32();
                int section6Count = br.ReadInt32();
                int section4Count = br.ReadInt32();
                br.AssertInt32(0);
                int section5Offset = br.ReadInt32();
                br.AssertInt32(0);
                int section6Offset = br.ReadInt32();
                br.AssertInt32(0);
                int section4Offset = br.ReadInt32();
                br.AssertInt32(0);

                br.StepIn(section4Offset);
                {
                    Section4s = new List<Section4>(section4Count);
                    for (int i = 0; i < section4Count; i++)
                        Section4s.Add(new Section4(br));
                }
                br.StepOut();

                br.StepIn(section5Offset);
                {
                    Section5s = new List<Section5>(section5Count);
                    for (int i = 0; i < section5Count; i++)
                        Section5s.Add(new Section5(br));
                }
                br.StepOut();

                br.StepIn(section6Offset);
                {
                    Section6s = new List<FFXDrawEntityHost>(section6Count);
                    for (int i = 0; i < section6Count; i++)
                        Section6s.Add(new FFXDrawEntityHost(br));
                }
                br.StepOut();
            }

            internal void Write(BinaryWriterEx bw, List<Section4> section4s)
            {
                int index = section4s.Count;
                bw.WriteInt16(Unk00);
                bw.WriteByte(0);
                bw.WriteByte(1);
                bw.WriteInt32(0);
                bw.WriteInt32(Section5s.Count);
                bw.WriteInt32(Section6s.Count);
                bw.WriteInt32(Section4s.Count);
                bw.WriteInt32(0);
                bw.ReserveInt32($"Section4Section5sOffset[{index}]");
                bw.WriteInt32(0);
                bw.ReserveInt32($"Section4Section6sOffset[{index}]");
                bw.WriteInt32(0);
                bw.ReserveInt32($"Section4Section4sOffset[{index}]");
                bw.WriteInt32(0);
                section4s.Add(this);
            }

            internal void WriteSection4s(BinaryWriterEx bw, List<Section4> section4s)
            {
                int index = section4s.IndexOf(this);
                if (Section4s.Count == 0)
                {
                    bw.FillInt32($"Section4Section4sOffset[{index}]", 0);
                }
                else
                {
                    bw.FillInt32($"Section4Section4sOffset[{index}]", (int)bw.Position);
                    foreach (Section4 section4 in Section4s)
                        section4.Write(bw, section4s);

                    foreach (Section4 section4 in Section4s)
                        section4.WriteSection4s(bw, section4s);
                }
            }

            internal void WriteSection5s(BinaryWriterEx bw, int index, ref int section5Count)
            {
                if (Section5s.Count == 0)
                {
                    bw.FillInt32($"Section4Section5sOffset[{index}]", 0);
                }
                else
                {
                    bw.FillInt32($"Section4Section5sOffset[{index}]", (int)bw.Position);
                    for (int i = 0; i < Section5s.Count; i++)
                        Section5s[i].Write(bw, section5Count + i);
                    section5Count += Section5s.Count;
                }
            }

            internal void WriteSection6s(BinaryWriterEx bw, int index, ref int section5Count, List<FFXDrawEntityHost> section6s)
            {
                bw.FillInt32($"Section4Section6sOffset[{index}]", (int)bw.Position);
                foreach (FFXDrawEntityHost section6 in Section6s)
                    section6.Write(bw, section6s);

                for (int i = 0; i < Section5s.Count; i++)
                    Section5s[i].WriteSection6s(bw, section5Count + i, section6s);
                section5Count += Section5s.Count;
            }
        }

        public class Section5
        {
            [XmlAttribute]
            public short Unk00 { get; set; }

            public List<FFXDrawEntityHost> Section6s { get; set; }

            public Section5()
            {
                Section6s = new List<FFXDrawEntityHost>();
            }

            internal Section5(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt16();
                br.AssertByte(0);
                br.AssertByte(1);
                br.AssertInt32(0);
                br.AssertInt32(0);
                int section6Count = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                int section6Offset = br.ReadInt32();
                br.AssertInt32(0);

                br.StepIn(section6Offset);
                {
                    Section6s = new List<FFXDrawEntityHost>(section6Count);
                    for (int i = 0; i < section6Count; i++)
                        Section6s.Add(new FFXDrawEntityHost(br));
                }
                br.StepOut();
            }

            internal void Write(BinaryWriterEx bw, int index)
            {
                bw.WriteInt16(Unk00);
                bw.WriteByte(0);
                bw.WriteByte(1);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(Section6s.Count);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.ReserveInt32($"Section5Section6sOffset[{index}]");
                bw.WriteInt32(0);
            }

            internal void WriteSection6s(BinaryWriterEx bw, int index, List<FFXDrawEntityHost> section6s)
            {
                bw.FillInt32($"Section5Section6sOffset[{index}]", (int)bw.Position);
                foreach (FFXDrawEntityHost section6 in Section6s)
                    section6.Write(bw, section6s);
            }
        }

        public class FFXDrawEntityHost
        {
            [XmlAttribute]
            public short Unk00 { get; set; }

            public bool Unk02 { get; set; }

            public bool Unk03 { get; set; }

            public int Unk04 { get; set; }

            public List<FFXProperty> Properties1 { get; set; }

            public List<FFXProperty> Properties2 { get; set; }

            public List<Section10> Section10s { get; set; }

            public List<int> Section11s1 { get; set; }

            public List<int> Section11s2 { get; set; }

            public FFXDrawEntityHost()
            {
                Properties1 = new List<FFXProperty>();
                Properties2 = new List<FFXProperty>();
                Section10s = new List<Section10>();
                Section11s1 = new List<int>();
                Section11s2 = new List<int>();
            }

            internal FFXDrawEntityHost(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt16();
                Unk02 = br.ReadBoolean();
                Unk03 = br.ReadBoolean();
                Unk04 = br.ReadInt32();
                int section11Count1 = br.ReadInt32();
                int section10Count = br.ReadInt32();
                int section7Count1 = br.ReadInt32();
                int section11Count2 = br.ReadInt32();
                br.AssertInt32(0);
                int section7Count2 = br.ReadInt32();
                int section11Offset = br.ReadInt32();
                br.AssertInt32(0);
                int section10Offset = br.ReadInt32();
                br.AssertInt32(0);
                int section7Offset = br.ReadInt32();
                br.AssertInt32(0);
                br.AssertInt32(0);
                br.AssertInt32(0);

                br.StepIn(section7Offset);
                {
                    Properties1 = new List<FFXProperty>(section7Count1);
                    for (int i = 0; i < section7Count1; i++)
                        Properties1.Add(new FFXProperty(br));

                    Properties2 = new List<FFXProperty>(section7Count2);
                    for (int i = 0; i < section7Count2; i++)
                        Properties2.Add(new FFXProperty(br));
                }
                br.StepOut();

                br.StepIn(section10Offset);
                {
                    Section10s = new List<Section10>(section10Count);
                    for (int i = 0; i < section10Count; i++)
                        Section10s.Add(new Section10(br));
                }
                br.StepOut();

                br.StepIn(section11Offset);
                {
                    Section11s1 = new List<int>(br.ReadInt32s(section11Count1));
                    Section11s2 = new List<int>(br.ReadInt32s(section11Count2));
                }
                br.StepOut();
            }

            internal void Write(BinaryWriterEx bw, List<FFXDrawEntityHost> section6s)
            {
                int index = section6s.Count;
                bw.WriteInt16(Unk00);
                bw.WriteBoolean(Unk02);
                bw.WriteBoolean(Unk03);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Section11s1.Count);
                bw.WriteInt32(Section10s.Count);
                bw.WriteInt32(Properties1.Count);
                bw.WriteInt32(Section11s2.Count);
                bw.WriteInt32(0);
                bw.WriteInt32(Properties2.Count);
                bw.ReserveInt32($"Section6Section11sOffset[{index}]");
                bw.WriteInt32(0);
                bw.ReserveInt32($"Section6Section10sOffset[{index}]");
                bw.WriteInt32(0);
                bw.ReserveInt32($"Section6Section7sOffset[{index}]");
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                bw.WriteInt32(0);
                section6s.Add(this);
            }

            internal void WriteSection7s(BinaryWriterEx bw, int index, List<FFXProperty> section7s)
            {
                bw.FillInt32($"Section6Section7sOffset[{index}]", (int)bw.Position);
                foreach (FFXProperty section7 in Properties1)
                    section7.Write(bw, section7s);
                foreach (FFXProperty section7 in Properties2)
                    section7.Write(bw, section7s);
            }

            internal void WriteSection10s(BinaryWriterEx bw, int index, List<Section10> section10s)
            {
                bw.FillInt32($"Section6Section10sOffset[{index}]", (int)bw.Position);
                foreach (Section10 section10 in Section10s)
                    section10.Write(bw, section10s);
            }

            internal void WriteSection11s(BinaryWriterEx bw, int index, ref int section11Count)
            {
                if (Section11s1.Count == 0 && Section11s2.Count == 0)
                {
                    bw.FillInt32($"Section6Section11sOffset[{index}]", 0);
                }
                else
                {
                    bw.FillInt32($"Section6Section11sOffset[{index}]", (int)bw.Position);
                    bw.WriteInt32s(Section11s1);
                    bw.WriteInt32s(Section11s2);
                    section11Count += Section11s1.Count + Section11s2.Count;
                }
            }
        }

        public class FFXProperty
        {
            [XmlAttribute]
            public short Unk00 { get; set; }

            public int Unk04 { get; set; }

            public List<Section8> Section8s { get; set; }

            public List<int> Section11s { get; set; }

            public FFXProperty()
            {
                Section8s = new List<Section8>();
                Section11s = new List<int>();
            }

            internal FFXProperty(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt16();
                br.AssertByte(0);
                br.AssertByte(1);
                Unk04 = br.ReadInt32();
                int section11Count = br.ReadInt32();
                br.AssertInt32(0);
                int section11Offset = br.ReadInt32();
                br.AssertInt32(0);
                int section8Offset = br.ReadInt32();
                br.AssertInt32(0);
                int section8Count = br.ReadInt32();
                br.AssertInt32(0);

                br.StepIn(section8Offset);
                {
                    Section8s = new List<Section8>(section8Count);
                    for (int i = 0; i < section8Count; i++)
                        Section8s.Add(new Section8(br));
                }
                br.StepOut();

                Section11s = new List<int>(br.GetInt32s(section11Offset, section11Count));
            }

            internal void Write(BinaryWriterEx bw, List<FFXProperty> section7s)
            {
                int index = section7s.Count;
                bw.WriteInt16(Unk00);
                bw.WriteByte(0);
                bw.WriteByte(1);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Section11s.Count);
                bw.WriteInt32(0);
                bw.ReserveInt32($"Section7Section11sOffset[{index}]");
                bw.WriteInt32(0);
                bw.ReserveInt32($"Section7Section8sOffset[{index}]");
                bw.WriteInt32(0);
                bw.WriteInt32(Section8s.Count);
                bw.WriteInt32(0);
                section7s.Add(this);
            }

            internal void WriteSection8s(BinaryWriterEx bw, int index, List<Section8> section8s)
            {
                bw.FillInt32($"Section7Section8sOffset[{index}]", (int)bw.Position);
                foreach (Section8 section8 in Section8s)
                    section8.Write(bw, section8s);
            }

            internal void WriteSection11s(BinaryWriterEx bw, int index, ref int section11Count)
            {
                if (Section11s.Count == 0)
                {
                    bw.FillInt32($"Section7Section11sOffset[{index}]", 0);
                }
                else
                {
                    bw.FillInt32($"Section7Section11sOffset[{index}]", (int)bw.Position);
                    bw.WriteInt32s(Section11s);
                    section11Count += Section11s.Count;
                }
            }
        }

        public class Section8
        {
            [XmlAttribute]
            public short Unk00 { get; set; }

            public int Unk04 { get; set; }

            public List<Section9> Section9s { get; set; }

            public List<int> Section11s { get; set; }

            public Section8()
            {
                Section9s = new List<Section9>();
                Section11s = new List<int>();
            }

            internal Section8(BinaryReaderEx br)
            {
                Unk00 = br.ReadInt16();
                br.AssertByte(0);
                br.AssertByte(1);
                Unk04 = br.ReadInt32();
                int section11Count = br.ReadInt32();
                int section9Count = br.ReadInt32();
                int section11Offset = br.ReadInt32();
                br.AssertInt32(0);
                int section9Offset = br.ReadInt32();
                br.AssertInt32(0);

                br.StepIn(section9Offset);
                {
                    Section9s = new List<Section9>(section9Count);
                    for (int i = 0; i < section9Count; i++)
                        Section9s.Add(new Section9(br));
                }
                br.StepOut();

                Section11s = new List<int>(br.GetInt32s(section11Offset, section11Count));
            }

            internal void Write(BinaryWriterEx bw, List<Section8> section8s)
            {
                int index = section8s.Count;
                bw.WriteInt16(Unk00);
                bw.WriteByte(0);
                bw.WriteByte(1);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Section11s.Count);
                bw.WriteInt32(Section9s.Count);
                bw.ReserveInt32($"Section8Section11sOffset[{index}]");
                bw.WriteInt32(0);
                bw.ReserveInt32($"Section8Section9sOffset[{index}]");
                bw.WriteInt32(0);
                section8s.Add(this);
            }

            internal void WriteSection9s(BinaryWriterEx bw, int index, List<Section9> section9s)
            {
                bw.FillInt32($"Section8Section9sOffset[{index}]", (int)bw.Position);
                foreach (Section9 section9 in Section9s)
                    section9.Write(bw, section9s);
            }

            internal void WriteSection11s(BinaryWriterEx bw, int index, ref int section11Count)
            {
                bw.FillInt32($"Section8Section11sOffset[{index}]", (int)bw.Position);
                bw.WriteInt32s(Section11s);
                section11Count += Section11s.Count;
            }
        }

        public class Section9
        {
            public int Unk04 { get; set; }

            public List<int> Section11s { get; set; }

            public Section9()
            {
                Section11s = new List<int>();
            }

            internal Section9(BinaryReaderEx br)
            {
                br.AssertInt16(48);
                br.AssertByte(0);
                br.AssertByte(1);
                Unk04 = br.ReadInt32();
                int section11Count = br.ReadInt32();
                br.AssertInt32(0);
                int section11Offset = br.ReadInt32();
                br.AssertInt32(0);

                Section11s = new List<int>(br.GetInt32s(section11Offset, section11Count));
            }

            internal void Write(BinaryWriterEx bw, List<Section9> section9s)
            {
                int index = section9s.Count;
                bw.WriteInt16(48);
                bw.WriteByte(0);
                bw.WriteByte(1);
                bw.WriteInt32(Unk04);
                bw.WriteInt32(Section11s.Count);
                bw.WriteInt32(0);
                bw.ReserveInt32($"Section9Section11sOffset[{index}]");
                bw.WriteInt32(0);
                section9s.Add(this);
            }

            internal void WriteSection11s(BinaryWriterEx bw, int index, ref int section11Count)
            {
                bw.FillInt32($"Section9Section11sOffset[{index}]", (int)bw.Position);
                bw.WriteInt32s(Section11s);
                section11Count += Section11s.Count;
            }
        }

        public class Section10
        {
            public List<int> Section11s { get; set; }

            public Section10()
            {
                Section11s = new List<int>();
            }

            internal Section10(BinaryReaderEx br)
            {
                int section11Offset = br.ReadInt32();
                br.AssertInt32(0);
                int section11Count = br.ReadInt32();
                br.AssertInt32(0);

                Section11s = new List<int>(br.GetInt32s(section11Offset, section11Count));
            }

            internal void Write(BinaryWriterEx bw, List<Section10> section10s)
            {
                int index = section10s.Count;
                bw.ReserveInt32($"Section10Section11sOffset[{index}]");
                bw.WriteInt32(0);
                bw.WriteInt32(Section11s.Count);
                bw.WriteInt32(0);
                section10s.Add(this);
            }

            internal void WriteSection11s(BinaryWriterEx bw, int index, ref int section11Count)
            {
                bw.FillInt32($"Section10Section11sOffset[{index}]", (int)bw.Position);
                bw.WriteInt32s(Section11s);
                section11Count += Section11s.Count;
            }
        }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
    }
}
