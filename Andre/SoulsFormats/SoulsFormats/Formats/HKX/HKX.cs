using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Numerics;
using DotNext.IO.MemoryMappedFiles;

namespace SoulsFormats
{
    /// <summary>
    /// A havok file that contains animations and collision data
    /// </summary>
    public partial class HKX : SoulsFile<HKX>
    {
#pragma warning disable CS1591
        public HKXHeader Header;

        public HKXSection ClassSection;
        public HKXSection TypeSection;
        public HKXSection DataSection;

        // BB, DS2, and DS3 all use the same havok version, but have different implementations of the same classes.
        // Hence the enum to help disambiguate the games.
        public enum HKXVariation
        {
            HKXDeS,
            HKXDS1,
            HKXDS2,
            HKXDS3,
            HKXBloodBorne
        };

        public HKXVariation Variation = HKXVariation.HKXDS3;
        public bool DeserializeObjects = true;

        protected override bool Is(BinaryReaderEx br)
        {
            return (br.ReadUInt32() == 0x57E0E057);
        }

        protected override void Read(BinaryReaderEx br)
        {
            br.BigEndian = false;

            // Peek ahead and read the endian byte
            br.StepIn(0x11);
            br.BigEndian = (br.ReadByte() == 0x0) ? true : false;
            br.StepOut();

            // Read header
            Header = new HKXHeader();
            Header.Magic0 = br.AssertUInt32(0x57E0E057);
            Header.Magic1 = br.AssertUInt32(0x10C0C010);
            //Header.UserTag = br.AssertInt32(0);
            Header.UserTag = br.ReadInt32();
            Header.Version = br.AssertInt32([0x05, 0x08, 0x0B]);
            if (Header.Version == 0x05)
            {
                Variation = HKXVariation.HKXDeS;
            }
            else if (Header.Version == 0x08)
            {
                Variation = HKXVariation.HKXDS1;
            }
            Header.PointerSize = br.AssertByte([4, 8]);
            Header.Endian = br.AssertByte([0, 1]);
            Header.PaddingOption = br.AssertByte([0, 1]);
            Header.BaseClass = br.AssertByte(1); // ?
            Header.SectionCount = br.AssertInt32(3); // Always 3 sections pretty sure
            Header.ContentsSectionIndex = br.ReadInt32();
            Header.ContentsSectionOffset = br.ReadInt32();
            Header.ContentsClassNameSectionIndex = br.ReadInt32();
            Header.ContentsClassNameSectionOffset = br.ReadInt32();
            Header.ContentsVersionString = br.ReadFixStr(16); // Should be hk_2014.1.0-r1
            Header.Flags = br.ReadInt32();

            // Later versions of Havok have an extended header
            if (Header.Version >= 0x0B)
            {
                Header.Unk3C = br.ReadInt16();
                Header.SectionOffset = br.ReadInt16();
                Header.Unk40 = br.ReadUInt32();
                Header.Unk44 = br.ReadUInt32();
                Header.Unk48 = br.ReadUInt32();
                Header.Unk4C = br.ReadUInt32();

                // Read the 3 sections in the file
                br.Position = Header.SectionOffset + 0x40;
            }
            else
            {
                // Just padding
                br.AssertUInt32(0xFFFFFFFF);
            }

            ClassSection = new HKXSection(br, Variation);
            ClassSection.SectionID = 0;
            TypeSection = new HKXSection(br, Variation);
            TypeSection.SectionID = 1;
            DataSection = new HKXSection(br, Variation);
            DataSection.SectionID = 2;

            // Process the class names
            ClassSection.ReadClassnames(this);

            // Deserialize the objects
            DataSection.ReadDataObjects(this, Variation, DeserializeObjects);
        }

        public static HKX Read(string path, HKXVariation variation, bool deserializeObjects = true)
        {
            using var file = MemoryMappedFile.CreateFromFile(path, FileMode.Open, null, 0, MemoryMappedFileAccess.Read);
            using var accessor = file.CreateMemoryAccessor(0, 0, MemoryMappedFileAccess.Read);
            BinaryReaderEx br = new BinaryReaderEx(false, accessor.Memory);
            HKX ret = new HKX();
            ret.Variation = variation;
            ret.DeserializeObjects = deserializeObjects;
            DCX.Type ctype;
            br = SFUtil.GetDecompressedBR(br, out ctype);
            ret.Compression = ctype;
            ret.Read(br);
            return ret;
        }

        public static HKX Read(Memory<byte> data, HKXVariation variation, bool deserializeObjects = true)
        {
            BinaryReaderEx br = new BinaryReaderEx(false, data);
            HKX file = new HKX();
            file.Variation = variation;
            file.DeserializeObjects = deserializeObjects;
            DCX.Type ctype;
            br = SFUtil.GetDecompressedBR(br, out ctype);
            file.Compression = ctype;
            file.Read(br);
            return file;
        }

