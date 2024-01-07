using System.Collections.Generic;

namespace SoulsFormats
{
    public partial class MSB2
    {
        private class MapstudioBoneName : Param<BoneName>
        {
            internal override int Version => 0;
            internal override string Name => "MAPSTUDIO_BONE_NAME_STRING";

            public List<BoneName> BoneNames { get; set; }

            public MapstudioBoneName()
            {
                BoneNames = new List<BoneName>();
            }

            internal override BoneName ReadEntry(BinaryReaderEx br)
            {
                return BoneNames.EchoAdd(new BoneName(br));
            }

            public override List<BoneName> GetEntries()
            {
                return BoneNames;
            }
        }

        internal class BoneName : NamedEntry
        {
            public BoneName()
            {
                Name = "Master";
            }

            public BoneName DeepCopy()
            {
                return (BoneName)MemberwiseClone();
            }

            internal BoneName(BinaryReaderEx br)
            {
                Name = br.ReadUTF16();
            }

            internal override void Write(BinaryWriterEx bw, int index)
            {
                bw.WriteUTF16(MSB.ReambiguateName(Name), true);
            }
        }
    }
}
