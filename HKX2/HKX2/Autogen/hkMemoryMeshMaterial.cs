using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkMemoryMeshMaterial : hkMeshMaterial
    {
        public override uint Signature { get => 3379202540; }
        
        public string m_materialName;
        public List<hkMeshTexture> m_textures;
        public Vector4 m_diffuseColor;
        public Vector4 m_ambientColor;
        public Vector4 m_specularColor;
        public Vector4 m_emissiveColor;
        public ulong m_userData;
        public float m_tesselationFactor;
        public float m_displacementAmount;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_materialName = des.ReadStringPointer(br);
            m_textures = des.ReadClassPointerArray<hkMeshTexture>(br);
            br.ReadUInt64();
            m_diffuseColor = des.ReadVector4(br);
            m_ambientColor = des.ReadVector4(br);
            m_specularColor = des.ReadVector4(br);
            m_emissiveColor = des.ReadVector4(br);
            m_userData = br.ReadUInt64();
            m_tesselationFactor = br.ReadSingle();
            m_displacementAmount = br.ReadSingle();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_materialName);
            s.WriteClassPointerArray<hkMeshTexture>(bw, m_textures);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_diffuseColor);
            s.WriteVector4(bw, m_ambientColor);
            s.WriteVector4(bw, m_specularColor);
            s.WriteVector4(bw, m_emissiveColor);
            bw.WriteUInt64(m_userData);
            bw.WriteSingle(m_tesselationFactor);
            bw.WriteSingle(m_displacementAmount);
        }
    }
}
