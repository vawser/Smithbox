using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbBoneIndexArray : hkbBindable
    {
        public override uint Signature { get => 1025963045; }
        
        public List<short> m_boneIndices;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_boneIndices = des.ReadInt16Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteInt16Array(bw, m_boneIndices);
        }
    }
}
