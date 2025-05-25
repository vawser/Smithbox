using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpPulleyConstraintDataAtoms : IHavokObject
    {
        public virtual uint Signature { get => 4129650384; }
        
        public hkpSetLocalTranslationsConstraintAtom m_translations;
        public hkpPulleyConstraintAtom m_pulley;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_translations = new hkpSetLocalTranslationsConstraintAtom();
            m_translations.Read(des, br);
            m_pulley = new hkpPulleyConstraintAtom();
            m_pulley.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_translations.Write(s, bw);
            m_pulley.Write(s, bw);
        }
    }
}
