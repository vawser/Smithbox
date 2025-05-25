using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpStiffSpringConstraintDataAtoms : IHavokObject
    {
        public virtual uint Signature { get => 3494829737; }
        
        public hkpSetLocalTranslationsConstraintAtom m_pivots;
        public hkpSetupStabilizationAtom m_setupStabilization;
        public hkpStiffSpringConstraintAtom m_spring;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_pivots = new hkpSetLocalTranslationsConstraintAtom();
            m_pivots.Read(des, br);
            m_setupStabilization = new hkpSetupStabilizationAtom();
            m_setupStabilization.Read(des, br);
            m_spring = new hkpStiffSpringConstraintAtom();
            m_spring.Read(des, br);
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_pivots.Write(s, bw);
            m_setupStabilization.Write(s, bw);
            m_spring.Write(s, bw);
            bw.WriteUInt32(0);
        }
    }
}
