using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiPersistentEdgeKey : IHavokObject
    {
        public virtual uint Signature { get => 4098110266; }
        
        public hkaiPersistentFaceKey m_faceKey;
        public short m_edgeOffset;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_faceKey = new hkaiPersistentFaceKey();
            m_faceKey.Read(des, br);
            m_edgeOffset = br.ReadInt16();
            br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_faceKey.Write(s, bw);
            bw.WriteInt16(m_edgeOffset);
            bw.WriteUInt16(0);
        }
    }
}