        public static HKX Read(string path, bool deserializeObjects = true)
        {
            return Read(path, HKXVariation.HKXDS3, deserializeObjects);
        }

        public static HKX Read(Memory<byte> data, bool deserializeObjects = true)
        {
            return Read(data, HKXVariation.HKXDS3, deserializeObjects);
        }

        protected override void Write(BinaryWriterEx bw)
        {
            bw.BigEndian = (Header.Endian == 0) ? true : false;
            bw.WriteUInt32(Header.Magic0);
            bw.WriteUInt32(Header.Magic1);
            bw.WriteInt32(Header.UserTag);
            bw.WriteInt32(Header.Version);
            bw.WriteByte(Header.PointerSize);
            bw.WriteByte(Header.Endian);
            bw.WriteByte(Header.PaddingOption);
            bw.WriteByte(Header.BaseClass);
            bw.WriteInt32(Header.SectionCount);
            bw.WriteInt32(Header.ContentsSectionIndex);
            bw.WriteInt32(Header.ContentsSectionOffset);
            bw.WriteInt32(Header.ContentsClassNameSectionIndex);
            bw.WriteInt32(Header.ContentsClassNameSectionOffset);
            bw.WriteFixStr(Header.ContentsVersionString, 16, 0xFF);
            bw.WriteInt32(Header.Flags);
            if (Header.Version >= 0x0B)
            {
                bw.WriteInt16(Header.Unk3C);
                bw.WriteInt16(Header.SectionOffset);
                bw.WriteUInt32(Header.Unk40);
                bw.WriteUInt32(Header.Unk44);
                bw.WriteUInt32(Header.Unk48);
                bw.WriteUInt32(Header.Unk4C);
            }
            else
            {
                bw.WriteUInt32(0xFFFFFFFF);
            }

            ClassSection.WriteHeader(bw, Variation);
            TypeSection.WriteHeader(bw, Variation);
            DataSection.WriteHeader(bw, Variation);
            ClassSection.WriteData(bw, this, HKXVariation.HKXDS3);
            TypeSection.WriteData(bw, this, HKXVariation.HKXDS3);
            DataSection.WriteData(bw, this, HKXVariation.HKXDS3);
        }

        // Utility for asserting an empty pointer that accounts for the hkx pointer size
        static private ulong AssertPointer(HKX hkx, BinaryReaderEx br)
        {
            if (hkx.Header.PointerSize == 8)
            {
                return br.AssertUInt64(0);
            }
            else
            {
                return br.AssertUInt32(0);
            }
        }

        // Writes an empty pointer taking into accound the HKX pointer size
        static private void WriteEmptyPointer(HKX hkx, BinaryWriterEx bw)
        {
            if (hkx.Header.PointerSize == 8)
            {
                bw.WriteUInt64(0);
            }
            else
            {
                bw.WriteUInt32(0);
            }
        }

        public class HKXHeader
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

        public class LocalFixup
        {
            public uint Src;
            public uint Dst;

            // The reference built from this fixup
            public HKXLocalReference Reference;

            internal LocalFixup(BinaryReaderEx br)
            {
                Src = br.ReadUInt32();
                Dst = br.ReadUInt32();
            }
        }

        public class GlobalFixup
        {
            public uint Src;
            public uint DstSectionIndex;
            public uint Dst;

            public HKXGlobalReference Reference;

            internal GlobalFixup(BinaryReaderEx br)
            {
                Src = br.ReadUInt32();
                DstSectionIndex = br.ReadUInt32();
                Dst = br.ReadUInt32();
            }
        }

        public class VirtualFixup
        {
            public uint Src;
            public uint SectionIndex;
            public uint NameOffset;

            internal VirtualFixup(BinaryReaderEx br)
            {
                Src = br.ReadUInt32();
                SectionIndex = br.ReadUInt32();
                NameOffset = br.ReadUInt32();
            }
        }

        public class HKXSection
        {
            public int SectionID;

            public string SectionTag;
            public uint AbsoluteDataStart;
            public uint LocalFixupsOffset;
            public uint GlobalFixupsOffset;
            public uint VirtualFixupsOffset;
            public uint ExportsOffset;
            public uint ImportsOffset;
            public uint EndOffset;

            public List<LocalFixup> LocalFixups;
            public List<GlobalFixup> GlobalFixups;
            public List<VirtualFixup> VirtualFixups;

            public List<HKXLocalReference> LocalReferences;
            public List<HKXGlobalReference> GlobalReferences;
            public List<HKXVirtualReference> VirtualReferences;

            public List<HKXObject> Objects;

            public byte[] SectionData;

