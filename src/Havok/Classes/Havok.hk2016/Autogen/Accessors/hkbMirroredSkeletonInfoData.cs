// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbMirroredSkeletonInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbMirroredSkeletonInfo>
{
    public hkbMirroredSkeletonInfoData(Havok.hk2016.hkbMirroredSkeletonInfo instance) : base(instance) { }

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
            case "m_mirrorAxis":
            case "mirrorAxis":
            {
                value = instance.m_mirrorAxis;
                return true;
            }
            case "m_bonePairMap":
            case "bonePairMap":
            {
                value = instance.m_bonePairMap;
                return true;
            }
            case "m_partitionPairMap":
            case "partitionPairMap":
            {
                value = instance.m_partitionPairMap;
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
            case "m_mirrorAxis":
            case "mirrorAxis":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_mirrorAxis = castValue;
                return true;
            }
            case "m_bonePairMap":
            case "bonePairMap":
            {
                if (value is not List<short> castValue) return false;
                instance.m_bonePairMap = castValue;
                return true;
            }
            case "m_partitionPairMap":
            case "partitionPairMap":
            {
                if (value is not List<short> castValue) return false;
                instance.m_partitionPairMap = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
