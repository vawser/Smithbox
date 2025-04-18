using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpVehicleSuspension : hkReferencedObject
    {
        public override uint Signature { get => 191844318; }
        
        public List<hknpVehicleSuspensionSuspensionWheelParameters> m_wheelParams;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_wheelParams = des.ReadClassArray<hknpVehicleSuspensionSuspensionWheelParameters>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hknpVehicleSuspensionSuspensionWheelParameters>(bw, m_wheelParams);
        }
    }
}