            internal HKXSection(BinaryReaderEx br, HKXVariation variation)
            {
                SectionTag = br.ReadFixStr(19);
                br.AssertByte(0xFF);
                AbsoluteDataStart = br.ReadUInt32();
                LocalFixupsOffset = br.ReadUInt32();
                GlobalFixupsOffset = br.ReadUInt32();
                VirtualFixupsOffset = br.ReadUInt32();
                ExportsOffset = br.ReadUInt32();
                ImportsOffset = br.ReadUInt32();
                EndOffset = br.ReadUInt32();

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
                        LocalFixups.Add(new LocalFixup(br));
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
                        GlobalFixups.Add(new GlobalFixup(br));
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
                        VirtualFixups.Add(new VirtualFixup(br));
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

                LocalReferences = new List<HKXLocalReference>();
                GlobalReferences = new List<HKXGlobalReference>();
                VirtualReferences = new List<HKXVirtualReference>();
                Objects = new List<HKXObject>();
            }

            public void WriteHeader(BinaryWriterEx bw, HKXVariation variation)
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
                if (variation == HKXVariation.HKXBloodBorne || variation == HKXVariation.HKXDS3)
                {
                    bw.WriteUInt32(0xFFFFFFFF);
                    bw.WriteUInt32(0xFFFFFFFF);
                    bw.WriteUInt32(0xFFFFFFFF);
                    bw.WriteUInt32(0xFFFFFFFF);
                }
            }

            public void WriteData(BinaryWriterEx bw, HKX hkx, HKXVariation variation)
            {
                uint absoluteOffset = (uint)bw.Position;
                bw.FillUInt32("absoffset" + SectionID, absoluteOffset);
                foreach (var obj in Objects)
                {
                    obj.Write(hkx, this, bw, absoluteOffset, variation);
                }

                // Local fixups
                bw.FillUInt32("locoffset" + SectionID, (uint)bw.Position - absoluteOffset);
                foreach (var loc in LocalReferences)
                {
                    loc.Write(bw);
                }
                while ((bw.Position % 16) != 0)
                {
                    bw.WriteByte(0xFF); // 16 byte align
                }

                // Global fixups
                bw.FillUInt32("globoffset" + SectionID, (uint)bw.Position - absoluteOffset);
                foreach (var glob in GlobalReferences)
                {
                    glob.Write(bw);
                }
                while ((bw.Position % 16) != 0)
                {
                    bw.WriteByte(0xFF); // 16 byte align
                }

                // Virtual fixups
                bw.FillUInt32("virtoffset" + SectionID, (uint)bw.Position - absoluteOffset);
                foreach (var virt in VirtualReferences)
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
            internal void ReadClassnames(HKX hkx)
            {
                BinaryReaderEx br = new BinaryReaderEx(false, SectionData);
                var classnames = new HKXClassNames();
                classnames.Read(hkx, this, br, HKXVariation.HKXDS3);
                Objects.Add(classnames);
            }

            // Should be used on a data section after initial reading for object deserialization
            internal void ReadDataObjects(HKX hkx, HKXVariation variation, bool deserializeObjects)
            {
                BinaryReaderEx br = new BinaryReaderEx((hkx.Header.Endian == 0) ? true : false, SectionData);

                // Virtual fixup table defines the hkx class instances
                for (int i = 0; i < VirtualFixups.Count; i++)
                {
                    var reference = new HKXVirtualReference();
                    reference.DestSection = hkx.ClassSection; // A bit of an assumption that better hold
                    reference.ClassName = ((HKXClassNames)hkx.ClassSection.Objects[0]).Lookup(VirtualFixups[i].NameOffset);

                    br.Position = VirtualFixups[i].Src;
                    var length = (i + 1 < VirtualFixups.Count) ? VirtualFixups[i + 1].Src - VirtualFixups[i].Src : LocalFixupsOffset - VirtualFixups[i].Src;
                    HKXObject hkobject;
                    if (deserializeObjects)
                    {
                        if (reference.ClassName.ClassName == "fsnpCustomParamCompressedMeshShape")
                        {
                            hkobject = new FSNPCustomParamCompressedMeshShape();
                            hkobject.Read(hkx, this, br, variation);
                        }
                        else if (reference.ClassName.ClassName == "hknpCompressedMeshShapeData")
                        {
                            hkobject = new HKNPCompressedMeshShapeData();
                            hkobject.Read(hkx, this, br, variation);
                        }
                        else if (reference.ClassName.ClassName == "hkpStorageExtendedMeshShapeMeshSubpartStorage")
                        {
                            hkobject = new HKPStorageExtendedMeshShapeMeshSubpartStorage();
                            hkobject.Read(hkx, this, br, variation);
                        }
                        else if (reference.ClassName.ClassName == "hknpPhysicsSystemData")
                        {
                            hkobject = new HKNPPhysicsSystemData();
                            hkobject.Read(hkx, this, br, variation);
                        }
                        else if (reference.ClassName.ClassName == "hkaiNavMesh")
                        {
                            hkobject = new HKAINavMesh();
                            hkobject.Read(hkx, this, br, variation);
                        }
                        else if (reference.ClassName.ClassName == "hkcdStaticTreeDefaultTreeStorage6")
                        {
                            hkobject = new HKCDStaticAABBTreeStorage();
                            hkobject.Read(hkx, this, br, variation);
                        }
                        else if (reference.ClassName.ClassName == "hkaiDirectedGraphExplicitCost")
                        {
                            hkobject = new HKAIDirectedGraphExplicitCost();
                            hkobject.Read(hkx, this, br, variation);
                        }
                        else if (reference.ClassName.ClassName == "hkaSkeleton")
                        {
                            hkobject = new HKASkeleton();
                            hkobject.Read(hkx, this, br, variation);
                        }
                        else if (reference.ClassName.ClassName == "hkaSplineCompressedAnimation")
                        {
                            hkobject = new HKASplineCompressedAnimation();
                            hkobject.Read(hkx, this, br, variation);
                        }
                        else if (reference.ClassName.ClassName == "hkaAnimationBinding")
                        {
                            hkobject = new HKAAnimationBinding();
                            hkobject.Read(hkx, this, br, variation);
                        }
                        else if (reference.ClassName.ClassName == "hkaDefaultAnimatedReferenceFrame")
                        {
                            hkobject = new HKADefaultAnimatedReferenceFrame();
                            hkobject.Read(hkx, this, br, variation);
                        }
                        else
                        {
                            hkobject = new HKXGenericObject();
                            ((HKXGenericObject)hkobject).Read(hkx, this, br, length, variation);
                        }
                    }
                    else
                    {
                        hkobject = new HKXGenericObject();
                        ((HKXGenericObject)hkobject).Read(hkx, this, br, length, variation);
                    }
                    Objects.Add(hkobject);
                    reference.SourceObject = hkobject;

                    VirtualReferences.Add(reference);
                }
            }
        }

