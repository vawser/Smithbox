using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpGenericConstraintDataSchemeConstraintInfo : IHavokObject
    {
        public virtual uint Signature { get => 3594657561; }
        
        public int m_maxSizeOfSchema;
        public int m_sizeOfSchemas;
        public int m_numSolverResults;
        public int m_numSolverElemTemps;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_maxSizeOfSchema = br.ReadInt32();
            m_sizeOfSchemas = br.ReadInt32();
            m_numSolverResults = br.ReadInt32();
            m_numSolverElemTemps = br.ReadInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt32(m_maxSizeOfSchema);
            bw.WriteInt32(m_sizeOfSchemas);
            bw.WriteInt32(m_numSolverResults);
            bw.WriteInt32(m_numSolverElemTemps);
        }
    }
}
