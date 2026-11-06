// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hknpExternMeshShapeDataData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hknpExternMeshShapeData>
{
    public hknpExternMeshShapeDataData(Havok.hk2016.hknpExternMeshShapeData instance) : base(instance) { }

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
            case "m_aabbTree":
            case "aabbTree":
            {
                value = instance.m_aabbTree;
                return true;
            }
            case "m_simdTree":
            case "simdTree":
            {
                value = instance.m_simdTree;
                return true;
            }
            case "m_hasBuildContext":
            case "hasBuildContext":
            {
                value = instance.m_hasBuildContext;
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
                if (value is not List<Havok.hk2016.hkDefaultPropertyBag> castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_aabbTree":
            case "aabbTree":
            {
                if (value is not Havok.hk2016.hkcdStaticTree.DefaultTree6 castValue) return false;
                instance.m_aabbTree = castValue;
                return true;
            }
            case "m_simdTree":
            case "simdTree":
            {
                if (value is not Havok.hk2016.hkcdSimdTree castValue) return false;
                instance.m_simdTree = castValue;
                return true;
            }
            case "m_hasBuildContext":
            case "hasBuildContext":
            {
                if (value is not bool castValue) return false;
                instance.m_hasBuildContext = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
