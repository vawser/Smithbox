using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpSimpleContactConstraintAtom : hkpConstraintAtom
    {
        public override uint Signature { get => 1680851461; }
        
        public ushort m_sizeOfAllAtoms;
        public ushort m_numContactPoints;
        public ushort m_numReservedContactPoints;
        public byte m_numUserDatasForBodyA;
        public byte m_numUserDatasForBodyB;
        public byte m_contactPointPropertiesStriding;
        public ushort m_maxNumContactPoints;
        public hkpSimpleContactConstraintDataInfo m_info;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_sizeOfAllAtoms = br.ReadUInt16();
            m_numContactPoints = br.ReadUInt16();
            m_numReservedContactPoints = br.ReadUInt16();
            m_numUserDatasForBodyA = br.ReadByte();
            m_numUserDatasForBodyB = br.ReadByte();
            m_contactPointPropertiesStriding = br.ReadByte();
            br.ReadByte();
            m_maxNumContactPoints = br.ReadUInt16();
            br.ReadUInt16();
            m_info = new hkpSimpleContactConstraintDataInfo();
            m_info.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt16(m_sizeOfAllAtoms);
            bw.WriteUInt16(m_numContactPoints);
            bw.WriteUInt16(m_numReservedContactPoints);
            bw.WriteByte(m_numUserDatasForBodyA);
            bw.WriteByte(m_numUserDatasForBodyB);
            bw.WriteByte(m_contactPointPropertiesStriding);
            bw.WriteByte(0);
            bw.WriteUInt16(m_maxNumContactPoints);
            bw.WriteUInt16(0);
            m_info.Write(s, bw);
        }
    }
}
