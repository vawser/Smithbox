// Automatically Generated

namespace HKLib.hk2018;

public class hclClothData : hkReferencedObject
{
    public string? m_name;

    public List<hclSimClothData?> m_simClothDatas = new();

    public List<hclBufferDefinition?> m_bufferDefinitions = new();

    public List<hclTransformSetDefinition?> m_transformSetDefinitions = new();

    public List<hclOperator?> m_operators = new();

    public List<hclClothState?> m_clothStateDatas = new();

    public List<hclStateTransition?> m_stateTransitions = new();

    public List<hclAction?> m_actions = new();

    public hclClothData.Platform m_targetPlatform;


    public enum Platform : int
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
        HCL_PLATFORM_MAC64 = 65536,
        HCL_PLATFORM_NX = 131072,
        HCL_PLATFORM_GDK = 262144,
        HCL_PLATFORM_COOKER = 524288
    }

}

