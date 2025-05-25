using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkCompressedMassProperties : IHavokObject
    {
        public virtual uint Signature { get => 2596654817; }
        
        public short m_centerOfMass_0;
        public short m_centerOfMass_1;
        public short m_centerOfMass_2;
        public short m_centerOfMass_3;
        public short m_inertia_0;
        public short m_inertia_1;
        public short m_inertia_2;
        public short m_inertia_3;
        public short m_majorAxisSpace_0;
        public short m_majorAxisSpace_1;
        public short m_majorAxisSpace_2;
        public short m_majorAxisSpace_3;
        public float m_mass;
        public float m_volume;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_centerOfMass_0 = br.ReadInt16();
            m_centerOfMass_1 = br.ReadInt16();
            m_centerOfMass_2 = br.ReadInt16();
            m_centerOfMass_3 = br.ReadInt16();
            m_inertia_0 = br.ReadInt16();
            m_inertia_1 = br.ReadInt16();
            m_inertia_2 = br.ReadInt16();
            m_inertia_3 = br.ReadInt16();
            m_majorAxisSpace_0 = br.ReadInt16();
            m_majorAxisSpace_1 = br.ReadInt16();
            m_majorAxisSpace_2 = br.ReadInt16();
            m_majorAxisSpace_3 = br.ReadInt16();
            m_mass = br.ReadSingle();
            m_volume = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt16(m_centerOfMass_0);
            bw.WriteInt16(m_centerOfMass_1);
            bw.WriteInt16(m_centerOfMass_2);
            bw.WriteInt16(m_centerOfMass_3);
            bw.WriteInt16(m_inertia_0);
            bw.WriteInt16(m_inertia_1);
            bw.WriteInt16(m_inertia_2);
            bw.WriteInt16(m_inertia_3);
            bw.WriteInt16(m_majorAxisSpace_0);
            bw.WriteInt16(m_majorAxisSpace_1);
            bw.WriteInt16(m_majorAxisSpace_2);
            bw.WriteInt16(m_majorAxisSpace_3);
            bw.WriteSingle(m_mass);
            bw.WriteSingle(m_volume);
        }
    }
}
