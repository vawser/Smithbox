using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum WheelCollideType
    {
        INVALID_WHEEL_COLLIDE = 0,
        RAY_CAST_WHEEL_COLLIDE = 1,
        LINEAR_CAST_WHEEL_COLLIDE = 2,
        USER_WHEEL_COLLIDE1 = 3,
        USER_WHEEL_COLLIDE2 = 4,
        USER_WHEEL_COLLIDE3 = 5,
        USER_WHEEL_COLLIDE4 = 6,
        USER_WHEEL_COLLIDE5 = 7,
    }
    
    public partial class hknpVehicleWheelCollide : hkReferencedObject
    {
        public override uint Signature { get => 587024455; }
        
        public bool m_alreadyUsed;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_alreadyUsed = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteBoolean(m_alreadyUsed);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
