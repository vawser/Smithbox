using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiSilhouetteGenerationParameters : IHavokObject
    {
        public virtual uint Signature { get => 2282972156; }
        
        public float m_extraExpansion;
        public float m_bevelThreshold;
        public float m_maxSilhouetteSize;
        public float m_simplify2dConvexHullThreshold;
        public hkaiSilhouetteReferenceFrame m_referenceFrame;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_extraExpansion = br.ReadSingle();
            m_bevelThreshold = br.ReadSingle();
            m_maxSilhouetteSize = br.ReadSingle();
            m_simplify2dConvexHullThreshold = br.ReadSingle();
            m_referenceFrame = new hkaiSilhouetteReferenceFrame();
            m_referenceFrame.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_extraExpansion);
            bw.WriteSingle(m_bevelThreshold);
            bw.WriteSingle(m_maxSilhouetteSize);
            bw.WriteSingle(m_simplify2dConvexHullThreshold);
            m_referenceFrame.Write(s, bw);
        }
    }
}