        public class HKXClassName
        {
            public uint Signature;
            public string ClassName;
            public uint SectionOffset;

            internal HKXClassName(BinaryReaderEx br)
            {
                Signature = br.ReadUInt32();
                br.AssertByte(0x09); // Seems random but ok
                SectionOffset = (uint)br.Position;
                ClassName = br.ReadASCII();
            }

            public void Write(BinaryWriterEx bw, uint sectionBaseOffset)
            {
                bw.WriteUInt32(Signature);
                bw.WriteByte(0x09);
                bw.WriteASCII(ClassName, true);
            }
        }

        // A local reference used to link two HKXObjects in the same section together
        public class HKXLocalReference
        {
            // The section this reference is in
            public HKXSection Section;

            // Byte offset into the source data structure to link
            public uint SourceLocalOffset;

            // Byte offset into the dest data structure to link
            public uint DestLocalOffset;

            // The source object linked
            public HKXObject SourceObject = null;

            // The destination object linked
            public HKXObject DestObject = null;

            public void Write(BinaryWriterEx bw)
            {
                bw.WriteUInt32(SourceLocalOffset + SourceObject.SectionOffset);
                bw.WriteUInt32(DestLocalOffset + DestObject.SectionOffset);
            }

            // Writes a placeholder in the source object and updates the local offset
            public void WritePlaceholder(BinaryWriterEx bw, uint sectionBaseOffset)
            {
                SourceLocalOffset = (uint)bw.Position - SourceObject.SectionOffset - sectionBaseOffset;
                bw.WriteInt64(sectionBaseOffset);
            }
        }

        public class HKXGlobalReference
        {
            // The section this reference is in
            public HKXSection SrcSection;

            // The section this reference points to
            public HKXSection DestSection;

            // Byte offset into the source data structure to link
            public uint SourceLocalOffset;

            // Byte offset into the dest data structure to link
            public uint DestLocalOffset;

            // The source object linked
            public HKXObject SourceObject = null;

            // The destination object linked
            public HKXObject DestObject = null;

            public void Write(BinaryWriterEx bw)
            {
                bw.WriteUInt32(SourceLocalOffset + SourceObject.SectionOffset);
                bw.WriteInt32(DestSection.SectionID);
                bw.WriteUInt32(DestLocalOffset + DestObject.SectionOffset);
            }

            // Writes a placeholder in the source object and updates the local offset
            public void WritePlaceholder(BinaryWriterEx bw, uint sectionBaseOffset)
            {
                SourceLocalOffset = (uint)bw.Position - SourceObject.SectionOffset - sectionBaseOffset;
                bw.WriteInt64(sectionBaseOffset);
            }
        }

        // HKX class instantiation reference
        public class HKXVirtualReference
        {
            public HKXObject SourceObject = null;
            public HKXSection DestSection = null;
            public HKXClassName ClassName = null;

