// Automatically Generated

namespace HKLib.hk2018;

public class hclRuntimeConversionInfo : IHavokObject
{
    public readonly hclRuntimeConversionInfo.SlotConversion[] m_slotConversions = new hclRuntimeConversionInfo.SlotConversion[4];

    public readonly hclRuntimeConversionInfo.ElementConversion[] m_elementConversions = new hclRuntimeConversionInfo.ElementConversion[4];

    public byte m_numSlotsConverted;

    public byte m_numElementsConverted;


    public enum VectorConversion : int
    {
        VC_FLOAT4 = 0,
        VC_FLOAT3 = 1,
        VC_BYTE4 = 2,
        VC_SHORT3 = 3,
        VC_HFLOAT3 = 4,
        VC_CUSTOM_A = 20,
        VC_CUSTOM_B = 21,
        VC_CUSTOM_C = 22,
        VC_CUSTOM_D = 23,
        VC_CUSTOM_E = 24,
        VC_NONE = 250
    }

    public class ElementConversion : IHavokObject
    {
        public byte m_index;

        public byte m_offset;

        public hclRuntimeConversionInfo.VectorConversion m_conversion;

    }


    public class SlotConversion : IHavokObject
    {
        public readonly byte[] m_elements = new byte[4];

        public byte m_numElements;

        public byte m_index;

        public bool m_partialWrite;

    }


}

