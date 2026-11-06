// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpExternMeshShapeDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpExternMeshShapeData>
{
    public hknpExternMeshShapeDataData(Havok.hk2018.hknpExternMeshShapeData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_simdTree":
            case "simdTree":
            {
                value = instance.m_simdTree;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_simdTree":
            case "simdTree":
            {
                if (value is not Havok.hk2018.hkcdSimdTree castValue) return false;
                instance.m_simdTree = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
