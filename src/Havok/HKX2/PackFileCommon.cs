using SoulsFormats;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace HKX2
{
    public enum HKXVariation
    {
        HKXDeS,
        HKXDS1,
        HKXDS2,
        HKXDS3,
        HKXBloodBorne,
        HKXBotwSwitch,
    };
    internal class HKXHeader
    {
        public uint Magic0;
        public uint Magic1;
        public int UserTag;
        public int Version;
        public byte PointerSize;
        public byte Endian;
        public byte PaddingOption;
        public byte BaseClass;
        public int SectionCount;
        public int ContentsSectionIndex;
        public int ContentsSectionOffset;
        public int ContentsClassNameSectionIndex;
        public int ContentsClassNameSectionOffset;
        public string ContentsVersionString;
        public int Flags;
        public short Unk3C;
        public short SectionOffset;
        public uint Unk40;
        public uint Unk44;
        public uint Unk48;
        public uint Unk4C;
    }

    internal class LocalFixup
    {
        public uint Src;
        public uint Dst;

        internal LocalFixup()
        {

        }

        internal LocalFixup(BinaryReaderEx br)
        {
            Src = br.ReadUInt32();
            Dst = br.ReadUInt32();
        }

        internal void Write(BinaryWriterEx bw)
        {
            bw.WriteUInt32(Src);
            bw.WriteUInt32(Dst);
        }
    }

    internal class GlobalFixup
    {
        public uint Src;
        public uint DstSectionIndex;
        public uint Dst;

        internal GlobalFixup()
        {

        }

        internal GlobalFixup(BinaryReaderEx br)
        {
            Src = br.ReadUInt32();
            DstSectionIndex = br.ReadUInt32();
            Dst = br.ReadUInt32();
        }

        internal void Write(BinaryWriterEx bw)
        {
            bw.WriteUInt32(Src);
            bw.WriteUInt32(DstSectionIndex);
            bw.WriteUInt32(Dst);
        }
    }

    internal class VirtualFixup
    {
        public uint Src;
        public uint SectionIndex;
        public uint NameOffset;

        internal VirtualFixup()
        {

        }

        internal VirtualFixup(BinaryReaderEx br)
        {
            Src = br.ReadUInt32();
            SectionIndex = br.ReadUInt32();
            NameOffset = br.ReadUInt32();
        }

        internal void Write(BinaryWriterEx bw)
        {
            bw.WriteUInt32(Src);
            bw.WriteUInt32(SectionIndex);
            bw.WriteUInt32(NameOffset);
        }
    }

    public class HKXClassName
    {
        public uint Signature;
        public string ClassName;
        public uint SectionOffset;

        internal HKXClassName()
        {

        }

        internal HKXClassName(BinaryReaderEx br)
        {
            Signature = br.ReadUInt32();
            br.AssertByte(0x09); // Seems random but ok
            SectionOffset = (uint)br.Position;
            ClassName = br.ReadASCII();
        }

        public void Write(BinaryWriterEx bw)
        {
            bw.WriteUInt32(Signature);
            bw.WriteByte(0x09);
            bw.WriteASCII(ClassName, true);
        }
    }

    // Class names data found in the __classnames__ section of the hkx
    internal class HKXClassNames
    {
        public List<HKXClassName> ClassNames;
        public Dictionary<uint, HKXClassName> OffsetClassNamesMap;

        public void Read(PackFileDeserializer hkx, HKXSection section, BinaryReaderEx br, HKX.HKXVariation variation)
        {
            ClassNames = new List<HKXClassName>();
            OffsetClassNamesMap = new Dictionary<uint, HKXClassName>();
            while (br.ReadByte() != 0xFF)
            {
                br.Position -= 1;
                uint stringStart = (uint)br.Position + 5;
                var className = new HKXClassName(br);
                ClassNames.Add(className);
                OffsetClassNamesMap.Add(stringStart, className);
                if (br.Position == br.Length)
                {
                    break;
                }
            }
        }

        public void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKX.HKXVariation variation)
        {
            foreach (var cls in ClassNames)
            {
                cls.Write(bw);
            }
            while ((bw.Position % 16) != 0)
            {
                // Write padding bytes to 16 byte align
                bw.WriteByte(0xFF);
            }
        }

        public HKXClassName Lookup(uint offset)
        {
            return OffsetClassNamesMap[offset];
        }
    }

    internal class HKXSection
    {
        public int SectionID;

        public string SectionTag;

        public List<LocalFixup> LocalFixups = new List<LocalFixup>();
        public List<GlobalFixup> GlobalFixups = new List<GlobalFixup>();
        public List<VirtualFixup> VirtualFixups = new List<VirtualFixup>();

        public Dictionary<uint, LocalFixup> _localMap = new Dictionary<uint, LocalFixup>();
        public Dictionary<uint, GlobalFixup> _globalMap = new Dictionary<uint, GlobalFixup>();
        public Dictionary<uint, VirtualFixup> _virtualMap = new Dictionary<uint, VirtualFixup>();

        public byte[] SectionData;

        internal HKXSection()
        {

        }

        internal HKXSection(BinaryReaderEx br, HKXVariation variation)
        {
            SectionTag = br.ReadFixStr(19);
            br.AssertByte(0xFF);
            var AbsoluteDataStart = br.ReadUInt32();
            var LocalFixupsOffset = br.ReadUInt32();
            var GlobalFixupsOffset = br.ReadUInt32();
            var VirtualFixupsOffset = br.ReadUInt32();
            var ExportsOffset = br.ReadUInt32();
            var ImportsOffset = br.ReadUInt32();
            var EndOffset = br.ReadUInt32();

            // Read Data
            br.StepIn(AbsoluteDataStart);
            SectionData = br.ReadBytes((int)LocalFixupsOffset);
            br.StepOut();

            // Local fixups
            LocalFixups = new List<LocalFixup>();
            br.StepIn(AbsoluteDataStart + LocalFixupsOffset);
            for (int i = 0; i < (GlobalFixupsOffset - LocalFixupsOffset) / 8; i++)
            {
                if (br.ReadUInt32() != 0xFFFFFFFF)
                {
                    br.Position -= 4;
                    var f = new LocalFixup(br);
                    _localMap.Add(f.Src, f);
                    LocalFixups.Add(f);
                }
            }
            br.StepOut();

            // Global fixups
            GlobalFixups = new List<GlobalFixup>();
            br.StepIn(AbsoluteDataStart + GlobalFixupsOffset);
            for (int i = 0; i < (VirtualFixupsOffset - GlobalFixupsOffset) / 12; i++)
            {
                if (br.ReadUInt32() != 0xFFFFFFFF)
                {
                    br.Position -= 4;
                    var f = new GlobalFixup(br);
                    _globalMap.Add(f.Src, f);
                    GlobalFixups.Add(f);
                }
            }
            br.StepOut();

            // Virtual fixups
            VirtualFixups = new List<VirtualFixup>();
            br.StepIn(AbsoluteDataStart + VirtualFixupsOffset);
            for (int i = 0; i < (ExportsOffset - VirtualFixupsOffset) / 12; i++)
            {
                if (br.ReadUInt32() != 0xFFFFFFFF)
                {
                    br.Position -= 4;
                    var f = new VirtualFixup(br);
                    _virtualMap.Add(f.Src, f);
                    VirtualFixups.Add(f);
                }
            }
            br.StepOut();

            if (variation == HKXVariation.HKXBloodBorne || variation == HKXVariation.HKXDS3)
            {
                br.AssertUInt32(0xFFFFFFFF);
                br.AssertUInt32(0xFFFFFFFF);
                br.AssertUInt32(0xFFFFFFFF);
                br.AssertUInt32(0xFFFFFFFF);
            }
        }

        public void WriteHeader(BinaryWriterEx bw, HKX.HKXVariation variation)
        {
            bw.WriteFixStr(SectionTag, 19);
            bw.WriteByte(0xFF);
            bw.ReserveUInt32("absoffset" + SectionID);
            bw.ReserveUInt32("locoffset" + SectionID);
            bw.ReserveUInt32("globoffset" + SectionID);
            bw.ReserveUInt32("virtoffset" + SectionID);
            bw.ReserveUInt32("expoffset" + SectionID);
            bw.ReserveUInt32("impoffset" + SectionID);
            bw.ReserveUInt32("endoffset" + SectionID);
            if (variation == HKX.HKXVariation.HKXBloodBorne || variation == HKX.HKXVariation.HKXDS3)
            {
                bw.WriteUInt32(0xFFFFFFFF);
                bw.WriteUInt32(0xFFFFFFFF);
                bw.WriteUInt32(0xFFFFFFFF);
                bw.WriteUInt32(0xFFFFFFFF);
            }
        }

        public void WriteData(BinaryWriterEx bw)
        {
            uint absoluteOffset = (uint)bw.Position;
            bw.FillUInt32("absoffset" + SectionID, absoluteOffset);
            bw.WriteBytes(SectionData);
            while ((bw.Position % 16) != 0)
            {
                bw.WriteByte(0xFF); // 16 byte align
            }

            // Local fixups
            bw.FillUInt32("locoffset" + SectionID, (uint)bw.Position - absoluteOffset);
            foreach (var loc in LocalFixups)
            {
                loc.Write(bw);
            }
            while ((bw.Position % 16) != 0)
            {
                bw.WriteByte(0xFF); // 16 byte align
            }

            // Global fixups
            bw.FillUInt32("globoffset" + SectionID, (uint)bw.Position - absoluteOffset);
            foreach (var glob in GlobalFixups)
            {
                glob.Write(bw);
            }
            while ((bw.Position % 16) != 0)
            {
                bw.WriteByte(0xFF); // 16 byte align
            }

            // Virtual fixups
            bw.FillUInt32("virtoffset" + SectionID, (uint)bw.Position - absoluteOffset);
            foreach (var virt in VirtualFixups)
            {
                virt.Write(bw);
            }
            while ((bw.Position % 16) != 0)
            {
                bw.WriteByte(0xFF); // 16 byte align
            }

            bw.FillUInt32("expoffset" + SectionID, (uint)bw.Position - absoluteOffset);
            bw.FillUInt32("impoffset" + SectionID, (uint)bw.Position - absoluteOffset);
            bw.FillUInt32("endoffset" + SectionID, (uint)bw.Position - absoluteOffset);
        }

        // Only use for a classnames structure after preliminary deserialization
        internal HKXClassNames ReadClassnames(PackFileDeserializer hkx)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, SectionData);
            var classnames = new HKXClassNames();
            classnames.Read(hkx, this, br, HKX.HKXVariation.HKXDS3);
            return classnames;
        }
    }
}
