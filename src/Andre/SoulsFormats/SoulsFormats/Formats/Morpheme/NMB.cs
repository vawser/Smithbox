using SoulsFormats.Formats.Morpheme.MorphemeBundle;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SoulsFormats.Formats.Morpheme
{
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
    public class NMB
    {
        public List<MorphemeBundle_Base> bundles;

        public List<MorphemeBundle_Base> rig = new List<MorphemeBundle_Base>();
        public List<RigToAnimMap> animMapping = new List<RigToAnimMap>();
        public List<EventTrack> eventTracks = new List<EventTrack>();
        public List<MorphemeBundle_Base> characterControllerDef = new List<MorphemeBundle_Base>();
        public NetworkBundle network;
        public MorphemeFileHeader fileHeader;
        public FileNameLookupTable fileNameLookupTable;

        public MorphemeBundle_Base networkRaw;

        public NMB() { }

        public NMB(BinaryReaderEx br)
        {
            Read(br);
        }

        public void Read(BinaryReaderEx br)
        {
            MorphemeBundle_Base.Set64BitAndEndianness(br);

            while (br.Position < br.Length - 0xC)
            {
                var nextBundle = MorphemeBundle_Base.ReadBundleType(br);
                switch (nextBundle)
                {
                    case eBundleType.Bundle_Skeleton:
                        rig.Add(new Rig(br));
                        break;
                    case eBundleType.Bundle_SkeletonToAnimMap:
                        animMapping.Add(new RigToAnimMap(br));
                        break;
                    case eBundleType.Bundle_DiscreteEventTrack:
                    case eBundleType.Bundle_DurationEventTrack:
                    case eBundleType.Bundle_CurveEventTrack:
                        eventTracks.Add(new EventTrack(br));
                        break;
                    case eBundleType.Bundle_CharacterControllerDef:
                        characterControllerDef.Add(new MorphemeBundleGeneric(br));
                        break;
                    case eBundleType.Bundle_Network:
                        br.StepIn(br.Position);
                        //network = new NetworkBundle(br);
                        br.StepOut();
                        networkRaw = new MorphemeBundleGeneric(br);
                        break;
                    case eBundleType.Bundle_FileHeader:
                        fileHeader = new MorphemeFileHeader(br);
                        break;
                    case eBundleType.Bundle_FileNameLookupTable:
                        fileNameLookupTable = new FileNameLookupTable(br);
                        break;
                    default:
                        bundles.Add(new MorphemeBundleGeneric(br));
                        Debug.WriteLine($"Unknown bundle type: {bundles.Last().bundleType} found");
                        break;
                }
            }
        }

        public void Write(BinaryWriterEx bw)
        {
            fileHeader.Write(bw);
            if (characterControllerDef.Count == rig.Count)
            {
                for (int i = 0; i < characterControllerDef.Count; i++)
                {
                    characterControllerDef[i].Write(bw);
                    rig[i].Write(bw);

                    bw.WriteBytes(new byte[] { 0xCD, 0xCD, 0xCD, 0xCD });
                }
            }

            foreach (var eTrack in eventTracks)
            {
                eTrack.Write(bw);
            }

            foreach (var msg in animMapping)
            {
                msg.Write(bw);
            }

            networkRaw.Write(bw);
            fileNameLookupTable.Write(bw);
        }
    }
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
}
