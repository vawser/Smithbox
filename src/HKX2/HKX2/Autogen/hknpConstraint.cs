using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpConstraint : IHavokObject
    {
        public virtual uint Signature { get => 1759752852; }
        
        public enum FlagsEnum
        {
            NO_FLAGS = 0,
            IS_EXPORTABLE = 1,
            IS_IMMEDIATE = 2,
            IS_DISABLED = 4,
            IS_DESTRUCTION_INTERNAL = 8,
            AUTO_REMOVE_ON_DESTRUCTION_RESET = 16,
            AUTO_REMOVE_ON_DESTRUCTION = 32,
            RAISE_CONSTRAINT_FORCE_EVENTS = 64,
            RAISE_CONSTRAINT_FORCE_EXCEEDED_EVENTS = 128,
        }
        
        public uint m_bodyIdA;
        public uint m_bodyIdB;
        public hkpConstraintData m_data;
        public uint m_id;
        public byte m_flags;
        public ConstraintType m_type;
        public ushort m_sizeOfAtoms;
        public ushort m_sizeOfSchemas;
        public byte m_numSolverResults;
        public byte m_numSolverElemTemps;
        public ushort m_runtimeSize;
        public ulong m_userData;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_bodyIdA = br.ReadUInt32();
            m_bodyIdB = br.ReadUInt32();
            m_data = des.ReadClassPointer<hkpConstraintData>(br);
            m_id = br.ReadUInt32();
            br.ReadUInt16();
            m_flags = br.ReadByte();
            m_type = (ConstraintType)br.ReadByte();
            br.ReadUInt64();
            m_sizeOfAtoms = br.ReadUInt16();
            m_sizeOfSchemas = br.ReadUInt16();
            m_numSolverResults = br.ReadByte();
            m_numSolverElemTemps = br.ReadByte();
            m_runtimeSize = br.ReadUInt16();
            br.ReadUInt64();
            m_userData = br.ReadUInt64();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt32(m_bodyIdA);
            bw.WriteUInt32(m_bodyIdB);
            s.WriteClassPointer<hkpConstraintData>(bw, m_data);
            bw.WriteUInt32(m_id);
            bw.WriteUInt16(0);
            bw.WriteByte(m_flags);
            bw.WriteByte((byte)m_type);
            bw.WriteUInt64(0);
            bw.WriteUInt16(m_sizeOfAtoms);
            bw.WriteUInt16(m_sizeOfSchemas);
            bw.WriteByte(m_numSolverResults);
            bw.WriteByte(m_numSolverElemTemps);
            bw.WriteUInt16(m_runtimeSize);
            bw.WriteUInt64(0);
            bw.WriteUInt64(m_userData);
        }
    }
}
