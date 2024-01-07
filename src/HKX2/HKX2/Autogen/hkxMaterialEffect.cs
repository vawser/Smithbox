using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum EffectType
    {
        EFFECT_TYPE_INVALID = 0,
        EFFECT_TYPE_UNKNOWN = 1,
        EFFECT_TYPE_HLSL_FX_INLINE = 2,
        EFFECT_TYPE_CG_FX_INLINE = 3,
        EFFECT_TYPE_HLSL_FX_FILENAME = 4,
        EFFECT_TYPE_CG_FX_FILENAME = 5,
        EFFECT_TYPE_MAX_ID = 6,
    }
    
    public partial class hkxMaterialEffect : hkReferencedObject
    {
        public override uint Signature { get => 1527191545; }
        
        public string m_name;
        public EffectType m_type;
        public List<byte> m_data;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_type = (EffectType)br.ReadByte();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
            m_data = des.ReadByteArray(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            bw.WriteByte((byte)m_type);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            s.WriteByteArray(bw, m_data);
        }
    }
}