            public void Write(BinaryWriterEx bw)
            {
                bw.WriteUInt32(SourceObject.SectionOffset);
                bw.WriteInt32(DestSection.SectionID);
                bw.WriteUInt32(ClassName.SectionOffset);
            }
        }

        // Base de/serialization object
        public abstract class HKXObject
        {
            // Offset within the section that this object is stored
            public uint SectionOffset;

            // Size of the serialized object
            public uint DataSize;

            public abstract void Read(HKX hkx, HKXSection section, BinaryReaderEx br, HKXVariation variation);

            public abstract void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation);

            protected List<LocalFixup> FindLocalFixupSources(HKXSection section)
            {
                List<LocalFixup> sources = new List<LocalFixup>();
                foreach (var fu in section.LocalFixups)
                {
                    if (fu.Src >= SectionOffset && fu.Src < (SectionOffset + DataSize))
                    {
                        sources.Add(fu);
                    }
                }
                return sources;
            }

            protected List<LocalFixup> FindLocalFixupDestinations(HKXSection section)
            {
                List<LocalFixup> sources = new List<LocalFixup>();
                foreach (var fu in section.LocalFixups)
                {
                    if (fu.Dst >= SectionOffset && fu.Dst < (SectionOffset + DataSize))
                    {
                        sources.Add(fu);
                    }
                }
                return sources;
            }

            protected List<GlobalFixup> FindGlobalFixupSources(HKXSection section)
            {
                List<GlobalFixup> sources = new List<GlobalFixup>();
                foreach (var gu in section.GlobalFixups)
                {
                    if (gu.Src >= SectionOffset && gu.Src < (SectionOffset + DataSize))
                    {
                        sources.Add(gu);
                    }
                }
                return sources;
            }

            protected List<GlobalFixup> FindGlobalFixupDestinations(HKX hkx, HKXSection section)
            {
                List<GlobalFixup> sources = new List<GlobalFixup>();
                foreach (var gu in hkx.ClassSection.GlobalFixups)
                {
                    if ((gu.DstSectionIndex == section.SectionID) && gu.Dst >= SectionOffset && gu.Dst < (SectionOffset + DataSize))
                    {
                        sources.Add(gu);
                    }
                }
                foreach (var gu in hkx.TypeSection.GlobalFixups)
                {
                    if ((gu.DstSectionIndex == section.SectionID) && gu.Dst >= SectionOffset && gu.Dst < (SectionOffset + DataSize))
                    {
                        sources.Add(gu);
                    }
                }
                foreach (var gu in hkx.DataSection.GlobalFixups)
                {
                    if ((gu.DstSectionIndex == section.SectionID) && gu.Dst >= SectionOffset && gu.Dst < (SectionOffset + DataSize))
                    {
                        sources.Add(gu);
                    }
                }
                return sources;
            }

            // Finds and resolves a local fixup representing a reference at an object relative offset
            internal LocalFixup ResolveLocalFixup(HKXSection section, uint offset)
            {
                foreach (var fu in section.LocalFixups)
                {
                    if (fu.Src == (SectionOffset + offset))
                    {
                        return fu;
                    }
                }
                return null;
            }

            // Finds and resolves a local reference
            internal HKXLocalReference ResolveLocalReference(HKXSection section, uint offset)
            {
                LocalFixup fu = ResolveLocalFixup(section, offset);
                if (fu != null)
                {
                    var reference = new HKXLocalReference();
                    reference.SourceObject = this;
                    reference.SourceLocalOffset = offset;
                    reference.Section = section;
                    section.LocalReferences.Add(reference);
                    fu.Reference = reference;
                    return reference;
                }
                return null;
            }

            internal HKXLocalReference ResolveLocalReference(HKX hkx, HKXSection section, BinaryReaderEx br)
            {
                br.Position += hkx.Header.PointerSize;
                return ResolveLocalReference(section, (uint)br.Position - SectionOffset - hkx.Header.PointerSize);
            }

            // Finds and resolves a local fixup representing a reference at an object relative offset
            internal GlobalFixup ResolveGlobalFixup(HKXSection section, uint offset)
            {
                foreach (var gu in section.GlobalFixups)
                {
                    if (gu.Src == (SectionOffset + offset))
                    {
                        return gu;
                    }
                }
                return null;
            }

            // Finds and resolves a global reference
            internal HKXGlobalReference ResolveGlobalReference(HKXSection section, uint offset)
            {
                GlobalFixup gu = ResolveGlobalFixup(section, offset);
                if (gu != null)
                {
                    var reference = new HKXGlobalReference();
                    reference.SourceObject = this;
                    reference.SourceLocalOffset = offset;
                    reference.SrcSection = section;
                    section.GlobalReferences.Add(reference);
                    gu.Reference = reference;
                    return reference;
                }
                return null;
            }

