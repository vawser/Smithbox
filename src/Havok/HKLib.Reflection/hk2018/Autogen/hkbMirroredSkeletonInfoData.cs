// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbMirroredSkeletonInfoData : HavokData<hkbMirroredSkeletonInfo> 
{
    public hkbMirroredSkeletonInfoData(HavokType type, hkbMirroredSkeletonInfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_mirrorAxis":
            case "mirrorAxis":
            {
                if (instance.m_mirrorAxis is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bonePairMap":
            case "bonePairMap":
            {
                if (instance.m_bonePairMap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_partitionPairMap":
            case "partitionPairMap":
            {
                if (instance.m_partitionPairMap is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
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
