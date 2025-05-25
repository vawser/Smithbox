using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ShaderType
    {
        EFFECT_TYPE_INVALID = 0,
        EFFECT_TYPE_UNKNOWN = 1,
        EFFECT_TYPE_HLSL_INLINE = 2,
        EFFECT_TYPE_CG_INLINE = 3,
        EFFECT_TYPE_HLSL_FILENAME = 4,
        EFFECT_TYPE_CG_FILENAME = 5,
        EFFECT_TYPE_MAX_ID = 6,
    }
    
    public partial class hkxMaterialShader : hkReferencedObject
    {
        public override uint Signature { get => 1339165424; }
        
        public string m_name;
        public ShaderType m_type;
        public string m_vertexEntryName;
        public string m_geomEntryName;
        public string m_pixelEntryName;
        public List<byte> m_data;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_type = (ShaderType)br.ReadByte();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
            m_vertexEntryName = des.ReadStringPointer(br);
            m_geomEntryName = des.ReadStringPointer(br);
            m_pixelEntryName = des.ReadStringPointer(br);
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
            s.WriteStringPointer(bw, m_vertexEntryName);
            s.WriteStringPointer(bw, m_geomEntryName);
            s.WriteStringPointer(bw, m_pixelEntryName);
            s.WriteByteArray(bw, m_data);
        }
    }
}
