using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum WeldResult
    {
        WELD_RESULT_REJECT_CONTACT_POINT = 0,
        WELD_RESULT_ACCEPT_CONTACT_POINT_MODIFIED = 1,
        WELD_RESULT_ACCEPT_CONTACT_POINT_UNMODIFIED = 2,
    }
    
    public partial class hkpConvexShape : hkpSphereRepShape
    {
        public override uint Signature { get => 4115545146; }
        
        public float m_radius;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_radius = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_radius);
            bw.WriteUInt32(0);
        }
    }
}
