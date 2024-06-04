// Automatically Generated

namespace HKLib.hk2018;

public class hkbCustomTestGeneratorComplexTypes : hkbCustomTestGeneratorSimpleTypes, hkbVerifiable
{
    public hkReferencedObject? m_complexTypeHkObjectPtr;

    public bool m_complexHiddenTypeCopyStart;

    public Quaternion m_complexTypeHkQuaternion = new();

    public Vector4 m_complexTypeHkVector4 = new();

    public hkbCustomTestGeneratorComplexTypes.CustomEnum m_complexTypeEnumHkInt8;

    public hkbCustomTestGeneratorComplexTypes.CustomEnum m_complexTypeEnumHkInt16;

    public hkbCustomTestGeneratorComplexTypes.CustomEnum m_complexTypeEnumHkInt32;

    public hkbCustomTestGeneratorComplexTypes.CustomEnum m_complexTypeEnumHkUint8;

    public hkbCustomTestGeneratorComplexTypes.CustomEnum m_complexTypeEnumHkUint16;

    public hkbCustomTestGeneratorComplexTypes.CustomEnum m_complexTypeEnumHkUint32;

    public hkbCustomTestGeneratorComplexTypes.CustomEnum m_complexTypeEnumHkInt8InvalidCheck;

    public hkbCustomTestGeneratorComplexTypes.CustomEnum m_complexTypeEnumHkInt16InvalidCheck;

    public hkbCustomTestGeneratorComplexTypes.CustomEnum m_complexTypeEnumHkInt32InvalidCheck;

    public hkbCustomTestGeneratorComplexTypes.CustomEnum m_complexTypeEnumHkUint8InvalidCheck;

    public hkbCustomTestGeneratorComplexTypes.CustomEnum m_complexTypeEnumHkUint16InvalidCheck;

    public hkbCustomTestGeneratorComplexTypes.CustomEnum m_complexTypeEnumHkUint32InvalidCheck;

    public hkbCustomTestGeneratorComplexTypes.CustomFlag m_complexTypeFlagsHkInt8;

    public hkbCustomTestGeneratorComplexTypes.CustomFlag m_complexTypeFlagsHkInt16;

    public hkbCustomTestGeneratorComplexTypes.CustomFlag m_complexTypeFlagsHkInt32;

    public hkbCustomTestGeneratorComplexTypes.CustomFlag m_complexTypeFlagsHkUint8;

    public hkbCustomTestGeneratorComplexTypes.CustomFlag m_complexTypeFlagsHkUint16;

    public hkbCustomTestGeneratorComplexTypes.CustomFlag m_complexTypeFlagsHkUint32;

    public hkbCustomTestGeneratorComplexTypes.CustomFlag m_complexTypeFlagsHkInt8InvalidCheck;

    public hkbCustomTestGeneratorComplexTypes.CustomFlag m_complexTypeFlagsHkInt16InvalidCheck;

    public hkbCustomTestGeneratorComplexTypes.CustomFlag m_complexTypeFlagsHkInt32InvalidCheck;

    public hkbCustomTestGeneratorComplexTypes.CustomFlag m_complexTypeFlagsHkUint8InvalidCheck;

    public hkbCustomTestGeneratorComplexTypes.CustomFlag m_complexTypeFlagsHkUint16InvalidCheck;

    public hkbCustomTestGeneratorComplexTypes.CustomFlag m_complexTypeFlagsHkUint32InvalidCheck;

    public bool m_complexHiddenTypeCopyEnd;


    [Flags]
    public enum CustomFlag : int
    {
        CUSTOM_FLAG_NONE = 0,
        CUSTOM_FLAG_UNO = 1,
        CUSTOM_FLAG_ZWEI = 2,
        CUSTOM_FLAG_SHI_OR_YON = 4,
        CUSTOM_FLAG_LOTS_O_BITS = 240
    }

    public enum CustomEnum : int
    {
        CUSTOM_ENUM_ALA = 0,
        CUSTOM_ENUM_DEPECHE = 1,
        CUSTOM_ENUM_FURIOUS = 5
    }

}