            internal HKXGlobalReference ResolveGlobalReference(HKX hkx, HKXSection section, BinaryReaderEx br)
            {
                br.Position += hkx.Header.PointerSize;
                return ResolveGlobalReference(section, (uint)br.Position - SectionOffset - hkx.Header.PointerSize);
            }

            // Find all references to this object and link them
            internal void ResolveDestinations(HKX hkx, HKXSection section)
            {
                var localDestinations = FindLocalFixupDestinations(section);
                foreach (var dest in localDestinations)
                {
                    if (dest.Reference == null)
                    {
                        var reference = new HKXLocalReference();
                        reference.Section = section;
                        reference.DestLocalOffset = dest.Dst - SectionOffset;
                        reference.DestObject = this;
                        section.LocalReferences.Add(reference);
                        dest.Reference = reference;
                    }
                    else
                    {
                        var reference = dest.Reference;
                        reference.DestLocalOffset = dest.Dst - SectionOffset;
                        // todo: see why this turns ds1 parent bone indices into hkaAnimation
                        // and remove this horrible "solution"
                        if (!(hkx.Variation == HKXVariation.HKXDS1 && this is HKASkeleton))
                            reference.DestObject = this;
                    }
                }

                var globalDestinations = FindGlobalFixupDestinations(hkx, section);
                foreach (var dest in globalDestinations)
                {
                    if (dest.Reference == null)
                    {
                        var reference = new HKXGlobalReference();
                        reference.DestSection = section;
                        reference.DestLocalOffset = dest.Dst - SectionOffset;
                        reference.DestObject = this;
                        dest.Reference = reference;
                    }
                    else
                    {
                        var reference = dest.Reference;
                        reference.DestSection = section;
                        reference.DestLocalOffset = dest.Dst - SectionOffset;
                        reference.DestObject = this;
                    }
                }
            }
        }

        // A serializable embedded object (such as an hkArray and its elements)
        public abstract class IHKXSerializable
        {
            public abstract void Read(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation);
            public abstract void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation);

            // Optional method to override to write any reference data that may be contained
            internal virtual void WriteReferenceData(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {

            }
        }

        // Container object for the data of an array
        public class HKArrayData<T> : HKXObject where T : IHKXSerializable, new()
        {
            public List<T> Elements;

            public override void Read(HKX hkx, HKXSection section, BinaryReaderEx br, HKXVariation variation)
            {
                throw new Exception("Use the other read function to supply array size");
            }

            public void Read(HKX hkx, HKXSection section, BinaryReaderEx br, HKXVariation variation, uint elementCount)
            {
                SectionOffset = (uint)br.Position;
                Elements = new List<T>();
                for (int i = 0; i < elementCount; i++)
                {
                    var elem = new T();
                    elem.Read(hkx, section, this, br, variation);
                    Elements.Add(elem);
                }
                DataSize = (uint)br.Position - SectionOffset;
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                SectionOffset = (uint)bw.Position - sectionBaseOffset;
                foreach (var elem in Elements)
                {
                    elem.Write(hkx, section, bw, sectionBaseOffset, variation);
                }
                while ((bw.Position % 16) != 0)
                {
                    // Write padding bytes to 16 byte align
                    bw.WriteByte(0xFF);
                }
                // Go through and recursively write reference data for all the array members
                foreach (var elem in Elements)
                {
                    elem.WriteReferenceData(hkx, section, bw, sectionBaseOffset, variation);
                }
                DataSize = (uint)bw.Position - SectionOffset;
            }
        }

        // Havok's array structure found in nearly every class
        public class HKArray<T> : IHKXSerializable where T : IHKXSerializable, new()
        {
            private HKXObject SourceObject;
            private HKXLocalReference Data;

            public uint Size;
            public uint Capacity;
            public byte Flags;

            public HKArray(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                Read(hkx, section, source, br, variation);
            }

            public override void Read(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                SourceObject = source;

                // Placeholder replaced with data pointer upon load in C++
                uint pointerOffset = (uint)br.Position - source.SectionOffset;
                if (hkx.Header.PointerSize == 8)
                {
                    br.AssertUInt64(0);
                }
                else
                {
                    br.AssertUInt32(0);
                }
                Size = br.ReadUInt32();

                uint capAndFlags = br.ReadUInt32();
                Capacity = capAndFlags & 0x00FFFFFF;
                Flags = (byte)((capAndFlags & 0xFF000000) >> 24);

                // Resolve pointer to array data
                LocalFixup fu = source.ResolveLocalFixup(section, pointerOffset);
                if (fu != null)
                {
                    Data = new HKXLocalReference();
                    Data.SourceLocalOffset = pointerOffset;
                    Data.SourceObject = source;
                    Data.DestLocalOffset = 0;
                    var data = new HKArrayData<T>();
                    br.StepIn(fu.Dst);
                    data.Read(hkx, section, br, variation, Size);
                    Data.DestObject = data;
                    Data.Section = section;
                    br.StepOut();

                    section.LocalReferences.Add(Data);
                    fu.Reference = Data;
                }
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                // Save new local position for local fixup
                if (Data != null)
                {
                    Data.SourceLocalOffset = (uint)bw.Position - SourceObject.SectionOffset - sectionBaseOffset;
                }
                if (hkx.Header.PointerSize == 8)
                {
                    bw.WriteUInt64(0); // Pointer placeholder
                }
                else
                {
                    bw.WriteUInt32(0);
                }
                bw.WriteUInt32(Size);
                bw.WriteUInt32(Capacity | (((uint)Flags) << 24));
            }

