using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpVehicleDefaultBrake : hknpVehicleBrake
    {
        public override uint Signature { get => 826511072; }
        
        public List<hknpVehicleDefaultBrakeWheelBrakingProperties> m_wheelBrakingProperties;
        public float m_wheelsMinTimeToBlock;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_wheelBrakingProperties = des.ReadClassArray<hknpVehicleDefaultBrakeWheelBrakingProperties>(br);
            m_wheelsMinTimeToBlock = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hknpVehicleDefaultBrakeWheelBrakingProperties>(bw, m_wheelBrakingProperties);
            bw.WriteSingle(m_wheelsMinTimeToBlock);
            bw.WriteUInt32(0);
        }
    }
}
