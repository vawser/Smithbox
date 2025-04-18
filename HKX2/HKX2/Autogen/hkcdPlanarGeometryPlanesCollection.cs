using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum Bounds
    {
        BOUND_POS_X = 0,
        BOUND_NEG_X = 1,
        BOUND_POS_Y = 2,
        BOUND_NEG_Y = 3,
        BOUND_POS_Z = 4,
        BOUND_NEG_Z = 5,
        NUM_BOUNDS = 6,
    }
    
    public partial class hkcdPlanarGeometryPlanesCollection : hkReferencedObject
    {
        public override uint Signature { get => 850946192; }
        
        public Vector4 m_offsetAndScale;
        public List<hkcdPlanarGeometryPrimitivesPlane> m_planes;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_offsetAndScale = des.ReadVector4(br);
            m_planes = des.ReadClassArray<hkcdPlanarGeometryPrimitivesPlane>(br);
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_offsetAndScale);
            s.WriteClassArray<hkcdPlanarGeometryPrimitivesPlane>(bw, m_planes);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
