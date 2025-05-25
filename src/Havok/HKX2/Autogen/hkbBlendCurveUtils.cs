using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum BlendCurve
    {
        BLEND_CURVE_SMOOTH = 0,
        BLEND_CURVE_LINEAR = 1,
        BLEND_CURVE_LINEAR_TO_SMOOTH = 2,
        BLEND_CURVE_SMOOTH_TO_LINEAR = 3,
    }
    
    public partial class hkbBlendCurveUtils : IHavokObject
    {
        public virtual uint Signature { get => 587471600; }
        
        
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
