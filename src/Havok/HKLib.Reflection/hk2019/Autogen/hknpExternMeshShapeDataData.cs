// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpExternMeshShapeDataData : HavokData<HKLib.hk2019.hknpExternMeshShapeData> 
{
    public hknpExternMeshShapeDataData(HavokType type, HKLib.hk2019.hknpExternMeshShapeData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_simdTree":
            case "simdTree":
            {
                if (instance.m_simdTree is not TGet castValue) return false;
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
            case "m_simdTree":
            case "simdTree":
            {
                if (value is not HKLib.hk2019.hkcdSimdTree castValue) return false;
                instance.m_simdTree = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
