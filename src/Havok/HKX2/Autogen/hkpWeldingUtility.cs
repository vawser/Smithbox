using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum WeldingType
    {
        WELDING_TYPE_ANTICLOCKWISE = 0,
        WELDING_TYPE_CLOCKWISE = 4,
        WELDING_TYPE_TWO_SIDED = 5,
        WELDING_TYPE_NONE = 6,
    }
    
    public enum SectorType
    {
        ACCEPT_0 = 1,
        SNAP_0 = 0,
        REJECT = 2,
        SNAP_1 = 4,
        ACCEPT_1 = 3,
    }
    
    public enum NumAngles
    {
        NUM_ANGLES = 31,
    }
    
    public partial class hkpWeldingUtility : IHavokObject
    {
        public virtual uint Signature { get => 2998149099; }
        
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteByte(0);
        }
    }
}
