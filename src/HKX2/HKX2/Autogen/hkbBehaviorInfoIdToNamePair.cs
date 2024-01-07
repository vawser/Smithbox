using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbBehaviorInfoIdToNamePair : IHavokObject
    {
        public virtual uint Signature { get => 1457694762; }
        
        public string m_behaviorName;
        public string m_nodeName;
        public NodeType m_toolType;
        public ushort m_id;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_behaviorName = des.ReadStringPointer(br);
            m_nodeName = des.ReadStringPointer(br);
            m_toolType = (NodeType)br.ReadUInt16();
            m_id = br.ReadUInt16();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_behaviorName);
            s.WriteStringPointer(bw, m_nodeName);
            bw.WriteUInt16((ushort)m_toolType);
            bw.WriteUInt16(m_id);
            bw.WriteUInt32(0);
        }
    }
}
