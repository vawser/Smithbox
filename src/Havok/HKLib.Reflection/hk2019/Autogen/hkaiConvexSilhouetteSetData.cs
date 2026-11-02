// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiConvexSilhouetteSetData : HavokData<HKLib.hk2019.hkaiConvexSilhouetteSet> 
{
    public hkaiConvexSilhouetteSetData(HavokType type, HKLib.hk2019.hkaiConvexSilhouetteSet instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexPool":
            case "vertexPool":
            {
                if (instance.m_vertexPool is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_silhouetteOffsets":
            case "silhouetteOffsets":
            {
                if (instance.m_silhouetteOffsets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cachedTransform":
            case "cachedTransform":
            {
                if (instance.m_cachedTransform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cachedUp":
            case "cachedUp":
            {
                if (instance.m_cachedUp is not TGet castValue) return false;
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
                if (value is not HKLib.hk2019.hkQTransform castValue) return false;
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
