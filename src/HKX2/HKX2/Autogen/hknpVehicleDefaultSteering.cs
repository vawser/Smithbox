using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpVehicleDefaultSteering : hknpVehicleSteering
    {
        public override uint Signature { get => 255712478; }
        
        public float m_maxSteeringAngle;
        public float m_maxSpeedFullSteeringAngle;
        public List<bool> m_doesWheelSteer;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_maxSteeringAngle = br.ReadSingle();
            m_maxSpeedFullSteeringAngle = br.ReadSingle();
            m_doesWheelSteer = des.ReadBooleanArray(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_maxSteeringAngle);
            bw.WriteSingle(m_maxSpeedFullSteeringAngle);
            s.WriteBooleanArray(bw, m_doesWheelSteer);
        }
    }
}
