using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiSilhouetteGeneratorSectionContext : IHavokObject
    {
        public virtual uint Signature { get => 3718371412; }
        
        public hkQTransform m_lastRelativeTransform;
        public hkaiSilhouetteGenerator m_generator;
        public bool m_generatedLastFrame;
        public bool m_generatingThisFrame;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_lastRelativeTransform = new hkQTransform();
            m_lastRelativeTransform.Read(des, br);
            m_generator = des.ReadClassPointer<hkaiSilhouetteGenerator>(br);
            br.ReadUInt32();
            m_generatedLastFrame = br.ReadBoolean();
            m_generatingThisFrame = br.ReadBoolean();
            br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_lastRelativeTransform.Write(s, bw);
            s.WriteClassPointer<hkaiSilhouetteGenerator>(bw, m_generator);
            bw.WriteUInt32(0);
            bw.WriteBoolean(m_generatedLastFrame);
            bw.WriteBoolean(m_generatingThisFrame);
            bw.WriteUInt16(0);
        }
    }
}
