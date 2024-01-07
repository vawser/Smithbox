using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ShapeInfoCodecTypeEnum
    {
        NULL_CODEC = 0,
        UFM358 = 1,
        MAX_NUM_CODECS = 16,
    }
    
    public partial class hkcdShapeInfoCodecType : IHavokObject
    {
        public virtual uint Signature { get => 1801223872; }
        
        
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
