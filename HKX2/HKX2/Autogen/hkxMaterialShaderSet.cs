using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxMaterialShaderSet : hkReferencedObject
    {
        public override uint Signature { get => 1958775068; }
        
        public List<hkxMaterialShader> m_shaders;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_shaders = des.ReadClassPointerArray<hkxMaterialShader>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkxMaterialShader>(bw, m_shaders);
        }
    }
}
