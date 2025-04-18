using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpVehicleFrictionDescription : hkReferencedObject
    {
        public override uint Signature { get => 3159292022; }
        
        public float m_wheelDistance;
        public float m_chassisMassInv;
        public hkpVehicleFrictionDescriptionAxisDescription m_axleDescr_0;
        public hkpVehicleFrictionDescriptionAxisDescription m_axleDescr_1;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_wheelDistance = br.ReadSingle();
            m_chassisMassInv = br.ReadSingle();
            m_axleDescr_0 = new hkpVehicleFrictionDescriptionAxisDescription();
            m_axleDescr_0.Read(des, br);
            m_axleDescr_1 = new hkpVehicleFrictionDescriptionAxisDescription();
            m_axleDescr_1.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_wheelDistance);
            bw.WriteSingle(m_chassisMassInv);
            m_axleDescr_0.Write(s, bw);
            m_axleDescr_1.Write(s, bw);
        }
    }
}
