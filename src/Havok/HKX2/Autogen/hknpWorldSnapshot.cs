using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpWorldSnapshot : hkReferencedObject
    {
        public override uint Signature { get => 1890204722; }
        
        public hknpWorldCinfo m_worldCinfo;
        public List<hknpBody> m_bodies;
        public List<string> m_bodyNames;
        public List<hknpMotion> m_motions;
        public List<hknpConstraintCinfo> m_constraints;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_worldCinfo = new hknpWorldCinfo();
            m_worldCinfo.Read(des, br);
            m_bodies = des.ReadClassArray<hknpBody>(br);
            m_bodyNames = des.ReadStringPointerArray(br);
            m_motions = des.ReadClassArray<hknpMotion>(br);
            m_constraints = des.ReadClassArray<hknpConstraintCinfo>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_worldCinfo.Write(s, bw);
            s.WriteClassArray<hknpBody>(bw, m_bodies);
            s.WriteStringPointerArray(bw, m_bodyNames);
            s.WriteClassArray<hknpMotion>(bw, m_motions);
            s.WriteClassArray<hknpConstraintCinfo>(bw, m_constraints);
        }
    }
}
