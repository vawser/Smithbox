// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiConvexSilhouetteSetData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiConvexSilhouetteSet>
{
    public hkaiConvexSilhouetteSetData(Havok.hk2018.hkaiConvexSilhouetteSet instance) : base(instance) { }

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
            case "m_vertexPool":
            case "vertexPool":
            {
                value = instance.m_vertexPool;
                return true;
            }
            case "m_silhouetteOffsets":
            case "silhouetteOffsets":
            {
                value = instance.m_silhouetteOffsets;
                return true;
            }
            case "m_cachedTransform":
            case "cachedTransform":
            {
                value = instance.m_cachedTransform;
                return true;
            }
            case "m_cachedUp":
            case "cachedUp":
            {
                value = instance.m_cachedUp;
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
            case "m_vertexPool":
            case "vertexPool":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_vertexPool = castValue;
                return true;
            }
            case "m_silhouetteOffsets":
            case "silhouetteOffsets":
            {
                if (value is not List<int> castValue) return false;
                instance.m_silhouetteOffsets = castValue;
                return true;
            }
            case "m_cachedTransform":
            case "cachedTransform":
            {
                if (value is not Havok.hk2018.hkQTransform castValue) return false;
                instance.m_cachedTransform = castValue;
                return true;
            }
            case "m_cachedUp":
            case "cachedUp":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_cachedUp = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
