using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum TextureType
    {
        TEX_UNKNOWN = 0,
        TEX_DIFFUSE = 1,
        TEX_REFLECTION = 2,
        TEX_BUMP = 3,
        TEX_NORMAL = 4,
        TEX_DISPLACEMENT = 5,
        TEX_SPECULAR = 6,
        TEX_SPECULARANDGLOSS = 7,
        TEX_OPACITY = 8,
        TEX_EMISSIVE = 9,
        TEX_REFRACTION = 10,
        TEX_GLOSS = 11,
        TEX_DOMINANTS = 12,
        TEX_NOTEXPORTED = 13,
    }
    
    public enum PropertyKey
    {
        PROPERTY_MTL_TYPE_BLEND = 1,
        PROPERTY_MTL_UV_ID_STAGE0 = 256,
        PROPERTY_MTL_UV_ID_STAGE1 = 257,
        PROPERTY_MTL_UV_ID_STAGE2 = 258,
        PROPERTY_MTL_UV_ID_STAGE3 = 259,
        PROPERTY_MTL_UV_ID_STAGE4 = 260,
        PROPERTY_MTL_UV_ID_STAGE5 = 261,
        PROPERTY_MTL_UV_ID_STAGE6 = 262,
        PROPERTY_MTL_UV_ID_STAGE7 = 263,
        PROPERTY_MTL_UV_ID_STAGE8 = 264,
        PROPERTY_MTL_UV_ID_STAGE9 = 265,
        PROPERTY_MTL_UV_ID_STAGE10 = 266,
        PROPERTY_MTL_UV_ID_STAGE11 = 267,
        PROPERTY_MTL_UV_ID_STAGE12 = 268,
        PROPERTY_MTL_UV_ID_STAGE13 = 269,
        PROPERTY_MTL_UV_ID_STAGE14 = 270,
        PROPERTY_MTL_UV_ID_STAGE15 = 271,
        PROPERTY_MTL_UV_ID_STAGE_MAX = 272,
    }
    
    public enum UVMappingAlgorithm
    {
        UVMA_SRT = 0,
        UVMA_TRS = 1,
        UVMA_3DSMAX_STYLE = 2,
        UVMA_MAYA_STYLE = 3,
    }
    
    public enum Transparency
    {
        transp_none = 0,
        transp_alpha = 2,
        transp_additive = 3,
        transp_colorkey = 4,
        transp_subtractive = 9,
    }
    
    public partial class hkxMaterial : hkxAttributeHolder
    {
        public override uint Signature { get => 2019476322; }
        
        public string m_name;
        public List<hkxMaterialTextureStage> m_stages;
        public Vector4 m_diffuseColor;
        public Vector4 m_ambientColor;
        public Vector4 m_specularColor;
        public Vector4 m_emissiveColor;
        public List<hkxMaterial> m_subMaterials;
        public hkReferencedObject m_extraData;
        public float m_uvMapScale_0;
        public float m_uvMapScale_1;
        public float m_uvMapOffset_0;
        public float m_uvMapOffset_1;
        public float m_uvMapRotation;
        public UVMappingAlgorithm m_uvMapAlgorithm;
        public float m_specularMultiplier;
        public float m_specularExponent;
        public Transparency m_transparency;
        public ulong m_userData;
        public List<hkxMaterialProperty> m_properties;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_stages = des.ReadClassArray<hkxMaterialTextureStage>(br);
            br.ReadUInt64();
            m_diffuseColor = des.ReadVector4(br);
            m_ambientColor = des.ReadVector4(br);
            m_specularColor = des.ReadVector4(br);
            m_emissiveColor = des.ReadVector4(br);
            m_subMaterials = des.ReadClassPointerArray<hkxMaterial>(br);
            m_extraData = des.ReadClassPointer<hkReferencedObject>(br);
            m_uvMapScale_0 = br.ReadSingle();
            m_uvMapScale_1 = br.ReadSingle();
            m_uvMapOffset_0 = br.ReadSingle();
            m_uvMapOffset_1 = br.ReadSingle();
            m_uvMapRotation = br.ReadSingle();
            m_uvMapAlgorithm = (UVMappingAlgorithm)br.ReadUInt32();
            m_specularMultiplier = br.ReadSingle();
            m_specularExponent = br.ReadSingle();
            m_transparency = (Transparency)br.ReadByte();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
            m_userData = br.ReadUInt64();
            m_properties = des.ReadClassArray<hkxMaterialProperty>(br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            s.WriteClassArray<hkxMaterialTextureStage>(bw, m_stages);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_diffuseColor);
            s.WriteVector4(bw, m_ambientColor);
            s.WriteVector4(bw, m_specularColor);
            s.WriteVector4(bw, m_emissiveColor);
            s.WriteClassPointerArray<hkxMaterial>(bw, m_subMaterials);
            s.WriteClassPointer<hkReferencedObject>(bw, m_extraData);
            bw.WriteSingle(m_uvMapScale_0);
            bw.WriteSingle(m_uvMapScale_1);
            bw.WriteSingle(m_uvMapOffset_0);
            bw.WriteSingle(m_uvMapOffset_1);
            bw.WriteSingle(m_uvMapRotation);
            bw.WriteUInt32((uint)m_uvMapAlgorithm);
            bw.WriteSingle(m_specularMultiplier);
            bw.WriteSingle(m_specularExponent);
            bw.WriteByte((byte)m_transparency);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteUInt64(m_userData);
            s.WriteClassArray<hkxMaterialProperty>(bw, m_properties);
            bw.WriteUInt64(0);
        }
    }
}
