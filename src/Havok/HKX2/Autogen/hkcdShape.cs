using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkcdShape : hkReferencedObject
    {
        public override uint Signature { get => 2030635963; }
        
        public ShapeDispatchTypeEnum m_dispatchType;
        public byte m_bitsPerKey;
        public ShapeInfoCodecTypeEnum m_shapeInfoCodecType;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadByte();
            m_dispatchType = (ShapeDispatchTypeEnum)br.ReadByte();
            m_bitsPerKey = br.ReadByte();
            m_shapeInfoCodecType = (ShapeInfoCodecTypeEnum)br.ReadByte();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteByte(0);
            bw.WriteByte((byte)m_dispatchType);
            bw.WriteByte(m_bitsPerKey);
            bw.WriteByte((byte)m_shapeInfoCodecType);
            bw.WriteUInt32(0);
        }
    }
}
