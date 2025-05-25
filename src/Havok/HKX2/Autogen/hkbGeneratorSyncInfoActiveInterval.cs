using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbGeneratorSyncInfoActiveInterval : IHavokObject
    {
        public virtual uint Signature { get => 692550698; }
        
        public hkbGeneratorSyncInfoSyncPoint m_syncPoints_0;
        public hkbGeneratorSyncInfoSyncPoint m_syncPoints_1;
        public float m_fraction;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_syncPoints_0 = new hkbGeneratorSyncInfoSyncPoint();
            m_syncPoints_0.Read(des, br);
            m_syncPoints_1 = new hkbGeneratorSyncInfoSyncPoint();
            m_syncPoints_1.Read(des, br);
            m_fraction = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_syncPoints_0.Write(s, bw);
            m_syncPoints_1.Write(s, bw);
            bw.WriteSingle(m_fraction);
        }
    }
}
