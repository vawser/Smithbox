// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpShapeMassPropertiesData : HavokData<HKLib.hk2019.hknpShapeMassProperties> 
{
    public hknpShapeMassPropertiesData(HavokType type, HKLib.hk2019.hknpShapeMassProperties instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_compressedMassProperties":
            case "compressedMassProperties":
            {
                if (instance.m_compressedMassProperties is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_compressedMassProperties":
            case "compressedMassProperties":
            {
                if (value is not HKLib.hk2019.hkCompressedMassProperties castValue) return false;
                instance.m_compressedMassProperties = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