            // Call to serialize the actual data
            internal override void WriteReferenceData(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                if (Data != null)
                {
                    GetArrayData().Write(hkx, section, bw, sectionBaseOffset, variation);
                }
            }

            public HKArrayData<T> GetArrayData()
            {
                if (Data == null)
                {
                    return null;
                }
                return (HKArrayData<T>)Data.DestObject;
            }

            /// <summary>
            /// Replaces the array data with a user supplied array and updates everything
            /// </summary>
            /// <param name="data">Array to replace with</param>
            public void SetArray(List<T> data)
            {
                Size = (uint)data.Count;
                Capacity = (uint)data.Count;
                ((HKArrayData<T>)Data.DestObject).Elements = data;
            }

            // Allow indexing as an array
            public T this[int i]
            {
                get { return GetArrayData().Elements[i]; }
                set { GetArrayData().Elements[i] = value; }
            }
        }

        // Container object for the data of an array
        public class HKCStringData : HKXObject
        {
            public string Data;

            public override void Read(HKX hkx, HKXSection section, BinaryReaderEx br, HKXVariation variation)
            {
                SectionOffset = (uint)br.Position;
                Data = br.ReadASCII();
                DataSize = (uint)br.Position - SectionOffset;
            }


            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                SectionOffset = (uint)bw.Position - sectionBaseOffset;
                bw.WriteASCII(Data, true);
                DataSize = (uint)bw.Position - SectionOffset;
            }
        }

        public class HKCString : IHKXSerializable
        {
            private HKXObject SourceObject;
            private HKXLocalReference Data;

            public HKCString(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                Read(hkx, section, source, br, variation);
            }

            public override void Read(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                SourceObject = source;

                // Placeholder replaced with data pointer upon load in C++
                uint pointerOffset = (uint)br.Position - source.SectionOffset;
                if (hkx.Header.PointerSize == 8)
                {
                    br.AssertUInt64(0);
                }
                else
                {
                    br.AssertUInt32(0);
                }


                // Resolve pointer to array data
                LocalFixup fu = source.ResolveLocalFixup(section, pointerOffset);
                if (fu != null)
                {
                    Data = new HKXLocalReference();
                    Data.SourceLocalOffset = pointerOffset;
                    Data.SourceObject = source;
                    Data.DestLocalOffset = 0;
                    var data = new HKCStringData();
                    br.StepIn(fu.Dst);
                    data.Read(hkx, section, br, variation);
                    Data.DestObject = data;
                    Data.Section = section;
                    br.StepOut();

                    section.LocalReferences.Add(Data);
                    fu.Reference = Data;
                }
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                // Save new local position for local fixup
                if (Data != null)
                {
                    Data.SourceLocalOffset = (uint)bw.Position - SourceObject.SectionOffset - sectionBaseOffset;
                }
                if (hkx.Header.PointerSize == 8)
                {
                    bw.WriteUInt64(0); // Pointer placeholder
                }
                else
                {
                    bw.WriteUInt32(0);
                }
            }

            // Call to serialize the actual data
            internal override void WriteReferenceData(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                if (Data != null)
                {
                    ((HKCStringData)Data.DestObject).Write(hkx, section, bw, sectionBaseOffset, variation);
                }
            }

            public string GetString()
            {
                if (Data == null)
                {
                    return null;
                }
                return ((HKCStringData)Data.DestObject).Data;
            }
        }

        public class HKFloat : IHKXSerializable
        {
            public float data;
            public override void Read(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                data = br.ReadSingle();
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                bw.WriteSingle(data);
            }
        }

        public class HKUInt : IHKXSerializable
        {
            public uint data;
            public override void Read(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                data = br.ReadUInt32();
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                bw.WriteUInt32(data);
            }
        }

        public class HKUShort : IHKXSerializable
        {
            public ushort data;
            public override void Read(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                data = br.ReadUInt16();
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                bw.WriteUInt16(data);
            }
        }

        public class HKShort : IHKXSerializable
        {
            public short data;
            public override void Read(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                data = br.ReadInt16();
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                bw.WriteInt16(data);
            }
        }

        public class HKByte : IHKXSerializable
        {
            public byte data;
            public HKByte() { data = 0; }
            public HKByte(byte b) { data = b; }
            public override void Read(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                data = br.ReadByte();
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                bw.WriteByte(data);
            }
        }

        public class HKVector4 : IHKXSerializable
        {
            public Vector4 Vector;
            public override void Read(HKX hkx, HKXSection section, HKXObject source, BinaryReaderEx br, HKXVariation variation)
            {
                Vector = br.ReadVector4();
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                bw.WriteVector4(Vector);
            }
        }

        // A generic undocumented object designed to be read and written with all references intact
        public class HKXGenericObject : HKXObject
        {
            public byte[] Bytes;

            List<HKXLocalReference> LocalReferences;
            List<HKXGlobalReference> GlobalReferences;

            public override void Read(HKX hkx, HKXSection section, BinaryReaderEx br, HKXVariation variation)
            {
                throw new NotImplementedException();
            }

            public void Read(HKX hkx, HKXSection section, BinaryReaderEx br, uint size, HKXVariation variation)
            {
                SectionOffset = (uint)br.Position;
                Bytes = br.ReadBytes((int)size);
                DataSize = size;

                // Resolve references where this object is a source
                var localSources = FindLocalFixupSources(section);
                LocalReferences = new List<HKXLocalReference>();
                foreach (var src in localSources)
                {
                    if (src.Reference == null)
                    {
                        var reference = new HKXLocalReference();
                        reference.Section = section;
                        reference.SourceLocalOffset = src.Src - SectionOffset;
                        reference.SourceObject = this;
                        section.LocalReferences.Add(reference);
                        src.Reference = reference;
                    }
                    else
                    {
                        var reference = src.Reference;
                        reference.SourceLocalOffset = src.Src - SectionOffset;
                        reference.SourceObject = this;
                    }
                    LocalReferences.Add(src.Reference);
                }

                // Resolve references where this object is a global source
                var globalSources = FindGlobalFixupSources(section);
                GlobalReferences = new List<HKXGlobalReference>();
                foreach (var src in globalSources)
                {
                    if (src.Reference == null)
                    {
                        var reference = new HKXGlobalReference();
                        reference.SrcSection = section;
                        reference.SourceLocalOffset = src.Src - SectionOffset;
                        reference.SourceObject = this;
                        section.GlobalReferences.Add(reference);
                        src.Reference = reference;
                    }
                    else
                    {
                        var reference = src.Reference;
                        reference.SrcSection = section;
                        reference.SourceLocalOffset = src.Src - SectionOffset;
                        reference.SourceObject = this;
                        section.GlobalReferences.Add(reference);
                    }
                    GlobalReferences.Add(src.Reference);
                }

                ResolveDestinations(hkx, section);
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                SectionOffset = (uint)bw.Position - sectionBaseOffset;
                bw.WriteBytes(Bytes);
            }

            public void WriteWithReferences(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                bw.BigEndian = (hkx.Header.Endian == 0) ? true : false;
                Write(hkx, section, bw, sectionBaseOffset, variation);
                foreach (var reference in LocalReferences)
                {
                    if (reference.DestObject == this)
                    {
                        bw.Position = reference.SourceLocalOffset;
                        bw.WriteUInt32(reference.DestLocalOffset);
                    }
                }
                foreach (var reference in GlobalReferences)
                {
                    if (reference.DestObject == this)
                    {
                        bw.Position = reference.SourceLocalOffset;
                        bw.WriteUInt32(reference.DestLocalOffset);
                    }
                }
            }
        }

        // Class names data found in the __classnames__ section of the hkx
        public class HKXClassNames : HKXObject
        {
            public List<HKXClassName> ClassNames;
            public Dictionary<uint, HKXClassName> OffsetClassNamesMap;

            public override void Read(HKX hkx, HKXSection section, BinaryReaderEx br, HKXVariation variation)
            {
                ClassNames = new List<HKXClassName>();
                OffsetClassNamesMap = new Dictionary<uint, HKXClassName>();
                while (br.ReadUInt16() != 0xFFFF)
                {
                    br.Position -= 2;
                    uint stringStart = (uint)br.Position + 5;
                    var className = new HKXClassName(br);
                    ClassNames.Add(className);
                    OffsetClassNamesMap.Add(stringStart, className);
                }
            }

            public override void Write(HKX hkx, HKXSection section, BinaryWriterEx bw, uint sectionBaseOffset, HKXVariation variation)
            {
                SectionOffset = (uint)bw.Position - sectionBaseOffset;
                foreach (var cls in ClassNames)
                {
                    cls.Write(bw, sectionBaseOffset);
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
#pragma warning restore CS1591
    }
}
