using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum DataType
    {
        HKX_DT_NONE = 0,
        HKX_DT_UINT8 = 1,
        HKX_DT_INT16 = 2,
        HKX_DT_UINT32 = 3,
        HKX_DT_FLOAT = 4,
    }
    
    public enum DataUsage
    {
        HKX_DU_NONE = 0,
        HKX_DU_POSITION = 1,
        HKX_DU_COLOR = 2,
        HKX_DU_NORMAL = 4,
        HKX_DU_TANGENT = 8,
        HKX_DU_BINORMAL = 16,
        HKX_DU_TEXCOORD = 32,
        HKX_DU_BLENDWEIGHTS = 64,
        HKX_DU_BLENDINDICES = 128,
        HKX_DU_USERDATA = 256,
    }
    
    public partial class hkxVertexDescription : IHavokObject
    {
        public virtual uint Signature { get => 1197541056; }
        
        public List<hkxVertexDescriptionElementDecl> m_decls;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_decls = des.ReadClassArray<hkxVertexDescriptionElementDecl>(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassArray<hkxVertexDescriptionElementDecl>(bw, m_decls);
        }
    }
}
