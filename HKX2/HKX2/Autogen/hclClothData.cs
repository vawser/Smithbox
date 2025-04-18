using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum Platform
    {
        HCL_PLATFORM_INVALID = 0,
        HCL_PLATFORM_WIN32 = 1,
        HCL_PLATFORM_X64 = 2,
        HCL_PLATFORM_MACPPC = 4,
        HCL_PLATFORM_IOS = 8,
        HCL_PLATFORM_MAC386 = 16,
        HCL_PLATFORM_PS3 = 32,
        HCL_PLATFORM_XBOX360 = 64,
        HCL_PLATFORM_WII = 128,
        HCL_PLATFORM_LRB = 256,
        HCL_PLATFORM_LINUX = 512,
        HCL_PLATFORM_PSVITA = 1024,
        HCL_PLATFORM_ANDROID = 2048,
        HCL_PLATFORM_CTR = 4096,
        HCL_PLATFORM_WIIU = 8192,
        HCL_PLATFORM_PS4 = 16384,
        HCL_PLATFORM_XBOXONE = 32768,
    }
    
    public partial class hclClothData : hkReferencedObject
    {
        public override uint Signature { get => 4181970594; }
        
        public string m_name;
        public List<hclSimClothData> m_simClothDatas;
        public List<hclBufferDefinition> m_bufferDefinitions;
        public List<hclTransformSetDefinition> m_transformSetDefinitions;
        public List<hclOperator> m_operators;
        public List<hclClothState> m_clothStateDatas;
        public List<hclAction> m_actions;
        public Platform m_targetPlatform;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_simClothDatas = des.ReadClassPointerArray<hclSimClothData>(br);
            m_bufferDefinitions = des.ReadClassPointerArray<hclBufferDefinition>(br);
            m_transformSetDefinitions = des.ReadClassPointerArray<hclTransformSetDefinition>(br);
            m_operators = des.ReadClassPointerArray<hclOperator>(br);
            m_clothStateDatas = des.ReadClassPointerArray<hclClothState>(br);
            m_actions = des.ReadClassPointerArray<hclAction>(br);
            m_targetPlatform = (Platform)br.ReadUInt32();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            s.WriteClassPointerArray<hclSimClothData>(bw, m_simClothDatas);
            s.WriteClassPointerArray<hclBufferDefinition>(bw, m_bufferDefinitions);
            s.WriteClassPointerArray<hclTransformSetDefinition>(bw, m_transformSetDefinitions);
            s.WriteClassPointerArray<hclOperator>(bw, m_operators);
            s.WriteClassPointerArray<hclClothState>(bw, m_clothStateDatas);
            s.WriteClassPointerArray<hclAction>(bw, m_actions);
            bw.WriteUInt32((uint)m_targetPlatform);
            bw.WriteUInt32(0);
        }
    }
}
