using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum hkaReferenceFrameTypeEnum
    {
        REFERENCE_FRAME_UNKNOWN = 0,
        REFERENCE_FRAME_DEFAULT = 1,
        REFERENCE_FRAME_PARAMETRIC = 2,
    }
    
    public partial class hkaAnimatedReferenceFrame : hkReferencedObject
    {
        public override uint Signature { get => 2556314263; }
        
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
        }
    }
}
