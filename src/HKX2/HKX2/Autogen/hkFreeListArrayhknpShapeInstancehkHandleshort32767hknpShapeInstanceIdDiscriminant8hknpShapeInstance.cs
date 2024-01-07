using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkFreeListArrayhknpShapeInstancehkHandleshort32767hknpShapeInstanceIdDiscriminant8hknpShapeInstance : IHavokObject
    {
        public virtual uint Signature { get => 2577527628; }
        
        public List<hknpShapeInstance> m_elements;
        public int m_firstFree;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_elements = des.ReadClassArray<hknpShapeInstance>(br);
            m_firstFree = br.ReadInt32();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassArray<hknpShapeInstance>(bw, m_elements);
            bw.WriteInt32(m_firstFree);
            bw.WriteUInt32(0);
        }
    }
}
