using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum UpVectorTransformMethod
    {
        USE_GLOBAL_UP = 0,
        USE_INSTANCE_TRANSFORM = 1,
        USE_FACE_NORMAL = 2,
    }
    
    public partial class hkaiReferenceFrameAndExtrusion : IHavokObject
    {
        public virtual uint Signature { get => 1490158809; }
        
        public Vector4 m_up;
        public float m_cellExtrusion;
        public float m_silhouetteRadiusExpasion;
        public UpVectorTransformMethod m_upTransformMethod;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_up = des.ReadVector4(br);
            m_cellExtrusion = br.ReadSingle();
            m_silhouetteRadiusExpasion = br.ReadSingle();
            m_upTransformMethod = (UpVectorTransformMethod)br.ReadByte();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_up);
            bw.WriteSingle(m_cellExtrusion);
            bw.WriteSingle(m_silhouetteRadiusExpasion);
            bw.WriteByte((byte)m_upTransformMethod);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
