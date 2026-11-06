// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hknpCompressedMeshShapeDataData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hknpCompressedMeshShapeData>
{
    public hknpCompressedMeshShapeDataData(Havok.hk2016.hknpCompressedMeshShapeData instance) : base(instance) { }

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
            case "m_meshTree":
            case "meshTree":
            {
                value = instance.m_meshTree;
                return true;
            }
            case "m_simdTree":
            case "simdTree":
            {
                value = instance.m_simdTree;
                return true;
            }
            case "m_connectivity":
            case "connectivity":
            {
                value = instance.m_connectivity;
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
                if (value is not Havok.hk2016.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_meshTree":
            case "meshTree":
            {
                if (value is not Havok.hk2016.hknpCompressedMeshShapeTree castValue) return false;
                instance.m_meshTree = castValue;
                return true;
            }
            case "m_simdTree":
            case "simdTree":
            {
                if (value is not Havok.hk2016.hkcdSimdTree castValue) return false;
                instance.m_simdTree = castValue;
                return true;
            }
            case "m_connectivity":
            case "connectivity":
            {
                if (value is not Havok.hk2016.hkcdStaticMeshTreeBase.Connectivity castValue) return false;
                instance.m_connectivity = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
